using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Raspisanie.Api.Application;

namespace Raspisanie.Tests;

public sealed class ApiTests : IDisposable
{
    private readonly WebApplicationFactory<Program> factory = new();
    private readonly HttpClient client;

    public ApiTests() => client = factory.CreateClient();

    private static object Details(string subject = "Физика") => new
    {
        date = "2026-10-25", startTime = "09:00:00", endTime = "10:30:00",
        subject, teacher = "Преподаватель", room = "101"
    };

    [Fact]
    public async Task CrudUsesExpectedStatusCodesAndPersistsChanges()
    {
        var created = await client.PostAsJsonAsync("/api/lessons", Details());
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.NotNull(created.Headers.Location);
        var lesson = (await created.Content.ReadFromJsonAsync<LessonDto>())!;
        Assert.Null(lesson.IsAttended);

        Assert.Equal(HttpStatusCode.NoContent, (await client.PatchAsJsonAsync($"/api/lessons/{lesson.Id}/attendance", new { isAttended = true })).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await client.PutAsJsonAsync($"/api/lessons/{lesson.Id}", Details("Химия"))).StatusCode);
        var updated = (await client.GetFromJsonAsync<LessonDto>(created.Headers.Location))!;
        Assert.Equal("Химия", updated.Subject);
        Assert.True(updated.IsAttended);

        Assert.Equal(HttpStatusCode.NoContent, (await client.PatchAsJsonAsync($"/api/lessons/{lesson.Id}/attendance", new { isAttended = (bool?)null })).StatusCode);
        Assert.Null((await client.GetFromJsonAsync<LessonDto>(created.Headers.Location))!.IsAttended);
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync(created.Headers.Location)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync(created.Headers.Location)).StatusCode);
    }

    [Theory]
    [InlineData("page=0")]
    [InlineData("pageSize=101")]
    [InlineData("pageSize=-1")]
    [InlineData("date=2026-02-30")]
    [InlineData("attendance=unknown")]
    [InlineData("page=abc")]
    public async Task InvalidQueryReturnsBadRequest(string query)
    {
        Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync($"/api/lessons?{query}")).StatusCode);
    }

    [Fact]
    public async Task ListAppliesFiltersAndPagination()
    {
        using var first = JsonDocument.Parse(await client.GetStringAsync("/api/lessons?page=1&pageSize=2"));
        using var second = JsonDocument.Parse(await client.GetStringAsync("/api/lessons?page=2&pageSize=2"));
        Assert.Equal(4, first.RootElement.GetProperty("total").GetInt32());
        Assert.Equal(2, first.RootElement.GetProperty("items").GetArrayLength());
        Assert.NotEqual(first.RootElement.GetProperty("items")[0].GetProperty("id").GetInt32(), second.RootElement.GetProperty("items")[0].GetProperty("id").GetInt32());
        var search = Uri.EscapeDataString("базы");
        using var filtered = JsonDocument.Parse(await client.GetStringAsync($"/api/lessons?date=2026-10-23&search={search}&attendance=absent"));
        Assert.Equal(1, filtered.RootElement.GetProperty("total").GetInt32());
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"subject\":\"Предмет\"}")]
    [InlineData("{\"date\":\"2026-10-25\",\"startTime\":\"10:00:00\",\"endTime\":\"09:00:00\",\"subject\":\"Предмет\",\"teacher\":\"Т\",\"room\":\"1\"}")]
    public async Task InvalidLessonReturnsBadRequest(string json)
    {
        var response = await client.PostAsync("/api/lessons", new StringContent(json, Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task EmptyAttendanceBodyDoesNotEraseExistingMark()
    {
        var response = await client.PatchAsJsonAsync("/api/lessons/1/attendance", new { });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.True((await client.GetFromJsonAsync<LessonDto>("/api/lessons/1"))!.IsAttended);
    }

    [Fact]
    public async Task ImportAndStatisticsAreRealEndpoints()
    {
        var response = await client.PostAsJsonAsync("/api/lessons/import", new { html = LessonTests.Html("Физика") });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var imported = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(1, imported.RootElement.GetProperty("count").GetInt32());
        var stats = (await client.GetFromJsonAsync<Statistics>("/api/statistics"))!;
        Assert.Equal(5, stats.Total);
        Assert.Equal(50, stats.Percentage);
        Assert.Equal(3, stats.Unmarked);
    }

    public void Dispose()
    {
        client.Dispose();
        factory.Dispose();
    }
}
