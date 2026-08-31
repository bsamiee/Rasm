# StateT: Pure, Composable State

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

## From ReaderT to StateT

A reader computation has the shape `Func<Env, K<M, A>>`. Its `Bind` supplies the same `Env` to both computations, so it cannot return a changed environment for later operations.

StateT changes the return type to `K<M, (A, S)>`. Its `Bind` runs the next computation with the state returned by the previous one:

```csharp
result => f(result.Value).runState(result.State)
```

That substitution makes each bind feed the previous computation's state into the next computation. `Readable.local` can alter a ReaderT environment only for a nested scope; an ordinary StateT update continues through subsequent StateT operations.

## A deck as state

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

## Sequencing the game

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

## Hide a deep transformer stack

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

## State opacity and separation

Removing explicit state arguments can make code terse and declarative, but it also hides which operations modify state. Used application-wide, state can begin to resemble a global variable even though its updates are pure.

Keep the state lifecycle deliberate:
- use small, descriptive state queries and updates;
- partition domain rules into pure functions over the state;
- keep IO separate from those rules in production code;
- use scoped state changes for temporary context and propagating updates for durable changes;
- encapsulate deep transformer stacks behind a domain type.

## Forked state

With `IO` inside StateT, forked computations - including automatically parallel work such as `Traverse` - inherit the current state but then evolve independent copies. A parent at `0` can fork two counters that each progress from `1` to `10`; after both complete, the parent is still `0`. Starting at `5` makes both branches begin from `5`, while the parent remains `5`.

```csharp
static StateT<int, IO, Unit> countTo10(string branch) =>
    from _  in StateT.modify<IO, int>(x => x + 1)
    from st in showState(branch)
    from __ in when(st < 10, countTo10(branch))
    select unit;
```

Parallel StateT branches therefore do not share or automatically merge state. To bring a change back, return the required value from the fork, await it, and explicitly set the parent state. This branch-local behavior makes immutable stateful expressions safe to run independently while keeping synchronization visible.
