# Higher Kinds in C# with LanguageExt

## Static interface members as traits

Static abstract interface members let a generic constraint describe operations that belong to a type rather than an instance:

```csharp
public interface Addable<SELF>
    where SELF : Addable<SELF>
{
    public static abstract SELF Add(SELF x, SELF y);
}
```

The recursive constraint requires `SELF` to implement `Addable<SELF>`. An implementing type therefore passes its own concrete type to the trait. This is opt-in, trait-like polymorphism rather than ad-hoc polymorphism: the type itself must declare the implementation.

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

Generic code can call the static member through its constrained type parameter:

```csharp
A AddAnything<A>(A x, A y)
    where A : Addable<A> =>
    A.Add(x, y);
```

Because the operation belongs to the type, the trait can also expose an identity value without requiring an existing instance:

```csharp
public interface Addable<SELF>
    where SELF : Addable<SELF>
{
    public static abstract SELF Empty { get; }
    public static abstract SELF Add(SELF x, SELF y);
}
```

The two implementations use an empty array-backed list and an empty string as their identities:

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

That supports generic folds. `FoldMap` maps each input to an `Addable` value, starts from `Empty`, and combines the mapped values with `Add`:

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

This generalizes folding and concatenation across otherwise unrelated types. The trait members return the concrete value rather than a potentially boxed `Addable<A>` interface value.

## Semigroups and monoids

`Addable` has the structure of a monoid. A semigroup provides an associative binary operation; a monoid extends it with an identity element:

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

In LanguageExt version 5, a type becomes a semigroup or monoid by implementing the corresponding trait. This is a shift from the ad-hoc polymorphism used in earlier versions.

The tradeoff is ownership: a type that cannot be modified cannot be made to implement these traits directly. For example, `string` and integer types cannot retroactively become monoids. The workaround is to place the external value in a small owned wrapper that implements the needed trait, then convert at the point where monoidal behavior is required.

## Why the self-type pattern cannot express mapping

The self-typed trait works when every operation stays within one concrete type. Mapping is different because it must be able to change the element type while retaining the surrounding shape.

A trait that takes only `SELF` cannot connect the element stored by a concrete implementation to the arbitrary input type of `Select`:

```csharp
public interface Mappable<SELF>
    where SELF : Mappable<SELF>
{
    public static abstract SELF Select<A, B>(
        SELF list,
        Func<A, B> f);
}
```

If `MyList<X>` implements this interface, it has no way to turn its stored `X` values into the unrelated `A` values expected by `f`.

Putting the source element type on the trait gets closer, but fixes the entire result to `SELF`:

```csharp
public interface Mappable<SELF, A>
    where SELF : Mappable<SELF, A>
{
    public static abstract SELF Select<B>(
        SELF list,
        Func<A, B> f);
}
```

For `SELF = MyList<A>`, `Select` must return `MyList<A>`, while applying `Func<A, B>` requires `MyList<B>`. The interface can name a fully applied list type, but not the list constructor independently of its element type.

## The missing type-constructor parameter

The relevant distinction is:

```text
Option<int>  - a fully concrete type
Option<A>    - a type constructor applied to a parameter
F<A>         - an arbitrary type constructor applied to a parameter
```

A parameterized type can be viewed as a type-level function: it accepts a type and produces a type. C# can parameterize the `A` in a known type such as `Option<A>`, but it cannot receive the `Option` part as a type parameter `F` and later form `F<A>`.

This limitation also helps explain C#'s compiler-recognized method patterns. Features involving `Select`, `SelectMany`, `Where`, `Join`, `GroupJoin`, `GetEnumerator`, `GetAwaiter`, collection `Add`, index initializers, and collection initializers depend on specially recognized members rather than one general, discoverable trait mechanism.

## `K<F, A>`

LanguageExt version 5 introduces this empty interface:

```csharp
public interface K<F, A>;
```

Its importance is not in members - it has none - but in giving C# a uniform representation with both a type-constructor parameter `F` and an element type `A`.

This representation enables users to define their own functors, applicatives, traversables, foldables, monads, and monad transformers, with those implementations gaining default behavior defined for the traits. It also allows the removal of 300,000 lines of generated and handwritten code that had previously simulated generalized traits.

`K<F, A>` is therefore the small type-level encoding on which LanguageExt version 5 builds higher-rank polymorphism and higher kinds in C#.
