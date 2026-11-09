# [TODO2]

`CLAUDE.md` imports this file. It states why the repository's tooling was torn down, what remains, where the evidence sits, and how each language comes back. Sessions read it whole before any tooling, project, or plugin work.

## [01]-[STATE]

The repository's Nx setup had grown one target at a time with no owner per ecosystem:
- `tools/nx/` was one plugin for every host: markers for csproj, `.cs` scripts, Cargo, Blender, pyproject, tsconfig, Storybook, Vitest, Vite, Cloudflare, UXP and CEP, fonts, SEA, Claude hooks, and Xcode, plus root checks, sync generators, and release version actions
- `nx.json` held ~30 root `lint:*`/`typecheck:*`/`upgrade:*` shell targets and multi-line `build`/`pack`/`install`/`publish` strings per host in `targetDefaults`
- `eng/` held one-off programs (release, upgrades, Blender packing, Adobe installs) and Rhino MSBuild host targets
- mise ran MCP servers as daemons, hk ran every checker as a git hook, and a sync generator copied one fact into eight files

It failed because it imagined each ecosystem instead of reading it. Swift modules came from a hand decode of `project.xcproj`, per-app facts sat in shell strings, and layouts were guessed where SwiftPM, Xcode, MSBuild, Cargo, and uv already state them. Every new feature landed as another row in the nearest layer, and performance suffered from duplicated and serialized work. The fix is to remove it all, accept a red tree, and rebuild each language on a focused plugin and corrected projects.

## [02]-[CLEANUP]

- Commit `c64a8b424` holds the full tree before the rebuild, the reference for any revert; `e8fbdeb92` and `e8084616d` unplugged it
- `.tmp/` (ignored) holds every moved file for reference, never binding: root files and `README.md`, the old `.config/`, `.cargo/`, `.github/`, `.playwright/`, `eng/`, `tools/nx/`, `plugins/playwright/`, the old `nx.json`
- Project manifests under `apps/`, `libs/`, `tests/`, `tools/`, `plugins/`, `infra/` are unplugged in place as `z.<file>`
- Live `.config/` holds `mise.toml` and `mise.lock` alone (node, pnpm, git-lfs, and the agent CLI tools, from `mise use`)
- Root holds `package.json` (`pnpm init`), `pnpm-workspace.yaml` (catalog, `catalogMode: strict`), `nx.json` (`nx init`), `tsconfig.base.json`, and empty central package slots: `Directory.Build.props`, `Directory.Build.targets`, `Directory.Packages.props`, `global.json`, `Cargo.toml`, `rust-toolchain.toml`, `pyproject.toml`
- Swift is back: `Package.swift`, `.swift-format`, `Xcode.xcconfig`, both `project.xcproj` and their `Package.resolved`, and the `apps/nx/apple` plugin
- The harness plugin marketplace, the Codex Bash hook, and hk's git hook are removed; the `manage-repo` skill sits in `.claude/skills/archive/`
- `plugins/function-hooks/package.json` is a session-only file outside git, deleted after a Claude Code restart

## [03]-[SOURCES]

| [INDEX] | [PLACE]                                  | [HOLDS]                                                                                     |
| :-----: | :--------------------------------------- | :------------------------------------------------------------------------------------------ |
|  [01]   | `docs/research/nx/plugin-authoring.md`    | How official Nx plugins are built, the shape every plugin copies                            |
|  [02]   | `docs/research/nx/prior-art.md`           | megamono's plugins, `@nx/dotnet`, why `tools/nx` grew as it did                             |
|  [03]   | `docs/research/nx/apple/`                 | `files.md` (every Apple file and its reader), `tools.md` (how each tool runs), `history.md` (old Swift setup, its lessons) |
|  [04]   | `docs/research/nx/apple-toolchain.md`     | Measured `swift` and `xcodebuild` JSON sources                                              |
|  [05]   | `.tmp/`, `git show c64a8b424:<path>`      | What each old file did                                                                      |
|  [06]   | `.claude/skills/archive/manage-repo/`     | Hard-won tool facts from the old setup, read critically                                     |

## [04]-[QUALITY]

