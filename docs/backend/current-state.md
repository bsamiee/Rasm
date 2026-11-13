# [CURRENT_STATE]

Delivery and infrastructure of this repository on 2026-11-12 at commit `8faa4f724`, read from files, `git` history, live accounts, and tool documentation. `[Snn]` names row `[nn]` of section 17.

## [01]-[FINDINGS]

[SCOPE]: What exists
- `infra/` holds 9 files and 1610 lines: 893 TypeScript, 655 YAML, 15 Bash, 47 JSON
- Pulumi Cloud holds 2 stacks of project `rasm-infra`: `rasm` (39 resources) and one named after its host (21 resources)
- One Hostinger VPS (Ubuntu 26.04.1, 8 CPUs, 32 GB memory, 387 GB disk) runs 12 containers of one Compose project `rasm`
- No stack, container, or workflow run hosts an app, `apps/web/portfolio/.openai/hosting.json` names an OpenAI Sites project

[DEFECTS]: Ordered by distance from owner's stated intent
1. Hosts are not declared in code: `DEPLOYED_HOSTS` is a JSON value in Doppler, and each VPS is bought and created by hand
2. Pulumi runs shell commands in place of resources: 11 of 16 host-stack resources are `command:local:Command`
3. One `local.Command` named `<host>-services` pipes `compose.yaml` to `docker compose` over SSH and stands for 18 services and 8 volumes
4. Each Command takes a deploy timestamp in its environment, so `create` reruns at every deploy and no preview can report zero changes
5. `compose.yaml` is the largest file and embeds 3 Dockerfiles and 10 configuration documents in SQL, INI, XML, and YAML as strings
6. Every host receives that same file: no host has a role, a service list, or a size of its own
7. App placement is read from Pulumi's stack list: a deploy without `--stack` applies each stack that already exists
8. Deploys run from one Mac: workflows `Deploy`, `Release`, and `Renovate` have zero runs, and CI failed 7 of its 11 runs
9. `nx deploy` applies without a preview since commit `9224d3244`, and stacks of one run apply at once with no order
10. Images build on the production host from folders copied by `scp`, and no registry holds an image
11. `BACKUP_ENDPOINT` names the storage service of that same host, so backups sit on the disk they protect
12. No command, target, skill, or guide tells an agent how to reach, inspect, or manage a host
13. Testcontainers rows sit in 4 ecosystem catalogs with one consumer, after a 2-day container test layer was deleted
14. Tools already installed cover hand-written parts: mise bootstrap, `remote.Command`, the Pulumi Docker provider, and Hostinger's VPS API

## [02]-[LIVE_STATE]

Read-only commands produced each value on 2026-11-12.

| [INDEX] | [SUBJECT]        | [VALUE]                                                                                            |
| :-----: | :--------------- | :------------------------------------------------------------------------------------------------- |
|  [01]   | State backend    | Pulumi Cloud, organization `bsamiee`, personal token in `.cache/pulumi/credentials.json`           |
|  [02]   | Secrets provider | `service` in both stacks                                                                           |
|  [03]   | Stack `rasm`     | 33 resources besides providers: 16 GitHub, 8 Doppler, 4 Tailscale, 4 Cloudflare, 1 `local.Command` |
|  [04]   | Host stack       | 16 resources besides providers: 11 `local.Command`, 2 DNS records, 1 tailnet key, 1 webhook, 1 key |
|  [05]   | App stacks       | None                                                                                               |
|  [06]   | Host containers  | `traefik`, `authelia`, `postgres`, `pgbouncer`, `clickhouse`, `storage`, `otelcol`, `grafana`      |
|  [07]   | Host containers  | `nx-cache`, `hooks`, `backup`, `clickhouse-backup`                                                 |
|  [08]   | Host boot        | 2026-11-10 04:54                                                                                   |
|  [09]   | Host Docker      | Engine `29.10.0-rc.1` from Docker's `test` apt channel                                             |
|  [10]   | Local containers | 7 of project `rasm` under Colima, default passwords                                                |
|  [11]   | Workflow runs    | `CI` 11 (2 passed, 7 failed, 1 cancelled, 1 running), `Deploy` 0, `Release` 0, `Renovate` 0        |
|  [12]   | Branch `main`    | No protection, one ruleset `release tags`                                                          |
|  [13]   | Environments     | `deploy` and `release`, no protection rule, no branch policy                                       |
|  [14]   | Doppler `deploy` | 37 rows, 12 with no reader in this repository, 2 more replaced by computed values                  |
|  [15]   | `BACKUP_KIND`    | `s3`, endpoint `https://storage.<PLATFORM_DOMAIN>`                                                 |

