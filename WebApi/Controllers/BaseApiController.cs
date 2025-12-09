using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    public class BaseApiController() : ControllerBase
    {
        private readonly ISender _sender = null;
        public ISender Sender => _sender ?? HttpContext.RequestServices.GetRequiredService<ISender>();

    }
}
