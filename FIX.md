# [FIX]

Known CI costs, measured on run 42919352504 of `ci.yml` (head `4175bc63d`, November 10): Linux 15 min, macOS 13.5 min, with under one minute of affected checks and tests in each job.

## [01]-[STORE_IMAGE]

Runs with an affected store client start `rasm-store:serve--compose`, which builds service `store` of `infra/compose.yaml`:
- Image `store` compiles `rasm-store` from source in 6 min without a layer cache, and a remote-cache hit of a store suite still starts the service
- Service `store` reads and writes a `gha` layer cache, which Buildx drops where no `ACTIONS_*` token is set, `crazy-max/ghaction-github-runtime` sets it in job `linux`
- Unverified until one CI run: the `gha` cache on the default `docker` driver needs the containerd image store (`docker info --format '{{.DriverStatus}}'`)
- A change under the paths `apps/data/store/Dockerfile` copies still compiles cold, BuildKit exports no `RUN --mount=type=cache` content

## [02]-[MACOS_SETUP]

Job `macos` spends 9 min before its first task:

| [INDEX] | [STEP]                                               | [TIME]  |
| :-----: | :--------------------------------------------------- | :------ |
|  [01]   | First Nx command, project graph built cold           | 3.2 min |
|  [02]   | `mcneel/setup-rhino3d`, when a Rhino project changed | 3 min   |
|  [03]   | `jdx/mise-action`                                    | 2.5 min |
|  [04]   | `nx build @rasm/nx-apple-darwin-arm64 -c release`    | 1.8 min |
|  [05]   | `pnpm install`                                       | 1.5 min |

Fix:
- Keep the built Apple analyzer between runs, keyed on its sources
- Shorten the cold graph: a developer machine takes 37 s with every plugin cache empty, 30 s of it `xcodebuild -showBuildSettings` per shared scheme in `@rasm/nx-apple`

## [03]-[QUEUED_RUNS]

Runs of `ci.yml` on `main` share one concurrency group with `cancel-in-progress` off:
- A push waits for the run before it, and a third push cancels the waiting second
- Several pushes in a row report one result per run length, 15 min each

Fix:
- Decide whether pushes to `main` cancel the run in progress, as pull requests do
