using System.Collections.Concurrent;
using System.Reflection;
using Grasshopper2.Components;
using Grasshopper2.Doc;
using Grasshopper2.Parameters;
using Grasshopper2.Parameters.Standard;
using Grasshopper2.UI.Icon;
using Grasshopper2.UI.InputPanel;
using Grasshopper2.UI.Toolbar;
using Grasshopper2.Undo;
using Grasshopper2.Undo.Actions;
using Rasm.Grasshopper.Doc;
using Rasm.Grasshopper.Parameters;
using Rasm.Imaging.Pixels;
using Rasm.Rhino;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Document.Units;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.Render.Settings;
using Rasm.Rhino.UI;
using Rasm.Rhino.UI.Assets;
using Rasm.Rhino.UI.Rows;
using Rhino;
using Rhino.Geometry.Intersect;
using UnitSystem = Rhino.UnitSystem;

namespace Rasm.Grasshopper.Components;

// --- [TYPES] ---------------------------------------------------------------------------
public interface IComponentDefinition<TSelf> where TSelf : ModularComponent {
    public static abstract ComponentRow<TSelf> Row { get; }
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ComponentRow<TSelf>(
    Nomen Face,
    IGlyph Glyph,
    int Master,
    SpecialColour Accent,
    Seq<ParameterRow> Inputs,
    Seq<ParameterRow> Outputs,
    Seq<Guid> SupportedPins,
    Func<TSelf, IDataAccess, IO<Unit>> Solve,
    Func<TSelf, Seq<ControlRow>> Panel,
    Option<Func<TSelf, Fin<IAttributes>>> Attributes)
    where TSelf : ModularComponent {
    // --- [HELP]
    public Option<HelpTopic> Help { get; init; }

    // --- [ATTACHMENT]
    public Func<TSelf, Action<IDisposable>, IO<Unit>> Attached { get; init; } = static (_, _) => IO.pure(unit);

    // --- [DECLARE]
    public static ComponentRow<TSelf> Of(
        Nomen face, IGlyph glyph, int master, SpecialColour accent, Seq<ParameterRow> inputs, Seq<ParameterRow> outputs, Seq<Guid> supportedPins,
        Func<TSelf, IDataAccess, IO<Unit>> solve) =>
        new(face, glyph, master, accent, inputs, outputs, supportedPins, solve, static _ => [], None);

    public static ComponentRow<TSelf> Of<TRecord>(
        Nomen face, IGlyph glyph, int master, SpecialColour accent, Seq<ParameterRow> inputs, Seq<ParameterField<TRecord>> fields, Seq<ParameterRow> outputs,
        Seq<Guid> supportedPins, TRecord fallback, Func<TSelf, IDataAccess, TRecord, IO<Unit>> solve)
        where TRecord : notnull =>
        Of(face, glyph, master, accent, inputs.Concat(fields.Map(static field => field.Input)).Strict(), outputs, supportedPins,
            (owner, access) => ParameterField.Read(fields, inputs.Count, access).Bind(set => solve(owner, access, set(fallback))));
}

public sealed record SolveUnits(Tolerances Tolerances, double UnitsPerMetre) {
    // --- [READ]
    public static IO<SolveUnits> Read(IDataAccess access) =>
        from native in IO.lift(() => {
            _ = access.GetTolerance(out double absolute, out double relative);
            _ = access.GetTolerance(out Angle angle);
            _ = access.GetUnitScaling(UnitSystem.Meters, out double unitsPerMetre);
            return (Absolute: absolute, Relative: relative, Angle: angle.Radians, UnitsPerMetre: unitsPerMetre);
        })
        from relative in IO.lift(Fin<double> () => double.IsNaN(native.Relative)
            ? Rhino.Missing.Unless(RhinoDoc.ActiveDoc, nameof(RhinoDoc.ActiveDoc)).Map(static document => document.ModelRelativeTolerance)
            : native.Relative)
        from tolerances in IO.lift(Fin<Tolerances> () => Tolerances.Validate(native.Absolute, relative, native.Angle, Intersection.MeshIntersectionsTolerancesCoefficient, out Tolerances? value) is { } error ? error : value!)
        select new SolveUnits(tolerances, native.UnitsPerMetre);

