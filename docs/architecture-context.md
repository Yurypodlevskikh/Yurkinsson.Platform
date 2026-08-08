# SpeedUp Architecture Context

## Overview

SpeedUp is a full-stack application built using a frontend + Backend-for-Frontend (BFF) + centralized Identity Service architecture.

The solution follows:

- Separation of concerns
- Backend-for-Frontend (BFF) pattern
- SPA-driven user experience
- JSON-only backend communication
- Layered architecture
- Incremental evolution instead of redesign

---

# Solution Structure

## Frontend

### SpeedUpVue (Vue 3 + Vite SPA)

Responsibilities:

- UI rendering
- Navigation
- Authentication UI
- Login
- Registration
- Email confirmation
- Password reset
- User interaction

Characteristics:

- SPA
- Tab-based authentication UI
- Vue 3
- Vite
- Router may be introduced when beneficial
- Communicates ONLY with SpeedUpMiniAPI

The frontend must never communicate directly with YurkinssonAuthentication.

---

## Backend

### SpeedUpMiniAPI (ASP.NET Core Minimal API)

Acts as:

- Backend-for-Frontend (BFF)
- Proxy
- Orchestration layer

Responsibilities:

- Single entry point for frontend
- Calls YurkinssonAuthentication
- Maps Identity responses into frontend DTOs
- Handles application business logic
- Stores metronome settings
- Performs token validation
- Coordinates communication between services

Architecture:

- WebApi
- BusinessLogic
- DataAccess

Rules:

- Do not bypass layers.
- Do not return HTML.
- Return JSON only.
- Do not contain Identity business logic.
- Keep orchestration responsibilities inside MiniAPI.

---

## Identity Service

### YurkinssonAuthentication

Responsibilities:

- Registration
- Login
- Email confirmation
- JWT issuing
- JWT validation
- Refresh tokens
- Password reset
- User management

Characteristics:

- Separate MariaDB database
- Independent service
- UI-agnostic
- Supports multiple client applications
- Uses ClientApp identification

Rules:

- Return JSON only.
- Never return HTML pages.
- Never perform frontend navigation.
- Never expose internal exceptions.
- Keep Identity independent from client applications.

---

# Communication Flow

Frontend

↓

SpeedUpMiniAPI

↓

YurkinssonAuthentication

Rules:

- Frontend never calls Identity directly.
- MiniAPI is always the single entry point.
- Communication between services is JSON-based.
- Identity never returns HTML.
- External clients must never communicate directly with YurkinssonAuthentication.
- YurkinssonAuthentication is an internal service.
- All external requests must go through the appropriate Backend-for-Frontend (BFF) service (for example, SpeedUpMiniAPI).
- Identity must not contain any client-specific routing or UI logic.

---

# Email Confirmation Architecture

Email confirmation is SPA-driven.

Desired flow:

1. User receives an email.

2. Email contains a frontend URL.

Example:

/email-confirmation?userId={id}&token={token}

3. Frontend extracts parameters.

4. Frontend calls MiniAPI.

5. MiniAPI calls Identity.

6. Identity validates confirmation.

7. Identity returns JSON.

8. MiniAPI maps the response.

9. Frontend updates the UI.

Frontend controls:

- navigation
- success messages
- error messages
- resend confirmation
- login flow

Identity never performs redirects.

---

# Standard API Response Contract

All backend services should use the same response model.

```json
{
  "success": true,
  "message": "string",
  "errorCode": "ErrorCodes",
  "validationErrors": {},
  "developerMessage": "string"
}
```

Rules:

- success is mandatory
- message is user friendly
- developerMessage is Development only
- validationErrors are optional
- errorCode uses existing ErrorCodes enum
- Do not replace enums with string literals.

---

# Error Handling

Use strongly typed enums.

Preferred:

ErrorCodes.InvalidToken

Never replace enum-based error handling with string constants unless explicitly requested.

Use appropriate HTTP status codes.

Keep response mapping centralized.

---

# Client Applications

Identity is designed for multiple client applications.

Applications identify themselves using ClientApp.

Confirmation links should be generated from configuration.

Never hardcode frontend URLs.

Prefer configuration such as:

Frontend:BaseUrl

or

ClientRedirectUrls

Support future Client Registry expansion.

---

# Development Principles

Always prefer:

- incremental improvements
- maintainability
- readability
- testability
- backward compatibility

Do not:

- redesign architecture
- merge projects
- move business logic between layers
- introduce unnecessary infrastructure

Reuse existing implementations whenever possible.

---

# Unit Testing Principles

Testing framework:

- xUnit
- Moq
- FluentAssertions

Rules:

- Update tests when production code changes.
- Do not modify production code only to satisfy tests.
- Keep mocks minimal.
- Mock only external dependencies.
- Prefer It.IsAny<T>() unless exact matching is required.
- Reuse Arrange helpers.
- Prefer behaviour verification over implementation verification.
- Keep tests resilient to refactoring.

---

# Existing Project Conventions

Assume existing implementations are intentional.

Reuse existing:

- DTOs
- Enums
- Services
- Controllers
- Minimal API endpoints
- Repository interfaces
- Response contracts

Extend existing code instead of replacing it.

Do not introduce duplicate models.

Do not create duplicate controllers.

Do not recreate DTOs that already exist.

---

# AI Collaboration Rules

When acting as an assistant for this solution:

Always:

- analyse existing code before proposing changes
- follow the existing architecture
- preserve existing coding style
- explain architectural reasoning before major changes

Never:

- replace enums with strings
- duplicate existing classes
- duplicate controllers
- duplicate DTOs
- duplicate endpoints
- create files under Miscellaneous Files

If a referenced source file is unavailable:

- ask the user to open the file
- never create a replacement file

Before creating any new model or service:

- verify that an equivalent implementation does not already exist

When context is missing:

- ask for the required source files first

---

# Copilot Working Rules

For every implementation request:

1. Analyse the existing implementation before proposing changes.

2. Explain the current implementation.

3. Identify the affected projects and source files.

4. Reuse the existing architecture, services, DTOs and response contracts.

5. Minimize code changes and preserve backward compatibility.

6. Do not modify unrelated files.

7. Update affected unit tests when necessary.

8. Explain why each change is required.

9. Explain how the implementation should be manually verified.

10. If required context is missing, ask the user to open the relevant source files before generating code.

Always:

- preserve existing coding style
- extend existing implementations instead of replacing them
- avoid duplicate classes, DTOs, controllers and endpoints

Never:

- redesign the architecture unless explicitly requested
- create files under Miscellaneous Files
- generate replacement files when the originals are unavailable
- skip the analysis step