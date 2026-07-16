# [RASM_RHINO_ACQUISITION]

Declarative input-acquisition owner (`Rasm.Rhino.Commands`). One `InputKind` matrix carries every command-line acquisition modality the host admits — constrained dynamic points, filtered object picks, bounded numbers and integers, text, toggles, colors, distances, the one-shot geometric primitives, views and viewport sets, and interactive transforms — each row one declared capability: the native route, the typed payload projection, the option participation, and the request-slot admission are row data, so a new modality is one row and the census-era `typeof(T)` registry with its per-getter factory branches is dead. One `Acquire` request record parameterizes every row — prompt, typed default, accept policy, command-line options, point constraints, snap and construction points, dynamic-draw feedback, numeric bounds, object filters — and one `Acquisition.Get` entry proves the session `Acquire` grant around the getter window, drives the getter with the internalized option loop, and returns one `AcquiredReceipt` whose terminal discriminates value, cancel, empty, and host exit on the rail. Scripted execution needs no second path: the host getters consume script tokens natively under the scripted run lane, so interactive and scripted acquisition are one matrix.

## [01]-[INDEX]

- [02]-[PAYLOAD_AND_TERMINAL]: the `Acquired` payload union, `PointEvidence`, `AcquireTerminal`, and the `AcquiredReceipt` product.
- [03]-[REQUEST_ALGEBRA]: `InputDefault`, `AcceptPolicy`, `PointPolicy`, `InputFeedback`, `ObjectPolicy`, and the one `Acquire` request record.
- [04]-[POINT_CONSTRAINTS]: the `PointConstraint` union over the full native constraint family.
- [05]-[INPUT_MATRIX]: the `InputKind` rows, the getter drive with the option loop, and the `Acquisition.Get` entry.
- [06]-[SURFACE_LEDGER]: the page's owner table.

## [02]-[PAYLOAD_AND_TERMINAL]

- Owner: `Acquired` `[Union]` — one typed case per payload shape the matrix produces; `PointEvidence` — the point-row evidence product read off the getter at the terminal; `AcquireTerminal` `[Union]` — the outcome family: `Value`, `Cancelled`, `Nothing`, `Undone`, `Exit`; `AcquiredReceipt` — terminal plus the option selections the drive accumulated and the default-accepted fact.
- Law: cancel, empty-enter, undo, and host shutdown are terminal cases, never faults — `GetResult.Cancel` folds to `Cancelled`, `GetResult.Nothing` to `Nothing`, `GetResult.Undo` to `Undone`, and `GetResult.ExitRhino` to `Exit`, so the command page's drive fold reads the outcome from the case; a native refusal, an inadmissible request, or a host result the row does not admit (`NoResult`, `Miss`, `Timeout`, a mismatched value kind) lands on the `Fin` fail side carrying the raw discriminant.
- Law: the payload preserves the row's own evidence — a point carries its viewport, osnap event, and base point; an object pick carries the selection captures the pick projection owner mints; a view choice carries the runtime serial rather than the live handle — so no host getter state survives past the receipt.
- Law: host handles never ride the payload — `RhinoView` projects to its runtime serial and `RhinoViewport` to its id, resolved back through the owning host surface at consumption time.
- Growth: a new payload shape is one `Acquired` case plus the row that produces it; the receipt, the terminal, and the command drive read it with zero new surface.

```csharp
// --- [TYPES] ------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Acquired {
    private Acquired() { }
    public sealed record Point(Point3d Value, PointEvidence Evidence) : Acquired;
    public sealed record Objects(Seq<PickCapture> Picks) : Acquired;
    public sealed record Number(double Value) : Acquired;
    public sealed record Count(int Value) : Acquired;
    public sealed record Text(string Value) : Acquired;
    public sealed record Toggle(bool Value) : Acquired;
    public sealed record Paint(System.Drawing.Color Value) : Acquired;
    public sealed record Distance(double Value) : Acquired;
    public sealed record Segment(Line Value) : Acquired;
    public sealed record Chain(Polyline Value) : Acquired;
    public sealed record ArcShape(Arc Value) : Acquired;
    public sealed record CircleShape(Circle Value) : Acquired;
    public sealed record PlaneShape(Plane Value) : Acquired;
    public sealed record RectangleShape(Arr<Point3d> Corners) : Acquired;
    public sealed record BoxShape(Box Value) : Acquired;
    public sealed record ViewChoice(uint ViewSerial) : Acquired;
    public sealed record ViewportChoice(Seq<Guid> ViewportIds) : Acquired;
    public sealed record Xform(Transform Value) : Acquired;

    public Option<TCase> As<TCase>() where TCase : Acquired => Optional(this as TCase);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AcquireTerminal {
    private AcquireTerminal() { }
    public sealed record Value(Acquired Payload) : AcquireTerminal;
    public sealed record Cancelled : AcquireTerminal;
    public sealed record Nothing : AcquireTerminal;
    public sealed record Undone : AcquireTerminal;
    public sealed record Exit : AcquireTerminal;
}

// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct PointEvidence(
    Option<Guid> ViewportId,
    global::Rhino.ApplicationSettings.OsnapModes Osnap,
    Option<Point3d> BasePoint);

public sealed record AcquiredReceipt(AcquireTerminal Terminal, Seq<OptionChoice> Options, bool GotDefault) : IDetachedDocumentResult {
    public Option<Acquired> Payload =>
        Terminal is AcquireTerminal.Value terminal ? Some(terminal.Payload) : Option<Acquired>.None;
}
```

## [03]-[REQUEST_ALGEBRA]

