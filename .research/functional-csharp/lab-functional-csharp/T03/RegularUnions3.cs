namespace Lab.T03;

[Union]
[UnionSwitchMapOverload(StopAt = [typeof(Failure)])]
internal abstract partial class RequestOutcome {
    internal sealed class Success : RequestOutcome;

    [Union]
    internal abstract partial class Failure : RequestOutcome {
        internal sealed class NotFound : Failure;
        internal sealed class Unauthorized : Failure;
    }
}

internal static class RequestOutcomes {
    public static int StatusCode(RequestOutcome outcome) => outcome.Map(failureUnauthorized: 401, failureNotFound: 404, success: 200);
    public static string Group(RequestOutcome outcome) => outcome.Map(success: "ok", failure: "failed");
}
