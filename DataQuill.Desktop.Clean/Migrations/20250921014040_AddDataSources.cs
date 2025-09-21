using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DataQuillDesktop.Migrations
{
    /// <inheritdoc />
    public partial class AddDataSources : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DataSources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    InterfaceType = table.Column<string>(type: "text", nullable: false),
                    ProtocolType = table.Column<string>(type: "text", nullable: false),
                    ConnectionString = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Configuration_Host = table.Column<string>(type: "text", nullable: false),
                    Configuration_Port = table.Column<int>(type: "integer", nullable: false),
                    Configuration_Username = table.Column<string>(type: "text", nullable: false),
                    Configuration_Password = table.Column<string>(type: "text", nullable: false),
                    Configuration_Timeout = table.Column<int>(type: "integer", nullable: false),
                    Configuration_RetryAttempts = table.Column<int>(type: "integer", nullable: false),
                    Configuration_UseSSL = table.Column<bool>(type: "boolean", nullable: false),
                    Configuration_FilePath = table.Column<string>(type: "text", nullable: false),
                    Configuration_FilePattern = table.Column<string>(type: "text", nullable: false),
                    Configuration_WatchForChanges = table.Column<bool>(type: "boolean", nullable: false),
                    Configuration_PortName = table.Column<string>(type: "text", nullable: false),
                    Configuration_BaudRate = table.Column<int>(type: "integer", nullable: false),
                    Configuration_DataBits = table.Column<int>(type: "integer", nullable: false),
                    Configuration_Parity = table.Column<string>(type: "text", nullable: false),
                    Configuration_StopBits = table.Column<string>(type: "text", nullable: false),
                    Configuration_Handshake = table.Column<string>(type: "text", nullable: false),
                    Configuration_Topic = table.Column<string>(type: "text", nullable: false),
                    Configuration_SlaveId = table.Column<int>(type: "integer", nullable: false),
                    Configuration_Endpoint = table.Column<string>(type: "text", nullable: false),
                    Configuration_ApiKey = table.Column<string>(type: "text", nullable: false),
                    Configuration_ClientId = table.Column<string>(type: "text", nullable: false),
                    Configuration_CustomParameters = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataSources", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DataSources");
        }
    }
}
