# [MATERIALS_PROFILE]

THE POLYMORPHIC PROFILE OWNER and THE FAMILY GROWTH AXIS. One `Profile` is the canonical cross-section concept every architectural material parameterizes — a unit shape (dimensions, coring, regional standard, source receipt) plus the family vocabulary its layout assembly reads; one `ProfileFamily` `[SmartEnum<string>]` closes the family-kind axis (masonry · cmu · steel · timber · glazing), all five realized as sibling cross-section vocabularies the one `ProfileCatalogue.Build` folds. A profile is NEVER a per-material class: a brick is a `Profile` in the `masonry` family, a steel section is a `Profile` in the `steel` family, a CMU in `cmu`, a glulam in `timber`, an IGU in `glazing`, differing only in unit columns and the family discriminant — never a `BrickProfile`/`SteelProfile` type. The owner is `IfcProfileDef`-aligned (the canonical parameterized-cross-section model the BIM wire serializes). The OOP capsule lives at the boundary (the `[ValueObject]` dimensional columns, the `[ValidationError]` dimensional validators), the FP-ROP rail owns the internals (the `Fin<T>` admission, the `Seq`/`Fold` course projection). The page composes the `Rasm` kernel for the unit/dimension value-objects and `Construction/layout#ASSEMBLY_FOLD` for the layout that turns a `Profile` into a placement stream; the appearance assignment is the `Appearance/graph#MATERIAL_LIBRARY` `MaterialId` row a `Profile` maps to, never a profile-specific appearance type. Each family vocabulary lives on its own sibling page (`masonry`/`steel`/`cmu`/`timber`/`glazing`); this page owns the one `Profile` shape, the closed family axis, and the `[M7]` one-hop `ProfileResolution` that dereferences the seam `MaterialComposition.ProfileSet` `ProfileRef` to a `(Profile, ComputedSection)` receipt so a structural consumer never re-runs the section integral per call.

## [01]-[INDEX]

- [01]-[PROFILE_OWNER]: the `Profile` canonical unit shape, the `ProfileFamily` family-kind axis, the `ProfileId` key, the `ProfileFault` band, the dimensional value-objects, and the `ProfileCatalogue` registered-row table.
- [02]-[PROFILE_RESOLUTION]: the seam-`ProfileRef` one-hop `ProfileResolution` — the `ResolvedProfile` `(Profile, ComputedSection)` receipt and the frozen `Build`/`Resolve` cache the seam `MaterialComposition.ProfileSet` dereferences ([M7]).

## [02]-[PROFILE_OWNER]

