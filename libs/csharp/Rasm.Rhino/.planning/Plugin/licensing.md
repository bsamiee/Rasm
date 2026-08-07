# [RASM_RHINO_PLUGIN_LICENSING]

`Licenses.Ask` folds the whole Zoo and CloudZoo entitlement surface into one request union, and `RasmPlugIn.Entitlement` owns the plug-in-side acquisition rail the host exposes as `protected` members. Both answer typed verdicts over detached evidence, so a live `LicenseData`, `LicenseStatus`, or `LicenseLease` never escapes the arm that read it and no consumer learns the host's out-parameter and delegate shapes.

The acquisition arm continues `lifecycle#ADAPTER`'s `RasmPlugIn` as the same partial class: `GetLicense`, `AskUserForLicense`, `ReturnLicense`, `GetLicenseOwner`, and `SetLicenseCapabilities` are `protected` on `PlugIn`, so only a derivation reaches them and a free-standing service could not compile. `LicenseEvidence` admits exactly the fields `LicenseData.IsValid` proves, so an evidence value that exists is one the host accepts. Entitlement probes over `CloudHostUtils` stay at `HostUi/shell#RUNTIME` under `HostProbe.Entitlement`; this page mints no second probe.

## [01]-[INDEX]

- [02]-[VOCABULARY]: `ProductKey`, `LicenseGrant`, `LicenseGrants`, `LicenseBuild`, `LicenseKind`, and `LicenseNode` close every entitlement axis.
- [03]-[EVIDENCE]: `LicenseEvidence`, `LicenseState`, `LeaseEvidence`, and `OwnerEvidence` detach the four host license payloads.
- [04]-[ACQUISITION]: `LicenseProgram`, `LicenseReply`, `Entitle`, `EntitlementReceipt`, and the `RasmPlugIn` entitlement arm own plug-in-side acquisition.
- [05]-[RAIL]: `LicenseAsk`, `LicenseVerdict`, and `Licenses.Ask` own the `LicenseUtils` census, checkout family, and CloudZoo session.
- [06]-[FACTS]: `LicenseFact` and `LicensePulse.Observe` detach the host license-state change.

## [02]-[VOCABULARY]

- Owner: `ProductKey` is the license product and license identity — a different concept from `PluginKey`, which keys the plug-in itself, so the two never substitute.
- Owner: `LicenseGrant` `[SmartEnum<LicenseCapabilities>]` carries the host capability flags whole and `LicenseGrants` is the admitted set; `Mask` is the one OR-fold.
- Law: the capability roster includes `NoCapabilities`, `CanBeSpecified`, and `EvaluationIsExpired` — the host declares them and a vocabulary omitting them could not round-trip a capability value the host returns.
- Law: `LicenseGrants.OfMask` admits a persisted capability bitfield here rather than through `LicenseUtils.GetLicenseCapabilities(int)` — that member is a bit-for-bit projection onto the same flag ordinals that THROWS above `0x1FF`, so routing the raw int onto the typed owner keeps exception control flow out of the rail and leaves one capability owner.
- Law: `LicenseNode` replaces the host's `standAlone` boolean, so a prompt request names the node type rather than a positional flag.
- Boundary: `LicenseBuild` and `LicenseKind` mirror their host enums completely, so `TryGet` over a host read is total and no arm invents a default row.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Collections.Frozen;
using Eto.Forms;
using Rasm.Domain;
using Rasm.Rhino.Document;
using Rasm.Rhino.HostUi;
using Rhino;
using Rhino.PlugIns;
using Rhino.Runtime;
using DrawingIcon = System.Drawing.Icon;

namespace Rasm.Rhino.Plugin;

// --- [TYPES] --------------------------------------------------------------------------------
[ValueObject<Guid>]
public readonly partial struct ProductKey {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Guid value) =>
        validationError = value == Guid.Empty ? new ValidationError(message: "Product identity is empty.") : null;

    internal static Option<ProductKey> Maybe(Guid value) =>
        Optional(value).Filter(static id => id != Guid.Empty).Map(Create);

    internal Fin<Unit> Admit(Op op) {
        ValidationError? fault = Validate(value: ToValue(), provider: null, out ProductKey? admitted);
        return op.AcceptValidated<ProductKey>(fault: fault, admitted: admitted).Map(static _ => unit);
    }
}

