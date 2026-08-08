# Copilot Working Rules

When responding to implementation requests:

1. Analyse first.
2. Explain the current implementation and affected files.
3. Propose a minimal implementation plan before generating code.
4. Reuse the existing architecture and patterns.
5. Make the smallest necessary changes.
6. Preserve existing API contracts unless a change is required.
7. Do not modify unrelated projects or files.
8. Summarize what changed after implementation.

## Code Output

- Do not output complete files when only part of a file changed.
- Show only changed methods or relevant code sections.
- Prefer concise diffs for small changes.
- Include file path and line numbers when available.
- Show a complete file only when it is new or substantial restructuring requires it.