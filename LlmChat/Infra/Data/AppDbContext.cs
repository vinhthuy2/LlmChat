using LlmChat.Chat;
using Microsoft.EntityFrameworkCore;

namespace LlmChat.Infra.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<ChatSession> ChatSessions => Set<ChatSession>();
    public DbSet<Message> Messages => Set<Message>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<ChatSession>()
            .HasKey(e => e.Id);

        modelBuilder
            .Entity<Message>()
            .HasOne<ChatSession>()
            .WithMany(s => s.Messages)
            .HasForeignKey(m => m.SessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
};