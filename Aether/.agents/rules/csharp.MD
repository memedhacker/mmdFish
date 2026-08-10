---
trigger: always_on
---

# C# Rules

* Code and all identifiers: English.
* Comments and code documentation: Turkish.
* Agent communication: Turkish.
* Follow standard C# conventions.
* Keep Designer.cs generated; custom logic goes in .cs.
* Reuse existing code; avoid unnecessary files/classes.
* Change only what is requested.
* Use UserControl for reusable/meaningful UI components.
* Prefer modular, maintainable, and reusable code structures.
* Keep each class and method focused on a single responsibility.
* Avoid putting unrelated responsibilities into the same class or method.
* When creating or significantly modifying a method, add a concise Turkish comment explaining what the method does and its purpose.
* For important classes, add a concise Turkish comment explaining the class's responsibility.
* Comments should explain purpose and behavior, not obvious syntax.
* Do not add excessive comments to every line; document meaningful methods, classes, and non-obvious logic.
* Prefer reusing existing components and abstractions before creating new ones.
* Avoid unnecessary duplication of logic.
* Keep UI logic, business logic, state management, and utility logic separated when this improves maintainability.
* Prefer small, focused methods over large methods with multiple responsibilities.
* Before creating a new class, file, method, or abstraction, check whether an existing one can be reused or extended.
* Do not over-engineer simple functionality.
* Preserve existing behavior unless the requested change explicitly requires changing it.
* Minimize unnecessary changes to the existing codebase.
* When refactoring, prioritize readability, modularity, maintainability, and low coupling.
