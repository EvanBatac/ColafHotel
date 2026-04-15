# ColafHotel Reservation System

ASP.NET Core MVC hotel reservation system using .NET 8, Entity Framework Core, SQLite, Bootstrap 5, and session-backed authentication with manual account management.

## Default Seed Login

- Admin email: `admin@colafhotel.com`
- Admin password: `Admin@123`

## Required Packages

Run these in the terminal:

```bash
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.EntityFrameworkCore.Tools
```

## Setup and Run

Use these exact commands when creating a fresh project from the terminal:

```bash
# Create the project
dotnet new mvc -n ColafHotel
cd ColafHotel

# Install packages
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
dotnet add package Microsoft.EntityFrameworkCore.Design

# Create and apply migrations
dotnet ef migrations add InitialCreate
dotnet ef database update

# Run the project
dotnet run
```

## Uploaded Files

- Profile pictures are stored in `wwwroot/uploads/profiles/`
- Room images are stored in `wwwroot/uploads/rooms/`