Rows of Doppler `deploy` no file reads: `BACKBLAZE_ACCESS_KEY_ID`, `BACKBLAZE_ENDPOINT`, `BACKBLAZE_REGION`, `BACKBLAZE_SECRET_ACCESS_KEY`, `NEON_API_KEY`, `GF_SECURITY_ADMIN_PASSWORD`, `HOSTINGER_VPS_<id>_SSH_PRIVATE_KEY`, `HOSTINGER_VPS_<id>_SSH_PUBLIC_KEY`, `HOSTINGER_VPS_ROOT_PASSWORD`, `DEPLOYED_SSH_KNOWN_HOSTS`, `DOCKERHUB_TOKEN`, `DOCKERHUB_USERNAME`. Code computes `PLATFORM_DOMAIN` from `DEPLOYED_HOSTS` and `TAILSCALE_WEBHOOK_SECRET` from a `Webhook` resource, and both computed values replace their Doppler rows. Pulumi CLI reads `PULUMI_ACCESS_TOKEN` from its environment, so that row has a reader no file names.

Host workload today is repository tooling: an Nx remote cache, a receiver of Tailscale webhook events, and Grafana over telemetry of those services. Service `postgres-catalog` creates databases `grafana` and `hooks`, and no app database exists. Service `cache` (Valkey) has profile `app`, which no deploy enables, so it never starts on a host.

## [03]-[DEPLOY_FLOW]

`nx deploy @rasm/infra` on a Mac performs these steps:
1. Nx builds `@rasm/hooks` with Vite, since `deploy` depends on `^build`
2. Executor `@rasm/nx-pulumi:stacks` imports `infra/stacks.ts` and calls `stacks(request)`
3. `command.ts` runs `doppler secrets download --config deploy` and passes every row to each Pulumi workspace as process environment
4. `repository-stack.ts` runs `gh repo view` and `doppler configure get token`, then declares GitHub, Doppler, Tailscale, and Cloudflare resources
5. `platform-stack.ts` runs `hostinger vps virtual-machines list`, finds each `DEPLOYED_HOSTS` name, and reads its memory size
6. `platform-stack.ts` runs `docker compose config` twice on the Mac to list variables and build contexts of `compose.yaml`
7. `tls.PrivateKey` creates a key, and one `local.Command` attaches its public half through `hostinger vps ssh-keys add-virtual-machine`
8. `@rasm/ssh` writes key, `config`, and `known_hosts` under `.cache/ssh/<host>/`
9. One `local.Command` sends `host.sh` over SSH, then 6 chained Commands write 6 files with `install -D` and reload their units
10. One `local.Command` runs `tailscale up` with a `TailnetKey` on stdin
11. One `local.Command` per build context runs `rm -rf`, `mkdir -p`, and `scp -r` of `.artifacts/vite/@rasm/hooks/node`
12. Command `<host>-services` runs `docker compose --file - up --build --detach --wait --remove-orphans` over SSH with secrets as `SendEnv` variables
13. Executor calls `stack.up({ refresh: true })` for every stack at once and prints one JSON line per stack

`nx deploy @rasm/portfolio --stack=<row>` follows `hosting-stack.ts`: it resolves the Vite config, runs `docker compose config` on `compose.yaml` to read ports and credentials, opens an SSH forward to Postgres, declares roles, databases, and buckets through providers, writes 2 temporary Compose documents, and pipes a third to `docker compose up --build` on its target.

## [04]-[HOST_SH]

`infra/host.sh` is 15 lines of Bash that add Docker's and Tailscale's apt sources and install each package named in its arguments.

[CAUSE]:
- Commit `59b914b62` (2026-11-09) added it with `platform-stack.ts`, the first commit that configured a VPS
- `equipped` in `platform-stack.ts` reads it as text and sends it as stdin of `ssh <host> bash -s -- <packages>` through one `local.Command`
- Hosts exist before any code runs, bought in Hostinger's panel, so no code path supplies first-boot configuration
- Program reaches a root shell over SSH and must install Docker there before `docker compose` can run
- Provider `@pulumi/command` runs shell text alone, so host setup had to be shell text [S03]
- First version (23 lines) also wrote 5 files with `printf`, commit `a3f12ee4f` moved those into table `files` of `platform-stack.ts`

[DEFECTS]:
- Host configuration sits in 3 places and 2 languages: `host.sh`, table `files`, and constant `packages`
- Script runs when its Command is created or its text changes, so installed packages never upgrade [S03]
- Nothing reads host state back, a file edited on a host stays edited and Pulumi reports no difference
- `lsb_release` and `apt-get` bind every host to Debian or Ubuntu
- Docker's source line states channels `stable test`, so each host installs release candidates of its engine
- `mise bootstrap` declares apt sources, packages, files, services, firewall rules, and Compose projects, with `remote` over SSH [S06]
- Hostinger CLI 4.20.4 lists `purchase`, `setup`, `recreate`, `post-install-scripts`, and `firewall`, code calls `list` and `ssh-keys` alone [S10]

## [05]-[COMMAND_TS]

`infra/command.ts` is 12 lines with 2 exports: `run` returns stdout of a child process or fails with its exit code, and `deployed` returns Doppler config `deploy` as a string record.

[CAUSE]:
- Commit `59b914b62` added `run`, commit `a3f12ee4f` added `deployed`, commit `034f30452` rebuilt `run` on `Completed.output` of `@rasm/process`
- `stacks.ts` and all 3 stack files read one or both exports
- `hosting-stack.ts` and `repository-stack.ts` import `platform-stack.ts`, so a module below all 3 avoids an import cycle

