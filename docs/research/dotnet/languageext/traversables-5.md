# [TRAVERSABLES]

## [01]-[OPERATION]

`Map` preserves the nesting produced by an effectful transformation:

```csharp
Seq<Option<int>> mapped =
    Seq("1", "2", "X").Map(parseInt);
// [Some(1), Some(2), None]
```

`Traverse` transforms every value and flips the two structures:

```csharp
Option<Seq<int>> parsed =
    Seq("1", "2", "X")
        .Traverse(parseInt)
        .As();
// None
```

The general transformation is:

```text
Traverse : (A -> F<B>) -> T<A> -> F<T<B>>
```

- `T` is the traversed structure.
- `F` is the effect produced for each value.
- The behavior used to combine results comes from `F`.

This is useful whenever `Map` leaves effects inside a structure but the next operation needs one effect containing the complete structure. Unlike a fold, traversal does not expose an explicit accumulator: the applicative's normal behavior performs the accumulation while the two structures are flipped.

## [02]-[OUTER_APPLICATIVE_SEMANTICS]

`Traverse` does not define one universal failure or evaluation policy. It repeatedly combines values through the selected `Applicative<F>`:

| [INDEX] | [TYPE]       | [BEHAVIOR]                                                                                  |
| :-----: | :----------- | :------------------------------------------------------------------------------------------ |
|  [01]   | `Option`     | Any `None` makes the whole result `None`.                                                   |
|  [02]   | `Validation` | All failures are collected.                                                                 |
|  [03]   | `Eff`        | File-reading effects in the example run in parallel; one failure fails the whole traversal. |

The transformation should therefore match the dependency structure:

```csharp
Seq<string> paths = Seq(
    @"C:\input\a.txt",
    @"C:\input\b.txt",
    @"C:\input\c.txt");

// For this Eff example, Traverse evaluates the reads in parallel.
var parallel = paths.Traverse(File<Runtime>.readAllText);

// Seq's monadic variant evaluates them serially.
var serial = paths.TraverseM(File<Runtime>.readAllText);
```

## [03]-[TRAVERSE_AND_SEQUENCE]

`Sequence` is `Traverse` with the identity function when the input is already nested:

```text
Traverse(f, T<A>)  -> F<T<B>>    where f : A -> F<B>
Sequence(T<F<A>>)  -> F<T<A>>
```

The trait supplies the derived operations:

```csharp
public static virtual K<F, K<T, A>> Sequence<F, A>(
    K<T, K<F, A>> ta)
    where F : Applicative<F> =>
    Traversable.traverse(x => x, ta);

public static virtual K<M, K<T, B>> TraverseM<M, A, B>(
    Func<A, K<M, B>> f,
    K<T, A> ta)
    where M : Monad<M> =>
    Traversable.traverse(f, ta);

public static virtual K<M, K<T, A>> SequenceM<M, A>(
    K<T, K<M, A>> ta)
    where M : Monad<M> =>
    Traversable.traverseM(x => x, ta);
```

`Sequence` and `SequenceM` perform the same structural flip, but their constraints select applicative or monadic composition. A type may override the defaults when it can provide materially better semantics or performance; `Seq<A>` overrides `TraverseM` to guarantee serial evaluation.

### [03.1]-[PREFER_TRAVERSE]

C# cannot generally convert a concrete nested value such as `Seq<Option<int>>` to the nested higher-kinded form `K<Seq, K<Option, int>>`. Consequently, this fails at compile time:

```csharp
var values = Seq(Some(1), Some(2), None);
// values.Sequence();
```

The practical equivalent keeps the inner conversion visible to inference:

```csharp
Option<Seq<int>> result = values.Traverse(x => x).As();
```

Use `Sequence` directly when the input is already represented as `K<T, K<F, A>>`; otherwise, use `Traverse(x => x)`.

## [04]-[HIGHER_KINDED_CONTRACT]

