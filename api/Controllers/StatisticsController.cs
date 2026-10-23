using Microsoft.AspNetCore.Mvc;
using Raspisanie.Api.Application;

namespace Raspisanie.Api.Controllers;

[ApiController]
[Route("api/statistics")]
public sealed class StatisticsController(LessonService service) : ControllerBase
{
    [HttpGet]
    public ActionResult<Statistics> Get() => Ok(service.Statistics());
}
