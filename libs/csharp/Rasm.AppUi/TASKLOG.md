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

The genuine tier-3 residual set, one row per DENSITY_BAR `SPIKE` owner. Each owner is fence-complete with its full member shape and its CPU/2D-Skia or settled fallback shipping today; the only open gate is the named live host context, host-shared GPU context, host-bound device/recognizer SDK, or unverified package member set. The embed-spike cluster ([1]-[3]) gates rows [1]/[2] and the embed-riding [26a]/[28a]; the GPU cluster ([4]) gates the viewport rows [33]-[38]; the host-SDK cluster ([5]-[7]) gates the input-fabric, caption, and office rows; the build-closure row ([8]) gates the dev-loop strip and the entity-writer admission gates DraftEmit.

| [INDEX] | [ITEM] | [OWNER (DENSITY_BAR row · page#cluster)] | [EXIT] |
| :-----: | ------ | ---------------------------------------- | ------ |
| [1] | `SurfaceHost`/`SurfaceFact` Avalonia-12-in-Rhino NSView embedding (pump coexistence, resize sync, render-backend contention) | rows [1]/[2] · surface-hosts#RESEARCH [EMBED_SPIKE] | all three scenarios pass without pump conflict or render regression under live RhinoWIP; closes [1]/[2] SPIKE->FINALIZED |
| [2] | Embedded-TopLevel service resolution (toasts, storage pickers) inside the rhino-panel root, riding the embedding seam | rows [1]/[2] · dialogs-notifications#RESEARCH [EMBEDDED_TOPLEVEL] | service resolution succeeds inside the panel root |
| [3] | Host-object drag across the NSView boundary; VoiceOver reach across the embedding | rows [1]/[2]/[28a] · input-interaction#RESEARCH [EMBEDDED_DRAG] + accessibility#AUTOMATION_PEERS | drag and VoiceOver reach confirmed across boundary under live RhinoWIP |
| [4] | Viewport GPU dispatch over the host-shared `GRContext` lease — `ISkiaSharpApiLease.TryLeasePlatformGraphicsApi` Metal/Vulkan backend-context, `SKRuntimeEffect` meshlet/path-trace emit, per-backend bindless descriptor table and ray-tracing acceleration structures; the CPU/2D-Skia fallback (composite raster, CPU meshlet cull, reference path tracer, CPU marching-cubes, blob-backed residency) ships today | rows [33]-[38] · viewport-pipeline#RESEARCH [VIEWPORT_GPU] | GPU passes dispatch through the embed-capsule lease with no render regression; de-risked standalone against a windowed `GRContext`, confirmed in-host against the embedded panel; closes [33]-[38] SPIKE->FINALIZED |
| [5] | `InputDevice`/`DeviceOutput` per-device SDK member spellings — SpaceMouse HID, gamepad API, gaze SDK, switch-access scan, speech recognition, MIDI, CNC/robot/haptic transport — bound through `SurfaceSeam` delegate columns at composition; the union, normalized `DeviceAxis`, and device-intent folds are settled | row [24a] · input-interaction#RESEARCH [INPUT_DEVICE_SDK] | each device SDK binds at composition and drives/maps through its `SurfaceSeam` column; per-device member spellings confirmed |
| [6] | `LiveCaption.Translated` recognizer-and-translator member spellings — the speech-recognition utterance source and machine-translation engine bound through a composition delegate; the `ShapedAnnotation` complex-script projection and `CaptionSource` union are settled | row [26a] · localization-culture#RESEARCH [LIVE_TRANSLATE] | recognizer and translator bind through the composition delegate; member spellings confirmed against the admitted speech-and-translation package |
| [7] | `OfficeExport` OpenXML part-graph member set — `SpreadsheetDocument`/`PresentationDocument`/`WordprocessingDocument` construction, content insertion, embedded-font packing, deterministic-ordering knobs; the `OfficeFormat`/`OfficeSheet`/`OfficeSpec` axes are settled, the writer pin lands at admission | row [30a] · visuals-offscreen#RESEARCH [OFFICE_OPENXML] | OpenXML part-graph emits a reproducible OOXML byte-stream; member set confirmed and the writer package admitted |
| [8] | `DevLoop` HotAvalonia Release-closure strip and markup-loader floor against the central Avalonia pin; `DraftEmit` DWG/DXF model-space entity-writer member set (PDF/SVG arms settled on the `SKDocument`/`SKSvgCanvas` rail) | rows [29a]/[43] · diagnostics-evidence#RESEARCH [DEV_LOOP_STRIP] + drafting-sheets#RESEARCH [DRAFT_ENTITY] | Release-closure strip verified against the built MSBuild closure; the entity-writer member set confirmed and the CAD package admitted; closes [29a]/[43] SPIKE->FINALIZED |

## [3]-[DEEPENING_FINDINGS]

| [INDEX] | [ITEM] | [EXIT] |
| :-----: | ------ | ------ |
| [1] | AppUi input-interaction gesture-trigger member spellings (TappedEventTrigger, DoubleTappedEventTrigger, RightTappedEventTrigger, IPointer.Capture, PointerCaptureLostEventTrigger) softened to prose; re-ground each against installed Avalonia.Xaml.Behaviors at implementation so input-interaction#POINTER_GESTURES carries exact member spellings | gesture-trigger members grounded to exact API spellings; no prose-only gesture rows remain |
