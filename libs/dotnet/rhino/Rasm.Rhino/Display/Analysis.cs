using System.Drawing;
using System.Runtime.CompilerServices;
using Rasm.Rhino.Document;
using Rasm.Rhino.Persistence.Settings;
using Rasm.Rhino.PlugIns;
using Rasm.Rhino.UI;
using Rhino;
using Rhino.ApplicationSettings;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Geometry.Collections;
using Rhino.UI;
using Wacton.Unicolour;

namespace Rasm.Rhino.Display;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record FalseColorScale(Interval Range, Seq<ColorStop> Stops) {
    public Option<Func<double, Color>> Tabulate() =>
        toSeq(Stops.OrderBy(static stop => stop.Position)) switch {
            var ordered => ordered.Head.Map(first => Enumerable.Range(0, byte.MaxValue + 1)
                .Select(texel => Sampled(ordered, first, texel / (double)byte.MaxValue))
                .ToArray() switch {
                    var table => (Func<double, Color>)(value => table[(int)Math.Round(double.Clamp(Range.NormalizedParameterAt(value), 0d, 1d) * byte.MaxValue, MidpointRounding.ToEven)]),
                }),
        };

    private static Color Sampled(Seq<ColorStop> ordered, ColorStop first, double at) =>
        (Low: ordered.Filter(stop => stop.Position <= at).Last.IfNone(first), High: (ordered.Find(stop => stop.Position >= at) | ordered.Last).IfNone(first)) switch {
            var (low, high) => Painted(Colour(low.Color).Mix(Colour(high.Color), ColourSpace.Oklab, high.Position > low.Position ? (at - low.Position) / (high.Position - low.Position) : 0d)),
        };

    private static Unicolour Colour(Color color) => new(ColourSpace.Rgb255, color.R, color.G, color.B, color.A / (double)byte.MaxValue);

