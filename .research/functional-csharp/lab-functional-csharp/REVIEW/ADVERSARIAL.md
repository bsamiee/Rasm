# ADVERSARIAL REVIEW - whole-set view of `.scratch/functional-csharp/`
Scope: cross-file coherence, ownership, drift from the books, presentation, and the mechanical gates.
Per-file member fabrication is owned by the per-file agents and is not repeated here.

## 1. Fabrication and wrong semantic claims

**File** 18-async.md
**Location** line 13, `## Task<T> as an effectful container`
**Claim** "`IO<A>` is the effect type for the workflow: it wraps a `Func<Task<A>>`, and each run creates a fresh task."
**Evidence** `ilspycmd -t 'LanguageExt.IO`1'`: `public abstract record IO<A> : K<IO, A>, Monoid<IO<A>>, Semigroup<IO<A>>` with abstract `Map`, `Bind`, and `BindAsync`. The value is a free-monad description built from `LanguageExt.DSL.IOPure<A>`, `IOBind<..>`, and `IOTail<A>` nodes; no `Func<Task<A>>` field exists. File 21's own table states the shape correctly as "abstract record class", so the set contradicts itself.
**Correction** Replace with: "`IO<A>` is the effect type for the workflow. It describes the effect and performs nothing until the host runs it, and every run performs the effect again. This is why fallback and retry operate on `IO<A>` and not on a started task."

**File** 21-languageext-result-types.md
**Location** line 174, `## Recovery as use`
**Claim** "The `Catch` overloads select by code, by error value, or by predicate. They return `K<F, A>`, so `.As()` restores the concrete type."
**Evidence** `IO_1.cs` line 736: `public virtual IO<A> Catch(Func<Error, bool> Predicate, Func<Error, K<IO, A>> Fail)` is an instance method returning `IO<A>`. Only the `int Code` and `Error Match` forms are `FallibleExtensions` returning `K<F, A>`. 10-discriminated-unions.md line 150 calls the predicate overload on an `IO` with no `.As()`, which the rule as written forbids.
**Correction** Replace with: "The `Catch` overloads select by code, by error value, or by predicate. The code and error-value overloads are extensions that return `K<F, A>`, so `.As()` restores the concrete type. `IO<A>` declares the predicate overload as an instance method that returns `IO<A>`."
Apply the same narrowing to 23-languageext-traits-and-transformers.md line 41, which asserts all three belong to `Fallible`.

**File** 22-languageext-effects.md
**Location** line 50, `## Exits at the host`
**Claim** "`Run` and `RunAsync` throw an `ErrorException` on failure and belong to `Main`."
**Evidence** `Expected.Throw<R>()` throws `ToErrorException()` (a `WrappedErrorExpectedException`), but `Exceptional.Throw<R>()` calls `Value.Rethrow<R>()` when `Value` is set, so the original exception surfaces and no `ErrorException` is constructed.
**Correction** Replace with: "`Run` and `RunAsync` throw on failure and belong to `Main`. An `Expected` error arrives as an `ErrorException`, and an `Exceptional` error rethrows the captured exception."

**File** 22-languageext-effects.md
**Location** line 229 prose against line 254 code
**Claim** Prose: "`Fold` on a lifted finite sequence emits nothing." Code: `public static Source<int> Folded => Observed.Fold(static (total, item) => total + item, 0);`
**Evidence** `LAB-NOTES.md` CORRECTION: `Source<A>.Fold(f, init)` emitted nothing for a lifted finite sequence. The file therefore ships a sample whose only property is that it does not work, and shows two paths for one concern.
**Correction** Delete the `Folded` member and the sentence "`Fold` on a lifted finite sequence emits nothing." `Reduce` is the one fold named in the file.

