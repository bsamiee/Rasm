using System.Globalization;
using System.Reflection;
using Eto.Forms;
using Rasm.Rhino.Persistence.Settings;
using Rhino.PlugIns;
using Rhino.Runtime;
using Rhino.UI;

namespace Rasm.Rhino.UI.Assets;

// --- [TYPES] ---------------------------------------------------------------------------
public interface IGlyph {
    public string Key { get; }
}

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GlyphRole : IGlyph {
    public static readonly GlyphRole Reset = new("reset");
    public static readonly GlyphRole Presets = new("presets");
    public static readonly GlyphRole Pick = new("pick");
    public static readonly GlyphRole Load = new("load");
    public static readonly GlyphRole Save = new("save");
    public static readonly GlyphRole Options = new("options");
    public static readonly GlyphRole LockOpen = new("lock-open");
    public static readonly GlyphRole LockClosed = new("lock-closed");
    public static readonly GlyphRole VerticalWipe = new("vertical-wipe");
    public static readonly GlyphRole HorizontalWipe = new("horizontal-wipe");
    public static readonly GlyphRole SideBySide = new("side-by-side");
    public static readonly GlyphRole Swap = new("swap");
    public static readonly GlyphRole Fit = new("fit");
    public static readonly GlyphRole ActualSize = new("actual-size");
    public static readonly GlyphRole Flip = new("flip");
    public static readonly GlyphRole Grid = new("grid");
    public static readonly GlyphRole List = new("list");
    public static readonly GlyphRole Search = new("search");
    public static readonly GlyphRole FavouriteOn = new("favourite-on");
    public static readonly GlyphRole FavouriteOff = new("favourite-off");
}

