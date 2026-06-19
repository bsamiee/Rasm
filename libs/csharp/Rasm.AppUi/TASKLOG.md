# [APPUI_TASKLOG]

Open and closed work for the product UI engine, distilled from the ideas in `IDEAS.md`. Each open card carries a status marker — `[QUEUED]`, `[ACTIVE]`, or `[BLOCKED]` — plus the capability or file to build, the external packages to integrate, the integration points and boundaries or wires, and the key considerations. Live-host probes and tool-gated arms are tracked as blocked tasks against the substrate that unblocks them.

## [1]-[OPEN]

[EMBED-PROBE]-[HOST-PROBE-DEFERRED] — live host-shared embedding and GPU lease
- Resolve the `Shell/hosts.md` and `Render/viewport.md` RESEARCH rows under a running integrated host: Avalonia-in-Rhino NSView embedding (pump coexistence, resize sync, render-backend contention), the seam `OnUiThread` access-assertion under the Rhino-owned AppKit run-loop, and the host-shared `GRContext` lease through `ISkiaSharpApiLease.TryLeasePlatformGraphicsApi` for meshlet/path-trace emit.
- Integrate the SkiaSharp GPU backend-context (`GRMtlBackendContext`/`GRVkBackendContext`) and `SKRuntimeEffect` over the leased context bound through one `SurfaceSeam` delegate column.
- Wires bind only at the app root that composes a live host; AppUi references the abstract seam columns, never a host API.
- HOST-PROBE-DEFERRED on the live RhinoWIP host surface and the host-owned GPU pipeline (external: an integrated running host does not exist); the CPU/2D-Skia fallback ships today, de-risked standalone against a windowed `GRContext`, and the `GpuBackend` `RenderTargetFactory` column the `T-BACKEND-PORT` realization landed gives the lease a backend-bound target seam to confirm in-host.

[INPUT-FABRIC-SDKS]-[UPSTREAM-BLOCKED] — alternative-input device SDK spellings
- Re-ground the `Shell/input.md` `InputDevice`/`DeviceOutput` per-device member spellings — SpaceMouse HID, gamepad API, gaze SDK, switch-access scan, speech recognition, MIDI, and CNC/robot/haptic transport — and the `localization/#RTL_MIRRORING` `LiveCaption.Translated` recognizer/translator spellings.
- Integrate each device SDK and the speech-and-translation package as `SurfaceSeam` delegate columns bound at composition.
- Wires fold every device sample onto the one `CommandIntent` table and symmetric output over normalized `DeviceAxis`; SDKs cross only as composition delegates.
- UPSTREAM-BLOCKED on the host-bound device/recognizer SDKs (external: SpaceMouse HID, gamepad, gaze, switch-access, speech, MIDI, CNC/robot/haptic transports are not-yet-admitted bindings crossing only as `SurfaceSeam` composition delegates); the fabric vocabulary and the one `CommandIntent`/`DeviceAxis` fold are fence-complete now.

## [2]-[CLOSED]

(none)
