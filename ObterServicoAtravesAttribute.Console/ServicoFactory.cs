using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading;
using ObterServicoAtravesAttribute.Console.Cache;

namespace ObterServicoAtravesAttribute.Console
{
    public class ServicoFactory
    {
        // Cache para mapear identificadores para tipos de serviço - imutável após inicialização
        private static readonly ConcurrentDictionary<string, Type> _cacheIdentificadorParaTipo = new();
        
        // Cache local como fallback quando não há cache distribuído configurado
        private static readonly ConcurrentDictionary<string, Lazy<IServico>> _cacheLocalInstancias = new();
        
        // Instância do cache distribuído
        private static ICacheDistribuido _cacheDistribuido;
        
        // Utiliza ReaderWriterLockSlim para melhor concorrência - leituras paralelas, escrita exclusiva
        private static readonly ReaderWriterLockSlim _rwLock = new(LockRecursionPolicy.NoRecursion);
        
        // Inicialização lazy para carregamento sob demanda
        private static readonly Lazy<bool> _inicializacao = new(InicializarCacheDeTypos, LazyThreadSafetyMode.ExecutionAndPublication);

        // Prefixo para chaves no cache distribuído
        private const string PREFIXO_CACHE = "ServicoDinamico:";
        
        // Tempo padrão de expiração para itens no cache
        private static readonly TimeSpan _tempoExpiracaoPadrao = TimeSpan.FromHours(1);

        /// <summary>
        /// Configura o provedor de cache distribuído a ser utilizado
        /// </summary>
        /// <param name="cacheDistribuido">Implementação de cache distribuído</param>
        public static void ConfigurarCacheDistribuido(ICacheDistribuido cacheDistribuido)
        {
            _cacheDistribuido = cacheDistribuido ?? throw new ArgumentNullException(nameof(cacheDistribuido));
        }

        /// <summary>
        /// Obtém um serviço pelo identificador especificado
        /// </summary>
        /// <param name="identificador">Identificador do serviço</param>
        /// <param name="usarCache">Indica se deve usar cache de instâncias (default: true)</param>
        /// <returns>Instância do serviço</returns>
        public static IServico ObterServico(string identificador, bool usarCache = true)
        {
            if (string.IsNullOrEmpty(identificador))
                throw new ArgumentNullException(nameof(identificador));

            // Garante que o cache de tipos seja inicializado
            var _ = _inicializacao.Value;

            // Se cache estiver desabilitado, cria nova instância a cada chamada
            if (!usarCache)
            {
                return CriarNovaInstanciaServico(identificador);
            }
            
            // Verificar cache distribuído se configurado
            if (_cacheDistribuido != null)
            {
                string chaveCache = $"{PREFIXO_CACHE}{identificador}";
                
                // Tentativa de obter do cache distribuído
                var servicoCached = _cacheDistribuido.Obter<IServico>(chaveCache);
                if (servicoCached != null)
                {
                    return servicoCached;
                }
                
                // Se não estiver em cache, cria nova instância
                var servico = CriarNovaInstanciaServico(identificador);
                
                // Armazena no cache distribuído com expiração
                _cacheDistribuido.Definir(chaveCache, servico, _tempoExpiracaoPadrao);
                
                return servico;
            }
            else
            {
                // Fallback para cache local utilizando Lazy<T>
                var lazyServico = _cacheLocalInstancias.GetOrAdd(identificador, chave => 
                    new Lazy<IServico>(() => CriarNovaInstanciaServico(chave), 
                                      LazyThreadSafetyMode.ExecutionAndPublication));
                
                return lazyServico.Value;
            }
        }
        
        /// <summary>
        /// Cria uma nova instância do serviço
        /// </summary>
        private static IServico CriarNovaInstanciaServico(string identificador)
        {
            if (!_cacheIdentificadorParaTipo.TryGetValue(identificador, out var tipoServico))
            {
                throw new InvalidOperationException($"Nenhum serviço encontrado com o identificador '{identificador}'");
            }
            
            return (IServico)Activator.CreateInstance(tipoServico);
        }

        /// <summary>
        /// Inicializa o cache de tipos de forma thread-safe
        /// </summary>
        private static bool InicializarCacheDeTypos()
        {
            try
            {
                // Usando ReaderWriterLockSlim para melhor desempenho em cenários com muitas leituras
                _rwLock.EnterWriteLock();
                
                var tiposServico = Assembly.GetExecutingAssembly()
                    .GetTypes()
                    .Where(t => typeof(IServico).IsAssignableFrom(t) && 
                                !t.IsInterface && 
                                !t.IsAbstract)
                    .ToList(); // Materializa a consulta

                foreach (var tipo in tiposServico)
                {
                    var atributo = tipo.GetCustomAttribute<ServicoAttribute>();
                    if (atributo != null)
                    {
                        _cacheIdentificadorParaTipo[atributo.Identificador] = tipo;
                    }
                }
                
                return true;
            }
            finally
            {
                if (_rwLock.IsWriteLockHeld)
                {
                    _rwLock.ExitWriteLock();
                }
            }
        }

        /// <summary>
        /// Limpa o cache local e distribuído
        /// </summary>
        public static void LimparCache()
        {
            _cacheLocalInstancias.Clear();
            
            // Limpar cada chave no cache distribuído
            if (_cacheDistribuido != null)
            {
                foreach (var identificador in _cacheIdentificadorParaTipo.Keys)
                {
                    string chaveCache = $"{PREFIXO_CACHE}{identificador}";
                    _cacheDistribuido.Remover(chaveCache);
                }
            }
        }
        
        /// <summary>
        /// Invalida um único serviço em cache
        /// </summary>
        public static void InvalidarCacheServico(string identificador)
        {
            if (string.IsNullOrEmpty(identificador))
                return;
                
            _cacheLocalInstancias.TryRemove(identificador, out _);
            
            if (_cacheDistribuido != null)
            {
                string chaveCache = $"{PREFIXO_CACHE}{identificador}";
                _cacheDistribuido.Remover(chaveCache);
            }
        }
        
        /// <summary>
        /// Método para gerenciar recursos, importante em aplicações de longa duração
        /// </summary>
        public static void Dispose()
        {
            _rwLock.Dispose();
        }
    }
} 