using System.Globalization;
using System.Numerics;
using Rasm.UI.Automation;
using Rasm.UI.Controls.Primitives;
using Rasm.UI.Layout;
using Rasm.UI.Media;
using Rasm.UI.Plotting;
using Rasm.UI.Styling;
using Rasm.UI.Typography;
using SkiaSharp;
using Wacton.Unicolour;

namespace Rasm.UI.Controls;

// --- [MODELS] --------------------------------------------------------------------------

public abstract record Field {

    // --- [STATE]

    private protected Field(string label) => Label = label;

    public string Label { get; init; }
    public required Key Key { get; init; }
    public Option<string> Help { get; init; }
    public bool ReadOnly { get; init; }
    public bool Enabled { get; init; } = true;
    public bool Visible { get; init; } = true;
    public bool Overridden { get; init; }

    internal virtual bool Modified => false;

    internal virtual Option<IO<Unit>> Reset => None;

    private protected static View Mixed { get; } = new View.Flex(PhysicalAxis.Horizontal, [new View.Box { Role = Roles.Checkmark, Conditions = [Condition.Mixed] }]) { Role = Roles.Field, Width = Sizing.Grow.Unit, Conditions = [Condition.Mixed] };

    // --- [DRAWING]

    public abstract View Drawn(CultureInfo culture);

    internal virtual View Caption(Seq<Span> marked) => new View.Text(marked) { Role = Roles.Label, MaxLines = 1 };

    internal virtual Option<string> Readout(CultureInfo culture) => None;

    private protected static Option<IO<Unit>> Sequenced<TField>(Seq<TField> fields) where TField : Field =>
        fields.Choose(static field => field.Reset) switch {
            var resets => resets.IsEmpty ? None : Some(resets.Iter(static reset => reset).As()),
        };

    // --- [CASES]

    public abstract record Bound<T> : Field where T : notnull {
        private protected Bound(string label, Option<T> value, Func<Edit<T>, IO<Unit>> edited) : base(label) => (Value, Edited) = (value, edited);

        public Option<T> Value { get; init; }
        public Func<Edit<T>, IO<Unit>> Edited { get; init; }
        public Option<T> Rest { get; init; }

        internal override bool Modified => Edit.Restoring(Rest, Value).IsSome;

        internal override Option<IO<Unit>> Reset => Edit.Restoring(Rest, Value).Filter(_ => !ReadOnly).Map(rest => Edited(Edit.Of(EditPhase.Commit, rest)));

        public sealed override View Drawn(CultureInfo culture) => ReadOnly ? Faced(value => Shown(value, culture)) : Editor(culture);

        internal sealed override Option<string> Readout(CultureInfo culture) => Value.Bind(value => Spoken(value, culture));

        private protected abstract View Editor(CultureInfo culture);

        private protected virtual Option<string> Spoken(T value, CultureInfo culture) => None;

        private protected virtual View Shown(T value, CultureInfo culture) =>
            Spoken(value, culture).Match(
                Some: static text => new View.Text([text]) { Role = Roles.Field, MaxLines = 1, Width = Sizing.Grow.Unit },
                None: () => Editor(culture).Conditioned(Condition.Disabled));

        private protected View Marked(View node) => Edit.Marked(Rest, Value) switch { var marks => node with { Conditions = node.Conditions.Union(Value.IsNone ? marks.TryAdd(Condition.Mixed) : marks) } };

        private protected View Faced(Func<T, View> face) => Value.Match(Some: value => Marked(face(value)), None: () => Marked(Mixed));
    }

    public sealed record Number(string Label, Option<float> Value, Func<Edit<float>, IO<Unit>> Edited, AxisRange Domain) : Bound<float>(Label, Value, Edited) {
        public Func<AxisRange, Axis> Track { get; init; } = static range => new Axis.Linear(range);
        public Option<AxisRange> SoftRange { get; init; }
        public Option<float> Step { get; init; }
        public Option<float> Gearing { get; init; }
        public Key Role { get; init; } = Roles.Field;
        public bool Scrubs { get; init; }

        private Option<Scrub> Scrubbed => Value.Filter(_ => !ReadOnly && Scrubs).Map(value => new Scrub(Label, Domain, value, Edited) { Gearing = Gearing, Track = Track, Rest = Rest });

        private protected override View Editor(CultureInfo culture) =>
            Scrubbed.Match(
                Some: scrub => Marked(scrub.Entry(culture)),
                None: () => new Slider(PhysicalAxis.Horizontal, Domain, Value.ToSeq(), edit => Edited(edit.Choose(static held => held.Head))) {
                    Track = Track,
                    SoftRange = SoftRange,
                    Step = Step,
                    Rest = Rest.Map(static rest => Seq(rest)),
                    Role = Role,
                }.Drawn(culture));

