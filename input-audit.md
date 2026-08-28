# 1. Delete the first mirrored cursor rows

`[02]-[POINTER]` code fence, `CursorRow` rows `Default` through `SplitAcross`.

From:
```csharp
    public static readonly CursorRow Default = new(key: 0, resolve: static () => Cursors.Default);
    public static readonly CursorRow Arrow = new(key: 1, resolve: static () => Cursors.Arrow);
    public static readonly CursorRow Crosshair = new(key: 2, resolve: static () => Cursors.Crosshair);
    public static readonly CursorRow Pointer = new(key: 3, resolve: static () => Cursors.Pointer);
    public static readonly CursorRow Caret = new(key: 4, resolve: static () => Cursors.IBeam);
    public static readonly CursorRow Move = new(key: 5, resolve: static () => Cursors.Move);
    public static readonly CursorRow Sizing = new(key: 6, resolve: static () => Cursors.SizeAll);
    public static readonly CursorRow Blocked = new(key: 7, resolve: static () => Cursors.NotAllowed);
    public static readonly CursorRow SplitDown = new(key: 8, resolve: static () => Cursors.VerticalSplit);
    public static readonly CursorRow SplitAcross = new(key: 9, resolve: static () => Cursors.HorizontalSplit);
```

To:
```csharp
// CursorRow rows Default through SplitAcross DELETED
```

Why: Every row is a one-to-one alias for the Eto cursor returned by its delegate and owns no policy.

Change: Pass the required `Eto.Forms.Cursor` directly at the Eto boundary.

Delta: -10 LOC; -10 fields.

Ripples: Remove the claimed shared kernel `CursorRow` from `libs/dotnet/Rasm.AppUi/.planning/Theme/assets.md`; its Avalonia cursor vocabulary remains host-owned.

# 2. Delete the remaining mirrored cursor rows

`[02]-[POINTER]` code fence, `CursorRow` rows `SizeLeft` through `SizeBottomRight`.

From:
```csharp
    public static readonly CursorRow SizeLeft = new(key: 10, resolve: static () => Cursors.SizeLeft);
    public static readonly CursorRow SizeTop = new(key: 11, resolve: static () => Cursors.SizeTop);
    public static readonly CursorRow SizeRight = new(key: 12, resolve: static () => Cursors.SizeRight);
    public static readonly CursorRow SizeBottom = new(key: 13, resolve: static () => Cursors.SizeBottom);
    public static readonly CursorRow SizeTopLeft = new(key: 14, resolve: static () => Cursors.SizeTopLeft);
    public static readonly CursorRow SizeTopRight = new(key: 15, resolve: static () => Cursors.SizeTopRight);
    public static readonly CursorRow SizeBottomLeft = new(key: 16, resolve: static () => Cursors.SizeBottomLeft);
    public static readonly CursorRow SizeBottomRight = new(key: 17, resolve: static () => Cursors.SizeBottomRight);
```

To:
```csharp
// CursorRow rows SizeLeft through SizeBottomRight DELETED
```

Why: These rows repeat the remaining built-in Eto cursor properties without adding validation, conversion, or behavior.

Change: Use the built-in resize cursors directly.

Delta: -8 LOC; -8 fields.

# 3. Delete the cursor forwarding owner

`[02]-[POINTER]` code fence, remaining `CursorRow` declaration after its rows are deleted.

From:
```csharp
[SmartEnum<int>]
public sealed partial class CursorRow {
    [UseDelegateFromConstructor] internal partial Cursor Resolve();

    public Fin<Unit> Apply(Control control);
    public Fin<Unit> Override();
}
```

To:
```csharp
// CursorRow DELETED
```

Why: `Resolve` forwards to one `Cursors` property, while `Apply` and `Override` add a second owner above `Control.Cursor` and `Mouse.SetCursor`.

Change: Delete the type and marshal the direct host write through `UiThread` at the existing boundary.

Delta: -7 LOC; -1 type and -3 members.

# 4. Admit every display rectangle

`[02]-[POINTER]` code fence, `DisplayFacts.IsValid`.

From:
```csharp
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Positive(value: Bounds.Width), ValidityClaim.Positive(value: Bounds.Height),
        ValidityClaim.Positive(value: WorkingArea.Width), ValidityClaim.Positive(value: WorkingArea.Height));
```

To:
```csharp
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Finite(frame: Bounds) && Bounds.Width > 0f && Bounds.Height > 0f,
        ValidityClaim.Finite(frame: WorkingArea) && WorkingArea.Width > 0f && WorkingArea.Height > 0f,
        ValidityClaim.Finite(frame: DisplayBounds) && DisplayBounds.Width > 0f && DisplayBounds.Height > 0f);
```

Why: The current fold omits every rectangle origin and all of `DisplayBounds`, so admitted display evidence can still contain non-finite foreign geometry.

