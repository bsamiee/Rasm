---
include:
  - "**/*.csproj"
  - "**/*.props"
  - "**/*.targets"
  - "**/*.slnx"
  - "**/NuGet.config"
  - "**/package.json"
  - "**/pyproject.toml"
  - "**/pnpm-workspace.yaml"
  - "**/global.json"
  - "**/.config/**"
---

# [DEPENDENCY_CUSTODY]

Package versions live only in the central manifests — `Directory.Packages.props` (whose row also obligates the consuming `.csproj`), `pyproject.toml`, `pnpm-workspace.yaml` — hand-edited, never scripted; a per-package `pyproject.toml`, `package.json` version block, or `.csproj` version attribute is a custody breach. Admitted packages are first-class implementation material mined to full capability; a thin wrapper renaming, forwarding, or partially exposing an admitted package without domain value is a defect.

## [01]-[MOVEMENT]

- Version movement is forward-only: a downgrade or stale floor without a recorded reason in the diff is a finding.
- A sanctioned pin carries an inline comment stating exactly what breaks without it; a pin missing that mechanism comment is a finding, and a pin carrying one is never flagged — `python_version` gates in `pyproject.toml` are evidence-backed rulings, re-tested only when the named blocker moves.
- Vendored SDKs are defects everywhere except the ruled `vendor/` LFS natives; a new vendored binary demands the LFS row and the ruling in the same change.
- A project reference breaching the strata edges the root `README.md` and `libs/.planning/architecture.md` declare — KERNEL to AEC-DOMAIN to APP-PLATFORM to HOST-BOUNDARY, strictly upward — is a strata defect.

## [02]-[GRAMMAR_AND_REGISTRIES]

- Every `Directory.Packages.props` row sits inside exactly one `<ItemGroup Label="...">` naming a consuming owner or concern, alphabetized within its group with `Version` columns aligned; large owner groups split into blank-line-separated closure sub-clusters each opened by a one-line comment, and an ungrouped, unsorted, or bare-pinned row is a finding.
- Every `Directory.Build.props`/`.targets` `PropertyGroup` and `ItemGroup` carries a `Label` naming its concern, and a condition-bearing group carries a one-line comment justifying the condition or suppression; a group without either is a finding.
- A `.csproj` groups `ProjectReference`s under their own label and every `PackageReference` inside a concern-labelled `ItemGroup` whose label matches the concern the folder README card uses, alphabetized, never version-bearing.
- A dependency admission is a same-change multi-surface fact: the central manifest row, the consuming `.csproj` reference, the folder README registry row under `[02]-[DOMAIN_PACKAGES]`/`[03]-[SUBSTRATE_PACKAGES]`, and the `.api/` catalogue tier move together — a package present in one surface but missing from another is registry drift.
- A README package card row is the backticked package id plus at most one dash-led charter clause; a version pin, a parenthetical, an `.api/` link, or multi-line depth is a finding — capability depth belongs to the `.api/` catalogue.
