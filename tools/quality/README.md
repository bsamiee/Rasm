# [H1][QUALITY_OPERATOR]
>**Dictum:** *One rail owns one proof claim.*

<br>

[IMPORTANT] `tools.quality` is an agent-only CLI. Run the narrowest rail that owns the claim. Report command, exit, and evidence path.

```bash
uv run python -m tools.quality <rail> <verb> [args]
```

Rails stay orthogonal: `static` never runs tests, `test` never opens Rhino, `bridge verify` never replaces `static build`, and `api` never launches Rhino.

[CRITICAL] Output contract: every verb writes exactly ONE JSON `Envelope` object to stdout — fields `rail`, `verb`, `status`, `exit_code`, `run_id`, `evidence`, `data`, `error`, `truncated`, `notes`. `status` is one of `ok | empty | skip | busy | timeout | unsupported | failed`. The per-verb payload (static plan, test summary, verify report, api query result) nests verbatim under `data`. Streamed `dotnet` build/test/run bytes and all structlog diagnostics go to STDERR only — never stdout. There is no raw-text passthrough and no World-A/World-B duality; stdout is always the single Envelope.

---
## [1][RAIL_MAP]
>**Dictum:** *Graph highlights routing and contention only.*

<br>

```mermaid
---
config:
  layout: elk
  look: neo
  theme: base
  themeVariables:
    background: "#282a36"
    fontFamily: "JetBrains Mono, monospace"
    primaryColor: "#44475a"
    primaryTextColor: "#f8f8f2"
    primaryBorderColor: "#8be9fd"
    lineColor: "#8be9fd"
    clusterBkg: "#21222c"
    clusterBorder: "#6272a4"
---
flowchart LR
  accTitle: Quality operator routing
  accDescr: Static fixes and builds CSharp projects. Test runs MTP and explicit mutation. Bridge owns live Rhino. Package owns Yak staging. API reads host metadata.

  cli["quality CLI"] --> scope["ArtifactScope<br/>run_id + DOTNET_CLI_HOME"]
  scope --> static["static<br/>fix report build full plan"]
  scope --> test["test<br/>MTP + coverage + mutation"]
  scope --> bridge["bridge<br/>Rhino client + verify"]
  scope --> package["package<br/>Yak stage deploy publish"]
  scope --> api["api<br/>ilspy surface + decompile"]

  static --> slnx["Workspace.slnx<br/>full/parity only"]
  static --> csproj["owner csproj closure<br/>path or git changes"]
  test --> mtp["global.json<br/>MTP runner"]
  test --> mutation["mutation.lock<br/>fail fast"]
  bridge --> rhino["bridge.lock<br/>fail fast"]
  package --> yak["stage lock<br/>fail fast"]
  api --> bundle["Rhino bundle<br/>read only"]

  classDef rail fill:#44475a,stroke:#8be9fd,color:#f8f8f2
  classDef proof fill:#282a36,stroke:#f1fa8c,color:#f8f8f2
  classDef exclusive fill:#282a36,stroke:#ff79c6,color:#f8f8f2
  class static,test,bridge,package,api rail
  class slnx,csproj,mtp,bundle proof
  class mutation,rhino,yak exclusive
```

<br>

| [INDEX] | [MODULE]           | [OWNERSHIP]                                      |
| :-----: | ------------------ | ------------------------------------------------ |
|   [1]   | `__main__.py`      | Cyclopts tree, `rail()`, single-Envelope stdout contract. |
|   [2]   | `settings.py`      | `QualitySettings`, root anchor, artifact paths.  |
|   [3]   | `process.py`       | Subprocess, dotnet args, nonblocking leases.     |
|   [4]   | `rails/static.py`  | C# fix/report/build planning.                    |
|   [5]   | `rails/test.py`    | MTP, coverage, explicit Stryker mutation.        |
|   [6]   | `rails/bridge.py`  | Bridge client, verify reports, Rhino lease.      |
|   [7]   | `rails/package.py` | Yak metadata, atomic stage, stage lease.         |
|   [8]   | `rails/api.py`     | Host and NuGet API resolver; ilspy surface + decompile. |

---
## [2][COMMAND_SURFACE]
>**Dictum:** *Verb names encode cost and mutability.*

<br>

Run from any path under the worktree. `QualitySettings.anchor()` walks parents until `Workspace.slnx`.

