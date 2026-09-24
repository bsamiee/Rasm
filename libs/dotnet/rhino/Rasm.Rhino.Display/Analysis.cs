using Rasm.Rhino.Document;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.UI;

namespace Rasm.Rhino.Display;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AnalysisKind {
    public sealed record FalseColor(Func<RhinoObject, Mesh, IO<Unit>> Colors) : AnalysisKind;

    public sealed record Texture(Func<RhinoObject, DisplayPipelineAttributes, IO<Unit>> SetUp) : AnalysisKind;

    public sealed record Wireframe() : AnalysisKind;
}

public sealed record AnalysisCallbacks(
    LocalizeStringPair Name,
    AnalysisKind Kind,
    bool ShowIsoCurves,
    Option<Func<RhinoObject, IO<bool>>> Supports,
    Option<Func<RhinoObject, Mesh, DisplayPipeline, IO<Unit>>> DrawMesh,
    Action<Error> Reject);

// --- [SERVICES] ------------------------------------------------------------------------
public abstract class CallbackAnalysisMode(AnalysisCallbacks callbacks) : VisualAnalysisMode {
    public sealed override string Name => callbacks.Name.Local;

    public sealed override AnalysisStyle Style =>
        callbacks.Kind.Map(falseColor: AnalysisStyle.FalseColor, texture: AnalysisStyle.Texture, wireframe: AnalysisStyle.Wireframe);

    public sealed override bool ShowIsoCurves => callbacks.ShowIsoCurves;

    public sealed override bool ObjectSupportsAnalysisMode(RhinoObject obj) =>
        Answers.Answer(callbacks.Supports.Map(supports => supports(obj)), callbacks.Reject, refused: false, () => base.ObjectSupportsAnalysisMode(obj));

    protected sealed override void SetUpDisplayAttributes(RhinoObject obj, DisplayPipelineAttributes attributes) {
        base.SetUpDisplayAttributes(obj, attributes);
        _ = Answers.Answer(
            callbacks.Kind.Switch(
                (Object: obj, Attributes: attributes),
                falseColor: static (target, _) => IO.lift(() => { target.Attributes.ShadeVertexColors = true; }),
                texture: static (target, texture) => texture.SetUp(target.Object, target.Attributes),
                wireframe: static (_, _) => IO.pure(unit)),
            callbacks.Reject,
            unit);
    }

    protected sealed override void UpdateVertexColors(RhinoObject obj, Mesh[] meshes) {
        base.UpdateVertexColors(obj, meshes);
        _ = callbacks.Kind
            .Switch(
                (Object: obj, Meshes: meshes),
                falseColor: static (target, falseColor) => toSeq(target.Meshes).Map(mesh => falseColor.Colors(target.Object, mesh)),
                texture: static (_, _) => Seq<IO<Unit>>(),
                wireframe: static (_, _) => Seq<IO<Unit>>())
            .Iter(color => Answers.Answer(color, callbacks.Reject, unit));
    }

    protected sealed override void DrawMesh(RhinoObject obj, Mesh mesh, DisplayPipeline pipeline) {
        base.DrawMesh(obj, mesh, pipeline);
        _ = Answers.Answer(callbacks.DrawMesh.Map(draw => draw(obj, mesh, pipeline)), callbacks.Reject, static () => unit);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class AnalysisModes {
    // --- [REGISTRY]
    public static IO<VisualAnalysisMode> Register<TMode>() where TMode : VisualAnalysisMode, new() =>
        from attributed in IO.lift(MissingGuid.Unless(typeof(TMode)))
        from mode in IO.lift(static () => Missing.Unless(VisualAnalysisMode.Register(typeof(TMode)), nameof(VisualAnalysisMode.Register)))
        select mode;

    public static IO<Option<VisualAnalysisMode>> Find(Guid id) =>
        IO.lift(() => Optional(VisualAnalysisMode.Find(id)));

    // --- [OBJECTS]
    public static IO<Unit> Enable(RhinoObject obj, VisualAnalysisMode mode, bool enabled) =>
        IO.lift(() => Refused.Unless(obj.EnableVisualAnalysisMode(mode, enabled), nameof(RhinoObject.EnableVisualAnalysisMode)));

    public static IO<Seq<VisualAnalysisMode>> Active(RhinoObject obj) =>
        IO.lift(() => Answers.Present(obj.GetActiveVisualAnalysisModes()));

    public static IO<Unit> AdjustMeshes(RhinoDoc doc, Guid modeId) =>
        IO.lift(() => Refused.Unless(VisualAnalysisMode.AdjustAnalysisMeshes(doc, modeId), nameof(VisualAnalysisMode.AdjustAnalysisMeshes)));
}
