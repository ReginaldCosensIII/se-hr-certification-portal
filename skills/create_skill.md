---
name: Skill Completion & creation
description: Instructions for identifying recurring patterns and creating new agent skills.
---

# Skill Creation Workflow

As we develop, if we identify a recurring complex task that requires multiple steps or specific standards, we should encapsulate it into a **Skill**.

## Process
1.  **Identify Pattern**: You are performing a task that you've done before or is complex (e.g., "Creating a new Razor Page with CRUD", "Adding a new Database Entity").
2.  **Draft Skill**: Create a markdown file in `skills/` (e.g., `skills/create_crud_page.md`).
3.  **Define Steps**:
    - Prerequisites (files needed).
    - Step-by-step instructions.
    - Code templates/snippets.
    - Verification steps.
4.  **Register**: Creating the file is enough. The user or system can then direct you to "Use the Create CRUD Page skill".

## Candidate Skills
- **Entity Creation**: Workflow for adding a Model -> DbContext -> Migration -> Update DB.
- **Page Creation**: Workflow for PageModel -> cshtml -> Sidebar link.
