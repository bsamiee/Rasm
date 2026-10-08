using System.Runtime.InteropServices;
using Rasm.Rhino.Objects.Shading;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.Render.Scenes;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.PlugIns;
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

public sealed record OverrideScope(RhinoDoc Document, BindingGroup Group, PlugIn PlugIn, IPlugInSink Sink);

// --- [SERVICES] ------------------------------------------------------------------------
[Guid("1E61FAC0-CB0C-4245-A299-03464E5F6EDC")]
internal sealed class ClayMeshProvider(PlugIn plugIn, IPlugInSink sink, uint serial, RenderMaterial clay) : DefinedMeshProvider(plugIn, sink, "Clay") {
    protected override IO<bool> Provides(MeshRequest request, RhinoDoc document, Guid objectId) =>
        IO.lift(() => request.Display.IsNone && document.RuntimeSerialNumber == serial);

    protected override IO<MeshBuild> Build(MeshRequest request, RhinoDoc document, Guid objectId, Seq<InstanceObject> ancestry, Seq<Instance> previous) =>
        IO.lift(() => previous.Iter(instance => (instance.Material, instance.IsForcedMaterial) = (clay, true)))
            .Map(_ => new MeshBuild(previous, ContentKey.Of(KeyDomain.RenderMeshes, stream => stream.Integer(clay.RenderHash)), Incomplete: false));
}

public static class RenderOverrides {
    // --- [SCOPE]
    public static IO<A> Around<A>(RenderOverride over, OverrideScope scope, IO<A> launch) =>
        Acquired(over, scope).Bracket(Use: _ => IO.pure(unit).Bind(_ => launch), Fin: Release);

    public static Validation<Error, BindingGroup> Group(BindingGroup scene) =>
        CyclesSetting.Bindings.Bind(application => BindingGroup.Of(scene.Bindings + application.Bindings));

    public static Func<PlugIn, IPlugInSink, IO<IDisposable>> Register =>
        static (_, sink) => IO.lift<IDisposable>(() => new Disposal<IPlugInSink>(
            sink, static held => Callbacks.Succeeded(ReleasedAll, new CallbackSite(held, typeof(RenderOverrides), nameof(Register)))));

    // --- [CAPTURE]
    private static readonly Atom<Registry> Captures = Atom(Registry.Empty);

    private static IO<uint> Acquired(RenderOverride over, OverrideScope scope) =>
        from before in scope.Group.Capture
        let fresh = new Capture(scope.Group, new ValueSet(before.Entries.Intersect(over.Values.Entries.Keys)), None)
        from serial in IO.lift(() => OverrideHeld.Unless(
            Captures.Swap(registry => registry.Claimed(scope.Document.RuntimeSerialNumber, fresh)).Held.Find(scope.Document.RuntimeSerialNumber).Exists(held => ReferenceEquals(held, fresh)),
            scope.Document.RuntimeSerialNumber))
        from applied in DisposalOps.OnFailure(Applied(over, scope, serial), Release(serial))
        select serial;

    private static IO<Unit> Applied(RenderOverride over, OverrideScope scope, uint serial) =>
        from diff in scope.Group.Apply(over.Values, LanguageExt.HashSet.empty<EntryKey>())
        from clay in over.Clay.Traverse(albedo => Clay(albedo, scope)).As()
        from attached in IO.lift(() => Captures.Swap(registry => registry.Attached(serial, clay)))
        select unit;

    private sealed record Capture(BindingGroup Group, ValueSet Values, Option<IDisposable> Clay);

    private sealed record Registry(HashMap<uint, Capture> Held, Seq<(uint Serial, Capture Capture)> Taken) {
        public static Registry Empty { get; } = new(HashMap<uint, Capture>(), []);

        public Registry Claimed(uint serial, Capture fresh) => new(Held.TryAdd(serial, fresh), []);

        public Registry Attached(uint serial, Option<IDisposable> clay) => new(Held.TrySetItem(serial, held => held with { Clay = clay }), []);

        public Registry Released(Seq<uint> serials) => new(Held.Except(serials), serials.Choose(serial => Held.Find(serial).Map(capture => (serial, capture))));
    }

    // --- [RESTORE]
    private static IO<Unit> Release(uint serial) => Released(_ => [serial]);

    private static IO<Unit> ReleasedAll => Released(static held => toSeq(held.Keys));

    private static IO<Unit> Released(Func<HashMap<uint, Capture>, Seq<uint>> serials) =>
        IO.lift(() => Captures.Swap(registry => registry.Released(serials(registry.Held))).Taken)
            .Bind(static taken => Callbacks.Each(taken.Map(static held => Restored(held.Serial, held.Capture))))
            .Map(static _ => unit);

    private static IO<Unit> Restored(uint serial, Capture capture) =>
        from released in DisposalOps.Release(capture.Clay.ToSeq())
        from document in IO.lift(() => Optional(RhinoDoc.FromRuntimeSerialNumber(serial)))
        from restored in document.Traverse(open => capture.Group.Apply(capture.Values, LanguageExt.HashSet.empty<EntryKey>())).As()
        select unit;

    // --- [CLAY]
    private static IO<IDisposable> Clay(Albedo albedo, OverrideScope scope) =>
        from material in Material(albedo, scope.Document)
        from provider in DisposalOps.OnFailure(
            DefinedMeshProvider.Register((plugIn, sink) => new ClayMeshProvider(plugIn, sink, scope.Document.RuntimeSerialNumber, material))(scope.PlugIn, scope.Sink),
            IO.lift(material.Dispose))
        select DisposalOps.Composite([material, provider], new CallbackSite(scope.Sink, typeof(RenderOverrides), nameof(Clay)));

    private static IO<RenderMaterial> Material(Albedo albedo, RhinoDoc document) =>
        from material in IO.lift(() => Missing.Unless(
            RenderContentType.NewContentFromTypeId(ContentUuids.PhysicallyBasedMaterialType, document) as RenderMaterial, nameof(RenderContentType.NewContentFromTypeId)))
        from written in DisposalOps.OnFailure(Written(material, albedo, document), IO.lift(material.Dispose))
        select material;

    private static IO<Unit> Written(RenderMaterial material, Albedo albedo, RhinoDoc document) =>
        from settings in IO.lift(() => document.RenderSettings)
        from workflow in IO.lift(() => settings.LinearWorkflow)
        from encoded in IO.pure<Seq<IDisposable>>([settings, workflow]).Bracket(
            Use: held => IO.lift(() => MathF.Pow(albedo, workflow.PreProcessColors || workflow.PreProcessTextures ? 1f / workflow.PreProcessGamma : 1f)),
            Fin: DisposalOps.Release)
        from written in IO.lift(() => material.BeginChange(RenderContent.ChangeContexts.Program)).Bracket(
            Use: held => IO.lift(() => Callbacks.Each(
                Seq<(string Name, object Value)>((PhysicallyBased.BaseColor, new Color4f(encoded, encoded, encoded, 1f)), (PhysicallyBased.Roughness, 0.5)),
                parameter => material.SetParameter(parameter.Name, parameter.Value),
                nameof(RenderContent.SetParameter))),
            Fin: held => IO.lift(material.EndChange))
        select written;
}
