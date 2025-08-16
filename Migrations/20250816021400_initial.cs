using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PLCDataCollector.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LineDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LineId = table.Column<string>(type: "TEXT", nullable: false),
                    LineName = table.Column<string>(type: "TEXT", nullable: false),
                    LineType = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    IsActive = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Data_Location = table.Column<string>(type: "TEXT", nullable: false),
                    PLC = table.Column<string>(type: "TEXT", nullable: false),
                    ShiftConfiguration_ShiftA_Name = table.Column<string>(type: "TEXT", nullable: false),
                    ShiftConfiguration_ShiftA_StartTime = table.Column<string>(type: "TEXT", nullable: false),
                    ShiftConfiguration_ShiftA_EndTime = table.Column<string>(type: "TEXT", nullable: false),
                    ShiftConfiguration_ShiftA_Color = table.Column<string>(type: "TEXT", nullable: false),
                    ShiftConfiguration_ShiftA_BreakTimes = table.Column<string>(type: "TEXT", nullable: false),
                    ShiftConfiguration_ShiftB_Name = table.Column<string>(type: "TEXT", nullable: false),
                    ShiftConfiguration_ShiftB_StartTime = table.Column<string>(type: "TEXT", nullable: false),
                    ShiftConfiguration_ShiftB_EndTime = table.Column<string>(type: "TEXT", nullable: false),
                    ShiftConfiguration_ShiftB_Color = table.Column<string>(type: "TEXT", nullable: false),
                    ShiftConfiguration_ShiftB_BreakTimes = table.Column<string>(type: "TEXT", nullable: false),
                    ShiftConfiguration_ShiftC_Name = table.Column<string>(type: "TEXT", nullable: false),
                    ShiftConfiguration_ShiftC_StartTime = table.Column<string>(type: "TEXT", nullable: false),
                    ShiftConfiguration_ShiftC_EndTime = table.Column<string>(type: "TEXT", nullable: false),
                    ShiftConfiguration_ShiftC_Color = table.Column<string>(type: "TEXT", nullable: false),
                    ShiftConfiguration_ShiftC_BreakTimes = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LineDetails", x => x.Id);
                    table.UniqueConstraint("AK_LineDetails_LineId", x => x.LineId);
                });

            migrationBuilder.CreateTable(
                name: "ConfigurationSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LineId = table.Column<string>(type: "TEXT", nullable: false),
                    SettingKey = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    SettingValue = table.Column<string>(type: "TEXT", nullable: false),
                    DataType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigurationSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConfigurationSettings_LineDetails_LineId",
                        column: x => x.LineId,
                        principalTable: "LineDetails",
                        principalColumn: "LineId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MaintenanceLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LineId = table.Column<string>(type: "TEXT", nullable: false),
                    MaintenanceType = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    ScheduledDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    Technician = table.Column<string>(type: "TEXT", nullable: false),
                    Comments = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaintenanceLogs_LineDetails_LineId",
                        column: x => x.LineId,
                        principalTable: "LineDetails",
                        principalColumn: "LineId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlcData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LineId = table.Column<string>(type: "TEXT", nullable: false),
                    CurrentCount = table.Column<int>(type: "INTEGER", nullable: false),
                    PartNumber = table.Column<string>(type: "TEXT", nullable: false),
                    CycleTime = table.Column<double>(type: "REAL", nullable: false),
                    SyncStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    IsRunning = table.Column<bool>(type: "INTEGER", nullable: false),
                    RawData = table.Column<string>(type: "TEXT", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlcData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlcData_LineDetails_LineId",
                        column: x => x.LineId,
                        principalTable: "LineDetails",
                        principalColumn: "LineId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Shifts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LineId = table.Column<string>(type: "TEXT", nullable: false),
                    ShiftName = table.Column<string>(type: "TEXT", nullable: false),
                    StartTime = table.Column<string>(type: "TEXT", nullable: false),
                    EndTime = table.Column<string>(type: "TEXT", nullable: false),
                    IsActive = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shifts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Shifts_LineDetails_LineId",
                        column: x => x.LineId,
                        principalTable: "LineDetails",
                        principalColumn: "LineId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LineId = table.Column<string>(type: "TEXT", nullable: false),
                    TagName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Address = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    DataType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    ScanRate = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tags_LineDetails_LineId",
                        column: x => x.LineId,
                        principalTable: "LineDetails",
                        principalColumn: "LineId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Downtimes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LineId = table.Column<string>(type: "TEXT", nullable: false),
                    ShiftId = table.Column<int>(type: "INTEGER", nullable: false),
                    StartTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Duration = table.Column<int>(type: "INTEGER", nullable: true),
                    Reason = table.Column<string>(type: "TEXT", nullable: false),
                    Category = table.Column<string>(type: "TEXT", nullable: false),
                    Comments = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Downtimes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Downtimes_LineDetails_LineId",
                        column: x => x.LineId,
                        principalTable: "LineDetails",
                        principalColumn: "LineId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Downtimes_Shifts_ShiftId",
                        column: x => x.ShiftId,
                        principalTable: "Shifts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductionSchedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LineId = table.Column<string>(type: "TEXT", nullable: false),
                    ShiftId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductCode = table.Column<string>(type: "TEXT", nullable: false),
                    PlannedQuantity = table.Column<int>(type: "INTEGER", nullable: false),
                    StartTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionSchedules_LineDetails_LineId",
                        column: x => x.LineId,
                        principalTable: "LineDetails",
                        principalColumn: "LineId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductionSchedules_Shifts_ShiftId",
                        column: x => x.ShiftId,
                        principalTable: "Shifts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AlarmDefinitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LineId = table.Column<string>(type: "TEXT", nullable: false),
                    TagId = table.Column<int>(type: "INTEGER", nullable: false),
                    AlarmType = table.Column<string>(type: "TEXT", nullable: false),
                    Threshold = table.Column<double>(type: "REAL", nullable: false),
                    Severity = table.Column<string>(type: "TEXT", nullable: false),
                    Message = table.Column<string>(type: "TEXT", nullable: false),
                    IsEnabled = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlarmDefinitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlarmDefinitions_LineDetails_LineId",
                        column: x => x.LineId,
                        principalTable: "LineDetails",
                        principalColumn: "LineId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlarmDefinitions_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TagHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TagId = table.Column<int>(type: "INTEGER", nullable: false),
                    LineId = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false),
                    Quality = table.Column<string>(type: "TEXT", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TagHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TagHistory_LineDetails_LineId",
                        column: x => x.LineId,
                        principalTable: "LineDetails",
                        principalColumn: "LineId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TagHistory_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductionData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LineId = table.Column<string>(type: "TEXT", nullable: false),
                    ScheduleId = table.Column<int>(type: "INTEGER", nullable: true),
                    ShiftId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductCode = table.Column<string>(type: "TEXT", nullable: false),
                    PartNumber = table.Column<string>(type: "TEXT", nullable: false),
                    ShiftName = table.Column<string>(type: "TEXT", nullable: false),
                    ActualCount = table.Column<int>(type: "INTEGER", nullable: false),
                    PlannedCount = table.Column<int>(type: "INTEGER", nullable: false),
                    GoodQuantity = table.Column<int>(type: "INTEGER", nullable: false),
                    ScrapQuantity = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "nvarchar(50)", nullable: false),
                    CycleTime = table.Column<decimal>(type: "TEXT", nullable: false),
                    Efficiency = table.Column<decimal>(type: "TEXT", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionData_LineDetails_LineId",
                        column: x => x.LineId,
                        principalTable: "LineDetails",
                        principalColumn: "LineId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductionData_ProductionSchedules_ScheduleId",
                        column: x => x.ScheduleId,
                        principalTable: "ProductionSchedules",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductionData_Shifts_ShiftId",
                        column: x => x.ShiftId,
                        principalTable: "Shifts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AlarmHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AlarmDefinitionId = table.Column<int>(type: "INTEGER", nullable: false),
                    LineId = table.Column<string>(type: "TEXT", nullable: false),
                    TagValue = table.Column<double>(type: "REAL", nullable: false),
                    TriggeredAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AcknowledgedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AcknowledgedBy = table.Column<string>(type: "TEXT", nullable: false),
                    ClearedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlarmHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlarmHistory_AlarmDefinitions_AlarmDefinitionId",
                        column: x => x.AlarmDefinitionId,
                        principalTable: "AlarmDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlarmHistory_LineDetails_LineId",
                        column: x => x.LineId,
                        principalTable: "LineDetails",
                        principalColumn: "LineId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QualityChecks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LineId = table.Column<string>(type: "TEXT", nullable: false),
                    ProductionDataId = table.Column<int>(type: "INTEGER", nullable: false),
                    CheckType = table.Column<string>(type: "TEXT", nullable: false),
                    Parameter = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<double>(type: "REAL", nullable: false),
                    Standard = table.Column<double>(type: "REAL", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    CheckedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CheckedBy = table.Column<string>(type: "TEXT", nullable: false),
                    ShiftsId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QualityChecks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QualityChecks_LineDetails_LineId",
                        column: x => x.LineId,
                        principalTable: "LineDetails",
                        principalColumn: "LineId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QualityChecks_ProductionData_ProductionDataId",
                        column: x => x.ProductionDataId,
                        principalTable: "ProductionData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QualityChecks_Shifts_ShiftsId",
                        column: x => x.ShiftsId,
                        principalTable: "Shifts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlarmDefinitions_LineId",
                table: "AlarmDefinitions",
                column: "LineId");

            migrationBuilder.CreateIndex(
                name: "IX_AlarmDefinitions_TagId",
                table: "AlarmDefinitions",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_AlarmHistory_AlarmDefinitionId",
                table: "AlarmHistory",
                column: "AlarmDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_AlarmHistory_LineId",
                table: "AlarmHistory",
                column: "LineId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfigurationSettings_LineId_SettingKey",
                table: "ConfigurationSettings",
                columns: new[] { "LineId", "SettingKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Downtimes_LineId",
                table: "Downtimes",
                column: "LineId");

            migrationBuilder.CreateIndex(
                name: "IX_Downtimes_ShiftId",
                table: "Downtimes",
                column: "ShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_LineDetails_LineId",
                table: "LineDetails",
                column: "LineId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceLogs_LineId",
                table: "MaintenanceLogs",
                column: "LineId");

            migrationBuilder.CreateIndex(
                name: "IX_PlcData_LineId_SyncStatus",
                table: "PlcData",
                columns: new[] { "LineId", "SyncStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductionData_LineId",
                table: "ProductionData",
                column: "LineId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionData_ScheduleId",
                table: "ProductionData",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionData_ShiftId",
                table: "ProductionData",
                column: "ShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionSchedules_LineId",
                table: "ProductionSchedules",
                column: "LineId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionSchedules_ShiftId",
                table: "ProductionSchedules",
                column: "ShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_QualityChecks_LineId",
                table: "QualityChecks",
                column: "LineId");

            migrationBuilder.CreateIndex(
                name: "IX_QualityChecks_ProductionDataId",
                table: "QualityChecks",
                column: "ProductionDataId");

            migrationBuilder.CreateIndex(
                name: "IX_QualityChecks_ShiftsId",
                table: "QualityChecks",
                column: "ShiftsId");

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_LineId",
                table: "Shifts",
                column: "LineId");

            migrationBuilder.CreateIndex(
                name: "IX_TagHistory_LineId",
                table: "TagHistory",
                column: "LineId");

            migrationBuilder.CreateIndex(
                name: "IX_TagHistory_TagId",
                table: "TagHistory",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_LineId_TagName",
                table: "Tags",
                columns: new[] { "LineId", "TagName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlarmHistory");

            migrationBuilder.DropTable(
                name: "ConfigurationSettings");

            migrationBuilder.DropTable(
                name: "Downtimes");

            migrationBuilder.DropTable(
                name: "MaintenanceLogs");

            migrationBuilder.DropTable(
                name: "PlcData");

            migrationBuilder.DropTable(
                name: "QualityChecks");

            migrationBuilder.DropTable(
                name: "TagHistory");

            migrationBuilder.DropTable(
                name: "AlarmDefinitions");

            migrationBuilder.DropTable(
                name: "ProductionData");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "ProductionSchedules");

            migrationBuilder.DropTable(
                name: "Shifts");

            migrationBuilder.DropTable(
                name: "LineDetails");
        }
    }
}
