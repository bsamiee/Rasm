using System.Drawing;
using Rasm.Rhino.UI.Styling;
using Rhino;
using Rhino.PlugIns;
using Rhino.Runtime;

namespace Rasm.Rhino.PlugIns.Licenses;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record LicenseDefinition(
    LicenseCapabilities Capabilities,
    Option<string> TextMask,
    Func<string, IO<LicenseData>> Validate,
    Func<Option<LicenseLease>, IO<Unit>> LeaseChanged);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Licensing {
    // --- [REQUEST]
    public static IO<Unit> Request(LicenseDefinition license, Func<LicenseCapabilities, string?, ValidateProductKeyDelegate, OnLeaseChangedDelegate, bool> getLicense) =>
        from glyph in Icons.PlugInImage()
        from icon in glyph.Traverse(static art => IconSlot.Banner.Pixels.Bind(pixels => Icons.DrawingIcon(art, pixels))).As()
        from granted in IO.lift(() => guard(getLicense(license.Capabilities, license.TextMask.ValueUnsafe(), Validator(license.Validate), LeaseHandler(license.LeaseChanged, icon)), Errors.Cancelled).ToFin())
        select unit;

    // --- [STATUS]
    public static HostEvent<Option<LicenseStatus>> Status(Guid productId) =>
        Subscriptions.On<LicenseStateChangedEventArgs>(static h => RhinoApp.LicenseStateChanged += h, static h => RhinoApp.LicenseStateChanged -= h)
            .Choose(_ => Some(Optional(LicenseUtils.GetOneLicenseStatus(productId))));

    // --- [CALLBACKS]
    private static ValidateProductKeyDelegate Validator(Func<string, IO<LicenseData>> validate) =>
        (productKey, out licenseData) => {
            (ValidateResult result, licenseData) = Session.Run(
                IO.pure(productKey).Bind(validate)
                    .Map(static data => (ValidateResult.Success, data))
                    .Catch(static error => error.Is(Errors.Cancelled), static _ => IO.pure((ValidateResult.ErrorHideMessage, new LicenseData())))
                    .Catch(static error => error.IsExpected, static error => IO.pure((ValidateResult.ErrorShowMessage, new LicenseData { ErrorMessage = Session.Messages(error) }))),
                (ValidateResult.ErrorShowMessage, new LicenseData()));
            return result;
        };

    private static OnLeaseChangedDelegate LeaseHandler(Func<Option<LicenseLease>, IO<Unit>> leaseChanged, Option<Icon> icon) =>
        (args, out answer) => {
            _ = Session.Run(IO.lift(() => Some(args.Lease).Filter(static lease => !string.IsNullOrEmpty(lease.LeaseId))).Bind(leaseChanged));
            answer = icon.ValueUnsafe();
        };
}
