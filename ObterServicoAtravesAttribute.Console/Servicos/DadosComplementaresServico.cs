using System;

namespace ObterServicoAtravesAttribute.Console.Servicos
{
    [Servico("DadosComplementares")]
    public class DadosComplementaresServico : IServico
    {
        public string Executar()
        {
            return "Executando serviço de Dados Complementares";
        }
    }
} 