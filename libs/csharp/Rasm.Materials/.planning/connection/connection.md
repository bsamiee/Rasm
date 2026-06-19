# [MATERIALS_CONNECTION]

THE POLYMORPHIC CONNECTION OWNER and THE FAMILY GROWTH AXIS. One `ConnectionItem` is the canonical structural-connection-material concept every reinforcement bar, fastener, and framing connector parameterizes — an item shape (the family discriminant, a sectional unit row, an installation-capacity receipt key, an appearance assignment) plus the family vocabulary its layout schedule reads; one `ConnectionFamily` `[SmartEnum<string>]` closes the family-kind axis (reinforcement · fastener · hanger), all three realized as sibling catalogue vocabularies the one `ConnectionCatalogue.Build` folds — the EXACT structural mirror of the `profiles/Profile` owner (`profile#PROFILE_OWNER`, `profile.md` line 13). A connection is NEVER a per-item class: a #5 rebar is a `ConnectionItem` in the `reinforcement` family (`connection/reinforcement#REINFORCEMENT_FAMILY`), a 3/8in hex bolt a `ConnectionItem` in the `fastener` family (`connection/fastener#FASTENER_FAMILY`), a Simpson-style joist hanger a `ConnectionItem` in the `hanger` family (`connection/hanger#HANGER_FAMILY`), differing only in section columns and the family discriminant — never a `Rebar`/`Bolt`/`Hanger` type. The axis is CLOSED at THREE families: `anchor` folds as a `FastenerKind` arm inside the fastener vocabulary, never a fourth sibling family or a fifth page. The owner is `IfcReinforcingBar`/`IfcMechanicalFastener`-aligned (the canonical structural-connection elements the BIM wire serializes at the `Rasm.Bim` boundary). The OOP capsule lives at the boundary (the `[ValueObject]` `ConnectionId` key, the `[ValidationError]`-derived `ConnectionFault` band), the FP-ROP rail owns the internals (the `Fin<T>` admission, the `Seq`/`Fold` catalogue projection). The page composes the `Rasm` kernel for the `PositiveMagnitude`/`Dimension` section value-objects exactly as `profile#PROFILE_OWNER` does, `appearance/graph#MATERIAL_LIBRARY` for the `MaterialId` appearance column (`metal.iron`/`metal.steel`, the `Profile.AppearanceId` mirror at `graph.md` line 212), `physical-properties#MATERIAL_PROPERTY` for the `Mechanical` capacity receipt key (read by `MaterialId`, never re-derived here — `properties.md` line 12), and `construction/layout#ASSEMBLY_FOLD` for the station-stepped placement a rebar schedule or fastener pattern reads over the SAME realized `Resolve` fold (`layout.md` line 66 / `StationStep` line 95), never a parallel layout owner. Each family vocabulary lives on its own sibling page (`reinforcement`/`fastener`/`hanger`); this page owns the one `ConnectionItem` shape, the closed family axis, the `ConnectionId` key, the band-2360 `ConnectionFault`, and the `ConnectionCatalogue` registered-row table.

## [1]-[INDEX]

| [CLUSTER]                  | [OWNS]                                                                                                                                                                                                                                                                       |
| -------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `[2]-[CONNECTION_OWNER]`   | The one `ConnectionItem` `[Union]` over the closed `ConnectionFamily` `[SmartEnum<string>]` axis (reinforcement/fastener/hanger), the `ConnectionId` `[ValueObject]` key with `ConnectionKeyPolicy` ordinal accessor, the band-2360 `ConnectionFault` `[Union]`, the `ConnectionItem.Of` polymorphic admission entry, and the context-folded `ConnectionCatalogue` registered-row table over all three family builders. |

## [2]-[CONNECTION_OWNER]

