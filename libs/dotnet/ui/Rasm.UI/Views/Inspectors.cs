using System.Globalization;
using System.Numerics;
using Rasm.UI.Components;
using Rasm.UI.Plotting;
using Rasm.UI.Styling;
using SkiaSharp;
using Wacton.Unicolour;

namespace Rasm.UI.Views;

// --- [MODELS] --------------------------------------------------------------------------
public abstract record Field {
    private static readonly Node Dash = new Node.Box { Role = Role.Checkmark, Conditions = [Condition.Mixed] };

    private protected Field(string label) => Label = label;

    // --- [STATE]
    public string Label { get; init; }
    public Option<Key> Key { get; init; }
    public Option<string> Help { get; init; }
    public bool ReadOnly { get; init; }
    public bool Enabled { get; init; } = true;
    public bool Visible { get; init; } = true;
    public bool Overridden { get; init; }

    internal Key Keyed => Key.IfNone(() => new Key.Named(Label));

    internal virtual bool Modified => false;

    internal virtual Option<IO<Unit>> Reset => None;

    private protected static Node Mixed { get; } = new Node.Flex(PhysicalAxis.Horizontal, [Dash]) { Role = Role.Field, Width = Sizing.Grow.Unit, Conditions = [Condition.Mixed] };

    // --- [DRAW]
    public abstract Node Drawn(CultureInfo culture);

    internal virtual Node Caption(Seq<Span> marked) => new Node.Text(marked) { Role = Role.Label, MaxLines = 1 };

    internal virtual Option<string> Said(CultureInfo culture) => None;

    private protected static Option<IO<Unit>> Sequenced<TField>(Seq<TField> fields) where TField : Field =>
        fields.Choose(static field => field.Reset) switch {
            var resets => resets.IsEmpty ? None : Some(resets.TraverseM(static reset => reset).As().Map(static _ => unit)),
        };

    // --- [CASES]
    public abstract record Bound<T> : Field where T : notnull {
        private protected Bound(string label, Option<T> value, Func<Edit<T>, IO<Unit>> edited) : base(label) => (Value, Edited) = (value, edited);

        public Option<T> Value { get; init; }
        public Func<Edit<T>, IO<Unit>> Edited { get; init; }
        public Option<T> Rest { get; init; }

        internal override bool Modified => Rest.Exists(rest => Some(rest) != Value);

        internal override Option<IO<Unit>> Reset => Rest.Filter(_ => !ReadOnly && Modified).Map(rest => Edited(Edit.Of(EditPhase.Commit, rest)));

        public sealed override Node Drawn(CultureInfo culture) => ReadOnly ? Faced(value => Shown(value, culture)) : Editor(culture);

        internal sealed override Option<string> Said(CultureInfo culture) => Value.Bind(value => Spoken(value, culture));

        private protected abstract Node Editor(CultureInfo culture);

        private protected virtual Option<string> Spoken(T value, CultureInfo culture) => None;

        private protected virtual Node Shown(T value, CultureInfo culture) =>
            Spoken(value, culture).Match(
                Some: static text => new Node.Text([text]) { Role = Role.Field, MaxLines = 1, Width = Sizing.Grow.Unit },
                None: () => Editor(culture).Conditioned(Condition.Disabled));

        private protected HashSet<Condition> Marks => toHashSet(Controls.Modified(Rest, Value) + (Value.IsNone ? Seq(Condition.Mixed) : []));

        private protected Node Marked(Node node) => node with { Conditions = node.Conditions.Union(Marks) };

        private protected Node Faced(Func<T, Node> face) => Value.Match(Some: value => Marked(face(value)), None: () => Marked(Mixed));
    }

    public sealed record Number(string Label, Option<float> Value, Func<Edit<float>, IO<Unit>> Edited, AxisRange Domain) : Bound<float>(Label, Value, Edited) {
        public Func<AxisRange, Axis> Track { get; init; } = static range => new Axis.Linear(range);
        public Option<AxisRange> SoftRange { get; init; }
        public Option<float> Step { get; init; }
        public Option<float> Gearing { get; init; }
        public Role Role { get; init; } = Role.Field;

