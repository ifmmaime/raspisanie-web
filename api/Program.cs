using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Raspisanie.Api.Application;
using Raspisanie.Api.Domain;
using Raspisanie.Api.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers().ConfigureApiBehaviorOptions(options =>
{
    options.InvalidModelStateResponseFactory = context => new BadRequestObjectResult(
        new ValidationProblemDetails(context.ModelState) { Title = "Проверьте заполнение полей." });
});
builder.Services.AddSingleton<ILessonRepository, InMemoryLessonRepository>();
builder.Services.AddScoped<LessonService>();
builder.Services.AddSingleton<ScheduleParser>();
builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = 2_000_000);

var app = builder.Build();
app.UseExceptionHandler(handler => handler.Run(async context =>
{
    var error = context.Features.Get<IExceptionHandlerFeature>()!.Error;
    var (status, message) = error switch
    {
        ArgumentException => (400, error.Message),
        KeyNotFoundException => (404, error.Message),
        _ => (500, "Не удалось обработать запрос.")
    };
    context.Response.StatusCode = status;
    await context.Response.WriteAsJsonAsync(new ProblemDetails { Status = status, Title = message });
}));
app.MapControllers();
app.Run();

// Точка входа для интеграционных тестов с WebApplicationFactory.
public partial class Program;
