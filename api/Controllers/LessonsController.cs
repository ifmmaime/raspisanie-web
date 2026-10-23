using Microsoft.AspNetCore.Mvc;
using Raspisanie.Api.Application;

namespace Raspisanie.Api.Controllers;

[ApiController]
[Route("api/lessons")]
public sealed class LessonsController(LessonService service) : ControllerBase
{
    [HttpGet]
    public ActionResult<object> List(DateOnly? date, string? search, string? attendance, int page = 1, int pageSize = 10)
    {
        var result = service.List(date, search, attendance, page, pageSize);
        return Ok(new { result.Items, result.Total, Page = result.PageNumber, result.PageSize });
    }

    [HttpGet("{id:int}")]
    public ActionResult<LessonDto> Get(int id) => Ok(service.Get(id));

    [HttpPost]
    public ActionResult<LessonDto> Create(LessonRequest request)
    {
        var lesson = service.Create(request.ToDetails());
        return CreatedAtAction(nameof(Get), new { id = lesson.Id }, lesson);
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, LessonRequest request)
    {
        service.Update(id, request.ToDetails());
        return NoContent();
    }

    [HttpPatch("{id:int}/attendance")]
    public IActionResult Mark(int id, AttendanceRequest request)
    {
        service.Mark(id, request.IsAttended);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        service.Delete(id);
        return NoContent();
    }

    [HttpPost("import")]
    public ActionResult<object> Import(ImportRequest request) => Ok(new { Count = service.Import(request.Html) });
}
