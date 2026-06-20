# [PY_DATA_TASKLOG]

Open and closed work for `data`, distilled from `IDEAS.md`. Each task card leads with a status marker — `[QUEUED]`, `[ACTIVE]`, or `[BLOCKED]` open; `[COMPLETE]` or `[DROPPED]` closed — and carries the capability or file to build, the external packages to integrate, the integration points and boundaries, and the key considerations. `[1]-[OPEN]` holds live work; `[2]-[CLOSED]` records finished or dropped tasks.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
- Ripple: <origin/counterpart card this entry pairs with across folders, as `pkg` `[SLUG]`; present only on a cross-folder ripple counterpart card>.
- Atomic: <present only on a minor-scope task; one short phrase naming the small unit so a later session does not overscope its turn>.
-->

[LASPY_CATALOGUE]-[QUEUED]: `laspy` API evidence settles the LAS/LAZ point-cloud row.
- Capability: folder `.api/laspy.md` captures the LAS/LAZ reflection surface for `spatial/mesh.md#POINTCLOUD` `[LASPY_SURFACE]`.
- Shape: the catalogue records `read`, `LasData.{x,y,z,header,write}`, `PointFormat.{id,dimension_names}`, and `header.parse_crs` as the point-cloud design's consumed member set.
- Unlocks: `PointCloud.read`, `PointCloud.to_arrow`, and `PointCloud.write` move from catalogue-pending settled form to source-transcribable LAS/LAZ file exchange.
- Anchors: `laspy` 2.7.0 is pure-Python, cp315-clean, and lock-resolved; host provisioning supplies `laspy` plus `assay api`.
- Tension: the remaining gap is host-environmental, not an upstream blocker.

[PDAL_CATALOGUE]-[BLOCKED]: `pdal` API evidence settles COPC octree-subset reads.
- Capability: folder `.api/pdal.md` captures the COPC reflection surface for `spatial/mesh.md#POINTCLOUD` `[PDAL_COPC]`.
- Shape: the catalogue records `Reader.copc(path, bounds=)` and `Pipeline.{execute,arrays}` as the `_copc_subset` member set feeding an Arrow point table.
- Unlocks: `.copc.laz` subset reads can stay in the point-cloud owner as function-local `pdal` imports rather than full-file LAS/LAZ reads or pdal-specific seam objects.
- Anchors: `PointCloud.subset`, `PointBounds.as_pdal`, the `python_version<'3.15'` gated band, and the `pdal` COPC octree reader.
- Tension: `pdal` is sdist-only with no cp315 wheel, so the `<3.15` companion host must install the distribution and confirm spelling by reflection.

[LANCE_VERSION_ACCESSOR]-[BLOCKED]: scalar Lance version reflection settles snapshot identity.
- Capability: confirm the current-version scalar accessor for `tabular/lakehouse.md` `[LANCE_VERSION]`.
- Shape: `LanceDataset.version` feeds `LakeReceipt.version` through `_lance_receipt`, while `LanceDataset.versions()` remains the history rail.
- Unlocks: Lance write, read, and merge receipts carry settled snapshot identity without hand-rolled versioning over Parquet or a private metadata path.
- Anchors: `libs/python/data/.api/pylance.md`, `lance.dataset(uri, version=)`, `LanceDataset.versions()`, and `_lance_receipt`.
- Tension: live `pylance` reflection must capture the scalar `.version` accessor because the catalogue lists only `versions()` today.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

(none)
