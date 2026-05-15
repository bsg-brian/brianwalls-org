# Architecture for .NET - Web Project Template

This document describes the project architecture, layout and structure for the .NET Web Project Template.

## **Architecture Summary**

This solution follows a modern, modular architecture that cleanly separates concerns across backend and frontend components. The backend implements **Clean Architecture** principles with a dedicated **Domain layer** at the core, surrounded by an **Application layer** that exposes use cases via ports (interfaces). The **Infrastructure layer** provides data access, persistence, and external integrations through adapters that implement those ports. The **WebUI** project serves as the presentation layer, using ASP.NET Core 8 MVC/Razor for server-side rendering and application composition.

The Application layer adopts **CQRS (Command/Query Responsibility Segregation)** and **Mediator** (by martinothamar) to organize business operations into focused commands, queries, and handlers. Mediator is a source-generated library that provides the same patterns as MediatR with better performance and MIT licensing. This approach enforces high testability, clear separation of reads vs writes, predictable request pipelines, and support for cross-cutting behaviors such as validation, logging, caching, and authorization.

On the frontend, the solution uses a **Vue 3 + Tailwind CSS** setup, powered by **Vite** for fast bundling, HMR, and production builds. All frontend source code and Node tooling are isolated within a dedicated `SampleApp.Frontend` project, ensuring the C# projects remain clean and free of Node dependencies. Vite outputs optimized JavaScript and CSS into the WebUI project's `wwwroot/dist` folder. Vue components are rendered using a lightweight "**Vue islands**" approach, where Razor views host Vue components and pass initial data via serialized props.

This architecture provides a scalable, maintainable template for modern .NET applications—combining clean backend boundaries with a flexible, high-performance frontend system.


## Solution Root Folder Structure

```
/src                           
  /SampleApp.Domain            # Domain project
  /SampleApp.Application       # Application project
  /SampleApp.Infrastructure    # Infrastructure project
  /SampleApp.WebUI             # ASP.NET Core MVC/Razor web project
  
/frontend
  /SampleApp.Frontend          # Front-end Javascript and CSS project (Vue, Tailwind, Vite, Node)

```

## Solution layout

**Projects:**

- `SampleApp.Domain` – core business model and rules: entities, value objects, domain events, domain services
    
- `SampleApp.Application` – use cases, services, ports (interfaces), DTOs, mapping; orchestrates the Domain layer
    
- `SampleApp.Infrastructure` – EF/Reverse POCO, repositories, external services, AutoMapper profiles that touch persistence; adapters implementing Application ports
    
- `SampleApp.WebUI` – ASP.NET Core 8 MVC (Tailwind + optional Vue); presentation layer and composition root
  
- `SampleApp.Frontend` – Project that surfaces `/frontend` assets in Solution Explorer
    

## Project References

- `SampleApp.Domain` ➜ **no project references** (innermost core)
    
- `SampleApp.Application` ➜ references **Domain**
    
- `SampleApp.Infrastructure` ➜ references **Application** and **Domain**
    
- `SampleApp.WebUI` ➜ references **Application** and **Infrastructure** (optionally **Domain** for enums/constants if needed)
    

`Domain` is the true core of the system; `Application` orchestrates use cases on top of it without knowing about Infrastructure or WebUI.

## Solution Error Handling Overview

Error handling in this architecture follows a layered approach:

- **Domain**  
  Enforces invariants and business rules. Domain code may:
  - Throw domain-specific exceptions when invariants are violated, or
  - Return failure results (e.g., `Result<T, Error>`) for expected business rule failures.
  Domain never concerns itself with HTTP status codes, logging sinks, or UI messaging.

- **Application (CQRS + Mediator)**  
  Coordinates use cases and is responsible for:
  - Translating Domain failures into application-level results (e.g., `Result<T, Error>`).
  - Running validation (FluentValidation) via pipeline behaviors.
  - Deciding whether a failure is an expected business error vs an unexpected system error.
  Handlers typically return a `Result<T>` or `Result<Unit>` that encapsulates success or failure, including a structured `Error` object.

- **Infrastructure**  
  Deals with IO failures (database, HTTP, queues, file system). Responsibilities:
  - Throw exceptions for technical failures (timeouts, connection errors, serialization issues).
  - Optionally wrap low-level exceptions into more meaningful application exceptions.
  - Never return UI-specific messages; it just signals failure upward.

- **WebUI**  
  Responsible for:
  - Mapping `Result<T, Error>` from Application to user-facing responses (views, redirects, validation messages).
  - Handling unexpected exceptions via global exception handling (middleware or filters).
  - Displaying friendly error pages (e.g., `Error.cshtml`) for unhandled errors.
  - Surfacing validation and business rule errors in a user-friendly way (e.g., ModelState, validation summary).

