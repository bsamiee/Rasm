namespace Lab.F13;

internal static class Ledgers {
    public static Map<string, AccountState> Open(Map<string, AccountState> accounts, string id, AccountState state) =>
        accounts.Add(id, state);

    public static Option<AccountState> Current(Map<string, AccountState> accounts, string id) =>
        accounts.Find(id);

    public static Map<string, AccountState> Replace(Map<string, AccountState> accounts, string id, AccountState state) =>
        accounts.SetItem(id, state);
}