        private Option<Scrub> Scrubbed => Value.Filter(_ => !ReadOnly && Role.Equals(Role.Label)).Map(value => new Scrub(Label, Domain, value, Edited) { Gearing = Gearing, Track = Track, Rest = Rest });

        private protected override Node Editor(CultureInfo culture) =>
            Scrubbed.Match(
                Some: scrub => Marked(scrub.Entry(culture)),
                None: () => new Slider(PhysicalAxis.Horizontal, Domain, Value.ToSeq(), edit => Edited(new Edit<float>(edit.Phase, edit.Value.Bind(static held => held.Head)))) {
                    Track = Track,
                    SoftRange = SoftRange,
                    Step = Step,
                    Rest = Rest.Map(static rest => Seq(rest)),
                    Role = Role.Equals(Role.Label) ? Role.Field : Role,
                }.Drawn(culture));

        internal override Node Caption(Seq<Span> marked) => Scrubbed.Match(Some: static scrub => scrub.Handle, None: () => base.Caption(marked));

        private protected override Option<string> Spoken(float value, CultureInfo culture) => Track(Domain).Spoken(value, culture);
    }

    public sealed record Vector(string Label, Seq<Number> Components) : Field(Label) {
        public Option<Boolean> Linked { get; init; }

        internal override bool Modified => Components.Exists(static component => component.Modified);

        internal override Option<IO<Unit>> Reset => ReadOnly ? None : Sequenced(Components);

        public override Node Drawn(CultureInfo culture) =>
            new Node.Flex(PhysicalAxis.Horizontal,
                Components.Map((component, index) => Ratioed(component, index) switch {
                    var held => (Node)new Node.Flex(PhysicalAxis.Horizontal, [held.Caption([held.Label]), held.Drawn(culture)]) {
                        Key = held.Keyed,
                        Width = Sizing.Grow.Unit,
                        Accessible = new Accessible { Label = held.Label, Help = held.Help },
                    },
                })
                + Linked.Map(linked => (linked.Content.IsEmpty ? linked with { Role = Role.Button, Content = [new Node.Box { Role = Role.Lock }] } : linked).Drawn(culture)).ToSeq()) {
                Width = Sizing.Grow.Unit,
            };

        internal override Option<string> Said(CultureInfo culture) =>
            Components.Choose(component => component.Said(culture)) switch { var said => said.IsEmpty ? None : Some(string.Join(' ', said)) };

        private Number Ratioed(Number component, int index) =>
            (component with { ReadOnly = component.ReadOnly || ReadOnly }) switch {
                var held => Linked.Exists(static linked => linked.Value == Some(value: true)) ? held with { Edited = edit => Scaled(index, edit) } : held,
            };

        private IO<Unit> Scaled(int index, Edit<float> edit) =>
            Components.Map((component, at) => at == index
                    ? Some(component.Edited(edit))
                    : edit.Value.Match(
                        Some: to => from start in Components[index].Value.Filter(static start => start is not 0f)
                                    from held in component.Value
                                    select component.Edited(Edit.Of(edit.Phase, held * to / start)),
                        None: () => Some(component.Edited(edit))))
                .Somes()
                .TraverseM(static effect => effect).As().Map(static _ => unit);
    }

    public sealed record Boolean(string Label, Option<bool> Value, Func<Edit<bool>, IO<Unit>> Edited) : Bound<bool>(Label, Value, Edited) {
        public Role Role { get; init; } = Role.Toggle;
        public Seq<Node> Content { get; init; }

        private protected override Node Editor(CultureInfo culture) =>
            new Toggle(Checked(Value), edit => Edited(new Edit<bool>(edit.Phase, edit.Value.Map(static check => check == Check.On)))) {
                Role = Role,
                Content = Content,
                Rest = Rest.Map(static rest => Checked(Some(rest))),
            }.Drawn;