---
## Domain Project 
### Domain Folder Structure

```
Domain

  /Entities        <-- Core business entities (Order, Customer, etc.)
  /ValueObjects    <-- Immutable value objects (Money, Address, DateRange, etc.)
  /Enums           <-- Domain-specific enums and statuses
  /Events          <-- Domain events (OrderCreatedEvent, etc.)
  /Services        <-- Domain services with pure business logic (no IO or infrastructure)
  /Common          <-- Base types, interfaces, shared domain abstractions

```

### Domain Notes

- Domain contains **pure business logic** and should not depend on Application, Infrastructure, or WebUI.
    
- Domain types (entities, value objects, events) model your core business language.
    
- Domain services encapsulate business rules that don't naturally fit on a single entity.
    
- Application, Infrastructure, and WebUI **all depend on** Domain, never the other way around.
    
---
## Application Project
### Application Folder Structure

```
Application

  /Interfaces
    /Services        <-- Interfaces for application-level services (use cases) | **Ports (inbound)**
    /Repositories    <-- Contracts for data access (IUnitOfWork, repositories) | **Ports (outbound)**

  /Services          <-- Cross-feature or non-CQRS services (shared logic)

  /Features          <-- Recommended structure for CQRS; feature-focused folders (Models, Commands, Queries, Handlers, Validators) (business logic), depend only on Ports

  /DTOs              <-- (Data Transfer Objects are typically stored with their feature not here).
    /Shared          <-- Reusable DTOs shared across multiple features/services are stored here.

  /Mapping           <-- AutoMapper profiles ONLY for Application types (Domain <-> DTOs)

  /Common
    /Exceptions      <-- Custom exceptions for domain/application logic
    /Behaviors       <-- Pipeline behaviors (validation, logging, caching, etc.)
    /Results         <-- Result<T> and Error types for railway-oriented error handling
    ApplicationMediator.cs  <-- Mediator source generator marker class

```

### Application Notes

- **Interfaces live here** so Application defines what it needs from Infrastructure and other outer layers.
    
- Application references **Domain** and uses Domain entities/value objects in its use cases.
    
- Application NEVER references Infrastructure or WebUI.
    
- Services orchestrate use cases and depend on interfaces (Ports), not concrete repositories or external service implementations.
    
- Mapping here should NOT reference EF entities or any persistence concerns; it should typically translate between **Domain models** and **DTOs**.
    
- DTOs are co-located with their feature (/Features/Orders/OrderDto.cs). `/DTOs/Shared` is used for cross-feature types.
  
- Use `/Common/Behaviors` for Mediator pipeline behaviors (validation, logging, performance, etc.) if using Mediator.

### Mediator Source Generator Setup

This solution uses **Mediator** (by martinothamar), a source-generated alternative to MediatR. It provides the same patterns with better performance and MIT licensing.

#### Required Packages (Application project)

```xml
<PackageReference Include="Mediator.Abstractions" Version="3.0.1" />
<PackageReference Include="Mediator.SourceGenerator" Version="3.0.1"
    OutputItemType="Analyzer"
    ReferenceOutputAssembly="false"
    PrivateAssets="all" />
```

> **Note:** `PrivateAssets="all"` prevents the source generator from flowing to dependent projects (WebUI), avoiding CS0436 duplicate type warnings. The source generator should only run in the Application project where handlers are defined.

#### Source Generator Marker Class

Create a partial class for the source generator to emit into:

```csharp
// /Application/Common/ApplicationMediator.cs
using Mediator;

namespace SampleApp.Application.Common;

[Mediator]
public partial class ApplicationMediator { }
```

The source generator analyzes your code at compile time and generates the dispatch logic as actual C# code. You can view the generated code in Visual Studio under Dependencies → Analyzers → Mediator.SourceGenerator.

#### DI Registration (WebUI CompositionRoot)

```csharp
// Register Mediator (generated implementation)
services.AddMediator(options =>
{
    options.ServiceLifetime = ServiceLifetime.Scoped;
});

// Register pipeline behaviors in order of execution
services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
```

**Note:** Pipeline behaviors should be registered as `Scoped` (not `Singleton`) because they typically inject scoped services like validators, DbContext, or IHttpContextAccessor.
    
### Error Handling - Result<T, Error> Pattern

The Application layer uses a `Result<T, Error>` pattern to represent the outcome of commands and queries. This pattern lives in `/Common/Results/`.

#### Error Type