Change: Reuse the shared rectangle claim and retain the stricter positive-extent requirement.

Delta: 0 LOC; 0 members.

# 5. Correct captured pointer evidence

`[02]-[POINTER]` code fence, `PointerSnapshot` and `PointerFact`.

From:
```csharp
[StructLayout(LayoutKind.Auto)]
public readonly record struct PointerSnapshot(EtoPointF Position, MouseButtons Buttons, Keys Modifiers) {
    public bool Holds(MouseButtons buttons) => (Buttons & buttons) == buttons;
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct PointerFact(
    EtoPointF Local, EtoPointF Content, MouseButtons Buttons, Keys Modifiers,
    EtoSizeF Delta, Option<UnitInterval> Pressure) : IValidityEvidence {
    public static Fin<PointerFact> Of(MouseEventArgs args, Control source);

    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Finite(value: Local.X), ValidityClaim.Finite(value: Local.Y),
        ValidityClaim.Finite(value: Content.X), ValidityClaim.Finite(value: Content.Y));
}
```

To:
```csharp
[StructLayout(LayoutKind.Auto)]
public readonly record struct PointerSnapshot(EtoPointF Position, MouseButtons Buttons, Keys Modifiers);

[StructLayout(LayoutKind.Auto)]
public readonly record struct PointerFact(
    EtoPointF Local, EtoPointF Content, MouseButtons Buttons, Keys Modifiers,
    EtoSizeF Delta, UnitInterval Pressure) : IValidityEvidence {
    public static Fin<PointerFact> Of(MouseEventArgs args, Control source);

    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Finite(point: Local), ValidityClaim.Finite(point: Content),
        ValidityClaim.Finite(value: Delta.Width), ValidityClaim.Finite(value: Delta.Height));
}
```

Why: `Holds` only renames a flags expression, Eto supplies a concrete pressure value even when its fallback is `1f`, and the current validity fold omits both delta components.

Change: Delete the convenience member, admit pressure as the value Eto supplies, and validate every raw coordinate component in `Of`.

Delta: -2 LOC; -1 method and one unnecessary `Option` carrier.

# 6. Remove pointer phase keys

`[03]-[GESTURE]` code fence, `PointerPhase` owner and row constructor lines.

From:
```csharp
[SmartEnum<int>]
    public static readonly PointerPhase Over = new(key: 0,
    public static readonly PointerPhase Leave = new(key: 1,
    public static readonly PointerPhase Down = new(key: 2,
    public static readonly PointerPhase Drag = new(key: 3,
    public static readonly PointerPhase Up = new(key: 4,
    public static readonly PointerPhase Wheel = new(key: 5,
    public static readonly PointerPhase SingleClick = new(key: 6,
    public static readonly PointerPhase DoubleClick = new(key: 7,
```

To:
```csharp
[SmartEnum]
    public static readonly PointerPhase Over = new(
    public static readonly PointerPhase Leave = new(
    public static readonly PointerPhase Down = new(
    public static readonly PointerPhase Drag = new(
    public static readonly PointerPhase Up = new(
    public static readonly PointerPhase Wheel = new(
    public static readonly PointerPhase SingleClick = new(
    public static readonly PointerPhase DoubleClick = new(
```

Why: Pointer phases are process-local behavior rows; no consumer reads, parses, persists, or orders their integers.

Change: Use a keyless Thinktecture smart enum while retaining generated exhaustive dispatch and the event-table columns.

Delta: 0 authored LOC; -1 generated `Key` member and the generated keyed lookup/conversion surface.

# 7. Delete invariant pointer admission delegates

`[03]-[GESTURE]` code fence, `PointerPhase` rows other than `SingleClick`.

From:
```csharp
            Add: static (c, h) => c.MouseEnter += h, Drop: static (c, h) => c.MouseEnter -= h),
        admit: static _ => true);
            Add: static (c, h) => c.MouseLeave += h, Drop: static (c, h) => c.MouseLeave -= h),
        admit: static _ => true);
            Add: static (c, h) => c.MouseDown += h, Drop: static (c, h) => c.MouseDown -= h),
        admit: static _ => true);
            Add: static (c, h) => c.MouseMove += h, Drop: static (c, h) => c.MouseMove -= h),
        admit: static _ => true);
            Add: static (c, h) => c.MouseUp += h, Drop: static (c, h) => c.MouseUp -= h),
        admit: static _ => true);
            Add: static (c, h) => c.MouseWheel += h, Drop: static (c, h) => c.MouseWheel -= h),
        admit: static _ => true);
            Add: static (c, h) => c.MouseDoubleClick += h, Drop: static (c, h) => c.MouseDoubleClick -= h),
        admit: static _ => true);
```

