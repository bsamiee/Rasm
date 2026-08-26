# [APPUI_RENDER_MEASURE]

The overlay-plane interaction owners for the viewport: `SectionDrag` manipulates the six-plane clip box through axis-constrained handles committed through the settled `Viewpoint.Section`, and `MeasureSession` takes source-addressed measurements whose pinned rows ARE viewpoint annotations, labelled through the unit-aware `MeasureExpression` evaluator. Both surfaces share `FrameView`, one world-to-screen projection arrow, and the `Shell/navigation#SHELL_CHROME` row family; their verbs reach the deck through the `Render/viewpoint.md` `ViewChrome` key roster and the `Shell/commands#INTENT_TABLE` rows that lift it.

## [01]-[INDEX]

- [02]-[SECTION_MANIPULATOR]: Six-plane vocabulary, axis-constrained drag handles, per-plane enablement, outline overlay, the viewpoint commit.
- [03]-[MEASURE_MODE]: Measurement kinds with their folds, the movable panel, pinned viewpoint annotations, unit-aware expression labels, the footer selection readout.

## [02]-[SECTION_MANIPULATOR]

- Owner: `SectionPlane` `[SmartEnum<string>]` the six-plane vocabulary carrying its axis, its sign, and its read/write pair over the box; `SectionState` the box beside its per-plane enablement and outline row; `SectionHandle` the overlay-plane drag target; `SectionDrag` the axis-constrained motion fold.
- Entry: `SectionDrag.Drag(SectionState state, SectionPlane plane, FrameView view, (double X, double Y) delta, Func<System.Numerics.Vector3, (double X, double Y)> project)` — the one manipulation fold: the screen delta projects onto the plane's own world axis through the frame's own world-to-screen arrow, the plane writer rewrites its single ordinate, and the box re-admits so a face dragged past its opposite refuses instead of inverting; `SectionState.Of(Viewpoint view, SectionBox extent)` — the manipulator OPENS on the viewpoint's own section (or the model extent where none is set); `SectionState.Commit(Viewpoint view)` — the ONE write back onto `Viewpoint.Section`, so the interactive state, the saved view, the BCF clipping planes, and the animation visibility track all read one section vocabulary; `SectionDrag.Passes(SectionState state, PaintCatalog paints, Func<System.Numerics.Vector3, (double X, double Y)> project)` — the outline and handle overlay rows.
- Auto: each plane row carries the axis it moves on, the sign of its outward normal, and the reader and writer that touch its ONE ordinate of the box, so the drag fold is total over the six and a seventh plane is a compile break; the screen-to-world projection scales the pointer delta by the plane axis's own on-screen length so a drag tracks the handle under the cursor at every camera angle and a plane nearly edge-on refuses rather than accelerating without bound; per-plane enablement clamps a disabled plane to its bound extent so disabling is a display fact rather than a geometry rewrite the box cannot undo; the outline draws the twelve box edges — derived from the three axis pairs, so the roster cannot disagree with the box — plus the enabled planes' handle marks through the overlay pass; the section fact reaches the HUD as one chip naming the enabled plane count.
- Law: manipulation renders through `RenderPass.Overlay` and COMMITS through the settled `SectionBox` on `Viewpoint` — `Of` opens on the viewpoint's box and `Commit` writes back to it, so a manipulator-held box that drifts from the saved view is unspellable past a commit and a saved view can never restore a section the user already dragged away.
- Packages: SkiaSharp, Thinktecture.Runtime.Extensions, LanguageExt.Core, System.Numerics (inbox)
- Growth: a new section display is one `SectionState` column; a new plane is impossible by construction because the box has six; a new manipulation gesture is one `Shell/input#POINTER_GESTURES` routing row naming this fold; zero new surface.
- Boundary: the box stays the axis-aligned `SectionBox` the viewpoint codec projects to six BCF planes, so an arbitrary cutting plane is NOT this owner's — inbound arbitrary planes exceed the axis-aligned shape and decode carries `None`; the handle hit test is a screen-space proximity read against the projected face centres and never a scene pick; the drag is CONSTRAINED to the plane's own axis by construction rather than by a modifier key; the section fact feeds the HUD chip through the chrome row family and never a viewport-local readout.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SectionPlane {
    public static readonly SectionPlane MinX = new("min-x", AxisLabel.X, -1f,
        static box => box.MinX, static (box, v) => box with { MinX = v });
    public static readonly SectionPlane MaxX = new("max-x", AxisLabel.X, 1f,
        static box => box.MaxX, static (box, v) => box with { MaxX = v });
    public static readonly SectionPlane MinY = new("min-y", AxisLabel.Y, -1f,
        static box => box.MinY, static (box, v) => box with { MinY = v });
    public static readonly SectionPlane MaxY = new("max-y", AxisLabel.Y, 1f,
        static box => box.MaxY, static (box, v) => box with { MaxY = v });
    public static readonly SectionPlane MinZ = new("min-z", AxisLabel.Z, -1f,
        static box => box.MinZ, static (box, v) => box with { MinZ = v });
    public static readonly SectionPlane MaxZ = new("max-z", AxisLabel.Z, 1f,
        static box => box.MaxZ, static (box, v) => box with { MaxZ = v });

    public AxisLabel Axis { get; }

    public float Sign { get; }

    [UseDelegateFromConstructor]
    public partial double Read(SectionBox box);

    [UseDelegateFromConstructor]
    public partial SectionBox Write(SectionBox box, double ordinate);

    public SectionPlane Opposite =>
        toSeq(Items).Find(row => row.Axis == Axis && row.Sign != Sign).IfNone(this);

    public System.Numerics.Vector3 Normal => Axis.Write(System.Numerics.Vector3.Zero, Sign);

    public System.Numerics.Vector3 Centre(SectionBox box) =>
        Axis.Write(
            new System.Numerics.Vector3(
                (float)((box.MinX + box.MaxX) * 0.5d),
                (float)((box.MinY + box.MaxY) * 0.5d),
                (float)((box.MinZ + box.MaxZ) * 0.5d)),
            (float)Read(box));
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record SectionState(
    SectionBox Box,
    LanguageExt.HashSet<string> Enabled,
    bool Outline,
    double HandleReachPx) {
    public const string ChipKey = "view.section";

    public static SectionState Of(Viewpoint view, SectionBox extent) =>
        new(view.Section.IfNone(extent), toHashSet(SectionPlane.Items.Select(static plane => plane.Key)), Outline: true, HandleReachPx: 12d);

    public Viewpoint Commit(Viewpoint view) => view with { Section = Some(Box) };

    public bool Cuts(SectionPlane plane) => Enabled.Contains(plane.Key);

    public SectionState Toggle(SectionPlane plane) =>
        this with { Enabled = Cuts(plane) ? Enabled.Remove(plane.Key) : Enabled.Add(plane.Key) };

    public SectionBox Clipped(SectionBox extent) =>
        toSeq(SectionPlane.Items).Fold(Box, (box, plane) => Cuts(plane) ? box : plane.Write(box, plane.Read(extent)));

    public string Chip =>
        toSeq(SectionPlane.Items).Count(Cuts) switch {
            0 => "view.section.off",
            var cutting => $"view.section.count:{cutting}",
        };
}

public readonly record struct SectionHandle(SectionPlane Plane, (double X, double Y) Screen, double AxisPixels);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SectionDrag {
    private const double AxisPixelFloor = 2d;

    public static Fin<SectionState> Drag(SectionState state, SectionPlane plane, FrameView view, (double X, double Y) delta, Func<System.Numerics.Vector3, (double X, double Y)> project) =>
        Reach(state.Box, plane, project) switch {
            var pixels when pixels < AxisPixelFloor =>
                Fin.Fail<SectionState>(new ViewportFault.ContextUnavailable($"section/edge-on:{plane.Key}")),
            var pixels => Admitted(
                state,
                plane,
                plane.Read(state.Box) + (Along(plane, view, delta) / pixels)),
        };

    private static double Along(SectionPlane plane, FrameView view, (double X, double Y) delta) {
        (_, (double rx, double ry, double rz), (double ux, double uy, double uz)) =
            OracleFrame.OfCamera(view.Camera.Frame);
        System.Numerics.Vector3 axis = plane.Normal;
        (double sx, double sy) = ((axis.X * rx) + (axis.Y * ry) + (axis.Z * rz), (axis.X * ux) + (axis.Y * uy) + (axis.Z * uz));
        return Math.Sqrt((sx * sx) + (sy * sy)) switch {
            var length when length <= double.Epsilon => 0d,
            var length => (((delta.X * sx) - (delta.Y * sy)) / length) * plane.Sign,
        };
    }

    private static double Reach(SectionBox box, SectionPlane plane, Func<System.Numerics.Vector3, (double X, double Y)> project) {
        (double ax, double ay) = project(plane.Centre(box));
        (double bx, double by) = project(plane.Centre(box) + plane.Normal);
        return Math.Sqrt(((bx - ax) * (bx - ax)) + ((by - ay) * (by - ay)));
    }

    private static Fin<SectionState> Admitted(SectionState state, SectionPlane plane, double ordinate) =>
        ordinate * plane.Sign < plane.Opposite.Read(state.Box) * plane.Sign
            ? Fin.Succ(state with { Box = plane.Write(state.Box, ordinate) })
            : Fin.Fail<SectionState>(new ViewportFault.ContextUnavailable($"section/inverted:{plane.Key}"));

    public static Seq<SectionHandle> Handles(SectionState state, Func<System.Numerics.Vector3, (double X, double Y)> project) =>
        toSeq(SectionPlane.Items).Filter(state.Cuts).Map(plane => new SectionHandle(
            plane, project(plane.Centre(state.Box)), Reach(state.Box, plane, project)));

    public static Option<SectionPlane> Hit(SectionState state, Seq<SectionHandle> handles, (double X, double Y) at) =>
        toSeq(handles
            .Map(handle => (handle.Plane, Distance: Math.Sqrt(
                ((handle.Screen.X - at.X) * (handle.Screen.X - at.X)) + ((handle.Screen.Y - at.Y) * (handle.Screen.Y - at.Y)))))
            .Filter(hit => hit.Distance <= state.HandleReachPx)
            .OrderBy(static hit => hit.Distance))
            .Head.Map(static hit => hit.Plane);

    public const string OutlinePass = "section/outline";

    public static Seq<RenderPass> Passes(SectionState state, PaintCatalog paints, Func<System.Numerics.Vector3, (double X, double Y)> project) =>
        state.Outline
            ? Seq<RenderPass>(new RenderPass.Overlay(OutlinePass, canvas =>
                paints.Paint(OutlinePass).Bind(paint => {
                    Edges(state.Box).Iter(edge => {
                        (double ax, double ay) = project(edge.A);
                        (double bx, double by) = project(edge.B);
                        canvas.DrawLine((float)ax, (float)ay, (float)bx, (float)by, paint);
                    });
                    Handles(state, project).Iter(handle => canvas.DrawCircle(
                        (float)handle.Screen.X, (float)handle.Screen.Y, (float)state.HandleReachPx, paint));
                    return Fin.Succ(unit);
                })))
            : Seq<RenderPass>();

    private static Seq<(System.Numerics.Vector3 A, System.Numerics.Vector3 B)> Edges(SectionBox box) {
        (float x0, float x1, float y0, float y1, float z0, float z1) =
            ((float)box.MinX, (float)box.MaxX, (float)box.MinY, (float)box.MaxY, (float)box.MinZ, (float)box.MaxZ);
        return toSeq(from j in Seq(y0, y1) from k in Seq(z0, z1)
                     select (new System.Numerics.Vector3(x0, j, k), new System.Numerics.Vector3(x1, j, k)))
             + toSeq(from i in Seq(x0, x1) from k in Seq(z0, z1)
                     select (new System.Numerics.Vector3(i, y0, k), new System.Numerics.Vector3(i, y1, k)))
             + toSeq(from i in Seq(x0, x1) from j in Seq(y0, y1)
                     select (new System.Numerics.Vector3(i, j, z0), new System.Numerics.Vector3(i, j, z1)));
    }
}
```

## [03]-[MEASURE_MODE]

- Owner: `MeasureKind` `[SmartEnum<string>]` the measurement vocabulary carrying its vertex arity, its open/closed ring posture, its readout role, and its own quantity fold; `MeasureRow` one taken measurement with its pin state; `MeasureSession` the mode's in-progress state; `MeasurePanel` the movable readout panel with its per-kind settings and its own axis-delta display set; `MeasureExpression` the unit-aware arithmetic evaluator dimension labels read; `SelectionReadout` the footer context pane.
- Cases: `MeasureKind` = point | perpendicular | angle | area | coordinate under the locked kind literals.
- Entry: `MeasureSession.Pick(ViewMeasurementPoint point, Instant at)` — the one vertex arrow, closing a fixed-arity kind's row the moment its last vertex lands and accumulating for an open kind; `Take(Instant at)` — the open kind's own terminator, closing the row and clearing the pick buffer in one transition and refusing a degenerate pick by name; `MeasureExpression.Evaluate(string source, MeasureRole role, MeasurePolicy policy)` — the unit-aware arithmetic fold a dimension label carries, the role electing the unit the result carries; `SelectionReadout.Rows(MeasurePanel panel)` — the footer and panel chrome rows.
- Auto: each kind row carries how many vertices it needs, whether that arity CLOSES the row or opens an unbounded ring (`Open` a declared ctor column, never a self-comparison), which `MeasureRole` its readout renders under, and the fold from those vertices to a quantity, so a pick auto-takes a point on its second vertex and an angle on its third while an area ring runs to the user's terminator; the picked vertices are `ViewMeasurementPoint` values carrying the payload key and sample index the snap resolved, so every measurement is source-addressed and a pinned row survives a reload; pinning promotes a row into the `Viewpoint.Measurements` seq, so a pinned measurement IS a viewpoint annotation and crosses to BCF as its own `BcfLine` rows; hover highlight rides the visibility-override highlight channel in both directions; axis deltas are the PANEL's display posture — `Components` is a pure derivation on the row and the panel's own toggle set decides which rows render it, so toggling deltas re-takes nothing and re-writes no row.
- Law: a dimension label is an EXPRESSION over quantities, not a formatted scalar — addition and subtraction demand one quantity family, multiplication and division admit a scalar operand alone, a bare magnitude stays a scalar until the fold ends, and the readout ROLE elects the unit the whole result must carry; a fragment the grammar cannot spell refuses naming itself rather than being filtered silently away.
- Packages: UnitsNet, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, System.Numerics (inbox), System.Text.RegularExpressions (inbox — the generated label grammar)
- Growth: a new measurement is one `MeasureKind` row carrying its arity, posture, role, and fold; a new panel setting is one `MeasurePanel` column; a new footer readout is one `ChromeContent.Pane` row naming an existing fact key; zero new surface.
- Boundary: measurements are the settled `ViewMeasurement`/`ViewMeasurementPoint` vocabulary and a pinned row is a viewpoint member, so a measurement store beside the viewpoint is the deleted form; every readout renders through `ResolvedLocale.Quantity` under the kind's own `MeasureRole`, so a hardcoded unit suffix, a locale-blind separator, and a precision literal at a label are the three deleted forms; snap participation is `Shell/input`'s vocabulary arriving as resolved points, so this owner runs no snap solver and holds no snap flag; the panel seats through the chrome family and the live selection readout is a `ChromeContent.Pane` on the status trail; the highlight channel is the `Render/viewpoint.md` override vocabulary, so panel-to-scene brushing and metric-panel brushing are one channel; the `measure.*` verbs resolve through `ViewChrome.MeasureKey` and land as `Shell/commands#INTENT_TABLE` rows — the deck lifting them is that page's obligation, stated there.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MeasureKind {
    public static readonly MeasureKind Point = new("point", arity: 2, open: false, MeasureRole.Distance, Between);
    public static readonly MeasureKind Perpendicular = new("perpendicular", arity: 3, open: false, MeasureRole.Distance, Perpendicularly);
    public static readonly MeasureKind Angle = new("angle", arity: 3, open: false, MeasureRole.Angle, Subtended);
    public static readonly MeasureKind Area = new("area", arity: 3, open: true, MeasureRole.Area, Enclosed);
    public static readonly MeasureKind Coordinate = new("coordinate", arity: 1, open: false, MeasureRole.Elevation, Height);

    public int Arity { get; }

    public bool Open { get; }

    public MeasureRole Role { get; }

    public string LabelKey => $"measure.kind.{Key}";

    [UseDelegateFromConstructor]
    public partial Fin<IQuantity> Fold(Seq<ViewMeasurementPoint> vertices);

    private const double Degenerate = 1e-9d;

    private static Fin<IQuantity> Between(Seq<ViewMeasurementPoint> vertices) =>
        (vertices[0].Position, vertices[1].Position) switch {
            var (a, b) => System.Numerics.Vector3.Distance(a, b) switch {
                var span when span <= Degenerate => Fin.Fail<IQuantity>(new ViewportFault.ContextUnavailable("measure/coincident")),
                var span => Fin.Succ<IQuantity>(UnitsNet.Length.FromMeters(span)),
            },
        };

    private static Fin<IQuantity> Perpendicularly(Seq<ViewMeasurementPoint> vertices) =>
        (vertices[0].Position, vertices[1].Position, vertices[2].Position) switch {
            var (a, b, p) => (b - a) switch {
                var axis when axis.Length() <= Degenerate => Fin.Fail<IQuantity>(new ViewportFault.ContextUnavailable("measure/degenerate-axis")),
                var axis => Fin.Succ<IQuantity>(UnitsNet.Length.FromMeters(
                    System.Numerics.Vector3.Cross(axis, p - a).Length() / axis.Length())),
            },
        };

    private static Fin<IQuantity> Subtended(Seq<ViewMeasurementPoint> vertices) =>
        (vertices[0].Position, vertices[1].Position, vertices[2].Position) switch {
            var (a, v, b) => ((a - v).Length(), (b - v).Length()) switch {
                var (left, right) when left <= Degenerate || right <= Degenerate =>
                    Fin.Fail<IQuantity>(new ViewportFault.ContextUnavailable("measure/degenerate-leg")),
                var (left, right) => Fin.Succ<IQuantity>(UnitsNet.Angle.FromRadians(
                    Math.Acos(Math.Clamp(System.Numerics.Vector3.Dot(a - v, b - v) / (left * right), -1d, 1d)))),
            },
        };

    private static Fin<IQuantity> Enclosed(Seq<ViewMeasurementPoint> vertices) =>
        vertices.Count < 3
            ? Fin.Fail<IQuantity>(new ViewportFault.ContextUnavailable("measure/open-ring"))
            : (toSeq(Enumerable.Range(0, vertices.Count))
                .Fold(System.Numerics.Vector3.Zero, (sum, index) =>
                    sum + System.Numerics.Vector3.Cross(
                        vertices[index].Position,
                        vertices[(index + 1) % vertices.Count].Position))
                .Length() * 0.5d) switch {
                var area when area <= Degenerate => Fin.Fail<IQuantity>(new ViewportFault.ContextUnavailable("measure/degenerate-ring")),
                var area => Fin.Succ<IQuantity>(UnitsNet.Area.FromSquareMeters(area)),
            };

    private static Fin<IQuantity> Height(Seq<ViewMeasurementPoint> vertices) =>
        Fin.Succ<IQuantity>(UnitsNet.Length.FromMeters(vertices[0].Position.Z));
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record MeasureRow(
    string Key,
    MeasureKind Kind,
    Seq<ViewMeasurementPoint> Vertices,
    IQuantity Value,
    bool Pinned,
    Instant At) {
    public Seq<(AxisLabel Axis, IQuantity Value)> Components =>
        Vertices.Count >= 2
            ? toSeq(AxisLabel.Items).Map(axis => (axis, (IQuantity)UnitsNet.Length.FromMeters(
                Math.Abs(axis.Read(Vertices[Vertices.Count - 1].Position) - axis.Read(Vertices[0].Position)))))
            : Seq<(AxisLabel, IQuantity)>();

    public ViewMeasurement Annotation =>
        new(Key, Vertices,
            Value is UnitsNet.Length length ? length : UnitsNet.Length.Zero,
            Value is UnitsNet.Angle angle ? Seq(angle) : Seq<UnitsNet.Angle>());

    public Seq<string> Sources =>
        toSeq(Vertices.Map(static vertex => ResidencyMarshal.KeyHex(vertex.SourceKey)).Distinct());
}

public sealed record MeasureSession(
    MeasureKind Kind,
    Seq<ViewMeasurementPoint> Picked,
    Seq<MeasureRow> Rows) {
    public static MeasureSession Of(MeasureKind kind) =>
        new(kind, Seq<ViewMeasurementPoint>(), Seq<MeasureRow>());

    public Fin<(MeasureSession Session, Option<MeasureRow> Row)> Pick(ViewMeasurementPoint point, Instant at) =>
        (this with { Picked = Picked.Add(point) }) switch {
            var picked when picked.Kind.Open || !picked.Ready => Fin.Succ((picked, Option<MeasureRow>.None)),
            var picked => picked.Take(at).Map(static closed => (closed.Session, Some(closed.Row))),
        };

    public bool Ready => Picked.Count >= Kind.Arity;

    public MeasureSession Cancel() => this with { Picked = Seq<ViewMeasurementPoint>() };

    public Fin<(MeasureSession Session, MeasureRow Row)> Take(Instant at) =>
        Ready
            ? Kind.Fold(Picked).Map(value => new MeasureRow(
                    $"{Kind.Key}/{Rows.Count}", Kind, Picked, value, Pinned: false, at))
                .Map(row => (this with { Picked = Seq<ViewMeasurementPoint>(), Rows = Rows.Add(row) }, row))
            : Fin.Fail<(MeasureSession, MeasureRow)>(new ViewportFault.ContextUnavailable($"measure/arity:{Kind.Key}"));

    public MeasureSession Pin(string key) =>
        this with { Rows = Rows.Map(row => row.Key == key ? row with { Pinned = true } : row) };

    public Seq<ViewMeasurement> Annotations =>
        Rows.Filter(static row => row.Pinned).Map(static row => row.Annotation);
}

public sealed record MeasurePanel(
    CornerPosition Corner,
    (double X, double Y) Offset,
    MeasurePolicy Settings,
    Option<string> Hovered,
    LanguageExt.HashSet<string> DeltaRows) {
    public const string PanelKey = "measure.panel";
    public const string SelectionKey = "measure.selection";

    public bool ShowsDeltas(MeasureRow row) => DeltaRows.Contains(row.Key);

    public MeasurePanel ToggleDeltas(string key) =>
        this with { DeltaRows = DeltaRows.Contains(key) ? DeltaRows.Remove(key) : DeltaRows.Add(key) };

    public Fin<string> Text(MeasureRow row, ResolvedLocale locale) =>
        Settings.Render(row.Value, row.Kind.Role, locale.Formats);

    public Seq<VisibilityOverride> Highlight(Seq<MeasureRow> rows, Seq<string> scene) =>
        Hovered.Match(
            None: static () => Seq<VisibilityOverride>(),
            Some: key => rows.Find(row => row.Key == key).Match(
                None: static () => Seq<VisibilityOverride>(),
                Some: row => HighlightChannel.Focus(scene, toHashSet(row.Sources))));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static partial class MeasureExpression {
    private static readonly FrozenDictionary<char, int> Precedence = new Dictionary<char, int> {
        ['+'] = 1, ['-'] = 1, ['*'] = 2, ['/'] = 2,
    }.ToFrozenDictionary();

    public static Fin<IQuantity> Evaluate(string source, MeasureRole role, MeasurePolicy policy) =>
        Scanned(source).Bind(Tokenized).Bind(Shunted).Bind(postfix => Reduced(postfix, policy.Unit(role)));

    [Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
    public abstract partial record MeasureTerm {
        private MeasureTerm() { }
        public sealed record Quantity(IQuantity Value) : MeasureTerm;
        public sealed record Scalar(double Value) : MeasureTerm;
        public sealed record Operator(char Symbol) : MeasureTerm;
        public sealed record Open : MeasureTerm;
        public sealed record Close : MeasureTerm;
    }

    [System.Text.RegularExpressions.GeneratedRegex(
        @"\d+(?:\.\d+)?\s*[^\s\d()+\-*/]*|[()+\-*/]", matchTimeoutMilliseconds: 50)]
    private static partial System.Text.RegularExpressions.Regex Tokens { get; }

    private static Fin<Seq<string>> Scanned(string source) =>
        toSeq(Tokens.Matches(source))
            .Fold(
                Fin.Succ((At: 0, Tokens: Seq<string>())),
                (acc, match) => acc.Bind(walk => source.AsSpan(walk.At, match.Index - walk.At).IsWhiteSpace()
                    ? Fin.Succ((At: match.Index + match.Length, Tokens: walk.Tokens.Add(match.Value)))
                    : Fin.Fail<(int At, Seq<string> Tokens)>(new ViewportFault.ContextUnavailable(
                        $"measure/token:{source[walk.At..match.Index].Trim()}"))))
            .Bind(walk => walk.Tokens.IsEmpty || !source.AsSpan(walk.At).IsWhiteSpace()
                ? Fin.Fail<Seq<string>>(new ViewportFault.ContextUnavailable($"measure/source:{source.Trim()}"))
                : Fin.Succ(walk.Tokens));

    private static Fin<Seq<MeasureTerm>> Tokenized(Seq<string> tokens) =>
        tokens.Fold(Fin.Succ(Seq<MeasureTerm>()), (acc, token) => acc.Bind(terms => token switch {
            ['('] => Fin.Succ(terms.Add(new MeasureTerm.Open())),
            [')'] => Fin.Succ(terms.Add(new MeasureTerm.Close())),
            [var symbol] when Precedence.ContainsKey(symbol) => Fin.Succ(terms.Add(new MeasureTerm.Operator(symbol))),
            _ => Magnitude(token).Map(terms.Add),
        }));

    private static Fin<MeasureTerm> Magnitude(string token) =>
        Split.Match(token) switch {
            { Success: true } parsed when double.TryParse(
                parsed.Groups[1].Value, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out double magnitude) =>
                parsed.Groups[2].Value switch {
                    "" => Fin.Succ<MeasureTerm>(new MeasureTerm.Scalar(magnitude)),
                    var abbreviation => Quantified(magnitude, abbreviation),
                },
            _ => Fin.Fail<MeasureTerm>(new ViewportFault.ContextUnavailable($"measure/token:{token}")),
        };

    [System.Text.RegularExpressions.GeneratedRegex(@"^(\d+(?:\.\d+)?)\s*(.*)$", matchTimeoutMilliseconds: 50)]
    private static partial System.Text.RegularExpressions.Regex Split { get; }

    private static Fin<MeasureTerm> Quantified(double magnitude, string abbreviation) =>
        UnitsNet.Quantity.TryFromUnitAbbreviation(
            System.Globalization.CultureInfo.InvariantCulture, magnitude, abbreviation, out IQuantity? parsed)
            && parsed is not null
            ? Fin.Succ<MeasureTerm>(new MeasureTerm.Quantity(parsed))
            : Fin.Fail<MeasureTerm>(new ViewportFault.ContextUnavailable($"measure/unit:{abbreviation}"));

    private static Fin<Seq<MeasureTerm>> Shunted(Seq<MeasureTerm> infix) {
        System.Collections.Generic.Stack<MeasureTerm> held = new();
        System.Collections.Generic.List<MeasureTerm> output = new(infix.Count);
        foreach (MeasureTerm term in infix) {
            switch (term) {
                case MeasureTerm.Quantity or MeasureTerm.Scalar: output.Add(term); break;
                case MeasureTerm.Open: held.Push(term); break;
                case MeasureTerm.Close:
                    while (held.Count > 0 && held.Peek() is not MeasureTerm.Open) { output.Add(held.Pop()); }
                    if (held.Count is 0) { return Fin.Fail<Seq<MeasureTerm>>(new ViewportFault.ContextUnavailable("measure/unbalanced")); }
                    ignore(held.Pop());
                    break;
                case MeasureTerm.Operator op:
                    while (held.Count > 0 && held.Peek() is MeasureTerm.Operator prior
                           && Precedence[prior.Symbol] >= Precedence[op.Symbol]) {
                        output.Add(held.Pop());
                    }
                    held.Push(op);
                    break;
            }
        }
        while (held.Count > 0) {
            MeasureTerm remaining = held.Pop();
            if (remaining is MeasureTerm.Open) { return Fin.Fail<Seq<MeasureTerm>>(new ViewportFault.ContextUnavailable("measure/unbalanced")); }
            output.Add(remaining);
        }
        return Fin.Succ(toSeq(output));
    }

    private static Fin<IQuantity> Reduced(Seq<MeasureTerm> postfix, Enum elected) =>
        postfix.Fold(Fin.Succ(Seq<MeasureTerm>()), (acc, term) => acc.Bind(stack => (term, stack) switch {
            (MeasureTerm.Operator op, [.. var rest, var left, var right]) =>
                Applied(op.Symbol, left, right).Map(result => rest.Add(result)),
            (MeasureTerm.Operator, _) => Fin.Fail<Seq<MeasureTerm>>(new ViewportFault.ContextUnavailable("measure/arity")),
            _ => Fin.Succ(stack.Add(term)),
        })).Bind(stack => stack switch {
            [MeasureTerm.Scalar scalar] => Fin.Succ(UnitsNet.Quantity.From(scalar.Value, elected)),
            [MeasureTerm.Quantity quantity] when quantity.Value.Unit.GetType() == elected.GetType() =>
                Fin.Succ(quantity.Value),
            [MeasureTerm.Quantity quantity] =>
                Fin.Fail<IQuantity>(new ViewportFault.ContextUnavailable($"measure/role:{quantity.Value.QuantityInfo.Name}")),
            _ => Fin.Fail<IQuantity>(new ViewportFault.ContextUnavailable("measure/not-a-quantity")),
        });

    private static Fin<MeasureTerm> Applied(char symbol, MeasureTerm left, MeasureTerm right) =>
        (symbol, left, right) switch {
            ('+' or '-', MeasureTerm.Quantity a, MeasureTerm.Quantity b) when Same(a, b) =>
                Fin.Succ<MeasureTerm>(new MeasureTerm.Quantity(UnitsNet.Quantity.From(
                    symbol is '+' ? a.Value.Value + b.Value.As(a.Value.Unit) : a.Value.Value - b.Value.As(a.Value.Unit),
                    a.Value.Unit))),
            ('+' or '-', MeasureTerm.Scalar a, MeasureTerm.Scalar b) =>
                Fin.Succ<MeasureTerm>(new MeasureTerm.Scalar(symbol is '+' ? a.Value + b.Value : a.Value - b.Value)),
            ('*', MeasureTerm.Quantity a, MeasureTerm.Scalar b) or ('*', MeasureTerm.Scalar b, MeasureTerm.Quantity a) =>
                Fin.Succ<MeasureTerm>(new MeasureTerm.Quantity(UnitsNet.Quantity.From(a.Value.Value * b.Value, a.Value.Unit))),
            ('/', MeasureTerm.Quantity a, MeasureTerm.Scalar b) when Math.Abs(b.Value) > double.Epsilon =>
                Fin.Succ<MeasureTerm>(new MeasureTerm.Quantity(UnitsNet.Quantity.From(a.Value.Value / b.Value, a.Value.Unit))),
            ('*' or '/', MeasureTerm.Scalar a, MeasureTerm.Scalar b) when symbol is '*' || Math.Abs(b.Value) > double.Epsilon =>
                Fin.Succ<MeasureTerm>(new MeasureTerm.Scalar(symbol is '*' ? a.Value * b.Value : a.Value / b.Value)),
            _ => Fin.Fail<MeasureTerm>(new ViewportFault.ContextUnavailable($"measure/operands:{symbol}")),
        };

    private static bool Same(MeasureTerm.Quantity a, MeasureTerm.Quantity b) =>
        a.Value.QuantityInfo.Name == b.Value.QuantityInfo.Name;
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public static class SelectionReadout {
    public static Seq<ChromeRow> Rows(MeasurePanel panel) => Seq(
        new ChromeRow(MeasurePanel.SelectionKey, ChromeSlot.Status, "status/center/selection", 40,
            static _ => true,
            new ChromeContent.Pane(
                Kind: PaneKind.Readout,
                Zone: StatusZone.Center,
                FactKey: MeasurePanel.SelectionKey,
                Badge: None,
                Measure: Some(MeasureRole.Extent))),
        new ChromeRow(MeasurePanel.PanelKey, ChromeSlot.Hud, "hud/panel/measure", 50,
            static _ => true,
            new ChromeContent.Chip(panel.Corner, MeasurePanel.PanelKey)));
}
```

## [04]-[RESEARCH]

(none)
