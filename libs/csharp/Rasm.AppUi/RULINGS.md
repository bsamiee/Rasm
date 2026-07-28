# [RASM_APPUI_RULINGS]

`Rasm.AppUi` rulings settle package-scoped decisions.

## [01]-[PACKAGES]

- (none)

## [02]-[SHAPE]

- Durable counterparts to an evidence fold are SOURCES, never second folds — `EvidenceSource` hands back the same `ReceiptEnvelope` values live or resident, so the correlation join and the billing accrual each stay one implementation; the recurring move writes a store-side query returning pre-aggregated rows, re-deriving the accrual at a second site, and re-litigation opens only if the durable plane stops carrying whole envelopes.
- Every device-resident cache carries a BYTE ceiling and a least-recently-touched release, per plane class and per backend — geometry pages at `Render/meshlets` `ResidencyBudget`, texture planes at `Render/shading` `ShaderAssetCache`; a handle count is not a budget, because one 16k plane outweighs a thousand masks. Each resolution stamps a generation every plane it touches carries, and eviction never releases a cell at or above the live generation, so a budgeted cache cannot free a handle the current draw holds; the recurring move grows a `ConcurrentDictionary` of native handles until the device refuses, and re-litigation opens only if a single owner takes both plane classes.
- Raster shading READS a resolved `EnvironmentLight`; it never integrates one. Prefilter products — the SH irradiance run, GGX roughness ladder, split-sum LUT, and stored equirect — cross as data and blobs, and the roughness-to-level correspondence crosses as the ladder itself rather than its formula, so the level a raster shade picks and the level the prefilter wrote agree by construction; the recurring move re-derives an SH reconstruction or a level formula on the Render side to save a uniform, and re-litigation opens only if a backend cannot bind the ladder as a level set.

## [03]-[COLLAPSE]

- (none)

## [04]-[STRUCTURE]

- (none)

## [05]-[PROCESS]

- (none)
