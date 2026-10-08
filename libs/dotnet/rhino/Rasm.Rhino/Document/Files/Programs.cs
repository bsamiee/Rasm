using Rhino;
using Rhino.DocObjects;
using Rhino.FileIO;

namespace Rasm.Rhino.Document.Files;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record StepOutcome {
    public sealed record Imported(Seq<Guid> Objects) : StepOutcome;
    public sealed record Landed(string Path, ContentHash Content) : StepOutcome;
    public sealed record Edited : StepOutcome;
    public sealed record Unchanged : StepOutcome;

    public static IO<StepOutcome> Import<T>(RhinoDoc document, IO<T> import) =>
        from next in IO.lift(static () => RhinoObject.NextRuntimeSerialNumber)
        from _ in import
        from added in IO.lift(() => Conversions.Rows(document.Objects.AllObjectsSince(next - 1u)).Map(static item => item.Id).Strict())
        select (StepOutcome)new Imported(added);

    public static IO<StepOutcome> Land(string path, Func<string, IO<Unit>> write) => Save(write(path).Map(_ => Some(path)));

    public static IO<StepOutcome> Save(IO<Option<string>> save) =>
        save.Bind(static written => IO.lift(() => written.Match<StepOutcome>(Some: static path => new Landed(path, ContentHash.CreateFromFile(path)), None: static () => new Unchanged())));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Programs {
    public static IO<Seq<T>> Ordered<T>(Seq<IO<T>> steps) => steps.Map(static (step, index) => Indexed(step, index)).TraverseM(static step => step).As();

    public static IO<(Seq<Error> Fails, Seq<(int Index, T Outcome)> Succs)> Partitioned<T>(Seq<IO<T>> steps) =>
        steps.Map<K<IO, (int Index, T Outcome)>>(static (step, index) => Indexed(step.Map(outcome => (Index: index, Outcome: outcome)), index)).PartitionFallible().As();

    private static IO<T> Indexed<T>(IO<T> step, int index) => step.MapFail(error => new StepFailed(index) + error);
}
