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
- Use `docs/system-api-map` for BCL, `System.*`, package/reference, C# meta, and RhinoWIP host-reference policy; use `docs/external-libs` for approved product library APIs and `docs/testing-libs` for test-tool APIs.

## [6][LIVE_RHINO_BRIDGE]
- Source lives under `tools/rhino-bridge`: `protocol/` is the Rhino-free named-pipe protocol shared by client/plugin, `plugin/` is the RhinoCommon `.rhp` that executes RhinoCode in-process, and `client/` is the local CLI that owns orchestration and phase JSON.
- Use `scripts/rhino.sh bridge build` to build the bridge protocol, plugin, and CLI.
- Use `scripts/rhino.sh bridge package <version>` and `scripts/rhino.sh bridge install <local-yak-path>` for repeatable bridge plugin install.
- Use `scripts/rhino.sh bridge launch` to open RhinoWIP and verify a `hello` round trip against `~/.rasm/rhino-bridge.json`.
- Use `scripts/rhino.sh bridge doctor` against a running RhinoWIP session with the bridge loaded.
- Use `scripts/rhino.sh bridge check <project.csproj>` for a real build plus RhinoCode load/runtime smoke inside RhinoWIP.
- Use `scripts/rhino.sh bridge check-source <source.cs>` to resolve the owning evaluated SDK project and return `unsupported` unless a real script target is supplied.
- Use `scripts/rhino.sh bridge script <script.csx|.cs>` for explicit transient RhinoCode execution, and `scripts/rhino.sh bridge load-smoke <assembly.dll>` for lower-level assembly load/unload evidence.
- Do not automate Rhino settings or template creation from this repo. Persistent startup is owned by the plugin `LoadTime.AtStartup`; `_RasmBridgeStart` may be entered manually in Rhino settings if an operator wants a command-list fallback.