    // --- [LENGTHS]
    public Length From(double value) => Length.FromMeters(value / UnitsPerMetre);

    public double As(Length length) => length.Meters.ToDouble() * UnitsPerMetre;
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class SolveProgress(IDataAccess access, long totalRows, long completedRows = 0) : IProgress<int> {
    public void Report(int value) => access.SetProgress((int)(100 * ((Int128)completedRows + value) / totalRows));
}

public abstract class DefinedComponent<TSelf> : ModularComponent, IPinCushion where TSelf : DefinedComponent<TSelf>, IComponentDefinition<TSelf> {
    // --- [STATE]
    private readonly AtomSeq<Action<IDataAccess, EnvIO, Fin<Unit>>> matches = AtomSeq<Action<IDataAccess, EnvIO, Fin<Unit>>>();
    private readonly Ref<Option<(Action<IDisposable> Retain, IDisposable Lease)>> attachment =
        Ref(Option<(Action<IDisposable> Retain, IDisposable Lease)>.None);
    private readonly Ref<Option<Option<(Action<IDisposable> Retain, IDisposable Lease)>>> observations =
        Ref(Option<Option<(Action<IDisposable> Retain, IDisposable Lease)>>.None);

    // --- [CONSTRUCTION]
    protected DefinedComponent() : base(ComponentOps.Localized(TSelf.Row.Face, typeof(TSelf).Assembly)) => DocumentChanged += AttachmentChanged;

    protected DefinedComponent(GrasshopperIO.IReader reader) : base(reader) {
        DocumentChanged += AttachmentChanged;
        IParameter[] parameters = [.. Parameters.Inputs, .. Parameters.Outputs];
        foreach (AngleParameter angle in parameters.OfType<AngleParameter>())
            angle.ReduceAngles = angle.CustomValues.Get(nameof(AngleParameter.ReduceAngles), @default: false);
        foreach (VectorParameter vector in parameters.OfType<VectorParameter>()) {
            vector.UnitiseVectors = vector.CustomValues.Get(nameof(VectorParameter.UnitiseVectors), @default: false);
            vector.ReverseVectors = vector.CustomValues.Get(nameof(VectorParameter.ReverseVectors), @default: false);
        }
        CallbackSite site = DefinedGrasshopperPlugin.Site(this);
        Callbacks.Run(Callbacks.Each(
            TSelf.Row.Inputs.Map((row, index) => (Row: row, Parameter: Parameters.Input(index)))
                .Concat(TSelf.Row.Outputs.Map((row, index) => (Row: row, Parameter: Parameters.Output(index))))
                .Bind(placed => placed.Row.Attach.Map(attach => attach(placed.Parameter, site.Sink)).ToSeq())), site);
    }

    public IEnumerable<Guid> SupportedPins => TSelf.Row.SupportedPins;

    protected override IIcon IconInternal {
        get {
            CallbackSite site = DefinedGrasshopperPlugin.Site(this);
            return Callbacks.Answer(CanvasIcons.Accented(site.Sink, TSelf.Row.Glyph, TSelf.Row.Master, TSelf.Row.Accent), () => base.IconInternal, site);
        }
    }

    protected sealed override void AddInputs(ModularInputAdder inputs) =>
        Added(TSelf.Row.Inputs, (row, sink) => row.Input(inputs, sink), DefinedGrasshopperPlugin.Site(this));

    protected sealed override void AddOutputs(ModularOutputAdder outputs) =>
        Added(TSelf.Row.Outputs, (row, sink) => row.Output(outputs, sink), DefinedGrasshopperPlugin.Site(this));