[DEFECTS]:
- File is correct TypeScript, and its existence follows from how programs get facts
- Programs call 4 CLIs at 8 places and parse printed JSON: `docker compose config` (4), `doppler` (2), `hostinger` (1), `gh` (1)
- `@pulumiverse/doppler` is a dependency, yet secrets arrive through `doppler` CLI text
- `docker compose config` serves as YAML parser for TypeScript, which makes Compose CLI a dependency of every plan
- Root `CLAUDE.md` forbids tiny files and extracted helpers, this file is both

## [06]-[INFRA_FILES]

### [06.1]-[COMPOSE_YAML]

`infra/compose.yaml` (655 lines) declares 18 services, 3 template services under `x-hosted-app`, 10 inline configuration documents, 8 volumes, and 5 profiles.

[CAUSE]:
- Commit `b259b6d93` (2026-11-09) added it at 190 lines for local development services
- Commit `59b914b62` made it each host's service definition, commit `a3f12ee4f` changed 747 of its lines
- 15 commits changed it between 2026-11-09 and 2026-11-12

[COUPLING]:
- `platform-stack.ts` sends its raw text as stdin of `docker compose --file -` on each host
- `hosting-stack.ts` decodes its `docker compose config` JSON with schema `Platform`, naming 5 services and config `authelia`
- `hosting-stack.ts` parses YAML text of config `authelia` to learn session domain and sign-in address
- `x-hosted-app` is a template `hosting-stack.ts` copies into each app's Compose document through `extends`
- `@rasm/nx-docker` infers targets `check--compose`, `check--buildx`, `build--compose`, and `install--compose` from it
- Step `compose` of hk validates it, CI builds its images through `build--compose`
- Doppler `deploy` rows fill its `${VARIABLE:-default}` slots, each default a literal password (`postgres`, `rustfsadmin`)

[DEFECTS]:
- Owner asked for no Dockerfile and no Compose file: Dockerfiles moved into 3 `dockerfile_inline` strings and stayed
- 10 documents in 4 other languages sit in YAML strings (psql, INI, XML, YAML), with no checker reading any
- Postgres image, version, extension list, and preload list are text in a `RUN` line and a `command` list
- Binding `postgres` of `App` has no fields, so an app cannot name an extension, a version, or a setting
- One file serves local development and every host, differing by 5 profiles and 32 variables
- Anchors and `<<` merges carry reuse (`&server`, `&node`, `&pgbackrest`), a second language of composition beside TypeScript
- `backup-bucket` holds `restart: on-failure:6` with a comment about Traefik timing, a retry around a readiness race
- Service `backup` mounts volume `authelia` alone, volumes `storage`, `grafana`, `traefik`, and `otelcol` have no backup
- Anchor `&retention "30"` feeds settings named in days and `BACKUPS_TO_KEEP_REMOTE`, a setting named as a count
- Image tags are literals Renovate must move, and Renovate has no run on record

### [06.2]-[PLATFORM_STACK_TS]

`infra/platform-stack.ts` (307 lines) declares one stack per `DEPLOYED_HOSTS` entry: an SSH key, apt setup, 6 files, tailnet join, copied build contexts, 2 DNS records, and Compose services.

[CAUSE]: Commit `59b914b62` added it at 82 lines when a Hostinger VPS first entered, commit `a3f12ee4f` grew it to its present reach.

[COUPLING]:
- Tables `machines`, `records`, and `backups` each hold 1 or 2 rows: `hostinger`, `cloudflare`, `s3` and `azure`
- Schema `Deployed` names 11 secret rows Doppler must hold before any plan
- `hooks.<domain>` and `paths.tailscale` of `@rasm/hooks` bind a tailnet webhook to an app in this repository
- Exports `backupTarget`, `connections`, `Deployed`, `definition`, `identified`, and `uploaded` serve both other stack files

[DEFECTS]:
- Host list, host key, DNS zone, tags, and routes come from one JSON string in a secret store, outside review and history
- VPS creation, operating system choice, disk, firewall, and snapshots are absent, `machines.hostinger` only lists and attaches a key
- 6 `local.Command` sites hand-build `ssh` and `scp` lines, where `remote.Command` and `remote.CopyToRemote` take a connection [S03]
- `PLATFORM_APPLIED` holds `Clock.currentTimeMillis`, so inputs of `<host>-services` differ on every run and `create` runs again [S03]
- Command `<host>-services` deletes with `docker compose down` and has `protect: true`, its state says nothing about one service
- `backups.s3` and `backups.azure` restate one target as 22 and 16 variables for 4 backup tools
- Constant `profiles` fixes `platform` and `deployed` for every host
- Postgres and ClickHouse memory settings divide memory Hostinger reports, with every host given both stores
- Root SSH private key is a stack output and a file under `.cache/ssh`, a cache folder holds a credential

### [06.3]-[HOSTING_STACK_TS]

