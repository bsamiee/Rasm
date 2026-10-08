using Rasm.Rhino.Document;
using Rhino.Runtime;
using Rhino.Runtime.RhinoAccounts;

namespace Rasm.Rhino.Ui;

// --- [MODELS] --------------------------------------------------------------------------
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

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Host {
    // --- [ASSEMBLIES]
    public static IO<System.Reflection.Assembly> Load(AssemblySource source) =>
        IO.lift(() => source.Switch(
            fromPath: static path => Missing.Unless(HostUtils.LoadAssemblyFrom(path.Path), nameof(HostUtils.LoadAssemblyFrom)),
            fromStream: static stream => Missing.Unless(HostUtils.LoadAssemblyFromStream(stream.Stream), nameof(HostUtils.LoadAssemblyFromStream)),
            fromName: static name => Missing.Unless(HostUtils.LoadAssemblyFromName(name.Name), nameof(HostUtils.LoadAssemblyFromName))));

    // --- [ACCOUNTS]
    public static IO<Entitlement> CheckEntitlement() =>
        from checkedEntitlement in IO.lift(CloudHostUtils.CheckEntitlement)
        from entitlement in IO.lift(static () => CloudHostUtils.IsEntitled
            ? new Entitlement.Entitled(Optional(CloudHostUtils.Signature))
            : Optional(CloudHostUtils.DenyReason).Match<Entitlement>(
                Some: static reason => new Entitlement.Denied(reason),
                None: static () => new Entitlement.NotApplicable()))
        select entitlement;

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
        IO.liftAsync(env => CapturedAsync<TValue>(
                capture => RhinoAccountsManager.ExecuteProtectedCodeAsync(async key => capture(await body(key, env.Token).ConfigureAwait(false))),
                nameof(RhinoAccountsManager.ExecuteProtectedCodeAsync)))
            .Bind(static answer => IO.lift(answer));

    private static async Task<Fin<T>> CapturedAsync<T>(Func<Action<Fin<T>>, Task> host, string member) {
        Option<Fin<T>> answer = None;
        await host(value => answer = Some(value)).ConfigureAwait(false);
        return answer.IfNone(new CallbackSkipped(member));
    }

    private static Fin<(IOpenIDConnectToken OpenId, IOAuth2Token OAuth)> Present(Tuple<IOpenIDConnectToken, IOAuth2Token> pair, string member) =>
        pair is (not null, not null) ? (OpenId: pair.Item1, OAuth: pair.Item2) : new Missing(member);
}
