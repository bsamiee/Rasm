# [RASM_RHINO_ACQUISITION]

`Acquisition.Get` interprets one admitted `Acquire` value inside a document acquire grant. Custom getters, options, point callbacks, modal routes, and native references remain scoped to that window; egress is one detached `AcquiredReceipt`.

## [01]-[PAYLOAD]

`Acquired` closes interactive, screen-space, scalar, object, geometry, view, transform, and file payloads. `AcquireTerminal` preserves every non-fault control terminal, including native timeout.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Acquired {
    private Acquired() { }
    public sealed record Point(Point3d Value, PointEvidence Evidence) : Acquired;
    public sealed record ScreenPoint(System.Drawing.Point Value) : Acquired;
    public sealed record Objects(Seq<PickCapture> Picks) : Acquired;
    public sealed record Number(double Value) : Acquired;
    public sealed record Count(int Value) : Acquired;
    public sealed record Text(string Value) : Acquired;
    public sealed record Toggle(bool Value) : Acquired;
    public sealed record Paint(System.Drawing.Color Value) : Acquired;
    public sealed record Distance(double Value) : Acquired;
    public sealed record Angle(double Radians) : Acquired;
    public sealed record Segment(Line Value) : Acquired;
    public sealed record Chain(Polyline Value) : Acquired;
    public sealed record ArcShape(Arc Value) : Acquired;
    public sealed record CircleShape(Circle Value) : Acquired;
    public sealed record PlaneShape(Plane Value) : Acquired;
    public sealed record RectangleShape(Arr<Point3d> Corners) : Acquired;
    public sealed record BoxShape(Box Value) : Acquired;
    public sealed record Xform(Transform Value) : Acquired;
    public sealed record FileName(string Value) : Acquired;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AcquireTerminal {
    private AcquireTerminal() { }
    public sealed record Value(Acquired Payload) : AcquireTerminal;
    public sealed record Cancelled : AcquireTerminal;
    public sealed record Nothing : AcquireTerminal;
    public sealed record Undone : AcquireTerminal;
    public sealed record TimedOut : AcquireTerminal;
    public sealed record Exit : AcquireTerminal;
}

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record PointEvidence(
    Option<uint> ViewSerial,
    Option<int> OsnapCode,
    Option<Point3d> BasePoint,
    Seq<Point3d> SnapPoints,
    Seq<Point3d> ConstructionPoints);

public sealed record AcquiredReceipt(
    AcquireTerminal Terminal,
    Seq<OptionChoice> Options,
    bool GotDefault) : IDetachedDocumentResult {
    public Option<Acquired> Payload => Terminal is AcquireTerminal.Value value ? Some(value.Payload) : None;
}
```

## [02]-[ACCEPTANCE]

`AcceptGate` rows carry every parameterless native accept call beside its result terminal, so acceptance grows by one row, never a new case. `AcceptRule` closes the gated, numeric, transparency, and wait modalities; each rule family derives its one-row-per-slot admission from `SlotKey`, the case identity a parameterized case overrides with its row value. Wait duration and option-cycle bounds are admitted once; no getter receives a raw flag bag.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
public interface ISlotted {
    object SlotKey { get; }
}

[SmartEnum<int>]
public sealed partial class AcceptGate {
    public static readonly AcceptGate Nothing = new(key: 0, terminal: None, enable: static target => target.AcceptNothing(enable: true));
    public static readonly AcceptGate Undo = new(key: 1, terminal: None, enable: static target => target.AcceptUndo(enable: true));
    public static readonly AcceptGate Enter = new(key: 2, terminal: None, enable: static target => target.AcceptEnterWhenDone(enable: true));
    public static readonly AcceptGate Point = new(key: 3, terminal: Some(GetResult.Point), enable: static target => target.AcceptPoint(enable: true));
    public static readonly AcceptGate Color = new(key: 4, terminal: Some(GetResult.Color), enable: static target => target.AcceptColor(enable: true));
    public static readonly AcceptGate Text = new(key: 5, terminal: Some(GetResult.String), enable: static target => target.AcceptString(enable: true));

    public Option<GetResult> Terminal { get; }

    [UseDelegateFromConstructor]
    internal partial void Enable(GetBaseClass getter);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AcceptRule : ISlotted {
    private AcceptRule() { }
    public sealed record Allowed(AcceptGate Gate) : AcceptRule { public override object SlotKey => Gate; }
    public sealed record Number(bool Zero) : AcceptRule;
    public sealed record Transparent(bool Enabled) : AcceptRule;
    public sealed record WaitFor(TimeSpan Duration) : AcceptRule;

    public virtual object SlotKey => GetType();

    internal Option<GetResult> Terminal => this switch {
        Allowed row => row.Gate.Terminal,
        Number => Some(GetResult.Number),
        _ => None,
    };

    internal Fin<Unit> Apply(GetBaseClass getter, Op key) => key.Catch(() => {
        Switch(
            state: getter,
            allowed: static (target, rule) => { rule.Gate.Enable(target); return unit; },
            number: static (target, rule) => { target.AcceptNumber(enable: true, acceptZero: rule.Zero); return unit; },
            transparent: static (target, rule) => { target.EnableTransparentCommands(enable: rule.Enabled); return unit; },
            waitFor: static (target, rule) => {
                int milliseconds = checked((int)Math.Ceiling(rule.Duration.TotalMilliseconds));
                target.SetWaitDuration(milliseconds: milliseconds);
                return unit;
            });
        return Fin.Succ(unit);
    });
}

[ComplexValueObject]
public sealed partial class AcceptPlan {
    public Seq<AcceptRule> Rules { get; }
    public int OptionBudget { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Seq<AcceptRule> rules,
        ref int optionBudget) {
        bool waitInvalid = rules.Exists(static rule => rule is AcceptRule.WaitFor wait
            && (wait.Duration <= TimeSpan.Zero || wait.Duration.TotalMilliseconds > int.MaxValue));
        validationError = rules.Exists(static rule => rule is null || rule is AcceptRule.Allowed { Gate: null })
            || optionBudget < 0 || optionBudget > 4096
            || !rules.OnePer()
            || waitInvalid
            ? new ValidationError(message: "accept plan is invalid")
            : validationError;
    }

    internal bool AcceptsNothing => Rules.Exists(static rule => rule is AcceptRule.Allowed allowed && allowed.Gate == AcceptGate.Nothing);

    internal Fin<Unit> Apply(GetBaseClass getter, Op key) =>
        Rules.TraverseM(rule => rule.Apply(getter: getter, key: key)).As().Map(static _ => unit);
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class Slots {
    public static bool OnePer<T>(this Seq<T> rules) where T : ISlotted =>
        rules.Map(static rule => rule.SlotKey).Distinct().Count == rules.Count;
}
```

## [03]-[POINT_ALGEBRA]

`PointConstraint` closes the native constraint family. `PointRule` parameterizes every independent point-getter setting as data — `PointGate` rows carry the boolean getter toggles and `SnapBarAxis` rows the curve-snap bars, so a new toggle is one row, never a new case — while `PointFeedback` carries rail-returning callbacks whose failures interrupt the native loop and surface after `Get` returns.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PointConstraint {
    private PointConstraint() { }
    public sealed record OnSegment(Point3d From, Point3d To) : PointConstraint;
    public sealed record OnLine(Line Value) : PointConstraint;
    public sealed record OnArc(Arc Value) : PointConstraint;
    public sealed record OnCircle(Circle Value) : PointConstraint;
    public sealed record OnPlane(Plane Value, bool AllowElevator) : PointConstraint;
    public sealed record OnSphere(Sphere Value) : PointConstraint;
    public sealed record OnCylinder(Cylinder Value) : PointConstraint;
    public sealed record OnCurve(Curve Value, bool AllowPickingOff) : PointConstraint;
    public sealed record OnSurface(Surface Value, bool AllowPickingOff) : PointConstraint;
    public sealed record OnBrep(Brep Value, int WireDensity, int FaceIndex, bool AllowPickingOff) : PointConstraint;
    public sealed record OnMesh(Mesh Value, bool AllowPickingOff) : PointConstraint;
    public sealed record OnConstructionPlane(bool ThroughBasePoint) : PointConstraint;
    public sealed record OnTargetPlane : PointConstraint;
    public sealed record OnCPlaneIntersection(Plane Value) : PointConstraint;

    internal Fin<Unit> Admit(Op key) => key.Catch(() => AdmitGeometry(key, this switch {
            OnSegment row => row.From.IsValid && row.To.IsValid,
            OnLine row => row.Value.IsValid,
            OnArc row => row.Value.IsValid,
            OnCircle row => row.Value.IsValid,
            OnPlane row => row.Value.IsValid,
            OnSphere row => row.Value.IsValid,
            OnCylinder row => row.Value.IsValid,
            OnCurve row => row.Value is { } value && value.IsValidWithLog(out _),
            OnSurface row => row.Value is { } value && value.IsValidWithLog(out _),
            OnMesh row => row.Value is { } value && value.IsValidWithLog(out _),
            OnBrep row => row.Value is { } value
                && value.IsValidWithLog(out _)
                && row.WireDensity >= 0
                && row.FaceIndex >= -1,
            OnCPlaneIntersection row => row.Value.IsValid,
            _ => true,
        }));

    internal static Fin<Unit> AdmitGeometry(Op key, params ReadOnlySpan<bool> validity) =>
        guard(flag: validity.IndexOf(false) < 0, False: key.InvalidInput()).ToFin();

    internal Fin<Unit> Apply(GetPoint getter, Op key) => key.Catch(() => Switch(
        state: (Getter: getter, Op: key),
        onSegment: static (held, rule) => held.Op.Confirm(held.Getter.Constrain(rule.From, rule.To)),
        onLine: static (held, rule) => held.Op.Confirm(held.Getter.Constrain(rule.Value)),
        onArc: static (held, rule) => held.Op.Confirm(held.Getter.Constrain(rule.Value)),
        onCircle: static (held, rule) => held.Op.Confirm(held.Getter.Constrain(rule.Value)),
        onPlane: static (held, rule) => held.Op.Confirm(held.Getter.Constrain(rule.Value, rule.AllowElevator)),
        onSphere: static (held, rule) => held.Op.Confirm(held.Getter.Constrain(rule.Value)),
        onCylinder: static (held, rule) => held.Op.Confirm(held.Getter.Constrain(rule.Value)),
        onCurve: static (held, rule) => held.Op.Confirm(held.Getter.Constrain(rule.Value, rule.AllowPickingOff)),
        onSurface: static (held, rule) => held.Op.Confirm(held.Getter.Constrain(rule.Value, rule.AllowPickingOff)),
        onBrep: static (held, rule) => held.Op.Confirm(held.Getter.Constrain(
            rule.Value, rule.WireDensity, rule.FaceIndex, rule.AllowPickingOff)),
        onMesh: static (held, rule) => held.Op.Confirm(held.Getter.Constrain(rule.Value, rule.AllowPickingOff)),
        onConstructionPlane: static (held, rule) => held.Op.Confirm(
            held.Getter.ConstrainToConstructionPlane(rule.ThroughBasePoint)),
        onTargetPlane: static (held, _) => held.Op.Catch(() => {
            held.Getter.ConstrainToTargetPlane();
            return Fin.Succ(unit);
        }),
        onCPlaneIntersection: static (held, rule) => held.Op.Confirm(
            held.Getter.ConstrainToVirtualCPlaneIntersection(rule.Value))));
}

[SmartEnum<int>]
public sealed partial class PointGate {
    public static readonly PointGate ObjectSnapCursor = new(key: 0, set: static (getter, on) => getter.EnableObjectSnapCursors(on));
    public static readonly PointGate Ortho = new(key: 1, set: static (getter, on) => getter.PermitOrthoSnap(on));
    public static readonly PointGate ObjectSnap = new(key: 2, set: static (getter, on) => getter.PermitObjectSnap(on));
    public static readonly PointGate ConstraintOptions = new(key: 3, set: static (getter, on) => getter.PermitConstraintOptions(on));
    public static readonly PointGate FromOption = new(key: 4, set: static (getter, on) => getter.PermitFromOption(on));
    public static readonly PointGate TabMode = new(key: 5, set: static (getter, on) => getter.PermitTabMode(on));
    public static readonly PointGate Curves = new(key: 6, set: static (getter, on) => getter.EnableSnapToCurves(on));
    public static readonly PointGate ExitRedraw = new(key: 7, set: static (getter, on) => getter.EnableNoRedrawOnExit(on));
    public static readonly PointGate FullFrame = new(key: 8, set: static (getter, on) => getter.FullFrameRedrawDuringGet = on);

    [UseDelegateFromConstructor]
    internal partial void Set(GetPoint getter, bool enabled);
}

[SmartEnum<int>]
public sealed partial class SnapBarAxis {
    public static readonly SnapBarAxis Tangent = new(key: 0, set: static (getter, on, ends) => getter.EnableCurveSnapTangentBar(on, ends));
    public static readonly SnapBarAxis Perpendicular = new(key: 1, set: static (getter, on, ends) => getter.EnableCurveSnapPerpBar(on, ends));

    [UseDelegateFromConstructor]
    internal partial void Set(GetPoint getter, bool enabled, bool ends);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PointRule : ISlotted {
    private PointRule() { }
    public sealed record Constrained(PointConstraint Value) : PointRule;
    public sealed record Snaps(Seq<Point3d> Values) : PointRule;
    public sealed record ConstructionPoints(Seq<Point3d> Values) : PointRule;
    public sealed record BasedAt(Point3d Value, bool ShowDistance, bool DrawLine) : PointRule;
    public sealed record Radial(double Distance) : PointRule;
    public sealed record Cursor(global::Rhino.UI.CursorStyle Value) : PointRule;
    public sealed record ElevatorMode(int Mode) : PointRule;
    public sealed record Gated(PointGate Gate, bool Enabled) : PointRule { public override object SlotKey => Gate; }
    public sealed record SnapBar(SnapBarAxis Axis, bool Enabled, bool Ends) : PointRule { public override object SlotKey => Axis; }
    public sealed record DirectionArrow(bool Enabled, bool Reverse) : PointRule;
    public sealed record OnMouseUp : PointRule;

    public virtual object SlotKey => GetType();

    internal Fin<Unit> Admit(Op key) => this switch {
        Constrained row => Optional(row.Value).ToFin(Fail: key.InvalidInput()).Bind(value => value.Admit(key)),
        Snaps row => guard(!row.Values.IsEmpty, key.InvalidInput()).ToFin()
            .Bind(_ => PointConstraint.AdmitGeometry(key, [.. row.Values.Map(static point => point.IsValid)])),
        ConstructionPoints row => guard(!row.Values.IsEmpty, key.InvalidInput()).ToFin()
            .Bind(_ => PointConstraint.AdmitGeometry(key, [.. row.Values.Map(static point => point.IsValid)])),
        BasedAt row => PointConstraint.AdmitGeometry(key, row.Value.IsValid),
        Radial row => guard(double.IsFinite(row.Distance) && row.Distance >= 0.0, key.InvalidInput()).ToFin(),
        Cursor row => guard(Enum.IsDefined(row.Value), key.InvalidInput()).ToFin(),
        Gated row => guard(row.Gate is not null, key.InvalidInput()).ToFin(),
        SnapBar row => guard(row.Axis is not null, key.InvalidInput()).ToFin(),
        _ => Fin.Succ(unit),
    };

    internal Fin<Unit> Apply(GetPoint getter, Op key) => Switch(
        state: (Getter: getter, Op: key),
        constrained: static (held, rule) => rule.Value.Apply(held.Getter, held.Op),
        snaps: static (held, rule) => held.Op.Catch(() => Fin.Succ(ignore(
            held.Getter.AddSnapPoints(points: [.. rule.Values])))),
        constructionPoints: static (held, rule) => held.Op.Catch(() => Fin.Succ(ignore(
            held.Getter.AddConstructionPoints(points: [.. rule.Values])))),
        basedAt: static (held, rule) => held.Op.Catch(() => {
            held.Getter.SetBasePoint(rule.Value, rule.ShowDistance);
            held.Getter.EnableDrawLineFromPoint(rule.DrawLine);
            if (rule.DrawLine) held.Getter.DrawLineFromPoint(rule.Value, rule.ShowDistance);
            return Fin.Succ(unit);
        }),
        radial: static (held, rule) => held.Op.Catch(() => {
            held.Getter.ConstrainDistanceFromBasePoint(rule.Distance);
            return Fin.Succ(unit);
        }),
        cursor: static (held, rule) => held.Op.Catch(() => { held.Getter.SetCursor(rule.Value); return Fin.Succ(unit); }),
        elevatorMode: static (held, rule) => held.Op.Catch(() => { held.Getter.PermitElevatorMode(rule.Mode); return Fin.Succ(unit); }),
        gated: static (held, rule) => held.Op.Catch(() => { rule.Gate.Set(held.Getter, rule.Enabled); return Fin.Succ(unit); }),
        snapBar: static (held, rule) => held.Op.Catch(() => { rule.Axis.Set(held.Getter, rule.Enabled, rule.Ends); return Fin.Succ(unit); }),
        directionArrow: static (held, rule) => held.Op.Catch(() => { held.Getter.EnableCurveSnapArrow(rule.Enabled, rule.Reverse); return Fin.Succ(unit); }),
        onMouseUp: static (_, _) => Fin.Succ(unit));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PointFeedback {
    private PointFeedback() { }
    public sealed record MouseMove(Func<GetPointMouseEventArgs, Fin<Unit>> Sink) : PointFeedback;
    public sealed record MouseDown(Func<GetPointMouseEventArgs, Fin<Unit>> Sink) : PointFeedback;
    public sealed record DynamicDraw(Func<GetPointDrawEventArgs, Fin<Unit>> Sink) : PointFeedback;
    public sealed record PostDraw(Func<DrawEventArgs, Fin<Unit>> Sink) : PointFeedback;

    internal Fin<Unit> Admit(Op key) => Switch(
        mouseMove: row => guard(row.Sink is not null, key.InvalidInput()).ToFin(),
        mouseDown: row => guard(row.Sink is not null, key.InvalidInput()).ToFin(),
        dynamicDraw: row => guard(row.Sink is not null, key.InvalidInput()).ToFin(),
        postDraw: row => guard(row.Sink is not null, key.InvalidInput()).ToFin());
}

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record PointPlan {
    private PointPlan(Seq<PointRule> rules, Seq<PointFeedback> feedback) {
        Rules = rules;
        Feedback = feedback;
    }

    public Seq<PointRule> Rules { get; }
    public Seq<PointFeedback> Feedback { get; }
    public static PointPlan Free { get; } = new(rules: [], feedback: []);

    public static Fin<PointPlan> Of(Seq<PointFeedback> feedback, params ReadOnlySpan<PointRule> rules) {
        PointPlan plan = new(rules: toSeq(rules.ToArray()), feedback: feedback);
        return plan.Admit(Op.Of(name: nameof(PointPlan))).Map(_ => plan);
    }

    internal bool OnMouseUp => Rules.Exists(static rule => rule is PointRule.OnMouseUp);

    internal Fin<Unit> Admit(Op op) {
        Seq<PointRule> singleton = Rules.Filter(static rule => rule is not null && rule is not PointRule.Constrained);
        return guard(
            Rules.ForAll(static rule => rule is not null)
            && Feedback.ForAll(static row => row is not null)
            && singleton.OnePer(),
            op.InvalidInput()).ToFin()
            .Bind(_ => Rules.TraverseM(rule => rule.Admit(op)).As().Map(static _ => unit))
            .Bind(_ => Feedback.TraverseM(row => row.Admit(op)).As().Map(static _ => unit));
    }
}
```

## [04]-[REQUEST]

`PromptCase` generates the interactive value space over one `GetPoint`; multiple distinct terminal cases compose number, text, color, 3D point, and 2D point acquisition without getter-specific helper classes, and getter configuration derives from each case's `Terminal`. `Acquire.Of` admits each option, prompt default, typed default, and accept terminal against the selected `AcquireIntent`; no configured terminal can outrun its projector. `ObjectPlan`, `ModalInput`, and `AcquireIntent` close the remaining custom and one-shot routes; `ShapeAsk` rows carry the parameterless one-shot shape getters as data, so a new native shape is one row.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class TextMeaning {
    public static readonly TextMeaning Literal = new(key: 0, parse: static (text, _, _) =>
        Fin.Succ<Acquired>(new Acquired.Text(Value: text)));
    public static readonly TextMeaning Number = new(key: 1, parse: static (text, _, key) => key.Catch(() => {
        StringParserSettings output = StringParserSettings.ParseSettingsDoubleNumber;
        int consumed = StringParser.ParseNumber(
            text, 0, StringParserSettings.ParseSettingsDoubleNumber, ref output, out double value);
        return consumed == text.Length && double.IsFinite(value)
            ? Fin.Succ<Acquired>(new Acquired.Number(Value: value))
            : Fin.Fail<Acquired>(key.InvalidInput());
    }));
    public static readonly TextMeaning Length = new(key: 2, parse: static (text, document, key) => key.Catch(() => {
        using LengthValue parsed = LengthValue.Create(
            s: text, ps: StringParserSettings.DefaultParseSettings, parsedAll: out bool parsedAll);
        return parsedAll && !parsed.IsUnset()
            ? Fin.Succ<Acquired>(new Acquired.Distance(Value: parsed.Length(document.ModelUnitSystem)))
            : Fin.Fail<Acquired>(key.InvalidInput());
    }));
    public static readonly TextMeaning AngleDegrees = new(key: 3, parse: static (text, _, key) => key.Catch(() =>
        StringParser.ParseAngleExpressionDegrees(text, out double value)
            && value * Math.PI / 180.0 is var radians
            && double.IsFinite(radians)
            ? Fin.Succ<Acquired>(new Acquired.Angle(Radians: radians))
            : Fin.Fail<Acquired>(key.InvalidInput())));
    public static readonly TextMeaning AngleRadians = new(key: 4, parse: static (text, _, key) => key.Catch(() =>
        StringParser.ParseAngleExpressionRadians(text, out double value) && double.IsFinite(value)
            ? Fin.Succ<Acquired>(new Acquired.Angle(Radians: value))
            : Fin.Fail<Acquired>(key.InvalidInput())));

    [UseDelegateFromConstructor]
    public partial Fin<Acquired> Parse(string text, RhinoDoc document, Op key);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PromptCase {
    private PromptCase() { }
    public sealed record Point3 : PromptCase;
    public sealed record Point2 : PromptCase;
    public sealed record NumberValue(NumericBand<double> Band) : PromptCase;
    public sealed record CountValue(NumericBand<int> Band) : PromptCase;
    public sealed record TextValue(TextMeaning Meaning) : PromptCase;
    public sealed record PaintValue : PromptCase;

    public GetResult Terminal => Switch(
        point3: static _ => GetResult.Point,
        point2: static _ => GetResult.Point2d,
        numberValue: static _ => GetResult.Number,
        countValue: static _ => GetResult.Number,
        textValue: static _ => GetResult.String,
        paintValue: static _ => GetResult.Color);

    internal Fin<Unit> Admit(Op key) => guard(this is not TextValue { Meaning: null }, key.InvalidInput()).ToFin();

    internal bool Accepts(InputDefault value) => (this, value) switch {
        (Point3, InputDefault.PointValue) => true,
        (NumberValue rule, InputDefault.NumberValue value) => rule.Band.Contains(value.Value),
        (CountValue rule, InputDefault.CountValue value) => rule.Band.Contains(value.Value),
        (TextValue, InputDefault.TextValue) => true,
        (PaintValue, InputDefault.PaintValue) => true,
        _ => false,
    };

    internal Fin<Unit> Configure(GetPoint getter, Op key) => key.Catch(() => {
        switch (Terminal) {
            case GetResult.Number: getter.AcceptNumber(enable: true, acceptZero: true); break;
            case GetResult.String: getter.AcceptString(enable: true); break;
            case GetResult.Color: getter.AcceptColor(enable: true); break;
            default: break;
        }
        return Fin.Succ(unit);
    });

    internal Fin<Acquired> Project(GetPoint getter, RhinoDoc document, Op key) => Switch(
        state: (Getter: getter, Document: document, Op: key),
        point3: static (held, _) => {
            Point3d value = held.Getter.Point();
            return Fin.Succ<Acquired>(new Acquired.Point(
                Value: value,
                Evidence: new PointEvidence(
                    ViewSerial: Optional(held.Getter.View()).Map(static view => view.RuntimeSerialNumber),
                    OsnapCode: Some(Convert.ToInt32(held.Getter.OsnapEventType, CultureInfo.InvariantCulture)),
                    BasePoint: held.Getter.TryGetBasePoint(basePoint: out Point3d anchor) ? Some(anchor) : None,
                    SnapPoints: toSeq(held.Getter.GetSnapPoints()),
                    ConstructionPoints: toSeq(held.Getter.GetConstructionPoints()))));
        },
        point2: static (held, _) => Fin.Succ<Acquired>(new Acquired.ScreenPoint(Value: held.Getter.Point2d())),
        numberValue: static (held, rule) => held.Getter.Number() is var value && rule.Band.Contains(value)
            ? Fin.Succ<Acquired>(new Acquired.Number(Value: value))
            : Fin.Fail<Acquired>(held.Op.InvalidInput()),
        countValue: static (held, rule) => held.Getter.Number() is var raw
            && raw == Math.Truncate(raw) && raw >= int.MinValue && raw <= int.MaxValue && rule.Band.Contains((int)raw)
            ? Fin.Succ<Acquired>(new Acquired.Count(Value: (int)raw))
            : Fin.Fail<Acquired>(held.Op.InvalidInput()),
        textValue: static (held, rule) => rule.Meaning.Parse(
            text: held.Getter.StringResult(), document: held.Document, key: held.Op),
        paintValue: static (held, _) => Fin.Succ<Acquired>(new Acquired.Paint(Value: held.Getter.Color())));
}

[SmartEnum<int>]
public sealed partial class ObjectGate {
    public static readonly ObjectGate PostSelect = new(key: 0, set: static (getter, on) => getter.EnablePostSelect(on));
    public static readonly ObjectGate Previous = new(key: 1, set: static (getter, on) => getter.EnableSelPrevious(on));
    public static readonly ObjectGate Highlight = new(key: 2, set: static (getter, on) => getter.EnableHighlight(on));
    public static readonly ObjectGate IgnoreGrips = new(key: 3, set: static (getter, on) => getter.EnableIgnoreGrips(on));
    public static readonly ObjectGate EnterPrompt = new(key: 4, set: static (getter, on) => getter.EnablePressEnterWhenDonePrompt(on));

    [UseDelegateFromConstructor]
    internal partial void Set(GetObject getter, bool enabled);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ObjectRule : ISlotted {
    private ObjectRule() { }
    public sealed record PreSelect(bool Enabled, bool IgnoreUnacceptable) : ObjectRule;
    public sealed record Gated(ObjectGate Gate, bool Enabled) : ObjectRule { public override object SlotKey => Gate; }
    public sealed record Filter(GetObjectGeometryFilter Value) : ObjectRule;

    public virtual object SlotKey => GetType();

    internal Fin<Unit> Apply(GetObject getter, Op key) => key.Catch(() => {
        Switch(
            state: getter,
            preSelect: static (target, rule) => { target.EnablePreSelect(rule.Enabled, rule.IgnoreUnacceptable); return unit; },
            gated: static (target, rule) => { rule.Gate.Set(target, rule.Enabled); return unit; },
            filter: static (target, rule) => { target.SetCustomGeometryFilter(rule.Value); return unit; });
        return Fin.Succ(unit);
    });
}

[ComplexValueObject]
public sealed partial class ObjectPlan {
    public int Minimum { get; }
    public int Maximum { get; }
    public Seq<ObjectRule> Rules { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref int minimum,
        ref int maximum,
        ref Seq<ObjectRule> rules) {
        validationError = rules.Exists(static rule => rule is null
                || rule is ObjectRule.Filter { Value: null }
                || rule is ObjectRule.Gated { Gate: null })
            || minimum < 0
            || maximum < 0
            || maximum is not 0 && maximum < minimum
            || !rules.OnePer()
            ? new ValidationError(message: "object plan is invalid")
            : validationError;
    }
}

[SmartEnum<int>]
public sealed partial class ShapeAsk {
    public static readonly ShapeAsk Segment = new(key: 0, run: static () =>
        Projected(RhinoGet.GetLine(out Line value), () => new Acquired.Segment(Value: value)));
    public static readonly ShapeAsk Chain = new(key: 1, run: static () =>
        Projected(RhinoGet.GetPolyline(out Polyline value), () => new Acquired.Chain(Value: value)));
    public static readonly ShapeAsk ArcShape = new(key: 2, run: static () =>
        Projected(RhinoGet.GetArc(out Arc value), () => new Acquired.ArcShape(Value: value)));
    public static readonly ShapeAsk CircleShape = new(key: 3, run: static () =>
        Projected(RhinoGet.GetCircle(out Circle value), () => new Acquired.CircleShape(Value: value)));
    public static readonly ShapeAsk PlaneShape = new(key: 4, run: static () =>
        Projected(RhinoGet.GetPlane(out Plane value), () => new Acquired.PlaneShape(Value: value)));
    public static readonly ShapeAsk RectangleShape = new(key: 5, run: static () =>
        Projected(RhinoGet.GetRectangle(out Point3d[] value), () => new Acquired.RectangleShape(Corners: toArr(value))));
    public static readonly ShapeAsk BoxShape = new(key: 6, run: static () =>
        Projected(RhinoGet.GetBox(out Box value), () => new Acquired.BoxShape(Value: value)));

    [UseDelegateFromConstructor]
    internal partial (Result Native, Func<Fin<Acquired>> Project) Run();

    private static (Result Native, Func<Fin<Acquired>> Project) Projected(Result native, Func<Acquired> wrap) =>
        (native, () => Fin.Succ(wrap()));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ModalInput {
    private ModalInput() { }
    public sealed record Point : ModalInput;
    public sealed record OneObject(ObjectType Filter) : ModalInput;
    public sealed record ManyObjects(ObjectType Filter) : ModalInput;
    public sealed record Text(string Seed) : ModalInput;
    public sealed record Toggle(string Off, string On, bool Seed) : ModalInput;
    public sealed record Number(double Seed, double Lower, double Upper) : ModalInput;
    public sealed record Count(int Seed, int Lower, int Upper) : ModalInput;
    public sealed record Paint(System.Drawing.Color Seed) : ModalInput;
    public sealed record Distance(double Seed) : ModalInput;
    public sealed record Shape(ShapeAsk Ask) : ModalInput;
    public sealed record View(Func<RhinoView, Fin<Acquired>> Project) : ModalInput;
    public sealed record Viewports(Func<Seq<RhinoViewport>, Fin<Acquired>> Project) : ModalInput;
    public sealed record File(GetFileNameMode Mode, string DefaultName, string Title, object Parent) : ModalInput;

    internal Fin<Unit> Admit(Op key) => Switch(
        point: static _ => Fin.Succ(unit),
        oneObject: static _ => Fin.Succ(unit),
        manyObjects: static _ => Fin.Succ(unit),
        text: row => Optional(row.Seed).Map(static _ => unit).ToFin(Fail: key.InvalidInput()),
        toggle: row => from _ in key.AcceptText(row.Off)
                       from __ in key.AcceptText(row.On)
                       from ___ in guard(!string.Equals(row.Off, row.On, StringComparison.OrdinalIgnoreCase), key.InvalidInput())
                       select unit,
        number: row => guard(double.IsFinite(row.Seed)
            && double.IsFinite(row.Lower)
            && double.IsFinite(row.Upper)
            && row.Lower <= row.Seed
            && row.Seed <= row.Upper, key.InvalidInput()).ToFin(),
        count: row => guard(row.Lower <= row.Seed && row.Seed <= row.Upper, key.InvalidInput()).ToFin(),
        paint: static _ => Fin.Succ(unit),
        distance: row => guard(double.IsFinite(row.Seed) && row.Seed >= 0.0, key.InvalidInput()).ToFin(),
        shape: row => guard(row.Ask is not null, key.InvalidInput()).ToFin(),
        view: row => guard(row.Project is not null, key.InvalidInput()).ToFin(),
        viewports: row => guard(row.Project is not null, key.InvalidInput()).ToFin(),
        file: row => from _ in key.AcceptText(row.Title)
                     from __ in guard(row.DefaultName is not null && row.Parent is not null, key.InvalidInput())
                     select unit);

    internal bool Accepts(AcceptRule rule) =>
        rule is AcceptRule.Allowed allowed && allowed.Gate == AcceptGate.Nothing
        && this is Point or OneObject or ManyObjects or Text or Toggle or Number or Count or Paint;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AcquireIntent {
    private AcquireIntent() { }
    public sealed record Interactive(Seq<PromptCase> Cases, PointPlan Point) : AcquireIntent;
    public sealed record Objects(ObjectPlan Plan) : AcquireIntent;
    public sealed record Transform(Func<RhinoViewport, Point3d, Transform> Calculate) : AcquireIntent;
    public sealed record Modal(ModalInput Input) : AcquireIntent;

    internal bool SupportsOptions => this is not Modal;

    internal bool SupportsPromptDefault => SupportsOptions;

    internal bool Accepts(InputDefault value) =>
        this is Interactive row && row.Cases.Exists(prompt => prompt.Accepts(value));

    internal bool Accepts(AcceptRule rule) => Switch(
        state: rule,
        interactive: static (accept, row) => accept.Terminal.ForAll(
            terminal => row.Cases.Exists(prompt => prompt.Terminal == terminal)),
        objects: static (accept, _) => accept.Terminal.IsNone,
        transform: static (accept, _) => accept.Terminal.IsNone,
        modal: static (accept, row) => row.Input.Accepts(accept));

    internal Fin<Unit> Admit(Op key) => Switch(
        interactive: row => from _ in guard(!row.Cases.IsEmpty
                               && row.Cases.ForAll(static value => value is not null)
                               && row.Cases.Map(static value => value.Terminal).Distinct().Count == row.Cases.Count,
                               key.InvalidInput()).ToFin()
                            from __ in guard(row.Point is not null, key.InvalidInput()).ToFin()
                            from ___ in row.Point.Admit(key)
                            from ____ in row.Cases.TraverseM(value => value.Admit(key)).As()
                            select unit,
        objects: row => guard(row.Plan is not null, key.InvalidInput()).ToFin(),
        transform: row => guard(row.Calculate is not null, key.InvalidInput()).ToFin(),
        modal: row => Optional(row.Input).ToFin(Fail: key.InvalidInput()).Bind(value => value.Admit(key)));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record InputDefault {
    private InputDefault() { }
    public sealed record PointValue(Point3d Value) : InputDefault;
    public sealed record NumberValue(double Value) : InputDefault;
    public sealed record CountValue(int Value) : InputDefault;
    public sealed record TextValue(string Value) : InputDefault;
    public sealed record PaintValue(System.Drawing.Color Value) : InputDefault;

    internal Fin<Unit> Admit(Op key) => Switch(
        pointValue: static _ => Fin.Succ(unit),
        numberValue: row => guard(double.IsFinite(row.Value), key.InvalidInput()).ToFin(),
        countValue: static _ => Fin.Succ(unit),
        textValue: row => key.AcceptText(row.Value).Map(static _ => unit),
        paintValue: static _ => Fin.Succ(unit));

    internal Unit Apply(GetBaseClass getter) => Switch(
        state: getter,
        pointValue: static (target, value) => Op.Side(() => target.SetDefaultPoint(value.Value)),
        numberValue: static (target, value) => Op.Side(() => target.SetDefaultNumber(value.Value)),
        countValue: static (target, value) => Op.Side(() => target.SetDefaultInteger(value.Value)),
        textValue: static (target, value) => Op.Side(() => target.SetDefaultString(value.Value)),
        paintValue: static (target, value) => Op.Side(() => target.SetDefaultColor(value.Value)));
}

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record Acquire {
    private Acquire(
        AcquireIntent intent,
        string prompt,
        AcceptPlan accept,
        Option<string> promptDefault,
        Option<InputDefault> @default,
        Option<OptionSet> options) {
        Intent = intent;
        Prompt = prompt;
        Accept = accept;
        PromptDefault = promptDefault;
        Default = @default;
        Options = options;
    }

    public AcquireIntent Intent { get; }
    public string Prompt { get; }
    public AcceptPlan Accept { get; }
    public Option<string> PromptDefault { get; }
    public Option<InputDefault> Default { get; }
    public Option<OptionSet> Options { get; }

    public static Fin<Acquire> Of(
        AcquireIntent intent,
        string prompt,
        AcceptPlan accept,
        Option<string> promptDefault = default,
        Option<InputDefault> @default = default,
        Option<OptionSet> options = default) {
        Op op = Op.Of(name: nameof(Acquire));
        return from admittedIntent in Optional(intent).ToFin(Fail: op.InvalidInput())
               from admittedAccept in Optional(accept).ToFin(Fail: op.InvalidInput())
               from admittedPrompt in op.AcceptText(prompt)
               from _ in admittedIntent.Admit(op)
               from __ in guard(options.IsNone || admittedIntent.SupportsOptions, op.InvalidInput()).ToFin()
               from ___ in guard(promptDefault.IsNone || admittedIntent.SupportsPromptDefault, op.InvalidInput()).ToFin()
               from ____ in guard(admittedAccept.Rules.ForAll(rule => admittedIntent.Accepts(rule)), op.InvalidInput()).ToFin()
               from _____ in promptDefault.Match(
                   Some: value => op.AcceptText(value).Map(static _ => unit),
                   None: static () => Fin.Succ(unit))
               from ______ in @default.Match(
                   Some: value => value.Admit(op).Bind(_ => guard(admittedIntent.Accepts(value), op.InvalidInput()).ToFin()),
                   None: static () => Fin.Succ(unit))
               select new Acquire(
                   admittedIntent, admittedPrompt, admittedAccept, promptDefault, @default, options);
    }
}
```

## [05]-[DRIVE]

`Acquisition.Probe` projects the four host getter-state probes inside one read grant. `GetterDrive.Run` owns one getter and option lease. A bounded `FoldM` consumes option terminals, then projects exactly one final discriminant. Modal payloads remain deferred until `Result.Success`, so failed one-shot calls never read uninitialized `out` values.

```csharp signature
// --- [MODELS] -----------------------------------------------------------------------------
internal sealed record GetterCycle(
    Seq<OptionChoice> Choices,
    Option<GetResult> Terminal);

public sealed record AcquireState(
    bool Any,
    bool Point,
    bool Object,
    bool DocumentPoint);

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class Acquisition {
    public static Fin<AcquireState> Probe(DocumentSession session) {
        Op op = Op.Of();
        return from _ in guard(RhinoApp.IsOnMainThread, op.InvalidContext())
               from target in Optional(session).ToFin(Fail: op.InvalidInput())
               from state in target.Demand(
                   use: document => op.Catch(() => Fin.Succ(new AcquireState(
                       Any: RhinoGet.InGet(document),
                       Point: RhinoGet.InGetPoint(document),
                       Object: RhinoGet.InGetObject(document),
                       DocumentPoint: document.InGetPoint))),
                   key: op,
                   needs: [SessionNeed.Read])
               select state;
    }

    public static Fin<AcquiredReceipt> Get(DocumentSession session, Acquire request) {
        Op op = Op.Of();
        return from _ in guard(RhinoApp.IsOnMainThread, op.InvalidContext())
               from target in Optional(session).ToFin(Fail: op.InvalidInput())
               from active in Optional(request).ToFin(Fail: op.InvalidInput())
               from receipt in target.Demand(
                   use: document => active.Intent.Switch(
                       state: (Request: active, Document: document, Op: op),
                       interactive: static (held, intent) => Interactive(held.Request, intent, held.Document, held.Op),
                       objects: static (held, intent) => Objects(held.Request, intent.Plan, held.Op),
                       transform: static (held, intent) => Transform(held.Request, intent.Calculate, held.Op),
                       modal: static (held, intent) => Modal(held.Request, intent.Input, held.Op)),
                   key: op,
                   needs: [SessionNeed.Acquire])
               select receipt;
    }

    private static Fin<AcquiredReceipt> Interactive(
        Acquire request,
        AcquireIntent.Interactive intent,
        RhinoDoc document,
        Op op) =>
        from receipt in GetterDrive.Run(
            request: request,
            create: static () => new GetPoint(),
            prepare: getter =>
                from __ in intent.Point.Rules.TraverseM(rule => rule.Apply(getter, op)).As()
                from ___ in intent.Cases.TraverseM(row => row.Configure(getter, op)).As()
                select unit,
            receive: getter => PointFeedbackLease.Attach(getter, intent.Point.Feedback, op).Bind(callbacks => {
                GetResult raw;
                using (callbacks) {
                    raw = getter.Get(
                        onMouseUp: intent.Point.OnMouseUp,
                        get2DPoint: intent.Cases.Exists(static row => row.Terminal is GetResult.Point2d));
                }
                return callbacks.Fault.Match(
                    Some: Fin.Fail<GetResult>,
                    None: () => Fin.Succ(raw));
            }),
            project: (getter, raw) => intent.Cases.Find(row => row.Terminal == raw)
                .ToFin(Fail: op.InvalidResult(detail: raw.ToString()))
                .Bind(row => row.Project(getter, document, op)),
            op: op)
        select receipt;

    private static Fin<AcquiredReceipt> Objects(Acquire request, ObjectPlan plan, Op op) => GetterDrive.Run(
        request: request,
        create: static () => new GetObject(),
        prepare: getter => plan.Rules.TraverseM(rule => rule.Apply(getter, op)).As().Map(static _ => unit),
        receive: getter => op.Catch(() => Fin.Succ(getter.GetMultiple(plan.Minimum, plan.Maximum))),
        project: (getter, raw) => raw is GetResult.Object
            ? Picks.CaptureOwned(references: getter.Objects(), key: op)
                .Map(static picks => (Acquired)new Acquired.Objects(Picks: picks))
            : Fin.Fail<Acquired>(op.InvalidResult(detail: raw.ToString())),
        op: op);

    private static Fin<AcquiredReceipt> Transform(
        Acquire request,
        Func<RhinoViewport, Point3d, Transform> calculate,
        Op op) => GetterDrive.Run(
        request: request,
        create: () => new TransformGetter(calculate),
        prepare: static _ => Fin.Succ(unit),
        receive: getter => op.Catch(() => Fin.Succ(getter.GetXform())),
        project: (getter, raw) => getter.Fault.Match(
            Some: Fin.Fail<Acquired>,
            None: () => getter.Calculated
                .Map(static value => (Acquired)new Acquired.Xform(Value: value))
                .ToFin(Fail: op.InvalidResult(detail: raw.ToString()))),
        op: op);

    private static Fin<AcquiredReceipt> Modal(
        Acquire request,
        ModalInput input,
        Op op) => input.Switch(
        state: (Request: request, Op: op),
        point: static (held, _) => ModalResult(held.Op, () => {
            Result native = RhinoGet.GetPoint(held.Request.Prompt, held.Request.Accept.AcceptsNothing, out Point3d value);
            return (native, () => Fin.Succ<Acquired>(new Acquired.Point(
                Value: value,
                Evidence: new PointEvidence(None, None, None, [], []))));
        }),
        oneObject: static (held, modal) => ModalResult(held.Op, () => {
            Result native = RhinoGet.GetOneObject(
                held.Request.Prompt, held.Request.Accept.AcceptsNothing, modal.Filter, out ObjRef reference);
            return (native, () => Picks.CaptureOwned([reference], held.Op)
                .Map(static picks => (Acquired)new Acquired.Objects(Picks: picks)));
        }),
        manyObjects: static (held, modal) => ModalResult(held.Op, () => {
            Result native = RhinoGet.GetMultipleObjects(
                held.Request.Prompt, held.Request.Accept.AcceptsNothing, modal.Filter, out ObjRef[] references);
            return (native, () => Picks.CaptureOwned(references, held.Op)
                .Map(static picks => (Acquired)new Acquired.Objects(Picks: picks)));
        }),
        text: static (held, modal) => ModalResult(held.Op, () => {
            string value = modal.Seed;
            Result native = RhinoGet.GetString(
                held.Request.Prompt, held.Request.Accept.AcceptsNothing, ref value);
            return (native, () => Fin.Succ<Acquired>(new Acquired.Text(Value: value)));
        }),
        toggle: static (held, modal) => ModalResult(held.Op, () => {
            bool value = modal.Seed;
            Result native = RhinoGet.GetBool(
                held.Request.Prompt, held.Request.Accept.AcceptsNothing, modal.Off, modal.On, ref value);
            return (native, () => Fin.Succ<Acquired>(new Acquired.Toggle(Value: value)));
        }),
        number: static (held, modal) => ModalResult(held.Op, () => {
            double value = modal.Seed;
            Result native = RhinoGet.GetNumber(
                held.Request.Prompt, held.Request.Accept.AcceptsNothing, ref value, modal.Lower, modal.Upper);
            return (native, () => Fin.Succ<Acquired>(new Acquired.Number(Value: value)));
        }),
        count: static (held, modal) => ModalResult(held.Op, () => {
            int value = modal.Seed;
            Result native = RhinoGet.GetInteger(
                held.Request.Prompt, held.Request.Accept.AcceptsNothing, ref value, modal.Lower, modal.Upper);
            return (native, () => Fin.Succ<Acquired>(new Acquired.Count(Value: value)));
        }),
        paint: static (held, modal) => ModalResult(held.Op, () => {
            System.Drawing.Color value = modal.Seed;
            Result native = RhinoGet.GetColor(
                held.Request.Prompt, held.Request.Accept.AcceptsNothing, ref value);
            return (native, () => Fin.Succ<Acquired>(new Acquired.Paint(Value: value)));
        }),
        distance: static (held, modal) => ModalResult(held.Op, () => {
            Result native = RhinoGet.GetDistance(held.Request.Prompt, modal.Seed, out double value);
            return (native, () => Fin.Succ<Acquired>(new Acquired.Distance(Value: value)));
        }),
        shape: static (held, modal) => ModalResult(held.Op, modal.Ask.Run),
        view: static (held, modal) => ModalResult(held.Op, () => {
            Result native = RhinoGet.GetView(held.Request.Prompt, out RhinoView value);
            return (native, () => modal.Project(value));
        }),
        viewports: static (held, modal) => ModalResult(held.Op, () => {
            Result native = RhinoGet.GetViewports(held.Request.Prompt, out RhinoViewport[] value);
            return (native, () => modal.Project(toSeq(value)));
        }),
        file: static (held, modal) => held.Op.Catch(() => {
            string value = RhinoGet.GetFileName(modal.Mode, modal.DefaultName, modal.Title, modal.Parent);
            return string.IsNullOrWhiteSpace(value)
                ? Fin.Succ(Receipt(new AcquireTerminal.Cancelled()))
                : Fin.Succ(Receipt(new AcquireTerminal.Value(new Acquired.FileName(Value: value))));
        }));

    private static Fin<AcquiredReceipt> ModalResult(
        Op op,
        Func<(Result Native, Func<Fin<Acquired>> Project)> run) => op.Catch(() => {
        (Result native, Func<Fin<Acquired>> project) = run();
        return native switch {
            Result.Success => project().Map(payload => Receipt(new AcquireTerminal.Value(Payload: payload))),
            Result.Cancel => Fin.Succ(Receipt(new AcquireTerminal.Cancelled())),
            Result.Nothing => Fin.Succ(Receipt(new AcquireTerminal.Nothing())),
            Result.ExitRhino => Fin.Succ(Receipt(new AcquireTerminal.Exit())),
            _ => Fin.Fail<AcquiredReceipt>(op.InvalidResult(detail: native.ToString())),
        };
    });

    private static AcquiredReceipt Receipt(AcquireTerminal terminal) =>
        new(Terminal: terminal, Options: [], GotDefault: false);
}

internal static class GetterDrive {
    internal static Fin<AcquiredReceipt> Run<TGetter>(
        Acquire request,
        Func<TGetter> create,
        Func<TGetter, Fin<Unit>> prepare,
        Func<TGetter, Fin<GetResult>> receive,
        Func<TGetter, GetResult, Fin<Acquired>> project,
        Op op)
        where TGetter : GetBaseClass => op.Catch(() => {
            using TGetter getter = create();
            getter.SetCommandPrompt(request.Prompt);
            _ = request.PromptDefault.Iter(getter.SetCommandPromptDefault);
            _ = request.Default.Iter(value => value.Apply(getter));
            return request.Accept.Apply(getter, op)
                .Bind(_ => prepare(getter))
                .Bind(_ => request.Options.Match(
                    Some: options => options.Bind(getter, op),
                    None: static () => Fin.Succ(new OptionLease())))
                .Bind(lease => {
                    using (lease) {
                        return toSeq(Enumerable.Range(0, request.Accept.OptionBudget + 1))
                            .FoldM<Fin, GetterCycle>(
                                new GetterCycle(Choices: [], Terminal: None),
                                (cycle, _) => cycle.Terminal.IsSome
                                    ? Fin.Succ(cycle)
                                    : receive(getter).Bind(raw => raw is GetResult.Option
                                        ? lease.Selected(getter, op).Map(choice => cycle with {
                                            Choices = cycle.Choices.Add(choice),
                                        })
                                        : Fin.Succ(cycle with { Terminal = Some(raw) })))
                            .As()
                            .Bind(cycle => cycle.Terminal.ToFin(Fail: op.InvalidResult(
                                    detail: nameof(AcceptPlan.OptionBudget)))
                                .Bind(raw => raw switch {
                                    GetResult.Cancel => Sealed(new AcquireTerminal.Cancelled(), getter, cycle.Choices),
                                    GetResult.Nothing => Sealed(new AcquireTerminal.Nothing(), getter, cycle.Choices),
                                    GetResult.Undo => Sealed(new AcquireTerminal.Undone(), getter, cycle.Choices),
                                    GetResult.Timeout => Sealed(new AcquireTerminal.TimedOut(), getter, cycle.Choices),
                                    GetResult.ExitRhino => Sealed(new AcquireTerminal.Exit(), getter, cycle.Choices),
                                    GetResult.NoResult or GetResult.Miss =>
                                        Fin.Fail<AcquiredReceipt>(op.InvalidResult(detail: raw.ToString())),
                                    _ => project(getter, raw).Bind(payload => Sealed(
                                        new AcquireTerminal.Value(Payload: payload), getter, cycle.Choices)),
                                }));
                    }
                });
        });

    private static Fin<AcquiredReceipt> Sealed(
        AcquireTerminal terminal,
        GetBaseClass getter,
        Seq<OptionChoice> choices) => Fin.Succ(new AcquiredReceipt(
        Terminal: terminal,
        Options: choices,
        GotDefault: getter.GotDefault()));
}
```

## [06]-[CALLBACK_BOUNDARY]

`PointFeedbackLease` converts every callback into a non-throwing native handler. `Subscription` owns attachment rollback and complete detachment. Callback, interrupt, and cleanup failures combine before acquisition resumes.

```csharp signature
// --- [BOUNDARIES] -------------------------------------------------------------------------
internal sealed class PointFeedbackLease : IDisposable {
    private readonly GetPoint getter;
    private readonly Op op;
    private readonly Atom<Option<Error>> fault = Atom(Option<Error>.None);
    private Subscription? observation;

    private PointFeedbackLease(GetPoint getter, Op op) {
        this.getter = getter;
        this.op = op;
    }

    internal Option<Error> Fault => fault.Value;

    internal static Fin<PointFeedbackLease> Attach(GetPoint getter, Seq<PointFeedback> feedback, Op op) {
        PointFeedbackLease lease = new(getter, op);
        return Subscription.AttachAll(feedback.Map(row => (Func<Fin<Subscription>>)(() => lease.Wire(row))))
            .Map(attached => {
                lease.observation = attached;
                return lease;
            });
    }

    private Fin<Subscription> Wire(PointFeedback feedback) => op.Catch(() =>
        feedback.Switch(
            state: this,
            mouseMove: static (lease, row) => lease.Hook<GetPointMouseEventArgs>(row.Sink,
                handler => lease.getter.MouseMove += handler, handler => lease.getter.MouseMove -= handler),
            mouseDown: static (lease, row) => lease.Hook<GetPointMouseEventArgs>(row.Sink,
                handler => lease.getter.MouseDown += handler, handler => lease.getter.MouseDown -= handler),
            dynamicDraw: static (lease, row) => lease.Hook<GetPointDrawEventArgs>(row.Sink,
                handler => lease.getter.DynamicDraw += handler, handler => lease.getter.DynamicDraw -= handler),
            postDraw: static (lease, row) => {
                lease.getter.FullFrameRedrawDuringGet = true;
                return lease.Hook<DrawEventArgs>(row.Sink,
                    handler => lease.getter.PostDrawObjects += handler, handler => lease.getter.PostDrawObjects -= handler);
            }));

    private Fin<Subscription> Hook<TArgs>(
        Func<TArgs, Fin<Unit>> sink,
        Action<EventHandler<TArgs>> attach,
        Action<EventHandler<TArgs>> remove) {
        EventHandler<TArgs> handler = (_, args) => Deliver(() => sink(args));
        return Subscription.Attach(subscribe: attach, unsubscribe: remove, handler: handler);
    }

    private void Deliver(Func<Fin<Unit>> effect) {
        if (fault.Value.IsSome) return;
        _ = op.Catch(effect).Match(
            Succ: static _ => unit,
            Fail: error => {
                Fin<Unit> interrupted = op.Catch(() => Fin.Succ(ignore(getter.InterruptMouseMove())));
                _ = fault.Swap(current => Some(interrupted.Match(
                    Succ: _ => current.IfNone(error),
                    Fail: interrupt => current.IfNone(error) + interrupt)));
                return unit;
            });
    }

    public void Dispose() {
        Subscription? attached = Interlocked.Exchange(ref observation, null);
        if (attached?.Close() is not SubscriptionRelease.Faulted failed) return;
        _ = failed.Errors.Head.Iter(first => {
            Error cleanup = failed.Errors.Tail.Fold(first, static (all, next) => all + next);
            _ = fault.Swap(current => Some(current.Match(
                Some: primary => primary + cleanup,
                None: () => cleanup)));
        });
    }
}

internal sealed class TransformGetter(Func<RhinoViewport, Point3d, Transform> calculate) : GetTransform {
    internal Option<Transform> Calculated { get; private set; }
    internal Option<Error> Fault { get; private set; }

    public override Transform CalculateTransform(RhinoViewport viewport, Point3d point) {
        if (Fault.IsSome) return Transform.Unset;
        Op op = Op.Of(name: nameof(CalculateTransform));
        return op.Catch(() => Fin.Succ(calculate(viewport, point))).Match(
            Succ: value => {
                if (!value.IsValid) {
                    Fault = Some(op.InvalidResult(detail: nameof(Transform)));
                    return Transform.Unset;
                }
                Calculated = Some(value);
                return value;
            },
            Fail: error => { Fault = Some(error); return Transform.Unset; });
    }
}
```

## [07]-[BOUNDARY]

`AcquireIntent` is the sole modality entry, `AcquireTerminal` is the sole control egress, and `Acquired` is the sole value egress. `OptionLease`, `PointFeedbackLease`, `GetBaseClass`, `ObjRef`, and every one-shot `out` value terminate before the receipt crosses the session boundary.
