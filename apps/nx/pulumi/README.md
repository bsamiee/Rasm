# [NX_PULUMI]

`@rasm/nx-pulumi` is an Nx 24 plugin for Pulumi Automation API inline programs. A program package states its stacks in TypeScript, and every project depending on it gets `install`, `deploy`, and `nx-release-publish` targets that apply those stacks through `Stack.up`.

## [01]-[USE]

```json
{ "plugins": ["@rasm/nx-pulumi"] }
```

- The plugin takes no options: package manifests state every fact it reads
- A program package declares `@pulumi/pulumi` in `dependencies` and exports `stacks` from its entry
- A project is hosted by a program package when it declares that package in `dependencies`, `devDependencies`, or `optionalDependencies`
- Releases select hosted projects through `release.projects` or `release.groups` in `nx.json`

```ts
import type { Stacks } from '@rasm/nx-pulumi/stacks';

const stacks: Stacks = (request) => [{ projectName: 'hosting', stackName: `${request.context.projectName}.local`, program: async () => ({}) }];

export { stacks };
```

`stacks` receives one `StackRequest` case and returns Pulumi's own `InlineProgramArgs`:

| [INDEX] | [CASE]    | [FIELDS]           | [SENT_BY]                              |
| :-----: | :-------- | :----------------- | :------------------------------------- |
|  [01]   | `Install` | `context`          | `install`                              |
|  [02]   | `Deploy`  | `context`, `stack` | `deploy`, `stack` from `--stack=<x>`   |
|  [03]   | `Release` | `context`, `version` | `nx-release-publish` under `nx release` |

- `context` is the Nx `ExecutorContext` of the task, so the package reads the project, its root, and `nx.json` from it
- The package decides what `stack` means and which stacks a request selects

## [02]-[INFERENCE]

| [INDEX] | [SOURCE]                                        | [GIVES]                                               |
| :-----: | :---------------------------------------------- | :---------------------------------------------------- |
|  [01]   | `package.json` with `@pulumi/pulumi` dependency | Program package, `deploy` on its own project          |
|  [02]   | `package.json` depending on a program package   | Hosted project: `install`, `deploy`, `nx-release-publish` |

- Nx's own package graph adds the edge from each hosted project to its program package, so `^install` reaches the program package's `install`
- A manifest that does not parse fails by name and keeps every other project

## [03]-[TARGETS]

| [INDEX] | [TARGET]             | [OPERATION]                                    | [DEPENDS_ON]             | [CACHE] |
| :-----: | :------------------- | :--------------------------------------------- | :----------------------- | :-----: |
|  [01]   | `install`            | `Stack.up` with refresh                        | `^install`               |   No    |
|  [02]   | `deploy`             | `Stack.up` with refresh, `Stack.destroy` with `--destroy` | None          |   No    |
|  [03]   | `nx-release-publish` | `Stack.up` with refresh, `Stack.preview` under a dry run | `^nx-release-publish` |   No    |

- Each stack runs in its own temporary work folder, so no `Pulumi.yaml` lands in the tree
- Every selected stack runs concurrently in one process and prints one JSON line: `project`, `stack`, and `changes` with non-secret `outputs`, `missing` for a stack a preview or destroy found absent, or `error`
- A task fails after every line prints when any stack failed
- The Automation API loads from the program package's own `@pulumi/pulumi`, the copy its inline programs register with
- `nx-release-publish` skips a project `nx release` left unversioned, and maps `dryRun` or `NX_DRY_RUN` to `Stack.preview`, which creates no stack
- Refresh, preview, and destroy run the program (`runProgram`), so providers configure from the program's current values

## [04]-[LAYOUT]

| [INDEX] | [PATH]                     | [HOLDS]                                                        |
| :-----: | :------------------------- | :------------------------------------------------------------- |
|  [01]   | `plugins/project-graph.ts` | Package entry: manifest decoder and `createNodes`              |
|  [02]   | `executors/stacks/`        | `install`, `deploy`, `release-publish` executors, request types, schemas |
