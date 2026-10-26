using System.Drawing;
using System.Reflection;
using Eto.Forms;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Settings;
using Rhino.PlugIns;
using Rhino.Resources;
using Rhino.Runtime;

namespace Rasm.Rhino.UI.Styling;

// --- [TYPES] ---------------------------------------------------------------------------
public interface IGlyph {
    public string Key { get; }

    public Assets Assets => Assets.Get(GetType().Assembly);
}

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GlyphRole : IGlyph {
    private static readonly Assets Own = Assets.Get(typeof(GlyphRole).Assembly);

    public static readonly GlyphRole Reset = new(ResourceIds.Svg_namedItemRestoreHotSvg, Assets.Rhino);
    public static readonly GlyphRole Load = new(ResourceIds.OpenSvg, Assets.Rhino);
    public static readonly GlyphRole Pick = new(ResourceIds.SVGEditorEyeDropperSvg, Assets.Rhino);
    public static readonly GlyphRole LockOpen = new(ResourceIds.Svg_LockSmallOpenSvg, Assets.Rhino);
    public static readonly GlyphRole LockClosed = new(ResourceIds.Svg_LockSmallClosedSvg, Assets.Rhino);
    public static readonly GlyphRole Presets = new("presets.svg", Own);
    public static readonly GlyphRole Options = new("options.svg", Own);
    public static readonly GlyphRole Grid = new("grid.svg", Own);
    public static readonly GlyphRole List = new("list.svg", Own);
    public static readonly GlyphRole VerticalWipe = new("vertical-wipe.svg", Own);
    public static readonly GlyphRole HorizontalWipe = new("horizontal-wipe.svg", Own);
    public static readonly GlyphRole SideBySide = new("side-by-side.svg", Own);
    public static readonly GlyphRole Swap = new("swap.svg", Own);
    public static readonly GlyphRole Fit = new("fit.svg", Own);
    public static readonly GlyphRole ActualSize = new("actual-size.svg", Own);
    public static readonly GlyphRole Flip = new("flip.svg", Own);

    public Assets Assets { get; }
}

[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class IconSlot {
    public static readonly IconSlot PanelTab = new(static () => Accessors.Scoped(nameof(Accessors.TabIconSize), IO.lift(static () => Accessors.TabIconSize(owner: null))));
    public static readonly IconSlot PanelButton = new(static () => Accessors.Scoped(nameof(Accessors.Get), IO.lift(static () => Accessors.Get(Accessors.PrivatePanelButtonSize(Accessors.ToolbarButtons)))));
    public static readonly IconSlot Small = new(static () => IO.pure(16));
    public static readonly IconSlot DropDownItem = new(static () => IO.pure(HostUtils.RunningOnOSX ? 14 : 16));
    public static readonly IconSlot Banner = new(static () => IO.pure(44));
    public static readonly IconSlot SectionHeader = new(static () => IO.pure(20));
    public static readonly IconSlot RenderTab = new(static () => IO.pure(HostUtils.RunningOnOSX ? 20 : 16));
    public static readonly IconSlot Cursor = new(static () => IO.pure(24));

    public IO<Size> Pixels => Size().Bind(static size => HostTheme.Device(new(size, size)));

    [UseDelegateFromConstructor]
    public partial IO<int> Size();
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Icons {
    // --- [ETO]
    public static Eto.Drawing.Icon Frame(IGlyph glyph, Eto.Drawing.Size logicalSize) =>
        glyph.Assets.Eto.Icons.Get(glyph.Key, logicalSize);

    public static IO<Eto.Drawing.Icon> Frames(IGlyph glyph, IconSlot slot) =>
        slot.Size().Map(size => Frame(glyph, new Eto.Drawing.Size(size, size)));

    public static IO<Unit> Themed(IPlugInSink plugIn, IGlyph glyph, IconSlot slot, HostEvent<Unit> changes, Action<Eto.Drawing.Icon> write, Action<IDisposable> retain) =>
        Frames(glyph, slot).Bind(icon => IO.lift(() => write(icon))) switch {
            var written => written.Bind(_ => changes.Inline(_ => written, plugIn, retain)),
        };

    public static IO<Cursor> PickCursor(Action<Cursor> retain) =>
        Frames(GlyphRole.Pick, IconSlot.Cursor).Bind(icon => DisposalOps.Acquire(IO.lift(() => new Cursor(icon, new(0f, icon.Height))), retain));

    // --- [DRAWING]
    public static IO<Bitmap> Raster(IGlyph glyph, IconSlot slot) => slot.Pixels.Bind(pixels => Raster(glyph, pixels));

    public static IO<Bitmap> Raster(IGlyph glyph, Size pixels) =>
        IO.lift(() => glyph.Assets.SystemDrawing.Bitmaps.Get(glyph.Key, pixels));

    public static IO<Icon> DrawingIcon(IGlyph glyph, Size pixels) =>
        IO.lift(() => glyph.Assets.SystemDrawing.Icons.Get(glyph.Key, pixels));

    public static IO<Option<TImage>> PlugInImage<TImage>(IPlugInSink plugIn, Func<Assets, string, TImage> read) =>
        IO.lift(() => {
            Assembly assembly = plugIn.GetType().Assembly;
            return toSeq(assembly.GetCustomAttributes<PlugInDescriptionAttribute>())
                .Find(static description => description.DescriptionType is DescriptionType.Icon)
                .Map(description => read(Assets.Get(assembly), description.Value));
        });
}
