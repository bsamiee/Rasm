namespace Lab.F13;

internal sealed record Customer(string Name, AccountState Account);

internal static class Lenses {
    public static readonly Lens<Customer, AccountState> Account =
        Lens<Customer, AccountState>.New(static customer => customer.Account, static account => customer => customer with { Account = account });

    public static readonly Lens<AccountState, decimal> AllowedOverdraft =
        Lens<AccountState, decimal>.New(static account => account.AllowedOverdraft, static overdraft => account => account.With(allowedOverdraft: overdraft));

    public static readonly Lens<Customer, decimal> CustomerOverdraft = lens(Account, AllowedOverdraft);

    public static Customer Raise(Customer customer, decimal amount) =>
        CustomerOverdraft.Update(overdraft => overdraft + amount, customer);

    public static Customer Reset(Customer customer) =>
        CustomerOverdraft.Set(0m, customer);
}
