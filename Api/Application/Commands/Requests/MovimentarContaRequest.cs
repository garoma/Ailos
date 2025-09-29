using MediatR;
using Application.Commands.Responses;
using System.Text.Json.Serialization;

namespace Application.Commands.Requests
{
    public class MovimentarContaRequest : IRequest<MovimentarContaResponse>
    {
        //[JsonIgnore] // não aparece no Swagger/body
        public Guid IdRequisicao { get; set; } = Guid.NewGuid(); // gera GUID automaticamente
        public string ContaCorrenteId { get; set; }
        public string Numero { get; set; }
        public decimal Valor { get; set; }
        public string Tipo { get; set; } // 'C' ou 'D'
    }
}