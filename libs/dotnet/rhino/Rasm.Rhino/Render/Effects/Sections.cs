using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using System.Reflection;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Grade.Balance;
using Rasm.Imaging.Grade.Curves;
using Rasm.Imaging.Output;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Tone;
using Rasm.Imaging.Tone.Formations;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Settings;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.UI.Assets;
using Rasm.Rhino.UI.Chrome;
using Rasm.Rhino.UI.Editors;
using Rasm.Rhino.UI.Inputs;
using Rasm.Rhino.UI.Numeric;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Viewers;
using Rasm.Rhino.UI.Views;
using Rhino;
using Rhino.PlugIns;
using Rhino.Render.PostEffects;
using Rhino.UI;
using Rhino.UI.Controls;
using Rhino.UI.Controls.DataSource;
using NativeDataSourceEventArgs = Rhino.UI.Controls.DataSource.EventArgs;

namespace Rasm.Rhino.Render.Effects;

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class DefinedEffectSection<TEffect> : EtoPostEffectCollapsibleSection, ICollapsibleSection where TEffect : PostEffect {
    private static readonly Guid Effect = typeof(TEffect).GUID;

    private readonly PostEffectType stage;
    private readonly IPlugInViews views;
    private readonly Option<SectionBody> body;
    private Seq<IDisposable> owned;

    public DefinedEffectSection(PostEffect effect, Func<EtoPostEffectCollapsibleSection, IO<View.Section>> row) {
        views = (IPlugInViews)IPlugInSink.Of(effect);
        (stage, Caption) = (effect.PostEffectType, RowText.Localize(effect.LocalName, table: Some<object>(views)));
        body = Callbacks.Answer(
            DisposalOps.Acquire(retain =>
                from section in row(this)
                from document in IO.lift(static () => Missing.Unless(RhinoDoc.ActiveDoc, nameof(RhinoDoc.ActiveDoc)))
                from scope in RowScope.Open(views, Some(document), new CommitMode.Immediate())
                from held in SectionBody.Realize(views, section, scope, retain)
                select held, resource => owned = owned.Add(resource))
                .Bind(held => IO.lift(() => { Content = held.Content; return Some(held); })),
            static () => Option<SectionBody>.None,
            new CallbackSite(views, GetType(), ConstructorInfo.ConstructorName));
    }

    public override Guid PostEffectId => Effect;
    public override string SettingsTag => Effect.ToString("D", CultureInfo.InvariantCulture);
    Guid ICollapsibleSection.Id => Effect;
    public override LocalizeStringPair Caption { get; }
    public override int SectionHeight => body.Match(Some: static held => held.Height, None: static () => 0);
    public override bool Hidden => !toSeq(GetPostEffects(stage)).Exists(static listed => listed.Id == Effect);

    protected override void OnLoad(System.EventArgs e) {
        base.OnLoad(e);
        Callbacks.Run(body.Traverse(static held => held.Shown(visible: true)).As().Map(static _ => unit), new CallbackSite(views, GetType(), nameof(OnLoad)));
    }

    protected override void OnUnLoad(System.EventArgs e) {
        Callbacks.Run(body.Traverse(static held => held.Shown(visible: false)).As().Map(static _ => unit), new CallbackSite(views, GetType(), nameof(OnUnLoad)));
        base.OnUnLoad(e);
    }

    protected override void Dispose(bool disposing) {
        if (disposing)
            Callbacks.Run(
                IO.lift(() => { Content = null; base.Dispose(disposing); })
                    .Bind(_ => IO.lift(() => { Seq<IDisposable> released = owned; owned = []; return released; }))
                    .Bind(DisposalOps.Release).Uninterruptible(),
                new CallbackSite(views, GetType(), nameof(Dispose)));
        else
            base.Dispose(disposing);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SectionRows {
    // --- [EFFECTS]
    public static Func<EtoPostEffectCollapsibleSection, IO<View.Section>> Stage<TState, TParameter>(
        PostEffect effect, EffectState<TState> state, Seq<(TParameter Parameter, ParameterText Text)> texts, Seq<(string Name, EffectLook<TState, TParameter> Look)> looks,
        Atom<StageFrame> input)
        where TState : IPixelStage<TState>
        where TParameter : class, IStateParameter<TState> =>
        section => Row(effect, state, texts, looks, section, source =>
            Demanded(input).Map(read => Seq(PreviewRows.Contextual(source, effect.LocalName, "Parameters over the stage input", read,
                [new HostEvent<Unit>(typeof(Atom<StageFrame>), nameof(input.Change), (deliver, site, retain) =>
                    Subscriptions.Attach<AtomChangedEvent<StageFrame>>(handler => input.Change += handler, handler => input.Change -= handler,
                        _ => Callbacks.Run(deliver(unit), site), retain))],
                stage => Blocks(source, texts, stage), RowRules.Always)) + Amount(state, section)));

    public static Func<EtoPostEffectCollapsibleSection, IO<View.Section>> Job<TState, TParameter>(
        PostEffect effect, EffectState<TState> state, Seq<(TParameter Parameter, ParameterText Text)> texts, Seq<(string Name, EffectLook<TState, TParameter> Look)> looks,
        Atom<WorkState> work)
        where TState : IStateRecord<TState>
        where TParameter : class, IStateParameter<TState> =>
        section => Row(effect, state, texts, looks, section, source =>
            IO.pure(Of(source, texts, None, static _ => RowRules.Always)
                .Add(TextRows.Progress("Progress", "Work the effect's last run reports", new ReadoutSource<WorkState>.Cell(work), RowRules.Always))));

    public static RowSource<TState> Source<TState, TParameter>(
        string owner, Seq<(TParameter Parameter, ParameterText Text)> texts, Func<RowScope, IO<Seq<ValueStore<TState>>>> stores, Option<TState> factory = default)
        where TState : IStateRecord<TState>
        where TParameter : class, IStateParameter<TState> =>
        RowSource.Fields(owner, texts.Map<(IStateParameter<TState> Parameter, string Caption)>(static pair => (pair.Parameter, pair.Text.Caption)), stores, factory);

    private static IO<View.Section> Row<TState, TParameter>(
        PostEffect effect, EffectState<TState> state, Seq<(TParameter Parameter, ParameterText Text)> texts, Seq<(string Name, EffectLook<TState, TParameter> Look)> looks,
        EtoPostEffectCollapsibleSection section, Func<RowSource<TState>, IO<Seq<ControlRow>>> rows)
        where TState : IStateRecord<TState>
        where TParameter : class, IStateParameter<TState> =>
        Source(state.Owner, texts, _ => IO.pure(Seq(Store(state, section))), Some(state.Default.Record)) switch {
            var source => rows(source).Map(children => new View.Section {
                Identity = effect.GetType(),
                Caption = effect.LocalName,
                Store = Some<ViewStore>(new ViewStore.ApplicationStores()),
                Children = Presets(source, looks, new DirectoryInfo(((PlugIn)IPlugInSink.Of(effect)).SettingsDirectory),
                        scope => new BindingGroup(Seq(source.Values(scope)) + state.Default.Amount.ToSeq().Map(factory =>
                            ValueBinding.Key(state.Owner, EffectStates.AmountKey(factory), Mixed(state, section, factory)))))
                    .Cons(children).Map(static row => (Child)row),
            }),
        };

    private static Seq<ControlRow> Amount<TState>(EffectState<TState> state, EtoPostEffectCollapsibleSection section)
        where TState : IStateRecord<TState> =>
        state.Default.Amount.ToSeq().Bind(factory => Amount(state.Owner, factory, _ => Mixed(state, section, factory)));

    public static Seq<ControlRow> Amount(string owner, Mix factory, Func<RowScope, ValueStore<Mix>> store) =>
        Accepted(RowSource.Keyed(owner, EffectStates.AmountKey(factory), store), RowRules.Always);

    private static IO<IO<Option<StageInput>>> Demanded(Atom<StageFrame> input) =>
        StageFrame.Watch(input).Map(static read => read.Map(static pair => pair.Map(static held =>
            new StageInput(IO.pure(Some(held.Frame)), held.Context, None))));

    // --- [LOOKS]
    public static ControlRow.Readout Presets<TState, TParameter>(
        RowSource<TState> source, Seq<(string Name, EffectLook<TState, TParameter> Look)> looks, DirectoryInfo settingsRoot, Func<RowScope, BindingGroup> bindings)
        where TState : IStateRecord<TState>
        where TParameter : class, IStateParameter<TState> =>
        new(RowShape.Inline, (scope, retain) =>
            from menu in ChoiceRows.Menu([], [], Some(new PresetBinding(
                    scope, bindings(scope), new DirectoryInfo(Path.Combine(settingsRoot.FullName, "Presets", source.Owner)))
                .Listing(looks.Map<(string Caption, ValueSet Values)>(look => (Wording.English.Shown(look.Name, scope.Sink), look.Look.Capture(source.Owner))))), scope.Sink, retain)
            from button in ButtonRows.Menu(menu, (GlyphRole.Presets, IconSlot.PanelButton.Master), IconSlot.PanelButton, RowText.Localize("Presets").Local, scope.Sink, retain)
            select new RowCells(button, None, None, None, [], [], new RowHelp(None, None), IO.pure(unit), None, None),
            RowText.Localize("Presets").Local, RowText.Localize("Stock looks of the effect and the presets saved for it").Local, RowRules.Always) { Wording = Wording.Localized };

    // --- [STORE]
    private static ValueStore<TState> Store<TState>(EffectState<TState> state, EtoPostEffectCollapsibleSection section)
        where TState : IStateRecord<TState> =>
        new(
            IO.lift(() => Some(state.Current).Filter(held => !EqualityComparer<TState>.Default.Equals(held, state.Default.Record))),
            next => state.Write(next.IfNone(state.Default.Record), entries =>
                Callbacks.Each(entries, (entry, _) => Try.lift(() => section.SetParameter(entry.Key, entry.Text)).Run()
                    .Bind(static accepted => Refused.Unless(accepted, nameof(EtoPostEffectCollapsibleSection.SetParameter)))).Map(static _ => unit)),
            EqualityComparer<TState>.Default.Equals,
            Applied.Live,
            Some(Changed(section)));

    private static ValueStore<Mix> Mixed<TState>(EffectState<TState> state, EtoPostEffectCollapsibleSection section, Mix factory)
        where TState : IStateRecord<TState> =>
        new(
            IO.lift(() => state.Value.Amount.Filter(held => held != factory)),
            next => state.Write(next.IfNone(factory), text =>
                    Try.lift(() => section.SetParameter(EffectStates.Amount.Name, text)).Run()
                        .Bind(static accepted => Refused.Unless(accepted, nameof(EtoPostEffectCollapsibleSection.SetParameter)))),
            EqualityComparer<Mix>.Default.Equals,
            Applied.Live,
            Some(Changed(section)));

    private static HostEvent<Unit> Changed(EtoPostEffectCollapsibleSection section) =>
        ValueStore.Signal(
            Subscriptions.Host<NativeDataSourceEventArgs>(
                typeof(EtoCollapsibleSection), handler => section.DataChanged += handler, handler => section.DataChanged -= handler, nameof(EtoCollapsibleSection.DataChanged)),
            static args => args.DataType == ProviderIds.RdkRenderingPostEffects);

    // --- [ITEMS]
    public static Seq<ControlRow> Of<TState, TParameter>(
        RowSource<TState> source, Seq<(TParameter Parameter, ParameterText Text)> texts, Option<StageInput> stage, Func<TParameter, RowRules> rules)
        where TState : IStateRecord<TState>
        where TParameter : class, IStateParameter<TState> =>
        texts.Bind(pair => pair.Parameter.Kind.Accept(new ItemRow<TState>(source, new RowField<TState>(pair.Parameter, pair.Text.Caption, pair.Text.Help), pair.Text, stage, rules(pair.Parameter))));

    public static Seq<ControlRow> Setting(PlugInSetting<bool, bool> row, RowRules rules) =>
        Accepted(RowSource.Toggle(nameof(PlugInSettings), row.Value, scope => PlugInSettings.Store(((IPlugInViews)scope.Sink).Settings, row)), rules);

    public static Seq<ControlRow> Setting<TValue, TKey, TError>(PlugInSetting<TValue, TKey> row, Presentation<TKey> presentation, RowRules rules)
        where TValue : IObjectFactory<TValue, TKey, TError>, IConvertible<TKey>, IMinMaxValue<TValue>
        where TKey : struct, INumber<TKey>
        where TError : Error, IValidationError<TError> =>
        Accepted(RowSource.Bounded(nameof(PlugInSettings), row.Value, presentation, scope => PlugInSettings.Store(((IPlugInViews)scope.Sink).Settings, row)), rules);

    public static Seq<ControlRow> Setting<TValue, TRaw>(PlugInSetting<TValue, TRaw> row, RowRules rules)
        where TValue : notnull
        where TRaw : notnull =>
        Accepted(RowSource.Keyed(nameof(PlugInSettings), row.Value, scope => PlugInSettings.Store(((IPlugInViews)scope.Sink).Settings, row)), rules);

    private static Seq<ControlRow> Accepted<T>((RowSource<T> Source, RowField<T> Field) pair, RowRules rules) where T : notnull =>
        pair.Field.Parameter.Kind.Accept(new ItemRow<T>(pair.Source, pair.Field, ParameterText.Of(pair.Field.Caption, pair.Field.Help), None, rules));

    private static Seq<ControlRow> Nested<TState, TNested>(
        RowSource<TState> source, Seq<string> path, Lens<TState, TNested> lens, HashMap<string, ParameterText> texts, Option<StageInput> stage, RowRules rules)
        where TState : notnull
        where TNested : IStateRecord<TNested> =>
        toSeq(TNested.Parameters).Map(parameter => (Parameter: parameter, Text: texts[parameter.Key])).Strict() switch {
            var pairs => Of(source.Within(path, lens, pairs.Map(static pair => (pair.Parameter, pair.Text.Caption))), pairs, stage, _ => rules),
        };

    private static RowRules Showing<TState>(RowRules rules, RowSource<TState> source, IStateParameter<TState> parameter, Func<TState, bool> holds) where TState : notnull =>
        RowRule.When(source, holds, parameter) switch {
            var shown => rules with { Visible = Some(rules.Visible.Map(visible => visible & shown).IfNone(shown)) },
        };

    private sealed class ItemRow<TState>(RowSource<TState> source, RowField<TState> field, ParameterText text, Option<StageInput> stage, RowRules rules)
        : IStateParameterVisitor<TState, Seq<ControlRow>> where TState : notnull {
        public Seq<ControlRow> Numeric<TValue, TKey>(Lens<TState, TValue> lens, Func<TKey, Fin<TValue>> create, Func<TValue, TKey> key, (TKey Low, TKey High) bounds, Presentation<TKey> presentation)
            where TValue : notnull
            where TKey : struct, INumber<TKey> =>
            [NumericRows.Numeric(source, field, lens, create, key, bounds, presentation, rules)];

        public Seq<ControlRow> OptionalNumeric<TValue, TKey>(Lens<TState, Gated<TValue>> lens, Func<TKey, Fin<TValue>> create, Func<TValue, TKey> key, (TKey Low, TKey High) bounds, Presentation<TKey> presentation)
            where TValue : notnull
            where TKey : struct, INumber<TKey> =>
            [NumericRows.OptionalNumeric(source, field, lens, create, key, bounds, presentation, rules)];

        public Seq<ControlRow> Choice<TValue, TError>(Lens<TState, TValue> lens)
            where TValue : ISmartEnum<string, TValue, TError>
            where TError : Error, IValidationError<TError> =>
            [ChoiceRows.Choice(source, field, lens, scope => IO.pure(ChoiceRows.Items<TValue, TError>(item => text.Items[item.ToValue()].Caption, None, scope.Sink)), rules)];

        public Seq<ControlRow> OptionalChoice<TValue, TError>(Lens<TState, Option<TValue>> lens)
            where TValue : ISmartEnum<string, TValue, TError>
            where TError : Error, IValidationError<TError> =>
            [ChoiceRows.Choice(source, field, lens, scope => IO.pure(ChoiceRows.Optional(
                text.Absent.IfNone(field.Caption), ChoiceRows.Items<TValue, TError>(item => text.Items[item.ToValue()].Caption, None, scope.Sink), scope.Sink)), rules)];

        public Seq<ControlRow> Variant<TValue>(Lens<TState, TValue> lens) where TValue : class =>
            StateCase<TValue>.Items.Map(item => (Item: item, Rows: item.Accept(new Cased<TState, TValue>(
                source, field.Parameter, lens, item.Key, text.Items[item.Key].Parameters, stage, rules)))).Strict() switch {
                    var cases => ChoiceRows.Choice(
                            source, field,
                            Lens<TState, StateCase<TValue>>.New(
                                current => StateCase<TValue>.Items.Single(item => item.Holds(lens.Get(current))),
                                picked => current => picked.Holds(lens.Get(current)) ? current : lens.Set(picked.Default, current)),
                            scope => IO.pure(cases.Map(each => new ChoiceItem<StateCase<TValue>>(
                                each.Item, Wording.English.Shown(text.Items[each.Item.Key].Caption, scope.Sink), None))), rules)
                        .Cons(cases.Bind(static each => each.Rows)),
                };

        public Seq<ControlRow> Enumerated<TEnum>(Lens<TState, TEnum> lens) where TEnum : struct, Enum =>
            [ChoiceRows.Choice(source, field, lens, scope => IO.pure(ChoiceRows.Members<TEnum>(member => text.Items[StoredText.Format(member)].Caption, scope.Sink)), rules)];

        public Seq<ControlRow> Record<TNested>(Lens<TState, TNested> lens) where TNested : IStateRecord<TNested> =>
            new ControlRow.Group(field.Caption, rules).Cons(Nested(source, Seq(field.Parameter.Key), lens, text.Parameters, stage, rules));

        public Seq<ControlRow> OptionalRecord<TNested>(Lens<TState, Gated<TNested>> lens) where TNested : IStateRecord<TNested> =>
            ToggleRows.Check(source, field, Prelude.lens(lens, Gated<TNested>.EnabledEntry.Lens), rules).Cons(Nested(
                source, Seq(field.Parameter.Key, Gated<TNested>.ValueEntry.Key), Prelude.lens(lens, Gated<TNested>.ValueEntry.Lens), text.Parameters, stage,
                Showing(rules, source, field.Parameter, current => lens.Get(current).Enabled)));

        public Seq<ControlRow> Toggle(Lens<TState, bool> lens) => [ToggleRows.Check(source, field, lens, rules)];

        public Seq<ControlRow> Raw<TRaw>(Lens<TState, TRaw> lens) where TRaw : notnull, ISpanParsable<TRaw> =>
            [TextRows.Raw(source, field, lens, new EntryForm.SingleLine(), rules)];

        public Seq<ControlRow> OptionalRaw<TRaw>(Lens<TState, Option<TRaw>> lens) where TRaw : notnull, ISpanParsable<TRaw> =>
            [TextRows.OptionalRaw(source, field, lens, new EntryForm.SingleLine(), rules)];

        public Seq<ControlRow> Color(Lens<TState, Swatch> lens) => [ColorRows.SwatchColor(source, field, lens, stage, rules)];

        public Seq<ControlRow> OptionalColor(Lens<TState, Gated<Swatch>> lens) => [ColorRows.SwatchColor(source, field, lens, stage, rules)];

        public Seq<ControlRow> Gradient(Lens<TState, Ramp> lens) =>
            [stage.Match(
                Some: input => ColorRows.Gradient(source, field, lens, interpolation => text.Items[(string)interpolation].Caption, input.Context.Working, rules),
                None: () => TextRows.Keyed<TState, Ramp, string, InvalidPixelValue>(source, field, lens, new EntryForm.SingleLine(), rules))];

        public Seq<ControlRow> Swatches<TValue, TError>(Lens<TState, TValue> lens)
            where TValue : IObjectFactory<TValue, Seq<Swatch>, TError>, IConvertible<Seq<Swatch>>, IObjectFactory<TValue, string, TError>, IConvertible<string>
            where TError : Error, IValidationError<TError> =>
            [ColorRows.Swatches<TState, TValue, TError>(source, field, lens, stage, rules)];

        public Seq<ControlRow> Keyed<TValue, TRaw, TError>(Lens<TState, TValue> lens)
            where TValue : IObjectFactory<TValue, TRaw, TError>, IConvertible<TRaw>
            where TRaw : notnull, ISpanParsable<TRaw>
            where TError : Error, IValidationError<TError> =>
            [TextRows.Keyed<TState, TValue, TRaw, TError>(source, field, lens, new EntryForm.SingleLine(), rules)];

        public Seq<ControlRow> OptionalKeyed<TValue, TRaw, TError>(Lens<TState, Option<TValue>> lens)
            where TValue : IObjectFactory<TValue, TRaw, TError>, IConvertible<TRaw>
            where TRaw : notnull, ISpanParsable<TRaw>
            where TError : Error, IValidationError<TError> =>
            [TextRows.OptionalKeyed<TState, TValue, TRaw, TError>(source, field, lens, new EntryForm.SingleLine(), rules)];

        public Seq<ControlRow> Loaded<TValue, TKey, TRaw, TError>(Lens<TState, Option<TValue>> lens, Func<TKey, IO<TValue>> load, Func<TValue, TKey> key)
            where TKey : IObjectFactory<TKey, TRaw, TError>, IConvertible<TRaw>
            where TRaw : notnull, ISpanParsable<TRaw>
            where TError : Error, IValidationError<TError> =>
            [TextRows.Loaded<TState, TValue, TKey, TRaw, TError>(source, field, lens, load, key, Eto.FileAction.OpenFile, [], rules)];

        public Seq<ControlRow> Opaque<TValue>(Lens<TState, TValue> lens) where TValue : notnull => throw new UnreachableException();
    }

    private sealed class Cased<TState, TValue>(
        RowSource<TState> source, IStateParameter<TState> parameter, Lens<TState, TValue> lens, string key, HashMap<string, ParameterText> texts, Option<StageInput> stage, RowRules rules)
        : IStateCaseVisitor<TValue, Seq<ControlRow>>
        where TState : notnull
        where TValue : class {
        public Seq<ControlRow> Case<TChild>() where TChild : class, TValue, IStateRecord<TChild> =>
             Nested(
                 source, Seq(parameter.Key, key),
                 Lens<TState, TChild>.New(current => lens.Get(current) as TChild ?? TChild.Default, child => current => lens.Set(child, current)),
                 texts, stage, Showing(rules, source, parameter, current => lens.Get(current) is TChild));
    }

    // --- [BLOCKS]
    public static Seq<ControlRow> Blocks<TState, TParameter>(RowSource<TState> source, Seq<(TParameter Parameter, ParameterText Text)> texts, Option<StageInput> stage)
        where TState : IPixelStage<TState>
        where TParameter : class, IStateParameter<TState> =>
        (source, texts) switch {
            (RowSource<ToneMapping> tone, Seq<(ToneMappingParameter Parameter, ParameterText Text)> pairs) =>
                stage.Map(input => CurveRows.Graph(tone, "Response", "Display code the stage answers over scene stops", input, RowRules.Always)).ToSeq()
                    + Of(tone, pairs, stage, item => Formed(tone, item.Formations))
                    .Add(ChartRows.RampKey(
                        "False color key", "Stops above middle grey each false color band marks", scope => scope.Read(tone).Map(static held => held.Look),
                        Formed(tone, [Formation.AgXFalseColor]))),
            (RowSource<ExposureState> exposure, Seq<(ExposureParameter Parameter, ParameterText Text)> pairs) =>
                Meter(exposure, stage).Cons(Of(exposure, pairs, stage, static _ => RowRules.Always)),
            (RowSource<PointCurves> curves, Seq<(PointCurveParameter Parameter, ParameterText Text)> pairs) =>
                Blocked(curves, pairs, stage, field => [CurveRows.Points(curves, field, "Curves", "Point curves over the stage input", stage, RowRules.Always)]),
            (RowSource<HueCurves> hues, Seq<(HueCurveKind Parameter, ParameterText Text)> pairs) =>
                Blocked(hues, pairs, stage, field => [CurveRows.Hues(hues, field, "Hue curves", "Hue curves over the stage input", stage, RowRules.Always)]),
            (RowSource<WheelsState> wheels, Seq<(WheelsParameter Parameter, ParameterText Text)> pairs) =>
                Blocked(wheels, pairs, stage, field => [ColorRows.Wheels(
                    wheels, wheel => wheel.Parameters switch { var (hue, strength, luma) => (field(hue), field(strength), field(luma)) },
                    "Color wheels", "Tints over the global, shadow, midtone, and highlight ranges",
                    static wheel => wheel.Map(@global: "Global", shadows: "Shadows", midtones: "Midtones", highlights: "Highlights"), stage.Map(static input => input.Context.Working), RowRules.Always)]),
            (RowSource<SelectiveColor> selective, Seq<(SelectiveColorParameter Parameter, ParameterText Text)> pairs) =>
                Blocked(selective, pairs, stage, field => toSeq(RangeAxis.Items).Map(axis => SelectiveColorParameter.Band(axis) switch {
                    var (center, width, softness) => ColorRows.Qualifier(
                        selective, Qualified(axis), "Band of the stage input the grade acts on", axis, SelectiveColorParameter.Range, (field(center), field(width), field(softness)),
                        Some<Func<SelectiveColor, Vector4, Vector4>>(static (graded, color) => graded.Graded()(color)), stage, RowRules.Always),
                })),
            (RowSource<HueIsolation> isolation, Seq<(HueIsolationParameter Parameter, ParameterText Text)> pairs) =>
                Blocked(isolation, pairs, stage, field => toSeq(RangeAxis.Items).Map(axis => HueIsolationParameter.Band(axis) switch {
                    var (center, width, softness) => ColorRows.Qualifier(
                        isolation, Qualified(axis), "Band of the stage input kept in color", axis, HueIsolationParameter.Range, (field(center), field(width), field(softness)),
                        stage.Map<Func<HueIsolation, Vector4, Vector4>>(static input => (kept, color) => kept.Graded(input.Context.Working.Luminance)(color)), stage, RowRules.Always),
                })),
            (RowSource<Levels> levels, Seq<(LevelsParameter Parameter, ParameterText Text)> pairs) =>
                Blocked(levels, pairs, stage, field => [CurveRows.LevelPoints(levels, field, "Levels", "Black and white points over the stage histogram", stage, RowRules.Always)]),
            (RowSource<CdlState> cdl, Seq<(CdlParameter Parameter, ParameterText Text)> pairs) =>
                Seq(Opened(cdl), Saved(cdl)) + Of(cdl, pairs, stage, static _ => RowRules.Always),
            _ => Of(source, texts, stage, static _ => RowRules.Always),
        };

    private static string Qualified(RangeAxis axis) => axis.Map(hue: "Hue range", saturation: "Saturation range", luma: "Luma range");

    private static Seq<ControlRow> Blocked<TRecord, TParameter>(
        RowSource<TRecord> source, Seq<(TParameter Parameter, ParameterText Text)> pairs, Option<StageInput> stage, Func<Func<TParameter, RowField<TRecord>>, Seq<ControlRow>> blocks)
        where TRecord : IStateRecord<TRecord>
        where TParameter : class, IStateParameter<TRecord> =>
        toHashMap(pairs) switch {
            var texts => blocks(parameter => new RowField<TRecord>(parameter, texts[parameter].Caption, texts[parameter].Help)) switch {
                var built => toHashSet(built.Bind(static block => block.Keys)) switch {
                    var covered => built + Of(source, pairs.Filter(pair => !source.Keys(pair.Parameter).Exists(covered.Contains)), stage, static _ => RowRules.Always),
                },
            },
        };

    // --- [TONE_MAPPING]
    private static RowRules Formed(RowSource<ToneMapping> tone, Seq<Formation> formations) =>
        new(None, Some(RowRule.When(tone, held => formations.Exists(formation => formation == held.Selected), ToneMappingParameter.Formation)));

    private static ControlRow.Field Meter(RowSource<ExposureState> exposure, Option<StageInput> stage) =>
        new(exposure, exposure.Fields.Bind(field => exposure.Keys(field.Parameter)), RowShape.Inline, (scope, retain) =>
            from mailbox in Subscriptions.Idle<Unit, Fin<ExposureState>>(metered => metered.Find(unit).Match(
                    Some: answer => IO.lift(answer).Bind(exposed => Committed(scope, exposure, exposed, [ExposureParameter.Exposure])),
                    None: static () => IO.pure(unit)))(new CallbackSite(scope.Sink, typeof(ExposureState), nameof(ExposureState.Metered)), retain)
            from command in IO.lift(static () => new Eto.Forms.Command {
                MenuText = RowText.Localize("Meter").Local, ToolBarText = RowText.Localize("Meter").Local,
                ToolTip = RowText.Localize("Write the exposure that sets the stage input's trimmed mean luminance at middle grey").Local,
            })
            let show = stage.Match(Some: static input => input.Read, None: static () => IO.pure(Option<PixelFrame>.None))
                .Bind(available => IO.lift(() => command.Enabled = available.IsSome)).Map(static _ => unit)
            let site = new CallbackSite(scope.Sink, typeof(Eto.Forms.Command), nameof(Eto.Forms.Command.Executed))
            from executed in Subscriptions.Attach(handler => command.Executed += handler, handler => command.Executed -= handler, Callbacks.Handler<System.EventArgs>(_ =>
                    from held in scope.Read(exposure)
                    from forked in stage.Match(
                        Some: input => input.Read.Bind(frame => frame.Match(
                            Some: read => DisposalOps.Fork(IO.lift<Fin<ExposureState>>(() => held.Metered(read, input.Context.Working)).Bind(answer => mailbox((unit, answer))), retain).Map(static _ => unit),
                            None: static () => IO.pure(unit))),
                        None: static () => IO.pure(unit))
                    select forked, site), retain)
            from button in ButtonRows.Push(command, retain)
            select new RowCells(button, None, None, None, [], [], new RowHelp(None, None), show, None, None),
            RowText.Localize("Exposure meter").Local, RowText.Localize("Meters the stage input once and writes its exposure").Local, RowRules.Always) { Wording = Wording.Localized };

    private static IO<Unit> Committed<TRecord>(RowScope scope, RowSource<TRecord> source, TRecord value, Seq<IStateParameter<TRecord>> writes) where TRecord : IStateRecord<TRecord> =>
        from diff in scope.Commit(new BindingGroup(Seq(source.Values(scope))), ValueSet.Fields(source.Owner, value, writes))
        select unit;

    // --- [CDL_FILES]
    private static ControlRow Opened(RowSource<CdlState> cdl) =>
        Pushed(new CommandRow.Run(
            new CommandFace("cdl-open", RowText.Localize("Open CDL...").Local, RowText.Localize("Read a correction from an ASC .cc, .ccc, or .cdl file").Local, None, None) {
                Wording = Wording.Localized,
            },
            None,
            scope => (
                from paths in HostDialogs.ShowOpenDialog(new OpenFileDialog { Title = RowText.Localize("Open CDL").Local, Filter = HostDialogs.Filter(Readable) })
                from path in IO.lift(paths.Head.ToFin(Errors.Cancelled))
                from text in IO.liftAsync(env => File.ReadAllTextAsync(path, env.Token))
                from read in IO.lift(CdlState.Read(text))
                from picked in read.Map(static (correction, index) => new Captioned<CdlState>(correction.State, correction.Id.Match(
                        Some: static id => new LocalizeStringPair(id, id),
                        None: () => RowText.Localize("Correction {0}", arguments: [index + 1]))))
                    .AsIterableNE()
                    .Match(
                        Some: corrections => read.Tail.IsEmpty
                            ? IO.pure(corrections.Head.Value)
                            : HostDialogs.ShowListBox(RowText.Localize("Open CDL"), RowText.Localize("Correction to open"), corrections, None),
                        None: static () => IO.fail<CdlState>(Errors.Cancelled))
                from written in Committed(scope, cdl, picked, cdl.Fields.Map(static field => field.Parameter))
                select written).Catch(static error => error.Is(Errors.Cancelled), static _ => IO.pure(unit))));

    private static ControlRow Saved(RowSource<CdlState> cdl) =>
        Pushed(new CommandRow.Run(
            new CommandFace("cdl-save", RowText.Localize("Save CDL...").Local, RowText.Localize("Write the correction as an ASC .cc, .ccc, or .cdl file").Local, None, None) {
                Wording = Wording.Localized,
            },
            None,
            scope => (
                from held in scope.Read(cdl)
                from chosen in HostDialogs.ShowSaveDialog(new SaveFileDialog {
                    Title = RowText.Localize("Save CDL").Local, Filter = HostDialogs.Filter(Corrections), DefaultExt = CdlFormat.Correction.Extension,
                })
                from target in IO.lift((Conversions.Validated<CdlFormat, string, InvalidGrade>(Path.GetExtension(chosen)).ToValidation(),
                        Conversions.Validated<OutputPath, string, InvalidOutput>(chosen).ToValidation())
                    .Apply(static (format, path) => (Format: format, Path: path)).As().ToFin())
                from written in CdlState.Save(held, None, target.Format, target.Path)
                select written).Catch(static error => error.Is(Errors.Cancelled), static _ => IO.pure(unit))));

    private static Seq<(string Description, Seq<string> Extensions)> Corrections =>
        toSeq(CdlFormat.Items).Map<(string Description, Seq<string> Extensions)>(static format => (
            RowText.Localize(format.Map(correction: "ASC color correction", collection: "ASC color correction collection", decisionList: "ASC color decision list")).Local,
            [format.Extension]));

    private static Seq<(string Description, Seq<string> Extensions)> Readable =>
        (Description: RowText.Localize("ASC color corrections").Local, Extensions: Corrections.Bind(static row => row.Extensions)).Cons(Corrections);

    public static ControlRow Pushed(CommandRow.Run run) =>
        new ControlRow.Command(run, RowShape.Unlabeled, (scope, retain) =>
            from realized in run.Realize(scope, retain)
            from button in ButtonRows.Push(realized.Commands, retain)
            select new RowCells(button, None, None, None, [], [], new RowHelp(None, None), realized.Refresh, None, None), None);
}
