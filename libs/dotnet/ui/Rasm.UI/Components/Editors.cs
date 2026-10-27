using System.Globalization;
using System.Numerics;
using System.Numerics.Tensors;
using Rasm.UI.Plotting;
using Rasm.UI.Styling;
using SuperLinq;

namespace Rasm.UI.Components;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record GradientEditor(ColorRamp Value, Func<Edit<ColorRamp>, IO<Unit>> Edited) {
    private static readonly PlotPlane.Track Track = PlotPlane.Track.Unit(PhysicalAxis.Horizontal);

    public Selection Selection { get; init; } = new Selection.Many([]);
    public Option<Func<Edit<Selection>, IO<Unit>>> Selected { get; init; }

    public Node Drawn =>
        new Handles<ColorRamp>(Value, Stops, Edited) {
            Selection = Selection,
            Selected = Selected,
            Handle = (key, at) => new Node.Pin(at, new Node.Box { Role = Role.Stop, Conditions = Selection.Holds(key) ? [Condition.Selected] : [] }) { Align = UnitPoint.Center },
        }.Drawn(Track, [Legend.Strip]) with {
            Sheet = Legend.Scaled(Value),
        };

    private static Points<ColorRamp> Stops { get; } =
        Points<ColorRamp>.Listed(
            static ramp => ramp.Stops,
            static stop => new Vector2((float)stop.At, 0.5f),
            static (stop, to) => Fraction.Validated(to.X).ToOption().Map(at => stop with { At = at }),
            static (ramp, stops) => stops.IsEmpty ? None : Some(ramp.WithStops(stops)),
            static (ramp, at) =>
                from position in Fraction.Validated(at.X).ToOption()
                from color in ramp.At(position)
                select new Stop(color, position));
}

public sealed record CurveEditor(PlotPlane.Cartesian Plane, Seq<Vector2> Points, Func<float, float> Fit, Func<Edit<Seq<Vector2>>, IO<Unit>> Edited) {
    public Seq<Func<float, float>> References { get; init; }
    public Option<Samples> Underlay { get; init; }
    public Seq<Node> Background { get; init; }
    public DataSeries Series { get; init; } = new(0);
    public Selection Selection { get; init; } = new Selection.Many([]);
    public Option<Func<Edit<Selection>, IO<Unit>>> Selected { get; init; }

    public Node Drawn =>
        new Handles<Seq<Vector2>>(Points, Knots, Edited) { Selection = Selection, Selected = Selected }.Drawn(Plane,
            Background
            + Underlay.Map(underlay => (Node)new Node.Mark(new Figure.Curve(Plane.X.Domain, underlay) { Fill = new Floor.Baseline(0f) }) { Role = Role.Trace }).ToSeq()
            + Seq<Node>(new Node.Mark(new Figure.Grid(PhysicalAxis.Horizontal)) { Role = Role.Grid }, new Node.Mark(new Figure.Grid(PhysicalAxis.Vertical)) { Role = Role.Grid })
            + (Plane.X == Plane.Y ? Seq<Node>(new Node.Mark(new Figure.Path([new Vector2(Plane.X.Domain.From), new Vector2(Plane.X.Domain.To)])) { Role = Role.GridFine }) : [])
            + References.Map(reference => (Node)new Node.Mark(new Figure.Graph(Plane.X.Domain, reference)) { Role = Role.Trace })
            + Seq<Node>(new Node.Mark(new Figure.Graph(Plane.X.Domain, Fit)) { Role = Role.Series, Series = Series })) with {
            Role = Role.Well,
            Aspect = Factor.Unit,
        };

    private Points<Seq<Vector2>> Knots =>
        Points<Seq<Vector2>>.Listed(
            static knots => knots,
            static knot => knot,
            static (_, to) => Some(to),
            static (_, knots) => Some(toSeq(knots.OrderBy(static knot => knot.X))),
            (_, at) => Some(at with { Y = Plane.Y.Constrained(Fit(at.X)) }));
}

public sealed record Slider(PhysicalAxis Along, AxisRange Domain, float Value, Func<Edit<float>, IO<Unit>> Edited) {
    public Func<AxisRange, Axis> Track { get; init; } = static range => new Axis.Linear(range);
    public Option<AxisRange> SoftRange { get; init; }
    public Option<float> Step { get; init; }
    public Option<float> Origin { get; init; }
    public Option<float> Rest { get; init; }
    public Seq<Node> Background { get; init; }

    public Node Drawn =>
        Placed.Match(
            Succ: Shown,
            Fail: error => Shown(Laid(SoftRange.IfNone(Domain))).Conditioned(Condition.Faulted) switch {
                var faulted => faulted with { Accessible = faulted.Accessible.Combine(new Accessible { Help = error.Message }) },
            });

    private Fin<Axis> Placed {
        get {
            Axis soft = Laid(SoftRange.IfNone(Domain));
            float position = soft.Fraction(Value);
            return position switch {
                < 0f => AxisRange.Validated(float.Max(soft.Value((2f * position) - 1f), Domain.Low), soft.Domain.To).Map(Laid),
                > 1f => AxisRange.Validated(soft.Domain.From, float.Min(soft.Value(2f * position), Domain.High)).Map(Laid),
                _ => soft,
            };
        }
    }

    private Axis Laid(AxisRange range) => Track(range) switch { var axis => axis with { Places = axis.Places | Step.Map(Axis.Digits) } };