To:
```csharp
            Add: static (c, h) => c.MouseEnter += h, Drop: static (c, h) => c.MouseEnter -= h));
            Add: static (c, h) => c.MouseLeave += h, Drop: static (c, h) => c.MouseLeave -= h));
            Add: static (c, h) => c.MouseDown += h, Drop: static (c, h) => c.MouseDown -= h));
            Add: static (c, h) => c.MouseMove += h, Drop: static (c, h) => c.MouseMove -= h));
            Add: static (c, h) => c.MouseUp += h, Drop: static (c, h) => c.MouseUp -= h));
            Add: static (c, h) => c.MouseWheel += h, Drop: static (c, h) => c.MouseWheel -= h));
            Add: static (c, h) => c.MouseDoubleClick += h, Drop: static (c, h) => c.MouseDoubleClick -= h));
```

Why: These seven predicates all return `true`; only the invalid single-click synthesis gives the column any variation.

Change: Remove the constant delegate argument from every unconditional row.

Delta: -7 LOC; seven delegate instances removed.

# 8. Name pointer movement accurately

`[03]-[GESTURE]` code fence, `PointerPhase.Drag` after invariant admission is removed.

From:
```csharp
    public static readonly PointerPhase Drag = new(
        table: new EventTable<Control, MouseEventArgs>(
            Add: static (c, h) => c.MouseMove += h, Drop: static (c, h) => c.MouseMove -= h));
```

To:
```csharp
    public static readonly PointerPhase Move = new(
        table: new EventTable<Control, MouseEventArgs>(
            Add: static (c, h) => c.MouseMove += h, Drop: static (c, h) => c.MouseMove -= h));
```

Why: `MouseMove` fires without a pressed button; drag engagement is separately derived by `DragEvidence.Engaged`.

Change: Rename the phase to the event it represents.

Delta: 0 LOC; 0 members.

Ripples: Replace `PointerPhase.Drag` in `libs/dotnet/Rasm.Grasshopper/.planning/Canvas/interaction.md` and `libs/dotnet/Rasm.Grasshopper/.planning/Components/attributes.md`.

# 9. Delete the fabricated single-click phase

`[03]-[GESTURE]` code fence, `PointerPhase.SingleClick`.

From:
```csharp
    public static readonly PointerPhase SingleClick = new(
        table: new EventTable<Control, MouseEventArgs>(
            Add: static (c, h) => c.MouseUp += h, Drop: static (c, h) => c.MouseUp -= h),
        admit: static args => args.Buttons is MouseButtons.Primary);
```

To:
```csharp
// PointerPhase.SingleClick DELETED
```

Why: Eto exposes no click count on `MouseEventArgs`; a primary `MouseUp` is not evidence that a double click will not follow.

Change: Keep host-specific single-click callbacks in the host fact band instead of deriving them from pointer release.

Delta: -4 LOC; -1 field.

Ripples: Move `MouseSingleClick` handling out of the kernel phase maps in `libs/dotnet/Rasm.Grasshopper/.planning/Canvas/interaction.md` and `libs/dotnet/Rasm.Grasshopper/.planning/Components/attributes.md`.

# 10. Delete pointer admission indirection

`[03]-[GESTURE]` code fence, `PointerPhase.Admit`, `Attach`, and `Bind` after `SingleClick` is deleted.

From:
```csharp
    [UseDelegateFromConstructor] internal partial bool Admit(MouseEventArgs args);

    internal Fin<IDisposable> Attach(Control control, Func<PointerFact, InputVerdict> respond) =>
        Bind(control: control, respond: respond, table: Table, admit: Admit);

    private static Fin<IDisposable> Bind(
        Control control, Func<PointerFact, InputVerdict> respond,
        EventTable<Control, MouseEventArgs> table, Func<MouseEventArgs, bool> admit);
```

To:
```csharp
    internal Fin<IDisposable> Attach(Control control, Func<PointerFact, InputVerdict> respond) =>
        Bind(control: control, respond: respond, table: Table);

    private static Fin<IDisposable> Bind(
        Control control, Func<PointerFact, InputVerdict> respond,
        EventTable<Control, MouseEventArgs> table);
```

Why: No remaining pointer phase has phase-specific admission, so threading a generated predicate through every event adds an unused failure path.

Change: Delete the delegate column and bind directly over the event table.

Delta: -3 LOC; -1 generated member and -1 parameter.

# 11. Remove key phase keys and repeated names

`[03]-[GESTURE]` code fence, `KeyPhase` owner and row constructor lines.

From:
```csharp
[SmartEnum<int>]
    public static readonly KeyPhase KeyDown = new(key: 0,
    public static readonly KeyPhase KeyUp = new(key: 1,
```

To:
```csharp
[SmartEnum]
    public static readonly KeyPhase Down = new(
    public static readonly KeyPhase Up = new(
```

