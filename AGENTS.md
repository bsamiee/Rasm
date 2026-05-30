# [AGENT_MANIFEST]

[REQUIRED]: Read and adhere to `CLAUDE.md`

## [0][LOAD_ORDER]
- **Cursor / Codex chain:** `CLAUDE.md` → root `AGENTS.md` → nearest nested `AGENTS.md` → matched `.cursor/rules/*.mdc` → skill by file type.
- **Codex budget:** nested `AGENTS.md` chains concatenate toward a 32 KiB limit — keep nested files delta-only; offload encyclopedic detail to co-located `_ARCHITECTURE.md`.
- **Optional local override:** gitignored `AGENTS.override.md` beside root or bridge AGENTS for machine-specific paths.

## [1][REQUIRED_STANDARDS]

If reviewing, refining, editing, creating, or modifying X file type, use skill Y (required):
- Typescript: `coding-ts`
- C#: `coding-csharp`
- C# tests (`.spec.cs`, `.verify.csx`, testkit): `cs-testing`
- Python: `coding-python`
- Bash/sh: `coding-bash`
- SQL: `coding-pg`
- Markdown/docs: `docgen` + `style-standards`

## [2][NAVIGATION_CONTEXT]
- Cross-stack owner precedence and proof order: `docs/usage.md` §1 and §5 before leaf API docs.
- Use `fd` for discovery, then `rg` for exact references.
- Use structural search (`ast-grep`) for symbol-aware changes when available.
- Use Nx topology (`nx graph`, affected commands, `nx-mcp`) before broad scans.
- Read minimal file slices necessary for the current task.
- Check `docs/system-api-map` before `System.*`, global using, or host-reference changes. `global.json` is absent; treat it as an optional trigger only if added later.
- Read `docs/external-libs` and `docs/host` before adding packages or host SDK assumptions.
- Navigation helpers:
  - `fd -H .`
  - `rg -n --hidden --glob '!.git' --glob '!node_modules' "<pattern>" <path>`
  - `pnpm exec ast-grep run --pattern "<structural-pattern>" <path>`
  - `ctags -R --exclude=.git --exclude=node_modules --exclude=dist --exclude=build --exclude=.nx .`

| [INDEX] | [PATH]                         | [OWNS]                                                            |
| :-----: | ------------------------------ | ----------------------------------------------------------------- |
|   [1]   | `libs/csharp/Rasm`             | Domain, Analysis, Vectors geometry kernel                         |
|   [2]   | `libs/csharp/Rasm.Rhino`       | RhinoWIP boundary — Commands, UI, Camera, Blocks, Exchange        |
|   [3]   | `libs/csharp/Rasm.Grasshopper` | GH2 components, data, UI rails                                    |
|   [4]   | `tests/csharp`                 | xUnit, CsCheck, testkit, bridge scenarios                         |
|   [5]   | `tools/rhino-bridge`           | Live RhinoWIP runtime verification                                |
|   [6]   | `docs/usage.md`                | Cross-stack owner ladder and proof hierarchy                      |
|   [7]   | `docs/host/`                   | RhinoCommon and GH2 native SDK boundaries                         |
|   [8]   | `docs/host-libraries.md`       | Composition-root packages — doc pins; not-in-graph until consumer |
|   [9]   | `docs/system-api-map`          | BCL, `System.*`, package and host reference policy                |
|  [10]   | `docs/external-libs`           | Approved product libraries                                        |
|  [11]   | `docs/testing-libs`            | Test library APIs                                                 |
|  [12]   | `docs/standards`               | Documentation voice and structure                                 |

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
- Cross-stack owner precedence: `docs/usage.md` §1 and §5.
- Product library API truth: `docs/external-libs`.
- Host SDK boundaries: `docs/host/` (`README.md` → `rhino.md`, `gh2.md`).
- Host composition adoption (Scrutor, EF, OTel…): `docs/host-libraries.md` — not-in-graph until a bootstrap consumer exists; pin truth in `Directory.Packages.props` and `docs/system-api-map/packages.md` §2.
- BCL, packages, host references: `docs/system-api-map`.
- Test-tool APIs: `docs/testing-libs`.
- Universal C# enforcement snippets: `.claude/skills/coding-csharp/references/`; repo posture and XML-backed proof: `docs/external-libs/`. Do not duplicate skill bodies in docs leaves.

## [6][LIVE_RHINO_BRIDGE]
- Runtime evidence and bridge operator routes: `CLAUDE.md` §5.2 and `tools/rhino-bridge/README.md`.
- Canonical bridge agent deltas: `tools/rhino-bridge/AGENTS.md`.
- Scenario authoring: `tests/csharp/AGENTS.md` §7 and `.claude/skills/cs-testing/references/bridge-runtime.md`.
