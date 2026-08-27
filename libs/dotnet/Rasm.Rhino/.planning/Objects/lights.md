# [RASM_RHINO_OBJECTS_LIGHTS]

## [01]-[INDEX]

- [02]-[KIND_AND_STAMP]: `LightModality`, `LightKind`, `SpotShape`, `ConeEvidence`, `AreaShape`, `LightFrame`, `LightFalloff`, `LightAttenuation`, `LightStamp` — the capability rows and the detached read.
- [03]-[SEED_AND_EDIT]: `LightSeed`, `RadianceUnit`, `Radiance`, `LightShade`, `LightEdit` — construction and the gated property edits.
- [05]-[SURFACE_LEDGER]: the page's owner table.

## [02]-[KIND_AND_STAMP]

- Owner: `LightModality` is the four-row capability vocabulary every edit gate reads; `LightKind` `[SmartEnum<int>]` keys every world style by its host `LightStyle` ordinal and carries `Grants` beside the descriptor `Wire` key; `SpotShape` couples the kernel cone with its hot-spot fraction; `AreaShape` carries the linear and rectangular extent vectors; `LightFalloff` closes the host attenuation roster with its coefficient vector and wire key; `LightAttenuation` is the seatable value over that roster and the free vector; `LightStamp` is the whole detached light read.
- Law: the family is closed at the world styles — `LightKind.Of` resolves through `Op.Row`'s host-enum arm, so a camera-space style, `Ambient`, and the sun style refuse at the gate instead of leaking a sixth modality; the sun's derived `Light` is `SunEvidence` custody on the render settings page and never enters this pipeline.
- Law: modality is ONE capability set, never four bool columns. `Grants` states what a kind admits, an edit states what it `Requires`, and every gate is `kind.Grants.Require(edit.Requires, refuse)` — a fifth modality is one `LightModality` row and one column edit, and no arm re-derives a capability the refusal already receives as its missing set.
- Law: cone math is kernel-owned — the spot cone crosses as `VectorCone` (apex, unit axis, admitted half-angle), half-angle and solid-angle questions answer through `ConeProjection` rows on the stamped cone, and inline spot trigonometry beside the owner is the deleted form.
- Law: colour crosses through the kernel boundary members ALONE — `PerceptualColor.OfHost` admits every host read and `ToDrawing` bounds every host write, so no component arithmetic, no byte-into-double alpha, and no hand `FromArgb` survives on this page. Writing is FALLIBLE by construction: a colour outside the display gamut is a paint instruction no consumer can attribute, so a shade edit refuses rather than seating a silently clipped colour.
- Law: the stamp is host evidence — intensity, watt, lumen, candela, shadow, spot-angle, and hot-spot values cross raw as read, and every WRITE payload admits (`UnitInterval` fractions, positive finite radiance, kernel cone), so reads never refuse a degenerate document and writes never pass one. `AreaShape` remains raw read evidence and admits both extent vectors at every write boundary.
- Law: cone absence and cone refusal are DIFFERENT answers — `ConeEvidence` carries `Absent` for a kind with no cone, `Shaped` for an admitted one, and `Degenerate` where the kind carries a cone the document's tolerance cannot admit; collapsing the last two onto one `None` erases the only signal a consumer has that the document, not the light, is the problem, while the raw scalars keep the underlying evidence in every case. `SceneMap` exhaustively seats the generated oneof, so no text vocabulary stands beside the union.
- Law: the coordinate frame and the attenuation model are bounded reads — `LightFrame` closes the host coordinate roster and `LightFalloff.Of` REFUSES an unrecognized attenuation row rather than reading its coefficient vector; `LightAttenuation.Free` survives as the write payload a caller seats through `SetAttenuation`, and the host's own read classifies that seated light back onto one of its three named rows.
- Law: `LightFalloff` is the RENAMED host mirror, never the kernel `Falloff` — `Numerics/calculus.md`'s `Falloff` is a radial-decay WEIGHT PROFILE carrying `SlopeBound`, `Weight`, and a metric sampler, while this row is the `Light.Attenuation` coefficient regime the host seats through `SetAttenuation`. Both wear one word over two concepts; composing the kernel union here hands a caller a Gaussian spread to write into a quadratic coefficient slot.
- Packages: Thinktecture.Runtime.Extensions (`[SmartEnum<T>]`, `[Union]`, `[ComplexValueObject]`, `[ValidationError]`, `[UseDelegateFromConstructor]`, `KeyMemberEqualityComparer`); LanguageExt.Core (`Fin`, `Option`, `Seq`, `Traverse`, `Choose`); RhinoCommon objects (`.api/api-rhinocommon-objects.md` — `Light`, `LightObject`, `LightStyle`, `Light.Attenuation`, `LightTable`); kernel `Numerics/atoms` (`PerceptualColor.OfHost`/`ToDrawing`/`ToRgb`, `UnitInterval`, `VectorCone`); kernel `Domain/validation` (`Op.Row`, `ICapability`, `CapabilitySet`); `Document/session.md` (`DraftFault`); `Document/tables.md` (`ResourceIndex`).

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Rasm.Domain;
using Rasm.Numerics;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
// Contracts are retired from this logic.

namespace Rasm.Rhino.Objects;


// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LightModality : ICapability<LightModality> {
    public static readonly LightModality Aims = new(key: "aims");
    public static readonly LightModality Cone = new(key: "cone");
    public static readonly LightModality Extent = new(key: "extent");
    public static readonly LightModality Breadth = new(key: "breadth");
}

[SmartEnum<int>]
public sealed partial class LightKind {
    private static readonly CapabilitySet<LightModality> Aimed = CapabilitySet<LightModality>.Of(LightModality.Aims);
    private static readonly CapabilitySet<LightModality> Placed = CapabilitySet<LightModality>.Of();
    private static readonly CapabilitySet<LightModality> Coned = CapabilitySet<LightModality>.Of(
        LightModality.Aims, LightModality.Cone);
    private static readonly CapabilitySet<LightModality> Stretched = CapabilitySet<LightModality>.Of(LightModality.Extent);
    private static readonly CapabilitySet<LightModality> Panelled = CapabilitySet<LightModality>.Of(
        LightModality.Extent, LightModality.Breadth);

    public static readonly LightKind Directional = new(key: (int)LightStyle.WorldDirectional, wire: Wire.LightKind.Directional, style: LightStyle.WorldDirectional, grants: Aimed);
    public static readonly LightKind Point = new(key: (int)LightStyle.WorldPoint, wire: Wire.LightKind.Point, style: LightStyle.WorldPoint, grants: Placed);
    public static readonly LightKind Spot = new(key: (int)LightStyle.WorldSpot, wire: Wire.LightKind.Spot, style: LightStyle.WorldSpot, grants: Coned);
    public static readonly LightKind Linear = new(key: (int)LightStyle.WorldLinear, wire: Wire.LightKind.Linear, style: LightStyle.WorldLinear, grants: Stretched);
    public static readonly LightKind Rectangular = new(key: (int)LightStyle.WorldRectangular, wire: Wire.LightKind.Rectangular, style: LightStyle.WorldRectangular, grants: Panelled);

