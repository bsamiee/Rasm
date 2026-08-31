# [FOLDABLES]

`Foldable<F>` abstracts aggregation over a structure. The structure decides which values participate and in what order; the caller supplies an initial state and a binary step that reduces those values to one result.

## [01]-[PRIMITIVE_OPERATIONS]

```csharp
public interface Foldable<F>
    where F : Foldable<F>
{
    static abstract S Fold<A, S>(
        K<F, A> fa,
        S initial,
        Func<S, A, S> step);
    static abstract S FoldBack<A, S>(
        K<F, A> fa,
        S initial,
        Func<S, A, S> step);
}
```

- `Fold` visits values from first to last.
- `FoldBack` visits the same values from last to first.
- `initial` is returned unchanged when the structure contributes no value.
- `step` receives the current state and the next value, and returns the next state.

For values `a`, `b`, `c`, seed `s`, and step `f`:

```text
Fold:     f(f(f(s, a), b), c)
FoldBack: f(f(f(s, c), b), a)
```

With this state-first `Func<S, A, S>` API, `FoldBack` is an accumulation over the reverse traversal.

The direction matters whenever `step` is not commutative:

```csharp
var forward = values.Fold("", (s, x) => $"{s}{x}");
var reverse = values.FoldBack("", (s, x) => $"{s}{x}");
```

For `[a, b, c]`, the results are `"abc"` and `"cba"`.

## [02]-[HIGHER_KINDED_REPRESENTATION]

`K<F, A>` separates the shape `F` from its contained value type `A`. A concrete data type participates by deriving from the matching higher-kinded marker, while a witness type implements the capability.

```csharp
public abstract record Either<L, R> : K<Either<L>, R>;

public record Left<L, R>(L Value) : Either<L, R>;
public record Right<L, R>(R Value) : Either<L, R>;

public class Either<L> :
    Functor<Either<L>>,
    Foldable<Either<L>>
{
    public static K<Either<L>, B> Map<A, B>(
        K<Either<L>, A> value,
        Func<A, B> f) =>
        value switch
        {
            Left<L, A> l => new Left<L, B>(l.Value),
            Right<L, A> r => new Right<L, B>(f(r.Value))
        };
    public static S Fold<A, S>(
        K<Either<L>, A> value,
        S initial,
        Func<S, A, S> step) =>
        value switch
        {
            Left<L, A> => initial,
            Right<L, A> r => step(initial, r.Value)
        };
    public static S FoldBack<A, S>(
        K<Either<L>, A> value,
        S initial,
        Func<S, A, S> step) =>
        Fold(value, initial, step);
}
```

The witness removes the concrete type's final value parameter and lets each trait method supply it. Consequently, `Either<L, R>` uses the partially applied witness `Either<L>`. The same pattern gives `List<A>` the witness `List` and `Maybe<A>` the witness `Maybe`.

This arrangement adds capabilities without modifying the concrete value type. One witness may implement several traits, such as `Functor<F>` and `Foldable<F>`.

## [03]-[SHAPE_SEMANTICS]

Implementations must preserve the meaning of their structure:
- A list contributes every element in list order; `FoldBack` reverses that traversal.
- `Just<A>` contributes its value once; `Nothing<A>` contributes none.
- `Right<L, A>` contributes its right value once; `Left<L, A>` contributes none.
- For either a zero-element or one-element shape, `Fold` and `FoldBack` produce the same result because traversal direction cannot change the visit sequence.

The fold removes the outer structure. A left value or absence is not converted into an arbitrary `A`; it simply leaves the state unchanged.

## [04]-[GENERIC_DISPATCH]

Extensions expose natural call syntax while dispatching to the concrete witness selected by `F`:

```csharp
public static S Fold<F, A, S>(
    this K<F, A> fa,
    S initial,
    Func<S, A, S> step)
    where F : Foldable<F> =>
    F.Fold(fa, initial, step);

public static S FoldBack<F, A, S>(
    this K<F, A> fa,
    S initial,
    Func<S, A, S> step)
    where F : Foldable<F> =>
    F.FoldBack(fa, initial, step);
```

Keep the two dispatch paths distinct: the `FoldBack` extension must call `F.FoldBack`, not `F.Fold`.

The witness selected by `F` retains knowledge of the concrete representation even though the caller is generic.

## [05]-[DERIVED_OPERATIONS]

