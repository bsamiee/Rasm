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

public sealed record Host(SKSize Size, Theme Theme, Visibility Visibility, Key Root, CultureInfo Culture) {
    private readonly Option<bool> cycles;

    public bool Cycles { get => cycles.IfNone(Root == Roles.Window); init => cycles = value; }
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
        return toSeq(Controls.AsIterable().FullOuterHashJoin(wanted.AsIterable(),
            static entry => entry.Key,
            static entry => entry.Key,
            held => Some(Removed(held.Value.Control).Map(_ => (Path: held.Key, Value: Option<(Hosted.Slot Slot, TControl Control)>.None))),
            slot => Some(Added(slot.Value, mount.Elements.Find(slot.Key)).Map(control => (Path: slot.Key, Value: Some((Slot: slot.Value, Control: control))))),
            (held, slot) => Some(slot.Value).Filter(next => held.Value.Slot != next).Map(next => Moved(held.Value.Control, next)
                .Map(control => (Path: slot.Key, Value: Some((Slot: next, Control: control)))))))
            .Somes()
            .Fold(IO.pure(this), static (sequence, change) =>
                from placed in sequence
                from next in change
                select placed with { Controls = next.Value.Map(value => placed.Controls.AddOrUpdate(next.Path, value)).IfNone(() => placed.Controls.Remove(next.Path)) });
    }
}
