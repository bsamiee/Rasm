# [RASM_RHINO_PLUGIN_LICENSING]

`Licenses.Ask` folds the whole Zoo and CloudZoo entitlement surface into one request union, and `RasmPlugIn.Entitlement` owns the plug-in-side acquisition pipeline the host exposes as `protected` members. Both answer typed verdicts over detached evidence, so a live `LicenseData`, `LicenseStatus`, or `LicenseLease` never escapes the arm that read it and no consumer learns the host's out-parameter and delegate shapes.

The acquisition arm continues `lifecycle#ADAPTER`'s `RasmPlugIn` as the same partial class: `GetLicense`, `AskUserForLicense`, `ReturnLicense`, `GetLicenseOwner`, and `SetLicenseCapabilities` are `protected` on `PlugIn`, so only a derivation reaches them and a free-standing service could not compile. That co-location also gives every arm the adapter's own `Record` sink, so a refusal raised inside a host `out`-delegate lands on the bounded refusal ring `lifecycle#DIAGNOSTICS` publishes rather than vanishing at a `void` boundary. `LicenseEvidence` admits exactly the fields `LicenseData.IsValid` proves, so an evidence value that exists is one the host accepts. Entitlement probes over `CloudHostUtils` stay at `HostUi/shell#RUNTIME` under `HostProbe.Entitlement`; this page mints no second probe.

Every capability word rides the kernel `CapabilitySet<T>`: the host's `LicenseCapabilities` bitfield admits through the one `OfMask` boundary arm and leaves through the one `Mask` fold at the two host members that take it, so the throwing `LicenseUtils.GetLicenseCapabilities(int)` path never enters the pipeline and no second grant carrier exists.

## [01]-[INDEX]

- [02]-[VOCABULARY]: `ProductKey`, `LicenseGrant`, `LicenseBuild`, `LicenseKind`, `LicensePosture`, and `LicenseNode` close every entitlement axis as keyed rows.
- [03]-[EVIDENCE]: `LicenseEvidence`, `CloudZooLease`, `LicenseState`, `LeaseEvidence`, `LeaseMap`, and `OwnerEvidence` detach the four host license payloads.
- [04]-[ACQUISITION]: `ILicenseProgram`, `LicenseReply`, `Entitle`, `EntitlementAsk`, `EntitleOutcome`, and the `RasmPlugIn` entitlement arm own plug-in-side acquisition.
- [05]-[PIPELINE]: `LicenseVerb`, `SessionVerb`, `LicenseAccount`, `LicenseAsk`, `LicenseVerdict`, and `Licenses.Ask` own the `LicenseUtils` census, checkout family, and CloudZoo session.
- [06]-[FACTS]: `LicenseFact` and `LicensePulse.Observe` detach the host license-state change.
- [07]-[SURFACE_LEDGER]: owner-to-ingress-to-state-to-egress roster across the two entries, the program floor, and the evidence carriers.

## [02]-[VOCABULARY]

- Owner: `ProductKey` is the license product and license identity — a different concept from `PluginKey`, which keys the plug-in itself, so the two never substitute; it admits through the typed-fault receiver, so its refusal is the boundary's own `PluginFault` with no translation hop.
- Owner: `LicenseGrant` `[SmartEnum<LicenseCapabilities>]` realizes `ICapability<LicenseGrant>` and a grant SET is the kernel `CapabilitySet<LicenseGrant>`; the host flag IS the capability, its member name is the canonical text, and its bit is projected only where the host mask is required.
- Law: a persisted or host-published capability BITFIELD admits through `CapabilitySet<LicenseGrant>.OfMask` — the declared-bit fold and its out-of-range refusal are the kernel's, so `LicenseUtils.GetLicenseCapabilities(int)`, which projects bit-for-bit onto the same ordinals and THROWS above `0x1FF`, never enters the pipeline (`.api/api-rhinocommon-plugins.md:115`).
- Law: the host's `NoCapabilities` row DELETES. It is the zero word, and the empty set already round-trips to it through `Mask`, so a row whose bit contributes nothing and whose only reader was an empty-selection fallback was the vocabulary answering a question the set already answers. NAMED LOSS: the explicit `LicenseGrant.None` spelling — bought back by `CapabilitySet<LicenseGrant>.None`, whose `Mask` is `LicenseCapabilities.NoCapabilities` exactly.
- Owner: `LicensePosture` carries the two evidence postures `LicenseData` publishes, each row reading its own host slot, so the pair projects back to two booleans at the ONE `new LicenseData(…)` mint and nowhere else.
- Law: `LicenseNode` replaces the host's `standAlone` boolean and converts implicitly to its key, so a prompt request names the node type and no call site spells `.Key` to erase the row back.
- Boundary: `LicenseBuild` and `LicenseKind` mirror their host enums completely, so `FactoryBridge.Row` over a host read is total and no arm invents a default row.
- Packages: Thinktecture.Runtime.Extensions (`libs/dotnet/.api/api-thinktecture-runtime-extensions.md` — `[ValueObject<Guid>]`, `[SmartEnum<THostEnum>]`, `[SmartEnum<bool>]` with `ConversionToKeyMemberType = Implicit`, `[UseDelegateFromConstructor]`, `[ValidationError]`); LanguageExt.Core (`api-languageext.md` — `Fin`, `Option`); kernel `Domain/validation` (`ICapability`, `CapabilitySet.OfMask`/`.Mask`, `FactoryBridge.Accept`), `Domain/results` ; RhinoCommon plug-ins (`Rasm.Rhino/.api/api-rhinocommon-plugins.md:54` — `LicenseCapabilities` ordinal order and the `0x1FF` union, `LicenseBuildType`, `LicenseType`; `:115` — `GetLicenseCapabilities(int)` and its throw).

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Eto.Forms;
using Rasm.Domain;
using Rasm.Interaction;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.PlugIns;
using Rhino.Runtime;
using Riok.Mapperly.Abstractions;
using GdiIcon = System.Drawing.Icon;

