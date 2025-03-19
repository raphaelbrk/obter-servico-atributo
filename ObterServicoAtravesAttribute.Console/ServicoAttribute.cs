using System;

namespace ObterServicoAtravesAttribute.Console
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ServicoAttribute : Attribute
    {
        public string Identificador { get; }

        public ServicoAttribute(string identificador)
        {
            Identificador = identificador ?? throw new ArgumentNullException(nameof(identificador));
        }
    }
} 