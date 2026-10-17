using System.Drawing;
using Rhino;
using Rhino.Runtime;
using Rhino.Runtime.RhinoAccounts;

namespace Rasm.Rhino.PlugIns.Licenses;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record AccountClient(string ClientId, string ClientSecret);

[Union]
public abstract partial record Entitlement {
    public sealed record Entitled(Option<string> Signature) : Entitlement;

    public sealed record Denied(string Reason) : Entitlement;

    public sealed record NotApplicable() : Entitlement;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class AccountOps {
    // --- [ACCOUNT]
    public static IO<Option<T>> LoggedInUser<T>(Func<string, Image, T> read) =>
        IO.lift(static () => Conversions.Present(RhinoApp.LoggedInUserName))
            .Bind(name => name.TraverseM(present => DisposalOps.Within(static () => RhinoApp.LoggedInUserAvatar, avatar => IO.pure(read(present, avatar)))).As());

    // --- [TOKENS]
    public static IO<Option<(IOpenIDConnectToken OpenId, IOAuth2Token OAuth)>> TryGetAuthTokens(AccountClient client) =>
        Protected(key => Optional(RhinoAccountsManager.TryGetAuthTokens(client.ClientId, key)).Map(static pair => pair.ToValueTuple()));

    public static IO<Option<(IOpenIDConnectToken OpenId, IOAuth2Token OAuth)>> TryGetAuthTokens(AccountClient client, Seq<string> scope) =>
        Protected(key => Optional(RhinoAccountsManager.TryGetAuthTokens(client.ClientId, scope, key)).Map(static pair => pair.ToValueTuple()));

    public static IO<(IOpenIDConnectToken OpenId, IOAuth2Token OAuth)> GetAuthTokens(AccountClient client) =>
        Protected((key, token) => RhinoAccountsManager.GetAuthTokensAsync(client.ClientId, client.ClientSecret, key, token))
            .Map(static pair => pair.ToValueTuple());

    public static IO<(IOpenIDConnectToken OpenId, IOAuth2Token OAuth)> GetAuthTokens(
        AccountClient client, Seq<string> scope, Option<string> prompt, Option<Duration> maxAge, bool showUI, Option<IProgress<RhinoAccoountsProgressInfo>> progress) =>
        IO.lift(maxAge.Traverse(static age => Conversions.Whole(age, Duration.FromSeconds(1))).As())
            .Bind(seconds => Protected((key, token) => RhinoAccountsManager.GetAuthTokensAsync(
                    client.ClientId, client.ClientSecret, scope, prompt.ValueUnsafe(), seconds.ToNullable(), showUI, progress.ValueUnsafe(), key, token)))
            .Map(static pair => pair.ToValueTuple());

    public static IO<(IOpenIDConnectToken OpenId, IOAuth2Token OAuth)> UpdateOpenIDConnectToken((IOpenIDConnectToken OpenId, IOAuth2Token OAuth) tokens) =>
        Protected((key, token) => RhinoAccountsManager.UpdateOpenIDConnectTokenAsync(tokens.OpenId, tokens.OAuth, key, token))
            .Map(openId => tokens with { OpenId = openId });

    public static IO<Unit> RevokeAuthToken(IOAuth2Token oauth) =>
        Protected((key, token) => RhinoAccountsManager.RevokeAuthTokenAsync(oauth, key, token).ToUnit());

    private static IO<T> Protected<T>(Func<SecretKey, T> call) =>
        Callbacks.Captured<SecretKey, T>(RhinoAccountsManager.ExecuteProtectedCode, key => IO.lift(() => call(key)), nameof(RhinoAccountsManager.ExecuteProtectedCode));

    private static IO<T> Protected<T>(Func<SecretKey, CancellationToken, Task<T>> call) =>
        cancelToken.Bind(token => Callbacks.Captured<T>(
            answer => IO.liftAsync(() => RhinoAccountsManager.ExecuteProtectedCodeAsync(async key => answer(await call(key, token).ConfigureAwait(false))).ToUnit()),
            nameof(RhinoAccountsManager.ExecuteProtectedCodeAsync)));

    // --- [ENTITLEMENT]
    public static IO<Entitlement> CheckEntitlement() =>
        IO.lift(CloudHostUtils.CheckEntitlement).Map(static _ => CloudHostUtils.IsEntitled
            ? new Entitlement.Entitled(Optional(CloudHostUtils.Signature))
            : Optional(CloudHostUtils.DenyReason).Match<Entitlement>(
                Some: static reason => new Entitlement.Denied(reason),
                None: static () => new Entitlement.NotApplicable()));
}
