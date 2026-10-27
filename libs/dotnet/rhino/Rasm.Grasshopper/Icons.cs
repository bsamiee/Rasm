using Grasshopper2.UI.Icon;
using Grasshopper2.UI.Icon.Vector;
using Grasshopper2.UI.Skinning;
using Rasm.RhinoCommon.UI.Styling;
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
            }));

    public static IO<Option<IIcon>> PlugIn(IGrasshopperPlugin plugin) =>
        Icons.PlugInImage(plugin).Bind(static found => found.Traverse(static glyph => Vector(glyph)).As());
}