        internal override View Caption(Seq<Span> marked) => Scrubbed.Match(Some: static scrub => scrub.Handle, None: () => base.Caption(marked));

        private protected override Option<string> Spoken(float value, CultureInfo culture) => Track(Domain).Spoken(value, culture);
    }

    public sealed record Vector(string Label, Seq<Number> Components) : Field(Label) {
        public Option<Boolean> Linked { get; init; }

        internal override bool Modified => Components.Exists(static component => component.Modified);

        internal override Option<IO<Unit>> Reset => ReadOnly ? None : Sequenced(Components);

        public override View Drawn(CultureInfo culture) =>
            new View.Flex(PhysicalAxis.Horizontal,
                Components.Map((component, index) => Coupled(component, index) switch {
                    var held => (View)new View.Flex(PhysicalAxis.Horizontal, [held.Caption([held.Label]), held.Drawn(culture)]) {
                        Key = held.Key,
                        Width = Sizing.Grow.Unit,
                        Accessible = new Accessible { Label = held.Label, Help = held.Help },
                    },
                })
                + Linked.Map(linked => (linked with { ReadOnly = linked.ReadOnly || ReadOnly }) switch {
                    var held => (held.Content.IsEmpty ? held with { Kind = ToggleKind.Button, Content = [new View.Box { Role = Roles.Lock }] } : held).Drawn(culture),
                }).ToSeq()) {
                Width = Sizing.Grow.Unit,
            };

        internal override Option<string> Readout(CultureInfo culture) =>
            Components.Choose(component => component.Readout(culture)) switch { var said => said.IsEmpty ? None : Some(string.Join(' ', said)) };

        private Number Coupled(Number component, int index) =>
            (component with { ReadOnly = component.ReadOnly || ReadOnly }) switch {
                var held => Linked.Exists(static linked => linked.Value == Some(value: true)) ? held with { Edited = edit => Scaled(index, edit) } : held,
            };

        private IO<Unit> Scaled(int index, Edit<float> edit) =>
            Components.Map((component, at) => component.ReadOnly ? Option<IO<Unit>>.None : at == index
                    ? Some(component.Edited(edit))
                    : edit.Value.Match(
                        Some: to => from start in Components[index].Value.Filter(static start => start is not 0f)
                                    from held in component.Value
                                    select component.Edited(Edit.Of(edit.Phase, (float)((double)held * to / start))),
                        None: () => Some(component.Edited(edit))))
                .Somes()
                .Iter(static effect => effect).As();
    }

    public sealed record Boolean(string Label, Option<bool> Value, Func<Edit<bool>, IO<Unit>> Edited) : Bound<bool>(Label, Value, Edited) {
        public ToggleKind Kind { get; init; } = ToggleKind.CheckBox;
        public Seq<View> Content { get; init; }

        private protected override View Editor(CultureInfo culture) =>
            new Toggle(Value, Edited) {
                Kind = Kind,
                Content = Content,
                Rest = Rest.Map(static rest => Check.Of(rest)),
            }.Drawn;
    }

    public sealed record Choice(string Label, Option<Key> Value, Func<Edit<Key>, IO<Unit>> Edited, Seq<Key> Items, Func<Key, string> Named) : Bound<Key>(Label, Value, Edited) {
        public Option<Func<Key, View>> Face { get; init; }
        public Option<TextField> Filter { get; init; }

        private protected override View Editor(CultureInfo culture) => new Picker(Items, Value, Named, Edited) { Face = Face, Filter = Filter, Rest = Rest }.Drawn(culture);

        private protected override Option<string> Spoken(Key value, CultureInfo culture) => Named(value);
    }

    public sealed record Text(string Label, Option<string> Value, Func<Edit<string>, IO<Unit>> Edited) : Bound<string>(Label, Value, Edited) {
        public Option<string> Placeholder { get; init; }
        public Option<int> MaxLines { get; init; } = Some(1);

        private protected override View Editor(CultureInfo culture) => Marked(new TextField(Value.IfNone(""), Edited) { Placeholder = Placeholder, MaxLines = MaxLines }.Drawn);

        private protected override Option<string> Spoken(string value, CultureInfo culture) => value;

        private protected override View Shown(string value, CultureInfo culture) => new View.Text([value]) { Role = Roles.Field, MaxLines = MaxLines, Width = Sizing.Grow.Unit };
    }

    public sealed record Color(string Label, Option<Unicolour> Value, Func<Edit<Unicolour>, IO<Unit>> Edited) : Bound<Unicolour>(Label, Value, Edited) {
        public bool Alpha { get; init; } = true;

