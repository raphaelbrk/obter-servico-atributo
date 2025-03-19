using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ObterServicoAtravesAttribute.Console
{
    /// <summary>
    /// Classe para gerenciamento de serviços por atributos via injeção de dependência
    /// </summary>
    public class GerenciadorServicos
    {
        private readonly Dictionary<string, Type> _tiposServicos = new();
        private readonly Dictionary<string, IServico> _instanciasServicos = new();
        private readonly IServiceProvider _serviceProvider;
        
        /// <summary>
        /// Construtor que recebe um provedor de serviços para injeção de dependência
        /// </summary>
        /// <param name="serviceProvider">Provedor de serviços para resolver dependências</param>
        public GerenciadorServicos(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            CarregarTiposServicos();
        }
        
        /// <summary>
        /// Carrega todos os tipos de serviços disponíveis
        /// </summary>
        private void CarregarTiposServicos()
        {
            var tiposServico = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => typeof(IServico).IsAssignableFrom(t) && 
                            !t.IsInterface && 
                            !t.IsAbstract)
                .ToList();

            foreach (var tipo in tiposServico)
            {
                var atributo = tipo.GetCustomAttribute<ServicoAttribute>();
                if (atributo != null)
                {
                    _tiposServicos[atributo.Identificador] = tipo;
                }
            }
        }
        
        /// <summary>
        /// Obtém um serviço pelo identificador
        /// </summary>
        /// <param name="identificador">Identificador do serviço definido no atributo</param>
        /// <param name="reutilizarInstancia">Se deve reutilizar a instância do serviço quando possível</param>
        /// <returns>Instância do serviço</returns>
        public IServico ObterServico(string identificador, bool reutilizarInstancia = true)
        {
            if (string.IsNullOrEmpty(identificador))
                throw new ArgumentNullException(nameof(identificador));
                
            // Se deve reutilizar instância e já existe uma instância, retorna a existente
            if (reutilizarInstancia && _instanciasServicos.TryGetValue(identificador, out var servicoExistente))
            {
                return servicoExistente;
            }
            
            // Verifica se existe um tipo de serviço com o identificador informado
            if (!_tiposServicos.TryGetValue(identificador, out var tipoServico))
            {
                throw new InvalidOperationException($"Nenhum serviço encontrado com o identificador '{identificador}'");
            }
            
            // Tenta obter o serviço do provedor de serviços
            var servico = _serviceProvider.GetService(tipoServico) as IServico;
            
            // Se não conseguiu obter do provedor, cria uma instância manualmente
            if (servico == null)
            {
                servico = Activator.CreateInstance(tipoServico) as IServico;
                
                if (servico == null)
                {
                    throw new InvalidOperationException($"Não foi possível criar uma instância do serviço '{identificador}'");
                }
            }
            
            // Se deve reutilizar a instância, armazena no dicionário
            if (reutilizarInstancia)
            {
                _instanciasServicos[identificador] = servico;
            }
            
            return servico;
        }
        
        /// <summary>
        /// Remove a instância de um serviço do cache de instâncias
        /// </summary>
        /// <param name="identificador">Identificador do serviço</param>
        public void LimparInstanciaServico(string identificador)
        {
            if (!string.IsNullOrEmpty(identificador))
            {
                _instanciasServicos.Remove(identificador);
            }
        }
        
        /// <summary>
        /// Remove todas as instâncias de serviços do cache de instâncias
        /// </summary>
        public void LimparTodasInstancias()
        {
            _instanciasServicos.Clear();
        }
        
        /// <summary>
        /// Retorna todos os identificadores de serviços disponíveis
        /// </summary>
        /// <returns>Lista de identificadores de serviços</returns>
        public IEnumerable<string> ObterIdentificadoresServicos()
        {
            return _tiposServicos.Keys;
        }
    }
} 