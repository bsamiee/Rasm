# [APPUI_TASKLOG]

Open work owned by this folder; closed items do not appear.

## [1]-[SPEC_PROOFS_AT_IMPLEMENTATION]

| [INDEX] | [ITEM] | [EXIT] |
| :-----: | ------ | ------ |
| [1] | DockSerializer round-trip preserving dockable identity; render-hash lanes pass as executed specs | round-trip preserves identity; render-hash lanes pass |
| [2] | Heat-land geo payload projection from the Compute `GeometryPayload` proto into the `GeoMap` land records (GEO_PAYLOAD wire boundary) | land-record projection verified against the settled Compute wire contract |
| [3] | `CrossFilter` dynamic-predicate brushing — `Filter(IObservable<Func<TRow,bool>>)` re-filters every non-source tile from one `FilterState` subject without a feed re-subscribe; the source tile self-excludes | brushing one tile filters siblings on the shared `Connect()` spine; self-filter loop is structurally impossible |
| [4] | `BoardState` STJ round-trip through the `IDockSerializer` impl preserving tile arrangement plus `FilterState`; `Version` mismatch falls back to the named dashboard row; restore re-pushes the brush onto `CrossFilter` | board snapshot round-trips and re-applies the persisted brush at mount; version-mismatch fallback holds |
| [5] | `GeoOverlay.Bind` folds the Persistence `SpatialDiff` `IChangeSet<HeatLand,string>` onto `HeatLandSeries.Lands` in place inside the `SyncContext` lock; `Change<HeatLand,string>` accessor and `Lands` setter spellings (GEO_OVERLAY_DELTAS) re-ground against decompiled DynamicData/LiveCharts | live overlay refresh is an incremental land swap, never a re-load; change-set accessor spellings confirmed at implementation |

## [2]-[PLANNING_CLOSE_OUT_SPIKES]

The genuine tier-3 host-bridge residual: the only DENSITY_BAR `SPIKE` owners — `SurfaceHost` (charter row [1]) and `SurfaceFact` (charter row [2]) — gated on the live Rhino-owned AppKit run-loop and NSView panel host per surface-hosts#RESEARCH [EMBED_SPIKE]; member shape is fence-complete, only the in-host confirmation remains. Closure environment: live RhinoWIP plugin ALC + NSView panel host.

| [INDEX] | [ITEM] | [EXIT] |
| :-----: | ------ | ------ |
| [1] | `SurfaceHost`/`SurfaceFact` Avalonia-12-in-Rhino NSView embedding (pump coexistence, resize sync, render-backend contention) — surface-hosts#RESEARCH [EMBED_SPIKE]; closes DENSITY_BAR rows [1]/[2] SPIKE->FINALIZED | all three scenarios pass without pump conflict or render regression under live RhinoWIP |
| [2] | Embedded-TopLevel service resolution (toasts, storage pickers) inside the rhino-panel root — dialogs-notifications#RESEARCH [EMBEDDED_TOPLEVEL], rides the embedding seam | service resolution succeeds inside the panel root |
| [3] | Host-object drag across the NSView boundary (input-interaction#RESEARCH [EMBEDDED_DRAG]); VoiceOver reach across the embedding (accessibility#AUTOMATION_PEERS) | drag and VoiceOver reach confirmed across boundary under live RhinoWIP |

## [3]-[DEEPENING_FINDINGS]

| [INDEX] | [ITEM] | [EXIT] |
| :-----: | ------ | ------ |
| [1] | AppUi input-interaction gesture-trigger member spellings (TappedEventTrigger, DoubleTappedEventTrigger, RightTappedEventTrigger, IPointer.Capture, PointerCaptureLostEventTrigger) softened to prose; re-ground each against installed Avalonia.Xaml.Behaviors at implementation so input-interaction#POINTER_GESTURES carries exact member spellings | gesture-trigger members grounded to exact API spellings; no prose-only gesture rows remain |