- Owner: `ConnectionItem` over the closed `ConnectionFamily` axis; `ConnectionId` key; `ConnectionFault` `[Union]` band 2360; `ConnectionKeyPolicy` ordinal accessor; `ConnectionCatalogue` the registered-row table.
- Cases: one `ConnectionItem` shape across all families — `Family` (the discriminant), `Designation` (the `ConnectionId` key, `connection.<designation>`), `Section` (the family-projected sectional unit row as a `ConnectionSection` `[Union]` arm: `Reinforcement`/`Fastener`/`Hanger`), `CapacityKey` (the `MaterialId` whose `physical-properties#MATERIAL_PROPERTY` `Mechanical` row carries the installation capacity), `AppearanceId` (the `graph#MATERIAL_LIBRARY` `MaterialId` row); family {reinforcement, fastener, hanger} closed at three (`anchor` is a `FastenerKind` arm inside the fastener vocabulary, never a family); an item is a `ConnectionFamily` ROW, never a connection subtype.
- Entry: `public static Fin<ConnectionItem> Of(ConnectionFamily family, string designation, ConnectionSection section, MaterialId capacityKey, MaterialId appearanceId, Op key)` — one polymorphic admission entry, never a `GetById`/`GetByFamily` family (the `Profile.Of` law, `profile.md` line 13); `Fin<T>` aborts on a malformed designation (`ConnectionFault.Designation`, key-correlated), a family/section discriminant mismatch (`ConnectionFault.Family`), an out-of-band grade (`ConnectionFault.Grade`), or a non-positive capacity column (`ConnectionFault.Capacity`); `ConnectionCatalogue.Build(context)` folds every family's row builder (`reinforcement#REINFORCEMENT_FAMILY` `BuildRebarRows` plus `fastener#FASTENER_FAMILY` `BuildFastenerRows` plus `hanger#HANGER_FAMILY` `BuildHangerRows`) into the one frozen registry, `ConnectionCatalogue.Lookup(rows, id, key)` resolves a registered `ConnectionId` to its catalogue `ConnectionItem`, and the same `Of` admits an ad-hoc item through the row validation a registered row passes — one polymorphic entry.
- Packages: Rasm (project — `PositiveMagnitude`/`Dimension`/`UnitInterval` value-objects the family sections compose), Thinktecture.Runtime.Extensions (`[Union]`/`[SmartEnum<string>]`/`[ValueObject]` generators at their deepest surface — generated total `Switch`, `[KeyMemberEqualityComparer]`/`[KeyMemberComparer]`), LanguageExt.Core (`Fin`/`Seq`/`Fold` for the admission rail and the catalogue fold), BCL inbox (`FrozenDictionary`); no new external package — the connection owner is the structural copy of the proven `profile#PROFILE_OWNER` shape, never a re-invention.
- Growth: a new structural connection is one `ConnectionItem` row in the matching `ConnectionFamily`; a new family is one `ConnectionFamily` case carrying its section vocabulary on its own sibling page folded into `ConnectionCatalogue.Build`; a new fault is one `ConnectionFault` case — never a `Rebar`/`Bolt`/`Hanger` type, never a per-family `ConnectionItem` variant. All three families are realized, each its own `ConnectionFamily` case with its own `ConnectionSection` arm and `BuildXRows` catalogue builder: `reinforcement` carries the `RebarGrade`/`BarSize` axis and the ASTM A615/A706 bar-diameter/area/unit-weight rows plus the ACI 318 bend/hook scalar tuple, `fastener` the `FastenerKind` (bolt/nut/screw/anchor) discriminant and the ISO 898-1/SAE J429 `FastenerGrade` proof/tensile axis over the thread/pitch/shank/head columns, `hanger` the `HangerType` framing-connector discriminant over the carried-member/download-uplift/gauge columns — a new bar size, bolt grade, or hanger is a row data addition, never a new surface.
- Boundary: `ConnectionItem` is the ONE structural-connection concept — a per-item class is the deleted form; the `ConnectionSection` `[Union]` (`Reinforcement`/`Fastener`/`Hanger`) carries each family's projected columns so the one item shape never branches into per-family types, and every length column composes the `Rasm` kernel `PositiveMagnitude` value-object (the double-backed `> 0` finite magnitude) exactly as `ProfileUnit` does (`profile.md` line 16) so a connection never re-mints a dimension primitive and a fractional bar diameter — an 11.3 mm #4 bar, a 9.525 mm 3/8in bolt — admits without the truncation an int-backed `Dimension` count would force, the `Dimension` carrier reserved for discrete counts (bar layers, thread starts); `ConnectionFault` is the one fault every `Fin.Fail` reads (designation/grade/capacity/family slots), an `Expected`-derived `Error` (`IValidationError<ConnectionFault>`) whose 2360 band IS the `Expected` `Code` so a bare typed case lifts directly into the `Fin<T>` rail (the `ProfileFault` law, `profile.md` lines 41-48), disjoint from `ProfileFault` 2300 / `ConstructionFault` 2350 / kernel `GeometryFault` 2400 / `MaterialFault` 2450 (the four-band statement, `bsdf.md` line 3 + `ARCHITECTURE.md` line 39), so a connection schedule never throws and never returns a sentinel; the `[ValueObject]` `ConnectionId` key and the `[ValidationError]`-derived `ConnectionFault` are the OOP capsule at the edge, the `Fin`/`Seq`/`Fold` catalogue projection is the FP-ROP internal; the capacity receipt is NOT re-derived here — `CapacityKey` is the `MaterialId` whose `physical-properties#MATERIAL_PROPERTY` `Mechanical` row (`DensityKgM3`/`YoungsModulusMpa`/`YieldStrengthMpa`/`PoissonsRatio`/`ThermalExpansionPerK`, `properties.md` line 12) the structural-connection-design seam reads by key, so a bolt's proof load and a bar's yield are read once from the property library, never duplicated as a connection column; the appearance assignment crosses to `appearance/graph#MATERIAL_LIBRARY` as a `MaterialId` row a `ConnectionItem.AppearanceId` column carries (the `Profile.AppearanceId` mirror, `graph.md` line 212), never a connection-specific surface; the placement that turns a `ConnectionItem` schedule into a station-stepped stream is `construction/layout#ASSEMBLY_FOLD` `Resolve`/`StationStep` (`layout.md` line 66 / line 95), composed not re-derived here — a rebar schedule and a fastener pattern are station-stepped placements over the SAME fold, never a parallel `connection/layout` owner; the bend/hook geometry of a reinforcement bar is a scalar bend-angle/radius/hook-extension tuple the host materializes (`connection/reinforcement#REINFORCEMENT_FAMILY`), NEVER a host curve here (the host-neutral scalar-`Placement` discipline, `layout.md` line 17); the family vocabularies (`RebarGrade`/`BarSize`/`RebarSection`, `FastenerKind`/`FastenerGrade`/`FastenerSection`, `HangerType`/`HangerSection`) live on their sibling pages, and the `ConnectionCatalogue.Build` registered-row table folds every family's `BuildXRows` into one frozen registry so a registered row keys the same way as each family lands its own builder; the item serializes to the IFC 4.3 `IfcReinforcingBar`/`IfcReinforcingMesh`/`IfcMechanicalFastener`/`IfcFastener` element at the `Rasm.Bim` boundary (portable scalar data here, never an interior `IfcOpenShell` evaluation), the per-family wire shape noted on each sibling page.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
[ValueObject<string>]
[KeyMemberEqualityComparer<ConnectionKeyPolicy, string>]
[KeyMemberComparer<ConnectionKeyPolicy, string>]
public readonly partial struct ConnectionId {
    static partial void NormalizeAndValidate(ref string value, ref ValidationError? validationError) =>
        validationError = string.IsNullOrWhiteSpace(value) || !value.StartsWith("connection.", StringComparison.Ordinal) || value.Length <= "connection.".Length
            ? new ValidationError("<connection-id requires 'connection.<designation>'>")
            : null;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ConnectionKeyPolicy, string>]
