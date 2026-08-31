namespace Lab.T07;

internal static class Comparers {
    public static IReadOnlySet<ProductName> ExactNames(ProductName name) => SingleItem.Set(name, Thinktecture.Collections.StringKeyedObjectComparer<ProductName>.Ordinal);
    public static IReadOnlySet<User> ByName(User user) => SingleItem.Set(user, new Thinktecture.Collections.ProjectionEqualityComparer<User, string>(static u => u.Name, StringComparer.OrdinalIgnoreCase));
}
