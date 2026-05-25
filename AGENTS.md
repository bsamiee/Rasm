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
- Use `scripts/rhino.sh package rasm-bridge <version>` and `scripts/rhino.sh deploy rasm-bridge <version>` for repeatable bridge plugin packaging and install.
- Use `scripts/rhino.sh bridge launch` to open RhinoWIP and verify a `hello` round trip against `~/.rasm/rhino-bridge.json`.
- Use `scripts/rhino.sh bridge doctor` against a running RhinoWIP session with the bridge loaded.
- Use `scripts/rhino.sh bridge check <target> [scenario.csx]` as the agent-first runtime diagnostic command. It accepts project, source, and script targets, prints JSON to stdout, and auto-writes the same report under `.artifacts/rhino/bridge/check/<target-path>/`.
- `bridge check` runs RhinoCode with isolated C# reference resolution and no script cache reuse, so other loaded Rhino plugins cannot poison LanguageExt, Thinktecture, or repo assembly identity.
- Use `scripts/rhino.sh bridge check <source.cs>` for source ownership/build proof. It returns `unsupported` unless a real scenario is supplied as the second positional argument.
- Use `scripts/rhino.sh verify <scenario-or-glob>` as the scenario convenience rail; it resolves the owning project and routes through `bridge check <project> <scenario.verify.csx>`.
- Keep scenarios source-only. Do not add `#r`, `#load`, or absolute build-output paths; the bridge owns reference projection and fresh artifact refs.
- Use `scripts/rhino.sh bridge clean <target>` to delete generated reports for one target; use `scripts/rhino.sh bridge load-smoke <assembly.dll>` for lower-level assembly load/unload evidence.
- Do not automate Rhino settings or template creation from this repo. Persistent startup is owned by the plugin `LoadTime.AtStartup`; `_RasmBridgeStart` may be entered manually in Rhino settings if an operator wants a command-list fallback.
