# [RASM_GRASSHOPPER_PORTS]

`PortRow` is the data-driven pin catalogue. Each row owns its verified carrier, semantic family, capability axes, and one side-aware `PortBinding`; carrier admission therefore returns the unique semantic candidate instead of forcing duplicate carriers into a dictionary. `PinPlan` carries only host-writable policy, `PinTrim` mirrors the complete writable modifier columns, and `Ports` rejects every policy value outside the selected row's consumed axes before invoking an adder.

## [01]-[INDEX]

- [02]-[PIN_VOCABULARY]: host discriminants, semantic carrier families, and row capability axes
- [03]-[PIN_PLAN]: writable pin policy, complete trim columns, and persistent tree data
- [04]-[PORT_CATALOG]: verified carrier rows and the side-aware binding union
- [05]-[DECLARATION_FOLD]: semantic admission, capability validation, declaration, and maintenance realization

## [02]-[PIN_VOCABULARY]

- Owner: the generated vocabularies own host side, access, presence, and visibility with semantic carrier families; `PortAxes` states whether a row consumes access, presence, appearance, hidden registration, and one side-scoped trim family — the four parallel axis FIELDS collapse to ONE `HashMap<PortAxis, CapabilitySet<PinSide>>` (E-G42), so an axis is a ROW and an absent row is the empty capability set, never a fifth field.
- Cases: `PinSide` mirrors `Side` AND realizes `ICapability<PinSide>` — side capability is `CapabilitySet<PinSide>` membership, so the four-row `PortSides` bool-pair vocabulary (`Both`/`InputOnly`/`OutputOnly`/`Neither`) deletes onto the set algebra (`All`/`Of(Input)`/`Of(Output)`/`None`); `PinAccess` mirrors `Access`, `PinPresence` mirrors `Requirement`, and `PinVisibility` closes shown/hidden. `Document/graph.md`'s `GripHit` composes the same `CapabilitySet<PinSide>` for its in-range sides — one side vocabulary across the folder.
- Entry: `PinSide.Of(Side, Op?)` is the one reverse projection; unknown host values fail typed.
- Growth: a new host discriminant value is one row on the owning vocabulary.
- Boundary: `Side`, `Access`, and `Requirement` never travel past a binding delegate; interior code holds only the folder vocabulary.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using Grasshopper2.Components;
using Rasm.Domain;

namespace Rasm.Grasshopper.Components;

// --- [TYPES] ---------------------------------------------------------------------------

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PinSide : ICapability<PinSide> {
    public static readonly PinSide Input = new(key: "input", host: Side.Input);
    public static readonly PinSide Output = new(key: "output", host: Side.Output);

    public Side Host { get; }

    public static CapabilityLaw<PinSide> Law => CapabilityLaw<PinSide>.Open;

    public static Fin<PinSide> Of(Side host, Op? key = null) => host switch {
        Side.Input => Fin.Succ(Input),
        Side.Output => Fin.Succ(Output),
        _ => Fin.Fail<PinSide>(new GhFault.ContractRefused(GhContract.Pin, new GhEvidence(key.OrDefault(), $"{nameof(Side)}:{host}"))),
    };
}

[SmartEnum]
public sealed partial class PinAccess {
    public static readonly PinAccess Item = new(Access.Item);
    public static readonly PinAccess Twig = new(Access.Twig);
    public static readonly PinAccess Tree = new(Access.Tree);

    public Access Host { get; }
}

[SmartEnum]
public sealed partial class PinPresence {
    public static readonly PinPresence MustExist = new(Requirement.MustExist);
    public static readonly PinPresence MayBeNull = new(Requirement.MayBeNull);
    public static readonly PinPresence MayBeMissing = new(Requirement.MayBeMissing);

