using NJsonSchema;
using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using System.Reflection;

namespace Infrastructure.OpenApi
{
    public class SwaggerHeaderAttributeProcessor : IOperationProcessor
    {
        public bool Process(OperationProcessorContext context)
        {
            if(context.MethodInfo.GetCustomAttribute(typeof(SwaggerHeaderAttribute)) is SwaggerHeaderAttribute swaggerHeader)
            {
                var parameters = context.OperationDescription.Operation.Parameters;

                var existingParameter = parameters
                    .FirstOrDefault(p => p.Kind == NSwag.OpenApiParameterKind.Header && p.Name == swaggerHeader.HeaderName);

                if (existingParameter is not null)
                {
                    parameters.Remove(existingParameter);
                }

                parameters.Add(new OpenApiParameter
                {
                    Name = swaggerHeader.HeaderName,
                    Kind = OpenApiParameterKind.Header,
                    IsRequired = true,
                    Description = swaggerHeader.Description,
                    Schema = new JsonSchema()
                    {
                        Type = JsonObjectType.String,
                        Description = swaggerHeader.DefaultValue,
                    }
                });
            }
            return true;
        }
    }
}