| [INDEX] | [RAIL]   | [COMMANDS]                                                                                 | [CLAIM]                    |
| :-----: | -------- | ------------------------------------------------------------------------------------------ | -------------------------- |
|   [1]   | `static` | `fix [paths...]`, `report [paths...]`, `build [paths...]`, `full`, `plan [paths...]`.       | C# cleanup and build proof. |
|   [2]   | `test`   | `run`, `list`, `coverage`; flags: `--target`, `--all`, `--no-build`, `--test-modules`, `--format json`, `--limit`, `--grep`. | Unit, coverage, mutation.  |
|   [3]   | `bridge` | `build-bridge`, `doctor`, `launch`, `quit`, `check`, `clean`, `verify`.                    | Live Rhino evidence.       |
|   [4]   | `bridge` | `package <slug> <version>`, `package plan <slug> <version>`, `package list`, `deploy <slug> <version>`, `publish <slug> <version>`. | Yak lifecycle.             |
|   [5]   | `api`    | `doctor [--strict]`, `resolve <key> [kind]`, `query <key> [symbol]`, `show <token>`. | Host and package API truth. |
|   [6]   | root     | `self-test [--rhino]`.                                                                     | Tool/path preflight.       |

Use the Python module entrypoint directly. Do not add package-manager aliases for this operator.

---
## [3][STATIC_RAIL]
>**Dictum:** *Fix before proof; build owns remaining diagnostics.*

<br>

[CRITICAL] `static fix` mutates files. `static report`, `static build`, `static full`, and `static plan` do not intentionally mutate tracked source.

| [INDEX] | [MODE]   | [BEHAVIOR]                                                                                 |
| :-----: | -------- | ------------------------------------------------------------------------------------------ |
|   [1]   | `fix`    | Runs one bare `dotnet format <csproj> --include <files> --severity error` per project.     |
|   [2]   | `report` | Runs same bare format pass with `--verify-no-changes` and `--report`.                      |
|   [3]   | `build`  | Restores/builds owner closure in `Debug` under stable per-closure `--artifacts-path`; full-trigger plans build `Workspace.slnx`. |
|   [4]   | `full`   | Verifies `.slnx` parity; restores/builds `Workspace.slnx` in `Debug` and `Release` under the `solution` closure. |
|   [5]   | `plan`   | Emits JSON: inputs, owners, closure, full triggers, exact dotnet commands.                 |

Routing:
- No paths: read unstaged diff, staged diff, and untracked files.
- Paths: route explicit files; expand directories with `fd`.
- Full triggers: `.config/dotnet-tools.json`, `Directory.Build.props`, `Directory.Build.targets`, `Directory.Packages.props`, `Workspace.slnx`, `.editorconfig`, `global.json`, and `tools/cs-analyzer/**`.
- Owner route: nearest `*.csproj`; `.cs` files join format groups; project seeds expand reverse `ProjectReference` closure.
- Orphan `.cs`, `.csproj`, `.props`, or `.targets`: force full scope.

Modern command ladder:

```bash
uv run python -m tools.quality static fix libs/csharp/Rasm.Grasshopper
uv run python -m tools.quality static build libs/csharp/Rasm.Grasshopper
uv run python -m tools.quality static full
```

Direct dotnet equivalence:
- Fix: `dotnet format <csproj> --include <files> --severity error`. One bare pass runs whitespace, style, and analyzers together.
- Build: `dotnet restore <csproj> --locked-mode`; then `dotnet build <csproj> -c Debug --no-restore -v:quiet /clp:ErrorsOnly -maxcpucount:<n>`.
- Full: same build shape against `Workspace.slnx` for both configured full configurations.

[CRITICAL] `static fix` and `static report` omit `--no-restore`. Bare `dotnet format` needs a restored compilation; `--no-restore` on a cold per-run project silently skips semantic IDE rules (IDE0001 name simplification, IDE0005). Implicit restore against the warm NuGet cache resolves them. `build` keeps explicit `restore --locked-mode` then `build --no-restore`.

Closure isolation:
- `build` and `full` run under a stable `--artifacts-path .artifacts/quality/build/<closure>`; closure is the first 16 hex of `sha256` over sorted project paths, and `full` resolves to `"solution"`.
- A non-blocking `build-<closure>.lock` lease guards each closure. Busy returns `busy` with owner text and exit `5`; it never hangs.
- Distinct closures build concurrently; the same closure twice returns instant `busy`.

---
## [4][TEST_RAIL]
>**Dictum:** *MTP execution stays explicit and target-bound.*

<br>

MTP source: `global.json` uses `"runner": "Microsoft.Testing.Platform"`.

