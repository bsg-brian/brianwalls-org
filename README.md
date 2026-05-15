# Introduction 
Web Project Layout with Clean Architecture, CQRS, Entity Framework Core, ASP.NET Core MVC, Vue.js, and Tailwind.


# WebUI Project

## Sample SQLite Database
A sample SQLite database file is created in the `BW.Website.WebUI` project folder as `BW.Website.db` upon first run.
If you wish to reset the database, simply delete this file and restart the application.

To remove, remove the following code block from program.cs:
```
// After line: var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();
}
```

## Error Handling
The WebUI project uses a custom error handling middleware located in the `BW.Website.WebUI.WebInfrastructure.Middleware` namespace.

### Error Test Endpoints
/Home/TestException
/Home/TestValidationException
/Home/TestMediatRValidation


# Front End Project

## Initialization / Setup
First time running project:

(in frontend project)
```
npm install
```

## Debugging
When debugging the WebUI project, start the frontend project seperately using:

(in frontend project)
```
npm run dev
```

This will allow start node and Vite's HMR (Hot Module Replacement) for instant changes 
to Tailwind and Vue when debugging.

## During production build or CI/CD
```
npm ci
npm run build
```

# Infrastructure Project

## Notes

Swap out Microsoft.EntityFrameworkCore.Sqlite with Microsoft.EntityFrameworkCore.SqlServer (if using SQL Server)

## EF Migrations
If you wish to use EF Migrations, you would run something similar to this:

From your solution folder, with Infrastructure as the startup project or specifying it:

```
dotnet ef migrations add InitialCreate -p BW.Website.Infrastructure -s BW.Website.WebUI
dotnet ef database update -p BW.Website.Infrastructure -s BW.Website.WebUI
```

You may also need to add the `Microsoft.EntityFrameworkCore.Design` NuGet package.