[KeyMemberComparer<ConnectionKeyPolicy, string>]
public sealed partial class ConnectionFamily {
    public static readonly ConnectionFamily Reinforcement = new("reinforcement");
    public static readonly ConnectionFamily Fastener = new("fastener");
    public static readonly ConnectionFamily Hanger = new("hanger");
}

// --- [ERRORS] ------------------------------------------------------------------------------
[Union]
public abstract partial record ConnectionFault : Expected, IValidationError<ConnectionFault> {
    private ConnectionFault(Op key, string detail) : base(detail, 2360, None) => Key = key;
    public Op Key { get; }
    public static ConnectionFault Create(string message) => new Family(default, message);
    public sealed record Designation(Op Key, string Detail) : ConnectionFault(Key, Detail) { public override string Category => "Designation"; }
    public sealed record Grade(Op Key, string Detail) : ConnectionFault(Key, Detail) { public override string Category => "Grade"; }
    public sealed record Capacity(Op Key, string Detail) : ConnectionFault(Key, Detail) { public override string Category => "Capacity"; }
    public sealed record Family(Op Key, string Detail) : ConnectionFault(Key, Detail) { public override string Category => "Family"; }
}

// --- [SERVICES] ----------------------------------------------------------------------------
public sealed class ConnectionKeyPolicy : IEqualityComparerAccessor<string>, IComparerAccessor<string> {
    public static IEqualityComparer<string> EqualityComparer => StringComparer.Ordinal;
    public static IComparer<string> Comparer => StringComparer.Ordinal;
}

