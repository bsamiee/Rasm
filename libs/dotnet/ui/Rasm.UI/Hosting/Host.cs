using System.Globalization;
using System.Numerics;
using Rasm.UI.Automation;
using Rasm.UI.Input;
using Rasm.UI.Media;
using Rasm.UI.Styling;
using SkiaSharp;
using SuperLinq;

namespace Rasm.UI.Hosting;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum]
public sealed partial class Visibility {
    public static readonly Visibility Hidden = new(drawn: false, hasKeyFocus: false);
    public static readonly Visibility Shown = new(drawn: true, hasKeyFocus: false);
    public static readonly Visibility Active = new(drawn: true, hasKeyFocus: true);

    public bool Drawn { get; }
    public bool HasKeyFocus { get; }

    public static Visibility Of(bool shown, bool hasKeyFocus) => !shown ? Hidden : hasKeyFocus ? Active : Shown;
}

[SmartEnum]
public sealed partial class Blending {
    public static readonly Blending WithinWindow = new(static _ => true);
    public static readonly Blending BehindWindow = new(static overSlot => !overSlot);
    public static readonly Blending Raster = new(static _ => false);

    [UseDelegateFromConstructor]
    internal partial bool Native(bool overSlot);
}

public sealed record Host(SKSize Size, Theme Theme, Visibility Visibility, Role Root, CultureInfo Culture) {
    public bool Cycles { get; init; } = Root == Role.Window;
    public Blending Blending { get; init; } = Blending.Raster;
    public Factor Headroom { get; init; } = Factor.Unit;
}

public sealed record SceneBackend(
    Func<SKImageInfo, Fin<SKSurface>> Allocated,
    Func<Seq<SKRectI>, IO<Unit>> Invalidated,
    Func<Func<Duration, IO<Unit>>, IO<Func<bool, IO<Unit>>>> DisplayLinked) {
    public Func<Mount, IO<Unit>> Applied { get; init; } = static _ => IO.pure(unit);
    public Option<Func<Transfer, SKPoint, IO<Unit>>> Lifted { get; init; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Hosted {
    public required Seq<Key> Path { get; init; }
    public required SKRect Frame { get; init; }
    public required SKRect Clip { get; init; }

    public sealed record Slot(Option<View.Text> Entry) : Hosted {
        public SKRectI ClipRect => SKRectI.Ceiling(Clip, outwards: true);
        public SKRectI ControlRect => SKRectI.Round(SKRect.Create(Frame.Location - ClipRect.Location, Frame.Size));
    }

    public sealed record Backdrop(Corners Corners, bool Clear, Option<SKColorF> Tint) : Hosted {
        public float Spacing { get; init; }
    }

    public sealed record Popup(Corners Corners) : Hosted;
}

public sealed record PlacedControls<TControl>(
    Func<Hosted.Slot, Option<AutomationElement>, IO<TControl>> Added,
    Func<TControl, Hosted.Slot, IO<TControl>> Moved,
    Func<TControl, IO<Unit>> Removed) where TControl : notnull {
    public HashMap<Seq<Key>, (Hosted.Slot Slot, TControl Control)> Controls { get; private init; }

    public IO<Unit> Released => Controls.Values.ToSeq().Iter(held => Removed(held.Control)).As();

    public IO<PlacedControls<TControl>> Reconciled(Mount mount) {
        HashMap<Seq<Key>, Hosted.Slot> wanted = toHashMap(mount.Hosted.Choose(static hosted => Optional(hosted as Hosted.Slot)).Map(static slot => (slot.Path, slot)));
        return toSeq(Controls.Keys).Concat(wanted.Keys).Distinct().Fold(IO.pure(this), (sequence, path) => sequence.Bind(placed => placed.Changed(path, wanted.Find(path), mount)));
    }

    private IO<PlacedControls<TControl>> Changed(Seq<Key> path, Option<Hosted.Slot> wanted, Mount mount) =>
        Controls.Find(path).Match(
            Some: held => wanted.Match(
                Some: slot => held.Slot == slot ? IO.pure(this) : Moved(held.Control, slot).Map(moved => this with { Controls = Controls.AddOrUpdate(path, (slot, moved)) }),
                None: () => Removed(held.Control).Map(_ => this with { Controls = Controls.Remove(path) })),
            None: () => wanted.Match(
                Some: slot => Added(slot, mount.Elements.Find(path)).Map(control => this with { Controls = Controls.AddOrUpdate(path, (slot, control)) }),
                None: () => IO.pure(this)));
}
