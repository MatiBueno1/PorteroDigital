using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PorteroDigital.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddResidentAuthAndDevices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastLoginAtUtc",
                table: "Residents",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Residents",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "ResidentDevices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ResidentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DeviceName = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Platform = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    PushToken = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    NotificationSound = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    LastSeenAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResidentDevices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResidentDevices_Residents_ResidentId",
                        column: x => x.ResidentId,
                        principalTable: "Residents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Residents",
                columns: new[] { "Id", "CreatedAtUtc", "Email", "FullName", "HouseId", "IsActive", "IsPrimaryContact", "LastLoginAtUtc", "PasswordHash", "PhoneNumber" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000001"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "casa01@portero.local", "Inquilino Casa 1", new Guid("00000000-0000-0000-0000-000000000001"), true, true, null, "v1.AAAAEAAAAAAAAAAAAAAAAQ==.if5Ty0ZIb7uzTwuz43QMHdTGByrmZSlQuD1ZjISQaSg=", "3410000001" },
                    { new Guid("10000000-0000-0000-0000-000000000002"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "casa02@portero.local", "Inquilino Casa 2", new Guid("00000000-0000-0000-0000-000000000002"), true, true, null, "v1.AAAAEAAAAAAAAAAAAAAAAg==.HoRkP04/2zC8kH+PBJf5jzVpzG8OdFFYQ6rYTEOGkJA=", "3410000002" },
                    { new Guid("10000000-0000-0000-0000-000000000003"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "casa03@portero.local", "Inquilino Casa 3", new Guid("00000000-0000-0000-0000-000000000003"), true, true, null, "v1.AAAAEAAAAAAAAAAAAAAAAw==.xgAR/ZBqox9YEGa+NWkxkcGplfPAttlK3ouEXOdBIog=", "3410000003" },
                    { new Guid("10000000-0000-0000-0000-000000000004"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "casa04@portero.local", "Inquilino Casa 4", new Guid("00000000-0000-0000-0000-000000000004"), true, true, null, "v1.AAAAEAAAAAAAAAAAAAAABA==.SU2Zziy244EHNoUH3QuduuS9Ux1fIwq5fxfirB5BoX8=", "3410000004" },
                    { new Guid("10000000-0000-0000-0000-000000000005"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "casa05@portero.local", "Inquilino Casa 5", new Guid("00000000-0000-0000-0000-000000000005"), true, true, null, "v1.AAAAEAAAAAAAAAAAAAAABQ==.DtEZz6DxiIs3xIB9XUZxvs6mhejZNRKTmgeiC4A7DvE=", "3410000005" },
                    { new Guid("10000000-0000-0000-0000-000000000006"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "casa06@portero.local", "Inquilino Casa 6", new Guid("00000000-0000-0000-0000-000000000006"), true, true, null, "v1.AAAAEAAAAAAAAAAAAAAABg==.9RBXsWBiqtiHn3bGzzvtGowMEX3nWl/bX8adfZbdml4=", "3410000006" },
                    { new Guid("10000000-0000-0000-0000-000000000007"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "casa07@portero.local", "Inquilino Casa 7", new Guid("00000000-0000-0000-0000-000000000007"), true, true, null, "v1.AAAAEAAAAAAAAAAAAAAABw==.cPEfqFkG0nu0mZ+92yenG+Upti48XbgsRinAWsB35Qc=", "3410000007" },
                    { new Guid("10000000-0000-0000-0000-000000000008"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "casa08@portero.local", "Inquilino Casa 8", new Guid("00000000-0000-0000-0000-000000000008"), true, true, null, "v1.AAAAEAAAAAAAAAAAAAAACA==.WUd5KIUtdXY7/RmzWvEIEmhRmfhDT7D2KrSCDDzqVeE=", "3410000008" },
                    { new Guid("10000000-0000-0000-0000-000000000009"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "casa09@portero.local", "Inquilino Casa 9", new Guid("00000000-0000-0000-0000-000000000009"), true, true, null, "v1.AAAAEAAAAAAAAAAAAAAACQ==.nz13qjy7MdjuD0qJagl0gwwkVHICu+oL4CmLD2579tU=", "3410000009" },
                    { new Guid("10000000-0000-0000-0000-000000000010"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "casa10@portero.local", "Inquilino Casa 10", new Guid("00000000-0000-0000-0000-000000000010"), true, true, null, "v1.AAAAEAAAAAAAAAAAAAAAEA==.HuuxfBRKW1NnRf7WQW/kgzI3+gkTEXROmX26KeX40lg=", "3410000010" },
                    { new Guid("10000000-0000-0000-0000-000000000011"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "casa11@portero.local", "Inquilino Casa 11", new Guid("00000000-0000-0000-0000-000000000011"), true, true, null, "v1.AAAAEAAAAAAAAAAAAAAAEQ==.3CJUB3SwaQpTzOEnh07vX2haqBHvFApVhOLqF+OEitM=", "3410000011" },
                    { new Guid("10000000-0000-0000-0000-000000000012"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "casa12@portero.local", "Inquilino Casa 12", new Guid("00000000-0000-0000-0000-000000000012"), true, true, null, "v1.AAAAEAAAAAAAAAAAAAAAEg==.59/X/RKEDVnXVS/tjcpt4DMAPCBsRGSvfw9t4gX4GpE=", "3410000012" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ResidentDevices_ResidentId_PushToken",
                table: "ResidentDevices",
                columns: new[] { "ResidentId", "PushToken" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ResidentDevices");

            migrationBuilder.DeleteData(
                table: "Residents",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Residents",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Residents",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "Residents",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "Residents",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "Residents",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000006"));

            migrationBuilder.DeleteData(
                table: "Residents",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000007"));

            migrationBuilder.DeleteData(
                table: "Residents",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000008"));

            migrationBuilder.DeleteData(
                table: "Residents",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000009"));

            migrationBuilder.DeleteData(
                table: "Residents",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000010"));

            migrationBuilder.DeleteData(
                table: "Residents",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000011"));

            migrationBuilder.DeleteData(
                table: "Residents",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000012"));

            migrationBuilder.DropColumn(
                name: "LastLoginAtUtc",
                table: "Residents");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "Residents");
        }
    }
}
