# Contributing & Development Guidelines

## Git Workflow
- **Main Branch (`main`)**: Stable, production-ready code.
- **Feature Branches (`feat/name`)**: New features.
- **Fix Branches (`fix/name`)**: Bug fixes.
- **Docs Branches (`docs/name`)**: Documentation updates.

### Commit Messages
We follow the [Conventional Commits](https://www.conventionalcommits.org/) specification.

**Format**: `<type>(<scope>): <subject>`

**Types**:
- `feat`: A new feature
- `fix`: A bug fix
- `docs`: Documentation only changes
- `style`: Changes that do not affect the meaning of the code (white-space, formatting, etc)
- `refactor`: A code change that neither fixes a bug nor adds a feature
- `perf`: A code change that improves performance
- `test`: Adding missing tests or correcting existing tests
- `chore`: Changes to the build process or auxiliary tools

**Example**:
```bash
git commit -m "feat(auth): implement login page layout"
git commit -m "fix(db): correct connection string parameter"
```

## Agent Workflows
- Use `.agent/workflows/` for complex tasks.
- If you identify a repeatable pattern, create a new skill in `skills/` using the `skills/create_skill.md` guide.

## Code Standards
- **C#**: Follow standard .NET coding conventions.
- **Razor Pages**: Keep logic in PageModels (`.cshtml.cs`), view in `.cshtml`.
- **UI**: Use AdminLTE classes and project-specific branding colors defined in `REQUIREMENTS.md`.
