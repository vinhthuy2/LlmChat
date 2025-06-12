using LlmChat.Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace LlmChat.Chat;

public class ChatSessionService(AppDbContext dbContext) : IChatSessionService
{
    public async Task<ChatSession?> GetSession(Guid sessionId)
    {
        return await dbContext.Set<ChatSession>()
            .Include(s => s.Messages)
            .FirstOrDefaultAsync(s => s.Id == sessionId);
    }
    public async Task<IReadOnlyList<ChatSession>> GetSessions()
    {
        return await dbContext.Set<ChatSession>()
            .Include(s => s.Messages)
            .ToListAsync();
    }
}