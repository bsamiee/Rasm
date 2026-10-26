using System.Globalization;
using Rhino.Commands;
using Rhino.Runtime;

namespace Rasm.Rhino.Modeling;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Answer<T>(Seq<IDisposable> Leaves, Validation<Seq<(string Reason, Option<int> Index)>, T> Value) : K<Answer, T> {
    public Answer<TResult> Map<TResult>(Func<T, TResult> f) => new(Leaves, Value.Map(f));
}

public sealed class Answer : Applicative<Answer> {
    // --- [PRODUCTS]
    public static Answer<T> One<T>(T? product) where T : class, IDisposable =>
        new(Conversions.Rows<IDisposable>([product]), Held(product, None, Refused.Absent));

    public static Answer<Option<T>> Optional<T>(T? product) where T : class, IDisposable =>
        product is null ? new([], Option<T>.None) : One(product).Map(Some);

    public static Answer<Seq<T>> Many<T>(IEnumerable<T?>? products) where T : class, IDisposable =>
        products is null ? Refuse<Seq<T>>(Refused.Absent) : toSeq(products).Strict() switch {
            var held => new(Conversions.Rows<IDisposable>(held), held.Map(static (product, index) => Held(product, Some(index), Refused.Declined)).Traverse(static product => product).As()),
        };

    public static Answer<Seq<T>> NonEmpty<T>(IEnumerable<T?>? products) where T : class, IDisposable =>
        Many(products) switch {
            var answer => answer with { Value = answer.Value.Bind(static held => held.IsEmpty ? Refusal<Seq<T>>(Refused.Absent, None) : held) },
        };

    public static Answer<Seq<Option<T>>> Sparse<T>(IEnumerable<T?>? products) where T : class, IDisposable =>
        products is null ? Refuse<Seq<Option<T>>>(Refused.Absent) : toSeq(products).Strict() switch {
            var held => new(Conversions.Rows<IDisposable>(held), held.Map(static (product, index) => Slot(product, index)).Traverse(static product => product).As()),
        };

    public static Answer<T> Successor<TTemporary, T>(TTemporary? temporary, Func<TTemporary, Answer<T>> successor) where TTemporary : class, IDisposable {
        using TTemporary? held = temporary;
        return held is null ? Refuse<T>(Refused.Absent) : successor(held);
    }

    public static Answer<Seq<TRow>> Rows<TRow>(IEnumerable<TRow>? rows, Func<TRow, Option<IDisposable>> leaf) =>
        rows is null ? Refuse<Seq<TRow>>(Refused.Absent) : toSeq(rows).Map(row => (Row: row, Leaf: leaf(row))).Strict() switch {
            var held => new(held.Bind(static row => row.Leaf.ToSeq()), held.Map(static (row, index) => row.Leaf.Match(
                Some: product => Held(product, Some(index), Refused.Declined).Map(_ => row.Row),
                None: () => row.Row)).Traverse(static row => row).As()),
        };

    // --- [VERDICTS]
    public static Answer<Unit> Accepted(bool accepted, string? text = null) =>
        accepted ? new([], unit) : Refuse<Unit>(Conversions.Present(text).IfNone(Refused.Declined));

    public static Answer<Unit> Ended(Result result) =>
        result is Result.Success ? new([], unit) : Refuse<Unit>($"ended with {result}");

    public static Answer<Unit> Reported(string? text) =>
        Conversions.Present(text).Match(Some: Refuse<Unit>, None: static () => new Answer<Unit>([], unit));

    // --- [PROVENANCE]
    public static Answer<Seq<Grouped<T, int>>> Keyed<T>(Answer<Seq<T>> outputs, int[]? key) =>
        (outputs, Found(key)).Apply(static (held, owners) => toSeq(owners).Map(static (owner, input) => (Owner: owner, Input: input)).ToLookup(static entry => entry.Owner, static entry => entry.Input) switch {
            var inputs => held.Map((output, index) => new Grouped<T, int>(output, toSeq(inputs[index]))).Strict(),
        }).As();

    public static Answer<Seq<Grouped<T, int>>> Mapped<T>(Answer<Seq<T>> outputs, IEnumerable<int[]>? map) =>
        new(outputs.Leaves, outputs.Value.Bind(held => toSeq(map ?? []) switch {
            var inputs when inputs.Count == held.Count => held.Zip(inputs, static (output, from) => new Grouped<T, int>(output, toSeq(from))).Strict(),
            _ => Refusal<Seq<Grouped<T, int>>>(Refused.Absent, None),
        }));

