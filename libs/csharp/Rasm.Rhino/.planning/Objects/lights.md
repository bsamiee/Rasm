# [RASM_RHINO_OBJECTS_LIGHTS]

Light objects belong to `Rasm.Rhino.Objects`. `LightKind` closes the world light family — point, spot, directional, linear, rectangular — as capability rows; `LightSeed` is the one polymorphic construction union, `LightEdit` the one property-edit union whose modalities gate on the kind columns, and `LightSelect` the table address vocabulary. `Lights.Ask` reads detached `LightStamp` rows and `Lights.Commit` mints, amends, purges, and revives through `LightTable` on the shared `ObjectSpine`, returning `ObjectReceipt<LightFact>`. Spot cones compose kernel `VectorCone`, colors compose `PerceptualColor`, and photometric power is one `Radiance` union; `TableKind.Lights` stays identity vocabulary — this page owns the commit rail it cannot express.

## [01]-[INDEX]

- [02]-[KIND_AND_STAMP]: `LightKind`, `SpotShape`, `ConeEvidence`, `AreaShape`, `LightFrame`, `Falloff`, `LightStamp` — the capability rows and the detached read.
- [03]-[SEED_AND_EDIT]: `LightSeed`, `Radiance`, `LightShade`, `LightEdit` — construction and the gated property edits, beside the `ScenePhotometry` wire band.
- [04]-[ASK_AND_COMMIT]: `LightSelect`, `LightOp`, `LightFact`, and the `Lights` entry pair.
- [05]-[SURFACE_LEDGER]: the page's owner table.

## [02]-[KIND_AND_STAMP]

- Owner: `LightKind` `[SmartEnum<int>]` keys every world style by its host `LightStyle` ordinal and carries the capability columns — `Aims`, `Cone`, `Extent`, `Breadth` — every edit gate reads; `SpotShape` couples the kernel cone with its hot-spot fraction; `AreaShape` carries the linear and rectangular extent vectors; `Falloff` closes the attenuation model; `LightStamp` is the whole detached light read, including its perpendicular direction and coordinate system.
- Law: the family is closed at the world styles — `LightKind.Of` resolves through one frozen style index, so a camera-space style, `Ambient`, and the sun style refuse at the gate instead of leaking a sixth modality; the sun's derived `Light` is `SunEvidence` custody on the render settings page and never enters this rail.
- Law: cone math is kernel-owned — the spot cone crosses as `VectorCone` (apex, unit axis, admitted half-angle), half-angle and solid-angle questions answer through `ConeProjection` rows on the stamped cone, and inline spot trigonometry beside the owner is the deleted form.
- Law: color is perceptual at the seam — `Diffuse`, `Ambient`, and `Specular` admit into `PerceptualColor` on read and quantize once through `ToRgb()` on write, so no sRGB component math ever rides this page.
- Law: the stamp is host evidence — intensity, watt, lumen, candela, shadow, spot-angle, and hot-spot values cross raw as read, and every WRITE payload admits (`UnitInterval` fractions, finite positive radiance, kernel cone), so reads never refuse a degenerate document and writes never pass one. `AreaShape` remains raw read evidence and admits both extent vectors at every write boundary.
- Law: cone absence and cone refusal are DIFFERENT answers — `ConeEvidence` carries `Absent` for a kind with no cone, `Shaped` for an admitted one, and `Degenerate` where the kind carries a cone the document's tolerance cannot admit; collapsing the last two onto one `None` erases the only signal a consumer has that the document, not the light, is the problem, while the raw scalars keep the underlying evidence in every case.
- Law: the coordinate frame and the attenuation model are bounded reads — `LightFrame` closes the host coordinate roster and `Falloff.Of` REFUSES an unrecognized attenuation row rather than reading its coefficient vector; `Falloff.Coefficients` survives as the write payload a caller seats through `SetAttenuation`, and the host's own read classifies that seated light back onto one of its three named rows.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Frozen;
using System.Linq;
using Rasm.Domain;
using Rasm.Numerics;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;

namespace Rasm.Rhino.Objects;

