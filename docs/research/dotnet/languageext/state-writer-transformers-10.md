# [STATE_AND_WRITER_TRANSFORMERS]

`StateT` models changing state without mutating a value in place. Each computation receives a state and returns its result with the next state inside another monad:

```csharp
public record StateT<S, M, A>(
    Func<S, K<M, (A Value, S State)>> runState) : K<StateT<S, M>, A>
    where M : Monad<M>
{
    public K<M, (A Value, S State)> Run(S state) => runState(state);
    public StateT<S, M, B> Bind<B>(Func<A, StateT<S, M, B>> f) =>
        new(state =>
            M.Bind(
                runState(state),
                result => f(result.Value).runState(result.State)));
}
```

The tuple is the hidden state context. Callers work with `StateT<S, M, A>` and see the tuple only at `Run`.

## [01]-[FROM_READERT_TO_STATET]

Reader computations have shape `Func<Env, K<M, A>>`. Its `Bind` supplies the same `Env` to both computations, it cannot return a changed environment for later operations. StateT changes the return type to `K<M, (A, S)>`, and its `Bind` runs the next computation with the state the previous one returned:

```csharp
result => f(result.Value).runState(result.State)
```

Each bind feeds the previous computation's state into the next. `Readable.local` alters a ReaderT environment only for a nested scope; a StateT update continues through subsequent operations.

## [02]-[DECK_AS_STATE]

Shuffled decks combine effects:
- `StateT` threads the current `Deck`;
- `OptionT` can stop the computation when no card remains;
- `IO` contains randomness and console interaction

```csharp
public record Deck(Seq<Card> Cards)
{
    public static Deck Empty = new([]);

    static IO<Deck> generate =>
        IO.lift(() =>
        {
            var random = new Random((int)DateTime.Now.Ticks);
            var array = LanguageExt.List
                                   .generate(52, ix => new Card(ix))
                                   .ToArray();
            random.Shuffle(array);
            return new Deck(array.ToSeqUnsafe());
        });
    public static StateT<Deck, OptionT<IO>, Deck> deck => StateT.get<OptionT<IO>, Deck>();
    public static StateT<Deck, OptionT<IO>, Unit> update(Deck deck) => StateT.put<OptionT<IO>, Deck>(deck);
    public static StateT<Deck, OptionT<IO>, Unit> shuffle =>
        from deck in generate
        from _    in update(deck)
        select unit;
}
```

`generate` belongs in `IO`, because random generation is not pure. `get` reads the current state; `put` replaces it. Once `shuffle` writes the new deck, StateT carries that deck into subsequent computations. The stack composes state, optional short-circuiting, and side effects without a bespoke type that hard-codes the combination, and the transformer passes the state instead of every function call.

Dealing a card guards the update with optional failure:

```csharp
public static StateT<Deck, OptionT<IO>, Card> deal =>
    from d in deck
    from c in OptionT<IO>.lift(d.Cards.Head)
    from _ in update(new Deck(d.Cards.Tail))
    select c;
```

`Seq<Card>.Head` returns `None` for an empty deck. Lifting that value into `OptionT` stops the computation, the deck update runs only when a card exists: the head is returned and the tail becomes the next state.

Projections read part of the state without exposing its representation:

```csharp
public static StateT<Deck, OptionT<IO>, int> cardsRemaining =>
    StateT.gets<OptionT<IO>, Deck, int>(d => d.Cards.Count);
```

`gets(f)` equals mapping `f` over `get`. Domain-named state accessors concentrate knowledge of the state shape and keep later refactoring local.

## [03]-[GAME_SEQUENCING]

Console operations lift into `IO` and compose with the transformers. When an earlier result is irrelevant, the `>>` operator expresses `ma.Bind(_ => mb)` without LINQ discard variables:

```csharp
public static StateT<Deck, OptionT<IO>, Unit> play =>
    Console.writeLine("First let's shuffle the cards (press a key)") >>
    Console.readKey >>
    Deck.shuffle >>
    Console.writeLine("Shuffle done, let's play...") >>
    Console.writeLine("Dealer is waiting (press a key)") >>
    deal;
```