    public static Answer<Seq<T>> Diagnosed<T>(IEnumerable<T?>? outputs, Point3d[] naked, Point3d[] bad, Point3d[] nonManifold) where T : class, IDisposable =>
        outputs is null
            ? Refuse<Seq<T>>($"{Refused.Absent}, naked edges at [{string.Join("; ", naked)}], bad intersections at [{string.Join("; ", bad)}], non-manifold edges at [{string.Join("; ", nonManifold)}]")
            : Many(outputs);

    // --- [TRAITS]
    static K<Answer, TResult> Functor<Answer>.Map<T, TResult>(Func<T, TResult> f, K<Answer, T> ma) => ma.As().Map(f);

    static K<Answer, T> Applicative<Answer>.Pure<T>(T value) => new Answer<T>([], value);

    static K<Answer, TResult> Applicative<Answer>.Apply<T, TResult>(K<Answer, Func<T, TResult>> mf, K<Answer, T> ma) => Applied(mf.As(), ma.As());

    static K<Answer, TResult> Applicative<Answer>.Apply<T, TResult>(K<Answer, Func<T, TResult>> mf, Memo<Answer, T> ma) => Applied(mf.As(), ma.Value.As());

    private static Answer<TResult> Applied<T, TResult>(Answer<Func<T, TResult>> mf, Answer<T> ma) => new(mf.Leaves + ma.Leaves, mf.Value.Apply(ma.Value));

    // --- [VALIDITY]
    private static Validation<Seq<(string Reason, Option<int> Index)>, T> Held<T>(T? product, Option<int> index, string absent) where T : class, IDisposable =>
        product switch {
            null => Refusal<T>(absent, index),
            CommonObject geometry when !geometry.IsValidWithLog(out string log) => Refusal<T>($"returned invalid {product.GetType().Name}. {log}", index),
            _ => product,
        };

    private static Validation<Seq<(string Reason, Option<int> Index)>, Option<T>> Slot<T>(T? product, int index) where T : class, IDisposable =>
        product is null ? Option<T>.None : Held(product, Some(index), Refused.Declined).Map(Some);

    private static Answer<T> Found<T>(T? value) where T : class =>
        value is null ? Refuse<T>(Refused.Absent) : new([], value);

    private static Answer<T> Refuse<T>(string reason) => new([], Refusal<T>(reason, None));

    private static Validation<Seq<(string Reason, Option<int> Index)>, T> Refusal<T>(string reason, Option<int> index) =>
        Seq((Reason: reason, Index: index));
}

public sealed record Grouped<T, TFrom>(T Value, Seq<TFrom> From);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Copies {
    // --- [COPIES]
    public static IO<T> Duplicate<T>(T source, Action<IDisposable> retain) where T : GeometryBase =>
        Owned(() => (T)source.Duplicate(), static copy => Fin.Succ(copy), retain);

    public static IO<T> DuplicateShallow<T>(T source, Action<IDisposable> retain) where T : GeometryBase =>
        Owned(() => (T)source.DuplicateShallow(), static copy => Fin.Succ(copy), retain);

    // --- [ACQUISITION]
    public static IO<T> Acquire<T>(string member, Func<CancellationToken, Answer<T>> host, Action<IDisposable> retain) =>
        from token in cancelToken
        from value in DisposalOps.Acquire(owned =>
            from answer in DisposalOps.Acquire(IO.lift(() => host(token)), static answer => DisposalOps.Release(answer.Leaves), owned)
            from accepted in IO.lift(() => answer.Value.Match<Fin<T>>(
                Fail: refusals => token.IsCancellationRequested
                    ? Errors.Cancelled
                    : Error.Many(refusals.Map<Error>(refusal => refusal.Index.Match(
                        Some: index => new Refused(member, refusal.Reason, index),
                        None: () => new Refused(member, refusal.Reason)))),
                Succ: static value => value))
            select accepted, retain)
        select value;

    extension<T>(K<Answer, T> answer) {
        public Answer<T> As() => (Answer<T>)answer;
    }

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
}
