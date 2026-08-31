# APPLIED - closing edits, file by file

## 21

Applied: F21 items 1 (NOTES/F21.md bullet; LAB-NOTES already carried the retraction), 2, 3, 4 (DECISIONS `Catch` wording), 5, 6, 7, 8; adversarial 2.4 (`IfNone` row); the `Age` correction from F03 item 5 (private constructor, `FromUnsafe(int repr) => From(repr).ThrowIfFail()`) in doc and `F21/Lifts.cs`.
Rejected: none.
Build 0/0, run exit 0, 247 lines, sweeps empty, 8 blocks identical.

## 22

Applied: F22 items 1 (DECISIONS `Run`/`RunAsync` wording), 2, 3 (`Inside` deleted from doc, `F22/Exits.cs`, and the `Samples` check), 4, 5, 6 (folded into the set-wide `Traverse`-under-`IO` wording), 7, 8, 9, 10 (API document bullet); adversarial 1.4 (`Folded` deleted from doc, `F22/Streams.cs`, and the `Samples` check; the `Fold` sentence stays per DECISIONS); the set-wide `tail` exit wording and `RepeatWhile` wording. The `Recursion` section and `Chunked` stay per DECISIONS (adversarial 3.2 and 2.2 overridden).
Rejected: F22 item 11 (no edit inside this file; the set owner made no ruling for the `Buffer` list), item 12 (no edit requested).
Build 0/0, run exit 0, 291 lines, sweeps empty, 8 blocks identical.

## 23

