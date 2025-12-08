using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using NSwag;
using NSwag.Generation.AspNetCore;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using System.Reflection;

namespace Infrastructure.OpenApi
{
    /// <summary>
    /// Processes Swagger operations to conditionally add a security requirement based on the presence of authorization
    /// metadata.
    /// </summary>
    /// <remarks>Use this processor to ensure that Swagger UI reflects the authentication requirements of your
    /// API endpoints. If an endpoint allows anonymous access, no security requirement is added. Otherwise, the
    /// specified authentication scheme is applied to the operation's security requirements.</remarks>
    /// <param name="scheme">The authentication scheme to use when adding a security requirement to the Swagger operation. If not specified,
    /// defaults to the JWT Bearer authentication scheme.</param>
    public class SwaggerGlobalAuthProcessor(string scheme) : IOperationProcessor
    {
        private readonly string _scheme = scheme;

        /// <summary>
        /// Initializes a new instance of the SwaggerGlobalAuthProcessor class using the default JWT bearer
        /// authentication scheme.
        /// </summary>
        /// <remarks>This constructor configures the processor to use the standard JWT bearer
        /// authentication scheme as defined by JwtBearerDefaults.AuthenticationScheme. Use this overload when you want
        /// to apply global authentication to Swagger endpoints with the default authentication settings.</remarks>
        public SwaggerGlobalAuthProcessor()
            : this(JwtBearerDefaults.AuthenticationScheme)
        {
        }

        /// <summary>
        /// process the given operation to conditionally add the security requirement for swagger
        /// </summary>
        /// <param name="context">The context containing operation and API meta data</param>
        /// <returns>
        /// <c>true</c> if the processing was successful and should continue, <c>false</c> to stop processing.
        /// This method always returns <c>true</c>.
        /// </returns>
        /// <remarks>
        /// The processor checks if the endpoint metadata has an <see cref="AllowAnonymousAttribute"/>.
        /// if present, no security requirement is added. Otherwise, a security requirement is added
        /// to the operation using the specified authentication scheme.
        /// </remarks>  

        public bool Process(OperationProcessorContext context)
        {
            IList<object> list = ((AspNetCoreOperationProcessorContext)context)
                .ApiDescription.ActionDescriptor.TryGetPropertyValue<IList<object>>("EndpointMetadata");

            if (list is not null)
            {
                if (list.OfType<AllowAnonymousAttribute>().Any())
                {
                    return true;
                }

                if (context.OperationDescription.Operation.Security.Count == 0)

                {
                    (context.OperationDescription.Operation.Security ??= new List<OpenApiSecurityRequirement>())
                        .Add(new OpenApiSecurityRequirement
                        {
                            {
                                _scheme,
                                Array.Empty<string>()
                            }
                        });
                }

            }
            return true;
        }
    }

    public static class ObjectExtenstions
    {
        public static T TryGetPropertyValue<T>(this object obj, string propertyName, T defaultValue = default) =>
            obj.GetType().GetRuntimeProperty(propertyName) is PropertyInfo propertyInfo
                ? (T)propertyInfo.GetValue(obj)
                : defaultValue;
    }
}
