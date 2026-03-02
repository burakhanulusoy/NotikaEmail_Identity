using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NotikaEmail_Identity.Migrations
{
    /// <inheritdoc />
    public partial class mig_added_message_isDraft : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "Messages",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "Messages");
        }
    }
}
