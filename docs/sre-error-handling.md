# Site Reliability Engineering (SRE) & Error Handling

This document outlines the global exception handling and graceful degradation strategies implemented across the HR Certification Portal to ensure maximum uptime and a resilient user experience.

## Global Exception Handling Strategy

During Epic 4, a comprehensive sweep of the codebase was conducted to isolate all database interactions and network dependencies within robust try-catch blocks.

### 1. ILogger<T> Implementation
Every Razor Page model (`.cshtml.cs`) now explicitly injects an `ILogger<T>`. This ensures that all critical failures are immediately logged to the server console/application insights for rapid incident response and post-mortem analysis.

### 2. Try-Catch Isolation
All `OnGetAsync`, `OnPostAsync`, and data-fetching methods are strictly wrapped in `try-catch` blocks. The application is designed to never throw unhandled exceptions directly to the user interface resulting in generic 500 Server Error screens.

### 3. Graceful UI Degradation
When a failure occurs (e.g., loss of database connectivity), the `catch` block intercepts the exception and sets a user-friendly alert via `TempData["ErrorMessage"]`. This triggers the global alert banner in `_Layout.cshtml` to notify the user of the outage without halting the entire application.

### 4. NullReferenceException Prevention
The most critical aspect of the error handling strategy is protecting the Razor Views (`.cshtml`) from crashing when iterating over missing data. In the event of a database failure, the `catch` blocks explicitly instantiate empty collections for all primary view models:
```csharp
catch (Exception ex)
{
    _logger.LogError(ex, "Critical failure loading data.");
    TempData["ErrorMessage"] = "System outage. Unable to connect to the database.";
    
    // Crucial: Initialize empty collections to prevent Razor crashes
    Agencies = new List<Agency>();
    Requests = new List<Request>();
    
    return Page();
}
```
By feeding the view an empty list rather than `null`, the UI seamlessly degrades to display the "No Records Found" zero-state fallbacks rather than throwing a YSOD (Yellow Screen of Death).