- Code meets `CLAUDE.md` and every coding skill whatever linters, checkers, or hooks are off: functional and expression-oriented, typed errors, sections and dividers, no comments, no hand-rolling a package or host provides
- Old files are reference for what a capability did, never for how code is written or where a project lives
- Every tool, language, format, field, and schema takes its newest release, no legacy form
- Files start from the owning tool's generator (`mise use`, `pnpm init`, `nx init`, `nx add`, `swift package init`, `cargo init`, `uv init`, `dotnet new`, `xcrun xcodeproj`), edited only where a fact differs
- Each language returns through its central package manager: pnpm catalog named by `catalog:`, `Directory.Packages.props`, `[workspace.dependencies]`, root `Package.swift` `dependencies`, uv workspace members
- Plugins are TypeScript where Nx calls them, with a native core where analysis needs one: Rust through napi-rs by default, the host's language where its API is native (MSBuild for .NET, `XcodeProjectFormat` for Xcode)
- Plugins are professional packages: minimal code, tested, documented, versioned, published once complete
- Everything returning, root and project-wide (project files, config files, tool files, scripts, `nx.json`), is checked against its tool's current docs; what is wrong (bad commands, scripts, fields, values, misplaced or duplicate files) is fixed, what is right stays as is
- Build and state output lands where its tool or a plugin option puts it, never scattered: stale folders from the old setup are removed, and a run through Nx proves every output path
- Every plugin passes an adversarial review of its code and architecture before it counts as done:
  - Structure and folders follow official Nx plugins; files, folders, members, fields, and parameters take the language's and Nx's naming conventions, no coined terms
  - No stringly logic or hardcoding: kinds, flags, and tool tokens are typed values or values a tool states, never literals restating them
  - Code is reduced and flat: near-identical objects fold into one higher-order form, parameters replace families, no helper with one reader
  - External packages are used fully and directly, the language's core packages included, nothing restated by hand
  - Nx integration follows the Nx docs for inputs, outputs, caching, metadata, options, and errors; messages follow Nx conventions and exist only where a user acts on them
  - Prose passes `clean-prose`
- Known problems and findings for a plugin sit beside its research (`docs/research/nx/apple/known-problems.md`), read by every coding and review pass

## [05]-[PLUGINS]

Every plugin starts from the ecosystem, never from this repository:
- Ecosystem first: every manifest, project, workspace, lock, and config file the toolchain reads, and the command printing each as data, is mapped under `docs/research/nx/<plugin>/` before code
- Prior art first: the official Nx plugin and the best community plugin set the model, ours changes only what quality, language, or performance demands
- Facts come from the toolchain's own output or its owner's decoder, never from file text, a glob convention, or a name
- The graph follows the tool's unit of work: projects where the tool has a unit, edges from declared dependencies, tasks at the level the tool builds and locks
- Options state a principle once (where outputs go, whether package macros are trusted), the plugin derives each tool's form, tool defaults when unset, never a per-tool or per-project field
- Values a project states (build settings, referenced files, schemes, platforms) are read from it and become task inputs, never restated in options
- Nx builds equal direct tool builds: nothing injected that the native tool or IDE alone would miss
- Performance lives in the code (one tool call per workspace, cached by a hash of inputs, options, and the plugin source) and in the graph (correct inputs and outputs, incremental builds uncached, no duplicated work), settled by measurement
- Each plugin works unchanged in an unrelated repository
- `nx.json` ends as one row per plugin with a few principle options; projects expose one `build`, `test`, `lint`, `format`, `check` each

## [06]-[RETURN]

Each language returns whole, on both ends, never anchored to the old layout of its projects, packages, folders, libs, or apps:
1. Research its ecosystem and prior art into `docs/research/nx/<plugin>/`
2. Correct its projects to newest standards: restructure, rename, remove, or add manifests, configs, and files where the old ones are wrong, through the tools' own writers; a `z.<file>` returns rewritten, not renamed back
3. Build its plugin from the research, assuming nothing the tools do not state
4. Add one `nx.json` row, then measure the graph, `build`, `test`, `lint`, and `check` on real projects
5. Pass an adversarial review for quality, nonsense, and misunderstood tooling, then mark it done here

## [07]-[RESEARCH]

`docs/research/` is git-ignored: local, durable, never pushed. Its `nx/` folder holds:
- `README.md`, the index of every sheet
- Shared sheets: `plugin-authoring.md` (Nx plugin contracts and official-plugin patterns), `prior-art.md` (other plugins, why `tools/nx` failed)
- One folder per plugin, `nx/<plugin>/`, holding `files.md`, `tools.md`, `history.md`, and `known-problems.md`
- Sheets are distilled rules, each fact with its source; a changed fact is edited in place, never appended

