# BRIEF for one file of `.scratch/functional-csharp/`

You own ONE markdown file of the functional C# corpus and its lab counterpart. The corpus teaches functional C# and must assume LanguageExt v5 (`5.0.0-beta-77`, `net10.0`) as the base library. Nothing is fabricated: every LanguageExt member you name is verified against the installed assembly and compiled in a lab under the repo analyzers.

## Sources of truth, ranked

1. The installed assembly. Decompile with `ilspycmd -t '<Full.Type`Arity>' <dll>`; the dlls are `~/.nuget/packages/languageext.core/5.0.0-beta-77/lib/net10.0/LanguageExt.Core.dll`, `~/.nuget/packages/languageext.sys/5.0.0-beta-77/lib/net10.0/LanguageExt.Sys.dll`, and `~/.nuget/packages/languageext.streaming/5.0.0-beta-77/lib/net10.0/LanguageExt.Streaming.dll`. Type lists: `DECOMP/core-types.txt`, `DECOMP/sys-types.txt`, `DECOMP/streaming-types.txt`. Ready decompiles: `DECOMP/*.cs`, and one-line public signatures per type in `DECOMP/sigs/*.txt`. Search `DECOMP/sigs/Prelude.txt` for Prelude functions.
2. The lab build. `LAB-NOTES.md` (next to this brief) lists what already compiled and ran, the corrections to the plan, and the analyzer rulings. Read it in full before writing code.
3. The author's articles in `.scratch/languageext/` (v5).
4. The doc pages under `LXDOCS/` (the library README and reference pages).
5. `.scratch/api-languageext.md`. If your lab proves a claim there wrong, report it in your notes; do not edit that file.
6. `.scratch/languageext-skill/SKILL.md` is reference only and names removed v4 types (`Aff`, `TryAsync`); copy nothing from it.

`DECOMP` is `/private/tmp/claude-501/-Users-bardiasamiee-Documents-99-Github-Rasm/350c8649-66bf-4aed-a56b-320562d70414/scratchpad/decomp`. `LXDOCS` is `/private/tmp/claude-501/-Users-bardiasamiee-Documents-99-Github-Rasm/b32e86c0-4f0e-47e4-8f7a-2dec8faea91b/scratchpad/lxdocs`.

## Global rulings (one path per concern, no alternatives shown in a file)