## 2. Contradictions between files
**Files** 17-indefinite-loops.md, 22-languageext-effects.md, 23-languageext-traits-and-transformers.md
**Location** 17:130, 22:151, 23:117
**Claim** Three spellings of the `tail` exit rule. 22 and 23: "`RunSafe`, `Try`, `Map`, and a later `Bind` ... fail with a tail-call error". 17: "`RunSafe()`, `Map`, and any later `Bind` push a map into the tail and fail."
**Evidence** `LAB-NOTES.md`: "`RunSafe()`, `Try()`, `Map`, and any later `Bind` push a map into the tail". 17 omits `Try()`, so a reader who follows 17 alone writes `io.Try()` and fails.
**Correction** 23 owns stack safety. Keep 23:117 as written. Reduce 17:130 to one clause that names the exit only: "A `tail`-recursive `IO` exits through `Run()` or `RunAsync()`; the host that needs a `Fin` captures with `Try.lift(io.Run).Run()`." Delete 22's paragraph (see finding 3.2).
**Files** 18-async.md, 22-languageext-effects.md, 23-languageext-traits-and-transformers.md
**Location** 18:162, 22:133, 23:251
**Claim** The bounded-concurrency chunk recipe appears three times in three spellings: `toSeq(airlines.Chunk(width)).Map(toSeq).TraverseM(SearchParallel)`, `toSeq(items.Chunk(width)).TraverseM(chunk => toSeq(chunk).Traverse(work).As())`, `toSeq(jobs.Chunk(width)).Map(toSeq).TraverseM(Parallel)`.
**Evidence** 23 owns traversal policy; its table row "Bounded concurrency | chunk, then `TraverseM` over the chunks" is the owner statement.
**Correction** Keep 23:251. Delete `Chunked` from 22 and `SearchChunked` from 18, and in each place state the bound in one clause naming the members: "Each fork takes one dedicated thread, so a large fan-out chunks the collection and applies `TraverseM` over the chunks."
**Files** 19-observables.md, 24-languageext-collections-and-shared-state.md
**Location** 19:245 and 19:264 against 24:92
**Claim** `Seq.Scan` emits the seed first, stated once in 24 and twice in 19.
**Evidence** Both 19 sentences carry the same fact ("`Seq.Scan` emits the seed first" and "The seed is emitted first so the first transaction can form a transition").
**Correction** Delete "`Seq.Scan` emits the seed first." at 19:245; the sentence at 19:264 already carries the fact where the reader needs it.
**Files** 21-languageext-result-types.md against 06, 13, 15, 19
**Location** 21:229 anti-pattern row "`IfNone` inside domain code hides absence behind a default"
**Claim** The rule bans `IfNone` in domain code without qualification.
**Evidence** 06:26 `Entries.Find(id).IfNone(onMiss)`, 06:278 `parseInt(text).IfNone(fallback)`, 13:80 `status.IfNone(Status)`, 15:45 `cache.Find(id).IfNone("unknown")`, and 19:117 `History.Find(pair).IfNone(Seq<decimal>())` all call `IfNone` in domain code, and in 06, 13, and 15 the caller supplies the fallback as the teaching point.
**Correction** Narrow the row to the case it means: "| `IfNone` with an invented default hides a missing reason | `ToFin` with an `Error`, as `Required` shows |".

## 3. Ownership and duplication

**3.1** 18-async.md `## Traverse: turn many effects into one effect`, `## Monadic and applicative validation traversal`, `## Traversing tasks` (lines 117-170) restate 23's `## The traversal policy` in full: parallel `Traverse`, serial `TraverseM`, chunked, and `PartitionFallible` with its `Fails`/`Succs` explanation. **Correction** Keep the chapter's `Traverse` signature diagram, `ParseAll`, and the `Traverse`/`TraverseM` contrast; delete `SearchChunked`, `SearchBestEffort`, and the `PartitionFallible` paragraph at 18:169.