        private protected override View Editor(CultureInfo culture) =>
            Faced(static colour => Swatch.Patch(colour)) switch {
                var face => (Value | Rest).Match(
                    Some: shown => Picker.Dropdown(face, new View.Flex(PhysicalAxis.Vertical, [new ColorPicker(shown, Edited) { Alpha = Alpha, Rest = Rest }.Drawn(culture)]) { Role = Roles.Popover }) with {
                        Width = Sizing.Grow.Unit,
                    },
                    None: () => face),
            };

        private protected override Option<string> Spoken(Unicolour value, CultureInfo culture) => Swatch.Hex(value);

        private protected override View Shown(Unicolour value, CultureInfo culture) => Swatch.Patch(value);
    }

    public sealed record Ramp(string Label, Option<ColorRamp> Value, Func<Edit<ColorRamp>, IO<Unit>> Edited) : Bound<ColorRamp>(Label, Value, Edited) {
        private protected override View Editor(CultureInfo culture) => Faced(ramp => new GradientEditor(ramp, Edited).Drawn);
    }

    public sealed record Curve(string Label, Option<Seq<Vector2>> Value, Func<Edit<Seq<Vector2>>, IO<Unit>> Edited, PlotPlane.Cartesian Plane, Func<Seq<Vector2>, Func<float, float>> Fit)
        : Bound<Seq<Vector2>>(Label, Value, Edited) {
        private protected override View Editor(CultureInfo culture) => Faced(points => new CurveEditor(Plane, points, Fit(points), Edited).Drawn);
    }

    public sealed record Path<T>(string Label, Option<T> Value, Func<Edit<T>, IO<Unit>> Edited, Func<SKRect, IO<Option<T>>> Pick, Func<T, string> Named) : Bound<T>(Label, Value, Edited) where T : notnull {
        private protected override View Editor(CultureInfo culture) =>
            new Part.Press(bounds => Pick(bounds).Bind(picked => picked.Match(Some: value => Edited(Edit.Of(EditPhase.Commit, value)), None: static () => IO.pure(unit)))).Drawn(Value.Match(Some: Face, None: () => Marked(Mixed)));

        private protected override Option<string> Spoken(T value, CultureInfo culture) => Named(value);

        private protected override View Shown(T value, CultureInfo culture) => Face(value);

        private View Face(T value) =>
            Named(value) switch {
                var name => Marked(new View.Flex(PhysicalAxis.Horizontal, [
                new View.Text([name]) { MaxLines = 1, Truncation = Fraction.Half, Width = Sizing.Grow.Unit },
                new View.Box { Role = Roles.Overflow },
            ]) {
                    Role = Roles.Field,
                    Width = Sizing.Grow.Unit,
                    Accessible = new Accessible { Value = new AccessValue.Text(name) },
                }),
            };
    }

    public sealed record Button(string Label, Func<SKRect, IO<Unit>> Run) : Field(Label) {
        public override View Drawn(CultureInfo culture) =>
            new Part.Press(Run).Drawn(new View.Flex(PhysicalAxis.Horizontal, [new View.Text([Label]) { MaxLines = 1 }]) { Role = Roles.Button, Width = Sizing.Grow.Unit });

        internal override View Caption(Seq<Span> marked) => new View.Box();
    }

    public sealed record Custom(string Label, View Content) : Field(Label) {
        public override View Drawn(CultureInfo culture) => Content;
    }

    public sealed record Group(string Label, Seq<Field> Fields) : Field(Label) {
        public bool Expanded { get; init; } = true;
        public Option<Func<Edit<bool>, IO<Unit>>> Edited { get; init; }

        internal override bool Modified => Fields.Exists(static held => held.Modified);

        internal override Option<IO<Unit>> Reset => ReadOnly ? None : Sequenced(Fields);

        public override View Drawn(CultureInfo culture) => new Inspector([this]).Drawn(culture);
    }
}

public sealed record Inspector(Seq<Field> Fields) {

    // --- [STATE]

    private static readonly PlotPlane.Track Divider = PlotPlane.Track.Unit(PhysicalAxis.Horizontal);
    private static readonly Key Unpinned = new Key.Ordinal(1);

    public Option<TextField> Filter { get; init; }
    public HashSet<Key> Pinned { get; init; }
    public Option<Func<Edit<HashSet<Key>>, IO<Unit>>> Pinning { get; init; }
    public Fraction LabelWidth { get; init; } = Fraction.Half;
    public Option<Func<Edit<Fraction>, IO<Unit>>> Resized { get; init; }

    // --- [DRAWING]

