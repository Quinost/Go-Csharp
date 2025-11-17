using API.Infrastructure;
using API.Infrastructure.Commands;
using API.Mediator;
using API.Shared.Requests;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;
[Route("api/job")]
[ApiController]
public class JobController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> AddJob()
    {
        var guid = await mediator.Send(new AddJobRequest("DefaultJob"));
        return Ok(guid);
    }
}
