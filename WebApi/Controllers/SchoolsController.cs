using Application.Features.Schools;
using Application.Features.Schools.Commands;
using Application.Features.Schools.Queries;
using Infrastructure.Constants;
using Infrastructure.Identity.Auth;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/schools")]
    public class SchoolsController : BaseApiController
    {

        [HttpGet("by-id/{schoolId}")]
        [ShouldHavePermission(action: SchoolAction.Read, feature: SchoolFeature.Schools)]
        public async Task<IActionResult>GetSchoolByIdAsync(int schoolId)
        {
            var response = await Sender.Send(new GetSchoolByIdQuery { SchoolId = schoolId });
            if(response.IsSuccessful)
            {
                return Ok(response);
            }
            return NotFound(response);
        }

        [HttpGet("by-name/{schoolName}")]
        [ShouldHavePermission(action: SchoolAction.Read, feature: SchoolFeature.Schools)]
        public async Task<IActionResult>GetSchoolByIdAsync(string schoolName)
        {
            var response = await Sender.Send(new GetSchoolByNameQuery { Name = schoolName });
            if(response.IsSuccessful)
            {
                return Ok(response);
            }
            return NotFound(response);
        }

        [HttpGet("all")]
        [ShouldHavePermission(action: SchoolAction.Read, feature: SchoolFeature.Schools)]
        public async Task<IActionResult> GetAllSchoolsAsync()
        {
            var response = await Sender.Send(new GetAllSchoolsQuery { });
            if (response.IsSuccessful)
            {
                return Ok(response);
            }
            return NotFound(response);
        }

        [HttpPost("add")]
        [ShouldHavePermission(action: SchoolAction.Create, feature: SchoolFeature.Schools)]
        public async Task<IActionResult> CreateSchoolAsync([FromBody] CreateSchoolRequest request)
        {
            var response = await Sender.Send(new CreateSchoolCommand { CreateSchool = request });
            if(response.IsSuccessful)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

        [HttpPut("update")]
        [ShouldHavePermission(action: SchoolAction.Update, feature: SchoolFeature.Schools)]
        public async Task<IActionResult> UpdateSchoolAsync([FromBody] UpdateSchoolRequest request)
        {
            var response = await Sender.Send(new UpdateSchoolCommand { UpdateSchool = request });
            if (response.IsSuccessful)
            {
                return Ok(response);
            }
            return NotFound(response);
        }

        [HttpDelete("{schoolId}")]
        [ShouldHavePermission(action: SchoolAction.Delete, feature: SchoolFeature.Schools)]
        public async Task<IActionResult> DeleteSchoolAsync(int schoolId)
        {
            var response = await Sender.Send(new DeleteSchoolCommand { SchoolId = schoolId });
            if (response.IsSuccessful)
            {
                return Ok(response);
            }
            return NotFound(response);
        }
    }
}
