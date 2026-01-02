# Banking System – Testing & Software Quality Exam Project

This project is a **backend banking system** built as part of the exam in  
**Testing and Software Quality**.

The primary goal is **not feature completeness**, but to demonstrate:
- structured architecture
- testability
- clear application of multiple testing techniques

The system is intentionally designed around **two core business flows**:
- **Transfers**
- **BSRun (Betalingsservice-like batch processing)**

---

## Architecture Overview

The project follows a strict layered architecture:

Controller -> Service (Business Logic) -> Repository (Persistence) -> DbContext (EF Core)


### Rules
- Controllers never access the DbContext
- Services contain all business rules
- Repositories only handle data access
- DbContext is only used in Infrastructure

This ensures:
- high testability
- low coupling
- clear responsibility boundaries

---

## Core Features

### Transfers
- Validate accounts (existence, active status)
- Balance checks
- Decision-based outcomes:
    - Success
    - Insufficient funds
    - Inactive account
    - Manual approval required

### BSRun (Betalingsservice)
- Batch-style processing of recurring payments
- Two main operations:
    - **NotifyUpcoming** – upcoming collections
    - **CollectDue** – due collections
- Supports failure handling:
    - cancelled mandates
    - missing settlement accounts
    - transfer failures

---

## Testing Focus (Exam Scope)

This project is explicitly designed to demonstrate:

### ✔ Behaviour-Driven Development (BDD)
- Scenarios documented in `SCENARIOS.md`
- Ready to be translated into Gherkin feature files

### ✔ Decision Table-Based Testing
- Used for Transfer validation rules
- Amount limits, account states, balances

### ✔ DD-Path Testing
- Transfer execution paths
- BSRun batch loop paths (success/failure branches)

### ✔ Automated API Testing
- Controllers are thin and predictable
- Deterministic seed data allows repeatable tests

---

## Deterministic Database Seeding

On application startup (Development environment only):

1. Database is **deleted**
2. Database is **recreated**
3. Deterministic seed data is inserted

### Why?
- Guarantees identical starting state
- Eliminates manual setup work
- Allows tests to rely on fixed IDs
- Ensures repeatable demos and grading

Seed data supports:
- Successful and failing transfers
- Active and cancelled mandates
- Due and upcoming BS collections

See `SCENARIOS.md` for exact mappings.

---

## Technology Stack

- **.NET 9**
- **ASP.NET Core Web API**
- **Entity Framework Core**
- **SQLite (local / tests)**
- **PostgreSQL (Docker runtime)**
- **Docker & Docker Compose**

---

## Running the Project

### Run locally (SQLite)
```bash
dotnet run --project Banking.Api
```
### or on Docker
```bash
docker compose up --build
```
http://localhost:8080/swagger