Why: The rows are process-local behavior values with no key consumer, and `KeyPhase.KeyDown` repeats the owner in the member name.

Change: Use a keyless smart enum and the scoped phase names `Down` and `Up`.

Delta: 0 authored LOC; -1 generated `Key` member and the generated keyed lookup/conversion surface.

Ripples: Rename phase references in `libs/dotnet/Rasm.Grasshopper/.planning/Canvas/interaction.md` and `libs/dotnet/Rasm.Grasshopper/.planning/Components/attributes.md`.

# 12. Remove drag phase keys

`[03]-[GESTURE]` code fence, `DragPhase` owner and row constructor lines.

From:
```csharp
[SmartEnum<int>]
    public static readonly DragPhase Enter = new(key: 0,
    public static readonly DragPhase Over = new(key: 1,
    public static readonly DragPhase Leave = new(key: 2,
    public static readonly DragPhase Drop = new(key: 3,
    public static readonly DragPhase End = new(key: 4,
```

To:
```csharp
[SmartEnum]
    public static readonly DragPhase Enter = new(
    public static readonly DragPhase Over = new(
    public static readonly DragPhase Leave = new(
    public static readonly DragPhase Drop = new(
    public static readonly DragPhase End = new(
```

Why: Drag phases are only singleton table owners; their integers have no identity, ordering, parsing, persistence, or wire consumer.

Change: Use a keyless Thinktecture smart enum.

Delta: 0 authored LOC; -1 generated `Key` member and the generated keyed lookup/conversion surface.

# 13. Remove lifecycle stage keys

`[03]-[GESTURE]` code fence, `LifecycleStage`.

From:
```csharp
[SmartEnum<int>]
public sealed partial class LifecycleStage {
    public static readonly LifecycleStage Initialized = new(key: 0);
    public static readonly LifecycleStage Load = new(key: 1);
    public static readonly LifecycleStage Shown = new(key: 2);
    public static readonly LifecycleStage Closing = new(key: 3);
    public static readonly LifecycleStage Closed = new(key: 4);
    public static readonly LifecycleStage Terminating = new(key: 5);
}
```

To:
```csharp
[SmartEnum]
public sealed partial class LifecycleStage {
    public static readonly LifecycleStage Initialized = new();
    public static readonly LifecycleStage Load = new();
    public static readonly LifecycleStage Shown = new();
    public static readonly LifecycleStage Closing = new();
    public static readonly LifecycleStage Closed = new();
    public static readonly LifecycleStage Terminating = new();
}
```

Why: The process-local lifecycle vocabulary is carried as a union payload and never read by integer key, lookup, conversion, persistence, or wire identity.

Change: Retain the generated owner and exhaustive dispatch without a second identity surface.

Delta: 0 authored LOC; -1 generated `Key` member and the generated keyed lookup/conversion surface.

# 14. Make the fact floor a marker bound

`[03]-[GESTURE]` code fence, `IUiFact`.

From:
```csharp
public interface IUiFact { string Kind { get; } }
```

To:
```csharp
public interface IUiFact { }
```

Why: The interface is used as a generic bound, while `Kind` forces each closed fact type to reproduce case identity as an unchecked string; `CaptureFrame` already demonstrates that the stated member is not satisfied.

Change: Retain only the generic marker.

Delta: 0 LOC; -1 interface member.

Ripples: Delete the interface-only `Invocation.Kind` in `libs/dotnet/Rasm/.planning/Interaction/chrome.md` and the mirrored `GhFact.Kind` switch in `libs/dotnet/Rasm.Grasshopper/.planning/Shell/events.md`; retain `ChromeTrace.Kind` because it is concrete trace payload.

# 15. Delete the union-case string roster

`[03]-[GESTURE]` code fence, `UiFact.Kind`.

From:
```csharp
    public string Kind => Switch(
        gestureCase:  static _ => "gesture",
        keyCase:      static _ => "key",
        textCase:     static _ => "text",
        dragCase:     static _ => "drag",
        focusCase:    static _ => "focus",
        boundsCase:   static _ => "bounds",
        densityCase:  static _ => "density",
        stateCase:    static _ => "state",
        lifeCase:     static _ => "life",
        modifierCase: static _ => "modifier",
        beatCase:     static _ => "beat",
        noticeCase:   static _ => "notice",
        faultCase:    static _ => "fault");
```

To:
```csharp
// UiFact.Kind DELETED
```

Why: The property mirrors the union's exhaustive case structure with thirteen unchecked literals and has no domain consumer.

Change: Dispatch directly through the generated exhaustive `Switch` when a consumer needs case-specific behavior.

Delta: -14 LOC; -1 member and -13 string literals.

# 16. Remove grip vocabulary keys

`[04]-[PICK]` code fence, `GripEdge` and `GripCorner` owner and row constructor lines.