    public View Drawn(CultureInfo culture) {
        Search query = Search.Of(Filter);
        Seq<Field> kept = Kept(Fields, query, culture);
        Seq<Field> pinned = Row<Field>.Flatten(kept.Filter(static field => field.Visible), static field => (field is Field.Group group ? group.Fields : []).Filter(static held => held.Visible), static field => field.Key, static _ => true, static _ => true)
            .Map(static row => row.Item).Filter(field => Pinned.Contains(field.Key));
        View body = new View.Flex(PhysicalAxis.Vertical,
            (pinned.IsEmpty ? [] : Seq<View>(new View.Flex(PhysicalAxis.Vertical, [
                new View.Flex(PhysicalAxis.Horizontal, [new View.Box { Role = Roles.Pin, Conditions = [Condition.Selected] }]) { Role = Roles.Header, Width = Sizing.Grow.Unit },
                Grid(pinned, query, culture),
            ]) { Width = Sizing.Grow.Unit }))
            + Seq<View>(Grid(kept, query, culture) with { Key = Unpinned })) {
            Width = Sizing.Grow.Unit,
        };
        return new View.Flex(PhysicalAxis.Vertical, Filter.Map(static field => field.Drawn).ToSeq().Add(Resized.Match(
            Some: resized => Split.Divided(new Handles<float>((float)LabelWidth, Points<float>.Scalar(Divider, Lens.identity<float>()),
                edit => resized(edit.Choose(static at => Fraction.Validated(at).ToOption()))), Divider, body),
            None: () => body))) {
            Role = Roles.Section,
            Width = Sizing.Grow.Unit,
            Accessible = new Accessible { Role = AccessRole.Group },
        };
    }

    private View.Grid Grid(Seq<Field> fields, Search query, CultureInfo culture) =>
        new(new Tracks([new Sizing.Percent(LabelWidth), Sizing.Grow.Unit]), fields.Filter(static field => field.Visible).Map(field => Cells(field, query, culture))) { Width = Sizing.Grow.Unit };

    private Seq<View> Cells(Field field, Search query, CultureInfo culture) =>
        (field is Field.Group group
            ? Seq(Grouped(group, query, culture))
            : field.Caption(query.Marked(field.Label, culture)) switch {
                var caption => field.Drawn(culture) switch {
                    var control => Seq(
                        caption with { Accessible = caption.Accessible.Combine(new Accessible { Help = field.Help }) },
                        new View.Flex(PhysicalAxis.Horizontal, Seq(control) + (control.Width.Exists(static width => width.Stretches) ? [] : Seq<View>(new View.Box { Width = Sizing.Grow.Unit })) + Decor(field)) {
                            Key = field.Key,
                            Width = Sizing.Grow.Unit,
                            Accessible = new Accessible { Label = field.Label, Help = field.Help },
                        }),
                },
            })
        .Map(cell => field.Enabled ? cell : cell.Conditioned(Condition.Disabled));

    private View Grouped(Field.Group group, Search query, CultureInfo culture) =>
        new Disclosure(
            new View.Flex(PhysicalAxis.Horizontal, Seq<View>(new View.Text(query.Marked(group.Label, culture)) { MaxLines = 1, Width = Sizing.Grow.Unit }) + Decor(group)) {
                Width = Sizing.Grow.Unit,
                Accessible = new Accessible { Help = group.Help },
            },
            Grid(group.ReadOnly ? group.Fields.Map(static field => field with { ReadOnly = true }) : group.Fields, query, culture),
            group.Expanded) {
            Edited = group.Edited,
        }.Drawn with {
            Key = group.Key,
        };

    private Seq<View> Decor(Field field) =>
        (field.Overridden ? Seq<View>(new View.Box { Role = Roles.Indicator }) : [])
        + field.Reset.Map(static reset => new Part.Press(_ => reset).Drawn(new View.Box { Role = Roles.Revert })).ToSeq()
        + Pinning.Map(pinning => new Part.Press(_ => pinning(Edit.Of(EditPhase.Commit, Pinned.SymmetricExcept([field.Key])))).Drawn(
            new View.Box { Role = Roles.Pin, Conditions = Pinned.Contains(field.Key) ? [Condition.Selected] : [] })).ToSeq();

    // --- [QUERIES]

    private static Seq<Field> Kept(Seq<Field> fields, Search query, CultureInfo culture) =>
        fields.Choose(field => query.Matches(field.Label, culture) || field.Readout(culture).Exists(said => query.Matches(said, culture))
            ? Some(field)
            : field is Field.Group group && Kept(group.Fields, query, culture) is { IsEmpty: false } held
                ? Some<Field>(group with { Fields = held, Expanded = true })
                : Option<Field>.None);
}
