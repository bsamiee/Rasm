# [FUNCTORS]

## [01]-[MAPPING_PROBLEM]

A mapping trait cannot bake the element type into the trait itself. Mapping a value of type `A` must be able to produce a value of type `B`, while preserving the surrounding type constructor.

The desired shape would be:

```csharp
public interface Mappable<F>
    where F : Mappable<F>
{
    static abstract F<B> Select<A, B>(F<A> list, Func<A, B> f);
}
```

This is not valid C#: a type parameter such as `F` cannot be applied as `F<A>` or `F<B>`. C# does not directly support higher-kinded type parameters.

`K<F, A>` supplies an encoding for that missing relationship:

```csharp
public interface Mappable<F>
    where F : Mappable<F>
{
    static abstract K<F, B> Select<A, B>(K<F, A> fa, Func<A, B> f);
}
```

`K` is short for "kind." `F` acts as an anchor for the structure, while `A` is the value type carried by that structure. Mapping replaces `A` with `B` without changing `F`.

## [02]-[LIST_IMPLEMENTATION]

The generic data type represents the data, while a non-generic sibling type implements the capability:

```csharp
public record List<A>(A[] Items) : K<List, A>;

public class List : Mappable<List>
{
    public static K<List, B> Select<A, B>(
        K<List, A> list,
        Func<A, B> f) =>
        new List<B>(((List<A>)list)
            .Items
            .Select(f)
            .ToArray());
}
```

Because `List<A>` implements `K<List, A>`, the implementation can recover the concrete list with a downcast. A context-specific extension keeps that cast in one place:

```csharp
public static class ListExtensions
{
    public static List<A> As<A>(this K<List, A> ma) =>
        (List<A>)ma;
}
```

The mapping implementation then becomes:

```csharp
public class List : Mappable<List>
{
    public static K<List, B> Select<A, B>(
        K<List, A> list,
        Func<A, B> f) =>
        new List<B>(list.As().Items.Select(f).ToArray());
}
```

The cast relies on an invariant: only one concrete type should derive from `K<List, A>`. Defining another representation for the same `F` and `A` would make the downcast fail on use.

## [03]-[MAP_EXTENSION]

The trait makes a single generic extension possible:

```csharp
public static class MappableExtensions
{
    public static K<F, B> Select<F, A, B>(
        this K<F, A> fa,
        Func<A, B> f)
        where F : Mappable<F> =>
        F.Select(fa, f);
}
```

Any data type that implements `K<F, A>` and has a sibling `F` implementing `Mappable<F>` gains the same LINQ `Select` operation:

```csharp
var list = new List<int>([1, 2, 3]);

var nlist = list.Select(x => x + 1)
                .Select(x => x * 2);
```

The result remains abstract: `Select` returns `K<List, int>`, not `List<int>`. Calls can continue to compose in that abstract form. A concrete list is recovered only where it is needed:

```csharp
List<int> nlist = list.Select(x => x + 1)
                      .Select(x => x * 2)
                      .As();
```

The compromise is a small cast at the concrete boundary. Staying in the abstract `K<F, A>` form usually avoids repeated calls to `As`.

## [04]-[MAYBE_FUNCTOR]

The same extension works for a different structure once that structure supplies its own trait implementation:

```csharp
public abstract record Maybe<A> : K<Maybe, A>;
public record Just<A>(A Value) : Maybe<A>;
public record Nothing<A>() : Maybe<A>;

public class Maybe : Mappable<Maybe>
{
    public static K<Maybe, B> Select<A, B>(
        K<Maybe, A> maybe,
        Func<A, B> f) =>
        maybe switch
        {
            Just<A>(var x) => new Just<B>(f(x)),
            Nothing<A> => new Nothing<B>()
        };
}

public static class MaybeExtensions
{
    public static Maybe<A> As<A>(this K<Maybe, A> ma) =>
        (Maybe<A>)ma;
}
```

`Maybe<A>` has the same two cases as language-ext's `Option<A>`: `Just` corresponds to `Some`, and `Nothing` to `None`. Mapping transforms the `Just` value and preserves `Nothing`:

```csharp
var mx = new Just<int>(100);
var my = new Nothing<int>();

var r1 = mx.Select(x => x + 1)
           .Select(x => x * 3); // Just(303)

var r2 = my.Select(x => x + 1)
           .Select(x => x * 3); // Nothing
```

## [05]-[GENERIC_FUNCTIONS]

Per-type extension methods could provide mapping syntax, but they would not let one function work across all mappable structures. The trait constraint does:

```csharp
public static K<F, int> Foo<F>(K<F, string> ma)
    where F : Mappable<F> =>
    ma.Select(x => x.Length);
```

`Foo` works for any `F` that implements `Mappable<F>` rather than requiring separate `List<string>` and `Maybe<string>` versions.

## [06]-[MAPPABLE_AS_FUNCTOR]

The conventional name for this capability is `Functor`, and its conventional operation is `Map`:

```csharp
public interface Functor<F>
    where F : Functor<F>
{
    static abstract K<F, B> Map<A, B>(K<F, A> fa, Func<A, B> f);
}
```

Its shape corresponds directly to the higher-kinded definition:

```text
class Functor f where
  fmap :: (a -> b) -> f a -> f b
```

The types align as follows:
- `Functor f` corresponds to `Functor<F>`.
- `f a` corresponds to `K<F, A>`.
- `f b` corresponds to `K<F, B>`.
- `a -> b` corresponds to `Func<A, B>`.

Only the argument order differs in this illustrative C# definition; the implemented API places the function first.

This encoding is not as general as type-system lambda abstraction in a language with native higher kinds, but it provides higher-rank polymorphism for traits. The C#-specific requirement is that each data type derive from `K<F, A>`.

## [07]-[SHAPE_AND_CAPABILITY]

The data types remain simple descriptions of shape:

```csharp
public record List<A>(A[] Items) : K<List, A>;

public abstract record Maybe<A> : K<Maybe, A>;

public record Just<A>(A Value) : Maybe<A>;
public record Nothing<A>() : Maybe<A>;
```

The sibling types `List` and `Maybe` carry the trait implementations. This resembles type-class instances: the data type represents the shape, and the trait implementation represents a capability. Keeping behavior out of the data representation leaves the structure easier to move through parallel processing and serialization boundaries.

## [08]-[HIGHER_KINDED_ABSTRACTION]

Moving from concrete generic types to abstractions over type constructors avoids the same kind of duplication that ordinary generics avoid over value types.

Without this encoding, operations involving multiple constructors cannot be written in their general form. For example, C# cannot express `T<F<A>>` with `T` as a traversable structure and `F` as an applicative structure. It can encode that nesting as:

```text
K<T, K<F, A>>
```

With the higher-kinded encoding, each type can implement its traversable trait once. User-defined traversable and applicative types can then compose with language-ext's types instead of requiring a new cross-product of specialized functions.
