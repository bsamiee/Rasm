using System.Collections.Concurrent;
using Grasshopper2.Components;
using Grasshopper2.Doc;
using Grasshopper2.Parameters;
using Grasshopper2.Parameters.Standard;
using Grasshopper2.UI.Icon;
using Grasshopper2.UI.InputPanel;
using Grasshopper2.Undo;
using Grasshopper2.Undo.Actions;
using Rasm.Grasshopper.Doc;
using Rasm.Grasshopper.Parameters;
using Rasm.Imaging.Pixels;
using Rasm.Rhino;
using Rasm.Rhino.Document.Units;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Stores;
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
    Option<Rasm.UI.GlyphSource> Glyph,
    Seq<ParameterRow> Inputs,
    Seq<ParameterRow> Outputs,
    Seq<Guid> SupportedPins,
    Func<TSelf, IDataAccess, IO<Unit>> Solve)
    where TSelf : ModularComponent {
    // --- [PRESENTATION]
    public Func<TSelf, Seq<ControlRow>> Panel { get; init; } = static _ => [];
    public Option<Func<TSelf, Fin<IAttributes>>> Attributes { get; init; }

    // --- [ATTACHMENT]
    public Seq<Func<TSelf, Action<IDisposable>, IO<Unit>>> Attached { get; init; }

    // --- [DECLARE]
    public static ComponentRow<TSelf> Of<TRecord>(
        Nomen face, Option<Rasm.UI.GlyphSource> glyph, Seq<ParameterRow> inputs, Seq<ParameterField<TRecord>> fields, Seq<ParameterRow> outputs,
        Seq<Guid> supportedPins, TRecord fallback, Func<TSelf, IDataAccess, TRecord, IO<Unit>> solve)
        where TRecord : notnull =>
        new(face, glyph, [.. inputs, .. fields.Map(static field => field.Input)], outputs, supportedPins,
            (owner, access) => ParameterField.Read(fields, inputs.Count, access).Bind(set => solve(owner, access, set(fallback))));
}

public sealed record SolveUnits(Tolerances Tolerances, double UnitsPerMeter) {
    // --- [READ]
    public static IO<SolveUnits> Read(IDataAccess access) =>
        from native in IO.lift(Fin<(double Absolute, double Relative, double Angle, double UnitsPerMeter)> () => {
            _ = access.GetTolerance(out double absolute);
            _ = access.GetTolerance(out Angle angle);
            _ = access.GetUnitScaling(UnitSystem.Meters, out double unitsPerMeter);
            return (access.GetItem(RelativeTolerancePin.Id, out double pinned) && pinned >= 0d
                    ? pinned
                    : Refused.UnlessFound(RhinoDoc.ActiveDoc, nameof(RhinoDoc.ActiveDoc)).Map(static document => document.ModelRelativeTolerance))
                .Map(relative => (Absolute: absolute, Relative: Math.Clamp(relative, 1e-12, 1e6), Angle: angle.Radians, UnitsPerMeter: unitsPerMeter));
        })
        from tolerances in IO.lift(() => ValidationResults.ToFin(Tolerances.Validate(native.Absolute, native.Relative, native.Angle, Intersection.MeshIntersectionsTolerancesCoefficient, out Tolerances? value), value))
        select new SolveUnits(tolerances, native.UnitsPerMeter);

    // --- [LENGTHS]
    public Length From(double value) => Length.FromMeters(value / UnitsPerMeter);

    public double As(Length length) => length.Meters.ToDouble() * UnitsPerMeter;
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class SolveProgress(IDataAccess access, long totalRows, long completedRows = 0) : IProgress<int> {
    public void Report(int value) => access.SetProgress((int)(100 * ((Int128)completedRows + value) / totalRows));
}

public abstract class DefinedComponent<TSelf> : ModularComponent, IPinCushion where TSelf : DefinedComponent<TSelf>, IComponentDefinition<TSelf> {
    // --- [STATE]
    private readonly AtomSeq<Action<IDataAccess, Fin<Unit>>> matches = AtomSeq<Action<IDataAccess, Fin<Unit>>>();
    private readonly Ref<Option<(Action<IDisposable> Retain, IDisposable Lease)>> attachment =
        Ref(Option<(Action<IDisposable> Retain, IDisposable Lease)>.None);

