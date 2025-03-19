using System;
using System.Diagnostics;
using System.Threading.Tasks;
using ObterServicoAtravesAttribute.Console.Servicos;

namespace ObterServicoAtravesAttribute.Console
{
    class Program
    {
        // O gerenciador de serviços global
        private static GerenciadorServicos _gerenciadorServicos;

        static void Main(string[] args)
        {
            System.Console.WriteLine("Demonstração de obtenção de serviços através de atributos");
            System.Console.WriteLine("==========================================================");
            System.Console.WriteLine();

            // Configurando o provedor de serviços para injeção de dependência
            var serviceProvider = ConfigurarServicos();
            
            // Inicializando o gerenciador de serviços
            _gerenciadorServicos = new GerenciadorServicos(serviceProvider);

            // Teste básico
            TestarServicoBasico();
            
            // Teste de performance
            TestarPerformance();
            
            // Teste de cache local de instâncias
            TestarCacheInstancias();
            
            // Teste de carga distribuída (simulação)
            TestarCargaDistribuida();
            
            System.Console.WriteLine("\nPressione qualquer tecla para sair...");
            System.Console.ReadKey();
        }

        static SimpleServiceProvider ConfigurarServicos()
        {
            var serviceProvider = new SimpleServiceProvider();
            
            // Aqui você pode registrar implementações específicas ou mockadas para testes
            // Por exemplo:
            // serviceProvider.RegisterSingleton<IDadosBasicosServico>(new DadosBasicosMockServico());
            
            // Registrar os serviços automaticamente
            serviceProvider.Register<DadosBasicosServico, DadosBasicosServico>();
            serviceProvider.Register<DadosComplementaresServico, DadosComplementaresServico>();
            
            return serviceProvider;
        }

        static void TestarServicoBasico()
        {
            try
            {
                System.Console.WriteLine("Obtendo serviço DadosBasicos:");
                var servicoDadosBasicos = _gerenciadorServicos.ObterServico("DadosBasicos");
                System.Console.WriteLine($"Resultado: {servicoDadosBasicos.Executar()}");
                
                System.Console.WriteLine("\nObtendo serviço DadosComplementares:");
                var servicoDadosComplementares = _gerenciadorServicos.ObterServico("DadosComplementares");
                System.Console.WriteLine($"Resultado: {servicoDadosComplementares.Executar()}");
                
                System.Console.WriteLine("\nTentando obter serviço inexistente:");
                try
                {
                    var servicoInexistente = _gerenciadorServicos.ObterServico("ServicoInexistente");
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
            
            // Primeira chamada (inicialização)
            System.Console.WriteLine($"\nPrimeira chamada (inicialização):");
            var stopwatch = Stopwatch.StartNew();
            var servico = _gerenciadorServicos.ObterServico("DadosBasicos");
            stopwatch.Stop();
            System.Console.WriteLine($"Tempo para primeira chamada: {stopwatch.ElapsedMilliseconds}ms");
            
            // Teste sem reutilização de instâncias
            System.Console.WriteLine($"\nChamadas sem reutilização de instâncias ({numeroIteracoes} iterações):");
            stopwatch.Restart();
            for (int i = 0; i < numeroIteracoes; i++)
            {
                servico = _gerenciadorServicos.ObterServico("DadosBasicos", false);
            }
            stopwatch.Stop();
            System.Console.WriteLine($"Tempo total: {stopwatch.ElapsedMilliseconds}ms");
            System.Console.WriteLine($"Tempo médio por chamada: {(double)stopwatch.ElapsedMilliseconds / numeroIteracoes}ms");
            
            // Teste com reutilização de instâncias
            System.Console.WriteLine($"\nChamadas com reutilização de instâncias ({numeroIteracoes} iterações):");
            stopwatch.Restart();
            for (int i = 0; i < numeroIteracoes; i++)
            {
                servico = _gerenciadorServicos.ObterServico("DadosBasicos", true);
            }
            stopwatch.Stop();
            System.Console.WriteLine($"Tempo total: {stopwatch.ElapsedMilliseconds}ms");
            System.Console.WriteLine($"Tempo médio por chamada: {(double)stopwatch.ElapsedMilliseconds / numeroIteracoes}ms");
        }
        
        static void TestarCacheInstancias()
        {
            System.Console.WriteLine("\nTestando cache de instâncias:");
            
            // Obtendo primeira vez
            var servico1 = _gerenciadorServicos.ObterServico("DadosBasicos");
            System.Console.WriteLine($"Instância 1: {servico1.GetHashCode()}");
            
            // Obtendo outra vez (deve ser a mesma instância)
            var servico2 = _gerenciadorServicos.ObterServico("DadosBasicos");
            System.Console.WriteLine($"Instância 2: {servico2.GetHashCode()}");
            System.Console.WriteLine($"Mesma instância: {object.ReferenceEquals(servico1, servico2)}");
            
            // Limpando o cache de instâncias
            System.Console.WriteLine("Limpando cache para DadosBasicos...");
            _gerenciadorServicos.LimparInstanciaServico("DadosBasicos");
            
            // Obtendo novamente (deve ser nova instância)
            var servico3 = _gerenciadorServicos.ObterServico("DadosBasicos");
            System.Console.WriteLine($"Instância 3 (após limpeza): {servico3.GetHashCode()}");
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
                        var servico = _gerenciadorServicos.ObterServico(identificador);
                        
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