- Owner: `InputDefault` `[Union]` — the typed default the getter displays and yields on empty accept, dispatched onto the `SetDefault*` family; `AcceptPolicy` — the accept-terminal grants, the transparent-command and wait-duration knobs, and the option-loop budget as one value; `PointPolicy` — the point row's whole policy: constraints, snap and construction points, base point with its status-bar distance and rubber-band line, distance-from-base confinement, draw color, cursor style, osnap-cursor and ortho/object-snap permits, the curve-snap tangent/perpendicular/arrow bars, constraint-option and from-option and tab-mode grants, the elevator mode, snap-to-curves, exit-redraw suppression, mouse-up capture, 2D capture, and the feedback sinks; `InputFeedback` — the dynamic-draw dimension: mouse-move, mouse-down, dynamic-draw, and post-draw sinks over the host frame payloads; `ObjectPolicy` — the object row's filter and selection-policy value; `SlotGrants` — the per-row slot-admission columns, `Modal` and `Custom` the two seeds a row refines; `Acquire` — the ONE request record every row reads its slots from.
- Law: the request is one record, never a per-row request family — each row declares the slots it consumes as `SlotGrants` columns, and one derived fold over (slot presence × grant) refuses a request carrying an ungranted slot with a typed admission fault before any native getter constructs; a slot a row cannot consume never rides silently, and admission logic never re-derives row identity.
- Law: feedback sinks are frame-window values — the host args a sink receives are valid only inside the callback, so a sink writes and returns; a post-draw sink requires the full-frame redraw regime, so the point row raises `FullFrameRedrawDuringGet` with the sink; the in-viewport gumball and preview overlay compose these sinks from the display owner, and this page carries only the policy dimension.
- Law: a default is typed data — `InputDefault` dispatches onto the matching `SetDefault*` member, and the accepted-default fact returns on the receipt through `GotDefault`, so a consumer distinguishes typed-in from defaulted without re-reading getter state.
- Boundary: option rows arrive as the options page's `OptionSet` and return as its `OptionChoice` — this page binds and loops, never re-derives option identity, decode, or lifetime.

```csharp
// --- [TYPES] ------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record InputDefault {
    private InputDefault() { }
    public sealed record PointValue(Point3d Value) : InputDefault;
    public sealed record NumberValue(double Value) : InputDefault;
    public sealed record CountValue(int Value) : InputDefault;
    public sealed record TextValue(string Value) : InputDefault;
    public sealed record PaintValue(System.Drawing.Color Value) : InputDefault;

    internal Unit Apply(GetBaseClass getter) =>
        Switch(
            state: getter,
            pointValue: static (g, value) => Op.Side(() => g.SetDefaultPoint(point: value.Value)),
            numberValue: static (g, value) => Op.Side(() => g.SetDefaultNumber(defaultNumber: value.Value)),
            countValue: static (g, value) => Op.Side(() => g.SetDefaultInteger(defaultValue: value.Value)),
            textValue: static (g, value) => Op.Side(() => g.SetDefaultString(defaultValue: value.Value)),
            paintValue: static (g, value) => Op.Side(() => g.SetDefaultColor(defaultColor: value.Value)));
}

// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct AcceptPolicy(
    bool Nothing = false,
    bool Undo = false,
    bool EnterWhenDone = false,
    Option<bool> NumberWithZero = default,
    bool Point = false,
    bool Color = false,
    bool Text = false,
    bool Transparent = true,
    Option<int> WaitMs = default,
    int OptionBudget = 64) {
    internal Unit Apply(GetBaseClass getter) {
        AcceptPolicy policy = this;
        return Op.Side(() => {
            getter.AcceptNothing(enable: policy.Nothing);
            getter.AcceptUndo(enable: policy.Undo);
            getter.AcceptEnterWhenDone(enable: policy.EnterWhenDone);
            _ = policy.NumberWithZero.Iter(zero => getter.AcceptNumber(enable: true, acceptZero: zero));
            getter.AcceptPoint(enable: policy.Point);
            getter.AcceptColor(enable: policy.Color);
            getter.AcceptString(enable: policy.Text);
            getter.EnableTransparentCommands(enable: policy.Transparent);
            _ = policy.WaitMs.Iter(ms => getter.SetWaitDuration(ms));
        });
    }
}

public sealed record InputFeedback(
    Option<Action<GetPointMouseEventArgs>> MouseMove = default,
    Option<Action<GetPointMouseEventArgs>> MouseDown = default,
    Option<Action<GetPointDrawEventArgs>> DynamicDraw = default,
    Option<Action<DrawEventArgs>> PostDraw = default);

public sealed record PointPolicy(
    Seq<PointConstraint> Constraints = default,
    Seq<Point3d> SnapPoints = default,
    Seq<Point3d> ConstructionPoints = default,
    Option<(Point3d Anchor, bool ShowDistance, bool DrawLine)> Base = default,
    Option<double> DistanceFromBase = default,
    Option<System.Drawing.Color> DrawColor = default,
    Option<InputFeedback> Feedback = default,
    Option<global::Rhino.UI.CursorStyle> Cursor = default,
    Option<bool> ObjectSnapCursors = default,
    Option<bool> PermitOrthoSnap = default,
    Option<bool> PermitObjectSnap = default,
    Option<(bool Enabled, bool Ends)> CurveSnapTangent = default,
    Option<(bool Enabled, bool Ends)> CurveSnapPerpendicular = default,
    Option<(bool Enabled, bool Reverse)> CurveSnapArrow = default,
    bool ConstraintOptions = true,
    bool FromOption = true,
    bool TabMode = true,
    int ElevatorMode = 0,
    bool SnapToCurves = false,
    bool NoRedrawOnExit = false,
    bool OnMouseUp = false,
    bool TwoDimensional = false) {
    public static PointPolicy Free { get; } = new();
}

public sealed record ObjectPolicy(
    ObjectType Types = ObjectType.AnyObject,
    Option<GetObjectGeometryFilter> Custom = default,
    (int Minimum, int Maximum) Range = default,
    (bool Enabled, bool IgnoreUnacceptable) PreSelect = default,
    bool PostSelect = true,
    bool SelPrevious = true,
    bool Highlight = true,
    bool IgnoreGrips = true,
    bool PressEnterWhenDonePrompt = true) {
    public static ObjectPolicy One { get; } = new(Range: (Minimum: 1, Maximum: 1), PreSelect: (Enabled: true, IgnoreUnacceptable: true));
    public static ObjectPolicy Many { get; } = new(Range: (Minimum: 1, Maximum: 0), PreSelect: (Enabled: true, IgnoreUnacceptable: true));
}

public readonly record struct SlotGrants(
    bool Options = false,
    bool Point = false,
    bool Objects = false,
    bool Bounds = false,
    bool ToggleNames = false,
    bool Calculate = false,
    bool TypedDefault = false,
    bool PromptDefault = false) {
    internal static SlotGrants Modal { get; } = new();
    internal static SlotGrants Custom { get; } = new(Options: true, TypedDefault: true, PromptDefault: true);
}

public sealed record Acquire(
    InputKind Kind,
    string Prompt,
    Option<string> PromptDefault = default,
    Option<InputDefault> Default = default,
    AcceptPolicy Accept = default,
    Option<OptionSet> Options = default,
    Option<PointPolicy> Point = default,
    Option<ObjectPolicy> Objects = default,
    Option<(double Lower, double Upper)> Bounds = default,
    Option<(string Off, string On)> ToggleNames = default,
    Option<Func<RhinoViewport, Point3d, Transform>> Calculate = default);
```

