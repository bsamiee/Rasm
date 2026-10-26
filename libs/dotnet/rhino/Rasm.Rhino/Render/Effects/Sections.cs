using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using Rasm.Imaging.Files;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Grade.Curves;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Tone;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Settings;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.Render.Presets;
using Rasm.Rhino.Render.UI;
using Rasm.Rhino.UI;
using Rasm.Rhino.UI.Inputs;
using Rasm.Rhino.UI.Numeric;
using Rasm.Rhino.UI.Rows;
using Rhino;
using Rhino.Render.PostEffects;
using Rhino.UI;
using Rhino.UI.Controls;
using Rhino.UI.Controls.DataSource;
using UnitsNet;
using NativeDataSourceEventArgs = Rhino.UI.Controls.DataSource.EventArgs;

namespace Rasm.Rhino.Render.Effects;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record HistorySettingRows(PlugInSetting<int, int> Capacity, PlugInSetting<int, int> Window) {
    public IO<HistoryLimits> Limits(ISettingReader reader) =>
        IO.lift(() => (reader.Current(Capacity), reader.Current(Window))
            .Apply(static (capacity, seconds) => new HistoryLimits(capacity, Duration.FromSeconds(seconds)))
            .As());
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class DefinedEffectSection<TEffect> : EtoPostEffectCollapsibleSection, ICollapsibleSection, ICollapsibleSection2, ICollapsibleSection3 where TEffect : PostEffect {
    private static readonly Guid Effect = typeof(TEffect).GUID;

    private readonly PostEffectType stage;
    private readonly Guid id;
    private readonly View.Section row;
    private readonly IPlugInViews views;
    private readonly SectionLifetime lifetime;

    public DefinedEffectSection(PostEffect effect, Func<EtoPostEffectCollapsibleSection, View.Section> build, Option<Atom<StageFrame>> watched) {
        (views, row) = (IPlugInViews.Of(effect), build(this));
        (stage, id, Caption) = (effect.PostEffectType, row.Identity.GUID, RowText.Localize(row.Caption, views));
        lifetime = new SectionLifetime(this, views, Some(ProviderIds.RdkRenderingPostEffects), () => Excluded, visible: true, retain =>
            from watching in watched.Traverse(input => StageFrame.Watch(input, retain)).As()
            from document in IO.lift(() => (Optional(ViewModel?.GetData(ProviderIds.RdkRendering, bForWrite: false, bAutoChangeBracket: false))
                    .Bind(static rendering => Optional(RhinoDoc.FromRuntimeSerialNumber(SectionRows.RhinoDocumentId(rendering)))) | Optional(RhinoDoc.ActiveDoc))
                .ToFin(new Refused(nameof(RhinoDoc.ActiveDoc), Refused.Absent)))
            from scope in RowScope.Open(views, Some(document), new CommitMode.Immediate())
            from held in ViewBody.Realize(views, row, scope, retain)
            select held);
    }

    // --- [IDENTITY]
    public override Guid PostEffectId => Effect;
    public override string SettingsTag => id.ToString("D", CultureInfo.InvariantCulture);
    Guid ICollapsibleSection.Id => id;
    public override LocalizeStringPair Caption { get; }
    public override int SectionHeight => lifetime.Height;
    public override bool Hidden => Excluded || lifetime.Hidden;

    private bool Excluded =>
        !toSeq(GetPostEffects(stage)).Exists(listed => listed.Id == Effect && listed.IsSelected && (stage is PostEffectType.ToneMapping || listed.Shown));

    // --- [HEADER]
    IHeaderButtonHandler ICollapsibleSection2.NewHeaderButtonHandler() => lifetime.Header(row).ValueUnsafe()!;

    void ICollapsibleSection2.OnAttachingToHolder(ICollapsibleSectionHolder2 holder) { }

    void ICollapsibleSection2.OnAttachedToHolder(ICollapsibleSectionHolder2 holder) { }

    void ICollapsibleSection2.OnDetachingFromHolder(ICollapsibleSectionHolder2 holder) { }

    void ICollapsibleSection2.OnDetachedFromHolder(ICollapsibleSectionHolder2 holder) { }

    bool ICollapsibleSection2.EnableHeaderButton(int index, bool bEnable) => false;

    bool ICollapsibleSection2.ShowHeaderButton(int index, bool bShow) => false;

    void ICollapsibleSection3.UpdateView(uint flags) => Callbacks.Run(lifetime.Reread, new CallbackSite(views, typeof(DefinedEffectSection<TEffect>), nameof(ICollapsibleSection3.UpdateView)));

    // --- [RELEASE]
    protected override void Dispose(bool disposing) {
        try {
            if (disposing)
                lifetime.Dispose();
        } finally {
            base.Dispose(disposing);
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SectionRows {
    // --- [HOSTS]
    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "RhinoDocumentId")]
    internal static extern uint RhinoDocumentId([UnsafeAccessorType("Rhino.Render.Rendering, RhinoCommon")] object rendering);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "GetPostEffects")]
    internal static extern PostEffect[] GetPostEffects([UnsafeAccessorType("Rhino.Render.RenderingPostEffects, RhinoCommon")] object effects, PostEffectType type);

