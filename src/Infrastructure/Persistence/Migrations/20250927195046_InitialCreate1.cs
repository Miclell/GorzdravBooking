using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_AppointmentSearchRequests_Period",
                table: "AppointmentSearchRequests");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_AppointmentSearchRequests_Period",
                table: "AppointmentSearchRequests",
                sql: "(\"PeriodStart\" IS NULL AND \"PeriodEnd\" IS NULL) OR \r\n            (\"PeriodStart\" IS NOT NULL AND \"PeriodEnd\" IS NOT NULL AND \r\n            \"PeriodStart\" <= \"PeriodEnd\")");
        }
    }
}
