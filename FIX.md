# [FIX]

Known CI costs, measured on run 42919352504 of `ci.yml` (head `4175bc63d`, November 10): Linux 15 min, macOS 13.5 min, with under one minute of affected checks and tests in each job.

## [01]-[STORE_IMAGE]

Step `nx install @rasm/infra -c store` of job `linux` takes 11.5 min on every run:
- Service `store` of `infra/compose.yaml` compiles `rasm-store` from source inside its image, and a runner starts with no build cache
- The step runs whether or not an affected test depends on `@rasm/infra`

Fix:
- Run the step only when an affected test needs the store, a link the test rebuild declares (`@rasm/nx-docker` derives no suite edge)
- Give the image build a cache a runner keeps between runs (BuildKit `cache-from` and `cache-to`), or pull an image a push built

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