Applied: F23 items 1 (row cell, `Parallel` renamed `Overlapped` in doc, `F23/Traversals.cs`, `Samples.cs`, and NOTES; the set-wide `Traverse`-under-`IO` paragraph replaces the item's added sentence), 2 (DECISIONS `Catch` fact), 5 (NOTES/F23.md `tailIO` line), 6, 7 (`Counters.Lifted` in doc and lab with a `Samples` check, plus the sentence); adversarial 1.2 (line 41), the set-wide `tail` exit wording. Item 3 settled per DECISIONS (section and line 3 kept). Item 4 is applied in 18.
Rejected: none.
Build 0/0, run exit 0, 263 lines, sweeps empty, 6 blocks identical.

## 24

Applied: F24 items 1 (table row and the set-wide `Seq` memoization wording), 2, 6 (NOTES/F24.md bullet); the set-wide `commute`/`atomic` wording. `LAB-NOTES.md`: no row calls `Seq` strict (the F13 row's `toSeq(IList<A>)` copies eagerly is true for that overload); the set-wide corrections section already carries the memoization fact, so nothing changed there.
Rejected: item 3 (DECISIONS keeps 24's `## Lenses`), item 4 (DECISIONS keeps 24's `Move`; 20 drops its copy), item 5 (no change required).
Build 0/0, run exit 0, 227 lines, sweeps empty, 6 blocks identical.

## 03

Applied: F03 items 1 (private constructor, get-only `Value`, `FromUnsafe(int repr) => From(repr).ThrowIfFail()` in doc and `F03/Age.cs`, plus the prose sentence), 2 (restored clause and design rule), 3 (`Match` at the host sentence, split at the semicolon), 4 (NOTES/F03.md). Per DECISIONS the closed-union reason (adversarial 6.3 third item) is not restored, and the nullable section stays.
Rejected: none.
Build 0/0, run exit 0, 215 lines, sweeps empty (the three `may` hits are book prose), 6 blocks identical.

## 08

Applied: F08 items 1 (the corpus `Age` with the DECISIONS private constructor and `FromUnsafe(int repr) => From(repr).ThrowIfFail()`, `ToOption` at the boundary in `ParseAge`, `To()` in `TotalAge`; doc, `F08/Domain.cs`, `CorePatterns.cs`, `Samples.cs`), 2, 3, 4, 5; adversarial 6.8 (the `Iter`-versus-`Map` overload sentence restored with the file's names, the `Option<Seq<A>>` direction restored as `Flattened` with `ToSeq().Flatten()` and one sentence, the `A<B<C<D<T>>>>` clause restored verbatim). Every member fragment fence wrapped in `internal static partial class CorePatterns { ... }`; the lab holds the same parts plus one extra part for the members the doc does not show.
Rejected: none.
Build 0/0, run exit 0, 251 lines, sweeps empty (four `may` hits are book prose), 15 blocks strict.

## 11

Applied: F11 items 1 (`Codes` block, `Codes.InvalidBic` in `Catch`; `F11/Typed.cs`, `Adapters.cs`), 2 (`Post` with `Workflow.Handle`, host-only block), 3, 5 (the one-sentence read-out); adversarial 6.10 (both narration sentences deleted; F11 item 4 superseded by the deletion).
Rejected: none.
Build 0/0, run exit 0, 242 lines, sweeps empty (three `should`/`may` hits are book prose), 7 blocks strict plus the labelled host-only block.

## 12

Applied: F12 items 1, 2, 3 (`internal static partial class Validators` around both fences; lab split into the two parts plus a `ValidatorsProbe`), 4, 5, 6; adversarial 6.11 (unary `Apply`, "every operand", the book's "keep the aggregate constructor private" clause restored verbatim). The `Queries` and `PhoneNumbers` lab classes now match their doc blocks exactly, with the probes moved to `QueriesProbe`/`PhoneNumbersProbe`. To stay under 300 lines the one-line `text` fence `Functor < Applicative < Monad < Fold` became an inline code span in its lead sentence (no content lost).
Rejected: none.
Build 0/0, run exit 0, 299 lines, sweeps empty (five hedge hits are book prose), 5 blocks strict and 8 statement fragments matched with one indent shift (the lab holds them inside probe methods).

## 18

Applied: F18 items 1, 3, 5, 6, 7 (`Recover` moved to `Host` in doc, lab, and `Samples`), 8; the set-wide `Traverse`-under-`IO` wording (F18 item 2, F23 item 4); adversarial 3.1 per DECISIONS (`SearchChunked` deleted from doc, lab, and `Samples`; `SearchBestEffort` and the `PartitionFallible` sentence kept), 8.2 (each `Traversals` and `Stacks` fence wrapped in its own `partial` class), 8.3 (`internal sealed record Runtime;` in the block). Lab `Codes.cs` merged into `Lifts.cs` so block 1 is one span.
Rejected: none.
Build 0/0, run exit 0, 266 lines, sweeps empty (one `may` is book prose), 9 blocks strict.

## 09

Applied: F09 items 1, 2, 3, 4.
Rejected: none.
Build 0/0, run exit 0, 245 lines, sweeps empty, 8 blocks strict.

## 14

Applied: F14 items 1 (`Account` as three `partial` parts in doc and `F14/Account.cs`), 2, 3, 4, 5 (the one-sentence form).
Rejected: item 6 (no doc change required; not in DECISIONS).
Build 0/0, run exit 0, 272 lines, sweeps empty, 9 blocks strict.

## 15

Applied: F15 items 1, 2, 3, 4 (the one-fact replacement), 5; adversarial 6.13. Every member fragment fence wrapped in its lab class (`Laziness`, `Composition`, `Parsing`, `Environments`, `Scopes`) as `partial` parts; the lab regenerated from the doc blocks with `DeleteRejected` in an extra `Scopes` part. The `memo` paragraph and `Twice` block stay per DECISIONS (adversarial 3.5 overridden).
Rejected: item 6 (no change: the Reader section is plan-mandated), item 7 (folded into item 1).
Build 0/0, run exit 0, 282 lines, sweeps empty, 16 blocks strict.

## 16

Applied: F16 items 1, 2, 3, 4; adversarial 6.14 (the toolchain clause deleted, the `Fork` fact kept as its own sentence; the generator sentence; the clock-seed rule restored after the `Run(seed)` sentence); adversarial 8.7 (the five generator fences wrapped in `internal static partial class Generator { ... }` and `F16/Generator.cs` regenerated as five parts).
Rejected: none.
Build 0/0, run exit 0, 235 lines, sweeps empty, 9 blocks strict.

## 17

Applied: F17 items 1, 2, 3, 4 (set-wide `tail` wording), 5, 6, 7; adversarial 6.15 (the `### Recursion in C#` heading deleted so its content folds into `## Tail recursion`; the `Tail recursion` table row restored, four rows per DECISIONS) and 8.9. The two limit bullets stayed beside `RunUntil` because they name its `next` and `stop` parameters. "all three choices" became "every choice" since the table now has four rows.
Rejected: none.
Build 0/0, run exit 0, 204 lines, sweeps empty, 8 blocks strict.

## 20

Applied: F20 items 1 and 7 per DECISIONS (the `Transfers` block deleted from doc and lab with its probe; line 18 reduced to the one sentence), 3, 4, 5, 6; item 2 and item 8 in NOTES/F20.md; adversarial 6.18, 8.8 (the `Agent.Start` wiring named in the paragraph), 8.9 (both rows, the STM limitation cell trimmed again to fit).
Rejected: none.
Build 0/0, run exit 0, 264 lines, sweeps empty, 4 blocks strict.

## 05

Applied: F05 items 1, 2, 3, 4, 5; the DECISIONS reduction sentences before the `Fold` sum; the `Fold`/`FoldBack` diagram kept (adversarial 6.5 second half rejected per DECISIONS). Every fragment fence wrapped in `internal static partial class Sequences { ... }`; block 12 keeps the `Story` record above a wrapped part, so the record moved from `Domain.cs` into the `Sequences.cs` span; the helper members the doc does not show sit in an extra part.
Rejected: none.
Build 0/0, run exit 0, 297 lines, sweeps empty, 12 blocks strict.

## 06

Applied: F06 items 2, 3, 4, 5, 6, 7 and adversarial 5 (the Pipe sentence, which supersedes item 1). `Transduce` not restored: the file is at the line limit with nothing lab-backed to remove; recorded in NOTES/F06.md.
Rejected: none beyond the recorded omission.
Build 0/0, run exit 0, 299 lines, sweeps empty (four hedge hits are book prose), 13 blocks strict.

## 07

Applied: F07 items 1, 2, 3 (doc, `F07/Root.cs`, and the sentence), 4, 5; the `FahrenheitToCelsius` direction restored (`SubtractBase` replaces `AddBase` in doc, `Pipeline.cs`, and the `Samples` checks).
Rejected: adversarial 6.7 section reorder: the chapter nests the Painter sections under the Buonanno headings and "Partial application in C#" already points forward at `Parsing.ParseBooks`, so the move is not pure.
Build 0/0, run exit 0, 296 lines, sweeps empty (hedge hits are book prose), 13 blocks strict.

## 13

Applied: F13 items 1, 2, 3, 4 (the minimal form, since the paragraph is Painter prose), 5, 6, 7; adversarial 6.12 (`AccountState.Opened(CurrencyCode, IList<Transaction>)` calling `toSeq` in doc and lab with the `toSeq` sentence pointed at it, the `Add` rebuild sentence, rule 6 in the DECISIONS wording), 8.5 (line 59 became `Transitions.Frozen` in doc, lab, and `Samples`). Item 8 is covered by the `Opened` factory.
Rejected: none.
Build 0/0, run exit 0, 262 lines, sweeps empty, 6 blocks strict.

## 10

Applied: F10 items 1, 2, 3 and 4 (merged with the adversarial 5 wording and the DECISIONS "operational failure to `None`" sentence; "a `Match` over their cases is total" replaces the adversarial's `switch`, since `Option` is a struct read through `Match`), 5; adversarial 8.6 per DECISIONS (`FlaggedCustomer` in doc and lab, `Flagged` folder dropped).
Rejected: adversarial 6.9 per DECISIONS (no subsections restored).
Build 0/0, run exit 0, 178 lines, sweeps empty, 8 blocks strict.

## 02

Applied: F02 items 1 (set-wide `Traverse`-under-`IO` wording), 2 and 3 (paragraph replaced), 4, 5, 6, 7, 8; adversarial 6.1, 6.2. The runtime record and `Capabilities` block stay per DECISIONS (adversarial 3.7 overridden).
Rejected: none.
Build 0/0, run exit 0, 229 lines, sweeps empty, 7 blocks strict.

## 04

Applied: F04 items 1, 2, 3, 4, 5; DECISIONS shape sentence kept with the book's ceremony sentence restored; adversarial 8.4 per DECISIONS (`MovieFields`, `MovieInit`, `Movie` in doc and one lab `Movies.cs`, sub-folders dropped, `Samples` updated).
Rejected: none.
Build 0/0, run exit 0, 199 lines, sweeps empty, 6 blocks strict.

## 01

Applied: F01 items 1, 2, 3, 4 (book list restored verbatim, semicolons included as book prose), 5; the member fragment fences wrapped in `internal static partial class CoreProperties`/`Traits` and the lab regenerated from the doc blocks with the extra members in separate parts. The Prelude paragraph and the `Tripled<F>` block stay per DECISIONS (adversarial 3.6 overridden). One blank line inside the first block dropped in doc and lab to hold the file at 299.
Rejected: item 6 (optional, not in DECISIONS).
Build 0/0, run exit 0, 299 lines, sweeps empty, 5 blocks strict and 5 statement fragments matched with one indent shift (the lab holds them inside methods).

## 19

Applied (F19 report absent): adversarial 2.3, 3.3 (`Pipeline` block deleted from doc, lab, and `Samples`; the sentence kept), 6.17 (the latest-wins/`Zip` rule as two sentences; no timer or lifted-effect source added because no lab-verified member backs one, so "timers" dropped from the layer sentence per DECISIONS).
Rejected: none.
Build 0/0, run exit 0, 282 lines, sweeps empty, 9 blocks strict.