```csharp
public interface Traversable<T> : Functor<T>, Foldable<T>
    where T : Traversable<T>, Functor<T>, Foldable<T>
{
    static abstract K<F, K<T, B>> Traverse<F, A, B>(
        Func<A, K<F, B>> f,
        K<T, A> ta)
        where F : Applicative<F>;
}
```

There are two independent type constructors:
- `T` can map over values and fold its shape.
- `F` can lift values with `Pure` and combine independent computations with `Apply`.
- `K<T, A>` is the traversed value in higher-kinded form.
- `K<F, K<T, B>>` is the transformed `T<B>` nested inside the chosen effect.

The contract allows one `Traverse` implementation for a type to compose with every applicative supported by the trait system.

## [05]-[COLLECTION_TRAVERSABLE]

A sequence implementation folds from the back, starts with an empty sequence lifted into `F`, and applicatively prepends each transformed item:

```csharp
static K<F, K<Seq, B>> Traverse<F, A, B>(
    Func<A, K<F, B>> f,
    K<Seq, A> ta)
    where F : Applicative<F>
{
    K<F, Seq<B>> folded = Foldable.foldBack(
        cons,
        F.Pure(empty<B>()),
        ta);

    return F.Map<Seq<B>, K<Seq, B>>(xs => xs, folded);

    K<F, Seq<B>> cons(K<F, Seq<B>> xs, A x) =>
        Applicative.lift(Prelude.Cons, f(x), xs);
}
```

Important details:
1. `foldBack` uses `Cons` to prepend each mapped item to the accumulated sequence.
2. `F.Pure(empty<B>())` is the result for an empty input.
3. `Applicative.lift` combines `f(x)` and the accumulated sequence through `F.Apply`; this is where failure accumulation, short-circuiting, or concurrency enters.
4. The final `F.Map` widens the concrete inner `Seq<B>` to `K<Seq, B>` explicitly.

Keeping `Seq<B>` concrete during the fold avoids calling `.As()` and `.Kind()` for every item. One outer `Map` is normally cheaper than repeated boxing of a value-type collection. Prefer the explicit, correct widening over variance tricks that compile but fail through nested runtime casts.

## [06]-[ALTERNATIVE_TRAVERSABLE]

For a type with success and failure cases, traverse only the case that contains a transformable value:

```csharp
static K<F, K<Either<L>, B>> Traverse<F, A, B>(
    Func<A, K<F, B>> f,
    K<Either<L>, A> value)
    where F : Applicative<F> =>
    value switch
    {
        Either.Right<L, A>(var right) =>
            F.Map<B, K<Either<L>, B>>(
                x => Either<L, B>.Right(x),
                f(right)),

        Either.Left<L, A>(var left) =>
            F.Pure<K<Either<L>, B>>(Either<L, B>.Left(left)),

        _ => throw new NotSupportedException()
    };
```

The `Left` branch does not call `f`; it preserves the existing structure and lifts it into `F`. The `Right` branch runs `f` once and maps the successful result back into `Either`.

## [07]-[CONCRETE_RETURN_TYPES]

The generic extension returns two nested `K` interfaces. Add concrete member methods to a traversable type so C# resolves them before the generic extensions and only the outer layer remains abstract:

```csharp
public readonly struct Seq<A>
{
    public K<F, Seq<B>> Traverse<F, B>(Func<A, K<F, B>> f)
        where F : Applicative<F> =>
        F.Map(x => x.As(), Traversable.traverse(f, this));

    public K<M, Seq<B>> TraverseM<M, B>(Func<A, K<M, B>> f)
        where M : Monad<M> =>
        M.Map(x => x.As(), Traversable.traverseM(f, this));
}
```

Call `.As()` once more when the outer concrete type is needed:

```csharp
Option<Seq<int>> parsed =
    Seq("10", "20", "30")
        .Traverse(parseInt)
        .As();
```

This repeated member pattern is a pragmatic response to C#'s handling of nested generic interfaces. It does not change traversal semantics.

Operations defined for `K`, such as `Map` and `Apply`, remain available before that conversion.

A domain type implements `Traverse` once and gains composition with the available foldable and applicative types, rather than requiring a separate method for every pairing.