    // --- [EFFECTS]
    public static Func<EtoPostEffectCollapsibleSection, View.Section> Stage<TState, TParameter>(
        PostEffect effect, EffectState<TState> state, Seq<(string Name, EffectLook<TState, TParameter> Look)> looks, Atom<StageFrame> input)
        where TState : IPixelStage<TState>
        where TParameter : class, IStateParameter<TState> =>
        section => Row(effect, state, looks, section, source =>
            Seq(PreviewRows.Contextual(source, effect.LocalName, "Parameters over stage input",
                FramePicker.Current.Find(IPlugInViews.Of(effect)).Bind(picker => IO.lift(() => input.Value.Input.Map(held => new StageInput(IO.pure(Some(held.Frame)), held.Context, picker)))),
                [Subscriptions.Changed(input, nameof(input.Change)).Choose(static _ => Some(unit))],
                stage => Blocks(source, stage), RowRules.Always)) + Amount(state, section));

    public static Func<EtoPostEffectCollapsibleSection, View.Section> Job<TState, TParameter>(
        PostEffect effect, EffectState<TState> state, Seq<(string Name, EffectLook<TState, TParameter> Look)> looks, Atom<WorkState> work)
        where TState : IStateRecord<TState>
        where TParameter : class, IStateParameter<TState> =>
        section => Row(effect, state, looks, section, source =>
            Of(source, source.Fields, None, static _ => RowRules.Always)
                .Add(TextRows.Progress("Progress", "Last effect run", new ReadoutSource<WorkState>.Cell(work), RowRules.Always)));

    private static View.Section Row<TState, TParameter>(
        PostEffect effect, EffectState<TState> state, Seq<(string Name, EffectLook<TState, TParameter> Look)> looks,
        EtoPostEffectCollapsibleSection section, Func<RowSource<TState>, Seq<ControlRow>> rows)
        where TState : IStateRecord<TState>
        where TParameter : class, IStateParameter<TState> =>
        RowSource.Fields(state.Owner, TState.Parameters, _ => IO.pure(Seq(Store(state, section))), Some(state.Default.Record)) switch {
            var source => new View.Section {
                Identity = effect.GetType(),
                Caption = effect.LocalName,
                Store = Some<ViewStore>(new ViewStore.ApplicationStores()),
                Presets = Some(Presets(source, looks,
                    scope => new BindingGroup(Seq(source.Values(scope)) + state.Default.Amount.ToSeq().Map(factory =>
                        ValueBinding.Key(state.Owner, EffectStates.AmountKey(factory), Mixed(state, section, factory)))))),
                Children = [.. rows(source)],
            },
        };

