# Gerenciador de Serviços por Atributos

Este projeto demonstra como implementar um sistema para obtenção de serviços baseados em atributos por meio de injeção de dependência, sem utilizar cache distribuído.

## Conceito

O conceito principal é permitir que serviços sejam identificados e obtidos com base em atributos decorados em suas classes. Isso permite um desacoplamento entre o código cliente e a implementação concreta dos serviços.

## Componentes Principais

### ServicoAttribute

Um atributo customizado que identifica uma classe como um serviço disponível para injeção, com um identificador único:

```csharp
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ServicoAttribute : Attribute
{
    public string Identificador { get; }

    public ServicoAttribute(string identificador)
    {
        Identificador = identificador;
    }
}
```

### IServico

Interface base para todos os serviços que podem ser obtidos pelo gerenciador:

```csharp
public interface IServico
{
    string Executar();
}
```

### GerenciadorServicos

Responsável por gerenciar os serviços e suas instâncias. O gerenciador carrega todos os tipos que implementam `IServico` e possuem o atributo `ServicoAttribute`, e permite obter instâncias desses serviços pelo identificador.

Características principais:
- Usa injeção de dependência através do `IServiceProvider`
- Mantém um cache local de instâncias por máquina
- Permite obter novos serviços ou reutilizar instâncias existentes

### SimpleServiceProvider

Uma implementação simples de `IServiceProvider` que permite:
- Registrar serviços como singletons
- Registrar tipos para injeção
- Registrar factories para criação de instâncias

## Como Usar

### 1. Crie um serviço implementando a interface IServico:

```csharp
[Servico("MeuServico")]
public class MeuServico : IServico
{
    public string Executar()
    {
        return "Executando Meu Serviço";
    }
}
```

### 2. Configure o provedor de serviços:

```csharp
var serviceProvider = new SimpleServiceProvider();
serviceProvider.Register<MeuServico, MeuServico>();
```

### 3. Crie uma instância do GerenciadorServicos:

```csharp
var gerenciador = new GerenciadorServicos(serviceProvider);
```

### 4. Obtenha o serviço pelo identificador:

```csharp
var meuServico = gerenciador.ObterServico("MeuServico");
var resultado = meuServico.Executar();
```

## Vantagens

- **Desacoplamento**: O código cliente não precisa conhecer a implementação concreta dos serviços
- **Flexibilidade**: Serviços podem ser substituídos facilmente sem alterar o código cliente
- **Testabilidade**: Facilita a criação de mocks para testes unitários
- **Performance**: Reutilização de instâncias para melhor performance
- **Controle de ciclo de vida**: Gerenciamento do ciclo de vida dos serviços totalmente sob controle

## Diferenças em relação ao cache distribuído

Esta implementação gerencia os serviços localmente por máquina, o que é mais eficiente para:

- Aplicações que rodam em um único servidor
- Cenários onde os serviços não precisam ser compartilhados entre diferentes instâncias
- Ambientes onde a sincronização de estado entre máquinas não é necessária

Caso seja necessário compartilhar serviços entre diferentes instâncias da aplicação, uma implementação com cache distribuído (como Redis) seria mais adequada. 