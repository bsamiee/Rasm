using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using System.Reflection;
using Eto.Forms;
using Rasm.Imaging.ColorManagement;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Grade.Balance;
using Rasm.Imaging.Grade.Curves;
using Rasm.Imaging.Output;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Tone;
using Rasm.Imaging.Tone.Formations;
using Rasm.Rhino.Document.Files;
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
using Rhino.Render;
using Rhino.Render.PostEffects;
using Rhino.UI;
using Rhino.UI.Controls;
using UnitsNet.Units;

namespace Rasm.Rhino.Render.Effects;

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class DefinedEffectSection<TEffect> : EtoPostEffectCollapsibleSection, ICollapsibleSection where TEffect : PostEffect {
    private static readonly Guid Effect = typeof(TEffect).GUID;

    private readonly PostEffectType stage;
    private readonly Option<SectionBody> body;

    public DefinedEffectSection(PostEffect effect, Func<EtoPostEffectCollapsibleSection, IO<View.Section>> row) {
        IPlugInViews views = (IPlugInViews)IPlugInSink.Of(effect);
        (stage, Caption) = (effect.PostEffectType, RowText.Localize(effect.LocalName, table: Some<object>(views)));
        body = Callbacks.Answer(
            from section in row(this)
            from document in IO.lift(static () => Missing.Unless(RhinoDoc.ActiveDoc, nameof(RhinoDoc.ActiveDoc)))
            from scope in RowScope.Open(views, Some(document), new CommitMode.Immediate())
            from held in SectionBody.Realize(views, section, scope)
            select Some(held),
            static () => Option<SectionBody>.None,
            new CallbackSite(views, GetType(), ConstructorInfo.ConstructorName));
        _ = body.Iter(held => Content = held.Content);
    }

    public override Guid PostEffectId => Effect;
    public override string SettingsTag => Effect.ToString("D", CultureInfo.InvariantCulture);
    Guid ICollapsibleSection.Id => Effect;
    public override LocalizeStringPair Caption { get; }
    public override int SectionHeight => body.Match(Some: static held => held.Height, None: static () => 0);
    public override bool Hidden => !toSeq(GetPostEffects(stage)).Exists(static listed => listed.Id == Effect);

    protected override void OnLoad(EventArgs e) {
        base.OnLoad(e);
        _ = body.Iter(static held => held.Shown(visible: true));
    }

    protected override void OnUnLoad(EventArgs e) {
        _ = body.Iter(static held => held.Shown(visible: false));
        base.OnUnLoad(e);
    }

    protected override void Dispose(bool disposing) {
        if (disposing)
            _ = body.Iter(static held => held.Dispose());
        base.Dispose(disposing);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SectionRows {
    // --- [EFFECTS]
    public static StageInput Unstaged { get; } = new(IO.pure(Option<PixelFrame>.None), EffectPipeline.Working, EffectPipeline.Display, EffectPipeline.Depth, None);

    public static Func<EtoPostEffectCollapsibleSection, IO<View.Section>> Stage<TState, TParameter>(
        PostEffect effect, EffectState<TState> state, Seq<(TParameter Parameter, ParameterText Text)> texts, Seq<(string Name, EffectLook<TState, TParameter> Look)> looks,
        Atom<StageFrame> input, Func<RenderContent.ChangeContexts, IO<Unit>, IO<Unit>> changing)
        where TState : IPixelStage<TState>
        where TParameter : class, IStateParameter<TState> =>
        section => Row(effect, state, texts, looks, section, changing, source =>
            Demanded(input).Map(stage => (effect.PostEffectType is PostEffectType.ToneMapping
                ? CurveRows.Graph(source, "Response", "Display code the stage answers over scene stops", stage, RowRules.Always).Cons(Blocks(source, texts, stage))
                : Blocks(source, texts, stage)) + Amount(state, section, changing)));

    public static Func<EtoPostEffectCollapsibleSection, IO<View.Section>> Job<TState, TParameter>(
        PostEffect effect, EffectState<TState> state, Seq<(TParameter Parameter, ParameterText Text)> texts, Seq<(string Name, EffectLook<TState, TParameter> Look)> looks,
        Atom<WorkState> work, Func<RenderContent.ChangeContexts, IO<Unit>, IO<Unit>> changing)
        where TState : IStateRecord<TState>
        where TParameter : class, IStateParameter<TState> =>
        section => Row(effect, state, texts, looks, section, changing, source =>
            IO.pure(Of(source, texts, Unstaged, static _ => RowRules.Always)
                .Add(TextRows.Progress("Progress", "Work the effect's last run reports", new ReadoutSource<WorkState>.Cell(work), RowRules.Always))));

    public static RowSource<TState> Source<TState, TParameter>(
        string owner, Seq<(TParameter Parameter, ParameterText Text)> texts, Func<RowScope, IO<Seq<ValueStore<TState>>>> stores)
        where TState : IStateRecord<TState>
        where TParameter : class, IStateParameter<TState> =>
        RowSource.Fields(owner, texts.Map<(IStateParameter<TState> Parameter, string Caption)>(static pair => (pair.Parameter, pair.Text.Caption)), stores);

    private static IO<View.Section> Row<TState, TParameter>(
        PostEffect effect, EffectState<TState> state, Seq<(TParameter Parameter, ParameterText Text)> texts, Seq<(string Name, EffectLook<TState, TParameter> Look)> looks,
        EtoPostEffectCollapsibleSection section, Func<RenderContent.ChangeContexts, IO<Unit>, IO<Unit>> changing, Func<RowSource<TState>, IO<Seq<ControlRow>>> rows)
        where TState : IStateRecord<TState>
        where TParameter : class, IStateParameter<TState> =>
        Source(state.Owner, texts, _ => IO.pure(Seq(Store(state, section, changing)))) switch {
            var source => rows(source).Map(children => new View.Section {
                Identity = effect.GetType(),
                Caption = effect.LocalName,
                Store = Some<ViewStore>(new ViewStore.ApplicationStores()),
                Children = Presets(source, looks).Cons(children).Map(static row => (Child)row),
            }),
        };

    private static Seq<ControlRow> Amount<TState>(EffectState<TState> state, EtoPostEffectCollapsibleSection section, Func<RenderContent.ChangeContexts, IO<Unit>, IO<Unit>> changing)
        where TState : IStateRecord<TState> =>
        state.Value.Amount.ToSeq().Bind(_ => Accepted(RowSource.Bounded(
            state.Owner, EffectStates.Amount,
            new Presentation<Mix, float> { Origin = (float)EffectStates.Amount.Default, Unit = UnitsNet.Quantity.GetUnitInfo(RatioUnit.DecimalFraction) },
            _ => Mixed(state, section, changing)), RowRules.Always));

    private static IO<StageInput> Demanded(Atom<StageFrame> input) =>
        StageFrame.Watch(input).Map(static read => new StageInput(read, EffectPipeline.Working, EffectPipeline.Display, EffectPipeline.Depth, None));

    // --- [LOOKS]
    private static ControlRow.Readout Presets<TState, TParameter>(RowSource<TState> source, Seq<(string Name, EffectLook<TState, TParameter> Look)> looks)
        where TState : IStateRecord<TState>
        where TParameter : class, IStateParameter<TState> =>
        new(RowShape.Inline, scope =>
            from menu in ChoiceRows.Menu([], [], Some(PresetBinding.Listing(scope,
                Stocked(source) + looks.Map<(string Caption, ValueSet Values)>(look => (Wording.English.Shown(look.Name, scope.Sink), Captured(source.Owner, look.Look))))), scope.Sink)
            from button in DisposalOps.OnFailure(
                ButtonRows.Menu(menu.Menu, GlyphRole.Presets, IconSlot.PanelButton, RowText.Localize("Presets").Local, scope.Sink), IO.lift(menu.Release.Dispose))
            select new RowCells(button.Control, None, None, None, [], [], new RowHelp(None, None), IO.pure(unit), None, None,
                DisposalOps.Composite(Seq(button.Release, menu.Release), new CallbackSite(scope.Sink, typeof(PresetBinding), nameof(PresetBinding.Listing)))),
            RowText.Localize("Presets").Local, RowText.Localize("Stock looks of the effect and the presets saved for it").Local, RowRules.Always) { Wording = Wording.Localized };

    private static Seq<(string Caption, ValueSet Values)> Stocked<TState>(RowSource<TState> source) where TState : notnull =>
        source switch {
            RowSource<ToneMapping> tone => EffectStates.Partial(
                    toSeq(FilmPreset.Items), static preset => ToneMapping.Default with { Film = preset.State },
                    toSeq(ToneMappingParameter.Items).Filter(static item => item.Formations.Equals(Seq(Formation.Film))))
                .Map(look => (RowText.Localize(look.Item.Map(@default: "Default", nostalgia: "Nostalgia", silver: "Silver")).Local, Captured(tone.Owner, look.Look))),
            RowSource<WhiteBalanceState> balance => toSeq(StandardIlluminant.Items).Map(illuminant =>
                (RowText.Localize(Illuminated(illuminant)).Local, Captured(balance.Owner, new WhiteBalanceState(illuminant.Temperature, illuminant.Tint), Every<WhiteBalanceState>()))),
            _ => [],
        };

    private static string Illuminated(StandardIlluminant illuminant) =>
        illuminant.Map(
            a: "Illuminant A", b: "Illuminant B", c: "Illuminant C", d50: "Illuminant D50", d55: "Illuminant D55", d65: "Illuminant D65", d75: "Illuminant D75",
            d93: "Illuminant D93", e: "Illuminant E", f1: "Illuminant F1", f2: "Illuminant F2", f3: "Illuminant F3", f4: "Illuminant F4", f5: "Illuminant F5",
            f6: "Illuminant F6", f7: "Illuminant F7", f8: "Illuminant F8", f9: "Illuminant F9", f10: "Illuminant F10", f11: "Illuminant F11", f12: "Illuminant F12",
            ledB1: "Illuminant LED-B1", ledB2: "Illuminant LED-B2", ledB3: "Illuminant LED-B3", ledB4: "Illuminant LED-B4", ledB5: "Illuminant LED-B5",
            ledBh1: "Illuminant LED-BH1", ledRgb1: "Illuminant LED-RGB1", ledV1: "Illuminant LED-V1", ledV2: "Illuminant LED-V2");

    private static ValueSet Captured<TState, TParameter>(string owner, EffectLook<TState, TParameter> look)
        where TState : IStateRecord<TState>
        where TParameter : class, IStateParameter<TState> =>
        look.Switch(
            owner,
            whole: static (held, whole) => Captured(held, whole.Record, Every<TState>()),
            partial: static (held, partial) => Captured(held, partial.Record, partial.Keys.Map<IStateParameter<TState>>(static key => key)));

    private static ValueSet Captured<TRecord>(string owner, TRecord value, Seq<IStateParameter<TRecord>> writes) where TRecord : IStateRecord<TRecord> =>
        new(toHashMap(writes.Bind(FieldTexts.Of).Choose(field => field.Capture(value).Map(text => (new EntryKey(owner, field.Path), text)))));

    // --- [STORE]
    private static ValueStore<TState> Store<TState>(EffectState<TState> state, EtoPostEffectCollapsibleSection section, Func<RenderContent.ChangeContexts, IO<Unit>, IO<Unit>> changing)
        where TState : IStateRecord<TState> =>
        ValueStore.Of(
            IO.lift(() => Some(state.Current).Filter(static held => !EqualityComparer<TState>.Default.Equals(held, TState.Default))),
            next => changing(RenderContent.ChangeContexts.UI, Written(state, section, next.IfNone(TState.Default))),
            Applied.Live,
            Some(Changed(section)));

    private static ValueStore<Mix> Mixed<TState>(EffectState<TState> state, EtoPostEffectCollapsibleSection section, Func<RenderContent.ChangeContexts, IO<Unit>, IO<Unit>> changing)
        where TState : IStateRecord<TState> =>
        ValueStore.Of(
            IO.lift(() => state.Value.Amount.Filter(static held => held != EffectStates.Amount.Default)),
            next => changing(RenderContent.ChangeContexts.UI, IO.lift(() => Refused.Unless(
                section.SetParameter(EffectStates.Amount.Name, EffectStates.Amount.Text(next.IfNone(EffectStates.Amount.Default))), nameof(section.SetParameter)))),
            Applied.Live,
            Some(Changed(section)));

    private static HostEvent<Unit> Changed(EtoPostEffectCollapsibleSection section) =>
        ValueStore.Signal(
            Subscriptions.Host<global::Rhino.UI.Controls.DataSource.EventArgs>(
                typeof(EtoCollapsibleSection), handler => section.DataChanged += handler, handler => section.DataChanged -= handler, nameof(EtoCollapsibleSection.DataChanged)),
            static args => args.DataType == global::Rhino.UI.Controls.DataSource.ProviderIds.RdkRenderingPostEffects);

    private static IO<Unit> Written<TState>(EffectState<TState> state, EtoPostEffectCollapsibleSection section, TState next) where TState : IStateRecord<TState> =>
        IO.lift(() => Callbacks.Each(
            EffectStates.Captured(next).Filter(field => !state.GetParam(field.Key).Exists(held => string.Equals(held, field.Text, StringComparison.Ordinal))),
            field => section.SetParameter(field.Key, field.Text),
            nameof(section.SetParameter)));

    // --- [ITEMS]
    public static Seq<ControlRow> Of<TState, TParameter>(
        RowSource<TState> source, Seq<(TParameter Parameter, ParameterText Text)> texts, StageInput stage, Func<TParameter, RowRules> rules)
        where TState : IStateRecord<TState>
        where TParameter : class, IStateParameter<TState> =>
        texts.Bind(pair => pair.Parameter.Kind.Accept(new ItemRow<TState>(source, new RowField<TState>(pair.Parameter, pair.Text.Caption, pair.Text.Help), pair.Text, stage, rules(pair.Parameter))));

    public static Seq<ControlRow> Setting(PlugInSetting<bool, bool, InvalidRhinoValue> row, RowRules rules) =>
        Accepted(RowSource.Toggle(nameof(PlugInSettings), row.Value, scope => PlugInSettings.Store(((IPlugInViews)scope.Sink).Settings, row)), rules);

    public static Seq<ControlRow> Setting<TValue, TKey, TError>(PlugInSetting<TValue, TKey, TError> row, Presentation<TValue, TKey> presentation, RowRules rules)
        where TValue : IObjectFactory<TValue, TKey, TError>, IConvertible<TKey>, IMinMaxValue<TValue>
        where TKey : struct, INumber<TKey>
        where TError : Error, IValidationError<TError> =>
        Accepted(RowSource.Bounded(nameof(PlugInSettings), row.Value, presentation, scope => PlugInSettings.Store(((IPlugInViews)scope.Sink).Settings, row)), rules);

    public static Seq<ControlRow> Setting<TValue, TRaw, TError>(PlugInSetting<TValue, TRaw, TError> row, RowRules rules)
        where TValue : IObjectFactory<TValue, TRaw, TError>, IConvertible<TRaw>
        where TRaw : notnull, ISpanParsable<TRaw>
        where TError : Error, IValidationError<TError> =>
        Accepted(RowSource.Keyed(nameof(PlugInSettings), row.Value, scope => PlugInSettings.Store(((IPlugInViews)scope.Sink).Settings, row)), rules);

    public static Seq<ControlRow> Setting<TRaw>(PlugInSetting<TRaw, TRaw, InvalidRhinoValue> row, RowRules rules) where TRaw : notnull, ISpanParsable<TRaw> =>
        Accepted(RowSource.Raw(nameof(PlugInSettings), row.Value, scope => PlugInSettings.Store(((IPlugInViews)scope.Sink).Settings, row)), rules);

    public static Seq<ControlRow> Setting<TValue, TError>(PlugInSetting<Option<TValue>, string, TError> row, RowRules rules)
        where TValue : IObjectFactory<TValue, string, TError>, IConvertible<string>
        where TError : Error, IValidationError<TError> =>
        Accepted(RowSource.OptionalKeyed(nameof(PlugInSettings), row.Value, scope => PlugInSettings.Store(((IPlugInViews)scope.Sink).Settings, row)), rules);

    private static Seq<ControlRow> Accepted<T>((RowSource<T> Source, RowField<T> Field) pair, RowRules rules) where T : notnull =>
        pair.Field.Parameter.Kind.Accept(new ItemRow<T>(pair.Source, pair.Field, ParameterText.Of(pair.Field.Caption, pair.Field.Help), Unstaged, rules));

    private static Seq<ControlRow> Nested<TState, TNested>(
        RowSource<TState> source, Seq<string> path, Lens<TState, TNested> lens, HashMap<string, ParameterText> texts, StageInput stage, RowRules rules)
        where TState : notnull
        where TNested : IStateRecord<TNested> =>
        toSeq(TNested.Parameters).Choose(parameter => texts.Find(parameter.Key).Map(text => (Parameter: parameter, Text: text))).Strict() switch {
            var pairs => Of(source.Within(path, lens, pairs.Map(static pair => (pair.Parameter, pair.Text.Caption))), pairs, stage, _ => rules),
        };

    private static RowRules Showing<TState>(RowRules rules, RowSource<TState> source, IStateParameter<TState> parameter, Func<TState, bool> holds) where TState : notnull =>
        RowRule.When(source, holds, parameter) switch {
            var shown => rules with { Visible = Some(rules.Visible.Map(visible => visible & shown).IfNone(shown)) },
        };

    private sealed class ItemRow<TState>(RowSource<TState> source, RowField<TState> field, ParameterText text, StageInput stage, RowRules rules)
        : IStateParameterVisitor<TState, Seq<ControlRow>> where TState : notnull {
        public Seq<ControlRow> Bounded<TValue, TKey, TError>(Lens<TState, TValue> lens, Presentation<TValue, TKey> presentation)
            where TValue : IObjectFactory<TValue, TKey, TError>, IConvertible<TKey>, IMinMaxValue<TValue>
            where TKey : struct, INumber<TKey>
            where TError : Error, IValidationError<TError> =>
            [NumericRows.Bounded<TState, TValue, TKey, TError>(source, field, lens, presentation, rules)];

        public Seq<ControlRow> OptionalBounded<TValue, TKey, TError>(Lens<TState, Gated<TValue>> lens, Presentation<TValue, TKey> presentation)
            where TValue : IObjectFactory<TValue, TKey, TError>, IConvertible<TKey>, IMinMaxValue<TValue>
            where TKey : struct, INumber<TKey>
            where TError : Error, IValidationError<TError> =>
            [NumericRows.OptionalBounded<TState, TValue, TKey, TError>(source, field, lens, presentation, rules)];

        public Seq<ControlRow> Choice<TValue, TError>(Lens<TState, TValue> lens)
            where TValue : ISmartEnum<string, TValue, TError>
            where TError : Error, IValidationError<TError> =>
            [ChoiceRows.Choice(source, field, lens, scope => IO.pure(ChoiceRows.Items<TValue, TError>(item => text.Items[item.ToValue()].Caption, None, scope.Sink)), rules)];

        public Seq<ControlRow> OptionalChoice<TValue, TError>(Lens<TState, Option<TValue>> lens)
            where TValue : ISmartEnum<string, TValue, TError>
            where TError : Error, IValidationError<TError> =>
            [ChoiceRows.Choice(source, field, lens, scope => IO.pure(ChoiceRows.Optional(
                text.Absent.IfNone(field.Caption), ChoiceRows.Items<TValue, TError>(item => text.Items[item.ToValue()].Caption, None, scope.Sink), scope.Sink)), rules)];

        public Seq<ControlRow> Variant<TValue, TCase, TError>(Lens<TState, TValue> lens)
            where TValue : class
            where TCase : class, IStateCase<TValue>, ISmartEnum<string, TCase, TError>
            where TError : Error, IValidationError<TError> =>
            toSeq(TCase.Items).Map(item => (Item: item, Case: item.Accept(new Cased<TState, TValue>(
                source, field.Parameter, lens, item.ToValue(), text.Items[item.ToValue()].Parameters, stage, rules)))).Strict() switch {
                    var cases => ChoiceRows.Choice(
                            source, field,
                            Lens<TState, Option<TCase>>.New(
                                current => cases.Find(each => each.Case.Holds(lens.Get(current))).Map(static each => each.Item),
                                picked => current => picked.Bind(item => cases.Find(each => each.Item == item)).Filter(each => !each.Case.Holds(lens.Get(current)))
                                    .Match(Some: each => lens.Set(each.Case.Default, current), None: () => current)),
                            scope => IO.pure(ChoiceRows.Items<TCase, TError>(item => text.Items[item.ToValue()].Caption, None, scope.Sink)
                                .Map(static item => new ChoiceItem<Option<TCase>>(Some(item.Value), item.Caption, item.Glyph))), rules)
                        .Cons(cases.Bind(static each => each.Case.Rows)),
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

        public Seq<ControlRow> Color(Lens<TState, Swatch> lens) => [ColorRows.SwatchColor(source, field, lens, Some(stage), rules)];

        public Seq<ControlRow> OptionalColor(Lens<TState, Gated<Swatch>> lens) => [ColorRows.SwatchColor(source, field, lens, Some(stage), rules)];

        public Seq<ControlRow> Gradient(Lens<TState, Ramp> lens) =>
            [ColorRows.Gradient(source, field, lens, interpolation => text.Items[(string)interpolation].Caption, stage.Working, rules)];

        public Seq<ControlRow> Swatches<TValue, TError>(Lens<TState, TValue> lens)
            where TValue : IObjectFactory<TValue, Seq<Swatch>, TError>, IConvertible<Seq<Swatch>>, IObjectFactory<TValue, string, TError>, IConvertible<string>
            where TError : Error, IValidationError<TError> =>
            [ColorRows.Swatches<TState, TValue, TError>(source, field, lens, Some(stage), rules)];

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
        RowSource<TState> source, IStateParameter<TState> parameter, Lens<TState, TValue> lens, string key, HashMap<string, ParameterText> texts, StageInput stage, RowRules rules)
        : IStateCaseVisitor<TValue, (Func<TValue, bool> Holds, TValue Default, Seq<ControlRow> Rows)>
        where TState : notnull
        where TValue : class {
        public (Func<TValue, bool> Holds, TValue Default, Seq<ControlRow> Rows) Case<TChild>() where TChild : class, TValue, IStateRecord<TChild> =>
            (static value => value is TChild, TChild.Default,
             Nested(
                 source, Seq(parameter.Key, key),
                 Lens<TState, TChild>.New(current => lens.Get(current) as TChild ?? TChild.Default, child => current => lens.Set(child, current)),
                 texts, stage, Showing(rules, source, parameter, current => lens.Get(current) is TChild)));
    }

    // --- [BLOCKS]
    private static Seq<ControlRow> Blocks<TState, TParameter>(RowSource<TState> source, Seq<(TParameter Parameter, ParameterText Text)> texts, StageInput stage)
        where TState : IPixelStage<TState>
        where TParameter : class, IStateParameter<TState> =>
        (source, texts) switch {
            (RowSource<ToneMapping> tone, Seq<(ToneMappingParameter Parameter, ParameterText Text)> pairs) =>
                Of(tone, pairs, stage, item => Formed(tone, item.Formations))
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
                    static wheel => wheel.Map(@global: "Global", shadows: "Shadows", midtones: "Midtones", highlights: "Highlights"), stage.Working, RowRules.Always)]),
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
                        Some<Func<HueIsolation, Vector4, Vector4>>((kept, color) => kept.Graded(stage.Working.Luminance)(color)), stage, RowRules.Always),
                })),
            (RowSource<Levels> levels, Seq<(LevelsParameter Parameter, ParameterText Text)> pairs) =>
                Blocked(levels, pairs, stage, field => [CurveRows.LevelPoints(levels, field, "Levels", "Black and white points over the stage histogram", stage, RowRules.Always)]),
            (RowSource<CdlState> cdl, Seq<(CdlParameter Parameter, ParameterText Text)> pairs) =>
                Seq(Opened(cdl), Saved(cdl)) + Of(cdl, pairs, stage, static _ => RowRules.Always),
            _ => Of(source, texts, stage, static _ => RowRules.Always),
        };

    private static string Qualified(RangeAxis axis) => axis.Map(hue: "Hue range", saturation: "Saturation range", luma: "Luma range");

    private static Seq<ControlRow> Blocked<TRecord, TParameter>(
        RowSource<TRecord> source, Seq<(TParameter Parameter, ParameterText Text)> pairs, StageInput stage, Func<Func<TParameter, RowField<TRecord>>, Seq<ControlRow>> blocks)
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

    private static ControlRow.Readout Meter(RowSource<ExposureState> exposure, StageInput stage) =>
        new(RowShape.Inline, scope =>
            from mailbox in Subscriptions.Idle<Unit, Fin<ExposureState>>(metered => metered.Find(unit).Match(
                    Some: answer => IO.lift(answer).Bind(exposed => Committed(scope, exposure.Owner, exposed, [ExposureParameter.Exposure])),
                    None: static () => IO.pure(unit)))(new CallbackSite(scope.Sink, typeof(ExposureState), nameof(ExposureState.Metered)))
            from command in IO.lift(static () => new Command {
                MenuText = RowText.Localize("Meter").Local, ToolBarText = RowText.Localize("Meter").Local,
                ToolTip = RowText.Localize("Write the exposure that sets the stage input's trimmed mean luminance at middle grey").Local,
            })
            let site = new CallbackSite(scope.Sink, typeof(Command), nameof(Command.Executed))
            from executed in DisposalOps.OnFailure(
                Subscriptions.Attach(handler => command.Executed += handler, handler => command.Executed -= handler, Callbacks.Handler<EventArgs>(_ =>
                    from held in scope.Read(exposure)
                    from frame in stage.Read
                    from forked in frame.Match(
                        Some: read => IO.lift<Fin<ExposureState>>(() => held.Metered(read, stage.Working)).Bind(answer => mailbox.Post((unit, answer))).Fork().Map(static _ => unit),
                        None: static () => IO.pure(unit))
                    select forked, site)),
                IO.lift(mailbox.Release.Dispose))
            from button in ButtonRows.Push(command)
            select new RowCells(button, None, None, None, [], [], new RowHelp(None, None), IO.pure(unit), None, None, DisposalOps.Composite(Seq(executed, mailbox.Release), site)),
            RowText.Localize("Exposure meter").Local, RowText.Localize("Meters the stage input once and writes its exposure").Local, RowRules.Always) { Wording = Wording.Localized };

    private static Seq<IStateParameter<TRecord>> Every<TRecord>() where TRecord : IStateRecord<TRecord> => toSeq(TRecord.Parameters);

    private static IO<Unit> Committed<TRecord>(RowScope scope, string owner, TRecord value, Seq<IStateParameter<TRecord>> writes) where TRecord : IStateRecord<TRecord> =>
        from history in IO.lift(scope.History.ToFin(new Missing(nameof(RowScope.History))))
        from diff in scope.Commit(history.Group, Captured(owner, value, writes))
        select unit;

    // --- [CDL_FILES]
    private static ControlRow Opened(RowSource<CdlState> cdl) =>
        Pushed(new CommandRow.Run(
            new CommandFace("cdl-open", RowText.Localize("Open CDL...").Local, RowText.Localize("Read a correction from an ASC .cc, .ccc, or .cdl file").Local, None, None) {
                Wording = Wording.Localized,
            },
            None,
            scope => (
                from paths in HostDialogs.ShowOpenDialog(new global::Rhino.UI.OpenFileDialog { Title = RowText.Localize("Open CDL").Local, Filter = HostDialogs.Filter(Readable) })
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
                from written in Committed(scope, cdl.Owner, picked, Every<CdlState>())
                select written).Catch(static error => error.Is(Errors.Cancelled), static _ => IO.pure(unit))));

    private static ControlRow Saved(RowSource<CdlState> cdl) =>
        Pushed(new CommandRow.Run(
            new CommandFace("cdl-save", RowText.Localize("Save CDL...").Local, RowText.Localize("Write the correction as an ASC .cc, .ccc, or .cdl file").Local, None, None) {
                Wording = Wording.Localized,
            },
            None,
            scope => (
                from held in scope.Read(cdl)
                from chosen in HostDialogs.ShowSaveDialog(new global::Rhino.UI.SaveFileDialog {
                    Title = RowText.Localize("Save CDL").Local, Filter = HostDialogs.Filter(Corrections), DefaultExt = CdlFormat.Correction.Extension,
                })
                from target in IO.lift((Conversions.Validated<CdlFormat, string, InvalidGrade>(Path.GetExtension(chosen)).ToValidation(),
                        Conversions.Validated<OutputPath, string, InvalidOutput>(chosen).ToValidation())
                    .Apply(static (format, path) => (Format: format, Path: path)).As().ToFin())
                from written in CdlState.Save(held, None, target.Format, target.Path)
                select written).Catch(static error => error.Is(Errors.Cancelled), static _ => IO.pure(unit))));

    private static Seq<FileTypeRow> Corrections =>
        toSeq(CdlFormat.Items).Map(static format => new FileTypeRow(
            RhinoApp.CurrentRhinoId,
            RowText.Localize(format.Map(correction: "ASC color correction", collection: "ASC color correction collection", decisionList: "ASC color decision list")).Local,
            [format.Extension]));

    private static Seq<FileTypeRow> Readable =>
        new FileTypeRow(RhinoApp.CurrentRhinoId, RowText.Localize("ASC color corrections").Local, Corrections.Bind(static row => row.Extensions)).Cons(Corrections);

    public static ControlRow Pushed(CommandRow.Run run) =>
        new ControlRow.Command(run, RowShape.Unlabeled, scope =>
            from realized in run.Realize(scope)
            from button in DisposalOps.OnFailure(ButtonRows.Push(realized.Commands), IO.lift(realized.Release.Dispose))
            select new RowCells(button, None, None, None, [], [], new RowHelp(None, None), realized.Refresh, None, None, realized.Release), None);
}
