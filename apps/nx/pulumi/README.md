# [NX_PULUMI]

`@rasm/nx-pulumi` is an Nx 24 plugin for Pulumi Automation API inline programs. A program package states its stacks in TypeScript, and every project depending on it gets `install--pulumi` under the `install` aggregate, `deploy`, and `nx-release-publish` targets that apply those stacks through `Stack.up`.

## [01]-[USE]

```json
{ "plugins": ["@rasm/nx-pulumi"] }
```

- Options: none, since package manifests state every fact the plugin reads
- Program packages declare `@pulumi/pulumi` in `dependencies` and `@rasm/nx-pulumi` in any dependency field, and export `stacks` from their entry
- Projects declaring a program package in `dependencies`, `devDependencies`, or `optionalDependencies` are hosted by it
- Hosted projects carry the `release` tag, so `"release": { "projects": ["tag:release"] }` in `nx.json` selects them with no list of names

```ts
import type { Stacks } from '@rasm/nx-pulumi/stacks';

const stacks: Stacks = (request) => [{ projectName: 'hosting', stackName: `${request.context.projectName}.local`, program: async () => ({}) }];

export { stacks };
```

`stacks` receives one `StackRequest` case and returns a promise of Pulumi's `InlineProgramArgs`, each optionally with `LocalWorkspaceOptions` fields (`projectSettings` with its backend, `stackSettings`, `secretsProvider`, `envVars`):

| [INDEX] | [CASE]    | [FIELDS]             | [SENT_BY]                            |
| :-----: | :-------- | :------------------- | :----------------------------------- |
|  [01]   | `Install` | `context`            | `install--pulumi`                    |
|  [02]   | `Deploy`  | `context`, `stack`   | `deploy`, `stack` from `--stack=<x>` |
|  [03]   | `Release` | `context`, `version` | `nx-release-publish`                 |

- `context` is the Nx `ExecutorContext` of the task, so the package reads the project, its root, and `nx.json` from it
- Program packages decide what `stack` means and which stacks a request selects

## [02]-[INFERENCE]

| [INDEX] | [SOURCE]                                      | [GIVES]                                     |
| :-----: | :-------------------------------------------- | :------------------------------------------ |
|  [01]   | `package.json` stating both packages above    | Program package, `deploy`                   |
|  [02]   | `package.json` depending on a program package | Hosted project: every target, tag `release` |

- Packages depending on `@pulumi/pulumi` alone (component libraries) take no target
- Nx's own package graph adds the edge from each hosted project to its program package, so `^install` reaches the program package's `install`
- Target descriptions come from `executors.json`
- Manifests that do not parse state no dependency to the plugin, so a broken fixture manifest outside the package workspaces fails no `nx` command

## [03]-[TARGETS]

| [INDEX] | [TARGET]             | [OPERATION]                                               | [DEPENDS_ON]          | [CACHE] |
| :-----: | :------------------- | :-------------------------------------------------------- | :-------------------- | :-----: |
|  [01]   | `install--pulumi`    | `Stack.up` with refresh                                   | `^install`            |   No    |
|  [02]   | `deploy`             | `Stack.up` with refresh, `Stack.destroy` with `--destroy` | None                  |   No    |
|  [03]   | `nx-release-publish` | `Stack.up` with refresh, `Stack.preview` under a dry run  | `^nx-release-publish` |   No    |
|  [04]   | `install`            | `nx:noop` over every `install--*` target of the project   | `install--*`          |   No    |

- Stacks run in their own temporary work folder unless their options name a `workDir`, so no `Pulumi.yaml` lands in the tree
- Named `workDir` folders holding `Pulumi.yaml` and `Pulumi.<stack>.yaml` supply the settings `projectSettings` and `stackSettings` leave out
- `pulumi config set` in such a folder persists
- Secrets reach stacks through the environment the task inherits, `envVars`, or stack configuration, and the runner names no secret store
- Interrupted tasks need no handler: Nx signals the whole process tree, and Pulumi cancels the update on its first `SIGINT` or `SIGTERM`
- Selected stacks run concurrently in one process, each printing one JSON line: `project`, `stack`, and `changes` with non-secret `outputs`, `missing`, or `error`
- `missing` marks a stack a preview or destroy found absent
- Program packages failing to load or to state their stacks print the error to stderr while other programs' stacks run on
- Tasks fail after every line prints when any stack or program failed
- Automation API loads from the program package's own `@pulumi/pulumi`, the copy its inline programs register with
- `nx-release-publish` takes the version `nx release` resolved and skips a project it left unversioned; run by `nx release publish` alone, it takes the `package.json` version Nx read
- `dryRun` or `NX_DRY_RUN` maps `nx-release-publish` to `Stack.preview`, which creates no stack
- Refresh, preview, and destroy run the program (`runProgram`), so providers configure from the program's current values

## [04]-[LAYOUT]

| [INDEX] | [PATH]                       | [HOLDS]                                                       |
| :-----: | :--------------------------- | :------------------------------------------------------------ |
|  [01]   | `plugins/project-graph.ts`   | Package entry: manifest decoder and `createNodes`             |
|  [02]   | `executors/deploy/`          | `deploy` executor, the stack runner, `StackRequest`, `Stacks` |
|  [03]   | `executors/install/`         | `install--pulumi` executor over the runner                    |
|  [04]   | `executors/release-publish/` | `nx-release-publish` executor over the runner                 |