| [INDEX] | [MODE]     | [BEHAVIOR]                                                    |
| :-----: | ---------- | ------------------------------------------------------------- |
|   [1]   | `run`      | `dotnet test` with `--minimum-expected-tests 1`.              |
|   [2]   | `list`     | MTP `--list-tests`.                                           |
|   [3]   | `coverage` | Coverlet JSON + Cobertura with includes/excludes in `test.py`. |

Targeting:
- Default project: `tests/csharp/libs/Rasm/Rasm.Tests.csproj`.
- `--target <csproj>` replaces default project.
- `--all` runs `Workspace.slnx`.
- `--no-build` adds MTP `--no-build`; use after successful static build.
- `--test-modules "<glob>"` runs built test modules with `--root-directory <repo>`.
- `--all` and `--test-modules` cannot combine.
- `list --format json --limit N --grep PATTERN` emits bounded JSON test discovery for agents.

Mutation:
- Default `--mutation off`; no implicit mutation on `test run`.
- `changed` mutates changed `.cs` files under `libs/csharp/Rasm`.
- `full` mutates `**/*.cs` excluding `bin/` and `obj/`.
- Eligible only for default `Rasm.Tests` plus `libs/csharp/Rasm/Rasm.csproj`.
- Tool: `dotnet-stryker`, MTP runner, thresholds `95/90/85`; version lives in `.config/dotnet-tools.json` as the source of truth.
- Lock: `.artifacts/locks/mutation.lock`; live contention fails fast.

---
## [5][BRIDGE_PACKAGE_RAIL]
>**Dictum:** *Runtime and package rails own exclusive resources.*

<br>

[CRITICAL] Live Rhino and package staging never wait. Contention returns `busy` with owner text and exit `5`.

Bridge commands:
- `build-bridge` builds protocol, plugin, and client under `ArtifactScope`; it does not acquire Rhino lease.
- `doctor`, `launch`, `quit`, `check`, and `clean` acquire `.artifacts/locks/bridge.lock`, build client, then run `dotnet run --no-build`.
- `verify <pattern>` acquires `bridge.lock`, expires old reports, builds client and scenario kit, launches Rhino, runs scenarios, and emits `VerifyReport` JSON.

Verify discovery order:
1. Direct `*.verify.csx` file.
2. Directory containing `*.verify.csx`.
3. Worktree glob; bare names expand as `**/<pattern>`.

Package commands:
- Resolve one `*.csproj` under `apps/` or `tools/` with matching `YakPackageSlug`.
- Validate `.rhp`, target dir, Yak platform `mac`, package glob `*-rh9_*-mac.yak`, and executable `yak`.
- Build artifact, copy manifest/package files, exclude host assemblies, run `yak build`, then replace stage dir under nonblocking stage lease.
- `package` emits a `package` rail Envelope with stage path under `data.artifact_paths.stage`.
- `package plan <slug> <version>` emits package project and evaluated Yak metadata under `Envelope.data` without staging.
- `package list` emits discovered package projects under `Envelope.data` without MSBuild or Yak mutation.
- `deploy` runs `yak install`; `rasm-bridge` also `quit`, `install`, `refresh`.
- `publish` runs deploy path plus `yak push` when `YakPushSource` exists.

Bridge `Envelope.status` → `exit_code`:

| [INDEX] | [STATUS]          | [EXIT] | [INTERPRETATION]                             |
| :-----: | ----------------- | -----: | -------------------------------------------- |
|   [1]   | `ok`, `skip`      |      0 | Valid or intentionally skipped scenario.     |
|   [2]   | `failed`          |      1 | Build, connect, execute, or scenario failure. |
|   [3]   | `unsupported`     |      3 | Build proof valid; no scenario path supplied. |
|   [4]   | `busy`, `timeout` |      5 | Exclusive resource busy or scenario timeout. |

---
## [6][API_RAIL]
>**Dictum:** *API truth returns clean reports and stores full evidence.*

<br>

API evidence root: `.artifacts/quality/api/<run-id>/`. Default commands emit compact JSON only. Full raw stdout/stderr, the type/namespace surface, decompiled source, and final `report.json` are stored in the run artifact directory. Four verbs own the rail; `query` replaces the former members, source, types, decompile, and xml-search probes.

| [INDEX] | [COMMAND]                         | [BEHAVIOR]                                                                  |
| :-----: | --------------------------------- | --------------------------------------------------------------------------- |
|   [1]   | `api doctor [--strict]`           | Host and NuGet inventory plus tool health for ilspycmd, Rhino, and RhinoCode; absorbs the former `sources` inventory. |
|   [2]   | `api resolve <key> [all\|assembly\|xml\|nuspec\|deps\|package-root]` | Resolved asset paths across managed, native, build, analyzer, and tool assets; default kind `all`. |
|   [3]   | `api query <key> [symbol] [--max-lines N] [--full] [--grep text]` | One polymorphic engine; discriminates on the shape of `symbol` against the ilspy surface. |
|   [4]   | `api show <artifact-or-symbol> [--lines A:B] [--grep text] [--full] [--latest]` | Current-run artifact preview or full content inside `Envelope.data`; historical lookup requires `--latest`.  |

