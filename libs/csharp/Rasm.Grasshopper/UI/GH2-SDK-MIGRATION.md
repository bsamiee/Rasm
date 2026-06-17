# GH2 SDK Migration Map — Responsive / Interaction / Attributes input model

Rasm.Grasshopper's UI input layer targets a Grasshopper2 input API that the live host SDK has replaced. The project compiles against the **live host** assembly, not the nuget reference, so the live host is the only authoritative surface.

## Compile target (authoritative)

`Directory.Build.props:130` resolves the GH2 reference to the live plugin:

```
$(RhinoWipResourcesPath)/ManagedPlugIns/Grasshopper2Plugin.rhp/Grasshopper2.dll
```

The nuget package `grasshopper2 2.0.9225-wip.14825` (`~/.nuget/packages/grasshopper2/.../ref/net7.0|net48/Grasshopper2.dll`) is an **older, divergent** API (it still has `IInteraction`/`IResponsiveAttributes` in `Grasshopper2.UI.Flex`). It is NOT the compile target — do not use it as the migration reference.

Decompile the live host type-scoped: `DOTNET_ROOT=/usr/local/share/dotnet ilspycmd -t <FullyQualifiedTypeName> "<live Grasshopper2.dll>"`. Full-assembly decompile (`ilspycmd <dll>`) yields 0 lines on the 27 MB live assembly — always pass `-t`.

## What the SDK removed / replaced

| Old surface (Rasm.Grasshopper uses) | Live host GH2 |
| --- | --- |
| `ComponentAttributes` override `RespondToMouse*`/`Handle*(MouseEventArgs) : Response` | input handling moved OFF the attributes; `ComponentAttributes : Attributes<Component>, IResponsive` owns a `Responses` responder |
| `IResponsiveAttributes` (the `RespondToMouse{Down,Move,Up,SingleClick,DoubleClick}` / `RespondToKey{Down,Up}` interface) | **removed** — replaced by `Grasshopper2.UI.Flex.Responses` (abstract base) reached via `IResponsive.Responder` |
| `IMouseHoverAttributes.RespondToMouseHover(controlPoint, contentPoint) : bool` | **removed** — hover is `Responses.MouseOver(ResponseMouseArgs)` / `MouseOverHook` (the args carry both control + content location) |
| `IInteraction`, `AbstractInteraction` | **removed** — an interaction is a `Responses` subclass; there is no marker interface or abstract interaction base |
| `Canvas.PushInteraction(IInteraction)` | `Canvas.PushFocus(Responses)` (e.g. live host: `PushFocus(new RewireInteraction(...))`, `PushFocus(objectDragInteraction)`) |
| `MouseEventArgs` (Eto) in attribute/interaction mouse handlers | `Grasshopper2.UI.Flex.ResponseMouseArgs` |
| `KeyEventArgs` (Eto) in key handlers | unchanged — `KeyDown/KeyUp` still take `Eto.Forms.KeyEventArgs` |

Unchanged and still valid (keep): `IContextMenuAware`, `ICursorAwareAttributes`, the `Response` enum (`Grasshopper2.UI.Flex.Response` = `{ Ignored, Release, Handled, Capture }`), and the alias `UiResponse = Grasshopper2.UI.Flex.Response`. The `Response` return semantics are identical.

## New contracts (live, decompile-verified)

### `Grasshopper2.UI.Flex.Responses` (abstract — the input model)