`infra/hosting-stack.ts` (315 lines) deploys one `App` from a project's `hosting.config.ts` to a row: `local`, `cloudflare`, or a host name.

[CAUSE]: Commit `814d77aa1` (2026-10-18) added `infra/hosting.ts` at 303 lines, and commit `2384f05bb` renamed it.

[COUPLING]:
- `App` of `@rasm/hosting/app` supplies 6 binding kinds: `sqlite`, `postgres`, `bucket`, `secret`, `text`, `json`
- Row `cloudflare` maps them to D1, Hyperdrive, R2, and Worker bindings through `deployApp`
- Host rows map them to a volume with Litestream, a database with 2 roles in shared Postgres, and a RustFS bucket
- `bundled` requires Nx target `build--vite` and a Vite environment named `node`
- `StackReference` reads `connection` and `identity` outputs of a host stack

[DEFECTS]:
- This binding model is nearest to owner's rows, and it is the one part with a provider choice
- Choice is per deploy, not per part: an app on a host cannot take hosted Postgres or a bucket at another vendor
- One app shape exists: a Vite server bundle on distroless Node, so Rust, Python, and .NET services cannot deploy
- Containers outside an app have no form: no sandbox, no standalone database, no one-off tool container
- `nx install @rasm/portfolio` records stack `portfolio-disposable.local` in Pulumi Cloud for containers on one developer's machine
- `APPLIED` timestamp forces `docker compose up --build` on every deploy, as in `platform-stack.ts`
- Module-level `ManagedRuntime` holds SSH forwards open until process exit
- No stack of this file exists today, so its 315 lines have no live result to compare with

### [06.4]-[REPOSITORY_STACK_TS]

`infra/repository-stack.ts` (167 lines) declares stack `rasm`: repository settings, labels, a tag ruleset, 2 environments, Doppler project with 4 configs and 2 secret syncs, tailnet policy, a federated identity, Zero Trust settings, and an R2 bucket for app updates.

[CAUSE]: Commit `7d0c24c95` (2026-09-03) started `infra/` with this program alone, importing an existing repository and Doppler project.

[DEFECTS]:
- Name says repository, content spans 4 vendors, and tailnet policy and Zero Trust settings belong to no repository
- Program reads Doppler config `deploy` before it runs and also declares that Doppler project, so a first run needs Doppler made by hand
- `gh repo view` and `doppler configure get token` need personal logins, workflow `deploy.yml` passes neither
- Code scanning is one `local.Command` over `gh api`, a shell call beside 16 provider resources
- Repository `Rasm-Assets` stays declared after commit `9ea493005` removed its Nx, CI, and mise integration
- Tailnet policy grants every source every destination, and no rule protects branch `main`

### [06.5]-[STACKS_TS]

`infra/stacks.ts` (92 lines) is the package entry: `stacks(request)` returns Pulumi inline programs for an `Install`, `Deploy`, or `Release` request.

[DEFECTS]:
- `held` lists existing stacks and returns matching rows, else row `local`, so backend state chooses where an app deploys
- First deploy to a host therefore needs `--stack=<host>` typed once by hand, and no file records that choice
- `request.context.projectName === manifest.name` splits one function into 2 programs by a name comparison
- Repository stack and host stacks return together and apply at once, though a tagged `TailnetKey` needs tag owners of stack `rasm`

### [06.6]-[PACKAGE_JSON_AND_TSCONFIG]

`infra/package.json` names package `@rasm/infra`, exports `stacks.ts`, and lists 11 Pulumi and provider packages. `infra/tsconfig.json` extends root options with `noEmit`. Dependency `@rasm/nx-pulumi` under `devDependencies` is what makes `@rasm/nx-pulumi` infer target `deploy`. Root catalog also pins `@pulumi/docker`, `@pulumi/docker-build`, `@pulumi/cloudinit`, `@pulumi/kubernetes`, `@pulumi/gcp`, `@pulumi/awsx`, `@pulumi/eks`, `@pulumi/policy`, `@pulumi/esc-sdk`, `@pulumi/pulumiservice`, `@pulumi/synced-folder`, `@pulumiverse/acme`, and `@pulumiverse/grafana`, which no package declares.

## [07]-[PULUMI_USE]

[MEASURE]: Against Pulumi's own documents
- Automation API fits: its page lists CI/CD, multi-stage deployments, and higher-level tools as uses, with inline programs [S01]
- Command provider describes itself as running scripts on `create` and `destroy`, with Terraform provisioners as comparison [S03]
- `create` reruns when inputs change and no `update` exists, a timestamp input turns that rule into run-always [S03]
- Pulumi's Docker provider manages containers and images through Docker's API and accepts `ssh://user@host` as host [S04]
- Pulumi's GitHub Actions guide runs `preview` on each pull request and `up` on push to `main` [S02]

[RESULT]:
- Stack `rasm` uses Pulumi as documented: provider resources with real identities and diffs
- Host stack uses Pulumi as an ordered script runner, 5 of its 16 resources have a provider that can read them back
- No pull request previews a stack, and no merge applies one

## [08]-[GITHUB_FILES]

