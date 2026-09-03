<!-- Source for monorepo-build-infrastructure [03]-[CONFIGURATION] and [07]-[CORRECTIONS], nothing integrated yet -->
# Polyglot monorepo organization on GitHub, 2025-2026

Fifteen polyglot monorepos, read at their default branches on 2026-09-03, show how they organize tooling, configuration, entry points, and shared build infrastructure against the workspace rules: root config owns settings, no `src/` directories, caches under `.cache/`, outputs under `.artifacts/`, one entry point per kind of work, and central versions.

## [00]-[RASM_BASELINE]

From `README.md`, `CLAUDE.md`, and the root files:

- Root manifests own versions: `Directory.Packages.props` (.NET), `pyproject.toml` with `uv.lock` (Python), `pnpm-workspace.yaml` (TypeScript)
- Nx is the task runner, and `nx.json` sets `"cacheDirectory": ".cache/nx/cache"` and `"neverConnectToCloud": true`
- Plugins in `nx.json`: `@nx/vite/plugin`, `@nx/vitest`, `@nx/dotnet` (with `exclude: ["eng/native/**"]`), and the local inference plugin `./tools/nx/native-packaging.ts`
- `eng/` holds shared build and release automation, `eng/scripts/` holds the Python automation Nx targets invoke (`gen_gmsh_bindings.py`, `provision.py`, `stage.py`), and `eng/native/` holds the native packaging projects
- `tools/` holds workspace-authored tooling: `tools/biome/*.grit` (thirteen GritQL lint rules), `tools/dotnet/Rasm.Policy.Analyzers` (a Roslyn analyzer project), `tools/nx/native-packaging.ts` (the Nx inference plugin)
- Root config files present: `.editorconfig`, `biome.json`, `Directory.Build.props`, `Directory.Build.targets`, `Directory.Packages.props`, `global.json`, `NuGet.config`, `nx.json`, `package.json`, `pnpm-workspace.yaml`, `pyproject.toml`, `stryker.config.json`, `stryker-config.json`, `tsconfig.base.json`, `tsconfig.json`, `vite.config.ts`, `vitest.config.ts`, `Workspace.slnx`, `.config/dotnet-tools.json`, `.mcp.json`
- `nx.json` `sharedGlobals` lists eleven root files (`global.json`, `NuGet.config`, `biome.json`, `package.json`, `pnpm-lock.yaml`, `pnpm-workspace.yaml`, `stryker.config.json`, `tsconfig.base.json`, `tsconfig.json`, `vite.config.ts`, `vitest.config.ts`) and omits `Directory.Build.props`, `Directory.Build.targets`, `Directory.Packages.props`, `pyproject.toml`, and `uv.lock`
- Toolchain versions: `global.json` pins the .NET SDK exactly (`"version": "10.0.400"`, `"rollForward": "disable"`) and sets `"test": { "runner": "Microsoft.Testing.Platform" }`. `package.json` has no `packageManager` field and no `scripts` field, and declares `"engines": { "node": ">=24.15.0" }` and `"devEngines": { "packageManager": { "name": "pnpm", "version": ">=11.9.0 <12", "onFail": "error" } }`, both floors
- `pnpm-workspace.yaml` routes both pnpm directories under `.cache/` (`cacheDir: .cache/pnpm/cache`, `storeDir: .cache/pnpm/store`), and sets `minimumReleaseAge: 1440`, `catalogMode: strict`, and `saveExact: true`
- No `.env`, `.env.example`, `.nvmrc`, `.python-version`, or `mise.toml` exists at the root
- `Directory.Build.props` sets `ArtifactsPath` to `.artifacts/dotnet`, and `tests/README.md` records that Microsoft.Testing.Platform results land beside the test app under `.artifacts/dotnet/bin`
- `eng/scripts/gen_gmsh_bindings.py` writes `GmshNative.g.cs` and `Gmsh.g.cs` into the staged `managed/` directory under `.artifacts/native/gmsh/stage`, called by `stage.py`, and the output is not committed

## [01]-[METHOD]

Candidates were found with GitHub code search over `nx.json`, `mise.toml`, `pyproject.toml`, and `pnpm-workspace.yaml`, then checked for commit activity, tooling recency, and structure. Archived repositories, templates, starter kits, and repositories without recent activity were rejected.

Fifteen repositories are accepted. Rejections, with reasons:

- `nrwl/nx` `examples/*/nx.json` and `nrwl/nx-examples`: example fixtures inside a tool's own repository
- `lucasvieirasilva/python-poetry-monorepo`, `daotl/web-monorepo-starter`, `diamirio/backend-nx-skeleton`, `sunduq-ai/usta-cli` `templates/nx-monorepo/`: templates and starter kits
- `siemens/element`: active, but its `mise.toml` is three lines, `node = "24"`, `pnpm = "11"`, `uv = "0.12"`, read 2026-09-03
- `bsamiee/Rasm` and `bsamiee/Parametric_Portal`: the owner's own repositories
- Most `filename:mise.toml` and `filename:justfile` matches with `uv` and `pnpm`: single-author repositories with no 2026 activity, not opened

Every count reflects a file read across the fifteen accepted repositories.

## [02]-[ACCEPTED_REPOSITORIES]

### [02.1]-[SAGE_MONOREPO]

<https://github.com/Sage-Bionetworks/sage-monorepo>, an Nx workspace over TypeScript/Angular, Java/Spring Boot with Gradle, Python with uv, and R. Newest commit on `main` 2026-09-01 (`579dd70`, "feat(model-ad): port search to Java with marmoset support"). Accepted under group (b), a pnpm workspace with a uv workspace in one tree. It declares `nxCloudId` in `nx.json`, it does not satisfy group (a), and nothing about the cloud service is described.

Root config files: `.editorconfig`, `.eslintignore`, `.eslintrc.json`, `.hadolint.yaml`, `.prettierignore`, `.prettierrc.yml`, `.stylelintignore`, `babel.config.json`, `build.gradle.kts`, `checkstyle.xml`, `checkstyle-suppressions.xml`, `gradle.properties`, `jest.config.ts`, `jest.preset.js`, `lint-staged.config.js`, `mkdocs.yml`, `nx.json`, `package.json`, `pnpm-workspace.yaml`, `pnpm-lock.yaml`, `pyproject.toml`, `uv.lock`, `redocly.yaml`, `settings.gradle.kts`, `stylelint.config.mjs`, `tsconfig.base.json`, `skills-lock.json`, with `dev-env.sh` and `gradlew`.

Three language ecosystems, one config per checker at the root: `.eslintrc.json` for TypeScript, `checkstyle.xml` for Java, `[tool.ruff]` inside the root `pyproject.toml`, `stylelint.config.mjs` for SCSS, `.prettierrc.yml` for formatting. <https://github.com/Sage-Bionetworks/sage-monorepo/blob/main/pyproject.toml> holds `[tool.ruff]`, `[tool.ruff.lint]`, `[tool.sqlfluff.core]`, `[tool.uv]`, `[tool.uv.workspace]`, `[dependency-groups]`, and `[project]` in one file, Python tool settings, workspace membership, and dependency groups are not split apart.