[SmartEnum<LicenseCapabilities>]
public sealed partial class LicenseGrant {
    public static readonly LicenseGrant None = new(key: LicenseCapabilities.NoCapabilities);
    public static readonly LicenseGrant Purchasable = new(key: LicenseCapabilities.CanBePurchased);
    public static readonly LicenseGrant Specifiable = new(key: LicenseCapabilities.CanBeSpecified);
    public static readonly LicenseGrant Evaluable = new(key: LicenseCapabilities.CanBeEvaluated);
    public static readonly LicenseGrant EvaluationExpired = new(key: LicenseCapabilities.EvaluationIsExpired);
    public static readonly LicenseGrant RhinoAccounts = new(key: LicenseCapabilities.SupportsRhinoAccounts);
    public static readonly LicenseGrant Standalone = new(key: LicenseCapabilities.SupportsStandalone);
    public static readonly LicenseGrant ZooPerUser = new(key: LicenseCapabilities.SupportsZooPerUser);
    public static readonly LicenseGrant ZooPerCore = new(key: LicenseCapabilities.SupportsZooPerCore);
    public static readonly LicenseGrant Discovery = new(key: LicenseCapabilities.SupportsLicenseDiscovery);
}

[ComplexValueObject]
public sealed partial class LicenseGrants {
    public FrozenSet<LicenseGrant> Values { get; }

    internal LicenseCapabilities Mask => toSeq(Values)
        .Fold(LicenseCapabilities.NoCapabilities, static (mask, grant) => mask | grant.Key);

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref FrozenSet<LicenseGrant> values) =>
        validationError = values is null || values.Count is 0 || values.Any(static grant => grant is null)
            ? new ValidationError(message: "License grant set is empty.")
            : null;

    public static Fin<LicenseGrants> Of(Op? key, params ReadOnlySpan<LicenseGrant> values) {
        Op op = key.OrDefault();
        return op.AcceptValidated<LicenseGrants>(
            fault: Validate(toSeq(values.ToArray()).ToFrozenSet(), out LicenseGrants? admitted),
            admitted: admitted);
    }

    // The admissible field is the OR-fold of the declared rows, derived so a roster edit moves the bound with it;
    // a wider field is a capability this vocabulary cannot name and refuses, and an empty selection admits as the
    // explicit `None` row rather than as an empty set.
    public static Fin<LicenseGrants> OfMask(int filter, Op? key = null) {
        Op op = key.OrDefault();
        int declared = toSeq(LicenseGrant.Items).Fold(0, static (mask, grant) => mask | (int)grant.Key);
        return guard(filter >= 0 && (filter & ~declared) == 0, op.InvalidInput()).ToFin().Bind(_ => {
            Seq<LicenseGrant> rows = toSeq(LicenseGrant.Items)
                .Filter(grant => grant.Key != LicenseCapabilities.NoCapabilities && (filter & (int)grant.Key) == (int)grant.Key)
                .Strict();
            return Of(op, rows.IsEmpty ? [LicenseGrant.None] : rows.ToArray());
        });
    }
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

