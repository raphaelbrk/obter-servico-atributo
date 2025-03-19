using System;
using System.Diagnostics;
using System.Threading.Tasks;
using ObterServicoAtravesAttribute.Console.Cache;

namespace ObterServicoAtravesAttribute.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Console.WriteLine("Demonstração de obtenção de serviços através de atributos");
            System.Console.WriteLine("==========================================================");
            System.Console.WriteLine();

            // Configurando o cache local para demonstração
            // Em produção, você usaria uma implementação para Redis ou outro cache distribuído
            ServicoFactory.ConfigurarCacheDistribuido(new CacheLocal());

            // Teste básico
            TestarServicoBasico();
            
            // Teste de performance
            TestarPerformance();
            
            // Teste de invalidação de cache
            TestarInvalidacaoCache();
            
            // Teste de carga distribuída (simulação)
            TestarCargaDistribuida();
            
            System.Console.WriteLine("\nPressione qualquer tecla para sair...");
            System.Console.ReadKey();
        }

        static void TestarServicoBasico()
        {
            try
            {
                System.Console.WriteLine("Obtendo serviço DadosBasicos:");
                var servicoDadosBasicos = ServicoFactory.ObterServico("DadosBasicos");
                System.Console.WriteLine($"Resultado: {servicoDadosBasicos.Executar()}");
                
                System.Console.WriteLine("\nObtendo serviço DadosComplementares:");
                var servicoDadosComplementares = ServicoFactory.ObterServico("DadosComplementares");
                System.Console.WriteLine($"Resultado: {servicoDadosComplementares.Executar()}");
                
                System.Console.WriteLine("\nTentando obter serviço inexistente:");
                try
                {
                    var servicoInexistente = ServicoFactory.ObterServico("ServicoInexistente");
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"Erro (esperado): {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Erro inesperado: {ex}");
            }
        }

        static void TestarPerformance()
        {
            System.Console.WriteLine("\nTestando performance:");
            
            const int numeroIteracoes = 100000;
            
            // Teste sem cache (primeira chamada)
            System.Console.WriteLine($"\nPrimeira chamada (inicialização do cache de tipos):");
            var stopwatch = Stopwatch.StartNew();
            var servico = ServicoFactory.ObterServico("DadosBasicos");
            stopwatch.Stop();
            System.Console.WriteLine($"Tempo para primeira chamada: {stopwatch.ElapsedMilliseconds}ms");
            
            // Teste com cache de tipos mas sem cache de instâncias
            System.Console.WriteLine($"\nChamadas sem cache de instâncias ({numeroIteracoes} iterações):");
            stopwatch.Restart();
            for (int i = 0; i < numeroIteracoes; i++)
            {
                servico = ServicoFactory.ObterServico("DadosBasicos", false);
            }
            stopwatch.Stop();
            System.Console.WriteLine($"Tempo total: {stopwatch.ElapsedMilliseconds}ms");
            System.Console.WriteLine($"Tempo médio por chamada: {(double)stopwatch.ElapsedMilliseconds / numeroIteracoes}ms");
            
            // Teste com cache completo
            System.Console.WriteLine($"\nChamadas com cache completo ({numeroIteracoes} iterações):");
            stopwatch.Restart();
            for (int i = 0; i < numeroIteracoes; i++)
            {
                servico = ServicoFactory.ObterServico("DadosBasicos", true);
            }
            stopwatch.Stop();
            System.Console.WriteLine($"Tempo total: {stopwatch.ElapsedMilliseconds}ms");
            System.Console.WriteLine($"Tempo médio por chamada: {(double)stopwatch.ElapsedMilliseconds / numeroIteracoes}ms");
        }
        
        static void TestarInvalidacaoCache()
        {
            System.Console.WriteLine("\nTestando invalidação de cache:");
            
            // Obtendo primeira vez
            var servico1 = ServicoFactory.ObterServico("DadosBasicos");
            System.Console.WriteLine($"Instância 1: {servico1.GetHashCode()}");
            
            // Obtendo outra vez (deve ser a mesma instância)
            var servico2 = ServicoFactory.ObterServico("DadosBasicos");
            System.Console.WriteLine($"Instância 2: {servico2.GetHashCode()}");
            System.Console.WriteLine($"Mesma instância: {object.ReferenceEquals(servico1, servico2)}");
            
            // Invalidando cache
            System.Console.WriteLine("Invalidando cache para DadosBasicos...");
            ServicoFactory.InvalidarCacheServico("DadosBasicos");
            
            // Obtendo novamente (deve ser nova instância)
            var servico3 = ServicoFactory.ObterServico("DadosBasicos");
            System.Console.WriteLine($"Instância 3 (após invalidação): {servico3.GetHashCode()}");
            System.Console.WriteLine($"Mesma instância que a anterior: {object.ReferenceEquals(servico2, servico3)}");
        }
        
        static void TestarCargaDistribuida()
        {
            System.Console.WriteLine("\nSimulando carga distribuída (processamento paralelo):");
            
            const int numThreads = 10;
            const int iteracoesPorThread = 1000;
            
            var tasks = new Task[numThreads];
            var stopwatch = Stopwatch.StartNew();
            
            for (int i = 0; i < numThreads; i++)
            {
                int threadId = i;
                tasks[i] = Task.Run(() => {
                    for (int j = 0; j < iteracoesPorThread; j++)
                    {
                        // Alternar entre os dois serviços
                        var identificador = j % 2 == 0 ? "DadosBasicos" : "DadosComplementares";
                        var servico = ServicoFactory.ObterServico(identificador);
                        
                        // Simulando alguma operação com o serviço
                        var resultado = servico.Executar();
                    }
                    System.Console.WriteLine($"Thread {threadId} concluída.");
                });
            }
            
            Task.WaitAll(tasks);
            stopwatch.Stop();
            
            System.Console.WriteLine($"Processamento paralelo concluído em {stopwatch.ElapsedMilliseconds}ms");
            System.Console.WriteLine($"Total de operações: {numThreads * iteracoesPorThread}");
            System.Console.WriteLine($"Média por operação: {(double)stopwatch.ElapsedMilliseconds / (numThreads * iteracoesPorThread)}ms");
        }
    }
}