    // --- [CONSTRUCTION]
    protected DefinedComponent() : base(ComponentOps.Localized(TSelf.Row.Face, GrasshopperPlugins.Owner(typeof(TSelf)))) => DocumentChanged += AttachmentChanged;

    protected DefinedComponent(GrasshopperIO.IReader reader) : base(reader) {
        DocumentChanged += AttachmentChanged;
        CallbackSite site = GrasshopperPlugins.Site(this);
        Callbacks.Run(Callbacks.Each(
            from placed in TSelf.Row.Inputs.Map((row, index) => (Row: row, Parameter: Parameters.Input(index))).Concat(TSelf.Row.Outputs.Map((row, index) => (Row: row, Parameter: Parameters.Output(index))))
            from attach in placed.Row.Attach
            select attach(placed.Parameter, site.Sink)), site);
    }

    public IEnumerable<Guid> SupportedPins => TSelf.Row.SupportedPins;

    protected override IIcon IconInternal =>
        GrasshopperPlugins.Site(this) switch {
            var site => Callbacks.Answer(TSelf.Row.Glyph.Map(glyph => CanvasIcons.Vector(site.Sink, glyph)), () => base.IconInternal, () => base.IconInternal, site),
        };

    protected sealed override void AddInputs(ModularInputAdder inputs) =>
        Added(TSelf.Row.Inputs, (row, sink) => row.Input(inputs, sink), GrasshopperPlugins.Site(this));

    protected sealed override void AddOutputs(ModularOutputAdder outputs) =>
        Added(TSelf.Row.Outputs, (row, sink) => row.Output(outputs, sink), GrasshopperPlugins.Site(this));

    protected override IAttributes CreateAttributes() =>
        Callbacks.Answer(
            TSelf.Row.Attributes.Map(create => IO.lift(() => create((TSelf)this))),
            () => base.CreateAttributes(), () => base.CreateAttributes(), GrasshopperPlugins.Site(this));

    private static void Added(Seq<ParameterRow> rows, Func<ParameterRow, IPlugInSink, IO<IParameter>> add, CallbackSite site) =>
        Callbacks.Run(rows.TraverseM(row => add(row, site.Sink)).As(), site);

    // --- [ATTACHMENT]
    private void AttachmentChanged(object? sender, ObjectDocumentEventArgs e) =>
        Callbacks.Started(Callbacks.Each(Seq(
            Detached,
            when(Document is not null, DisposalOps.Acquire(
                retain => TSelf.Row.Attached.TraverseM(attached => attached((TSelf)this, retain)).As().Map(retain),
                (retain, lease) => atomic(() => attachment.Value = Some((retain, lease)))).Map(static _ => unit)).As())), GrasshopperPlugins.Site(this));

    private IO<Unit> Detached => atomicIO(() => {
        Option<(Action<IDisposable> Retain, IDisposable Lease)> held = attachment.Value;
        attachment.Value = None;
        return held;
    }).Bind(static held => held.Traverse(static scope => IO.lift(scope.Lease.Dispose)).As()).Map(unit);

    public IO<A> Retained<A>(Func<Action<IDisposable>, IO<A>> acquire) =>
        atomicIO(() => attachment.Value.Map(static scope => scope.Retain).ToFin(Errors.Cancelled))
            .Bind(static held => IO.lift(held)).Bind(acquire);

    // --- [SOLVE]
    protected sealed override void Process(IDataAccess access) {
        Fin<Unit> solved = Callbacks.Settled(TSelf.Row.Solve((TSelf)this, access), access.Solution.Token);
        _ = matches.ToSeq().Iter(deliver => deliver(access, solved));
        Seq<Error> failed = solved.Match(
            Succ: static _ => Seq<Error>(),
            Fail: static error => toSeq(error.AsIterable()).Filter(static leaf => !leaf.Is(Errors.Cancelled)).Strict());
        _ = failed.Iter(leaf => access.AddMessage(ComponentOps.Rendered(Message.Error, leaf, [])));
        _ = failed.Filter(static leaf => leaf.IsExceptional).Iter(leaf => GrasshopperPlugins.Site(this).Sink.Report(leaf, GetType(), nameof(Process)));
    }

