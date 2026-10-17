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
                Refused.Unless(entry.Mesh.VertexColors.SetColors(entry.Colors), nameof(MeshVertexColorList.SetColors), entry.Face)))
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
        Callbacks.Run(definition.UserInterface.Map(show => IO.pure(on).Bind(show)), CallbackSite.Of(this));

    public sealed override bool ObjectSupportsAnalysisMode(RhinoObject obj) =>
        Callbacks.Answer(definition.Supports.Map(supports => IO.lift(() => supports(obj))), () => base.ObjectSupportsAnalysisMode(obj), static () => false, CallbackSite.Of(this));

    // --- [APPEARANCE]
    protected sealed override void SetUpDisplayAttributes(RhinoObject obj, DisplayPipelineAttributes attributes) =>
        Callbacks.Run(
            IO.pure((Object: obj, Attributes: attributes)).Bind(args => definition.Switch(
                args,
                falseColor: static (input, _) => IO.lift(() => { input.Attributes.ShadeVertexColors = true; }),
                texture: static (input, texture) => texture.SetUp(input.Object, input.Attributes),
                wireframe: static (_, _) => IO.pure(unit))),
            CallbackSite.Of(this));

    protected sealed override void UpdateVertexColors(RhinoObject obj, Mesh[] meshes) =>
        Callbacks.Run(
            IO.pure((Object: obj, Meshes: meshes)).Bind(args => definition.Switch(
                args,
                falseColor: static (input, color) => color.Paint(input.Object, input.Meshes),
                texture: static (_, _) => IO.pure(unit),
                wireframe: static (_, _) => IO.pure(unit))),
            CallbackSite.Of(this));

    // --- [DRAWING]
    protected sealed override void DrawBrepObject(BrepObject brep, DisplayPipeline pipeline) =>
        Callbacks.Run(definition.DrawObject.Map(draw => IO.pure(brep).Bind(obj => draw(obj, pipeline))), CallbackSite.Of(this));

    protected sealed override void DrawExtrusionObject(ExtrusionObject extrusion, DisplayPipeline pipeline) =>
        Callbacks.Run(definition.DrawObject.Map(draw => IO.pure(extrusion).Bind(obj => draw(obj, pipeline))), CallbackSite.Of(this));

    protected sealed override void DrawMeshObject(MeshObject mesh, DisplayPipeline pipeline) =>
        Callbacks.Run(definition.DrawObject.Map(draw => IO.pure(mesh).Bind(obj => draw(obj, pipeline))), CallbackSite.Of(this));

    protected sealed override void DrawSubDObject(SubDObject subd, DisplayPipeline pipeline) =>
        Callbacks.Run(definition.DrawObject.Map(draw => IO.pure(subd).Bind(obj => draw(obj, pipeline))), CallbackSite.Of(this));

    protected sealed override void DrawPointObject(PointObject point, DisplayPipeline pipeline) =>
        Callbacks.Run(definition.DrawObject.Map(draw => IO.pure(point).Bind(obj => draw(obj, pipeline))), CallbackSite.Of(this));

    protected sealed override void DrawPointCloudObject(PointCloudObject pointCloud, DisplayPipeline pipeline) =>
        Callbacks.Run(definition.DrawObject.Map(draw => IO.pure(pointCloud).Bind(obj => draw(obj, pipeline))), CallbackSite.Of(this));

    protected sealed override void DrawMesh(RhinoObject obj, Mesh mesh, DisplayPipeline pipeline) =>
        Callbacks.Run(definition.DrawGeometry.Map(draw => IO.pure(mesh).Bind(geometry => draw(obj, geometry, pipeline))), CallbackSite.Of(this));

    protected sealed override void DrawNurbsCurve(RhinoObject obj, NurbsCurve curve, DisplayPipeline pipeline) =>
        Callbacks.Run(definition.DrawGeometry.Map(draw => IO.pure(curve).Bind(geometry => draw(obj, geometry, pipeline))), CallbackSite.Of(this));

    protected sealed override void DrawNurbsSurface(RhinoObject obj, NurbsSurface surface, DisplayPipeline pipeline) =>
        Callbacks.Run(definition.DrawGeometry.Map(draw => IO.pure(surface).Bind(geometry => draw(obj, geometry, pipeline))), CallbackSite.Of(this));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class AnalysisModes {
    // --- [REGISTRATION]
    public static IO<VisualAnalysisMode> Register<TMode>() where TMode : DefinedAnalysisMode, new() =>
        IO.lift(static () => VisualAnalysisMode.Register(typeof(TMode)));

    // --- [OBJECTS]
    public static IO<Unit> Enable(RhinoDoc doc, Seq<RhinoObject> objects, VisualAnalysisMode mode, bool enabled, RedrawPolicy redraw) =>
        Commits.WithinRedraw(doc, redraw, IO.lift(() => Callbacks.Each(objects, obj => obj.EnableVisualAnalysisMode(mode, enabled), nameof(RhinoObject.EnableVisualAnalysisMode))));
}