    protected override IAttributes CreateAttributes() =>
        Callbacks.Answer(
            TSelf.Row.Attributes.Map(create => IO.lift(() => create((TSelf)this))),
            () => base.CreateAttributes(), () => base.CreateAttributes(), DefinedGrasshopperPlugin.Site(this));

    private static void Added(Seq<ParameterRow> rows, Func<ParameterRow, IPlugInSink, IO<IParameter>> add, CallbackSite site) =>
        Callbacks.Run(rows.TraverseM(row => add(row, site.Sink)).As(), site);

    // --- [ATTACHMENT]
    private void AttachmentChanged(object? sender, ObjectDocumentEventArgs e) =>
        Callbacks.Run(Callbacks.Each(Seq(
            Detached,
            when(Document is not null, DisposalOps.Acquire(
                retain =>
                    from attached in TSelf.Row.Attached((TSelf)this, retain)
                    from closing in DisposalOps.Acquire(
                        IO.lift(() => new Disposal<Unit>(unit, _ => Unobserved.Run())), static lease => IO.lift(lease.Dispose), retain)
                    from opened in atomicIO(() => observations.Value = Some(Option<(Action<IDisposable> Retain, IDisposable Lease)>.None))
                    select retain,
                (retain, lease) => atomic(() => attachment.Value = Some((retain, lease)))).Map(static _ => unit)).As())), DefinedGrasshopperPlugin.Site(this));

    private IO<Unit> Detached => atomicIO(() => {
        Option<(Action<IDisposable> Retain, IDisposable Lease)> held = attachment.Value;
        attachment.Value = None;
        return held;
    }).Bind(static held => held.Traverse(static scope => IO.lift(scope.Lease.Dispose)).As()).Map(static _ => unit);

    public IO<A> Retained<A>(Func<Action<IDisposable>, IO<A>> acquire) =>
        atomicIO(() => attachment.Value.Map(static scope => scope.Retain).ToFin(Errors.Cancelled))
            .Bind(static held => IO.lift(held)).Bind(acquire);

    private IO<Unit> Unobserved => atomicIO(() => {
        Option<Option<(Action<IDisposable> Retain, IDisposable Lease)>> held = observations.Value;
        observations.Value = None;
        return held.Bind(identity);
    }).Bind(static held => held.Traverse(static scope => IO.lift(scope.Lease.Dispose)).As()).Map(static _ => unit).Uninterruptible();

    protected override void PreProcess(Solution solution) {
        base.PreProcess(solution);
        Callbacks.Run(DisposalOps.Replace(
            static retain => IO.pure(retain), (retain, lease) => {
                Option<(Action<IDisposable> Retain, IDisposable Lease)> before = atomic(() => observations.Value.Match(
                    Some: held => { observations.Value = Some(Some((retain, lease))); return held; },
                    None: static () => throw Errors.Cancelled.ToException()));
                return before.Traverse(static held => IO.lift(held.Lease.Dispose)).As().Map(static _ => unit);
            }), DefinedGrasshopperPlugin.Site(this));
    }

    public IO<A> Solving<A>(Func<Action<IDisposable>, IO<A>> acquire) =>
        atomicIO(() => observations.Value.Bind(identity).Map(static held => held.Retain).ToFin(Errors.Cancelled))
            .Bind(static held => IO.lift(held)).Bind(acquire);

    public IO<Unit> Observe(Option<HostEvent<Unit>> source) => source.Match(
        Some: signal => Solving(retain => signal.Inline(_ => IO.lift(() => {
            Expire();
            return Optional(Document).Map(static document => document.Solution.Start());
        }).Map(static _ => unit), DefinedGrasshopperPlugin.Site(this).Sink, retain)),
        None: static () => IO.pure(unit));