// `Rasm.Numerics` is in scope for `PerceptualContext`, `UnitInterval`, and `VectorCone`, so every host colour on
// this page spells `System.Drawing.Color` in full and no bare `Color` resolves against two candidates.

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class LightKind {
    public static readonly LightKind Directional = new(key: (int)LightStyle.WorldDirectional, style: LightStyle.WorldDirectional, aims: true, cone: false, extent: false, breadth: false);
    public static readonly LightKind Point = new(key: (int)LightStyle.WorldPoint, style: LightStyle.WorldPoint, aims: false, cone: false, extent: false, breadth: false);
    public static readonly LightKind Spot = new(key: (int)LightStyle.WorldSpot, style: LightStyle.WorldSpot, aims: true, cone: true, extent: false, breadth: false);
    public static readonly LightKind Linear = new(key: (int)LightStyle.WorldLinear, style: LightStyle.WorldLinear, aims: false, cone: false, extent: true, breadth: false);
    public static readonly LightKind Rectangular = new(key: (int)LightStyle.WorldRectangular, style: LightStyle.WorldRectangular, aims: false, cone: false, extent: true, breadth: true);

    internal LightStyle Style { get; }
    internal bool Aims { get; }
    internal bool Cone { get; }
    internal bool Extent { get; }
    internal bool Breadth { get; }

    private static readonly Lazy<FrozenDictionary<LightStyle, LightKind>> ByStyle =
        new(static () => Items.ToFrozenDictionary(static row => row.Style));

    internal static Fin<LightKind> Of(LightStyle style, Op key) =>
        ByStyle.Value.TryGetValue(style, out LightKind? row) ? Fin.Succ(value: row) : Fin.Fail<LightKind>(error: key.InvalidInput());
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Falloff {
    private Falloff() { }
    public sealed record Constant : Falloff;
    public sealed record Linear : Falloff;
    public sealed record InverseSquared : Falloff;
    public sealed record Coefficients(Vector3d Value) : Falloff;

    // An unrecognized host attenuation row is a REFUSAL, not a coefficient vector: reading the vector for an
    // unknown model publishes a law nobody wrote, and the two sibling host-value folds already fail on theirs.
    internal static Fin<Falloff> Of(Light native, Op key) =>
        native.AttenuationType switch {
            Light.Attenuation.Constant => Fin.Succ<Falloff>(value: new Constant()),
            Light.Attenuation.Linear => Fin.Succ<Falloff>(value: new Linear()),
            Light.Attenuation.InverseSquared => Fin.Succ<Falloff>(value: new InverseSquared()),
            var unknown => Fin.Fail<Falloff>(error: key.InvalidResult(detail: unknown.ToString())),
        };

    internal Fin<Falloff> Admit(Op op) =>
        Switch(
            context: op,
            constant: static (_, law) => Fin.Succ<Falloff>(law),
            linear: static (_, law) => Fin.Succ<Falloff>(law),
            inverseSquared: static (_, law) => Fin.Succ<Falloff>(law),
            coefficients: static (key, law) => key.AcceptInput(value: law.Value).Map(_ => (Falloff)law));

    internal Unit Apply(Light working) =>
        Switch(
            context: working,
            constant: static (light, _) => Seat(light, Light.ConstantAttenuationVector),
            linear: static (light, _) => Seat(light, Light.LinearAttenuationVector),
            inverseSquared: static (light, _) => Seat(light, Light.InverseSquaredAttenuationVector),
            coefficients: static (light, law) => Seat(light, law.Value));

    private static Unit Seat(Light light, Vector3d coefficients) {
        light.SetAttenuation(coefficients.X, coefficients.Y, coefficients.Z);
        return unit;
    }
}

// --- [MODELS] -----------------------------------------------------------------------------
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

// A spot light's cone answers on three distinguishable states, never one collapsed absence: the kind carries no
// cone, the kind carries one and the document admits it, or the kind carries one the document degenerates. The
// raw `SpotAngle`/`HotSpot` scalars survive on the stamp either way, so a consumer separates "no cone" from
// "cone the model cannot admit" without re-reading the host.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ConeEvidence {
    private ConeEvidence() { }
    public sealed record Absent : ConeEvidence;
    public sealed record Shaped(SpotShape Value) : ConeEvidence;
    public sealed record Degenerate : ConeEvidence;
}

[SmartEnum<int>]
public sealed partial class LightFrame {
    public static readonly LightFrame World = new(key: (int)CoordinateSystem.World);
    public static readonly LightFrame Camera = new(key: (int)CoordinateSystem.Camera);
    public static readonly LightFrame Clip = new(key: (int)CoordinateSystem.Clip);
    public static readonly LightFrame Screen = new(key: (int)CoordinateSystem.Screen);

    internal CoordinateSystem Host => (CoordinateSystem)Key;

    internal static Fin<LightFrame> Of(CoordinateSystem system, Op key) =>
        key.Row<int, LightFrame>((int)system);
}

public readonly record struct AreaShape(Vector3d Length, Option<Vector3d> Width = default) {
    internal Fin<AreaShape> Admit(Op op) =>
        from length in op.AcceptInput(value: Length)
        from width in Width.Traverse(value => op.AcceptInput(value: value)).As()
        select new AreaShape(Length: length, Width: width);
}

