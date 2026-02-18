# Testing Strategy

Reliability is critical for this internal tool. We strictly enforce testing for all features.

## Requirements
- **Unit Tests**: Every new feature, logic class, or critical path must have corresponding unit tests.
- **Regression Testing**: Existing tests must pass before any new code is merged.

## Test Stack
- **Framework**: xUnit
- **Mocking**: Moq
- **Assertion**: FluentAssertions

## Running Tests
```bash
dotnet test
```

## Continuous Integration
(Future) Automated testing pipelines should run on every push to `main` or pull request.
