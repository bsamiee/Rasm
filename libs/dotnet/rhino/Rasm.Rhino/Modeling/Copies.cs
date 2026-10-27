using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Rasm.Rhino.Modeling;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Grouped<T, TFrom>(T Value, Seq<TFrom> From);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Copies {
    // --- [COPIES]
    public static IO<T> Duplicate<T>(T source) where T : GeometryBase => use(() => (T)source.Duplicate());

    // --- [PROVENANCE]
    public static IO<Seq<Grouped<T, int>>> Keyed<T>(IO<Seq<T>> outputs, int[]? key, [CallerArgumentExpression(nameof(key))] string call = "") =>
        outputs.Bind(held => IO.lift(() => Conversions.Found(key, call: call).Bind(owners =>
            Callbacks.Each(toSeq(owners).Map(static (owner, input) => (Owner: owner, Input: input)).Filter(static entry => entry.Owner is not -1), (entry, _) =>
                Conversions.Within(entry.Owner, held.Count).Map(_ => entry))).Map(entries =>
            held.Map((output, index) => new Grouped<T, int>(output, entries.Filter(entry => entry.Owner == index).Map(static entry => entry.Input))).Strict())));

    public static IO<Seq<Grouped<T, int>>> Mapped<T>(IO<Seq<T>> outputs, IEnumerable<int[]>? map, [CallerArgumentExpression(nameof(map))] string call = "") =>
        outputs.Bind(held => IO.lift(() =>
            from inputs in Conversions.Found(map, call: call).Map(toSeq)
            from matched in RhinoFault.Unless(inputs.Count == held.Count, call: call)
            select held.Zip(inputs, static (output, sources) => new Grouped<T, int>(output, toSeq(sources))).Strict()));

    // --- [INDICES]
    public static Fin<Seq<ComponentIndex>> Within(GeometryBase geometry, Seq<ComponentIndex> components) =>
        Callbacks.Each(components, (component, at) => Holds(geometry, component).Match(
            Some: held => held
                ? Fin.Succ(component)
                : new RhinoFault(string.Create(CultureInfo.InvariantCulture, $"{component.ComponentIndexType} {component.Index} of {geometry.ObjectType}"), RhinoReason.Outside, at),
            None: () => new RhinoFault($"{component.ComponentIndexType} of {geometry.ObjectType}", RhinoReason.Mismatched, at)));

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
                Seq(hatch.Get2dCurves(outer: true), hatch.Get2dCurves(outer: false)).Sum(static loops => loops.Count(static curve => {
                    using Curve? held = curve;
                    return held is not null;
                }))),
            ComponentIndexType.InvalidType or ComponentIndexType.NoType or ComponentIndexType.InstanceDefinitionPart or ComponentIndexType.GroupMember
                or ComponentIndexType.DimLinearPoint or ComponentIndexType.DimRadialPoint or ComponentIndexType.DimAngularPoint or ComponentIndexType.DimOrdinatePoint
                or ComponentIndexType.DimTextPoint or ComponentIndexType.DimCentermarkPoint or ComponentIndexType.DimLeaderPoint => None,
        };

    private static Option<bool> Counted<T>(GeometryBase geometry, int index, Func<T, int> count) where T : GeometryBase =>
        Held<T>(geometry, index, (held, at) => at >= 0 && at < count(held));

    private static Option<bool> Held<T>(GeometryBase geometry, int index, Func<T, int, bool> holds) where T : GeometryBase =>
        geometry is T held ? Some(holds(held, index)) : None;
}
