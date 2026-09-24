using System.Drawing;
using System.Runtime.InteropServices;
using LanguageExt.UnsafeValueAccess;
using Rasm.Rhino.Document;
using Rhino.Runtime;
using Rhino.Runtime.RhinoAccounts;

namespace Rasm.Rhino.Ui;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record HostState(
    string ProcessName,
    Version ProcessVersion,
    string OsEdition,
    string OsProductName,
    string OsBuildNumber,
    string OsInstallationType,
    uint OsLanguage,
    int ProcessorCount,
    Architecture ProcessArchitecture,
    bool DarkMode,
    bool Server,
    bool PreRelease,
    bool Mono,
    Seq<string> ReferenceAssemblies,
    Seq<string> SearchPaths);

public sealed record PrinterMargins(double LeftMillimeters, double TopMillimeters, double RightMillimeters, double BottomMillimeters);

public sealed record PrinterForm(string Name, Option<(double Width, double Height)> Millimeters, Option<PrinterMargins> Portrait, Option<PrinterMargins> Landscape);

public sealed record Printer(string Name, Option<double> HorizontalDpi, Option<double> VerticalDpi, Seq<PrinterForm> Forms);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Entitlement {
    public sealed record Entitled(Option<string> Signature) : Entitlement;

    public sealed record Denied(string Reason) : Entitlement;

    public sealed record NotApplicable() : Entitlement;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AssemblySource {
    public sealed record FromPath(string Path) : AssemblySource;

    public sealed record FromStream(Stream Stream) : AssemblySource;

    public sealed record FromName(System.Reflection.AssemblyName Name) : AssemblySource;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SkinEvent {
    public sealed record MainFrameWindowCreated() : SkinEvent;

    public sealed record LicenseCheckCompleted() : SkinEvent;

    public sealed record BuiltInCommandsRegistered() : SkinEvent;

    public sealed record BeginLoadAtStartPlugIns(int ExpectedCount) : SkinEvent;

    public sealed record BeginLoadPlugIn(string Description) : SkinEvent;

    public sealed record EndLoadPlugIn() : SkinEvent;

    public sealed record EndLoadAtStartPlugIns() : SkinEvent;

    public sealed record ShowSplash() : SkinEvent;

    public sealed record HideSplash() : SkinEvent;

    public sealed record ShowHelp() : SkinEvent;

    public sealed record ShowChooseTemplate() : SkinEvent;

    public sealed record ShowChooseRecent() : SkinEvent;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TokenRequest {
    public sealed record Acquire(string ClientId, string ClientSecret) : TokenRequest;

    public sealed record AcquireScoped(
        string ClientId,
        string ClientSecret,
        Seq<string> Scopes,
        Option<string> Prompt,
        Option<int> MaxAge,
        bool ShowUi,
        Option<IProgress<RhinoAccoountsProgressInfo>> Progress) : TokenRequest;

    public sealed record Cached(string ClientId, Option<Seq<string>> Scopes) : TokenRequest;
}

// --- [SERVICES] ------------------------------------------------------------------------
public abstract class CallbackSkin(Option<Bitmap> icon, Option<string> applicationName, Func<SkinEvent, IO<Unit>> onEvent, Action<Error> reject) : Skin {
    protected override Bitmap MainRhinoIcon => icon.IfNone(() => base.MainRhinoIcon);

    protected override string ApplicationName => applicationName.IfNone(() => base.ApplicationName);

    protected override void OnMainFrameWindowCreated() => _ = Answers.Answer(onEvent(new SkinEvent.MainFrameWindowCreated()), reject, unit);

    protected override void OnLicenseCheckCompleted() => _ = Answers.Answer(onEvent(new SkinEvent.LicenseCheckCompleted()), reject, unit);

    protected override void OnBuiltInCommandsRegistered() => _ = Answers.Answer(onEvent(new SkinEvent.BuiltInCommandsRegistered()), reject, unit);

    protected override void OnBeginLoadAtStartPlugIns(int expectedCount) => _ = Answers.Answer(onEvent(new SkinEvent.BeginLoadAtStartPlugIns(expectedCount)), reject, unit);

    protected override void OnBeginLoadPlugIn(string description) => _ = Answers.Answer(onEvent(new SkinEvent.BeginLoadPlugIn(description)), reject, unit);

    protected override void OnEndLoadPlugIn() => _ = Answers.Answer(onEvent(new SkinEvent.EndLoadPlugIn()), reject, unit);

    protected override void OnEndLoadAtStartPlugIns() => _ = Answers.Answer(onEvent(new SkinEvent.EndLoadAtStartPlugIns()), reject, unit);

    protected override void ShowSplash() => _ = Answers.Answer(onEvent(new SkinEvent.ShowSplash()), reject, unit);

    protected override void HideSplash() => _ = Answers.Answer(onEvent(new SkinEvent.HideSplash()), reject, unit);

    protected override void ShowHelp() => _ = Answers.Answer(onEvent(new SkinEvent.ShowHelp()), reject, unit);

    protected override void ShowChooseTemplate() => _ = Answers.Answer(onEvent(new SkinEvent.ShowChooseTemplate()), reject, unit);

