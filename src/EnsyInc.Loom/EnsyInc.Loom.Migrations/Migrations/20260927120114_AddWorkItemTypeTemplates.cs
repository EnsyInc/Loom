using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnsyInc.Loom.Migrations.Migrations;

/// <inheritdoc />
public partial class AddWorkItemTypeTemplates : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "TplWorkItemStatuses",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                Category = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TplWorkItemStatuses", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "TplWorkItems",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                IconUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                InitialStatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TplWorkItems", x => x.Id);
                table.ForeignKey(
                    name: "FK_TplWorkItems_TplWorkItemStatuses_InitialStatusId",
                    column: x => x.InitialStatusId,
                    principalTable: "TplWorkItemStatuses",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "ProjectWorkItemTypes",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProjectWorkItemTypes", x => x.Id);
                table.ForeignKey(
                    name: "FK_ProjectWorkItemTypes_Projects_ProjectId",
                    column: x => x.ProjectId,
                    principalTable: "Projects",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_ProjectWorkItemTypes_TplWorkItems_TypeId",
                    column: x => x.TypeId,
                    principalTable: "TplWorkItems",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "StatusTransitions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                TypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                FromStatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ToStatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_StatusTransitions", x => x.Id);
                table.CheckConstraint("CK_StatusTransition_FromToDifferent", "[FromStatusId] <> [ToStatusId]");
                table.ForeignKey(
                    name: "FK_StatusTransitions_TplWorkItemStatuses_FromStatusId",
                    column: x => x.FromStatusId,
                    principalTable: "TplWorkItemStatuses",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_StatusTransitions_TplWorkItemStatuses_ToStatusId",
                    column: x => x.ToStatusId,
                    principalTable: "TplWorkItemStatuses",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_StatusTransitions_TplWorkItems_TypeId",
                    column: x => x.TypeId,
                    principalTable: "TplWorkItems",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "TplWorkItemFields",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                TypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Key = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                Label = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                DataType = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                Required = table.Column<bool>(type: "bit", nullable: false),
                DefaultValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TplWorkItemFields", x => x.Id);
                table.ForeignKey(
                    name: "FK_TplWorkItemFields_TplWorkItems_TypeId",
                    column: x => x.TypeId,
                    principalTable: "TplWorkItems",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "WorkItemTypeStatuses",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                TypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                StatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_WorkItemTypeStatuses", x => x.Id);
                table.ForeignKey(
                    name: "FK_WorkItemTypeStatuses_TplWorkItemStatuses_StatusId",
                    column: x => x.StatusId,
                    principalTable: "TplWorkItemStatuses",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_WorkItemTypeStatuses_TplWorkItems_TypeId",
                    column: x => x.TypeId,
                    principalTable: "TplWorkItems",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "TplWorkItemFieldOptions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                FieldId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Value = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                Label = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                Rank = table.Column<int>(type: "int", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TplWorkItemFieldOptions", x => x.Id);
                table.ForeignKey(
                    name: "FK_TplWorkItemFieldOptions_TplWorkItemFields_FieldId",
                    column: x => x.FieldId,
                    principalTable: "TplWorkItemFields",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ProjectWorkItemTypes_ProjectId_TypeId",
            table: "ProjectWorkItemTypes",
            columns: new[] { "ProjectId", "TypeId" },
            unique: true,
            filter: "[DeletedAt] IS NULL");

        migrationBuilder.CreateIndex(
            name: "IX_ProjectWorkItemTypes_TypeId",
            table: "ProjectWorkItemTypes",
            column: "TypeId");

        migrationBuilder.CreateIndex(
            name: "IX_StatusTransitions_FromStatusId",
            table: "StatusTransitions",
            column: "FromStatusId");

        migrationBuilder.CreateIndex(
            name: "IX_StatusTransitions_ToStatusId",
            table: "StatusTransitions",
            column: "ToStatusId");

        migrationBuilder.CreateIndex(
            name: "IX_StatusTransitions_TypeId_FromStatusId_ToStatusId",
            table: "StatusTransitions",
            columns: new[] { "TypeId", "FromStatusId", "ToStatusId" },
            unique: true,
            filter: "[DeletedAt] IS NULL");

        migrationBuilder.CreateIndex(
            name: "IX_TplWorkItemFieldOptions_FieldId_Value",
            table: "TplWorkItemFieldOptions",
            columns: new[] { "FieldId", "Value" },
            unique: true,
            filter: "[DeletedAt] IS NULL");

        migrationBuilder.CreateIndex(
            name: "IX_TplWorkItemFields_TypeId_Key",
            table: "TplWorkItemFields",
            columns: new[] { "TypeId", "Key" },
            unique: true,
            filter: "[DeletedAt] IS NULL");

        migrationBuilder.CreateIndex(
            name: "IX_TplWorkItems_InitialStatusId",
            table: "TplWorkItems",
            column: "InitialStatusId");

        migrationBuilder.CreateIndex(
            name: "IX_TplWorkItems_Name",
            table: "TplWorkItems",
            column: "Name",
            unique: true,
            filter: "[DeletedAt] IS NULL");

        migrationBuilder.CreateIndex(
            name: "IX_TplWorkItemStatuses_Name",
            table: "TplWorkItemStatuses",
            column: "Name",
            unique: true,
            filter: "[DeletedAt] IS NULL");

        migrationBuilder.CreateIndex(
            name: "IX_WorkItemTypeStatuses_StatusId",
            table: "WorkItemTypeStatuses",
            column: "StatusId");

        migrationBuilder.CreateIndex(
            name: "IX_WorkItemTypeStatuses_TypeId_StatusId",
            table: "WorkItemTypeStatuses",
            columns: new[] { "TypeId", "StatusId" },
            unique: true,
            filter: "[DeletedAt] IS NULL");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ProjectWorkItemTypes");

        migrationBuilder.DropTable(
            name: "StatusTransitions");

        migrationBuilder.DropTable(
            name: "TplWorkItemFieldOptions");

        migrationBuilder.DropTable(
            name: "WorkItemTypeStatuses");

        migrationBuilder.DropTable(
            name: "TplWorkItemFields");

        migrationBuilder.DropTable(
            name: "TplWorkItems");

        migrationBuilder.DropTable(
            name: "TplWorkItemStatuses");
    }
}