| Concern | LanguageExt type and members | Where |
|---|---|---|
| Absence | `Option<A>`: `Some`, `None`, `Optional(x)` at the null boundary, `Match`, `Map`, `Bind`, `Filter`, `Do`, `IfNone`, `ToFin(Error)`, `ToValidation`, `ToSeq`, `Iter` | lookups, parsing without a reason, optional fields |
| Expected failure with a reason, short-circuit | `Fin<A>`: implicit lifts from `A`, `Error`, `Pure`; `Map`, `MapFail`, `BiMap`, `Bind`, `BindFail`, `Match`, `IfFail`, `IfSucc`, `Iter`, `guard` in LINQ | pure domain transitions, dependent validation, smart constructors through `From` over the generated `Validate` |
| Two value types, neither an error | `Either<L, R>` | only when `L` is data, not failure |
| Independent failures, accumulate | `Validation<Error, A>`: implicit lifts, tuple `Apply`, `&` for same-type success, `|`, `Traverse` for open sets, `MapFail` for context, `ToFin` at the exit | the boundary that admits a form or command; pure; never in the middle of a dependent chain; a check that needs data receives it as a value loaded before validation |
| Exception capture, deferred, synchronous | `Try<A>`: `Try.lift(f)`, `Run() : Fin<A>` | one narrow adapter around a throwing synchronous dependency |
| Side effects with a failure channel | `IO<A>`: `IO.lift`, `IO.liftAsync`, `IO.lift(Func<Fin<A>>)`, `IO.pure`, `IO.fail`, `Bracket`, `use`, `Fork`, `awaitAll`, `awaitAny`, `timeout`, `Retry(Schedule)`, `Repeat(Schedule)`, `Catch`, `\|`, `tail` | every leaf effect; a domain rejection inside an effect is a typed `Expected` on the `IO` error channel, never `IO<Fin<A>>` |
| Effects that read a capability | `Eff<RT, A>`: `Eff.runtime<RT>()`, `Has<Eff<RT>, T>`, `Console<RT>`, `File<RT>`, `LanguageExt.Sys.Live.Runtime`/`LanguageExt.Sys.Test.Runtime`; an `IO<A>` enters by implicit conversion or `Eff<RT, A>.LiftIO` | where a capability is read; `Eff<A>` is not used |
| Host exit | `Eff<RT, A>.Run(rt) : Fin<A>` (or `RunAsync`) then `Match`; a bare `IO<A>` exits with `RunSafe() : Fin<A>`; `IO.Run()`/`RunAsync()` throw and belong to `Main` only | the host only; domain functions never run an effect |
| Error values | `Error` from `LanguageExt.Common`: domain errors are `sealed record X() : Expected("message", code)` with codes in one closed block; `Exceptional` for captured exceptions; `ManyErrors` from accumulation; classify with `Is`, `HasCode`, `IsType<E>`, never by message text; an error that a value object or smart enum raises also implements `IValidationError<X>` with `public static X Create(string message) => new()` | defined by the package that raises them |
| Recovery | `Catch(code, f)`, `Catch(Error, f)`, `Catch(predicate, f)`, `@catch`, `\|` alternative, `IfFail` at the host | at the boundary that owns the error |
| Environment and state | `Reader<Env, A>`/`ReaderT<Env, M, A>` with `ask`/`asks`/`local`, instance `With`/`ReaderT.with`; `State<S, A>`/`StateT<S, M, A>` with `get`/`gets`/`put`/`modify`, `Stateful.state`; `Writer<W, A>`/`WriterT` with `tell`; `Readable`/`Stateful`/`Writable` traits | workflows that thread configuration, state, or a log |
| Stacked effects | `OptionT<IO, A>` for effectful lookups, `FinT<M, A>`, `EitherT`, `ValidationT<Error, IO, A>` only when accumulation must happen inside an effect, `ReaderT`, `StateT`, `WriterT`, `RWST`; `liftIO` through a stack; `Run` one layer at a time; a domain wrapper with `Deriving.*` hides the stack | never nested bare types (`IO<Option<A>>`, `IO<Fin<A>>`) |
| Collections | `Seq<A>` default; `Arr<A>` indexed; `Lst<A>` only for `Insert`/`RemoveAt`/`SetItem`; `Map`/`HashMap`; `Set`/`HashSet`; `Iterable<A>` lazy; `IterableNE` non-empty; construction from `Prelude`; `Fold`/`FoldBack`, `Choose`, `Partition`, `Zip`, `Scan`, `At`, `toSeq(Range(...))`, `toSeq(LanguageExt.List.unfold(...))` | every collection in domain code; BCL `List`/`Dictionary` only inside a scope that publishes an immutable value |
| Traversal policy | instance `Traverse` applicative (accumulates under `Validation`; under `IO` it starts every element effect before it awaits any: `IO.liftAsync` effects overlap without a bound, `IO.lift` effects run in order on the calling thread), instance `TraverseM` monadic and serial, `Fork` takes one thread per fork so a large fan-out chunks first, or a `Conduit` with `Buffer.Bounded(n)`, `PartitionFallible`/`Succs`/`Fails` for best-effort | chosen by dependency structure and by the concurrency bound |
| Shared mutable state | `Atom<A>` with `Swap`/`SwapMaybe` (the function re-runs on conflict, no effects inside), `AtomHashMap` with `SwapKey`/`TryAdd`/`Find`, `Ref` under `atomic` (STM), `TrackingHashMap` change logs | where one logical value must be shared |
| Streams and inboxes | `Source<A>`/`SourceT<M, A>` with `Reduce : IO<S>`, `Source.lift(IObservable<A>)`, `Source.merge`, instance `Zip`; `Sink`/`SinkT` with `Post : IO<Unit>` and `Comap`; `Conduit` with `Buffer` policies as the queue and as an agent inbox; `Pipes` (`ProducerT`, `PipeT`, `ConsumerT` fused with `\|` into `EffectT`) | push and pull dataflow; an effect surface |
| Stack-safe iteration | `Trampoline<A>` for pure recursion; `Monad.recur` with `Next.Loop`/`Next.Done` for an effectful state loop; `tail` for deep `IO` binds (exit through `Run` only); `IO.RepeatUntil`/`RepeatWhile` for polling one effect; `LanguageExt.List.unfold` for a state sequence | replaces the loop-backed runner and the custom enumerator |
| Unit | `LanguageExt.Unit`, `unit` | replaces the `ValueTuple` alias |
| Higher kinds | `K<F, A>`, `Functor<F>`, `Applicative<F>`, `Monad<M>`, `Foldable<T>`, `Traversable<T>`, `.As()` at the concrete edge; LINQ query syntax comes from `Monad<M>` | replaces "C# cannot encode `C<_>`" claims |
| Domain values | Thinktecture `[ValueObject<TKey>]`, `[SmartEnum<TKey>]`, and `[Union]`; `[ValidationError<X>]` names the typed `Expected` the hook raises; `From : TKey -> Fin<T>` is `Validate(value, provider: null, out T item) is { } error ? error : item`; `TryGet` and `TryCreate` map to `Option<T>`; `Switch` selects a union or smart enum case; `Create`, `Parse`, and `Get` throw on a defect and admit no input; a string key needs both comparer attributes (`TTRESG048`) | value objects, smart enums, closed unions |

