using System.Drawing;
using System.Numerics;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.Document;
using Rasm.Rhino.Persistence.Settings;
using Rhino;
using Rhino.ApplicationSettings;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Geometry.Collections;
using Rhino.UI;
using Wacton.Unicolour;

namespace Rasm.Rhino.Display;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record FalseColorScale(Interval Range, RampTable Ramp) {
    public Color ColorAt(double value) => Displayed(Ramp.Sample(Range.NormalizedParameterAt(value)));

    public static Color Displayed(Vector4 linear) =>
        new Unicolour(Configuration.Default, ColourSpace.RgbLinear, linear.X, linear.Y, linear.Z, linear.W).MapToRgbGamut() switch {
            var mapped => mapped.Rgb.Byte255 switch {
                var rgb => Color.FromArgb(mapped.Alpha.A255, rgb.R, rgb.G, rgb.B),
            },
        };
}

[Union]
public abstract partial record AnalysisDefinition {
    // --- [DEFINITION]
    private AnalysisDefinition(LocalizeStringPair name) => Name = name;

    public LocalizeStringPair Name { get; }
    public Option<Func<RhinoObject, bool>> Supports { get; init; }
    public Option<IO<bool>> ShowIsoCurves { get; init; }
    public Option<Func<bool, IO<Unit>>> UserInterface { get; init; }
    public Option<Func<RhinoObject, DisplayPipeline, IO<Unit>>> DrawObject { get; init; }
    public Option<Func<RhinoObject, GeometryBase, DisplayPipeline, IO<Unit>>> DrawGeometry { get; init; }

    // --- [STYLES]
    public sealed record FalseColor(LocalizeStringPair Name, Func<RhinoObject, int, Mesh, int, double> Measure, IO<FalseColorScale> Scale) : AnalysisDefinition(Name) {
        internal IO<Unit> Paint(RhinoObject obj, Mesh[] meshes) =>
            from scale in Scale
            let colors = toSeq(
                from entry in meshes.Select(static (mesh, face) => (Mesh: mesh, Face: face))
                from mesh in Optional(entry.Mesh).ToSeq()
                select (Mesh: mesh, entry.Face, Colors: Enumerable.Range(0, mesh.Vertices.Count).Select(vertex => scale.ColorAt(Measure(obj, entry.Face, mesh, vertex))).ToArray()))
            from painted in IO.lift(() => Callbacks.Each(colors, static (entry, _) =>
                RefusedElement.Unless(entry.Mesh.VertexColors.SetColors(entry.Colors), nameof(MeshVertexColorList.SetColors), entry.Face)))
            select unit;
    }

    public sealed record Texture(LocalizeStringPair Name, Func<RhinoObject, DisplayPipelineAttributes, IO<Unit>> SetUp) : AnalysisDefinition(Name);
    public sealed record Wireframe(LocalizeStringPair Name) : AnalysisDefinition(Name);
}

