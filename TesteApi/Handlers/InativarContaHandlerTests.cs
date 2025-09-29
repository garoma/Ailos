using Application.Handlers;
using Application.Requests;
using Application.Validators;
using FluentAssertions;
using FluentValidation;
using Infrastructure.Database.QueryStore;
using MediatR;
using Moq;

public class InativarContaHandlerTests
{
    private readonly Mock<IContaCorrenteQueryStore> _mockQueryStore;
    private readonly Mock<IContaCorrenteCommandStore> _mockCommandStore;
    private readonly IValidator<InativarContaRequest> _validator;
    private readonly InativarContaHandler _handler;

    public InativarContaHandlerTests()
    {
        _mockQueryStore = new Mock<IContaCorrenteQueryStore>();
        _mockCommandStore = new Mock<IContaCorrenteCommandStore>();
        _validator = new InativarContaValidator();
        _handler = new InativarContaHandler(_mockQueryStore.Object, _mockCommandStore.Object, _validator);
    }

    [Fact]
    public async Task Handle_ContaNaoEncontrada_DeveLancarInvalidAccountException()
    {
        // Arrange
        _mockQueryStore.Setup(q => q.ObterPorNumeroAsync(It.IsAny<string>()))
            .ReturnsAsync((Domain.Entities.ContaCorrente?)null);

        var request = new InativarContaRequest { Senha = "123", NumeroConta = "0001" };

        // Act
        var act = async () => await _handler.Handle(request, CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<Exception>(act);
        ex.Message.Should().StartWith("INVALID_ACCOUNT");
    }

    [Fact]
    public async Task Handle_SenhaIncorreta_DeveLancarInvalidPasswordException()
    {
        // Arrange
        var conta = new Domain.Entities.ContaCorrente
        {
            Senha = "senhaCorreta",
            Ativo = true,
            Numero = "0001"
        };
        _mockQueryStore.Setup(q => q.ObterPorNumeroAsync(It.IsAny<string>())).ReturnsAsync(conta);

        var request = new InativarContaRequest { Senha = "senhaErrada", NumeroConta = "0001" };

        // Act
        var act = async () => await _handler.Handle(request, CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<Exception>(act);
        ex.Message.Should().StartWith("INVALID_PASSWORD");
    }

    [Fact]
    public async Task Handle_SenhaVazia_DeveFalharValidacao()
    {
        // Arrange
        var request = new InativarContaRequest { Senha = "", NumeroConta = "0001" };

        // Act
        var act = async () => await _handler.Handle(request, CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<Exception>(act);
        ex.Message.Should().Contain("Senha é obrigatória");
    }
}