| [INDEX] | [FILE]                     | [PURPOSE]                                                    |
| :-----: | :------------------------- | :----------------------------------------------------------- |
|  [01]   | `workflows/ci.yml`         | `nx affected --targets=check,test,build` on Linux and macOS  |
|  [02]   | `workflows/deploy.yml`     | `nx deploy @rasm/infra --stack=<input>` on manual dispatch   |
|  [03]   | `workflows/release.yml`    | `nx release`, `attach`, and `nx release publish` on dispatch |
|  [04]   | `workflows/renovate.yml`   | Renovate on a daily schedule                                 |
|  [05]   | `actions/setup/action.yml` | `mise` tools, `pnpm install`, Xcode path, cache server probe |
|  [06]   | `renovate.json5`           | `config:best-practices`, automerge, lock maintenance         |
|  [07]   | `jactionlint.yaml`         | Workflow lint settings                                       |
|  [08]   | `ISSUE_TEMPLATE/*.yml`     | 2 forms, `repository-stack.ts` reads their labels            |

GitHub Actions reads workflows as YAML files under `.github/workflows/` [S02], so those 4 workflow files cannot become code.

[DEFECTS]:
- No workflow deploys on merge, on tag, or on release, so a merged infrastructure change reaches no host
- `deploy.yml` takes a typed stack name and has never run, so its Tailscale identity path is untested
- `deploy.yml` covers `@rasm/infra` alone, no workflow deploys an app, and none runs `deploy -c release`
- Stack `rasm` cannot apply in `deploy.yml`: job holds no `GH_TOKEN`, and its Doppler token is a read token of one config
- `release.yml` reads 7 secrets GitHub does not hold: 2 `DEVELOPER_ID_*`, 3 `ASC_API_*`, `NODE_AUTH_TOKEN`, `UV_PUBLISH_TOKEN`
- CI job `linux` failed in step `nx affected` in 7 runs, and no run passed after 2026-11-12 11:34 UTC
- CI's remote cache is a container on that one host, `actions/setup` continues without it when its probe fails
- `actions/setup` spells `Xcode_27.2.app` as a literal path
- Renovate automerges with no required status check, by a comment in `renovate.json5`

## [09]-[MISE_FILES]

| [INDEX] | [FILE]                    | [DELIVERY_CONTENT]                                                                           |
| :-----: | :------------------------ | :------------------------------------------------------------------------------------------- |
|  [01]   | `.config/mise.toml`       | Tools `pulumi`, `doppler`, `docker-compose`, `docker/buildx`, `docker-cli`, `colima`, `lima` |
|  [02]   | `.config/mise.toml`       | Tools `github:hostinger/api-cli`, `conda:openssh` on Linux, `crane`, `pitchfork`             |
|  [03]   | `.config/mise.toml`       | Variables `DOPPLER_PROJECT`, `DOPPLER_CONFIG=dev`, `PULUMI_HOME`, `NODE_VERSION`             |
|  [04]   | `.config/mise.macos.toml` | `DOCKER_HOST`, `COLIMA_HOME`, `TESTCONTAINERS_DOCKER_SOCKET_OVERRIDE`, daemon `docker`       |
|  [05]   | `.config/mise.macos.toml` | `[bootstrap.macos.launchd.agents.pitchfork]`, a login agent for the daemon supervisor        |
|  [06]   | `.config/miserc.toml`     | `auto_env = true`, which loads `mise.macos.toml` on a Mac [S07]                              |
|  [07]   | `.config/hk.pkl`          | Step `compose` runs `docker compose --profile '*' config --quiet` on each Compose file       |

[DEFECTS]:
- `PULUMI_HOME` is `.cache/pulumi`, so a Pulumi Cloud login lives in a folder named and treated as cache
- `NODE_VERSION` flows from mise to `platform-stack.ts` to a Compose build argument to `mise install node` inside an image
- `docker-cli` installs from Docker's `test` channel, matching host engine `29.10.0-rc.1`
- `crane` is installed and no file calls it
- `[bootstrap]` holds one macOS agent, while mise bootstrap parts for packages, files, services, and remote hosts go unused [S06]
- `[daemons]` runs Colima alone, mise daemons also run containers and service presets (`postgres`, `redis`) per project [S07]
- `mise daemons status` lists `rasm/services` as completed, a daemon no config declares now
- Rows for Colima and Testcontainers match Testcontainers' Colima page [S09]

## [10]-[NX_PLUGINS]

| [INDEX] | [PATH]                                           | [ROLE]                                                                 |
| :-----: | :----------------------------------------------- | :--------------------------------------------------------------------- |
|  [01]   | `apps/nx/pulumi/plugin.ts`                       | Infers `deploy` and `install--pulumi` from `package.json` dependencies |
|  [02]   | `apps/nx/pulumi/executors/stacks/executor.ts`    | Loads a program package, applies each stack through Automation API     |
|  [03]   | `apps/nx/docker/plugin.ts`                       | Infers Compose check, build, install, and serve targets                |
|  [04]   | `apps/nx/docker/ready-check.ts`                  | Waits until Compose services run and pass health checks                |
|  [05]   | `apps/nx/devkit/executors/attach`                | `gh release upload --clobber` of packaged files                        |
|  [06]   | `apps/nx/devkit/executors/publish`               | Publish command of one tool over packaged files                        |
|  [07]   | `apps/nx/distribution/executors/install`         | Host installer for a local build                                       |
|  [08]   | `apps/nx/distribution/executors/release-publish` | Host store upload                                                      |