Boundary rule (state it once per file where relevant): the result or effect type is chosen where input enters and is kept through the domain; conversion (`ToFin`, `ToValidation`, `ToOption`, `liftIO`, `OptionT.lift`) happens at a named boundary; `Match`, `Run`, `RunSafe`, `RunAsync`, `IfNone`, `IfFail` are host operations; domain functions never run an effect.

Naming: `Return` becomes `Pure`; the set's `TraverseA` is LanguageExt `Traverse`; `ForEach` is `Iter`; an observer on an elevated value is `Do` where the type has it (`Option`, `Seq`, `Either`, `Validation`) and `IfSucc` on `Fin`; an observer inside an effect chain is an `IO` step; `Where` on `Option` is `Filter` (LINQ `where` still works); the hand-rolled `Partial` and per-arity `Curry` are replaced by `par` and `curry`; `Pipe` stays as the one hand-rolled value-level helper (LanguageExt has no value pipe).

Banned in the result: `DomainType<`, `Aff<`, `TryAsync`, `HasCancel`, `Exceptional<`, `StatefulComputation`, `Middleware<`, `IterateUntil`, `ToValueOrDefault`, `ActionBlock`, `Async<`, `ContinueWith`, `Task.FromResult` (docs), `Try().Run`, `release(`, `IO<Option<`, `IO<Fin<`, `Eff<X>` with one type argument (except inside `Has<Eff<RT>, ...>` or `K<Eff<RT>, ...>`), any `NN-name.md` cross-file reference, any `Console<RT>`-style claim you did not compile.

## Edit discipline for an existing file (surgical, not a rewrite)

The twenty existing files are finished textbook chapters. You integrate LanguageExt into the chapter you own; you do not rewrite it.

