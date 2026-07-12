using System;

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1861

namespace Praxiara.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialPersistence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "agent_tasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Goal = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: false),
                    State = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agent_tasks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ifs_environment_audit_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EnvironmentId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ActorId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Operation = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ConfigurationHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ifs_environment_audit_events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ifs_environments",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    BaseUri = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    Tenant = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Locale = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    EnvironmentKind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AllowedProjectionNames = table.Column<string>(type: "jsonb", nullable: false),
                    AuthenticationMode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SecretFilePath = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    TokenEndpoint = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    ClientId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ifs_environments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Payload = table.Column<string>(type: "jsonb", nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbox_messages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ifs_environment_audit_events_EnvironmentId_OccurredAt",
                table: "ifs_environment_audit_events",
                columns: new[] { "EnvironmentId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_outbox_messages_ProcessedAt_OccurredAt",
                table: "outbox_messages",
                columns: new[] { "ProcessedAt", "OccurredAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "agent_tasks");

            migrationBuilder.DropTable(
                name: "ifs_environment_audit_events");

            migrationBuilder.DropTable(
                name: "ifs_environments");

            migrationBuilder.DropTable(
                name: "outbox_messages");
        }
    }
}

#pragma warning restore CA1861