```csharp
public sealed record Error
{
    public string Code { get; }                                      // Machine-friendly (e.g., "SampleWorkOrder.NotFound")
    public string Message { get; }                                   // Human-readable description
    public IReadOnlyDictionary<string, string[]>? ValidationErrors { get; init; }  // Field-level errors

    public static Error Create(string code, string message) => new(code, message);
    public static Error NotFound(string entityName, object id) => ...;
    public static Error Validation(string message, IReadOnlyDictionary<string, string[]>? errors = null) => ...;
    public static Error Conflict(string message) => ...;
    public static Error Forbidden(string message) => ...;
}
```

#### Result Types

```csharp
// For operations that don't return a value (e.g., Delete)
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    public static Result Success() => ...;
    public static Result Failure(Error error) => ...;
    public static Result<T> Success<T>(T value) => ...;
    public static Result<T> Failure<T>(Error error) => ...;
    public static Result NotFound(string entityName, object id) => ...;
}

// For operations that return a value
public class Result<T> : Result
{
    public T Value { get; }  // Throws if IsFailure

    public static implicit operator Result<T>(T value) => Success(value);
    public static new Result<T> NotFound(string entityName, object id) => ...;
}
```

#### Usage in Commands/Queries

Commands and queries declare `Result<T>` as their response type. Mediator uses `ICommand<T>` for write operations and `IQuery<T>` for read operations:

```csharp
// Command - modifies state
public sealed record CreateSampleWorkOrderCommand : ICommand<Result<SampleWorkOrderDto>>
{
    public string WorkOrderNumber { get; init; } = string.Empty;
    public string CustomerName { get; init; } = string.Empty;
    // ...
}

// Command - no return value
public sealed record DeleteSampleWorkOrderCommand(int Id) : ICommand<Result>;

// Query - retrieves data
public sealed record GetSampleWorkOrderByIdQuery(int Id) : IQuery<Result<SampleWorkOrderDto>>;

// Query - retrieves collection
public sealed record GetSampleWorkOrdersQuery : IQuery<Result<IReadOnlyList<SampleWorkOrderDto>>>;
```

#### Usage in Handlers

Handlers implement `ICommandHandler<TCommand, TResponse>` or `IQueryHandler<TQuery, TResponse>` and return `ValueTask<T>`:

```csharp
public sealed class UpdateSampleWorkOrderCommandHandler 
    : ICommandHandler<UpdateSampleWorkOrderCommand, Result<SampleWorkOrderDto>>
{
    private readonly ISampleWorkOrderRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateSampleWorkOrderCommandHandler(
        ISampleWorkOrderRepository repository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async ValueTask<Result<SampleWorkOrderDto>> Handle(
        UpdateSampleWorkOrderCommand command,
        CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(command.Id, cancellationToken);

        if (entity is null)
        {
            return Result<SampleWorkOrderDto>.NotFound("SampleWorkOrder", command.Id);
        }

        // Update entity...
        await _repository.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = _mapper.Map<SampleWorkOrderDto>(entity);
        return Result.Success(dto);
    }
}
```

Query handler example:

```csharp
public sealed class GetSampleWorkOrderByIdQueryHandler 
    : IQueryHandler<GetSampleWorkOrderByIdQuery, Result<SampleWorkOrderDto>>
{
    private readonly ISampleWorkOrderRepository _repository;
    private readonly IMapper _mapper;

    public GetSampleWorkOrderByIdQueryHandler(
        ISampleWorkOrderRepository repository,
        IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async ValueTask<Result<SampleWorkOrderDto>> Handle(
        GetSampleWorkOrderByIdQuery query,
        CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(query.Id, cancellationToken);

        if (entity is null)
        {
            return Result<SampleWorkOrderDto>.NotFound("SampleWorkOrder", query.Id);
        }

        var dto = _mapper.Map<SampleWorkOrderDto>(entity);
        return Result.Success(dto);
    }
}
```

#### Error Flow Summary

- **Success path**: Handlers return `Result.Success(value)` where `IsSuccess = true`.
- **Expected failures**: Handlers return `Result.Failure(error)` or `Result<T>.NotFound(...)` for business rule violations.
- **Unexpected failures**: Technical exceptions (database down, etc.) propagate as exceptions and are handled by global middleware.
- **Validation failures**: The `ValidationBehavior` automatically converts FluentValidation errors into `Result.Failure` with field-level error details.

### CQRS & Mediator Usage (Recommended for Application Layer)

The Application layer is an ideal location for **CQRS (Command Query Responsibility Segregation)** patterns and **Mediator** request/response messaging. These patterns help enforce separation of concerns, maintainability, and testability across your clean architecture.

### CQRS Structure

A typical structure inside `/Application` when using CQRS might be:

