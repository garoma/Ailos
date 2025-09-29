namespace Application.Commands.Responses
{
    public class MovimentarContaResponse
    {
        public string MovimentoId { get; set; }
        public MovimentarContaResponse(string movimentoId)
        {
            MovimentoId = movimentoId;
        }
    }
}