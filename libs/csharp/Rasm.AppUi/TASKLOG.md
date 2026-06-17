# [APPUI_TASKLOG]

Open work owned by this folder; closed items do not appear. `[STATUS]` is one of `QUEUED`, `ACTIVE`, `BLOCKED`, `SPIKE`; owner state is read at `ARCHITECTURE.md` `[OWNER_REGISTRY]`. Every `SPIKE` row names the probe that flips its owner registry cell to `FINALIZED`.

## [1]-[LIVE_HOST_PROBES]

Tier-3 probes that strictly require the running integrated host (the live Rhino-owned AppKit run-loop and NSView panel host, the host-shared GPU context, or a host-bound device/recognizer SDK); each is named in its page RESEARCH cluster. The embed-spike cluster gates the `SurfaceHost`/`SurfaceFact` owners and the embed-riding caption/scene-a11y rows; the GPU cluster gates the viewport owners; the host-SDK cluster gates the input-fabric, caption, and office rows.

| [INDEX] | [ITEM] | [PAGE#CLUSTER] | [STATUS] |
| :-----: | ------ | -------------- | :------: |
| [1] | `SurfaceHost`/`SurfaceFact` Avalonia-12-in-Rhino NSView embedding — pump coexistence, resize sync, render-backend contention pass without conflict or regression under live RhinoWIP | surface-hosts#EMBED_CAPSULE · surface-hosts#HOST_AXIS | SPIKE |
| [2] | Embedded-TopLevel service resolution (toasts, storage pickers) inside the rhino-panel root, riding the embedding seam | dialogs-notifications#NOTIFICATIONS · dialogs-notifications#PICKERS_HOST_MODALITY | SPIKE |
| [3] | Host-object drag across the NSView boundary; VoiceOver reach across the embedding under live RhinoWIP | input-interaction#DRAG_CLIPBOARD · accessibility#AUTOMATION_PEERS | SPIKE |
| [4] | Viewport GPU dispatch over the host-shared `GRContext` lease — `ISkiaSharpApiLease.TryLeasePlatformGraphicsApi` Metal/Vulkan backend-context, `SKRuntimeEffect` meshlet/path-trace emit, per-backend bindless descriptor table and ray-tracing acceleration structures; the CPU/2D-Skia fallback ships today, de-risked standalone against a windowed `GRContext` | viewport-pipeline#RENDER_GRAPH · viewport-pipeline#GEOMETRY_VIRTUAL · viewport-pipeline#PATH_TRACE | SPIKE |
| [5] | `InputDevice`/`DeviceOutput` per-device SDK member spellings — SpaceMouse HID, gamepad API, gaze SDK, switch-access scan, speech recognition, MIDI, CNC/robot/haptic transport — bound through `SurfaceSeam` delegate columns at composition | input-interaction#INPUT_FABRIC | SPIKE |
| [6] | `LiveCaption.Translated` recognizer-and-translator member spellings — the speech-recognition utterance source and machine-translation engine bound through a composition delegate against the admitted speech-and-translation package | localization-culture#RTL_MIRRORING | SPIKE |
| [7] | macOS reduce-motion preference live probe feeding the global reduced-motion degrade switch | motion-tokens#REDUCED_MOTION | SPIKE |

## [2]-[SPEC_AND_TOOL_GATES]

Implementation-time gates discharged against scratch processes, the spec project, a decompile probe, or a manifest decision — no live host required.

| [INDEX] | [ITEM] | [PAGE#CLUSTER] | [STATUS] |
| :-----: | ------ | -------------- | :------: |
| [1] | `Rasm.AppUi.Tests` row admitted on the assay test rail; `Verify.XunitV3` catalogue lands; unblocks every spec and render-hash lane | diagnostics-evidence#HEADLESS_DERIVATION | SPIKE |
| [2] | `OfficeExport` OpenXML part-graph member set — `SpreadsheetDocument`/`PresentationDocument`/`WordprocessingDocument` construction, content insertion, embedded-font packing, deterministic-ordering knobs; the writer pin lands at admission | visuals-offscreen#DOCUMENT_EXPORT | SPIKE |
| [3] | `DraftEmit` DWG/DXF model-space entity-writer member set; the CAD package admitted (PDF/SVG arms settled on the `SKDocument`/`SKSvgCanvas` rail) | drafting-sheets#DRAFT_EMIT | SPIKE |
| [4] | `DevLoop` HotAvalonia Release-closure strip and markup-loader floor verified against the built MSBuild Release closure | diagnostics-evidence#DEV_LOOP | SPIKE |
| [5] | input-interaction gesture-trigger member re-grounding — `TappedEventTrigger`, `DoubleTappedEventTrigger`, `RightTappedEventTrigger`, `IPointer.Capture`, `PointerCaptureLostEventTrigger` re-ground to exact spellings against installed `Xaml.Behaviors.Avalonia` | input-interaction#POINTER_GESTURES · input-interaction#DRAG_CLIPBOARD | SPIKE |
| [6] | Heat-land geo payload projection from the Compute `GeometryPayload` proto into the `GeoMap` land records, verified against the settled Compute wire contract | charts-dashboards#SERIES_TABLE | SPIKE |
| [7] | `CrossFilter` dynamic-predicate brushing — `Filter(IObservable<Func<TRow,bool>>)` re-filters every non-source tile from one `FilterState` subject without a feed re-subscribe; the source tile self-excludes | charts-dashboards#DASHBOARD_TILES | SPIKE |
| [8] | `BoardState` STJ round-trip through the `IDockSerializer` impl preserving tile arrangement plus `FilterState`; `Version` mismatch falls back to the named dashboard row; restore re-pushes the brush | charts-dashboards#STREAM_BINDING | SPIKE |
| [9] | `GeoOverlay.Bind` folds the Persistence `SpatialDiff` `IChangeSet<HeatLand,string>` onto `HeatLandSeries.Lands` in place inside the `SyncContext` lock; `Change<HeatLand,string>` accessor and `Lands` setter spellings re-ground against decompiled DynamicData/LiveCharts | charts-dashboards#SERIES_TABLE | SPIKE |
| [10] | `DockSerializer` round-trip preserving dockable identity; render-hash lanes pass as executed specs | shell-navigation#DOCK_LAYOUTS | SPIKE |

## [3]-[TRANSCRIPTION]

The implementation sequence is the `ARCHITECTURE.md` `[SOURCE_TREE]` build order (vocabulary owners before consumers, `Hosts/SurfaceVocabulary.cs` through `Animation/AnimationRail.cs`); each file transcribes its page clusters verbatim and resolves the RESEARCH rows those pages carry. The BUILD_ORDER seam notes bind at transcription: `Surfaces.Mount`/`InspectorSurface.Mount`/`InspectorSurface.Attach` carry `ClockPolicy`; the input file consumes the frozen deck's gesture-conflict fold; `AssetKeys` nameof spellings are the only cross-file asset references; the `PhaseMotion` map keys mirror the Compute `ProgressPhase` nine-case set; `AccessProof.Sweep` and `ProofEngine.Derive` share the typed `(ThemeVariantRow, DensityRow)` grid. Production source is absent.

| [INDEX] | [ITEM] | [PAGE#CLUSTER] | [STATUS] |
| :-----: | ------ | -------------- | :------: |
| [1] | Transcribe the BUILD_ORDER files per `ARCHITECTURE.md` `[SOURCE_TREE]`; the test project `Rasm.AppUi.Tests` node is present and empty | surface-hosts#HOST_AXIS | QUEUED |