    private static Seq<ControlRow> Amount<TState>(EffectState<TState> state, EtoPostEffectCollapsibleSection section)
        where TState : IStateRecord<TState> =>
        state.Default.Amount.ToSeq().Bind(factory => Accepted(RowSource.Keyed(state.Owner, EffectStates.AmountKey(factory), _ => Mixed(state, section, factory)), RowRules.Always));

    // --- [SCOPES]
    public static IO<Option<ScopeFrames>> Scoped<TState>(PostEffectType stage, EffectState<TState> state, Atom<StageFrame> input) where TState : IPixelStage<TState> =>
        Atom(Option<(PixelFrame Input, EffectValue<TState> Value, ScopeFrames Frames)>.None) switch {
            var formed => IO.lift(() => input.Value.Input.Map(held => (held.Frame, held.Context, state.Value))).Bind(current => current.Traverse(now =>
                formed.Value.Filter(prior => ReferenceEquals(prior.Input, now.Frame) && prior.Value == now.Value).Match(
                    Some: static prior => IO.pure(prior.Frames),
                    None: () =>
                        from frames in Formed(stage, now.Frame, now.Context, now.Value)
                        from kept in formed.SwapIO(_ => Some((now.Frame, now.Value, frames)))
                        select frames)).As()),
        };

    private static IO<ScopeFrames> Formed<TState>(PostEffectType stage, PixelFrame input, PassContext context, EffectValue<TState> value) where TState : IPixelStage<TState> =>
        EffectPipeline.Staged(value, context) switch {
            var pass =>
                from formed in IO.lift(() => pass.IsSome ? input.Copy() : input)
                from ran in pass.Traverse(staged => IO.lift(() => staged.Run(formed, new Progress<int>()))).As()
                let displayed = stage is not PostEffectType.Early
                select new ScopeFrames(
                    formed,
                    displayed ? context.Display.Encoding.Gamut : context.Working,
                    stage is PostEffectType.ToneMapping ? context.Display.Encoding.Transfer : context.Signal,
                    context.Depth.Levels.Filter(_ => displayed).Map(static levels => (float)levels.MaxCode),
                    context.Depth.Levels.Filter(_ => displayed).Map(static levels => levels.Clipping),
                    Some((input, context.Working))),
        };

    // --- [LOOKS]
    public static Func<RowScope, IO<Seq<MenuPick>>> Presets<TState, TParameter>(
        RowSource<TState> source, Seq<(string Name, EffectLook<TState, TParameter> Look)> looks, Func<RowScope, BindingGroup> bindings)
        where TState : IStateRecord<TState>
        where TParameter : class, IStateParameter<TState> =>
        scope => new PresetBinding(scope, bindings(scope), source.Owner, looks.Map(look => new Preset(PresetName.Create(look.Name), look.Look.Capture(source.Owner)))).Listing();

    // --- [STORE]
    private static ValueStore<TState> Store<TState>(EffectState<TState> state, EtoPostEffectCollapsibleSection section)
        where TState : IStateRecord<TState> =>
        new(
            IO.lift(() => Some(state.Current).Filter(held => !EqualityComparer<TState>.Default.Equals(held, state.Default.Record))),
            next => state.Write(next.IfNone(state.Default.Record), SetParameter(section)),
            EqualityComparer<TState>.Default.Equals,
            Applied.Live,
            Some(Changed(section)));

    private static ValueStore<AxisFraction> Mixed<TState>(EffectState<TState> state, EtoPostEffectCollapsibleSection section, AxisFraction factory)
        where TState : IStateRecord<TState> =>
        new(
            IO.lift(() => state.Value.Amount.Filter(held => held != factory)),
            next => state.Write(next.IfNone(factory), SetParameter(section)),
            EqualityComparer<AxisFraction>.Default.Equals,
            Applied.Live,
            Some(Changed(section)));

    private static Func<string, string, Fin<Unit>> SetParameter(EtoPostEffectCollapsibleSection section) =>
        (key, text) => Try.lift(() => section.SetParameter(key, text)).Run()
            .Bind(static accepted => Refused.Unless(accepted, nameof(EtoPostEffectCollapsibleSection.SetParameter)));

