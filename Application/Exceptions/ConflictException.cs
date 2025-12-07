using System.Net;

namespace Application.Exceptions
{
    public class ConflictException : Exception
    {
        public List<string> ErrorMessages { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public ConflictException(List<string> errorMessages)
        {
            ErrorMessages = errorMessages;
            StatusCode = HttpStatusCode.Conflict;
        }
    }
}