## [04]-[POINT_CONSTRAINTS]

- Owner: `PointConstraint` `[Union]` — the whole native `GetPoint` constraint family as cases: segment, line, arc, circle, plane with its elevator grant, sphere, cylinder, curve, surface, brep face with wire density, mesh, the construction plane with its base-point anchor, the target plane, and the virtual construction-plane intersection — one `Apply` fold onto the getter, confirmation-railed per case.
- Law: constraints compose in declaration order — the request carries a `Seq<PointConstraint>` and the drive traverses the fold, so a failing constraint refuses the whole acquisition before the getter runs rather than silently picking free.
- Law: the construction-plane case anchors through the base point when demanded — the native surface has no member that disables construction-plane confinement, so the case payload carries only the anchor decision.
- Boundary: pure constraint geometry — which plane, which curve — arrives as admitted kernel-checked values from the caller; this union only crosses them onto the native getter.

```csharp
// --- [TYPES] ------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PointConstraint {
    private PointConstraint() { }
    public sealed record OnSegment(Point3d From, Point3d To) : PointConstraint;
    public sealed record OnLine(Line Value) : PointConstraint;
    public sealed record OnArc(Arc Value) : PointConstraint;
    public sealed record OnCircle(Circle Value) : PointConstraint;
    public sealed record OnPlane(Plane Value, bool AllowElevator = true) : PointConstraint;
    public sealed record OnSphere(Sphere Value) : PointConstraint;
    public sealed record OnCylinder(Cylinder Value) : PointConstraint;
    public sealed record OnCurve(Curve Value, bool AllowPickingOff = false) : PointConstraint;
    public sealed record OnSurface(Surface Value, bool AllowPickingOff = false) : PointConstraint;
    public sealed record OnBrep(Brep Value, int WireDensity = 1, int FaceIndex = -1, bool AllowPickingOff = false) : PointConstraint;
    public sealed record OnMesh(Mesh Value, bool AllowPickingOff = false) : PointConstraint;
    public sealed record OnConstructionPlane(bool ThroughBasePoint = true) : PointConstraint;
    public sealed record OnTargetPlane : PointConstraint;
    public sealed record OnCPlaneIntersection(Plane Value) : PointConstraint;

    internal Fin<Unit> Apply(GetPoint getter, Op key) =>
        Switch(
            state: (Getter: getter, Op: key),
            onSegment: static (ctx, c) => ctx.Op.Confirm(success: ctx.Getter.Constrain(from: c.From, to: c.To)),
            onLine: static (ctx, c) => ctx.Op.Confirm(success: ctx.Getter.Constrain(line: c.Value)),
            onArc: static (ctx, c) => ctx.Op.Confirm(success: ctx.Getter.Constrain(arc: c.Value)),
            onCircle: static (ctx, c) => ctx.Op.Confirm(success: ctx.Getter.Constrain(circle: c.Value)),
            onPlane: static (ctx, c) => ctx.Op.Confirm(success: ctx.Getter.Constrain(plane: c.Value, allowElevator: c.AllowElevator)),
            onSphere: static (ctx, c) => ctx.Op.Confirm(success: ctx.Getter.Constrain(sphere: c.Value)),
            onCylinder: static (ctx, c) => ctx.Op.Confirm(success: ctx.Getter.Constrain(cylinder: c.Value)),
            onCurve: static (ctx, c) => ctx.Op.Confirm(success: ctx.Getter.Constrain(curve: c.Value, allowPickingPointOffObject: c.AllowPickingOff)),
            onSurface: static (ctx, c) => ctx.Op.Confirm(success: ctx.Getter.Constrain(surface: c.Value, allowPickingPointOffObject: c.AllowPickingOff)),
            onBrep: static (ctx, c) => ctx.Op.Confirm(success: ctx.Getter.Constrain(
                brep: c.Value, wireDensity: c.WireDensity, faceIndex: c.FaceIndex, allowPickingPointOffObject: c.AllowPickingOff)),
            onMesh: static (ctx, c) => ctx.Op.Confirm(success: ctx.Getter.Constrain(mesh: c.Value, allowPickingPointOffObject: c.AllowPickingOff)),
            onConstructionPlane: static (ctx, c) => ctx.Op.Confirm(success: ctx.Getter.ConstrainToConstructionPlane(throughBasePoint: c.ThroughBasePoint)),
            onTargetPlane: static (ctx, _) => Fin.Succ(value: Op.Side(ctx.Getter.ConstrainToTargetPlane)),
            onCPlaneIntersection: static (ctx, c) => ctx.Op.Confirm(success: ctx.Getter.ConstrainToVirtualCPlaneIntersection(plane: c.Value)));
}
```

