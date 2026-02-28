using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NotikaEmail_Identity.Migrations
{
    /// <inheritdoc />
    public partial class mig_added_message_IfromFile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AttachedFilePath",
                table: "Messages",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttachedFilePath",
                table: "Messages");
        }
    }
}
