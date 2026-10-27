using System.Collections.Concurrent;
using System.Collections.Generic;
using Grasshopper2.Components;
using Grasshopper2.Doc;
using Grasshopper2.Parameters;
using Grasshopper2.UI;
using Grasshopper2.UI.Icon;
using Grasshopper2.Undo;
using Grasshopper2.Undo.Actions;
using Rasm.Grasshopper.Doc;
using Rasm.Grasshopper.Parameters;
using Rhino;

namespace Rasm.Grasshopper.Components;

// --- [TYPES] ---------------------------------------------------------------------------
public interface IComponentDefinition<TSelf> where TSelf : ModularComponent {
    public static abstract ComponentRow<TSelf> Row { get; }
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ComponentRow<TSelf>(
    Nomen Face,
    Option<IIcon> Icon,
    Seq<ParameterRow> Inputs,
    Seq<ParameterRow> Outputs,
    Seq<Guid> SupportedPins,
    Func<TSelf, IDataAccess, IO<Unit>> Solve)
    where TSelf : ModularComponent {
    public Seq<Func<TSelf, IO<Unit>>> Attached { get; init; }
}

public sealed record SolveUnits(double UnitsPerMeter) {
    // --- [READ]
    public static IO<SolveUnits> Read(IDataAccess access) =>
        IO.lift(() => GrasshopperFault.Unless(access.GetUnitScaling(UnitSystem.Meters, out double unitsPerMeter)).Map(_ => new SolveUnits(unitsPerMeter)));

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
    protected DefinedComponent() : this(GrasshopperPlugins.Owner(typeof(TSelf))) { }

    private DefinedComponent(IGrasshopperPlugin plugin) : base(Chaptered(TSelf.Row.Face, plugin)) {
        Plugin = plugin;
        _ = Plugin.Session.Run(Hooks.On<ObjectDocumentEventArgs>(h => DocumentChanged += h, h => DocumentChanged -= h, _ => Reattached, Plugin).Under(scope));
    }

    protected DefinedComponent(GrasshopperIO.IReader reader) : base(reader) {
        Plugin = GrasshopperPlugins.Owner(GetType());
        _ = Plugin.Session.Run(Hooks.On<ObjectDocumentEventArgs>(h => DocumentChanged += h, h => DocumentChanged -= h, _ => Reattached, Plugin).Under(scope));
        _ = Plugin.Session.Run((
            from placed in TSelf.Row.Inputs.Map((row, index) => (Row: row, Parameter: Parameters.Input(index))).Concat(TSelf.Row.Outputs.Map((row, index) => (Row: row, Parameter: Parameters.Output(index))))
            from attach in placed.Row.Attach
            select attach(placed.Parameter, Plugin)).TraverseM(static attached => attached).As());
    }

    public IGrasshopperPlugin Plugin { get; }

    public IEnumerable<Guid> SupportedPins => TSelf.Row.SupportedPins;

    protected override IIcon IconInternal => TSelf.Row.Icon.IfNone(() => base.IconInternal);

    protected sealed override void AddInputs(ModularInputAdder inputs) =>
        Plugin.Session.Run(TSelf.Row.Inputs.TraverseM(row => row.Input(inputs, Plugin)).As());

    protected sealed override void AddOutputs(ModularOutputAdder outputs) =>
        Plugin.Session.Run(TSelf.Row.Outputs.TraverseM(row => row.Output(outputs, Plugin)).As());

    private static Nomen Chaptered(Nomen face, IGrasshopperPlugin plugin) => string.IsNullOrEmpty(face.Chapter) ? face.WithChapter(plugin.Plugin.Name) : face;

    // --- [ATTACHMENT]
    private IO<Unit> Reattached =>
        Optional(Document).Match(
            Some: _ => Leases.Swapped(attachment, TSelf.Row.Attached.TraverseM(attached => attached((TSelf)this)).As().Map(unit)),
            None: () => Leases.Cleared(attachment)).Under(scope);

