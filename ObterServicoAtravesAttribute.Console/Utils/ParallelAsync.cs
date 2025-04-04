using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ObterServicoAtravesAttribute.Console.Utils
{
    /// <summary>
    /// Classe demonstrativa do uso de Parallel.ForEachAsync para operações assíncronas paralelas
    /// </summary>
    public static class ParallelAsync
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        
        /// <summary>
        /// Processa uma coleção de URLs de forma paralela e assíncrona
        /// </summary>
        /// <param name="urls">Lista de URLs para processar</param>
        /// <param name="maxConcurrency">Máximo de operações paralelas</param>
        /// <param name="progressCallback">Callback para reportar progresso (opcional)</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Lista com resultados do processamento</returns>
        public static async Task<List<string>> ProcessarUrlsAsync(
            IEnumerable<string> urls, 
            int maxConcurrency = 0,
            Action<string, int, int> progressCallback = null,
            CancellationToken cancellationToken = default)
        {
            // Se maxConcurrency não for especificado, use a configuração recomendada para I/O
            if (maxConcurrency <= 0)
            {
                maxConcurrency = ParallelismCalculator.CalcularGrauParalelismoIO();
            }
            
            var resultados = new List<string>();
            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = maxConcurrency,
                CancellationToken = cancellationToken
            };
            
            var urlsList = urls.ToList();
            var totalUrls = urlsList.Count;
            var processados = 0;
            
            // Semáforo para sincronização ao adicionar resultados
            using var semaforo = new SemaphoreSlim(1, 1);
            
            await Parallel.ForEachAsync(urlsList, options, async (url, ct) =>
            {
                try
                {
                    // Processa a URL
                    var resultado = await ProcessarUrlAsync(url, ct);
                    
                    // Sincroniza acesso à lista de resultados
                    await semaforo.WaitAsync(ct);
                    try
                    {
                        resultados.Add(resultado);
                        
                        // Atualiza contador e reporta progresso
                        Interlocked.Increment(ref processados);
                        progressCallback?.Invoke(url, processados, totalUrls);
                    }
                    finally
                    {
                        semaforo.Release();
                    }
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    // Em caso de erro, adiciona informação do erro
                    await semaforo.WaitAsync(ct);
                    try
                    {
                        resultados.Add($"Erro processando {url}: {ex.Message}");
                        Interlocked.Increment(ref processados);
                        progressCallback?.Invoke(url, processados, totalUrls);
                    }
                    finally
                    {
                        semaforo.Release();
                    }
                }
            });
            
            return resultados;
        }
        
        /// <summary>
        /// Processa uma URL individualmente (exemplo de operação assíncrona)
        /// </summary>
        private static async Task<string> ProcessarUrlAsync(string url, CancellationToken ct)
        {
            try
            {
                var response = await _httpClient.GetAsync(url, ct);
                response.EnsureSuccessStatusCode();
                var conteudo = await response.Content.ReadAsStringAsync(ct);
                
                // Retorna alguns dados da resposta
                return $"URL: {url}, Status: {response.StatusCode}, Tamanho: {conteudo.Length} bytes";
            }
            catch (HttpRequestException ex)
            {
                return $"Erro HTTP em {url}: {ex.Message}";
            }
        }
        
        /// <summary>
        /// Exemplo de processamento em lote de forma assíncrona e paralela
        /// </summary>
        /// <typeparam name="T">Tipo dos itens de entrada</typeparam>
        /// <typeparam name="TResult">Tipo do resultado</typeparam>
        /// <param name="items">Itens para processar</param>
        /// <param name="processarItemAsync">Função para processar cada item</param>
        /// <param name="maxConcurrency">Número máximo de operações paralelas</param>
        /// <param name="maxBatchSize">Tamanho máximo de cada lote</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Lista com resultados</returns>
        public static async Task<List<TResult>> ProcessarLotesAsync<T, TResult>(
            IEnumerable<T> items,
            Func<IEnumerable<T>, CancellationToken, Task<IEnumerable<TResult>>> processarItemAsync,
            int maxConcurrency = 0,
            int maxBatchSize = 100,
            CancellationToken cancellationToken = default)
        {
            if (maxConcurrency <= 0)
            {
                maxConcurrency = ParallelismCalculator.CalcularGrauParalelismoIO();
            }
            
            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = maxConcurrency,
                CancellationToken = cancellationToken
            };
            
            // Particionar os itens em lotes
            var lotes = ParticionarEmLotes(items, maxBatchSize).ToList();
            
            var resultados = new List<TResult>();
            using var semaforo = new SemaphoreSlim(1, 1);
            
            await Parallel.ForEachAsync(lotes, options, async (lote, ct) =>
            {
                var resultadosLote = await processarItemAsync(lote, ct);
                
                await semaforo.WaitAsync(ct);
                try
                {
                    resultados.AddRange(resultadosLote);
                }
                finally
                {
                    semaforo.Release();
                }
            });
            
            return resultados;
        }
        
        /// <summary>
        /// Divide uma coleção em lotes de tamanho especificado
        /// </summary>
        private static IEnumerable<IEnumerable<T>> ParticionarEmLotes<T>(IEnumerable<T> source, int tamanhoLote)
        {
            var loteAtual = new List<T>(tamanhoLote);
            
            foreach (var item in source)
            {
                loteAtual.Add(item);
                
                if (loteAtual.Count == tamanhoLote)
                {
                    yield return loteAtual;
                    loteAtual = new List<T>(tamanhoLote);
                }
            }
            
            if (loteAtual.Count > 0)
            {
                yield return loteAtual;
            }
        }
        
        /// <summary>
        /// Executa várias tarefas assíncronas paralelamente com limite de concorrência
        /// </summary>
        /// <typeparam name="T">Tipo de entrada</typeparam>
        /// <typeparam name="TResult">Tipo do resultado</typeparam>
        /// <param name="items">Itens para processar</param>
        /// <param name="funcAsync">Função assíncrona para processar cada item</param>
        /// <param name="maxConcurrency">Máximo de operações paralelas</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Lista com resultados</returns>
        public static async Task<List<TResult>> ProcessarItensAsync<T, TResult>(
            IEnumerable<T> items,
            Func<T, CancellationToken, Task<TResult>> funcAsync,
            int maxConcurrency = 0,
            CancellationToken cancellationToken = default)
        {
            if (maxConcurrency <= 0)
            {
                maxConcurrency = ParallelismCalculator.CalcularGrauParalelismoIO();
            }
            
            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = maxConcurrency,
                CancellationToken = cancellationToken
            };
            
            var resultados = new List<TResult>();
            using var semaforo = new SemaphoreSlim(1, 1);
            
            await Parallel.ForEachAsync(items, options, async (item, ct) =>
            {
                var resultado = await funcAsync(item, ct);
                
                await semaforo.WaitAsync(ct);
                try
                {
                    resultados.Add(resultado);
                }
                finally
                {
                    semaforo.Release();
                }
            });
            
            return resultados;
        }
        
        /// <summary>
        /// Implementação avançada para execução de trabalhos assíncronos com controle de taxa e limite
        /// </summary>
        /// <typeparam name="T">Tipo de entrada</typeparam>
        /// <typeparam name="TResult">Tipo do resultado</typeparam>
        /// <param name="items">Itens para processar</param>
        /// <param name="funcAsync">Função assíncrona para processar cada item</param>
        /// <param name="maxConcurrency">Máximo de operações paralelas</param>
        /// <param name="requestsPerSecond">Máximo de requisições por segundo (0 = ilimitado)</param>
        /// <param name="progressCallback">Callback para reportar progresso</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Dicionário com resultados</returns>
        public static async Task<Dictionary<T, TResult>> ProcessarComControleRateAsync<T, TResult>(
            IEnumerable<T> items,
            Func<T, CancellationToken, Task<TResult>> funcAsync,
            int maxConcurrency = 0,
            int requestsPerSecond = 0,
            Action<int, int> progressCallback = null,
            CancellationToken cancellationToken = default)
        {
            if (maxConcurrency <= 0)
            {
                maxConcurrency = ParallelismCalculator.CalcularGrauParalelismoIO();
            }
            
            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = maxConcurrency,
                CancellationToken = cancellationToken
            };
            
            var resultados = new Dictionary<T, TResult>();
            using var semaforo = new SemaphoreSlim(1, 1);
            
            // Se houver controle de taxa, preparamos o semáforo específico
            SemaphoreSlim rateLimiter = null;
            Timer rateTimer = null;
            
            if (requestsPerSecond > 0)
            {
                rateLimiter = new SemaphoreSlim(requestsPerSecond, requestsPerSecond);
                
                // Repõe tokens a cada segundo
                rateTimer = new Timer(state => 
                {
                    var sem = (SemaphoreSlim)state;
                    var count = requestsPerSecond - sem.CurrentCount;
                    if (count > 0)
                    {
                        sem.Release(count);
                    }
                }, rateLimiter, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
            }
            
            var itemsList = items.ToList();
            var totalItems = itemsList.Count;
            var processados = 0;
            
            try
            {
                await Parallel.ForEachAsync(itemsList, options, async (item, ct) =>
                {
                    // Se houver rate limiting, aguarda um token
                    if (rateLimiter != null)
                    {
                        await rateLimiter.WaitAsync(ct);
                    }
                    
                    try
                    {
                        var resultado = await funcAsync(item, ct);
                        
                        await semaforo.WaitAsync(ct);
                        try
                        {
                            resultados[item] = resultado;
                            
                            // Atualiza progresso
                            var atual = Interlocked.Increment(ref processados);
                            progressCallback?.Invoke(atual, totalItems);
                        }
                        finally
                        {
                            semaforo.Release();
                        }
                    }
                    catch (Exception) when (rateLimiter != null)
                    {
                        // Em caso de erro, devolve o token para o rate limiter
                        rateLimiter.Release();
                        throw;
                    }
                });
                
                return resultados;
            }
            finally
            {
                rateTimer?.Dispose();
                rateLimiter?.Dispose();
            }
        }
    }
} 