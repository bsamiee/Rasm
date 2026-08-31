namespace Lab.F24;

internal sealed record Address(string City, string Postcode);

internal sealed record Customer(string Name, Address Address);

internal static class Lenses {
    public static readonly Lens<Customer, Address> AddressOf =
        Lens<Customer, Address>.New(static customer => customer.Address, static address => customer => customer with { Address = address });

    public static readonly Lens<Address, string> PostcodeOf =
        Lens<Address, string>.New(static address => address.Postcode, static postcode => address => address with { Postcode = postcode });

    public static readonly Lens<Customer, string> CustomerPostcode = lens(AddressOf, PostcodeOf);

    public static Customer Moved(Customer customer, string postcode) => CustomerPostcode.Set(postcode, customer);

    public static Customer Uppercased(Customer customer) => CustomerPostcode.Update(static postcode => postcode.ToUpperInvariant(), customer);

    public static string Read(Customer customer) => CustomerPostcode.Get(customer);
}
