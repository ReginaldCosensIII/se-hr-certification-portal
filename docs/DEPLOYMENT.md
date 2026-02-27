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
