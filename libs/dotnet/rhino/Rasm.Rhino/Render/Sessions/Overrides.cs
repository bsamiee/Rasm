using System.Runtime.InteropServices;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.Render.Settings;
using Rasm.Rhino.UI;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Render;
using Rhino.Render.CustomRenderMeshes;
using Rhino.Render.ParameterNames;

namespace Rasm.Rhino.Render.Sessions;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct Albedo : System.Numerics.IMinMaxValue<Albedo> {
    public static Albedo MinValue { get; } = new(0f);
    public static Albedo MaxValue { get; } = new(1f);
    public static Albedo Clay { get; } = new(0.522522f);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

public sealed record RenderOverride(string Name, ValueSet Values, Option<Albedo> Clay);

public sealed record OverrideScope(RhinoDoc Document, BindingGroup Group, IPlugInViews Sink);

// --- [SERVICES] ------------------------------------------------------------------------
[Guid("1E61FAC0-CB0C-4245-A299-03464E5F6EDC")]
internal sealed class ClayMeshProvider(IPlugInViews sink, uint serial, RenderMaterial clay) : DefinedMeshProvider(sink, "Clay") {
    protected override IO<(bool Provides, Flags Flags)> Provides(MeshRequest request, RhinoDoc document, Guid objectId) =>
        IO.lift(() => (request.Display.IsNone && document.RuntimeSerialNumber == serial, request.Flags));

    protected override IO<MeshBuild> Build(MeshRequest request, RhinoDoc document, Guid objectId, Seq<InstanceObject> ancestry, Seq<Instance> previous, Action<IDisposable> retain) =>
        IO.lift(() => previous.Iter(instance => (instance.Material, instance.IsForcedMaterial) = (clay, true)))
            .Map(_ => new MeshBuild(previous, ContentKey.Of(KeyDomain.RenderMeshes, stream => stream.Integer(clay.RenderHash)), Flags: request.Flags));
}

public static class RenderOverrides {
    // --- [SCOPE]
    public static IO<T> Around<T>(RenderOverride over, OverrideScope scope, IO<T> launch) =>
        DisposalOps.Within(() => (Serial: scope.Document.RuntimeSerialNumber, Capture: new Capture(scope.Sink, scope.Group, Ref(Option<ValueSet>.None), Ref(Option<IDisposable>.None))) switch {
            var claim => Captures.Swap(held => held.Active.ContainsKey(claim.Serial) || held.Retiring.ContainsKey(claim.Serial) ? held : (held.Active.Add(claim.Serial, claim.Capture), held.Retiring))
                .Active.Find(claim.Serial).Exists(found => ReferenceEquals(found, claim.Capture))
                    ? Fin.Succ(claim)
                    : Fin.Fail<(uint Serial, Capture Capture)>(new OverrideHeld(claim.Serial)),
        }, answer =>
            from held in IO.lift(answer)
            from before in scope.Group.Capture
            from captured in atomicIO(() => held.Capture.Values.Swap(_ => Some(new ValueSet(before.Entries.Intersect(over.Values.Entries.Keys)))))
            from applied in Applied(over, scope, held.Serial, held.Capture)
            from launched in launch
            select launched, static answer => answer.Match(
                Succ: static held => Release(held.Serial, held.Capture),
                Fail: static _ => IO.pure(unit)));

    public static IO<BindingGroup> Group(BindingGroup scene) =>
        CyclesSetting.Bindings.Map(cycles => new BindingGroup(scene.Bindings + cycles.Bindings));

    public static Func<IPlugInSink, Action<IDisposable>, IO<Unit>> Register =>
        static (sink, retain) => DisposalOps.Acquire(
            IO.lift(() => new Disposal<IPlugInSink>(sink, static held => Released(held).Run())),
            static lease => IO.lift(lease.Dispose), retain).Map(unit);

    // --- [CAPTURE]
    private static readonly Atom<(HashMap<uint, Capture> Active, HashMap<uint, Capture> Retiring)> Captures = Atom((HashMap<uint, Capture>(), HashMap<uint, Capture>()));