[DEFECTS]:
- Target `deploy` appears on any package depending on a program package, so `@rasm/portfolio` gains it through `@rasm/infra`
- Executor refuses a dry run for `Deploy` and `Install` (commit `9224d3244`), `preview` exists for `Release` alone
- Executor applies stacks with `concurrency: 'unbounded'` and no dependency order between them
- One failed stack prints an error line and others continue, so a run can end half applied
- `served` and `ready-check.ts` of `@rasm/nx-docker` serve no project: no service names a Dockerfile path, and no project holds `serve--compose`
- Install and deploy mean 3 things under 2 names: `install--compose` starts local services, `install--pulumi` deploys an app locally, `deploy` applies stacks
- `tests/typescript/nx/inference/pulumi.test.ts` and `docker.test.ts` test inference, and no test covers a program of `infra/`

Executors `attach`, `publish`, `install`, and `release-publish` deliver packages to GitHub releases, registries, and desktop hosts. They read build outputs through Nx, hold no server concern, and showed no defect in this reading beyond `Release` never having run.

## [11]-[LIBRARIES]

| [INDEX] | [PACKAGE]        | [PATH]                               | [USE_IN_INFRA]                                              |
| :-----: | :--------------- | :----------------------------------- | :---------------------------------------------------------- |
|  [01]   | `@rasm/hosting`  | `libs/typescript/web/hosting`        | `App`, `Access`, `Sites`, `deployApp`                       |
|  [02]   | `@rasm/workers`  | `libs/typescript/runtime/workers`    | `deployWorker` behind `deployApp`                           |
|  [03]   | `@rasm/ssh`      | `libs/typescript/network/ssh`        | `Host`, `Ssh`, `Forward`: OpenSSH config, shared connection |
|  [04]   | `@rasm/process`  | `libs/typescript/runtime/process`    | `Completed.output` behind `run`                             |
|  [05]   | `@rasm/webhooks` | `libs/typescript/messaging/webhooks` | `TailscaleEvent` kinds for a webhook subscription           |
|  [06]   | `@rasm/hooks`    | `apps/web/hooks`                     | Route path of a webhook, image `rasm-hooks`                 |

[DEFECTS]:
- `@rasm/hosting` mixes page rendering, sign-in identity, a Vite plugin, a Node server, and Pulumi deployment in one package
- `@rasm/hosting/pulumi` deploys to Cloudflare alone, host deployment sits in `infra/hosting-stack.ts`, so one concept has 2 owners
- `App` is a web app record: `limits`, `allowed` sign-in addresses, and a Vite bundle contract, with no service, job, or database form
- `@rasm/ssh` (252 lines) rebuilds connection sharing, host key pinning, and forwards that `remote.Command` connections state as fields [S03]
- `@rasm/ssh` exposes no command, so its config files under `.cache/ssh` are reachable only by a caller who knows that path
- `deployApp` empties an R2 bucket through a Node script inside a `local.Command` string

## [12]-[CONTAINER_TESTS]

[TODAY]:
- One suite starts containers: `libs/typescript/network/tests/ssh/sshd.ts` builds a 2-line `Dockerfile` and starts 2 `sshd` containers
- That suite matches Testcontainers' model: containers per run, mapped ports, a private network, disposal after tests [S08]
- Catalog rows remain for 8 npm packages, 7 NuGet packages, 2 crates, and 1 Python distribution, with that one npm consumer

[HISTORY]:
- Commit `a3f12ee4f` (2026-11-10) added container fixtures per language and a Rust gRPC `store` service that provisioned databases for tests
- `apps/nx/devkit/plugin.ts` held a default that made every `test` target wait on `build--compose` and `serve--compose` of its dependencies
- `tests/typescript/support/compose.ts` wrapped `DockerComposeEnvironment` and started service `cache` from `infra/compose.yaml`
- `.config/mise.toml` then held `POSTGRES_DURABILITY=off`, `POSTGRES_MAX_CONNECTIONS=400`, and 5 ClickHouse drop timings for tests
- Commit `dd11faf1a` (2026-11-12) deleted that layer, the store, and its clients: 8426 lines in 124 files

[DEFECTS]:
- Testcontainers exists so each run owns isolated services and no shared one is prepared beforehand [S08]
- Removed layer did the reverse: one long-lived Compose project, shared by every test target, with test tuning in its service file
- Removed layer also put test tuning variables into the file that defines production services
- Remaining catalog rows describe modules (`Kafka`, `LocalStack`, `MinIO`, `ClickHouse`) no test imports

## [13]-[INTENT_COMPARISON]

