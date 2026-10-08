namespace Rasm.Rhino.Plugin.Licenses;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    AccountsBusy = 1,
    AccountsUnreachable,
    AccountsFailed,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record AccountsBusy : Expected {
    public AccountsBusy(string member, Error cause) : base("{Member} found another Rhino Accounts operation running", (int)Codes.AccountsBusy, cause) => Member = member;

    public string Member { get; }
}

public sealed record AccountsUnreachable : Expected {
    public AccountsUnreachable(string member, Error cause) : base("{Member} could not reach the Rhino Accounts server", (int)Codes.AccountsUnreachable, cause) => Member = member;

    public string Member { get; }
}

public sealed record AccountsFailed : Expected {
    public AccountsFailed(string member, Error cause) : base("{Member} failed in Rhino Accounts", (int)Codes.AccountsFailed, cause) => Member = member;

    public string Member { get; }
}
