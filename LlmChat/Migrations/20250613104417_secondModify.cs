using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LlmChat.Migrations
{
    /// <inheritdoc />
    public partial class secondModify : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastUpdated",
                table: "ChatSessions",
                newName: "UpdatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "ChatSessions",
                newName: "LastUpdated");
        }
    }
}
