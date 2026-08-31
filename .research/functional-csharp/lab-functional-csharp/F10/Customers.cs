namespace Lab.F10;

internal sealed record FlaggedCustomer(string Email, bool IsRegistered, string Name, bool IsEligible);

[Union]
internal abstract partial record Customer {
    internal sealed record RegisteredCustomer(string Name, string Email, bool IsEligible) : Customer;
    internal sealed record GuestCustomer(string Email) : Customer;
}
