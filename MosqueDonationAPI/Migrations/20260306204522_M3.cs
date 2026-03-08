using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MosqueDonationAPI.Migrations
{
    /// <inheritdoc />
    public partial class M3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChildAttendances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ChildId = table.Column<int>(type: "INTEGER", nullable: false),
                    ClassId = table.Column<int>(type: "INTEGER", nullable: false),
                    MosqueId = table.Column<int>(type: "INTEGER", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    CheckInTime = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                    CheckOutTime = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                    Remarks = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    MarkedById = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChildAttendances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChildAttendances_Children_ChildId",
                        column: x => x.ChildId,
                        principalTable: "Children",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChildAttendances_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChildAttendances_Imaams_MarkedById",
                        column: x => x.MarkedById,
                        principalTable: "Imaams",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ChildAttendances_Mosques_MosqueId",
                        column: x => x.MosqueId,
                        principalTable: "Mosques",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChildFees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ChildId = table.Column<int>(type: "INTEGER", nullable: false),
                    ClassId = table.Column<int>(type: "INTEGER", nullable: false),
                    MosqueId = table.Column<int>(type: "INTEGER", nullable: false),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    Month = table.Column<int>(type: "INTEGER", nullable: false),
                    TuitionFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AdmissionFee = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ExaminationFee = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    BooksFee = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    UniformFee = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    OtherFees = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    OtherFeesDescription = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ScholarshipDiscount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SiblingDiscount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    OtherDiscount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DiscountRemarks = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    AmountPaid = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentStatus = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    LastPaymentDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastPaymentMethod = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    DueDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LateFee = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ProcessedById = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChildFees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChildFees_Children_ChildId",
                        column: x => x.ChildId,
                        principalTable: "Children",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChildFees_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChildFees_Mosques_MosqueId",
                        column: x => x.MosqueId,
                        principalTable: "Mosques",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChildFees_Users_ProcessedById",
                        column: x => x.ProcessedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ImaamAttendances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ImaamId = table.Column<int>(type: "INTEGER", nullable: false),
                    MosqueId = table.Column<int>(type: "INTEGER", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    CheckInTime = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                    CheckOutTime = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                    Remarks = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    MarkedById = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImaamAttendances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImaamAttendances_Imaams_ImaamId",
                        column: x => x.ImaamId,
                        principalTable: "Imaams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ImaamAttendances_Mosques_MosqueId",
                        column: x => x.MosqueId,
                        principalTable: "Mosques",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ImaamAttendances_Users_MarkedById",
                        column: x => x.MarkedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ImaamSalaries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ImaamId = table.Column<int>(type: "INTEGER", nullable: false),
                    MosqueId = table.Column<int>(type: "INTEGER", nullable: false),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    Month = table.Column<int>(type: "INTEGER", nullable: false),
                    BasicSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    HousingAllowance = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TransportAllowance = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    OtherAllowances = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AbsenceDeduction = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    LateDeduction = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    OtherDeductions = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DeductionRemarks = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    PaymentStatus = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PaymentMethod = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    PaymentRemarks = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ProcessedById = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImaamSalaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImaamSalaries_Imaams_ImaamId",
                        column: x => x.ImaamId,
                        principalTable: "Imaams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ImaamSalaries_Mosques_MosqueId",
                        column: x => x.MosqueId,
                        principalTable: "Mosques",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ImaamSalaries_Users_ProcessedById",
                        column: x => x.ProcessedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FeePayments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ChildFeeId = table.Column<int>(type: "INTEGER", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PaymentMethod = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Remarks = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ReceivedById = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeePayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeePayments_ChildFees_ChildFeeId",
                        column: x => x.ChildFeeId,
                        principalTable: "ChildFees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FeePayments_Users_ReceivedById",
                        column: x => x.ReceivedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChildAttendances_ChildId_Date",
                table: "ChildAttendances",
                columns: new[] { "ChildId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChildAttendances_ClassId",
                table: "ChildAttendances",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_ChildAttendances_MarkedById",
                table: "ChildAttendances",
                column: "MarkedById");

            migrationBuilder.CreateIndex(
                name: "IX_ChildAttendances_MosqueId",
                table: "ChildAttendances",
                column: "MosqueId");

            migrationBuilder.CreateIndex(
                name: "IX_ChildFees_ChildId_Year_Month",
                table: "ChildFees",
                columns: new[] { "ChildId", "Year", "Month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChildFees_ClassId",
                table: "ChildFees",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_ChildFees_MosqueId",
                table: "ChildFees",
                column: "MosqueId");

            migrationBuilder.CreateIndex(
                name: "IX_ChildFees_ProcessedById",
                table: "ChildFees",
                column: "ProcessedById");

            migrationBuilder.CreateIndex(
                name: "IX_FeePayments_ChildFeeId",
                table: "FeePayments",
                column: "ChildFeeId");

            migrationBuilder.CreateIndex(
                name: "IX_FeePayments_ReceivedById",
                table: "FeePayments",
                column: "ReceivedById");

            migrationBuilder.CreateIndex(
                name: "IX_ImaamAttendances_ImaamId_Date",
                table: "ImaamAttendances",
                columns: new[] { "ImaamId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImaamAttendances_MarkedById",
                table: "ImaamAttendances",
                column: "MarkedById");

            migrationBuilder.CreateIndex(
                name: "IX_ImaamAttendances_MosqueId",
                table: "ImaamAttendances",
                column: "MosqueId");

            migrationBuilder.CreateIndex(
                name: "IX_ImaamSalaries_ImaamId_Year_Month",
                table: "ImaamSalaries",
                columns: new[] { "ImaamId", "Year", "Month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImaamSalaries_MosqueId",
                table: "ImaamSalaries",
                column: "MosqueId");

            migrationBuilder.CreateIndex(
                name: "IX_ImaamSalaries_ProcessedById",
                table: "ImaamSalaries",
                column: "ProcessedById");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChildAttendances");

            migrationBuilder.DropTable(
                name: "FeePayments");

            migrationBuilder.DropTable(
                name: "ImaamAttendances");

            migrationBuilder.DropTable(
                name: "ImaamSalaries");

            migrationBuilder.DropTable(
                name: "ChildFees");
        }
    }
}
