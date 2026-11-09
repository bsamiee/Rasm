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
- Live `.config/` holds `mise.toml` and `mise.lock` (node, pnpm, git-lfs, and the agent CLI tools, from `mise use`), plus the original `tsconfig.base.json` and `tsconfig.web.json` every TypeScript project extends
- Root holds `package.json` (`pnpm init`), `pnpm-workspace.yaml` (catalog, `catalogMode: strict`), `nx.json` (`nx init`), the original `tsconfig.json`, and empty central package slots: `Directory.Build.props`, `Directory.Build.targets`, `Directory.Packages.props`, `global.json`, `Cargo.toml`, `rust-toolchain.toml`, `pyproject.toml`
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
- One verb set for every language and host, so an agent learns one way: `build`, `test`, `lint`, `format`, `check` on every project; `install` on every app and host project (macOS apps, Rhino and Grasshopper 2, Adobe, Blender, Unreal); `upgrade` on every ecosystem root; `nx release` for shipping
  - A distributable is `build` in its release configuration (Nx `configurations`, the mode mechanism); shipping is `nx release` running each project's inferred `nx-release-publish`; no `archive`, `pack`, or `publish` verb per host
  - `install` puts the current local build into its host through the channel the host itself installs from, with no version, tag, or release: the same path a user's install takes, fed from the local build. Never a script imitating the host (the old macOS `install` quit the app, booted out its agents with `launchctl`, and copied it into `/Applications` with `ditto`)
  - `upgrade` moves an ecosystem's dependencies to their newest releases through its own tool (`pnpm`, `cargo`, `uv`, `dotnet`, `swift package`, Xcode resolution), writing its catalog and lock
  - `eng/` existed for exactly these two, local install and upgrades across languages, as one-off programs mixed with release; each returns as a verb its plugin infers, never as a program

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
- [ ] Release and publish per host (methodology below), CI, `infra/`
- [ ] Harness plugins and marketplace

Release methodology, one system for every language:
- `nx release` alone versions, writes changelogs, creates GitHub releases, and publishes; no separate release program (the old `eng/typescript/release.ts` wrapped `releaseVersion`/`releaseChangelog` and restated checks `nx release` already makes)
- `nx.json` holds the shared policy alone: projects released independently, conventional commits, GitHub release creation, tag pattern, groups chosen by project, never by tag strings (`tag:output:nupkg`)
- Each language plugin supplies its ecosystem's two pieces, the pattern the official plugins use (`docs/research/nx/plugin-authoring.md` [04]):
  1. A `VersionActions` module writing the version where the ecosystem keeps it: Apple `MARKETING_VERSION` and `CURRENT_PROJECT_VERSION` in `project.xcproj` through `XcodeProjectFormat`, Rust `Cargo.toml`, .NET `<Version>`, Python `pyproject.toml`; a Swift package has no version field, so tags carry it. The old `@rasm/nx/release` reported `0.0.0` and wrote nothing, with versions in MinVer shell strings
  2. An inferred `nx-release-publish` target per project for its ecosystem's registry: NuGet, crates.io, PyPI, npm. Apple apps build their distributable through Xcode's own steps, `xcodebuild archive` then `-exportArchive` when the project holds export options; the old defect was copying the `.app` out of the archive with `ditto` and zipping it, never the archive itself