**3.2** 22-languageext-effects.md `## Recursion` (lines 149-167) duplicates 23 `## Stack safety`. Both state the `tail` exit rule in the same words and both define the same `CountTo` body. **Correction** Delete 22's `## Recursion` section. 23 owns stack safety; 22 keeps only `RepeatUntil`/`RepeatWhile` if it needs a polling member, named in one sentence under `## Concurrency`.

**3.3** 19-observables.md lines 283-294 duplicate 22-languageext-effects.md lines 285-293 verbatim: `Numbers`, `Doubled`, and `Accumulate` are identical, and 19's `Sum` differs from 22's `Pipeline` only in the method name. **Correction** Delete the `Pipeline` code block from 19 and keep the one sentence already there: "A `ProducerT`, a `PipeT`, and a `ConsumerT` fuse with `|` into one `EffectT` that the host runs."

**3.4** 13-immutable-data.md lines 115-135 (`Lens<A, B>.New`, `Set`, `Update`, `lens(outer, inner)`, and the `Lenses` block) restate 24 `## Lenses`. **Correction** Delete the block and the paragraph; replace with one sentence where the section needs it: "`Lens<A, B>.New` pairs a getter with a setter, and `lens(outer, inner)` composes two so one update reaches a nested field."

**3.5** 15-lazy-try-continuations.md lines 19-26 restate 24:183 near-verbatim ("`memo(Func<A>)` returns a `Memo<A>` whose `Value` runs the function once"). **Correction** Delete the `Twice` block and reduce the paragraph to one clause naming `memo` and `Memo<A>.Value`.

**3.6** 01-functional-model.md lines 178-188 (the `K<F, A>` / `Functor<F>` / `Applicative<F>` / `Monad<M>` / `As()` paragraph and the `Tripled<F>` sample) restate 23 `## Higher kinds`, and 08-core-patterns.md lines 46-52 carries a third copy of the same generic-over-`F : Functor<F>` sample. **Correction** In 01 keep only the Prelude sentence ("The static import of `LanguageExt.Prelude` supplies constructors and functions as bare names") and delete the traits paragraph and the `Tripled<F>` block. Keep 08's copy, which the chapter's functor argument needs.

**3.7** 02-purity.md lines 175-192 (a runtime record with one `Has<Eff<RT>, T>` per capability, plus the `Capabilities` block) restate 22 `## Runtimes`. **Correction** Delete lines 175-192 and the checklist clause "and many capabilities through a runtime `RT`" at 02:227.

**3.8** 04-language-features.md line 113 replaced the book's "Modeling Alternatives Explicitly" body with a runtime-shape roster ("`Option<A>` is a readonly struct, `Fin<A>` is an abstract class, `Either<L, R>` and `Validation<Error, A>` are record classes") that duplicates 21's "One type per concern" table. **Correction** Delete the roster; see finding 6.4 for what the section must carry instead.

**3.9** 20-agents.md line 18 and its `Transfers.Move` block restate 24:183's STM sentence (`Ref<A>` under `atomic`, `swap`, `commute`, `Isolation.Serialisable`), and 20's `Move` body is identical to 24's `SharedState.Move`. **Correction** Keep 20's strategy table row and the one sentence "`Ref<A>` under `atomic` supplies these properties in process." Delete the remaining three sentences and the `Transfers` block.

## 4. Alternatives and hedges left in
No added hedge survives. Every `should`, `may`, `might`, and `could` in the twenty chapters matches book prose within the two book folders (checked line by line against `buonanno-functional-csharp/` and `painter-functional-csharp/`), and the four addenda contain none. The only alternatives-for-one-concern defects are the `Source.Fold` sample (finding 1.4) and the three chunk recipes (finding 2.2).

## 5. Coined terms

**File** 21-languageext-result-types.md, line 172 heading
**Claim** `## Recovery as use`
**Evidence** "as use" is not a phrase the corpus or LanguageExt uses, and the section is about recovery members only.
**Correction** Rename to `## Recovery`.

