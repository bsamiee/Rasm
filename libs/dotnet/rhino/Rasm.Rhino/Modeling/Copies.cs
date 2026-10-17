namespace Rasm.Rhino.Modeling;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Copies {
    // --- [COPIES]
    public static IO<T> Duplicate<T>(T source, Action<IDisposable> retain) where T : GeometryBase =>
        Owned(() => (T)source.Duplicate(), static copy => Fin.Succ(copy), retain);

    public static IO<T> DuplicateShallow<T>(T source, Action<IDisposable> retain) where T : GeometryBase =>
        Owned(() => (T)source.DuplicateShallow(), static copy => Fin.Succ(copy), retain);

    public static IO<(T Copy, TResult Result)> Edit<T, TResult>(T source, Func<T, IO<TResult>> edit, string member, Action<IDisposable> retain) where T : GeometryBase =>
        Owned(() => (T)source.Duplicate(),
            copy =>
                from result in edit(copy)
                from valid in IO.lift(() => Measurements.Valid(copy, member))
                select (valid, result),
            retain);

    // --- [ACQUISITION]
    public static IO<T> Acquire<T>(Func<T?> create, string member, Action<IDisposable> retain) where T : GeometryBase =>
        Owned(create, product => Measurements.Valid(product, member),
            static product => DisposalOps.Release(product), retain);

    public static IO<T> Acquire<T>(Func<CancellationToken, T?> create, string member, Action<IDisposable> retain) where T : GeometryBase =>
        from token in cancelToken
        from product in Owned(
            () => create(token),
            product => product is null && token.IsCancellationRequested ? Errors.Cancelled : Measurements.Valid(product, member),
            static product => DisposalOps.Release(product), retain)
        select product;

    public static IO<T> Acquire<T>(Func<Fin<T>> create, string member, Action<IDisposable> retain) where T : GeometryBase =>
        Owned(create, answer =>
            from product in answer
            from valid in Measurements.Valid(product, member)
            select valid,
            static answer => answer.Match(Succ: static product => DisposalOps.Release(product), Fail: static _ => IO.pure(unit)), retain);

    public static IO<Seq<T>> Acquire<T>(Func<T?[]?> create, string member, Action<IDisposable> retain) where T : GeometryBase =>
        Owned(create, products => Refused.UnlessFound(products, member).Bind(held => Measurements.Valid([.. held], member)), Released, retain);

    public static IO<Seq<T>> Acquire<T>(Func<Fin<T[]>> create, string member, Action<IDisposable> retain) where T : GeometryBase =>
        Owned(create, answer =>
            from products in answer
            from valid in Measurements.Valid([.. products], member)
            select valid,
            static answer => answer.Match(Succ: Released, Fail: static _ => IO.pure(unit)), retain);

    public static IO<Seq<Seq<T>>> Acquire<T>(Func<List<T[]>?> create, string member, Action<IDisposable> retain) where T : GeometryBase =>
        Owned(create,
            groups => Refused.UnlessFound(groups, member).Bind(held => Callbacks.Each(toSeq(held), (group, _) => Measurements.Valid([.. group], member))),
            static groups => DisposalOps.Release(Conversions.Rows(groups).Bind(static group => Conversions.Rows(group))), retain);

    public static IO<Seq<T>> AcquireNonEmpty<T>(Func<T?[]?> create, string member, Action<IDisposable> retain) where T : GeometryBase =>
        Owned(create, products => Refused.UnlessFound(products, member)
            .Bind(held => Measurements.Valid([.. held], member)).Bind(held => Conversions.NonEmpty(held, member)), Released, retain);

    public static IO<Seq<T>> AcquireNonEmpty<T>(Func<Fin<T[]>> create, string member, Action<IDisposable> retain) where T : GeometryBase =>
        Owned(create, answer =>
            from products in answer
            from valid in Measurements.Valid([.. products], member)
            from nonempty in Conversions.NonEmpty(valid, member)
            select nonempty,
            static answer => answer.Match(Succ: Released, Fail: static _ => IO.pure(unit)), retain);

    public static IO<Seq<Option<T>>> AcquireSparse<T>(Func<T?[]?> create, string member, Action<IDisposable> retain) where T : GeometryBase =>
        Owned(create, products => Refused.UnlessFound(products, member)
            .Bind(held => Measurements.Valid(toSeq(held).Map(static product => Optional(product)), member)), Released, retain);

    public static IO<Seq<(T Result, TRow Row)>> Acquire<T, TRow>(Func<(T?[]? Results, IReadOnlyList<TRow>? Rows)> create, string member, Action<IDisposable> retain) where T : GeometryBase =>
        Owned(create,
            answer => (Refused.UnlessFound(answer.Results, member).Bind(products => Measurements.Valid([.. products], member)).ToValidation(), Refused.UnlessFound(answer.Rows, member).ToValidation())
                .Apply(static (kept, rows) => kept.Zip(toSeq(rows))).As().ToFin(),
            static answer => Released(answer.Results), retain);

    // --- [INDICES]
    public static Fin<Unit> InRange(Seq<int> indices, int itemCount, string member) =>
        Callbacks.Each(indices, (index, _) => Conversions.Within(index, itemCount, member)).Map(static _ => unit);

    // --- [OWNERSHIP]
    public static IO<TKept> Owned<TMade, TKept>(Func<TMade> create, Func<TMade, Fin<TKept>> accept, Func<TMade, IO<Unit>> release, Action<IDisposable> retain) =>
        Owned(create, product => IO.lift(() => accept(product)), release, retain);

    public static IO<TKept> Owned<TMade, TKept>(Func<TMade> create, Func<TMade, IO<TKept>> body, Func<TMade, IO<Unit>> release, Action<IDisposable> retain) =>
        IO.lift(create).Bind(product => DisposalOps.OnFailure(
            from kept in IO.pure(product).Bind(body)
            from retained in IO.lift(() => retain(new Disposal<TMade>(product, held => release(held).Run())))
            select kept,
            IO.pure(product).Bind(release))).Uninterruptible();

    public static IO<TKept> Owned<TMade, TKept>(Func<TMade> create, Func<TMade, Fin<TKept>> accept, Action<IDisposable> retain) where TMade : IDisposable =>
        Owned(create, accept, static product => IO.lift(product.Dispose), retain);

    public static IO<TKept> Owned<TMade, TKept>(Func<TMade> create, Func<TMade, IO<TKept>> body, Action<IDisposable> retain) where TMade : IDisposable =>
        Owned(create, body, static product => IO.lift(product.Dispose), retain);

    private static IO<Unit> Released<T>(T?[]? products) where T : GeometryBase => DisposalOps.Release(Conversions.Rows(products));
}
