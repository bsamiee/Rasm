using System.Collections.Concurrent;
using System.Collections.Generic;
using Grasshopper2.Components;
using Grasshopper2.Doc;
using Grasshopper2.Parameters;
using Grasshopper2.Parameters.Standard;
using Grasshopper2.UI;
using Grasshopper2.UI.Icon;
using Grasshopper2.UI.InputPanel;
using Grasshopper2.Undo;
using Grasshopper2.Undo.Actions;
using Rasm.Grasshopper.Doc;
using Rasm.Grasshopper.Parameters;
using Rasm.RhinoCommon;
using Rasm.RhinoCommon.Document.Units;
using Rasm.RhinoCommon.Events;
using Rasm.RhinoCommon.Persistence.Stores;
using Rasm.RhinoCommon.UI.Rows;
using Rasm.UI;
using Rhino;
using Rhino.Geometry.Intersect;

namespace Rasm.Grasshopper.Components;

// --- [TYPES] ---------------------------------------------------------------------------
public interface IComponentDefinition<TSelf> where TSelf : ModularComponent {
    public static abstract ComponentRow<TSelf> Row { get; }
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ComponentRow<TSelf>(
    Nomen Face,
    Option<GlyphSource> Glyph,
    Seq<ParameterRow> Inputs,
    Seq<ParameterRow> Outputs,
    Seq<Guid> SupportedPins,
    Func<TSelf, IDataAccess, IO<Unit>> Solve)
    where TSelf : ModularComponent {
    // --- [PRESENTATION]
    public Func<TSelf, Seq<ControlRow>> Panel { get; init; } = static _ => [];
    public Option<Func<TSelf, Fin<IAttributes>>> Attributes { get; init; }

    // --- [ATTACHMENT]
    public Seq<Func<TSelf, IO<Unit>>> Attached { get; init; }
}

public sealed record SolveUnits(Tolerances Tolerances, double UnitsPerMeter) {
    // --- [READ]
    public static IO<SolveUnits> Read(IDataAccess access) =>
        IO.lift(Fin<SolveUnits> () => {
            _ = access.GetTolerance(out double absolute);
            _ = access.GetTolerance(out Angle angle);
            _ = access.GetUnitScaling(Rhino.UnitSystem.Meters, out double unitsPerMeter);
            return (access.GetItem(RelativeTolerancePin.Id, out double pinned) && pinned >= 0d
                    ? pinned
                    : Conversions.Found(RhinoDoc.ActiveDoc).Map(static document => document.ModelRelativeTolerance))
                .Map(relative => new SolveUnits(
                    new Tolerances(absolute, Math.Clamp(relative, 1e-12, 1e6), angle.Radians, Intersection.MeshIntersectionsTolerancesCoefficient), unitsPerMeter));
        });

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
    private readonly EnvIO scope = EnvIO.New(token: CancellationToken.None);
    private readonly Ref<Option<Lease<Unit>>> attachment = Ref(Option<Lease<Unit>>.None);

    // --- [CONSTRUCTION]
    protected DefinedComponent() : this(GrasshopperPlugins.Owner(typeof(TSelf)).ThrowIfFail()) { }

    private DefinedComponent(IGrasshopperPlugin plugin) : base(ComponentOps.Localized(TSelf.Row.Face, plugin)) {
        Plugin = plugin;
        _ = Plugin.Run(GrasshopperEvents.DocumentChanged(this).Inline(_ => Reattached, Plugin).Under(scope));
    }

    protected DefinedComponent(GrasshopperIO.IReader reader) : base(reader) {
        Plugin = GrasshopperPlugins.Owner(GetType()).ThrowIfFail();
        _ = Plugin.Run(GrasshopperEvents.DocumentChanged(this).Inline(_ => Reattached, Plugin).Under(scope));
        _ = Plugin.Run(Callbacks.Each(
            from placed in TSelf.Row.Inputs.Map((row, index) => (Row: row, Parameter: Parameters.Input(index))).Concat(TSelf.Row.Outputs.Map((row, index) => (Row: row, Parameter: Parameters.Output(index))))
            from attach in placed.Row.Attach
            select attach(placed.Parameter, Plugin)));
    }

    public IGrasshopperPlugin Plugin { get; }

    public IEnumerable<Guid> SupportedPins => TSelf.Row.SupportedPins;

    protected override IIcon IconInternal =>
        TSelf.Row.Glyph.Bind(glyph => Plugin.Answer(CanvasIcons.Vector(glyph).Map(static icon => Some(icon)), Option<IIcon>.None)).IfNone(() => base.IconInternal);

    protected sealed override void AddInputs(ModularInputAdder inputs) =>
        Plugin.Run(TSelf.Row.Inputs.TraverseM(row => row.Input(inputs, Plugin)).As());

    protected sealed override void AddOutputs(ModularOutputAdder outputs) =>
        Plugin.Run(TSelf.Row.Outputs.TraverseM(row => row.Output(outputs, Plugin)).As());

    protected override IAttributes CreateAttributes() =>
        TSelf.Row.Attributes.Bind(create => Plugin.Answer(IO.lift(() => create((TSelf)this)).Map(static attributes => Some(attributes)), Option<IAttributes>.None)).IfNone(base.CreateAttributes);

