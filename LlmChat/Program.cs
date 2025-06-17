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
builder.Services.AddSingleton(OllamaClientFactory.Phi3Client());
builder.Services.AddScoped<IChatSessionService, ChatSessionService>();
builder.Services.AddSingleton<ILlmAgent, OllamaAgent>();
builder.Services.AddSingleton<ILlmSupervisory, OllamaSupervisory>();
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

app.MapPost("/api/chat",
        async (OllamaAgent agent, ChatRequestDto chatRequest) =>
        {
            var sessionId = chatRequest.SessionId ?? Guid.NewGuid();
            var answer = await agent.AnswerAsync(chatRequest.Content, sessionId);

            return Results.Json(new ChatResponseDto(
                answer,
                "assistant",
                sessionId
            ));
        })
    .WithName("chat")
    .WithOpenApi();

app.MapPost("/api/chatDefer",
        (OllamaAgent llmAgent, ChatRequestDto chatRequest) =>
        {
            var sessionId = chatRequest.SessionId ?? Guid.NewGuid();
            llmAgent.DeferAMessageAsync(chatRequest.Content, sessionId);
            return Results.Json(sessionId);
        })
    .WithOpenApi();

app.MapGet("/api/chatStream",
        async ([FromQuery] Guid sessionId, OllamaAgent agent, HttpContext httpContext) =>
        {
            httpContext.Response.ContentType = "text/event-stream";
            httpContext.Response.Headers.Append("Cache-Control", "no-cache");

            try
            {
                var stream = await agent.StreamedAnswerAsync(sessionId);
                await foreach (var chunk in stream)
                {
                    await httpContext.Response.WriteAsync($"data:{chunk}\n\n");
                    await httpContext.Response.Body.FlushAsync();
                }

                await httpContext.Response.WriteAsync("data: [done]");
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

app.MapPost("/api/supervisory",
    async (OllamaSupervisory supervisory, OriginalMessageDto originalMessage) =>
    {
        var answer = await supervisory.ReviseAsync(originalMessage.Content, Guid.NewGuid(), originalMessage.ExtraSystemPrompt);
        return Results.Json(new SupervisedMessageDto(answer));
    })
    .WithOpenApi();

app.Run();