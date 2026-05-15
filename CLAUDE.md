# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

A .NET 8 web application template using Clean Architecture, CQRS with Mediator, ASP.NET Core MVC with Razor views, and Vue.js Islands for interactive UI components.

See `/docs/ai/WebProjectLayout.md` for detailed architecture documentation.

## Solution Structure

```
/src
  BW.Website.Domain          # Entities, value objects, domain services (no dependencies)
  BW.Website.Application     # CQRS commands/queries/handlers, DTOs, validators, interfaces
  BW.Website.Infrastructure  # EF Core, repositories, external services
  BW.Website.WebUI           # MVC controllers, Razor views, composition root

/frontend
  BW.Website.Frontend        # Vue 3, Tailwind CSS v4, Vite
```

**Dependency flow**: Domain ← Application ← Infrastructure ← WebUI

## Build Commands

```bash
# .NET
dotnet build
dotnet run --project src/BW.Website.WebUI

# Frontend (from /frontend/BW.Website.Frontend)
npm install
npm run dev          # Development with HMR
npm run build        # Production build to wwwroot/dist
npm run build:debug  # Debug build (no minification)
```

## Code Patterns

### CQRS + Mediator
- Features organized by folder: `/Application/Features/{Feature}/`
- Commands (write), Queries (read), Handlers, Validators, Models (DTOs)
- Controllers dispatch via `IMediator.Send()`
- Cross-cutting concerns via pipeline behaviors (`/Application/Common/Behaviors/`)

### Vue Islands
- Islands live in `/frontend/src/features/{Feature}/islands/`
- Mounted via `data-vue-island` attribute in Razor views
- Props passed via `data-vue-props` (JSON serialized)
- Lazy-loaded per island (each is a separate chunk)

### Repository Pattern
- Interfaces defined in `/Application/Interfaces/Repositories/`
- Implementations in `/Infrastructure/Repositories/`
- EF Core with Fluent API configurations

## Tech Stack

- **Backend**: C# 12, .NET 8, ASP.NET Core MVC, Entity Framework Core
- **Frontend**: Vue 3, Tailwind CSS v4, Vite, TypeScript
- **Patterns**: Clean Architecture, CQRS, Mediator (source-generated), FluentValidation

## Code Conventions

### C# Naming
- PascalCase for classes, methods, public members
- camelCase for local variables and private fields
- Prefix interfaces with "I" (e.g., `ISampleWorkOrderRepository`)

### Style
- Use `var` when type is obvious
- Prefer LINQ and lambda expressions
- Use `sealed` on classes not designed for inheritance
- Use `init` properties on commands/queries
