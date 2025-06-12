using LlmChat.Chat;
using Microsoft.EntityFrameworkCore;

namespace LlmChat.Infra.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<ChatSession> ChatSessions => Set<ChatSession>();
};