public sealed record LightStamp(
    Guid Id,
    int Index,
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
    Falloff Attenuation) : IDetachedDocumentResult {
    internal static Fin<LightStamp> Of(int index, LightObject native, Context model, Op key) =>
        key.Catch(() =>
            from light in Optional(native.LightGeometry).ToFin(Fail: key.InvalidResult())
            from kind in LightKind.Of(style: light.LightStyle, key: key)
            from frame in LightFrame.Of(system: light.CoordinateSystem, key: key)
            from attenuation in Falloff.Of(native: light, key: key)
            from diffuse in Shade(color: light.Diffuse, key: key)
            from ambient in Shade(color: light.Ambient, key: key)
            from specular in Shade(color: light.Specular, key: key)
            from cone in kind.Cone
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
                Area: kind.Extent
                    ? Some(new AreaShape(Length: light.Length, Width: kind.Breadth ? Some(light.Width) : Option<Vector3d>.None))
                    : Option<AreaShape>.None,
                Attenuation: attenuation));

    private static Fin<PerceptualColor> Shade(System.Drawing.Color color, Op key) =>
        PerceptualColor.OfRgb(color.R, color.G, color.B, alpha: color.A, key: key);

    internal static System.Drawing.Color Rgb(PerceptualColor shade) =>
        shade.ToRgb() switch {
            var (red, green, blue, alpha) => System.Drawing.Color.FromArgb(alpha: alpha, red: red, green: green, blue: blue),
        };
}
```

## [03]-[SEED_AND_EDIT]

- Owner: `LightSeed` `[Union]` closes construction — every case mints one admitted `Light` with its style set once; `Radiance` `[Union]` closes the photometric power axis — intensity scale, watts, lumens, candela — as one value; `LightShade` couples the diffuse write with its optional ambient and specular companions; `LightEdit` `[Union]` closes rename, enablement, power, shade, shadow, cone, area, pose, and attenuation over one gated dispatch.
- Law: modality gates are kind columns — the cone edit demands `Cone`, extent demands `Extent`, breadth demands `Breadth`, a directional pose demands `Aims`, and each refusal is the typed `InvalidInput` fault; a per-arm style ladder re-deriving what the row already states is the deleted form.
- Law: seeds admit before the host — locations, vectors, and complete area shapes pass admission before the document grant, the spot seed consumes an already-admitted `SpotShape`, and the style writes exactly once at mint, so no half-styled light ever reaches the table.
- Law: the mint owns its native — `LightSeed.Mint` answers `Fin<Lease<Light>>` and brackets the seat, so a refused or throwing seat releases the fresh `Light` before the fault leaves and the table commit consumes the lease through `Use`; a raw returned handle relies on the caller reaching its own `using` first, which a throwing seat forecloses.
- Law: photometric power is one axis — `Radiance` discriminates scale from watts, lumens, and candela by case, the host converts between them, and a per-unit sibling edit family is the collapsed form.
- Owner: `PhotometricAuthority`, `PhotometricPower`, `PhotometricWebRef`, and `ScenePhotometry` are the daylighting descriptor's light band, projected out of `LightStamp` and never read back in.
- Law: authority is a producer RULE over four host readings, never a host flag — `Light` stores one power and publishes watts, lumens, and candela as converted views with no field naming which the modeller set, so `radiant-flux-w` claims the row exactly when watts reads finite and positive and `relative-scale` claims it otherwise.
- Law: `relative-scale` is dimensionless and refuses every engine photometric slot by name — a bare `Intensity` seated as a lighting level publishes watts nobody authored.
- Law: the descriptor emits METRES — `LightStamp` poses and extents read in model units, so `ScenePhotometry.Of` takes the caller's `metresPerUnit` factor and three peers never fork one conversion.
- Law: write admissions bind the wire too — `shadow_fraction` crosses as an admitted `UnitInterval` even though the read path takes `ShadowIntensity` raw, so a degenerate document refuses at the producer rather than seating an out-of-range fraction on a consuming engine.
- Boundary: the photometric-web (IES) payload is the render kinds page's `PhotometricWeb` — dialect-admitted by `PhotometricDialect`, minted through `PhotometricPress`, and landed on the light's attached render material child slot by `AttachTo`; this rail's photometric reach ends at `Radiance`, and `LightEdit` never grows an IES case.
- Boundary: `PhotometricWebRef` carries the web by content key and dialect KEY STRING — the descriptor never decodes a distribution, and carrying the dialect as its own row binds this stratum to the render page it sits above.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Radiance {
    private Radiance() { }
    public sealed record Scale(double Value) : Radiance;
    public sealed record Watts(double Value) : Radiance;
    public sealed record Lumens(double Value) : Radiance;
    public sealed record Candela(double Value) : Radiance;

    internal Fin<Radiance> Admit(Op op) =>
        Switch(
            context: op,
            scale: static (key, power) => key.Positive(power.Value).Map(_ => (Radiance)power),
            watts: static (key, power) => key.Positive(power.Value).Map(_ => (Radiance)power),
            lumens: static (key, power) => key.Positive(power.Value).Map(_ => (Radiance)power),
            candela: static (key, power) => key.Positive(power.Value).Map(_ => (Radiance)power));

    internal Unit Apply(Light working) =>
        Switch(
            context: working,
            scale: static (light, power) => { light.Intensity = power.Value; return unit; },
            watts: static (light, power) => { light.PowerWatts = power.Value; return unit; },
            lumens: static (light, power) => { light.PowerLumens = power.Value; return unit; },
            candela: static (light, power) => { light.PowerCandela = power.Value; return unit; });
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LightSeed {
    private LightSeed() { }
    public sealed record Point(Point3d Location) : LightSeed;
    public sealed record Spot(SpotShape Shape) : LightSeed;
    public sealed record Directional(Point3d Location, Vector3d Direction) : LightSeed;
    public sealed record Linear(Point3d Location, Vector3d Length) : LightSeed;
    public sealed record Rectangular(Point3d Corner, Vector3d Length, Vector3d Width) : LightSeed;

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

    internal Fin<Lease<Light>> Mint(Op op) =>
        Switch(
            context: op,
            point: static (key, seed) => Styled(kind: LightKind.Point, key: key, seat: light => { light.Location = seed.Location; }),
            spot: static (key, seed) => Styled(kind: LightKind.Spot, key: key, seat: light => {
                light.Location = seed.Shape.Cone.Apex;
                light.Direction = seed.Shape.Cone.Axis.Value;
                light.SpotAngleRadians = seed.Shape.Cone.HalfAngle.Value;
                light.HotSpot = (double)seed.Shape.HotSpot;
            }),
            directional: static (key, seed) => Styled(kind: LightKind.Directional, key: key, seat: light => {
                light.Location = seed.Location;
                light.Direction = seed.Direction;
            }),
            linear: static (key, seed) => Styled(kind: LightKind.Linear, key: key, seat: light => {
                light.Location = seed.Location;
                light.Length = seed.Length;
            }),
            rectangular: static (key, seed) => Styled(kind: LightKind.Rectangular, key: key, seat: light => {
                light.Location = seed.Corner;
                light.Length = seed.Length;
                light.Width = seed.Width;
            }));

    // The mint owns the native from its first statement: a throwing seat releases it before the fault leaves,
    // so no path can strand a styled `Light` between construction and the caller's lease.
    private static Fin<Lease<Light>> Styled(LightKind kind, Op key, Action<Light> seat) =>
        key.Catch(() => Fin.Succ(value: new Light { LightStyle = kind.Style, IsEnabled = true }))
            .Bind(fresh => key.Catch(() => {
                    seat(fresh);
                    return Fin.Succ<Lease<Light>>(value: new Lease<Light>.Owned(Value: fresh));
                })
                .MapFail(error => {
                    fresh.Dispose();
                    return error;
                }));
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
    public sealed record Pose(Option<Point3d> Location = default, Option<Vector3d> Direction = default) : LightEdit;
    public sealed record Attenuate(Falloff Value) : LightEdit;

    internal Fin<LightEdit> Admit(Op op) =>
        Switch(
            context: op,
            rename: static (key, edit) => key.AcceptText(value: edit.Name).Map(name => (LightEdit)new Rename(Name: name)),
            toggle: static (key, edit) => key.Need(edit.Signal).Map(_ => (LightEdit)edit),
            power: static (key, edit) => key.Need(edit.Value)
                .Bind(power => power.Admit(op: key)).Map(power => (LightEdit)new Power(Value: power)),
            shade: static (key, edit) => key.Need(edit.Value)
                .Bind(shade => shade.Admit(op: key)).Map(shade => (LightEdit)new Shade(Value: shade)),
            shadow: static (key, edit) => key.AcceptValidated<UnitInterval>(candidate: (double)edit.Value)
                .Map(value => (LightEdit)new Shadow(Value: value)),
            cone: static (key, edit) => edit.Value.Admit(op: key)
                .Map(shape => (LightEdit)new Cone(Value: shape)),
            area: static (key, edit) => edit.Value.Admit(op: key)
                .Map(area => (LightEdit)new Area(Value: area)),
            pose: static (key, edit) =>
                from location in edit.Location.Traverse(value => key.AcceptInput(value: value)).As()
                from direction in edit.Direction.Traverse(value => key.AcceptInput(value: value)).As()
                from _ in guard(location.IsSome || direction.IsSome, key.InvalidInput()).ToFin()
                select (LightEdit)new Pose(Location: location, Direction: direction),
            attenuate: static (key, edit) => key.Need(edit.Value)
                .Bind(law => law.Admit(op: key)).Map(law => (LightEdit)new Attenuate(Value: law)));

    internal Fin<Unit> Apply(Light working, LightKind kind, Op op) =>
        Switch(
            context: (Working: working, Kind: kind, Op: op),
            rename: static (context, edit) => context.Op.Catch(() => context.Working.Name = edit.Name),
            toggle: static (context, edit) => context.Op.Catch(() => context.Working.IsEnabled = edit.Signal.On),
            power: static (context, edit) => context.Op.Catch(() => edit.Value.Apply(working: context.Working)),
            shade: static (context, edit) => context.Op.Catch(() => {
                context.Working.Diffuse = LightStamp.Rgb(shade: edit.Value.Diffuse);
                _ = edit.Value.Ambient.Iter(shade => context.Working.Ambient = LightStamp.Rgb(shade: shade));
                _ = edit.Value.Specular.Iter(shade => context.Working.Specular = LightStamp.Rgb(shade: shade));
            }),
            shadow: static (context, edit) => context.Op.Catch(() => context.Working.ShadowIntensity = (double)edit.Value),
            cone: static (context, edit) =>
                from _ in guard(context.Kind.Cone, context.Op.InvalidInput()).ToFin()
                from __ in context.Op.Catch(() => {
                    context.Working.Location = edit.Value.Cone.Apex;
                    context.Working.Direction = edit.Value.Cone.Axis.Value;
                    context.Working.SpotAngleRadians = edit.Value.Cone.HalfAngle.Value;
                    context.Working.HotSpot = (double)edit.Value.HotSpot;
                })
                select unit,
            area: static (context, edit) =>
                from _ in guard(context.Kind.Extent, context.Op.InvalidInput()).ToFin()
                from __ in guard(edit.Value.Width.IsNone || context.Kind.Breadth, context.Op.InvalidInput()).ToFin()
                from ___ in context.Op.Catch(() => {
                    context.Working.Length = edit.Value.Length;
                    _ = edit.Value.Width.Iter(width => context.Working.Width = width);
                })
                select unit,
            pose: static (context, edit) =>
                from _ in guard(edit.Direction.IsNone || context.Kind.Aims || context.Kind.Cone, context.Op.InvalidInput()).ToFin()
                from __ in context.Op.Catch(() => {
                    _ = edit.Location.Iter(location => context.Working.Location = location);
                    _ = edit.Direction.Iter(direction => context.Working.Direction = direction);
                })
                select unit,
            attenuate: static (context, edit) => context.Op.Catch(() => edit.Value.Apply(working: context.Working)));
}

[SmartEnum<string>]
public sealed partial class PhotometricAuthority {
    public static readonly PhotometricAuthority RadiantFluxWatts = new("radiant-flux-w");
    public static readonly PhotometricAuthority RelativeScale = new("relative-scale");
}

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record LightShade(
    PerceptualColor Diffuse,
    Option<PerceptualColor> Ambient = default,
    Option<PerceptualColor> Specular = default) {
    internal Fin<LightShade> Admit(Op op) =>
        from diffuse in op.Need(Diffuse)
        from ambient in Ambient.Traverse(value => op.Need(value)).As()
        from specular in Specular.Traverse(value => op.Need(value)).As()
        select new LightShade(Diffuse: diffuse, Ambient: ambient, Specular: specular);
}

// Four readings over ONE stored power beside the column that carries authority. `Light` exposes no field naming
// which quantity the modeller set, so picking one reading and dropping three loses the host evidence a consumer
// needs to explain a converted figure, while shipping four unranked floats hands every peer the same guess.
public readonly record struct PhotometricPower(
    PhotometricAuthority Authority, double Watts, double Lumens, double Candela, double Scale) {
    internal static PhotometricPower Of(LightStamp stamp) =>
        new(Authority: double.IsFinite(stamp.Watts) && stamp.Watts > 0d
                ? PhotometricAuthority.RadiantFluxWatts
                : PhotometricAuthority.RelativeScale,
            Watts: stamp.Watts,
            Lumens: stamp.Lumens,
            Candela: stamp.Candela,
            Scale: stamp.Intensity);
}

// Dialect crosses as its `PhotometricDialect.Key` text: the render page owns the roster and sits one stratum below
// this one, so carrying the row itself seats a downward type on a detached value every peer decodes by string.
public readonly record struct PhotometricWebRef(string ContentKey, string Dialect, long ByteLength);

// `ConeEvidence`'s kernel cone carries an apex in MODEL units that repeats the stamp's own location, so the wire
// keeps the three-state answer and drops the coordinate: half-angle and hot-spot are dimensionless, and the apex
// is already `LocationMetres`. Carrying both hands a consumer two positions to reconcile under one unit change.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SceneCone {
    private SceneCone() { }
    public sealed record Absent : SceneCone;
    public sealed record Shaped(double HalfAngleRadians, UnitInterval HotSpot) : SceneCone;
    public sealed record Degenerate : SceneCone;

    internal static SceneCone Of(ConeEvidence evidence) =>
        evidence.Switch(
            absent: static _ => (SceneCone)new Absent(),
            shaped: static law => new Shaped(
                HalfAngleRadians: law.Value.Cone.HalfAngle.Value, HotSpot: law.Value.HotSpot),
            degenerate: static _ => new Degenerate());
}

public sealed record ScenePhotometry(
    Guid Id,
    Option<string> Name,
    LightKind Kind,
    bool Enabled,
    Point3d LocationMetres,
    Vector3d Direction,
    Vector3d PerpendicularDirection,
    PhotometricPower Power,
    PerceptualColor Diffuse,
    PerceptualColor Ambient,
    PerceptualColor Specular,
    UnitInterval ShadowFraction,
    SceneCone Cone,
    Option<AreaShape> ExtentMetres,
    Falloff Attenuation,
    Option<PhotometricWebRef> Web) : IDetachedDocumentResult {
    internal static Fin<ScenePhotometry> Of(
        LightStamp stamp, double metresPerUnit, Op key, Option<PhotometricWebRef> web = default) =>
        from active in key.Need(stamp)
        from scale in key.Positive(metresPerUnit)
        from shadow in key.AcceptValidated<UnitInterval>(candidate: active.Shadow)
        from extent in active.Area.Traverse(area => Scaled(area: area, scale: scale, key: key)).As()
        select new ScenePhotometry(
            Id: active.Id,
            Name: active.Name,
            Kind: active.Kind,
            Enabled: active.Enabled,
            LocationMetres: Scaled(point: active.Location, scale: scale),
            Direction: active.Direction,
            PerpendicularDirection: active.PerpendicularDirection,
            Power: PhotometricPower.Of(stamp: active),
            Diffuse: active.Diffuse,
            Ambient: active.Ambient,
            Specular: active.Specular,
            ShadowFraction: shadow,
            Cone: SceneCone.Of(evidence: active.Cone),
            ExtentMetres: extent,
            Attenuation: active.Attenuation,
            Web: web);

    private static Fin<AreaShape> Scaled(AreaShape area, double scale, Op key) =>
        new AreaShape(
            Length: area.Length * scale,
            Width: area.Width.Map(width => width * scale)).Admit(op: key);

    private static Point3d Scaled(Point3d point, double scale) =>
        new(x: point.X * scale, y: point.Y * scale, z: point.Z * scale);
}
```

