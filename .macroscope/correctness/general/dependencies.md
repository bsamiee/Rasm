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
- A central row with no direct manifest consumer carries a `Rides <puller>` maintenance comment naming its direct puller; an uncommented transitive row is illegible and re-flags on every census, and a row carrying the comment is proven indirect custody, never an orphan or unused-dependency finding.
- A version family ruled to move as a matched set — sibling packages riding one upstream release wave, twin release tracks, a channel choice a composed member requires — moves together or not at all: a single-member bump, a routine bump-to-newest against a ruled hold, and a unify-the-versions consistency demand against ruled twin tracks are the defect, the ruling riding the RULINGS registry or the pin comment.
- An overlapping second package enters only as a bounded fast-path under a named split predicate deciding which surface serves a call — a per-page arbitrary choice between two overlapping surfaces is the defect, and a fast-path member never owns the concern it accelerates.
- Vendored SDKs are defects everywhere except the ruled `vendor/` LFS natives; a new vendored binary demands the LFS row and the ruling in the same change.
- A project reference breaching the strata edges the root `README.md` and `libs/.planning/architecture.md` declare — KERNEL to AEC-DOMAIN to APP-PLATFORM to HOST-BOUNDARY, strictly upward — is a strata defect.

## [02]-[GRAMMAR_AND_REGISTRIES]

- Every `Directory.Packages.props` row sits inside exactly one `<ItemGroup Label="...">` naming a consuming owner or concern, alphabetized within its group with `Version` columns aligned; large owner groups split into blank-line-separated closure sub-clusters each opened by a one-line comment, and an ungrouped, unsorted, or bare-pinned row is a finding.
- Every `Directory.Build.props`/`.targets` `PropertyGroup` and `ItemGroup` carries a `Label` naming its concern, and a condition-bearing group carries a one-line comment justifying the condition or suppression; a group without either is a finding.
- A `.csproj` groups `ProjectReference`s under their own label and every `PackageReference` inside a concern-labelled `ItemGroup` whose label matches the concern the folder README card uses, alphabetized, never version-bearing.
- A dependency or capability admission is a same-change multi-surface fact: the central manifest row, the consuming `.csproj` reference, the branch README registry row, the folder README registry row under `[02]-[DOMAIN_PACKAGES]`/`[03]-[SUBSTRATE_PACKAGES]`, and the owning `.api/` tier move together — a member of the set present without its counterparts is drift in either direction.
- A README package card row is the backticked package id with at most one dash-led charter clause; a version pin, a parenthetical, an `.api/` link, or multi-line depth is a finding — capability depth belongs to the `.api/` catalogue.