```
/Application
  /Features                      <-- Grouped by business capability or domain area
    /Orders                      <-- Example feature folder
      /Queries                   <-- Query types (read-only operations)
      /Commands                  <-- Command types (write/update operations)
      /Handlers                  <-- Handlers for commands/queries using Mediator
      /Validators                <-- FluentValidation classes for commands/queries

```

### CQRS Concepts

- **Commands** modify system state (CreateOrder, UpdateOrder, CancelOrder).
    
- **Queries** retrieve information (GetOrderById, GetOrdersForCustomer).
    
- Commands and queries are **separate classes**, promoting clarity and focused operations.
    
- Each command/query has a dedicated **handler**, which orchestrates logic using:
    
    - Domain entities & domain services
        
    - Repositories from Infrastructure (via Application ports/interfaces)
        
    - Application-level DTOs
        

### Mediator Integration

Mediator provides a clean way to:

- Dispatch commands and queries via `IMediator`.
    
- Apply **pipeline behaviors** (logging, validation, caching, authorization).
    
- Keep controllers thin by delegating all business logic to Application.

#### Mediator Interfaces Summary

| Interface | Purpose | Example |
|-----------|---------|---------|
| `ICommand<TResponse>` | Write operations that modify state | `CreateOrderCommand : ICommand<Result<OrderDto>>` |
| `IQuery<TResponse>` | Read operations that retrieve data | `GetOrderByIdQuery : IQuery<Result<OrderDto>>` |
| `ICommandHandler<TCommand, TResponse>` | Handles a command | `CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, Result<OrderDto>>` |
| `IQueryHandler<TQuery, TResponse>` | Handles a query | `GetOrderByIdQueryHandler : IQueryHandler<GetOrderByIdQuery, Result<OrderDto>>` |
| `IPipelineBehavior<TMessage, TResponse>` | Cross-cutting concerns | `ValidationBehavior<TMessage, TResponse>` |
    

Example controller usage with Result handling:

```csharp
public class OrdersController : Controller
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var result = await _mediator.Send(new GetOrderByIdQuery(id));

        if (result.IsFailure)
        {
            if (result.Error.Code.EndsWith(".NotFound"))
                return NotFound();

            return StatusCode(500);
        }

        return View(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateOrderCommand command)
    {
        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            AddErrorsToModelState(result.Error);
            return View(vm);
        }

        return RedirectToAction(nameof(Details), new { id = result.Value.Id });
    }

    private void AddErrorsToModelState(Error error)
    {
        if (error.ValidationErrors is not null)
        {
            foreach (var (propertyName, messages) in error.ValidationErrors)
            {
                foreach (var message in messages)
                {
                    ModelState.AddModelError(propertyName, message);
                }
            }
        }
        else
        {
            ModelState.AddModelError(string.Empty, error.Message);
        }
    }
}
```

### Pipeline Behaviors

CQRS + Mediator shines when using behaviors to enforce cross-cutting policies:

- **ValidationBehavior** – runs FluentValidation validators automatically

- **LoggingBehavior** – logs request execution time and results

- **CachingBehavior** – caches query results for read operations

- **AuthorizationBehavior** – checks permissions before executing a handler


These behaviors are defined in `/Application/Common/Behaviors`.

#### ValidationBehavior and Result Integration

The `ValidationBehavior` is Result-aware. When validation fails and the response type is `Result` or `Result<T>`, it returns a failure result instead of throwing an exception:

```csharp
public sealed class ValidationBehavior<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    private readonly IEnumerable<IValidator<TMessage>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TMessage>> validators)
    {
        _validators = validators;
    }

    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next(message, cancellationToken);
        }

        var context = new ValidationContext<TMessage>(message);
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count != 0)
        {
            // For Result types: return Result.Failure with validation errors
            // For non-Result types: throw ValidationException (backwards compatibility)
            return CreateValidationFailureResult(failures);
        }

        return await next(message, cancellationToken);
    }

    private static TResponse CreateValidationFailureResult(List<ValidationFailure> failures)
    {
        // Implementation that checks if TResponse is Result or Result<T>
        // and returns appropriate failure, or throws ValidationException
        ...
    }
}
```

This means:
- Validation errors flow through the Result pattern, not exceptions
- Controllers receive `Result.IsFailure = true` with `Error.ValidationErrors` populated
- Field-level errors are grouped by property name for easy ModelState mapping
- No try/catch needed in controllers for validation failures

### Why CQRS Fits the Application Layer

- Enforces **separation between reads and writes**
    
- Simplifies **unit testing** handlers (each is focused and isolated)
    
- Keeps **Domain logic pure** and avoids leaking infrastructure concerns
    
- Encourages **scalable feature folders**, making the structure intuitive for large systems
    