// --- [MODELS] ------------------------------------------------------------------------------
[Union]
public abstract partial record ConnectionSection {
    public sealed record Reinforcement(RebarSection Bar) : ConnectionSection;
    public sealed record Fastener(FastenerSection Bolt) : ConnectionSection;
    public sealed record Hanger(HangerSection Connector) : ConnectionSection;

    public ConnectionFamily Family => Switch(
        reinforcement: static _ => ConnectionFamily.Reinforcement,
        fastener:      static _ => ConnectionFamily.Fastener,
        hanger:        static _ => ConnectionFamily.Hanger);

    public PositiveMagnitude NominalMm => Switch(
        reinforcement: static r => r.Bar.DiameterMm,
        fastener:      static f => f.Bolt.ThreadDiameterMm,
        hanger:        static h => h.Connector.CarriedMemberWidthMm);
}

public sealed record ConnectionItem(
    ConnectionFamily Family,
    ConnectionId Designation,
    ConnectionSection Section,
    MaterialId CapacityKey,
    MaterialId AppearanceId) {

    public static Fin<ConnectionItem> Of(ConnectionFamily family, string designation, ConnectionSection section, MaterialId capacityKey, MaterialId appearanceId, Op key) =>
        from id in ConnectionId.Validate(value: designation, provider: CultureInfo.InvariantCulture, item: out ConnectionId? built) is { } error
            ? Fin.Fail<ConnectionId>(ConnectionFault.Designation(key, $"<malformed-designation:{designation}:{error.Message}>"))
            : Fin.Succ(built!)
        from matched in guard(section.Family == family, ConnectionFault.Family(key, $"<family-section-mismatch:{family.Key}<>{section.Family.Key}>"))
        from positive in guard(section.NominalMm.Value > 0.0, ConnectionFault.Capacity(key, $"<non-positive-nominal:{section.NominalMm.Value}>"))
        select new ConnectionItem(family, id, section, capacityKey, appearanceId);
}

// --- [TABLES] ------------------------------------------------------------------------------
public static class ConnectionCatalogue {
    public static FrozenDictionary<ConnectionId, ConnectionItem> Build(Context context) =>
        Reinforcement.ConnectionCatalogue.BuildRebarRows(context)
            .Concat(Fastener.ConnectionCatalogue.BuildFastenerRows(context))
            .Concat(Hanger.ConnectionCatalogue.BuildHangerRows(context))
            .ToFrozenDictionary(static r => r.Key, static r => r.Value, ConnectionKeyPolicy.EqualityComparer);

    public static Fin<ConnectionItem> Lookup(FrozenDictionary<ConnectionId, ConnectionItem> rows, ConnectionId id, Op key) =>
        rows.TryGetValue(id, out ConnectionItem? row) ? Fin.Succ(row!) : Fin.Fail<ConnectionItem>(ConnectionFault.Family(key, $"<unregistered-connection:{id.Value}>"));
}
```