`query` shapes (discriminated by `symbol`):

| [INDEX] | [SHAPE]     | [TRIGGER]                                          | [RETURN]                                                       |
| :-----: | ----------- | -------------------------------------------------- | -------------------------------------------------------------- |
|   [1]   | `index`     | Empty `symbol`.                                    | Namespaces plus type count; cheap.                             |
|   [2]   | `namespace` | `symbol` matches a namespace.                      | Types under the namespace; cheap.                              |
|   [3]   | `type`      | `symbol` matches a type FQN, exact or suffix.      | Signature, optional xml-doc, decompiled body bounded by `--max-lines`. |
|   [4]   | `member`    | `symbol` is `Type.Member`.                         | Signature, optional xml-doc, body window around the member.    |
|   [5]   | `search`    | No match.                                          | Ranked grep over surface plus xml; never empty.                |

Type resolution ranks exact FQN above suffix match. Engine: `ilspycmd` invoked through `dotnet tool run ilspycmd -- ...` — the nix-resilient path, with an apphost `DOTNET_ROOT` overlay as belt-and-suspenders, not a bare PATH tool. The XML-free type/namespace surface comes from `ilspycmd -l cisde`, cached per key plus assembly fingerprint under `.artifacts/quality/api/surface/`, and bodies decompile via `ilspycmd -t`. XML sidecars are doc-prose enrichment only and never required. No reference-assembly generator tool exists to lean on — verified absent from nuget.org, the dnceng dotnet-tools feed, and SDK 10.0.300 — so the surface engine owns extraction end to end. One `dotnet tool restore` runs per api scope.

Key resolution is fuzzy and categorical: 7 host aliases, then exact casefold package id, then unique casefold prefix, then unique substring, then unique token-containment; ambiguity returns a typed error listing candidates. So `languageext` resolves `LanguageExt.Core` and `avalonia.datagrid` resolves `Avalonia.Controls.DataGrid`.

API `data` payload (nested under the top-level `Envelope.data`):
- `query`: operation, key, and pattern/type.
- `shape`: `index`, `namespace`, `type`, `member`, or `search`.
- `signature`: resolved type or member signature.
- `doc`: xml-doc prose when a sidecar enriches the shape.
- `source`: key, source kind, selected assembly/XML, package version when relevant, and asset counts.
- `counts`: matches, types, assemblies, bytes, lines, or paths.
- `artifact_paths`: direct paths for `report`, raw streams, `surface.txt`, `decompile.cs`, or `source.preview.cs`.
- `results`: small ranked preview records; no broad result stream is printed by default.
- `preview`: inline bounded source/artifact text when the command owns a direct inspection surface.

[CRITICAL] Unknown API source keys fail with a typed Envelope error. Valid source keys with no symbol/artifact match return `status: empty` and no raw fallback bytes. `api show --full` keeps full text inside `Envelope.data.content`; stdout still contains exactly one Envelope.

Keys:

| [INDEX] | [KEY]               | [ASSEMBLY]                                                | [XML]                                                     |
| :-----: | ------------------- | --------------------------------------------------------- | --------------------------------------------------------- |
|   [1]   | `rhino-common`      | `RhinoCommon.dll`                                         | `RhinoCommon.xml`                                         |
|   [2]   | `rhino-ui`          | `Rhino.UI.dll`                                            | `Rhino.UI.xml`                                            |
|   [3]   | `rhino-code`        | `Rhino.Runtime.Code.dll`                                  | none                                                      |
|   [4]   | `rhino-code-remote` | `Rhino.Runtime.Code.Remote.dll`                           | none                                                      |
|   [5]   | `eto`               | `Eto.dll`                                                 | `Eto.xml`                                                 |
|   [6]   | `gh2`               | `ManagedPlugIns/Grasshopper2Plugin.rhp/Grasshopper2.dll`  | `ManagedPlugIns/Grasshopper2Plugin.rhp/Grasshopper2.xml`  |
|   [7]   | `gh2-io`            | `ManagedPlugIns/Grasshopper2Plugin.rhp/GrasshopperIO.dll` | `ManagedPlugIns/Grasshopper2Plugin.rhp/GrasshopperIO.xml` |