```csharp
public abstract class Responses {
    public bool HasFocus { get; }
    public virtual bool HadEffect => true;
    public CoordinateSystem CoordinatesContext { get; }
    public RectangleF RegionBoundary { get; set; }
    public Func<PointF, bool> RegionFilter { get; set; }

    public virtual Response MouseDown(ResponseMouseArgs e);
    public virtual Response MouseDrag(ResponseMouseArgs e);   // replaces the old "MouseMove while captured"
    public virtual Response MouseUp(ResponseMouseArgs e);
    public virtual void     MouseOver(ResponseMouseArgs e);   // replaces hover
    public virtual void     MouseLeave();
    public virtual Response KeyDown(KeyEventArgs e);
    public virtual Response KeyUp(KeyEventArgs e);

    // Extension points — augment default behavior WITHOUT subclassing:
    public event Func<ResponseMouseArgs, Response> MouseDownHook;
    public event Func<ResponseMouseArgs, Response> MouseDragHook;
    public event Func<ResponseMouseArgs, Response> MouseUpHook;
    public event Action<ResponseMouseArgs>         MouseOverHook;
    public event Action                            MouseLeaveHook;
    public event Func<KeyEventArgs, Response>       KeyDownHook;
    public event Func<KeyEventArgs, Response>       KeyUpHook;

    public event EventHandler GotFocus;
    public event EventHandler LostFocus;
    public event EventHandler RedrawRequired;
}
```