From:
```csharp
[SmartEnum<int>]
    public static readonly GripEdge Left = new(key: 0);
    public static readonly GripEdge Right = new(key: 1);
    public static readonly GripEdge Top = new(key: 2);
    public static readonly GripEdge Bottom = new(key: 3);
[SmartEnum<int>]
    public static readonly GripCorner TopLeft = new(key: 0, across: GripEdge.Left, down: GripEdge.Top);
    public static readonly GripCorner TopRight = new(key: 1, across: GripEdge.Right, down: GripEdge.Top);
    public static readonly GripCorner BottomLeft = new(key: 2, across: GripEdge.Left, down: GripEdge.Bottom);
    public static readonly GripCorner BottomRight = new(key: 3, across: GripEdge.Right, down: GripEdge.Bottom);
```

To:
```csharp
[SmartEnum]
    public static readonly GripEdge Left = new();
    public static readonly GripEdge Right = new();
    public static readonly GripEdge Top = new();
    public static readonly GripEdge Bottom = new();
[SmartEnum]
    public static readonly GripCorner TopLeft = new(across: GripEdge.Left, down: GripEdge.Top);
    public static readonly GripCorner TopRight = new(across: GripEdge.Right, down: GripEdge.Top);
    public static readonly GripCorner BottomLeft = new(across: GripEdge.Left, down: GripEdge.Bottom);
    public static readonly GripCorner BottomRight = new(across: GripEdge.Right, down: GripEdge.Bottom);
```

Why: Neither process-local vocabulary has a key consumer; corner identity is already its singleton case with the two valid edge columns.

Change: Use keyless generated owners and retain the load-bearing corner columns.

Delta: 0 authored LOC; -2 generated `Key` members and both generated keyed lookup/conversion surfaces.

# 17. Use an ad-hoc union for grip alternatives

`[04]-[PICK]` code fence, `EdgeGrip`.

From:
```csharp
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record EdgeGrip {
    private EdgeGrip() { }
    public sealed record Edge(GripEdge Side) : EdgeGrip;
    public sealed record Corner(GripCorner At) : EdgeGrip;
    public sealed record Whole : EdgeGrip;
}
```

To:
```csharp
[Union<GripEdge, GripCorner, Unit>(T1Name = "Edge", T2Name = "Corner", T3Name = "Whole", T3IsStateless = true)]
public readonly partial struct EdgeGrip;
```

Why: The alternatives are a positional sum of two existing values and one stateless case; three nested record types add no payload semantics.

Change: Let Thinktecture generate the compact union and exhaustive dispatch.

Delta: -5 LOC; -3 nested types.

Ripples: Replace nested-case construction and matching with the generated named ad-hoc union surface in `libs/dotnet/Rasm.Grasshopper/.planning/Canvas/interaction.md`.

# 18. Remove atomicity keys

`[04]-[PICK]` code fence, `Atomicity`.

From:
```csharp
[SmartEnum<int>]
public sealed partial class Atomicity {
    public static readonly Atomicity Partial = new(key: 0);
    public static readonly Atomicity AllOrNothing = new(key: 1);
}
```

To:
```csharp
[SmartEnum]
public sealed partial class Atomicity {
    public static readonly Atomicity Partial = new();
    public static readonly Atomicity AllOrNothing = new();
}
```

Why: Atomicity is a process-local behavior choice and no consumer uses integer identity, lookup, conversion, persistence, or ordering.

Change: Use a keyless Thinktecture owner.

Delta: 0 authored LOC; -1 generated `Key` member and the generated keyed lookup/conversion surface.

# 19. Use an ad-hoc union for event anchors

`[04]-[PICK]` code fence, `EventAnchor`.

From:
```csharp
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record EventAnchor {
    private EventAnchor() { }
    public sealed record OnControl(Control Value) : EventAnchor;
    public sealed record OnWindow(Window Value) : EventAnchor;
    public sealed record Ambient : EventAnchor;
    public sealed record OnClock(UiClock Value) : EventAnchor;
}
```

To:
```csharp
[Union<Control, Window, Unit, UiClock>(
    T1Name = "OnControl", T2Name = "OnWindow", T3Name = "Ambient", T4Name = "OnClock", T3IsStateless = true)]
public readonly partial struct EventAnchor;
```

Why: The anchor is a positional sum of three existing host values and one stateless alternative; wrapper records contribute no domain state.

Change: Use the generated named ad-hoc union and its exhaustive dispatch.

Delta: -5 LOC; -4 nested types.

Ripples: Update `EventAnchor.OnControl` and `EventAnchor.Ambient` construction and matching to the generated union surface in `libs/dotnet/Rasm.Grasshopper/.planning/Shell/events.md` and `libs/dotnet/Rasm.Grasshopper/.planning/Document/solution.md`.