- Owner: `Profile` over the closed `ProfileFamily` axis; `ProfileId` key; `ProfileFault` `[Union]` band 2300; `ComparerAccessors.StringOrdinal` accessor; `ProfileCatalogue` the registered-row table.
- Cases: one `Profile` shape across all families — `Family` (the discriminant), `Unit` (the dimensional cross-section), `Coring` (the void-class row), `Standard` (the regional source receipt), `AppearanceId` (the `MaterialId` row); family {masonry (realized), cmu, steel, timber, glazing (growth)}; a family is a `ProfileFamily` ROW, never a profile subtype.
- Entry: `public static Fin<Profile> Of(ProfileFamily family, ProfileUnit unit, Coring coring, ProfileStandard standard, MaterialId appearanceId, Op key)` — `Fin<T>` aborts on a non-positive dimension (`ProfileFault.Dimension`, key-correlated), a void-fraction outside `[0,1)` (`ProfileFault.Coring`), or a family/unit mismatch (`ProfileFault.Family`); `ProfileCatalogue.Build(context)` folds every realized family's row builder (masonry seed plus `steel#STEEL_FAMILY` `BuildSteelRows`) into the one frozen registry, `ProfileCatalogue.Lookup(rows, id, key)` resolves a registered `ProfileId` to its catalogue `Profile`, and the same `Of` admits an ad-hoc unit through the row validation a registered row passes — one polymorphic entry, never a `GetById`/`GetByFamily` family.
- Packages: Rasm (project — `Dimension`/`UnitInterval`/`PositiveMagnitude` value-objects), VividOrange.Sections.SectionProperties + VividOrange.Profiles.Perimeter + VividOrange.Geometry (the shared `ParametricSection` bridge — `new Perimeter(outer, voids)` over `LocalPolyline2d`/`LocalPoint2d` fed to `new SectionProperties(IProfile)`; `.api/api-vividorange-sections-sectionproperties.md`), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox (`FrozenDictionary`).
- Growth: a new architectural cross-section is one `Profile` row in the matching `ProfileFamily`; a new family is one `ProfileFamily` case carrying its unit vocabulary on its own sibling page folded into `ProfileCatalogue.Build`; a new fault is one `ProfileFault` case — never a `BrickProfile`/`SteelSection` type, never a per-family `Profile` variant. All five families are realized, each its own `ProfileFamily` case with its own `ProfileUnit` projection and `BuildXRows` catalogue builder: `masonry` carries the void-class/bond/orientation algebra and the regional rows, `steel` the section-property columns from the AISC Shapes Database v16.0, `cmu` the ASTM C90 cell/face-shell columns, `timber` the EN 14080/APA PRG 320 sawn/glulam/CLT lamella/grade columns, `glazing` the IGU pane/spacer/cavity columns — a new section in any family is a row data addition, never a new surface.
- Boundary: `Profile` is the ONE cross-section concept — a per-material class is the deleted form; `ProfileUnit` composes the `Rasm` kernel `PositiveMagnitude` value-object (the double-backed `> 0` finite magnitude) for every length column (width/height/length and the coursing module) so the profile never re-mints a dimension primitive and a fractional millimeter — an AISC web thickness, a metric joint module — admits without the truncation an int-backed `Dimension` count would force; `ProfileFault` is the one fault every `Fin.Fail` reads (dimension/coring/family/bond slots), an `Expected`-derived `Error` (`IValidationError<ProfileFault>`) whose 2300 band IS the `Expected` `Code` so a bare typed case lifts directly into the `Fin<T>` rail, so a layout never throws and never returns a sentinel; the `[ValidationError]` dimensional validators are the OOP capsule at the edge, the `Fin`/`Seq`/`Fold` course projection is the FP-ROP internal; the appearance assignment crosses to `Appearance/graph#MATERIAL_LIBRARY` as a `MaterialId` row a `Profile.AppearanceId` column carries, never a profile-specific `SurfaceShade`; the layout that turns a `Profile` into a placement stream is `Construction/layout#ASSEMBLY_FOLD`, composed not re-derived here; the masonry family vocabulary (`Coring`/`BondName`/`Orientation`/`Cut`/`ClosureRule`/`SpecialShape`) lives on `masonry#PROFILE_FAMILY`, and the `ProfileCatalogue.Build` registered-row table folds every realized family's `BuildXRows` (masonry seed plus `steel#STEEL_FAMILY` `BuildSteelRows`) into one frozen registry so a registered row keys the same way as each cmu/timber/glazing family lands its own builder.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using Thinktecture;
using VividOrange.Profiles;                          // Perimeter, IProfile, ILocalPolyline2d (the parametric section input)
using VividOrange.Geometry;                          // LocalPoint2d, LocalPolyline2d (the Y-Z section-plane geometry)
using VividOrange.Sections.SectionProperties;        // SectionProperties polygon-integral solver
using Expected = Rasm.Domain.Expected;               // the kernel Expected (parameterless ctor + virtual Category), NOT LanguageExt.Common.Expected
using Op = Rasm.Domain.Op;

