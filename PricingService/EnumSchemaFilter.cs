using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;


public class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type.IsEnum)
        {
            string[] enumValues = context.Type
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Select(f => $"{f.Name} = {(int)f.GetValue(null)}")
                .ToArray();

            schema.Description += string.Join("<br>", enumValues);
        }
    }
}