    internal static HostEvent<Unit> Changed(EtoPostEffectCollapsibleSection section) =>
        ValueStore.Signal(
            Subscriptions.Host<NativeDataSourceEventArgs>(
                typeof(EtoCollapsibleSection), handler => section.DataChanged += handler, handler => section.DataChanged -= handler, nameof(EtoCollapsibleSection.DataChanged)),
            static args => args.DataType == ProviderIds.RdkRenderingPostEffects);

    // --- [ITEMS]
    public static Seq<ControlRow> Of<TState, TParameter>(
        RowSource<TState> source, Seq<TParameter> parameters, Option<StageInput> stage, Func<TParameter, RowRules> rules)
        where TState : IStateRecord<TState>
        where TParameter : class, IStateParameter<TState> =>
        parameters.Bind(parameter => parameter.Kind.Accept(new ItemRow<TState>(source, new RowField<TState>(parameter), stage, rules(parameter))));

    public static Seq<ControlRow> Setting<TValue, TRaw>(PlugInSetting<TValue, TRaw> row, RowRules rules)
        where TValue : notnull
        where TRaw : notnull =>
        Accepted(Setting(row), rules);

    public static (RowSource<TValue> Source, RowField<TValue> Field) Setting<TValue, TRaw>(PlugInSetting<TValue, TRaw> row)
        where TValue : notnull
        where TRaw : notnull =>
        RowSource.Keyed(nameof(PlugInSettings), row.Value, scope => PlugInSettings.Store(((IPlugInViews)scope.Sink).Settings, row)) switch {
            var (source, field) => (source with { Setting = Some<PlugInSetting>(row) }, field),
        };

    private static Seq<ControlRow> Accepted<T>((RowSource<T> Source, RowField<T> Field) pair, RowRules rules) where T : notnull =>
        pair.Field.Parameter.Kind.Accept(new ItemRow<T>(pair.Source, pair.Field, None, rules));

    private static Seq<ControlRow> Nested<TState, TNested>(
        RowSource<TState> source, Seq<string> path, Lens<TState, TNested> lens, Option<StageInput> stage, RowRules rules)
        where TState : notnull
        where TNested : IStateRecord<TNested> =>
        Of(source.Within(path, lens, TNested.Parameters), TNested.Parameters, stage, _ => rules);

    private static RowRules Showing<TState>(RowRules rules, RowSource<TState> source, IStateParameter<TState> parameter, Func<TState, bool> holds) where TState : notnull =>
        RowRule.When(source, holds, parameter) switch {
            var shown => rules with { Visible = Some(rules.Visible.Map(visible => visible & shown).IfNone(shown)) },
        };

