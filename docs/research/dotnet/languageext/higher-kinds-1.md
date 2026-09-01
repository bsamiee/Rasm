# [HIGHER_KINDS]

## [01]-[STATIC_INTERFACE_MEMBERS]

Static abstract interface members let a generic constraint describe operations that belong to a type rather than an instance:

```csharp
public interface Addable<SELF>
    where SELF : Addable<SELF>
{
    public static abstract SELF Add(SELF x, SELF y);
}
```

The recursive constraint requires `SELF` to implement `Addable<SELF>`, an implementing type passes its own concrete type to the trait. This is opt-in, trait-like polymorphism: the type itself declares the implementation.

```csharp
public record MyList<A>(A[] values) : Addable<MyList<A>>
{
    public static MyList<A> Add(MyList<A> x, MyList<A> y) =>
        new(x.values.Concat(y.values).ToArray());
}

public record MyString(string value) : Addable<MyString>
{
    public static MyString Add(MyString x, MyString y) =>
        new(x.value + y.value);
}
```

Generic code calls the static member through the constrained type parameter:

```csharp
A AddAnything<A>(A x, A y)
    where A : Addable<A> =>
    A.Add(x, y);
```

Because the operation belongs to the type, the trait can expose an identity value without an existing instance:

```csharp
public interface Addable<SELF>
    where SELF : Addable<SELF>
{
    public static abstract SELF Empty { get; }
    public static abstract SELF Add(SELF x, SELF y);
}
```

The implementations use an empty array-backed list and an empty string as their identities:

```csharp
public record MyList<A>(A[] values) : Addable<MyList<A>>
{
    public static MyList<A> Empty { get; } = new([]);
    public static MyList<A> Add(MyList<A> x, MyList<A> y) =>
        new(x.values.Concat(y.values).ToArray());
}

public record MyString(string value) : Addable<MyString>
{
    public static MyString Empty { get; } = new("");
    public static MyString Add(MyString x, MyString y) =>
        new(x.value + y.value);
}
```

`FoldMap` maps each input to an `Addable` value, starts from `Empty`, and combines the mapped values with `Add`:

```csharp
B FoldMap<A, B>(IEnumerable<A> xs, Func<A, B> f)
    where B : Addable<B>
{
    var result = B.Empty;
    foreach (var x in xs)
        result = B.Add(result, f(x));
    return result;
}

A Concat<A>(IEnumerable<A> xs)
    where A : Addable<A> =>
    FoldMap(xs, x => x);
```

One definition covers folding and concatenation across otherwise unrelated types. The trait members return the concrete value, not a boxed `Addable<A>` interface value.

## [02]-[SEMIGROUPS_AND_MONOIDS]

`Addable` has the structure of a monoid. Semigroups provide an associative binary operation; monoids extend them with an identity element:

```csharp
public interface Semigroup<A>
    where A : Semigroup<A>
{
    public static abstract A operator +(A x, A y);
}

public interface Monoid<A> : Semigroup<A>
    where A : Monoid<A>
{
    public static abstract A Empty { get; }
}
```

Types become semigroups or monoids by implementing the trait. Types outside your control, such as `string` or an integer type, cannot implement it retroactively. Place the external value in a small owned wrapper that implements the trait, and convert where monoidal behavior is required.

## [03]-[SELF_TYPE_LIMITATION]

The self-typed trait works while every operation stays within one concrete type. Mapping must change the element type while it keeps the surrounding shape, and the self-typed trait cannot express that. Traits over `SELF` alone cannot connect the stored element to the input type of `Select`:

```csharp
public interface Mappable<SELF>
    where SELF : Mappable<SELF>
{
    public static abstract SELF Select<A, B>(
        SELF list,
        Func<A, B> f);
}
```

If `MyList<X>` implements this interface, nothing turns its stored `X` values into the unrelated `A` values that `f` expects. Putting the source element type on the trait fixes the entire result to `SELF`:

```csharp
public interface Mappable<SELF, A>
    where SELF : Mappable<SELF, A>
{
    public static abstract SELF Select<B>(
        SELF list,
        Func<A, B> f);
}
```

For `SELF = MyList<A>`, `Select` must return `MyList<A>`, while applying `Func<A, B>` requires `MyList<B>`. The interface can name a fully applied list type, but not the list constructor apart from its element type.

## [04]-[TYPE_CONSTRUCTOR_PARAMETER]

```text
Option<int>  - a fully concrete type
Option<A>    - a type constructor applied to a parameter
F<A>         - an arbitrary type constructor applied to a parameter
```

Parameterized types are type-level functions: they take a type and produce a type. C# can parameterize the `A` in a known type such as `Option<A>`. C# cannot receive the `Option` part as a type parameter `F` and later form `F<A>`. The same limitation explains C#'s compiler-recognized method patterns: `Select`, `SelectMany`, `Where`, `Join`, `GroupJoin`, `GetEnumerator`, `GetAwaiter`, collection `Add`, index initializers, and collection initializers bind to specially recognized members, not to one general trait mechanism.

## [05]-[K_INTERFACE]

LanguageExt defines one empty interface:

```csharp
public interface K<F, A>;
```

`K<F, A>` has no members. Its value is the uniform representation with a type-constructor parameter `F` and an element type `A`. On this encoding, LanguageExt builds higher-rank polymorphism and higher kinds in C#, and users define their own functors, applicatives, foldables, traversables, monads, and monad transformers that inherit the default behavior defined for the traits.
