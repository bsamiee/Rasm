using System.Drawing;
using LanguageExt.UnsafeValueAccess;
using Rasm.Rhino.Document;
using Rhino.PlugIns;
using Riok.Mapperly.Abstractions;

[assembly: UseStaticMapper(typeof(Answers))]

namespace Rasm.Rhino.Plugin;

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
    DateTime IssuedAt,
    DateTime Expiration,
    Option<DateTime> RenewableUntil) {
    public static IO<Option<LeaseState>> Read(LicenseLease lease) =>
        IO.lift(() => Answers.Present(lease.LeaseId).Map(leaseId => RegistryMapper.ToState(lease, leaseId)));
}

public sealed record PlugInState(
    Guid Id,
    Option<string> Name,
    Option<string> Description,
    Option<string> FileName,
    PlugInType PlugInType,
    PlugInLoadTime PlugInLoadTime,
    bool IsLoaded,
    bool ShipsWithRhino,
    bool IsDotNet,
    Seq<string> CommandNames,
    Seq<string> FileTypeDescriptions,
    Seq<string> FileTypeExtensions,
    Option<bool> LoadSilently,
    bool LoadProtected);

public sealed record LicenseState(
    Guid PluginId,
    Guid ProductId,
    LicenseBuildType BuildType,
    Option<string> LicenseTitle,
    Option<string> SerialNumber,
    LicenseType LicenseType,
    Option<DateTime> ExpirationDate,
    Option<DateTime> CheckOutExpirationDate,
    Option<string> RegisteredOwner,
    Option<string> RegisteredOrganization,
    bool CloudZooLeaseIsValid,
    Option<DateTime> CloudZooLeaseExpiration);

public readonly record struct FileTypeRow(Guid PlugInId, string Description, Seq<string> Extensions);

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record LoadRefused(string Path, LoadPlugInResult Result) : Expected("LoadPlugIn returned {Result} for {Path}", ErrorOps.Code<LoadRefused>());

public sealed record Unloaded(Guid Id) : Expected("Plug-in {Id} is installed and not loaded", ErrorOps.Code<Unloaded>()) {
    public static Fin<Unit> Unless(bool loaded, Guid id) => loaded ? unit : new Unloaded(id);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class PlugInRegistry {
    // --- [LOADING]
    public static IO<(LoadPlugInResult Result, Option<Guid> Id)> LoadPlugIn(string path) =>
        from existing in Answers.ExistingPath(path)
        from loaded in IO.lift(() => {
            LoadPlugInResult result = PlugIn.LoadPlugIn(existing, out Guid id);
            return result is LoadPlugInResult.Success or LoadPlugInResult.SuccessAlreadyLoaded
                ? Fin.Succ((Result: result, Id: Answers.Present(id)))
                : new LoadRefused(existing, result);
        })
        select loaded;

    public static IO<Unit> LoadPlugIn(Guid id, bool loadQuietly, bool forceLoad) =>
        IO.lift(() => Refused.Unless(PlugIn.LoadPlugIn(id, loadQuietly, forceLoad), nameof(PlugIn.LoadPlugIn)));

    // --- [READS]
    public static IO<Option<PlugInState>> ReadPlugIn(Guid id) =>
        IO.lift(() => PlugIn.PlugInExists(id, out _, out bool loadProtected)
            ? Missing.Unless(PlugIn.GetPlugInInfo(id), nameof(PlugIn.GetPlugInInfo))
                .Map(info => Some(RegistryMapper.ToState(info, Answers.Found(PlugIn.GetLoadProtection(id, out bool loadSilently), loadSilently), loadProtected)))
            : Option<PlugInState>.None);

    public static IO<Seq<FileTypeRow>> InstalledFileTypes(PlugInType type) =>
        IO.lift(() => toSeq(PlugIn.GetInstalledPlugIns().Keys)
            .Choose(static id => Optional(PlugIn.GetPlugInInfo(id)))
            .Filter(info => info.PlugInType.HasFlag(type))
            .Bind(static info => toSeq(info.FileTypeDescriptions).Zip(
                toSeq(info.FileTypeExtensions),
                (description, extensions) => new FileTypeRow(info.Id, description, toSeq(extensions.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)))))
            .Strict());

