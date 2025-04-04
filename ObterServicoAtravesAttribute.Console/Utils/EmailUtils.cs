using System;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace ObterServicoAtravesAttribute.Console.Utils
{
    /// <summary>
    /// Classe utilitária para envio de e-mails com diagnóstico de problemas
    /// </summary>
    public class EmailUtils
    {
        private readonly string _servidor;
        private readonly int _porta;
        private readonly string _usuario;
        private readonly string _senha;
        private readonly bool _usarSsl;
        private readonly int _timeout;
        private readonly bool _ignorarCertificadoInvalido;

        /// <summary>
        /// Construtor da classe utilitária de e-mail
        /// </summary>
        /// <param name="servidor">Servidor SMTP (ex: smtp.gmail.com)</param>
        /// <param name="porta">Porta do servidor (geralmente 25, 465 ou 587)</param>
        /// <param name="usuario">Usuário para autenticação</param>
        /// <param name="senha">Senha para autenticação</param>
        /// <param name="usarSsl">Se deve usar SSL/TLS (recomendado)</param>
        /// <param name="timeout">Timeout em milissegundos (padrão 30000 = 30 segundos)</param>
        /// <param name="ignorarCertificadoInvalido">Se deve ignorar problemas com certificados SSL</param>
        public EmailUtils(
            string servidor,
            int porta,
            string usuario,
            string senha,
            bool usarSsl = true,
            int timeout = 30000,
            bool ignorarCertificadoInvalido = false)
        {
            _servidor = servidor ?? throw new ArgumentNullException(nameof(servidor));
            _porta = porta;
            _usuario = usuario ?? throw new ArgumentNullException(nameof(usuario));
            _senha = senha ?? throw new ArgumentNullException(nameof(senha));
            _usarSsl = usarSsl;
            _timeout = timeout;
            _ignorarCertificadoInvalido = ignorarCertificadoInvalido;
        }

        /// <summary>
        /// Envia um e-mail com tratamento de erro e diagnóstico detalhado
        /// </summary>
        /// <param name="de">Endereço de e-mail do remetente</param>
        /// <param name="para">Endereço(s) de e-mail do(s) destinatário(s)</param>
        /// <param name="assunto">Assunto do e-mail</param>
        /// <param name="corpo">Corpo do e-mail</param>
        /// <param name="ehHtml">Se o corpo do e-mail está em formato HTML</param>
        /// <returns>Resultado detalhado da operação</returns>
        public ResultadoEnvioEmail Enviar(
            string de,
            string para,
            string assunto,
            string corpo,
            bool ehHtml = false)
        {
            var resultado = new ResultadoEnvioEmail();
            var detalhes = new StringBuilder();

            try
            {
                detalhes.AppendLine("Iniciando configuração do cliente SMTP...");
                detalhes.AppendLine($"Servidor: {_servidor}");
                detalhes.AppendLine($"Porta: {_porta}");
                detalhes.AppendLine($"Usar SSL: {_usarSsl}");
                detalhes.AppendLine($"Timeout: {_timeout}ms");
                detalhes.AppendLine($"Ignorar certificado inválido: {_ignorarCertificadoInvalido}");

                // Configurar certificado SSL personalizado se necessário
                if (_ignorarCertificadoInvalido)
                {
                    detalhes.AppendLine("Configurando validação de certificado personalizada...");
                    ServicePointManager.ServerCertificateValidationCallback = ValidarCertificado;
                }

                // Configurar cliente SMTP
                using var smtpClient = new SmtpClient(_servidor, _porta)
                {
                    EnableSsl = _usarSsl,
                    Timeout = _timeout,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(_usuario, _senha)
                };

                detalhes.AppendLine("Configurando mensagem de e-mail...");
                detalhes.AppendLine($"De: {de}");
                detalhes.AppendLine($"Para: {para}");
                detalhes.AppendLine($"Assunto: {assunto}");
                detalhes.AppendLine($"É HTML: {ehHtml}");

                // Configurar mensagem
                using var mensagem = new MailMessage(de, para, assunto, corpo)
                {
                    IsBodyHtml = ehHtml
                };

                detalhes.AppendLine("Enviando e-mail...");
                smtpClient.Send(mensagem);

                detalhes.AppendLine("E-mail enviado com sucesso!");
                resultado.Sucesso = true;
            }
            catch (SmtpException ex)
            {
                detalhes.AppendLine($"Erro SMTP: {ex.Message}");
                detalhes.AppendLine($"Status: {ex.StatusCode}");
                detalhes.AppendLine($"Stack Trace: {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    detalhes.AppendLine($"Erro interno: {ex.InnerException.Message}");
                    detalhes.AppendLine($"Tipo: {ex.InnerException.GetType().Name}");
                }

                resultado.Sucesso = false;
                resultado.Erro = ex;
            }
            catch (Exception ex)
            {
                detalhes.AppendLine($"Erro geral: {ex.Message}");
                detalhes.AppendLine($"Tipo: {ex.GetType().Name}");
                detalhes.AppendLine($"Stack Trace: {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    detalhes.AppendLine($"Erro interno: {ex.InnerException.Message}");
                    detalhes.AppendLine($"Tipo: {ex.InnerException.GetType().Name}");
                }

                resultado.Sucesso = false;
                resultado.Erro = ex;
            }
            finally
            {
                // Restaurar validação de certificado padrão
                if (_ignorarCertificadoInvalido)
                {
                    ServicePointManager.ServerCertificateValidationCallback = null;
                }
            }

            resultado.Detalhes = detalhes.ToString();
            return resultado;
        }

        /// <summary>
        /// Testa a conexão com o servidor SMTP
        /// </summary>
        /// <returns>Resultado detalhado do teste</returns>
        public ResultadoEnvioEmail TestarConexao()
        {
            var resultado = new ResultadoEnvioEmail();
            var detalhes = new StringBuilder();

            try
            {
                detalhes.AppendLine("Testando conexão com servidor SMTP...");
                detalhes.AppendLine($"Servidor: {_servidor}");
                detalhes.AppendLine($"Porta: {_porta}");

                // Testar conectividade básica
                using var client = new System.Net.Sockets.TcpClient();
                var connectResult = client.BeginConnect(_servidor, _porta, null, null);
                var success = connectResult.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(5));

                if (!success)
                {
                    detalhes.AppendLine("Falha na conexão TCP básica. Possíveis causas:");
                    detalhes.AppendLine("- Servidor inacessível");
                    detalhes.AppendLine("- Porta bloqueada por firewall");
                    detalhes.AppendLine("- Nome de servidor incorreto");
                    resultado.Sucesso = false;
                    return resultado;
                }

                detalhes.AppendLine("Conexão TCP estabelecida com sucesso.");
                client.EndConnect(connectResult);

                // Testar autenticação
                detalhes.AppendLine("Testando autenticação SMTP...");
                using var smtpClient = new SmtpClient(_servidor, _porta)
                {
                    EnableSsl = _usarSsl,
                    Timeout = _timeout,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(_usuario, _senha)
                };

                // A maioria dos servidores SMTP não permitem NOOP sem autenticação completa
                // Então fazemos uma operação mínima para testar a autenticação
                var testMessage = new MailMessage(
                    new MailAddress(_usuario),
                    new MailAddress(_usuario)
                )
                {
                    Subject = "Test",
                    Body = "Test"
                };

                detalhes.AppendLine("Tentando autenticação...");
                smtpClient.Send(testMessage);
                detalhes.AppendLine("Autenticação bem-sucedida!");

                resultado.Sucesso = true;
            }
            catch (SmtpException ex)
            {
                detalhes.AppendLine($"Erro SMTP: {ex.Message}");
                detalhes.AppendLine($"Status: {ex.StatusCode}");

                switch (ex.StatusCode)
                {
                    case SmtpStatusCode.ServiceNotAvailable:
                        detalhes.AppendLine("Serviço indisponível. O servidor pode estar fora do ar ou bloqueando conexões.");
                        break;
                    case SmtpStatusCode.AuthenticationFailed:
                        detalhes.AppendLine("Falha na autenticação. Verifique usuário e senha.");
                        break;
                    case SmtpStatusCode.MustIssueStartTlsFirst:
                        detalhes.AppendLine("O servidor requer conexão segura. Habilite SSL/TLS.");
                        break;
                    case SmtpStatusCode.GeneralFailure:
                        detalhes.AppendLine("Falha geral. Verifique as configurações e tente novamente.");
                        break;
                }

                if (ex.InnerException != null)
                {
                    detalhes.AppendLine($"Erro interno: {ex.InnerException.Message}");
                    if (ex.InnerException is System.IO.IOException)
                    {
                        detalhes.AppendLine("Erro de I/O. A conexão pode ter sido fechada pelo servidor ou por um firewall.");
                    }
                    else if (ex.InnerException is System.Security.Authentication.AuthenticationException)
                    {
                        detalhes.AppendLine("Erro de autenticação SSL. Possíveis causas:");
                        detalhes.AppendLine("- Certificado inválido ou expirado");
                        detalhes.AppendLine("- Versão SSL/TLS incompatível");
                        detalhes.AppendLine("- Problema com a configuração de segurança do servidor");
                    }
                }

                resultado.Sucesso = false;
                resultado.Erro = ex;
            }
            catch (Exception ex)
            {
                detalhes.AppendLine($"Erro geral: {ex.Message}");
                detalhes.AppendLine($"Tipo: {ex.GetType().Name}");

                resultado.Sucesso = false;
                resultado.Erro = ex;
            }

            resultado.Detalhes = detalhes.ToString();
            return resultado;
        }

        /// <summary>
        /// Função para validar certificado SSL (usado quando ignorarCertificadoInvalido = true)
        /// </summary>
        private bool ValidarCertificado(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            // Aceitar qualquer certificado quando configurado para ignorar problemas
            return true;
        }

        /// <summary>
        /// Gera recomendações com base em uma exceção de SMTP
        /// </summary>
        public static string GerarRecomendacoes(Exception ex)
        {
            var recomendacoes = new StringBuilder();
            recomendacoes.AppendLine("Recomendações para resolver o problema:");

            if (ex is SmtpException smtpEx)
            {
                switch (smtpEx.StatusCode)
                {
                    case SmtpStatusCode.ServiceNotAvailable:
                        recomendacoes.AppendLine("1. Verifique se o servidor SMTP está ativo e respondendo");
                        recomendacoes.AppendLine("2. Verifique se a porta utilizada está correta");
                        recomendacoes.AppendLine("3. Verifique se não há bloqueios de firewall ou proxy");
                        recomendacoes.AppendLine("4. Tente usar outra porta (25, 465 ou 587)");
                        break;

                    case SmtpStatusCode.AuthenticationFailed:
                        recomendacoes.AppendLine("1. Verifique se o usuário e senha estão corretos");
                        recomendacoes.AppendLine("2. Para contas Google, ative 'Permitir apps menos seguros' ou use senhas de app");
                        recomendacoes.AppendLine("3. Verifique se não há bloqueio por autenticação de dois fatores");
                        recomendacoes.AppendLine("4. Tente fazer login manualmente no webmail para verificar se a conta está normal");
                        break;

                    case SmtpStatusCode.MustIssueStartTlsFirst:
                        recomendacoes.AppendLine("1. Habilite SSL/TLS nas configurações (usarSsl = true)");
                        recomendacoes.AppendLine("2. Use a porta correta para conexões seguras (geralmente 465 ou 587)");
                        break;

                    default:
                        recomendacoes.AppendLine("1. Verifique se o servidor SMTP está correto");
                        recomendacoes.AppendLine("2. Verifique se a porta está correta");
                        recomendacoes.AppendLine("3. Verifique credenciais de acesso");
                        recomendacoes.AppendLine("4. Teste com outro provedor de e-mail para isolar o problema");
                        break;
                }
            }
            else if (ex is System.IO.IOException)
            {
                recomendacoes.AppendLine("1. Verifique sua conexão com a internet");
                recomendacoes.AppendLine("2. Aumente o valor de timeout");
                recomendacoes.AppendLine("3. Verifique se o servidor não está bloqueando sua conexão");
                recomendacoes.AppendLine("4. Verifique se não há firewall bloqueando a conexão");
            }
            else if (ex is System.Security.Authentication.AuthenticationException)
            {
                recomendacoes.AppendLine("1. Experimente definir ignorarCertificadoInvalido = true");
                recomendacoes.AppendLine("2. Atualize seu sistema operacional e certificados");
                recomendacoes.AppendLine("3. Verifique se o servidor de e-mail e certificados estão válidos");
            }
            else
            {
                recomendacoes.AppendLine("1. Verifique todas as configurações de conexão SMTP");
                recomendacoes.AppendLine("2. Teste com diferentes portas (25, 465, 587)");
                recomendacoes.AppendLine("3. Verifique se o provedor permite envio SMTP externo");
                recomendacoes.AppendLine("4. Tente usando outro cliente de e-mail para isolar o problema");
            }

            return recomendacoes.ToString();
        }
    }

    /// <summary>
    /// Classe que armazena o resultado de uma operação de envio de e-mail
    /// </summary>
    public class ResultadoEnvioEmail
    {
        /// <summary>
        /// Indica se a operação foi bem-sucedida
        /// </summary>
        public bool Sucesso { get; set; }

        /// <summary>
        /// Exceção que ocorreu durante a operação, se houver
        /// </summary>
        public Exception Erro { get; set; }

        /// <summary>
        /// Detalhes técnicos sobre a operação
        /// </summary>
        public string Detalhes { get; set; }

        /// <summary>
        /// Obtém recomendações para resolver o problema, se houver
        /// </summary>
        /// <returns>Texto com recomendações</returns>
        public string ObterRecomendacoes()
        {
            if (Sucesso || Erro == null)
            {
                return "Operação bem-sucedida. Nenhuma recomendação necessária.";
            }

            return EmailUtils.GerarRecomendacoes(Erro);
        }
    }
} 