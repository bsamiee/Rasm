namespace Arches;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
internal sealed partial class PlainArch {
    public static readonly PlainArch Semicircular = new(nameof(Semicircular), "Semicircular", "Semicircular arch: half circle on the span", new Interval(0.5, 0.5), ArchGeometry.Circular);
    public static readonly PlainArch Segmental = new(nameof(Segmental), "Segmental", "Segmental arch: circle segment rising less than half the span", new Interval(0, 0.5), ArchGeometry.Circular);
    public static readonly PlainArch Rounded = new(nameof(Rounded), "Rounded", "Horseshoe arch: circle arc reaching wider than the span", new Interval(0.5, double.PositiveInfinity), ArchGeometry.Circular);
    public static readonly PlainArch PointedHorseshoe =
        new("Pointed", "Pointed", "Pointed horseshoe arch: two arcs reaching wider than the span and meeting in a point", new Interval(double.Sqrt(5d / 12), double.PositiveInfinity), ArchGeometry.PointedHorseshoe);
    public static readonly PlainArch Equilateral = new(nameof(Equilateral), "Equilateral", "Equilateral arch: two arcs with radius equal to the span", new Interval(ArchGeometry.EquilateralRise, ArchGeometry.EquilateralRise), ArchGeometry.Gothic);
    public static readonly PlainArch Lancet = new(nameof(Lancet), "Lancet", "Lancet arch: pointed arch with radius longer than the span", new Interval(ArchGeometry.EquilateralRise, double.PositiveInfinity), ArchGeometry.Gothic);
    public static readonly PlainArch Drop = new(nameof(Drop), "Drop", "Drop arch: pointed arch with radius shorter than the span", new Interval(0.5, ArchGeometry.EquilateralRise), ArchGeometry.Gothic);
    public static readonly PlainArch ThreeCenteredOgee = new(nameof(ThreeCenteredOgee), "Three-Centered Ogee", "Three-centered ogee arch: semicircle haunches reversing into concave arcs meeting in a point",
        new Interval(0.5, ArchGeometry.OgeeRise), static (span, apex) => ArchGeometry.Ogee(span, apex, 1d / 2));
    public static readonly PlainArch ReverseOgee = new(nameof(ReverseOgee), "Reverse Ogee", "Reverse ogee arch: concave then convex arcs meeting in a rounded crown at a chosen rise below half the span",
        new Interval(0, 0.5), ArchGeometry.ReverseOgee);
    public static readonly PlainArch Tented = new(nameof(Tented), "Tented", "Tented arch: four concave arcs rising to a point", new Interval(ArchGeometry.EquilateralRise, ArchGeometry.EquilateralRise), ArchGeometry.Tented);
    public static readonly PlainArch FourCenteredOgee = new(nameof(FourCenteredOgee), "Four-Centered Ogee", "Four-centered ogee arch: haunch arcs of a third of the span reversing into concave arcs meeting in a point",
        new Interval(0.5, ArchGeometry.OgeeRise), static (span, apex) => ArchGeometry.Ogee(span, apex, 1d / 3));
    public static readonly PlainArch BasketHandle = new(nameof(BasketHandle), "Basket Handle", "Basket-handle arch: three 60° arcs rising about a third of the span",
        new Interval(ArchGeometry.BasketRise, ArchGeometry.BasketRise), static (span, _) => ArchGeometry.BasketHandle(span));
    public static readonly PlainArch Depressed = new(nameof(Depressed), "Depressed", "Depressed three-centered arch: three arcs at a chosen rise below half the span", new Interval(0, 0.5), ArchGeometry.DepressedThreeCentered);
    public static readonly PlainArch Tudor = new(nameof(Tudor), "Tudor", "Tudor arch: four arcs, quarter-span haunches turning into a flat pointed crown at a chosen rise up to the Persian rise",
        new Interval(0.25, ArchGeometry.PersianRise), ArchGeometry.Tudor);
    public static readonly PlainArch Persian = new(nameof(Persian), "Persian", "Persian arch: four arcs, haunches turning 45° into a pointed crown rising just under half the span",
        new Interval(ArchGeometry.PersianRise, ArchGeometry.PersianRise), static (span, _) => ArchGeometry.Persian(span));
    public static readonly PlainArch Keel = new(nameof(Keel), "Keel", "Keel arch: flared springing and convex arcs meeting in a point", new Interval(ArchGeometry.KeelRise, ArchGeometry.KeelRise), static (span, _) => ArchGeometry.Keel(span));
    public static readonly PlainArch Parabolic = new(nameof(Parabolic), "Parabolic", "Parabolic arch: parabola at a chosen rise", new Interval(0, double.PositiveInfinity), ArchGeometry.Parabolic);
    public static readonly PlainArch Elliptical = new(nameof(Elliptical), "Elliptical", "Elliptical arch: half ellipse at a chosen rise", new Interval(0, double.PositiveInfinity), ArchGeometry.Elliptical);