- Keep the title, the heading order, the section structure, and every paragraph of book prose that still holds. Do not reorder sections, do not merge sections, do not add an introduction or a summary.
- Prose is adjusted if and only if it is needed. The adjusted sentence keeps the chapter's focused, direct textbook voice: a plain statement of the fact, no filler words, no coined terms, no narration.
- Change a sentence only when it is now false (it names a hand-rolled type the file no longer defines, it claims C# or the set lacks something LanguageExt supplies, or it describes a removed code block). Adjust that sentence in place with the fewest words that make it true.
- Replace a code block only when your integration row lists it as removed or replaced. The replacement keeps the block's place and its teaching point and shows the LanguageExt form. A code block the row does not name stays as it is unless it fails to compile under the code text rule below; then adjust it to the code text rule with the smallest change.
- Add a sentence where the file's logic needs a LanguageExt member: name the member, its type, and the boundary it belongs to. One sentence per need. Do not add sections that the integration row does not ask for.
- Cross-file references (`06-higher-order-functions.md`, "see file 12") are removed and replaced by the fact itself in one clause, or by nothing when the file already states it.
- Remove hedges and alternatives that the row asks you to remove; do not add new ones. The file shows one path per concern.
- Prefer deletion over addition: removed hand-rolled code is what makes room. The file ends under 300 lines.
- Terminology: use only the words the corpus and LanguageExt already use. "result type", "effect type", "boundary", "host", "domain", "overload resolution", "`Catch` overloads", "transformer", "runtime", "capability", "trait", "applicative", "monadic", "traversal", "accumulate", "short-circuit". Do not coin names for patterns, layers, rules, or shapes (no "effect ladder", "typed rail", "boundary contract", "lift discipline", "result algebra", "gate pattern", or any other new compound). Do not invent method names; every identifier in prose or code exists in the assembly or in the file's own code.

## Code text rule

Doc code blocks and lab code are the same text. Write every `csharp` block so that it compiles in the lab under the analyzer rulings in `LAB-NOTES.md`: explicit types, `static` non-capturing lambdas, expression-bodied single-return methods, K&R braces, `internal` types, `internal static class` sample holders, `internal sealed record` data, invariant `string.Create` for numeric holes, `_ =` discards, no `catch`, no `Random`, no `var`, no file-level `using`. The lab file wraps the blocks in `namespace Lab.FNN;` and adds a `Samples.Run() : Fin<Unit>` that executes the samples and checks outcomes; the namespace line and the `Run` method are lab scaffolding and stay out of the doc. A block the doc marks as host-only (a web framework result, a real database) is kept conceptual in the doc, and its reason is recorded in your notes. `text` and `fsharp` fences are not compiled. Book fragments that never compiled are completed with a stub or replaced.

## Prose rules for added or changed text

Load the skill `simple-english:simple-english` before writing prose. Descriptive sentences under 25 words, procedural under 20; simple tenses; active voice; no contractions; no semicolons; one owner per concept in a file; no counts, dates, versions, or file paths inside the doc; verified identifiers are the payload; no coined terms (use "result type", "effect type", "boundary", "overload resolution", "`Catch` overloads"); no hedges ("may", "might", "could", "should"), no alternatives ("or use X instead"), no "-ly" adverbs in added text, no filler, no citations, no narration about the edit. Book prose that stays is not rewritten; only added or adjusted sentences follow these rules. Do not restate the same fact twice in one file.

## Your lab

Create a private lab so that builds do not collide with other agents:

```
mkdir -p .scratch/lab-fNN/FNN
cp .scratch/lab-functional-csharp/Lab.csproj .scratch/lab-fNN/LabFNN.csproj
```

Edit `LabFNN.csproj`: add `<RootNamespace>Lab</RootNamespace>` inside the first `PropertyGroup` (nothing else changes; never add `NoWarn`, `TreatWarningsAsErrors`, or any analysis property: the root targets reject them with `RASM0002`). Write `.scratch/lab-fNN/Program.cs`:

```csharp
namespace Lab;

internal static class Program {
    private static int Main() =>
        FNN.Samples.Run().Match(
            Succ: static _ => 0,
            Fail: static error => {
                Console.Error.WriteLine(error.ToString());
                return 1;
            });
}
```

Put the code in `.scratch/lab-fNN/FNN/*.cs` under `namespace Lab.FNN;` (one file per doc section is fine; `Samples.cs` holds `internal static class Samples { public static Fin<Unit> Run() ... }`). Build with `dotnet build .scratch/lab-fNN/LabFNN.csproj -nologo -v q` and run with `dotnet run --project .scratch/lab-fNN/LabFNN.csproj --no-build -nologo`; the exit code must be 0. Fix every diagnostic; never suppress one. When done, copy the folder into the shared lab: `cp -R .scratch/lab-fNN/FNN .scratch/lab-functional-csharp/FNN` (the shared `Program.cs` is registered by the coordinator; do not edit it). Do not touch `.scratch/lab-functional-csharp/F00`.

## Deliverables

1. The edited doc file under 300 lines (`wc -l`), self-contained (no reference to another file of the set), the book's voice kept, every code block identical to the lab text.
2. The lab folder copied into the shared lab, building with zero diagnostics and running with exit 0.
3. `.scratch/lab-functional-csharp/NOTES/FNN.md`: members verified (type and member, one line each), corrections to the plan or the API document with the decompile or run evidence, host-only blocks and their reason, and anything a project built on the file would still need.

Final self-checks before you report: `wc -l` under 300; `rg -n '[0-9]{2}-[a-z-]+\.md' <file>` empty; `rg -n 'Aff<|TryAsync|HasCancel|Exceptional<|StatefulComputation|Middleware<|IterateUntil|ToValueOrDefault|ActionBlock|Async<|ContinueWith|Task\.FromResult|Try\(\)\.Run|\brelease\(' <file>` empty; `rg -n 'IO<Option<|IO<Fin<' <file>` empty; `rg -n 'Eff<[A-Z][A-Za-z]*>' <file> | rg -v 'Has<Eff<|K<Eff<'` empty; no `should`, `may`, `might`, `could` in added prose; no semicolons in prose.