**File** 06-higher-order-functions.md, line 97
**Claim** "This value-level operation, also called Chain, applies one function to one value; Painter names it `Map`. It is named `Pipe` here so that `Map` keeps its structure-preserving meaning. ... LanguageExt has no value-level pipe, so `Pipe` stays hand-rolled."
**Evidence** The prose rules ban citations and narration about the edit; the book author is named in the text.
**Correction** Replace with: "This value-level operation, also called Chain or Pipe, applies one function to one value. `Map` keeps its structure-preserving meaning over a sequence."

**File** 10-discriminated-unions.md, lines 164-166
**Claim** "recurring shapes are generic and shipped", "The shipped `Option` and `Fin` are closed", "a hand `Success<T>` does not reject `null`".
**Evidence** "shipped" as an adjective and "a hand `Success<T>`" are not corpus terms; the last is ungrammatical.
**Correction** Replace with: "`Option<A>` covers a value or nothing, and `Fin<A>` covers a value or a failure with a reason. Both are closed, so a `switch` over their cases is total. A hand-written `Success<T>` does not reject `null`."
No invented method name survives at the whole-set level: every identifier in the addenda's prose resolves in `LanguageExt.Core`, `LanguageExt.Sys`, or `LanguageExt.Streaming`, and the mechanical sweeps for `Eff<X>` with one argument and for the banned v4 names are empty.

## 6. Drift from the books
Only the load-bearing losses are listed; relocations across the merged set are by design and are not reported.

**6.1** 02-purity.md:49 - "The outer workflow is necessarily impure." is false beside the `Eff<RT, Unit>` code, which is a pure description. **Correction** "`Greet` describes the console reads and writes as an `Eff<RT, Unit>`, and the host performs them at `Run(rt)`."

**6.2** 02-purity.md - Buonanno ch2's `FormatInParallel` example and its payoff sentence are deleted, leaving 02:113 as an unillustrated assertion. **Correction** Append to 02:111: "so the same expression runs in parallel unchanged, because state was represented as input data rather than a shared update."

**6.3** 03-signatures-and-types.md - three stated rules dropped with no replacement: the defect-versus-expected-outcome reason for the private constructor (book L75), the design rule "Keep constructors private when public construction could violate invariants" (book L202), and the C# reason the closed `Some`/`None` union cannot be written (book L141). The last matters because the heading is "The implementation shape in C#". **Correction** Restore all three, adapting the first to `FromUnsafe`.

**6.4** 04-language-features.md - both "Modeling Alternatives Explicitly" and "Nullable Reference Types as a Boundary Guard" lose their bodies and their Title Case, and the nullable body now sits in 03. The file's own opening bullet promises the nullable section. **Correction** Restore both headings with their book names, move the nullable body back from 03:174-196, and keep in the alternatives section the ceremony sentence: "This approximates a discriminated union in C#, although the abstract base and variant declarations require more ceremony than a native union definition."

**6.5** 05-sequences-and-linq.md:116 - the book's rule "Use a specific reduction when one already exists:" and the `Sum`/`Average` examples are deleted, and the section now opens by teaching the reader to hand-roll a sum with `Fold`, which is the opposite instruction. **Correction** Restore the rule and the `Sum`/`Average` forms, then keep `Fold` for the custom reduction. Also delete 05:156-168 (a `Fold`/`FoldBack` diagram and glossary that 24 owns) and rename `## Non-mutating updates to lazy sequences`, which heads a `Seq<A>` example that the same file calls strict, to `## Non-mutating updates to sequences`.

**6.6** 06-higher-order-functions.md - `### Transduce: transform a sequence, then aggregate` is deleted with no replacement anywhere in the set (the string "transduce" appears in no corpus file), and the all-same-type `Map` overload with its tradeoff sentence is gone. **Correction** Restore the subsection between `Compose` and `Do` over `Seq<A>`, and restore the second overload plus "This form is shorter, but it cannot represent a type change between steps."

