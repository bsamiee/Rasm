using System.Runtime.CompilerServices;

namespace Rasm.Rhino;

// --- [MODELS] --------------------------------------------------------------------------

public sealed record Step<T>(Func<T, IO<T>> Run, object Subject) where T : class;

public sealed record Stage<T>(Func<T, IO<T>> Copy, Func<T, T, bool> Unchanged, Func<T, T, IO<T>> Commit) where T : class;

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class Steps {

    // --- [STEPS]

    public static Step<T> Of<T>(Func<T, bool> call, [CallerMemberName] string member = "", [CallerLineNumber] int line = 0) where T : class =>
        new(target => IO.lift(() => call(target) ? Fin.Succ(target) : Fault.Of(RhinoReason.Declined, (member, line))), (member, line));

    public static Step<T> Then<T>(Func<T, T?> call, [CallerMemberName] string member = "", [CallerLineNumber] int line = 0) where T : class =>
        new(target => Products.Of(() => call(target), member, line), (member, line));

    public static Step<T> Derived<T>(Func<T, IO<T>> run, [CallerMemberName] string member = "", [CallerLineNumber] int line = 0) where T : class => new(run, (member, line));

    public static Step<T> Count<T>(Func<T, long> call, Func<long, bool> accepts, [CallerMemberName] string member = "", [CallerLineNumber] int line = 0) where T : class =>
        new(target => IO.lift(() => call(target) switch {
            var count => Fault.Unless(accepts(count), RhinoReason.Declined, count).Map(_ => target),
        }), (member, line));

    public static Step<T> At<T>(ComponentIndexType kind, Seq<int> indices, Func<T, Seq<int>, bool> call, [CallerMemberName] string member = "", [CallerLineNumber] int line = 0) where T : GeometryBase =>
        new(target => IO.lift(() => target.Within(kind, indices).Bind(held => call(target, held) ? Fin.Succ(target) : Fault.Of(RhinoReason.Declined, (member, line)))), (member, line));

    // --- [COMPONENTS]

    extension(GeometryBase geometry) {
        public Fin<Seq<ComponentIndex>> Within(Seq<ComponentIndex> components) =>
            Products.Each(components, (component, at) => Holds(geometry, component).Match(
                Some: held => held
                    ? Fin.Succ(component)
                    : Fault.Of(RhinoReason.Outside, (Component: component, Geometry: geometry.ObjectType), at),
                None: () => Fault.Of(RhinoReason.Mismatched, (Component: component.ComponentIndexType, Geometry: geometry.ObjectType), at)));

        public Fin<Seq<int>> Within(ComponentIndexType kind, Seq<int> indices) =>
            geometry.Within(indices.Map(index => new ComponentIndex(kind, index))).Map(static held => held.Map(static component => component.Index));
    }

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
            ComponentIndexType.ExtrusionCapSurface => Tested<Extrusion>(geometry, component.Index, static (extrusion, index) =>
                index is 0 ? extrusion.IsCappedAtBottom : index is 1 && extrusion.IsCappedAtTop),
            ComponentIndexType.ExtrusionPath => Tested<Extrusion>(geometry, component.Index, static (_, index) => index is >= -1 and <= 1),
            ComponentIndexType.SubdVertex or ComponentIndexType.SubdEdge or ComponentIndexType.SubdFace =>
                Tested<SubD>(geometry, component.Index, (subd, id) => id > 0 && subd.ComponentFromComponentIndex(component) is not null),
            ComponentIndexType.HatchLoop => Counted<Hatch>(geometry, component.Index, static hatch =>
                Seq(hatch.Get2dCurves(outer: true), hatch.Get2dCurves(outer: false)).Sum(static loops => loops.Count(static curve => {
                    using Curve? held = curve;
                    return held is not null;
                }))),
            ComponentIndexType.InvalidType or ComponentIndexType.NoType or ComponentIndexType.InstanceDefinitionPart or ComponentIndexType.GroupMember
                or ComponentIndexType.DimLinearPoint or ComponentIndexType.DimRadialPoint or ComponentIndexType.DimAngularPoint or ComponentIndexType.DimOrdinatePoint
                or ComponentIndexType.DimTextPoint or ComponentIndexType.DimCentermarkPoint or ComponentIndexType.DimLeaderPoint => None,
        };

    private static Option<bool> Counted<T>(GeometryBase geometry, int index, Func<T, int> count) where T : GeometryBase =>
        Tested<T>(geometry, index, (held, at) => at >= 0 && at < count(held));

    private static Option<bool> Tested<T>(GeometryBase geometry, int index, Func<T, int, bool> holds) where T : GeometryBase =>
        geometry is T held ? Some(holds(held, index)) : None;
}

public static class Stages {

    // --- [IN_PLACE]

    public static Stage<T> InPlace<T>() where T : class => new(Copy: IO.pure, Unchanged: ReferenceEquals, Commit: static (_, staged) => IO.pure(staged));

    // --- [GEOMETRY]

    public static Stage<T> Geometry<T>() where T : GeometryBase =>
        new(
            Copy: static live =>
                from copy in Products.Copied(live)
                from cached in IO.lift(() => (Copy: copy, Live: live) switch {
                    (Copy: SubD edited, Live: SubD original) => ignore(edited.CopyEvaluationCache(in original)),
                    _ => unit,
                })
                select copy,
            Unchanged: static (_, _) => false,
            Commit: static (_, staged) =>
                from subd in IO.pure(Optional(staged as SubD))
                from tagged in IO.lift(() => subd.Map(static held => held.UpdateAllTagsAndSectorCoefficients()))
                from cleared in IO.lift(() => subd.Iter(static held => held.ClearEvaluationCache()))
                from valid in IO.lift(staged.IsValidWithLog(out string log) ? Fin.Succ(staged) : Fault.Of(RhinoReason.Invalid, staged.ObjectType, inner: Error.New(log)))
                select valid);
}

public static class Edits {

    // --- [APPLICATION]

    public static IO<T> Apply<T>(Stage<T> stage, T live, Seq<Step<T>> steps) where T : class =>
        from staged in stage.Copy(live)
        from edited in steps.Map(static (step, index) => (Step: step, Index: index)).FoldM(staged, static (current, entry) =>
            entry.Step.Run(current).Catch(error => IO.fail<T>(Indexed(error, entry.Step.Subject, entry.Index)))).As()
        from committed in stage.Unchanged(live, edited) ? IO.pure(live) : stage.Commit(live, edited)
        select committed;

    private static Fault<RhinoReason> Indexed(Error error, object subject, int index) =>
        error.Count is 1 && error.Filter<Fault<RhinoReason>>().Head is Fault<RhinoReason> fault
            ? fault with { Index = index }
            : Fault.Of(RhinoReason.Declined, subject, index, error);
}
