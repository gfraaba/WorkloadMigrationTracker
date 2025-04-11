using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApi.Migrations
{
    /// <inheritdoc />
    public partial class UpdateResourceTypeIdToTypeId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Resources_ResourceTypes_ResourceTypeId",
                table: "Resources");

            migrationBuilder.RenameColumn(
                name: "ResourceTypeId",
                table: "Resources",
                newName: "TypeId");

            migrationBuilder.RenameIndex(
                name: "IX_Resources_ResourceTypeId",
                table: "Resources",
                newName: "IX_Resources_TypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Resources_ResourceTypes_TypeId",
                table: "Resources",
                column: "TypeId",
                principalTable: "ResourceTypes",
                principalColumn: "TypeId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Resources_ResourceTypes_TypeId",
                table: "Resources");

            migrationBuilder.RenameColumn(
                name: "TypeId",
                table: "Resources",
                newName: "ResourceTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_Resources_TypeId",
                table: "Resources",
                newName: "IX_Resources_ResourceTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Resources_ResourceTypes_ResourceTypeId",
                table: "Resources",
                column: "ResourceTypeId",
                principalTable: "ResourceTypes",
                principalColumn: "TypeId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