    private static Color Painted(Unicolour mixed) =>
        mixed.MapToRgbGamut() switch {
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

    // --- [STYLES]
    public sealed record FalseColor(LocalizeStringPair Name, Func<RhinoObject, int, Mesh, int, double> Measure, IO<FalseColorScale> Scale) : AnalysisDefinition(Name) {
        internal IO<Unit> Paint(RhinoObject obj, Mesh[] meshes) =>
            from scale in Scale
            from painted in scale.Tabulate().Traverse(color => IO.lift(() => Callbacks.Each(
                toSeq(
                    from entry in meshes.Select(static (mesh, face) => (Mesh: mesh, Face: face))
                    from mesh in Optional(entry.Mesh).ToSeq()
                    select (Mesh: mesh, entry.Face, Colors: Enumerable.Range(0, mesh.Vertices.Count).Select(vertex => color(Measure(obj, entry.Face, mesh, vertex))).ToArray())),
                static (entry, _) => Refused.Unless(entry.Mesh.VertexColors.SetColors(entry.Colors), nameof(MeshVertexColorList.SetColors), entry.Face)))).As()
            select unit;
    }

    public sealed record Texture(LocalizeStringPair Name, Func<RhinoObject, Seq<ModeEdit>> SetUp) : AnalysisDefinition(Name);
    public sealed record Wireframe(LocalizeStringPair Name) : AnalysisDefinition(Name) {
        public Option<Func<RhinoObject, DisplayPipeline, IO<Unit>>> DrawObject { get; init; }
        public Option<Func<RhinoObject, GeometryBase, DisplayPipeline, IO<Unit>>> DrawGeometry { get; init; }
    }
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
public abstract class DefinedAnalysisMode : VisualAnalysisMode {
    // --- [STATE]
    private readonly AnalysisDefinition definition;
    private readonly IPlugInViews owner;

    protected DefinedAnalysisMode(AnalysisDefinition definition) => (this.definition, owner) = (definition, HostedTypes.Owner(GetType()));

    // --- [DEFINITION]
    public sealed override string Name => definition.Name.Local;
    public sealed override AnalysisStyle Style => definition.Map(falseColor: AnalysisStyle.FalseColor, texture: AnalysisStyle.Texture, wireframe: AnalysisStyle.Wireframe);
    public sealed override bool ShowIsoCurves => Callbacks.Answer(definition.ShowIsoCurves, () => base.ShowIsoCurves, static () => false, CallbackSite.Of(owner, this));

    public sealed override void EnableUserInterface(bool on) =>
        Callbacks.Run(definition.UserInterface.Map(show => IO.pure(on).Bind(show)), CallbackSite.Of(owner, this));

    public sealed override bool ObjectSupportsAnalysisMode(RhinoObject obj) =>
        Callbacks.Answer(definition.Supports.Map(supports => IO.lift(() => supports(obj))), () => base.ObjectSupportsAnalysisMode(obj), static () => false, CallbackSite.Of(owner, this));

    // --- [APPEARANCE]
    protected sealed override void SetUpDisplayAttributes(RhinoObject obj, DisplayPipelineAttributes attributes) =>
        Callbacks.Run(
            IO.pure((Object: obj, Attributes: attributes)).Bind(args => definition.Switch(
                args,
                falseColor: static (input, _) => IO.lift(() => { input.Attributes.ShadeVertexColors = true; }),
                texture: static (input, texture) => IO.lift(() => texture.SetUp(input.Object).Traverse(edit => edit.Write(input.Attributes)).As()).Map(static _ => unit),
                wireframe: static (_, _) => IO.pure(unit))),
            CallbackSite.Of(owner, this));

    protected sealed override void UpdateVertexColors(RhinoObject obj, Mesh[] meshes) =>
        Callbacks.Run(
            IO.pure((Object: obj, Meshes: meshes)).Bind(args => definition.Switch(
                args,
                falseColor: static (input, color) => color.Paint(input.Object, input.Meshes),
                texture: static (_, _) => IO.pure(unit),
                wireframe: static (_, _) => IO.pure(unit))),
            CallbackSite.Of(owner, this));

    // --- [DRAWING]
    private Option<AnalysisDefinition.Wireframe> Wires =>
        definition.Switch(
            falseColor: static _ => Option<AnalysisDefinition.Wireframe>.None,
            texture: static _ => Option<AnalysisDefinition.Wireframe>.None,
            wireframe: static wires => Some(wires));

    protected sealed override void DrawCurveObject(CurveObject curve, DisplayPipeline pipeline) =>
        Wires.Bind(static wires => wires.DrawObject).Match(
            Some: draw => Callbacks.Run(IO.pure(curve).Bind(held => draw(held, pipeline)), CallbackSite.Of(owner, this)),
            None: () => base.DrawCurveObject(curve, pipeline));

    protected sealed override void DrawBrepObject(BrepObject brep, DisplayPipeline pipeline) => Drawn(brep, pipeline);

    protected sealed override void DrawExtrusionObject(ExtrusionObject extrusion, DisplayPipeline pipeline) => Drawn(extrusion, pipeline);

    protected sealed override void DrawMeshObject(MeshObject mesh, DisplayPipeline pipeline) => Drawn(mesh, pipeline);

    protected sealed override void DrawSubDObject(SubDObject subd, DisplayPipeline pipeline) => Drawn(subd, pipeline);

    protected sealed override void DrawPointObject(PointObject point, DisplayPipeline pipeline) => Drawn(point, pipeline);

    protected sealed override void DrawPointCloudObject(PointCloudObject pointCloud, DisplayPipeline pipeline) => Drawn(pointCloud, pipeline);

    protected sealed override void DrawMesh(RhinoObject obj, Mesh mesh, DisplayPipeline pipeline) => Drawn(obj, mesh, pipeline);

    protected sealed override void DrawNurbsCurve(RhinoObject obj, NurbsCurve curve, DisplayPipeline pipeline) => Drawn(obj, curve, pipeline);

    protected sealed override void DrawNurbsSurface(RhinoObject obj, NurbsSurface surface, DisplayPipeline pipeline) => Drawn(obj, surface, pipeline);

    private void Drawn(RhinoObject obj, DisplayPipeline pipeline, [CallerMemberName] string member = "") =>
        Callbacks.Run(Wires.Bind(static wires => wires.DrawObject).Map(draw => IO.pure(obj).Bind(held => draw(held, pipeline))), CallbackSite.Of(owner, this, member));

    private void Drawn(RhinoObject obj, GeometryBase geometry, DisplayPipeline pipeline, [CallerMemberName] string member = "") =>
        Callbacks.Run(Wires.Bind(static wires => wires.DrawGeometry).Map(draw => IO.pure(geometry).Bind(held => draw(obj, held, pipeline))), CallbackSite.Of(owner, this, member));
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