    private Node Shown(Axis axis) {
        PlotPlane.Track plane = new(Along, axis);
        return plane.Stretched(new Handles<float>(
                Value,
                Points<float>.Scalar(plane, Lens<float, float>.New(static held => held, shown => _ => float.Clamp(Step.Map(step => MathF.Round(shown / step, MidpointRounding.ToEven) * step).IfNone(shown), Domain.Low, Domain.High))),
                Edited) { Rest = Rest }
            .Drawn(plane, plane.Stretched(new Node.Box { Role = Role.Track }).Cons(Background)
                + AxisRange.Validated(Origin.IfNone(axis.Domain.From), Value).ToOption().Map(static span => (Node)new Node.Mark(new Figure.Region { First = Some(span) }) { Role = Role.Span }).ToSeq()));
    }
}

public sealed record Dial(Axis.Periodic Axis, float Value, Func<Edit<float>, IO<Unit>> Edited) {
    public Option<float> Rest { get; init; }

    private PlotPlane.Polar Plane => new(Axis, None);

    public Node Drawn =>
        new Handles<float>(Value, Points<float>.Fixed([Lens<float, Vector2>.New(static held => new Vector2(held, 1f), at => _ => Axis.Constrained(at.X))]), Edited) { Rest = Rest }
            .Drawn(Plane, [
                new Node.Mark(new Figure.Region()) { Role = Role.Track },
                new Node.Mark(new Figure.Path([new Vector2(Value, 0f), new Vector2(Value, 1f)])) { Role = Role.Needle },
            ]) with {
            Aspect = Plane.Aspect,
        };
}

public sealed record Scrub(string Label, AxisRange Domain, float Value, Func<Edit<float>, IO<Unit>> Edited) {
    public Option<float> Gearing { get; init; }

    public Node Drawn(CultureInfo culture) {
        Axis.Linear axis = new(Domain);
        PlotPlane.Track plane = new(PhysicalAxis.Horizontal, axis);
        return new Node.Flex(PhysicalAxis.Horizontal, [
            new Handles<float>(Value, Points<float>.Scalar(plane, Lens.identity<float>()), Edited) {
                Gearing = Gearing,
                Handle = (_, _) => new Node.Pin(new Vector2(Domain.Low, 0f), new Node.Box { Width = Sizing.Grow.Unit, Height = Sizing.Grow.Unit }) {
                    Role = Role.Label,
                    Span = new Vector2(MathF.Abs(Domain.Span), 1f),
                    Accessible = new Accessible { Label = Label },
                },
            }.Drawn(plane, [new Node.Text([Label]) { Role = Role.Label }]) with {
                Width = Sizing.Fit.Content,
            },
            new Node.Text([axis.Formatted(Value, culture)]) {
                MaxLines = 1,
                Width = Sizing.Grow.Unit,
                Edited = Some<Func<Edit<string>, IO<Unit>>>(edit => IO.lift(Traversable.traverse<Edit, Fin, string, float>(text => Number(axis, text, culture), edit).As().Map(static next => next.As())).Bind(Edited)),
            },
        ]) {
            Width = Sizing.Grow.Unit,
        };
    }

    private Fin<float> Number(Axis axis, string text, CultureInfo culture) =>
        float.TryParse(text, NumberStyles.Float, culture, out float parsed)
            ? UIFault.Unless(axis.Contains(Domain, parsed), UIReason.Range, parsed).Map(_ => parsed)
            : new UIFault(text, UIReason.Unreadable);
}

public sealed record Pane(Node Content, Factor Weight);

public sealed record Split(PhysicalAxis Along, Seq<Pane> Panes, Func<Edit<Seq<Factor>>, IO<Unit>> Edited) {
    public Fin<Node> Drawn {
        get {
            float total = TensorPrimitives.Sum<float>([.. Panes.Map(static pane => (float)pane.Weight)]);
            return Panes.Traverse(pane => Fraction.Validated((float)pane.Weight / total).Map(share => Along.Map(
                horizontal: pane.Content with { Width = new Sizing.Percent(share) },
                vertical: pane.Content with { Height = new Sizing.Percent(share) }))).As().Map(shared => {
                    Node panes = new Node.Flex(Along, shared.Intersperse(new Node.Box { Role = Role.Divider }).Strict()) { Width = Sizing.Grow.Unit, Height = Sizing.Grow.Unit };
                    return AxisRange.Validated(Along.Map(horizontal: 0f, vertical: total), Along.Map(horizontal: total, vertical: 0f)).ToOption().Filter(_ => Panes.Count > 1).Match(
                        Some: domain => Divided(new PlotPlane.Track(Along, new Axis.Linear(domain)), total, panes),
                        None: () => panes);
                });
        }
    }

    private Node Divided(PlotPlane.Track plane, float total, Node panes) =>
        new Handles<Seq<Factor>>(Panes.Map(static pane => pane.Weight), Points<Seq<Factor>>.Listed(
                static weights => weights.Map(static weight => (float)weight).Scan(0f, static (sum, weight) => sum + weight).Tail.Init,
                edge => Along.With(new Vector2(0.5f), edge),
                (_, to) => Some(Along.Of(to)),
                (_, edges) => toSeq((Seq(0f) + edges).Add(total).AsIterable().Window(2).Select(static pair => Factor.Validated(pair[1] - pair[0]))).Traverse(static weight => weight).As().ToOption(),
                static (_, _) => None) with {
            Removed = static (_, _) => None,
        },
            Edited) {
            BackgroundCursor = CursorShape.Default,
            Handle = (_, at) => new Node.Pin(Along.Cross.With(at, 0f), new Node.Box()) { Span = Along.Cross.With(Vector2.Zero, 1f) },
        }.Drawn(plane, [panes]);
}