[SmartEnum<bool>]
public sealed partial class LicenseNode {
    public static readonly LicenseNode Network = new(false);
    public static readonly LicenseNode Standalone = new(true);
}
```

## [03]-[EVIDENCE]

- Owner: `LicenseEvidence` is the ONE license payload carrier and crosses in both directions — the boundary detaches a host `LicenseData` into it and mints a host `LicenseData` from it inside the validate callback.
- Law: admission mirrors `LicenseData.IsValid` exactly — non-blank product license, serial, and title, a defined build row, and a positive count — so an admitted evidence value is one the host's own validity read accepts.
- Owner: `LicenseState` detaches `LicenseStatus`; `LeaseEvidence` detaches `LicenseLease`; `OwnerEvidence` detaches the two `GetLicenseOwner` out-parameters.
- Law: `LicenseLeaseChangedEventArgs.Lease` mints a FRESH managed wrapper over a borrowed native pointer on every read, so the lease detaches inside the callback and no wrapper is retained past it.
- Boundary: `LicenseData.ProductIcon` and `LicenseStatus.ProductIcon` are disposable native handles, so neither rides a detached record; the lease-changed hook answers its badge icon as its own return and the boundary hands it straight back to the host.

```csharp signature
// --- [MODELS] -------------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class LicenseEvidence {
    public string ProductLicense { get; }
    public string SerialNumber { get; }
    public string LicenseTitle { get; }
    public LicenseBuild Build { get; }
    public int Count { get; }
    public Option<DateTime> Expires { get; }
    public bool RequiresOnlineValidation { get; }
    public bool UpgradeFromPreviousVersion { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref string productLicense,
        ref string serialNumber,
        ref string licenseTitle,
        ref LicenseBuild build,
        ref int count,
        ref Option<DateTime> expires,
        ref bool requiresOnlineValidation,
        ref bool upgradeFromPreviousVersion) =>
        validationError = string.IsNullOrWhiteSpace(productLicense)
            || string.IsNullOrWhiteSpace(serialNumber)
            || string.IsNullOrWhiteSpace(licenseTitle)
            || build is null
            || count < 1
            ? new ValidationError(message: "License evidence is incomplete.")
            : null;

    internal Fin<LicenseData> Mint(Op op) => op.Catch(() => Fin.Succ(value: new LicenseData(
        productLicense: ProductLicense,
        serialNumber: SerialNumber,
        licenseTitle: LicenseTitle,
        buildType: Build.Key,
        licenseCount: Count,
        expirationDate: Expires.Map(static value => (DateTime?)value).IfNone((DateTime?)null),
        productIcon: null,
        requiresOnlineValidation: RequiresOnlineValidation,
        isUpgradeFromPreviousVersion: UpgradeFromPreviousVersion)));

    internal static Fin<LicenseEvidence> Detach(LicenseData data, Op op) =>
        from row in op.Need(data)
        from build in op.Row<LicenseBuildType, LicenseBuild>(row.BuildType)
        from evidence in op.AcceptValidated<LicenseEvidence>(
            fault: Validate(
                row.ProductLicense,
                row.SerialNumber,
                row.LicenseTitle,
                build,
                row.LicenseCount,
                Optional(row.DateToExpire),
                row.RequiresOnlineValidation,
                row.IsUpgradeFromPreviousVersion,
                out LicenseEvidence? candidate),
            admitted: candidate)
        select evidence;
}

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
    bool CloudZooLeaseValid,
    Option<DateTime> CloudZooLeaseExpires);

public sealed record LeaseEvidence(
    string LeaseId,
    string ProductId,
    string ProductTitle,
    string ProductVersion,
    string ProductEdition,
    string GroupId,
    string GroupName,
    string UserId,
    string UserName,
    DateTime IssuedAt,
    DateTime Expiration,
    Option<DateTime> RenewableUntil);

