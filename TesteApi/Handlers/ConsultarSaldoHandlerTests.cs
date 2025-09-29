using Domain.Entities;
using FluentAssertions;
using Infrastructure.Database.QueryStore;
using Moq;

namespace Tests.Application.Handlers
{
    public class ConsultarSaldoHandlerTests
    {
        private readonly Mock<IContaCorrenteQueryStore> _mockQueryStore;
        private readonly ConsultarSaldoHandler _handler;

        public ConsultarSaldoHandlerTests()
        {
            _mockQueryStore = new Mock<IContaCorrenteQueryStore>();
            _handler = new ConsultarSaldoHandler(_mockQueryStore.Object);
        }

        [Fact]
        public async Task DeveRetornarErro_QuandoContaNaoExiste()
        {
            // Arrange
            var request = new ConsultarSaldoRequest("B6BAFC09-6967-ED11-A567-055DFA4A16C9");

            _mockQueryStore.Setup(q => q.ObterPorNumeroAsync("0000012"))
                           .ReturnsAsync((ContaCorrente)null);

            // Act
            Func<Task> act = async () => await _handler.Handle(request, CancellationToken.None);

            // Assert
            await act.Should()
                .ThrowAsync<ApplicationException>()
                .WithMessage("Conta corrente não encontrada. | Tipo: INVALID_ACCOUNT");
        }
    }
}
