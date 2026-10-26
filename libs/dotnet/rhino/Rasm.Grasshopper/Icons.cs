using System.Text;
using Grasshopper2.UI.Icon;
using Grasshopper2.UI.Icon.Vector;
using Grasshopper2.UI.Skinning;
using Rasm.Rhino;
using Rasm.Rhino.UI.Styling;
using Rasm.UI;
using SkiaSharp;

namespace Rasm.Grasshopper;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CanvasIcons {
    // --- [VECTORS]
    public static IO<IIcon> Vector(GlyphSource glyph) =>
        IO.lift(() => glyph.Switch<IIcon>(
            tinted: static held => SvgIcon.FromResource(held.Assembly, held.Resource),
            paired: static held => Skin.Current.Shades[ShadeKind.Normal].Apex switch {
                var apex => SvgIcon.FromResource(held.Assembly, held.Resource(new SKColorF(apex.R, apex.G, apex.B, apex.A))),
            },
            picture: static held => {
                using MemoryStream stream = new();
                using (SKCanvas canvas = SKSvgCanvas.Create(held.Art.CullRect, stream))
                    canvas.DrawPicture(held.Art);
                return SvgIcon.FromXml(Encoding.UTF8.GetString(stream.ToArray()));
            }));

    internal static IO<Option<IIcon>> PlugIn(IPlugInSink plugIn) =>
        Icons.PlugInImage(plugIn).Bind(static found => found.Traverse(static glyph => Vector(glyph)).As());
}
