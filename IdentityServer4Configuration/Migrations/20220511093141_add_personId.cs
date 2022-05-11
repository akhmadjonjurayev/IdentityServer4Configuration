using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IdentityServer4Configuration.Migrations
{
    public partial class add_personId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PersonId",
                schema: "identity",
                table: "AspNetUsers",
                type: "uniqueidentifier",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PersonId",
                schema: "identity",
                table: "AspNetUsers");
        }
    }
}