namespace Rasm.Rhino.Plugin;

// --- [TYPES] ---------------------------------------------------------------------------
[ValueObject<Guid>]
[ValidationError]
public readonly partial struct ProductKey {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Guid value) =>
        validationError = value == Guid.Empty ? new ValidationError(string.Join(" | ", new object?[] { nameof(ProductKey) })) : null;

    internal static Option<ProductKey> Maybe(Guid value) =>
        Optional(value).Filter(static id => id != Guid.Empty).Map(Create);

    internal Fin<Unit> Admit() =>
        FactoryBridge.Accept<ProductKey>(candidate: ToValue()).Map(static _ => unit);
}

[SmartEnum<LicenseCapabilities>]
public sealed partial class LicenseGrant : ICapability<LicenseGrant> {
    public static readonly LicenseGrant Purchasable = new(key: LicenseCapabilities.CanBePurchased);
    public static readonly LicenseGrant Specifiable = new(key: LicenseCapabilities.CanBeSpecified);
    public static readonly LicenseGrant Evaluable = new(key: LicenseCapabilities.CanBeEvaluated);
    public static readonly LicenseGrant EvaluationExpired = new(key: LicenseCapabilities.EvaluationIsExpired);
    public static readonly LicenseGrant RhinoAccounts = new(key: LicenseCapabilities.SupportsRhinoAccounts);
    public static readonly LicenseGrant Standalone = new(key: LicenseCapabilities.SupportsStandalone);
    public static readonly LicenseGrant ZooPerUser = new(key: LicenseCapabilities.SupportsZooPerUser);
    public static readonly LicenseGrant ZooPerCore = new(key: LicenseCapabilities.SupportsZooPerCore);
    public static readonly LicenseGrant Discovery = new(key: LicenseCapabilities.SupportsLicenseDiscovery);

    string ICapability<LicenseGrant>.Key => Key.ToString();

    internal static int Bit(LicenseGrant row) => (int)row.Key;
}

[SmartEnum<LicenseBuildType>]
public sealed partial class LicenseBuild {
    public static readonly LicenseBuild Unspecified = new(key: LicenseBuildType.Unspecified);
    public static readonly LicenseBuild Release = new(key: LicenseBuildType.Release);
    public static readonly LicenseBuild Evaluation = new(key: LicenseBuildType.Evaluation);
    public static readonly LicenseBuild Beta = new(key: LicenseBuildType.Beta);
}

