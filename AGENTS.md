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
- `tools/rhino-bridge` answers runtime questions that static .NET gates cannot. Architecture (operator CLI → client → protocol → in-Rhino plugin), command catalog, output contract, failure reading, and validation ladder live in `tools/rhino-bridge/README.md` and `tools/rhino-bridge/AGENTS.md`.
- Agent-first scenario rail: `bash scripts/rhino.sh verify <scenario-or-glob>` for `*.verify.csx` files under `tests/csharp/libs/<Project>/<MirrorPath>/scenarios/`. Scenarios are source-only — no `#r`, no `#load`, no absolute paths.
- Scenarios consume universal capsules from `Rasm.TestKit.Scenarios` (`Scenario.Run`, `FactBag`, `Probe`, `DocumentScope`, `Capture`). The bridge stages `Rasm.TestKit.dll` automatically; no manual reference setup. Author scenarios as `Scenario.Run("theme", CAPTURE_PATH, (key, facts) => { … });` with `facts.Add(string key, object value);` for per-fact evidence — the harness emits a single batched `facts={json}` plain line and a `rasm.rhino-bridge.evidence=facts={json}` marker on scope exit.
- Diagnostic commands: `bridge build`, `bridge doctor`, `bridge check <target> [scenario]`, `bridge launch` (idempotent — reuses existing endpoint or relaunches), `bridge clean`, `bridge quit`. API metadata: `api doctor|path|xml|types|decompile`. Packaging: `package rasm-bridge <ver>` / `deploy rasm-bridge <ver>` / `publish rasm-bridge <ver>` (install + push merged into one route).
- Do not automate Rhino settings or template creation from this repo. Persistent startup is owned by the plugin `LoadTime.AtStartup`.
