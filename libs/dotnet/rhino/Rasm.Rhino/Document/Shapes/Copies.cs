namespace Rasm.Rhino.Document.Shapes;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Copies {
    // --- [COPIES]
    public static IO<T> Duplicate<T>(T source) where T : GeometryBase => Typed<T>(source.Duplicate, nameof(GeometryBase.Duplicate));

    public static IO<T> DuplicateShallow<T>(T source) where T : GeometryBase => Typed<T>(source.DuplicateShallow, nameof(GeometryBase.DuplicateShallow));

    public static IO<(T Copy, TResult Result)> Edit<T, TResult>(T source, Func<T, Fin<TResult>> edit, string member) where T : GeometryBase =>
        Owned(
            Duplicate(source),
            copy =>
                from result in edit(copy)
                from valid in Measurements.Valid(copy, member)
                select (valid, result));

    public static IO<T> Edit<T>(T source, Func<T, Fin<Unit>> edit, string member) where T : GeometryBase =>
        Edit<T, Unit>(source, edit, member).Map(static edited => edited.Copy);

    // --- [ACQUISITION]
    public static IO<T> Acquire<T>(Func<T?> create, string member) where T : GeometryBase =>
        Owned(IO.lift(() => Missing.Unless(create(), member)), product => Measurements.Valid(product, member));

    public static IO<T> Acquire<T>(Func<Fin<T>> create, string member) where T : GeometryBase =>
        Owned(IO.lift(() => create().Bind(product => Missing.Unless(product, member))), product => Measurements.Valid(product, member));

    public static IO<Seq<T>> Acquire<T>(Func<T?[]?> create, string member) where T : GeometryBase =>
        Owned(IO.lift(() => Missing.Unless(create(), member)), products => Measurements.Valid([.. products], member), Released);

    public static IO<Seq<T>> Acquire<T>(Func<Fin<T[]>> create, string member) where T : GeometryBase =>
        Owned(IO.lift(() => create().Bind(products => Missing.Unless(products, member))), products => Measurements.Valid([.. products], member), Released);

    public static IO<Seq<Seq<T>>> Acquire<T>(Func<List<T[]>?> create, string member) where T : GeometryBase =>
        Owned(
            IO.lift(() => Missing.Unless(create(), member)),
            groups => Callbacks.Each(toSeq(groups), (group, _) => Measurements.Valid([.. group], member)),
            static groups => DisposalOps.Release(toSeq(groups).Bind(static group => Conversions.Rows(group))));

    public static IO<Seq<T>> AcquireNonEmpty<T>(Func<T?[]?> create, string member) where T : GeometryBase =>
        Acquire(create, member).Bind(products => IO.lift(Conversions.NonEmpty(products, member)));

    public static IO<Seq<T>> AcquireNonEmpty<T>(Func<Fin<T[]>> create, string member) where T : GeometryBase =>
        Acquire(create, member).Bind(products => IO.lift(Conversions.NonEmpty(products, member)));

    public static IO<Seq<Option<T>>> AcquireSparse<T>(Func<T?[]?> create, string member) where T : GeometryBase =>
        Owned(IO.lift(() => Missing.Unless(create(), member)), products => Measurements.Valid(toSeq(products).Map(static product => Optional(product)), member), Released);

    public static IO<Seq<(T Result, TRow Row)>> Acquire<T, TRow>(Func<(T?[]? Results, IReadOnlyList<TRow>? Rows)> create, string member) where T : GeometryBase =>
        Owned(
            IO.lift(create),
            answer => (Missing.Unless(answer.Results, member).Bind(products => Measurements.Valid([.. products], member)).ToValidation(), Missing.Unless(answer.Rows, member).ToValidation())
                .Apply(static (kept, rows) => kept.Zip(toSeq(rows)))
                .As()
                .ToFin(),
            static answer => Released(answer.Results));

    // --- [INDICES]
    public static Fin<Unit> InRange(Seq<int> indices, int itemCount, string member) =>
        Callbacks.Each(indices, (index, _) => IndexOutOfRange.Unless(index, itemCount, member)).Map(static _ => unit);

    // --- [OWNERSHIP]
    public static IO<TKept> Owned<TMade, TKept>(IO<TMade> made, Func<TMade, Fin<TKept>> accept, Func<TMade, IO<Unit>> release) =>
        from product in made
        from kept in DisposalOps.OnFailure(IO.lift(() => accept(product)), release(product))
        select kept;

    public static IO<TKept> Owned<TMade, TKept>(IO<TMade> made, Func<TMade, Fin<TKept>> accept) where TMade : IDisposable =>
        Owned(made, accept, static product => IO.lift(product.Dispose));

    private static IO<T> Typed<T>(Func<GeometryBase?> copy, string member) where T : GeometryBase =>
        Owned(IO.lift(() => Missing.Unless(copy(), member)), static made => WrongType.Unless<T>(made));

    private static IO<Unit> Released<T>(T?[]? products) where T : GeometryBase => DisposalOps.Release(Conversions.Rows(products));
}