[SmartEnum<LicenseType>]
public sealed partial class LicenseKind {
    public static readonly LicenseKind Standalone = new(key: LicenseType.Standalone);
    public static readonly LicenseKind Network = new(key: LicenseType.Network);
    public static readonly LicenseKind NetworkLoanedOut = new(key: LicenseType.NetworkLoanedOut);
    public static readonly LicenseKind NetworkCheckedOut = new(key: LicenseType.NetworkCheckedOut);
    public static readonly LicenseKind CloudZoo = new(key: LicenseType.CloudZoo);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LicensePosture : ICapability<LicensePosture> {
    public static readonly LicensePosture OnlineValidation = new(
        key: "online-validation", holds: static (online, _) => online);
    public static readonly LicensePosture Upgrade = new(
        key: "upgrade", holds: static (_, upgrade) => upgrade);

    [UseDelegateFromConstructor] internal partial bool Holds(bool online, bool upgrade);
}

[SmartEnum<bool>(ConversionToKeyMemberType = ConversionOperatorsGeneration.Implicit)]
public sealed partial class LicenseNode {
    public static readonly LicenseNode Network = new(false);
    public static readonly LicenseNode Standalone = new(true);
}
```

## [03]-[EVIDENCE]

- Owner: `LicenseEvidence` is the ONE license payload carrier and crosses in both directions — the boundary detaches a host `LicenseData` into it and mints a host `LicenseData` from it inside the validate callback, so forward and inverse of one correspondence share one owner.
- Law: admission mirrors `LicenseData.IsValid` exactly — non-blank product license, serial, and title, a defined build row, and a positive count — so an admitted evidence value is one the host's own validity read accepts, and the refusal is this boundary's typed fault with no translation hop.
- Owner: `LicenseState` detaches `LicenseStatus`; `LeaseEvidence` detaches `LicenseLease` through `LeaseMap`; `OwnerEvidence` detaches the two `GetLicenseOwner` out-parameters.
- Law: the CloudZoo lease is `Option<CloudZooLease>`, not a valid-flag beside an expiry — the host publishes both off one lease record and a false flag left the expiry slot dead, which is a corner the option makes unrepresentable.
- Law: every host string crosses through `HostEdge.Text`, so a null and an empty registry field are one absence; the eleven fabricated empty strings this page carried were forged values the reader could not tell from a genuine blank the server sent.
- Law: `LeaseEvidence` is a GENERATED projection — twelve mechanical member renames under one null policy is exactly the correspondence `[Mapper]` owns, and the two user mappings (`string?` to `Option<string>`, `DateTime?` to `Option<DateTime>`) are the whole policy the generator needs.
- Law: `LicenseLeaseChangedEventArgs.Lease` mints a FRESH managed wrapper over a borrowed native pointer on every read, so the lease detaches inside the callback and no wrapper is retained past it.
- Boundary: `LicenseData.ProductIcon` and `LicenseStatus.ProductIcon` are disposable native handles, so neither rides a detached record; the lease-changed hook answers its badge through the kernel raster owner and the boundary hands the host an icon it mints at that one boundary.
- Packages: Riok.Mapperly (`libs/dotnet/.api/api-mapperly.md` — `[Mapper]`, `[UserMapping]`); Thinktecture.Runtime.Extensions (`[ComplexValueObject]`, `[ValidationError]`); LanguageExt.Core (`Fin`, `Option`, `Seq`); kernel `Domain/results` (`HostEdge.Text`, `Admit.Need`, `Try.lift`, `HostEdge.Nullable`, `ValidityClaim`), `Domain/validation` (`FactoryBridge.Row`, `CapabilitySet`); `Document/events` (`PluginKey`); RhinoCommon plug-ins (`.api/api-rhinocommon-plugins.md` — `LicenseData` constructor and reads, `LicenseStatus`, `LicenseLease`).

```csharp
// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
[ValidationError]
public sealed partial class LicenseEvidence {
    public string ProductLicense { get; }
    public string SerialNumber { get; }
    public string LicenseTitle { get; }
    public LicenseBuild Build { get; }
    public int Count { get; }
    public Option<DateTime> Expires { get; }
    public CapabilitySet<LicensePosture> Posture { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref string productLicense,
        ref string serialNumber,
        ref string licenseTitle,
        ref LicenseBuild build,
        ref int count,
        ref Option<DateTime> expires,
        ref CapabilitySet<LicensePosture> posture) =>
        validationError = ValidityClaim.All(
            !string.IsNullOrWhiteSpace(productLicense),
            !string.IsNullOrWhiteSpace(serialNumber),
            !string.IsNullOrWhiteSpace(licenseTitle),
            build is not null,
            count >= 1)
            ? validationError
            : new ValidationError(string.Join(" | ", new object?[] { nameof(LicenseEvidence), "a non-blank product licence, serial, and title, a defined build row, and a positive count" }));

    internal Fin<LicenseData> Mint() => Try.lift(() => Fin.Succ(value: new LicenseData(
        productLicense: ProductLicense,
        serialNumber: SerialNumber,
        licenseTitle: LicenseTitle,
        buildType: Build.Key,
        licenseCount: Count,
        expirationDate: HostEdge.Nullable(Expires),
        productIcon: null,
        requiresOnlineValidation: Posture.Admits(LicensePosture.OnlineValidation),
        isUpgradeFromPreviousVersion: Posture.Admits(LicensePosture.Upgrade)))).Run().Bind(static inner => inner);

