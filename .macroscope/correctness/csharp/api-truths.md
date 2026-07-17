---
include:
  - "libs/csharp/**"
  - "**/*.cs"
---

# [CSHARP_API_TRUTHS]

Verified library and host truths a generic reviewer misjudges — each listed shape is deliberate or required; flagging it, or demanding its inverse, is a false positive. `LanguageExt.Prelude` is static-imported, so bare `Optional`, `guard`, `Some`, `None`, `unit`, `fun`, `pure`, and `atomic` are legal unqualified; fences use neutral placeholder identifiers (`<value-a>`, `Route`, `Fault`) by law, never as unfinished work.

## [01]-[THINKTECTURE_AND_LANGUAGEEXT]

- `partial` struct/class/record with no visible body (`public readonly partial struct FieldKey;`) is generator-completed, not an empty type; `[Union]` on an abstract partial root with nested sealed cases is the canonical family, not nested-class smell; attribute walls of `OperatorsGeneration` knobs on a `[ValueObject]` are load-bearing algebra grants.
- State-threaded `Switch(state: ..., static (s, c) => ...)` collapsing 3+ case overrides into one parent body is the canonical shape; explicit `Switch<TState, TResult>` type arguments and a positional leading state argument are deliberate compiler workarounds — never demand per-case virtual methods back or the removal of the type arguments.
- An optional parameter cannot default to a `[ValueObject]`/`[SmartEnum]` value or `default` (CS1736/TTRESG047); `Option<TVo> = default` plus `IfNone` is the fix, never a smell.
- `[Equatable]`/`[OrderedEquality]` (Generator.Equals) on class-root unions is intentional — class roots forfeit generated record equality.
- `.As()` re-anchoring after every trait combinator (`TraverseM`, `Traverse`, tuple `.Apply`) is mandatory, not redundant; `.TraverseM` (abort) versus `.Traverse` (accumulate) is a correctness choice, not duplication.
- `Try.lift(...).Run().MapFail(...).Bind(static r => r)` self-flatten is required — dropping the `.Bind` discards the error; `K<F, A>` carrier-polymorphic bodies under trait constraints are one body serving all carriers, never an unresolved generic.
- Operator glyphs are per-carrier algebras (`|` as Validation-choice, Fallible-catch, Schedule-union, or Eff-finally); constrained operators live only inside `extension<E, F, A>(K<F, A>)` blocks — the sanctioned mechanism.
- A domain union kept as abstract record plus `switch (this)` instead of `[Union]` is a deliberate CS9244 avoidance where a `ref struct` state parameter flows through record consumers.
- An outer lambda parameter renamed from `_` to a real binding where the body uses `_ = expr` is a required CS8820/CS0029 fix — never restore the discard convention.
- A block-body lambda copying a mutable struct field to a local before use is required; the IDE0200 method-group reduction silently freezes the receiver by value — never accept it.
- Operators without `Negate`/`Multiply` named alternates are canonical (CA2225 is silenced estate-wide); per-type epsilon lives as a typeclass interface member, never one shared constant across differing-unit types.
- Inside a `from..select` expression, `group`/`by`/`into`/`on`/`equals` as range variables or named arguments cause CS0745 cascades; positional arguments and renames are the fix — an error wall in a LINQ file is a keyword collision, not a brace imbalance.

## [02]-[HOST_AND_NATIVE]

- Direct `RhinoApp.InvokeOnUiThread` is the bug — it silently swallows exceptions; the `RhinoUi` marshal wrapper is mandatory, never ceremony.
- Rhino API truths hold: obsolete members (`TextFormula`, `SetScale` with `UnitSystem`) are genuinely gone and their `LengthUnit`/rich-text replacements correct; `bool`-returning members where `false` is a benign no-op stay bare-bool, never lifted to `Fin.Fail`.
- Eto 2.11 macOS workarounds are law, not over-engineering: `using` on Eto brushes is inert, `MeasureString` leaks state (private `FormattedText` instead), `CreateGraphics` throws on Mac, and font-keyed caches add `FontDecoration` because `Font.Equals` omits it.
- Process-static plus `[ThreadStatic]` caches for expensive native measure/gradient state, and token-gated singleton custody (`Atom<Option<Guid>>` with take/release/withdraw) over native singletons, are required patterns — a naive bind/dispose subscription is the bug.
- `TimeProvider` injection (`TimeProvider.System` default) drives motion and clock state; demanding a fake-clock test double the estate deliberately omits is a false positive.
- `SafeHandle` + `[LibraryImport]` + `DangerousAddRef`/`DangerousRelease` + `unsafe` inside a capsule kernel is the named platform-forced exemption, marked `// Exemption:` — not unsafe-code smell.
- DuckDB.NET exposes no public zero-copy column span; per-chunk materialization into Arrow builders is the honest pattern — demanding zero-copy there is illusory.
- `Complex` lacks `IMultiplyOperators<Complex, double, Complex>`; separate real and complex method bodies are correct, not duplication to unify via generic math.
- Extern aliases (`Triangle` as `TriangleNet`, `SharpVoronoiLib` as `Voronoi`) are preventive collision fixes — never demand renames of the colliding domain types or blanket suppressions.
- CsCheck generators filter with `Gen.Where(predicate).Select(transform)`; a throwing `Select` breaks shrinking — flag the throw, never "simplify" the pair back into one.
- Admitted-package rulings are settled: QuikGraph stays admitted; MathNet, CSparse, Riok.Mapperly, and Generator.Equals are core substrate to compose at depth, never to wrap.
