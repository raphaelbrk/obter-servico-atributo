using System;
using System.Threading.Tasks;

namespace ObterServicoAtravesAttribute.Console.Cache
{
    /// <summary>
    /// Interface para implementação de cache distribuído
    /// Esta interface pode ser implementada usando Redis, Memcached ou outro provedor de cache
    /// </summary>
    public interface ICacheDistribuido
    {
        /// <summary>
        /// Obtém um item do cache
        /// </summary>
        /// <typeparam name="T">Tipo do item</typeparam>
        /// <param name="chave">Chave do item</param>
        /// <returns>O item se encontrado, ou default(T) se não existir</returns>
        T Obter<T>(string chave) where T : class;
        
        /// <summary>
        /// Obtém um item do cache de forma assíncrona
        /// </summary>
        /// <typeparam name="T">Tipo do item</typeparam>
        /// <param name="chave">Chave do item</param>
        /// <returns>O item se encontrado, ou default(T) se não existir</returns>
        Task<T> ObterAsync<T>(string chave) where T : class;
        
        /// <summary>
        /// Adiciona ou atualiza um item no cache
        /// </summary>
        /// <typeparam name="T">Tipo do item</typeparam>
        /// <param name="chave">Chave do item</param>
        /// <param name="valor">Valor a ser armazenado</param>
        /// <param name="tempoExpiracao">Tempo de expiração opcional</param>
        void Definir<T>(string chave, T valor, TimeSpan? tempoExpiracao = null) where T : class;
        
        /// <summary>
        /// Adiciona ou atualiza um item no cache de forma assíncrona
        /// </summary>
        /// <typeparam name="T">Tipo do item</typeparam>
        /// <param name="chave">Chave do item</param>
        /// <param name="valor">Valor a ser armazenado</param>
        /// <param name="tempoExpiracao">Tempo de expiração opcional</param>
        Task DefinirAsync<T>(string chave, T valor, TimeSpan? tempoExpiracao = null) where T : class;
        
        /// <summary>
        /// Remove um item do cache
        /// </summary>
        /// <param name="chave">Chave do item a ser removido</param>
        void Remover(string chave);
        
        /// <summary>
        /// Remove um item do cache de forma assíncrona
        /// </summary>
        /// <param name="chave">Chave do item a ser removido</param>
        Task RemoverAsync(string chave);
    }
} 