- Works seamlessly with **clean architecture** boundaries
    

Controllers in WebUI should:

- Receive an HTTP request
    
- Convert it to a Command or Query
    
- Send it through Mediator to the Application layer
    
- Render a view or return an API result
    

Infrastructure fulfills Application interfaces, and Domain remains the innermost pure logic layer.

---
## Infrastructure Project
### Infrastructure Folder Structure

```
Infrastructure

  /Data
    /Contexts        <-- EF Core DbContext(s)
    /Migrations      <-- EF migrations (if used)
    EfUnitOfWork.cs  <-- Unit of Work implementation

  /Repositories      <-- Implementations of Application repository interfaces using Domain entities | **Adapters**

  /Configurations    <-- EF entity configurations (Fluent API)

  /Mapping           <-- AutoMapper profiles touching persistence models and mapping to/from Domain

  /ExternalServices  <-- Email, HTTP clients, queues, S3/Blob, etc.

  /Utilities         <-- Helpers for persistence, file IO, encryption, etc.

```

### Infrastructure Notes

- Infrastructure **implements** interfaces (Ports) defined in Application.
    
- Contains ALL technology-specific concerns (EF, Redis, HTTP clients, queues, blob storage, etc.).
    
- SHOULD NOT contain any business logic.
    
- AutoMapper profiles here convert between **persistence models** and **Domain models**; Application handles Domain ↔ DTO mappings.
    
- `/ExternalServices` is where integrations with outside systems live.

- `/Utilities` is for low-level helpers used internally but NOT exposed to Application or WebUI.

### Unit of Work Pattern

The solution uses an explicit Unit of Work pattern to control persistence boundaries:

**Interface (Application Layer):**

```csharp
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken ct = default);
}
```

**Implementation (Infrastructure Layer):**

`EfUnitOfWork` wraps `ApplicationDbContext` and implements `IUnitOfWork`.

**Key Principles:**

- **Repositories do NOT call SaveChangesAsync** - they only modify the EF change tracker
- **Command handlers call SaveChangesAsync explicitly** after all repository operations
- **Handlers control the transaction boundary** - they decide when changes are committed
- **For Create operations**, call `SaveChangesAsync` before mapping to DTO (to populate DB-generated Id)

**Handler Example:**

```csharp
public sealed class CreateSampleWorkOrderCommandHandler 
    : ICommandHandler<CreateSampleWorkOrderCommand, Result<SampleWorkOrderDto>>
{
    private readonly ISampleWorkOrderRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateSampleWorkOrderCommandHandler(
        ISampleWorkOrderRepository repository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async ValueTask<Result<SampleWorkOrderDto>> Handle(
        CreateSampleWorkOrderCommand command,
        CancellationToken cancellationToken)
    {
        var entity = new SampleWorkOrder { /* ... */ };

        await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);  // Explicit save

        var dto = _mapper.Map<SampleWorkOrderDto>(entity);      // Id now populated
        return Result.Success(dto);
    }
}
```

**When to use `ExecuteInTransactionAsync`:**

- Multiple `SaveChangesAsync` calls that must be atomic
- Explicit isolation level control
- Mixing EF operations with raw SQL in one transaction

For most single-operation commands, just `SaveChangesAsync()` is sufficient (EF wraps it in a transaction automatically).

---
## WebUI Project
### WebUI Folder Structure

```
WebUI

/Controllers        <-- MVC controllers (UI endpoints calling Application services)

/Views              <-- MVC views
  /Shared           <-- Shared views and layouts
    /Components     <-- Razor ViewComponent views (server-side components)

/Models             <-- MVC view models

/WebInfrastructure  <-- UI-specific infrastructure (filters, middleware, helpers)
  /Authentication
  /Authorization
  /CompositionRoot  <-- Dependency injection
  /Extensions
  /Logging
  /Middleware       <-- Global HTTP Pipeline components
  /TagHelpers       <-- Custom Razor TagHelpers for reusable UI markup

/wwwroot            <-- Static web root served by ASP.NET
  /css              <-- Optional: legacy or non-Vite-managed CSS for the app
  /dist             <-- Vite build output (bundled JS/CSS from /frontend)
  /images
    /icons          
  /js               <-- Optional: legacy or non-Vite-managed JavaScript for the app
  /lib              <-- UI client libraries

```

### WebUI Notes

- `/SampleApp.Frontend` contains **authoritative** frontend source assets (Tailwind, Vue, JS) and all Node/Vite/Tailwind configurations. These assets are built into `/src/SampleApp.WebUI/wwwroot/dist`.
    
- `/wwwroot` holds **public, compiled, static assets** (CSS, JS bundles, libraries, images). These files are served directly by ASP.NET Core. 
    
