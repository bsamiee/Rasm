# [RASM_RHINO_API_ETO_RUNTIME]

`Eto.Forms` ambient runtime is a process-wide singleton set, so this boundary registers it whole and adds no carrier: one Rhino process holds one application instance and one clipboard, shared with the Grasshopper boundary rather than partitioned from it. This partition states how the Rhino host boundary reaches that runtime — the dispatch every background producer crosses, the keyed payload a document transfer carries, the tray and toast a long-running operation reports through, and the screen capture a boundary read takes.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Eto.Forms` — Rhino host-boundary runtime reach
- package: `Eto.Forms` (host-provided; resolved from the Rhino host assembly set, never a central `PackageReference`)
- assembly: `Eto` (`Eto.dll`)
- namespace: `Eto.Forms`
- asset: the `Eto` assembly the Rhino host loads; `macOS`, `WinForms`, and `Wpf` platform handlers back one managed surface
- rail: eto-runtime

## [02]-[BOUNDARY_REACH]

- Registers the `Eto.Forms` ambient runtime (`libs/csharp/.api/api-eto-runtime.md`): `Application` dispatch and lifecycle, `UITimer`, `Keyboard`/`Mouse`/`Cursors` live input, `Clipboard`/`DataObject`/`IDataObject`/`DataFormats` typed transfer, `Notification`/`TrayIndicator`, and `Screen` display state carry their algebra there. A process singleton admits no per-folder partition, so this boundary adds no carrier and states its reach and composition law over the registered surface.

| [INDEX] | [BOUNDARY_CONCERN]           | [REGISTERED_MEMBERS]                                                                 |
| :-----: | :--------------------------- | :----------------------------------------------------------------------------------- |
|  [01]   | background-producer crossing | `Application.Invoke`, `AsyncInvoke`, `InvokeAsync`, `EnsureUIThread`, `RunIteration` |
|  [02]   | portable clock fallback      | `UITimer(EventHandler<EventArgs>)`, `Start()`, `Stop()`, `Interval`                  |
|  [03]   | keyed document transfer      | `Clipboard.Instance` `Set*`/`Get*` pairs, `Contains(type)`, `Clear()`                |
|  [04]   | drag negotiation             | `IDataObject`, `DragEventArgs.Data`/`AllowedEffects`/`Effects`, `SetDropDescription` |
|  [05]   | operation completion notice  | `TrayIndicator.SetMenu`/`Show`/`Hide`, `Notification.Show`, `UserData`               |
|  [06]   | display resolution and grab  | `Screen.FromPoint`, `Screen.FromRectangle`, `Screen.GetImage(RectangleF)`            |
|  [07]   | pointer and modifier probe   | `Mouse.IsSupported`, `IsAnyButtonPressed`, `SetCursor`, `Keyboard.IsKeyLocked`       |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every control-tree read or write at this boundary executes on the UI thread and a background producer crosses through exactly one registered dispatch shape; `EnsureUIThread` guards a UI-only method and `RunIteration` pumps the loop where a synchronous wait is unavoidable.
- Transfer keys on a MIME type across both lifetimes, and one `IDataObject`-typed body serves the clipboard and the drag payload alike; the stream pair is class-level on each carrier and off the interface, so a stream transfer names its carrier.
- Tray presence and toast delivery are one pair: a toast declaring `RequiresTrayIndicator` needs a live tray host, and activation correlates back through `UserData` rather than a boundary-side pending map.
- The registered `UITimer` is the portable pace this boundary falls back to; the macOS display-link pace supersedes it under the host gate (`libs/csharp/Rasm.Rhino/.api/api-macos-native.md`).

[STACKING]:
- `libs/csharp/.api/api-eto-runtime.md`: the registered singleton algebra; this boundary composes it and re-tables none of it.
- `libs/csharp/Rasm.Rhino/.api/api-eto-forms.md`: dialog presentation and control invalidation are the construction-side consumers that marshal through the registered application singleton.
- `libs/csharp/Rasm.Rhino/.api/api-rhino-ui.md`: the Rhino host marshal owner is the outer seam — an Eto-level thread-affinity test never replaces it, and a document-touching callback marshals there first.
- `LanguageExt.Core`(`libs/csharp/.api/api-languageext.md`): a dispatch wraps into `Eff<A>`/`IO<A>` and folds to `Fin<A>`, `Option<A>` lifts every nullable transfer read gated by the presence probe, and the clock and tray acquire and release through the `use` rail so neither leaks past its owning scope.
- `Thinktecture.Runtime.Extensions`(`libs/csharp/.api/api-thinktecture-runtime-extensions.md`): the cursor roster, drag effects, and button masks bind as `[SmartEnum]` and flag owners routed by generated dispatch, and a MIME type binds as `[ValueObject<string>]` so transfer access is keyed by a validated owner.

[LOCAL_ADMISSION]:
- Runtime state is host-provided and never re-declared; this boundary internalizes a dispatch, transfer, timer, or tray concern behind one canonical rail so downstream code composes a marshalled effect or a keyed payload.
- The application singleton and a stringy MIME key never cross into a domain signature.

[RAIL_LAW]:
- Partition: `Eto.Forms` ambient runtime, Rhino host-boundary reach
- Owns: the composition law placing background-producer dispatch, keyed document transfer, tray and toast reporting, and display resolution on the registered singletons
- Accept: marshalled effects, keyed transfer payloads, resource-scoped clocks and tray icons, display resolution and region capture
- Reject: a re-tabling of the registered singleton algebra, control and window construction (`libs/csharp/Rasm.Rhino/.api/api-eto-forms.md`), custom painting (`libs/csharp/Rasm.Rhino/.api/api-eto-drawing.md`), document output (`libs/csharp/Rasm.Rhino/.api/api-eto-printing.md`), platform selection and native hosting (`libs/csharp/Rasm.Rhino/.api/api-eto-platform.md`), and leaking the application singleton or a stringy MIME key past the owning rail