public sealed record OwnerEvidence(string Owner, string Organization);
```

## [04]-[ACQUISITION]

- Owner: `LicenseProgram` carries the two host callbacks in typed form — key validation answering a `LicenseReply`, and lease change answering an optional badge icon.
- Owner: `Entitle` closes the plug-in-side acquisition family; `EntitlementReceipt` carries its outcomes.
- Law: `RasmPlugIn` continues as a partial class here because the host declares its entitlement members `protected`; the arm is co-located with the derivation the language forces it onto, never lifted to a free-standing service that could not reach them.
- Law: the validate callback is total by construction — a refused rail, a faulted program, or a malformed reply all settle on `ErrorHideMessage` with an empty payload, because the host reads the `out` slot unconditionally and a null there faults inside native code.
- Law: `ByBuild` is the capability-free overload the host supplies — it passes `NoCapabilities` and a null mask internally — so a request naming both a build and a grant set uses `ByCapability` and the two never merge.
- Boundary: `AskUserForLicense` is INTERACTIVE and rides `HostThread.Run`; every other arm is a non-interactive host call.
- Boundary: the prompt arm admits an optional Eto parent — decompile-proven through the shipped Zoo client, `ZooClientParameters.ParentWindow` lands on `ValidationClient.UI.Models.LicenseUiModel.ParentWindow : Eto.Forms.Control` and feeds `MessageBox.Show`/dialog parenting directly, so the accepted concrete shape is an Eto `Control` and `null` is the parentless floor the arm defaults to.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LicenseReply {
    private LicenseReply() { }
    public sealed record Accepted(LicenseEvidence Evidence) : LicenseReply;
    public sealed record RefusedLoudly(string Message) : LicenseReply;
    public sealed record RefusedQuietly : LicenseReply;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Entitle {
    private Entitle() { }
    public sealed record ByCapability(LicenseProgram Program, LicenseGrants Grants, Option<string> TextMask) : Entitle;
    public sealed record ByBuild(LicenseProgram Program, LicenseBuild Build) : Entitle;
    public sealed record ByPrompt(LicenseProgram Program, LicenseBuild Build, LicenseNode Node, Option<string> TextMask, Option<Control> Parent = default) : Entitle;
    public sealed record Release : Entitle;
    public sealed record Declare(LicenseGrants Grants, Option<string> TextMask, ProductKey License) : Entitle;
    public sealed record Owner : Entitle;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record EntitlementReceipt {
    private EntitlementReceipt() { }
    public sealed record Held : EntitlementReceipt;
    public sealed record Released : EntitlementReceipt;
    public sealed record Declared(LicenseGrants Grants) : EntitlementReceipt;
    public sealed record Registered(OwnerEvidence Evidence) : EntitlementReceipt;
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record LicenseProgram(
    Func<string, Fin<LicenseReply>> Validate,
    Func<Option<LeaseEvidence>, Fin<Option<DrawingIcon>>> LeaseChanged);

// --- [SERVICES] -----------------------------------------------------------------------------
// The entitlement arm continues the lifecycle adapter: `PlugIn` publishes GetLicense, AskUserForLicense,
// ReturnLicense, GetLicenseOwner, and SetLicenseCapabilities as `protected`, so a derivation is the only caller
// the language admits and the concern lands on this class rather than beside it.
public abstract partial class RasmPlugIn {
    public Fin<EntitlementReceipt> Entitlement(Entitle ask, Op? key = null) {
        Op op = key.OrDefault();
        return op.Need(ask).Bind(request => request.Switch(
            op,
            byCapability: (held, row) => Program(row.Program, held).Bind(program => held.Catch(() => GetLicense(
                    licenseCapabilities: row.Grants.Mask,
                    textMask: row.TextMask.IfNone((string?)null)!,
                    validateProductKeyDelegate: Validator(program, held),
                    leaseChanged: LeaseWatcher(program, held))
                ? Fin.Succ<EntitlementReceipt>(value: new EntitlementReceipt.Held())
                : Fin.Fail<EntitlementReceipt>(error: held.InvalidResult(detail: nameof(GetLicense))))),
            byBuild: (held, row) => Program(row.Program, held).Bind(program => held.Catch(() => GetLicense(
                    productBuildType: row.Build.Key,
                    validateProductKeyDelegate: Validator(program, held),
                    leaseChangedDelegate: LeaseWatcher(program, held))
                ? Fin.Succ<EntitlementReceipt>(value: new EntitlementReceipt.Held())
                : Fin.Fail<EntitlementReceipt>(error: held.InvalidResult(detail: nameof(GetLicense))))),
            byPrompt: (held, row) => Program(row.Program, held).Bind(program => HostThread.Run(
                work: new HostWork<EntitlementReceipt>.Execute(Body: () => held.Catch(() => AskUserForLicense(
                        productBuildType: row.Build.Key,
                        standAlone: row.Node.Key,
                        textMask: row.TextMask.IfNone((string?)null)!,
                        // Zoo client casts this to Eto.Forms.Control (decompile-proven); None stays the parentless floor.
                        parentWindow: row.Parent.Match(Some: static owner => (object)owner, None: static () => (object?)null),
                        validateProductKeyDelegate: Validator(program, held),
                        onLeaseChangedDelegate: LeaseWatcher(program, held))
                    ? Fin.Succ<EntitlementReceipt>(value: new EntitlementReceipt.Held())
                    : Fin.Fail<EntitlementReceipt>(error: held.InvalidResult(detail: nameof(AskUserForLicense))))),
                key: held)),
            release: (held, _) => held.Catch(() => ReturnLicense()
                ? Fin.Succ<EntitlementReceipt>(value: new EntitlementReceipt.Released())
                : Fin.Fail<EntitlementReceipt>(error: held.InvalidResult(detail: nameof(ReturnLicense)))),
            declare: (held, row) => row.License.Admit(held).Bind(_ => held.Catch(() => {
                SetLicenseCapabilities(
                    textMask: row.TextMask.IfNone((string?)null)!,
                    capabilities: row.Grants.Mask,
                    licenseId: row.License.ToValue());
                return Fin.Succ<EntitlementReceipt>(value: new EntitlementReceipt.Declared(Grants: row.Grants));
            })),
            owner: (held, _) => held.Catch(() => GetLicenseOwner(
                    registeredOwner: out string registeredOwner,
                    registeredOrganization: out string registeredOrganization)
                ? Fin.Succ<EntitlementReceipt>(value: new EntitlementReceipt.Registered(Evidence: new OwnerEvidence(
                    Owner: registeredOwner ?? string.Empty,
                    Organization: registeredOrganization ?? string.Empty)))
                : Fin.Fail<EntitlementReceipt>(error: held.MissingContext()))));
    }

    private static Fin<LicenseProgram> Program(LicenseProgram program, Op op) =>
        op.Need(program).Bind(row => guard(
            row.Validate is not null && row.LeaseChanged is not null,
            op.InvalidInput()).ToFin().Map(_ => row));

    // The host reads the `out` slot unconditionally and dereferences it inside native code, so every refusal path
    // writes a payload; only the accepted arm carries evidence, and a loud refusal carries its own message.
    private ValidateProductKeyDelegate Validator(LicenseProgram program, Op op) =>
        (string productKey, out LicenseData licenseData) => {
            Fin<(ValidateResult Verdict, LicenseData Data)> settled = Record(outcome:
                from key in op.AcceptText(value: productKey)
                from reply in op.Catch(() => program.Validate(arg: key))
                from projected in reply.Switch(
                    op,
                    accepted: static (held, row) => row.Evidence.Mint(held)
                        .Map(static data => (Verdict: ValidateResult.Success, Data: data)),
                    refusedLoudly: static (held, row) => held.AcceptText(value: row.Message)
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

    // A null `Lease` is the server revoking the product, so absence is the fact the hook receives rather than a
    // refusal; the wrapper the host mints is borrowed, so it detaches here and never leaves the callback.
    private OnLeaseChangedDelegate LeaseWatcher(LicenseProgram program, Op op) =>
        (LicenseLeaseChangedEventArgs args, out DrawingIcon icon) => {
            Option<DrawingIcon> badge = Record(outcome:
                from row in op.Need(args)
                from lease in op.Catch(() => Fin.Succ(value: Optional(row.Lease)))
                from evidence in lease.Match(
                    Some: held => Detach(held, op).Map(Some),
                    None: static () => Fin.Succ(value: Option<LeaseEvidence>.None))
                from answer in op.Catch(() => program.LeaseChanged(arg: evidence))
                select answer)
                .Match(Succ: static value => value, Fail: static _ => Option<DrawingIcon>.None);
            icon = badge.IfNone((DrawingIcon?)null)!;
        };

    private static Fin<LeaseEvidence> Detach(LicenseLease lease, Op op) => op.Catch(() => Fin.Succ(value: new LeaseEvidence(
        LeaseId: lease.LeaseId ?? string.Empty,
        ProductId: lease.ProductId ?? string.Empty,
        ProductTitle: lease.ProductTitle ?? string.Empty,
        ProductVersion: lease.ProductVersion ?? string.Empty,
        ProductEdition: lease.ProductEdition ?? string.Empty,
        GroupId: lease.GroupId ?? string.Empty,
        GroupName: lease.GroupName ?? string.Empty,
        UserId: lease.UserId ?? string.Empty,
        UserName: lease.UserName ?? string.Empty,
        IssuedAt: lease.IssuedAt,
        Expiration: lease.Expiration,
        RenewableUntil: Optional(lease.RenewableUntil))));
}
```

