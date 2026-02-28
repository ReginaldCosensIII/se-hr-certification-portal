# Deployment Guide (IIS)

This application is designed to be hosted on a Windows Server running Internet Information Services (IIS) within the local network.

## Prerequisites
- Windows Server with IIS enabled.
- .NET 10 Hosting Bundle (LTS) installed.
- PostgreSQL Server installed and accessible from the IIS server.
- URL Rewrite Module for IIS (recommended).

## Publishing the Application
1. Open a terminal in the project root.
2. Run the publish command:
   ```bash
   dotnet publish -c Release -o ./publish
   ```
3. Copy the contents of the `./publish` folder to the IIS server (e.g., `C:\inetpub\wwwroot\hr-portal`).

## IIS Configuration
1. Open **IIS Manager**.
2. Right-click **Sites** -> **Add Website**.
   - **Site name**: HRPortal
   - **Physical path**: `C:\inetpub\wwwroot\hr-portal`
   - **Port**: 80 (or a specific internal port/binding).
3. Ensure the Application Pool is set to **No Managed Code**.

## Environment Configuration & DevOps Strategy

### Connection Strings
`appsettings.json` contains a placeholder for production. Production credentials must be injected securely via **IIS Environment Variables** (`ConnectionStrings__DefaultConnection`). Local developers must create a git-ignored `appsettings.Development.json` for their local PostgreSQL credentials.

### Environment Variables
The `ASPNETCORE_ENVIRONMENT` variable dictates application behavior. It should be set to `Development` on local machines for debugging and `Production` (or `Staging`) in IIS.

### Database Seeding (`DbSeeder.cs`)
The application automatically seeds baseline dictionaries (Agencies and Certifications) on day 1 if tables are empty, regardless of environment.
**CRITICAL:** Mock data (fake employees and requests) is strictly gated by `if (_env.IsDevelopment())`. This mathematically guarantees the mock data generator will physically never execute in Production.

## Database Configuration
- Ensure the connection string in `appsettings.json` (or `appsettings.Production.json`) points to the production PostgreSQL instance.
- Run migrations against the production database using `dotnet ef database update` from the development machine (pointing to prod DB) or by generating a SQL script:
  ```bash
  dotnet ef migrations script -o deploy.sql
  ```
  And running `deploy.sql` on the production database.

## Client Access
- Users can access the portal via the server's hostname or IP address (e.g., `http://spe-server/hr-portal`).
- Create desktop shortcuts for users pointing to this URL.