    internal Wire.LightKind Wire { get; }
    internal LightStyle Style { get; }
    internal CapabilitySet<LightModality> Grants { get; }

    internal static Fin<LightKind> Of(LightStyle style, Op key) =>
        key.Row<LightStyle, LightKind>(candidate: style, ordinal: static value => (int)value);
}

[SmartEnum<int>]
public sealed partial class LightFalloff {
    public static readonly LightFalloff Constant = new(
        key: (int)Light.Attenuation.Constant, wire: Wire.Falloff.Constant, vector: Light.ConstantAttenuationVector);
    public static readonly LightFalloff Linear = new(
        key: (int)Light.Attenuation.Linear, wire: Wire.Falloff.Linear, vector: Light.LinearAttenuationVector);
    public static readonly LightFalloff InverseSquared = new(
        key: (int)Light.Attenuation.InverseSquared, wire: Wire.Falloff.InverseSquared, vector: Light.InverseSquaredAttenuationVector);

    internal Wire.Falloff Wire { get; }
    internal Vector3d Vector { get; }

    internal static Fin<LightFalloff> Of(Light.Attenuation model, Op key) =>
        key.Row<Light.Attenuation, LightFalloff>(candidate: model, ordinal: static value => (int)value);
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LightAttenuation {
    private LightAttenuation() { }
    public sealed record Named(LightFalloff Row) : LightAttenuation;
    public sealed record Free(Vector3d Coefficients) : LightAttenuation;

    internal Vector3d Coefficients => Switch<Vector3d>(
        named: static law => law.Row.Vector, free: static law => law.Coefficients);

    internal static Fin<LightAttenuation> Of(Light native, Op key) =>
        LightFalloff.Of(model: native.AttenuationType, key: key)
            .Map(static row => (LightAttenuation)new Named(Row: row));

    internal Fin<LightAttenuation> Admit(Op op) =>
        Switch(
            context: op,
            named: static (_, law) => Fin.Succ<LightAttenuation>(law),
            free: static (key, law) => key.AcceptInput(value: law.Coefficients).Map(_ => (LightAttenuation)law));

    internal Unit Apply(Light working) {
        Vector3d seat = Coefficients;
        working.SetAttenuation(seat.X, seat.Y, seat.Z);
        return unit;
    }
}

[SmartEnum<int>]
public sealed partial class LightFrame {
    public static readonly LightFrame World = new(key: (int)CoordinateSystem.World);
    public static readonly LightFrame Camera = new(key: (int)CoordinateSystem.Camera);
    public static readonly LightFrame Clip = new(key: (int)CoordinateSystem.Clip);
    public static readonly LightFrame Screen = new(key: (int)CoordinateSystem.Screen);

    internal CoordinateSystem Host => (CoordinateSystem)Key;

