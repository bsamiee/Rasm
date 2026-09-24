using System.Drawing;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.DocObjects;
using Rhino.UI;

namespace Rasm.Rhino.Ui;

// --- [TYPES] ---------------------------------------------------------------------------
public enum LinetypePreviewKind { Dashes = 0, CurveShapes = 1, TextSurfaceShapes = 2 }

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MeshColor {
    public sealed record Default() : MeshColor;

    public sealed record Uniform(Color Value) : MeshColor;

    public sealed record PerMesh(Seq<Color> Values) : MeshColor;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class HostDrawing {
    // --- [RASTERS]
    public static IO<Bitmap> SvgBitmap(string svg, int width, int height, bool darkMode) =>
        IO.lift(() => Missing.Unless(DrawingUtilities.BitmapFromSvg(svg, width, height, adjustForDarkMode: darkMode), nameof(DrawingUtilities.BitmapFromSvg)));

    public static IO<byte[]> SvgPixels(string svg, int width, int height, bool premultiply, Color background, bool darkMode) =>
        IO.lift(() => Missing.Unless(DrawingUtilities.PixelsFromSvg(svg, width, height, premultiplyAlpha: premultiply, background, adjustForDarkMode: darkMode), nameof(DrawingUtilities.PixelsFromSvg)));

    public static IO<Bitmap> MeshPreview(RhinoDoc doc, Seq<Mesh> meshes, MeshColor color, Size size) =>
        from colors in color.Switch(
            (Doc: doc, meshes.Count),
            @default: static (state, _) => TableOps.WithAttributes(state.Doc, None, attributes => IO.lift(() => toSeq(Enumerable.Repeat(attributes.DrawColor(state.Doc), state.Count)))),
            uniform: static (state, uniform) => IO.pure(toSeq(Enumerable.Repeat(uniform.Value, state.Count))),
            perMesh: static (state, perMesh) => IO.lift(() => CountMismatch.Unless(state.Count, perMesh.Values.Count, nameof(DrawingUtilities.CreateMeshPreviewImage))).Map(_ => perMesh.Values))
        from bitmap in IO.lift(() => Missing.Unless(DrawingUtilities.CreateMeshPreviewImage(doc, meshes, colors, size), nameof(DrawingUtilities.CreateMeshPreviewImage)))
        select bitmap;

    // --- [GEOMETRY]
    public static IO<Seq<Point2f[]>> LinetypePreview(Curve curve, Linetype linetype, int width, int height, Option<(double LinetypeScale, LinetypePreviewKind Kind)> pattern) =>
        IO.lift(() => toSeq(pattern.Match(
            Some: scaled => DrawingUtilities.CreateLinetypePreviewGeometryEx(curve, linetype, width, height, scaled.LinetypeScale, (int)scaled.Kind),
            None: () => DrawingUtilities.CreateCurvePreviewGeometry(curve, linetype, width, height))));
}