    public Requirement Host { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PortAxis {
    public static readonly PortAxis Access = new(key: "access");
    public static readonly PortAxis Presence = new(key: "presence");
    public static readonly PortAxis Appearance = new(key: "appearance");
    public static readonly PortAxis Hidden = new(key: "hidden");
}

[SmartEnum]
public sealed partial class PinVisibility {
    public static readonly PinVisibility Shown = new();
    public static readonly PinVisibility Hidden = new();
}

[SmartEnum]
public sealed partial class PortFamily {
    private static readonly Func<Type, Type, bool> Assignable = static (declared, candidate) => declared.IsAssignableFrom(candidate);

    public static readonly PortFamily Standard = new(Assignable);
    public static readonly PortFamily Generic = new(static (_, _) => true);
    public static readonly PortFamily Numeric = new(static (_, candidate) =>
        candidate == typeof(int)
        || candidate == typeof(double)
        || candidate == typeof(System.Numerics.BigInteger)
        || candidate == typeof(System.Numerics.Complex)
        || candidate == typeof(Grasshopper2.Types.Numeric.Angle));
    public static readonly PortFamily Index = new(Assignable);
    public static readonly PortFamily Pattern = new(Assignable);
    public static readonly PortFamily Topology = new(Assignable);

    [UseDelegateFromConstructor]
    public partial bool Accepts(Type declared, Type candidate);
}

public sealed record PortAxes(
    HashMap<PortAxis, CapabilitySet<PinSide>> Rows,
    Option<(Type Type, CapabilitySet<PinSide> Sides)> Trim) {
    public static PortAxes Modular => new(HashMap(
        (PortAxis.Access, CapabilitySet<PinSide>.All),
        (PortAxis.Presence, CapabilitySet<PinSide>.Of(PinSide.Input)),
        (PortAxis.Appearance, CapabilitySet<PinSide>.All),
        (PortAxis.Hidden, CapabilitySet<PinSide>.All)), None);
    public static PortAxes Regular => new(HashMap(
        (PortAxis.Access, CapabilitySet<PinSide>.All),
        (PortAxis.Presence, CapabilitySet<PinSide>.Of(PinSide.Input))), None);
    public static PortAxes Identity => new(HashMap<PortAxis, CapabilitySet<PinSide>>(), None);

    public CapabilitySet<PinSide> Row(PortAxis axis) => Rows.Find(axis).IfNone(CapabilitySet<PinSide>.None);

    public PortAxes WithTrim<TTrim>(Option<CapabilitySet<PinSide>> sides = default) where TTrim : PinTrim =>
        this with { Trim = Some((Type: typeof(TTrim), Sides: sides.IfNone(() => CapabilitySet<PinSide>.All))) };
}
```

## [03]-[PIN_PLAN]

- Owner: `PinKey` `[ValueObject<string>]` is the admitted pin identity (E-G44) — trimmed and non-blank through the kernel default validation bridge; `PinPlan` carries the `PinKey` name and nick, selected row, access, presence, writable appearance, one optional complete trim, visibility, and optional `ITree` persistence. `PinTrim` mirrors every verified writable modifier column; `PresetsWeak` and `TypeAssistantWeak` remain read-only host projections, and assistant reads live on `GardenData`.
- Cases: `PinTrim` closes over Boolean null/negation policy, connection collection, vector flags, angle enforcement and reduction, integer index policy and hint, number hint, numeric exotic filtering, curve parameterization and flip, surface mesh admission/parameterization/flip, text/file behavior, and text-pattern behavior. Invalid corners are UNREPRESENTABLE, not guarded (E-G43): `NullPosture` (`AsIs`/`AsTrue`/`AsFalse`) replaces the Boolean case's two-flag pair whose both-true cell the old `IsValid` had to refuse, and `IndexPosture` (`Plain(Option<UiInteger>)`/`Indexed(IndexRow)` — the host-enum owner row, so the raw `IndexModifier` never crosses a folder signature) replaces the integer case's `AsIndex` flag whose false-with-indexing cell the old guard AND the `AdmitsTrim` cross-checks re-refused; the property writes are the GENERATED `TrimMap` Mapperly seam — one `[MapProperty]`-annotated update pair per case, `[MapDerivedType]`-style correspondence stated as data, the eleven hand assignment blocks deleted.
- Entry: `PinPlan.Realize(IParameter)` admits one trim, projects its exact host property types, and assigns only carrier-compatible persistent tree data.
- Law: a refused trim-to-parameter pairing is `GhFault.ContractRefused(GhContract.Pin, evidence)` naming both shapes.
- Growth: a new writable parameter policy is one trim case and one row capability; a new adder shape is one `PortBinding` case.
- Boundary: policy assignment crosses through `HostCall.Run`; an incompatible trim fails by exact case and host type before any property is written. Every column a trim writes is a `public { get; set; }` auto-property on the concrete parameter, so a trim is a post-declaration write and never a declaration argument.
- Boundary: `AngleParameter.EnforceKind` is a raw host `int` with NO host enum behind it — the persisted `Integer32("EnforceKind")` and the base's own `== 1`/`== 2`/`== 3` toolbar reads ARE the protocol — so `AngleEnforcement` is the owner that types those four wire constants and its `int Host` column is the host's own value, not a hand-numbered stand-in for an enum ordinal.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using Grasshopper2.Components;
using Grasshopper2.Data;
using Grasshopper2.Parameters;
using Grasshopper2.Parameters.Standard;
using Grasshopper2.UI;
using Rasm.Domain;
using Riok.Mapperly.Abstractions;

namespace Rasm.Grasshopper.Components;

// --- [MODELS] --------------------------------------------------------------------------

[ValueObject<string>]
[ValidationError]
public readonly partial struct PinKey {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value?.Trim() ?? string.Empty;
        validationError = value.Length > 0 ? null : new ValidationError("PinKey requires a non-blank identity.");
    }
}

[SmartEnum<int>]
public sealed partial class NullPosture {
    public static readonly NullPosture AsIs = new(key: 0, asTrue: false, asFalse: false);
    public static readonly NullPosture AsTrue = new(key: 1, asTrue: true, asFalse: false);
    public static readonly NullPosture AsFalse = new(key: 2, asTrue: false, asFalse: true);
    internal bool AsTrueHost { get; }
    internal bool AsFalseHost { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record IndexPosture {
    private IndexPosture() { }
    public sealed record Plain(Option<UiInteger> Hint) : IndexPosture;
    public sealed record Indexed(IndexRow Indexing) : IndexPosture;
}

[SmartEnum<int>]
public sealed partial class IndexRow {
    public static readonly IndexRow Strict = new(key: 0, host: Grasshopper2.Parameters.Standard.IndexModifier.None);
    public static readonly IndexRow Clip = new(key: 1, host: Grasshopper2.Parameters.Standard.IndexModifier.Clip);
    public static readonly IndexRow Wrap = new(key: 2, host: Grasshopper2.Parameters.Standard.IndexModifier.Wrap);
    public static readonly IndexRow Ping = new(key: 3, host: Grasshopper2.Parameters.Standard.IndexModifier.Ping);
    internal Grasshopper2.Parameters.Standard.IndexModifier Host { get; }
}

[SmartEnum]
public sealed partial class AngleEnforcement {
    public static readonly AngleEnforcement None = new(0);
    public static readonly AngleEnforcement Degrees = new(1);
    public static readonly AngleEnforcement Radians = new(2);
    public static readonly AngleEnforcement Turns = new(3);

    public int Host { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PinTrim {
    private PinTrim() { }

    public sealed record Vector(bool Unitise, bool Reverse) : PinTrim;
    public sealed record Angle(AngleEnforcement Enforce, bool Reduce) : PinTrim;
    public sealed record Boolean(bool Negate, NullPosture Nulls) : PinTrim;
    public sealed record Connection(bool Collect) : PinTrim;
    public sealed record Integer(IndexPosture Posture) : PinTrim;
    public sealed record Number(UiNumber Hint) : PinTrim;
    public sealed record Numeric(NumericFilter Exotic) : PinTrim;
    public sealed record Curve(Grasshopper2.Parameters.Standard.CurveParameter.NormalisationMethod Domains, bool Flip) : PinTrim;
    public sealed record Surface(
        bool AcceptMeshes,
        Grasshopper2.Parameters.Standard.CurveParameter.NormalisationMethod Domains,
        bool Flip) : PinTrim;
    public sealed record Text(
        TextFlavour Flavour,
        Seq<string> FileExtensions,
        bool WatchFiles,
        TextParameter.CasingBehaviour Casing,
        bool CleanWhitespace) : PinTrim;
    public sealed record TextPattern(TextPatternKind Kind, bool CaseSensitive) : PinTrim;

    public bool IsValid => Switch(
        vector: static _ => true,
        angle: static trim => trim.Enforce is not null,
        boolean: static _ => true,
        connection: static _ => true,
        integer: static _ => true,
        number: static trim => trim.Hint is not null,
        numeric: static trim => (trim.Exotic & ~NumericFilter.All) == 0,
        curve: static trim => Enum.IsDefined(trim.Domains),
        surface: static trim => Enum.IsDefined(trim.Domains),
        text: static trim => trim.Flavour is TextFlavour.String or TextFlavour.File && Enum.IsDefined(trim.Casing) &&
            trim.FileExtensions.ForAll(static extension => !string.IsNullOrWhiteSpace(extension)) &&
            (trim.Flavour == TextFlavour.File || trim.FileExtensions.IsEmpty && !trim.WatchFiles),
        textPattern: static trim => Enum.IsDefined(trim.Kind));

    internal Fin<Unit> Apply(IParameter parameter, Op key) => (this, parameter) switch {
        (Vector trim, VectorParameter host) => HostCall.Run(() => TrimMap.Write(trim, host), key),
        (Angle trim, AngleParameter host) => HostCall.Run(() => TrimMap.Write(trim, host), key),
        (Boolean trim, BooleanParameter host) => HostCall.Run(() => TrimMap.Write(trim, host), key),
        (Connection trim, ConnectionParameter host) => HostCall.Run(() => TrimMap.Write(trim, host), key),
        (Integer trim, IntegerParameter host) => HostCall.Run(() => TrimMap.Write(trim, host), key),
        (Number trim, NumberParameter host) => HostCall.Run(() => TrimMap.Write(trim, host), key),
        (Numeric trim, NumericParameter host) => HostCall.Run(() => TrimMap.Write(trim, host), key),
        (Curve trim, CurveParameter host) => HostCall.Run(() => TrimMap.Write(trim, host), key),
        (Surface trim, SurfaceParameter host) => HostCall.Run(() => TrimMap.Write(trim, host), key),
        (Text trim, TextParameter host) => HostCall.Run(() => TrimMap.Write(trim, host), key),
        (TextPattern trim, TextPatternParameter host) => HostCall.Run(() => TrimMap.Write(trim, host), key),
        _ => Fin.Fail<Unit>(new GhFault.ContractRefused(GhContract.Pin, new GhEvidence(key, $"{GetType().Name}:{parameter.GetType().Name}"))),
    };
}

[Mapper]
internal static partial class TrimMap {
    [MapProperty(nameof(PinTrim.Vector.Unitise), nameof(VectorParameter.UnitiseVectors))]
    [MapProperty(nameof(PinTrim.Vector.Reverse), nameof(VectorParameter.ReverseVectors))]
    internal static partial void Write(PinTrim.Vector trim, VectorParameter host);

    [MapPropertyFromSource(nameof(AngleParameter.EnforceKind), Use = nameof(EnforceOf))]
    [MapProperty(nameof(PinTrim.Angle.Reduce), nameof(AngleParameter.ReduceAngles))]
    internal static partial void Write(PinTrim.Angle trim, AngleParameter host);

    [MapProperty(nameof(PinTrim.Boolean.Negate), nameof(BooleanParameter.NegateValues))]
    [MapPropertyFromSource(nameof(BooleanParameter.ReplaceNullsWithTrue), Use = nameof(NullTrueOf))]
    [MapPropertyFromSource(nameof(BooleanParameter.ReplaceNullsWithFalse), Use = nameof(NullFalseOf))]
    internal static partial void Write(PinTrim.Boolean trim, BooleanParameter host);

    [MapProperty(nameof(PinTrim.Connection.Collect), nameof(ConnectionParameter.DoCollect))]
    internal static partial void Write(PinTrim.Connection trim, ConnectionParameter host);

    internal static void Write(PinTrim.Integer trim, IntegerParameter host) => ignore(trim.Posture.Switch(
        state: host,
        plain: static (target, posture) => Op.Side(action: () => {
            target.IsIndex = false;
            target.Indexing = Grasshopper2.Parameters.Standard.IndexModifier.None;
            posture.Hint.Iter(hint => target.Hint = hint);
        }),
        indexed: static (target, posture) => Op.Side(action: () => {
            target.IsIndex = true;
            target.Indexing = posture.Indexing.Host;
        })));

    [MapProperty(nameof(PinTrim.Number.Hint), nameof(NumberParameter.Hint))]
    internal static partial void Write(PinTrim.Number trim, NumberParameter host);

    [MapProperty(nameof(PinTrim.Numeric.Exotic), nameof(NumericParameter.ExoticFilter))]
    internal static partial void Write(PinTrim.Numeric trim, NumericParameter host);

    [MapProperty(nameof(PinTrim.Curve.Domains), nameof(CurveParameter.NormaliseDomains))]
    [MapProperty(nameof(PinTrim.Curve.Flip), nameof(CurveParameter.FlipCurves))]
    internal static partial void Write(PinTrim.Curve trim, CurveParameter host);

    [MapProperty(nameof(PinTrim.Surface.AcceptMeshes), nameof(SurfaceParameter.AcceptMeshes))]
    [MapProperty(nameof(PinTrim.Surface.Domains), nameof(SurfaceParameter.NormaliseDomains))]
    [MapProperty(nameof(PinTrim.Surface.Flip), nameof(SurfaceParameter.FlipSurfaces))]
    internal static partial void Write(PinTrim.Surface trim, SurfaceParameter host);

    [MapProperty(nameof(PinTrim.Text.Flavour), nameof(TextParameter.Flavour))]
    [MapProperty(nameof(PinTrim.Text.FileExtensions), nameof(TextParameter.FileExtensions))]
    [MapProperty(nameof(PinTrim.Text.WatchFiles), nameof(TextParameter.WatchFiles))]
    [MapProperty(nameof(PinTrim.Text.Casing), nameof(TextParameter.Casing))]
    [MapProperty(nameof(PinTrim.Text.CleanWhitespace), nameof(TextParameter.CleanWhitespace))]
    internal static partial void Write(PinTrim.Text trim, TextParameter host);

    [MapProperty(nameof(PinTrim.TextPattern.Kind), nameof(TextPatternParameter.PatternKind))]
    [MapProperty(nameof(PinTrim.TextPattern.CaseSensitive), nameof(TextPatternParameter.CaseSensitive))]
    internal static partial void Write(PinTrim.TextPattern trim, TextPatternParameter host);

    private static int EnforceOf(PinTrim.Angle trim) => trim.Enforce.Host;
    private static bool NullTrueOf(PinTrim.Boolean trim) => trim.Nulls.AsTrueHost;
    private static bool NullFalseOf(PinTrim.Boolean trim) => trim.Nulls.AsFalseHost;
}

public sealed record PinPlan {
    public required PinKey Name { get; init; }

    public required PinKey Nick { get; init; }

    public required string Info { get; init; }

    public required PortRow Kind { get; init; }

    public PinAccess Access { get; init; } = PinAccess.Item;

    public PinPresence Presence { get; init; } = PinPresence.MustExist;

    public Option<PinTrim> Trim { get; init; } = default;

    public PinVisibility Visibility { get; init; } = PinVisibility.Shown;

    public Option<string> Category { get; init; } = default;

    public Option<Eto.Drawing.Color> Colour { get; init; } = default;

    public Option<ITree> Persistent { get; init; } = default;

    internal IParameter Mint(
        Func<string, string, string, string, Eto.Drawing.Color, Access, Requirement, IParameter> shown,
        Func<string, string, string, string, Eto.Drawing.Color, Access, Requirement, IParameter> hidden) =>
        (Visibility == PinVisibility.Shown ? shown : hidden)(
            (string)Name, (string)Nick, Info, Category.IfNone(""), Colour.IfNone(Eto.Drawing.Colors.Transparent), Access.Host, Presence.Host);

    internal IParameter Mint(
        Func<string, string, string, string, Eto.Drawing.Color, Access, IParameter> shown,
        Func<string, string, string, string, Eto.Drawing.Color, Access, IParameter> hidden) =>
        (Visibility == PinVisibility.Shown ? shown : hidden)(
            (string)Name, (string)Nick, Info, Category.IfNone(""), Colour.IfNone(Eto.Drawing.Colors.Transparent), Access.Host);

    internal IParameter Mint(Func<string, string, string, Access, Requirement, IParameter> bare) =>
        bare((string)Name, (string)Nick, Info, Access.Host, Presence.Host);

    internal IParameter Mint(Func<string, string, string, Access, IParameter> bare) =>
        bare((string)Name, (string)Nick, Info, Access.Host);

    public Fin<Unit> Realize(IParameter parameter, Op? key = null) =>
        Trim.Match(
                Some: trim => trim.IsValid
                    ? trim.Apply(parameter, key.OrDefault())
                    : Fin.Fail<Unit>(new GhFault.ContractRefused(GhContract.Pin, new GhEvidence(key.OrDefault(), $"{nameof(PinTrim)}:{trim.GetType().Name}"))),
                None: static () => Fin.Succ(unit))
            .Bind(_ => Persistent.Match(
                Some: held => Kind is not null && Kind.Family.Accepts(Kind.Carrier, held.Type)
                    ? HostCall.Run(() => { parameter.PersistentDataWeak = held; }, key.OrDefault())
                    : Fin.Fail<Unit>(new GhFault.ContractRefused(GhContract.Pin, new GhEvidence(key.OrDefault(), $"{nameof(Persistent)}:{held.Type.Name}"))),
                None: static () => Fin.Succ(unit)));
}
```

## [04]-[PORT_CATALOG]

- Owner: `PortRow` is the `[SmartEnum<string>]` catalogue; every row carries its verified value carrier, semantic `PortFamily`, its `PortAxes` axis map, and one package-minted `PortBinding` whose `Sides` projects as `CapabilitySet<PinSide>`. Its internal binding union owns both-sided, input-only, and output-only adder invocation without sentinel delegates or a public delegate-construction surface.
- Cases: modular rows consume appearance and hidden policy; regular rows consume access and input presence; mixed rows state their consumed axes per side; topology consumes only its input-side connection trim.
- Entry: `Candidates(Type, PortFamily)` performs assignable carrier matching inside one semantic family, and `Admit` requires that match to resolve exactly one row.
- Auto: `Accepts` rejects unsupported side, hidden, access, presence, appearance, trim, and persistent carrier policy before any adder call.
- Packages: `Thinktecture.Runtime.Extensions` generates the row vocabulary; carriers are `Rhino.Geometry` value and geometry types and the `Grasshopper2` data types.
- Growth: a new host pin kind is one row carrying its exact value type, family, axes, and binding case.
- Law: row keys are the `[a-z0-9-]` kebab grammar every `[SmartEnum<string>]` in this package holds, and they reach fault detail alone — no host member consumes a `PortRow.Key`, so the grammar is free of host naming.
- Boundary: `AddTopological` carries identity text only; `Numeric`, `Generic`, `Index`, `TextPattern`, and connection semantics remain distinct families even where their CLR carrier overlaps another row.
- Boundary: a host `Add*` marked `[Obsolete]` is never suppressed — the row mints its parameter and attaches it through the public `InputAdder.Add(IParameter, Requirement)` seam, which is the same declaration with no diagnostic to silence.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using Grasshopper2.Components;
using Grasshopper2.Data;
using Grasshopper2.Parameters;
using Rasm.Domain;

namespace Rasm.Grasshopper.Components;

// --- [SERVICES] ------------------------------------------------------------------------

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial record PortBinding {
    private PortBinding() { }

    internal sealed record BothCase(
        Func<ModularInputAdder, PinPlan, IParameter> Input,
        Func<ModularOutputAdder, PinPlan, IParameter> Output) : PortBinding;
    internal sealed record InputCase(Func<ModularInputAdder, PinPlan, IParameter> Value) : PortBinding;
    internal sealed record OutputCase(Func<ModularOutputAdder, PinPlan, IParameter> Value) : PortBinding;

    public CapabilitySet<PinSide> Sides => Switch(
        bothCase: static _ => CapabilitySet<PinSide>.All,
        inputCase: static _ => CapabilitySet<PinSide>.Of(PinSide.Input),
        outputCase: static _ => CapabilitySet<PinSide>.Of(PinSide.Output));

    public Fin<IParameter> Bind(ModularInputAdder adder, PinPlan plan, Op key) => Switch(
        bothCase: row => HostCall.Run(() => row.Input(adder, plan), key),
        inputCase: row => HostCall.Run(() => row.Value(adder, plan), key),
        outputCase: _ => Fin.Fail<IParameter>(new GhFault.ContractRefused(GhContract.Pin, new GhEvidence(key, nameof(PinSide.Input)))));

    public Fin<IParameter> Bind(ModularOutputAdder adder, PinPlan plan, Op key) => Switch(
        bothCase: row => HostCall.Run(() => row.Output(adder, plan), key),
        inputCase: _ => Fin.Fail<IParameter>(new GhFault.ContractRefused(GhContract.Pin, new GhEvidence(key, nameof(PinSide.Output)))),
        outputCase: row => HostCall.Run(() => row.Value(adder, plan), key));
}

[SmartEnum<string>]
public sealed partial class PortRow {
    public static readonly PortRow Path = new("path", typeof(Grasshopper2.Data.Path), PortFamily.Standard, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddPath, a.AddHiddenPath), static (a, p) => p.Mint(a.AddPath, a.AddHiddenPath)));
    public static readonly PortRow Site = new("site", typeof(Grasshopper2.Data.Site), PortFamily.Standard, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddSite, a.AddHiddenSite), static (a, p) => p.Mint(a.AddSite, a.AddHiddenSite)));
    public static readonly PortRow Topological = new("topological", typeof(System.Guid), PortFamily.Topology,
        PortAxes.Identity.WithTrim<PinTrim.Connection>(Some(CapabilitySet<PinSide>.Of(PinSide.Input))),
        Input(static (a, p) => a.RegularAdder.AddTopological((string)p.Name, (string)p.Nick, p.Info)));
    public static readonly PortRow Colour = new("colour", typeof(Grasshopper2.Types.Colour.Colour), PortFamily.Standard, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddColour, a.AddHiddenColour), static (a, p) => p.Mint(a.AddColour, a.AddHiddenColour)));
    public static readonly PortRow Point = new("point", typeof(Rhino.Geometry.Point3d), PortFamily.Standard, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddPoint, a.AddHiddenPoint), static (a, p) => p.Mint(a.AddPoint, a.AddHiddenPoint)));
    public static readonly PortRow Vector = new("vector", typeof(Rhino.Geometry.Vector3d), PortFamily.Standard, PortAxes.Modular.WithTrim<PinTrim.Vector>(),
        Both(static (a, p) => p.Mint(a.AddVector, a.AddHiddenVector), static (a, p) => p.Mint(a.AddVector, a.AddHiddenVector)));
    public static readonly PortRow Line = new("line", typeof(Rhino.Geometry.Line), PortFamily.Standard, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddLine, a.AddHiddenLine), static (a, p) => p.Mint(a.AddLine, a.AddHiddenLine)));
    public static readonly PortRow Arc = new("arc", typeof(Rhino.Geometry.Arc), PortFamily.Standard, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddArc, a.AddHiddenArc), static (a, p) => p.Mint(a.AddArc, a.AddHiddenArc)));
    public static readonly PortRow Circle = new("circle", typeof(Rhino.Geometry.Circle), PortFamily.Standard, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddCircle, a.AddHiddenCircle), static (a, p) => p.Mint(a.AddCircle, a.AddHiddenCircle)));
    public static readonly PortRow Rectangle = new("rectangle", typeof(Rhino.Geometry.Rectangle3d), PortFamily.Standard, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddRectangle, a.AddHiddenRectangle), static (a, p) => p.Mint(a.AddRectangle, a.AddHiddenRectangle)));
    public static readonly PortRow Curve = new("curve", typeof(Rhino.Geometry.Curve), PortFamily.Standard, PortAxes.Modular.WithTrim<PinTrim.Curve>(),
        Both(static (a, p) => p.Mint(a.AddCurve, a.AddHiddenCurve), static (a, p) => p.Mint(a.AddCurve, a.AddHiddenCurve)));
    public static readonly PortRow Surface = new("surface", typeof(Rhino.Geometry.Surface), PortFamily.Standard, PortAxes.Modular.WithTrim<PinTrim.Surface>(),
        Both(static (a, p) => p.Mint(a.AddSurface, a.AddHiddenSurface), static (a, p) => p.Mint(a.AddSurface, a.AddHiddenSurface)));
    public static readonly PortRow Box = new("box", typeof(Rhino.Geometry.Box), PortFamily.Standard, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddBox, a.AddHiddenBox), static (a, p) => p.Mint(a.AddBox, a.AddHiddenBox)));
    public static readonly PortRow Cage = new("cage", typeof(Grasshopper2.Types.Shapes.Cage), PortFamily.Standard, PortAxes.Regular,
        Both(static (a, p) => p.Mint(a.RegularAdder.AddCage), static (a, p) => p.Mint(a.RegularAdder.AddCage)));
    public static readonly PortRow Sphere = new("sphere", typeof(Rhino.Geometry.Sphere), PortFamily.Standard, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddSphere, a.AddHiddenSphere), static (a, p) => p.Mint(a.AddSphere, a.AddHiddenSphere)));
    public static readonly PortRow Plane = new("plane", typeof(Rhino.Geometry.Plane), PortFamily.Standard, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddPlane, a.AddHiddenPlane), static (a, p) => p.Mint(a.AddPlane, a.AddHiddenPlane)));
    public static readonly PortRow Dot = new("dot", typeof(Rhino.Geometry.TextDot), PortFamily.Standard,
        new PortAxes(HashMap(
            (PortAxis.Access, CapabilitySet<PinSide>.Of(PinSide.Output)),
            (PortAxis.Appearance, CapabilitySet<PinSide>.Of(PinSide.Output)),
            (PortAxis.Hidden, CapabilitySet<PinSide>.Of(PinSide.Output))), None),
        Output(static (a, p) => p.Mint(a.AddDot, a.AddHiddenDot)));
    public static readonly PortRow Transform = new("transform", typeof(Rhino.Geometry.Transform), PortFamily.Standard, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddTransform, a.AddHiddenTransform), static (a, p) => p.Mint(a.AddTransform, a.AddHiddenTransform)));
    public static readonly PortRow View = new("view", typeof(Rhino.DocObjects.ViewportInfo), PortFamily.Standard,
        new PortAxes(HashMap(
            (PortAxis.Access, CapabilitySet<PinSide>.All),
            (PortAxis.Presence, CapabilitySet<PinSide>.Of(PinSide.Input)),
            (PortAxis.Appearance, CapabilitySet<PinSide>.Of(PinSide.Output)),
            (PortAxis.Hidden, CapabilitySet<PinSide>.Of(PinSide.Output))), None),
        Both(static (a, p) => p.Mint(a.RegularAdder.AddView), static (a, p) => p.Mint(a.AddView, a.AddHiddenView)));
    public static readonly PortRow Graph = new("graph", typeof(Grasshopper2.Types.Graphs.Graph), PortFamily.Standard, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddGraph, a.AddHiddenGraph), static (a, p) => p.Mint(a.AddGraph, a.AddHiddenGraph)));
    public static readonly PortRow Field = new("field", typeof(Grasshopper2.Types.Fields.Field), PortFamily.Standard, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddField, a.AddHiddenField), static (a, p) => p.Mint(a.AddField, a.AddHiddenField)));
    public static readonly PortRow Function = new("function", typeof(Grasshopper2.Types.Functions.Function), PortFamily.Standard, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddFunction, a.AddHiddenFunction), static (a, p) => p.Mint(a.AddFunction, a.AddHiddenFunction)));
    public static readonly PortRow Tuple = new("tuple", typeof(Grasshopper2.Types.NTuple), PortFamily.Standard, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddTuple, a.AddHiddenTuple), static (a, p) => p.Mint(a.AddTuple, a.AddHiddenTuple)));
    public static readonly PortRow Integer = new("integer", typeof(int), PortFamily.Standard,
        PortAxes.Modular.WithTrim<PinTrim.Integer>(Some(CapabilitySet<PinSide>.Of(PinSide.Input))),
        Both(static (a, p) => p.Mint(a.AddInteger, a.AddHiddenInteger), static (a, p) => p.Mint(a.AddInteger, a.AddHiddenInteger)));
    public static readonly PortRow Index = new("index", typeof(int), PortFamily.Index,
        PortAxes.Regular.WithTrim<PinTrim.Integer>(Some(CapabilitySet<PinSide>.Of(PinSide.Input))),
        Input(static (a, p) => p.Mint(a.RegularAdder.AddIndex)));
    public static readonly PortRow Interval = new("interval", typeof(Rhino.Geometry.Interval), PortFamily.Standard, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddInterval, a.AddHiddenInterval), static (a, p) => p.Mint(a.AddInterval, a.AddHiddenInterval)));
    public static readonly PortRow Angle = new("angle", typeof(Grasshopper2.Types.Numeric.Angle), PortFamily.Standard, PortAxes.Modular.WithTrim<PinTrim.Angle>(),
        Both(static (a, p) => p.Mint(a.AddAngle, a.AddHiddenAngle), static (a, p) => p.Mint(a.AddAngle, a.AddHiddenAngle)));
    public static readonly PortRow Number = new("number", typeof(double), PortFamily.Standard,
        PortAxes.Modular.WithTrim<PinTrim.Number>(Some(CapabilitySet<PinSide>.Of(PinSide.Input))),
        Both(static (a, p) => p.Mint(a.AddNumber, a.AddHiddenNumber), static (a, p) => p.Mint(a.AddNumber, a.AddHiddenNumber)));
    public static readonly PortRow Complex = new("complex", typeof(System.Numerics.Complex), PortFamily.Standard, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddComplex, a.AddHiddenComplex), static (a, p) => p.Mint(a.AddComplex, a.AddHiddenComplex)));
    public static readonly PortRow Numeric = new("numeric", typeof(object), PortFamily.Numeric, PortAxes.Modular.WithTrim<PinTrim.Numeric>(),
        Both(static (a, p) => p.Mint(a.AddNumeric, a.AddHiddenNumeric), static (a, p) => p.Mint(a.AddNumeric, a.AddHiddenNumeric)));
    public static readonly PortRow Guid = new("guid", typeof(System.Guid), PortFamily.Standard,
        new PortAxes(HashMap(
            (PortAxis.Access, CapabilitySet<PinSide>.Of(PinSide.Output)),
            (PortAxis.Appearance, CapabilitySet<PinSide>.Of(PinSide.Output)),
            (PortAxis.Hidden, CapabilitySet<PinSide>.Of(PinSide.Output))), None),
        Output(static (a, p) => p.Mint(a.AddGuid, a.AddHiddenGuid)));
    public static readonly PortRow Random = new("random", typeof(Grasshopper2.Types.Random.RandomEngine), PortFamily.Standard, PortAxes.Regular,
        Both(static (a, p) => p.Mint(a.RegularAdder.AddRandom), static (a, p) => p.Mint(a.RegularAdder.AddRandom)));
    public static readonly PortRow Continuous = new("continuous", typeof(Grasshopper2.Types.Random.ContinuousDistribution), PortFamily.Standard, PortAxes.Regular,
        Both(static (a, p) => p.Mint(a.RegularAdder.AddContinuous), static (a, p) => p.Mint(a.RegularAdder.AddContinuous)));
    public static readonly PortRow Discrete = new("discrete", typeof(Grasshopper2.Types.Random.DiscreteDistribution), PortFamily.Standard, PortAxes.Regular,
        Both(static (a, p) => p.Mint(a.RegularAdder.AddDiscrete), static (a, p) => p.Mint(a.RegularAdder.AddDiscrete)));
    public static readonly PortRow Boolean = new("boolean", typeof(bool), PortFamily.Standard, PortAxes.Modular.WithTrim<PinTrim.Boolean>(),
        Both(static (a, p) => p.Mint(a.AddBoolean, a.AddHiddenBoolean), static (a, p) => p.Mint(a.AddBoolean, a.AddHiddenBoolean)));
    public static readonly PortRow Text = new("text", typeof(string), PortFamily.Standard, PortAxes.Modular.WithTrim<PinTrim.Text>(),
        Both(static (a, p) => p.Mint(a.AddText, a.AddHiddenText), static (a, p) => p.Mint(a.AddText, a.AddHiddenText)));
    public static readonly PortRow TextPattern = new("text-pattern", typeof(string), PortFamily.Pattern,
        new PortAxes(HashMap(
            (PortAxis.Access, CapabilitySet<PinSide>.Of(PinSide.Input)),
            (PortAxis.Presence, CapabilitySet<PinSide>.Of(PinSide.Input)),
            (PortAxis.Appearance, CapabilitySet<PinSide>.Of(PinSide.Input)),
            (PortAxis.Hidden, CapabilitySet<PinSide>.Of(PinSide.Input))),
            Some((Type: typeof(PinTrim.TextPattern), Sides: CapabilitySet<PinSide>.Of(PinSide.Input)))),
        Input(static (a, p) => p.Mint(a.AddTextPattern, a.AddHiddenTextPattern)));
    public static readonly PortRow Gradient = new("gradient", typeof(Grasshopper2.Types.Colour.Gradient), PortFamily.Standard, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddGradient, a.AddHiddenGradient), static (a, p) => p.Mint(a.AddGradient, a.AddHiddenGradient)));
    public static readonly PortRow DateTime = new("date-time", typeof(System.DateTime), PortFamily.Standard, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddDateTime, a.AddHiddenDateTime), static (a, p) => p.Mint(a.AddDateTime, a.AddHiddenDateTime)));
    public static readonly PortRow TimeSpan = new("time-span", typeof(System.TimeSpan), PortFamily.Standard, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddTimeSpan, a.AddHiddenTimeSpan), static (a, p) => p.Mint(a.AddTimeSpan, a.AddHiddenTimeSpan)));
    public static readonly PortRow Language = new("language", typeof(Grasshopper2.Types.Linguistic.Language), PortFamily.Standard, PortAxes.Regular,
        Both(
            static (a, p) => p.Mint((name, nick, info, access, presence) => {
                Grasshopper2.Parameters.Standard.LanguageParameter parameter = new(name, nick, info, access);
                a.RegularAdder.Add(parameter, presence);
                return parameter;
            }),
            static (a, p) => p.Mint(a.RegularAdder.AddLanguage)));
    public static readonly PortRow MetaName = new("meta-name", typeof(Grasshopper2.Data.Meta.MetaName), PortFamily.Standard, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddMetaKey, a.AddHiddenMetaKey), static (a, p) => p.Mint(a.AddMetaKey, a.AddHiddenMetaKey)));
    public static readonly PortRow Meta = new("meta", typeof(MetaData), PortFamily.Standard, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddMetaData, a.AddHiddenMetaData), static (a, p) => p.Mint(a.AddMetaData, a.AddHiddenMetaData)));
    public static readonly PortRow Mesh = new("mesh", typeof(Rhino.Geometry.Mesh), PortFamily.Standard, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddMesh, a.AddHiddenMesh), static (a, p) => p.Mint(a.AddMesh, a.AddHiddenMesh)));
    public static readonly PortRow Polyline = new("polyline", typeof(Rhino.Geometry.Polyline), PortFamily.Standard, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddPolyline, a.AddHiddenPolyline), static (a, p) => p.Mint(a.AddPolyline, a.AddHiddenPolyline)));
    public static readonly PortRow Generic = new("generic", typeof(object), PortFamily.Generic, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddGeneric, a.AddHiddenGeneric), static (a, p) => p.Mint(a.AddGeneric, a.AddHiddenGeneric)));
    public static readonly PortRow Triangle = new("triangle", typeof(Grasshopper2.Types.Shapes.Triangle), PortFamily.Standard, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddTriangle, a.AddHiddenTriangle), static (a, p) => p.Mint(a.AddTriangle, a.AddHiddenTriangle)));
    public static readonly PortRow Tube = new("tube", typeof(Grasshopper2.Types.Shapes.Tube), PortFamily.Standard, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddTube, a.AddHiddenTube), static (a, p) => p.Mint(a.AddTube, a.AddHiddenTube)));
    public static readonly PortRow Region = new("region", typeof(Grasshopper2.Types.Shapes.Region), PortFamily.Standard, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddRegion, a.AddHiddenRegion), static (a, p) => p.Mint(a.AddRegion, a.AddHiddenRegion)));
    public static readonly PortRow CurveLocus = new("curve-locus", typeof(Grasshopper2.Types.Shapes.CurveLocus), PortFamily.Standard, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddCurveLocus, a.AddHiddenCurveLocus), static (a, p) => p.Mint(a.AddCurveLocus, a.AddHiddenCurveLocus)));
    public static readonly PortRow SurfaceLocus = new("surface-locus", typeof(Grasshopper2.Types.Shapes.SurfaceLocus), PortFamily.Standard, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddSurfaceLocus, a.AddHiddenSurfaceLocus), static (a, p) => p.Mint(a.AddSurfaceLocus, a.AddHiddenSurfaceLocus)));
    public static readonly PortRow MeshFacet = new("mesh-facet", typeof(Rhino.Geometry.MeshFace), PortFamily.Standard, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddMeshFacet, a.AddHiddenMeshFacet), static (a, p) => p.Mint(a.AddMeshFacet, a.AddHiddenMeshFacet)));
    public static readonly PortRow NPoint = new("n-point", typeof(Grasshopper2.Types.Coordinates.NPoint), PortFamily.Standard, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddNPoint, a.AddHiddenNPoint), static (a, p) => p.Mint(a.AddNPoint, a.AddHiddenNPoint)));
    public static readonly PortRow UvPoint = new("uv-point", typeof(Rhino.Geometry.Point2d), PortFamily.Standard, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddUvPoint, a.AddHiddenUvPoint), static (a, p) => p.Mint(a.AddUvPoint, a.AddHiddenUvPoint)));
    public static readonly PortRow Deform = new("deform", typeof(Grasshopper2.Types.Shapes.Deform), PortFamily.Standard, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddDeform, a.AddHiddenDeform), static (a, p) => p.Mint(a.AddDeform, a.AddHiddenDeform)));

    public Type Carrier { get; }

    public PortFamily Family { get; }

    public PortAxes Axes { get; }

    internal PortBinding Binding { get; }

    public CapabilitySet<PinSide> Sides => Binding.Sides;

    public static Seq<PortRow> Candidates(Type carrier, PortFamily family) =>
        toSeq(Items).Filter(row => row.Family == family && family.Accepts(row.Carrier, carrier)).Strict();

    public static Validation<Error, PortRow> Admit(Type carrier, PortFamily family, Op? key = null) {
        Seq<PortRow> candidates = Candidates(carrier, family);
        return candidates.Count switch {
            1 => Success<Error, PortRow>(candidates[0]),
            _ => Fail<Error, PortRow>(new GhFault.ContractRefused(GhContract.Pin, new GhEvidence(key.OrDefault(), $"{carrier.Name}:{family}:{candidates.Count}"))),
        };
    }

    public Fin<Unit> Accepts(PinPlan plan, PinSide side, Op key) =>
        (
            Side: Sides.Admits(side),
            Hidden: plan.Visibility == PinVisibility.Shown || Axes.Row(PortAxis.Hidden).Admits(side),
            Access: plan.Access == PinAccess.Item || Axes.Row(PortAxis.Access).Admits(side),
            Presence: plan.Presence == PinPresence.MustExist || Axes.Row(PortAxis.Presence).Admits(side),
            Appearance: plan.Category.IsNone && plan.Colour.IsNone || Axes.Row(PortAxis.Appearance).Admits(side),
            Trim: plan.Trim.ForAll(trim => AdmitsTrim(trim, side)),
            Persistent: plan.Persistent.ForAll(tree => Family.Accepts(Carrier, tree.Type))
        ) switch {
            { Side: false } => Fin.Fail<Unit>(new GhFault.ContractRefused(GhContract.Pin, new GhEvidence(key, $"{Key}:{side}"))),
            { Hidden: false } => Fin.Fail<Unit>(new GhFault.ContractRefused(GhContract.Pin, new GhEvidence(key, $"{Key}:{nameof(PinVisibility.Hidden)}:{side}"))),
            { Access: false } => Fin.Fail<Unit>(new GhFault.ContractRefused(GhContract.Pin, new GhEvidence(key, $"{Key}:{nameof(PinPlan.Access)}:{side}"))),
            { Presence: false } => Fin.Fail<Unit>(new GhFault.ContractRefused(GhContract.Pin, new GhEvidence(key, $"{Key}:{nameof(PinPlan.Presence)}:{side}"))),
            { Appearance: false } => Fin.Fail<Unit>(new GhFault.ContractRefused(GhContract.Pin, new GhEvidence(key, $"{Key}:{nameof(PinPlan.Category)}:{side}"))),
            { Trim: false } => Fin.Fail<Unit>(new GhFault.ContractRefused(GhContract.Pin, new GhEvidence(key, $"{Key}:{nameof(PinPlan.Trim)}:{side}"))),
            { Persistent: false } => Fin.Fail<Unit>(new GhFault.ContractRefused(GhContract.Pin, new GhEvidence(key, $"{Key}:{nameof(PinPlan.Persistent)}:{side}"))),
            _ => Fin.Succ(unit),
        };

    private bool AdmitsTrim(PinTrim trim, PinSide side) =>
        trim is { IsValid: true } && Axes.Trim.Map(axis => axis.Sides.Admits(side) && axis.Type.IsInstanceOfType(trim)
            && (this != Index || trim is not PinTrim.Integer { Posture: IndexPosture.Plain })
            && (this != Integer || trim is not PinTrim.Integer { Posture: IndexPosture.Indexed })).IfNone(false);

    private static PortBinding Both(
        Func<ModularInputAdder, PinPlan, IParameter> input,
        Func<ModularOutputAdder, PinPlan, IParameter> output) => new PortBinding.BothCase(input, output);

    private static PortBinding Input(Func<ModularInputAdder, PinPlan, IParameter> input) => new PortBinding.InputCase(input);

    private static PortBinding Output(Func<ModularOutputAdder, PinPlan, IParameter> output) => new PortBinding.OutputCase(output);
}
```

## [05]-[DECLARATION_FOLD]

- Owner: `Ports` is the one declaration fold — side selection is the adder argument's static type, every plan folds through the accumulating carrier so a malformed pin roster reports every violation at once, and each minted `IParameter` realizes its trim and persistent data on the same fault rail.
- Entry: `Declare` admits every row policy, invokes the binding union, and realizes the minted parameter; `Realize` re-applies the trim and persistent tree through `ComponentParameters.Input(int)` and `Output(int)`; the `DeclareEnum<T>` input/output pair admits only non-flags, `Int32`-backed enums and retains the integer row's applicable policy.
- Law: declaration returns `Validation<Error, Seq<IParameter>>` carrying row-policy, host, trim, and persistence failures.
- Growth: a new maintenance projection is one fold over the returned parameter seq; enum pins remain integer carriers, while `T` supplies presets and the input seed.
- Boundary: a rejected policy never reaches the host; presets and assistants are observed through their get-only host contracts rather than projected as plan setters.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using Grasshopper2.Components;
using Grasshopper2.Parameters;
using Rasm.Domain;

namespace Rasm.Grasshopper.Components;

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class Ports {
    public static Validation<Error, Seq<IParameter>> Declare(ModularInputAdder adder, Seq<PinPlan> plans, Op? key = null) {
        Op op = key.OrDefault();
        return plans.Traverse(plan => Minted(plan, PinSide.Input, () => plan.Kind.Binding.Bind(adder, plan, op), op).ToValidation()).As();
    }

    public static Validation<Error, Seq<IParameter>> Declare(ModularOutputAdder adder, Seq<PinPlan> plans, Op? key = null) {
        Op op = key.OrDefault();
        return plans.Traverse(plan => Minted(plan, PinSide.Output, () => plan.Kind.Binding.Bind(adder, plan, op), op).ToValidation()).As();
    }

    public static Fin<IParameter> DeclareEnum<T>(ModularInputAdder adder, PinPlan plan, T seed, Op? key = null) where T : struct, Enum {
        Op op = key.OrDefault();
        string category = plan.Category.IfNone("");
        Eto.Drawing.Color colour = plan.Colour.IfNone(Eto.Drawing.Colors.Transparent);
        return EnumType<T>(op).Bind(_ => plan.Kind == PortRow.Integer
            ? plan.Kind.Accepts(plan, PinSide.Input, op).Bind(_ => HostCall.Run<IParameter>(() => plan.Visibility == PinVisibility.Shown
                    ? adder.AddEnum((string)plan.Name, (string)plan.Nick, plan.Info, category, colour, seed, plan.Access.Host, plan.Presence.Host)
                    : adder.AddHiddenEnum((string)plan.Name, (string)plan.Nick, plan.Info, category, colour, seed, plan.Access.Host, plan.Presence.Host), op))
                .Bind(parameter => plan.Realize(parameter, op).Map(_ => parameter))
            : Fin.Fail<IParameter>(new GhFault.ContractRefused(GhContract.Pin, new GhEvidence(op, $"{plan.Kind.Key}:{nameof(DeclareEnum)}"))));
    }

    public static Fin<IParameter> DeclareEnum<T>(ModularOutputAdder adder, PinPlan plan, Op? key = null) where T : struct, Enum {
        Op op = key.OrDefault();
        return EnumType<T>(op)
            .Bind(_ => AcceptsOutputEnum(plan, op))
            .Bind(_ => HostCall.Run<IParameter>(() => adder.RegularAdder.AddEnum<T>(
                (string)plan.Name, (string)plan.Nick, plan.Info, plan.Access.Host), op))
            .Bind(parameter => plan.Realize(parameter, op).Map(_ => parameter));
    }

    public static Validation<Error, Unit> Realize(ComponentParameters parameters, Seq<PinPlan> inputs, Seq<PinPlan> outputs, Op? key = null) {
        Op op = key.OrDefault();
        return (inputs.Map(static (plan, index) => (Plan: plan, Index: index))
                .Traverse(row => HostCall.Run<IParameter>(() => parameters.Input(row.Index), op)
                    .Bind(parameter => row.Plan.Realize(parameter, op)).ToValidation()).As(),
            outputs.Map(static (plan, index) => (Plan: plan, Index: index))
                .Traverse(row => HostCall.Run<IParameter>(() => parameters.Output(row.Index), op)
                    .Bind(parameter => row.Plan.Realize(parameter, op)).ToValidation()).As())
            .Apply(static (_, _) => unit)
            .As();
    }

    private static Fin<IParameter> Minted(PinPlan plan, PinSide side, Func<Fin<IParameter>> bind, Op key) =>
        plan.Kind.Accepts(plan, side, key)
            .Bind(_ => bind())
            .Bind(parameter => plan.Realize(parameter, key).Map(_ => parameter));

    private static Fin<Unit> EnumType<T>(Op key) where T : struct, Enum =>
        Enum.GetUnderlyingType(typeof(T)) == typeof(int) && !typeof(T).IsDefined(typeof(FlagsAttribute), inherit: false)
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new GhFault.ContractRefused(GhContract.Pin, new GhEvidence(key, $"{typeof(T).Name}:{nameof(DeclareEnum)}")));

    private static Fin<Unit> AcceptsOutputEnum(PinPlan plan, Op key) =>
        plan.Kind == PortRow.Integer &&
        plan.Visibility == PinVisibility.Shown &&
        plan.Presence == PinPresence.MustExist &&
        plan.Category.IsNone &&
        plan.Colour.IsNone &&
        plan.Trim.IsNone
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new GhFault.ContractRefused(GhContract.Pin, new GhEvidence(key, $"{plan.Kind.Key}:{nameof(DeclareEnum)}:{nameof(PinSide.Output)}")));
}
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
