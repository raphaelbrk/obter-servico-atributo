using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace ObterServicoAtravesAttribute.Console.Cache
{
    /// <summary>
    /// Implementação de cache em memória para testes e desenvolvimento
    /// Em produção, esta classe deve ser substituída por uma implementação que use Redis ou outro cache distribuído
    /// </summary>
    public class CacheLocal : ICacheDistribuido
    {
        private class ItemCache<T>
        {
            public T Valor { get; set; }
            public DateTime? DataExpiracao { get; set; }
            
            public bool EstaExpirado => DataExpiracao.HasValue && DateTime.UtcNow > DataExpiracao.Value;
        }

        private readonly ConcurrentDictionary<string, object> _cache = new();

        public T Obter<T>(string chave) where T : class
        {
            if (_cache.TryGetValue(chave, out var item) && item is ItemCache<T> cacheItem)
            {
                if (cacheItem.EstaExpirado)
                {
                    Remover(chave);
                    return default;
                }
                
                return cacheItem.Valor;
            }
            
            return default;
        }

        public Task<T> ObterAsync<T>(string chave) where T : class
        {
            return Task.FromResult(Obter<T>(chave));
        }

        public void Definir<T>(string chave, T valor, TimeSpan? tempoExpiracao = null) where T : class
        {
            var item = new ItemCache<T>
            {
                Valor = valor,
                DataExpiracao = tempoExpiracao.HasValue ? 
                    DateTime.UtcNow.Add(tempoExpiracao.Value) : 
                    null
            };
            
            _cache[chave] = item;
        }

        public Task DefinirAsync<T>(string chave, T valor, TimeSpan? tempoExpiracao = null) where T : class
        {
            Definir(chave, valor, tempoExpiracao);
            return Task.CompletedTask;
        }

        public void Remover(string chave)
        {
            _cache.TryRemove(chave, out _);
        }

        public Task RemoverAsync(string chave)
        {
            Remover(chave);
            return Task.CompletedTask;
        }
    }
} 