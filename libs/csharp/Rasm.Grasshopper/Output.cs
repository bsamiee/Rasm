namespace Rasm.Grasshopper;

public interface IOutputGroup {
    public Seq<IPort> Ports { get; }
    public Unit Run(IDataAccess access, int slot, GrasshopperRuntime runtime);
    public Unit Empty(IDataAccess access, int slot);
}
public readonly record struct Hints(
    Seq<(IPort Port, int Slot)> Inputs) {
    public static Hints Capture(Seq<IPort> inputs, IDataAccess access) =>
        new(Inputs: inputs.Map((port, slot) => (Port: port, Slot: slot)));
    public Option<int> Slot(IPort port) => Inputs.Find(predicate: input => input.Port.Equals(port)).Map(static input => input.Slot);
    public Option<int> Index(IDataAccess access, Port<int> port, int limit) {
        ArgumentNullException.ThrowIfNull(argument: access);
        return Slot(port: port).Bind(slot => access.GetIndex(indexParameter: slot, limit: limit, index: out int value) ? Some(value) : Option<int>.None);
    }
    public Option<TVal> Value<TVal>(IDataAccess access, Port<TVal> port) {
        ArgumentNullException.ThrowIfNull(argument: access);
        return Inputs.Find(predicate: input => input.Port.Equals(port)).Bind(input => Bridge.Read<TVal>(access: access, slot: input.Slot, port: port).ToOption().Bind(static values => values.Head));
    }
}
internal readonly record struct OutputSlot<TSource>(
    IPort Port,
    Func<IDataAccess, int, GrasshopperRuntime, Seq<TSource>, Unit> Write,
    Func<IDataAccess, int, Unit> Empty);
