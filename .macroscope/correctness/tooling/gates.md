---
include:
  - "**/package.json"
  - "**/pyproject.toml"
  - "**/nx.json"
  - "**/tsconfig*.json"
  - "docs/stacks/**"
---

# [GATE_SINGULARITY]

`uv run python -m tools.assay static` is the one three-language static entrypoint; each language resolves to one canonical checker invocation, and the central manifest is the truth source for which toolchain exists.

- A second compiler or checker lane beside the canonical gate — a manifest script minting an alternate typecheck path, a stale or dev toolchain build running beside the stable release — is a split-brain gate whose fix is the one canonical invocation everywhere.
- Doctrine cites only tooling the owning manifest installs; a durable page naming a checker, compiler, or package no central manifest carries is drift repaired to the canonical gate spelling, never a second lane.
- A gate operator and its sibling scripts check the same config topology — one lane reading the root project set while another checks per-project configs proves the split, and one root config owns the gate's project set.