    // --- [PERSISTENCE]
    public override void Store(GrasshopperIO.IWriter writer) {
        IParameter[] parameters = [.. Parameters.Inputs, .. Parameters.Outputs];
        foreach (AngleParameter angle in parameters.OfType<AngleParameter>())
            angle.CustomValues.Set(nameof(AngleParameter.ReduceAngles), angle.ReduceAngles);
        foreach (VectorParameter vector in parameters.OfType<VectorParameter>()) {
            vector.CustomValues.Set(nameof(VectorParameter.UnitiseVectors), vector.UnitiseVectors);
            vector.CustomValues.Set(nameof(VectorParameter.ReverseVectors), vector.ReverseVectors);
        }
        base.Store(writer);
    }

    // --- [SOLVE]
    protected sealed override void Process(IDataAccess access) {
        using EnvIO environment = EnvIO.New(token: access.Solution.Token);
        Fin<Unit> solved = Try.lift(() => TSelf.Row.Solve((TSelf)this, access).Run(environment)).Run();
        _ = matches.ToSeq().Iter(deliver => deliver(access, environment, solved));
        Seq<Error> failed = solved.Match(
            Succ: static _ => Seq<Error>(),
            Fail: static error => toSeq(error.AsIterable()).Filter(static leaf => !leaf.Is(Errors.Cancelled)).Strict());
        _ = failed.Iter(leaf => access.AddMessage(ComponentOps.Rendered(Message.Error, leaf, [])));
        _ = failed.Filter(static leaf => leaf.IsExceptional).Iter(leaf => DefinedGrasshopperPlugin.Site(this).Sink.Report(leaf, GetType(), nameof(Process)));
    }

    public IO<Seq<A>> Matched<A>(SolutionMode mode, Func<IDataAccess, IO<A>> read) =>
        from document in IO.lift(() => Rhino.Missing.Unless(Document, nameof(Document)))
        from captured in IO.lift(static () => new ConcurrentDictionary<(Solution Solution, int Index), Fin<A>>())
        from token in cancelToken
        from values in DisposalOps.Within<IDisposable, Seq<A>>(retain =>
            from source in DisposalOps.Acquire(
                IO.lift(() => CancellationTokenSource.CreateLinkedTokenSource(token)), static source => IO.lift(source.Dispose), retain)
            let requested = source.Token
            from collected in Subscriptions.Attach<Action<IDataAccess, EnvIO, Fin<Unit>>>(
                handler => _ = matches.Add(handler), handler => _ = matches.Swap(held => held.Filter(deliver => deliver != handler).Strict()), (access, environment, solved) => {
                    if (access.Solution.Token != requested) return;
                    Fin<A> value = Try.lift(() => read(access).Run(environment)).Run();
                    captured[(access.Solution, access.Index)] = solved.Match(
                        Succ: _ => value,
                        Fail: error => value.Match(Succ: _ => Fin.Fail<A>(error), Fail: fault => Fin.Fail<A>(error + fault)));
                }, retain)
            from expiring in Subscriptions.Attach<EventHandler<SolutionIdEventArgs>>(
                handler => document.Solution.SolutionAboutToStart += handler,
                handler => document.Solution.SolutionAboutToStart -= handler, (_, _) => Expire(), retain)
            from solution in Solutions.Start(document.Solution, source, mode)
            from parts in toSeq(captured.Where(row => ReferenceEquals(row.Key.Solution, solution)).OrderBy(static row => row.Key.Index))
                .Map<K<IO, A>>(static row => IO.lift(row.Value)).PartitionFallible().As()
            from failed in unless(parts.Fails.IsEmpty, IO.fail<Unit>(Error.Many(parts.Fails))).As()
            select parts.Succs, DisposalOps.Release)
        select values;

    // --- [PANEL]
    public override void AppendToInputPanel(InputPanel panel) {
        base.AppendToInputPanel(panel);
        CallbackSite site = DefinedGrasshopperPlugin.Site(this);
        Callbacks.Run(Appended(panel, site), site);
    }