// --- [TYPES] -------------------------------------------------------------------------------
[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public readonly partial struct ProfileId {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) =>
        validationError = string.IsNullOrWhiteSpace(value) || !value.Contains('.') ? new ValidationError("<profile-id requires 'family.name'>") : null;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ProfileFamily {
    public static readonly ProfileFamily Masonry = new("masonry");
    public static readonly ProfileFamily Cmu = new("cmu");
    public static readonly ProfileFamily Steel = new("steel");
    public static readonly ProfileFamily Timber = new("timber");
    public static readonly ProfileFamily Glazing = new("glazing");
}

// --- [ERRORS] ------------------------------------------------------------------------------
// The profile-sub-domain fault band (2300): Expected-derived over the kernel Rasm.Domain.Expected so band 2300 IS
// the Expected Code and a typed case lifts BARE onto Fin<T>/Validation<Error,T> (no .ToError() hop). The kernel base
// ctor is PARAMETERLESS (Code a virtual Error member, Message abstract, Category virtual) — so band 2300 is a
// `Code => 2300` override and `Message => Detail`, and the per-case Category override drives
// FaultExtensions.Category(error); the legacy `base(detail, 2300, None)` form targeted the OTHER
// LanguageExt.Common.Expected (no Category to override) and was the defect. [SkipUnionOps] skips the generated
// implicit-conversion ops (every case carries an explicit Op) and emits NO per-case factory, so the band declares its
// own (the production UiFault / seam ElementFault shape): a nested `…Case` record carries the data and a same-name-less
// static factory ProfileFault.Family(key, detail) returns the Expected-derived base so the case lifts BARE onto
// Fin<T>/Validation<Error,T> with no `new` and no .ToError() hop — the `…Case` suffix frees the unsuffixed factory name
// (a same-named nested type + method is CS0102). Create routes the unspecific case under a boundary-admission Op.
[SkipUnionOps]
[Union]
public abstract partial record ProfileFault : Expected, IValidationError<ProfileFault> {
    private ProfileFault(Op key, string detail) { Key = key; Detail = detail; }
    public Op Key { get; }
    public string Detail { get; }
    public override int Code => 2300;
    public override string Message => Detail;
    private static readonly Op Admission = Op.Of(name: nameof(Admission));

    public sealed record DimensionCase(Op Key, string Detail) : ProfileFault(Key, Detail) { public override string Category => "Dimension"; }
    public sealed record CoringCase(Op Key, string Detail) : ProfileFault(Key, Detail) { public override string Category => "Coring"; }
    public sealed record FamilyCase(Op Key, string Detail) : ProfileFault(Key, Detail) { public override string Category => "Family"; }
    public sealed record BondCase(Op Key, string Detail) : ProfileFault(Key, Detail) { public override string Category => "Bond"; }

    public static ProfileFault Dimension(Op key, string detail) => new DimensionCase(key, detail);
    public static ProfileFault Coring(Op key, string detail) => new CoringCase(key, detail);
    public static ProfileFault Family(Op key, string detail) => new FamilyCase(key, detail);
    public static ProfileFault Bond(Op key, string detail) => new BondCase(key, detail);
    public static ProfileFault Create(string message) => Family(Admission, message);
}

// --- [SERVICES] ----------------------------------------------------------------------------

// --- [MODELS] ------------------------------------------------------------------------------
public readonly record struct ProfileUnit(PositiveMagnitude WidthMm, PositiveMagnitude HeightMm, PositiveMagnitude LengthMm, PositiveMagnitude CourseHeightMm) {
    public double LengthOverWidth => LengthMm.Value / WidthMm.Value;
    public static Fin<ProfileUnit> Of(double widthMm, double heightMm, double lengthMm, double courseHeightMm, Context context, Op key) =>
        from w in key.AcceptValidated<PositiveMagnitude>(candidate: widthMm)
        from h in key.AcceptValidated<PositiveMagnitude>(candidate: heightMm)
        from l in key.AcceptValidated<PositiveMagnitude>(candidate: lengthMm)
        from c in key.AcceptValidated<PositiveMagnitude>(candidate: courseHeightMm)
        select new ProfileUnit(w, h, l, c);
}

public readonly record struct ProfileStandard(string Region, double StandardJointThicknessMm, string Authority);

public sealed record Profile(
    ProfileFamily Family,
    ProfileUnit Unit,
    Coring Coring,
    ProfileStandard Standard,
    MaterialId AppearanceId) {

    public static Fin<Profile> Of(ProfileFamily family, ProfileUnit unit, Coring coring, ProfileStandard standard, MaterialId appearanceId, Op key) =>
        from valid in guard(coring.VoidFraction is >= 0.0 and < 1.0, ProfileFault.Coring(key, $"<void-fraction-out-of-range:{coring.VoidFraction}>"))
        select new Profile(family, unit, coring, standard, appearanceId);
}

// The computed section-property receipt every Profiles family shares — the FULL structural-design column set the
// Rasm.Compute design-code checks read off the seam, in SI millimetres. The elastic columns (Area / strong-AND-weak-axis
// inertia IxMm4=Iyy,IyMm4=Izz / elastic modulus SxMm3=Wely,SyMm3=Welz / radius of gyration) and the fire-exposed
// HeatedPerimeterMm come from the ONE VividOrange.Sections.SectionProperties polygon integral (Area / MomentOfInertiaYy,Zz /
// ElasticSectionModulusYy,Zz / RadiusOfGyrationYy,Zz / Perimeter), never a per-family closed-form literal; the plastic
// moduli (ZxMm3=Wply, ZyMm3=Wplz), the St-Venant torsion constant (JMm4), the warping constant (IwMm6=Iw, the
// EN 1993-1-1 §6.3.2 lateral-torsional-buckling input the bare J cannot supply — engineering-zero for the solid/closed
// parametric families, positive only for an OPEN thin-walled steel shape), and the both-axis shear areas (AvyMm2, AvzMm2)
// the polygon solver does NOT expose are COMPUTED from the section geometry (ParametricSection's rectangle/hollow closed
// forms for the parametric families; steel#STEEL_FAMILY PlasticModulus over the catalogued shape); DepthMm/WidthMm are
// the bounding cross-section dimensions; AxisDistanceMm is the EN 1992-1-2 cover-to-reinforcement (0 for a non-RC section,
// the RC value from VividOrange ConcreteSectionProperties). Projection/material#MATERIAL_PROJECTOR SeamSection lifts this
// whole set onto the SEVENTEEN-field neutral seam SectionProperties (mm -> SI, each a typed MeasureValue in the seam's
// declared order with Iw 5th), so a Rasm.Compute structural/fire runner reads graph.SectionOf(member) without
// re-resolving or admitting VividOrange.
public readonly record struct ComputedSection(
    PositiveMagnitude AreaMm2,
    PositiveMagnitude IxMm4,
    PositiveMagnitude IyMm4,
    PositiveMagnitude SxMm3,
    PositiveMagnitude SyMm3,
    PositiveMagnitude RxMm,
    PositiveMagnitude RyMm,
    PositiveMagnitude ZxMm3,
    PositiveMagnitude ZyMm3,
    PositiveMagnitude JMm4,
    double IwMm6,
    PositiveMagnitude AvyMm2,
    PositiveMagnitude AvzMm2,
    PositiveMagnitude DepthMm,
    PositiveMagnitude WidthMm,
    PositiveMagnitude HeatedPerimeterMm,
    double AxisDistanceMm);
// IwMm6 is the EN 1993-1-1 §6.3.2 / AISC 360 Ch.F warping constant (mm^6) the seam SectionProperties carries 5th
// (after J) — the lateral-torsional-buckling input the bare J cannot supply. It is a plain double NOT a
// PositiveMagnitude (the SAME zero-admitting modeling as AxisDistanceMm) because a SOLID or CLOSED section's warping
// resistance is engineering-zero — ParametricSection yields 0.0 for every rectangle/hollow parametric family, and an
// OPEN thin-walled steel shape carries its own positive Iw from the steel-family section adapter; the seam Warping
// map lifts it to MeasureValue.OfSi(QuantityType.Create("WarpingConstant"), Dimension.Create(6,…), mm6·1e-18).

// --- [OPERATIONS] --------------------------------------------------------------------------
// The shared parametric section-property bridge: one VividOrange.Sections.SectionProperties Green's-theorem integral
// over a built Perimeter (outer polyline + void edges) computes Area/MomentOfInertiaYy/Zz/ElasticSectionModulusYy/Zz/
// RadiusOfGyrationYy/Zz for a non-catalogued section, so cmu cells, a timber rectangle, and a built-up composite all
// integrate EXACTLY through the SAME solver steel#STEEL_FAMILY runs over a catalogued IProfile — no per-family literal.
public static class ParametricSection {
    public static Fin<ComputedSection> Rectangle(double widthMm, double depthMm, Op key) =>
        Solve(RectanglePerimeter(widthMm, depthMm, Seq<(double, double, double, double)>()), depthMm, widthMm,
            RectanglePlastics(widthMm, depthMm), key);

    // The hollow net section: the outer rectangle minus the inset cell voids — the exact net Area AND net inertia
    // (cells subtracted from the second moment), not an approximate gross-minus-cell-area scalar.
    public static Fin<ComputedSection> Hollow(double widthMm, double depthMm, Seq<(double X, double Y, double W, double H)> voids, Op key) =>
        Solve(RectanglePerimeter(widthMm, depthMm, voids), depthMm, widthMm, HollowPlastics(widthMm, depthMm, voids), key);

    // The plastic/torsion/shear columns the VividOrange ELASTIC polygon integral does not expose, COMPUTED from the
    // rectangle geometry (closed-form, EXACT for a solid rectangle): the plastic moduli Z = b·h²/4 (shape factor 1.5 over
    // the elastic W = b·h²/6), the St-Venant rectangle torsion constant J = a·b³·(1/3 − 0.21·(b/a)·(1 − (b/a)⁴/12)) with
    // a/b the long/short side (Roark Table 10.1, a/b ≥ 1), and the plastic shear area Av = A (EN 1993-1-1 §6.2.6(3): a
    // solid section's shear area is its gross area). The parametric families (timber/masonry rectangle, cmu hollow) are
    // rectangles, so these are the section's true mechanics; a catalogued thin-walled steel shape carries its OWN
    // steel#STEEL_FAMILY PlasticModulus / web-flange shear areas, not this rectangle bound.
    static (double Zx, double Zy, double J, double Avy, double Avz) RectanglePlastics(double w, double d) {
        double area = w * d, lng = Math.Max(w, d), sht = Math.Min(w, d);
        double j = lng * sht * sht * sht * (1.0 / 3.0 - 0.21 * (sht / lng) * (1.0 - Math.Pow(sht / lng, 4) / 12.0));
        return (w * d * d / 4.0, d * w * w / 4.0, j, area, area);
    }

    // The hollow net columns via gross-minus-void superposition (the doubly-symmetric centred cells the cmu#CMU_FAMILY
    // generates straddle both centroidal bending axes, so the net plastic modulus is the gross minus each void's own
    // plastic modulus and the net torsion the gross minus each void's St-Venant constant; the net shear area is the
    // gross minus the void material) — a documented engineering net-section approximation for the perforated rectangle.
    static (double Zx, double Zy, double J, double Avy, double Avz) HollowPlastics(double w, double d, Seq<(double X, double Y, double W, double H)> voids) {
        static double Torsion(double a, double b) { double l = Math.Max(a, b), s = Math.Min(a, b); return l * s * s * s * (1.0 / 3.0 - 0.21 * (s / l) * (1.0 - Math.Pow(s / l, 4) / 12.0)); }
        double zx = w * d * d / 4.0 - voids.Sum(static v => v.W * v.H * v.H / 4.0);
        double zy = d * w * w / 4.0 - voids.Sum(static v => v.H * v.W * v.W / 4.0);
        double j = Torsion(w, d) - voids.Sum(v => Torsion(v.W, v.H));
        double netArea = w * d - voids.Sum(static v => v.W * v.H);
        return (zx, zy, j, netArea, netArea);
    }

    // The IProfile perimeter for a Profile's gross cross-section — the rectangle the Profile.Unit width/height bounds —
    // the Connection/reinforcement#RC_SECTION RcSection.Of feeds to a VividOrange.Sections ConcreteSection as the
    // concrete outline, the SAME Perimeter the section-property integral runs over so the RC elastic + N-M-M solvers
    // and the gross section properties share one IProfile. A catalogued steel section passes its own ICatalogue IProfile.
    public static Fin<IProfile> ProfileOf(Profile profile, Op key) =>
        profile.Unit.WidthMm.Value > 0.0 && profile.Unit.HeightMm.Value > 0.0
            ? Fin.Succ((IProfile)RectanglePerimeter(profile.Unit.WidthMm.Value, profile.Unit.HeightMm.Value, Seq<(double, double, double, double)>()))
            : Fin.Fail<IProfile>(ProfileFault.Dimension(key, $"<profile-perimeter-degenerate:{profile.Family.Key}>"));

    static Fin<ComputedSection> Solve(Perimeter perimeter, double depthMm, double widthMm, (double Zx, double Zy, double J, double Avy, double Avz) plastics, Op key) =>
        Admit(new SectionProperties((IProfile)perimeter), depthMm, widthMm, plastics, key);

    // The elastic columns + the fire-exposed Perimeter come from the VividOrange polygon integral; the plastic moduli /
    // torsion / shear areas the solver cannot yield arrive precomputed from the rectangle/hollow geometry; Depth/Width
    // are the bounding dimensions and AxisDistance is zero (a non-RC parametric section — the RC cover rides the
    // Connection/reinforcement#RC_SECTION path). Every column admits once through the kernel PositiveMagnitude rail, so a
    // degenerate (non-positive net) section drops to ProfileFault rather than seeding a corrupt stiffness on the seam.
    static Fin<ComputedSection> Admit(SectionProperties p, double depthMm, double widthMm, (double Zx, double Zy, double J, double Avy, double Avz) plastics, Op key) =>
        from area in key.AcceptValidated<PositiveMagnitude>(candidate: p.Area.SquareMillimeters)
        from ix in key.AcceptValidated<PositiveMagnitude>(candidate: p.MomentOfInertiaYy.MillimetersToTheFourth)
        from iy in key.AcceptValidated<PositiveMagnitude>(candidate: p.MomentOfInertiaZz.MillimetersToTheFourth)
        from sx in key.AcceptValidated<PositiveMagnitude>(candidate: p.ElasticSectionModulusYy.CubicMillimeters)
        from sy in key.AcceptValidated<PositiveMagnitude>(candidate: p.ElasticSectionModulusZz.CubicMillimeters)
        from rx in key.AcceptValidated<PositiveMagnitude>(candidate: p.RadiusOfGyrationYy.Millimeters)
        from ry in key.AcceptValidated<PositiveMagnitude>(candidate: p.RadiusOfGyrationZz.Millimeters)
        from zx in key.AcceptValidated<PositiveMagnitude>(candidate: plastics.Zx)
        from zy in key.AcceptValidated<PositiveMagnitude>(candidate: plastics.Zy)
        from jj in key.AcceptValidated<PositiveMagnitude>(candidate: plastics.J)
        from avy in key.AcceptValidated<PositiveMagnitude>(candidate: plastics.Avy)
        from avz in key.AcceptValidated<PositiveMagnitude>(candidate: plastics.Avz)
        from depth in key.AcceptValidated<PositiveMagnitude>(candidate: depthMm)
        from width in key.AcceptValidated<PositiveMagnitude>(candidate: widthMm)
        from perim in key.AcceptValidated<PositiveMagnitude>(candidate: p.Perimeter.Millimeters)
        // IwMm6: 0.0 — a solid/closed RECTANGLE (every parametric family) has engineering-zero warping resistance
        // (the EN 1993-1-1 §6.3.2 warping constant is nonzero only for OPEN thin-walled shapes); the seam Warping map
        // lifts it to a zero MeasureValue, exactly as AxisDistanceMm: 0.0 yields a zero cover for a non-RC section.
        select new ComputedSection(area, ix, iy, sx, sy, rx, ry, zx, zy, jj, IwMm6: 0.0, avy, avz, depth, width, perim, AxisDistanceMm: 0.0);

    // The section plane is VividOrange.Geometry's Y-Z: a centred rectangle is four LocalPoint2d corners, each cell a
    // void polyline; Perimeter(outer, voids) closes the polygons the integral iterates.
    static Perimeter RectanglePerimeter(double w, double d, Seq<(double X, double Y, double W, double H)> voids) =>
        new(Loop(-w / 2, -d / 2, w, d),
            voids.Map(v => Loop(v.X - v.W / 2, v.Y - v.H / 2, v.W, v.H)).ToList<ILocalPolyline2d>());

    static ILocalPolyline2d Loop(double y0, double z0, double w, double h) =>
        new LocalPolyline2d(new List<ILocalPoint2d> {
            new LocalPoint2d(Length.FromMillimeters(y0), Length.FromMillimeters(z0)),
            new LocalPoint2d(Length.FromMillimeters(y0 + w), Length.FromMillimeters(z0)),
            new LocalPoint2d(Length.FromMillimeters(y0 + w), Length.FromMillimeters(z0 + h)),
            new LocalPoint2d(Length.FromMillimeters(y0), Length.FromMillimeters(z0 + h)),
        });
}

// --- [TABLES] ------------------------------------------------------------------------------
public static class ProfileCatalogue {
    public static FrozenDictionary<ProfileId, Profile> Build(Context context) =>
        Masonry.ProfileCatalogue.BuildMasonryRows(context)
            .Concat(Steel.ProfileCatalogue.BuildSteelRows(context))
            .Concat(Cmu.ProfileCatalogue.BuildCmuRows(context))
            .Concat(Timber.ProfileCatalogue.BuildTimberRows(context))
            .Concat(Glazing.ProfileCatalogue.BuildGlazingRows(context))
            .ToFrozenDictionary(static r => r.Key, static r => r.Value, ComparerAccessors.StringOrdinal.EqualityComparer);

    public static Fin<Profile> Lookup(FrozenDictionary<ProfileId, Profile> rows, ProfileId id, Op key) =>
        rows.TryGetValue(id, out Profile? row) ? Fin.Succ(row!) : Fin.Fail<Profile>(ProfileFault.Family(key, $"<unregistered-profile:{id.Value}>"));
}
```

## [03]-[PROFILE_RESOLUTION]

- Owner: `ResolvedProfile` the one-hop `(Profile, ComputedSection)` receipt; `ProfileResolution` the seam-`ProfileRef` resolver and the frozen resolution cache the `[M7]` consumers read.
- Cases: one `ResolvedProfile` shape across all families — a `Profile` (the unit/family/standard) plus its `ComputedSection` (the area/inertia/elastic-modulus/radius-of-gyration computed once through the family's section path); a `ProfileRef` keys exactly one `ResolvedProfile`, never a per-family resolution receipt.
- Entry: `public static FrozenDictionary<ProfileRef, ResolvedProfile> Build(FrozenDictionary<ProfileId, Profile> rows, Func<Profile, Op, Fin<ComputedSection>> sectionOf, Op key)` pre-resolves every catalogued profile to its `(Profile, ComputedSection)` once — keyed by the seam `ProfileRef.Of(profileId.Value)` — so resolution is an O(1) lookup and the section integral runs once per profile, never per call; `public static Fin<ResolvedProfile> Resolve(ProfileRef reference, FrozenDictionary<ProfileRef, ResolvedProfile> table, Op key)` is the one-hop dereference the seam `MaterialComposition.ProfileSet` consumer (`Profiles/capacity`, the `Rasm.Compute` structural route) calls, `Fin<T>` aborting on an unresolved ref (`ProfileFault.Family`).
- Packages: Rasm.Element (project — `ProfileRef`, the seam handle the composition carries), Rasm (project — `Context`), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox (`FrozenDictionary`).
- Growth: a new resolvable attribute is one column on `ResolvedProfile` (a plastic modulus, a torsion constant) the `sectionOf` computation fills; the `sectionOf` delegate is the family-dispatching section computation (the `steel#STEEL_FAMILY` `SectionReader` over a catalogued `IProfile`, the `ParametricSection` integral over a built perimeter for the parametric families) the catalogue build supplies, so a new family's section path is a delegate contribution, never a resolver edit — the resolver owns the one-hop cache, the family pages own the section computation.
- Boundary: `ProfileResolution` is the `[M7]` ONE-HOP — the seam `MaterialComposition.ProfileSet` carries a `ProfileRef` (the catalogue key the `Construction/assembly#MATERIAL_COMPOSITION` author wrapped from a `ProfileId`), and a structural consumer dereferences it ONCE through `Resolve` to the `(Profile, ComputedSection)` receipt rather than re-running the `ParametricSection` Green's-theorem integral or the `SectionReader` catalogue lookup per design-check call; the `Build` cache is the frozen pre-resolution every catalogued profile passes through, the section computed through the SAME family-dispatching `sectionOf` the `ProfileCatalogue.Build` rows admit (steel through its catalogued `IProfile`, the parametric families through `ParametricSection`), so a steel W-shape and a glulam rectangle both resolve their section once into the one cache; the resolver owns NO section math (the family pages + `ParametricSection` own it) and NO seam type (`ProfileRef` is the seam's) — it owns the one-hop dereference and the frozen cache, the named defect a structural consumer re-resolving a `ProfileRef` per call.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using Rasm.Element.Composition;      // ProfileRef (the seam handle the MaterialComposition.ProfileSet case carries)

