using Infrastructure.Constants;
using Infrastructure.Identity.Auth;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        [HttpGet]
        [ShouldHavePermission(action:SchoolAction.Read, feature: SchoolFeature.Schools)]
        public IEnumerable<int> Get()
        {
            return [1, 2, 4, 5, 6];
        }
    }
}