    private sealed class ItemRow<TState>(RowSource<TState> source, RowField<TState> field, Option<StageInput> stage, RowRules rules)
        : IStateParameterVisitor<TState, Seq<ControlRow>> where TState : notnull {
        public Seq<ControlRow> Numeric<TValue, TKey>(Lens<TState, TValue> lens, NumericDomain<TValue, TKey> domain)
            where TValue : notnull
            where TKey : struct, INumber<TKey> =>
            [NumericRows.Numeric(source, field, lens, domain, rules)];

        public Seq<ControlRow> OptionalNumeric<TValue, TKey>(Lens<TState, Gated<TValue>> lens, NumericDomain<TValue, TKey> domain)
            where TValue : notnull
            where TKey : struct, INumber<TKey> =>
            [NumericRows.OptionalNumeric(source, field, lens, domain, rules)];

        public Seq<ControlRow> Choice<TValue>(Lens<TState, TValue> lens)
            where TValue : ISmartEnum<string, TValue, ValidationError>, ICaptioned =>
            [ChoiceRows.Choice(source, field, lens, static scope => IO.pure(ChoiceRows.Items<TValue>(None, scope.Sink)), rules)];

        public Seq<ControlRow> OptionalChoice<TValue>(Lens<TState, Option<TValue>> lens, string absent)
            where TValue : ISmartEnum<string, TValue, ValidationError>, ICaptioned =>
            [ChoiceRows.Choice(source, field, lens, scope => IO.pure(ChoiceRows.Optional(absent, ChoiceRows.Items<TValue>(None, scope.Sink), scope.Sink)), rules)];

        public Seq<ControlRow> Variant<TValue>(Lens<TState, TValue> lens) where TValue : class =>
            StateCase<TValue>.Items.Map(item => (Item: item, Rows: item.Accept(new Cased<TState, TValue>(source, field.Parameter, lens, item.Key, stage, rules)))).Strict() switch {
                var cases => ChoiceRows.Choice(
                        source, field,
                        Lens<TState, StateCase<TValue>>.New(
                            current => StateCase<TValue>.Items.First(item => item.Holds(lens.Get(current))),
                            picked => current => picked.Holds(lens.Get(current)) ? current : lens.Set(picked.Default, current)),
                        scope => IO.pure(cases.Map(each => new ChoiceItem<StateCase<TValue>>(
                            each.Item, RowText.Localize(each.Item.Caption, scope.Sink).Local, None))), rules)
                    .Cons(cases.Bind(static each => each.Rows)),
            };

        public Seq<ControlRow> Enumerated<TEnum>(Lens<TState, TEnum> lens, Func<TEnum, string> caption) where TEnum : struct, Enum =>
            [ChoiceRows.Choice(source, field, lens, scope => IO.pure(ChoiceRows.Members(caption, scope.Sink)), rules)];

        public Seq<ControlRow> Record<TNested>(Lens<TState, TNested> lens) where TNested : IStateRecord<TNested> =>
            new ControlRow.Group(field.Caption, rules).Cons(Nested(source, Seq(field.Parameter.Key), lens, stage, rules));

        public Seq<ControlRow> OptionalRecord<TNested>(Lens<TState, Gated<TNested>> lens) where TNested : IStateRecord<TNested> =>
            ToggleRows.Check(source, field, Prelude.lens(lens, Gated<TNested>.EnabledEntry.Lens), rules).Cons(Nested(
                source, Seq(field.Parameter.Key, Gated<TNested>.ValueEntry.Key), Prelude.lens(lens, Gated<TNested>.ValueEntry.Lens), stage,
                Showing(rules, source, field.Parameter, current => lens.Get(current).Enabled)));

        public Seq<ControlRow> Toggle(Lens<TState, bool> lens) => [ToggleRows.Check(source, field, lens, rules)];

        public Seq<ControlRow> Raw<TRaw>(Lens<TState, TRaw> lens) where TRaw : notnull, ISpanParsable<TRaw> =>
            [TextRows.Field(source, field, new EntryForm.SingleLine(), None, rules)];

        public Seq<ControlRow> OptionalRaw<TRaw>(Lens<TState, Option<TRaw>> lens) where TRaw : notnull, ISpanParsable<TRaw> =>
            [TextRows.Field(source, field, new EntryForm.SingleLine(), None, rules)];

        public Seq<ControlRow> Color(Lens<TState, Swatch> lens) => [ColorRows.SwatchColor(source, field, lens, stage, rules)];

        public Seq<ControlRow> OptionalColor(Lens<TState, Gated<Swatch>> lens) => [ColorRows.SwatchColor(source, field, lens, stage, rules)];

        public Seq<ControlRow> Gradient(Lens<TState, Ramp> lens) =>
            [stage.Match(
                Some: input => ColorRows.Gradient(source, field, lens, input.Context.Working, rules),
                None: () => TextRows.Field(source, field, new EntryForm.SingleLine(), None, rules))];

        public Seq<ControlRow> Swatches<TValue>(Lens<TState, TValue> lens)
            where TValue : IObjectFactory<TValue, Seq<Swatch>, ValidationError>, IConvertible<Seq<Swatch>>, IObjectFactory<TValue, string, ValidationError>, IConvertible<string> =>
            [ColorRows.Swatches(source, field, lens, stage, rules)];

        public Seq<ControlRow> Keyed<TValue, TRaw>(Lens<TState, TValue> lens)
            where TValue : IObjectFactory<TValue, TRaw, ValidationError>, IConvertible<TRaw>
            where TRaw : notnull, ISpanParsable<TRaw> =>
            [TextRows.Field(source, field, new EntryForm.SingleLine(), None, rules)];

        public Seq<ControlRow> OptionalKeyed<TValue, TRaw>(Lens<TState, Option<TValue>> lens)
            where TValue : IObjectFactory<TValue, TRaw, ValidationError>, IConvertible<TRaw>
            where TRaw : notnull, ISpanParsable<TRaw> =>
            [TextRows.Field(source, field, new EntryForm.SingleLine(), None, rules)];

        public Seq<ControlRow> Loaded<TValue, TKey, TRaw>(Lens<TState, Option<TValue>> lens, Func<TKey, IO<TValue>> load, Func<TValue, TKey> key)
            where TKey : IObjectFactory<TKey, TRaw, ValidationError>, IConvertible<TRaw>
            where TRaw : notnull, ISpanParsable<TRaw> =>
            [TextRows.Field(source, field, new EntryForm.SingleLine(), Some((Eto.FileAction.OpenFile, Seq<FileTypeRow>())), rules)];

        public Seq<ControlRow> Opaque<TValue>(Lens<TState, TValue> lens) where TValue : notnull => throw new UnreachableException();
    }

