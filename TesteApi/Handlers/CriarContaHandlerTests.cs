using Xunit;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;
using Infrastructure.Database.CommandStore;
using Infrastructure.Database.QueryStore;
using Application.Commands.Requests;
using Application.Handlers;
using FluentAssertions;

public class CriarContaHandlerTests
{
    private readonly Mock<IContaCorrenteQueryStore> _queryStoreMock;
    private readonly Mock<IContaCorrenteCommandStore> _commandStoreMock;
    private readonly CriarContaHandler _handler;

    public CriarContaHandlerTests()
    {
        _queryStoreMock = new Mock<IContaCorrenteQueryStore>();
        _commandStoreMock = new Mock<IContaCorrenteCommandStore>();
        _handler = new CriarContaHandler(_commandStoreMock.Object, _queryStoreMock.Object);
    }

    [Fact]
    public async Task Deve_Criar_Conta_Com_Sucesso()
    {
        // Arrange
        var command = new CriarContaRequest
        {
            Nome = "João da Silva",
            Cpf = "12345678900",
            Senha = "senha123"
        };

        _queryStoreMock.Setup(x => x.ObterPorCpfAsync(command.Cpf))
            .ReturnsAsync((ContaCorrente)null!);

        _commandStoreMock.Setup(x => x.CriarAsync(It.IsAny<ContaCorrente>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.NumeroConta.Should().NotBeNullOrEmpty();
        result.Nome.Should().Be("João da Silva");

        _commandStoreMock.Verify(x => x.CriarAsync(It.Is<ContaCorrente>(c =>
            c.Nome == command.Nome &&
            c.Cpf == command.Cpf &&
            !string.IsNullOrEmpty(c.Senha) &&
            !string.IsNullOrEmpty(c.Salt)
        )), Times.Once);
    }

    [Fact]
    public async Task Nao_Deve_Permitir_Cadastro_Com_Cpf_Duplicado()
    {
        // Arrange
        var command = new CriarContaRequest
        {
            Nome = "Maria Souza",
            Cpf = "98765432100",
            Senha = "senhaSegura"
        };

        _queryStoreMock.Setup(x => x.ObterPorCpfAsync(command.Cpf))
            .ReturnsAsync(new ContaCorrente { IdContaCorrente = Guid.NewGuid().ToString() });

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Já existe uma conta cadastrada com este CPF.");

        _commandStoreMock.Verify(x => x.CriarAsync(It.IsAny<ContaCorrente>()), Times.Never);
    }
}
