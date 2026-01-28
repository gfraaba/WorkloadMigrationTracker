# EF Migrations Applied — 2026-01-28

- Command run: `docker compose -f .devcontainer/docker-compose.yml exec dev bash -lc "cd /workspace && dotnet ef database update --project WebApi --startup-project WebApi --no-build"`
- Environment: dev container (platform: linux/arm64), database container: mcr.microsoft.com/mssql/server:2022-CU18-ubuntu-22.04
- Result: Migrations applied successfully to database `WorkloadMigration`.

Applied migrations (examples from run):
- 20250410035034_InitialCreate
- 20250411001248_UpdateResourceTypeIdToTypeId
- 20250411045758_UpdateResourceSchema
- 20250421185957_ConfigureResourcePropertyPrimaryKey
- 20250423022300_UpdateCascadeDelete
- 20250423115111_ConfigureWorkloadCascadeDelete

Key output excerpt:

    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250410035034_InitialCreate', N'10.0.2');
    Applying migration '20250411001248_UpdateResourceTypeIdToTypeId'.
    Applying migration '20250411045758_UpdateResourceSchema'.
    Applying migration '20250421185957_ConfigureResourcePropertyPrimaryKey'.
    Done.

Notes:
- The update was executed inside the `dev` container and targeted the `WebApi` project/startup.
- SQL Server 2022 was used for the local DB because SQL Server 2025 exhibits x86/AVX runtime requirements on Apple Silicon (see devcontainer DB investigation notes).
- If you want the full console log captured, I can append the complete output to this file or add a separate log file under `_docs/logs/`.