## [05]-[INPUT_MATRIX]

- Owner: `InputKind` `[SmartEnum<int>]` — one row per acquisition modality, each carrying the `Custom` column (multi-step getter versus one-shot modal), the `Grants` slot-admission columns, and its `Run` delegate: construct or invoke the native route, project the typed payload, and answer the terminal; `Acquisition` — the ONE entry proving `SessionNeed.Acquire` and driving the row.
- Entry: `Acquisition.Get(DocumentSession, Acquire) : Fin<AcquiredReceipt>` — admit the request against the row's slots, then run the row inside one `Demand` window under `SessionNeed.Acquire` and seal the receipt; the option loop is internalized — a `GetResult.Option` terminal folds the selection through the bound `OptionLease` and re-runs the same getter, the lease releases its carriers on every exit, its cycle count is bounded by `AcceptPolicy.OptionBudget`, and a consumer never writes the while-option idiom.
- Law: projection is raw-discriminated — every custom row receives the terminal `GetResult` and projects only the value kinds its accepts admit (the point row also answers `Number` and `String` crossings under their grants), so a host result the row cannot type is a fault carrying the raw discriminant, never a garbage read off unrelated getter state.
- Law: the matrix is the whole modality roster — custom getter rows (`Point`, `Objects`, `Number`, `Count`, `Text`, `Xform`) construct their `Rhino.Input.Custom` getter, apply the shared configuration fold (prompt, prompt default, typed default, accepts, options), and drive; modal rows bind their `RhinoGet` member one-shot — and a request naming options on a modal row, bounds on an unbounded row, or a transform row without its calculator is refused typed at admission.
- Law: scripted acquisition is the same matrix — the host getters read script tokens under the scripted lane and `RhinoGet` consumes command-line input directly, so no parallel scripted decode path exists; the scripted lane still proves the `Acquire` grant, whose row already admits the scripted mode.
- Law: numeric bounds are getter facts — the `Number` and `Count` rows push `SetLowerLimit`/`SetUpperLimit` onto the native getter so the host enforces the band during typing, and the projected value re-checks nothing.
- Law: the object row hands its picks to the selection owner at the terminal — `Objects()` projects through `Picks.Capture` inside the getter window, so no `ObjRef` survives the row.
- Law: units-aware text lands scalar through `ScalarText` — `StringParser.ParseLengthExpession(string, StringParserSettings, UnitSystem, out double) : int` answers characters consumed (the host misspells `Expession`; the long-form angle sibling `ParseAngleExpession` shares the misspelling), `ParseAngleExpressionDegrees`/`ParseAngleExpressionRadians(string, out double) : bool` answer angles, presets ride `StringParserSettings.DefaultParseSettings`, and `StringParser.ParseNumber` plus the `LengthValue.Create`/`IsUnset` round trip stay unit-text scope on the same verified family — a partially consumed expression is a typed refusal, so the `Text` row's raw string admits to a unit-carried scalar with no kernel re-parse.
- Growth: a new host getter modality is one row with its `Run` delegate and slot admission; `Acquisition.Get`, the receipt, and the command page's `Acquire` stage read it with zero new surface.