internal sealed record PreparedGroup<TSource>(
    Seq<OutputSlot<TSource>> Slots,
    Func<IDataAccess, GrasshopperRuntime, Fin<Seq<TSource>>> Source,
    bool EmptyUnsupported) : IOutputGroup {
    public Seq<IPort> Ports => Slots.Map(static slot => slot.Port);
    public Unit Run(IDataAccess access, int slot, GrasshopperRuntime runtime) {
        ArgumentNullException.ThrowIfNull(argument: access);
        return Source(arg1: access, arg2: runtime).Match(
            Succ: values => Slots.Iter((offset, output) => output.Write(arg1: access, arg2: slot + offset, arg3: runtime, arg4: values)),
            Fail: error => (
                (EmptyUnsupported, error.Code == OpFault.UnsupportedCode) switch {
                    (true, true) => Unit.Default,
                    _ => fun((IDataAccess target) => { target.AddWarning(text: Ports.Head.Map(static port => port.Name).IfNone("Output"), details: error.Message); return Unit.Default; })(access),
                },
                Empty(access: access, slot: slot)).Item2);
    }
    public Unit Empty(IDataAccess access, int slot) {
        ArgumentNullException.ThrowIfNull(argument: access);
        return Slots.Iter((offset, output) => output.Empty(arg1: access, arg2: slot + offset));
    }
}
public static class Output {
    private static OutputSlot<TSource> Slot<TSource, TOut>(
        Port<TOut> port,
        Func<GrasshopperRuntime, Seq<TSource>, Fin<Seq<TOut>>> project) =>
        new(
            Port: port,
            Write: (access, slot, runtime, source) => project(arg1: runtime, arg2: source).Match(
                Succ: values => Bridge.Write(access: access, slot: slot, name: port.Name, targetAccess: port.Access, values: values),
                Fail: error => {
                    access.AddWarning(text: port.Name, details: error.Message);
                    return Bridge.Write(access: access, slot: slot, name: port.Name, targetAccess: port.Access, values: Seq<TOut>());
                }),
            Empty: (access, slot) => Bridge.Write(access: access, slot: slot, name: port.Name, targetAccess: port.Access, values: Seq<TOut>()));
    private static PreparedGroup<TSource> Prepared<TSource>(
        Func<IDataAccess, GrasshopperRuntime, Fin<Seq<TSource>>> source,
        bool emptyUnsupported = false,
        params OutputSlot<TSource>[] slots) => new(Slots: toSeq(slots), Source: source, EmptyUnsupported: emptyUnsupported);
    public static Unit Write(IDataAccess access, GrasshopperRuntime runtime, Seq<IOutputGroup> groups) =>
        Fold(groups: groups, action: group => slot => group.Run(access: access, slot: slot, runtime: runtime));
    public static Unit Empty(IDataAccess access, Seq<IOutputGroup> groups) =>
        Fold(groups: groups, action: group => slot => group.Empty(access: access, slot: slot));
    private static Unit Fold(Seq<IOutputGroup> groups, Func<IOutputGroup, Func<int, Unit>> action) =>
        groups.Fold(initialState: 0, f: (slot, group) => (action(arg: group)(arg: slot), slot + group.Ports.Count).Item2) switch { _ => Unit.Default };
    public static IOutputGroup Query<TOut>(Port<Shape> input, Port<TOut> port, Func<IDataAccess, GrasshopperRuntime, Query<object, TOut>> operation, bool emptyUnsupported = false) =>
        Prepared(
            source: (access, runtime) => ShapeSource(input: input, access: access, runtime: runtime, project: shape => operation(arg1: access, arg2: runtime).Apply(geometry: shape.Inner)),
            emptyUnsupported: emptyUnsupported,
            slots: [Slot<TOut, TOut>(port: port, project: static (_, values) => Fin.Succ(values))]);
    public static IOutputGroup CurveDetails(Port<Shape> input, Port<Curve> curves, Port<ComponentIndex> sources, Port<CurveFeature> features, Func<IDataAccess, GrasshopperRuntime, Curves> aspect, bool emptyUnsupported = false) =>
        Prepared(
            source: (access, runtime) => ShapeSource(input: input, access: access, runtime: runtime, project: shape => Rasm.Analysis.Query.CurveProjections(geometry: shape.Inner, aspect: aspect(arg1: access, arg2: runtime))),
            emptyUnsupported: emptyUnsupported,
            slots: [
                Plain<CurveProjection, Curve>(port: curves, project: static value => value.Curve),
                Plain<CurveProjection, ComponentIndex>(port: sources, project: static value => value.Source),
                Plain<CurveProjection, CurveFeature>(port: features, project: static value => value.Feature),
            ]);
    public static IOutputGroup FaceDetails(Port<Shape> input, Port<Brep> faces, Port<int> indices, Func<IDataAccess, GrasshopperRuntime, Faces> selector) =>
        Prepared(
            source: (access, runtime) => ShapeSource(input: input, access: access, runtime: runtime, project: shape => Rasm.Analysis.Query.FaceProjections(geometry: shape.Inner, selector: selector(arg1: access, arg2: runtime))),
            slots: [
                Plain<FaceProjection, Brep>(port: faces, project: static value => value.Brep),
                Plain<FaceProjection, int>(port: indices, project: static value => value.FaceIndex),
            ]);
    public static IOutputGroup IndexedFaceDetails(Port<Shape> input, Port<int> index, Port<Brep> faces, Port<Plane> frames, Port<Point3d> centers, Port<Vector3d> normals, Port<int> indices, Port<ComponentIndex> components, Port<Interval> domains) =>
        Prepared(
            source: (access, runtime) => ShapeSource(input: input, access: access, runtime: runtime, project: shape => Rasm.Analysis.Query.FaceProjections(
                geometry: shape.Inner,
                selector: Faces.At(index: runtime.Hints.Index(access: access, port: index, limit: int.MaxValue).Map(static value => (int?)value).IfNone(static () => null)))),
            slots: [
                Plain<FaceProjection, Brep>(port: faces, project: static value => value.Brep),
                FaceValue(port: frames, project: static (face, context) => Rasm.Analysis.Query.FrameAtCentroid(face: face, runtime: context)),
                FaceValue(port: centers, project: static (face, context) => Rasm.Analysis.Query.FaceCentroid(face: face, runtime: context)),
                FaceValue(port: normals, project: static (face, context) => Rasm.Analysis.Query.FrameAtCentroid(face: face, runtime: context).Map(static frame => frame.ZAxis)),
                Plain<FaceProjection, int>(port: indices, project: static value => value.FaceIndex),
                Plain<FaceProjection, ComponentIndex>(port: components, project: static value => new ComponentIndex(type: ComponentIndexType.BrepFace, index: value.FaceIndex)),
                Slot<FaceProjection, Interval>(port: domains, project: static (_, values) => values.Traverse(Rasm.Analysis.Query.FaceDomains).Map(static nested => nested.Bind(static domains => domains)).As()),
            ]);
    private static Fin<Seq<TSource>> ShapeSource<TSource>(Port<Shape> input, IDataAccess access, GrasshopperRuntime runtime, Func<Shape, Eff<Analyze.Runtime, Seq<TSource>>> project) =>
        from shape in runtime.Shape(access: access, port: input)
        from context in runtime.Scope.Context
        from values in project(arg: shape).Run(env: new Analyze.Runtime(Context: context, Cancellation: access.Solution.Token, Progress: new Bridge.Progress(access: access)))
        select values;
    private static OutputSlot<TSource> Plain<TSource, TOut>(Port<TOut> port, Func<TSource, TOut> project) =>
        Slot<TSource, TOut>(port: port, project: (_, values) => Fin.Succ(values.Map(value => project(arg: value))));
    private static OutputSlot<FaceProjection> FaceValue<TOut>(Port<TOut> port, Func<FaceProjection, Context, Fin<TOut>> project) =>
        Slot<FaceProjection, TOut>(port: port, project: (runtime, values) => runtime.Scope.Context
                .Bind(context => values.Traverse(face => project(arg1: face, arg2: context)).As()));
}
