using System;
using System.ComponentModel;

namespace ObterServicoAtravesAttribute.Console.Utils
{
    /// <summary>
    /// Classe utilitária para manipulação de enums
    /// </summary>
    public static class EnumUtils
    {
        /// <summary>
        /// Tenta obter um valor de enum a partir de um número inteiro
        /// </summary>
        /// <typeparam name="TEnum">Tipo do enum</typeparam>
        /// <param name="valor">Valor inteiro a ser convertido</param>
        /// <param name="valorPadrao">Valor padrão caso a conversão falhe</param>
        /// <returns>Valor do enum correspondente ou o valor padrão se não encontrar</returns>
        public static TEnum ObterEnumPorInt<TEnum>(int valor, TEnum valorPadrao) where TEnum : struct
        {
            if (!typeof(TEnum).IsEnum)
            {
                throw new ArgumentException($"O tipo {typeof(TEnum).Name} não é um enum");
            }

            // Verifica se o valor existe no enum
            if (Enum.IsDefined(typeof(TEnum), valor))
            {
                return (TEnum)Enum.ToObject(typeof(TEnum), valor);
            }

            return valorPadrao;
        }

        /// <summary>
        /// Tenta obter um valor de enum a partir de um número inteiro
        /// </summary>
        /// <typeparam name="TEnum">Tipo do enum</typeparam>
        /// <param name="valor">Valor inteiro a ser convertido</param>
        /// <returns>Valor do enum correspondente ou null se não encontrar</returns>
        public static TEnum? ObterEnumPorInt<TEnum>(int valor) where TEnum : struct
        {
            if (!typeof(TEnum).IsEnum)
            {
                throw new ArgumentException($"O tipo {typeof(TEnum).Name} não é um enum");
            }

            // Verifica se o valor existe no enum
            if (Enum.IsDefined(typeof(TEnum), valor))
            {
                return (TEnum)Enum.ToObject(typeof(TEnum), valor);
            }

            return null;
        }

        /// <summary>
        /// Obtém a descrição de um valor de enum (se definida via atributo Description)
        /// </summary>
        /// <typeparam name="TEnum">Tipo do enum</typeparam>
        /// <param name="valor">Valor do enum</param>
        /// <returns>Descrição do valor do enum ou o nome do valor se não houver descrição</returns>
        public static string ObterDescricao<TEnum>(TEnum valor) where TEnum : struct
        {
            if (!typeof(TEnum).IsEnum)
            {
                throw new ArgumentException($"O tipo {typeof(TEnum).Name} não é um enum");
            }

            var campo = typeof(TEnum).GetField(valor.ToString());
            if (campo != null)
            {
                var atributo = campo.GetCustomAttribute<DescriptionAttribute>();
                if (atributo != null)
                {
                    return atributo.Description;
                }
            }

            return valor.ToString();
        }

        /// <summary>
        /// Verifica se um valor inteiro corresponde a um valor válido do enum
        /// </summary>
        /// <typeparam name="TEnum">Tipo do enum</typeparam>
        /// <param name="valor">Valor inteiro a ser verificado</param>
        /// <returns>True se o valor existe no enum, false caso contrário</returns>
        public static bool EhValorValido<TEnum>(int valor) where TEnum : struct
        {
            if (!typeof(TEnum).IsEnum)
            {
                throw new ArgumentException($"O tipo {typeof(TEnum).Name} não é um enum");
            }

            return Enum.IsDefined(typeof(TEnum), valor);
        }
    }
} 