    public IO<A> Attached<A>(IO<A> acquire) =>
        attachment.ValueIO.Bind(held => held.Match(Some: lease => acquire.Under(lease.Scope), None: static () => IO.fail<A>(Errors.Cancelled)));

    // --- [SOLVE]
    protected sealed override void Process(IDataAccess access) {
        Fin<Unit> solved = PluginSession.Settled(TSelf.Row.Solve((TSelf)this, access), access.Solution.Token);
        _ = matches.ToSeq().Iter(deliver => deliver(access, solved));
        Seq<Error> failed = solved.Match(
            Succ: static _ => Seq<Error>(),
            Fail: static error => toSeq(error.AsIterable()).Filter(static leaf => !leaf.Is(Errors.Cancelled)).Strict());
        _ = failed.Iter(leaf => access.AddMessage(Message.Error(leaf.Message, leaf.Inner.Map(static cause => cause.Message).ValueUnsafe())));
        _ = Plugin.Session.Run(IO.lift(() => failed.Filter(static leaf => leaf.IsExceptional).Traverse(static leaf => Fin.Fail<Unit>(leaf)).As()));
    }

    public IO<Seq<A>> Matched<A>(SolutionMode mode, Func<IDataAccess, IO<A>> read) =>
        (from document in IO.lift(() => Hosts.Found(Document))
         from captured in IO.lift(static () => new ConcurrentDictionary<(Solution Solution, int Index), Fin<A>>())
         from token in cancelToken
         from source in use(() => CancellationTokenSource.CreateLinkedTokenSource(token))
         from collected in use(() => Collector(read, captured, source.Token), handler => IO.lift(() => ignore(matches.Swap(held => held.Filter(deliver => deliver != handler).Strict()))))
         from expiring in Hooks.On<SolutionIdEventArgs>(h => document.Solution.SolutionAboutToStart += h, h => document.Solution.SolutionAboutToStart -= h, _ => IO.lift(Expire), Plugin)
         from solution in Solutions.Start(document.Solution, source, mode, Plugin)
         from matched in IO.lift(() => toSeq(captured.Where(row => ReferenceEquals(row.Key.Solution, solution)).OrderBy(static row => row.Key.Index))
             .Traverse(static row => row.Value).As())
         select matched).Bracket();

    private Action<IDataAccess, Fin<Unit>> Collector<A>(Func<IDataAccess, IO<A>> read, ConcurrentDictionary<(Solution Solution, int Index), Fin<A>> captured, CancellationToken requested) {
        Action<IDataAccess, Fin<Unit>> deliver = (access, solved) => {
            if (access.Solution.Token != requested) return;
            Fin<A> value = PluginSession.Settled(read(access), requested);
            captured[(access.Solution, access.Index)] = solved.Match(
                Succ: _ => value,
                Fail: error => value.Match(Succ: _ => error, Fail: fault => error + fault));
        };
        _ = matches.Add(deliver);
        return deliver;
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ComponentOps {
    // --- [COMPLETED_INPUTS]
    public static IO<Option<A>> Completed<A>(ModularComponent component, Func<ObjectSolutionState, Option<A>> read) =>
        IO.lift(() => Some(component.State).Filter(static state => state.Phase is Grasshopper2.Doc.Phase.Completed).Bind(read));

    public static Option<A> CurrentInput<A>(ModularComponent component, ObjectSolutionState state, int index) =>
        Optional(state.Data.Tree(component.Parameters.Input(index).InstanceId))
            .Bind(static tree => tree.NonNullItems.Cast<A>().Distinct().ToArray() is [var value] ? Some(value) : None);

    // --- [STATE]
    public static IO<Unit> Stored<T>(IDocumentObject owner, string key, T value, VerbNoun name) where T : GrasshopperIO.IStorable =>
        HistoryOps.Recorded(
            owner, name, AutoSaveReason.ObjectChanged,
            () => new CustomValueAction(owner, key, Effects.ExpireSolution), IO.lift(() => owner.CustomValues.Set(key, value)), static _ => true);
}
