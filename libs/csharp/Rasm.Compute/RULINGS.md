# [RASM_COMPUTE_RULINGS]

`Rasm.Compute` rulings settle package-scoped decisions.

## [01]-[PACKAGES]

- (none)

## [02]-[SHAPE]

- `FieldMask` has one mint and one apply — `WireServices.Mask` alone constructs and validates masks at the wire edge (`FieldRef` admission under the `FieldMask.IsValid` gate), while `FrameEdge.Patch` alone merges validated masks onto live messages under the transport frame law; a dedup sweep reading the twin `Union`/`Normalize` calls as duplication folds the apply leg into the mint and couples frame application to wire-edge admission — one mask-shape change then breaks both seams in one edit.

## [03]-[COLLAPSE]

- (none)

## [04]-[STRUCTURE]

- (none)

## [05]-[PROCESS]

- (none)