    private sealed class Cased<TState, TValue>(
        RowSource<TState> source, IStateParameter<TState> parameter, Lens<TState, TValue> lens, string key, Option<StageInput> stage, RowRules rules)
        : IStateCaseVisitor<TValue, Seq<ControlRow>>
        where TState : notnull
        where TValue : class {
        public Seq<ControlRow> Case<TChild>() where TChild : class, TValue, IStateRecord<TChild> =>
            Nested(
                source, Seq(parameter.Key, key),
                Lens<TState, TChild>.New(current => lens.Get(current) as TChild ?? TChild.Default, child => current => lens.Set(child, current)),
                stage, Showing(rules, source, parameter, current => lens.Get(current) is TChild));
    }

    // --- [BLOCKS]
    public static Seq<ControlRow> Blocks<TState>(RowSource<TState> source, Option<StageInput> stage) where TState : IPixelStage<TState> =>
        source switch {
            RowSource<ToneMapping> tone =>
                stage.Map(input => CurveRows.Graph(tone, "Response", "Display code over scene stops", input, RowRules.Always)).ToSeq()
                    + Of(tone, toSeq(ToneMappingParameter.Items), stage, item => Formed(tone, item.Formations))
                    .Add(ChartRows.RampKey(
                        "False color key", "Stops above middle gray per false color band", scope => scope.Read(tone).Map(static held => held.Look),
                        Formed(tone, [Formation.AgXFalseColor]))),
            RowSource<ExposureState> exposure =>
                Meter(exposure, stage).Cons(Of(exposure, exposure.Fields, stage, static _ => RowRules.Always)),
            RowSource<PointCurves> curves =>
                Blocked(curves, stage, [CurveRows.Points(curves, "Curves", "Point curves over stage input", stage, RowRules.Always)]),
            RowSource<HueCurves> hues =>
                Blocked(hues, stage, [CurveRows.Hues(hues, "Hue curves", "Hue curves over stage input", stage, RowRules.Always)]),
            RowSource<WheelsState> wheels =>
                Blocked(wheels, stage, [ColorRows.Wheels(
                    wheels, "Color wheels", "Tints over global, shadow, midtone, and highlight ranges", stage.Map(static input => input.Context.Working), RowRules.Always)]),
            RowSource<SelectiveColor> selective =>
                Blocked(selective, stage, toSeq(RangeAxis.Items).Map(axis => ColorRows.Qualifier(
                    selective, Qualified(axis), "Stage input band to grade", axis, SelectiveColor.RangeOf, new RowField<SelectiveColor>(SelectiveColorParameter.Range),
                    Some<Func<SelectiveColor, Vector4, Vector4>>(static (graded, color) => graded.Graded()(color)), stage, RowRules.Always))),
            RowSource<HueIsolation> isolation =>
                Blocked(isolation, stage, toSeq(RangeAxis.Items).Map(axis => ColorRows.Qualifier(
                    isolation, Qualified(axis), "Stage input band kept in color", axis, HueIsolation.RangeOf, new RowField<HueIsolation>(HueIsolationParameter.Range),
                    stage.Map<Func<HueIsolation, Vector4, Vector4>>(static input => (kept, color) => kept.Graded(input.Context.Working.Luminance)(color)), stage, RowRules.Always))),
            RowSource<Levels> levels =>
                Blocked(levels, stage, [CurveRows.LevelPoints(levels, "Levels", "Black and white points over stage histogram", stage, RowRules.Always)]),
            RowSource<CdlState> cdl =>
                Seq(Opened(cdl), Saved(cdl)) + Of(cdl, cdl.Fields, stage, static _ => RowRules.Always),
            _ => Of(source, source.Fields, stage, static _ => RowRules.Always),
        };

