# [APPUI_TASKLOG]

Open and closed work for the product UI engine, distilled from the ideas in `IDEAS.md`. Each open card carries a status marker — `[QUEUED]`, `[ACTIVE]`, or `[BLOCKED]` — plus the capability or file to build, the external packages to integrate, the integration points and boundaries or wires, and the key considerations. Live-host probes and tool-gated arms are tracked as blocked tasks against the substrate that unblocks them.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
-->

[EMBED-PROBE]-[BLOCKED]: live Rhino embedding proves the shared AppUi GPU seam.
- Capability: live-host validation for `SurfaceHost.RhinoPanel`, `EmbedCapsule`, `SurfaceScheduler.Affinity`, and `RenderGraph.Lease` against the Rhino-owned AppKit run-loop and GPU pipeline.
- Shape: one `SurfaceSeam`-bound GPU lease carries `ISkiaSharpApiLease.TryLeasePlatformGraphicsApi`, `GRMtlBackendContext`/`GRVkBackendContext`, and `SKRuntimeEffect` into the `GpuBackend` `RenderTargetFactory` rows without per-host render arms.
- Unlocks: meshlet, path-trace, live `Taa`/`Fsr`, and embedded-panel frame-receipt proof while preserving the CPU/2D-Skia fallback already de-risked against a windowed `GRContext`.
- Anchors: `.planning/Shell/hosts.md#07research`, `.planning/Render/viewport.md#09research`, `.api/api-avalonia-skia.md`, `.api/api-skiasharp.md`, `.api/api-avalonia-gpu-interop.md`, `SurfaceSeam.OnUiThread`, `GpuBackend`, and `RenderTargetFactory`.
- Tension: no integrated running RhinoWIP host surface exists yet, so pump coexistence, resize sync, render-backend contention, and host-owned shared-context spelling remain blocked.

[INPUT-FABRIC-SDKS]-[BLOCKED]: composition-bound device SDKs complete the alternative-input fabric.
- Capability: per-device SDK admission for `InputDevice`, `DeviceOutput`, and `CaptionSource.Translated` delegate spellings across SpaceMouse, gamepad, gaze, switch access, speech, MIDI, CNC, robot, haptic, recognizer, and translation surfaces.
- Shape: each device or speech package lands as a `SurfaceSeam` or composition delegate column feeding `InputFabric.Map`, `InputFabric.Drive`, or `LiveCaption.Stream`; samples normalize to `DeviceAxis`, and commands fold through the single `CommandIntent` table.
- Unlocks: alternative input and device output, voice command routing, localized live captions, and haptic/robot/CNC feedback without per-device handlers or a second command grammar.
- Anchors: `.planning/Shell/input.md#07research`, `.planning/Theme/locale.md#06research`, `.planning/Shell/commands.md`, `InputDevice`, `DeviceOutput`, `InputFabric`, `DeviceAxis`, `CommandIntent`, and `CaptionSource.Translated`.
- Tension: host-bound device, recognizer, translation, MIDI, CNC, robot, and haptic package admissions are absent; vocabulary and folds are fence-complete, but member spellings remain unverified.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

(none)