[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class IconMaster {
    public static readonly IconMaster Row = new(16);
    public static readonly IconMaster Header = new(20);
    public static readonly IconMaster Toolbar = new(24);

    public int Points { get; }
}

[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class IconSlot {
    public static readonly IconSlot PanelTab = Drawn(IconMaster.Row);
    public static readonly IconSlot PanelButton = new(IconMaster.Row, static () => Accessors.Scoped(nameof(Accessors.PanelButtonSize), IO.lift(static () => Accessors.PanelButtonSize(Accessors.ToolbarButtons))));
    public static readonly IconSlot PickButton = Drawn(IconMaster.Row);
    public static readonly IconSlot ListCell = Drawn(IconMaster.Row);
    public static readonly IconSlot DropDownItem = new(IconMaster.Row, static () => IO.pure(HostUtils.RunningOnOSX ? 14 : 16));
    public static readonly IconSlot MenuItem = Drawn(IconMaster.Row);
    public static readonly IconSlot Window = Drawn(IconMaster.Row);
    public static readonly IconSlot Banner = new(IconMaster.Row, static () => IO.pure(44));
    public static readonly IconSlot ContentMenu = Drawn(IconMaster.Row);
    public static readonly IconSlot SectionHeader = Drawn(IconMaster.Header);
    public static readonly IconSlot RenderTab = Drawn(HostUtils.RunningOnOSX ? IconMaster.Header : IconMaster.Row);
    public static readonly IconSlot PropertiesPage = Drawn(IconMaster.Toolbar);
    public static readonly IconSlot Cursor = Drawn(IconMaster.Toolbar);

    public IconMaster Master { get; }

    [UseDelegateFromConstructor]
    public partial IO<int> Size();

    private static IconSlot Drawn(IconMaster master) => new(master, () => IO.pure(master.Points));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Icons {
    // --- [RESOURCES]
    public static string ResourceName(IGlyph glyph, IconMaster master) =>
        string.Create(CultureInfo.InvariantCulture, $"{glyph.Key}-{master.Points}.svg");

    public static IO<Unit> Check(IPlugInSink plugIn, Seq<IGlyph> glyphs, IconSlot slot) =>
        IO.lift(() => glyphs.Traverse(glyph => Cached(plugIn, ResourceName(glyph, slot.Master), slot.Master.Points).ToValidation()).As().ToFin().Map(static _ => unit));

    private static global::Rhino.Resources.Assets Source(IPlugInSink plugIn) =>
        global::Rhino.Resources.Assets.Get(plugIn.GetType().Assembly);

    private static Fin<Eto.Drawing.Icon> Cached(IPlugInSink plugIn, string name, int size) =>
        GlyphMissing.Unless(Source(plugIn).Eto.Icons.TryGet(name, new(size, size)), name);

    // --- [ETO]
    public static IO<Eto.Drawing.Icon> Frames(IPlugInSink plugIn, IGlyph glyph, IconSlot slot) =>
        slot.Size().Bind(size => IO.lift(() => Cached(plugIn, ResourceName(glyph, slot.Master), size)));

    public static Eto.Drawing.Icon Frame(IPlugInSink plugIn, IGlyph glyph, IconSlot slot) =>
        Source(plugIn).Eto.Icons.Get(ResourceName(glyph, slot.Master), new(slot.Master.Points, slot.Master.Points));

    public static IO<IDisposable> Themed(IPlugInSink plugIn, IGlyph glyph, IconSlot slot, Action<Eto.Drawing.Icon> write) =>
        from written in IO.pure(Frames(plugIn, glyph, slot).Bind(icon => IO.lift(() => write(icon))))
        from _ in written
        from subscription in Themes.Changed.Inline(__ => written, plugIn)
        select subscription;

    public static IO<Cursor> PickCursor(IPlugInSink plugIn) =>
        Frames(plugIn, GlyphRole.Pick, IconSlot.Cursor).Bind(static icon => IO.lift(() => new Cursor(icon, new(4f, 20f))));

    // --- [DRAWING]
    public static IO<System.Drawing.Bitmap> Raster(IPlugInSink plugIn, IGlyph glyph, IconSlot slot) =>
        from size in slot.Size()
        from pixels in Themes.Device(new(size, size))
        from bitmap in IO.lift(() => Rastered(plugIn, ResourceName(glyph, slot.Master), pixels))
        select bitmap;

    public static IO<System.Drawing.Bitmap> Raster(IPlugInSink plugIn, IGlyph glyph, System.Drawing.Size pixels) =>
        IO.lift(() => Rastered(
            plugIn,
            ResourceName(glyph, toSeq(IconMaster.Items).Find(master => master.Points >= Math.Max(pixels.Width, pixels.Height)).IfNone(IconMaster.Toolbar)),
            pixels));

    public static IO<System.Drawing.Icon> DrawingIcon(IPlugInSink plugIn, IGlyph glyph, IconSlot slot) =>
        from size in slot.Size()
        let name = ResourceName(glyph, slot.Master)
        from icon in IO.lift(() => GlyphMissing.Unless(Source(plugIn).SystemDrawing.Icons.TryGet(name, new(size, size)), name))
        select icon;

    public static IO<System.Drawing.Bitmap> PageImage(IPlugInSink plugIn) =>
        IO.lift(() => Missing.Unless(((PlugIn)plugIn).Icon(new(128, 128)), nameof(PlugIn.Icon)));

    public static IO<Option<System.Drawing.Icon>> PlugInIcon(IPlugInSink plugIn) =>
        IO.lift(() => toSeq(plugIn.GetType().Assembly.GetCustomAttributes<PlugInDescriptionAttribute>())
            .Find(static description => description.DescriptionType == DescriptionType.Icon)
            .Bind(description => Optional(DrawingUtilities.IconFromResource(description.Value, plugIn.GetType().Assembly))));

    private static Fin<System.Drawing.Bitmap> Rastered(IPlugInSink plugIn, string name, System.Drawing.Size pixels) =>
        GlyphMissing.Unless(Source(plugIn).SystemDrawing.Bitmaps.TryGet(name, pixels), name);
}