    public IO<Seq<A>> Matched<A>(SolutionMode mode, Func<IDataAccess, IO<A>> read) =>
        from document in IO.lift(() => Refused.UnlessFound(Document, nameof(Document)))
        from captured in IO.lift(static () => new ConcurrentDictionary<(Solution Solution, int Index), Fin<A>>())
        from token in cancelToken
        from values in DisposalOps.Within<IDisposable, Seq<A>>(retain =>
            from source in DisposalOps.Acquire(IO.lift(() => CancellationTokenSource.CreateLinkedTokenSource(token)), retain)
            let requested = source.Token
            from collected in Subscriptions.Attach<Action<IDataAccess, Fin<Unit>>>(
                handler => _ = matches.Add(handler), handler => _ = matches.Swap(held => held.Filter(deliver => deliver != handler).Strict()), (access, solved) => {
                    if (access.Solution.Token != requested) return;
                    Fin<A> value = Callbacks.Settled(read(access), requested);
                    captured[(access.Solution, access.Index)] = solved.Match(
                        Succ: _ => value,
                        Fail: error => value.Match(Succ: _ => error, Fail: fault => error + fault));
                }, retain)
            from expiring in Subscriptions.Attach<EventHandler<SolutionIdEventArgs>>(
                handler => document.Solution.SolutionAboutToStart += handler,
                handler => document.Solution.SolutionAboutToStart -= handler, (_, _) => Expire(), retain)
            from solution in Solutions.Start(document.Solution, source, mode)
            from matched in IO.lift(() => Callbacks.Each(toSeq(captured.Where(row => ReferenceEquals(row.Key.Solution, solution)).OrderBy(static row => row.Key.Index)),
                static (row, _) => row.Value))
            select matched, DisposalOps.Release)
        select values;

    // --- [PANEL]
    public override void AppendToInputPanel(InputPanel panel) {
        base.AppendToInputPanel(panel);
        CallbackSite site = GrasshopperPlugins.Site(this);
        Callbacks.Run(Appended(panel, site), site);
    }

    private IO<Unit> Appended(InputPanel panel, CallbackSite site) =>
        IO.lift(() => TSelf.Row.Panel((TSelf)this)).Bind(rows => unless(rows.IsEmpty,
              from document in IO.lift(static () => Refused.UnlessFound(RhinoDoc.ActiveDoc, nameof(RhinoDoc.ActiveDoc)))
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
    public static Nomen Localized(Nomen face, IGrasshopperPlugin plugin) =>
        face.WithChapter(string.IsNullOrEmpty(face.Chapter) ? plugin.Plugin.Name : face.Chapter)
            .WithName(RowText.Localize(face.Name, plugin)).WithInfo(RowText.Localize(face.Info, plugin));

    // --- [COMPLETED_INPUTS]
    public static IO<Option<A>> Completed<A>(ModularComponent component, Func<ObjectSolutionState, Option<A>> read) =>
        IO.lift(() => Some(component.State).Filter(static state => state.Phase is Grasshopper2.Doc.Phase.Completed).Bind(read));

    public static Option<A> CurrentInput<A>(ModularComponent component, ObjectSolutionState state, int index) =>
        Optional(state.Data.Tree(component.Parameters.Input(index).InstanceId))
            .Bind(static tree => tree.NonNullItems.Cast<A>().Distinct().ToArray() is [var value] ? Some(value) : None);

    // --- [MESSAGES]
    public static Message Rendered(Func<string, string, MessageAction[], Message> level, Error note, Seq<MessageAction> actions) =>
        level(note.Message, Conversions.Unset(note.Inner.Map(static cause => ErrorOps.Localize(cause))), [.. actions]);

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