Note the renames vs. the old `RespondTo*`: `RespondToMouseMove` → `MouseDrag` (fires only while captured/focused), there is no `SingleClick` method (single-click is derived from down/up; handle in `MouseUp` or via the canvas's click detection).

### `Grasshopper2.UI.Flex.ResponseMouseArgs` (replaces `MouseEventArgs`)

```csharp
public sealed class ResponseMouseArgs {
    public PointF       ControlLocation { get; }   // cursor in control space
    public PointF       ContentLocation { get; }   // cursor in content space (lazy projection)
    public MouseButtons Buttons   => UnderlyingEtoEventArgs.Buttons;
    public Keys         Modifiers => UnderlyingEtoEventArgs.Modifiers;
    public SizeF        Delta     => UnderlyingEtoEventArgs.Delta;
    public float        Pressure  => UnderlyingEtoEventArgs.Pressure;
    public void Invalidate();   // schedule a redraw
    // UnderlyingEtoEventArgs exposes the raw Eto MouseEventArgs.
}
```

`RespondToMouseHover(PointF controlPoint, PointF contentPoint)`'s two arguments are both present on a single `ResponseMouseArgs` (`ControlLocation` + `ContentLocation`).

### `ComponentAttributes` shape (the pattern to mirror)

```csharp
public class ComponentAttributes : Attributes<Component>, IResponsive {
    private sealed class ComponentResponder : Responses {
        public ComponentResponder(ComponentAttributes atts) { ... }
        public override Response MouseDown(ResponseMouseArgs e) { ... }
        // MouseDrag / MouseUp / MouseOver / MouseLeave / KeyDown / KeyUp ...
    }
    private readonly ComponentResponder _responder;
    public Responses Responder => _responder;   // IResponsive
}
```

Two valid extension strategies for a derived attributes class:
1. **Hooks (preferred where it suffices):** keep the base responder; in the attributes constructor wire `Responder.MouseDownHook += e => ...; Responder.MouseOverHook += e => ...;` etc. Hooks run after the base logic and can return a `Response` to override it. No subclass needed.
2. **Responder subclass:** if base behavior must be replaced (not augmented), provide a custom `Responses` subclass and expose it as `Responder` (verify whether `ComponentAttributes.Responder` is `virtual`/replaceable before relying on this; if not, the hooks path is the only seam).

## Affected files (complete inventory)

Removed/changed-type usage counts across `libs/csharp/Rasm.Grasshopper` (grep of `IInteraction|AbstractInteraction|IResponsiveAttributes|IMouseHoverAttributes|RespondToMouse|RespondToKey|HandleMouse|HandleKey|PushInteraction|MouseEventArgs|KeyEventArgs|: ComponentAttributes|ICursorAwareAttributes`):

| File | Usages | Scope |
| --- | --- | --- |
| `Components/Attributes.cs` | 30 | `RasmAttributes` input overrides, `IMouseHoverAttributes`, `MouseEventArgs`/`KeyEventArgs` plumbing, `Callback.Mouse`/`MouseKind` |
| `UI/Interaction.cs` | 17 | `InteractionOp` union (`PushCase(IInteraction)`), `Push(IInteraction)`, `ResizeInteraction : AbstractInteraction`, the nuget-era `RespondToMouseMove`/`RespondToMouseUp` overrides |
| `UI/Events.cs` | 3 | references to the removed types |
| `UI/Input.cs` | 2 | references to the removed types |
| `UI/Canvas.cs` | 2 | canvas-side push/interaction references |

The strict build only surfaces `Components/Attributes.cs` (10) + `UI/Interaction.cs` (3) + the generated `InteractionOp.RegularUnion.g.cs` (1) because compilation stops at the first cascade; `Events.cs`/`Input.cs`/`Canvas.cs` errors appear only after the leading files resolve.

## Per-file migration plan

### `Components/Attributes.cs` — `RasmAttributes`
- Base list: drop `IMouseHoverAttributes` (and `IResponsiveAttributes` if present). Keep `ComponentAttributes`, `IContextMenuAware`, `ICursorAwareAttributes`.
- Replace the 7 `Handle*`/`RespondTo*(MouseEventArgs|KeyEventArgs) : UiResponse` overrides with either responder hooks wired in the constructor, or a nested `Responses` subclass:
  - `HandleMouseDown` → `MouseDown(ResponseMouseArgs)`
  - `HandleMouseMove` → `MouseDrag(ResponseMouseArgs)` (fires only while captured) plus, if free-move is needed, `MouseOver(ResponseMouseArgs)`
  - `HandleMouseUp` → `MouseUp(ResponseMouseArgs)`
  - `HandleSingleClick` → no direct equivalent; derive from `MouseDown`+`MouseUp` (same location within the canvas click window) or drop if the canvas already raises clicks
  - `HandleDoubleClick` → handle in `MouseUp` via the canvas double-click detection (no `RespondToMouseDoubleClick` on `Responses`)
  - `HandleKeyDown`/`HandleKeyUp` → `KeyDown(KeyEventArgs)`/`KeyUp(KeyEventArgs)` (Eto args unchanged)
  - `IMouseHoverAttributes.RespondToMouseHover(controlPoint, contentPoint)` → `MouseOver(ResponseMouseArgs)` / `MouseOverHook` (both locations on the args)
- Re-target `Callback.Mouse`/`MouseKind` to carry `ResponseMouseArgs` instead of `MouseEventArgs`; read button/modifier/location from `ResponseMouseArgs` (`Buttons`, `Modifiers`, `ControlLocation`, `ContentLocation`).
- Expose the responder via `IResponsive.Responder` (the base `ComponentAttributes` already implements `IResponsive`; supply/augment its responder).

### `UI/Interaction.cs`
- `IInteraction` → `Responses`; `AbstractInteraction` → `Responses`.
- `InteractionOp.PushCase(IInteraction Target)` → `PushCase(Responses Target)` (re-runs the `[Union]` generator; `IResponsive`-based members like `RegisterCase(IResponsive ...)` already resolve and stay).
- `Push(IInteraction target)` → `Push(Responses target)` calling `canvas.PushFocus(target)` (replaces `canvas.PushInteraction`).
- `ResizeInteraction : AbstractInteraction` → `ResizeInteraction : Responses`; its `RespondToMouseMove`/`RespondToMouseUp` overrides → `MouseDrag(ResponseMouseArgs)` / `MouseUp(ResponseMouseArgs)`.

### `UI/Events.cs`, `UI/Input.cs`, `UI/Canvas.cs`
- Replace the residual `IInteraction`/`PushInteraction`/`MouseEventArgs` references with `Responses`/`PushFocus`/`ResponseMouseArgs` per the table above. Re-verify each after the two leading files compile, since the strict build will only enumerate these once the cascade clears.

## Verification

- The migration is unverifiable at runtime until bridge scenarios run again; gate on a clean strict build of `Rasm.Grasshopper.csproj` (all severities) plus the `Rasm.AppUi`/`Rasm.Architecture.Tests` closures that include it.
- Re-decompile any member before use against the live host assembly (type-scoped `ilspycmd -t`). Treat the nuget ref as misleading.