| [INDEX] | [OWNER_INTENT]                              | [TODAY]                                                                    |
| :-----: | :------------------------------------------ | :------------------------------------------------------------------------- |
|  [01]   | Pure code, no Dockerfile, Compose, or YAML  | 655 YAML lines, 3 inline Dockerfiles, 1 test Dockerfile, 15 Bash lines     |
|  [02]   | Infrastructure covers repository            | Met by stack `rasm`                                                        |
|  [03]   | Infrastructure covers VPS                   | Existing VPS configured over SSH, creation by hand                         |
|  [04]   | Simple remote management for agents         | None, an agent must read `@rasm/ssh` to find `.cache/ssh/<host>/config`    |
|  [05]   | Self-hosted or hosted per part              | Per app deploy target, backups by `BACKUP_KIND`, nothing per part          |
|  [06]   | Hosted backups at any vendor                | `s3` and `azure` tables, endpoint set to own host                          |
|  [07]   | One configuration source of rows            | Doppler JSON, `compose.yaml`, `hosting.config.ts`, and 3 TypeScript tables |
|  [08]   | Define any number of hosts                  | N hosts possible, each an identical copy of one file                       |
|  [09]   | Host, container, and app as separate things | Host and platform services are one stack, an app is the only other form    |
|  [10]   | Container for any purpose                   | No form exists                                                             |
|  [11]   | Postgres with extensions as one row         | 6 extensions in an image `RUN` line, binding `postgres` takes no field     |
|  [12]   | Newest versions without upkeep              | Literal tags in YAML, moved by a Renovate workflow with no run             |
|  [13]   | Sockets and ports already right             | Unix socket volume for Grafana and `hooks`, loopback ports in YAML         |
|  [14]   | No manual deploy or hand sync               | Laptop `nx deploy`, `scp` of bundles, builds on host                       |
|  [15]   | Tests with containers done properly         | One correct suite, unused catalog rows, removed shared layer               |
|  [16]   | Metrics and telemetry placed by category    | ClickHouse through one collector for all signals, no row to choose it      |

Owner's phrase "IaC of a single host" is accurate in one sense and understated in another: code accepts many hosts, and each would be that single host again.

## [14]-[PACKAGE_BOUNDARIES]

Findings of the design and tool reviews about this repository, moved here from their documents.

[BETWEEN_PACKAGES]:
- `infra/` holds deployment logic, and library `@rasm/hosting` holds an `App` model and a Workers deployer, which inverts logic and rows
- `@rasm/workers` exports `deployWorker` and `workerConfig` beside its runtime entry, which couples a worker runtime to infrastructure
- `queue` and `scheduled` of `@rasm/workers` take Cloudflare's `Message` and `ScheduledController`, which ties handler code to one vendor
- `@rasm/hosting/node` exports `serveNode`, an HTTP server alone, and nothing self-hosted consumes a queue or runs on a schedule
- `stacks.ts` and `hosting-stack.ts` take `ExecutorContext` of `@nx/devkit`, and `repository-stack.ts` imports release internals of `nx`
- `App.Binding` is a fixed union with no cache case, and a new type edits a library and both deployers
- Bucket lifecycle is written once per hosting target with different resource types, and `infrequentAccess` exists on one target alone

[ENGINE_AND_DAEMON]:
- `.config/mise.macos.toml` declares daemon `docker` with `run`, `ready_cmd`, `stop_signal`, `logs`, and `auto`, read by mise and its pitchfork supervisor
- `mise bootstrap` registers pitchfork as a `launchd` agent, so `launchd` keeps pitchfork running and pitchfork keeps Colima running
- Linux developer computers get a Docker client and no row that installs or starts a daemon
- `compose.yaml` states restart, stop, logging, and health check defaults as YAML anchors, and `platform-stack.ts` writes Docker's daemon settings
- Each value above is correct for one repository and unreachable for any other, since a tool configuration file holds it

[PULUMI_PINS_AND_CREDENTIALS]:
- Catalog pins `@pulumi/pulumi` `3.275.0-alpha`, `@pulumi/command` `1.4.0-alpha`, and `@pulumi/tailscale` `1.0.0-alpha`, and `.config/mise.lock` holds CLI 3.274.1
- `PULUMI_ACCESS_TOKEN`, Cloudflare, Hostinger, and Doppler tokens are long-lived
- Six host files are created by commands with no delete, so removing one from code leaves it on each host
- Executor applies stacks with unbounded concurrency, and the tool review read Pulumi Cloud Free as allowing 1 concurrent stack update

[CAUSE]: One platform definition was written for one machine and then repeated per machine name. A row here names a machine that receives everything, where machine, service, app, daemon, and worker are separate kinds.

[MATCHES_INTENT]: App rows beside each app (`hosting.config.ts`), separate migrator and runtime database roles per app, protection of kept data against disposable data, federated CI identity on a private network, and a typed SSH host model.

## [15]-[ADOPTION]

Successor of each present file once the backend product of `target-architecture.md` exists and this repository consumes it:

| [INDEX] | [FILE]                                            | [SUCCESSOR]                                        |
| :-----: | :------------------------------------------------ | :------------------------------------------------- |
|  [01]   | `repository-stack.ts`                             | Account rows, logic in builder                     |
|  [02]   | `platform-stack.ts`                               | Machine rows, base system in builder               |
|  [03]   | `hosting-stack.ts`                                | App level of builder                               |
|  [04]   | `stacks.ts`                                       | Reconcile entry of builder behind an Nx target     |
|  [05]   | `compose.yaml`                                    | Catalog members and container defaults             |
|  [06]   | `host.sh`                                         | Machine base system of builder                     |
|  [07]   | `command.ts`                                      | None                                               |
|  [08]   | `@rasm/workers` runtime modules                   | Workers package                                    |
|  [09]   | `@rasm/workers` deploy and config modules         | Vendor package of builder                          |
|  [10]   | `@rasm/hosting` `pulumi` and `cloudflare` modules | App level of builder                               |
|  [11]   | Daemon rows of `.config/mise.macos.toml`          | Engine shape with a daemon value                   |
|  [12]   | Testcontainers catalog rows without an importer   | None, fixtures package where a test needs a server |
|  [13]   | `hostinger`, `gh`, and `doppler` CLI calls        | Vendor packages                                    |

`infra/` is rebuilt and not ported, since its files hold no row model to keep. Order follows: product first in its own repository, this repository second.

## [16]-[UNVERIFIED]

- Unverified: whether `deploy.yml` succeeds for a host stack; `gh workflow run deploy.yml -f stack=<host>` settles it and changes a host
- Unverified: whether Tailscale refuses a tagged `TailnetKey` before stack `rasm` sets tag owners; Tailscale's auth key API page settles it
- Unverified: whether `DOCKER_HOST=ssh://` transfers build contexts so no `scp` is needed; Docker's page states only that `DOCKER_HOST` selects a remote host [S05]
- Unverified: cron schedule of image `offen/docker-volume-backup` when `BACKUP_CRON_EXPRESSION` is absent; its reference page settles it
- Unverified: unit of `BACKUPS_TO_KEEP_REMOTE` in `altinity/clickhouse-backup` 2.8.3; its README settles it
- Unverified: whether a restore from any backup works; no restore command or test exists in this repository
- Unverified: why Renovate has no run 2 days after its workflow was added; `gh run list --workflow renovate.yml` after 01:17 UTC settles it
- Unverified: how `@rasm/portfolio` reaches OpenAI Sites; no workflow, target, or program here deploys to it
- Partly read: Pulumi's GitHub Actions guide, through its first workflow example pair [S02]
- Partly read: `local/command.d.ts` through `triggers`, and `README.md` through its use list [S03]

## [17]-[SOURCES]

| [INDEX] | [SOURCE]                                                                                       | [VERSION]                |
| :-----: | :--------------------------------------------------------------------------------------------- | :----------------------- |
|  [01]   | `https://www.pulumi.com/docs/iac/automation-api/`                                              | Read 2026-11-12          |
|  [02]   | `https://www.pulumi.com/docs/iac/guides/continuous-delivery/github-actions/`                   | Read 2026-11-12          |
|  [03]   | `infra/node_modules/@pulumi/command`: `README.md`, `local/command.d.ts`, `remote/*.d.ts`       | `1.4.0-alpha.1794116294` |
|  [04]   | `https://www.pulumi.com/registry/packages/docker/`                                             | `v5.3.0`, 2026-11-12     |
|  [05]   | `https://docs.docker.com/compose/how-tos/production/`                                          | Read 2026-11-12          |
|  [06]   | `https://mise.jdx.dev/bootstrap.html`, `mise bootstrap --help`, `mise bootstrap remote --help` | `2026.11.7`              |
|  [07]   | `https://mise.jdx.dev/daemons.html`, `https://mise.jdx.dev/configuration/settings.html`        | `2026.11.7`              |
|  [08]   | `https://testcontainers.com/getting-started/`                                                  | Read 2026-11-12          |
|  [09]   | `https://node.testcontainers.org/supported-container-runtimes/`, `/features/compose/`          | Read 2026-11-12          |
|  [10]   | `hostinger vps --help`, `hostinger vps virtual-machines --help`                                | CLI `4.20.4`             |

[COMMANDS]: Live state readers
- `pulumi stack ls --all --json` and `pulumi stack export --stack <name>` with Pulumi CLI `3.274.1`
- `gh run list`, `gh run view`, `gh api repos/bsamiee/Rasm/{environments,rulesets,actions/secrets,branches/main/protection}`
- `doppler secrets --only-names --config <config>` and `doppler secrets get DEPLOYED_HOSTS BACKUP_KIND BACKUP_ENDPOINT --config deploy`
- `ssh -F .cache/ssh/<mesh name>/config <mesh name> 'docker ps; docker volume ls; docker compose ls'`
- `nx show project @rasm/infra --json`, `nx show projects --with-target <target>`, `mise daemons status`, `docker ps`
- `git log --follow`, `git log -S`, `git show --stat` for each file's cause