Research for a new plugin reaches this depth before any code:
- `files.md`: every manifest, project, workspace, lock, configuration, and toolchain-selection file the ecosystem's newest tools read, the tool reading it, the command printing it as data, and whether it is a marker, an input, or ignored; monorepo cases (nesting, several roots, workspace grouping, shared or per-project locks)
- `tools.md`: how each tool runs, measured in scratch copies: output and state paths and their precedence (per project or per user), locks, what invalidates incremental state (environment, flags), parallelism, trust and validation gates, test result formats, format and lint configuration discovery, performance levers; which tool processes the plugin calls instead of restating in code
- `history.md`: what the old setup did for the ecosystem, read from `.tmp/` (root files, `.config/`, `tools/nx/`, `eng/`), the `z.<file>` project manifests and project configs, `.claude/skills/archive/manage-repo/`, and git history from `c64a8b424` back; each capability judged real, app-owned, or junk, with why it went wrong
- Prior art: the official Nx plugin and the top community plugins for the ecosystem, read at source
- `known-problems.md`: defects and findings gathered during the work, read by every coding and review pass
- Research never writes into the tree; anything unverified is marked

## [08]-[AGENTS]

Briefs state the goal, files, sources, and constraints, never the expected answer:
- Coding agents read `CLAUDE.md`, this file, the coding skills of their languages (`typescript-coding`, `typescript-coding-effect`, `swift-coding`, `rust-coding`, `dotnet-coding`, `python-coding`), `refactor-code`, `clean-prose` for prose, and the plugin's research folder; every behavior traces to a sheet or a primary source; they edit only named files, never commit, and verify live with measurements
- The adversarial agent starts fresh with the code, the research folder, and `known-problems.md`, never the coding agent's report or conclusions; it reads the same standards plus the Nx docs and official plugin sources itself, judges every demand in [04]-[QUALITY] and [05]-[PLUGINS], and also judges tool understanding (file types, tool processes restated in code, legacy forms), portability to an unrelated repository, and performance by measurement
- The adversarial agent fixes what it proves in place, reruns the verification, and returns what it changed, with evidence, and what needs an owner decision

## [09]-[ORDER]

- [x] Teardown, minimal mise, root, language slots, manifests unplugged
- [ ] Configuration deep dive: the top Nx plugins and their option surfaces, `nx.json` fields in the current Nx docs, the plugin-building docs, and packages for plugin development (`@nx/plugin`, `@nx/plugin/testing`, `@nx/devkit` testing utilities) we should use, recorded under `docs/research/nx/` before the next plugin
- [ ] MCP servers run directly from their own packages and `.mcp.json` rows, no mise daemon or bridge between
- [ ] Swift: `apps/nx/apple` (section 09)
- [ ] Rust: `apps/nx/cargo`, shape from megamono `packages/nx-cargo`, cargo's incremental build and nextest kept, inference from one `cargo metadata`
- [ ] .NET: fork `@nx/dotnet` (TypeScript client plus `MsbuildAnalyzer`) into `apps/nx/dotnet`, adding Rhino host resolution, Yak, and Windows builds the official plugin lacks
- [ ] Python: `apps/nx/uv` over one uv workspace, Blender interpreter kept apart
- [ ] TypeScript: official `@nx/js`, `@nx/vite`, `@nx/vitest`
- [ ] Playwright rethought as a tool
- [ ] Release and publish per host, CI, `infra/`
- [ ] Harness plugins and marketplace

## [10]-[CATALOG]

What the moved files did, each capability rebuilt by the language that owns it, or dropped:

