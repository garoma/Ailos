using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public class IdempotencyHeaderFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Parameters == null)
            operation.Parameters = new List<OpenApiParameter>();

        // Adiciona o header Idempotency-Key com um GUID dinâmico
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "Idempotency-Key",
            In = ParameterLocation.Header,
            Required = true,
            Description = "Chave única para garantir idempotência da operação",
            Schema = new OpenApiSchema
            {
                Type = "string",
                Format = "uuid",
                Example = new OpenApiString(Guid.NewGuid().ToString()) // GUID aleatório
            }
        });
    }
}