**6.7** 07-currying-and-partial-application.md - the painter sections are reordered against each other with no cause, `### Curried functions in higher-order pipelines` now contains only `par` and no `curry`, and 07:292 still lists "no native currying or general partial-application mechanism" as a cost after 07:47 states the Prelude ships `par` and `curry`. **Correction** Restore the book order, restore the `FahrenheitToCelsius` direction of the temperature example, and change the cost bullet to "no currying or partial-application syntax in the language; both arrive as Prelude functions".

**6.8** 08-core-patterns.md - three dropped rules with no replacement: the reason a dedicated `Iter` beats overloading `Map` with an `Action<T>` (book L76), the `Option<Seq<A>>` flattening direction (08:163 still says "Each turns a sequence of options into a plain `Seq<A>`" while only one direction is shown), and the deep-stacking warning about `A<B<C<D<T>>>>` (book L231). **Correction** Restore all three; append the stacking warning to 08:212 beside the `OptionT<IO, A>` sentence.

**6.9** 10-discriminated-unions.md - the whole `## Reusable Generic Unions` section collapses from four subsections to two paragraphs, and three teaching points vanish: the warning that returning `Nothing<T>` for an operational error hides the failure, the empty-versus-missing collection convention, and the boilerplate cost of encoding more than two alternatives. **Correction** Restore the four subsections and the three rules, each stated over `Option`, `Fin`, and `Either`.

**6.10** 11-error-handling.md:38 "That is the shape every type in this file has." and 11:142 "This block is host-only: `IActionResult`, `Ok`, and `BadRequest` come from the web framework." are narration about the edit. **Correction** Delete both sentences; 11:133 already says translation happens in the outer adapter.

**6.11** 12-applicatives-and-laws.md - 12:46 "delegate to this unary implementation" dangles because the unary `Apply` body was removed; 12:226 "failures from both operands" is false beside a three-validator example; the rule "keep the aggregate constructor private" is dropped while `PhoneNumber` at 12:195 has a public constructor. **Correction** "delegate to the unary `Apply`, so one effect-specific rule serves every arity"; "both operands" to "every operand"; restore the constructor clause.

**6.12** 13-immutable-data.md - 13:97 describes a `toSeq` defensive copy that no code in the file performs; 13:221 and 13:244 describe the removed hand-built `Tree<T>` operations; 13:253 rule 6 refers to hand-built list and tree implementations the file no longer contains. **Correction** Add a boundary factory that calls `toSeq`, rewrite 13:244 as "An `Add` rebuilds only the nodes on the path from the root to the new key and shares every untouched subtree", and delete rule 6.

**6.13** 15-lazy-try-continuations.md - 15:183 names "a connection string, logger, or operation name" for signatures that carry only an operation name, and 15:189 names a `trace` operation the file no longer shows. **Correction** Trim 15:183 to "such as the operation name" and drop "or trace" from 15:189.

**6.14** 16-stateful-computations.md - 16:70 "The installed build cannot fork a `StateT` over `IO`" narrates the toolchain rather than the subject; 16:177 "A generator is `State<S, A>` specialized to an integer seed" inverts general and specific; the book's clock-seed rule ("a convenience runner that seeds from the clock is impure and not testable") is dropped with no replacement. **Correction** Delete 16:70. Write 16:177 as "A generator is `State<int, A>`: the general `State<S, A>` specialized to an integer seed." Restore the clock-seed sentence after 16:104.

**6.15** 17-indefinite-loops.md - `### Recursion in C#` (lines 49-90) is a heading the book chapter does not have, and the "Choosing an approach" table lost the book's `Tail recursion` row while `## Tail recursion` immediately follows it, so the table no longer lists the approach it then teaches. **Correction** Delete the added heading and fold its two limit bullets into the paragraph at 17:47. Restore the table row: `| Tail recursion | Small, direct, expression-oriented | Unbounded calls can grow the stack in C# | The maximum depth is small and bounded |`.

