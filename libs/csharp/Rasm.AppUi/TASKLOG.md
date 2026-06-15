# [APPUI_TASKLOG]

Open work owned by this folder; closed items do not appear.

## [1]-[SPEC_PROOFS_AT_IMPLEMENTATION]

| [INDEX] | [ITEM] | [EXIT] |
| :-----: | ------ | ------ |
| [1] | DockSerializer round-trip preserving dockable identity; render-hash lanes pass as executed specs | round-trip preserves identity; render-hash lanes pass |
| [2] | Heat-land geo payload projection from the Compute `GeometryPayload` proto into the `GeoMap` land records (GEO_PAYLOAD wire boundary) | land-record projection verified against the settled Compute wire contract |

## [2]-[PLANNING_CLOSE_OUT_SPIKES]

| [INDEX] | [ITEM] | [EXIT] |
| :-----: | ------ | ------ |
| [1] | Avalonia-12-in-Rhino NSView embedding (pump coexistence, resize sync, render-backend contention scenarios) | all three scenarios pass without pump conflict or render regression |
| [2] | Embedded-TopLevel service resolution (toasts, storage pickers) inside the rhino-panel root | service resolution succeeds inside the panel root |
| [3] | Host-object drag across the NSView boundary; VoiceOver reach across the embedding | drag and VoiceOver reach confirmed across boundary |

## [3]-[DEEPENING_FINDINGS]

| [INDEX] | [ITEM] | [EXIT] |
| :-----: | ------ | ------ |
| [1] | AppUi input-interaction gesture-trigger member spellings (TappedEventTrigger, DoubleTappedEventTrigger, RightTappedEventTrigger, IPointer.Capture, PointerCaptureLostEventTrigger) softened to prose; re-ground each against installed Avalonia.Xaml.Behaviors at implementation so input-interaction#POINTER_GESTURES carries exact member spellings | gesture-trigger members grounded to exact API spellings; no prose-only gesture rows remain |
