using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.Document;
using Rasm.Rhino.Persistence.Settings;
using Rhino;
using Rhino.ApplicationSettings;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Geometry.Collections;
using Rhino.PlugIns;
using Rhino.UI;
using Wacton.Unicolour;

namespace Rasm.Rhino.Display;

// --- [TYPES] ---------------------------------------------------------------------------
public delegate double VertexMeasure(RhinoObject obj, int face, Mesh mesh, int vertex);

// --- [MODELS] --------------------------------------------------------------------------
public sealed record FalseColorScale(Interval Range, RampTable Ramp) {
    public Color ColorAt(double value) => Displayed(Ramp.Sample(Range.NormalizedParameterAt(value)));

    public static Color Displayed(Vector4 linear) =>
        new Unicolour(Configuration.Default, ColourSpace.RgbLinear, linear.X, linear.Y, linear.Z, linear.W).MapToRgbGamut() switch {
            var mapped => Color.FromArgb(mapped.Alpha.A255, mapped.Rgb.Byte255.R, mapped.Rgb.Byte255.G, mapped.Rgb.Byte255.B),
        };
}

[Union]
public abstract partial record AnalysisLook {
    public abstract VisualAnalysisMode.AnalysisStyle Style { get; }

    public abstract IO<Unit> SetUpDisplayAttributes(RhinoObject obj, DisplayPipelineAttributes attributes);

    public abstract IO<Unit> UpdateVertexColors(RhinoObject obj, Mesh[] meshes);

    public sealed record FalseColor(VertexMeasure Measure, IO<FalseColorScale> Scale) : AnalysisLook {
        public override VisualAnalysisMode.AnalysisStyle Style => VisualAnalysisMode.AnalysisStyle.FalseColor;

        public override IO<Unit> SetUpDisplayAttributes(RhinoObject obj, DisplayPipelineAttributes attributes) =>
            IO.lift(() => { attributes.ShadeVertexColors = true; });

        public override IO<Unit> UpdateVertexColors(RhinoObject obj, Mesh[] meshes) =>
            from scale in Scale
            from painted in IO.lift(() => Callbacks.Each(
                toSeq(meshes).Map(static (mesh, face) => (Mesh: Optional(mesh), Face: face)),
                entry => entry.Mesh.ForAll(mesh => mesh.VertexColors.SetColors(
                    [.. Enumerable.Range(0, mesh.Vertices.Count).Select(vertex => scale.ColorAt(Measure(obj, entry.Face, mesh, vertex)))])),
                nameof(MeshVertexColorList.SetColors)))
            select painted;
    }

    public sealed record Texture(Func<RhinoObject, DisplayPipelineAttributes, IO<Unit>> SetUp) : AnalysisLook {
        public override VisualAnalysisMode.AnalysisStyle Style => VisualAnalysisMode.AnalysisStyle.Texture;

        public override IO<Unit> SetUpDisplayAttributes(RhinoObject obj, DisplayPipelineAttributes attributes) => SetUp(obj, attributes);

        public override IO<Unit> UpdateVertexColors(RhinoObject obj, Mesh[] meshes) => IO.pure(unit);
    }

    public sealed record Wireframe() : AnalysisLook {
        public override VisualAnalysisMode.AnalysisStyle Style => VisualAnalysisMode.AnalysisStyle.Wireframe;

        public override IO<Unit> SetUpDisplayAttributes(RhinoObject obj, DisplayPipelineAttributes attributes) => IO.pure(unit);

        public override IO<Unit> UpdateVertexColors(RhinoObject obj, Mesh[] meshes) => IO.pure(unit);
    }
}

public sealed record AnalysisDefinition(LocalizeStringPair Name, AnalysisLook Look) {
    public Option<Func<RhinoObject, bool>> Supports { get; init; }

    public IO<bool> ShowIsoCurves { get; init; } = IO.pure(value: false);

    public Option<Func<bool, IO<Unit>>> UserInterface { get; init; }

    public Option<Func<RhinoObject, DisplayPipeline, IO<Unit>>> DrawObject { get; init; }

    public Option<Func<RhinoObject, GeometryBase, DisplayPipeline, IO<Unit>>> DrawGeometry { get; init; }
}

[SmartEnum]
public sealed partial class CurvatureFit {
    public static readonly CurvatureFit Auto = new(
        static (meshes, state) => Refused.Unless(CurvatureAnalysisSettings.CalculateCurvatureAutoRange(meshes, ref state), state, nameof(CurvatureAnalysisSettings.CalculateCurvatureAutoRange)),
        VisualAnalysisMode.CurvatureColorAutoRange);

    public static readonly CurvatureFit Max = new(
        static (meshes, state) => Refused.Unless(CurvatureAnalysisSettings.CalculateCurvatureMaxRange(meshes, state), state, nameof(CurvatureAnalysisSettings.CalculateCurvatureMaxRange)),
        VisualAnalysisMode.CurvatureColorMaxRange);

    public IO<CurvatureAnalysisSettingsState> Fit(Seq<Mesh> meshes) =>
        AppSettings.CurvatureAnalysis.Current.Bind(state => IO.lift(() => Calculate(meshes, state)));