**6.16** 18-async.md:148 - "The same operation combines many validators for one value." points at an operation the file no longer shows; the book's `HarvestErrors` example was removed with nothing in its place. 18:57 also drops the book's `async`/`await` definition while the file still uses both at 18:219. **Correction** Restore the example as `validators.Traverse(v => v(value)).Map(_ => value)` with its two-sentence explanation, or delete 18:148. Restore the `await` sentence at 18:57.

**6.17** 19-observables.md - the `CombineLatest` rule ("appropriate when either input invalidates a derived value") is deleted while `CombineLatest` remains a row in the operator table at 19:105 and `Zip` is silently substituted; the book's timer and task sources are gone while layer 1 at 19:56 still promises "Adapt timers, callbacks, tasks, collections". **Correction** Add after 19:205: "Use a latest-wins combine when either input invalidates the derived value; `Zip` is the choice when each value has one matching partner." Add a timer source and a lifted-effect source to the `Sources` block.

**6.18** 20-agents.md:54 - "The state type is hidden because it is an implementation detail; callers only need the message contract." is false beside the replacement API: `Start` returns `IO<ForkIO<S>>` and the caller holds `Conduit<M, M>`. **Correction** "The state type appears only in the started fork's result; callers hold the inbox and the message contract."

## 7. Boundary placement
`Match`, `Run`, `RunSafe`, `IfNone`, and `IfFail` appear only at host positions or on `Option` eliminations that the surrounding chapter teaches; the one rule defect is the over-broad `IfNone` row reported in finding 2.4. The sweeps for `IO<Option<`, `IO<Fin<`, `Eff<X>` with one type argument, and error classification by message text are all empty. `Validation` appears mid-chain nowhere: 11:202 and 21:70 both convert with `ToFin` at the admitting boundary before the dependent chain begins.

## 8. Presentation

**8.1** 14-event-sourcing.md - `internal static class Account {` opens in the fence at lines 99-110 and closes only at line 184, in the fence at 178-185, across two `##` headings and three fences. The fence at 128-133 (`Rebuild`) is four-space indented with no visible enclosing type, and the fence at 178-185 follows a fence of `record` declarations, so its closing `}` reads as closing a record. **Correction** Give each of the three fences its own `internal static class Account { ... }` wrapper, or merge them into one fence.

**8.2** 18-async.md - the same defect twice. `internal static class Traversals {` opens at 128-132 and closes at 154-167, across two `##` headings and three fences. `internal static class Stacks {` opens at 191-223 and closes at 229-251. **Correction** Wrap each fence in its own class declaration.

**8.3** 18-async.md line 191 block - the block uses `Eff<Runtime, AccountState>` at line 248 but never declares `Runtime`. The lab's `F18/Stacks.cs:47` has `internal sealed record Runtime;`, which the doc block dropped, so the doc text is not the lab text. **Correction** Add `internal sealed record Runtime;` to the block beside the other record declarations.

**8.4** 04-language-features.md lines 143, 163, and 177 declare three different types named `Movie` in one file (two `internal readonly struct`, one `internal sealed record`), and none of the three appears in `lab-functional-csharp/F04`. The file states that every `csharp` block compiles. **Correction** Give each block a distinct name (`MovieFields`, `MovieInit`, `Movie`) and add all three to the lab, or mark the two struct blocks `text`.

**8.5** 13-immutable-data.md line 59 - `AccountState frozen = active.With(AccountStatus.Frozen);` is a bare statement in a `csharp` fence with no enclosing scope and no declaration of `active`, and it is not in the lab. **Correction** Fence it as `text`, or place it inside the `AccountState` sample class as `public static AccountState Frozen(AccountState active) => active.With(AccountStatus.Frozen);`.

**8.6** 10-discriminated-unions.md line 20 - `internal sealed record Customer(string Email, bool IsRegistered, string Name, bool IsEligible);` collides with `internal abstract record Customer;` at line 24 of the same file, and it is not in the lab. **Correction** Fence the anti-pattern block as `text`.