        private static Check Checked(Option<bool> value) => value.Match(Some: static on => on ? Check.On : Check.Off, None: static () => Check.Mixed);
    }

    public sealed record Choice(string Label, Option<Key> Value, Func<Edit<Key>, IO<Unit>> Edited, Seq<Key> Items, Func<Key, string> Named) : Bound<Key>(Label, Value, Edited) {
        public Option<Func<Key, Node>> Face { get; init; }
        public Option<TextField> Filter { get; init; }

        private protected override Node Editor(CultureInfo culture) => new Picker(Items, Value, Named, Edited) { Face = Face, Filter = Filter, Rest = Rest }.Drawn(culture);

        private protected override Option<string> Spoken(Key value, CultureInfo culture) => Named(value);
    }

    public sealed record Text(string Label, Option<string> Value, Func<Edit<string>, IO<Unit>> Edited) : Bound<string>(Label, Value, Edited) {
        public Option<string> Placeholder { get; init; }
        public Option<int> MaxLines { get; init; } = Some(1);

        private protected override Node Editor(CultureInfo culture) => Marked(new TextField(Value.IfNone(""), Edited) { Placeholder = Placeholder, MaxLines = MaxLines }.Drawn);

        private protected override Option<string> Spoken(string value, CultureInfo culture) => value;

        private protected override Node Shown(string value, CultureInfo culture) => new Node.Text([value]) { Role = Role.Field, MaxLines = MaxLines, Width = Sizing.Grow.Unit };
    }

    public sealed record Color(string Label, Option<Unicolour> Value, Func<Edit<Unicolour>, IO<Unit>> Edited) : Bound<Unicolour>(Label, Value, Edited) {
        public bool Alpha { get; init; } = true;

        private protected override Node Editor(CultureInfo culture) =>
            Faced(static colour => Swatch.Patch(colour.ToSKColorF())) switch {
                var face => (Value | Rest).Match(
                    Some: shown => Controls.Dropdown(face, new Node.Flex(PhysicalAxis.Vertical, [new ColorPicker(shown, Edited) { Alpha = Alpha, Rest = Rest }.Drawn(culture)]) { Role = Role.Popover }) with {
                        Width = Sizing.Grow.Unit,
                    },
                    None: () => face),
            };

        private protected override Option<string> Spoken(Unicolour value, CultureInfo culture) => value.Hex;

        private protected override Node Shown(Unicolour value, CultureInfo culture) => Swatch.Patch(value.ToSKColorF());
    }

    public sealed record Ramp(string Label, Option<ColorRamp> Value, Func<Edit<ColorRamp>, IO<Unit>> Edited) : Bound<ColorRamp>(Label, Value, Edited) {
        private protected override Node Editor(CultureInfo culture) => Faced(ramp => new GradientEditor(ramp, Edited).Drawn);
    }

    public sealed record Curve(string Label, Option<Seq<Vector2>> Value, Func<Edit<Seq<Vector2>>, IO<Unit>> Edited, PlotPlane.Cartesian Plane, Func<Seq<Vector2>, Func<float, float>> Fit)
        : Bound<Seq<Vector2>>(Label, Value, Edited) {
        private protected override Node Editor(CultureInfo culture) => Faced(points => new CurveEditor(Plane, points, Fit(points), Edited).Drawn);
    }

    public sealed record Path<T>(string Label, Option<T> Value, Func<Edit<T>, IO<Unit>> Edited, Func<SKRect, IO<Option<T>>> Pick, Func<T, string> Named) : Bound<T>(Label, Value, Edited) where T : notnull {
        private protected override Node Editor(CultureInfo culture) =>
            new Part.Press(bounds => Pick(bounds).Bind(picked => picked.Match(Some: value => Edited(Edit.Of(EditPhase.Commit, value)), None: static () => IO.pure(unit)))).Drawn(Value.Match(Some: Face, None: () => Marked(Mixed)));

        private protected override Option<string> Spoken(T value, CultureInfo culture) => Named(value);

