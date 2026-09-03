<!-- Ideas integrated into .claude/skills/dotnet-languageext/references/traits-and-transformers.md on the real types, the toy Maybe, List, and MaybeT code enters no skill
# [FOLDABLES]

`Foldable<F>` abstracts aggregation over a structure. The structure decides which values participate and in what order, the caller supplies an initial state and a binary step that reduces those values to one result.

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

- `Fold` visits values from first to last
- `FoldBack` visits the same values from last to first
- `initial` returns unchanged when the structure contributes no value
- `step` receives the current state and the next value and returns the next state

For values `a`, `b`, `c`, seed `s`, and step `f`:

```text
Fold:     f(f(f(s, a), b), c)
FoldBack: f(f(f(s, c), b), a)
```

With this state-first `Func<S, A, S>` API, `FoldBack` is an accumulation over the reverse traversal. Direction matters whenever `step` is not commutative:

```csharp
var forward = values.Fold("", (s, x) => $"{s}{x}");
var reverse = values.FoldBack("", (s, x) => $"{s}{x}");
```

For `[a, b, c]`, the results are `"abc"` and `"cba"`.

## [02]-[HIGHER_KINDED_REPRESENTATION]

`K<F, A>` separates the shape `F` from the contained value type `A`. Concrete data types derive from the matching higher-kinded marker, a witness type implements the capability.

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

The witness removes the concrete type's final value parameter and lets each trait method supply it. `Either<L, R>` uses the partially applied witness `Either<L>`. The same pattern gives `List<A>` the witness `List` and `Maybe<A>` the witness `Maybe`. The arrangement adds capabilities without modifying the concrete value type, and one witness implements several traits (`Functor<F>`, `Foldable<F>`).

## [03]-[SHAPE_SEMANTICS]

Implementations preserve the meaning of their structure:
- Lists contribute every element in list order, `FoldBack` reverses that traversal
- `Just<A>` contributes its value once, `Nothing<A>` contributes none
- `Right<L, A>` contributes its right value once, `Left<L, A>` contributes none
- For a zero-element or one-element shape, `Fold` and `FoldBack` agree, because direction cannot change the visit sequence

The fold removes the outer structure. Left values and absence never become an arbitrary `A`, the state stays unchanged.

## [04]-[GENERIC_DISPATCH]

Extensions expose natural call syntax and dispatch to the witness selected by `F`:

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

Keep the dispatch paths distinct: the `FoldBack` extension calls `F.FoldBack`, not `F.Fold`. The witness retains knowledge of the concrete representation while the caller stays generic.

## [05]-[DERIVED_OPERATIONS]

`Fold` defines a family of operations once:

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
- `Count` and numeric `Sum` return zero
- `All` returns `true`, because no value disproves the predicate
- `Any` returns `false`, because no value satisfies the predicate
- `IsEmpty` stays `true` until a value is encountered

For `Nothing<int>`, `Just<int>(100)`, and `List<int>([1, 2, 3, 4, 5])`, `Count` returns `0`, `1`, and `5`, and `Sum` returns `0`, `100`, and `15`.

Monoidal element types supply both pieces of seedless aggregation: the identity is the seed and the associative operation is the step.

```csharp
static virtual A Fold<A>(K<F, A> fa)
    where A : Monoid<A> =>
    F.Fold(fa, A.Empty, (state, x) => state + x);
```

Conversion to an enumerable also derives from the fold:

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

Put universally correct implementations on `Foldable<F>` as `static virtual` members. Every witness receives the complete operation family, override only where the representation enables materially better behavior. For an array-backed list:

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

Useful specializations:
- `Map` by preallocating the result array and filling it in one pass
- `Count` and `IsEmpty` from a stored length, without traversal
- `AsEnumerable` from the backing array, without allocation and copying
- `All` and `Any` as loops that stop as soon as the answer is known
- `Fold` as a forward loop and `FoldBack` as an index-based reverse loop, without intermediate reversal and iterator overhead

The defaults are close to the natural implementation for zero-or-one-value shapes (`Maybe`, `Either`). Representation-specific overrides pay off most for array-backed structures. The default fold-derived `All`, `Any`, and `IsEmpty` visit the whole structure, because a strict fold cannot stop the traversal. Boolean short-circuiting inside `step` skips later predicate calls, it does not stop enumeration. True early exit requires a witness override.

Specialization preserves results, traversal direction, empty behavior, and predicate evaluation order. Optimize the representation, not the semantics.

## [07]-[ABSTRACTION_AND_REPRESENTATION]

Generic functions target any `Foldable<F>`. Dispatch still reaches that `F`'s optimized trait members. This differs from exposing a collection as `IEnumerable<A>`: through that interface, generic extensions see an enumerator and lose representation facts (a stored length, direct indexing). Foldable witnesses keep those facts behind the common capability and use them for `Count`, reverse traversal, conversion, and early exit.

The working sequence:
1. Implement `Fold` and `FoldBack` for the shape
2. Receive the complete default operation family
3. Write generic functions against `Foldable<F>`
4. Specialize trait members where the representation provides a real advantage
-->
