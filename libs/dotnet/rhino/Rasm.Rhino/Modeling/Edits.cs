namespace Rasm.Rhino.Modeling;

// --- [MODELS] --------------------------------------------------------------------------
[Union<Unit, bool, TypeParamRef1>(T1Name = "Completed", T2Name = "Changed", T3Name = "Count", T1IsStateless = true, MapMethods = SwitchMapMethodsGeneration.None)]
public readonly partial struct EditResult<TCount> where TCount : struct, System.Numerics.IBinaryInteger<TCount>;

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record EditStep<T, TCount> where T : GeometryBase where TCount : struct, System.Numerics.IBinaryInteger<TCount> {
    public sealed record Changed(EditResult<TCount> Result) : EditStep<T, TCount>;

    public sealed record Replaced(T Successor) : EditStep<T, TCount>;
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record SmoothFrame {
    public sealed record World : SmoothFrame;

    public sealed record ObjectUvn : SmoothFrame;

    public sealed record CPlane(Plane Plane) : SmoothFrame;
}

public sealed record SmoothOptions(double Factor, bool X, bool Y, bool Z, bool FixBoundaries, SmoothFrame Frame) {
    public (SmoothingCoordinateSystem System, Plane Plane) Coordinates =>
        Frame.Switch(
            world: static _ => (SmoothingCoordinateSystem.World, Plane.WorldXY),
            objectUvn: static _ => (SmoothingCoordinateSystem.Object, Plane.WorldXY),
            cPlane: static frame => (SmoothingCoordinateSystem.CPlane, frame.Plane));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Edits {
    // --- [FOLD]
    public static IO<(T Result, Seq<(TEdit Edit, EditResult<TCount> Result)> Results)> Apply<T, TEdit, TCount>(
        T source, Seq<TEdit> edits, Func<T, TEdit, Action<IDisposable>, IO<EditStep<T, TCount>>> step, Func<T, IO<Unit>> settle, Action<IDisposable> retain)
        where T : GeometryBase where TCount : struct, System.Numerics.IBinaryInteger<TCount> =>
        DisposalOps.Acquire(owned =>
            from subject in DisposalOps.Acquire(IO.lift(static () => new Retained<IDisposable>(DisposalOps.Release)), owned)
            from copy in Copies.Duplicate(source, subject.Retain)
            from cached in IO.lift(() => (Copy: copy, Source: source) switch {
                (Copy: SubD edited, Source: SubD original) => ignore(edited.CopyEvaluationCache(original)),
                _ => unit,
            })
            from folded in edits.FoldM((Current: copy, Results: Seq<(TEdit Edit, EditResult<TCount> Result)>()), (state, edit) =>
                from stepped in DisposalOps.Acquire(held => step(state.Current, edit, held), (result, lease) => result.Switch(
                    (Subject: subject, Lease: lease),
                    changed: static (at, _) => IO.lift(at.Lease.Dispose),
                    replaced: static (at, _) => at.Subject.Replace(at.Lease)))
                select stepped.Switch(
                    (State: state, Edit: edit),
                    changed: static (at, changed) => (at.State.Current, at.State.Results.Add((at.Edit, changed.Result))),
                    replaced: static (at, replaced) => (replaced.Successor, at.State.Results.Add((at.Edit, EditResult<TCount>.CreateCompleted()))))).As()
            from settled in settle(folded.Current)
            select folded, retain);

    // --- [STEPS]
    public static Answer<EditStep<T, TCount>> Accepted<T, TCount>(T edited, bool accepted) where T : GeometryBase where TCount : struct, System.Numerics.IBinaryInteger<TCount> =>
        (Answer.Accepted(accepted), Edited(edited, EditResult<TCount>.CreateCompleted())).Apply(static (_, step) => step).As();

    public static Answer<EditStep<T, TCount>> Changed<T, TCount>(T edited, bool changed) where T : GeometryBase where TCount : struct, System.Numerics.IBinaryInteger<TCount> =>
        Edited(edited, EditResult<TCount>.CreateChanged(changed));

    public static Answer<EditStep<T, TCount>> Count<T, TCount>(T edited, TCount count) where T : GeometryBase where TCount : struct, System.Numerics.IBinaryInteger<TCount> =>
        Edited(edited, EditResult<TCount>.CreateCount(count));

    public static Answer<EditStep<T, TCount>> Completed<T, TCount>(T edited) where T : GeometryBase where TCount : struct, System.Numerics.IBinaryInteger<TCount> =>
        Edited(edited, EditResult<TCount>.CreateCompleted());

    public static Answer<EditStep<T, TCount>> Replaced<T, TCount>(T? successor) where T : GeometryBase where TCount : struct, System.Numerics.IBinaryInteger<TCount> =>
        Answer.One(successor).Map(static EditStep<T, TCount> (held) => new EditStep<T, TCount>.Replaced(held));

    private static Answer<EditStep<T, TCount>> Edited<T, TCount>(T edited, EditResult<TCount> result) where T : GeometryBase where TCount : struct, System.Numerics.IBinaryInteger<TCount> =>
        (Answer.One(edited) with { Leaves = [] }).Map(EditStep<T, TCount> (_) => new EditStep<T, TCount>.Changed(result));
}
