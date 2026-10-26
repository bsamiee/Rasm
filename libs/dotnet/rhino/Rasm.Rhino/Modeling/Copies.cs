using System.Globalization;

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
            answer => (Refused.UnlessFound(answer.Results, member).Bind(products => Measurements.Valid([.. products], member)), Refused.UnlessFound(answer.Rows, member))
                .Apply(static (kept, rows) => kept.Zip(toSeq(rows))).As(),
            static answer => Released(answer.Results), retain);

    // --- [INDICES]
    public static Fin<Seq<ComponentIndex>> Within(GeometryBase geometry, Seq<ComponentIndex> components) =>
        Callbacks.Each(components, (component, at) => Holds(geometry, component).Match(
            Some: held => held
                ? Fin.Succ(component)
                : Error.New(ErrorCodes.ValidationFailed, string.Create(CultureInfo.InvariantCulture, $"{component.ComponentIndexType} {component.Index} at element {at} is absent from {geometry.ObjectType}")),
            None: () => Error.New(ErrorCodes.ValidationFailed, string.Create(CultureInfo.InvariantCulture, $"{component.ComponentIndexType} at element {at} names no {geometry.ObjectType} component"))));

    public static Fin<Seq<int>> Within(GeometryBase geometry, ComponentIndexType type, Seq<int> indices) =>
        Within(geometry, indices.Map(index => new ComponentIndex(type, index))).Map(static held => held.Map(static component => component.Index));

    private static Option<bool> Holds(GeometryBase geometry, ComponentIndex component) =>
        component.ComponentIndexType switch {
            ComponentIndexType.BrepVertex => Counted<Brep>(geometry, component.Index, static brep => brep.Vertices.Count),
            ComponentIndexType.BrepEdge => Counted<Brep>(geometry, component.Index, static brep => brep.Edges.Count),
            ComponentIndexType.BrepFace => Counted<Brep>(geometry, component.Index, static brep => brep.Faces.Count),
            ComponentIndexType.BrepTrim => Counted<Brep>(geometry, component.Index, static brep => brep.Trims.Count),
            ComponentIndexType.BrepLoop => Counted<Brep>(geometry, component.Index, static brep => brep.Loops.Count),
            ComponentIndexType.MeshVertex => Counted<Mesh>(geometry, component.Index, static mesh => mesh.Vertices.Count),
            ComponentIndexType.MeshTopologyVertex => Counted<Mesh>(geometry, component.Index, static mesh => mesh.TopologyVertices.Count),
            ComponentIndexType.MeshTopologyEdge => Counted<Mesh>(geometry, component.Index, static mesh => mesh.TopologyEdges.Count),
            ComponentIndexType.MeshFace => Counted<Mesh>(geometry, component.Index, static mesh => mesh.Faces.Count),
            ComponentIndexType.MeshNgon => Counted<Mesh>(geometry, component.Index, static mesh => mesh.Ngons.Count),
            ComponentIndexType.PolycurveSegment => Counted<PolyCurve>(geometry, component.Index, static polycurve => polycurve.SegmentCount),
            ComponentIndexType.PointCloudPoint => Counted<PointCloud>(geometry, component.Index, static cloud => cloud.Count),
            ComponentIndexType.ExtrusionBottomProfile or ComponentIndexType.ExtrusionTopProfile or ComponentIndexType.ExtrusionWallSurface =>
                Counted<Extrusion>(geometry, component.Index, static extrusion => extrusion.ProfileCount),
            ComponentIndexType.ExtrusionWallEdge => Counted<Extrusion>(geometry, component.Index, static extrusion => 2 * extrusion.ProfileCount),
            ComponentIndexType.ExtrusionCapSurface => Held<Extrusion>(geometry, component.Index, static (extrusion, index) =>
                index is 0 ? extrusion.IsCappedAtBottom : index is 1 && extrusion.IsCappedAtTop),
            ComponentIndexType.ExtrusionPath => Held<Extrusion>(geometry, component.Index, static (_, index) => index is >= -1 and <= 1),
            ComponentIndexType.SubdVertex => Held<SubD>(geometry, component.Index, static (subd, id) => id > 0 && subd.Vertices.Find((uint)id) is not null),
            ComponentIndexType.SubdEdge => Held<SubD>(geometry, component.Index, static (subd, id) => id > 0 && subd.Edges.Find((uint)id) is not null),
            ComponentIndexType.SubdFace => Held<SubD>(geometry, component.Index, static (subd, id) => id > 0 && subd.Faces.Find((uint)id) is not null),
            ComponentIndexType.HatchLoop => Counted<Hatch>(geometry, component.Index, static hatch =>
                hatch.Get2dCurves(outer: true).Count(DisposalOps.Present) + hatch.Get2dCurves(outer: false).Count(DisposalOps.Present)),
            ComponentIndexType.InvalidType or ComponentIndexType.NoType or ComponentIndexType.InstanceDefinitionPart or ComponentIndexType.GroupMember
                or ComponentIndexType.DimLinearPoint or ComponentIndexType.DimRadialPoint or ComponentIndexType.DimAngularPoint or ComponentIndexType.DimOrdinatePoint
                or ComponentIndexType.DimTextPoint or ComponentIndexType.DimCentermarkPoint or ComponentIndexType.DimLeaderPoint => None,
        };

    private static Option<bool> Counted<T>(GeometryBase geometry, int index, Func<T, int> count) where T : GeometryBase =>
        Held<T>(geometry, index, (held, at) => at >= 0 && at < count(held));

    private static Option<bool> Held<T>(GeometryBase geometry, int index, Func<T, int, bool> holds) where T : GeometryBase =>
        geometry is T held ? Some(holds(held, index)) : None;

    // --- [OWNERSHIP]
    public static IO<TKept> Owned<TMade, TKept>(Func<TMade> create, Func<TMade, Fin<TKept>> accept, Func<TMade, IO<Unit>> release, Action<IDisposable> retain) =>
        Owned(create, product => IO.lift(() => accept(product)), release, retain);

    public static IO<TKept> Owned<TMade, TKept>(Func<TMade> create, Func<TMade, IO<TKept>> body, Func<TMade, IO<Unit>> release, Action<IDisposable> retain) =>
        DisposalOps.Acquire(owned => DisposalOps.Acquire(IO.lift(create), release, owned).Bind(body), retain);

    public static IO<TKept> Owned<TMade, TKept>(Func<TMade> create, Func<TMade, Fin<TKept>> accept, Action<IDisposable> retain) where TMade : IDisposable =>
        Owned(create, accept, static product => IO.lift(product.Dispose), retain);

    public static IO<TKept> Owned<TMade, TKept>(Func<TMade> create, Func<TMade, IO<TKept>> body, Action<IDisposable> retain) where TMade : IDisposable =>
        Owned(create, body, static product => IO.lift(product.Dispose), retain);

    private static IO<Unit> Released<T>(T?[]? products) where T : GeometryBase => DisposalOps.Release(Conversions.Rows(products));
}