- Distribution past a registry (files on a GitHub release, Sparkle appcast, Rhino Yak, the Blender extension index, Adobe plugin packages) is one distribution plugin reading the artifacts each language plugin declares as outputs, so every host distributes the same way; it owns both directions of each channel, publishing a release to it and `install` pulling a local build through it, researched per host (each host's own repository, feed, or installer and whether it takes a local source) before code; no host's channel lives in shell strings or in `nx.json` (the old `publish` rows chained `doppler`, `gh release upload`, `generate_appcast`, `dotnet nuget push`, and Blender commands per tag filter)

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
- [x] Inference over `Package.swift` (`swift package describe`), `project.xcproj`, and `contents.xcworkspacedata` (core, 20 ms for both apps); cold graph 0.80 s with the daemon start, plugin-cache hit 0.63 s, cached graph 0.17 s
- [x] `build` and `test` once per package with module aliases (per-module SwiftPM runs measured 30–35 s waiting on the scratch lock, one package build 4.4 s), `lint`, `format`, `check` per module, builds uncached
- [x] Shared `.xcscheme` files read by the core, entries checked by `BlueprintIdentifier`, `test` per test plan with `-testPlan`, plans as inputs; workspaces as projects, a project one workspace lists building through it against the workspace lock
- [x] Each target's `INFOPLIST_FILE`, `CODE_SIGN_ENTITLEMENTS`, xcconfig per configuration with its `#include` chain read through the core as inputs; a missing file, stale scheme entry, or unlocatable configuration file fails that project alone (`AggregateCreateNodesError`); `$(…)` values evaluate through `-showBuildSettings -json -alltargets`
- [x] Edges keyed by package path: module to module, package to modules and local packages, Xcode to package modules through each package's `products`, cross-project through remote targets, imported products, workspace members; mapped to final project names by root in `createDependencies`, which reads the report `createNodes` built
- [x] Options `outputPath` (relative, absolute, `~`; `<outputPath>/<root>/{swiftpm,xcode,results,describe}`), `trustPackages`; resolution locked to `Package.resolved` under CI through Nx's `isCI()`, no option; `describe` on its own scratch path; graph hash over the files inference reads, the options, the plugin, and the core
- [x] `run` executor removing every `NX_*` and `LERNA_PACKAGE_NAME` from the child environment (walked through Effect's env `ConfigProvider`), clearing result files and creating their folders, forwarding `--` arguments: no-op root package build 3.76–6.27 s before, 2.45–2.58 s after (manifests load from SwiftPM's db cache), 1.95–2.12 s direct
- [x] Package-only schemes built with `-destination generic/platform=<name>` from `describe` platforms or the host triple (both measured to build); a generic destination cannot run tests, which `swift test` covers
- [x] Relay and Atelier: per-app `Info.plist` and `<App>.entitlements` beside each project (the files Xcode's Info and Signing & Capabilities editors write per target), `SPARKLE_PUBLIC_KEY` once in `Xcode.xcconfig`, Info.plist excluded from each synchronized folder through `xcrun xcodeproj`; both build and sign end to end (team BBXB9R367P, hardened-process entitlements and expanded Sparkle keys in each bundle); `bootstrap-icons` back as a catalog row for Relay's asset symlinks
- [x] Full Swift set (`build lint check`, 15 projects) from clean state: cold 36.5 s, warm 9.5 s with 15 of 45 tasks cached (lints; builds incremental by design)
- [x] Decided against one `.xcworkspace` over both apps and the root package: shared derived data saved 7 s on the second app cold (13 s against 20 s) and 0.6 GB, but with the root package as a member Xcode wrote the workspace resolution into the root `Package.resolved`, adding Sparkle and swift-http-types and dropping the package's own pins, which conflicts with SwiftPM resolving the root package

Open:
- [ ] Adversarial review per [08], and `apple-toolchain.md` folded into `apple/tools.md` with the facts measured here (generic destinations, `--xunit-output` needing its folder, workspace resolution writing a member package's lock)
- [ ] Swift test targets: the root `Package.swift` holds their rows ahead of them, `swift-custom-dump` (`expectNoDifference` value diffs), `swift-issue-reporting` (the manifest loop adds `IssueReportingTestSupport` to every test target, so a `reportIssue` inside a library fails the running test), `swift-property-based`; until the first test target, SwiftPM prints `dependency 'swift-issue-reporting' is not used by any target` on every root build, a warning with no switch (`ModulesGraph+Loading.swift` 6.4.2 skips only packages declaring traits, executables, or command plugins), owner-accepted
- [ ] A workspace over the two apps alone (no root package member) is unmeasured
- [ ] Release version actions writing `MARKETING_VERSION` and `CURRENT_PROJECT_VERSION` through `XcodeProjectFormat`, and `archive`/export, removed from this pass, returning with release
- [ ] `$(…)` setting values evaluate at the default configuration alone; SwiftLint stays out until a repository holds `.swiftlint.yml`
- [ ] Generators, fixture tests, prebuilt core in the published package, npm publish
