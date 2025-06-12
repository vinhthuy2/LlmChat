using LlmChat.Agents;
using LlmChat.Chat;
using LlmChat.Infra.Data;
using LlmChat.Infra.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day));

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite("Data Source=ChatLlm.db"));
builder.Services.AddSingleton<IChatSessionStore, ChatSessionStore>();
builder.Services.AddScoped<IChatSessionService, ChatSessionService>();
builder.Services.AddSingleton<ILlmAgent, OllamaAgent>();
builder.Services.AddSingleton<ILoggingService, LoggingService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder =>
        {
            builder.AllowAnyOrigin() // Allow all origins
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

var app = builder.Build();
app.UseCors("AllowAllOrigins");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/chat",
        async (ILlmAgent agent, ChatRequestDto chatRequest) =>
        {
            var sessionId = chatRequest.SessionId ?? Guid.NewGuid();
            var answer = await agent.Answer(chatRequest.Content, sessionId);

            return Results.Json(new ChatResponseDto(
                answer,
                "assistant",
                sessionId
            ));
        })
    .WithName("chat")
    .WithOpenApi();

app.MapPost("/chatDefer",
        (ILlmAgent llmAgent, ChatRequestDto chatRequest) =>
        {
            var sessionId = chatRequest.SessionId ?? Guid.NewGuid();
            llmAgent.DeferAMessage(chatRequest.Content, sessionId);
            return chatRequest.SessionId ?? Guid.NewGuid();
        })
    .WithOpenApi();

app.MapGet("/chatStream",
        async ([FromQuery] Guid sessionId, ILlmAgent agent, HttpContext httpContext) =>
        {
            httpContext.Response.ContentType = "text/event-stream";
            httpContext.Response.Headers.Append("Cache-Control", "no-cache");

            try
            {
                var stream = await agent.StreamedAnswer(sessionId);
                await foreach (var chunk in stream)
                {
                    await httpContext.Response.WriteAsync($"data: {chunk}\n\n");
                    await httpContext.Response.Body.FlushAsync();
                }

                await httpContext.Response.WriteAsync("data: [done]\n\n");
                await httpContext.Response.Body.FlushAsync();
            }
            catch (Exception ex)
            {
                await httpContext.Response.WriteAsync($"data: [error] {ex.Message}\n\n");
                await httpContext.Response.Body.FlushAsync();
                httpContext.Abort();
            }
        })
    .WithOpenApi();

app.Run();