# 20. Correct pointer, key, and text source delegates

`[04]-[PICK]` code fence, `UiSource` pointer, key, and text row delegate headers.

From:
```csharp
        attach: static (anchor, emit, key) => OnControl(anchor, emit, PointerPhase.Over.Table, Gesture(PointerPhase.Over)));
        attach: static (anchor, emit, key) => OnControl(anchor, emit, PointerPhase.Leave.Table, Gesture(PointerPhase.Leave)));
        attach: static (anchor, emit, key) => OnControl(anchor, emit, PointerPhase.Down.Table, Gesture(PointerPhase.Down)));
    public static readonly UiSource PointerDrag = new(key: "pointer.drag",
        attach: static (anchor, emit, key) => OnControl(anchor, emit, PointerPhase.Drag.Table, Gesture(PointerPhase.Drag)));
        attach: static (anchor, emit, key) => OnControl(anchor, emit, PointerPhase.Up.Table, Gesture(PointerPhase.Up)));
        attach: static (anchor, emit, key) => OnControl(anchor, emit, PointerPhase.Wheel.Table, Gesture(PointerPhase.Wheel)));
        attach: static (anchor, emit, key) => OnControl(anchor, emit, PointerPhase.DoubleClick.Table, Gesture(PointerPhase.DoubleClick)));
        attach: static (anchor, emit, key) => OnControl(anchor, emit, KeyPhase.KeyDown.Table, Keyed(KeyPhase.KeyDown)));
        attach: static (anchor, emit, key) => OnControl(anchor, emit, KeyPhase.KeyUp.Table, Keyed(KeyPhase.KeyUp)));
        attach: static (anchor, emit, key) => OnControl(anchor, emit,
```

To:
```csharp
        attach: static (anchor, emit) => OnControl(anchor, emit, PointerPhase.Over.Table, Gesture(PointerPhase.Over)));
        attach: static (anchor, emit) => OnControl(anchor, emit, PointerPhase.Leave.Table, Gesture(PointerPhase.Leave)));
        attach: static (anchor, emit) => OnControl(anchor, emit, PointerPhase.Down.Table, Gesture(PointerPhase.Down)));
    public static readonly UiSource PointerMove = new(key: "pointer.move",
        attach: static (anchor, emit) => OnControl(anchor, emit, PointerPhase.Move.Table, Gesture(PointerPhase.Move)));
        attach: static (anchor, emit) => OnControl(anchor, emit, PointerPhase.Up.Table, Gesture(PointerPhase.Up)));
        attach: static (anchor, emit) => OnControl(anchor, emit, PointerPhase.Wheel.Table, Gesture(PointerPhase.Wheel)));
        attach: static (anchor, emit) => OnControl(anchor, emit, PointerPhase.DoubleClick.Table, Gesture(PointerPhase.DoubleClick)));
        attach: static (anchor, emit) => OnControl(anchor, emit, KeyPhase.Down.Table, Keyed(KeyPhase.Down)));
        attach: static (anchor, emit) => OnControl(anchor, emit, KeyPhase.Up.Table, Keyed(KeyPhase.Up)));
        attach: static (anchor, emit) => OnControl(anchor, emit,
```

Why: Generated `Attach` has two parameters, while every stored delegate declares a nonexistent third input; the movement source also repeats the false drag name.

Change: Match the generated constructor delegate and apply the corrected phase names and source key.

Delta: 0 LOC; 0 members.

# 21. Correct drag and control attachment calls

`[04]-[PICK]` code fence, `UiSource` drag, focus, resize, load, and shown row delegate headers.

From:
```csharp
        attach: static (anchor, emit, key) =>
            OnControl(anchor, key, emit, DragPhase.Enter.Table, Dragged(DragPhase.Enter)));
        attach: static (anchor, emit, key) =>
            OnControl(anchor, key, emit, DragPhase.Over.Table, Dragged(DragPhase.Over)));
        attach: static (anchor, emit, key) =>
            OnControl(anchor, key, emit, DragPhase.Leave.Table, Dragged(DragPhase.Leave)));
        attach: static (anchor, emit, key) =>
            OnControl(anchor, key, emit, DragPhase.Drop.Table, Dragged(DragPhase.Drop)));
        attach: static (anchor, emit, key) =>
            OnControl(anchor, key, emit, DragPhase.End.Table, Dragged(DragPhase.End)));
        attach: static (anchor, emit, key) => OnControl(anchor, emit,
        attach: static (anchor, emit, key) => OnControl(anchor, emit,
        attach: static (anchor, emit, key) => OnControl(anchor, emit,
        attach: static (anchor, emit, key) => OnControl(anchor, emit,
        attach: static (anchor, emit, key) => OnControl(anchor, emit,
```

