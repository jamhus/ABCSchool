using Application.Exceptions;
using Application.Wrappers;
using System;
using System.Net;
using System.Text.Json;

namespace WebApi
{
    public class ErrorHandlingMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next;
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {

                var response = context.Response;
                response.ContentType = "application/json";

                var responseWrapper = ResponseWrapper.Fail();

                switch (ex)
                {
                    case ConflictException exception:
                        response.StatusCode = (int)exception.StatusCode;
                        responseWrapper.Messages = exception.ErrorMessages;
                        break;

                    case NotFoundException exception:
                        response.StatusCode = (int)exception.StatusCode;
                        responseWrapper.Messages = exception.ErrorMessages;
                        break;

                    case ForbiddenException exception:
                        response.StatusCode = (int)exception.StatusCode;
                        responseWrapper.Messages = exception.ErrorMessages;
                        break;

                    case IdentityException exception:
                        response.StatusCode = (int)exception.StatusCode;
                        responseWrapper.Messages = exception.ErrorMessages;
                        break;

                    case UnauthorizedException exception:
                        response.StatusCode = (int)exception.StatusCode;
                        responseWrapper.Messages = exception.ErrorMessages;
                        break;


                    default:
                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        responseWrapper.Messages = ["Something went south. Contact Administrator"];
                        break;
                }

                var result = JsonSerializer.Serialize(responseWrapper);

                await response.WriteAsync(result);
            }
        }
    }
}
