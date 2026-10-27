using System.Drawing;
using Eto.Forms;
using Rasm.RhinoCommon.UI.Styling;
using Rhino.PlugIns;
using Riok.Mapperly.Abstractions;

namespace Rasm.RhinoCommon.PlugIns.Licenses;

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
    public static IO<Unit> Request(IPlugInLifetime plugIn, LicenseDefinition license, Func<LicenseCapabilities, string?, ValidateProductKeyDelegate, OnLeaseChangedDelegate, bool> getLicense) =>
        Asked(plugIn, license, (validate, leaseChanged) => getLicense(license.Capabilities, license.TextMask.ValueUnsafe(), validate, leaseChanged));

    public static IO<Unit> AskUser(
        IPlugInLifetime plugIn, LicenseDefinition license, LicenseBuildType build, Option<Control> parent,
        Func<LicenseBuildType, bool, string?, object?, ValidateProductKeyDelegate, OnLeaseChangedDelegate, bool> askUserForLicense) =>
        Asked(plugIn, license, (validate, leaseChanged) => askUserForLicense(build, false, license.TextMask.ValueUnsafe(), parent.ValueUnsafe(), validate, leaseChanged));

    private static IO<Unit> Asked(IPlugInLifetime plugIn, LicenseDefinition license, Func<ValidateProductKeyDelegate, OnLeaseChangedDelegate, bool> ask) =>
        Icons.PlugInImage(plugIn).Bind(static glyph => glyph.Traverse(static art => IconSlot.Banner.Pixels.Bind(pixels => Icons.DrawingIcon(art, pixels))).As())
            .Bind(icon => IO.lift(() => guard(ask(Validator(license.Validate, plugIn), LeaseHandler(license.LeaseChanged, icon, plugIn)), Errors.Cancelled).ToFin()));

    // --- [RELEASE]
    public static IO<Unit> Return(Func<bool> returnLicense) =>
        IO.lift(() => RhinoFault.Unless(returnLicense()));

    // --- [STATUS]
    public static IO<Seq<LicenseState>> GetLicenseStatus() =>
        IO.lift(static () => Conversions.Rows(LicenseUtils.GetLicenseStatus()).Map(LicenseMapper.ToState).Strict());

    public static IO<Option<LicenseState>> GetOneLicenseStatus(Guid productId) =>
        IO.lift(() => Optional(LicenseUtils.GetOneLicenseStatus(productId)).Map(LicenseMapper.ToState));

    // --- [CALLBACKS]
    private static ValidateProductKeyDelegate Validator(Func<string, IO<LicenseData>> validate, IPlugInLifetime plugIn) =>
        (productKey, out licenseData) => {
            (ValidateResult result, licenseData) = plugIn.Answer(
                IO.pure(productKey).Bind(validate)
                    .Map(static data => (ValidateResult.Success, data))
                    .Catch(static error => error.Is(Errors.Cancelled), static _ => IO.pure((ValidateResult.ErrorHideMessage, new LicenseData())))
                    .Catch(static error => error.IsExpected, static error => IO.pure((ValidateResult.ErrorShowMessage, new LicenseData { ErrorMessage = ErrorOps.Messages(error) }))),
                (ValidateResult.ErrorShowMessage, new LicenseData()));
            return result;
        };

    private static OnLeaseChangedDelegate LeaseHandler(Func<Option<LeaseState>, IO<Unit>> leaseChanged, Option<Icon> icon, IPlugInLifetime plugIn) =>
        (args, out answer) => {
            _ = plugIn.Run(
                IO.pure(args).Bind(changed => leaseChanged(
                    from lease in Some(changed.Lease)
                    from leaseId in Conversions.Present(lease.LeaseId)
                    select LicenseMapper.ToLease(lease, leaseId, lease.IssuedAt.ToInstant(), lease.Expiration.ToInstant()))));
            answer = icon.ValueUnsafe();
        };
}
