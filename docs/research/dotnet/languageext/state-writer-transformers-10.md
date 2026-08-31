# [STATE_AND_WRITER_TRANSFORMERS]

`StateT` models changing state without mutating a value in place. Each computation receives a state and returns both its result and the next state inside another monad:

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

The tuple is the hidden state context. A caller normally works with `StateT<S, M, A>` and sees the tuple only when calling `Run`.

## [01]-[FROM_READERT_TO_STATET]

A reader computation has the shape `Func<Env, K<M, A>>`. Its `Bind` supplies the same `Env` to both computations, so it cannot return a changed environment for later operations.

StateT changes the return type to `K<M, (A, S)>`. Its `Bind` runs the next computation with the state returned by the previous one:

```csharp
result => f(result.Value).runState(result.State)
```

That substitution makes each bind feed the previous computation's state into the next computation. `Readable.local` can alter a ReaderT environment only for a nested scope; an ordinary StateT update continues through subsequent StateT operations.

## [02]-[DECK_AS_STATE]

A shuffled deck combines three effects:
- `StateT` threads the current `Deck`;
- `OptionT` can stop the computation when no card remains;
- `IO` contains randomness and console interaction.

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

`generate` belongs in `IO` because random generation is not pure. `get` reads the current state, while `put` replaces it. Once `shuffle` writes the new deck, StateT carries that deck into subsequent computations.

The stack composes state management, optional short-circuiting, and side effects without requiring a bespoke type that hard-codes that exact combination. State is passed by the transformer rather than manually through every function call.

Dealing a card uses optional failure to guard the update:

```csharp
public static StateT<Deck, OptionT<IO>, Card> deal =>
    from d in deck
    from c in OptionT<IO>.lift(d.Cards.Head)
    from _ in update(new Deck(d.Cards.Tail))
    select c;
```

`Seq<Card>.Head` returns `None` for an empty deck. Lifting that value into `OptionT` stops the computation, so the deck update runs only when a card exists. Otherwise, the head is returned and the tail becomes the next state.

A projection can read part of the state without exposing its representation throughout the program:

```csharp
public static StateT<Deck, OptionT<IO>, int> cardsRemaining =>
    StateT.gets<OptionT<IO>, Deck, int>(d => d.Cards.Count);
```

`gets(f)` is equivalent to mapping `f` over `get`. Domain-named state accessors concentrate knowledge of the state shape and make later refactoring local.

## [03]-[GAME_SEQUENCING]

Console operations are lifted into `IO`, so they compose with the two transformers. When an earlier result is irrelevant, language-ext's `>>` operator expresses `ma.Bind(_ => mb)` without LINQ discard variables:

```csharp
public static StateT<Deck, OptionT<IO>, Unit> play =>
    Console.writeLine("First let's shuffle the cards (press a key)") >>
    Console.readKey >>
    Deck.shuffle >>
    Console.writeLine("Shuffle done, let's play...") >>
    Console.writeLine("Dealer is waiting (press a key)") >>
    deal;
```

The simple game recursively deals cards. It terminates when `Deck.deal` lifts `None` after the deck is exhausted. The `IO` monad supports this recursive form without growing the CLR stack indefinitely.

This example deliberately interleaves state logic and console IO to demonstrate composition. In a real application, game rules should be pure functions over `GameState`, kept separate from IO boundaries.

## [04]-[STACK_ENCAPSULATION]

Repeatedly exposing `StateT<GameState, OptionT<IO>, A>` makes domain code noisy. The fuller Pontoon example wraps it in `Game<A>` and has a companion `Game` type derive the capabilities of the underlying transformer:

- `MonadIO<Game>` for IO;
- `Stateful<Game, GameState>` for reading and writing state;
- `Choice<Game>` for choice behavior inherited from the wrapped transformer.

`Stateful` is the state-oriented counterpart to `Readable`: it generalizes state reads and writes across a structure, like Haskell's `MonadState`.

Transform and co-transform functions move between `K<Game, A>` and the wrapped StateT. The game itself can then read as a sequence of domain operations:

```csharp
public static Game<Unit> play =>
    Display.askPlayerNames >>
    enterPlayerNames       >>
    Display.introduction   >>
    Deck.shuffle           >>
    playHands;
```

The wrapper hides the transformer stack, so workflows need not change if that internal representation changes. Display operations also isolate user-facing text from the game flow.

The Pontoon loop composes several kinds of work:
- `playHands` initializes players, plays a hand, asks whether to continue, and recurses only for `Y`;
- `playHand` deals initial cards, runs the stick-or-twist rounds, displays winners, and reports the remaining deck;
- `playRound` continues only while `isGameActive` and traverses the active players;
- `twist` deals from the deck, updates the current player's hand, displays the card, and reports a bust when applicable.

`when` evaluates its second computation only when its monadic Boolean condition is true, which keeps those conditional steps inside the same composed workflow.

`GameState` holds player states, the deck, and an optional current player. `Players.with` traverses the active players and runs an action for each player under a temporary current-player context:

```csharp
public static Game<A> with<A>(Player player, Game<A> ma) =>
    Stateful.local<Game, GameState, A>(setCurrent(player), ma).As();
```

Unlike a normal propagating update, `Stateful.local` restores the prior state after the nested action. It is useful for contextual operations such as selecting the current player without leaking that selection beyond its scope.

## [05]-[STATE_OPACITY]

