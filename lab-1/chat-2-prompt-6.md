## User

Build errors: CS0103 - SetCurrentPage and SetBreadcrumbs not found in current context across all controllers.

## Claude

Root cause: C# extension methods require explicit `this.` prefix when called from within the class instance. The compiler does not search for extension methods in the implicit `this` scope.

Fix: Used sed to replace all `SetCurrentPage(` and `SetBreadcrumbs(` calls with `this.SetCurrentPage(` and `this.SetBreadcrumbs(` across all 6 controller files. Build succeeded with 0 errors.
