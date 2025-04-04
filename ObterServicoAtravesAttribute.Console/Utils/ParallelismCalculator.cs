using System;
using System.Threading;
using System.Threading.Tasks;

namespace ObterServicoAtravesAttribute.Console.Utils
{
    /// <summary>
    /// Utilitário para calcular configurações ideais de paralelismo
    /// </summary>
    public static class ParallelismCalculator
    {
        /// <summary>
        /// Calcula o grau ideal de paralelismo baseado no número de processadores
        /// </summary>
        /// <param name="percentualProcessadores">Percentual dos processadores a utilizar (0.0 a 1.0)</param>
        /// <param name="minimo">Valor mínimo de paralelismo</param>
        /// <param name="maximo">Valor máximo de paralelismo</param>
        /// <returns>Número ideal de tarefas paralelas</returns>
        public static int CalcularGrauParalelismo(
            double percentualProcessadores = 0.75, 
            int minimo = 1, 
            int maximo = 16)
        {
            // Obtém o número de processadores lógicos (threads)
            int processadores = Environment.ProcessorCount;
            
            // Calcula o número ideal baseado no percentual
            int ideal = Math.Max(1, (int)Math.Round(processadores * percentualProcessadores));
            
            // Garante que esteja dentro dos limites especificados
            return Math.Min(maximo, Math.Max(minimo, ideal));
        }
        
        /// <summary>
        /// Calcula o grau ideal de paralelismo para tarefas com I/O
        /// </summary>
        /// <param name="multiplicador">Multiplicador de núcleos para tarefas de I/O (recomendado entre 1 e 4)</param>
        /// <param name="maximo">Limite máximo de paralelismo</param>
        /// <returns>Número ideal de tarefas paralelas para operações de I/O</returns>
        public static int CalcularGrauParalelismoIO(double multiplicador = 2.0, int maximo = 32)
        {
            // Para tarefas de I/O podemos usar mais threads do que processadores físicos
            // pois elas passam boa parte do tempo aguardando
            int processadores = Environment.ProcessorCount;
            int ideal = (int)Math.Round(processadores * multiplicador);
            
            return Math.Min(maximo, ideal);
        }
        
        /// <summary>
        /// Cria um objeto ParallelOptions configurado para o grau de paralelismo ideal para CPU
        /// </summary>
        /// <param name="percentualProcessadores">Percentual dos processadores a utilizar (0.0 a 1.0)</param>
        /// <param name="cancellationToken">Token de cancelamento opcional</param>
        /// <returns>ParallelOptions configurado</returns>
        public static ParallelOptions CriarParallelOptionsCPU(
            double percentualProcessadores = 0.75,
            CancellationToken cancellationToken = default)
        {
            return new ParallelOptions
            {
                MaxDegreeOfParallelism = CalcularGrauParalelismo(percentualProcessadores),
                CancellationToken = cancellationToken
            };
        }
        
        /// <summary>
        /// Cria um objeto ParallelOptions configurado para o grau de paralelismo ideal para I/O
        /// </summary>
        /// <param name="multiplicador">Multiplicador de núcleos para tarefas de I/O (recomendado entre 1 e 4)</param>
        /// <param name="cancellationToken">Token de cancelamento opcional</param>
        /// <returns>ParallelOptions configurado</returns>
        public static ParallelOptions CriarParallelOptionsIO(
            double multiplicador = 2.0,
            CancellationToken cancellationToken = default)
        {
            return new ParallelOptions
            {
                MaxDegreeOfParallelism = CalcularGrauParalelismoIO(multiplicador),
                CancellationToken = cancellationToken
            };
        }
        
        /// <summary>
        /// Calcula o tamanho ideal de partição para processamento em lote
        /// </summary>
        /// <param name="totalItens">Número total de itens a processar</param>
        /// <param name="idealPorThread">Número ideal de itens por thread</param>
        /// <returns>Tamanho ideal de partição</returns>
        public static int CalcularTamanhoParticao(int totalItens, int idealPorThread = 100)
        {
            int processadores = Environment.ProcessorCount;
            
            // Se tivermos poucos itens, não vale a pena particionar muito
            if (totalItens <= processadores * 10)
            {
                return Math.Max(1, totalItens / processadores);
            }
            
            // Para volumes maiores, tentamos equalizar o trabalho
            return Math.Max(1, Math.Min(idealPorThread, totalItens / processadores));
        }
        
        /// <summary>
        /// Estima o grau ideal de paralelismo com base na carga atual do sistema
        /// </summary>
        /// <param name="percentualMax">Percentual máximo de processadores a utilizar quando o sistema estiver ocioso</param>
        /// <param name="percentualMin">Percentual mínimo de processadores a utilizar quando o sistema estiver sobrecarregado</param>
        /// <returns>Grau de paralelismo ajustado à carga atual</returns>
        public static int CalcularGrauParalelismoDinamico(double percentualMax = 0.9, double percentualMin = 0.3)
        {
            try
            {
                // Esta abordagem é aproximada e serve apenas como demonstração
                // Em produção, considere usar Performance Counters para medições mais precisas
                
                // Fazemos uma pequena pausa para obter a diferença na carga
                var initialCpuTime = TimeSpan.FromTicks(Environment.TickCount64 * TimeSpan.TicksPerMillisecond);
                Thread.Sleep(100);
                var elapsedCpuTime = TimeSpan.FromTicks(Environment.TickCount64 * TimeSpan.TicksPerMillisecond) - initialCpuTime;
                
                // Estimativa rudimentar da carga (melhor usar Performance Counters reais)
                var cargaEstimada = elapsedCpuTime.TotalMilliseconds / 100.0;
                cargaEstimada = Math.Max(0.0, Math.Min(1.0, cargaEstimada));
                
                // Ajusta o percentual com base na carga
                var percentualAjustado = percentualMax - ((percentualMax - percentualMin) * cargaEstimada);
                
                return CalcularGrauParalelismo(percentualAjustado);
            }
            catch
            {
                // Em caso de erro, usa uma abordagem segura
                return CalcularGrauParalelismo(0.5);
            }
        }
    }
} 