    // --- [ATTACHMENT]
    private IO<Unit> Reattached =>
        (Document is null
            ? Products.Cleared(attachment)
            : Products.Swapped(attachment, TSelf.Row.Attached.TraverseM(attached => attached((TSelf)this)).As().Map(unit))).Under(scope);

    public IO<A> Attached<A>(IO<A> acquire) =>
        attachment.ValueIO.Bind(held => held.Match(Some: lease => acquire.Under(lease.Scope), None: static () => IO.fail<A>(Errors.Cancelled)));

    // --- [SOLVE]
    protected sealed override void Process(IDataAccess access) {
        Fin<Unit> solved = Callbacks.Settled(TSelf.Row.Solve((TSelf)this, access), access.Solution.Token);
        _ = matches.ToSeq().Iter(deliver => deliver(access, solved));
        Seq<Error> failed = solved.Match(
            Succ: static _ => Seq<Error>(),
            Fail: static error => toSeq(error.AsIterable()).Filter(static leaf => !leaf.Is(Errors.Cancelled)).Strict());
        _ = failed.Iter(leaf => access.AddMessage(ComponentOps.Rendered(Message.Error, leaf, [])));
        Seq<Error> exceptional = failed.Filter(static leaf => leaf.IsExceptional);
        if (!exceptional.IsEmpty) _ = Plugin.Run(IO.fail<Unit>(Error.Many(exceptional)));
    }

    public IO<Seq<A>> Matched<A>(SolutionMode mode, Func<IDataAccess, IO<A>> read) =>
        (from document in IO.lift(() => Conversions.Found(Document))
         from captured in IO.lift(static () => new ConcurrentDictionary<(Solution Solution, int Index), Fin<A>>())
         from token in cancelToken
         from source in use(() => CancellationTokenSource.CreateLinkedTokenSource(token))
         from collected in use(() => Collector(read, captured, source.Token), handler => _ = matches.Swap(held => held.Filter(deliver => deliver != handler).Strict()))
         from expiring in Subscriptions.During<SolutionIdEventArgs>(h => document.Solution.SolutionAboutToStart += h, h => document.Solution.SolutionAboutToStart -= h)
             .Inline(_ => IO.lift(Expire), Plugin)
         from solution in Solutions.Start(document.Solution, source, mode, Plugin)
         from matched in IO.lift(() => Callbacks.Each(toSeq(captured.Where(row => ReferenceEquals(row.Key.Solution, solution)).OrderBy(static row => row.Key.Index)),
             static (row, _) => row.Value))
         select matched).Bracket();

    private Action<IDataAccess, Fin<Unit>> Collector<A>(Func<IDataAccess, IO<A>> read, ConcurrentDictionary<(Solution Solution, int Index), Fin<A>> captured, CancellationToken requested) {
        Action<IDataAccess, Fin<Unit>> deliver = (access, solved) => {
            if (access.Solution.Token != requested) return;
            Fin<A> value = Callbacks.Settled(read(access), requested);
            captured[(access.Solution, access.Index)] = solved.Match(
                Succ: _ => value,
                Fail: error => value.Match(Succ: _ => error, Fail: fault => error + fault));
        };
        _ = matches.Add(deliver);
        return deliver;
    }

    // --- [PANEL]
    public override void AppendToInputPanel(InputPanel panel) {
        base.AppendToInputPanel(panel);
        _ = Plugin.Run(Appended(panel));
    }

    private IO<Unit> Appended(InputPanel panel) =>
        IO.lift(() => TSelf.Row.Panel((TSelf)this)).Bind(rows => unless(rows.IsEmpty,
            from document in IO.lift(static () => Conversions.Found(RhinoDoc.ActiveDoc))
            from lifetime in IO.lift(static () => EnvIO.New(token: CancellationToken.None))
            from grid in (from opened in RowScope.Open(Plugin, Some(document), new CommitMode.Immediate())
                          from realized in RowGrid.Realize(ControlRows.Fixed(rows), opened)
                          from unloaded in Subscriptions.Host<EventArgs>(h => realized.View.UnLoad += h, h => realized.View.UnLoad -= h)
                              .Inline(_ => IO.lift(lifetime.Dispose), Plugin)
                          select realized).Under(lifetime).Catch(error => IO.lift(lifetime.Dispose) >> IO.fail<RowGrid>(error)).As()
            from added in IO.lift(() => {
                using IDisposable category = panel.BeginCategory(RowText.Localize("Settings"));
                panel.Add(grid.View);
            }).Uninterruptible()
            select added).As());
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
        level(note.Message, Conversions.Unset(note.Inner.Map(static cause => ErrorOps.Messages(cause))), [.. actions]);

    // --- [STATE]
    public static ValueStore<TRecord> State<TRecord>(IDocumentObject owner, string key, TRecord fallback) where TRecord : notnull =>
        KeyedText.Of(owner.CustomValues, key) switch {
            var text => Recorded(owner, text.Owner, text.Store(fallback)),
        };

    public static ValueStore<T> Recorded<T>(IDocumentObject owner, string filter, ValueStore<T> store) where T : notnull =>
        store with {
            Put = value => HistoryOps.Recorded(
                owner, new VerbNoun(RowText.Localize("Change"), owner.InstanceId.ToString()), AutoSaveReason.ObjectChanged,
                () => new CustomValueAction(owner, filter, Effects.ExpireSolution), store.Put(value), static change => change.HasChanged),
        };
}