## [04]-[ASK_AND_COMMIT]

- Owner: `LightSelect` `[Union]` closes the table address — every row, an index, an object id, a name; `LightOp` `[Union]` closes the commit verbs — mint, amend, purge, revive; `LightFact` owns one typed consequence per verb; `ObjectReceipt<LightFact>` collects facts and undo serials; `Lights.Ask` and `Lights.Commit` are the two entries.
- Law: resolution is index-paired but id-addressed — the complete roster enumerates the table's own `IEnumerable<LightObject>` and resolves each live row's slot from its id, while `Find`, `FindName`, and the revival probe address a single row; one predicate-parameterized scalar projector proves bounds plus live/deleted state for selectors and revival. The slot survives only because `Modify`, `Delete`, and `Undelete` take one, and a deleted row has no live id, so the index address stays the revival ingress alone.
- Law: the working copy is the mutation site — an amend duplicates through `DuplicateLightGeometry`, applies its admitted edits to the duplicate, and lands once through `Modify(index, working)`, so the live table never observes a half-applied edit and the duplicate disposes on every path.
- Law: the purge's host-dialogue column is the spine's `HostInteraction`, not `ObjectSignal`. `ObjectSignal` names ENABLED and DISABLED, and the host argument it fed is `quiet` — so `ObjectSignal.Enabled` on a column named `Quiet` read as "enabled" at every call site and meant "suppress the warning", an inversion that is invisible at the call and wrong in exactly the direction that silences a dialogue a caller asked for. `HostInteraction.Quiet`/`Interactive` name the posture directly and `IsQuiet` is the only read.
- Law: the commit rides `ObjectSpine.Commit` — admission precedes the grant, the spine derives mutate-plus-undo needs with redraw joining by policy, and this page supplies only the per-verb fact fold; every light verb records undo.
- Law: placement stays the object rail — whole-object transform, delete-by-id, and selection of `LightObject`s ride `TableOp` through `TableTarget.Query` with `IncludeLights`; this rail owns what the object rail cannot spell — light-specific properties, index-addressed table verbs, and kind-gated modality.
- Boundary: `LightTable.Sun` and `Skylight` stay the render settings page's — `SunState`, `SkylightState`, and `SunEvidence` own that projection; `EventFamily.LightTable` already observes this table onto `EventPayload.Component(TableKind.Lights, …)`, and `SnapshotCategory.Lights` carries snapshot participation.
- Growth: a new light verb is one `LightOp` case with its fact; a new property axis is one `LightEdit` case gated by the kind columns.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LightSelect {
    private LightSelect() { }
    public sealed record Every : LightSelect;
    public sealed record At(int Index) : LightSelect;
    public sealed record Of(Guid Id) : LightSelect;
    public sealed record Named(string Name) : LightSelect;

    internal Fin<Seq<(int Index, LightObject Native)>> Resolve(RhinoDoc document, Op key) =>
        Switch(
            context: (Document: document, Op: key),
            // The table enumerates as `IEnumerable<LightObject>` and the host discourages index lookup in favour
            // of ids, so the roster sweep reads the table directly and pairs each live row with the index its own
            // id resolves — the index survives only because `Modify`, `Delete`, and `Undelete` address by slot.
            every: static (context, _) => context.Op.Catch(() => Fin.Succ(value:
                context.Document.Lights.AsIterable().ToSeq()
                    .Filter(static native => !native.IsDeleted)
                    .Map(native => (Index: context.Document.Lights.Find(native.Id, ignoreDeleted: true), Native: native))
                    .Filter(static row => row.Index >= 0))),
            at: static (context, address) => context.Op.Catch(() =>
                from _ in guard(address.Index >= 0 && address.Index < context.Document.Lights.Count, context.Op.InvalidInput()).ToFin()
                from rows in Row(document: context.Document, index: address.Index, key: context.Op)
                select rows),
            of: static (context, address) => context.Op.Catch(() =>
                from index in Fin.Succ(value: context.Document.Lights.Find(address.Id, ignoreDeleted: true))
                from _ in guard(index >= 0, context.Op.MissingContext()).ToFin()
                from rows in Row(document: context.Document, index: index, key: context.Op)
                select rows),
            named: static (context, address) =>
                from name in context.Op.AcceptText(value: address.Name)
                from rows in context.Op.Catch(() =>
                    from found in Optional(context.Document.Lights.FindName(name)).ToFin(Fail: context.Op.MissingContext())
                    from index in Fin.Succ(value: context.Document.Lights.Find(found.Id, ignoreDeleted: true))
                    from _ in guard(index >= 0, context.Op.MissingContext()).ToFin()
                    from row in Row(document: context.Document, index: index, key: context.Op)
                    select row)
                select rows);

    internal static Fin<(int Index, LightObject Native)> Indexed(
        RhinoDoc document,
        int index,
        Func<LightObject, bool> state,
        Error failure) =>
        from _ in guard(index >= 0 && index < document.Lights.Count, failure).ToFin()
        from native in Optional(document.Lights.FindIndex(index)).ToFin(Fail: failure)
        from __ in guard(state(native), failure).ToFin()
        select (index, native);

    private static Fin<Seq<(int Index, LightObject Native)>> Row(RhinoDoc document, int index, Op key) =>
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
            revive: static (key, work) => guard(work.Index >= 0, key.InvalidInput()).ToFin().Map(_ => (LightOp)work));

    internal Fin<Seq<LightFact>> Apply(RhinoDoc document, Op op) =>
        Switch(
            context: (Document: document, Op: op),
            mint: static (context, work) => work.Seed.Mint(op: context.Op).Bind(lease => lease.Use(
                fresh => context.Op.Catch(() => {
                    _ = work.Name.Iter(name => fresh.Name = name);
                    int index = context.Document.Lights.Add(fresh);
                    return from _ in guard(index >= 0, context.Op.InvalidResult()).ToFin()
                           from native in Optional(context.Document.Lights.FindIndex(index)).ToFin(Fail: context.Op.InvalidResult())
                           select Seq<LightFact>(new LightFact.Minted(Index: index, Id: native.Id));
                }))),
            amend: static (context, work) =>
                from rows in work.Select.Resolve(document: context.Document, key: context.Op)
                from facts in rows.TraverseM(row => context.Op.Catch(() =>
                    from working in Optional(row.Native.DuplicateLightGeometry()).ToFin(Fail: context.Op.InvalidResult())
                    from fact in context.Op.Catch(() => {
                        using Light live = working;
                        return from kind in LightKind.Of(style: live.LightStyle, key: context.Op)
                               from _ in work.Edits.TraverseM(edit => edit.Apply(working: live, kind: kind, op: context.Op)).As()
                               from __ in context.Op.Confirm(success: context.Document.Lights.Modify(row.Index, live))
                               select (LightFact)new LightFact.Amended(Id: row.Native.Id, Edits: work.Edits.Count);
                    })
                    select fact)).As()
                select facts,
            purge: static (context, work) =>
                from rows in work.Select.Resolve(document: context.Document, key: context.Op)
                from facts in rows.TraverseM(row => context.Op
                    .Confirm(success: context.Document.Lights.Delete(row.Index, work.Interaction.IsQuiet))
                    .Map(_ => (LightFact)new LightFact.Purged(Id: row.Native.Id))).As()
                select facts,
            revive: static (context, work) =>
                from row in LightSelect.Indexed(
                    document: context.Document,
                    index: work.Index,
                    state: static native => native.IsDeleted,
                    failure: context.Op.InvalidInput())
                from _ in context.Op.Confirm(success: context.Document.Lights.Undelete(row.Index))
                select Seq<LightFact>(new LightFact.Revived(Index: row.Index)));
}

