using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppConfiguration",
                columns: table => new
                {
                    Key = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppConfiguration", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Username = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.CheckConstraint("CK_Users_Username_Length", "LENGTH(\"username\") >= 3");
                });

            migrationBuilder.CreateTable(
                name: "PatientProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LpuId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    PatientId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    LpuShortName = table.Column<string>(type: "TEXT", nullable: false),
                    LpuAddress = table.Column<string>(type: "TEXT", nullable: false),
                    RecipientEmail = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    MobilePhoneNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    PatientLastName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    PatientFirstName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    PatientMiddleName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    PatientBirthdate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientProfiles", x => x.Id);
                    table.CheckConstraint("CK_PatientProfiles_Birthdate", "\"PatientBirthdate\" <= CURRENT_DATE");
                    table.CheckConstraint("CK_PatientProfiles_Email_Format", "\"RecipientEmail\" IS NULL OR \"RecipientEmail\" LIKE '%@%.%'");
                    table.ForeignKey(
                        name: "FK_PatientProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TimePreferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Day = table.Column<int>(type: "INTEGER", nullable: true),
                    PreferredTimeFrom = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                    PreferredTimeTo = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                    AnyTime = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimePreferences", x => x.Id);
                    table.CheckConstraint("CK_TimePreferences_Day_Required", "(\"AnyTime\" = true OR \"Day\" IS NOT NULL)");
                    table.CheckConstraint("CK_TimePreferences_TimeRange", "(\"AnyTime\" = true OR (\"PreferredTimeFrom\" IS NOT NULL AND \r\n            \"PreferredTimeTo\" IS NOT NULL AND \r\n            \"PreferredTimeFrom\" < \"PreferredTimeTo\"))");
                    table.ForeignKey(
                        name: "FK_TimePreferences_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Appointments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PatientProfileId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AppointmentId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    VisitStart = table.Column<DateTime>(type: "TEXT", nullable: false),
                    VisitEnd = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Address = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Number = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Room = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Speciality = table.Column<string>(type: "TEXT", nullable: false),
                    Doctor = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appointments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Appointments_PatientProfiles_PatientProfileId",
                        column: x => x.PatientProfileId,
                        principalTable: "PatientProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Записи на прием к врачу");

            migrationBuilder.CreateTable(
                name: "AppointmentSearchRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PatientProfileId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LpuName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    DoctorId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    DoctorName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Speciality = table.Column<string>(type: "TEXT", nullable: false),
                    SearchInterval = table.Column<long>(type: "INTEGER", nullable: false, defaultValue: 36000000000L),
                    SpecificStartPoints = table.Column<string>(type: "TEXT", nullable: false),
                    TimePreferencesPresetName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ViewOnly = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastSearchAttempt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AttemptCount = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    Status = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "Pending"),
                    MaxDaysToSearch = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 30)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppointmentSearchRequests", x => x.Id);
                    table.CheckConstraint("CK_AppointmentSearchRequests_AttemptCount", "\"AttemptCount\" >= 0");
                    table.ForeignKey(
                        name: "FK_AppointmentSearchRequests_PatientProfiles_PatientProfileId",
                        column: x => x.PatientProfileId,
                        principalTable: "PatientProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_AppointmentId_Unique",
                table: "Appointments",
                column: "AppointmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_PatientId_VisitStart",
                table: "Appointments",
                columns: new[] { "PatientProfileId", "VisitStart" });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_PatientProfileId",
                table: "Appointments",
                column: "PatientProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_VisitEnd",
                table: "Appointments",
                column: "VisitEnd");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_VisitStart",
                table: "Appointments",
                column: "VisitStart");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentSearchRequests_CreatedAt",
                table: "AppointmentSearchRequests",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentSearchRequests_Doctor_Lpu",
                table: "AppointmentSearchRequests",
                columns: new[] { "DoctorId", "LpuName" });

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentSearchRequests_PatientProfileId",
                table: "AppointmentSearchRequests",
                column: "PatientProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentSearchRequests_Status",
                table: "AppointmentSearchRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PatientProfiles_PatientId",
                table: "PatientProfiles",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientProfiles_UserId",
                table: "PatientProfiles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientProfiles_UserId_PatientId_Unique",
                table: "PatientProfiles",
                columns: new[] { "UserId", "PatientId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TimePreferences_UserId",
                table: "TimePreferences",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TimePreferences_UserId_Day",
                table: "TimePreferences",
                columns: new[] { "UserId", "Day" });

            migrationBuilder.CreateIndex(
                name: "IX_TimePreferences_UserId_Name",
                table: "TimePreferences",
                columns: new[] { "UserId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_CreatedAt",
                table: "Users",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppConfiguration");

            migrationBuilder.DropTable(
                name: "Appointments");

            migrationBuilder.DropTable(
                name: "AppointmentSearchRequests");

            migrationBuilder.DropTable(
                name: "TimePreferences");

            migrationBuilder.DropTable(
                name: "PatientProfiles");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