`nx.json` (<https://github.com/Sage-Bionetworks/sage-monorepo/blob/main/nx.json>) declares three plugins and no more:

```json
"plugins": [
  { "plugin": "@nxlv/python", "options": { "packageManager": "uv" } },
  { "plugin": "@nx/gradle" },
  { "plugin": "@sagebionetworks/sage-monorepo-nx-plugin", "options": { "buildImageTargetName": "build-image" } }
],
"useInferencePlugins": false
```

`useInferencePlugins: false` turns off automatic plugin registration while the three listed plugins still infer targets, inference is opted into plugin by plugin.

Non-JavaScript targets come from those plugins, and `targetDefaults` keys on the executor name rather than on a target name for them:

```json
"@nxlv/python:build":        { "cache": true, "dependsOn": ["^build"], "inputs": ["productionPython", "^productionPython"] },
"@nxlv/python:run-commands": { "cache": true, "inputs": ["defaultPython", "^productionPython"] },
"@nxlv/python:ruff-check":   { "cache": true, "inputs": ["defaultPython"] },
"@nx/gradle:gradle":         { "cache": true, "inputs": ["defaultGradle", "^productionGradle"], "options": { "excludeDependsOn": false } }
```

Per-language `namedInputs` make cross-language caching correct. There are three pairs rather than one `default`/`production` pair:

```json
"defaultPython": ["{projectRoot}/**/*", "{workspaceRoot}/pyproject.toml", "{workspaceRoot}/uv.lock"],
"productionPython": ["defaultPython"],
"defaultGradle": ["{projectRoot}/**/*", "{workspaceRoot}/gradle/libs.versions.toml",
                  "{workspaceRoot}/gradle/wrapper/**/*", "{workspaceRoot}/buildSrc/**/*",
                  "{workspaceRoot}/build.gradle.kts", "{workspaceRoot}/settings.gradle.kts"],
"productionGradle": ["defaultGradle"]
```

Each language's input set names that language's root manifests, a change to `uv.lock` invalidates Python tasks and a change to `gradle/libs.versions.toml` invalidates Gradle tasks, without either invalidating the other. Rasm's flat `sharedGlobals` set makes any of its eleven files invalidate every cached task.

Central versions per language, each in one file: `package.json` pins every npm dependency to an exact version with no ranges (`"@nx/angular": "21.5.3"`, `"typescript": "5.9.2"`), `gradle/libs.versions.toml` is named in `CLAUDE.md` as "single source of truth for all Java dependency versions", and the root `pyproject.toml` `[tool.uv] constraint-dependencies` pins shared Python bounds across all workspace members.

`tools/` (<https://github.com/Sage-Bionetworks/sage-monorepo/tree/main/tools>) holds workspace scripts and custom Nx executors, among them `executors/`, `nx-util.js`, `git-util.js`, `coverage-merger.js`, `generate-sitemap.js`, `generate-svg-icon-registry.js`, `prepare-java-envs.js`, `prepare-nodejs-envs.js`, `prepare-python-envs.js`, `prepare-r-envs.js`, `setup-projects.sh`, `workspace-install.sh`, `workspace-nuke.sh`, `tsconfig.tools.json`. One `prepare-*-envs.js` per language, and one `workspace-install.sh` / `workspace-nuke.sh` pair for the whole workspace.

Package scripts in `package.json` are nine entries, five of which are `nx affected:*` pass-throughs and two of which are husky hooks. Nothing wraps `nx build` or `nx test` in an npm alias.

Documented commands sit in `CLAUDE.md` (<https://github.com/Sage-Bionetworks/sage-monorepo/blob/main/CLAUDE.md>) under "Common Commands", one invocation per language: `nx test|lint|build [project]` and `nx affected --target=test` for TypeScript, `./gradlew :project-name:test --no-daemon --console=plain` for Java, and `uv sync` / `uv run pytest` / `uv run ruff check` for Python.

Per-project config is not avoided: `CLAUDE.md` states "Each project has a `project.json` defining targets (build, test, lint, serve, integration-test) and tags". Project-level `project.json` files coexist with plugin inference.

`src/` directories exist (the `production` named input references `{projectRoot}/src/test-setup.[jt]s` and `{projectRoot}/src/test/**/*`), the repository does not match Rasm's no-`src/` rule.

### [02.2]-[PULUMI]

<https://github.com/pulumi/pulumi>, Go, TypeScript/Node, Python, .NET, Java, and PCL in one tree. Newest commit on `master` 2026-09-03. Accepted under group (c), the toolchain is pinned in the repository and the entry point is unified at the root.

Root config files: `.mise.toml`, `Makefile`, `.golangci.yml`, `.custom-gcl.yml`, `.changie.yaml`, `.goreleaser.yml`, `.vale.ini`, `.envrc.template`, `.yarnrc`, `codecov.yml`, `renovate.json5`, `AGENTS.md`, and `CLAUDE.md` as a symlink to `AGENTS.md` (<https://github.com/pulumi/pulumi/blob/master/CLAUDE.md>, type `symlink`, target `AGENTS.md`). Rasm uses the same symlink pattern for `AGENTS.md` → `CLAUDE.md`.

Tool version pinning is in one file. `.mise.toml` (<https://github.com/pulumi/pulumi/blob/master/.mise.toml>) pins the language runtimes and every CLI the build shells out to, among them:

```toml
[tools]
go = "1.27"
node = "22"
python = '3.11'
uv = "0.11.28"
dotnet = "8"
java = "temurin-11"
gradle = "7.6"
bun = "1.3.14"
protoc = "29.5"
github-cli = "2.93.0"
golangci-lint = "2.13.0"
changie = "1.24.2"
vale = "3.17.1"
jq = "1.8.1"
"npm:pnpm" = "11"
"npm:typescript" = "6.0.3"
"go:golang.org/x/tools/gopls" = "0.23.0"
"go:google.golang.org/protobuf/cmd/protoc-gen-go" = "v1.36.6"
"github:WebAssembly/wabt" = "1.0.37"
```

mise's backend prefixes (`npm:`, `go:`, `github:`, `asdf:`) let one manifest pin tools that have no native mise plugin, including code generators and language servers. `[env]` in the same file sets `PULUMI_TEST_USE_NPM = "true"`, environment defaults sit with the tool pins rather than in a separate `.env`. Platform-conditional pins are inline: `"go:github.com/mitranim/gow" = { version = "...", os = ["linux", "macos"] }`. A `[tool_alias]` and `[plugins]` table point `protoc` and `gradle` at asdf plugins.

One entry point per kind of work, written down as a list. `AGENTS.md` has a section titled "Command canon" (<https://github.com/pulumi/pulumi/blob/master/AGENTS.md>), stating "All commands assume you're at the repo root", then naming one command per job: `make build`, `make lint`, `make lint_fix`, `make format`, `make test_fast`, `make test_all`, `make tidy`, `make build_proto`, `make check_proto`, `make changelog`, each written with the `mise exec --` prefix. It states the wrapper rule: without a shell-activated mise, "prefix all `make` commands with `mise exec --`".

Shared build infrastructure sits in `build/` and `scripts/`. The root `Makefile` (<https://github.com/pulumi/pulumi/blob/master/Makefile>) begins:

```make
SDKS         ?= nodejs python go pcl
SUB_PROJECTS := $(SDKS:%=sdk/%)
include build/common.mk
```

`build/common.mk` is the one included makefile, and each SDK directory has its own `Makefile` that the root delegates to through `SUB_PROJECTS`. `AGENTS.md` describes the two directories as "`build/` — Build system scaffolding (`common.mk`)" and "`scripts/` — CI and development helper scripts". Make recipes call scripts by name: `./scripts/pulumi-version.sh`, `./scripts/tidy.sh --check`, `./scripts/renovate-changelog.py`, `scripts/wasm-size-check.py`, `$(CURDIR)/scripts/go-test.py`, `./scripts/run-conformance.sh`. Shell and Python coexist in `scripts/`, and control flow sits there and not in the makefile.

Outputs and build stamps go to two fixed directories, and `clean` removes exactly those two (`rm -rf bin/*` and `rm -rf .make`). `.make/` holds sentinel files that make incrementality across languages: `.make/proto`, `.make/ensure/go`, `.make/ensure/phony`, `.make/ensure/golangci-lint`, `.make/go-version`. The `.make/go-version` recipe records `go version` output and rewrites the file when the output changes, dependents rebuild on a toolchain upgrade without being touched on every invocation. Nx expresses the same fact as an input, the toolchain version file (`global.json`, the installer manifest) in the language's `namedInputs`.

Generated files are checked in and verified by a diff target rather than regenerated in CI. `make check_proto` runs `git diff --quiet` over the generated proto output in four languages and fails with "Proto output is out of date. Run 'make build_proto' and commit." `AGENTS.md` states the invariant and the forbidden action: "Do not edit generated proto files by hand — edit `proto/*.proto` and run `mise exec -- make build_proto`."

Lint fans out over a named list of module directories rather than being discovered (`LINT_GOLANG_PKGS := sdk pkg tests sdk/go/pulumi-language-go ...`, `lint:: .make/ensure/golangci-lint lint_golang lint_pulumi_json lint_changelog`, `lint_fix:: lint_golang_fix lint_pulumi_json_fix`).

Biome is used for a single JSON file and is reached through the Node SDK's own install rather than a root dependency: `cd sdk/nodejs && npx biome format ../../pkg/codegen/schema/pulumi.json`, with a comment in the recipe explaining why it does not depend on the general `ensure` target.

`AGENTS.md` holds an "If you change..." table mapping a changed file kind to the exact commands to run: `.go` → `make format && make lint && make test_fast`, `proto/*.proto` → `make build_proto && make check_proto`, `go.mod` → `make tidy`. Subdirectory `AGENTS.md` files exist under `pkg/`, `sdk/nodejs/`, `sdk/python/`, and `sdk/go/`.

### [02.3]-[SFTKIT]

<https://github.com/SFTtech/sftkit>, an Nx workspace over a Python library (`sftkit/`) and React packages (`web/`). Newest commit on `master` 2026-02-12 (`49dbd73`, "chore(release): publish - project: sftkit 0.4.3"). Accepted under groups (a) and (b): plugin inference in `nx.json`, no `nxCloudId`, a uv workspace and a JavaScript workspace in one tree.

Root config files: `.env`, `.eslintignore`, `.eslintrc.config.js`, `.prettierignore`, `.prettierrc`, `jest.config.ts`, `jest.preset.ts`, `nx.json`, `package.json`, `package-lock.json`, `pyproject.toml`, `uv.lock`, `tsconfig.base.json`. Two source trees: `sftkit/` and `web/`.

The root `pyproject.toml` (<https://github.com/SFTtech/sftkit/blob/master/pyproject.toml>) is 612 bytes and holds all four Python concerns: `[dependency-groups] dev = ["mypy", "ruff", "pytest", "pytest-asyncio", "pytest-cov"]`, `[tool.uv.workspace] members = ["sftkit"]`, `[tool.ruff]` with `line-length` and `target-version`, `[tool.ruff.lint]` with an explicit `select`/`ignore` pair, and `[tool.mypy]`.

`sftkit/project.json` (<https://github.com/SFTtech/sftkit/blob/master/sftkit/project.json>) declares each Python tool as an `nx:run-commands` target under the same target names the TypeScript projects use, `nx run-many -t lint` covers both languages:

```json
"typecheck": { "executor": "nx:run-commands",
               "options": { "command": "uv run mypy .", "cwd": "{projectRoot}" } },
"lint":      { "executor": "nx:run-commands",
               "options": { "command": "uv run ruff check .", "cwd": "{projectRoot}" },
               "dependsOn": ["typecheck"] },
"format":    { "executor": "nx:run-commands",
               "options": { "command": "uv run ruff format .", "cwd": "{projectRoot}" } },
"test":      { "executor": "nx:run-commands",
               "options": { "command": "uv run pytest . --doctest-modules", "cwd": "{projectRoot}" } },
"build":     { "executor": "nx:run-commands", "inputs": ["default"],
               "options": { "command": "uv build", "cwd": "{projectRoot}" } }
```

Every command is `uv run <tool>` with `cwd: "{projectRoot}"`, and the project is tagged `"tags": ["lang:python"]`, one language can be selected with `nx run-many -t lint --projects=tag:lang:python`.

`nx.json` (<https://github.com/SFTtech/sftkit/blob/master/nx.json>) lists four plugins, `@nx/vite/plugin`, `@nx/eslint/plugin`, `@nx/jest/plugin`, `@nxlv/python`, and sets `"sharedGlobals": []`, `"parallel": 5`, `"tui": { "enabled": false }`. There is no `targetDefaults` block, and caching and inputs come from the plugins and from the per-project `inputs`.

A root `.env` file is committed (24 bytes) rather than a `.env.example`, and `nx.json` does not reference it.

Node dependency versions in `package.json` mix exact pins for the Nx and React packages (`"nx": "22.0.3"`, `"react": "19.1.0"`, `"typescript": "5.9.3"`) with carets for the rest. Scripts are one entry, `"npx": "npx"`, there is no npm-script layer over Nx.

### [02.4]-[AGENTSTACK]

<https://github.com/i-am-bee/agentstack>, Python (uv), TypeScript (pnpm), Java/Maven, Helm, and Lima VM images in one tree. Newest commit on `main` 2026-04-03 (`79c7860`, "Update to python v3.14 in Quickstart (#2507)"). Accepted under groups (b) and (c). Of every repository read, it is closest to Rasm's stated rules, and its task model is the strongest single finding.

Root config files: `mise.toml`, `mise.lock`, `tasks.toml`, `pnpm-workspace.yaml`, `pnpm-lock.yaml`, `agent-registry.yaml`, `policy.yaml`, `agentstack.code-workspace`, `install.sh`, `.aiignore`, `.gitattributes`, `.dockerignore`. There is no root `pyproject.toml`, no `package.json`, no `Makefile`, no `justfile`, and no per-language config file at the root: `mise.toml` and `tasks.toml` are the whole task feature set.

`mise.lock` is committed beside `mise.toml`, tool resolution is reproducible the way `uv.lock` and `pnpm-lock.yaml` are for dependencies.

Tool pinning and settings in one file, `mise.toml` (<https://github.com/i-am-bee/agentstack/blob/main/mise.toml>), among its `[tools]` rows:

```toml
[tools]
uv = "latest"
nodejs = "24"
pnpm = "10"
java = "21"
"pipx:toml-cli" = "latest"
"github:google/addlicense" = "latest"
fd = "latest"
yq = "latest"
hadolint = "latest"

[settings]
experimental = true
python.uv_venv_auto = true
task.disable_spec_from_run_scripts = true

[hooks]
postinstall = ["{{ mise_bin }} setup"]

[env]
UV_PYTHON_PREFERENCE = "only-managed"
```

`[hooks] postinstall` makes installing the toolchain run the workspace `setup` task as well, there is no separate bootstrap script. `[env]` holds the environment defaults, and there is no `.env` file at the root.

Reusable task definitions sit at the root, one per tool, and per-project files bind them to a directory. `mise.toml` declares `[task_templates]`:

```toml
[task_templates."python:check:ruff"]
run = "uv run ruff check --quiet"
sources = ["src/**/*.py"]
outputs = { auto = true }

[task_templates."python:check:pyrefly"]
run = "uv run pyrefly check src"
sources = ["src/**/*.py"]
outputs = { auto = true }

[task_templates."node:check:tsc"]
run = "pnpm tsc --noEmit"
sources = ["src/**/*.ts", "src/**/*.tsx"]
outputs = { auto = true }

[task_templates."docker:check:hadolint"]
run = "hadolint --ignore DL3018 Dockerfile"
sources = ["Dockerfile"]
outputs = { auto = true }
```

Each template names the tool once, together with the file globs that invalidate it, and `outputs = { auto = true }` caches the task without a named output path. Templates exist for `python:setup`, `python:check:ruff`, `python:check:ruff-format`, `python:check:pyrefly`, `python:check:pytest-marks`, `python:fix:ruff`, `python:fix:ruff-format`, `node:check:prettier`, `node:check:eslint`, `node:check:tsc`, `node:check:stylelint`, `node:fix:prettier`, `node:fix:eslint`, `node:fix:stylelint`, and `docker:check:hadolint`.

A per-project `tasks.toml` extends a template and supplies the directory alone. From `apps/agentstack-sdk-py/tasks.toml` (<https://github.com/i-am-bee/agentstack/blob/main/apps/agentstack-sdk-py/tasks.toml>):

```toml
["agentstack-sdk-py:check"]
depends = ["agentstack-sdk-py:check:*"]

["agentstack-sdk-py:check:ruff-check"]
extends = "python:check:ruff"
depends = ["agentstack-sdk-py:setup"]
dir = "{{config_root}}/apps/agentstack-sdk-py"
```

The tool command appears exactly once in the repository, and a project file never repeats `uv run ruff check`. The task files that participate are listed explicitly in `mise.toml` `[task_config] includes` (thirteen `tasks.toml` paths), and no glob discovers them.

Four entry points cover the whole workspace, each defined by a wildcard over project tasks, in the root `tasks.toml` (<https://github.com/i-am-bee/agentstack/blob/main/tasks.toml>):

```toml
["setup"] depends = ["*:setup"]
["check"] depends = ["*:check"]
["fix"]   depends = ["*:fix"]
["test"]  depends = ["*:test"]
```

`CLAUDE.md` (<https://github.com/i-am-bee/agentstack/blob/main/CLAUDE.md>) documents them in two sentences: "All testing and linting can be done via `mise run check`" and "Formatting can be fixed via `mise run fix`".

A task generates the git hooks in place of a hook manager. `common:setup:git-hooks` writes `.git/hooks/pre-commit` containing `mise run git-hooks:pre-commit`, and `["git-hooks:pre-commit"] depends = ["check"]`, the same entry point runs in the hook and by hand. No husky, no lefthook, no `pre-commit` framework.

One version across every language, enforced by a task. `common:check:version` reads the version out of `helm/Chart.yaml` (`version` and `appVersion`), three `pyproject.toml` files, three `package.json` files, and `agent-registry.yaml`, sorts unique, fails when the count is not one, and then checks the value against semver and, under a tag build, against `GITHUB_REF`. Its write counterpart is `release:set-version`, which uses `toml set`, `yq -i`, and `fd` to stamp the same version into every `pyproject.toml` and `package.json` in `apps/` and `agents/`, re-runs `uv lock` per project, and commits.

`pnpm-workspace.yaml` (<https://github.com/i-am-bee/agentstack/blob/main/pnpm-workspace.yaml>) holds both a `catalog:` of shared TypeScript versions and an `overrides:` block, with each override annotated by the advisory it pins past and the transitive path that pulls it in. Rasm uses `pnpm-workspace.yaml` as the version catalog with an `overrides:` block of its own.

A cleanup entry point exists as one named task, `please`, which uninstalls every mise tool, removes `**/.venv`, `**/node_modules`, `**/.next`, clears mise's `task-sources` and `task-auto-outputs` state directories, upgrades mise, reinstalls, and runs `setup ::: check`.

### [02.5]-[BSB]

<https://github.com/dbbs-lab/bsb>, an Nx workspace containing Python packages alone, with no Node manifest at the root. Newest commit on `main` 2026-06-30 (`cc76deb`, "chore(release): 7.6.0 [skip ci]"). Accepted under group (a), plugin inference and no `nxCloudId`.

Root config files: `nx.json`, `nx` (a committed shell wrapper), `nx.bat`, `.pre-commit-config.yaml`, `codecov.yml`, `codemeta.json`, `.gitattributes`. There is no `package.json`, no `package-lock.json`, no root `pyproject.toml`, and no root `uv.lock`. Source trees are `packages/`, `libs/`, `examples/`, `devtools/`.

Nx itself is pinned inside `nx.json` rather than in a Node manifest (<https://github.com/dbbs-lab/bsb/blob/main/nx.json>):

```json
"installation": {
  "version": "21.1.3",
  "plugins": { "@nxlv/python": "21.0.2" }
},
"plugins": [
  { "plugin": "@nxlv/python", "options": { "packageManager": "uv" } }
]
```

The committed `nx` wrapper checks for `node` and `npm` on `PATH`, then runs `node $path_to_root/.nx/nxw.js $@`, a contributor never installs Nx globally and no `node_modules` tree exists at the root. `nx.json` `installation` pins the Nx version and its plugins, the `nx`/`nx.bat` wrappers are the entry point, and `.nx/` holds the installed copy.

Hooks are declared in `.pre-commit-config.yaml` (<https://github.com/dbbs-lab/bsb/blob/main/.pre-commit-config.yaml>) rather than as Nx targets: `ruff` and `ruff-format` from `astral-sh/ruff-pre-commit` at `v0.11.5` with `--exclude=examples/*`, `conventional-pre-commit` on `commit-msg`, and one `repo: local` hook, `api-test`, running `python3 packages/bsb-core/tools/generate_public_api.py`.

Ruff runs from a pinned pre-commit revision (`v0.11.5`) here and from `[dependency-groups]` inside each package elsewhere, two paths to the same tool, which is the duplication Rasm's one-entry-point rule exists to prevent.

`nx.json` `generators."@nxlv/python:uv-project"` fixes the defaults every new Python package inherits, `"linter": "ruff"`, `"unitTestRunner": "pytest"`, `"codeCoverage": true`, `"pyprojectPythonDependency": ">=3.9,<3.12"`, a generated project cannot pick a different linter or test runner.

### [02.6]-[OPENLLMETRY]

<https://github.com/traceloop/openllmetry>, an Nx workspace of many PyPI-publishable Python packages. Newest commit on `main` 2026-08-10 (`62e24c2`, "bump: version 0.62.2 → 0.62.3"). Accepted under group (a).

Root config files: `nx.json` (127 bytes), `package.json` (225 bytes), `package-lock.json`, `.cz.toml`, `.gitignore`, `CLAUDE.md`. Source trees are `packages/` and `scripts/`.

`nx.json` is three lines (<https://github.com/traceloop/openllmetry/blob/main/nx.json>):

```json
{
  "extends": "nx/presets/npm.json",
  "$schema": "./node_modules/nx/schemas/nx-schema.json",
  "plugins": ["@nxlv/python"]
}
```

All target definition happens in per-package `project.json`. `packages/traceloop-sdk/project.json` (<https://github.com/traceloop/openllmetry/blob/main/packages/traceloop-sdk/project.json>) declares ten targets, each Python tool as an `nx:run-commands` call:

```json
"lint":       { "executor": "nx:run-commands", "options": { "command": "uv run ruff check .",   "cwd": "packages/traceloop-sdk" } },
"type-check": { "executor": "nx:run-commands", "outputs": [], "options": { "command": "uv run mypy traceloop/sdk", "cwd": "packages/traceloop-sdk" } },
"install":    { "executor": "nx:run-commands", "options": { "command": "uv sync --all-groups", "cwd": "packages/traceloop-sdk" } },
"lock":       { "executor": "nx:run-commands", "options": { "command": "uv lock",              "cwd": "packages/traceloop-sdk" } }
```

Test outputs are declared at the workspace root and not inside the project, all reports and coverage land in two top-level directories:

```json
"test": {
  "executor": "nx:run-commands",
  "outputs": [
    "{workspaceRoot}/reports/packages/traceloop-sdk/unittests",
    "{workspaceRoot}/coverage/packages/traceloop-sdk"
  ],
  "options": { "command": "uv run pytest tests/", "cwd": "packages/traceloop-sdk" }
}
```

Rasm's `.artifacts/` rule is the same idea with one directory instead of two, and the mechanism is the `{workspaceRoot}/...` output path in the target, which is what makes `nx reset` and cache restoration place files consistently.

`CLAUDE.md` (<https://github.com/traceloop/openllmetry/blob/main/CLAUDE.md>) documents the entry points as `nx run-many -t test`, `nx run-many -t lint`, `nx run-many -t lock`, `nx affected:test`, and `nx affected:lint`, and states the invocation rule for the language: "All packages use uv as the package manager. Always execute commands through uv: `uv run <command>`."

Per-package configuration is stated as policy rather than avoided: "Ruff is used for code linting. Configuration is in each package's `pyproject.toml` under `[tool.ruff]`." With no root `pyproject.toml` the ruff settings are duplicated across every package, the opposite of Rasm's rule and of what sftkit and Sage-Bionetworks do.

The `cwd` values in `project.json` are literal paths (`"cwd": "packages/traceloop-sdk"`) rather than `{projectRoot}`, the path is repeated in every target and again in the project's own filename. sftkit's `"cwd": "{projectRoot}"` is the form that does not repeat.

### [02.7]-[CONVEX_BACKEND]

<https://github.com/get-convex/convex-backend>, Rust, TypeScript (pnpm with Turbo), and Python in one tree. Newest commit on `main` 2026-09-03 (`d971c26`). Accepted under group (c).

Root config files: `mise.toml`, `mise.lock`, `Justfile`, `Cargo.toml`, `Cargo.lock`, `rust-toolchain`, `rustfmt.toml`, `dprint.json`, `.prettierrc.js`, `.prettierignore`, `.nvmrc`, `.nvmrc.24`, `.cargo/`, `.config/`, `BUILD.md`. Source trees are `crates/`, `npm-packages/`, `scripts/`, `self-hosted/`, `demo/`.

`mise.toml` (<https://github.com/get-convex/convex-backend/blob/main/mise.toml>) is the most heavily reasoned tool manifest read. Each pin states the reason it exists, and the file states which tools it does not own:

```toml
# Node is NOT here -- it stays in .nvmrc (via idiomatic_version_file in [settings]),
# the shared source of truth for the builder image, copybara, and the OSS repo.
# Rust (rustup), Python (uv), and pnpm/Turbo manage themselves.
min_version = "2026.8.9"

[tools]
uv       = "0.11.5"
just     = "1.52.0"
cmake    = "3.31.12"   # 3.x line; 4.x drops compat with our older CMakeLists
"cargo:cargo-machete" = "0.9.2"

[settings]
idiomatic_version_file_enable_tools = ["node"]
lockfile = true
```

The file pins `jq`, `protoc`, `cargo-binstall`, `cargo-sort`, and `binaryen` as well, through the `github:` backend with a `version_prefix`. Mechanisms here that apply to Rasm:

- `min_version` makes the config refuse to run under an older tool binary, the pin file itself is versioned
- `lockfile = true` with a committed `mise.lock` checksum-pins the downloaded binaries across platforms, the same guarantee `uv.lock` and `pnpm-lock.yaml` give for dependencies
- `idiomatic_version_file_enable_tools = ["node"]` points the tool manager at an existing version file rather than restating the version, one file stays authoritative. Rasm's `global.json` is the .NET SDK version file the installer reads the same way

Environment variables are computed per platform inside the same file with mise's template functions, no `.env` and no shell profile holds them:

```toml
[env]
SODIUM_USE_PKG_CONFIG = "1"
ROCKSDB_LIB_DIR = "{% if os() == 'macos' %}/opt/homebrew/lib{% elif os() == 'linux' %}/usr/lib{% endif %}"
```

OS packages that mise cannot version-pin are declared separately from tools, under `[bootstrap.packages]`, with `apt:` and `brew:` prefixes and per-OS filters (`"brew:pkgconf" = { version = "latest", os = "macos" }`). A comment records why they are `latest`: "apt/brew can't portably pin and mise.lock doesn't cover OS packages."

The `Justfile` (<https://github.com/get-convex/convex-backend/blob/main/Justfile>) is the command entry point and opens with the reason the repository does not use make: "Instead of `Makefile`s, Convex uses Justfiles, which are similar, but avoid several footguns associated with Makefiles, since using make as a macro runner can sometimes conflict with Makefiles desire to have some rudimentary understanding of build artifacts and associated dependencies."

The package manager and the task runner are themselves wrapped as recipes, there is exactly one path to each binary, the copy pinned under `scripts/node_modules/.bin`:

```
# (*) pnpm, the JS package manager (pinned in scripts/package.json)
pnpm *ARGS:
  cd {{invocation_directory()}}; "{{justfile_directory()}}/scripts/node_modules/.bin/pnpm" "$@"

install-js:
  cd "{{justfile_directory()}}/npm-packages"; just pnpm install --frozen-lockfile
update-js:
  cd "{{justfile_directory()}}/npm-packages"; just pnpm install
```

`install-js` and `update-js` are two named recipes for the two intents (restore exactly, versus change the lockfile), a contributor never has to remember `--frozen-lockfile`. Rasm's equivalent split is `dotnet restore` under `RestoreLockedMode` versus an explicit update target.

`_default: @just --list` makes the bare command print every recipe, and each user-facing recipe is prefixed with `(*)` in its doc comment to separate the entry points from the internal helpers.

### [02.8]-[RENOVATE]

<https://github.com/renovatebot/renovate>, TypeScript with pnpm, with a small uv-managed Python tree for the docs site. Newest commit on `main` 2026-09-03. Accepted under group (c). It is the one accepted repository that uses Biome as a workspace-wide checker (pulumi/pulumi uses it for one JSON file), the reference point for Rasm's `biome.json`.

Root config files: `mise.toml`, `package.json`, `pnpm-workspace.yaml`, `pnpm-lock.yaml`, `pyproject.toml`, `uv.lock`, `biome.json`, `.oxlintrc.json`, `.prettierrc.json`, `.prettierignore`, `.editorconfig`, `.nvmrc`, `.python-version`, `.markdownlint-cli2.mjs`, `.ls-lint.yml`, `.lintstagedrc.json`, `tsconfig.json`, `tsdown.config.mts`, `vitest.config.mts`, `codecov.yml`, `renovate.json`, `.releaserc.json`, `AGENTS.md`, `CLAUDE.md`. Source trees are `lib/`, `test/`, `tools/`, `data/`, `docs/`, `patches/`, `__mocks__/`.

`mise.toml` (<https://github.com/renovatebot/renovate/blob/main/mise.toml>) does the whole bootstrap:

```toml
[tools]
node = "24.20.0"
pnpm = "11.24.0"
uv   = "0.12.9"

[hooks]
postinstall = '''
pnpm install
uv sync
'''

[settings]
experimental = true
```

Both package managers are pinned to exact versions, and `[hooks] postinstall` runs both installs, `mise install` is the single onboarding command for a two-language repository. There is no `setup.sh` and no `make bootstrap`.

The one duplication left standing: `.nvmrc` holds `24.20.0` as well, and a root `.python-version` (`3.14.5`) sits beside the mise `uv` pin, with no `idiomatic_version_file_enable_tools` setting pointing mise at either. The two Node values agree and nothing enforces that they keep agreeing, which is the state ComposioHQ/composio removed in its Phase 2 commit and convex-backend avoided by pointing mise at `.nvmrc`.

`biome.json` (<https://github.com/renovatebot/renovate/blob/main/biome.json>) shows Biome scoped to a narrow job:

```json
"linter": {
  "enabled": true,
  "rules": {
    "preset": "none",
    "correctness": { "noUndeclaredDependencies": "error" },
    "style": { "noParameterProperties": "error" }
  }
},
"assist": { "actions": { "source": { "organizeImports": {
  "level": "on", "options": { "identifierOrder": "lexicographic" } } } } },
"formatter": { "enabled": false }
```

`"preset": "none"` turns off every default rule, the two named rules alone run, and `"formatter": { "enabled": false }` leaves formatting to Prettier and linting proper to oxlint. `noUndeclaredDependencies` enforces the same policy as Rasm's "reference a package directly in every project that names its types".

The exclusion list is one `files.includes` array at the root covering every language's excluded paths, among them `!**/node_modules`, `!**/dist`, `!**/coverage`, `!**/.venv/**`, `!.worktrees/**/*`, `!.claude/worktrees/**/*`, `!patches`, `!tools/mkdocs/.cache`, `!tools/mkdocs/site`, in place of a `.biomeignore` or per-directory configs. `.venv` and the Python docs tool's working directories are excluded from the JavaScript linter's config rather than from a second file.

`pyproject.toml` holds two pinned docs dependencies (`mkdocs-material==9.7.7`, `mkdocs-awesome-pages-plugin==2.10.1`), `requires-python = ">=3.11"`, and `[tool.uv] package = false`. Python is a tool dependency of the docs build and not a source language, and the manifest says so by declaring nothing else.

### [02.9]-[TYPESPEC_AZURE]

<https://github.com/Azure/typespec-azure>, TypeScript with pnpm, with Python, Go, and Java emitters. Newest commit on `main` 2026-09-03. `mise.toml` (<https://github.com/Azure/typespec-azure/blob/main/mise.toml>):

```toml
[tools]
python = "3.12"
uv = "latest"
go = "1.26.1"
java = "microsoft-11"
maven = "3.9.16"
node = "26"

[settings]
idiomatic_version_file_enable_tools = ["pnpm"]
```

`idiomatic_version_file_enable_tools = ["pnpm"]` makes mise read the pnpm version from the `packageManager` field in `package.json` instead of restating it, the same one-owner-per-fact move convex-backend makes for Node through `.nvmrc`.

Rasm's `package.json` has no `packageManager` field and declares pnpm as a range under `devEngines`, no exact pnpm or Node version exists anywhere in the repository. The plan settles it: mise pins node and pnpm, and the root manifest declares `packageManager` with `devEngines`, the manifest and the installer agree.

### [02.10]-[OPSML]

<https://github.com/demml/opsml>, a Rust workspace, a Python binding package (`py-opsml`), and a SvelteKit UI. Newest commit on `main` 2026-05-14 (`fd9731e`, "Merge pull request #399 from demml/migration-mise"). Accepted under group (c). The head commit is the migration to mise, the repository documents a consolidation in progress.

`mise.toml` (<https://github.com/demml/opsml/blob/main/mise.toml>) is both the tool manifest and the task file: `[tools]`, `[env]`, and about eighty `[tasks.…]` entries in one file. The root holds a `Makefile`, `cliff.toml`, `setup.cfg`, `.coveragerc`, and `codecov.yml` as well.

Directory constants sit in `[env]`, no task repeats a path:

```toml
[env]
UI_DIR = "crates/opsml_server/opsml_ui"
PY_DIR = "py-opsml"
SOURCE_OBJECTS = "python/opsml"
FORMAT_OBJECT = "python/opsml examples"
```

Task names are namespaced by language and by action (`py:lints-ruff`, `py:lints-ty`, `py:test:unit`, `py:docs:build`, `ui:build`, `ui:dev`, `test:sql-postgres`, `start:server`), and aggregates are composed with `depends`:

```toml
[tasks."py:lints"]
description = "Run all Python linters"
depends = ["py:lints-ruff", "py:lints-pylint", "py:lints-ty"]
```

Every task has a `description`, which `mise tasks` lists, the task file is its own command documentation. Tasks that must run in a package set `dir` once (`dir = "py-opsml"`) instead of prefixing `cd`.

`[settings] disable_tools = ["python", "gitleaks"]` with a comment stating why is the notable defensive detail: "Python and Rust are intentionally supplied by uv/setup-python and rustup in CI. Pinning them here shadows matrix/toolchain selections when mise-action updates PATH." A tool manager adopted into a repository with an existing CI matrix must be told which tools not to own.

The repository is a counter-example on tool count: Python runs `isort`, `black`, `ruff`, `pylint`, and `ty`, with `py:format` chaining three of them and `py:lints-ci` running four. Rasm's `ruff`, `ty`, `mypy` set is smaller, and the aggregate-task pattern here is what keeps a set of that size to one invocation.

### [02.11]-[ADSP_MONOREPO]

<https://github.com/GovAlta/adsp-monorepo>, an Nx workspace over TypeScript, .NET, Python, and Java. Newest commit on `main` 2026-09-02 (`d301a32`). Accepted under group (a): plugin inference, no `nxCloudId`, and `"analytics": false` in `nx.json`. It is the one accepted repository that combines Nx with MSBuild, the closest structural match to Rasm.

Root config files: `nx.json`, `package.json`, `package-lock.json`, `tsconfig.base.json`, `eslint.config.mjs`, `.prettierrc`, `.prettierignore`, `.editorconfig`, `babel.config.json`, `jest.config.ts`, `jest.preset.js`, `jest-cover.preset.js`, `jest-mongodb-config.js`, `Directory.Build.props`, `Directory.Build.targets`, `core-services.sln`, `.nx-dotnet.rc.json`, `.nxignore`, `.java-version`, `.env`, `.env.github-scripts.example`, `.releaserc.json`, `.cleancode.yml`, `.unittest.yml`, `migrations.json`, `globalConfig.json`, `directory.platform.json`, `nginx.conf`, `AGENTS.md`. Source trees are `apps/`, `libs/`, `tools/`, `tests/`, `samples/`, `docs/`, `architecture/`, `patches/`, `.openshift/`.

The .NET side sends every output to a single repository-root directory computed from the project's path, the same rule as Rasm's `.artifacts/`. From `Directory.Build.props` (<https://github.com/GovAlta/adsp-monorepo/blob/main/Directory.Build.props>):

```xml
<RepoRoot>$([System.IO.Path]::GetFullPath('$(MSBuildThisFileDirectory)'))</RepoRoot>
<ProjectRelativePath>$([System.IO.Path]::GetRelativePath($(RepoRoot), $(MSBuildProjectDirectory)))</ProjectRelativePath>
<BaseOutputPath>$(RepoRoot)dist/$(ProjectRelativePath)</BaseOutputPath>
<OutputPath>$(BaseOutputPath)</OutputPath>
<VSTestResultsDirectory>$(RepoRoot)coverage/$(ProjectRelativePath)</VSTestResultsDirectory>
<RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>
<AppendTargetFrameworkToOutputPath>false</AppendTargetFrameworkToOutputPath>
```

`RestorePackagesWithLockFile` is on for the whole repository from the root props file, and `AppendTargetFrameworkToOutputPath=false` keeps the output layout matching the source layout. Test package references (`xunit`, `Moq`, `FluentAssertions`, `coverlet.collector`, `ReportGenerator`, `Microsoft.NET.Test.Sdk`, `RichardSzalay.MockHttp`) are declared once in the root props under `Condition="'$(Configuration)' == 'Debug'"` rather than repeated per test project, and a `Coverage` target runs `ReportGenerator` over `$(VSTestResultsDirectory)/**/coverage.cobertura.xml`.

Plugin inference is scoped by an explicit path list rather than left to match everything. In `nx.json` (<https://github.com/GovAlta/adsp-monorepo/blob/main/nx.json>) the ESLint plugin holds an `include` array naming about 57 TypeScript projects one by one:

```json
"plugins": [
  "@nx/dotnet",
  "@nxlv/python",
  {
    "plugin": "@nx/eslint/plugin",
    "options": { "targetName": "lint" },
    "include": ["apps/agent-service/**/*", "apps/api-docs-app/**/*", ... , "tools/workspace-plugin/**/*"]
  }
]
```

`@nx/dotnet` and `@nxlv/python` are registered as unqualified strings with no options, and the JavaScript linter alone is scoped. Rasm does the inverse, `@nx/dotnet` holds `exclude: ["eng/native/**"]` while the JavaScript plugins are unscoped. Both `include` and `exclude` are supported, and the choice is whether the maintained list is of included or excluded projects.

`.nxignore` (27 bytes) and `.nx-dotnet.rc.json` (26 bytes) sit at the root as the two scoping files for the non-JavaScript side.

`sharedGlobals` here names five root files (`workspace.json`, `tsconfig.base.json`, `tslint.json`, `nx.json`, `babel.config.json`), all JavaScript-side. `Directory.Build.props` is not among them, a change to the root MSBuild props does not invalidate cached .NET tasks. Rasm's `sharedGlobals` has the same gap on the .NET and Python side.

### [02.12]-[MUDITA_CENTER]

<https://github.com/mudita/mudita-center>, an Nx workspace over an Electron desktop application and its libraries. Newest commit on `develop` 2026-07-27 (`9886724`). Accepted under group (a), `"neverConnectToCloud": true` in `nx.json`, the same setting Rasm uses.

Root config files: `nx.json`, `package.json`, `package-lock.json`, `project.json`, `tsconfig.base.json`, `eslint.config.js`, `.prettierrc`, `.prettierignore`, `.stylelintrc.json`, `.editorconfig`, `.npmrc`, `.nvmrc`, `.env.example`, `.codecov.yml`, `jest.config.ts`, `jest.preset.js`, `migrations.json`, `SCRIPTS.md`. Source trees are `apps/`, `libs/`, `scripts/`, `resources/`, `patches/`.

Every checker is registered as an Nx plugin with an explicit target name, target names are uniform across projects (<https://github.com/mudita/mudita-center/blob/develop/nx.json>):

```json
{ "plugin": "@nx/vite/plugin",  "options": { "buildTargetName": "build", "typecheckTargetName": "typecheck", ... } },
{ "plugin": "nx-stylelint/plugin", "options": { "targetName": "stylelint", "extensions": ["css", "ts", "tsx"] } },
{ "plugin": "@nx/eslint/plugin", "options": { "targetName": "lint" } },
{ "plugin": "@nx/jest/plugin",  "options": { "targetName": "test" }, "exclude": ["apps/app-e2e/**/*"] },
{ "plugin": "@nx/vitest",       "options": { "testTargetName": "test" } }
```

`nx-stylelint/plugin` is a third-party inference plugin for a tool with no first-party Nx plugin, the alternative to hand-writing `nx:run-commands` targets that sftkit and openllmetry use.

`targetDefaults` adds inputs for the tool's own config file, a config edit invalidates its cache:

```json
"stylelint": {
  "inputs": ["default", "{workspaceRoot}/.stylelintrc(.(json|yml|yaml|js))?"],
  "cache": true,
  "options": { "args": ["--allow-empty-input"] }
}
```

`sharedGlobals` is a single entry, `["{workspaceRoot}/.github/workflows/validate.yml"]`, the CI definition is a cache input and a change to how CI runs invalidates every cached task. No other repository read here does this.

`release.version.preVersionCommand` is `"npx nx run-many -t build"`, the release path re-enters the same build target rather than defining its own.

Commands are documented in a dedicated root file, `SCRIPTS.md` (<https://github.com/mudita/mudita-center/blob/develop/SCRIPTS.md>), which states its purpose in one line, "This document explains different scripts available in the `package.json`", and groups every script under Main, Development, Building, Code quality, Storybook, Nx utilities, and Project tools, each with a one-line comment.

### [02.13]-[JETSTREAM]

<https://github.com/jetstreamapp/jetstream>, an Nx workspace over a web app, an Electron desktop client, a browser extension, and shared libraries. Newest commit on `main` 2026-09-01 (`3d2aece`, "Merge pull request #2034 … chore: migrate playwright to inferred"). Accepted under group (a), `"neverConnectToCloud": true` and `"analytics": false` in `nx.json`.

Root config files: `nx.json`, `package.json`, `pnpm-workspace.yaml`, `pnpm-lock.yaml`, `tsconfig.json`, `tsconfig.base.json`, `.oxlintrc.json`, `.oxfmtrc.json`, `.editorconfig`, `.nvmrc`, `.nxignore`, `.env.example`, `.svgo-config.json`, `babel.config.json`, `commitlint.config.js`, `vitest.config.mts`, `playwright.config.base.ts`, `electron-builder.config.js`, `next-sitemap.config.js`, `prisma.config.ts`, `.release-it.json`, `.release-it-desktop.json`, `.release-it-web-ext.json`, `version.json`, `migrations.json`, `AGENTS.md`, `CLAUDE.md`. Source trees are `apps/`, `apps-sfdx/`, `libs/`, `tools/`, `scripts/`, `prisma/`, `playwright/`, `build-resources/`, `custom-typings/`, `custom-express-typings/`, `mock-idp/`.

The repository answers the `.env` question directly: environment variables that change build output are declared as cache inputs in `nx.json` (<https://github.com/jetstreamapp/jetstream/blob/main/nx.json>) rather than left implicit:

```json
"build": {
  "dependsOn": ["^build"],
  "inputs": ["production", "^production",
             { "env": "NX_PUBLIC_SERVER_URL" },
             { "env": "NX_PUBLIC_CLIENT_URL" }],
  "cache": true
}
```

An `{ "env": "NAME" }` input makes the variable's value part of the cache key, a build run with a different value does not restore a stale artifact. `.env.example` alone (8.2 KB) is committed, and the real `.env` is ignored. Rasm's `nx.json` declares no `env` inputs.

Plugin inference is scoped with `include` on the plugin that needs scoping and left unscoped on the rest:

```json
{ "plugin": "@nx/vite/plugin", "options": { ... },
  "include": ["apps/jetstream/**/*", "apps/jetstream-canvas/**/*",
              "apps/jetstream-desktop-client/**/*", "apps/jetstream-web-extension/**/*"] },
{ "plugin": "@nx/js/typescript", "options": { "typecheck": { "targetName": "typecheck" } } },
{ "plugin": "@nx/vitest",   "options": { "testTargetName": "test", "testMode": "watch" } },
{ "plugin": "@nx/playwright/plugin", "options": { "targetName": "e2e", "ciTargetName": "e2e-ci" } }
```

Both the Vite plugin and the TypeScript plugin set `typecheckTargetName`/`targetName` to `typecheck`, and both the Vite and Vitest plugins settle on `build`/`test`, a project served by either plugin answers to the same target name. `"sync": { "applyChanges": true }` lets Nx write TypeScript project references itself, and Rasm disables that generator with `"disabledTaskSyncGenerators": ["@nx/js:typescript-sync"]`.

Lint and format run on `oxlint` and `oxfmt` from `.oxlintrc.json` and `.oxfmtrc.json` at the root, one config file per tool with no per-project overrides, the same shape as Rasm's single `biome.json`.

`useLegacyCache: false`, `useDaemonProcess: true`, `parallel: 4`, and `tui.enabled: false` are set explicitly rather than left to defaults.

### [02.14]-[COMPOSIO]

<https://github.com/ComposioHQ/composio>, TypeScript (pnpm), Python (uv), and Rust in one tree. Newest commit on `next` 2026-09-03 (`fc681b8`). Accepted under groups (b) and (c).

Root config files: `mise.toml`, `mise.lock`, `toolchain-versions.json`, `pnpm-workspace.yaml`, `pnpm-lock.yaml`, `package.json`, `pyproject.toml`, `uv.lock`, `turbo.jsonc`, `tsconfig.base.json`, `tsdown.config.base.ts`, `.oxlintrc.json`, `.prettierrc`, `.pnpmfile.cjs`, `knip.json`, `context7.json`, `parity-variance.json`, `examples-manifest.json`, `skills-lock.json`, `AGENTS.md`, `CLAUDE.md`, `INSTALL.md`. Source trees are `ts/`, `python/`, `harness/`, `scripts/`, `test/`, `install/`, `docs/`, `skills/`.

The top-level source split is by language name, `ts/` and `python/`, rather than by `packages/` with mixed contents.

`mise.toml` (<https://github.com/ComposioHQ/composio/blob/next/mise.toml>) states its own role in its first line and records why each choice was made:

```toml
# Single source of truth for Node, Bun, Deno, Python, uv, and pnpm versions.
#
# Local dev:  `mise install`
# CI:         exact versions are read from `mise.lock` by the composite actions
#             under `.github/actions/`
#
# pnpm is pinned through mise's npm backend so mise owns the full local and CI
# toolchain. We do not rely on Corepack because Node.js no longer distributes it
# starting with v25.

min_version = "2026.8.15"

[tools]
"npm:pnpm" = "11.8.0"
node = "24.17.0"
deno = "2.6.7"
python = "3.12"
uv = "0.8.19"

[env]
_.path = ["{{config_root}}/node_modules/.bin"]

[settings]
lockfile = true
locked = true
```

Details to take:

- The comment names the file's job and the two consumers (local `mise install`, and CI reading `mise.lock` through composite actions), the file is not silently duplicated in workflow YAML
- `_.path = ["{{config_root}}/node_modules/.bin"]` puts the workspace's own binaries on `PATH`, an unqualified `biome`/`tsc`/`vitest` invocation resolves to the pinned local copy rather than a global one
- `lockfile = true` with `locked = true` refuses to silently resolve a version outside `mise.lock`

A tool with an exact build that is not a release tag is pinned by URL and checksum per platform, under `[tools.bun.platforms.*]`, and the comment states what the checksum does and does not prove, with the one test that catches a half-finished bump (`pnpm run test:toolchain`, `test/mise-bun-pin.test.ts`). Rasm pins native library archives in `eng/native/` and `eng/scripts/provision.py` with the same convention: a URL, a checksum, and a check that compares the label against the installed artifact.

`toolchain-versions.json` (106 bytes) holds the sets of versions CI matrices iterate over, separately from the single version developers use:

```json
{
  "node": ["22.22.3", "24.17.0", "25.9.0"],
  "deno": ["2.6.7"],
  "python": ["3.10", "3.11", "3.12"]
}
```

The file separates the version developers use (`mise.toml`) from the versions CI must keep working (the matrix file), instead of hard-coding a matrix list in each workflow.

### [02.15]-[RAGBITS]

<https://github.com/deepsense-ai/ragbits>, a uv workspace of Python packages with a TypeScript tree. Newest commit on `main` 2026-05-13 (`32074a7`, "chore: update package versions for nightly build"). Accepted under group (b).

Root config files: `pyproject.toml`, `uv.lock`, `package.json`, `package-lock.json`, `tsconfig.json`, `.editorconfig`, `.pre-commit-config.yaml`, `mkdocs.yml`, `mkdocs_hooks.py`, `release_checklist.md`. Source trees are `packages/`, `typescript/`, `scripts/`, `examples/`, `docs/`.

One root `pyproject.toml` (<https://github.com/deepsense-ai/ragbits/blob/main/pyproject.toml>) owns every Python setting for eight workspace members: `[tool.uv.workspace] members`, `[tool.uv.sources]` marking each member `workspace = true`, `[dependency-groups] dev` holding `mypy`, `ruff`, `pytest` and the docs toolchain, and then `[tool.pytest.ini_options]`, `[tool.coverage.run]`, `[tool.mypy]`, `[[tool.mypy.overrides]]`, `[tool.ruff]`, `[tool.ruff.lint]`, `[tool.ruff.lint.pydocstyle]`, `[tool.ruff.lint.flake8-annotations]`, `[tool.ruff.lint.per-file-ignores]`, `[tool.ruff.format]`, and `[tool.ruff.lint.isort]`.

Mechanisms in that file that apply to Rasm's root `pyproject.toml`:

- `[tool.mypy] mypy_path` lists every member's source directory explicitly, type checking resolves cross-package imports without any package being installed in editable mode
- `[[tool.mypy.overrides]] module = "ragbits.*"` with `disallow_untyped_defs = true` raises strictness for first-party code alone, while `ignore_missing_imports = true` stays global for third-party code, strictness is scoped by module pattern in one file
- `[tool.ruff.lint.per-file-ignores]` holds the test and notebook relaxations (`"**/tests/**/*.py"` allows `S101` and drops docstring rules), tests need no separate ruff config

`[tool.ruff.lint] preview = true` with `explicit-preview-rules = true` and an `extend-select` naming exactly two preview rules (`RUF022`, `PLR6301`) is the pattern for adopting individual preview rules without enabling the whole preview set.

## [03]-[NX_SPECIFICS]

### [03.1]-[UNOFFICIAL_TOOL_TARGETS]

Three shapes appear across the accepted Nx repositories for tools with no official Nx plugin:

1. `nx:run-commands` in a per-project `project.json` (SFTtech/sftkit with `"cwd": "{projectRoot}"`, traceloop/openllmetry with a literal path)
2. A `command` in `targetDefaults`, no project repeats it. Rasm does this for `typecheck` in `nx.json` (`"command": "tsc --build --pretty false"`, `"options": { "cwd": "{projectRoot}" }`, `"dependsOn": ["^typecheck"]`, inputs with `{ "externalDependencies": ["typescript"] }`, `"outputs": ["{projectRoot}/dist"]`)
3. A third-party inference plugin (mudita/mudita-center's `nx-stylelint/plugin`)

Across GitHub, `nx.json` files that put a `biome check` command inside `targetDefaults` total 12, and `nx.json` files that put a `uv run` command there total 2 (GitHub code search, `"biome check" "targetDefaults" filename:nx.json` and `"uv run" "targetDefaults" "command" filename:nx.json`, rerun 2026-09-03). None of those 12 met the activity and structure bar, the pattern is real but rare. The plan settles Rasm on shape 2: `lint`, `format`, and `typecheck` declared once per language in `targetDefaults` with check and write configurations, language tags on projects, and one aggregate target for the whole set.

Conventions that hold in every accepted repository:

- The tool is invoked through its package manager (`uv run ruff`, `pnpm eslint`) and never from a global install
- The target name matches the name the other languages use, `nx run-many -t lint` crosses language boundaries, and sftkit tags projects `"tags": ["lang:python"]` so that one language can still be selected

### [03.2]-[ENV_FILES]

Two mechanisms exist, and one of them alone makes caching correct.

Declared `env` inputs: jetstreamapp/jetstream names each build-affecting variable in the target's `inputs`. The commit that added them (`0b3a882`, 2026-08-26, <https://github.com/jetstreamapp/jetstream/commit/0b3a88274974cbfbbeba643fd91a6efd9a42810b>) states why: "The new env inputs in nx.json are load-bearing: the URLs are baked into the client and landing bundles at build time, so without them a port change cache-hits a stale bundle."

Environment in the tool manifest in place of a `.env`: i-am-bee/agentstack puts `UV_PYTHON_PREFERENCE = "only-managed"` in `mise.toml` `[env]`, get-convex/convex-backend computes `ROCKSDB_LIB_DIR` per platform in the same block, pulumi/pulumi sets `PULUMI_TEST_USE_NPM`. None of the three has a root `.env`.

On the file itself: mudita/mudita-center, jetstreamapp/jetstream, and GovAlta/adsp-monorepo commit a `.env.example`, and SFTtech/sftkit and GovAlta/adsp-monorepo commit an actual `.env` at the root. No accepted repository lists `.env` or `.env.example` in `namedInputs`.

Rasm, by the plan: no `.env` and no `.env.example`, build-affecting variables declared as `{ "env": "NAME" }` inputs on the affected targets, installer `[env]` limited to shell and tool discovery, native library paths as machine state, and secrets reached through Doppler.

### [03.3]-[GENERATED_FILES]

Check the output in and verify with a diff target: pulumi/pulumi commits the generated proto output and gates it with `make check_proto`, with the generating target made incremental by a sentinel file over `PROTO_SOURCES`.

Regenerate through a target and never by hand: Sage-Bionetworks/sage-monorepo generates OpenAPI server stubs and clients for four languages and documents the regeneration command as `nx run-many -t=generate -p=<scope>-*`, with the rule "Implement business logic in the generated skeletons — do not hand-edit generated files". demml/opsml regenerates Python type stubs from Rust bindings in `py:build:stubs` and makes `py:setup` depend on it.

Declare the output path so that the cache restores it: traceloop/openllmetry points test outputs at `{workspaceRoot}/reports/...` and `{workspaceRoot}/coverage/...`, and GovAlta/adsp-monorepo's `Directory.Build.props` points every .NET build and test output at `$(RepoRoot)dist/...` and `$(RepoRoot)coverage/...`.

Enumerate the generated and vendored paths in writing, and mark them in `.gitattributes`: ComposioHQ/composio's `AGENTS.md` (<https://github.com/ComposioHQ/composio/blob/next/AGENTS.md>) has a section "Generated And Vendored Paths", "Do not hand-edit these. They are regenerated or vendored, and edits will be overwritten. They are also marked `linguist-generated`/`linguist-vendored` in `.gitattributes`", followed by the exact globs, including the lockfiles: "`pnpm-lock.yaml`, `uv.lock`, `**/bun.lock` — package-manager lockfiles; change them by running the package manager, never by hand."

Rasm's generated Gmsh bindings follow the second shape: `stage.py` regenerates them into the staged `managed/` directory under `.artifacts/native`, the bindings project compiles `$(StageRoot)managed/*.cs`, and nothing generated is committed.

### [03.4]-[NON_JAVASCRIPT_PROJECTS]

- Register one plugin per ecosystem, and scope it: GovAlta/adsp-monorepo registers `"@nx/dotnet"` and `"@nxlv/python"` as unqualified strings and puts the path list on `@nx/eslint/plugin`, jetstreamapp/jetstream puts `include` on `@nx/vite/plugin`, mudita/mudita-center puts `exclude` on `@nx/jest/plugin`, and Rasm puts `exclude: ["eng/native/**"]` on `@nx/dotnet`. All four use `include`/`exclude` on the plugin entry rather than `.nxignore` alone
- Give each ecosystem its own `namedInputs` pair: Sage-Bionetworks/sage-monorepo defines `defaultPython`/`productionPython` naming `{workspaceRoot}/pyproject.toml` and `{workspaceRoot}/uv.lock`, and `defaultGradle`/`productionGradle` naming `gradle/libs.versions.toml`, `buildSrc/**/*`, `build.gradle.kts`, and `settings.gradle.kts`. The .NET pair names `Directory.Build.props`, `Directory.Build.targets`, `Directory.Packages.props`, `global.json`, `NuGet.config`, and `packages.lock.json`
- Key `targetDefaults` on the executor for plugin-provided targets: Sage-Bionetworks/sage-monorepo sets caching and inputs on `"@nxlv/python:build"`, `"@nxlv/python:ruff-check"`, `"@nxlv/python:run-commands"`, and `"@nx/gradle:gradle"` rather than on target names
- Nx without a Node manifest: dbbs-lab/bsb pins Nx and its plugins in `nx.json` `installation` and commits `nx` / `nx.bat` wrappers, a Python-only workspace has no root `package.json`
- Turn off automatic plugin registration: Sage-Bionetworks/sage-monorepo sets `"useInferencePlugins": false` while still listing three plugins, inference is opted into one plugin at a time

## [04]-[CORRECTIONS_IN_COMMIT_HISTORY]

Every entry is a commit with a message that states the defect it removed, reread with `get_commit` on 2026-09-03.

### [04.1]-[COMPOSIO]

A three-phase move to one toolchain file:

- Phase 1, `07160d1` (2026-06-05), "feat(toolchain): introduce mise.toml as single source of truth (#3492)", <https://github.com/ComposioHQ/composio/commit/07160d14fa0411088d215bbbd82bae8bd499609c>. The defect, in the commit's words: "The repo already has real toolchain drift, not just duplicated version strings. The internal release docs had stale Node/Bun/pnpm versions, Deno is repeated across workflows, Dockerfiles, docs, and e2e helpers, and Python local setup still has a separate `3.11` venv path while the repo pins `3.12`." The phase is deliberately additive and does not yet delete `.nvmrc`, `.bun-version`, `.python-version`, or `.dvmrc`
- Phase 2, `4fe7768` (2026-06-15), "chore(toolchain): finish mise migration (#3493)", <https://github.com/ComposioHQ/composio/commit/4fe776898a38101357bf014f5fdf31b013599e88>. "This completes the Phase 2 migration by removing the transitional version-file layer and making `mise.toml` plus `mise.lock` the repository toolchain source of truth… so there is one place to update tool versions. That avoids silent drift between local setup, GitHub Actions, Docker E2E images, release docs, and install-time checks." Deleted `.nvmrc`, `.bun-version`, `.dvmrc`, root `.python-version`, and `python/.python-version`, removed `idiomatic_version_file_enable_tools`, added a committed `mise.lock`, moved CI matrices into `toolchain-versions.json`
- `025a657` (2026-06-19), "feat(ts): drop CommonJS support (#3494)", <https://github.com/ComposioHQ/composio/commit/025a657597c60d9b1c3ac1d53edcaa59ac3a2436>. "Pins pnpm in `mise.toml` as `\"npm:pnpm\" = \"10.28.2\"` and makes mise the single source of truth for the pnpm version. Removes the root `packageManager` / `devEngines.packageManager` Corepack pin entirely (rather than keeping a second copy of the version that could drift)." The one thing that forced a workaround is named: Turbo requires a `packageManager` field, `dangerouslyDisablePackageManagerCheck: true` was set in `turbo.jsonc`, "which is the only reason the field would otherwise need to stay"
- `be8e978` (2026-09-03), "fix(toolchain): pin Bun canary for valid macOS signatures (#4315)", <https://github.com/ComposioHQ/composio/commit/be8e978c3a9f19cd52028b38004bf6a48b26e88d>. "makes `mise.toml` the editable source of truth for Bun … installs Bun through mise in CI and Docker E2E images, removing the independent `bun-version` input and the `oven-sh/setup-bun` channel"

### [04.2]-[PULUMI]

- `08b352c` (2026-06-09), "pin mise dependencies (#23477)", <https://github.com/pulumi/pulumi/commit/08b352c81d59477efaf3711fbdce45b33974be02>. "Currently we have some dependencies in `.mise.toml` pinned to fixed versions, while others just use `latest`… this loses some of the caching benefits we could get." Mixed `latest` and exact pins in one manifest is recorded as a defect
- `90c3879` (2026-05-11), "Revert 'use mise to install dependencies' (#22957)", <https://github.com/pulumi/pulumi/commit/90c3879fae1a6c149fc6e9b5b656a4eb9be7ce88>. "This unfortunately seems *more* flaky than just installing the dependencies regularly. So I think we can call this a failed experiment for now, and revert it." The tool manager kept ownership of tool versions while CI dependency installation went back to the ecosystem's own installers, the one recorded negative result on adopting a tool manager wholesale, later followed by the pinning commit, which names itself "a first step for trying to re-introduce mise in CI"
- `b0d42ff` (2026-05-15), "Use golangci-lint (#23199)", <https://github.com/pulumi/pulumi/commit/b0d42ff16fb241664305a94fa1738f8d448948ce>. "I ran into some reformatting because I had a different version of `gofumpt` installed locally. Let's run this via `golanglint-ci` so things are pinned nicely and there's no drift with what ships with `golanglint-ci` and `gofumpt` releases." A formatter reached two ways produced drift, and the direct path was removed
- `ca79d20` (2026-06-12), "Use wasm-size-check in goreleaser build (#23539)", <https://github.com/pulumi/pulumi/commit/ca79d20574bacbf0ea90bdfb4a611d19878c443e>. "When running PRs they only build the binaries via goreleaser, not via the makefile. As such we missed our normal 'wasm-size-check' script, and just had a hardcoded 77mb limit as part of the goreleaser script… This changes goreleaser to run the same script as the makefile against the same golden size target." Two build entry points had diverged and one held a hard-coded constant, and both were pointed at one script and one golden file
- `d20aa8e` (2026-06-25), "Use npm as the package manager for the Node.js SDK (#23655)", <https://github.com/pulumi/pulumi/commit/d20aa8e36c366c5c2d8af7c21469afcb7e00f192>. Yarn removed in favor of one package manager for that SDK

### [04.3]-[CONVEX_BACKEND]

- `b9f3e2e` (2026-07-23), "Replace Rush with pnpm workspaces + Turborepo (#55025)", <https://github.com/get-convex/convex-backend/commit/b9f3e2e7ab5c736112d68534e00177eb6df531ee>. The message holds a before/after command table, contributors relearn one row at a time: `just rush install` → `just install-js`, `just rush add -p foo` → edit `package.json`, then `just update-js`, `just rush build [-t pkg]` → `just build-js` or `just turbo run build [--filter=pkg...]`, `rush check` → `just check-js-versions`. It records that the lockfile was converted mechanically with "zero version drift" and that overrides, a 7-day `minimumReleaseAge`, and node-range enforcement all carried over, with measured build times
- `848f520` (2026-08-26), "mise lock (#56762)", <https://github.com/get-convex/convex-backend/commit/848f520fed846484be2e0999734c867bb8a907ef>. Added the committed tool lockfile after the tool manifest already existed
- `3fa3cef` (2026-08-28), "mise: apply the brew bootstrap packages on macOS only (#56893)", <https://github.com/get-convex/convex-backend/commit/3fa3cefcfb3658428a33aad072cc5e5cd04ec60c>. Unfiltered `brew:` entries were installing on Linux too, and the fix is the `os = "macos"` filter documented in a comment in `mise.toml` explaining that mise pours bottles itself
- `69888c4` (2026-06-23), "use canonical brew:pkgconf formula in mise.toml (#53506)", <https://github.com/get-convex/convex-backend/commit/69888c44b2f1d41de92d38e47e95b6d873868d06>. "`pkg-config` is now a `pkgconf` alias in Homebrew, and mise's brew backend 404s on aliases. Use the canonical formula."
- `78ada62` (2026-08-26), "Use cargo-machete instead of cargo-udeps (#56756)", one unused-dependency checker replaced another rather than both being kept

### [04.4]-[JETSTREAM]

- `21b8276` (2026-09-01), "chore: migrate playwright to inferred", <https://github.com/jetstreamapp/jetstream/commit/21b82760638b0975b2414a6c015e7afa840455db>. The message quotes the deprecation it answers: "The `@nx/playwright:playwright` executor is deprecated and will be removed in Nx v24. Run `nx g @nx/playwright:convert-to-inferred` to migrate to the `@nx/playwright/plugin` inferred targets." Hand-written executor targets moved to plugin inference
- `0b3a882` (2026-08-26), "chore(e2e): add pnpm e2e:local one-command runner on an isolated port", <https://github.com/jetstreamapp/jetstream/commit/0b3a88274974cbfbbeba643fd91a6efd9a42810b>. Two defects in one commit. The procedure it replaced: "Running web E2E locally required editing NX_PUBLIC_CLIENT_URL in .env and manually building, and the server clashed with local dev on 3333/4200." And the cache bug, quoted under the `.env` mechanisms
- `4a1c745` (2026-08-26), "build: run prisma db scripts from the workspace install instead of pnpm dlx", <https://github.com/jetstreamapp/jetstream/commit/4a1c745fc09a239a1f9c39510bf74920355ad0bb>. "`pnpm dlx` fetches the npm latest tag, which now points at the Prisma 8 RC whose rewritten CLI rejects `migrate deploy`, breaking every CI run. Using the workspace bin (as `db:generate` already does) pins these scripts to the locked 7.x version." One script reached a tool a second way, and that path was removed
- `e1b86c2` (2026-08-09), "chore(tooling): migrate lint and format from ESLint + Prettier to oxlint + oxfmt (#1923)", <https://github.com/jetstreamapp/jetstream/commit/e1b86c2f3a3a6dd658f9a4e756875c2b4e7c3feb>. Two tools and their config files replaced by two files, `.oxlintrc.json` and `.oxfmtrc.json`
- `0b4cf4d` (2026-05-22), "chore: migrate to pnpm (#1730)", <https://github.com/jetstreamapp/jetstream/commit/0b4cf4d180ba7f3eb90c3ce13dfed7d8e289a01f>

### [04.5]-[OPSML]

- `fd9731e` (2026-05-14), "Merge pull request #399 from demml/migration-mise", <https://github.com/demml/opsml/commit/fd9731e9fa90b1f689bf0f539e9f82e46ecd18ff>. The result is the current `mise.toml` holding `[tools]`, `[env]`, and about eighty tasks, with a root `Makefile` still present

## [05]-[COMMAND_DOCUMENTATION]

Verified locations, by repository:

- A flat command list in an agent file: pulumi/pulumi, "Command canon" in `AGENTS.md`, prefixed by "All commands assume you're at the repo root". ComposioHQ/composio, "Common Commands" in `AGENTS.md`, one fenced block of `pnpm …` and one of `make …` run from `python/`, preceded by "Use `mise install` for the pinned toolchain. pnpm is managed through mise, not Corepack." Sage-Bionetworks/sage-monorepo, "Common Commands" in `CLAUDE.md`, one block per language. traceloop/openllmetry, "Nx Workspace Commands" in `CLAUDE.md`
- Two commands and nothing else: i-am-bee/agentstack's `CLAUDE.md`
- A rule in place of a list: jetstreamapp/jetstream's `AGENTS.md` holds the Nx-generated block with the first rule "When running tasks (for example build, lint, test, e2e, etc.), always prefer running the task through `nx` (i.e. `nx run`, `nx run-many`, `nx affected`) instead of using the underlying tooling directly."
- A dedicated document: mudita/mudita-center's `SCRIPTS.md`
- Self-documenting task listings: demml/opsml gives every `[tasks.…]` entry a `description`, which `mise tasks` prints, and get-convex/convex-backend's `Justfile` sets `_default: @just --list` and marks user-facing recipes with `(*)` in their doc comments

One rule appears in ComposioHQ/composio's `AGENTS.md` alone and handles documentation drift directly: "Verify every command you write against the current `package.json`, `Makefile`, `noxfile.py`, or workflow file."

## [06]-[UNIQUE_PRACTICES]

Practices found in one repository:

- Task templates defined once at the root and bound per project, i-am-bee/agentstack
- A cross-language version-consistency check as a task, i-am-bee/agentstack's `common:check:version`, with the write counterpart `release:set-version`
- The CI workflow file as a cache input, mudita/mudita-center's `"sharedGlobals": ["{workspaceRoot}/.github/workflows/validate.yml"]`
- Environment variables declared as Nx cache inputs, jetstreamapp/jetstream's `{ "env": "NX_PUBLIC_SERVER_URL" }`, added after a stale-cache bug the commit message records
- A separate file for CI matrix version sets, ComposioHQ/composio's `toolchain-versions.json`
- Nx pinned inside `nx.json` with committed wrapper scripts and no `package.json`, dbbs-lab/bsb
- `useInferencePlugins: false` with plugins still listed, Sage-Bionetworks/sage-monorepo
- A one-word full-reset task, i-am-bee/agentstack's `please`
- The task runner wraps the package manager, get-convex/convex-backend's `just pnpm` and `just turbo` recipes pointing at the copies under `scripts/node_modules/.bin`, with `install-js` and `update-js` as separate recipes for restore-exactly versus change-the-lockfile
- OS packages declared beside tool versions in their own block, get-convex/convex-backend's `[bootstrap.packages]` with `apt:` / `brew:` prefixes and per-OS filters
- A build stamp file that records the toolchain version and rewrites on change alone, pulumi/pulumi's `.make/go-version` rule
- Every npm dependency pinned to an exact version, no ranges, Sage-Bionetworks/sage-monorepo's `package.json`
- Preview lint rules adopted individually, deepsense-ai/ragbits' `[tool.ruff.lint] preview = true` with `explicit-preview-rules = true` and an `extend-select` naming exactly `RUF022` and `PLR6301`
- Biome narrowed to the rules no other tool covers, renovatebot/renovate's `biome.json` with `"preset": "none"`, two rules, import organizing, and the formatter disabled, and its `files.includes` array as the workspace's single exclusion list

## [07]-[RECURRING_PRACTICES]

Counted over the fifteen accepted repositories, each item holds the practice, the count, and the repositories:

- One repository-owned tool manifest pins every language runtime and CLI, 7: pulumi, convex-backend, renovate, typespec-azure, opsml, agentstack, composio
- Nx plugin inference declared in `nx.json` `plugins`, 7: sage-monorepo, sftkit, bsb, openllmetry, adsp-monorepo, mudita-center, jetstream
- Exactly one root config file per checker, no per-package overrides, 7: sage-monorepo, sftkit, ragbits, mudita-center, jetstream, adsp-monorepo, composio
- Commands documented in an agent file (`CLAUDE.md` / `AGENTS.md`) and not the README, 6: sage-monorepo, pulumi, agentstack, openllmetry, composio, jetstream
- A Python tool is invoked as `uv run <tool>` alone, 6: sage-monorepo, sftkit, openllmetry, agentstack, opsml, pulumi
- `tui.enabled: false` set in `nx.json`, 4: sage-monorepo, sftkit, mudita-center, jetstream
- Aggregate entry points composed from per-project tasks rather than restated, 4: agentstack, opsml, pulumi, convex-backend
- A `scripts/` directory holds the steps with control flow, called by name from the task runner, 4: pulumi, convex-backend, jetstream, ragbits
- A `tools/` directory holds workspace-authored lint rules, executors, plugins, or code generators, 4: sage-monorepo (`executors/`, `prepare-*-envs.js`), pulumi (`automation/`), adsp-monorepo (`workspace-plugin/`, `eslint-rules/`), jetstream (`oxlint/`)
- Plugin inference scoped with `include` or `exclude` on the plugin entry, 3: adsp-monorepo, jetstream, mudita-center
- A root `pyproject.toml` owns uv workspace membership and every Python tool setting, 3: sage-monorepo, sftkit, ragbits
- Build and test outputs land in repository-root directories, declared in the target or the props file, 3: openllmetry, adsp-monorepo, pulumi
- A committed tool lockfile (`mise.lock`) beside the tool manifest, 3: agentstack, convex-backend, composio
- A committed `.env.example` with the real `.env` ignored, 2: mudita-center, jetstream
- A real `.env` committed at the root, 2: sftkit (24 bytes), adsp-monorepo (209 bytes, beside `.env.github-scripts.example`)
- Nx cloud connection and analytics explicitly disabled, 2: mudita-center, jetstream
- `CLAUDE.md` is a symlink to `AGENTS.md` (or the reverse), 2: pulumi, Rasm (`AGENTS.md` → `CLAUDE.md`)
- Python tool configuration duplicated per package instead of at the root (counter-practice), 2: openllmetry, bsb
- Per-language `namedInputs` pairs, a manifest change invalidates its own language alone, 1: sage-monorepo
- Environment variables declared as cache inputs, 1: jetstream

## [08]-[SCATTERED_CONFIGURATION]

Each item names the scattered or duplicated state, the repository, and the correction it applied:

- A tool version restated in `.nvmrc`, `.bun-version`, `.dvmrc`, two `.python-version` files, workflow YAML, Dockerfiles, and release docs, ComposioHQ/composio before #3492/#3493: add `mise.toml` additively in one PR, then delete every version file and add `mise.lock` in the next
- The pnpm version kept in both the tool manifest and `package.json` `packageManager`/`devEngines`, ComposioHQ/composio: remove the `packageManager` pin and set `dangerouslyDisablePackageManagerCheck: true` in `turbo.jsonc` (#3494)
- A tool installed by both mise and a dedicated CI setup action, ComposioHQ/composio (Bun through `oven-sh/setup-bun` with a `bun-version` input): install through mise everywhere and delete the separate input and the action (#4315)
- Some tools pinned exactly and others set to `latest` in the same manifest, pulumi/pulumi: pin them all (#23477)
- A formatter reachable both directly and through the lint runner, producing version drift, pulumi/pulumi (`gofumpt` vs `golangci-lint`): run it through `golangci-lint` alone (#23199)
- Two build entry points diverged, one holding a hard-coded size limit, pulumi/pulumi (Makefile vs goreleaser, 77 MB literal): point both at the same `scripts/wasm-size-check.py` and the same golden-size file (#23539)
- A tool manager taking over CI dependency installation as well as tool versions, pulumi/pulumi: revert that part and keep the manifest for tool versions (#22957)
- Two package managers in one repository (yarn for one SDK, npm elsewhere), pulumi/pulumi: move that SDK to npm (#23655)
- A separate monorepo tool with its own commands layered over the package manager, get-convex/convex-backend (Rush): replace with pnpm workspaces and Turbo, and publish a before/after command table in the commit message (#55025)
- OS-package entries applying on every platform, get-convex/convex-backend (unfiltered `brew:` entries installing on Linux): add `os = "macos"` filters (#56893)
- Tool binaries resolved without checksums, get-convex/convex-backend: add `lockfile = true` and commit `mise.lock` (#56762)
- Hand-written executor targets where the plugin infers them, jetstreamapp/jetstream (`@nx/playwright:playwright`): run `nx g @nx/playwright:convert-to-inferred` and delete the executor targets (`21b8276`)
- A local workflow that required hand-editing `.env` and building manually, jetstreamapp/jetstream: one `pnpm e2e:local` command on an isolated port that overrides the variables on the command line (`0b3a882`)
- Build-affecting environment variables not declared as cache inputs, a changed value restored a stale artifact, jetstreamapp/jetstream: add `{ "env": "NX_PUBLIC_SERVER_URL" }` and `{ "env": "NX_PUBLIC_CLIENT_URL" }` to the `build` target inputs (`0b3a882`)
- A CLI reached both from the workspace install and by an on-demand fetch that resolved `latest`, jetstreamapp/jetstream (`prisma` through `pnpm dlx`): call the workspace bin, as the sibling script did, pinning to the locked version (`4a1c745`)
- Two JavaScript tools and two config files where one of each suffices, jetstreamapp/jetstream (ESLint with Prettier): replace with oxlint and oxfmt, one root config each (#1923)
- Two unused-dependency checkers, get-convex/convex-backend (`cargo-udeps` and `cargo-machete`): keep one (#56756)
- Scripts and task definitions spread across a Makefile, shell scripts, and package scripts, demml/opsml: move the tasks into `mise.toml` `[tasks.…]` (#399), with a root `Makefile` still present

## [09]-[SETTLED_AND_OPEN]

Settled by the plan, applied to the findings:
- Per-language `namedInputs` pairs replace the flat `sharedGlobals`: the .NET pair names `Directory.Build.props`, `Directory.Build.targets`, `Directory.Packages.props`, `global.json`, `NuGet.config`, and `packages.lock.json`, the Python pair names `pyproject.toml` and `uv.lock`, the TypeScript pair names the eleven files listed in the baseline
- `lint`, `format`, and `typecheck` are declared once per language in `targetDefaults` with check and write configurations, projects carry language tags, and one aggregate target covers the whole set, `nx run-many -t lint` crosses languages and a tag selects one
- mise pins node, pnpm, python, and uv with a committed lockfile, reads `global.json` for the .NET SDK, and every config path and `.vscode/settings.json` follow it, and `eng/scripts/provision.py` keeps vcpkg and the pinned archives that no installer expresses
- The root `package.json` declares `packageManager` with `devEngines`, matching the installer pin
- No `.env` or `.env.example`, build-affecting variables are `{ "env": "NAME" }` inputs on the affected targets, the installer `[env]` holds shell and tool discovery alone, native library paths stay machine state, and secrets come through Doppler with no `doppler.yaml`
- Every action a developer or CI runs is an Nx target, the target list is the command list and no separate command document is kept
- `.artifacts/dotnet` receives .NET build and test output through `ArtifactsPath`, and coverage merges into one report per language with no threshold
- The code, contracts, and schema carry no version and no package publishes, no cross-language version check exists, and the native packaging versions are checked by `EnsureManifestVersionMatch` and `EnsureCentralPackageVersionMatch`
- Generated Gmsh bindings are regenerated by the `stage` target under `.artifacts/native` and never committed
- Biome runs through a `targetDefaults` command, as Nx has no official Biome plugin

Open, the answer changes the design:
1. The root `package.json` mirror of the catalog (round six)
2. `tools/biome/*.grit`: all thirteen plugin entries in `biome.json` share the identical `includes` array (`**/libs/typescript/**`, `**/tools/nx/**`, with test exclusions) and leave `apps/` out, and no plan decision names whether `apps/` is outside the rules or the array folds into one shared list
