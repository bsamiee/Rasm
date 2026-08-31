# Coordinator decisions for the corrections pass

These rulings settle conflicts between the review reports and the plan. Where a report and this file disagree, this file wins. Where two reports disagree, this file names the winner.

## Ownership versus self-containment

- Every file stays self-contained: no cross-file reference, no "see the addendum". A chapter that the plan asked to integrate a member keeps that member: one sentence that names it and, where the chapter's teaching point needs it, one short sample. The addendum owns the full treatment.
- Delete byte-identical duplicates of an addendum's code block in the non-owner: the `Pipeline` block in 19 (22 owns it), the `Transfers.Move` block in 20 (24 owns it). Keep one sentence in each place.
- KEEP (plan-mandated, shrink only if a reviewer's correction says so): 01's Prelude paragraph and the `Tripled<F>` sample (apply the F01 wording corrections), 02's runtime record and `Capabilities` block (apply the F02 corrections), 13's lens paragraph and block (apply the F13 corrections), 15's `memo` paragraph and `Twice` block, 18's `Traverse`/`TraverseM`/`Validation` traversal sections, 22's `Recursion` section and `Chunked` sample.
- 18 deletes `SearchChunked` and keeps `PartitionFallible` as one sentence plus its one-line sample; 23 keeps its traversal table; 22 keeps `Chunked`.
- 04 keeps the one-sentence shape statement (`Option` readonly struct, `Fin` abstract class, `Either` and `Validation` record classes, all through `Match`) and restores the book's ceremony sentence beside it (adversarial 6.4, second half). Restore the two book headings (`## Modeling Alternatives Explicitly`, `## Nullable Reference Types as a Boundary Guard`). Restore the nullable section's three claims as prose in 04 (F04 item 2); 03 keeps its own nullable section unchanged.
- 10: do not restore four subsections (adversarial 6.9). Restore the two teaching points as one sentence each: the empty-versus-missing collection (F10 item 4) and "a producer that maps an operational failure to `None` hides the failure from the consumer". Apply the F10 wording corrections and the adversarial 5 wording for lines 164-166.
- 03: apply F03 items 1 to 3 (private constructor, `FromUnsafe(int repr) => From(repr).ThrowIfFail()`, restored clause and design rule, `Match` at the host sentence). Do not restore the "C# cannot write the closed union" reason; the shipped struct replaced it.
- The same `Age` correction applies to 21 (F03 item 5) and 08 (F08 item 1 with the `ToOption` boundary).

## Set-wide facts (see also LAB-NOTES.md, "Set-wide corrections from the reviews")

- `Traverse` under `IO` (F23 probe: three 200 ms `IO.liftAsync` effects took 216 ms under `Traverse` and 629 ms under `TraverseM`; three `IO.lift` effects took 652 ms under `Traverse`, in order, on one thread). One wording for 02, 18, 22, 23, and the brief's traversal row: "`Traverse` under `IO` starts every element effect before it awaits any: effects built with `IO.liftAsync` overlap without a bound, and effects built with `IO.lift` run in order on the calling thread. `TraverseM` runs the effects one after another. `Fork` takes one thread per fork, so a large fan-out chunks the collection first." Rename 23's `Parallel` sample to `Overlapped` (doc and lab). `awaitAll(Seq<IO<A>>)` forks nothing.
- `Seq<A>` is memoized, not strict (F24: `toSeq` copies an array, a list, or a collection at once and wraps any other `IEnumerable<A>` in a lazy memoizing sequence; `Seq.Map`/`Filter` return lazy sequences). One wording for 05, 24, and any file that says `Seq` is strict: "`Seq<A>` reads its source once and memoizes every item, so a second enumeration repeats no work. `toSeq` copies an array, a list, or a collection at once. `Map` and `Filter` on a `Seq` are deferred until enumeration. `Iterable<A>` holds no memory, so a second enumeration runs the source again." Fix `LAB-NOTES.md` (the F05 row) and the 24 table row (`the default, ordered, memoized`). Apply F24 items 1, 2, 6; keep 24's `## Lenses` (the plan gives `Lens` to 24; 13 keeps its short block) and keep 24's `Move` (20 drops its copy).
- 23: apply F23 items 1, 2, 5 (notes), 6, 7 (the `Lifted` member and its sentence); item 3 is settled by keeping both 22's `Recursion` section and 23's `Stack safety` section with the one `tail` wording, and line 3 of 23 keeps its ownership sentence.
- `Catch`: the code and error-value overloads are extensions returning `K<F, A>` (`.As()` follows); `IO<A>` declares the predicate overload as an instance method returning `IO<A>`. Fix 21 (item 4 / adversarial 1.2) and 23 line 41.
- `tail` exit rule, one wording in 17, 22, 23: "A `tail`-recursive `IO` exits through `Run()` or `RunAsync()` only. `RunSafe()`, `Try()`, `Map`, and a later `Bind` push a map into the tail and fail. A host that needs a `Fin` captures with `Try.lift(io.Run).Run()`."
- `Run`/`RunAsync`: "throw on failure and belong to `Main`. An `Expected` error arrives as an `ErrorException`, and an `Exceptional` error rethrows the captured exception."
- `RepeatWhile` polls while the predicate holds; `Retry(Schedule)` reruns a deferred effect that failed (a pre-built `IO.fail` is not rerun); `Source<A>` is a stream of values; no "fork pool".
- 22: delete the `Folded` member from the block and the lab; keep the one sentence that states `Fold` emits nothing for a lifted finite sequence and that `Reduce` is the fold. Delete the `Inside` member and its clause (F22 item 3). Apply F22 items 1, 2, 4 to 9.
- `commute`: file 24 states "`commute` applies its function inside the transaction and again at the commit point against the last committed value; `atomic(Func<R>)` returns the in-transaction result". File 20 keeps only "`Ref<A>` under `atomic` supplies these properties in process, and a transaction body holds no effects because a conflict re-runs it."
- 21 anti-pattern row: narrow to "`IfNone` with an invented default hides a missing reason" (adversarial 2.4).
- 11: add the `Codes` block and use `Codes.InvalidBic` in `Catch` (F11 item 1); rename the host-only method to `Post` with `Workflow.Handle` (F11 item 2); delete the two narration sentences (adversarial 6.10) and apply F11 items 3 and 5.
- 12: apply all six F12 items and adversarial 6.11.
- 16: apply F16 items 1 to 4 and adversarial 6.14.
- 17: apply F17 items 1 to 7, plus adversarial 6.15 (delete the added `### Recursion in C#` heading, fold its content, restore the `Tail recursion` table row) and 8.9 (table widths). Where F17 item 2 (`Monad.recur` alone in row 2) and the restored row differ, the table has four rows: `Tail recursion`, `Trampoline<A>`, `Monad.recur`, `LanguageExt.List.unfold`; `RepeatUntil` stays in its own paragraph.
- 14: partial classes per F14 item 1; apply F14 items 2 to 5 (item 5: the one-sentence form).
- 18: wrap each `Traversals` and `Stacks` fence in its own `partial` class, add `internal sealed record Runtime;` to the block (adversarial 8.2, 8.3), move `Recover` into `Host` (F18 item 7), apply F18 items 1, 5, 6, 8 and the set-wide traversal wording.
- 16: wrap the five generator fences in `internal static partial class Generator { ... }` (adversarial 8.7).
- 04: rename the three `Movie` types `MovieFields`, `MovieInit`, `Movie` in doc and lab (drop the sub-folders); apply F04 items 3 to 5.
- 13: make line 59 a member of the sample class (`Frozen`), apply F13 items 1 to 7 and adversarial 6.12 (except: keep rule 6 reworded to "Treat the list and tree models as models; the production containers are `Seq`, `Lst`, `Map`, and `HashMap`").
- 10: rename the anti-pattern record `FlaggedCustomer` in doc and lab (drop the `Flagged` sub-folder).
- 20: apply F20 items 1, 3, 4, 5, 6 and adversarial 6.18, 8.8 (name the wiring in the paragraph), 8.9.
- 05: restore the book rule sentence "Use a specific reduction when one already exists" followed by one sentence: "On a `Seq` the simple `Sum()` and `Average()` calls are ambiguous between the LanguageExt and LINQ forms, so `Fold` is the reduction shown." Apply F05 items 1 to 5. Do NOT delete the `Fold`/`FoldBack` diagram (adversarial 6.5 second half is rejected; the chapter needs it).
- 06: apply F06 items 1 to 7 and adversarial 5 (Pipe sentence); restore the `Transduce` subsection only if it fits under 300 lines without removing lab-backed code; otherwise record the omission in NOTES/F06.md.
- 07: apply F07 items 1 to 5; restore the book section order and the `FahrenheitToCelsius` direction (adversarial 6.7) if the moves are pure and the file stays under 300 lines.
- 01: apply F01 items 1 to 5.
- 02: apply F02 items 1 to 8 and adversarial 6.1, 6.2.
- 08: apply F08 items 1 to 5 and adversarial 6.8.
- 09: apply F09 items 1 to 4.
- 15: apply F15 items 1 to 5 and adversarial 6.13.
- 19: apply the F19 report when it lands, plus adversarial 2.3, 3.3, 6.17 (the timer source only if it compiles without new packages; otherwise the sentence at 19:56 drops "timers").
- 23, 24: apply their reports when they land, plus the set-wide facts above.
- API document `.scratch/api-languageext.md`: apply F22 item 10 ("An `Error` thrown by the effect returns unchanged") and nothing else unproven.

## Method

- One agent applies everything, file by file, with the smallest edit that satisfies each item. Book prose is restored verbatim where a report says a book claim was dropped without cause.
- After each file: rebuild that file's lab folder inside the shared lab (`dotnet build .scratch/lab-functional-csharp/Lab.csproj -nologo -v q` must stay at zero diagnostics, `dotnet run ... --no-build` at exit 0), re-check `wc -l` under 300, and re-run the brief's sweeps on the file.
- Doc blocks and lab text stay identical. Where a block changes, the lab file changes with it, and the reverse.
- Do not add new members that no report or LAB-NOTES verifies.