**8.7** 16-stateful-computations.md - five `csharp` fences (110-117, 125-127, 135-145, 153-155, 159-169) open with four-space-indented `public static` members and no enclosing type anywhere in the file. The lab's `F16/Generator.cs` wraps all of them in `internal static class Generator { ... }`, which the doc dropped, so the doc text is not the lab text and a reader cannot place any of the five. **Correction** Open `internal static class Generator {` in the first fence and close it in the last, or repeat the declaration in each fence.

**8.8** 20-agents.md lines 125-138 - `Counting.Process` is declared and never wired: `Counter` posts to an `inbox` that no shown call starts with `Agent.Start(inbox, 0, Counting.Process)`. **Correction** Add the start call to `Counter`, or name the wiring in the paragraph above the block.

**8.9** Table rows over 150 columns: 17-indefinite-loops.md:26 (151) and :28 (164); 20-agents.md:15 (169) and :16 (163). **Correction** Trim the cells to the fact: 17:26 "final or intermediate values through `Bind`" to "values through `Bind`"; 17:28 "Intermediate states are meaningful or need transformation" to "Intermediate states are meaningful"; 20:15 "Coordinated in-memory updates with isolation and atomic commit" to "Coordinated in-memory updates, atomic commit"; 20:16 "State owned by a lightweight process and changed only while handling messages" to "State owned by a process, changed only in a handler".

## 9. Lab gates
All green except the code-text mismatches already reported.

- `dotnet build .scratch/lab-functional-csharp/Lab.csproj -nologo -v q`: 0 warnings, 0 errors.
- `dotnet run --project .scratch/lab-functional-csharp/Lab.csproj --no-build -nologo`: exit 0.
- `wc -l`: every file under 300, maximum 299 (06, 07, 12, 22).
- `rg -n '[0-9]{2}-[a-z-]+\.md'`: empty. Banned-token sweep: empty. `IO<Option<` / `IO<Fin<`: empty. One-argument `Eff<X>`: empty.
- Doc-versus-lab code diff over all twenty-four files: the only doc lines that appear in no lab file are 04:143-188 (finding 8.4), 10:20 (8.6), 13:59 (8.5), and the labelled host-only block at 11:136-139. The only lab line missing from its doc is `F18/Stacks.cs:47` (8.3).

## Blockers

- 1.1 - 18:13 states a false mechanism for `IO<A>` and contradicts 21's table.
- 1.2 - 21:174 and 23:41 state a `Catch` return-type rule that 10:150 and 22:60 break.
- 1.4 - 22 ships a `Source.Fold` sample that the same page says emits nothing.
- 2.1 - the `tail` exit rule loses `Try()` in 17, so a reader who follows 17 alone writes failing code.
- 8.1, 8.2, 8.3, 8.7 - four classes split across or dropped from their fences with no visible close, and two doc blocks that omit a type the lab declares.
- 8.4, 8.5, 8.6 - four `csharp` blocks that do not compile and are absent from the lab, including three colliding `Movie` declarations in one file.
- 6.4 - 04 promises a nullable-reference section in its own opening bullet and no longer has one.
- 6.5 - 05 teaches the reverse of the book's stated reduction rule.
- 6.15 - 17's approach table omits the approach the next section teaches.

## Non-blockers

- 1.3 - `ErrorException` over-generalized in 22.
- 2.2, 2.3, 2.4 - repeated chunk recipe, repeated `Scan` fact, over-broad `IfNone` row.
- 3.1 to 3.9 - nine ownership duplications; each costs lines that the affected chapters need for their own material.
- 5 - three coined or cited phrases.
- 6.1 to 6.3, 6.6 to 6.14, 6.16 to 6.18 - book drift; each is a one-sentence or one-block restore.
- 8.8, 8.9 - one unwired sample and four wide table rows.
