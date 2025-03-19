using System;

namespace ObterServicoAtravesAttribute.Console.Servicos
{
    [Servico("DadosBasicos")]
    public class DadosBasicosServico : IServico
    {
        public string Executar()
        {
            return "Executando serviço de Dados Básicos";
        }
    }
} 