    internal static Fin<LightFrame> Of(CoordinateSystem system, Op key) =>
        key.Row<CoordinateSystem, LightFrame>(candidate: system, ordinal: static value => (int)value);
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct SpotShape(VectorCone Cone, UnitInterval HotSpot) {
    internal Fin<SpotShape> Admit(Op op) =>
        from _ in Rasm.Domain.Admit.Cone(
            apex: Cone.Apex,
            axis: Cone.Axis.Value,
            halfAngle: Cone.HalfAngle.Value,
            key: op)
        from hotSpot in op.AcceptValidated<UnitInterval>(candidate: (double)HotSpot)
        select new SpotShape(Cone: Cone, HotSpot: hotSpot);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ConeEvidence {
    private ConeEvidence() { }
    public sealed record Absent : ConeEvidence;
    public sealed record Shaped(SpotShape Value) : ConeEvidence;
    public sealed record Degenerate : ConeEvidence;

}

public readonly record struct AreaShape(Vector3d Length, Option<Vector3d> Width = default) {
    internal Fin<AreaShape> Admit(Op op) =>
        from length in op.AcceptInput(value: Length)
        from width in Width.Traverse(value => op.AcceptInput(value: value)).As()
        select new AreaShape(Length: length, Width: width);

    internal Fin<AreaShape> Scaled(double scale, Op op) =>
        new AreaShape(Length: Length * scale, Width: Width.Map(width => width * scale)).Admit(op: op);
}

public sealed record LightStamp(
    Guid Id,
    ResourceIndex Index,
    Option<string> Name,
    LightKind Kind,
    bool Enabled,
    Point3d Location,
    Vector3d Direction,
    Vector3d PerpendicularDirection,
    LightFrame Frame,
    double Intensity,
    double Watts,
    double Lumens,
    double Candela,
    PerceptualColor Diffuse,
    PerceptualColor Ambient,
    PerceptualColor Specular,
    double Shadow,
    double SpotAngle,
    double HotSpot,
    ConeEvidence Cone,
    Option<AreaShape> Area,
    LightAttenuation Attenuation) : IDetachedDocumentResult {
    internal static Fin<LightStamp> Of(ResourceIndex index, LightObject native, Context model, Op key) =>
        key.Catch(() =>
            from light in Optional(native.LightGeometry).ToFin(Fail: key.InvalidResult())
            from kind in LightKind.Of(style: light.LightStyle, key: key)
            from frame in LightFrame.Of(system: light.CoordinateSystem, key: key)
            from attenuation in LightAttenuation.Of(native: light, key: key)
            from diffuse in PerceptualColor.OfHost(host: light.Diffuse, key: key)
            from ambient in PerceptualColor.OfHost(host: light.Ambient, key: key)
            from specular in PerceptualColor.OfHost(host: light.Specular, key: key)
            from cone in kind.Grants.Admits(capability: LightModality.Cone)
                ? (from value in VectorCone.Of(
                       apex: light.Location, axis: light.Direction,
                       halfAngleRadians: light.SpotAngleRadians, context: model, key: key)
                   from hot in key.AcceptValidated<UnitInterval>(candidate: light.HotSpot)
                   select (ConeEvidence)new ConeEvidence.Shaped(Value: new SpotShape(Cone: value, HotSpot: hot)))
                    .BindFail(static _ => Fin.Succ<ConeEvidence>(value: new ConeEvidence.Degenerate()))
                : Fin.Succ<ConeEvidence>(value: new ConeEvidence.Absent())
            select new LightStamp(
                Id: native.Id,
                Index: index,
                Name: Op.Text(light.Name),
                Kind: kind,
                Enabled: light.IsEnabled,
                Location: light.Location,
                Direction: light.Direction,
                PerpendicularDirection: light.PerpendicularDirection,
                Frame: frame,
                Intensity: light.Intensity,
                Watts: light.PowerWatts,
                Lumens: light.PowerLumens,
                Candela: light.PowerCandela,
                Diffuse: diffuse,
                Ambient: ambient,
                Specular: specular,
                Shadow: light.ShadowIntensity,
                SpotAngle: light.SpotAngleRadians,
                HotSpot: light.HotSpot,
                Cone: cone,
                Area: kind.Grants.Admits(capability: LightModality.Extent)
                    ? Some(new AreaShape(
                        Length: light.Length,
                        Width: kind.Grants.Admits(capability: LightModality.Breadth)
                            ? Some(light.Width)
                            : Option<Vector3d>.None))
                    : Option<AreaShape>.None,
                Attenuation: attenuation));
}
```

## [03]-[SEED_AND_EDIT]

- Owner: `LightSeed` `[Union]` closes construction — each case carries its own payload, answers its `Kind` through one total fold, and seats it through one; `RadianceUnit` `[SmartEnum<string>]` closes the photometric write axis as delegate rows; `Radiance` is the admitted `(unit, magnitude)` pair; `LightShade` couples the diffuse write with its optional ambient and specular companions; `LightEdit` `[Union]` closes rename, enablement, power, shade, shadow, cone, area, placement, aim, and attenuation over one gated dispatch.
- Law: modality gates read the capability set ONCE — `LightEdit.Requires` states what the edit needs and `kind.Grants.Require` answers, so a per-arm style ladder and its four separately-spelled guards collapse to one refusal whose axis is the MISSING rows the door hands it, never the whole demand restated.
- Law: seeds admit before the host — locations, vectors, and complete area shapes pass admission before the document grant, the spot seed consumes an already-admitted `SpotShape`, and the style writes exactly once at mint, so no half-styled light ever reaches the table.
- Law: the mint owns its native — `LightSeed.Mint` funnels the host construction through `Lease<Light>.Acquire` and brackets the seat, so a refused or throwing seat releases the fresh `Light` before the fault leaves and the table commit consumes the lease through `Use`; a raw returned handle relies on the caller reaching its own `using` first, which a throwing seat forecloses.
- Law: photometric power is one axis carrying one magnitude — `RadianceUnit` rows own the host member each unit seats, `Radiance` admits the magnitude once at construction, and a per-unit sibling edit family is the collapsed form. NAMED LOSS: per-unit compile-time exhaustiveness at a consumer — bought back by `Radiance.Unit` row equality, which reads the same discriminant as a value and gains the `Wire` key the union never published.
- Law: pose is TWO edits, never one optional pair. `Place` moves and `Aim` turns; a program carrying both applies both to ONE working duplicate inside a single `Modify`, so atomicity is the commit's and the "both absent" corner is unrepresentable rather than guarded.
- Packages: Thinktecture.Runtime.Extensions (`[Union]`, `[SmartEnum<string>]`, `[ComplexValueObject]`, `[UseDelegateFromConstructor]`, `[ValidationError]`); LanguageExt.Core (`Fin`, `Option`, `Seq`, `TraverseM`); kernel `Domain/results` (`Lease<T>.Acquire`, `Lease<T>.Use`); kernel `Numerics/atoms` (`PerceptualColor.ToDrawing`, `UnitInterval`); kernel `Domain/validation` (`CapabilitySet.Require`); RhinoCommon objects (`Light.Intensity`/`PowerWatts`/`PowerLumens`/`PowerCandela`, `SetAttenuation`).

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RadianceUnit {
    public static readonly RadianceUnit Scale = new(key: "scale", seat: static (light, value) => light.Intensity = value);
    public static readonly RadianceUnit Watts = new(key: "watts", seat: static (light, value) => light.PowerWatts = value);
    public static readonly RadianceUnit Lumens = new(key: "lumens", seat: static (light, value) => light.PowerLumens = value);
    public static readonly RadianceUnit Candela = new(key: "candela", seat: static (light, value) => light.PowerCandela = value);

    [UseDelegateFromConstructor]
    internal partial void Seat(Light working, double value);
}

[ComplexValueObject]
[ValidationError]
public sealed partial class Radiance {
    public RadianceUnit Unit { get; }
    public double Magnitude { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError, ref RadianceUnit unit, ref double magnitude) {
        Op op = Op.Of(name: nameof(Radiance));
        double value = magnitude;
        validationError = FactoryValidation.Of(FactoryValidation.Violated(
                (unit is null, () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(Unit) }))),
                (!double.IsFinite(value) || value <= 0d, () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(Magnitude), value, "positive and finite" })))));
    }

    public static Fin<Radiance> Of(RadianceUnit unit, double magnitude, Op? key = null) =>
        key.OrDefault().AcceptValidated<Radiance>(
            fault: Validate(unit, magnitude, out Radiance? admitted), admitted: admitted);

    internal Unit Apply(Light working) {
        Unit.Seat(working: working, value: Magnitude);
        return unit;
    }
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LightSeed {
    private LightSeed() { }
    public sealed record Point(Point3d Location) : LightSeed;
    public sealed record Spot(SpotShape Shape) : LightSeed;
    public sealed record Directional(Point3d Location, Vector3d Direction) : LightSeed;
    public sealed record Linear(Point3d Location, Vector3d Length) : LightSeed;
    public sealed record Rectangular(Point3d Corner, Vector3d Length, Vector3d Width) : LightSeed;

    internal LightKind Kind => Map(
        point: static _ => LightKind.Point,
        spot: static _ => LightKind.Spot,
        directional: static _ => LightKind.Directional,
        linear: static _ => LightKind.Linear,
        rectangular: static _ => LightKind.Rectangular);

    internal Fin<LightSeed> Admit(Op op) =>
        Switch(
            context: op,
            point: static (key, seed) =>
                from location in key.AcceptInput(value: seed.Location)
                select (LightSeed)new Point(Location: location),
            spot: static (key, seed) => seed.Shape.Admit(op: key)
                .Map(shape => (LightSeed)new Spot(Shape: shape)),
            directional: static (key, seed) =>
                from location in key.AcceptInput(value: seed.Location)
                from direction in key.AcceptInput(value: seed.Direction)
                select (LightSeed)new Directional(Location: location, Direction: direction),
            linear: static (key, seed) =>
                from location in key.AcceptInput(value: seed.Location)
                from area in new AreaShape(Length: seed.Length).Admit(op: key)
                select (LightSeed)new Linear(Location: location, Length: area.Length),
            rectangular: static (key, seed) =>
                from corner in key.AcceptInput(value: seed.Corner)
                from area in new AreaShape(Length: seed.Length, Width: Some(seed.Width)).Admit(op: key)
                from width in area.Width.ToFin(Fail: key.InvalidResult())
                select (LightSeed)new Rectangular(Corner: corner, Length: area.Length, Width: width));

    private Unit Seat(Light working) =>
        Switch(
            context: working,
            point: static (light, seed) => { light.Location = seed.Location; return unit; },
            spot: static (light, seed) => {
                light.Location = seed.Shape.Cone.Apex;
                light.Direction = seed.Shape.Cone.Axis.Value;
                light.SpotAngleRadians = seed.Shape.Cone.HalfAngle.Value;
                light.HotSpot = (double)seed.Shape.HotSpot;
                return unit;
            },
            directional: static (light, seed) => {
                light.Location = seed.Location;
                light.Direction = seed.Direction;
                return unit;
            },
            linear: static (light, seed) => {
                light.Location = seed.Location;
                light.Length = seed.Length;
                return unit;
            },
            rectangular: static (light, seed) => {
                light.Location = seed.Corner;
                light.Length = seed.Length;
                light.Width = seed.Width;
                return unit;
            });

    internal Fin<Lease<Light>> Mint(Op op) {
        LightSeed seed = this;
        return Lease<Light>.Acquire(
                mint: () => new Light { LightStyle = seed.Kind.Style, IsEnabled = true }, key: op)
            .Bind(lease => op.Catch(() => Fin.Succ(value: seed.Seat(working: lease.Resource)))
                .Map(_ => lease)
                .Rollback(release: () => op.Catch(() => Fin.Succ(value: lease.Dispose())), key: op));
    }
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LightEdit {
    private LightEdit() { }
    public sealed record Rename(string Name) : LightEdit;
    public sealed record Toggle(ObjectSignal Signal) : LightEdit;
    public sealed record Power(Radiance Value) : LightEdit;
    public sealed record Shade(LightShade Value) : LightEdit;
    public sealed record Shadow(UnitInterval Value) : LightEdit;
    public sealed record Cone(SpotShape Value) : LightEdit;
    public sealed record Area(AreaShape Value) : LightEdit;
    public sealed record Place(Point3d Location) : LightEdit;
    public sealed record Aim(Vector3d Direction) : LightEdit;
    public sealed record Attenuate(LightAttenuation Value) : LightEdit;

    internal CapabilitySet<LightModality> Requires => Switch<CapabilitySet<LightModality>>(
        rename: static _ => CapabilitySet<LightModality>.Of(),
        toggle: static _ => CapabilitySet<LightModality>.Of(),
        power: static _ => CapabilitySet<LightModality>.Of(),
        shade: static _ => CapabilitySet<LightModality>.Of(),
        shadow: static _ => CapabilitySet<LightModality>.Of(),
        cone: static _ => CapabilitySet<LightModality>.Of(LightModality.Cone),
        area: static edit => edit.Value.Width.IsSome
            ? CapabilitySet<LightModality>.Of(LightModality.Extent, LightModality.Breadth)
            : CapabilitySet<LightModality>.Of(LightModality.Extent),
        place: static _ => CapabilitySet<LightModality>.Of(),
        aim: static _ => CapabilitySet<LightModality>.Of(LightModality.Aims),
        attenuate: static _ => CapabilitySet<LightModality>.Of());

    internal Fin<LightEdit> Admit(Op op) =>
        Switch(
            context: op,
            rename: static (key, edit) => key.AcceptText(value: edit.Name).Map(name => (LightEdit)new Rename(Name: name)),
            toggle: static (key, edit) => key.Need(edit.Signal).Map(_ => (LightEdit)edit),
            power: static (key, edit) => key.Need(edit.Value).Map(_ => (LightEdit)edit),
            shade: static (key, edit) => key.Need(edit.Value)
                .Bind(shade => shade.Admit(op: key)).Map(shade => (LightEdit)new Shade(Value: shade)),
            shadow: static (key, edit) => key.AcceptValidated<UnitInterval>(candidate: (double)edit.Value)
                .Map(value => (LightEdit)new Shadow(Value: value)),
            cone: static (key, edit) => edit.Value.Admit(op: key)
                .Map(shape => (LightEdit)new Cone(Value: shape)),
            area: static (key, edit) => edit.Value.Admit(op: key)
                .Map(area => (LightEdit)new Area(Value: area)),
            place: static (key, edit) => key.AcceptInput(value: edit.Location)
                .Map(location => (LightEdit)new Place(Location: location)),
            aim: static (key, edit) => key.AcceptInput(value: edit.Direction)
                .Map(direction => (LightEdit)new Aim(Direction: direction)),
            attenuate: static (key, edit) => key.Need(edit.Value)
                .Bind(law => law.Admit(op: key)).Map(law => (LightEdit)new Attenuate(Value: law)));

    internal Fin<Unit> Apply(Light working, LightKind kind, Op op) =>
        kind.Grants
            .Require(demanded: Requires, refuse: missing => op.InvalidInput(axis: missing.Wire))
            .Bind(_ => Seat(working: working, op: op));

    private Fin<Unit> Seat(Light working, Op op) =>
        Switch(
            context: (Working: working, Op: op),
            rename: static (context, edit) => context.Op.Catch(() => context.Working.Name = edit.Name),
            toggle: static (context, edit) => context.Op.Catch(() => context.Working.IsEnabled = edit.Signal.On),
            power: static (context, edit) => context.Op.Catch(() => edit.Value.Apply(working: context.Working)),
            shade: static (context, edit) => edit.Value.Seat(working: context.Working, op: context.Op),
            shadow: static (context, edit) => context.Op.Catch(() => context.Working.ShadowIntensity = (double)edit.Value),
            cone: static (context, edit) => context.Op.Catch(() => {
                context.Working.Location = edit.Value.Cone.Apex;
                context.Working.Direction = edit.Value.Cone.Axis.Value;
                context.Working.SpotAngleRadians = edit.Value.Cone.HalfAngle.Value;
                context.Working.HotSpot = (double)edit.Value.HotSpot;
            }),
            area: static (context, edit) => context.Op.Catch(() => {
                context.Working.Length = edit.Value.Length;
                _ = edit.Value.Width.Iter(width => context.Working.Width = width);
            }),
            place: static (context, edit) => context.Op.Catch(() => context.Working.Location = edit.Location),
            aim: static (context, edit) => context.Op.Catch(() => context.Working.Direction = edit.Direction),
            attenuate: static (context, edit) => context.Op.Catch(() => edit.Value.Apply(working: context.Working)));
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record LightShade(
    PerceptualColor Diffuse,
    Option<PerceptualColor> Ambient = default,
    Option<PerceptualColor> Specular = default) {
    internal Fin<LightShade> Admit(Op op) =>
        from diffuse in op.Need(Diffuse)
        from ambient in Ambient.Traverse(value => op.Need(value)).As()
        from specular in Specular.Traverse(value => op.Need(value)).As()
        select new LightShade(Diffuse: diffuse, Ambient: ambient, Specular: specular);

    internal Fin<Unit> Seat(Light working, Op op) =>
        from diffuse in Diffuse.ToDrawing(key: op)
        from ambient in Ambient.Traverse(shade => shade.ToDrawing(key: op)).As()
        from specular in Specular.Traverse(shade => shade.ToDrawing(key: op)).As()
        from _ in op.Catch(() => {
            working.Diffuse = diffuse;
            _ = ambient.Iter(shade => working.Ambient = shade);
            _ = specular.Iter(shade => working.Specular = shade);
        })
        select unit;
}
```

## [04]-[ASK_AND_COMMIT]

- Law: resolution is index-paired but id-addressed — the complete roster enumerates the table's own `IEnumerable<LightObject>` and resolves each live row's slot from its id, while `Find`, `FindName`, and the revival probe address a single row. Every host index crosses `ResourceIndex.Maybe`, whose `-1` refusal IS the host's miss answer, so the pipeline admits no negative slot. Slot addressing survives only because `Modify`, `Delete`, and `Undelete` take one, and a deleted row has no live id, so the index address stays the revival ingress alone.
- Law: the working copy is the mutation site — an amend duplicates through `DuplicateLightGeometry`, applies its admitted edits to the duplicate, and lands once through `Modify(index, working)`, so the live table never observes a half-applied edit and the duplicate disposes on every path.
- Law: the purge's host-dialogue column is the spine's `HostInteraction`, not `ObjectSignal`. `ObjectSignal` names ENABLED and DISABLED, and the host argument it fed is `quiet` — so `ObjectSignal.Enabled` on a column named `Quiet` read as "enabled" at every call site and meant "suppress the warning", an inversion that is invisible at the call and wrong in exactly the direction that silences a dialogue a caller asked for. `HostInteraction.Quiet`/`Interactive` name the posture directly and `IsQuiet` is the only read.
- Law: the commit rides `ObjectSpine.Commit` — admission precedes the grant, every light verb records undo, and the command returns `Unit` after the complete operation roster settles.
- Law: placement stays the object pipeline — whole-object transform, delete-by-id, and selection of `LightObject`s ride `TableOp` through `TableTarget.Query` with `IncludeLights`; this pipeline owns what the object pipeline cannot spell — light-specific properties, index-addressed table verbs, and kind-gated modality.
- Law: this page is the WHOLE-DESCRIPTOR emitter and the strata edge is downward — Objects (S2) composes the Render (S1) `SceneSun` band as an admitted VALUE beside its own photometric rows, so the sun's astronomy stays its owner's and no second solar derivation exists here. Shading rides as call data: the GLB body is the manifest's `keyed-artifact`/`glb` product — `Rasm.Bim`'s `Exchange/export#EXPORT_PIPELINE` emits it under `Rasm.Compute`'s content keys — so this emitter carries its artifact coordinate, counts, and declared fidelity and never tessellates.
- Law: the descriptor emits METRES from ONE authority — the capture reads the document's `ModelUnit` inside the grant, scales every pose and extent by `MetersPerUnit`, and publishes that same regime as `source_unit`, so the factor and its provenance cannot disagree and no caller supplies a scale the descriptor then contradicts.
- Law: spectra cross LINEAR and OPAQUE. `SceneSpectrum.Of` reads `PerceptualColor` through `RgbProfile.Srgb` under `RgbTransfer.Linear`, so the sRGB byte leg never reaches a wire declaring scene light; and because the message carries three components, a non-opaque light colour REFUSES at the producer rather than dropping its coverage silently. Both paths leave the stamp's full colour intact.
- Law: identity crosses as RFC-4122 byte order. `Guid.ToByteArray(bigEndian: true)` is the mapper's ONE identity column, because the platform's default layout writes the first three fields little-endian and the consuming peer reads `row.id.hex()` — the two orders agree on nothing but the last eight bytes, so a byte-order slip renames every light in the descriptor without failing a decode.
- Law: artifacts cross through kernel `ArtifactContent` — its 32-byte SHA-256 identity and unsigned extent admit at the native owner, and the mapper projects that value once onto generated `ArtifactRef`; the scene content key remains the independent XXH3-128 digest of the descriptor preimage.
- Law: closed vocabularies cross as generated enums and oneofs. `LightKind`, `ConeEvidence`, `LightAttenuation`, power authority, and web dialect each have one typed producer-to-wire fold; no string alias is parsed by a peer.
- Law: authority is a producer RULE over four host readings, never a host flag — `Light` stores one power and publishes watts, lumens, and candela as converted views with no field naming which the modeller set, so `radiant-flux-w` claims the row exactly when watts reads finite and positive and `relative-scale` claims it otherwise. `relative-scale` is dimensionless and refuses every engine photometric slot by name.
- Law: write admissions bind the wire too — `shadow_fraction` crosses as an admitted `UnitInterval` even though the read path takes `ShadowIntensity` raw, so a degenerate document refuses at the producer rather than seating an out-of-range fraction on a consuming engine.
- Law: the descriptor `key` is the seed-zero content identity of the generated descriptor with only that key omitted. Protobuf field tags, oneofs, repeated order, and nested artifact coordinates frame every band once, so a new schema field joins the preimage through the same mapper instead of a second hand-maintained field roster.
- Boundary: the photometric-web payload is the render kinds page's `PhotometricFile` — dialect-admitted by `PhotometricDialect`, minted through `PhotometricPress`, and landed on the light's attached render material child slot; this pipeline's photometric reach ends at `Radiance`, `IPhotometricRegistry` is the ONE address it holds into that stratum, the descriptor carries one `ArtifactRef` plus a generated dialect, and `LightEdit` never grows an IES case.
- Law: an unread column is a defect, not thrift — the web column has a producer, so `Lights.Capture` demands the registry rather than defaulting it, and the consuming census that counts web-bearing rows counts documents rather than an absence this emitter manufactured.
- Boundary: `LightTable.Sun` and `Skylight` stay the render settings page's — `SunState`, `SkylightState`, and `SunEvidence` own that projection; `EventFamily.LightTable` already observes this table onto `EventPayload.Component(TableKind.Lights, …)`, and `SnapshotCategory.Lights` carries snapshot participation.
- Growth: a new light verb is one `LightOp` case; a new property axis is one `LightEdit` case with its `Requires` column; a new descriptor column is one appended proto field beside one mapper column.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Google.Protobuf;
using NodaTime;
using NodaTime.Serialization.Protobuf;
// Contracts are retired from this logic.

// --- [TYPES] ---------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LightSelect {
    private LightSelect() { }
    public sealed record Every : LightSelect;
    public sealed record At(int Index) : LightSelect;
    public sealed record Of(Guid Id) : LightSelect;
    public sealed record Named(string Name) : LightSelect;

    internal Fin<Seq<(ResourceIndex Index, LightObject Native)>> Resolve(RhinoDoc document, Op key) =>
        Switch(
            context: (Document: document, Op: key),
            every: static (context, _) => context.Op.Catch(() => Fin.Succ(value:
                context.Document.Lights.AsIterable().ToSeq()
                    .Filter(static native => !native.IsDeleted)
                    .Choose(native => ResourceIndex
                        .Maybe(value: context.Document.Lights.Find(native.Id, ignoreDeleted: true))
                        .Map(index => (Index: index, Native: native))))),
            at: static (context, address) => context.Op.Catch(() => Row(
                document: context.Document, index: address.Index, key: context.Op)),
            of: static (context, address) => context.Op.Catch(() => Row(
                document: context.Document,
                index: context.Document.Lights.Find(address.Id, ignoreDeleted: true),
                key: context.Op)),
            named: static (context, address) =>
                from name in context.Op.AcceptText(value: address.Name)
                from rows in context.Op.Catch(() =>
                    from found in Optional(context.Document.Lights.FindName(name)).ToFin(Fail: context.Op.MissingContext())
                    from row in Row(
                        document: context.Document,
                        index: context.Document.Lights.Find(found.Id, ignoreDeleted: true),
                        key: context.Op)
                    select row)
                select rows);

    internal static Fin<(ResourceIndex Index, LightObject Native)> Indexed(
        RhinoDoc document,
        int index,
        Func<LightObject, bool> state,
        Error failure) =>
        from slot in ResourceIndex.Maybe(value: index).ToFin(Fail: failure)
        from native in Optional(document.Lights.FindIndex(slot.Value)).ToFin(Fail: failure)
        from _ in guard(state(native), failure).ToFin()
        select (slot, native);

    private static Fin<Seq<(ResourceIndex Index, LightObject Native)>> Row(RhinoDoc document, int index, Op key) =>
        Indexed(
            document: document,
            index: index,
            state: static native => !native.IsDeleted,
            failure: key.MissingContext())
            .Map(static row => Seq(row));
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LightOp {
    private LightOp() { }
    public sealed record Mint(LightSeed Seed, Option<string> Name = default) : LightOp;
    public sealed record Amend(LightSelect Select, Seq<LightEdit> Edits) : LightOp;
    public sealed record Purge(LightSelect Select, HostInteraction Interaction) : LightOp;
    public sealed record Revive(int Index) : LightOp;

    internal Fin<LightOp> Admit(Op op) =>
        Switch(
            context: op,
            mint: static (key, work) =>
                from seed in key.Need(work.Seed).Bind(value => value.Admit(op: key))
                from name in work.Name.Traverse(value => key.AcceptText(value: value)).As()
                select (LightOp)new Mint(Seed: seed, Name: name),
            amend: static (key, work) =>
                from address in key.Need(work.Select)
                from _ in guard(!work.Edits.IsEmpty, key.InvalidInput()).ToFin()
                from edits in work.Edits.TraverseM(edit => key.Need(edit)
                    .Bind(value => value.Admit(op: key))).As()
                select (LightOp)new Amend(Select: address, Edits: edits),
            purge: static (key, work) =>
                from address in key.Need(work.Select)
                from _ in key.Need(work.Interaction)
                select (LightOp)work,
            revive: static (key, work) => ResourceIndex.Admit(value: work.Index, key: key).Map(_ => (LightOp)work));

    internal Fin<Unit> Apply(RhinoDoc document, Op op) =>
        Switch(
            context: (Document: document, Op: op),
            mint: static (context, work) => work.Seed.Mint(op: context.Op).Bind(lease => lease.Use(
                fresh => context.Op.Catch(() => {
                    _ = work.Name.Iter(name => fresh.Name = name);
                    return ResourceIndex.Admit(value: context.Document.Lights.Add(fresh), key: context.Op)
                        .Map(static _ => unit);
                }),
                context.Op)),
            amend: static (context, work) =>
                from rows in work.Select.Resolve(document: context.Document, key: context.Op)
                from _ in rows.TraverseM(row => context.Op.Catch(() =>
                    from working in Optional(row.Native.DuplicateLightGeometry()).ToFin(Fail: context.Op.InvalidResult())
                    from applied in context.Op.Catch(() => {
                        using Light live = working;
                        return from kind in LightKind.Of(style: live.LightStyle, key: context.Op)
                               from __ in work.Edits.TraverseM(edit => edit.Apply(
                                   working: live, kind: kind, op: context.Op)).As()
                               from ___ in context.Op.Confirm(
                                   success: context.Document.Lights.Modify(row.Index.Value, live))
                               select unit;
                    })
                    select applied)).As()
                select unit,
            purge: static (context, work) =>
                from rows in work.Select.Resolve(document: context.Document, key: context.Op)
                from _ in rows.TraverseM(row => context.Op.Confirm(
                    success: context.Document.Lights.Delete(row.Index.Value, work.Interaction.IsQuiet))).As()
                select unit,
            revive: static (context, work) =>
                from row in LightSelect.Indexed(
                    document: context.Document,
                    index: work.Index,
                    state: static native => native.IsDeleted,
                    failure: context.Op.InvalidInput())
                from _ in context.Op.Confirm(success: context.Document.Lights.Undelete(row.Index.Value))
                select unit);
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record LightRoster(Seq<LightStamp> Rows) : IDetachedDocumentResult;

public readonly record struct SceneSpectrum(double R, double G, double B) {
    internal static Fin<SceneSpectrum> Of(PerceptualColor colour, Op key) =>
        colour.ToRgb(profile: RgbProfile.Srgb, transfer: RgbTransfer.Linear) switch {
            var (red, green, blue, alpha) when alpha >= 1.0 =>
                Fin.Succ(value: new SceneSpectrum(R: red, G: green, B: blue)),
            _ => Fin.Fail<SceneSpectrum>(error: key.InvalidInput(axis: nameof(PerceptualColor.Alpha))),
        };
}

public readonly record struct PhotometricPower(
    bool RadiantFluxIsAuthority, double Watts, double Lumens, double Candela, double Scale) {
    internal static PhotometricPower Of(LightStamp stamp) =>
        new(RadiantFluxIsAuthority: double.IsFinite(stamp.Watts) && stamp.Watts > 0d,
            Watts: stamp.Watts,
            Lumens: stamp.Lumens,
            Candela: stamp.Candela,
            Scale: stamp.Intensity);
}

public readonly record struct PhotometricWebRef(ArtifactContent Artifact, Wire.WebDialect Dialect) {
    internal Fin<PhotometricWebRef> Admit(Op op) =>
        from artifact in op.Need(Artifact)
        from _ in guard(Dialect != Wire.WebDialect.Unspecified, op.InvalidInput()).ToFin()
        select this with { Artifact = artifact };
}

public readonly record struct SceneShading(
    ArtifactContent Artifact,
    ulong ElementCount,
    ulong TriangleCount,
    Geometry.TessellationPolicy Fidelity) {
    internal Fin<SceneShading> Admit(Op op) =>
        from artifact in op.Need(Artifact)
        from fidelity in op.Need(Fidelity)
        from _ in guard(
            fidelity.TriangleBudget > 0UL && TriangleCount <= fidelity.TriangleBudget
            && double.IsFinite(fidelity.DeflectionM) && fidelity.DeflectionM > 0d
            && double.IsFinite(fidelity.AngleToleranceRad) && fidelity.AngleToleranceRad > 0d,
            op.InvalidInput()).ToFin()
        select this with { Artifact = artifact, Fidelity = fidelity.Clone() };
}

// --- [SERVICES] ------------------------------------------------------------------------
public interface IPhotometricRegistry {
    Option<PhotometricWebRef> WebOf(Guid light);
}

// --- [BOUNDARIES] ----------------------------------------------------------------------
public sealed record ScenePhotometry(
    Guid Id,
    Option<string> Name,
    Wire.LightKind Kind,
    bool Enabled,
    Spatial.Point3 LocationMetres,
    Spatial.UnitDirection3 Direction,
    Spatial.UnitDirection3 Perpendicular,
    PhotometricPower Power,
    SceneSpectrum Diffuse,
    SceneSpectrum Ambient,
    SceneSpectrum Specular,
    UnitInterval ShadowFraction,
    ConeEvidence Cone,
    Option<AreaShape> ExtentMetres,
    LightAttenuation Attenuation,
    Option<PhotometricWebRef> Web) : IDetachedDocumentResult {
    internal static Fin<ScenePhotometry> Of(
        LightStamp stamp, double metresPerUnit, Option<PhotometricWebRef> web, Op key) =>
        from active in key.Need(stamp)
        from scale in key.Positive(metresPerUnit)
        from shadow in key.AcceptValidated<UnitInterval>(candidate: active.Shadow)
        from diffuse in SceneSpectrum.Of(colour: active.Diffuse, key: key)
        from ambient in SceneSpectrum.Of(colour: active.Ambient, key: key)
        from specular in SceneSpectrum.Of(colour: active.Specular, key: key)
        let power = PhotometricPower.Of(stamp: active)
        from _ in guard(
            double.IsFinite(power.Lumens) && power.Lumens >= 0d
            && double.IsFinite(power.Candela) && power.Candela >= 0d
            && (power.RadiantFluxIsAuthority
                ? double.IsFinite(power.Watts) && power.Watts > 0d
                : double.IsFinite(power.Scale) && power.Scale >= 0d),
            key.InvalidInput()).ToFin()
        from direction in SceneMap.Direction(value: active.Direction, key: key)
        from perpendicular in SceneMap.Direction(value: active.PerpendicularDirection, key: key)
        from extent in active.Area.Traverse(area => area.Scaled(scale: scale, op: key)).As()
        from reference in web.Traverse(value => value.Admit(op: key)).As()
        select new ScenePhotometry(
            Id: active.Id,
            Name: active.Name,
            Kind: active.Kind.Wire,
            Enabled: active.Enabled,
            LocationMetres: SceneMap.Point(value: active.Location, scale: scale),
            Direction: direction,
            Perpendicular: perpendicular,
            Power: power,
            Diffuse: diffuse,
            Ambient: ambient,
            Specular: specular,
            ShadowFraction: shadow,
            Cone: active.Cone,
            ExtentMetres: extent,
            Attenuation: active.Attenuation,
            Web: reference);
}

public sealed record SceneCapture(
    UInt128 Key,
    string SourceUnit,
    SceneSun Sun,
    Seq<ScenePhotometry> Lights,
    SceneShading Shading,
    Instant CapturedAt) : IDetachedDocumentResult {
    internal static Fin<SceneCapture> Of(
        SceneSun sun, Seq<ScenePhotometry> lights, SceneShading shading, ModelUnit unit, Instant moment, Op op) =>
        from band in op.Need(sun)
        from _ in guard(
            double.IsFinite(band.IntensityScale) && band.IntensityScale >= 0d,
            op.InvalidInput()).ToFin()
        from artifact in op.Need(shading).Bind(value => value.Admit(op: op))
        from regime in op.Need(unit)
        let source = regime.Name.IfNone(() => regime.System.ToString())
        let unstamped = new SceneCapture(
            Key: 0, SourceUnit: source, Sun: band, Lights: lights, Shading: artifact, CapturedAt: moment)
        from key in SceneMap.ContentKey(capture: unstamped, key: op)
        select unstamped with { Key = key };
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Lights {
    public static Fin<LightRoster> Ask(DocumentSession session, LightSelect scope, Op? key = null) {
        Op op = key.OrDefault();
        return from address in op.Need(scope)
               from roster in session.Demand(
                   use: document =>
                       from model in Rasm.Domain.Context.Of(doc: document).ToFin()
                       from rows in address.Resolve(document: document, key: op)
                       from stamps in rows.TraverseM(row => LightStamp.Of(
                           index: row.Index, native: row.Native, model: model, key: op)).As()
                       select new LightRoster(Rows: stamps),
                   key: op,
                   needs: [SessionNeed.Read])
               select roster;
    }

    public static Fin<Unit> Commit(
        DocumentSession session, RedrawPolicy redraw, params ReadOnlySpan<LightOp> operations) {
        Op op = Op.Of();
        return from policy in op.Need(redraw)
               from requested in LanguageExt.Iterable<LightOp>.FromSpan(operations).ToSeq()
                   .TraverseM(work => op.Need(work)).As()
               from _ in guard(!requested.IsEmpty, op.InvalidInput()).ToFin()
               from plan in requested.TraverseM(work => work.Admit(op: op)).As()
               from _ in ObjectSpine.Commit(
                   session: session,
                   name: nameof(Lights),
                   redraw: policy,
                   run: (document, key) => plan
                       .TraverseM(work => work.Apply(document: document, op: key)).As()
                       .Map(static _ => unit),
                   op: op)
               select unit;
    }

    public static Fin<(SceneCapture Capture, ReadOnlyMemory<byte> Bytes)> Capture(
        DocumentSession session,
        SceneSun sun,
        SceneShading shading,
        IPhotometricRegistry webs,
        Instant moment,
        Op? key = null) {
        Op op = key.OrDefault();
        return from band in op.Need(sun)
               from artifact in op.Need(shading)
               from registry in op.Need(webs)
               from capture in session.Demand(
                   use: document =>
                       from model in Rasm.Domain.Context.Of(doc: document).ToFin()
                       from rows in new LightSelect.Every().Resolve(document: document, key: op)
                       from stamps in rows.TraverseM(row => LightStamp.Of(
                           index: row.Index, native: row.Native, model: model, key: op)).As()
                       from photometry in stamps.TraverseM(stamp => ScenePhotometry.Of(
                           stamp: stamp,
                           metresPerUnit: model.Unit.MetersPerUnit,
                           web: registry.WebOf(light: stamp.Id),
                           key: op)).As()
                       from sealed_ in SceneCapture.Of(
                           sun: band, lights: photometry, shading: artifact,
                           unit: model.Unit, moment: moment, op: op)
                       select sealed_,
                   key: op,
                   needs: [SessionNeed.Read])
               from bytes in SceneMap.Encode(capture: capture, key: op)
               select (capture, bytes);
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public static class SceneMap {
    internal static Fin<UInt128> ContentKey(SceneCapture capture, Op key) =>
        key.Catch(() => Fin.Succ(ContentHash.Of(Descriptor(capture: capture, includeKey: false).ToByteArray())));

    public static Fin<ReadOnlyMemory<byte>> Encode(SceneCapture capture, Op? key = null) {
        Op op = key.OrDefault();
        return from admitted in op.Need(capture)
               from bytes in op.Catch(() => Fin.Succ(
                   value: (ReadOnlyMemory<byte>)Descriptor(capture: admitted, includeKey: true).ToByteArray()))
               select bytes;
    }

    private static Wire.SceneDescriptor Descriptor(SceneCapture capture, bool includeKey) {
        Wire.SceneDescriptor result = new() {
            SourceUnit = capture.SourceUnit,
            Sun = Sun(band: capture.Sun),
            Shading = Shading(value: capture.Shading),
            CapturedAt = capture.CapturedAt.ToTimestamp(),
        };
        if (includeKey) result.Key = ContentHash.Wire(digest: capture.Key);
        result.Lights.Add(capture.Lights.Map(Photometry));
        return result;
    }

    private static Wire.Photometry Photometry(ScenePhotometry row) {
        Wire.Photometry result = new() {
            Id = Identity(value: row.Id),
            Kind = row.Kind,
            Enabled = row.Enabled,
            Location = row.LocationMetres,
            Direction = row.Direction,
            Perpendicular = row.Perpendicular,
            Power = Power(value: row.Power),
            Diffuse = Spectrum(value: row.Diffuse),
            Ambient = Spectrum(value: row.Ambient),
            Specular = Spectrum(value: row.Specular),
            ShadowFraction = (double)row.ShadowFraction,
        };
        _ = row.Name.Iter(name => result.Name = name);
        SeatCone(target: result, evidence: row.Cone);
        _ = row.ExtentMetres.Iter(area => SeatExtent(target: result, area: area));
        SeatAttenuation(target: result, value: row.Attenuation);
        _ = row.Web.Iter(value => result.Web = Web(value: value));
        return result;
    }

    private static Wire.Spectrum Spectrum(SceneSpectrum value) => new() { R = value.R, G = value.G, B = value.B };

    private static Wire.Power Power(PhotometricPower value) => value.RadiantFluxIsAuthority
        ? new Wire.Power {
            RadiantFluxW = value.Watts,
            Lumens = value.Lumens,
            Candela = value.Candela,
        }
        : new Wire.Power {
            RelativeScale = value.Scale,
            Lumens = value.Lumens,
            Candela = value.Candela,
        };

    private static Wire.WebRef Web(PhotometricWebRef value) => new() {
        Dialect = value.Dialect,
        Artifact = ArtifactRef(value: value.Artifact),
    };

    private static Wire.Shading Shading(SceneShading value) => new() {
        ElementCount = value.ElementCount,
        TriangleCount = value.TriangleCount,
        Fidelity = value.Fidelity,
        Artifact = ArtifactRef(value: value.Artifact),
    };

    private static Artifact.ArtifactRef ArtifactRef(ArtifactContent value) => new() {
        Sha256 = ByteString.CopyFrom(Convert.FromHexString(value.Sha256)),
        ArtifactBytes = value.Bytes,
    };

    internal static Spatial.Point3 Point(Point3d value, double scale = 1.0) => new() {
        XM = value.X * scale,
        YM = value.Y * scale,
        ZM = value.Z * scale,
    };

    internal static Spatial.Displacement3 Displacement(Vector3d value) => new() {
        XM = value.X,
        YM = value.Y,
        ZM = value.Z,
    };

    internal static Fin<Spatial.UnitDirection3> Direction(Vector3d value, Op key) {
        Vector3d admitted = value;
        return admitted.Unitize()
            ? Fin.Succ(new Spatial.UnitDirection3 { X = admitted.X, Y = admitted.Y, Z = admitted.Z })
            : Fin.Fail<Spatial.UnitDirection3>(key.InvalidInput(axis: "scene-direction"));
    }

    internal static Wire.AttenuationCoefficients Coefficients(Vector3d value) => new() {
        Constant = value.X,
        Linear = value.Y,
        Quadratic = value.Z,
    };

    private static void SeatCone(Wire.Photometry target, ConeEvidence evidence) => evidence.Switch(
        state: target,
        absent: static (_, _) => unit,
        shaped: static (wire, row) => {
            wire.Shaped = new Wire.Cone {
                HalfAngleRad = row.Value.Cone.HalfAngle.Value,
                HotSpot = (double)row.Value.HotSpot,
            };
            return unit;
        },
        degenerate: static (wire, _) => {
            wire.Degenerate = new Google.Protobuf.WellKnownTypes.Empty();
            return unit;
        });

    private static Unit SeatExtent(Wire.Photometry target, AreaShape area) => area.Width.Match(
        Some: width => {
            target.Rectangular = new Wire.Extent {
                Length = Displacement(value: area.Length),
                Width = Displacement(value: width),
            };
            return unit;
        },
        None: () => {
            target.Linear = Displacement(value: area.Length);
            return unit;
        });

    private static void SeatAttenuation(Wire.Photometry target, LightAttenuation value) => value.Switch(
        context: target,
        named: static (wire, row) => {
            wire.Falloff = row.Row.Wire;
            return unit;
        },
        free: static (wire, row) => {
            wire.Coefficients = Coefficients(value: row.Coefficients);
            return unit;
        });

    private static Wire.SceneSun Sun(SceneSun band) => new() {
        Enabled = band.Enabled,
        IntensityScale = band.IntensityScale,
        Sited = band.Derivation is SunDerivation.Sited sited
            ? new Wire.SitedSun { Frame = Frame(frame: sited.Frame), Angles = Angles(angles: sited.Angles) }
            : null,
        Authored = band.Derivation is SunDerivation.Authored authored ? Angles(angles: authored.Angles) : null,
    };

    private static Wire.SolarFrame Frame(SolarFrame frame) => new() {
        LatitudeDeg = frame.Site.LatitudeDeg,
        LongitudeDeg = frame.Site.LongitudeDeg,
        TimeZoneHours = frame.Site.OffsetHours,
        ElevationM = frame.Site.ElevationM,
        NorthAxisDeg = frame.NorthAxisDegrees,
        DaylightSavingMinutes = frame.DaylightSavingMinutes,
        Moment = frame.Moment.ToTimestamp(),
    };

    private static Wire.SolarAngles Angles(SunPosition angles) => new() {
        AltitudeDeg = angles.AltitudeDeg,
        AzimuthDeg = angles.AzimuthDeg,
    };

    private static ByteString Identity(Guid value) {
        Span<byte> bytes = stackalloc byte[KeyWidth];
        _ = value.TryWriteBytes(destination: bytes, bigEndian: true, bytesWritten: out _);
        return ByteString.CopyFrom(bytes: bytes);
    }

    private const int KeyWidth = 16;
}

```

## [05]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]           | [OWNER]                | [FORM]                                                      | [ENTRY]                 |
| :-----: | :------------------ | :--------------------- | :---------------------------------------------------------- | :---------------------- |
|  [01]   | kind capability     | `LightKind`            | world-style rows with one grants set and a wire key         | `LightKind.Of`          |
|  [02]   | modality vocabulary | `LightModality`        | four capability rows every gate reads as one set            | `Grants.Require`        |
|  [03]   | detached read       | `LightStamp`           | whole light state with kernel cone and perceptual colour    | `Lights.Ask`            |
|  [04]   | attenuation         | `LightFalloff`         | host roster with coefficient vector and wire key            | `LightAttenuation.Of`   |
|  [05]   | construction        | `LightSeed`            | one polymorphic mint, style written once through a lease    | `LightOp.Mint`          |
|  [06]   | photometric power   | `RadianceUnit`         | delegate rows over one admitted magnitude                   | `LightEdit.Power`       |
|  [07]   | property edits      | `LightEdit`            | required-capability gate over one working duplicate         | `LightOp.Amend`         |
|  [08]   | table address       | `LightSelect`          | every, index, id, and name onto `ResourceIndex`-paired rows | `Lights.Ask` / `Commit` |
|  [09]   | host dialogue       | spine owner            | `HostInteraction` composed, never a signal re-spelling      | `LightOp.Purge`         |
|  [10]   | photometric rank    | `PhotometricPower`     | four host readings under one declared authority             | `PhotometricPower.Of`   |
|  [11]   | descriptor light    | `ScenePhotometry`      | metres-scaled wire row with unit-free cone                  | `ScenePhotometry.Of`    |
|  [12]   | descriptor capture  | `SceneCapture`         | sun, photometry, and shading under one content key          | `Lights.Capture`        |
|  [13]   | web address         | `IPhotometricRegistry` | the one port into the render content stratum                | `WebOf`                 |
|  [14]   | wire transcription  | `SceneMap`             | one mapper with byte-order and vocabulary columns           | `SceneMap.Encode`       |

## [06]-[RESEARCH]

(none)
