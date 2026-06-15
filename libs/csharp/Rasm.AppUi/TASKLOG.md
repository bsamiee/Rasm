# [APPUI_TASKLOG]

Open work owned by this folder; closed items do not appear.

## [1]-[SPEC_PROOFS_AT_IMPLEMENTATION]

| [INDEX] | [ITEM] | [EXIT] |
| :-----: | ------ | ------ |
| [1] | DockSerializer round-trip preserving dockable identity; render-hash lanes; AvaloniaFact-under-MTP dispatch spike | round-trip preserves identity; render-hash lanes pass; dispatch spike recorded |
| [2] | LiveCharts net8-asset render fidelity on Avalonia 12; heat-land geo payload | render hash matches expected fidelity; geo payload verified |

## [2]-[PLANNING_CLOSE_OUT_SPIKES]

| [INDEX] | [ITEM] | [EXIT] |
| :-----: | ------ | ------ |
| [1] | Avalonia-12-in-Rhino NSView embedding (pump coexistence, resize sync, render-backend contention scenarios) | all three scenarios pass without pump conflict or render regression |
| [2] | Embedded-TopLevel service resolution (toasts, storage pickers) inside the rhino-panel root | service resolution succeeds inside the panel root |
| [3] | Host-object drag across the NSView boundary; VoiceOver reach across the embedding | drag and VoiceOver reach confirmed across boundary |

## [3]-[DEEPENING_FINDINGS]

| [INDEX] | [ITEM] | [EXIT] |
| :-----: | ------ | ------ |
| [1] | AppUi input-interaction net-lost-grounding: gesture-trigger and clipboard member spellings (TappedEventTrigger, DoubleTappedEventTrigger, RightTappedEventTrigger, IPointer.Capture, PointerCaptureLostEventTrigger, ExportState, multi-format clipboard write) softened to prose; re-ground each against installed Avalonia.Xaml.Behaviors and Avalonia clipboard surfaces at implementation so input-interaction#POINTER_GESTURES/#DRAG_CLIPBOARD carry exact member spellings | all named members grounded to exact API spellings; no prose-only rows remain on the input-interaction page |
