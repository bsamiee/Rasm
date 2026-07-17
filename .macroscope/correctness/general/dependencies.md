---
include:
  - "**/*.csproj"
  - "**/*.props"
  - "**/*.targets"
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
- A dependency admission is a multi-surface fact: the central manifest row obligates the folder `README.md` registry row and the `.api/` catalogue tier in the same change.
- A project reference breaching the strata edges the root `README.md` and `libs/.planning/architecture.md` declare — KERNEL to AEC-DOMAIN to APP-PLATFORM to HOST-BOUNDARY, strictly upward — is a strata defect.
