using LlmChat.Agents;
using LlmChat.Chat;
using LlmChat.Infra.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite("Data Source=ChatLlm.db"));
builder.Services.AddScoped<ILlmAgent, OllamaAgent>();
builder.Services.AddScoped<IChatSessionService, ChatSessionService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/chat",
        async (AppDbContext db, ILlmAgent agent, ChatRequestDto chatRequest) =>
        {
            var sessionId = chatRequest.SessionId;
            var session = await db.ChatSessions
                    .AsNoTracking()
                    .Include(c => c.Messages)
                    .SingleOrDefaultAsync(c => c.Id == sessionId)
                ?? db.ChatSessions.Add(new ChatSession()).Entity;

            var message = new Message(
                Guid.NewGuid(),
                chatRequest.Content,
                chatRequest.Sender,
                session.Id,
                chatRequest.Timestamp
            );

            var answer = await agent.Answer(session.Id, message.Content);
            var answerMessage = new Message(
                Guid.NewGuid(),
                answer,
                "assistant",
                session.Id,
                DateTimeOffset.UtcNow
            );

            session.AddMessage(message);
            session.AddMessage(answerMessage);

            await db.SaveChangesAsync();

            return Results.Json(new ChatResponseDto(
                answerMessage.Id,
                answerMessage.Content,
                answerMessage.Sender,
                answerMessage.SessionId,
                answerMessage.Timestamp
            ));
        })
    .WithName("chat")
    .WithOpenApi();

app.Run();