    private IO<Unit> Appended(InputPanel panel, CallbackSite site) =>
        IO.lift(() => TSelf.Row.Help.Iter(topic =>
            panel.AddBar(drawCategoryLabels: false).AddPushButton(
                icon: null,
                new Nomen(RowText.Localize("Help"), RowText.Localize("Open help for this component.")),
                () => Callbacks.Run(topic.Show, site)).Style = IconTextStyle.Text))
        .Bind(_ => IO.lift(() => TSelf.Row.Panel((TSelf)this))).Bind(rows => unless(rows.IsEmpty,
              from document in IO.lift(static () => Rhino.Missing.Unless(RhinoDoc.ActiveDoc, nameof(RhinoDoc.ActiveDoc)))
              from scope in RowScope.Open(site.Sink, Some(document), new CommitMode.Immediate())
              from held in IO.lift(static () => AtomSeq<IDisposable>())
              from installed in DisposalOps.OnFailure(
                  from grid in RowGrid.Realize([], rows, scope, owned => _ = held.Add(owned))
                  from added in IO.lift(() => {
                      grid.UnLoad += Callbacks.Handler<EventArgs>(_ => IO.lift(grid.Dispose), site);
                      using IDisposable category = panel.BeginCategory(RowText.Localize("Settings"));
                      panel.Add(grid);
                  }).Uninterruptible()
                  select added,
                  IO.lift(held.ToSeq).Bind(DisposalOps.Release))
              select installed).As());
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ComponentOps {
    // --- [METADATA]
    public static Nomen Localized(Nomen face, Assembly assembly) =>
        face.WithChapter(string.IsNullOrEmpty(face.Chapter)
                ? Optional(assembly.GetCustomAttribute<AssemblyTitleAttribute>()).Map(static title => title.Title)
                    .Filter(static title => !string.IsNullOrWhiteSpace(title)).IfNone(() => Path.GetFileNameWithoutExtension(assembly.Location))
                : face.Chapter)
            .WithName(RowText.Localize(face.Name, table: assembly)).WithInfo(RowText.Localize(face.Info, table: assembly));

    // --- [COMPLETED_INPUTS]
    public static IO<Option<A>> Completed<A>(ModularComponent component, Func<ObjectSolutionState, Option<A>> read) =>
        IO.lift(() => Some(component.State).Filter(static state => state.Phase == Grasshopper2.Doc.Phase.Completed).Bind(read));

    public static Option<A> CurrentInput<A>(ModularComponent component, ObjectSolutionState state, int index) =>
        Optional(state.Data.Tree(component.Parameters.Input(index).InstanceId))
            .Bind(static tree => tree.NonNullItems.Cast<A>().Distinct().ToArray() is [var value] ? Some(value) : None);

    // --- [RENDER_SETTINGS]
    public static ComponentRow<TSelf> RenderSettings<TSelf, TRecord, TParameter>(Nomen face, IGlyph glyph, int master, SpecialColour accent)
        where TSelf : DefinedComponent<TSelf>, IComponentDefinition<TSelf>
        where TRecord : notnull, IRenderSettingsRecord<TRecord, TParameter>
        where TParameter : class, IStateParameter<TRecord>, ISmartEnum<string, TParameter, InvalidRhinoValue> {
        Seq<(TParameter Parameter, ParameterText Text)> texts = ParameterText.Join<TRecord, TParameter, InvalidRhinoValue>(TRecord.Text);
        Seq<ParameterField<TRecord>> fields = ParameterField.Join<TRecord, TParameter>(texts, static parameter => (parameter.ToValue(), None), glyph, master, accent);
        ParameterRow Data<T>(string caption, string code, string help, Requirement requirement, Seq<(string Caption, T Value)> presets = default) where T : notnull =>
            ParameterRow.Opaque<T>(new ParameterFace(ParameterText.Of(caption, help), code, None), glyph, master, accent,
                Access.Item, requirement, presets);
        ParameterRow State(string help, Requirement requirement) => ParameterRow.Of(
            new StateParameter<TRecord>.Record<TRecord>(Lens.identity<TRecord>()),
            new ParameterFace(ParameterText.Of("State", help), "S", None), glyph, master, accent, Access.Item, requirement, None);
        Seq<ParameterRow> inputs = Seq(
            Data("Document", "D", "Optional native document supplying the initial render settings and receiving applied settings.", Requirement.MayBeMissing,
                Seq<(string, DocumentSource)>(("Active document", new DocumentSource.Active()))),
            State("Optional complete state before individual field overrides.", Requirement.MayBeMissing),
            Data<bool>("Apply", "A", "Write the effective settings into the document.", Requirement.MayBeMissing));
        Seq<ParameterRow> outputs = Seq(
            State("The complete native render settings.", Requirement.MustExist),
            Data<ValueSet>("Values", "V", "The settings as typed store entries.", Requirement.MustExist));
        return ComponentRow<TSelf>.Of(face, glyph, master, accent,
            inputs.Concat(fields.Map(static field => field.Input)), outputs.Concat(fields.Map(static field => field.Output)), [],
            (owner, access) =>
                from document in IO.lift(() => Callbacks.Found(access.GetItem(0, out DocumentSource source), source))
                from supplied in IO.lift(() => Callbacks.Found(access.GetItem(1, out TRecord state), state))
                from apply in IO.lift(() => access.GetItem(2, out bool held) && held)
                from seed in supplied.Match(
                    Some: IO.pure,
                    None: () => document.Match(
                        Some: source => Documents.Read(source, static doc => RenderSettingsSources.Read(doc, TRecord.Read),
                            static doc => RenderSettingsSources.Signal(doc, TRecord.Changed), owner.Observe),
                        None: static () => IO.pure(TRecord.Default)))
                from set in ParameterField.Read(fields, inputs.Count, access)
                let effective = set(seed)
                from published in IO.lift(() => {
                    access.SetItem(0, effective);
                    access.SetItem(1, ValueSet.Fields(TRecord.Owner, effective, texts.Map<IStateParameter<TRecord>>(static row => row.Parameter)));
                })
                from written in ParameterField.Write(fields, outputs.Count, effective, access)
                from applied in document.Filter(_ => apply).Match(
                    Some: source => owner.Solving(retain => Subscriptions.Idle<Unit, Unit>(_ => Documents.WithDocument(source, doc =>
                            RenderSettingsSources.Store<TRecord>(doc).Write(effective).Map(static _ => unit)))(DefinedGrasshopperPlugin.Site(owner), retain))
                        .Bind(static post => post((unit, unit))),
                    None: static () => unitIO)
                select applied);
    }

    // --- [MESSAGES]
    public static Message Rendered(Func<string, string, MessageAction[], Message> level, Error note, Seq<MessageAction> actions) =>
        level(ErrorOps.Line(note), Conversions.Unset(note.Inner.Map(static cause => ErrorOps.Localize(cause))), [.. actions]);

    public static IO<Unit> Note(IDataAccess access, Func<string, string, MessageAction[], Message> level, Error note, Seq<MessageAction> actions) =>
        IO.lift(() => access.AddMessage(Rendered(level, note, actions)));

    // --- [STATE]
    public static ValueStore<TRecord> State<TRecord>(IDocumentObject owner, string key, TRecord fallback) where TRecord : IStateRecord<TRecord> {
        KeyedText text = KeyedText.Of(owner.CustomValues, key);
        return Recorded(owner, text.Pattern, GrasshopperStores.Of(text, fallback));
    }

    public static ValueStore<T> Recorded<T>(IDocumentObject owner, string filter, ValueStore<T> store) where T : notnull =>
        store with {
            Put = value => HistoryOps.Recorded(
                owner, new VerbNoun(RowText.Localize("Change"), owner.InstanceId.ToString()), AutoSaveReason.ObjectChanged,
                () => new CustomValueAction(owner, filter, Effects.ExpireSolution), store.Put(value), static change => change.HasChanged),
        };
}
