using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public class GuidExampleSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        // Aplica apenas para Guid
        if (context.Type == typeof(Guid) || context.Type == typeof(Guid?))
        {
            schema.Example = new OpenApiString(Guid.NewGuid().ToString());
        }
    }
}