    internal static Fin<LicenseEvidence> Detach(LicenseData data) =>
        from row in Admit.Need(data)
        from build in FactoryBridge.Row<LicenseBuildType, LicenseBuild>(row.BuildType)
        from evidence in FactoryBridge.Accept<LicenseEvidence>(
            fault: Validate(
                row.ProductLicense,
                row.SerialNumber,
                row.LicenseTitle,
                build,
                row.LicenseCount,
                Optional(row.DateToExpire),
                CapabilitySet<LicensePosture>.Of(toSeq(LicensePosture.Items)
                    .Filter(posture => posture.Holds(
                        online: row.RequiresOnlineValidation, upgrade: row.IsUpgradeFromPreviousVersion))
                    .ToArray()),
                out LicenseEvidence? candidate),
            admitted: candidate)
        select evidence;
}

public sealed record CloudZooLease(Option<DateTime> Expires);

public sealed record LicenseState(
    Option<PluginKey> Plugin,
    Option<ProductKey> Product,
    LicenseKind Kind,
    LicenseBuild Build,
    Option<string> Title,
    Option<string> SerialNumber,
    Option<DateTime> Expires,
    Option<DateTime> CheckOutExpires,
    Option<string> RegisteredOwner,
    Option<string> RegisteredOrganization,
    Option<CloudZooLease> Lease);

public sealed record LeaseEvidence(
    Option<string> LeaseId,
    Option<string> ProductId,
    Option<string> ProductTitle,
    Option<string> ProductVersion,
    Option<string> ProductEdition,
    Option<string> GroupId,
    Option<string> GroupName,
    Option<string> UserId,
    Option<string> UserName,
    DateTime IssuedAt,
    DateTime Expiration,
    Option<DateTime> RenewableUntil);

public sealed record OwnerEvidence(Option<string> Owner, Option<string> Organization);

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class LeaseMap {
    internal static partial LeaseEvidence Detach(LicenseLease source);

    [UserMapping] private static Option<string> Text(string? value) => HostEdge.Text(value);

    [UserMapping] private static Option<DateTime> Moment(DateTime? value) => Optional(value);
}
```

## [04]-[ACQUISITION]

- Owner: `ILicenseProgram` is the plug-in's acquisition declaration behind an instance-interface floor — key validation answering a `LicenseReply`, and lease change answering an optional badge raster. The implementation is FOREIGN code, so the floor is the type and the null guard the delegate pair needed has no spelling left (`surfaces-and-dispatch.md [OPEN_FLOOR_DISPATCH]`, folder-wide with `document#PROGRAM`'s `IParticipant`).
- Owner: `Entitle` closes the plug-in-side acquisition family; `EntitleOutcome` carries its outcomes and `Held` names the `EntitlementAsk` row it answers, so a caller holding the outcome knows which of the three acquisitions succeeded (folder RULINGS `[02]`).
- Law: `RasmPlugIn` continues as a partial class here because the host declares its entitlement members `protected`; the arm is co-located with the derivation the language forces it onto, never lifted to a free-standing service that could not reach them. That co-location also gives every arm `lifecycle#ADAPTER`'s `Record` sink, so a refusal raised inside a host `out`-delegate reaches the bounded refusal ring instead of dying at the boundary.
- Law: the validate callback is total by construction — a refused result, a faulted program, or a malformed reply all settle on `ErrorHideMessage` with an empty payload, because the host reads the `out` slot unconditionally and a null there faults inside native code.
- Law: `ByBuild` is the capability-free overload the host supplies — it passes `NoCapabilities` and a null mask internally — so a request naming both a build and a grant set uses `ByCapability` and the two never merge.
- Law: `HostEdge.Slot` is the ONE null spelling at every host slot this arm writes — the text mask, the parent window, the expiry, and the badge icon — so five hand null-forgiving projections collapse to the kernel's one boundary arm.
- Boundary: the three interactive host calls cross the kernel dispatch on the MODAL lane — `UiThread.Run(new UiDispatch<T>.Blocking(…), DispatchLane.Modal, key)` — because a licence dialog owns the host's input for its whole life; every other arm is a non-interactive host call made on the caller's thread.
- Boundary: the prompt arm admits an optional Eto parent — decompile-proven through the shipped Zoo client, `ZooClientParameters.ParentWindow` lands on `ValidationClient.UI.Models.LicenseUiModel.ParentWindow : Eto.Forms.Control` and feeds `MessageBox.Show`/dialog parenting directly, so the accepted concrete shape is an Eto `Control` and absence is the parentless floor the arm writes as null.
- Boundary: the badge crosses as a kernel `AssetRaster` and the host's `out System.Drawing.Icon` slot is filled at this boundary alone — `GetHicon` hands the host an unmanaged icon handle whose lifetime the host then owns, while the leased bitmap disposes with its lease, so the conversion happens once here and no consumer holds a host icon.
- Packages: Thinktecture.Runtime.Extensions (`[Union]`, `[SmartEnum<string>]`); LanguageExt.Core (`Fin`, `Option`); kernel `Domain/results` (`Admit.Need`, `Try.lift`, `Acceptance.Text`, `HostEdge.Slot`), `Domain/validation` (`CapabilitySet.Mask`), `Interaction/dispatch` (`UiThread.Run`, `UiDispatch<T>.Blocking`, `DispatchLane.Modal`), `Interaction/asset` (`AssetRaster`), `Interaction/paint` (the GDI bitmap lease the badge carries); Eto.Forms (`Control` — `.api/api-eto-forms.md`); RhinoCommon plug-ins (`.api/api-rhinocommon-plugins.md:113` — `GetLicense` both overloads, `AskUserForLicense`, `ReturnLicense`, `GetLicenseOwner`, `SetLicenseCapabilities`, `ValidateProductKeyDelegate`, `OnLeaseChangedDelegate`).

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LicenseReply {
    private LicenseReply() { }
    public sealed record Accepted(LicenseEvidence Evidence) : LicenseReply;
    public sealed record RefusedLoudly(string Message) : LicenseReply;
    public sealed record RefusedQuietly : LicenseReply;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class EntitlementAsk {
    public static readonly EntitlementAsk Capability = new("capability");
    public static readonly EntitlementAsk Build = new("build");
    public static readonly EntitlementAsk Prompt = new("prompt");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Entitle {
    private Entitle() { }
    public sealed record ByCapability(
        ILicenseProgram Program, CapabilitySet<LicenseGrant> Grants, Option<string> TextMask) : Entitle;
    public sealed record ByBuild(ILicenseProgram Program, LicenseBuild Build) : Entitle;
    public sealed record ByPrompt(
        ILicenseProgram Program,
        LicenseBuild Build,
        LicenseNode Node,
        Option<string> TextMask,
        Option<Control> Parent = default) : Entitle;
    public sealed record Release : Entitle;
    public sealed record Declare(
        CapabilitySet<LicenseGrant> Grants, Option<string> TextMask, ProductKey License) : Entitle;
    public sealed record Owner : Entitle;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record EntitleOutcome {
    private EntitleOutcome() { }
    public sealed record Held(EntitlementAsk Asked) : EntitleOutcome;
    public sealed record Released : EntitleOutcome;
    public sealed record Declared(CapabilitySet<LicenseGrant> Grants) : EntitleOutcome;
    public sealed record Registered(OwnerEvidence Evidence) : EntitleOutcome;
}

// --- [SERVICES] ------------------------------------------------------------------------
public interface ILicenseProgram {
    Fin<LicenseReply> Validate(string productKey);
    Fin<Option<AssetRaster>> LeaseChanged(Option<LeaseEvidence> lease);
}

public abstract partial class RasmPlugIn {
    public Fin<EntitleOutcome> Entitlement(Entitle ask) {
        return Admit.Need(ask).Bind(request => request.Switch(byCapability: (held, row) => Admit.Need(row.Program).Bind(program => Try.lift(() => GetLicense(
                    licenseCapabilities: (LicenseCapabilities)row.Grants.Mask(bit: LicenseGrant.Bit),
                    textMask: HostEdge.Slot(row.TextMask)!,
                    validateProductKeyDelegate: Validator(program, held),
                    leaseChanged: LeaseWatcher(program, held))
                ? Fin.Succ<EntitleOutcome>(value: new EntitleOutcome.Held(Asked: EntitlementAsk.Capability))
                : Fin.Fail<EntitleOutcome>(error: new PluginFault.Dismissed(
                    Key: held, Member: nameof(GetLicense)))).Run().Bind(static inner => inner)),
            byBuild: (held, row) => Admit.Need(row.Program).Bind(program => Try.lift(() => GetLicense(
                    productBuildType: row.Build.Key,
                    validateProductKeyDelegate: Validator(program, held),
                    leaseChangedDelegate: LeaseWatcher(program, held))
                ? Fin.Succ<EntitleOutcome>(value: new EntitleOutcome.Held(Asked: EntitlementAsk.Build))
                : Fin.Fail<EntitleOutcome>(error: new PluginFault.Dismissed(
                    Key: held, Member: nameof(GetLicense)))).Run().Bind(static inner => inner)),
            byPrompt: (held, row) => Admit.Need(row.Program).Bind(program => UiThread.Run(
                new UiDispatch<EntitleOutcome>.Blocking(() => Try.lift(() => AskUserForLicense(
                        productBuildType: row.Build.Key,
                        standAlone: row.Node,
                        textMask: HostEdge.Slot(row.TextMask)!,
                        parentWindow: HostEdge.Slot(row.Parent),
                        validateProductKeyDelegate: Validator(program, held),
                        onLeaseChangedDelegate: LeaseWatcher(program, held))
                    ? Fin.Succ<EntitleOutcome>(value: new EntitleOutcome.Held(Asked: EntitlementAsk.Prompt))
                    : Fin.Fail<EntitleOutcome>(error: new PluginFault.Dismissed(
                        Key: held, Member: nameof(AskUserForLicense)))).Run().Bind(static inner => inner)),
                DispatchLane.Modal,
                held)),
            release: (held, _) => Try.lift(() => ReturnLicense()
                ? Fin.Succ<EntitleOutcome>(value: new EntitleOutcome.Released())
                : Fin.Fail<EntitleOutcome>(error: new PluginFault.HostRefused(
                    Key: held, Member: nameof(ReturnLicense), Detail: nameof(EntitleOutcome.Released)))).Run().Bind(static inner => inner),
            declare: (held, row) => row.License.Admit(held)
                .Bind(_ => Try.lift(() => SetLicenseCapabilities(
                    textMask: HostEdge.Slot(row.TextMask)!,
                    capabilities: (LicenseCapabilities)row.Grants.Mask(bit: LicenseGrant.Bit),
                    licenseId: row.License.ToValue())).Run().Bind(static inner => inner))
                .Map<EntitleOutcome>(_ => new EntitleOutcome.Declared(Grants: row.Grants)),
            owner: (held, _) => Try.lift(() => Admit.Probe(() => {
                    bool answered = GetLicenseOwner(
                        registeredOwner: out string owner, registeredOrganization: out string organization);
                    return (Ok: answered, Value: new OwnerEvidence(
                        Owner: HostEdge.Text(owner), Organization: HostEdge.Text(organization)));
                })
                .ToFin(Fail: new PluginFault.HostRefused(
                    Key: held, Member: nameof(GetLicenseOwner), Detail: nameof(OwnerEvidence)))
                .Map<EntitleOutcome>(static evidence => new EntitleOutcome.Registered(Evidence: evidence))).Run().Bind(static inner => inner)));
    }

    private ValidateProductKeyDelegate Validator(ILicenseProgram program) =>
        (string productKey, out LicenseData licenseData) => {
            Fin<(ValidateResult Verdict, LicenseData Data)> settled = Record(outcome:
                from key in Acceptance.Text(value: productKey)
                from reply in Try.lift(() => program.Validate(productKey: key)).Run().Bind(static inner => inner)
                from projected in reply.Switch(accepted: static (held, row) => row.Evidence.Mint(held)
                        .Map(static data => (Verdict: ValidateResult.Success, Data: data)),
                    refusedLoudly: static (held, row) => Acceptance.Text(value: row.Message)
                        .Map(static message => (
                            Verdict: ValidateResult.ErrorShowMessage,
                            Data: new LicenseData { ErrorMessage = message })),
                    refusedQuietly: static (_, _) => Fin.Succ((
                        Verdict: ValidateResult.ErrorHideMessage,
                        Data: new LicenseData())))
                select projected);
            (ValidateResult verdict, LicenseData data) = settled.Match(
                Succ: static row => (row.Verdict, row.Data),
                Fail: static _ => (ValidateResult.ErrorHideMessage, new LicenseData()));
            licenseData = data;
            return verdict;
        };

    private OnLeaseChangedDelegate LeaseWatcher(ILicenseProgram program) =>
        (LicenseLeaseChangedEventArgs args, out GdiIcon icon) => {
            Option<GdiIcon> badge = Record(outcome:
                from row in Admit.Need(args)
                from lease in Try.lift(() => Fin.Succ(value: Optional(row.Lease))).Run().Bind(static inner => inner)
                from evidence in Try.lift(() => Fin.Succ(value: lease.Map(LeaseMap.Detach))).Run().Bind(static inner => inner)
                from answer in Try.lift(() => program.LeaseChanged(lease: evidence)).Run().Bind(static inner => inner)
                from badged in answer.TraverseM(raster => Badge(raster, op)).As()
                select badged)
                .Match(Succ: static value => value, Fail: static _ => Option<GdiIcon>.None);
            icon = HostEdge.Slot(badge)!;
        };

    private static Fin<GdiIcon> Badge(AssetRaster raster) => raster.Switch(toolkit: static (held, _) => Fin.Fail<GdiIcon>(error: new KernelFault.InvalidValue(nameof(OnLeaseChangedDelegate), "a GDI raster the host icon slot accepts")),
        gdi: static (held, row) => Try.lift(() =>
            Fin.Succ(value: row.Bitmap.Use(static bitmap => GdiIcon.FromHandle(handle: bitmap.GetHicon())))).Run().Bind(static inner => inner),
        pixels: static (held, _) => Fin.Fail<GdiIcon>(error: new KernelFault.InvalidValue(nameof(OnLeaseChangedDelegate), "a GDI raster the host icon slot accepts")));
}
```

## [05]-[PIPELINE]

- Owner: `LicenseAsk` closes the `LicenseUtils` surface — status census, the checkout family, CloudZoo session, the account read, and the two interactive dialogs.
- Law: the checkout family is ONE case over a five-row verb table, not five cases over five identical arms — the verb is the row's own delegate column, so a sixth Zoo verb is one line and the pipeline, the admission, and the verdict are untouched. NAMED LOSS: the five case names — bought back as five row keys `nameof` still supplies; WITNESS: the old `Toggled` private, which the collapse deletes whole.
- Law: a verdict names the request it answers — `Acted` carries its `LicenseVerb` and `SessionSettled` its `SessionVerb`, so the seven verbs that once shared one nameless `Flag(bool)` are each addressable from the answer alone (folder RULINGS `[02]`).
- Law: the account read is a CASE SPLIT, not a flag beside a name — a signed-out session has no user name, and `LicenseAccount` makes the pair that could disagree unrepresentable.
- Law: the Zoo and CloudZoo hops are NETWORK-backed and their refusals ride `PluginFault.Unreachable`, whose `Retriability` is `Transient`; an app root composing `Redrive.Run(RedrivePolicy, …)` over these entries re-drives exactly the transient class and nothing else, and no retry owner is minted here (kernel `Domain/results#[05]-[REDRIVE]`).
- Law: `GetOneLicenseStatus` answers null for an unlicensed product, so absence rides `Option<LicenseState>` and is not a fault.
- Boundary: `ShowBuyLicenseUi` and `ShowLicenseValidationUi` are INTERACTIVE and cross the kernel dispatch on the modal lane; the buy dialog returns nothing, so its answer names the product it offered and carries no outcome the host does not publish.
- Boundary: `LoginToCloudZoo` opens the host's own account flow; the account case answers the resulting identity as detached text off `RhinoApp`.
- Packages: Thinktecture.Runtime.Extensions (`[Union]`, `[SmartEnum<string>]` with `[UseDelegateFromConstructor]`); LanguageExt.Core (`Fin`, `Option`, `Seq`, `Traverse`, `.Strict()`); kernel `Domain/results` (`Admit.Need`, `Try.lift`, `Acceptance.Text`, `HostEdge.Text`, `Retriability`, `RedrivePolicy`), `Domain/validation` (`FactoryBridge.Row`), `Interaction/dispatch` (`UiThread.Run`, `UiDispatch<T>.Blocking`, `DispatchLane.Modal`); `Document/events` (`PluginKey.Maybe`); RhinoCommon plug-ins (`.api/api-rhinocommon-plugins.md` — `LicenseUtils.GetLicenseStatus`, `GetOneLicenseStatus`, `CheckOutLicense`, `CheckInLicense`, `ReturnLicense`, `ConvertLicense`, `DeleteLicense`, `IsCheckOutEnabled`, `LoginToCloudZoo`, `LogoutOfCloudZoo`, `ShowBuyLicenseUi`, `ShowLicenseValidationUi`), RhinoCommon runtime (`api-rhinocommon-runtime.md` — `RhinoApp.UserIsLoggedIn`, `RhinoApp.LoggedInUserName`).

```csharp
// --- [TABLES] --------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LicenseVerb {
    public static readonly LicenseVerb CheckOut = new("check-out", run: static id => LicenseUtils.CheckOutLicense(productId: id));
    public static readonly LicenseVerb CheckIn = new("check-in", run: static id => LicenseUtils.CheckInLicense(productId: id));
    public static readonly LicenseVerb Release = new("release", run: static id => LicenseUtils.ReturnLicense(productId: id));
    public static readonly LicenseVerb Convert = new("convert", run: static id => LicenseUtils.ConvertLicense(productId: id));
    public static readonly LicenseVerb Delete = new("delete", run: static id => LicenseUtils.DeleteLicense(productId: id));

    [UseDelegateFromConstructor] internal partial bool Run(Guid product);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SessionVerb {
    public static readonly SessionVerb Login = new("login", run: LicenseUtils.LoginToCloudZoo);
    public static readonly SessionVerb Logout = new("logout", run: LicenseUtils.LogoutOfCloudZoo);

    [UseDelegateFromConstructor] internal partial bool Run();
}

// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LicenseAccount {
    private LicenseAccount() { }
    public sealed record SignedOut : LicenseAccount;
    public sealed record SignedIn(string User) : LicenseAccount;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LicenseAsk {
    private LicenseAsk() { }
    public sealed record Census : LicenseAsk;
    public sealed record One(ProductKey Product) : LicenseAsk;
    public sealed record Act(ProductKey Product, LicenseVerb Verb) : LicenseAsk;
    public sealed record CheckOutEnabled : LicenseAsk;
    public sealed record Session(SessionVerb Verb) : LicenseAsk;
    public sealed record Account : LicenseAsk;
    public sealed record BuyUi(ProductKey Product) : LicenseAsk;
    public sealed record ValidateUi(string CdKey) : LicenseAsk;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LicenseVerdict {
    private LicenseVerdict() { }
    public sealed record Rows(Seq<LicenseState> States) : LicenseVerdict;
    public sealed record Row(Option<LicenseState> State) : LicenseVerdict;
    public sealed record Acted(LicenseVerb Verb, bool Accepted) : LicenseVerdict;
    public sealed record CheckOut(bool Enabled) : LicenseVerdict;
    public sealed record SessionSettled(SessionVerb Verb) : LicenseVerdict;
    public sealed record Signed(LicenseAccount Account) : LicenseVerdict;
    public sealed record BuyOffered(ProductKey Product) : LicenseVerdict;
    public sealed record Validated(bool Accepted) : LicenseVerdict;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Licenses {
    public static Fin<LicenseVerdict> Ask(LicenseAsk ask) {
        return Admit.Need(ask).Bind(request => request.Switch(census: static (held, _) => Try.lift(() => toSeq(LicenseUtils.GetLicenseStatus())
                .Traverse(status => State(status, held))
                .As()
                .Map<LicenseVerdict>(static rows => new LicenseVerdict.Rows(States: rows.Strict()))).Run().Bind(static inner => inner),
            one: static (held, row) => row.Product.Admit(held).Bind(_ => Try.lift(() =>
                Optional(LicenseUtils.GetOneLicenseStatus(productid: row.Product.ToValue()))
                    .TraverseM(status => State(status, held))
                    .As()
                    .Map<LicenseVerdict>(state => new LicenseVerdict.Row(state))).Run().Bind(static inner => inner)),
            act: static (held, row) => row.Product.Admit(held)
                .Bind(_ => Admit.Need(row.Verb))
                .Bind(verb => Try.lift(() => Fin.Succ<LicenseVerdict>(value: new LicenseVerdict.Acted(
                    Verb: verb, Accepted: verb.Run(product: row.Product.ToValue())))).Run().Bind(static inner => inner)),
            checkOutEnabled: static (held, _) => Try.lift(() => Fin.Succ<LicenseVerdict>(
                value: new LicenseVerdict.CheckOut(Enabled: LicenseUtils.IsCheckOutEnabled()))).Run().Bind(static inner => inner),
            session: static (held, row) => Admit.Need(row.Verb).Bind(verb => Try.lift(() => verb.Run()
                ? Fin.Succ<LicenseVerdict>(value: new LicenseVerdict.SessionSettled(Verb: verb))
                : Fin.Fail<LicenseVerdict>(error: new PluginFault.Unreachable(
                    Key: held, Member: verb.Key))).Run().Bind(static inner => inner)),
            account: static (held, _) => Try.lift(() => Fin.Succ<LicenseVerdict>(
                value: new LicenseVerdict.Signed(Account:
                    (RhinoApp.UserIsLoggedIn ? HostEdge.Text(RhinoApp.LoggedInUserName) : None).Match(
                        Some: static user => (LicenseAccount)new LicenseAccount.SignedIn(User: user),
                        None: static () => new LicenseAccount.SignedOut())))).Run().Bind(static inner => inner),
            buyUi: static (held, row) => row.Product.Admit(held).Bind(_ => UiThread.Run(
                new UiDispatch<LicenseVerdict>.Blocking(() => Try.lift(() => LicenseUtils.ShowBuyLicenseUi(productId: row.Product.ToValue())).Run().Bind(static inner => inner)
                    .Map<LicenseVerdict>(_ => new LicenseVerdict.BuyOffered(Product: row.Product))),
                DispatchLane.Modal,
                held)),
            validateUi: static (held, row) => Acceptance.Text(value: row.CdKey).Bind(cdkey => UiThread.Run(
                new UiDispatch<LicenseVerdict>.Blocking(() => Try.lift(() => Fin.Succ<LicenseVerdict>(
                    value: new LicenseVerdict.Validated(Accepted: LicenseUtils.ShowLicenseValidationUi(cdkey: cdkey)))).Run().Bind(static inner => inner)),
                DispatchLane.Modal,
                held))));
    }

    private static Fin<LicenseState> State(LicenseStatus status) =>
        from row in Optional(status).ToFin(Fail: new PluginFault.Unbound(Member: nameof(LicenseStatus)))
        from kind in FactoryBridge.Row<LicenseType, LicenseKind>(row.LicenseType)
        from build in FactoryBridge.Row<LicenseBuildType, LicenseBuild>(row.BuildType)
        select new LicenseState(
            Plugin: PluginKey.Maybe(row.PluginId),
            Product: ProductKey.Maybe(row.ProductId),
            Kind: kind,
            Build: build,
            Title: HostEdge.Text(row.LicenseTitle),
            SerialNumber: HostEdge.Text(row.SerialNumber),
            Expires: Optional(row.ExpirationDate),
            CheckOutExpires: Optional(row.CheckOutExpirationDate),
            RegisteredOwner: HostEdge.Text(row.RegisteredOwner),
            RegisteredOrganization: HostEdge.Text(row.RegisteredOrganization),
            Lease: row.CloudZooLeaseIsValid
                ? Some(new CloudZooLease(Expires: Optional(row.CloudZooLeaseExpiration)))
                : None);
}
```

## [06]-[FACTS]

- Owner: `LicenseFact` detaches `LicenseStateChangedEventArgs`, whose only payload is whether the host admits a RhinoCommon call — one genuinely independent axis with no sibling column, which is why it stays a bool.
- Entry: `LicensePulse.Observe` binds `RhinoApp.LicenseStateChanged` through `Document/lifetime#SUBSCRIPTION`'s `Subscription.Attach` capsule, so attach and release are symmetric and this page mints no second subscription mechanism.
- Law: the fact is a process-wide gate, not a per-product state — a consumer wanting product detail asks `Licenses.Ask` with a `One` case on the change.
- Boundary: a sink fault records nowhere on this pipeline; the observation is a pure detach-and-deliver, and a consumer needing a ledger supplies one inside its own sink.
- Packages: LanguageExt.Core (`Fin`, `Option`); kernel `Domain/results` (`Admit.Need`, `Try.lift`, `HostEdge.Side`); `Document/lifetime` (`Subscription.Attach`); RhinoCommon runtime (`.api/api-rhinocommon-runtime.md:187` — `LicenseStateChangedEventArgs.CallingRhinoCommonAllowed`, `RhinoApp.LicenseStateChanged`).

```csharp
// --- [MODELS] --------------------------------------------------------------------------
public sealed record LicenseFact(bool CallingAllowed);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class LicensePulse {
    public static Fin<Subscription> Observe(Action<LicenseFact> sink) {
        return from receiver in Admit.Need(sink)
               from subscription in Subscription.Attach(
                   subscribe: static (EventHandler<LicenseStateChangedEventArgs> handler) => RhinoApp.LicenseStateChanged += handler,
                   unsubscribe: static handler => RhinoApp.LicenseStateChanged -= handler,
                   handler: (_, args) => ignore(Try.lift(() => Fin.Succ(value: HostEdge.Side(() => receiver(
                       arg: new LicenseFact(CallingAllowed: args.CallingRhinoCommonAllowed))))).Run().Bind(static inner => inner)))
               select subscription;
    }
}
```

## [07]-[SURFACE_LEDGER]

| [INDEX] | [OWNER]           | [INGRESS]                       | [STATE]                             | [EGRESS]                              |
| :-----: | :---------------- | :------------------------------ | :---------------------------------- | :------------------------------------ |
|  [01]   | `Licenses`        | `Ask(LicenseAsk)`               | none — the Zoo owns the entitlement | `LicenseVerdict` per request row      |
|  [02]   | `RasmPlugIn`      | `Entitlement(Entitle)`          | the adapter's refusal ring          | `EntitleOutcome` naming its ask       |
|  [03]   | `ILicenseProgram` | foreign plug-in implementation  | program-owned                       | `LicenseReply` · badge `AssetRaster`  |
|  [04]   | `LicenseEvidence` | `Detach(LicenseData)` · `Mint`  | generated admission                 | both directions of one correspondence |
|  [05]   | `LeaseMap`        | `LicenseLease` callback wrapper | none — a generated projection       | `LeaseEvidence`                       |
|  [06]   | `LicenseVerb`     | `LicenseAsk.Act`                | row-owned host verb                 | the verb the verdict names            |
|  [07]   | `LicensePulse`    | `RhinoApp.LicenseStateChanged`  | `Subscription` capsule              | `LicenseFact`                         |

## [08]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
