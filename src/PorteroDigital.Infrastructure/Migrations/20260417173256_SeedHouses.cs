using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PorteroDigital.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedHouses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Houses",
                columns: new[] { "Id", "AddressLabel", "CreatedAtUtc", "Identifier", "IsActive", "QrToken" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000001"), "Pasillo Unidad 1", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Casa 1", true, "TOKEN-CASA-01" },
                    { new Guid("00000000-0000-0000-0000-000000000002"), "Pasillo Unidad 2", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Casa 2", true, "TOKEN-CASA-02" },
                    { new Guid("00000000-0000-0000-0000-000000000003"), "Pasillo Unidad 3", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Casa 3", true, "TOKEN-CASA-03" },
                    { new Guid("00000000-0000-0000-0000-000000000004"), "Pasillo Unidad 4", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Casa 4", true, "TOKEN-CASA-04" },
                    { new Guid("00000000-0000-0000-0000-000000000005"), "Pasillo Unidad 5", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Casa 5", true, "TOKEN-CASA-05" },
                    { new Guid("00000000-0000-0000-0000-000000000006"), "Pasillo Unidad 6", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Casa 6", true, "TOKEN-CASA-06" },
                    { new Guid("00000000-0000-0000-0000-000000000007"), "Pasillo Unidad 7", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Casa 7", true, "TOKEN-CASA-07" },
                    { new Guid("00000000-0000-0000-0000-000000000008"), "Pasillo Unidad 8", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Casa 8", true, "TOKEN-CASA-08" },
                    { new Guid("00000000-0000-0000-0000-000000000009"), "Pasillo Unidad 9", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Casa 9", true, "TOKEN-CASA-09" },
                    { new Guid("00000000-0000-0000-0000-000000000010"), "Pasillo Unidad 10", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Casa 10", true, "TOKEN-CASA-10" },
                    { new Guid("00000000-0000-0000-0000-000000000011"), "Pasillo Unidad 11", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Casa 11", true, "TOKEN-CASA-11" },
                    { new Guid("00000000-0000-0000-0000-000000000012"), "Pasillo Unidad 12", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Casa 12", true, "TOKEN-CASA-12" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Houses",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Houses",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Houses",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "Houses",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "Houses",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "Houses",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000006"));

            migrationBuilder.DeleteData(
                table: "Houses",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000007"));

            migrationBuilder.DeleteData(
                table: "Houses",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000008"));

            migrationBuilder.DeleteData(
                table: "Houses",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000009"));

            migrationBuilder.DeleteData(
                table: "Houses",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000010"));

            migrationBuilder.DeleteData(
                table: "Houses",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000011"));

            migrationBuilder.DeleteData(
                table: "Houses",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000012"));
        }
    }
}