    // --- [SETTINGS]
    public static IO<Unit> SavePluginSettings(Guid id) =>
        from info in IO.lift(() => Missing.Unless(PlugIn.GetPlugInInfo(id), nameof(PlugIn.GetPlugInInfo)))
        from loaded in IO.lift(() => Unloaded.Unless(info.IsLoaded, id))
        from saved in IO.lift(() => PlugIn.SavePluginSettings(id))
        select saved;

    // --- [LICENSING]
    public static ValidateProductKeyDelegate Validator(Func<string, IO<LicenseData>> validate) =>
        (productKey, out licenseData) => {
            (ValidateResult result, licenseData) = validate(productKey).RunSafe().Match(
                Succ: static data => (ValidateResult.Success, data),
                Fail: static error => error.IsType<Canceled>()
                    ? (ValidateResult.ErrorHideMessage, new LicenseData())
                    : (ValidateResult.ErrorShowMessage, new LicenseData { ErrorMessage = ErrorOps.Localize(error) }));
            return result;
        };

    public static OnLeaseChangedDelegate LeaseChangedHandler(Func<Option<LeaseState>, IO<Option<Icon>>> leaseChanged, Action<Error> reject) =>
        (args, out icon) => icon = Answers.Answer(
            IO.lift(() => args.Lease).Bind(LeaseState.Read).Bind(leaseChanged),
            reject,
            Option<Icon>.None).ValueUnsafe();

    public static IO<Unit> ShowLicenseValidationUi(string cdKey) =>
        IO.lift(() => Refused.Unless(LicenseUtils.ShowLicenseValidationUi(cdKey), nameof(LicenseUtils.ShowLicenseValidationUi)));

    public static IO<Seq<LicenseState>> GetLicenseStatus() =>
        IO.lift(static () => toSeq(LicenseUtils.GetLicenseStatus()).Map(RegistryMapper.ToState).Strict());

    public static IO<Option<LicenseState>> GetOneLicenseStatus(Guid productId) =>
        IO.lift(() => Optional(LicenseUtils.GetOneLicenseStatus(productId)).Map(RegistryMapper.ToState));

    public static IO<Unit> CheckOutLicense(Guid productId) =>
        IO.lift(() => Refused.Unless(LicenseUtils.CheckOutLicense(productId), nameof(LicenseUtils.CheckOutLicense)));

    public static IO<Unit> CheckInLicense(Guid productId) =>
        IO.lift(() => Refused.Unless(LicenseUtils.CheckInLicense(productId), nameof(LicenseUtils.CheckInLicense)));

    public static IO<Unit> ReturnLicense(Guid productId) =>
        IO.lift(() => Refused.Unless(LicenseUtils.ReturnLicense(productId), nameof(LicenseUtils.ReturnLicense)));

    public static IO<Unit> ConvertLicense(Guid productId) =>
        IO.lift(() => Refused.Unless(LicenseUtils.ConvertLicense(productId), nameof(LicenseUtils.ConvertLicense)));

    public static IO<Unit> DeleteLicense(Guid productId) =>
        IO.lift(() => Refused.Unless(LicenseUtils.DeleteLicense(productId), nameof(LicenseUtils.DeleteLicense)));

    public static IO<Unit> LoginToCloudZoo() =>
        IO.lift(static () => Refused.Unless(LicenseUtils.LoginToCloudZoo(), nameof(LicenseUtils.LoginToCloudZoo)));

    public static IO<Unit> LogoutOfCloudZoo() =>
        IO.lift(static () => Refused.Unless(LicenseUtils.LogoutOfCloudZoo(), nameof(LicenseUtils.LogoutOfCloudZoo)));
}

[Mapper]
internal static partial class RegistryMapper {
    internal static partial PlugInState ToState(PlugInInfo info, Option<bool> loadSilently, bool loadProtected);

    internal static partial LicenseState ToState(LicenseStatus status);

    [MapperIgnoreSource(nameof(LicenseLease.LeaseId), Justification = "Resolved to the leaseId parameter")]
    internal static partial LeaseState ToState(LicenseLease lease, string leaseId);

    [UserMapping]
    private static Option<DateTime> Present(DateTime? date) => Optional(date);
}
