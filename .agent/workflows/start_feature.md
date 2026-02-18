---
description: Start a new feature development workflow
---

# Feature Development Workflow

This workflow guides you through starting a new feature for the HR Certification Portal.

1.  **Understand Requirements**: Read the user request and any linked documentation.
2.  **Check Existing Tasks**: Review `task.md` to see where this feature fits.
3.  **Create Branch**:
    ```bash
    git checkout main
    git pull origin main
    git checkout -b feat/your-feature-name
    ```
4.  **Create Implementation Plan**:
    - Create or update `implementation_plan.md`.
    - Outline the changes:
        - Database schema changes (Migrations).
        - New Razor Pages or modifications.
        - API/Service layer changes.
        - **Unit Tests** (Required).
5.  **Review**: Ask the user to review the plan.
6.  **Execute**:
    - Write code (TDD preferred).
    - Run tests: `dotnet test`.
    - Run app: `dotnet run`.
7.  **Finalize**:
    - Verify all tests pass.
    - Commit changes.
    - Push branch.
