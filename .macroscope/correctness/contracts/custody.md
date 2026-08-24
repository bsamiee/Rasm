---
include:
  - "libs/contracts/**"
  - "libs/contracts/.api/**"
---

# [CONTRACTS_CUSTODY]

`libs/contracts/` is the one cross-language contracts estate every branch imports. Read `libs/contracts/manifest.json` and `libs/contracts/ARCHITECTURE.md` `[REGISTRY]` live before a finding; never carry a copy of either roster here.

## [01]-[REGISTRY]

- Authority is case-level: read the atomic case live from `libs/contracts/manifest.json` and apply `libs/contracts/ARCHITECTURE.md` `[REGISTRY]`; a case contradicting its declared class, actors, readiness, proof, or asset custody is a finding, and entry grouping never widens those facts.
- Cases whose actors all sit inside one runtime belong in that branch's proof tree, never in the estate.
- `manifest.json` validates against `manifest.schema.json`, and `buf.yaml` with `buf.gen.yaml` are the only generation authorities; a manifest case, buf row, or fence naming a family, actor, or path the tree does not hold is a phantom finding.

## [02]-[DIRECTORIES]

- `proto/rasm/contracts/<family>/` is the single definition language every binding derives from.
- `vendor/<publisher>/` holds frozen publisher bytes; a hand edit there is a finding.
- `conformance/<seam>/` holds the proof assets manifest cases cite.
- `gen/{dotnet,python,typescript}/` holds only buf emissions and gate-projected markers swept on every generate; a hand-authored file or edit under `gen/` is a finding, and a hand spelling beside a generated binding is a second wire model that deletes.
- `.api/` carries `bufbuild-buf.md` beside one catalog per branch — the stacking tier every contract fence composes from.