To:
```csharp
        attach: static (anchor, emit) =>
            OnControl(anchor, emit, DragPhase.Enter.Table, Dragged(DragPhase.Enter)));
        attach: static (anchor, emit) =>
            OnControl(anchor, emit, DragPhase.Over.Table, Dragged(DragPhase.Over)));
        attach: static (anchor, emit) =>
            OnControl(anchor, emit, DragPhase.Leave.Table, Dragged(DragPhase.Leave)));
        attach: static (anchor, emit) =>
            OnControl(anchor, emit, DragPhase.Drop.Table, Dragged(DragPhase.Drop)));
        attach: static (anchor, emit) =>
            OnControl(anchor, emit, DragPhase.End.Table, Dragged(DragPhase.End)));
        attach: static (anchor, emit) => OnControl(anchor, emit,
        attach: static (anchor, emit) => OnControl(anchor, emit,
        attach: static (anchor, emit) => OnControl(anchor, emit,
        attach: static (anchor, emit) => OnControl(anchor, emit,
        attach: static (anchor, emit) => OnControl(anchor, emit,
```

Why: The stored delegate has two inputs, and `OnControl` accepts `(anchor, emit, table, project)` rather than the five arguments used by the drag rows.

Change: Remove both nonexistent key inputs.

Delta: 0 LOC; 0 members.

# 22. Correct window, application, ambient, and clock delegates

`[04]-[PICK]` code fence, `UiSource` window, application, ambient, and clock row delegate headers.

From:
```csharp
        attach: static (anchor, emit, key) => OnWindow(anchor, emit,
        attach: static (anchor, emit, key) => OnWindow(anchor, emit,
        attach: static (anchor, emit, key) => OnWindow(anchor, emit,
        attach: static (anchor, emit, key) => OnWindow(anchor, emit,
        attach: static (anchor, emit, key) => OnApp(anchor, emit,
        attach: static (anchor, emit, key) => OnApp(anchor, emit,
        attach: static (anchor, emit, key) => OnApp(anchor, emit,
        attach: static (anchor, emit, key) => OnApp(anchor, emit,
        attach: static (anchor, emit, key) => OnAmbient(anchor, emit,
            static _ => Fin.Succ<UiFact>(new UiFact.ModifierCase(Modifiers: Keyboard.Modifiers))));
        attach: static (anchor, emit, key) => OnClock(anchor, emit,
            static (clock, observer, op) => clock.Tap(observer: observer),
```

To:
```csharp
        attach: static (anchor, emit) => OnWindow(anchor, emit,
        attach: static (anchor, emit) => OnWindow(anchor, emit,
        attach: static (anchor, emit) => OnWindow(anchor, emit,
        attach: static (anchor, emit) => OnWindow(anchor, emit,
        attach: static (anchor, emit) => OnApp(anchor, emit,
        attach: static (anchor, emit) => OnApp(anchor, emit,
        attach: static (anchor, emit) => OnApp(anchor, emit,
        attach: static (anchor, emit) => OnApp(anchor, emit,
        attach: static (anchor, emit) => OnAmbient(anchor, emit,
            static () => Fin.Succ<UiFact>(new UiFact.ModifierCase(Modifiers: Keyboard.Modifiers))));
        attach: static (anchor, emit) => OnClock(anchor, emit,
            static (clock, observer) => clock.Tap(observer: observer),
```

Why: `Attach` has two inputs, `OnAmbient` takes a zero-input projection, and the clock tap delegate takes only the clock and observer.

Change: Match each declared delegate surface exactly.

Delta: 0 LOC; 0 members.

# 23. Correct event projection arity

`[04]-[PICK]` code fence, inline projections and projection factories from `Gesture` through `Focused`.

From:
```csharp
            static (_, args, _) => Fin.Succ<UiFact>(new UiFact.TextCase(Text: args.Text))));
            static (control, _, _) => Fin.Succ<UiFact>(new UiFact.BoundsCase(Bounds: control.Bounds))));
            static (window, _, _) => Fin.Succ<UiFact>(new UiFact.StateCase(State: window.WindowState))));
            static (window, _, op) => FactoryBridge.Accept<PositiveMagnitude>(
            static (_, args, op) => Admit.Need(value: args.ExceptionObject as Exception)
            static (_, args, _) => Fin.Succ<UiFact>(new UiFact.NoticeCase(Id: args.ID, Data: Optional(args.UserData))));
        (control, args, key) => PointerFact.Of(args: args, source: control)
        (_, args, _) => Fin.Succ<UiFact>(new UiFact.KeyCase(Key: args.Key, Modifiers: args.Modifiers, Phase: phase));
        (_, args, _) => Fin.Succ<UiFact>(new UiFact.DragCase(At: args.Location, Effect: args.AllowedEffects, Phase: phase));
        (_, _, _) => Fin.Succ<UiFact>(new UiFact.LifeCase(Stage: stage));
        (_, _, _) => Fin.Succ<UiFact>(new UiFact.FocusCase(Gained: gained));
```