    public IO<Unit> Adjust() => IO.lift(AdjustAnalyzed);

    [UseDelegateFromConstructor]
    private partial Fin<CurvatureAnalysisSettingsState> Calculate(IEnumerable<Mesh> meshes, CurvatureAnalysisSettingsState state);

    [UseDelegateFromConstructor]
    private partial void AdjustAnalyzed();
}

// --- [SERVICES] ------------------------------------------------------------------------
public abstract class DefinedAnalysisMode(AnalysisDefinition definition) : VisualAnalysisMode {
    public sealed override string Name => definition.Name.Local;

    public sealed override AnalysisStyle Style => definition.Look.Style;

    public sealed override bool ShowIsoCurves => Callbacks.Answer(definition.ShowIsoCurves, static () => false, CallbackSite.Of(this));

    public sealed override void EnableUserInterface(bool on) =>
        _ = definition.UserInterface.Iter(show => Callbacks.Answer(on, show, static () => unit, CallbackSite.Of(this)));

    public sealed override bool ObjectSupportsAnalysisMode(RhinoObject obj) =>
        Callbacks.Answer(definition.Supports.Map(supports => IO.lift(() => supports(obj))), () => base.ObjectSupportsAnalysisMode(obj), static () => false, CallbackSite.Of(this));

    protected sealed override void SetUpDisplayAttributes(RhinoObject obj, DisplayPipelineAttributes attributes) =>
        _ = Callbacks.Answer(
            (definition.Look, Object: obj, Attributes: attributes),
            static args => args.Look.SetUpDisplayAttributes(args.Object, args.Attributes),
            static () => unit,
            CallbackSite.Of(this));

    protected sealed override void UpdateVertexColors(RhinoObject obj, Mesh[] meshes) =>
        _ = Callbacks.Answer(
            (definition.Look, Object: obj, Meshes: meshes),
            static args => args.Look.UpdateVertexColors(args.Object, args.Meshes),
            static () => unit,
            CallbackSite.Of(this));

    protected sealed override void DrawBrepObject(BrepObject brep, DisplayPipeline pipeline) => Draw(brep, pipeline);

    protected sealed override void DrawExtrusionObject(ExtrusionObject extrusion, DisplayPipeline pipeline) => Draw(extrusion, pipeline);

    protected sealed override void DrawMeshObject(MeshObject mesh, DisplayPipeline pipeline) => Draw(mesh, pipeline);

    protected sealed override void DrawSubDObject(SubDObject subd, DisplayPipeline pipeline) => Draw(subd, pipeline);

    protected sealed override void DrawPointObject(PointObject point, DisplayPipeline pipeline) => Draw(point, pipeline);

    protected sealed override void DrawPointCloudObject(PointCloudObject pointCloud, DisplayPipeline pipeline) => Draw(pointCloud, pipeline);

    protected sealed override void DrawMesh(RhinoObject obj, Mesh mesh, DisplayPipeline pipeline) => Draw(obj, mesh, pipeline);

    protected sealed override void DrawNurbsCurve(RhinoObject obj, NurbsCurve curve, DisplayPipeline pipeline) => Draw(obj, curve, pipeline);

    protected sealed override void DrawNurbsSurface(RhinoObject obj, NurbsSurface surface, DisplayPipeline pipeline) => Draw(obj, surface, pipeline);

    private void Draw(RhinoObject obj, DisplayPipeline pipeline, [CallerMemberName] string member = "") =>
        _ = definition.DrawObject.Iter(draw => Callbacks.Answer(
            (Draw: draw, Object: obj, Pipeline: pipeline),
            static args => args.Draw(args.Object, args.Pipeline),
            static () => unit,
            CallbackSite.Of(this, member)));

    private void Draw(RhinoObject obj, GeometryBase geometry, DisplayPipeline pipeline, [CallerMemberName] string member = "") =>
        _ = definition.DrawGeometry.Iter(draw => Callbacks.Answer(
            (Draw: draw, Object: obj, Geometry: geometry, Pipeline: pipeline),
            static args => args.Draw(args.Object, args.Geometry, args.Pipeline),
            static () => unit,
            CallbackSite.Of(this, member)));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class AnalysisModes {
    // --- [REGISTRATION]
    public static Func<PlugIn, IPlugInSink, IO<IDisposable>> Register<TMode>() where TMode : DefinedAnalysisMode, new() =>
        static (_, _) => IO.lift(static () => MissingGuid.Unless(typeof(TMode)).Map(static _ => VisualAnalysisMode.Register(typeof(TMode))))
            .Map(static _ => Thinktecture.Empty.Disposable());

    // --- [OBJECTS]
    public static IO<Unit> Enable(RhinoDoc doc, Seq<RhinoObject> objects, VisualAnalysisMode mode, bool enabled, RedrawPolicy redraw) =>
        Commits.WithinRedraw(doc, redraw, IO.lift(() => Callbacks.Each(objects, obj => obj.EnableVisualAnalysisMode(mode, enabled), nameof(RhinoObject.EnableVisualAnalysisMode))));
}