    private static string Qualified(RangeAxis axis) => axis.Map(hue: "Hue range", saturation: "Saturation range", luma: "Luma range");

    private static Seq<ControlRow> Blocked<TRecord>(RowSource<TRecord> source, Option<StageInput> stage, Seq<ControlRow> built) where TRecord : IStateRecord<TRecord> =>
        toHashSet(built.Bind(static block => block.Keys)) switch {
            var covered => built + Of(source, source.Fields.Filter(parameter => !source.Keys(parameter).Exists(covered.Contains)), stage, static _ => RowRules.Always),
        };

    // --- [TONE_MAPPING]
    private static RowRules Formed(RowSource<ToneMapping> tone, Seq<Formation> formations) =>
        new(None, Some(RowRule.When(tone, held => formations.Exists(formation => formation == held.Selected), ToneMappingParameter.Formation)));

    private static ControlRow.Field Meter(RowSource<ExposureState> exposure, Option<StageInput> stage) =>
        new(exposure, exposure.Fields.Bind(exposure.Keys), RowShape.Inline, (scope, retain) =>
            from mailbox in Subscriptions.Idle<Unit, Fin<ExposureState>>(metered => metered.Find(unit).Match(
                    Some: answer => IO.lift(answer).Bind(exposed => Committed(scope, exposure, exposed, [ExposureParameter.Exposure])),
                    None: static () => IO.pure(unit)))(new CallbackSite(scope.Sink, typeof(ExposureState), nameof(ExposureState.Metered)), retain)
            from command in IO.lift(static () => new Eto.Forms.Command {
                MenuText = RowText.Localize("Meter").Local, ToolBarText = RowText.Localize("Meter").Local,
                ToolTip = RowText.Localize("Write exposure placing trimmed mean luminance at middle gray").Local,
            })
            let show = stage.Match(Some: static input => input.Read, None: static () => IO.pure(Option<PixelFrame>.None))
                .Bind(available => IO.lift(() => command.Enabled = available.IsSome)).Map(unit)
            let site = new CallbackSite(scope.Sink, typeof(Eto.Forms.Command), nameof(Eto.Forms.Command.Executed))
            from executed in Subscriptions.Attach(handler => command.Executed += handler, handler => command.Executed -= handler, Callbacks.Handler<System.EventArgs>(_ => Metered(scope, exposure, stage, mailbox, retain), site), retain)
            from button in ButtonRows.Push(command, None, scope.Sink, retain)
            select new RowCells(button, None, None, None, [], [], new RowHelp(None, None), show, None, None),
            "Exposure meter", "Meters stage input once and writes exposure", RowRules.Always);