To:
```csharp
            static (_, args) => Fin.Succ<UiFact>(new UiFact.TextCase(Text: args.Text))));
            static (control, _) => Fin.Succ<UiFact>(new UiFact.BoundsCase(Bounds: control.Bounds))));
            static (window, _) => Fin.Succ<UiFact>(new UiFact.StateCase(State: window.WindowState))));
            static (window, _) => FactoryBridge.Accept<PositiveMagnitude>(
            static (_, args) => Admit.Need(value: args.ExceptionObject as Exception)
            static (_, args) => Fin.Succ<UiFact>(new UiFact.NoticeCase(Id: args.ID, Data: Optional(args.UserData))));
        (control, args) => PointerFact.Of(args: args, source: control)
        (_, args) => Fin.Succ<UiFact>(new UiFact.KeyCase(Key: args.Key, Modifiers: args.Modifiers, Phase: phase));
        (_, args) => Fin.Succ<UiFact>(new UiFact.DragCase(At: args.Location, Effect: args.AllowedEffects, Phase: phase));
        (_, _) => Fin.Succ<UiFact>(new UiFact.LifeCase(Stage: stage));
        (_, _) => Fin.Succ<UiFact>(new UiFact.FocusCase(Gained: gained));
```

Why: `OnControl`, `OnWindow`, and `OnApp` all declare two-input projectors; every listed lambda currently declares a third input.

Change: Remove the nonexistent operation parameter from each projector.

Delta: 0 LOC; 0 members.

# 24. Cache the default drain policy directly

`[04]-[PICK]` code fence, `DrainPolicy`.

From:
```csharp
public sealed record DrainPolicy(Dimension Capacity, BoundedChannelFullMode Full) {
    public static DrainPolicy Default => Seed.Value;

    private static readonly Lazy<DrainPolicy> Seed = new(static () => new(
        Capacity: Dimension.Create(value: (int)(TimeSpan.FromSeconds(value: 1d) / DispatchLane.Paced.Bound)),
        Full: BoundedChannelFullMode.DropOldest));
}
```

To:
```csharp
public sealed record DrainPolicy(Dimension Capacity, BoundedChannelFullMode Full) {
    public static DrainPolicy Default { get; } = new(
        Capacity: Dimension.Create(value: (int)(TimeSpan.FromSeconds(value: 1d) / DispatchLane.Paced.Bound)),
        Full: BoundedChannelFullMode.DropOldest);
}
```

Why: The immutable default needs one cached value, not a separate `Lazy<T>` cell and forwarding property.

Change: Initialize the static property directly.

Delta: -2 LOC; -1 field.

# 25. Use event-subscription terminology

`[04]-[PICK]` code fence, `UiSubscription.Seated`.

From:
```csharp
    public Seq<IUiSource<TFact>> Seated { get; }
```

To:
```csharp
    public Seq<IUiSource<TFact>> Attached { get; }
```

Why: `Attached` is the established state of an event subscription; `Seated` introduces unrelated terminology for the same host operation.

Change: Rename the successful attachment roster.

Delta: 0 LOC; 0 members.

# 26. Remove the second drain consumer surface

`[04]-[PICK]` code fence, `EvidenceDrain<TFact>.Open`.

From:
```csharp
    public static Fin<Lease<EvidenceDrain<TFact>>> Open(
        MonotonicTimeline clock,
        Option<DrainPolicy> policy = default,
        Option<Action<UiEvent<TFact>>> onShed = default);
```

To:
```csharp
    public static Fin<Lease<EvidenceDrain<TFact>>> Open(
        MonotonicTimeline clock,
        Option<DrainPolicy> policy = default);
```

Why: The optional callback creates a second event-consumption path beside `Reader`; loss observation is already represented by the `Shed` counter.

Change: Keep the bounded-channel drop observer internal for accounting and expose only the reader and counters.

Delta: -1 LOC; -1 parameter.

# 27. Make idempotent completion total

`[04]-[PICK]` code fence, `EvidenceDrain<TFact>.Complete`.

From:
```csharp
    public Fin<Unit> Complete();
```

To:
```csharp
    public Unit Complete();
```

Why: Declared-idempotent channel completion has no expected failure for a caller to recover from; `Fin` fabricates a failure branch around `TryComplete`.

Change: Collapse the host boolean internally and return `Unit`.

Delta: 0 LOC; one unnecessary `Fin` carrier removed.

Ripples: Replace `Fin<Unit> completed = drain.Complete()` with a total completion call in `libs/dotnet/Rasm.Grasshopper/.planning/Platform/capture.md`.
