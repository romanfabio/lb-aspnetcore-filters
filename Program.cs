using Dapper;
using lb_aspnetcore_filters.Common;
using lb_aspnetcore_filters.Common.Models;
using lb_aspnetcore_filters.Filters;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ConnectionPoolOptions>(
builder.Configuration.GetSection("ConnectionStrings"));

builder.Services.AddSingleton<ConnectionPool>();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/logs", async (ConnectionPool pool) =>
{
    await using var connection = pool.GetConnection();
    await connection.OpenAsync();

    var logs = await connection.QueryAsync<Log>("SELECT * FROM Logs ORDER BY Timestamp DESC");
    return Results.Ok(logs);
});

app.MapGet("/protected", (HttpContext context) =>
    {
        return Results.Text($"I'm authenticated with user id {context.User.Identity.Name}");
    }).AddEndpointFilter<RequestLoggerFilter>()
    .AddEndpointFilter<AuthenticationFilter>()
    .AddEndpointFilter<RateLimiterFilter>();

app.MapPost("/users", async (ConnectionPool pool, [FromBody] User user) =>
{
    await using var connection = pool.GetConnection();
    await connection.OpenAsync();

    var sql = "INSERT INTO Users VALUES (@FirstName, @LastName, @AccessToken, @Role, NULL)";
    await connection.ExecuteAsync(sql, user);
}).AddEndpointFilter<AuthenticationFilter>()
    .AddEndpointFilter<RequestLoggerFilter>()
    .AddEndpointFilter<RateLimiterFilter>()
    .AddEndpointFilter<UserValidatorFilter>();

app.MapGet("/hello", (HttpContext context) =>
{
    var lang = context.Items["lang"] as string;

    switch (lang)
    {
        case "it": return Results.Ok("Ciao");
        case "es": return Results.Ok("Hola");
        case "de": return Results.Ok("Halo");
        default: return Results.Ok("Hello");
    }
}).AddEndpointFilter<LocalizationFilter>();

app.Run();