# [AGENT_MANIFEST]

[REQUIRED]: Read and adhere to `CLAUDE.md`

## [1][REQUIRED_STANDARDS]

If reviewing, refining, editing, creating, or modifying X file type, use skill Y (required):
- Typescript: `coding-ts`
- C#: `coding-csharp`
- Python: `coding-python`
- Bash/sh: `coding-bash`
- Markdown/docs: `docgen` + `style-standards`

## [2][NAVIGATION_CONTEXT]
- Use `fd` for discovery, then `rg` for exact references.
- Use structural search (`ast-grep`) for symbol-aware changes when available.
- Use Nx topology (`nx graph`, affected commands, `nx-mcp`) before broad scans.
- Read minimal file slices necessary for the current task.
- Navigation helpers:
  - `fd -H .`
  - `rg -n --hidden --glob '!.git' --glob '!node_modules' "<pattern>" <path>`
  - `pnpm exec ast-grep run --pattern "<structural-pattern>" <path>`
  - `ctags -R --exclude=.git --exclude=node_modules --exclude=dist --exclude=build --exclude=.nx .`

## [3][LANGUAGE_POLICY]
- ALWAYS: follow `CLAUDE.md` Effect-first approach.
- C#: preserve strict analyzer and formatting posture in `.editorconfig` and `Directory.Build.props`.
- Python: enforce Python 3.14+ baseline via Ruff + ty with explicit configuration.

## [4][CODING_POLICY]
- Prefer refining/extending existing modules over adding wrappers or duplicate helpers.
- Always read a file fully, identify if possible to do less code and refactor/extend existing logic over spamming new functionality.
- Keep implementations dense, strongly typed, and test/validation-backed.
- Avoid verbosity spam in plans or explanations; keep detail high and signal-focused.

## [5][DOCUMENTATION_POLICY]
- Route README, ADR, changelog, architecture, code documentation, and standards changes through `docgen`.
- Route Markdown structure, headers, lists, tables, diagrams, separators, and voice changes through `style-standards`.
- Keep documentation rooted in existing paths, commands, and configured tooling; remove invented or stale paths.
