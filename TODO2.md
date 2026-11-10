# [TODO2]

Handoff after the infrastructure session of November 9. `docs/research/infrastructure/plan.md` orders every remaining unit and `plan/` holds them by concern (`platform.md`, `catalogs.md`, `database.md`, `definition.md`, `stores.md`, `hosting.md`, `testing.md`, `plugins.md`, `release.md`). `docs/research/nx/infrastructure/known-problems.md` records defects found while building, `git show 6c55aa2dd:TODO2.md` the decision log of the tooling rebuild.

## [01]-[BUILT]

| [INDEX] | [CONCERN]            | [OWNER]                                                    | [PROOF]                                         |
| :-----: | :------------------- | :--------------------------------------------------------- | :---------------------------------------------- |
|  [01]   | Doppler project      | `infra/repository-stack.ts`, four configs                  | `doppler configs --json`                        |
|  [02]   | Doppler scope        | `.config/mise.toml` `[env]` `DOPPLER_*` rows               | `doppler configure debug --json`                |
|  [03]   | CI secrets           | `ci` and `release` syncs in `infra/repository-stack.ts`    | `gh secret list`                                |
|  [04]   | Deploy credentials   | `infra/stacks.ts` reads config `deploy`                    | `nx deploy @rasm/infra` takes no prefix         |
|  [05]   | CodeQL default setup | `infra/repository-stack.ts`                                | `gh api <repo>/code-scanning/default-setup`     |
|  [06]   | Deployed host        | `infra/platform-stack.ts`, `infra/host.sh`                 | `nx deploy @rasm/infra --stack=<host>`          |
|  [07]   | DNS                  | Apex and wildcard `CNAME` in `infra/platform-stack.ts`     | `dig +short nx-cache.<domain>`                  |
|  [08]   | Nx remote cache      | `nx-cache` in `infra/compose.yaml`, Linux rows in `ci.yml` | `curl <server>/health` prints `OK`              |
|  [09]   | Pulumi project name  | `infra/stacks.ts`, from `infra/package.json`               | `pulumi stack ls --all`                         |
|  [10]   | `NODE_VERSION` row   | `.config/mise.toml`, from mise's resolved Node             | `app` in `infra/compose.yaml` is its one reader |

## [02]-[OPEN]

Each unit sits in the plan file named:
- Platform inputs in every plugin's cached targets, taken by session `rasm-c1`; the macOS CI job joins the remote cache after it, and config `dev` then takes the read-only token (`plan/plugins.md`)
- Local engine start for `nx install @rasm/infra`, an owner decision between a machine service and a plugin target (`plan/platform.md`)
- Session-state services for suites that read Compose services: CI starts them with `nx install @rasm/infra`, local sessions wait on engine start (`plan/platform.md`, `plan/testing.md`)
- Backups proven by a restore, deployed telemetry, a CI deploy job, and image tag upgrades (`plan/platform.md`, `plan/definition.md`)
- Store service and its fixtures per language (`plan/stores.md`)
- Hosting program rebuilt onto the shared platform of `infra/compose.yaml`, replacing the per-app container program in `infra/hosting-stack.ts` (`plan/hosting.md`)
- Catalog removals and joins, column naming, and the ClickHouse dialect (`plan/catalogs.md`, `plan/database.md`)
- Playwright, Storybook, Vite, and Vitest configuration for many projects and concurrent agents, with the plugins rebuilt to Nx's own plugin form (`plan/testing.md`, `plan/plugins.md`)

## [03]-[OUTSIDE]

Steps on accounts, each in `plan/release.md` with its registry values:
- Scope `@rasm` on npm belongs to another npm user, an owner decision on the scope every `@rasm/nx-*` package publishes under
- Trusted publisher policies on nuget.org and PyPI, set in a browser, then on npm through `npm trust`
- First release: `nx release --dry-run` stops on commit scopes `ui` and `rhino`, each matching more than one project
- Darkroom's first extensions.blender.org submission through the website