```csharp
// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class InputKind {
    public static readonly InputKind Point = new(key: 0, custom: true, grants: SlotGrants.Custom with { Point = true }, run: InputRuns.PointRun);
    public static readonly InputKind Objects = new(key: 1, custom: true, grants: SlotGrants.Custom with { Objects = true }, run: InputRuns.ObjectsRun);
    public static readonly InputKind Number = new(key: 2, custom: true, grants: SlotGrants.Custom with { Bounds = true }, run: InputRuns.NumberRun);
    public static readonly InputKind Count = new(key: 3, custom: true, grants: SlotGrants.Custom with { Bounds = true }, run: InputRuns.CountRun);
    public static readonly InputKind Text = new(key: 4, custom: true, grants: SlotGrants.Custom, run: InputRuns.TextRun);
    public static readonly InputKind Xform = new(key: 5, custom: true, grants: SlotGrants.Custom with { Calculate = true }, run: InputRuns.XformRun);
    public static readonly InputKind Toggle = new(key: 6, custom: false, grants: SlotGrants.Modal with { ToggleNames = true }, run: InputRuns.ToggleRun);
    public static readonly InputKind Paint = new(key: 7, custom: false, grants: SlotGrants.Modal, run: InputRuns.PaintRun);
    public static readonly InputKind Distance = new(key: 8, custom: false, grants: SlotGrants.Modal with { TypedDefault = true }, run: InputRuns.DistanceRun);
    public static readonly InputKind Segment = new(key: 9, custom: false, grants: SlotGrants.Modal, run: InputRuns.SegmentRun);
    public static readonly InputKind Chain = new(key: 10, custom: false, grants: SlotGrants.Modal, run: InputRuns.ChainRun);
    public static readonly InputKind ArcShape = new(key: 11, custom: false, grants: SlotGrants.Modal, run: InputRuns.ArcRun);
    public static readonly InputKind CircleShape = new(key: 12, custom: false, grants: SlotGrants.Modal, run: InputRuns.CircleRun);
    public static readonly InputKind PlaneShape = new(key: 13, custom: false, grants: SlotGrants.Modal, run: InputRuns.PlaneRun);
    public static readonly InputKind RectangleShape = new(key: 14, custom: false, grants: SlotGrants.Modal, run: InputRuns.RectangleRun);
    public static readonly InputKind BoxShape = new(key: 15, custom: false, grants: SlotGrants.Modal, run: InputRuns.BoxRun);
    public static readonly InputKind ViewChoice = new(key: 16, custom: false, grants: SlotGrants.Modal, run: InputRuns.ViewRun);
    public static readonly InputKind ViewportChoice = new(key: 17, custom: false, grants: SlotGrants.Modal, run: InputRuns.ViewportsRun);

    public bool Custom { get; }

    public SlotGrants Grants { get; }

    [UseDelegateFromConstructor]
    internal partial Fin<AcquiredReceipt> Run(AcquireContext context);
}

// --- [MODELS] -----------------------------------------------------------------------------
internal readonly record struct AcquireContext(Acquire Request, Op Op);

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class Acquisition {
    public static Fin<AcquiredReceipt> Get(DocumentSession session, Acquire request) {
        Op op = Op.Of();
        return from active in Optional(request).ToFin(Fail: op.InvalidInput())
               from prompt in op.AcceptText(value: active.Prompt)
               from _ in Admission(request: active, grants: active.Kind.Grants, op: op)
               from receipt in session.Demand(
                   use: _ => active.Kind.Run(context: new AcquireContext(Request: active, Op: op)),
                   key: op,
                   needs: [SessionNeed.Acquire])
               select receipt;
    }

    private static Fin<Unit> Admission(Acquire request, SlotGrants grants, Op op) =>
        Seq((request.Options.IsSome, grants.Options),
            (request.Point.IsSome, grants.Point),
            (request.Objects.IsSome, grants.Objects),
            (request.Bounds.IsSome, grants.Bounds),
            (request.ToggleNames.IsSome, grants.ToggleNames),
            (request.Calculate.IsSome, grants.Calculate),
            (request.Default.IsSome, grants.TypedDefault),
            (request.PromptDefault.IsSome, grants.PromptDefault))
            .TraverseM(slot => guard(!slot.Item1 || slot.Item2, op.InvalidInput()).ToFin()).As()
            .Map(static _ => unit);
}

internal static class InputRuns {
    internal static Fin<AcquiredReceipt> PointRun(AcquireContext ctx) {
        PointPolicy policy = ctx.Request.Point.IfNone(PointPolicy.Free);
        return Driven(ctx: ctx, create: static () => new GetPoint(), prepare: (getter, op) =>
            from _ in policy.Constraints.TraverseM(constraint => constraint.Apply(getter: getter, key: op)).As()
            select Op.Side(() => {
                _ = policy.Cursor.Iter(cursor => getter.SetCursor(cursor));
                _ = policy.ObjectSnapCursors.Iter(enabled => getter.EnableObjectSnapCursors(enable: enabled));
                _ = policy.PermitOrthoSnap.Iter(enabled => getter.PermitOrthoSnap(permit: enabled));
                _ = policy.PermitObjectSnap.Iter(enabled => getter.PermitObjectSnap(permit: enabled));
                _ = policy.DrawColor.Iter(color => getter.DynamicDrawColor = color);
                _ = policy.CurveSnapTangent.Iter(bar => getter.EnableCurveSnapTangentBar(drawTangentBarAtSnapPoint: bar.Enabled, drawEndPoints: bar.Ends));
                _ = policy.CurveSnapPerpendicular.Iter(bar => getter.EnableCurveSnapPerpBar(drawPerpBarAtSnapPoint: bar.Enabled, drawEndPoints: bar.Ends));
                _ = policy.CurveSnapArrow.Iter(bar => getter.EnableCurveSnapArrow(drawDirectionArrowAtSnapPoint: bar.Enabled, reverseArrow: bar.Reverse));
                getter.PermitConstraintOptions(policy.ConstraintOptions);
                getter.PermitFromOption(policy.FromOption);
                getter.PermitTabMode(policy.TabMode);
                getter.PermitElevatorMode(policy.ElevatorMode);
                getter.EnableSnapToCurves(enable: policy.SnapToCurves);
                getter.EnableNoRedrawOnExit(policy.NoRedrawOnExit);
                _ = Op.SideWhen(!policy.SnapPoints.IsEmpty, () => getter.AddSnapPoints(points: [.. policy.SnapPoints]));
                _ = Op.SideWhen(!policy.ConstructionPoints.IsEmpty, () => getter.AddConstructionPoints(points: [.. policy.ConstructionPoints]));
                _ = policy.Base.Iter(anchor => {
                    getter.SetBasePoint(anchor.Anchor, showDistanceInStatusBar: anchor.ShowDistance);
                    _ = Op.SideWhen(anchor.DrawLine, () => getter.DrawLineFromPoint(anchor.Anchor, showDistanceInStatusBar: anchor.ShowDistance));
                });
                _ = policy.DistanceFromBase.Iter(distance => getter.ConstrainDistanceFromBasePoint(distance: distance));
                _ = policy.Feedback.Iter(feedback => {
                    _ = feedback.MouseMove.Iter(sink => getter.MouseMove += (_, args) => sink(obj: args));
                    _ = feedback.MouseDown.Iter(sink => getter.MouseDown += (_, args) => sink(obj: args));
                    _ = feedback.DynamicDraw.Iter(sink => getter.DynamicDraw += (_, args) => sink(obj: args));
                    _ = feedback.PostDraw.Iter(sink => {
                        getter.FullFrameRedrawDuringGet = true;
                        getter.PostDrawObjects += (_, args) => sink(obj: args);
                    });
                });
            }),
            receive: getter => getter.Get(onMouseUp: policy.OnMouseUp, get2DPoint: policy.TwoDimensional),
            project: static (getter, raw, op) => raw switch {
                GetResult.Point => Fin.Succ<Acquired>(value: new Acquired.Point(
                    Value: getter.Point(),
                    Evidence: new PointEvidence(
                        ViewportId: Optional(getter.View()).Map(static view => view.ActiveViewportID),
                        Osnap: getter.OsnapEventType,
                        BasePoint: getter.TryGetBasePoint(basePoint: out Point3d anchor) ? Some(anchor) : Option<Point3d>.None))),
                GetResult.Number => Fin.Succ<Acquired>(value: new Acquired.Number(Value: getter.Number())),
                GetResult.String => Fin.Succ<Acquired>(value: new Acquired.Text(Value: getter.StringResult())),
                var other => Fin.Fail<Acquired>(error: op.InvalidResult(detail: other.ToString())),
            });
    }

    internal static Fin<AcquiredReceipt> ObjectsRun(AcquireContext ctx) {
        ObjectPolicy policy = ctx.Request.Objects.IfNone(ObjectPolicy.One);
        return Driven(ctx: ctx, create: static () => new GetObject(), prepare: (getter, _) =>
            Fin.Succ(value: Op.Side(() => {
                getter.GeometryFilter = policy.Types;
                _ = policy.Custom.Iter(filter => getter.SetCustomGeometryFilter(filter));
                getter.EnablePreSelect(enable: policy.PreSelect.Enabled, ignoreUnacceptablePreselectedObjects: policy.PreSelect.IgnoreUnacceptable);
                getter.EnablePostSelect(enable: policy.PostSelect);
                getter.EnableSelPrevious(enable: policy.SelPrevious);
                getter.EnableHighlight(enable: policy.Highlight);
                getter.EnableIgnoreGrips(enable: policy.IgnoreGrips);
                getter.EnablePressEnterWhenDonePrompt(enable: policy.PressEnterWhenDonePrompt);
            })),
            receive: getter => getter.GetMultiple(minimumNumber: policy.Range.Minimum, maximumNumber: policy.Range.Maximum),
            project: static (getter, raw, op) => raw is GetResult.Object
                ? toSeq(getter.Objects())
                    .TraverseM(reference => Picks.Capture(reference: reference, key: op)).As()
                    .Map(static captures => (Acquired)new Acquired.Objects(Picks: captures))
                : Fin.Fail<Acquired>(error: op.InvalidResult(detail: raw.ToString())));
    }

    internal static Fin<AcquiredReceipt> NumberRun(AcquireContext ctx) =>
        Driven(ctx: ctx, create: static () => new GetNumber(), prepare: (getter, _) =>
            Fin.Succ(value: Op.Side(() => ctx.Request.Bounds.Iter(bounds => {
                getter.SetLowerLimit(lowerLimit: bounds.Lower, strictlyGreaterThan: false);
                getter.SetUpperLimit(upperLimit: bounds.Upper, strictlyLessThan: false);
            }))),
            receive: static getter => getter.Get(),
            project: static (getter, raw, op) => raw is GetResult.Number
                ? Fin.Succ<Acquired>(value: new Acquired.Number(Value: getter.Number()))
                : Fin.Fail<Acquired>(error: op.InvalidResult(detail: raw.ToString())));

    internal static Fin<AcquiredReceipt> CountRun(AcquireContext ctx) =>
        Driven(ctx: ctx, create: static () => new GetInteger(), prepare: (getter, _) =>
            Fin.Succ(value: Op.Side(() => ctx.Request.Bounds.Iter(bounds => {
                getter.SetLowerLimit((int)bounds.Lower, false);
                getter.SetUpperLimit((int)bounds.Upper, false);
            }))),
            receive: static getter => getter.Get(),
            project: static (getter, raw, op) => raw is GetResult.Number
                ? Fin.Succ<Acquired>(value: new Acquired.Count(Value: (int)getter.Number()))
                : Fin.Fail<Acquired>(error: op.InvalidResult(detail: raw.ToString())));

    internal static Fin<AcquiredReceipt> TextRun(AcquireContext ctx) =>
        Driven(ctx: ctx, create: static () => new GetString(), prepare: static (_, _) => Fin.Succ(value: unit),
            receive: static getter => getter.Get(),
            project: static (getter, raw, op) => raw is GetResult.String
                ? Fin.Succ<Acquired>(value: new Acquired.Text(Value: getter.StringResult()))
                : Fin.Fail<Acquired>(error: op.InvalidResult(detail: raw.ToString())));

    internal static Fin<AcquiredReceipt> XformRun(AcquireContext ctx) =>
        ctx.Request.Calculate.ToFin(Fail: ctx.Op.InvalidInput()).Bind(calculate =>
            Driven(ctx: ctx, create: () => new TransformGetter(calculate: calculate), prepare: static (_, _) => Fin.Succ(value: unit),
                receive: static getter => getter.GetXform(),
                project: static (getter, _, op) => op.Catch(() => Fin.Succ<Acquired>(value: new Acquired.Xform(
                    Value: getter.CalculateTransform(viewport: getter.View().ActiveViewport, point: getter.Point()))))));

    internal static Fin<AcquiredReceipt> ToggleRun(AcquireContext ctx) =>
        Shape(ctx: ctx, run: () => {
            bool value = false;
            (string off, string on) = ctx.Request.ToggleNames.IfNone(("Off", "On"));
            Result native = RhinoGet.GetBool(
                prompt: ctx.Request.Prompt, acceptNothing: ctx.Request.Accept.Nothing,
                offPrompt: off, onPrompt: on, boolValue: ref value);
            return (native, (Acquired)new Acquired.Toggle(Value: value));
        });

    internal static Fin<AcquiredReceipt> PaintRun(AcquireContext ctx) =>
        Shape(ctx: ctx, run: () => {
            System.Drawing.Color color = System.Drawing.Color.Empty;
            Result native = RhinoGet.GetColor(prompt: ctx.Request.Prompt, acceptNothing: ctx.Request.Accept.Nothing, color: ref color);
            return (native, (Acquired)new Acquired.Paint(Value: color));
        });

    internal static Fin<AcquiredReceipt> DistanceRun(AcquireContext ctx) =>
        Shape(ctx: ctx, run: () => {
            double seed = ctx.Request.Default.Bind(static row => row is InputDefault.NumberValue number
                ? Some(number.Value) : Option<double>.None).IfNone(noneValue: 0.0);
            Result native = RhinoGet.GetDistance(commandPrompt: ctx.Request.Prompt, defaultDistance: seed, distance: out double value);
            return (native, (Acquired)new Acquired.Distance(Value: value));
        });

    internal static Fin<AcquiredReceipt> SegmentRun(AcquireContext ctx) =>
        Shape(ctx: ctx, run: static () => (RhinoGet.GetLine(line: out Line value), (Acquired)new Acquired.Segment(Value: value)));

    internal static Fin<AcquiredReceipt> ChainRun(AcquireContext ctx) =>
        Shape(ctx: ctx, run: static () => (RhinoGet.GetPolyline(polyline: out Polyline value), (Acquired)new Acquired.Chain(Value: value)));

    internal static Fin<AcquiredReceipt> ArcRun(AcquireContext ctx) =>
        Shape(ctx: ctx, run: static () => (RhinoGet.GetArc(arc: out Arc value), (Acquired)new Acquired.ArcShape(Value: value)));

    internal static Fin<AcquiredReceipt> CircleRun(AcquireContext ctx) =>
        Shape(ctx: ctx, run: static () => (RhinoGet.GetCircle(circle: out Circle value), (Acquired)new Acquired.CircleShape(Value: value)));

    internal static Fin<AcquiredReceipt> PlaneRun(AcquireContext ctx) =>
        Shape(ctx: ctx, run: static () => (RhinoGet.GetPlane(plane: out Plane value), (Acquired)new Acquired.PlaneShape(Value: value)));

    internal static Fin<AcquiredReceipt> RectangleRun(AcquireContext ctx) =>
        Shape(ctx: ctx, run: static () => (RhinoGet.GetRectangle(corners: out Point3d[] corners), (Acquired)new Acquired.RectangleShape(Corners: toArr(corners))));

    internal static Fin<AcquiredReceipt> BoxRun(AcquireContext ctx) =>
        Shape(ctx: ctx, run: static () => (RhinoGet.GetBox(box: out Box value), (Acquired)new Acquired.BoxShape(Value: value)));

    internal static Fin<AcquiredReceipt> ViewRun(AcquireContext ctx) =>
        Shape(ctx: ctx, run: () => (RhinoGet.GetView(commandPrompt: ctx.Request.Prompt, view: out RhinoView view),
            (Acquired)new Acquired.ViewChoice(ViewSerial: view.RuntimeSerialNumber)));

    internal static Fin<AcquiredReceipt> ViewportsRun(AcquireContext ctx) =>
        Shape(ctx: ctx, run: () => (RhinoGet.GetViewports(commandPrompt: ctx.Request.Prompt, viewports: out RhinoViewport[] viewports),
            (Acquired)new Acquired.ViewportChoice(ViewportIds: toSeq(viewports).Map(static viewport => viewport.Id))));

    private static Fin<AcquiredReceipt> Driven<TGetter>(
        AcquireContext ctx, Func<TGetter> create, Func<TGetter, Op, Fin<Unit>> prepare,
        Func<TGetter, GetResult> receive, Func<TGetter, GetResult, Op, Fin<Acquired>> project)
        where TGetter : GetBaseClass =>
        ctx.Op.Catch(() => {
            using TGetter getter = create();
            getter.SetCommandPrompt(prompt: ctx.Request.Prompt);
            _ = ctx.Request.PromptDefault.Iter(text => getter.SetCommandPromptDefault(defaultValue: text));
            _ = ctx.Request.Default.Iter(row => row.Apply(getter: getter));
            _ = ctx.Request.Accept.Apply(getter: getter);
            return ctx.Request.Options
                .Match(
                    Some: set => set.Bind(getter: getter, key: ctx.Op).Map(Some),
                    None: static () => Fin.Succ(value: Option<OptionLease>.None))
                .Bind(lease => {
                    Fin<AcquiredReceipt> outcome =
                        from prepared in prepare(arg1: getter, arg2: ctx.Op)
                        from receipt in Loop(getter: getter, lease: lease, receive: receive, project: project,
                            chosen: Seq<OptionChoice>(), remaining: ctx.Request.Accept.OptionBudget, op: ctx.Op)
                        select receipt;
                    _ = lease.Iter(static held => held.Dispose());
                    return outcome;
                });
        });

    private static Fin<AcquiredReceipt> Loop<TGetter>(
        TGetter getter, Option<OptionLease> lease, Func<TGetter, GetResult> receive,
        Func<TGetter, GetResult, Op, Fin<Acquired>> project, Seq<OptionChoice> chosen, int remaining, Op op)
        where TGetter : GetBaseClass =>
        remaining <= 0
            ? Fin.Fail<AcquiredReceipt>(error: op.InvalidResult(detail: nameof(AcceptPolicy.OptionBudget)))
            : receive(arg: getter) switch {
                GetResult.Cancel => Sealed(terminal: new AcquireTerminal.Cancelled(), getter: getter, chosen: chosen),
                GetResult.Nothing => Sealed(terminal: new AcquireTerminal.Nothing(), getter: getter, chosen: chosen),
                GetResult.Undo => Sealed(terminal: new AcquireTerminal.Undone(), getter: getter, chosen: chosen),
                GetResult.ExitRhino => Sealed(terminal: new AcquireTerminal.Exit(), getter: getter, chosen: chosen),
                GetResult raw and (GetResult.NoResult or GetResult.Miss or GetResult.Timeout) =>
                    Fin.Fail<AcquiredReceipt>(error: op.InvalidResult(detail: raw.ToString())),
                GetResult.Option => lease.ToFin(Fail: op.InvalidResult())
                    .Bind(held => held.Selected(getter: getter, key: op))
                    .Bind(choice => Loop(getter: getter, lease: lease, receive: receive, project: project,
                        chosen: chosen.Add(value: choice), remaining: remaining - 1, op: op)),
                var raw => project(arg1: getter, arg2: raw, arg3: op).Bind(payload =>
                    Sealed(terminal: new AcquireTerminal.Value(Payload: payload), getter: getter, chosen: chosen)),
            };

    private static Fin<AcquiredReceipt> Sealed(AcquireTerminal terminal, GetBaseClass getter, Seq<OptionChoice> chosen) =>
        Fin.Succ(value: new AcquiredReceipt(Terminal: terminal, Options: chosen, GotDefault: getter.GotDefault()));

    private static Fin<AcquiredReceipt> Shape(AcquireContext ctx, Func<(Result Native, Acquired Payload)> run) =>
        ctx.Op.Catch(() => {
            (Result native, Acquired payload) = run();
            return native switch {
                Result.Success => Fin.Succ(value: Receipt(terminal: new AcquireTerminal.Value(Payload: payload))),
                Result.Cancel => Fin.Succ(value: Receipt(terminal: new AcquireTerminal.Cancelled())),
                Result.Nothing => Fin.Succ(value: Receipt(terminal: new AcquireTerminal.Nothing())),
                Result.ExitRhino => Fin.Succ(value: Receipt(terminal: new AcquireTerminal.Exit())),
                var other => Fin.Fail<AcquiredReceipt>(error: ctx.Op.InvalidResult(detail: other.ToString())),
            };
        });

    private static AcquiredReceipt Receipt(AcquireTerminal terminal) =>
        new(Terminal: terminal, Options: Seq<OptionChoice>(), GotDefault: false);
}

public static class ScalarText {
    public static Fin<double> Length(string expression, UnitSystem output, Option<StringParserSettings> settings = default, Op? key = null) {
        Op op = key.OrDefault();
        return from text in op.AcceptText(value: expression)
               from parsed in op.Catch(() => StringParser.ParseLengthExpession(
                       expression: text,
                       parse_settings_in: settings.IfNone(StringParserSettings.DefaultParseSettings),
                       output_unit_system: output,
                       value_out: out double value) == text.Length
                   ? Fin.Succ(value: value)
                   : Fin.Fail<double>(error: op.InvalidInput()))
               select parsed;
    }

    public static Fin<double> Angle(string expression, AngleUnitSystem output, Op? key = null) {
        Op op = key.OrDefault();
        return from text in op.AcceptText(value: expression)
               from parsed in op.Catch(() => output switch {
                   AngleUnitSystem.Radians => StringParser.ParseAngleExpressionRadians(expression: text, angle_radians: out double radians)
                       ? Fin.Succ(value: radians)
                       : Fin.Fail<double>(error: op.InvalidInput()),
                   AngleUnitSystem.Degrees => StringParser.ParseAngleExpressionDegrees(expression: text, angle_degrees: out double degrees)
                       ? Fin.Succ(value: degrees)
                       : Fin.Fail<double>(error: op.InvalidInput()),
                   _ => Fin.Fail<double>(error: op.Unsupported(geometryType: typeof(AngleUnitSystem), outputType: typeof(double))),
               })
               select parsed;
    }
}

// --- [BOUNDARIES] -------------------------------------------------------------------------
internal sealed class TransformGetter(Func<RhinoViewport, Point3d, Transform> calculate) : GetTransform {
    public override Transform CalculateTransform(RhinoViewport viewport, Point3d point) =>
        calculate(arg1: viewport, arg2: point);
}
```

## [06]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]            | [OWNER]           | [FORM]                                                 | [ENTRY]                    |
| :-----: | :------------------- | :---------------- | :----------------------------------------------------- | :------------------------- |
|  [01]   | payload vocabulary   | `Acquired`        | one union, typed per-row evidence                      | `AcquiredReceipt.Payload`  |
|  [02]   | outcome discriminant | `AcquireTerminal` | value / cancelled / nothing / exit                     | `AcquiredReceipt.Terminal` |
|  [03]   | request algebra      | `Acquire`         | one record, per-row slot admission                     | `Acquisition.Get`          |
|  [04]   | point constraints    | `PointConstraint` | one union over the native constraint family            | `Apply(getter, key)`       |
|  [05]   | modality matrix      | `InputKind`       | rows with `Custom`/`Grants` columns and `Run` delegate | `Run(context)`             |
|  [06]   | acquisition entry    | `Acquisition`     | `Acquire` demand, internalized option loop             | `Get(session, request)`    |
