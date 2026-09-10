# [INFRA]

Pulumi's Automation API runs the typed program in process, GitHub Actions runs workflows of commands.

## [01]-[PROGRAM]

- Rows are `as const satisfies` their provider's argument type
- `LocalWorkspace.createOrSelectStack` takes an inline program, `up` and `refresh` results carry `summary.resourceChanges`
- `import: <id>` in resource options adopts an existing resource on one run and comes out of the options after it, `protect: true` refuses deletion
- Use `secrets` for a token or variable a program or workflow reads

## [02]-[WORKFLOWS]

| [INDEX] | [DECISION]   | [FORM]                                                                                                            |
| :-----: | :----------- | :---------------------------------------------------------------------------------------------------------------- |
|  [01]   | Event        | Repository event or schedule that requires the work, `workflow_dispatch` with typed `inputs`                      |
|  [02]   | Permissions  | `permissions` names the keys jobs call, every unnamed key is `none`, `permissions: {}` for none                   |
|  [03]   | Concurrency  | `group` by workflow and ref, `cancel-in-progress` as an expression, `queue: max` excludes `cancel-in-progress`    |
|  [04]   | Checkout     | `persist-credentials: false` unless the job pushes, `fetch-depth: 0` for `nx-set-shas` and `nx affected`          |
|  [05]   | Cache        | `actions/cache` keyed by `hashFiles` over the files that decide the contents, no `restore-keys`                   |
|  [06]   | Shell        | `defaults.run.shell` covers workflow `run` steps, each composite `run` step declares `shell`                      |
|  [07]   | Expressions  | `${{ }}` values reach a script through the step `env` map                                                         |
|  [08]   | Status check | Fan-in job with `needs` over every job and `if: always()`, failing on a `needs.<job>.result` other than `success` |

- Caches restore an exact `key` match and save under it when the job succeeds, a key over a manifest with `latest` rows restores the first save
- Jobs skipped by a conditional or a failed dependency pass as a required status check and read `skipped` in `needs.<job>.result`
