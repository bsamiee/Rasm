# Rasm Project Cursor Rules

Rule pack for the Rasm monorepo. Files live in `.cursor/rules/` at the repo root and are versioned with the project.

## Activation

| Method | Auto globs | Notes |
|--------|------------|-------|
| **Project rules (default)** | Yes | Cursor loads `.mdc` files here when the workspace is open |
| **Personal skills** (`~/.cursor/skills/rasm-*`) | Via skill descriptions | Optional global router; complements project rules |
| **User Rules paste** | No | Optional — copy `user-rules-bootstrap.txt` into Cursor Settings → Rules → User Rules |
| **Manual @mention** | No | Reference `@rasm-csharp-production` etc. in Agent chat |

Cursor docs: project `.mdc` rules use YAML frontmatter (`description`, `globs`, `alwaysApply`). User Rules in Settings are plain text only (no frontmatter).

## Rule index

| File | Scope |
|------|-------|
| `rasm-core.mdc` | Always — manifest pointers, philosophy, gates |
| `rasm-monorepo-navigation.mdc` | Intelligent — fd/rg/Nx discovery |
| `rasm-csharp-production.mdc` | `**/*.cs` (not tests) |
| `rasm-csharp-tests.mdc` | `**/*.spec.cs`, `**/*.verify.csx` |
| `rasm-bash-scripts.mdc` | `**/*.sh` |
| `rasm-docs-markdown.mdc` | `**/*.md` |
| `rasm-rhino-bridge.mdc` | Bridge tool + scenarios |
| `rasm-dependencies.mdc` | Package manifests |

Repo truth stays in `CLAUDE.md`, `AGENTS.md`, and `docs/` — these rules distill Cursor-specific routing only.