Many useful operations can be defined once in terms of `Fold`:

```csharp
static virtual bool IsEmpty<A>(K<F, A> fa) =>
    F.Fold(fa, true, (_, _) => false);

static virtual int Count<A>(K<F, A> fa) =>
    F.Fold(fa, 0, (n, _) => n + 1);

static virtual A Sum<A>(K<F, A> fa)
    where A : INumber<A> =>
    F.Fold(fa, A.Zero, (total, x) => total + x);

static virtual bool All<A>(K<F, A> fa, Func<A, bool> predicate) =>
    F.Fold(fa, true, (result, x) => result && predicate(x));

static virtual bool Any<A>(K<F, A> fa, Func<A, bool> predicate) =>
    F.Fold(fa, false, (result, x) => result || predicate(x));

static virtual bool Contains<A>(K<F, A> fa, A value)
    where A : IEquatable<A> =>
    F.Any(fa, x => x.Equals(value));
```

The seed determines empty-structure behavior:
- `Count` and numeric `Sum` return zero.
- `All` returns `true`, because no value disproves the predicate.
- `Any` returns `false`, because no value satisfies the predicate.
- `IsEmpty` remains `true` until a value is encountered.

For `Nothing<int>`, `Just<int>(100)`, and `List<int>([1, 2, 3, 4, 5])`, `Count` returns `0`, `1`, and `5`, while `Sum` returns `0`, `100`, and `15`.

A monoidal element type supplies both pieces required for seedless-looking aggregation: its identity becomes the seed and its associative operation becomes the step.

```csharp
static virtual A Fold<A>(K<F, A> fa)
    where A : Monoid<A> =>
    F.Fold(fa, A.Empty, (state, x) => state + x);
```

Conversion to an enumerable is also derivable. The default implementation builds a list during the fold.

```csharp
static virtual IEnumerable<A> AsEnumerable<A>(K<F, A> fa)
{
    var result = new System.Collections.Generic.List<A>();
    return F.Fold(fa, result, (items, x) =>
    {
        items.Add(x);
        return items;
    });
}
```

## [06]-[DEFAULTS_AND_SPECIALIZATION]

Put universally correct implementations on `Foldable<F>` as `static virtual` members. Every witness receives the complete operation family immediately. Override only when the representation enables materially better behavior.

For an array-backed list:

```csharp
public static int Count<A>(K<List, A> values) =>
    values.As().Items.Length;

public static bool Any<A>(K<List, A> values, Func<A, bool> predicate)
{
    foreach (var item in values.As().Items)
        if (predicate(item)) return true;

    return false;
}
```

Useful specializations include:
- `Map` by preallocating the result array and filling it in one pass.
- `Count` and `IsEmpty` from a stored length, avoiding traversal.
- `AsEnumerable` from the backing array, avoiding allocation and copying.
- `All` and `Any` as loops that stop as soon as the answer is known.
- `Fold` as a forward loop and `FoldBack` as an index-based reverse loop, avoiding intermediate reversal and iterator overhead.

The defaults are already close to the natural implementation for zero-or-one-value shapes such as `Maybe` and `Either`. Representation-specific overrides are most valuable for structures such as an array-backed list.

The default fold-derived `All`, `Any`, and `IsEmpty` still visit the whole structure because a strict fold cannot stop the traversal. Boolean short-circuiting inside `step` may skip later predicate calls, but it does not stop enumeration. A witness override is required for true early exit.

Specialization must preserve results, traversal direction, empty behavior, and predicate evaluation order. Optimize the representation, not the semantics.

## [07]-[ABSTRACTION_AND_REPRESENTATION]

The higher-kinded marker and witness keep generic code abstract while letting each structure provide specialized implementations. A generic function can target any `Foldable<F>`; dispatch still reaches that `F`'s optimized trait members.

This differs from exposing a concrete collection only as `IEnumerable<A>`. Once viewed through that interface, generic extensions principally see an enumerator, not representation-specific facts such as an array's stored length or direct indexing. A foldable witness can preserve those facts behind the common capability and use them for `Count`, reverse traversal, conversion, and early exit.

The practical sequence is therefore:
1. Implement `Fold` and `FoldBack` for the shape.
2. Receive the complete default operation family.
3. Write generic functions against `Foldable<F>`.
4. Specialize trait members later where the representation provides a real advantage.