        private protected override Node Shown(T value, CultureInfo culture) => Face(value);

        private Node Face(T value) =>
            Marked(new Node.Flex(PhysicalAxis.Horizontal, [
                new Node.Text([Named(value)]) { MaxLines = 1, Truncation = Truncation.Middle, Width = Sizing.Grow.Unit },
                new Node.Box { Role = Role.Overflow },
            ]) {
                Role = Role.Field,
                Width = Sizing.Grow.Unit,
                Accessible = new Accessible { Value = new AccessValue.Text(Named(value)) },
            });
    }

    public sealed record Button(string Label, Func<SKRect, IO<Unit>> Run) : Field(Label) {
        public override Node Drawn(CultureInfo culture) =>
            new Part.Press(Run).Drawn(new Node.Flex(PhysicalAxis.Horizontal, [new Node.Text([Label]) { MaxLines = 1 }]) { Role = Role.Button, Width = Sizing.Grow.Unit });

        internal override Node Caption(Seq<Span> marked) => new Node.Box();
    }

    public sealed record Custom(string Label, Node Content) : Field(Label) {
        public override Node Drawn(CultureInfo culture) => Content;
    }

    public sealed record Group(string Label, Seq<Field> Fields) : Field(Label) {
        public bool Expanded { get; init; } = true;
        public Option<Func<Edit<bool>, IO<Unit>>> Expanding { get; init; }

        internal override bool Modified => Fields.Exists(static held => held.Modified);

        internal override Option<IO<Unit>> Reset => ReadOnly ? None : Sequenced(Fields);

        public override Node Drawn(CultureInfo culture) => new Inspector([this]).Drawn(culture);
    }
}

public sealed record Inspector(Seq<Field> Fields) {
    private static readonly Search Everything = new("");
    private static readonly PlotPlane.Track Divider = PlotPlane.Track.Unit(PhysicalAxis.Horizontal);

    // --- [STATE]
    public Option<TextField> Filter { get; init; }
    public HashSet<Key> Pinned { get; init; }
    public Option<Func<Edit<HashSet<Key>>, IO<Unit>>> Pinning { get; init; }
    public Fraction Split { get; init; } = Fraction.Half;
    public Option<Func<Edit<Fraction>, IO<Unit>>> Resized { get; init; }

    // --- [DRAW]
    public Node Drawn(CultureInfo culture) {
        Search query = Filter.Map(static field => new Search(field.Text)).IfNone(Everything);
        Seq<Field> kept = Kept(Fields, query, culture);
        Seq<Field> pinned = Flattened(kept).Filter(field => Pinned.Contains(field.Keyed));
        Node body = new Node.Flex(PhysicalAxis.Vertical,
            (pinned.IsEmpty ? [] : Seq<Node>(new Node.Flex(PhysicalAxis.Vertical, [
                new Node.Flex(PhysicalAxis.Horizontal, [new Node.Box { Role = Role.Pin, Conditions = [Condition.Selected] }]) { Role = Role.Header, Width = Sizing.Grow.Unit },
                Table(pinned, query, culture),
            ]) { Key = new Key.Ordinal(0), Width = Sizing.Grow.Unit }))
            + Seq<Node>(Table(kept, query, culture) with { Key = new Key.Ordinal(1) })) {
            Width = Sizing.Grow.Unit,
        };
        return new Node.Flex(PhysicalAxis.Vertical, Filter.Map(static field => field.Drawn).ToSeq().Add(Resized.Match(Some: resized => Divided(body, resized), None: () => body))) {
            Role = Role.Section,
            Width = Sizing.Grow.Unit,
            Accessible = new Accessible { Role = AccessRole.Group },
        };
    }

    private Node.Grid Table(Seq<Field> fields, Search query, CultureInfo culture) =>
        new(new Tracks([new Sizing.Percent(Split), Sizing.Grow.Unit]), fields.Filter(static field => field.Visible).Map(field => Row(field, query, culture))) { Width = Sizing.Grow.Unit };

