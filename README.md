# PersonalWebApi

## Technical Details

- **.NET Version**: .NET 9
- **C# Version**: 13.0
- **Packages**:
  - `FluentValidation.AspNetCore` - Version 11.3.0
  - `Microsoft.AspNetCore.Authentication.JwtBearer` - Version 9.0.0
  - `Microsoft.AspNetCore.OpenApi` - Version 9.0.0
  - `Microsoft.EntityFrameworkCore` - Version 9.0.0
  - `Microsoft.EntityFrameworkCore.Sqlite` - Version 9.0.0
  - `Microsoft.EntityFrameworkCore.Tools` - Version 9.0.0
  - `Newtonsoft.Json` - Version 13.0.3
  - `NLog.Extensions.Logging` - Version 5.3.15
  - `NLog.Web.AspNetCore` - Version 5.3.15
  - `Swashbuckle.AspNetCore` - Version 7.2.0

## Features

- **Logging**: Configured using NLog, settings in `appsettings.json`.
- **Admin Account Seed**:
  - Username/Password/Email as specified in `appsettings.json` under `UserSettings:Administrator`.
  - Verification token for password change in `appsettings.json` under `UserSettings:Administrator:ChangePasswordToken`.
- **User Roles Seed**: `Administrator` and `User` roles are seeded and immutable.
- **Authentication**: JWT tokens (SymmetricSecurityKey).
  - **Name**: Authorization (in Headers)
  - **Security Scheme**: Bearer (in Headers - `Bearer <token>`)
  - **Token Expiry**: Configurable in `appsettings.json`
  - **Signing Credentials**: HmacSha256
- **Password Storage**: Passwords are hashed using `Microsoft.AspNetCore.Identity.IPasswordHasher`.
- **Validation**: Implemented using FluentValidation.
- **API Documentation**: Swagger with documentation generation.
- **Exception Handling**: Custom exception handler middleware.
- **Authorization**:
  - Use custom attributes for functions `[Authorize]`, custom attribute - `[DynamicRoleAuthorize]`.
  - All roles assigned to functions can be found in `appsettings.json` under the `RolesConfig` section.

## Getting Started

1. **Configure Settings**: First, rename `appsettings_schema.json` to `appsettings.json`, then configure and familiarize yourself with the `appsettings.json` file.
2. **Database Migration**: Run migrations and create the database if it does not exist. By default, the SQLite database is set.
3. **Login**:
   - Send a POST request to `/api/system/account/login`.
   - **Body**:
      {
       "email": "",
       "password": ""
      }
4. **Retrieve JWT Token**: Obtain the Bearer JWT token and include it in the Headers of your HTTP requests.
5. **Token Expiry**: Check the token expiry duration in `appsettings.json`.

## Azure - App Services

Before deploying to Azure App Service, remove all directories and files except `launchSettings.json` from the Properties directory.

### Azure Application Insights

The project is configured to use Azure Application Insights. To use it, follow the steps below:

1. Rename `nlogsettings_azureinsightsapp_schema.json` to `nlogsettings_azureinsightsapp.json`.
2. Add the `instrumentationKey` and `EndpointAddress` from your insights application in the `nlogsettings_azureinsightsapp.json`.

## Template

A branch called `TemplateAPI-branch` is available, which serves as a template with the main API features without any additional functionality. After downloading the branch for your new API project, change the project name, solution name, namespaces, etc., and read the 'Getting Started' section from the branch `README.md`.
A brach called `TemaplateApiAzure-branch` is available, which serves as a template with the main API features without any additional functionality. After downloading the branch for your new API project, change the project name, solution name, namespaces, etc., and read the 'Getting Started' section from the branch `README.md`.