Removing explicit state arguments can make code terse and declarative, but it also hides which operations modify state. Used application-wide, state can begin to resemble a global variable even though its updates are pure.

Keep the state lifecycle deliberate:
- use small, descriptive state queries and updates;
- partition domain rules into pure functions over the state;
- keep IO separate from those rules in production code;
- use scoped state changes for temporary context and propagating updates for durable changes;
- encapsulate deep transformer stacks behind a domain type.

## [06]-[FORKED_STATE]

With `IO` inside StateT, forked computations - including automatically parallel work such as `Traverse` - inherit the current state but then evolve independent copies. A parent at `0` can fork two counters that each progress from `1` to `10`; after both complete, the parent is still `0`. Starting at `5` makes both branches begin from `5`, while the parent remains `5`.

```csharp
static StateT<int, IO, Unit> countTo10(string branch) =>
    from _  in StateT.modify<IO, int>(x => x + 1)
    from st in showState(branch)
    from __ in when(st < 10, countTo10(branch))
    select unit;
```

Parallel StateT branches therefore do not share or automatically merge state. To bring a change back, return the required value from the fork, await it, and explicitly set the parent state. This branch-local behavior makes immutable stateful expressions safe to run independently while keeping synchronization visible.

## [07]-[WRITER_OUTPUT]

`WriterT` accumulates output during a monadic expression. Its output type `W` is constrained by `Monoid<W>`, so it has an empty value and can combine output values. This makes the transformer useful for logging multiple outputs or building one aggregate output with pure expressions.

Operationally, `WriterT` is the same as `StateT`: the state type `S` is renamed to the output type `W` and constrained to be a monoid. The distinction remains useful because `WriterT` declares that the threaded value is accumulated output, not arbitrary state.

## [08]-[CLASSIC_WRITER]

A direct `Writer` representation returns both an output and a value:

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

`Bind` runs the first computation, uses its value to select the next one, then combines the two returned outputs. In this representation, `tell` only needs to return the supplied output alongside `Unit`:

```csharp
public static class Writer
{
    public static Writer<W, Unit> tell<W>(W output)
        where W : Monoid<W> =>
        new(() => (output, unit));
}
```

Individual outputs can then be accumulated in a query expression:

```csharp
static Writer<Seq<string>, Unit> example =>
    from _1 in tell(Seq("Hello"))
    from _2 in tell(Seq("World"))
    select unit;
```

## [09]-[BIND_COMBINATION_COST]

`Bind` is normally called far more often than `tell`, yet the classic design calls `Combine` for every bind. This causes two problems:
- one or both outputs are often empty, so their combination is wasted work;
- non-empty outputs may be expensive to combine.

For example, concatenating two immutable linked lists of 100 items can require a 100-item traversal to build a new list. Repeatedly combining growing immutable outputs can therefore rebuild the same elements many times and become very expensive.

The combination should happen in `tell`, where output is deliberately added, instead of in every `Bind`.

## [10]-[OUTPUT_THREADING]

Change the runner from a function with no input:

```csharp
Func<(W Output, A Value)> runWriter
```

to a function that receives the accumulated output:

```csharp
Func<W, (W Output, A Value)> runWriter
```

`Bind` can now pass each updated output to the next computation without combining anything:

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

This is the same mechanism as `State`: it threads a value through a computation and returns the updated value. `tell` now performs the output combination:

```csharp
public static Writer<W, Unit> tell<W>(W value)
    where W : Monoid<W> =>
    new(output => (output.Combine(value), unit));
```

Writer output is commonly a collection, so `tell` usually appends or prepends a single item. That avoids repeatedly concatenating whole accumulated collections, provided the monoid itself combines values efficiently.

The revised `Writer<W, A>` no longer needs a `Monoid<W>` constraint. Only `tell` combines values, so only `tell` needs the constraint.

## [11]-[WRITER_VIA_STATET]

Because the revised implementation is otherwise identical to `State`, a `WriterT` operation can be expressed with `StateT.modify`:

```csharp
public static StateT<W, M, Unit> tell<M, W>(W value)
    where W : Monoid<W>
    where M : Monad<M> =>
    StateT.modify<M, W>(output => output.Combine(value));
```

The operation can also work with any type that implements `Stateful<M, W>`:

```csharp
public static K<M, Unit> tell<M, W>(W value)
    where W : Monoid<W>
    where M : Stateful<M, W> =>
    Stateful.modify<M, W>(output => output.Combine(value));
```

Any such computation can aggregate output using the `Monoid<W>` operation. Dedicated `Writer` and `WriterT` types are still worth keeping because their names communicate the intended role of the threaded value.

## [12]-[RWST]

`ReaderT`, `WriterT`, and `StateT` can be stacked over a base monad `M`:

```csharp
public record RWST<R, W, S, M, A>(
    ReaderT<R, WriterT<W, StateT<S, M>>, A> runRWS)
    : K<RWST<R, W, S, M>, A>
    where M : Monad<M>
    where W : Monoid<W>;
```

The stack combines four behaviors: reading configuration through `ReaderT`, aggregating output through `WriterT`, carrying state through `StateT`, and lifting a base monad such as `IO` or `Option`. This makes it suitable as an application monad that needs all four behaviors.

Its trait witness exposes the capabilities already provided by the wrapped types:

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

The wrapped types already implement the required behaviors. `RWST` only needs to lift those behaviors into its own wrapper rather than reimplement them.
