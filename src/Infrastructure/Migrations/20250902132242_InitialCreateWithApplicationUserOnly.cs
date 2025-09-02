using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FHIRAI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateWithApplicationUserOnly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "fhir_resources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    resource_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    fhir_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    version_id = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    resource_json = table.Column<string>(type: "jsonb", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "active"),
                    last_updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    fhir_created = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    search_parameters = table.Column<string>(type: "jsonb", nullable: true),
                    security_labels = table.Column<string>(type: "jsonb", nullable: true),
                    tags = table.Column<string>(type: "jsonb", nullable: true),
                    patient_reference = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    organization_reference = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    practitioner_reference = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    last_modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fhir_resources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "patients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    fhir_patient_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    first_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    last_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    middle_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    date_of_birth = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    gender = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    marital_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    address_line1 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    address_line2 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    state = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    postal_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    patient_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    emergency_contact_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    emergency_contact_phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    emergency_contact_email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    emergency_contact_relationship = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    primary_care_provider_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    managing_organization_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    last_modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_patients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "todo_lists",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    colour_code = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    last_modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_todo_lists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    user_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_user_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    email_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: true),
                    security_stamp = table.Column<string>(type: "text", nullable: true),
                    concurrency_stamp = table.Column<string>(type: "text", nullable: true),
                    phone_number = table.Column<string>(type: "text", nullable: true),
                    phone_number_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    two_factor_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    lockout_end = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    lockout_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    access_failed_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "patient_accesses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    patient_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    access_level = table.Column<int>(type: "integer", nullable: false),
                    granted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    granted_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    reason = table.Column<string>(type: "text", nullable: true),
                    is_emergency_access = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    emergency_justification = table.Column<string>(type: "text", nullable: true),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    last_modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_patient_accesses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_patient_accesses_patients_patient_id",
                        column: x => x.patient_id,
                        principalTable: "patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "patient_consents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    patient_id = table.Column<Guid>(type: "uuid", nullable: false),
                    consent_type = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    granted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    granted_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    revoked_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    revoked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_electronic_consent = table.Column<bool>(type: "boolean", nullable: false),
                    consent_ip_address = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    user_agent = table.Column<string>(type: "text", nullable: true),
                    purpose = table.Column<string>(type: "text", nullable: true),
                    details = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    last_modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_patient_consents", x => x.Id);
                    table.ForeignKey(
                        name: "fk_patient_consents_patients_patient_id",
                        column: x => x.patient_id,
                        principalTable: "patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "todo_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    list_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    priority = table.Column<int>(type: "integer", nullable: false, defaultValue: 2),
                    done = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    reminder = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    last_modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_todo_items", x => x.Id);
                    table.ForeignKey(
                        name: "fk_todo_items_todo_lists_list_id",
                        column: x => x.list_id,
                        principalTable: "todo_lists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "ix_fhir_resources_fhir_created",
                table: "fhir_resources",
                column: "fhir_created");

            migrationBuilder.CreateIndex(
                name: "ix_fhir_resources_fhir_id",
                table: "fhir_resources",
                column: "fhir_id");

            migrationBuilder.CreateIndex(
                name: "ix_fhir_resources_last_updated",
                table: "fhir_resources",
                column: "last_updated");

            migrationBuilder.CreateIndex(
                name: "ix_fhir_resources_organization_reference",
                table: "fhir_resources",
                column: "organization_reference");

            migrationBuilder.CreateIndex(
                name: "ix_fhir_resources_organization_reference_resource_type_status",
                table: "fhir_resources",
                columns: new[] { "organization_reference", "resource_type", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_fhir_resources_patient_reference",
                table: "fhir_resources",
                column: "patient_reference");

            migrationBuilder.CreateIndex(
                name: "ix_fhir_resources_patient_reference_resource_type_status",
                table: "fhir_resources",
                columns: new[] { "patient_reference", "resource_type", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_fhir_resources_practitioner_reference",
                table: "fhir_resources",
                column: "practitioner_reference");

            migrationBuilder.CreateIndex(
                name: "ix_fhir_resources_resource_type",
                table: "fhir_resources",
                column: "resource_type");

            migrationBuilder.CreateIndex(
                name: "ix_fhir_resources_resource_type_fhir_id",
                table: "fhir_resources",
                columns: new[] { "resource_type", "fhir_id" });

            migrationBuilder.CreateIndex(
                name: "ix_fhir_resources_resource_type_status_last_updated",
                table: "fhir_resources",
                columns: new[] { "resource_type", "status", "last_updated" });

            migrationBuilder.CreateIndex(
                name: "ix_fhir_resources_search_parameters",
                table: "fhir_resources",
                column: "search_parameters")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "ix_fhir_resources_security_labels",
                table: "fhir_resources",
                column: "security_labels")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "ix_fhir_resources_status",
                table: "fhir_resources",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_fhir_resources_tags",
                table: "fhir_resources",
                column: "tags")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "ix_patient_accesses_access_level",
                table: "patient_accesses",
                column: "access_level");

            migrationBuilder.CreateIndex(
                name: "ix_patient_accesses_expires_at",
                table: "patient_accesses",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "ix_patient_accesses_granted_at",
                table: "patient_accesses",
                column: "granted_at");

            migrationBuilder.CreateIndex(
                name: "ix_patient_accesses_is_enabled",
                table: "patient_accesses",
                column: "is_enabled");

            migrationBuilder.CreateIndex(
                name: "ix_patient_accesses_patient_id",
                table: "patient_accesses",
                column: "patient_id");

            migrationBuilder.CreateIndex(
                name: "ix_patient_accesses_patient_id_is_emergency_access_is_enabled",
                table: "patient_accesses",
                columns: new[] { "patient_id", "is_emergency_access", "is_enabled" });

            migrationBuilder.CreateIndex(
                name: "ix_patient_accesses_patient_id_is_enabled_expires_at",
                table: "patient_accesses",
                columns: new[] { "patient_id", "is_enabled", "expires_at" });

            migrationBuilder.CreateIndex(
                name: "ix_patient_accesses_patient_id_user_id_access_level",
                table: "patient_accesses",
                columns: new[] { "patient_id", "user_id", "access_level" });

            migrationBuilder.CreateIndex(
                name: "ix_patient_accesses_user_id",
                table: "patient_accesses",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_patient_accesses_user_id_access_level_is_enabled",
                table: "patient_accesses",
                columns: new[] { "user_id", "access_level", "is_enabled" });

            migrationBuilder.CreateIndex(
                name: "ix_patient_consents_consent_type",
                table: "patient_consents",
                column: "consent_type");

            migrationBuilder.CreateIndex(
                name: "ix_patient_consents_consent_type_is_active_granted_at",
                table: "patient_consents",
                columns: new[] { "consent_type", "is_active", "granted_at" });

            migrationBuilder.CreateIndex(
                name: "ix_patient_consents_expires_at",
                table: "patient_consents",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "ix_patient_consents_granted_at",
                table: "patient_consents",
                column: "granted_at");

            migrationBuilder.CreateIndex(
                name: "ix_patient_consents_is_active",
                table: "patient_consents",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_patient_consents_is_electronic_consent",
                table: "patient_consents",
                column: "is_electronic_consent");

            migrationBuilder.CreateIndex(
                name: "ix_patient_consents_patient_id",
                table: "patient_consents",
                column: "patient_id");

            migrationBuilder.CreateIndex(
                name: "ix_patient_consents_patient_id_consent_type_is_active",
                table: "patient_consents",
                columns: new[] { "patient_id", "consent_type", "is_active" });

            migrationBuilder.CreateIndex(
                name: "ix_patient_consents_patient_id_is_active_expires_at",
                table: "patient_consents",
                columns: new[] { "patient_id", "is_active", "expires_at" });

            migrationBuilder.CreateIndex(
                name: "ix_patient_consents_revoked_at",
                table: "patient_consents",
                column: "revoked_at");

            migrationBuilder.CreateIndex(
                name: "ix_patients_date_of_birth",
                table: "patients",
                column: "date_of_birth");

            migrationBuilder.CreateIndex(
                name: "ix_patients_email",
                table: "patients",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "ix_patients_fhir_patient_id",
                table: "patients",
                column: "fhir_patient_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_patients_gender",
                table: "patients",
                column: "gender");

            migrationBuilder.CreateIndex(
                name: "ix_patients_last_name_first_name",
                table: "patients",
                columns: new[] { "last_name", "first_name" });

            migrationBuilder.CreateIndex(
                name: "ix_patients_patient_type",
                table: "patients",
                column: "patient_type");

            migrationBuilder.CreateIndex(
                name: "ix_patients_phone",
                table: "patients",
                column: "phone");

            migrationBuilder.CreateIndex(
                name: "ix_patients_status",
                table: "patients",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_patients_status_patient_type",
                table: "patients",
                columns: new[] { "status", "patient_type" });

            migrationBuilder.CreateIndex(
                name: "ix_todo_items_list_id",
                table: "todo_items",
                column: "list_id");

            migrationBuilder.CreateIndex(
                name: "ix_todo_items_list_id_done",
                table: "todo_items",
                columns: new[] { "list_id", "done" });

            migrationBuilder.CreateIndex(
                name: "ix_todo_items_list_id_priority",
                table: "todo_items",
                columns: new[] { "list_id", "priority" });

            migrationBuilder.CreateIndex(
                name: "ix_todo_items_reminder",
                table: "todo_items",
                column: "reminder");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "users",
                column: "normalized_email");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "users",
                column: "normalized_user_name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "fhir_resources");

            migrationBuilder.DropTable(
                name: "patient_accesses");

            migrationBuilder.DropTable(
                name: "patient_consents");

            migrationBuilder.DropTable(
                name: "todo_items");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "patients");

            migrationBuilder.DropTable(
                name: "todo_lists");
        }
    }
}
