# [INFRA]

`infra/` holds the Pulumi program on the Automation API for the repository's own resources, the GitHub repository settings and the Doppler project that holds its runtime secrets. The root `package.json` and the `pnpm-workspace.yaml` catalog pin every package, the root `tsconfig.json` includes the files, and every command runs from the root through Nx.

| [INDEX] | [FILE]          | [PURPOSE]                                                                                  |
| :-----: | :-------------- | :----------------------------------------------------------------------------------------- |
|  [01]   | `resources.ts`  | Typed declarations of the Doppler and GitHub resources, each with its import id and inputs |
|  [02]   | `program.ts`    | Inline program that registers the declared resources with explicit providers               |
|  [03]   | `automation.ts` | Entry that resolves credentials, selects the stack, and runs preview, up, and refresh      |

## [01]-[TARGETS]

Targets belong to the root project in the `nx` field of the root `package.json`, run uncached, and take `--import` after `--` on a first apply of imported rows.

| [INDEX] | [TARGET]                                          | [PROVES]                                                          |
| :-----: | :------------------------------------------------ | :---------------------------------------------------------------- |
|  [01]   | `nx run rasm-workspace:preview`                   | Diff of declared against live state, steady state is `{"same":N}` |
|  [02]   | `nx run rasm-workspace:preview:expect-no-changes` | Refreshed state plans no change, the drift check                  |
|  [03]   | `nx run rasm-workspace:up`                        | Prints the plan, asks for confirmation, then applies it           |
|  [04]   | `nx run rasm-workspace:refresh`                   | State reconciled with live provider reads                         |

## [02]-[CREDENTIALS]

Each credential comes from the ambient variable when set and from its store otherwise, and none is written to a file in the repository.

| [INDEX] | [VARIABLE]                 | [STORE]                                          |
| :-----: | :------------------------- | :----------------------------------------------- |
|  [01]   | `PULUMI_CONFIG_PASSPHRASE` | `op read op://Tokens/PULUMI_RASM_INFRA/password` |
|  [02]   | `DOPPLER_TOKEN`            | `doppler configure get token --plain`            |
|  [03]   | `GITHUB_TOKEN`, `GH_TOKEN` | `op read op://Tokens/GITHUB_TOKEN/token`         |

State sits at `file://${XDG_STATE_HOME:-$HOME/.local/state}/rasm-infra` with mode 0700 under the passphrase secrets provider, and provider plugins sit under the `PULUMI_HOME` that `mise.toml` sets. The Automation API writes the project file into a temporary directory, and the repository holds no `Pulumi.yaml`, no stack file, no `.env`, and no `doppler.yaml`.

## [03]-[CHANGES]

- Declare a live resource with `imported: true` and its import id, then run `preview -- --import` until it plans the import alone and `up -- --import`
- Remove a row to destroy its resource on the next `up`, the repository row holds `archiveOnDestroy`, and `program.ts` sets `protect: true` on it
- Write a secret value with `doppler secrets set <NAME> --project rasm --config <config>` from stdin, values stay out of the rows
- Issue a token for a new consumer as a service token row on its config, and hand it to GitHub as an Actions secret row

## [04]-[DEPENDENCIES]

- `@pulumi/pulumi` — Automation API, resource options, and secret outputs
- `@pulumi/github` — repository and Actions secret resources
- `@pulumiverse/doppler` — project, environment, branch config, and service token resources
- `pulumi` — the CLI the Automation API drives from `node_modules/.bin`
- `effect`, `@effect/platform`, `@effect/platform-node` — configuration, processes, filesystem, and terminal at the boundary