Game loops recursively deal cards and terminate when `Deck.deal` lifts `None` after the deck is exhausted. The `IO` monad supports this recursive form without growing the CLR stack. This example interleaves state logic and console IO to show composition; production code keeps game rules as pure functions over the state, separate from IO boundaries.

## [04]-[STACK_ENCAPSULATION]

Exposing `StateT<GameState, OptionT<IO>, A>` throughout domain code is noisy. `Game<A>` wrappers hide it, and a companion `Game` type derives the capabilities of the underlying transformer:
- `MonadIO<Game>` for IO;
- `Stateful<Game, GameState>` for state reads and writes;
- `Choice<Game>` for the choice behavior of the wrapped transformer

`Stateful` is the state counterpart of `Readable`: it generalizes state reads and writes across a structure, like Haskell's `MonadState`. Transform and co-transform functions move between `K<Game, A>` and the wrapped StateT. The workflow then reads as a sequence of domain operations:

```csharp
public static Game<Unit> play =>
    Display.askPlayerNames >>
    enterPlayerNames       >>
    Display.introduction   >>
    Deck.shuffle           >>
    playHands;
```

The wrapper hides the transformer stack, workflows do not change when the internal representation changes. Display operations isolate user-facing text from the flow. `when` evaluates its second computation only when its monadic Boolean condition is true, which keeps conditional steps inside the composed workflow.

`Players.with` runs an action for one player under a temporary current-player context:

```csharp
public static Game<A> with<A>(Player player, Game<A> ma) =>
    Stateful.local<Game, GameState, A>(setCurrent(player), ma).As();
```

Unlike a propagating update, `Stateful.local` restores the prior state after the nested action. Use it for contextual operations, such as selecting the current player without leaking that selection beyond its scope.

## [05]-[STATE_OPACITY]

Removing explicit state arguments makes code terse and declarative; it also hides which operations modify state. Application-wide state approaches a global variable even when its updates are pure. Keep the state lifecycle deliberate:
- Use small, descriptive state queries and updates;
- Partition domain rules into pure functions over the state;
- Keep IO separate from those rules;
- Use scoped state changes for temporary context and propagating updates for durable changes;
- Encapsulate deep transformer stacks behind a domain type

## [06]-[FORKED_STATE]

With `IO` inside StateT, forked computations (including automatically parallel work such as `Traverse`) inherit the current state and then evolve independent copies. Parents at `0` fork two counters that each progress from `1` to `10`; after both complete, each parent is still `0`. Parents at `5` start both branches at `5` and stay `5`.

```csharp
static StateT<int, IO, Unit> countTo10(string branch) =>
    from _  in StateT.modify<IO, int>(x => x + 1)
    from st in showState(branch)
    from __ in when(st < 10, countTo10(branch))
    select unit;
```

Parallel StateT branches neither share nor merge state. To bring a change back, return the required value from the fork, await it, and set the parent state explicitly. Branch-local state makes immutable stateful expressions safe to run independently and keeps synchronization visible.

## [07]-[WRITER_OUTPUT]

`WriterT` accumulates output during a monadic expression. Its output type `W` is a `Monoid<W>`, with an empty value and combination, which suits logging and aggregate output built from pure expressions. Operationally `WriterT` is `StateT` with the state type renamed to the output type and constrained to a monoid. The distinct name declares that the threaded value is accumulated output, not arbitrary state.

## [08]-[CLASSIC_WRITER]

Direct `Writer` representations return an output beside a value:

```csharp
public record Writer<W, A>(Func<(W Output, A Value)> runWriter)
    where W : Monoid<W>
{
    public Writer<W, B> Bind<B>(Func<A, Writer<W, B>> f) =>
        new(() =>
        {
            var (output1, value1) = runWriter();
            var (output2, value2) = f(value1).runWriter();
            return (output1.Combine(output2), value2);
        });
}
```

`Bind` runs the first computation, uses its value to select the next, then combines the two outputs. `tell` returns the supplied output beside `Unit`:

```csharp
public static class Writer
{
    public static Writer<W, Unit> tell<W>(W output)
        where W : Monoid<W> =>
        new(() => (output, unit));
}
```

Individual outputs accumulate in a query expression:

```csharp
static Writer<Seq<string>, Unit> example =>
    from _1 in tell(Seq("Hello"))
    from _2 in tell(Seq("World"))
    select unit;
```

## [09]-[BIND_COMBINATION_COST]

`Bind` runs far more often than `tell`, yet the classic design calls `Combine` on every bind. Problems follow: one or both outputs are often empty, the combination is wasted work, and non-empty outputs cost real work to combine. Concatenating two immutable linked lists of 100 items traverses 100 items to build a new list; repeated combination of growing immutable outputs rebuilds the same elements many times. The combination belongs in `tell`, where output is deliberately added, not in every `Bind`.

## [10]-[OUTPUT_THREADING]

Change the runner from a function with no input:

```csharp
Func<(W Output, A Value)> runWriter
```

to a function that receives the accumulated output:

```csharp
Func<W, (W Output, A Value)> runWriter
```

`Bind` now passes each updated output to the next computation and combines nothing:

```csharp
public record Writer<W, A>(Func<W, (W Output, A Value)> runWriter)
{
    public Writer<W, B> Bind<B>(Func<A, Writer<W, B>> f) =>
        new(output0 =>
        {
            var (output1, value1) = runWriter(output0);
            var (output2, value2) = f(value1).runWriter(output1);
            return (output2, value2);
        });
}
```

This is the `State` mechanism: thread a value through the computation and return the updated value. `tell` performs the combination:

```csharp
public static Writer<W, Unit> tell<W>(W value)
    where W : Monoid<W> =>
    new(output => (output.Combine(value), unit));
```

Writer output is commonly a collection, `tell` appends or prepends a single item and avoids concatenating whole accumulated collections, provided the monoid combines efficiently. The revised `Writer<W, A>` needs no `Monoid<W>` constraint; only `tell` combines, only `tell` needs it.

## [11]-[WRITER_VIA_STATET]

Because the threaded representation matches `State`, a `WriterT` operation is expressible with `StateT.modify`:

```csharp
public static StateT<W, M, Unit> tell<M, W>(W value)
    where W : Monoid<W>
    where M : Monad<M> =>
    StateT.modify<M, W>(output => output.Combine(value));
```

The operation also works with any type that implements `Stateful<M, W>`:

```csharp
public static K<M, Unit> tell<M, W>(W value)
    where W : Monoid<W>
    where M : Stateful<M, W> =>
    Stateful.modify<M, W>(output => output.Combine(value));
```

Any such computation aggregates output through the `Monoid<W>` operation. Dedicated `Writer` and `WriterT` types remain worth keeping: their names communicate the role of the threaded value.

## [12]-[RWST]

`ReaderT`, `WriterT`, and `StateT` stack over a base monad `M`:

```csharp
public record RWST<R, W, S, M, A>(
    ReaderT<R, WriterT<W, StateT<S, M>>, A> runRWS)
    : K<RWST<R, W, S, M>, A>
    where M : Monad<M>
    where W : Monoid<W>;
```

The stack combines four behaviors: configuration reads through `ReaderT`, output aggregation through `WriterT`, state through `StateT`, and a lifted base monad such as `IO` or `Option`. That suits an application monad that needs all four. Its trait witness exposes the capabilities the wrapped types already provide:

```csharp
public class RWST<R, W, S, M> :
    MonadT<RWST<R, W, S, M>, M>,
    Readable<RWST<R, W, S, M>, R>,
    Writable<RWST<R, W, S, M>, W>,
    Stateful<RWST<R, W, S, M>, S>
    where M : Monad<M>
    where W : Monoid<W>
{
    // Lift each existing behavior into the wrapper-transformer.
}
```

The wrapped types implement the required behaviors; `RWST` lifts those behaviors into its own wrapper instead of reimplementing them.
