using System.ComponentModel;

namespace ObterServicoAtravesAttribute.Console.Enums
{
    public enum TipoServico
    {
        [Description("Serviço de Dados Básicos")]
        DadosBasicos = 1,

        [Description("Serviço de Dados Complementares")]
        DadosComplementares = 2,

        [Description("Serviço de Dados Financeiros")]
        DadosFinanceiros = 3,

        [Description("Serviço de Dados Pessoais")]
        DadosPessoais = 4
    }
} 