## [05]-[RAIL]

- Owner: `LicenseAsk` closes the `LicenseUtils` surface — status census, the checkout family, CloudZoo session, and the two interactive dialogs.
- Law: the checkout family — check out, check in, release, convert, delete — is five cases on one union over identical host shapes, so the rail carries one arm each and no five-method surface forms.
- Law: `GetOneLicenseStatus` answers null for an unlicensed product, so absence rides `Option<LicenseState>` and is not a fault.
- Boundary: `ShowBuyLicenseUi` and `ShowLicenseValidationUi` are INTERACTIVE and ride `HostThread.Run`; the buy dialog returns nothing, so its receipt is the settled case and carries no outcome the host does not publish.
- Boundary: `LoginToCloudZoo` opens the host's own account flow; the session case answers the resulting identity as detached text.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LicenseAsk {
    private LicenseAsk() { }
    public sealed record Census : LicenseAsk;
    public sealed record One(ProductKey Product) : LicenseAsk;
    public sealed record CheckOut(ProductKey Product) : LicenseAsk;
    public sealed record CheckIn(ProductKey Product) : LicenseAsk;
    public sealed record Release(ProductKey Product) : LicenseAsk;
    public sealed record Convert(ProductKey Product) : LicenseAsk;
    public sealed record Delete(ProductKey Product) : LicenseAsk;
    public sealed record CheckOutEnabled : LicenseAsk;
    public sealed record CloudZooLogin : LicenseAsk;
    public sealed record CloudZooLogout : LicenseAsk;
    public sealed record Session : LicenseAsk;
    public sealed record BuyUi(ProductKey Product) : LicenseAsk;
    public sealed record ValidateUi(string CdKey) : LicenseAsk;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LicenseVerdict {
    private LicenseVerdict() { }
    public sealed record Rows(Seq<LicenseState> States) : LicenseVerdict;
    public sealed record Row(Option<LicenseState> State) : LicenseVerdict;
    public sealed record Settled : LicenseVerdict;
    public sealed record Flag(bool Value) : LicenseVerdict;
    public sealed record Account(bool LoggedIn, Option<string> User) : LicenseVerdict;
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class Licenses {
    public static Fin<LicenseVerdict> Ask(LicenseAsk ask, Op? key = null) {
        Op op = key.OrDefault();
        return op.Need(ask).Bind(request => request.Switch(
            op,
            census: static (held, _) => held.Catch(() => toSeq(LicenseUtils.GetLicenseStatus())
                .Traverse(status => State(status, held))
                .As()
                .Map<LicenseVerdict>(static rows => new LicenseVerdict.Rows(States: rows.Strict()))),
            one: static (held, row) => row.Product.Admit(held).Bind(_ => held.Catch(() =>
                Optional(LicenseUtils.GetOneLicenseStatus(productid: row.Product.ToValue())).Match(
                    Some: status => State(status, held).Map<LicenseVerdict>(static state => new LicenseVerdict.Row(State: Some(state))),
                    None: static () => Fin.Succ<LicenseVerdict>(value: new LicenseVerdict.Row(State: None))))),
            checkOut: static (held, row) => Toggled(row.Product, held, static id => LicenseUtils.CheckOutLicense(productId: id)),
            checkIn: static (held, row) => Toggled(row.Product, held, static id => LicenseUtils.CheckInLicense(productId: id)),
            release: static (held, row) => Toggled(row.Product, held, static id => LicenseUtils.ReturnLicense(productId: id)),
            convert: static (held, row) => Toggled(row.Product, held, static id => LicenseUtils.ConvertLicense(productId: id)),
            delete: static (held, row) => Toggled(row.Product, held, static id => LicenseUtils.DeleteLicense(productId: id)),
            checkOutEnabled: static (held, _) => held.Catch(() => Fin.Succ<LicenseVerdict>(
                value: new LicenseVerdict.Flag(Value: LicenseUtils.IsCheckOutEnabled()))),
            cloudZooLogin: static (held, _) => held.Catch(() => LicenseUtils.LoginToCloudZoo()
                ? Fin.Succ<LicenseVerdict>(value: new LicenseVerdict.Settled())
                : Fin.Fail<LicenseVerdict>(error: held.InvalidResult(detail: nameof(LicenseUtils.LoginToCloudZoo)))),
            cloudZooLogout: static (held, _) => held.Catch(() => LicenseUtils.LogoutOfCloudZoo()
                ? Fin.Succ<LicenseVerdict>(value: new LicenseVerdict.Settled())
                : Fin.Fail<LicenseVerdict>(error: held.InvalidResult(detail: nameof(LicenseUtils.LogoutOfCloudZoo)))),
            session: static (held, _) => held.Catch(() => Fin.Succ<LicenseVerdict>(value: new LicenseVerdict.Account(
                LoggedIn: RhinoApp.UserIsLoggedIn,
                User: Op.Text(RhinoApp.LoggedInUserName)))),
            buyUi: static (held, row) => row.Product.Admit(held).Bind(_ => HostThread.Run(
                work: new HostWork<LicenseVerdict>.Execute(Body: () => held.Catch(() => {
                    LicenseUtils.ShowBuyLicenseUi(productId: row.Product.ToValue());
                    return Fin.Succ<LicenseVerdict>(value: new LicenseVerdict.Settled());
                })),
                key: held)),
            validateUi: static (held, row) => held.AcceptText(value: row.CdKey).Bind(cdkey => HostThread.Run(
                work: new HostWork<LicenseVerdict>.Execute(Body: () => held.Catch(() => Fin.Succ<LicenseVerdict>(
                    value: new LicenseVerdict.Flag(Value: LicenseUtils.ShowLicenseValidationUi(cdkey: cdkey))))),
                key: held))));
    }

    private static Fin<LicenseVerdict> Toggled(ProductKey product, Op op, Func<Guid, bool> run) =>
        product.Admit(op).Bind(_ => op.Catch(() => Fin.Succ<LicenseVerdict>(
            value: new LicenseVerdict.Flag(Value: run(arg: product.ToValue())))));

    private static Fin<LicenseState> State(LicenseStatus status, Op op) =>
        from row in Optional(status).ToFin(Fail: op.InvalidResult(detail: nameof(LicenseStatus)))
        from kind in op.Row<LicenseType, LicenseKind>(row.LicenseType)
        from build in op.Row<LicenseBuildType, LicenseBuild>(row.BuildType)
        select new LicenseState(
            Plugin: PluginKey.Maybe(row.PluginId),
            Product: ProductKey.Maybe(row.ProductId),
            Kind: kind,
            Build: build,
            Title: Op.Text(row.LicenseTitle),
            SerialNumber: Op.Text(row.SerialNumber),
            Expires: Optional(row.ExpirationDate),
            CheckOutExpires: Optional(row.CheckOutExpirationDate),
            RegisteredOwner: Op.Text(row.RegisteredOwner),
            RegisteredOrganization: Op.Text(row.RegisteredOrganization),
            CloudZooLeaseValid: row.CloudZooLeaseIsValid,
            CloudZooLeaseExpires: Optional(row.CloudZooLeaseExpiration));
}
```

## [06]-[FACTS]

- Owner: `LicenseFact` detaches `LicenseStateChangedEventArgs`, whose only payload is whether calling RhinoCommon is currently allowed.
- Entry: `LicensePulse.Observe` binds `RhinoApp.LicenseStateChanged` through `Document/events#STREAM_OWNER`'s `Subscription.Attach` capsule, so attach and release are symmetric and this page mints no second subscription mechanism.
- Law: the fact is a process-wide gate, not a per-product state — a consumer wanting product detail asks `Licenses.Ask` with a `One` case on the change.
- Boundary: a sink fault records nowhere on this rail; the observation is a pure detach-and-deliver, and a consumer needing a ledger supplies one inside its own sink.

```csharp signature
// --- [MODELS] -------------------------------------------------------------------------------
public sealed record LicenseFact(bool CallingAllowed);

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class LicensePulse {
    public static Fin<Subscription> Observe(Action<LicenseFact> sink, Op? key = null) {
        Op op = key.OrDefault();
        return from receiver in op.Need(sink)
               from subscription in Subscription.Attach(
                   subscribe: static (EventHandler<LicenseStateChangedEventArgs> handler) => RhinoApp.LicenseStateChanged += handler,
                   unsubscribe: static handler => RhinoApp.LicenseStateChanged -= handler,
                   handler: (_, args) => ignore(op.Catch(() => Fin.Succ(value: Op.Side(() => receiver(
                       arg: new LicenseFact(CallingAllowed: args.CallingRhinoCommonAllowed)))))))
               select subscription;
    }
}
```

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
