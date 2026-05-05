# [H1][TESTING_TOOLING]
>**Dictum:** *Tooling routes tests through configured gates, not stale command folklore.*

<br>

Testing tools are discovered from repository configuration before use. Prefer existing Nx, language runner, coverage, mutation, and E2E targets over invented commands.

---
## [1][DISCOVERY]
>**Dictum:** *Configuration is the source of truth for commands and thresholds.*

<br>

| [INDEX] | [SOURCE]                | [READ_FOR]                                           |
| :-----: | ----------------------- | ---------------------------------------------------- |
|   [1]   | **Workspace manifests** | Test dependencies, package manager, script names.    |
|   [2]   | **Nx metadata**         | Project test targets and affected-project selection. |
|   [3]   | **Language configs**    | Runner, environment, coverage provider, strictness.  |
|   [4]   | **CI workflows**        | Required gates and report artifacts.                 |
|   [5]   | **Mutation configs**    | Mutator scope, thresholds, excluded files.           |

[IMPORTANT]:
1. [ALWAYS] **Read config first:** Verify runner names and thresholds locally.
2. [ALWAYS] **Use affected scope:** Prefer impacted targets for documentation-only validation when supported.
3. [ALWAYS] **Record exact command:** Cite command used in completion summary.

---
## [2][COMMAND_ROUTING]
>**Dictum:** *Command shape follows configured project type.*

<br>

| [INDEX] | [TASK]                | [PREFERRED_ROUTE]                                               |
| :-----: | --------------------- | --------------------------------------------------------------- |
|   [1]   | **Unit tests**        | Existing Nx or language-specific test target.                   |
|   [2]   | **Integration tests** | Existing integration target with required services provisioned. |
|   [3]   | **System/E2E tests**  | Existing E2E target with configured runtime lifecycle.          |
|   [4]   | **Coverage**          | Existing coverage flag or target from test configuration.       |
|   [5]   | **Mutation**          | Existing mutation command and configured mutator scope.         |

[CRITICAL]:
- [NEVER] invent commands from package names.
- [NEVER] create per-package runner configs to bypass root tooling.
- [NEVER] document commands that have not been verified from local config.

---
## [3][TOOL_CLASSES]
>**Dictum:** *Capability matters more than vendor-specific syntax.*

<br>

| [INDEX] | [CLASS]             | [REQUIRED_CAPABILITY]                                           |
| :-----: | ------------------- | --------------------------------------------------------------- |
|   [1]   | **Runner**          | Execute specs under `tests/` and report deterministic failures. |
|   [2]   | **Property engine** | Generate inputs, shrink failures, and encode preconditions.     |
|   [3]   | **Coverage engine** | Report changed-file or project-level coverage.                  |
|   [4]   | **Mutation engine** | Mutate source and report surviving behavioral mutants.          |
|   [5]   | **E2E runner**      | Manage browser, CLI, or service lifecycle when configured.      |

---
## [4][ARTIFACTS]
>**Dictum:** *Reports support review without becoming source truth.*

<br>

Generated reports, screenshots, traces, coverage output, and mutation output are artifacts. Store them where configured by tooling; do not treat generated report paths as policy.

[IMPORTANT]:
1. [ALWAYS] **Ignore generated output:** Keep author-owned policy and specs separate from reports.
2. [ALWAYS] **Link durable evidence:** Prefer CI artifact links or command summaries over committed report snapshots.