[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class CurvatureFit {
    // --- [FITS]
    public static readonly CurvatureFit Auto = new(
        IO.lift(static () => VisualAnalysisMode.CurvatureColorAutoRange()),
        static (meshes, state) => Refused.Unless(CurvatureAnalysisSettings.CalculateCurvatureAutoRange(meshes, ref state), state, nameof(CurvatureAnalysisSettings.CalculateCurvatureAutoRange)));

    public static readonly CurvatureFit Max = new(
        IO.lift(static () => VisualAnalysisMode.CurvatureColorMaxRange()),
        static (meshes, state) => Refused.Unless(CurvatureAnalysisSettings.CalculateCurvatureMaxRange(meshes, state), state, nameof(CurvatureAnalysisSettings.CalculateCurvatureMaxRange)));

    // --- [RANGES]
    public IO<Unit> Adjust { get; }

    public IO<CurvatureAnalysisSettingsState> Fit(Seq<Mesh> meshes, Option<CurvatureAnalysisSettings.CurvatureStyle> style = default) =>
        from state in AppSettings.CurvatureAnalysis.Current
        from selected in IO.lift(() => style.Iter(value => state.Style = value))
        from fitted in IO.lift(() => Calculate(meshes, state))
        select fitted;

    [UseDelegateFromConstructor]
    private partial Fin<CurvatureAnalysisSettingsState> Calculate(IEnumerable<Mesh> meshes, CurvatureAnalysisSettingsState state);
}

// --- [SERVICES] ------------------------------------------------------------------------
public abstract class DefinedAnalysisMode(AnalysisDefinition definition) : VisualAnalysisMode {
    // --- [DEFINITION]
    public sealed override string Name => definition.Name.Local;
    public sealed override AnalysisStyle Style => definition.Map(falseColor: AnalysisStyle.FalseColor, texture: AnalysisStyle.Texture, wireframe: AnalysisStyle.Wireframe);
    public sealed override bool ShowIsoCurves => Callbacks.Answer(definition.ShowIsoCurves, () => base.ShowIsoCurves, static () => false, CallbackSite.Of(this));

    public sealed override void EnableUserInterface(bool on) =>
        _ = definition.UserInterface.Iter(show => Callbacks.Answer(on, show, static () => unit, CallbackSite.Of(this)));

    public sealed override bool ObjectSupportsAnalysisMode(RhinoObject obj) =>
        Callbacks.Answer(definition.Supports.Map(supports => IO.lift(() => supports(obj))), () => base.ObjectSupportsAnalysisMode(obj), static () => false, CallbackSite.Of(this));

    // --- [APPEARANCE]
    protected sealed override void SetUpDisplayAttributes(RhinoObject obj, DisplayPipelineAttributes attributes) =>
        _ = definition.Switch(
            (Object: obj, Attributes: attributes, Site: CallbackSite.Of(this)),
            falseColor: static (args, _) => Callbacks.Answer(IO.lift(() => { args.Attributes.ShadeVertexColors = true; }), static () => unit, args.Site),
            texture: static (args, texture) => Callbacks.Answer(args, input => texture.SetUp(input.Object, input.Attributes), static () => unit, args.Site),
            wireframe: static (_, _) => unit);

    protected sealed override void UpdateVertexColors(RhinoObject obj, Mesh[] meshes) =>
        _ = definition.Switch(
            (Object: obj, Meshes: meshes, Site: CallbackSite.Of(this)),
            falseColor: static (args, color) => Callbacks.Answer((Color: color, args.Object, args.Meshes), static input => input.Color.Paint(input.Object, input.Meshes), static () => unit, args.Site),
            texture: static (_, _) => unit,
            wireframe: static (_, _) => unit);

    // --- [DRAWING]
    protected sealed override void DrawBrepObject(BrepObject brep, DisplayPipeline pipeline) =>
        _ = definition.DrawObject.Iter(draw => Callbacks.Answer((Object: brep, Pipeline: pipeline), args => draw(args.Object, args.Pipeline), static () => unit, CallbackSite.Of(this)));

    protected sealed override void DrawExtrusionObject(ExtrusionObject extrusion, DisplayPipeline pipeline) =>
        _ = definition.DrawObject.Iter(draw => Callbacks.Answer((Object: extrusion, Pipeline: pipeline), args => draw(args.Object, args.Pipeline), static () => unit, CallbackSite.Of(this)));

    protected sealed override void DrawMeshObject(MeshObject mesh, DisplayPipeline pipeline) =>
        _ = definition.DrawObject.Iter(draw => Callbacks.Answer((Object: mesh, Pipeline: pipeline), args => draw(args.Object, args.Pipeline), static () => unit, CallbackSite.Of(this)));

    protected sealed override void DrawSubDObject(SubDObject subd, DisplayPipeline pipeline) =>
        _ = definition.DrawObject.Iter(draw => Callbacks.Answer((Object: subd, Pipeline: pipeline), args => draw(args.Object, args.Pipeline), static () => unit, CallbackSite.Of(this)));

    protected sealed override void DrawPointObject(PointObject point, DisplayPipeline pipeline) =>
        _ = definition.DrawObject.Iter(draw => Callbacks.Answer((Object: point, Pipeline: pipeline), args => draw(args.Object, args.Pipeline), static () => unit, CallbackSite.Of(this)));

    protected sealed override void DrawPointCloudObject(PointCloudObject pointCloud, DisplayPipeline pipeline) =>
        _ = definition.DrawObject.Iter(draw => Callbacks.Answer((Object: pointCloud, Pipeline: pipeline), args => draw(args.Object, args.Pipeline), static () => unit, CallbackSite.Of(this)));

    protected sealed override void DrawMesh(RhinoObject obj, Mesh mesh, DisplayPipeline pipeline) =>
        _ = definition.DrawGeometry.Iter(draw => Callbacks.Answer((Object: obj, Geometry: mesh, Pipeline: pipeline), args => draw(args.Object, args.Geometry, args.Pipeline), static () => unit, CallbackSite.Of(this)));

    protected sealed override void DrawNurbsCurve(RhinoObject obj, NurbsCurve curve, DisplayPipeline pipeline) =>
        _ = definition.DrawGeometry.Iter(draw => Callbacks.Answer((Object: obj, Geometry: curve, Pipeline: pipeline), args => draw(args.Object, args.Geometry, args.Pipeline), static () => unit, CallbackSite.Of(this)));

    protected sealed override void DrawNurbsSurface(RhinoObject obj, NurbsSurface surface, DisplayPipeline pipeline) =>
        _ = definition.DrawGeometry.Iter(draw => Callbacks.Answer((Object: obj, Geometry: surface, Pipeline: pipeline), args => draw(args.Object, args.Geometry, args.Pipeline), static () => unit, CallbackSite.Of(this)));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class AnalysisModes {
    // --- [REGISTRATION]
    public static IO<VisualAnalysisMode> Register<TMode>() where TMode : DefinedAnalysisMode, new() =>
        IO.lift(static () => MissingGuid.Unless(typeof(TMode)).Map(static _ => VisualAnalysisMode.Register(typeof(TMode))));

    // --- [OBJECTS]
    public static IO<Unit> Enable(RhinoDoc doc, Seq<RhinoObject> objects, VisualAnalysisMode mode, bool enabled, RedrawPolicy redraw) =>
        Commits.WithinRedraw(doc, redraw, IO.lift(() => Callbacks.Each(objects, obj => obj.EnableVisualAnalysisMode(mode, enabled), nameof(RhinoObject.EnableVisualAnalysisMode))));
}
