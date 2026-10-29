using System.Drawing;
using Rhino;
using Rhino.Runtime;
using Rhino.Runtime.RhinoAccounts;

namespace Rasm.Rhino.PlugIns.Licenses;

// --- [MODELS] --------------------------------------------------------------------------
[Union]
public abstract partial record Entitlement {
    public sealed record Entitled(Option<string> Signature) : Entitlement;

    public sealed record Denied(string Reason) : Entitlement;

    public sealed record NotApplicable : Entitlement;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Accounts {
    // --- [ACCOUNT]
    public static IO<Option<T>> LoggedInUser<T>(Func<string, Image, T> read) =>
        IO.lift(static () => Conversions.Present(RhinoApp.LoggedInUserName))
            .Bind(name => name.TraverseM(present => Products.Of(static () => RhinoApp.LoggedInUserAvatar).Map(avatar => read(present, avatar)).Bracket()).As());

    // --- [TOKENS]
    public static IO<Option<(IOpenIDConnectToken OpenId, IOAuth2Token OAuth)>> TryGetAuthTokens(string clientId, Option<Seq<string>> scope = default) =>
        Protected(key => Optional(scope.Match(
            Some: requested => RhinoAccountsManager.TryGetAuthTokens(clientId, requested, key),
            None: () => RhinoAccountsManager.TryGetAuthTokens(clientId, key))).Map(static pair => (pair.Item1, pair.Item2)));

    public static IO<(IOpenIDConnectToken OpenId, IOAuth2Token OAuth)> GetAuthTokens(string clientId, string clientSecret) =>
        Protected((key, token) => RhinoAccountsManager.GetAuthTokensAsync(clientId, clientSecret, key, token)).Map(static pair => (pair.Item1, pair.Item2));

    public static IO<(IOpenIDConnectToken OpenId, IOAuth2Token OAuth)> GetAuthTokens(
        string clientId, string clientSecret, Seq<string> scope, Option<string> prompt, Option<Duration> maxAge, bool showUI, Option<IProgress<RhinoAccoountsProgressInfo>> progress) =>
        IO.lift(maxAge.Traverse(static age => Conversions.Whole(age, Duration.FromSeconds(1))).As())
            .Bind(seconds => Protected((key, token) => RhinoAccountsManager.GetAuthTokensAsync(
                clientId, clientSecret, scope, prompt.ValueUnsafe(), seconds.ToNullable(), showUI, progress.ValueUnsafe(), key, token)))
            .Map(static pair => (pair.Item1, pair.Item2));

    public static IO<IOpenIDConnectToken> UpdateOpenIDConnectToken(IOpenIDConnectToken openId, IOAuth2Token oauth) =>
        Protected((key, token) => RhinoAccountsManager.UpdateOpenIDConnectTokenAsync(openId, oauth, key, token));

    public static IO<Unit> RevokeAuthToken(IOAuth2Token oauth) =>
        Protected((key, token) => RhinoAccountsManager.RevokeAuthTokenAsync(oauth, key, token).ToUnit());

    private static IO<T> Protected<T>(Func<SecretKey, T> call) =>
        Products.Captured<SecretKey, T>(RhinoAccountsManager.ExecuteProtectedCode, key => IO.lift(() => call(key)));

    private static IO<T> Protected<T>(Func<SecretKey, CancellationToken, Task<T>> call) =>
        cancelToken.Bind(token => Products.Captured<T>(
            answer => IO.liftAsync(() => RhinoAccountsManager.ExecuteProtectedCodeAsync(async key => answer(await call(key, token).ConfigureAwait(false))).ToUnit())));

    // --- [ENTITLEMENT]
    public static IO<Entitlement> CheckEntitlement =>
        IO.lift(CloudHostUtils.CheckEntitlement).Map(static _ => CloudHostUtils.IsEntitled
            ? new Entitlement.Entitled(Conversions.Present(CloudHostUtils.Signature))
            : Conversions.Present(CloudHostUtils.DenyReason).Match<Entitlement>(
                Some: static reason => new Entitlement.Denied(reason),
                None: static () => new Entitlement.NotApplicable()));
}
