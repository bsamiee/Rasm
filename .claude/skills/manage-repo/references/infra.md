# [INFRA]

Pulumi's Automation API runs the typed program in process, GitHub Actions runs workflows of commands.

## [01]-[PROGRAM]

- Rows are `as const satisfies` their provider's argument type
- `LocalWorkspace.createOrSelectStack` takes the inline program, `stackName`, `projectName`, and an existing `workDir`
- Stack operations stream stdout through `onOutput`, stderr through `onError`, and stop on `signal`
- `Pulumi.yaml` under `workDir` freezes `projectName` and `main` when `projectSettings` is absent
- `PULUMI_HOME/workspaces` holds one file per `workDir` path
- Programs run `up` and `refresh` alone
- `import: <id>` in resource options adopts an existing resource for one run, `protect: true` refuses deletion
- Use `secrets` for a token or variable a program or workflow reads

## [02]-[WORKFLOWS]

| [INDEX] | [DECISION]   | [FORM]                                                                                                            |
| :-----: | :----------- | :---------------------------------------------------------------------------------------------------------------- |
|  [01]   | Event        | Repository event or schedule that requires the work, `workflow_dispatch` with typed `inputs`                      |
|  [02]   | Permissions  | `permissions` names the keys jobs call, every unnamed key is `none`, `permissions: {}` for none                   |
|  [03]   | Concurrency  | `group` by workflow and ref, `cancel-in-progress` as an expression, `queue: max` excludes `cancel-in-progress`    |
|  [04]   | Checkout     | `persist-credentials: false` unless the job pushes, `fetch-depth: 0` for `nx-set-shas` and `nx affected`          |
|  [05]   | Cache        | `actions/cache` keyed by `hashFiles` over files deciding cache contents, no `restore-keys`                        |
|  [06]   | Shell        | `defaults.run.shell` covers workflow `run` steps, each composite `run` step declares `shell`                      |
|  [07]   | Expressions  | `${{ }}` values reach a script through the step `env` map                                                         |
|  [08]   | Status check | Fan-in job with `needs` over every job and `if: always()`, failing on a `needs.<job>.result` other than `success` |

- Caches restore an exact `key` match and save under it when their job succeeds, a key over a file with `latest` rows restores the first save
- `jdx/mise-action` restores its data directory, then `mise install` resolves every `latest` row again, the cache freezes no version
- Composite steps for one ecosystem take `if: runner.os == '<os>'` when one runner alone runs its projects
- Jobs skipped by a conditional or a failed dependency pass as a required status check and read `skipped` in `needs.<job>.result`
