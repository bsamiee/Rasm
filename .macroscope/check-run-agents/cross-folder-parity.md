---
title: Rasm Cross-Folder Parity
input: full_diff
effort: high
conclusion: neutral
include:
  - "libs/**"
---

# [CROSS_FOLDER_PARITY]

Capability parity across sibling folders sharing a concept is this pass's one question: whether a capability deepened on one folder's owner reaches every counterpart page realizing the same concept, at that counterpart's own grain. File-local correctness cannot see the sibling folder; this pass walks the shared concept across the whole diff. Owner shape and seam coupling belong to architecture-integrity, folder topology to folder-architecture, strata composition to strata-leverage, and `.api` stacking depth to api-stacking — none re-litigated here.

Hunt these classes, each finding naming the leading owner, the shared concept, and each lagging counterpart owner:
- Deepened-owner drift: a new classification axis, category, field family, enum member, or value-object property landed on one folder's owner whose cross-folder counterpart pages, realizing the same concept in their own owners, still compose the narrower surface — demand the counterpart carry the capability at its own grain.
- Seam-endpoint disagreement: a seam-registry edge whose two endpoint pages disagree on the carried fields — one endpoint widened, the other predating it — demand the lagging endpoint admit the added shape.
- Consumer lag behind a widened surface: a `.api` catalogue or wire producer that gained a member or field whose consumer pages still read the narrow surface — demand the consumer compose the widened form.

Never a mirror demand: each side realizes the shared concept in its own owners, vocabulary, and casing, so the finding is the missing capability, never missing identical text. Never-regress is law: a parity fix widens the lagging side, and shrinking the leading side to match is the rejected resolution. A pass that finds nothing after walking every shared concept across its folders states that verdict plainly.
