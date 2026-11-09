# [TODO2]

Handoff to the infrastructure session. The tooling rebuild this file tracked is finished: every language returns through its own Nx plugin (`apps/nx/*`), hk owns file-level checks, CI and release run from `.github/workflows/`, and each plugin's README states what it infers. What the rebuild learned lives where its readers find it: the root `README.md` (principles, owners, tasks), each plugin's README, and `docs/research/nx/` (one folder per plugin, each with `known-problems.md`). `git show c64a8b424` holds the tree before the rebuild, `git show 6c55aa2dd:TODO2.md` the full decision log.

## [01]-[INFRASTRUCTURE]

The infrastructure session builds from `docs/research/infrastructure/` (`plan.md`, `plan/`, `design/`, `decisions.md`), read critically: it predates the plugins, so any step it routes through old root targets, `eng/`, or hand-written Nx configuration takes the inferred form instead (`docs/research/nx/infrastructure/plan.md` states what holds and what goes). Open for that session:
- The store service and its stack runner, built to the publishable standard: configuration from the caller, no repository paths, one service definition in `infra/compose.yaml` with Pulumi reading it rather than restating images, Pulumi project names derived from their packages
- Docker hosts stated per stack, and who starts the local engine: Colima's mise daemon row is gone, so `nx install @rasm/infra` (Compose `up`) needs a running engine first; CI's Linux runner has one
- Session-state services for the suites that read Compose services (`@rasm/test-support/compose`): started once per session or CI run, never a `test` dependency
- The self-hosted Nx remote cache (`NX_SELF_HOSTED_REMOTE_CACHE_*`, `docs/research/nx/ci.md` [05]) and its tokens in Doppler's `ci` config
- CodeQL default setup as a repository-stack setting (`ci.md` [08]), and the `NODE_VERSION` mise row checked against the env-row rule
- `@rasm/nx-pulumi` and `@rasm/nx-docker` extended only where a stack needs a fact they lack (`docs/research/nx/{pulumi,docker}/known-problems.md`)

## [02]-[OUTSIDE]

Steps that sit with the owner or Forge, not the repository:
- CI secrets synced from Doppler: `RHINO_EMAIL` (McNeel's installer), `NUGET_USER` (NuGet trusted publishing), `YAK_TOKEN`, beside the existing `RHINO_TOKEN`
- The first release: `nx release version 0.1.0 --first-release`, then conventional commits
- Darkroom's first extensions.blender.org submission through the website