- `/WebInfrastructure` contains **UI-specific plumbing** such as:
    
    - Authentication & authorization helpers specific to WebUI
        
    - Middleware (request logging, security headers, culture, etc.)
        
    - TagHelpers, Filters, Extensions
        
    - Dependency injection wiring (`CompositionRoot`)
        
    - Web-only logging helpers (not core logging configuration)
        
- `/Views/Shared/Components` contains Razor views for **ViewComponents**, which are reusable UI blocks with server-side logic.
    
- `/Models` contains **view models**, not domain models and not DTOs from the Application layer. These models exist purely to support UI rendering.
    
- Controllers should use Application services and DTOs, but never reference Infrastructure directly. Only WebInfrastructure handles wiring to Infrastructure.
    

### WebUI Error Handling

Controllers receive `Result<T>` from the Application layer and must inspect the result before accessing the value.

#### Handling Result in Controllers

```csharp
[HttpPost]
public async Task<IActionResult> Edit(SampleWorkOrderEditVm vm, CancellationToken cancellationToken)
{
    if (!ModelState.IsValid)
        return View(vm);

    var result = await _mediator.Send(new UpdateSampleWorkOrderCommand
    {
        Id = vm.Id,
        WorkOrderNumber = vm.WorkOrderNumber,
        // ...
    }, cancellationToken);

    if (result.IsFailure)
    {
        // Handle not-found errors
        if (result.Error.Code.EndsWith(".NotFound"))
            return NotFound();

        // Handle validation/business errors - add to ModelState
        AddErrorsToModelState(result.Error);
        return View(vm);
    }

    return RedirectToAction(nameof(Details), new { id = result.Value.Id });
}
```

#### Error Handling Patterns by Error Type

| Error Code Pattern | Controller Response |
|--------------------|---------------------|
| `*.NotFound` | `return NotFound();` |
| `Validation.Failed` | Add to ModelState, re-render view |
| `Forbidden` | `return Forbid();` |
| `Conflict` | Add to ModelState or return `Conflict()` |
| Other/Unknown | `return StatusCode(500);` or add to ModelState |

#### JSON/API Endpoints

For Vue islands or API consumers, return structured error responses:

```csharp
[HttpGet]
public async Task<IActionResult> ListJson(CancellationToken cancellationToken)
{
    var result = await _mediator.Send(new GetSampleWorkOrdersQuery(), cancellationToken);

    if (result.IsFailure)
    {
        return StatusCode(500, new { error = result.Error.Message });
    }

    return Json(result.Value);
}
```

#### Global Exception Handling

For truly unexpected exceptions (not covered by Result), configure global middleware:
- `UseExceptionHandler` for production error pages
- Environment-specific behavior (detailed in Development, generic in Production)
- Optional ProblemDetails for API endpoints

---
## Frontend Project

(Vue Islands + Vite + Tailwind)

The frontend uses a **minimal Node footprint** architecture, with all Node/Vite/Tailwind/Vue assets isolated under `/SampleApp.Frontend`.  
The ASP.NET WebUI project consumes **only the compiled output** under `wwwroot/dist`.

The frontend is **not a full SPA by default**. Instead, it uses a **Vue Islands** approach to progressively enhance MVC/Razor views with small, feature-focused interactive components.


## Frontend Folder Structure

```
Frontend

  /src
    /features                     <-- Feature-based frontend code (vertical slices)
      /SampleWorkOrders
        /islands                  <-- Mountable Vue entry components for this feature
          SampleWorkOrders.vue
        /components               <-- Feature-scoped Vue UI components
          StatusPill.vue
        /composables              <-- Feature-scoped Vue composables (useXxx hooks)
          useSampleWorkOrders.ts
        /types                    <-- Feature-specific TypeScript DTOs/contracts
          sampleWorkOrders.ts

    /components                   <-- Shared Vue UI components (cross-feature)
    /composables                  <-- Shared Vue composables (cross-feature)
    /types                        <-- Shared TypeScript types (cross-feature)
    /lib                          <-- Plain TS helpers (fetch wrappers, formatters, validators)
    /css                          <-- Global/shared CSS (Tailwind entry, tokens, components, forms)
    /assets                       <-- Images, icons, fonts (imported by Vite)

    /islands
      index.ts                    <-- Vue island auto-discovery & lazy-mount logic

    main.ts                       <-- Frontend bootstrap:
                                   - imports global CSS
                                   - mounts optional SPA root (if present)
                                   - mounts Vue islands

  /node_modules                   <-- Node dependencies (generated; not checked in)

  # package.json                  <-- Front-end dependencies and scripts
  # vite.config.*                 <-- Vite configuration (outputs to WebUI wwwroot/dist)
  # tailwind.config.*             <-- Tailwind configuration (Vue + cshtml content scanning)
  # postcss.config.*              <-- Only if needed; Tailwind v4 often does not require this

```