| [INDEX] | [AREA]          | [FILES]                                                                                        | [CAPABILITY]                                                                                                     |
| :-----: | :-------------- | :--------------------------------------------------------------------------------------------- | :--------------------------------------------------------------------------------------------------------------- |
|  [01]   | Task graph      | `nx.json`, `.nxignore`, `tools/nx/`                                                            | Official plugins plus `@rasm/nx`, root shell targets, release groups, `targetDefaults` per host                  |
|  [02]   | Sync generators | `tools/nx/sync/`                                                                               | `Directory.Build.props` identity and hk URLs, Cloudflare hosting bindings, ast-grep injection utils               |
|  [03]   | Programs        | `eng/typescript/`, `eng/python/`, `eng/dotnet/`, `eng/rust/`, `eng/scripts/`                   | Release and its commit scope check, upgrades, Adobe installs, Blender assets and packing, Rhino host targets, Illustrator MCP token |
|  [04]   | JavaScript      | `package.json`, `pnpm-workspace.yaml`, `pnpm-lock.yaml`, `tsconfig.json`, `vitest.config.ts`, `biome.json` | Workspace globs, catalog, root dev tools, root TypeScript project, Vitest projects, Biome             |
|  [05]   | Rust            | `Cargo.toml`, `Cargo.lock`, `rust-toolchain.toml`, `.cargo/`, Clippy, rustfmt, nextest        | Workspace, dependency catalog, lints, nightly toolchain, build dirs, cargo-deny, test runner                     |
|  [06]   | Python          | `pyproject.toml`, `uv.lock`, `pixi.lock`, `stubtest.ini`                                       | uv workspace and groups, Ruff, ty, mypy, pytest, Pixi native libraries                                          |
|  [07]   | .NET            | `NuGet.config`, `Workspace.slnx`, `Directory.Build.*`, `Directory.Packages.props`, `analyzers.globalconfig` | Feeds, solution, build defaults, Rhino host targets, central versions, analyzer levels              |
|  [08]   | Swift           | `swiftlint.yml`, `Sparkle-Info.plist`, `Enhanced.entitlements`                                 | Lint rules, Sparkle updater keys, Enhanced Security entitlements                                                |
|  [09]   | Checkers        | `hk.pkl`, `shellcheckrc`, `yamllint.yaml`, `pmd.xml`, `squawk.toml`                            | Git hooks and every file checker and writer                                                                      |
|  [10]   | Environment     | old `.config/mise*.toml`, `miserc.toml`                                                        | Tool rows, process env, MCP daemons, `update` and `clean` tasks                                                  |
|  [11]   | CI and harness  | `.github/`, `plugins/` marketplace rows, Codex hook, `.playwright/`                            | Check matrix, command correction, agent LSP and MCP, Playwright skills                                           |

## [11]-[NX_APPLE]

`apps/nx/apple/` (`@rasm/nx-apple`): a TypeScript layer over `@nx/devkit` and Effect 4, and a Swift core reading `project.xcproj` through `XcodeProjectFormat`.

Built and measured:
- [x] Inference over `Package.swift` (`swift package describe`) and `project.xcproj` (core, 5 ms for both apps against 5–6 s per `xcodebuild -list`), cached graph 0.19 s, cold 0.5 s
- [x] `build` and `test` once per package with module aliases (per-module SwiftPM runs measured 30–35 s waiting on the scratch lock, one package build 4.4 s), `lint`, `format`, `check` per module, `archive` for apps, builds uncached
- [x] `outputPath` option placing each tool's output in its own folder, tool defaults when unset

Now: a coding agent rebuilds the plugin and the Swift projects from `docs/research/nx/apple/`; next, an adversarial review per [08], then a cold and warm run of every Swift project through Nx confirming caching, output paths, and every option, and `apple-toolchain.md` folded into `apple/tools.md`.

Open, from `docs/research/nx/apple/`:
- [ ] Shared `.xcscheme` files mapped to targets by `BlueprintIdentifier`, test plans as inputs, workspaces (`.xcworkspace`) and their locks modelled
- [ ] Each target's `INFOPLIST_FILE`, `CODE_SIGN_ENTITLEMENTS`, and xcconfig read through the core as inputs, a missing file failing with target, setting, and path
- [ ] Edges keyed by package path, package-to-package edges from `describe`, cross-project target edges
- [ ] Package macro and plugin trust as one option, `--no-color-diagnostics`, resolution flags, `describe` off the build's scratch lock, a narrower graph hash
- [ ] Relay and Atelier corrected to newest standards: the Info.plist and entitlements files they reference are back under the apps' ownership, both build end to end
- [ ] Release version actions writing `MARKETING_VERSION` and `CURRENT_PROJECT_VERSION` through `XcodeProjectFormat`
- [ ] Generators, fixture tests, prebuilt core in the published package, npm publish
