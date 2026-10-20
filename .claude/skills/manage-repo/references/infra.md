# [INFRA]

Pulumi's Automation API runs the typed inline program of each `infra/<program>.ts` module, GitHub Actions runs workflows of commands.

## [01]-[PROGRAM]

- Modules export `stack` (`projectName`, `stackName`) and `program`, or a function returning both from its arguments
- `main.ts` holds one subcommand per module
- `selectStack` opens an existing stack, `createOrSelectStack` one named from validated arguments, both with `workDir` `PULUMI_HOME/<projectName>`
- Hosting stacks are `<worker>.<target>` in project `rasm-hosting`
- Hosting targets are entries of the `targets` record, each mapping decoded binding types to their resources
- App bindings, `domains`, and fetch `triggers` come from `worker.config.json` in the app's Cloudflare Build Output, located through `readBuildOutput`
- Effect Schema decodes `worker.config.json`, a binding or trigger a target cannot provision fails the decode
- Data resources (D1, R2, buckets, volumes) take `protect` from the stack, `--disposable` selects `<worker>-disposable.<target>` with `protect` off
- `destroy` runs `previewDestroy` first, a protected stack refuses before any delete until `pulumi state unprotect`
- Signed-in identity reaches an app as `@rasm/hosting/sites.ts` `contract` headers alone, each front strips client copies
- Sites sets the headers, the Docker proxy copies them from its forward-auth answer, the Worker sets them from a verified Access JWT
- Generated credentials (store keys, owner password) are `RandomPassword` stack secrets
- Hyperdrive bindings take a migrator role owning a schema of its name and a runtime role with `USAGE` on that schema
- Migrator URLs bind as `migrator(<variable>)` from `@rasm/hosting/bindings.ts`, the one derivation infra and apps read
- Containers a host provider reaches publish on `127.0.0.1` from a non-internal network, a container on `internal` networks alone publishes no port
- Resource URNs hold stack, project, type, parent, and logical name, a module move keeps them and a rename takes `aliases`
- Root targets `infra` and `hosting` change state on `up` and `refresh` with no preview
- Operations report through typed `onEvent` engine events in place of CLI text
- Repository settings, labels, environments, and security features are rows, issue forms and workflows stay `.github/` files
- Repository name and logical names derive from the checkout's `origin` remote, the default provider derives the owner from `GITHUB_TOKEN`
- Resource families are one table that `satisfies` their provider's argument type, mapped through its constructor
- Arguments the provider sends a default for stay unset, an argument it leaves computed takes a value to stay owned
- `import: <id>` adopts an existing resource in the `up` that first declares it and goes once state holds it, `protect: true` refuses deletion
- Removed rows delete their resources through the provider their saved state names, the deleting `up` reads that provider's credential

## [02]-[WORKFLOWS]

| [INDEX] | [DECISION]  | [FORM]                                                                                                         |
| :-----: | :---------- | :------------------------------------------------------------------------------------------------------------- |
|  [01]   | Event       | Repository event or schedule that requires the work, `workflow_dispatch` with typed `inputs`                   |
|  [02]   | Permissions | `permissions` names the keys jobs call, every unnamed key is `none`, `permissions: {}` for none                |
|  [03]   | Concurrency | `group` by workflow and ref, `cancel-in-progress` as an expression, `queue: max` excludes `cancel-in-progress` |
|  [04]   | Checkout    | `persist-credentials: false` unless the job pushes, `fetch-depth: 0` for `nx-set-shas` and `nx affected`       |
|  [05]   | Cache       | `actions/cache` keyed by `hashFiles` over files deciding cache contents, no `restore-keys`                     |
|  [06]   | Shell       | `defaults.run.shell` covers workflow `run` steps, each composite `run` step declares `shell`                   |
|  [07]   | Expressions | `${{ }}` values reach a script through the step `env` map                                                      |

- Caches restore an exact `key` match and save under it when their job succeeds, a key over a file with `latest` rows restores the first save
- `jdx/mise-action` restores its data directory under a key rotating every 30 days, and `mise install` resolves each `latest` row again
- Composite steps for one ecosystem take `if: runner.os == '<os>'` when one runner alone runs its projects
- Jobs name `check` and `test:integration` in one `nx affected -t` step on the runner whose engine `rasm:docker` starts
- `rasm:upgrade:actions` alone moves action tags, each at its precision, with no Dependabot update or `pnpm update --include-github-actions` hash pin