    private Seq<Node> Row(Field field, Search query, CultureInfo culture) =>
        (field is Field.Group group
            ? Seq(Grouped(group, query, culture))
            : field.Caption(query.Marked(field.Label, culture)) switch {
                var caption => field.Drawn(culture) switch {
                    var control => Seq(
                        caption with { Accessible = caption.Accessible.Combine(new Accessible { Help = field.Help }) },
                        new Node.Flex(PhysicalAxis.Horizontal, Seq(control) + (control.Width.Exists(static width => width.Stretches) ? [] : Seq<Node>(new Node.Box { Width = Sizing.Grow.Unit })) + Decor(field)) {
                            Key = field.Keyed,
                            Width = Sizing.Grow.Unit,
                            Accessible = new Accessible { Label = field.Label, Help = field.Help },
                        }),
                },
            })
        .Map(cell => field.Enabled ? cell : cell.Conditioned(Condition.Disabled));

    private Node Grouped(Field.Group group, Search query, CultureInfo culture) {
        Node header = new Node.Flex(PhysicalAxis.Horizontal, Seq<Node>(new Node.Text(query.Marked(group.Label, culture)) { MaxLines = 1, Width = Sizing.Grow.Unit }) + Decor(group)) {
            Width = Sizing.Grow.Unit,
            Accessible = new Accessible { Help = group.Help },
        };
        Node table = Table(group.ReadOnly ? group.Fields.Map(static field => field with { ReadOnly = true }) : group.Fields, query, culture);
        return group.Expanding.Match(
            Some: expanding => new Disclosure(header, table, group.Expanded, expanding).Drawn,
            None: () => new Node.Flex(PhysicalAxis.Vertical, [header with { Role = Role.Header }, table]) { Width = Sizing.Grow.Unit }) with {
            Key = group.Keyed,
        };
    }

    private Seq<Node> Decor(Field field) =>
        (field.Overridden ? Seq<Node>(new Node.Box { Role = Role.Indicator }) : [])
        + field.Reset.Map(static reset => new Part.Press(_ => reset).Drawn(new Node.Box { Role = Role.Revert })).ToSeq()
        + Pinning.Map(pinning => new Part.Press(_ => pinning(Edit.Of(EditPhase.Commit, Pinned.Contains(field.Keyed) ? Pinned.Remove(field.Keyed) : Pinned.Add(field.Keyed)))).Drawn(
            new Node.Box { Role = Role.Pin, Conditions = Pinned.Contains(field.Keyed) ? [Condition.Selected] : [] })).ToSeq();

    private Node Divided(Node body, Func<Edit<Fraction>, IO<Unit>> resized) =>
        new Handles<float>((float)Split, Points<float>.Scalar(Divider, Lens.identity<float>()) with { Placed = static (_, _, _) => None },
            edit => resized(new Edit<Fraction>(edit.Phase, edit.Value.Bind(static at => Fraction.Validated(at).ToOption())))) {
            BackgroundCursor = CursorShape.Default,
            Handle = static (_, at) => new Node.Pin(PhysicalAxis.Vertical.With(at, 0f), new Node.Box()) { Span = PhysicalAxis.Vertical.With(Vector2.Zero, 1f) },
        }.Drawn(Divider, [body]);

    // --- [QUERY]
    private static Seq<Field> Kept(Seq<Field> fields, Search query, CultureInfo culture) =>
        fields.Choose(field => query.Matches(field.Label, culture) || field.Said(culture).Exists(said => query.Matches(said, culture))
            ? Some(field)
            : field is Field.Group group && Kept(group.Fields, query, culture) is { IsEmpty: false } held
                ? Some<Field>(group with { Fields = held, Expanded = true })
                : Option<Field>.None);

    private static Seq<Field> Flattened(Seq<Field> fields) =>
        fields.Filter(static field => field.Visible).Bind(static field => field is Field.Group group ? Seq<Field>(group) + Flattened(group.Fields) : Seq(field));
}
