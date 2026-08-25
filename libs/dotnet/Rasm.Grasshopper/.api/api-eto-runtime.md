# [RASM_GRASSHOPPER_API_ETO_RUNTIME]

`Eto.Forms` ambient runtime is a process-wide singleton set, so this boundary registers it whole and adds no carrier: one Rhino process holds one application instance and one clipboard, and a GH2-hosted panel composes them alongside the Rhino boundary rather than partitioning them. This partition states how the canvas and panel reach that runtime — the tick that paces canvas animation, the live input read during a drag, the keyed payload a canvas drop carries, and the density a panel places logical geometry against.

## [01]-[BOUNDARY_REACH]

- Registers the `Eto.Forms` ambient runtime (`libs/dotnet/.api/api-eto-runtime.md`): `Application` dispatch and lifecycle, `UITimer`, `Keyboard`/`Mouse`/`Cursors` live input, `Clipboard`/`DataObject`/`DataFormats` typed transfer, `Notification`/`TrayIndicator`, and `Screen` density carry their algebra there. Process singleton admits no per-folder partition, so this boundary adds no carrier and states its reach and composition law over the registered surface.

| [INDEX] | [BOUNDARY_CONCERN]          | [REGISTERED_MEMBERS]                                                               |
| :-----: | :-------------------------- | :--------------------------------------------------------------------------------- |
|  [01]   | off-thread panel mutation   | `Application.Invoke`, `AsyncInvoke`, `InvokeAsync`, `EnsureUIThread`, `IsUIThread` |
|  [02]   | canvas animation cadence    | `UITimer.Interval`, `Start()`, `Stop()`, `Elapsed`                                 |
|  [03]   | live drag and modifier read | `Keyboard.Modifiers`, `ModifiersChanged`, `Mouse.Position`, `Mouse.Buttons`        |
|  [04]   | pointer-mode feedback       | `Cursor`, the `Cursors` roster, `Mouse.SetCursor(Cursor)`                          |
|  [05]   | canvas drop payload         | `DataObject` keyed accessors, `DataFormats`, `DragEffects`, `DragEventArgs.Data`   |
|  [06]   | component copy and paste    | `Clipboard.Instance` typed `Set*`/`Get*` pairs gated by `Contains(type)`           |
|  [07]   | logical-to-device placement | `Screen.LogicalPixelSize`, `Screen.Scale`, `Screen.DPI`, `Screen.PrimaryScreen`    |
|  [08]   | long-run completion notice  | `Notification.Show(TrayIndicator)`, `Application.NotificationActivated`            |

## [02]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Canvas paces on the registered `UITimer` tick and reads input live inside it: `Keyboard.Modifiers` and `Mouse.Position`/`Buttons` answer the ambient state a drag interpolates against, distinct from the per-event snapshots a control raises, so a drag never reconstructs modifier state from a stale event payload.
- Canvas drop is one keyed payload: the component type string is a `DataFormats` identifier and the payload rides `DataObject` under it, so a drop discriminates on the registered key and never on a parsed blob.
- Density resolves once per paint from `Screen`, so canvas logical geometry maps to device pixels through one read rather than a scale constant threaded through the painter.
- Every off-thread mutation of panel or canvas state folds through one registered dispatch shape; a second marshal path inside the boundary is the deleted form.

[STACKING]:
- `api-eto-runtime`(`libs/dotnet/.api/api-eto-runtime.md`): the registered singleton algebra; this boundary composes it and re-tables none of it.
- `api-eto-forms`(`libs/dotnet/Rasm.Grasshopper/.api/api-eto-forms.md`): control invalidation and dialog presentation are the panel-side consumers that marshal through the registered application singleton.
- `api-macos-native`(`libs/dotnet/Rasm.Grasshopper/.api/api-macos-native.md`): the registered tick and density reads are the host-neutral fallback the macOS layer supersedes with display-link pacing and screen refresh metrics for high-cadence canvas work.
- `api-languageext`(`libs/dotnet/.api/api-languageext.md`): a UI-thread result marshal defers as `Eff<A>` run through the registered dispatch and lands `Fin<A>`; a clipboard read null-gates into `Option<A>`; a tick drives an `IO<A>` step chain per frame.
- `api-thinktecture-runtime-extensions`(`libs/dotnet/.api/api-thinktecture-runtime-extensions.md`): the `DataFormats` identifiers project onto a `[SmartEnum<string>]` payload-kind owner carrying parse and serialize behaviour, and the modifier and button masks bind as flag owners routed by generated dispatch.

[LOCAL_ADMISSION]:
- Cross-thread marshal calls the registered application singleton and a cadence uses the registered tick; a hand-rolled `SynchronizationContext` capture or `System.Threading.Timer` beside them is the deleted form.
- Transfer payloads ride the typed accessors keyed by a `DataFormats` identifier; a stringly-parsed blob past them is the deleted form.
- Display density reads from `Screen`; a hardcoded scale constant is the deleted form.
