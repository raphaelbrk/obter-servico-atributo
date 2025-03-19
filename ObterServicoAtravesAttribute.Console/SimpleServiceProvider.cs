using System;
using System.Collections.Generic;

namespace ObterServicoAtravesAttribute.Console
{
    /// <summary>
    /// Implementação simples de provedor de serviços (container de injeção de dependência)
    /// </summary>
    public class SimpleServiceProvider : IServiceProvider
    {
        private readonly Dictionary<Type, object> _services = new();
        private readonly Dictionary<Type, Func<object>> _serviceFactories = new();
        
        /// <summary>
        /// Registra um serviço com uma instância específica
        /// </summary>
        /// <typeparam name="TService">Tipo do serviço</typeparam>
        /// <param name="implementacao">Instância da implementação</param>
        public void RegisterSingleton<TService>(TService implementacao) where TService : class
        {
            _services[typeof(TService)] = implementacao ?? throw new ArgumentNullException(nameof(implementacao));
        }
        
        /// <summary>
        /// Registra um serviço com um tipo de implementação
        /// </summary>
        /// <typeparam name="TService">Tipo do serviço</typeparam>
        /// <typeparam name="TImplementacao">Tipo da implementação</typeparam>
        public void Register<TService, TImplementacao>() 
            where TService : class 
            where TImplementacao : class, TService, new()
        {
            _serviceFactories[typeof(TService)] = () => new TImplementacao();
        }
        
        /// <summary>
        /// Registra um serviço com uma factory
        /// </summary>
        /// <typeparam name="TService">Tipo do serviço</typeparam>
        /// <param name="factory">Função para criar a instância</param>
        public void Register<TService>(Func<TService> factory) where TService : class
        {
            _serviceFactories[typeof(TService)] = () => factory() ?? throw new InvalidOperationException($"A factory para {typeof(TService).Name} retornou null");
        }
        
        /// <summary>
        /// Obtém um serviço do tipo especificado
        /// </summary>
        /// <param name="serviceType">Tipo do serviço</param>
        /// <returns>Instância do serviço ou null se não encontrado</returns>
        public object GetService(Type serviceType)
        {
            // Verificar se existe uma instância registrada
            if (_services.TryGetValue(serviceType, out var service))
            {
                return service;
            }
            
            // Verificar se existe uma factory registrada
            if (_serviceFactories.TryGetValue(serviceType, out var factory))
            {
                var instance = factory();
                
                // Registrar a instância para futuras solicitações (singleton)
                _services[serviceType] = instance;
                
                return instance;
            }
            
            // Se chegou aqui, não encontrou o serviço
            return null;
        }
        
        /// <summary>
        /// Obtém um serviço do tipo especificado
        /// </summary>
        /// <typeparam name="T">Tipo do serviço</typeparam>
        /// <returns>Instância do serviço</returns>
        public T GetService<T>() where T : class
        {
            return GetService(typeof(T)) as T;
        }
    }
} 