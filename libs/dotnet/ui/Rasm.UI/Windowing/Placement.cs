using System.Numerics;
using Rasm.UI.Scenes;
using SkiaSharp;

namespace Rasm.UI.Windowing;

// --- [TYPES] ---------------------------------------------------------------------------
public enum PageHost {
    Options,
    Document,
    Object,
    View,
}

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Anchor {
    public sealed record Placed(string Key) : Anchor;
    public sealed record Viewport(Guid View) : Anchor;
    public sealed record Window(uint Document) : Anchor;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Placement {
    public sealed record Docked : Placement;
    public sealed record Section : Placement;
    public sealed record Page(PageHost Host) : Placement;
    public sealed record Window(uint Document, bool Attached) : Placement;
    public sealed record Rail(Anchor Anchor, Edge Edge) : Placement;
    public sealed record Popover(Anchor Anchor, SKRect Target) : Placement;
    public sealed record Toast(Anchor Anchor) : Placement;
    public sealed record Hud(Guid View, Alignment Align, Option<Vector3> Tracking) : Placement;
    public sealed record DropZones(Seq<(SKRect Zone, Placement Into)> Zones) : Placement;
}

public sealed record Placed(string Key, Placement Placement, Option<SKRect> Frame, bool Folded);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Change {
    public sealed record Open(Placed Placed) : Change;
    public sealed record Update(Placed Placed) : Change;
    public sealed record Close(string Key) : Change;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Placements {
    public static Seq<Change> Reconcile(HashMap<string, Placed> live, Seq<Placed> wanted) =>
        toHashSet(wanted.Map(static placed => placed.Key)) switch {
            var kept => wanted.Choose(placed => live.Find(placed.Key).Match(
                    Some: shown => shown == placed ? Option<Change>.None : new Change.Update(placed),
                    None: () => new Change.Open(placed)))
                .Concat(live.Keys.Filter(key => !kept.Contains(key)).Map(static Change (key) => new Change.Close(key))),
        };
}
