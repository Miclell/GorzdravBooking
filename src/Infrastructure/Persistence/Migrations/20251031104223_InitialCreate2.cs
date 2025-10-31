using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TimePreferences_PatientProfiles_PatientProfileId",
                table: "TimePreferences");

            migrationBuilder.RenameColumn(
                name: "PatientProfileId",
                table: "TimePreferences",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_TimePreferences_PatientProfileId",
                table: "TimePreferences",
                newName: "IX_TimePreferences_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_TimePreferences_PatientId_Name",
                table: "TimePreferences",
                newName: "IX_TimePreferences_UserId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_TimePreferences_PatientId_Day",
                table: "TimePreferences",
                newName: "IX_TimePreferences_UserId_Day");

            migrationBuilder.AddForeignKey(
                name: "FK_TimePreferences_Users_UserId",
                table: "TimePreferences",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TimePreferences_Users_UserId",
                table: "TimePreferences");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "TimePreferences",
                newName: "PatientProfileId");

            migrationBuilder.RenameIndex(
                name: "IX_TimePreferences_UserId_Name",
                table: "TimePreferences",
                newName: "IX_TimePreferences_PatientId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_TimePreferences_UserId_Day",
                table: "TimePreferences",
                newName: "IX_TimePreferences_PatientId_Day");

            migrationBuilder.RenameIndex(
                name: "IX_TimePreferences_UserId",
                table: "TimePreferences",
                newName: "IX_TimePreferences_PatientProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_TimePreferences_PatientProfiles_PatientProfileId",
                table: "TimePreferences",
                column: "PatientProfileId",
                principalTable: "PatientProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
