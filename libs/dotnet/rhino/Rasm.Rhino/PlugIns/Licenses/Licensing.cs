using System.Drawing;
using Eto.Forms;
using Rasm.Rhino.UI.Styling;
using Rhino.PlugIns;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.PlugIns.Licenses;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record LeaseState(
    string LeaseId,
    Option<string> ProductId,
    Option<string> ProductTitle,
    Option<string> ProductVersion,
    Option<string> ProductEdition,
    Option<string> GroupId,
    Option<string> GroupName,
    Option<string> UserId,
    Option<string> UserName,
    Instant IssuedAt,
    Instant Expiration,
    Option<Instant> RenewableUntil);

public sealed record LicenseState(
    Option<Guid> PluginId,
    Guid ProductId,
    LicenseBuildType BuildType,
    Option<string> LicenseTitle,
    Option<string> SerialNumber,
    LicenseType LicenseType,
    Option<LocalDateTime> ExpirationDate,
    Option<LocalDateTime> CheckOutExpirationDate,
    Option<string> RegisteredOwner,
    Option<string> RegisteredOrganization,
    Option<Instant> CloudZooLeaseExpiration);

public sealed record LicenseDefinition(
    LicenseCapabilities Capabilities,
    Option<string> TextMask,
    Func<string, IO<LicenseData>> Validate,
    Func<Option<LeaseState>, IO<Unit>> LeaseChanged);

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class LicenseMapper {
    [MapperIgnoreSource(nameof(LicenseStatus.ProductIcon), Justification = "Icons.PlugInImage reads plug-in icon")]
    [MapperIgnoreSource(nameof(LicenseStatus.CloudZooLeaseIsValid), Justification = "CloudZooLeaseExpiration presence marks valid lease")]
    internal static partial LicenseState ToState(LicenseStatus status);

    [MapperIgnoreSource(nameof(LicenseLease.LeaseId), Justification = "leaseId parameter supplies value")]
    internal static partial LeaseState ToLease(LicenseLease lease, string leaseId, Instant issuedAt, Instant expiration);

    [UserMapping]
    private static Option<Instant> ToInstant(DateTime? value) => Optional(value).Map(DateTimeExtensions.ToInstant);
}

public static class Licensing {
    // --- [REQUEST]
    public static IO<Unit> Request(IPlugInSink sink, LicenseDefinition license, Func<LicenseCapabilities, string?, ValidateProductKeyDelegate, OnLeaseChangedDelegate, bool> getLicense) =>
        Asked(sink, license, (validate, leaseChanged) => getLicense(license.Capabilities, license.TextMask.ValueUnsafe(), validate, leaseChanged));

    public static IO<Unit> AskUser(
        IPlugInSink sink, LicenseDefinition license, LicenseBuildType build, Option<Control> parent,
        Func<LicenseBuildType, bool, string?, object?, ValidateProductKeyDelegate, OnLeaseChangedDelegate, bool> askUserForLicense) =>
        Asked(sink, license, (validate, leaseChanged) => askUserForLicense(build, false, license.TextMask.ValueUnsafe(), parent.ValueUnsafe(), validate, leaseChanged));

    private static IO<Unit> Asked(IPlugInSink sink, LicenseDefinition license, Func<ValidateProductKeyDelegate, OnLeaseChangedDelegate, bool> ask) =>
        Icons.PlugInImage(sink).Bind(static glyph => glyph.Traverse(static art => Icons.DrawingIcon(art, Size.Empty)).As())
            .Bind(icon => IO.lift(() => guard(ask(Validator(license.Validate, sink), LeaseHandler(license.LeaseChanged, icon, sink)), Errors.Cancelled).ToFin()));

    // --- [RELEASE]
    public static IO<Unit> Return(Func<bool> returnLicense) =>
        IO.lift(() => Refused.Unless(returnLicense(), nameof(returnLicense)));

    // --- [STATUS]
    public static IO<Seq<LicenseState>> GetLicenseStatus() =>
        IO.lift(static () => Conversions.Rows(LicenseUtils.GetLicenseStatus()).Map(LicenseMapper.ToState).Strict());

    public static IO<Option<LicenseState>> GetOneLicenseStatus(Guid productId) =>
        IO.lift(() => Optional(LicenseUtils.GetOneLicenseStatus(productId)).Map(LicenseMapper.ToState));

    // --- [CALLBACKS]
    private static ValidateProductKeyDelegate Validator(Func<string, IO<LicenseData>> validate, IPlugInSink sink) =>
        (productKey, out licenseData) => {
            (ValidateResult result, licenseData) = Callbacks.Answer(
                productKey,
                key => validate(key)
                    .Map(static data => (ValidateResult.Success, data))
                    .Catch(static error => error.Is(Errors.Cancelled), static _ => IO.pure((ValidateResult.ErrorHideMessage, new LicenseData())))
                    .Catch(static error => error.IsExpected, static error => IO.pure((ValidateResult.ErrorShowMessage, new LicenseData { ErrorMessage = ErrorOps.Localize(error) }))),
                static () => (ValidateResult.ErrorShowMessage, new LicenseData()),
                new CallbackSite(sink, typeof(ValidateProductKeyDelegate), nameof(ValidateProductKeyDelegate.Invoke)));
            return result;
        };

    private static OnLeaseChangedDelegate LeaseHandler(Func<Option<LeaseState>, IO<Unit>> leaseChanged, Option<Icon> icon, IPlugInSink sink) =>
        (args, out answer) => {
            Callbacks.Run(
                IO.pure(args).Bind(changed => leaseChanged(
                    from lease in Some(changed.Lease)
                    from leaseId in Conversions.Present(lease.LeaseId)
                    select LicenseMapper.ToLease(lease, leaseId, lease.IssuedAt.ToInstant(), lease.Expiration.ToInstant()))),
                new CallbackSite(sink, typeof(OnLeaseChangedDelegate), nameof(OnLeaseChangedDelegate.Invoke)));
            answer = icon.ValueUnsafe();
        };
}
