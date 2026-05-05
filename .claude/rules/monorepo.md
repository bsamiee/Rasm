# Nx Monorepo Constraints

## Nx Execution

Always `pnpm exec nx <command>` from repo root — never bare `nx`, `npx nx`, or `cd` into a package.

## Workspace Topology

This foundation currently owns root tooling, `.claude/`, `apps/cs-analyzer`, and root `tests/`. Add package roots or new app roots only when the directory exists and its package manifest is real.

`pnpm exec nx show projects` currently returns no active Nx projects. Treat Nx guidance as future-ready until a physical project root is added.

## Dependency Isolation

Enforce Nx module boundary rules only for roots that physically exist. Apps import shared packages only after those packages are materialized; apps never import other apps.

## Task Pipeline

Do not claim Nx target inheritance until `nx.json` exists. When an Nx project is materialized, add explicit target defaults with that project and verify with `pnpm exec nx show project <name> --web`.

## Workspace Catalogs

Shared dependency versions in `pnpm-workspace.yaml` catalogs. Package references use `"<dep>": "catalog:"`. Never pin versions in individual `package.json` when a catalog entry exists.
