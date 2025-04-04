using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using ObterServicoAtravesAttribute.Console.Servicos;
using ObterServicoAtravesAttribute.Console.Enums;
using ObterServicoAtravesAttribute.Console.Utils;

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

            while (true)
            {
                MostrarMenu();
                var opcao = LerOpcao();

                switch (opcao)
                {
                    case 1:
                        TestarServicoBasico();
                        break;
                    case 2:
                        TestarPerformance();
                        break;
                    case 3:
                        TestarCacheInstancias();
                        break;
                    case 4:
                        TestarCargaDistribuida();
                        break;
                    case 5:
                        TestarEnumUtils();
                        break;
                    case 6:
                        TestarSmtpEmail();
                        break;
                    case 7:
                        TestarParalelismo();
                        break;
                    case 8:
                        TestarParalelismoAsync().GetAwaiter().GetResult();
                        break;
                    case 0:
                        System.Console.WriteLine("\nEncerrando aplicação...");
                        return;
                    default:
                        System.Console.WriteLine("\nOpção inválida. Tente novamente.");
                        break;
                }

                System.Console.WriteLine("\nPressione qualquer tecla para continuar...");
                System.Console.ReadKey();
                System.Console.Clear();
            }
        }

        static void MostrarMenu()
        {
            System.Console.WriteLine("Selecione uma opção:");
            System.Console.WriteLine("1 - Testar serviços básicos");
            System.Console.WriteLine("2 - Testar performance");
            System.Console.WriteLine("3 - Testar cache de instâncias");
            System.Console.WriteLine("4 - Testar carga distribuída");
            System.Console.WriteLine("5 - Testar manipulação de Enums");
            System.Console.WriteLine("6 - Testar envio de e-mail SMTP");
            System.Console.WriteLine("7 - Testar cálculo de paralelismo");
            System.Console.WriteLine("8 - Testar paralelismo assíncrono (ForEachAsync)");
            System.Console.WriteLine("0 - Sair");
            System.Console.Write("\nOpção: ");
        }

        static int LerOpcao()
        {
            if (int.TryParse(System.Console.ReadLine(), out int opcao))
            {
                return opcao;
            }
            return -1;
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

        static void TestarEnumUtils()
        {
            System.Console.WriteLine("\nTestando manipulação de enums:");
            
            // Teste com valor válido
            var valorValido = 1;
            var enumValido = EnumUtils.ObterEnumPorInt<TipoServico>(valorValido);
            System.Console.WriteLine($"Valor {valorValido} corresponde ao enum: {enumValido}");
            System.Console.WriteLine($"Descrição: {EnumUtils.ObterDescricao(enumValido.Value)}");
            
            // Teste com valor inválido
            var valorInvalido = 999;
            var enumInvalido = EnumUtils.ObterEnumPorInt<TipoServico>(valorInvalido);
            System.Console.WriteLine($"\nValor {valorInvalido} corresponde ao enum: {(enumInvalido.HasValue ? enumInvalido.Value.ToString() : "null")}");
            
            // Teste com valor padrão
            var enumComPadrao = EnumUtils.ObterEnumPorInt(valorInvalido, TipoServico.DadosBasicos);
            System.Console.WriteLine($"Valor {valorInvalido} com padrão: {enumComPadrao}");
            
            // Teste de validação
            System.Console.WriteLine($"\nO valor {valorValido} é válido? {EnumUtils.EhValorValido<TipoServico>(valorValido)}");
            System.Console.WriteLine($"O valor {valorInvalido} é válido? {EnumUtils.EhValorValido<TipoServico>(valorInvalido)}");
        }

        static void TestarSmtpEmail()
        {
            System.Console.WriteLine("\nTeste de conexão SMTP e diagnóstico de problemas");
            System.Console.WriteLine("==============================================");

            System.Console.Write("Servidor SMTP: ");
            var servidor = System.Console.ReadLine();
            
            System.Console.Write("Porta (25, 465, 587): ");
            if (!int.TryParse(System.Console.ReadLine(), out var porta))
            {
                porta = 587; // Porta padrão
            }
            
            System.Console.Write("Usuário/Email: ");
            var usuario = System.Console.ReadLine();
            
            System.Console.Write("Senha: ");
            var senha = LerSenha();
            
            System.Console.Write("Usar SSL/TLS (S/N)? ");
            var usarSsl = System.Console.ReadLine()?.Trim().ToUpper().StartsWith("S") ?? true;
            
            System.Console.Write("Ignorar problemas de certificado (S/N)? ");
            var ignorarCertificado = System.Console.ReadLine()?.Trim().ToUpper().StartsWith("S") ?? false;

            try
            {
                var emailUtils = new EmailUtils(
                    servidor, 
                    porta, 
                    usuario, 
                    senha, 
                    usarSsl, 
                    timeout: 10000, 
                    ignorarCertificadoInvalido: ignorarCertificado);

                System.Console.WriteLine("\nTestando conexão com o servidor SMTP...");
                var resultado = emailUtils.TestarConexao();

                if (resultado.Sucesso)
                {
                    System.Console.WriteLine("\n✅ Conexão bem-sucedida!\n");
                    System.Console.WriteLine("Detalhes da conexão:");
                    System.Console.WriteLine(resultado.Detalhes);

                    System.Console.Write("\nDeseja enviar um e-mail de teste (S/N)? ");
                    if (System.Console.ReadLine()?.Trim().ToUpper().StartsWith("S") ?? false)
                    {
                        EnviarEmailTeste(emailUtils, usuario);
                    }
                }
                else
                {
                    System.Console.WriteLine("\n❌ Falha na conexão!\n");
                    System.Console.WriteLine("Detalhes do erro:");
                    System.Console.WriteLine(resultado.Detalhes);
                    System.Console.WriteLine("\nRecomendações:");
                    System.Console.WriteLine(resultado.ObterRecomendacoes());
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"\n❌ Erro ao configurar conexão: {ex.Message}");
            }
        }

        static void EnviarEmailTeste(EmailUtils emailUtils, string emailPadrao)
        {
            System.Console.WriteLine("\nEnvio de e-mail de teste");
            System.Console.WriteLine("=======================");

            System.Console.Write($"De (e-mail): [{emailPadrao}] ");
            var de = System.Console.ReadLine();
            if (string.IsNullOrWhiteSpace(de))
            {
                de = emailPadrao;
            }

            System.Console.Write("Para (e-mail): ");
            var para = System.Console.ReadLine();
            if (string.IsNullOrWhiteSpace(para))
            {
                para = emailPadrao;
            }

            System.Console.Write("Assunto: ");
            var assunto = System.Console.ReadLine();
            if (string.IsNullOrWhiteSpace(assunto))
            {
                assunto = "Teste de e-mail";
            }

            System.Console.WriteLine("Corpo do e-mail (termine com uma linha vazia):");
            var corpo = new System.Text.StringBuilder();
            string linha;
            while (!string.IsNullOrWhiteSpace(linha = System.Console.ReadLine()))
            {
                corpo.AppendLine(linha);
            }

            if (corpo.Length == 0)
            {
                corpo.AppendLine("Este é um e-mail de teste enviado pelo aplicativo de diagnóstico SMTP.");
                corpo.AppendLine($"Data/Hora: {DateTime.Now}");
            }

            System.Console.Write("Usar formato HTML (S/N)? ");
            var ehHtml = System.Console.ReadLine()?.Trim().ToUpper().StartsWith("S") ?? false;

            System.Console.WriteLine("\nEnviando e-mail...");
            var resultado = emailUtils.Enviar(de, para, assunto, corpo.ToString(), ehHtml);

            if (resultado.Sucesso)
            {
                System.Console.WriteLine("\n✅ E-mail enviado com sucesso!\n");
                System.Console.WriteLine("Detalhes:");
                System.Console.WriteLine(resultado.Detalhes);
            }
            else
            {
                System.Console.WriteLine("\n❌ Falha ao enviar e-mail!\n");
                System.Console.WriteLine("Detalhes do erro:");
                System.Console.WriteLine(resultado.Detalhes);
                System.Console.WriteLine("\nRecomendações:");
                System.Console.WriteLine(resultado.ObterRecomendacoes());
            }
        }

        static string LerSenha()
        {
            var senha = new System.Text.StringBuilder();
            while (true)
            {
                var tecla = System.Console.ReadKey(true);
                if (tecla.Key == ConsoleKey.Enter)
                {
                    System.Console.WriteLine();
                    break;
                }
                else if (tecla.Key == ConsoleKey.Backspace)
                {
                    if (senha.Length > 0)
                    {
                        senha.Remove(senha.Length - 1, 1);
                        System.Console.Write("\b \b");
                    }
                }
                else
                {
                    senha.Append(tecla.KeyChar);
                    System.Console.Write("*");
                }
            }
            return senha.ToString();
        }

        static void TestarParalelismo()
        {
            System.Console.WriteLine("\nTestando cálculo de paralelismo");
            System.Console.WriteLine("==============================");
            
            // Informações do sistema
            System.Console.WriteLine($"Número de processadores lógicos: {Environment.ProcessorCount}");
            
            // Demonstração de diferentes configurações
            System.Console.WriteLine("\nCálculos baseados no número de processadores (CPU-bound):");
            System.Console.WriteLine($"- 25% dos processadores: {ParallelismCalculator.CalcularGrauParalelismo(0.25)}");
            System.Console.WriteLine($"- 50% dos processadores: {ParallelismCalculator.CalcularGrauParalelismo(0.5)}");
            System.Console.WriteLine($"- 75% dos processadores (padrão): {ParallelismCalculator.CalcularGrauParalelismo()}");
            System.Console.WriteLine($"- 100% dos processadores: {ParallelismCalculator.CalcularGrauParalelismo(1.0)}");
            
            System.Console.WriteLine("\nCálculos para operações de I/O (I/O-bound):");
            System.Console.WriteLine($"- 1x processadores: {ParallelismCalculator.CalcularGrauParalelismoIO(1.0)}");
            System.Console.WriteLine($"- 2x processadores (padrão): {ParallelismCalculator.CalcularGrauParalelismoIO()}");
            System.Console.WriteLine($"- 4x processadores: {ParallelismCalculator.CalcularGrauParalelismoIO(4.0)}");
            
            System.Console.WriteLine("\nCálculo dinâmico baseado na carga do sistema:");
            System.Console.WriteLine($"- Paralelismo dinâmico: {ParallelismCalculator.CalcularGrauParalelismoDinamico()}");
            
            // Demonstração com diferentes volumes de dados
            System.Console.WriteLine("\nTamanho ideal de partição para diferentes volumes:");
            System.Console.WriteLine($"- 10 itens: {ParallelismCalculator.CalcularTamanhoParticao(10)} itens por thread");
            System.Console.WriteLine($"- 100 itens: {ParallelismCalculator.CalcularTamanhoParticao(100)} itens por thread");
            System.Console.WriteLine($"- 1000 itens: {ParallelismCalculator.CalcularTamanhoParticao(1000)} itens por thread");
            System.Console.WriteLine($"- 10000 itens: {ParallelismCalculator.CalcularTamanhoParticao(10000)} itens por thread");
            
            // Demonstração de processamento paralelo com diferentes configurações
            DemonstrarProcessamentoParalelo();
        }
        
        static void DemonstrarProcessamentoParalelo()
        {
            System.Console.WriteLine("\nComparando performance com diferentes configurações de paralelismo:");
            
            // Criar uma lista grande para processamento
            const int numeroDeItens = 10_000_000;
            var numeros = Enumerable.Range(1, numeroDeItens).ToArray();
            
            // Função simulando processamento intenso
            Func<int, long> processarItem = (numero) => {
                long resultado = 0;
                // Simula operação intensa
                for (int i = 0; i < 100; i++)
                {
                    resultado += (numero * i) % 10;
                }
                return resultado;
            };
            
            // Teste sequencial (referência)
            System.Console.WriteLine("\nProcessamento sequencial:");
            var stopwatch = Stopwatch.StartNew();
            
            long resultadoSequencial = 0;
            foreach (var numero in numeros.Take(1_000_000)) // Usamos menos para não demorar muito
            {
                resultadoSequencial += processarItem(numero);
            }
            
            stopwatch.Stop();
            System.Console.WriteLine($"- Tempo: {stopwatch.ElapsedMilliseconds}ms");
            System.Console.WriteLine($"- Resultado: {resultadoSequencial}");
            
            // Teste com diferentes configurações de paralelismo
            TestarProcessamentoParalelo(numeros, processarItem, 1, "1 thread");
            TestarProcessamentoParalelo(numeros, processarItem, Environment.ProcessorCount / 2, "50% dos processadores");
            TestarProcessamentoParalelo(numeros, processarItem, Environment.ProcessorCount, "100% dos processadores");
            TestarProcessamentoParalelo(numeros, processarItem, -1, "Automático (TPL decide)");
            
            // Teste com o ParallelismCalculator
            var options = ParallelismCalculator.CriarParallelOptionsCPU();
            System.Console.WriteLine($"\nProcessamento paralelo com ParallelismCalculator ({options.MaxDegreeOfParallelism} threads):");
            
            stopwatch.Restart();
            long resultado = 0;
            
            Parallel.ForEach(
                numeros, 
                options,
                () => 0L, // Valor inicial para cada thread
                (numero, state, threadLocal) => threadLocal + processarItem(numero), // Processamento
                threadLocal => Interlocked.Add(ref resultado, threadLocal) // Agregação final
            );
            
            stopwatch.Stop();
            System.Console.WriteLine($"- Tempo: {stopwatch.ElapsedMilliseconds}ms");
            System.Console.WriteLine($"- Resultado: {resultado}");
            System.Console.WriteLine($"- Speedup: {(double)resultadoSequencial / resultado:F2}x");
        }
        
        static void TestarProcessamentoParalelo(int[] numeros, Func<int, long> processarItem, int maxDegree, string descricao)
        {
            System.Console.WriteLine($"\nProcessamento paralelo com {descricao}:");
            
            var options = new ParallelOptions { MaxDegreeOfParallelism = maxDegree };
            var stopwatch = Stopwatch.StartNew();
            long resultado = 0;
            
            Parallel.ForEach(
                numeros, 
                options,
                () => 0L, // Valor inicial para cada thread
                (numero, state, threadLocal) => threadLocal + processarItem(numero), // Processamento
                threadLocal => Interlocked.Add(ref resultado, threadLocal) // Agregação final
            );
            
            stopwatch.Stop();
            System.Console.WriteLine($"- Tempo: {stopwatch.ElapsedMilliseconds}ms");
            System.Console.WriteLine($"- Resultado: {resultado}");
        }

        static async Task TestarParalelismoAsync()
        {
            System.Console.WriteLine("\nTestando Parallel.ForEachAsync");
            System.Console.WriteLine("=============================");
            
            // Testar diferentes aspectos do paralelismo assíncrono
            await TestarProcessamentoUrlsAsync();
            
            System.Console.WriteLine("\nTestando processamento de itens genéricos");
            await TestarProcessamentoItensAsync();
            
            System.Console.WriteLine("\nTestando processamento em lotes");
            await TestarProcessamentoLotesAsync();
            
            System.Console.WriteLine("\nTestando limitação de taxa (rate limiting)");
            await TestarRateLimitingAsync();
        }
        
        static async Task TestarProcessamentoUrlsAsync()
        {
            System.Console.WriteLine("\nProcessamento paralelo de URLs");
            System.Console.WriteLine("----------------------------");
            
            // Lista de URLs para testar
            var urls = new List<string>
            {
                "https://www.google.com",
                "https://www.microsoft.com",
                "https://www.github.com",
                "https://www.stackoverflow.com",
                "https://www.cnn.com",
                "https://www.uol.com.br",
                "https://www.globo.com",
                "https://www.amazon.com",
                "https://dotnet.microsoft.com",
                "https://docs.microsoft.com"
            };
            
            System.Console.WriteLine($"Processando {urls.Count} URLs em paralelo...");
            
            // Configurar callback de progresso
            Action<string, int, int> reportarProgresso = (url, processados, total) =>
            {
                System.Console.WriteLine($"Processado: {url} ({processados}/{total})");
            };
            
            // Usar nosso utilitário para processar as URLs
            var stopwatch = Stopwatch.StartNew();
            
            var resultados = await ParallelAsync.ProcessarUrlsAsync(
                urls,
                maxConcurrency: 4, // 4 operações paralelas
                progressCallback: reportarProgresso,
                cancellationToken: default);
            
            stopwatch.Stop();
            
            System.Console.WriteLine($"\nProcessamento concluído em {stopwatch.ElapsedMilliseconds}ms");
            System.Console.WriteLine($"Resultados obtidos: {resultados.Count}");
            
            // Mostrar estatísticas
            var sucessos = resultados.Count(r => !r.Contains("Erro"));
            var falhas = resultados.Count - sucessos;
            
            System.Console.WriteLine($"Sucessos: {sucessos}, Falhas: {falhas}");
        }
        
        static async Task TestarProcessamentoItensAsync()
        {
            System.Console.WriteLine("\nProcessamento paralelo de itens genéricos");
            System.Console.WriteLine("----------------------------------------");
            
            // Criar uma lista de números para processar
            var numeros = Enumerable.Range(1, 20).ToList();
            
            // Função para processar cada número de forma assíncrona
            async Task<int> ProcessarNumeroAsync(int numero, CancellationToken ct)
            {
                // Simular operação assíncrona que leva tempo variável
                var delay = numero * 100;
                await Task.Delay(delay, ct);
                return numero * numero;
            }
            
            System.Console.WriteLine($"Processando {numeros.Count} números em paralelo...");
            
            var stopwatch = Stopwatch.StartNew();
            
            var resultados = await ParallelAsync.ProcessarItensAsync(
                numeros,
                ProcessarNumeroAsync,
                maxConcurrency: 5);
            
            stopwatch.Stop();
            
            System.Console.WriteLine($"\nProcessamento concluído em {stopwatch.ElapsedMilliseconds}ms");
            
            // Mostrar alguns resultados
            System.Console.WriteLine("Primeiros 5 resultados:");
            foreach (var resultado in resultados.Take(5))
            {
                System.Console.WriteLine($"  {resultado}");
            }
            
            // Comparar com execução sequencial
            System.Console.WriteLine("\nComparando com execução sequencial:");
            stopwatch.Restart();
            
            var resultadosSequenciais = new List<int>();
            foreach (var numero in numeros)
            {
                var resultado = await ProcessarNumeroAsync(numero, default);
                resultadosSequenciais.Add(resultado);
            }
            
            stopwatch.Stop();
            System.Console.WriteLine($"Execução sequencial: {stopwatch.ElapsedMilliseconds}ms");
            
            // Calcular speedup
            var speedup = (double)stopwatch.ElapsedMilliseconds / resultados.Count;
            System.Console.WriteLine($"Speedup aproximado: {speedup:F2}x");
        }
        
        static async Task TestarProcessamentoLotesAsync()
        {
            System.Console.WriteLine("\nProcessamento em lotes paralelo");
            System.Console.WriteLine("-------------------------------");
            
            // Criar uma lista grande para processamento em lotes
            var items = Enumerable.Range(1, 150).ToList();
            int tamanhoLote = 10;
            
            System.Console.WriteLine($"Processando {items.Count} itens em lotes de {tamanhoLote}...");
            
            // Função para processar um lote completo
            async Task<IEnumerable<string>> ProcessarLoteAsync(IEnumerable<int> lote, CancellationToken ct)
            {
                // Processar o lote inteiro de uma vez
                var resultados = new List<string>();
                
                System.Console.WriteLine($"Processando lote com {lote.Count()} itens...");
                
                // Simular processamento assíncrono do lote inteiro
                await Task.Delay(500, ct); // Simular acesso ao banco de dados
                
                foreach (var item in lote)
                {
                    resultados.Add($"Item {item} processado no lote");
                }
                
                return resultados;
            }
            
            var stopwatch = Stopwatch.StartNew();
            
            var resultados = await ParallelAsync.ProcessarLotesAsync(
                items,
                ProcessarLoteAsync,
                maxConcurrency: 3,
                maxBatchSize: tamanhoLote);
            
            stopwatch.Stop();
            
            System.Console.WriteLine($"\nProcessamento concluído em {stopwatch.ElapsedMilliseconds}ms");
            System.Console.WriteLine($"Total de resultados: {resultados.Count}");
            
            // Mostrar alguns resultados
            System.Console.WriteLine("Primeiros 5 resultados:");
            foreach (var resultado in resultados.Take(5))
            {
                System.Console.WriteLine($"  {resultado}");
            }
        }
        
        static async Task TestarRateLimitingAsync()
        {
            System.Console.WriteLine("\nProcessamento com controle de taxa (rate limiting)");
            System.Console.WriteLine("-----------------------------------------------");
            
            // Criar uma lista de itens para processar com controle de taxa
            var itens = Enumerable.Range(1, 50).ToList();
            int maxPorSegundo = 5; // 5 requisições por segundo no máximo
            
            System.Console.WriteLine($"Processando {itens.Count} itens com limitação de {maxPorSegundo} por segundo...");
            
            // Função para processar cada item
            async Task<string> ProcessarItemComDelayAsync(int item, CancellationToken ct)
            {
                // Simular chamada a API externa
                await Task.Delay(100, ct); // Algum trabalho mínimo
                return $"Item {item} processado em {DateTime.Now:HH:mm:ss.fff}";
            }
            
            // Callback de progresso
            Action<int, int> reportarProgresso = (atual, total) =>
            {
                if (atual % 5 == 0 || atual == total)
                {
                    System.Console.WriteLine($"Progresso: {atual}/{total} ({atual * 100 / total}%)");
                }
            };
            
            var stopwatch = Stopwatch.StartNew();
            
            var resultados = await ParallelAsync.ProcessarComControleRateAsync(
                itens,
                ProcessarItemComDelayAsync,
                maxConcurrency: 10, // Máximo de 10 threads, mas apenas 5 requisições/segundo
                requestsPerSecond: maxPorSegundo,
                progressCallback: reportarProgresso);
            
            stopwatch.Stop();
            
            System.Console.WriteLine($"\nProcessamento concluído em {stopwatch.ElapsedMilliseconds}ms");
            System.Console.WriteLine($"Tempo médio por item: {stopwatch.ElapsedMilliseconds / (double)itens.Count:F2}ms");
            
            // Verificar a distribuição dos timestamps
            var timestamps = resultados.Values
                .Select(v => DateTime.Parse(v.Split(" em ")[1]))
                .OrderBy(t => t)
                .ToList();
            
            if (timestamps.Count > 0)
            {
                var primeira = timestamps.First();
                var ultima = timestamps.Last();
                var duracao = (ultima - primeira).TotalSeconds;
                var taxaEfetiva = itens.Count / duracao;
                
                System.Console.WriteLine($"\nPrimeira requisição: {primeira:HH:mm:ss.fff}");
                System.Console.WriteLine($"Última requisição: {ultima:HH:mm:ss.fff}");
                System.Console.WriteLine($"Duração total: {duracao:F2} segundos");
                System.Console.WriteLine($"Taxa efetiva: {taxaEfetiva:F2} req/s (limite configurado: {maxPorSegundo} req/s)");
            }
        }
    }
}
