# [TODO2]

State after the close-out of November 10. `docs/research/infrastructure/plan.md` orders the units and `plan/` holds them by concern. Rows of `[01]` were checked against the tree and the accounts that day, rows of `[02]` are what no run has proven, and rows of `[03]` need the owner's accounts.

## [01]-[BUILT]

| [INDEX] | [CONCERN]             | [OWNER]                                                         | [PROOF]                                                |
| :-----: | :-------------------- | :-------------------------------------------------------------- | :----------------------------------------------------- |
|  [01]   | Doppler project       | `infra/repository-stack.ts`, four configs                       | `doppler configs --json`                               |
|  [02]   | Doppler scope         | `.config/mise.toml` `[env]` `DOPPLER_*` rows                    | `doppler configure debug --json`                       |
|  [03]   | CI secrets            | `ci` and `release` syncs in `infra/repository-stack.ts`         | `gh secret list`                                       |
|  [04]   | Deploy credentials    | `infra/stacks.ts` reads config `deploy`                         | `nx deploy @rasm/infra` takes no prefix                |
|  [05]   | CodeQL default setup  | `infra/repository-stack.ts`                                     | `gh api <repo>/code-scanning/default-setup`            |
|  [06]   | Deployed host         | `infra/platform-stack.ts`, `infra/host.sh`                      | `nx deploy @rasm/infra --stack=<host>`                 |
|  [07]   | DNS                   | Apex and wildcard `CNAME` in `infra/platform-stack.ts`          | `dig +short nx-cache.<domain>`                         |
|  [08]   | Nx remote cache       | `nx-cache` in `infra/compose.yaml`, cache rows in `ci.yml`      | `curl <server>/health` prints `OK`                     |
|  [09]   | Pulumi project name   | `infra/stacks.ts`, from `infra/package.json`                    | `pulumi stack ls --all`                                |
|  [10]   | `NODE_VERSION` row    | `.config/mise.toml`, from mise's resolved Node                  | `x-node` in `infra/compose.yaml` reads it              |
|  [11]   | Platform inputs       | `PLATFORM` in `apps/nx/devkit/inference.ts`                     | `nx show project <name> --json` lists it per target    |
|  [12]   | Local engine          | `[daemons.docker]` in `.config/mise.macos.toml`                 | `mise daemons start docker`                            |
|  [13]   | Session services      | `[daemons.services]`, `nx install @rasm/infra -c store`         | `docker ps` lists each service healthy                 |
|  [14]   | Store service         | `apps/data/store`, service `store` of `infra/compose.yaml`      | `nx run-many -t test` over the four support suites     |
|  [15]   | Store fixtures        | `tests/<language>/support` over `store.proto`                   | Each suite reaches the store through its generated client |
|  [16]   | Store links           | `@rasm/nx-docker` option `clients`, Dockerfile owner            | `nx show target inputs <suite>:<target> --check <file>` |
|  [17]   | Test configuration    | `tests/typescript/support`, `@rasm/nx-vite`, `@rasm/nx-playwright` | `nx run-many -t test--vitest test--playwright`      |
|  [18]   | Image tag upgrades    | `apps/nx/docker/executors/upgrade`                              | `nx run @rasm/infra:upgrade--compose --dryRun`         |
|  [19]   | Host access           | `@rasm/ssh`, `DEPLOYED_HOSTS` routes                            | A call answers over the tailnet and over the address   |

## [02]-[OPEN]

No run has proven these:
- `nx deploy @rasm/infra` after November 10: applies `DOPPLER_TOKEN` to environment `release` and the moved image tags to the host
- Workflow `deploy.yml` has never run
- Workflow `release.yml` was rebuilt per platform and has never run
- Backups: no restore from the rebuilt backup services (`pgbackrest info`, `clickhouse-backup list remote`, one query per restored store)
- Hosting program of `infra/hosting-stack.ts` has never been applied, Pulumi holds no portfolio stack
- Doppler config `dev` holds no remote cache server or read token, so local runs never read the cache CI fills
- Kafka rows stand in all four catalogs and in the Rust test support, against `decisions.md` [01]
- Pyroscope waits on an owner decision (`plan/definition.md`)

Test costs found and left:
- Rhino test host waits the platform's 300 s handshake when its boot script fails, and leaves `RhinoMonitor` running
- `check--dotnet-format` recompiles each batch Nx forms, 79 s to 177 s over 23 projects
- `Rasm.Analyzers.Tests` runs 63 `Samples` cases one after another, 37 s
- `sshd.test.ts` installs OpenSSH from the network into each container it starts
- Rust store endpoint of `tests/rust/support/store.rs` states no deadline

## [03]-[OUTSIDE]

Steps on accounts, each in `plan/release.md` with its registry values:
- Scope `@rasm` on npm belongs to another npm user, an owner decision on the scope every `@rasm/nx-*` package publishes under
- Trusted publisher policies on nuget.org and PyPI, set in a browser, then on npm through `npm trust`
- First release: `nx release --dry-run` stops on commit scopes `ui` and `rhino`, each matching more than one project
- Darkroom's first extensions.blender.org submission through the website