    private static IO<Unit> Applied(RenderOverride over, OverrideScope scope, uint serial, Capture capture) =>
        from diff in scope.Group.Apply(over.Values, LanguageExt.HashSet.empty<EntryKey>())
        from clay in over.Clay.TraverseM(albedo => Clay(albedo, scope, lease =>
            atomic(() => {
                if (!Captures.Value.Active.ContainsKey(serial))
                    return Fin.Fail<Unit>(Errors.Cancelled);
                _ = capture.Clay.Swap(_ => Some(lease));
                return Fin.Succ(unit);
            }).ThrowIfFail())).As()
        select unit;

    private sealed record Capture(IPlugInSink Sink, BindingGroup Group, Ref<Option<ValueSet>> Values, Ref<Option<IDisposable>> Clay);

    // --- [RESTORE]
    private static IO<Unit> Release(uint serial, Capture capture) =>
        DisposalOps.Within(
            () => Captures.Swap(held => (held.Active.Remove(serial), held.Retiring.AddOrUpdate(serial, capture))),
            _ => Restored(serial, capture),
            _ => Captures.SwapIO(held => (held.Active, held.Retiring.Remove(serial))).Map(unit)).Uninterruptible();

    private static IO<Unit> Released(IPlugInSink sink) =>
        Captures.SwapIO(held => toSeq(held.Active.Filter(capture => capture.Sink == sink).Keys) switch {
            var serials => (held.Active.Except(serials), held.Retiring + held.Active.Intersect(serials)),
        }).Map(unit);

    private static IO<Unit> Restored(uint serial, Capture capture) =>
        Callbacks.Each(Seq(
            DisposalOps.Release(capture.Clay.Value.ToSeq()),
            IO.lift(() => Optional(RhinoDoc.FromRuntimeSerialNumber(serial)).Bind(_ => capture.Values.Value))
                .Bind(values => values.TraverseM(prior => capture.Group.Apply(prior, LanguageExt.HashSet.empty<EntryKey>())).As())
                .Map(unit))).Map(static _ => unit);

    // --- [CLAY]
    private static IO<Unit> Clay(Albedo albedo, OverrideScope scope, Action<IDisposable> retain) =>
        DisposalOps.Acquire(owned =>
            from material in DisposalOps.Acquire(
                IO.lift(() => RenderContentType.NewContentFromTypeId(ContentUuids.PhysicallyBasedMaterialType, scope.Document)),
                static content => DisposalOps.Release(Optional(content).ToSeq()),
                content => Optional(content).Iter(owned))
                .Bind(static content => IO.lift(() => Missing.Unless(content, nameof(RenderContentType.NewContentFromTypeId)).Bind(WrongType.Unless<RenderMaterial>)))
            from written in Written(material, albedo, scope.Document)
            from provider in DefinedMeshProvider.Register(sink => new ClayMeshProvider(sink, scope.Document.RuntimeSerialNumber, material))(scope.Sink, owned)
            select unit, (_, lease) => retain(lease));

    private static IO<Unit> Written(RenderMaterial material, Albedo albedo, RhinoDoc document) =>
        from encoded in DisposalOps.Within(() => document.RenderSettings,
            settings => DisposalOps.Within(() => settings.LinearWorkflow,
                workflow => IO.lift(() => MathF.Pow(albedo, workflow.PreProcessColors || workflow.PreProcessTextures ? 1f / workflow.PreProcessGamma : 1f)),
                static workflow => IO.lift(workflow.Dispose)),
            static settings => IO.lift(settings.Dispose))
        from written in DisposalOps.Within(
            () => { material.BeginChange(RenderContent.ChangeContexts.Program); return material; },
            held => IO.lift(() => Callbacks.Each(
                Seq<(string Name, object Value)>((PhysicallyBased.BaseColor, new Color4f(encoded, encoded, encoded, 1f)), (PhysicallyBased.Roughness, 0.5)),
                parameter => held.SetParameter(parameter.Name, parameter.Value),
                nameof(RenderContent.SetParameter))),
            static held => IO.lift(held.EndChange))
        select written;
}