    protected override void ShowChooseRecent() => _ = Answers.Answer(onEvent(new SkinEvent.ShowChooseRecent()), reject, unit);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Host {
    // --- [PROCESS]
    public static IO<HostState> Snapshot() =>
        IO.lift(static () => {
            HostUtils.GetCurrentProcessInfo(out string name, out Version version);
            return new HostState(
                name,
                version,
                HostUtils.OperatingSystemEdition,
                HostUtils.OperatingSystemProductName,
                HostUtils.OperatingSystemBuildNumber,
                HostUtils.OperatingSystemInstallationType,
                HostUtils.CurrentOSLanguage,
                HostUtils.GetSystemProcessorCount(),
                RuntimeInformation.ProcessArchitecture,
                HostUtils.RunningInDarkMode,
                HostUtils.RunningOnServer,
                HostUtils.IsPreRelease,
                HostUtils.RunningInMono,
                toSeq(HostUtils.GetSystemReferenceAssemblies()).Strict(),
                toSeq(HostUtils.GetAssemblySearchPaths()));
        });

    public static IO<Seq<Printer>> Printers() =>
        IO.lift(static () => toSeq(HostUtils.GetPrinterNames()).Map(ReadPrinter).Strict());

    public static IO<Entitlement> CheckEntitlement() =>
        from checkedEntitlement in IO.lift(CloudHostUtils.CheckEntitlement)
        from entitlement in IO.lift(static () => CloudHostUtils.IsEntitled
            ? new Entitlement.Entitled(Optional(CloudHostUtils.Signature))
            : Optional(CloudHostUtils.DenyReason).Match<Entitlement>(
                Some: static reason => new Entitlement.Denied(reason),
                None: static () => new Entitlement.NotApplicable()))
        select entitlement;

    private static Printer ReadPrinter(string printer) =>
        new(
            printer,
            Dpi(printer, horizontal: true),
            Dpi(printer, horizontal: false),
            toSeq(HostUtils.GetPrinterFormNames(printer))
                .Map(form => new PrinterForm(
                    form,
                    Answers.Found(HostUtils.GetPrinterFormSize(printer, form, out double width, out double height), (Width: width, Height: height)),
                    Margins(printer, form, portrait: true),
                    Margins(printer, form, portrait: false)))
                .Strict());

    private static Option<PrinterMargins> Margins(string printer, string form, bool portrait) =>
        Answers.Found(
            HostUtils.GetPrinterFormMargins(printer, form, portrait, out double left, out double top, out double right, out double bottom),
            new PrinterMargins(left, top, right, bottom));

    private static Option<double> Dpi(string printer, bool horizontal) =>
        Some(HostUtils.GetPrinterDPI(printer, horizontal)).Filter(static dpi => dpi != 0.0);

    // --- [ASSEMBLIES]
    public static IO<System.Reflection.Assembly> Load(AssemblySource source) =>
        IO.lift(() => source.Switch(
            fromPath: static path => Missing.Unless(HostUtils.LoadAssemblyFrom(path.Path), nameof(HostUtils.LoadAssemblyFrom)),
            fromStream: static stream => Missing.Unless(HostUtils.LoadAssemblyFromStream(stream.Stream), nameof(HostUtils.LoadAssemblyFromStream)),
            fromName: static name => Missing.Unless(HostUtils.LoadAssemblyFromName(name.Name), nameof(HostUtils.LoadAssemblyFromName))));

    // --- [ACCOUNTS]
    public static IO<(IOpenIDConnectToken OpenId, IOAuth2Token OAuth)> Tokens(TokenRequest request) =>
        request.Switch(
            acquire: static acquire => Protected(async (key, token) =>
                Present(await RhinoAccountsManager.GetAuthTokensAsync(acquire.ClientId, acquire.ClientSecret, key, token).ConfigureAwait(false), nameof(RhinoAccountsManager.GetAuthTokensAsync))),
            acquireScoped: static withScopes => Protected(async (key, token) =>
                Present(
                    await RhinoAccountsManager.GetAuthTokensAsync(
                        withScopes.ClientId,
                        withScopes.ClientSecret,
                        withScopes.Scopes,
                        withScopes.Prompt.ValueUnsafe(),
                        withScopes.MaxAge.ToNullable(),
                        withScopes.ShowUi,
                        withScopes.Progress.ValueUnsafe(),
                        key,
                        token).ConfigureAwait(false),
                    nameof(RhinoAccountsManager.GetAuthTokensAsync))),
            cached: static cached => Protected((key, _) => Task.FromResult(Present(
                cached.Scopes.Match(
                    Some: scopes => RhinoAccountsManager.TryGetAuthTokens(cached.ClientId, scopes, key),
                    None: () => RhinoAccountsManager.TryGetAuthTokens(cached.ClientId, key)),
                nameof(RhinoAccountsManager.TryGetAuthTokens)))));

    public static IO<IOpenIDConnectToken> Refresh(IOpenIDConnectToken current, IOAuth2Token oauth) =>
        Protected(async (key, token) =>
            Missing.Unless(await RhinoAccountsManager.UpdateOpenIDConnectTokenAsync(current, oauth, key, token).ConfigureAwait(false), nameof(RhinoAccountsManager.UpdateOpenIDConnectTokenAsync)));

    public static IO<Unit> Revoke(IOAuth2Token oauth) =>
        Protected<Unit>(async (key, token) => {
            await RhinoAccountsManager.RevokeAuthTokenAsync(oauth, key, token).ConfigureAwait(false);
            return unit;
        });

    private static IO<TValue> Protected<TValue>(Func<SecretKey, CancellationToken, Task<Fin<TValue>>> body) =>
        IO.liftAsync(env => Answers.CapturedAsync<TValue>(
                capture => RhinoAccountsManager.ExecuteProtectedCodeAsync(async key => capture(await body(key, env.Token).ConfigureAwait(false))),
                nameof(RhinoAccountsManager.ExecuteProtectedCodeAsync)))
            .Bind(static answer => IO.lift(answer));

    private static Fin<(IOpenIDConnectToken OpenId, IOAuth2Token OAuth)> Present(Tuple<IOpenIDConnectToken, IOAuth2Token> pair, string member) =>
        pair is (not null, not null) ? (OpenId: pair.Item1, OAuth: pair.Item2) : new Missing(member);
}
