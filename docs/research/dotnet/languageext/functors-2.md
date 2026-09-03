<!-- Ideas integrated into .claude/skills/dotnet-coding-languageext/references/traits-and-transformers.md on the real types, the toy Maybe, List, and MaybeT code enters no skill
# [FUNCTORS]

## [01]-[MAPPING_PROBLEM]

Mapping traits cannot fix the element type. Mapping an `A` must produce a `B` while it preserves the surrounding type constructor. The desired shape is not valid C#, because a type parameter `F` cannot be applied as `F<A>`:

```csharp
public interface Mappable<F>
    where F : Mappable<F>
{
    static abstract F<B> Select<A, B>(F<A> list, Func<A, B> f);
}
```

`K<F, A>` encodes the missing relationship:

```csharp
public interface Mappable<F>
    where F : Mappable<F>
{
    static abstract K<F, B> Select<A, B>(K<F, A> fa, Func<A, B> f);
}
```

`K` is short for "kind". `F` is the structure, and `A` is the value type carried by that structure. Mapping replaces `A` with `B` without changing `F`.

## [02]-[LIST_IMPLEMENTATION]

The generic data type represents the data, a non-generic sibling type implements the capability:

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

Because `List<A>` implements `K<List, A>`, the implementation recovers the concrete list with a downcast. Context-specific extensions keep the cast in one place:

```csharp
public static class ListExtensions
{
    public static List<A> As<A>(this K<List, A> ma) =>
        (List<A>)ma;
}
```

```csharp
public class List : Mappable<List>
{
    public static K<List, B> Select<A, B>(
        K<List, A> list,
        Func<A, B> f) =>
        new List<B>(list.As().Items.Select(f).ToArray());
}
```

The cast relies on an invariant: exactly one concrete type derives from `K<List, A>`. Two representations for the same `F` and `A` make the downcast fail on use.

## [03]-[MAP_EXTENSION]

The trait makes one generic extension possible:

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

Every data type that implements `K<F, A>` with a sibling `F : Mappable<F>` gains the same LINQ `Select`:

```csharp
var list = new List<int>([1, 2, 3]);

var nlist = list.Select(x => x + 1)
                .Select(x => x * 2);
```

The result stays abstract: `Select` returns `K<List, int>`, not `List<int>`, and calls continue to compose in that form. Recover the concrete list only where it is needed:

```csharp
List<int> nlist = list.Select(x => x + 1)
                      .Select(x => x * 2)
                      .As();
```

The cost is one cast at the concrete boundary. Staying in `K<F, A>` avoids repeated `As` calls.

## [04]-[MAYBE_FUNCTOR]

The same extension serves a different structure once that structure supplies its own trait implementation:

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

`Maybe<A>` has the cases of LanguageExt's `Option<A>`: `Just` corresponds to `Some`, and `Nothing` to `None`. Mapping transforms the `Just` value and preserves `Nothing`:

```csharp
var mx = new Just<int>(100);
var my = new Nothing<int>();

var r1 = mx.Select(x => x + 1)
           .Select(x => x * 3); // Just(303)

var r2 = my.Select(x => x + 1)
           .Select(x => x * 3); // Nothing
```

## [05]-[GENERIC_FUNCTIONS]

Per-type extension methods give mapping syntax, they do not let one function serve every mappable structure. The trait constraint does:

```csharp
public static K<F, int> Foo<F>(K<F, string> ma)
    where F : Mappable<F> =>
    ma.Select(x => x.Length);
```

`Foo` works for any `F : Mappable<F>` instead of requiring separate `List<string>` and `Maybe<string>` versions.

## [06]-[MAPPABLE_AS_FUNCTOR]

The conventional name for this capability is `Functor`, and its conventional operation is `Map`:

```csharp
public interface Functor<F>
    where F : Functor<F>
{
    static abstract K<F, B> Map<A, B>(K<F, A> fa, Func<A, B> f);
}
```

The shape corresponds to the higher-kinded definition:

```text
class Functor f where
  fmap :: (a -> b) -> f a -> f b
```

- `Functor f` is `Functor<F>`
- `f a` is `K<F, A>`
- `f b` is `K<F, B>`
- `a -> b` is `Func<A, B>`

Only the argument order differs in this illustrative C# definition, the implemented API places the function first.

## [07]-[SHAPE_AND_CAPABILITY]

The data types stay descriptions of shape:

```csharp
public record List<A>(A[] Items) : K<List, A>;

public abstract record Maybe<A> : K<Maybe, A>;

public record Just<A>(A Value) : Maybe<A>;
public record Nothing<A>() : Maybe<A>;
```

The sibling types `List` and `Maybe` carry the trait implementations, as type-class instances do: the data type is the shape, the trait implementation is a capability. Behavior kept out of the data representation leaves the structure free to cross parallel-processing and serialization boundaries.

## [08]-[HIGHER_KINDED_ABSTRACTION]

Abstraction over type constructors removes the duplication that ordinary generics remove over value types. Without the encoding, operations that involve two constructors have no general form: C# cannot express `T<F<A>>` with `T` traversable and `F` applicative. It can encode the nesting as:

```text
K<T, K<F, A>>
```

Each type implements its traversable trait once. User-defined traversable and applicative types then compose with LanguageExt's types instead of requiring a new cross-product of specialized functions.
-->