    public string Caption { get; }
    public string Help { get; }
    public Interval Ratios { get; }

    [UseDelegateFromConstructor]
    public partial IO<Seq<Curve>> Curves(ArchSpan span, Point3d apex);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
internal sealed partial class FoiledArch {
    public static readonly FoiledArch Cinquefoil = new(nameof(Cinquefoil), "Cinquefoil", "Cinquefoil arch: five lobes", IterableNE.create(5), static (span, pointed, lobes) => ArchGeometry.Foils(span, pointed, lobes, pointedCrown: pointed));
    public static readonly FoiledArch Trefoil = new(nameof(Trefoil), "Trefoil", "Trefoil arch: three lobes", IterableNE.create(3), static (span, pointed, _) => ArchGeometry.Trefoil(span, pointed));
    public static readonly FoiledArch Multifoil = new(nameof(Multifoil), "Multifoil", "Multifoil arch: an odd number of lobes keying a lobe at the crown", IterableNE.create(7, 9, 11, 13, 15, 17),
        static (span, pointed, lobes) => ArchGeometry.Foils(span, pointed, lobes, pointedCrown: false));

    public string Caption { get; }
    public string Help { get; }
    public IterableNE<int> Lobes { get; }

    [UseDelegateFromConstructor]
    public partial IO<Seq<Curve>> Curves(ArchSpan span, bool pointed, int lobes);
}

[Union]
internal abstract partial record ArchChoice {
    public string Caption => Switch(plain: static choice => choice.Arch.Caption, foiled: static choice => choice.Arch.Caption);
    public string Help => Switch(plain: static choice => choice.Arch.Help, foiled: static choice => choice.Arch.Help);
    public string Option => Switch(plain: static choice => choice.Arch.Key, foiled: static choice => choice.Arch.Key);

    public Either<PlainArch, IO<Seq<Curve>>> Spanned(ArchSpan span) =>
        Switch(span,
            plain: static (span, plain) => plain.Arch.Ratios.IsSingleton
                ? Right<PlainArch, IO<Seq<Curve>>>(plain.Arch.Curves(span, span.Apex(plain.Arch.Ratios.T0 * span.Baseline.Length)))
                : Left<PlainArch, IO<Seq<Curve>>>(plain.Arch),
            foiled: static (span, foiled) => Right<PlainArch, IO<Seq<Curve>>>(foiled.Arch.Curves(span, foiled.Pointed, foiled.Lobes)));

    public sealed record Plain(PlainArch Arch) : ArchChoice;

    public sealed record Foiled(FoiledArch Arch, bool Pointed, int Lobes) : ArchChoice;
}

[SmartEnum]
internal sealed partial class ArchFamily {
    public static readonly ArchFamily Circular = new("CircularArch", Plain(PlainArch.Semicircular, PlainArch.Segmental));
    public static readonly ArchFamily Horseshoe = new("HorseshoeArch", Plain(PlainArch.Rounded, PlainArch.PointedHorseshoe));
    public static readonly ArchFamily Gothic = new("GothicArch", Plain(PlainArch.Equilateral, PlainArch.Lancet, PlainArch.Drop));
    public static readonly ArchFamily Ogee = new("OgeeArch", Plain(PlainArch.ThreeCenteredOgee, PlainArch.ReverseOgee, PlainArch.Tented, PlainArch.FourCenteredOgee));
    public static readonly ArchFamily ThreeCentered = new("ThreeCenteredArch", Plain(PlainArch.BasketHandle, PlainArch.Depressed));
    public static readonly ArchFamily FourCentered = new("FourCenteredArch", Plain(PlainArch.Tudor, PlainArch.Persian, PlainArch.Keel));
    public static readonly ArchFamily Multifoil = new("MultifoilArch",
        IterableNE.create(FoiledArch.Cinquefoil, FoiledArch.Trefoil, FoiledArch.Multifoil).Map(static arch => (ArchChoice)new ArchChoice.Foiled(arch, Pointed: false, arch.Lobes.Head)));
    public static readonly ArchFamily Conic = new("ConicArch", Plain(PlainArch.Parabolic, PlainArch.Elliptical));

    public string Command { get; }
    public IterableNE<ArchChoice> Choices { get; }

    private static IterableNE<ArchChoice> Plain(PlainArch first, params ReadOnlySpan<PlainArch> rest) =>
        IterableNE.create(first, rest).Map(static arch => (ArchChoice)new ArchChoice.Plain(arch));
}