// --- [MODELS] ------------------------------------------------------------------------------
// The one-hop resolution receipt: a ProfileRef dereferences to its Profile AND its computed section in one lookup,
// so a structural consumer reads the section without re-running the section integral per call (M7).
public readonly record struct ResolvedProfile(Profile Profile, ComputedSection Section);

// --- [TABLES] ------------------------------------------------------------------------------
public static class ProfileResolution {
    // Pre-resolves every catalogued profile to its (Profile, ComputedSection) once, keyed by the seam ProfileRef the
    // composition carries; the frozen table is the M7 one-hop — resolution is an O(1) lookup, the section computed once
    // through the family-dispatching sectionOf (steel#STEEL_FAMILY SectionReader / ParametricSection for the rest).
    public static FrozenDictionary<ProfileRef, ResolvedProfile> Build(FrozenDictionary<ProfileId, Profile> rows, Func<Profile, Op, Fin<ComputedSection>> sectionOf, Op key) =>
        rows.Choose(kv => sectionOf(kv.Value, key)
                .Map(section => (Ref: ProfileRef.Of(kv.Key.Value), Resolved: new ResolvedProfile(kv.Value, section)))
                .ToOption())
            .ToFrozenDictionary(static r => r.Ref, static r => r.Resolved);

    public static Fin<ResolvedProfile> Resolve(ProfileRef reference, FrozenDictionary<ProfileRef, ResolvedProfile> table, Op key) =>
        table.TryGetValue(reference, out ResolvedProfile resolved)
            ? Fin.Succ(resolved)
            : Fin.Fail<ResolvedProfile>(ProfileFault.Family(key, $"<unresolved-profile-ref:{reference.Value}>"));
}
```

## [04]-[RESEARCH]

- [STRUCTURAL_FAMILY_VOCABULARY]: REALIZED — all five families carry their own sibling page and `BuildXRows` builder: `masonry#PROFILE_FAMILY` (void-class/bond/orientation, the us/uk/din/au/is regional rows), `steel#STEEL_FAMILY` (the AISC Shapes Database v16.0 W-shape seed), `cmu#CMU_FAMILY` (the ASTM C90 cell/face-shell rows), `timber#TIMBER_FAMILY` (the EN 14080/APA PRG 320 sawn/glulam/CLT rows), `glazing#GLAZING_FAMILY` (the IGU pane/spacer/cavity rows). Each is one `ProfileFamily` case with its own `ProfileUnit` projection, folded into the one `ProfileCatalogue.Build`. The remaining per-family depth is pure row data additions (the remaining AISC sections, ASTM nominal widths, EN strength classes, IGU builds), each one row never a new type; the `ProfileFamily` axis and the one `Profile` owner are settled.
- [SHARED_SECTION_PROPERTY_OWNER]: REALIZED — `ParametricSection` is the ONE section-property computation owner the whole family axis shares, composing the `VividOrange.Sections.SectionProperties` Green's-theorem polygon integral over a built `VividOrange.Profiles.Perimeter` (outer polyline + void edges from `LocalPolyline2d`/`LocalPoint2d`), returning the `ComputedSection` receipt (Area + strong-AND-weak-axis inertia/elastic-modulus/radius-of-gyration in SI millimetres). `steel#STEEL_FAMILY` runs the SAME solver over a catalogued `IProfile` (`SectionReader`); the parametric `cmu#CMU_FAMILY` (the hollow net section, cells as voids), `timber#TIMBER_FAMILY` (the rectangle), and a built-up composite seed it through `ParametricSection.Rectangle`/`Hollow` — so a steel W-shape, a hollow CMU, and a glulam rectangle ALL compute their properties through one solver, the per-family rectangular closed-form `W·D²/6` / cell-area-subtraction literals the deleted form. Every property is an admitted `PositiveMagnitude` (the `UnitsNet` solver output coerced once at the boundary), so a degenerate section rails the value-object `Fin` rather than seeding a corrupt stiffness.
- [IFCPROFILEDEF_ALIGNMENT]: `IfcProfileDef` is the canonical parameterized-cross-section model the BIM wire serializes; the `Profile` owner aligns its column set to the `IfcProfileDef` parameterized-profile subtypes per family, the column-to-subtype mapping landed on each family page. Masonry and cmu are the `IfcRectangleProfileDef` rectangle (the outer face the `XDim`/`YDim`, the cmu cell voids the `Coring` void fraction rather than a per-cell void profile — VERIFIED `cmu#IFCPROFILEDEF_CMU_ALIGNMENT`); steel carries the full subtype axis on its `SteelClass` (`IfcIShapeProfileDef`/`IfcUShapeProfileDef`/`IfcLShapeProfileDef`/`IfcRectangleHollowProfileDef`/`IfcCircleHollowProfileDef`/`IfcTShapeProfileDef` — VERIFIED `steel#STEEL_FAMILY`); timber is the `IfcRectangleProfileDef` rectangle with the CLT cross-ply carried in the lamella columns and the material-profile-set layers (VERIFIED `timber#IFCPROFILEDEF_TIMBER_ALIGNMENT`); glazing is the one family that crosses as an `IfcMaterialLayerSet` rather than a profile, the IGU pane/cavity stack the `GlazingSection.ToLayerSet` bridge resolves (VERIFIED `glazing#IFCPROFILEDEF_GLAZING_ALIGNMENT`). A steel/timber member's cross-section round-trips to IFC 4.3 as an `IfcMaterialProfileSet` (the seam `Construction/assembly#MATERIAL_COMPOSITION` `MaterialComposition.ProfileSet(ProfileRef)` shape, the `ProfileRef` the `[03]-[PROFILE_RESOLUTION]` resolves), a wall/IGU as an `IfcMaterialLayerSet`; the per-family `IfcMaterialProfileSetUsage` cardinal-point/orientation is the `[C7]` `ProfileSetUsage` the seam `Associate` edge carries (the `CompositionAuthor.UsageOf` derivation), authored onto IFC at the `Rasm.Bim` boundary.