// --- [MODELS] -----------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LightFact {
    private LightFact() { }
    public sealed record Minted(int Index, Guid Id) : LightFact;
    public sealed record Amended(Guid Id, int Edits) : LightFact;
    public sealed record Purged(Guid Id) : LightFact;
    public sealed record Revived(int Index) : LightFact;
}

public sealed record LightRoster(Seq<LightStamp> Rows) : IDetachedDocumentResult;

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class Lights {
    public static Fin<LightRoster> Ask(DocumentSession session, LightSelect scope) {
        Op op = Op.Of();
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

    public static Fin<ObjectReceipt<LightFact>> Commit(DocumentSession session, RedrawPolicy redraw, params ReadOnlySpan<LightOp> operations) {
        Op op = Op.Of();
        return from policy in op.Need(redraw)
               from requested in LanguageExt.Iterable<LightOp>.FromSpan(operations).ToSeq()
                   .TraverseM(work => op.Need(work)).As()
               from _ in guard(!requested.IsEmpty, op.InvalidInput()).ToFin()
               from plan in requested.TraverseM(work => work.Admit(op: op)).As()
               from receipt in ObjectSpine.Commit(
                   session: session,
                   name: nameof(Lights),
                   redraw: policy,
                   fold: (document, key) => plan.TraverseM(work => work.Apply(document: document, op: key)).As()
                       .Map(static grouped => grouped.Bind(static facts => facts)),
                   op: op)
               select receipt;
    }
}
```

## [05]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]          | [OWNER]       | [FORM]                                                  | [ENTRY]                 |
| :-----: | :----------------- | :------------ | :------------------------------------------------------ | :---------------------- |
|  [01]   | kind capability    | `LightKind`   | world-style rows with modality columns and frozen index | `LightKind.Of`          |
|  [02]   | detached read      | `LightStamp`  | whole light state with kernel cone and perceptual color | `Lights.Ask`            |
|  [03]   | construction       | `LightSeed`   | one polymorphic mint, style written once                | `LightOp.Mint`          |
|  [04]   | photometric power  | `Radiance`    | scale, watts, lumens, candela as one axis               | `LightEdit.Power`       |
|  [05]   | property edits     | `LightEdit`   | kind-gated modalities over one working duplicate        | `LightOp.Amend`         |
|  [06]   | table address      | `LightSelect` | every, index, id, and name onto index-paired rows       | `Lights.Ask` / `Commit` |
|  [07]   | commit consequence | `LightFact`   | minted, amended, purged, revived onto the shared spine  | `Lights.Commit`         |
|  [08]   | host dialogue      | spine owner   | `HostInteraction` composed, never a signal re-spelling  | `LightOp.Purge`         |
|  [09]   | photometric rank   | `PhotometricPower` | four host readings under one declared authority   | `PhotometricPower.Of`   |
|  [10]   | descriptor light   | `ScenePhotometry`  | metres-scaled wire row with unit-free cone        | `ScenePhotometry.Of`    |

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