    private static IO<Unit> Metered(
        RowScope scope, RowSource<ExposureState> exposure, Option<StageInput> stage, Func<(Unit Key, Fin<ExposureState> Value), IO<Unit>> mailbox, Action<IDisposable> retain) =>
        from held in scope.Read(exposure)
        from forked in stage.Traverse(input => input.Read.Bind(frame => frame.Traverse(read =>
            DisposalOps.Fork(IO.lift<Fin<ExposureState>>(() => held.Metered(read, input.Context.Working)).Bind(answer => mailbox((unit, answer))), retain)).As())).As()
        select unit;

    private static IO<Unit> Committed<TRecord>(RowScope scope, RowSource<TRecord> source, TRecord value, Seq<IStateParameter<TRecord>> writes) where TRecord : IStateRecord<TRecord> =>
        scope.Commit(new BindingGroup(Seq(source.Values(scope))), ValueSet.Fields(source.Owner, value, writes)).Map(unit);

    // --- [CDL_FILES]
    private static ControlRow Opened(RowSource<CdlState> cdl) =>
        Pushed(new CommandRow.Run(
            new CommandFace("cdl-open", "Open CDL...", "Read correction from ASC .cc, .ccc, or .cdl file", None, None),
            None,
            scope => (
                from paths in HostDialogs.ShowOpenDialog(new OpenFileDialog {
                    Title = RowText.Localize("Open CDL").Local,
                    Filter = HostDialogs.Filter([(RowText.Localize("ASC color corrections").Local, Corrections.Bind(static row => row.Extensions)), .. Corrections]),
                })
                from path in IO.lift(paths.Head.ToFin(Errors.Cancelled))
                from text in IO.liftAsync(env => File.ReadAllTextAsync(path, env.Token))
                from read in IO.lift(CdlState.Read(text))
                let corrections = read.Map(static (correction, index) => new Captioned<CdlState>(correction.State, correction.Id.Match(
                    Some: static id => new LocalizeStringPair(id, id),
                    None: () => RowText.Localize("Correction {0}", arguments: [index + 1]))))
                from picked in read.Tail.Any()
                    ? HostDialogs.ShowListBox(RowText.Localize("Open CDL"), RowText.Localize("Correction to open"), corrections, None)
                    : IO.pure(corrections.Head.Value)
                from written in Committed(scope, cdl, picked, cdl.Fields)
                select written).Catch(static error => error.Is(Errors.Cancelled), static _ => IO.pure(unit))));

    private static ControlRow Saved(RowSource<CdlState> cdl) =>
        Pushed(new CommandRow.Run(
            new CommandFace("cdl-save", "Save CDL...", "Write correction as ASC .cc, .ccc, or .cdl file", None, None),
            None,
            scope => (
                from held in scope.Read(cdl)
                from chosen in HostDialogs.ShowSaveDialog(new SaveFileDialog {
                    Title = RowText.Localize("Save CDL").Local, Filter = HostDialogs.Filter(Corrections), DefaultExt = CdlFormat.Correction.Extension,
                })
                from target in IO.lift((CdlFormat.Validated(Path.GetExtension(chosen)), OutputPath.Validated(chosen))
                    .Apply(static (format, path) => (Format: format, Path: path)).As())
                from written in FilePublication.Write(target.Path, CdlState.Write(held, None, target.Format))
                select written).Catch(static error => error.Is(Errors.Cancelled), static _ => IO.pure(unit))));

    private static Seq<(string Description, Seq<string> Extensions)> Corrections =>
        toSeq(CdlFormat.Items).Map<(string Description, Seq<string> Extensions)>(static format => (
            RowText.Localize(format.Map(correction: "ASC color correction", collection: "ASC color correction collection", decisionList: "ASC color decision list")).Local,
            [format.Extension]));

    public static ControlRow Pushed(CommandRow.Run run) =>
        new ControlRow.Readout(RowShape.Unlabeled, (scope, retain) =>
            from realized in run.Realize(scope, retain)
            from button in ButtonRows.Push(realized.Commands, None, scope.Sink, retain)
            select new RowCells(button, None, None, None, [], [], new RowHelp(None, None), realized.Refresh, None, None),
            run.Face.Caption, run.Face.Help, new RowRules(run.Enabled, None));
}