Package keys resolve from `Directory.Packages.props` and restored assets under `.cache/nuget/packages`, with user NuGet cache as a read-only fallback. First package `query` resolves assets lazily and may run an isolated restore probe under the API artifact directory.

Observed gotchas:
- `rhino-ui` declares `Rhino.UI.xml`, but the current RhinoWIP bundle lacks that file; `query` builds an XML-free surface, so it resolves types and members without the sidecar.
- GH2 symbols are namespace-sensitive. `query gh2 Document` ranks `Grasshopper2.Doc.Document`; `Grasshopper.Document` is not the current exact type.
- The surface lists arity-free generic names; pin a generic by passing CLR arity, for example `query <key> Type\`1`.
- Multi-assembly packages stay bounded by the preview cap; compact JSON keeps stdout bounded and stores full files on disk.
- A central package with no owning `.csproj` shows in `api doctor` with empty `owners`; treat such an entry as a pre-consumer pin, not active API surface.

---
## [7][ARTIFACTS_CONCURRENCY]
>**Dictum:** *Artifact isolation replaces static locks.*

<br>

Artifact scope:
- `rail()` opens `.artifacts/quality/<rail>/<run_id>/` and isolated `DOTNET_CLI_HOME`.
- Dotnet build/test/run verbs receive `--artifacts-path`, `--disable-build-servers`, and `MSBUILDDISABLENODEREUSE=1`.
- Streamed process logs live under `.artifacts/quality/<rail>/<run_id>/process/<command-id>/`.
- Test results live under `.artifacts/test/<slice>/<run_id>/`.
- Mutation output lives under `.artifacts/mutation/<slice>/<run_id>/`.
- Verify reports live under `.artifacts/rhino/verify/<run_id>/` and expire by retention.

Concurrency:
- Parallel: `static fix`, `static report`, `static build`, and `test run` with distinct `run_id`.
- Exclusive fail-fast: mutation, live Rhino bridge commands, `bridge verify`, bridge package live steps, and package staging from cleanup through commit.
- Lease files remain stable and are truncated after release; do not delete lock files as stale cleanup.

---
## [8][AGENT_ROUTING]
>**Dictum:** *Route by proof claim, not habit.*

<br>

Use:
- `static fix` before `static build` for C# edits.
- `static report` when mutation is disallowed and format diagnostics are needed.
- `static build [paths...]` for compile/analyzer proof on touched project closure.
- `static full` after solution, central package, global runner, `.editorconfig`, or analyzer changes.
- `static plan [paths...]` before costly proof or when routing looks suspicious.
- `test run --no-build` after successful static build.
- `test run --test-modules "<glob>"` for already-built MTP assemblies.
- `bridge verify <pattern>` for Rhino scenario proof.
- `api query` for host SDK and central package API truth.
- `api doctor` before guessing package or tool availability.
- `api show <artifact-or-symbol>` for bounded follow-up inspection without rerunning broad API queries.

Avoid:
- Treating `dotnet format` as compile/analyzer proof.
- Running `.slnx` builds for ordinary leaf edits.
- Running raw `dotnet test --help` as a harmless probe under MTP.
- Running mutation implicitly on every unit test pass.
- Waiting on locks; busy means choose another proof or retry later.

---
## [9][MAINTENANCE]
>**Dictum:** *Validation follows edited surface.*

<br>

Load `.claude/skills/coding-python/SKILL.md` before Python edits. Dependencies live in root `pyproject.toml`.

Python edits:

```bash
uv run pytest tests/tools/quality/test_quality.py -q
pnpm check:py
```

README or Mermaid edits:

```bash
git diff --check
pnpm exec mmdc -i tools/quality/README.md -a .artifacts/mermaid -q
```

Preflight:

```bash
uv run python -m tools.quality self-test
uv run python -m tools.quality self-test --rhino
```

Dependency currency (central package + tool versions; `-pre Auto` keeps each beta-channel pin on its prerelease line while ignoring previews for stable packages):

```bash
dotnet outdated Workspace.slnx -r -pre Auto
```

Required tools: `dotnet`, `fd`, `git`, `rg`. Local-manifest tools (`ilspycmd`, `dotnet-stryker`, `dotnet-outdated`) live in `.config/dotnet-tools.json` and are invoked via `dotnet tool run`/`dotnet outdated`; `ilspycmd` health is checked live by `api doctor`, not a PATH preflight. Required files: `Workspace.slnx`, default test csproj, and `.config/dotnet-tools.json`. Rhino preflight also checks executable `Contents/Resources/bin/yak`.