### Organization Notes

- **Feature folders (`/src/features/<Feature>`)** are the primary unit of organization
    
- **Shared folders** (`/components`, `/types`, `/composables`) remain available for cross-feature reuse
    
- Vue Islands live **inside feature folders**, reinforcing vertical-slice ownership
    


## Frontend Notes

### Project Type (Dummy Organizational Project)

A .NET class library project is used **only** to surface frontend files inside Visual Studio.  
It has **no runtime or build output** and exists solely for developer convenience.

### Sample `.csproj`

```
<Project Sdk="Microsoft.NET.Sdk">
  <!--
    Dummy organizational project for Visual Studio.
    Surfaces frontend files in Solution Explorer without any build output.
  -->

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <IsPackable>false</IsPackable>
    <GenerateAssemblyInfo>false</GenerateAssemblyInfo>
  </PropertyGroup>

  <ItemGroup>
    <!-- Source files -->
    <None Include="src\**\*.*" />

    <!-- Config files -->
    <None Include="package.json" />
    <None Include="package-lock.json" Condition="Exists('package-lock.json')" />
    <None Include="tsconfig.json" />
    <None Include="tsconfig.app.json" />
    <None Include="tsconfig.node.json" />
    <None Include="vite.config.ts" />
    <None Include="index.html" />
  </ItemGroup>

  <!-- Exclude node_modules and build output -->
  <ItemGroup>
    <None Remove="node_modules\**" />
    <None Remove="dist\**" />
  </ItemGroup>

  <Target Name="SkipBuild" BeforeTargets="Build">
    <Message Text="SampleApp.Frontend is a container project for frontend assets only; no build step." Importance="low" />
  </Target>

</Project>

```


## Vue Islands in Razor

Vue is used via an **islands pattern**, where each island is:

- A self-contained Vue single-file component
    
- Mounted into a specific Razor view location
    
- Responsible for its own data fetching and UI logic
    
- Lazy-loaded only when present on the page
    

### Island Registration & Mounting

- Vue islands are **auto-discovered at build time** using `import.meta.glob`
    
- Each island is emitted as a **separate JavaScript chunk**
    
- At runtime, a small bootstrap script scans the DOM for island mount points and loads only the required chunks
    

### Razor Usage

Razor views declare Vue islands using simple HTML attributes:

```
<div   data-vue-island="sample-work-orders"   data-vue-props='{"listUrl": "/SampleWorkOrders/ListJson"}'> </div>
```

```
<div
  data-vue-island="sample-work-orders"
  data-vue-props='{
    "listUrl": "@Url.Action("ListJson", "SampleWorkOrders")",
    "detailsBaseUrl": "@Url.Action("Details", "SampleWorkOrders")"
  }'>
</div>
```

```
@{
    var islandProps = new
    {
        listUrl = Url.Action("ListJson", "SampleWorkOrders"),
        detailsBaseUrl = Url.Action("Details", "SampleWorkOrders"),
        pageSize = 25,
        enableSearch = true
    };

    var islandPropsJson =
        System.Text.Json.JsonSerializer.Serialize(islandProps);
}

<div
  data-vue-island="sample-work-orders"
  data-vue-props='@Html.Raw(islandPropsJson)'>
</div>
```

- `data-vue-island` identifies the island to mount
    
- `data-vue-props` contains JSON-serialized props passed into the Vue component
    
**Guideline:**
- Use hard-coded props for static behavior
- Use `Url.Action` for routing safety
- Use serialized objects for complex or conditional configuration

No Razor HTML helpers are required.


## Data Flow: Controller ➜ Razor ➜ Vue

1. **Controller** prepares any required URLs or configuration values.
    
2. **Razor view** emits a `<div>` with `data-vue-island` and optional `data-vue-props`.
    
3. **Frontend bootstrap (`main.ts`)** scans for island mount points.
    
4. The island registry lazy-loads the matching Vue component.
    
5. Props are passed directly into the Vue component via `defineProps`.
    
6. The Vue island manages its own reactive state and performs API calls as needed.
    

## JavaScript Bundling Strategy

- `main.js` is a small, stable bootstrap file and may be loaded globally
    
- Each Vue island is emitted as a **separate lazy-loaded chunk**
    
- Island chunks are downloaded **only** on pages where the corresponding island exists
    
- Pages without islands incur minimal JavaScript overhead
    

## TypeScript Types

TypeScript DTOs and contracts are stored:

