using System.Net;

namespace Application.Exceptions
{
    public class IdentityException : Exception
    {
        public List<string> ErrorMessages { get; set; }
        public HttpStatusCode StatusCode { get; set; }

        public IdentityException(List<string> errorMessages)
        {
            ErrorMessages = errorMessages;
            StatusCode = HttpStatusCode.InternalServerError;
        }
    }
}