- **Per feature:** `/src/features/<Feature>/types`
    
- **Shared:** `/src/types`
    

These types often mirror Application-layer DTOs and are shared across islands, components, and composables.


## Tailwind & CSS Handling

- Tailwind CSS v4 is configured under `/frontend`
    
- Tailwind scans:
    
    - Razor views (`../src/SampleApp.WebUI/Views/**/*.cshtml`)
        
    - Vue components and frontend source files
        
- Tailwind configuration is referenced via `@config` inside the main CSS entry file
    
- Global CSS files live under `/src/css` and are imported by `main.ts`
    
- Vite outputs optimized CSS bundles to `wwwroot/dist/assets`, which are linked from Razor layouts
    
## Vite Config (vite.config.ts)
```
import { defineConfig } from "vite";
import vue from "@vitejs/plugin-vue";
import tailwindcss from "@tailwindcss/vite";
import { resolve } from "path";

export default defineConfig(({ mode }) => {
    const isDebug = mode === "debug";

    return {
        plugins: [
            vue(),
            tailwindcss(),
        ],
        resolve: {
            alias: {
                "@": resolve(__dirname, "./src"),
            },
        },
        build: {
            // Output to the MVC project's wwwroot/dist
            outDir: resolve(__dirname, "../../src/SampleApp.WebUI/wwwroot/dist"),
            emptyOutDir: true,
            minify: isDebug ? false : true,
            sourcemap: isDebug,

            rollupOptions: {
                input: resolve(__dirname, "src/main.ts"),
                output: {
                    entryFileNames: "main.js",

                    // Hash chunks for cache busting (main.js references these)
                    chunkFileNames: isDebug
                        ? "chunks/[name].js"
                        : "chunks/[name].[hash].js",

                    assetFileNames: (assetInfo) => {
                        if (assetInfo.name?.endsWith(".css")) {
                            return "assets/style.css";
                        }
                        if (/\.(png|jpe?g|gif|svg|ico|webp)$/.test(assetInfo.name ?? "")) {
                            return "assets/[name][extname]";
                        }
                        return "assets/[name][extname]";
                    },
                },
            },
        },
    };
});

```

## Git Ignore Additions (.gitignore)
```
# Claude Code
CLAUDE.local.md
.claude/settings.local.json
.claude/**/*.local.md

# Certificates (override global *.pfx ignore)
!certs/**/*.pfx

# WebUI
## Ignore everything in dist
**/wwwroot/dist/**
## Keep the dist folder itself
!**/wwwroot/dist/
## Keep its .gitkeep file
!**/wwwroot/dist/.gitkeep

## Ignore everything in dist/assets
**/wwwroot/dist/assets/**
## Keep the assets folder itself
!**/wwwroot/dist/assets/
## Keep its .gitkeep file
!**/wwwroot/dist/assets/.gitkeep

# Frontend (Node/Vite)
## Environment files
.env
.env.local
.env.*.local

## Transpiled config (source is vite.config.ts)
**/vite.config.js
```

## Package.json Example
(versions may differ in your project)

```
{
  "name": "sampleapp-frontend",
  "private": true,
  "version": "0.0.0",
  "type": "module",
  "scripts": {
    "dev": "vite build --watch --mode debug",
    "build": "vue-tsc -b && vite build",
    "build:debug": "vue-tsc -b && vite build --mode debug",
    "preview": "vite preview"
  },
  "dependencies": {
    "@tailwindcss/vite": "^4.1.17",
    "vue": "^3.5.24"
  },
  "devDependencies": {
    "@types/node": "^24.10.1",
    "@vitejs/plugin-vue": "^6.0.1",
    "@vue/tsconfig": "^0.8.1",
    "autoprefixer": "^10.4.22",
    "postcss": "^8.5.6",
    "tailwindcss": "^4.1.17",
    "typescript": "~5.9.3",
    "vite": "^7.2.4",
    "vue-tsc": "^3.1.4"
  }
}
```

## Build Integration

### Development

- Run `npm install` once in `/frontend`
    
- Run `npm run dev` for frontend development and HMR
    
- ASP.NET Core continues to serve Razor views
    
- Vite handles bundling and hot reload for frontend assets
    

### Publish / CI

- CI or MSBuild runs `npm run build` in `/frontend`
    
- Vite emits production-ready assets to `wwwroot/dist`
    
- ASP.NET Core serves the compiled static assets at runtime
    

### Summary

This frontend architecture:

- Keeps Node/Vite concerns fully isolated
    
- Aligns frontend structure with Clean Architecture feature slices
    
- Avoids unnecessary SPA complexity
    
- Enables incremental, scalable client-side enhancement
    
- Remains optional, modular, and replaceable