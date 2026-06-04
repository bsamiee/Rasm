# [CODE_DOCUMENTATION_STANDARDS]

Document every public symbol with the semantic contract its name and type cannot express: intent, caller obligations, effect and failure channels, invariants, and lifecycle. The signature owns the types; the comment owns everything a caller must know that the types do not state. This standard decides what semantic information a source-level comment must carry; the language toolchain decides the comment syntax that emits it.

The governing invariant is identical across C#, TypeScript, and Python: the signature owns types, arity, nullability, and generic bounds, and the comment owns only the semantics the signature cannot state. A comment that repeats a declared parameter type, return type, or nullability is duplication, not documentation, and must be deleted.

## [1][USE_WHEN]

Apply this standard when writing or reviewing a source-level comment on a public symbol. Route by what the symbol exposes:

- public visible types, members, functions, and methods;
- public error, fault, or result variants and the effect channels they travel;
- non-obvious invariants, and lifecycle, resource, or concurrency obligations a caller must honor;
- generic type parameters whose meaning or constraint is not evident from the bound;
- inline rationale for a non-obvious implementation choice.

Skip a comment that only restates the signature, an obvious accessor, a private implementation detail, or a name the type already makes unambiguous. For a generated contract or an external vendor API surface, route to the API topic; for curated lookup facts that live outside source, route to the reference topic; for prose mechanics inside a comment, route to the style topic.

## [2][REQUIRED_STRUCTURE]

Every code-documentation comment author works from the profile discriminator down to the per-toolchain tag table. The standard itself follows this section template; copy it when authoring a derived per-language convention and preserve the cardinality of each section.

```markdown template
# [CODE_DOCUMENTATION_STANDARDS]

## [1][USE_WHEN_REQUIRED]

## [2][REQUIRED_STRUCTURE_REQUIRED]

## [3][DOCUMENTATION_PROFILES_REQUIRED]

## [4][LEAD_SENTENCE_SHAPE]

## [5][TOOLCHAIN_OWNERSHIP_REQUIRED]

## [6][C_XML_COMMENTS]

## [7][TYPESCRIPT_COMMENTS_CONDITIONAL]

## [8][PYTHON_DOCSTRINGS_CONDITIONAL]

## [9][EFFECT_FAILURE_CHANNELS]

## [10][CROSS_REFERENCES_INLINE]

## [11][ANTI_PATTERNS_REQUIRED]

## [12][BOUNDARIES_REQUIRED_ONCE]

## [13][REVIEW_CHECKLIST_REQUIRED]

```

A comment is complete only when it satisfies its profile's mandatory-field set, leads with the per-kind sentence shape, carries the toolchain tags at their stated cardinality, and trips none of the anti-patterns. Treat each acceptance criterion in this standard as an executable reject test, not advice.

## [3][DOCUMENTATION_PROFILES]

A public symbol falls into one of three documentation profiles by what it exposes. Each profile sets which semantic fields are mandatory; the language toolchain sets the syntax. Declare the profile first, then emit the required fields through the toolchain's tags. Keep profile facts together and use compact field names rather than splitting the chooser into records.

| [INDEX] | [PROFILE]        | [APPLIES]                | [FIELDS]                         | [CHANNEL_RULE]     |
| :-----: | :--------------- | :----------------------- | :------------------------------- | :----------------- |
|   [1]   | Pure surface     | total functions, values  | purpose, params, return          | no failure channel |
|   [2]   | Effect surface   | result/effect/status API | purpose, params, success, faults | no phantom throws  |
|   [3]   | Throwing surface | actual throws or interop | purpose, params, return, throws  | only real throws   |

Acceptance criterion for the profile choice: a reader who knows the language but not the codebase must be able to call the symbol correctly and handle every outcome from the signature plus the comment alone, without reading the body.

## [4][LEAD_SENTENCE_SHAPE]

Lead every comment with a controlling sentence shaped by the symbol kind, because `Purpose in one controlling sentence` is satisfiable with a name echo otherwise. The shape carries contract, not filler:

- Type: a noun phrase opening with `Represents` or `Provides` that names the concept the type models, not its fields.
- Method or function: a present-tense effect clause that names what the call does — `Resolves the host symbol`, `Cancels the pending render`.
- Boolean return: `<c>true</c> if <condition>; otherwise, <c>false</c>.` in C#, and the equivalent `Returns true when <condition>` phrasing in TypeScript and Python.
- Property and Python `@property`: an attribute-style noun phrase naming what the value is, never `Returns the …` or `Gets the …`.

Acceptance criterion: a summary that paraphrases the symbol name and adds no intent, obligation, or constraint fails this shape and must be rewritten to carry contract. `Gets the value` on a property named `Value` is a reject; `The resolved assembly version, or the host default when the catalog is empty` passes.

## [5][TOOLCHAIN_OWNERSHIP]

The language toolchain owns comment syntax, tag validation, reference resolution, and generated output. Use the format the toolchain parses, and let it own the mechanics this standard does not:

```text conceptual
public symbol
  ├─ signature  ─────────────  owns types, arity, nullability, generics bounds
  └─ doc comment
       ├─ toolchain syntax ──  tags, cref/link resolution, generated XML or HTML
       └─ this standard ─────  semantic contract: intent, obligations, channels
```

- C#: XML documentation comments own tag names, compiler validation, `cref` resolution, and generated XML output.
- TypeScript: TSDoc owns exported-API comment syntax, tags, links, and parser-compatible Markdown.
- Python: PEP 257 owns docstring placement, and Google-style sections apply only when the project already uses that dialect.

State a type's full shape in the signature, never in the comment. A comment that repeats a parameter type, a return type, or a nullability the signature already declares is duplication, not documentation.

## [6][C_XML_COMMENTS]

Use these tags by their semantic role. Cardinality is per documented symbol: `required` appears once when the role is present, `optional` appears zero or one time, `repeatable` appears once per item it describes. The profile column cross-binds the tag to the profile chosen above, so the profile and the tag table are not read independently.

| [INDEX] | [TAG]                 | [CARRIES]              | [CARDINALITY]        | [PROFILE] |
| :-----: | :-------------------- | :--------------------- | :------------------- | :-------- |
|   [1]   | `<summary>`           | controlling purpose    | required             | all       |
|   [2]   | `<param>`             | meaning, unit, duty    | repeat per param     | all       |
|   [3]   | `<typeparam>`         | generic meaning        | repeat per generic   | all       |
|   [4]   | `<returns>`           | value/effect/failure   | required when useful | non-void  |
|   [5]   | `<value>`             | property meaning       | optional             | pure      |
|   [6]   | `<remarks>`           | invariant or lifecycle | optional             | any       |
|   [7]   | `<exception>`         | actual thrown type     | repeat per throw     | throwing  |
|   [8]   | `<see>` / `<seealso>` | resolvable reference   | repeatable           | any       |
|   [9]   | `<inheritdoc>`        | inherited contract     | optional replacement | any       |

Use `<paramref>` for a parameter reference, `<c>` for an inline symbol, and `<code>` for a multiline example. Use `<include>` only when the external XML file is maintained alongside the source member. A `<param name>` that does not match a signature parameter and an `<exception cref>` that does not resolve both raise a compiler warning, so these references are checked, not free prose.

```csharp conceptual
/// <summary>Resolves a host symbol to its loaded assembly version.</summary>
/// <param name="symbol">Fully qualified type or member name from the host catalog.</param>
/// <returns>
///   <see cref="Result{T}"/> carrying the resolved version on success, or a
///   <c>SymbolNotFound</c> fault when the catalog has no matching entry. Never throws.
/// </returns>
public static Result<Version> ResolveVersion(string symbol) => /* ... */;
```

The block above is `conceptual`: it shows the tag-to-channel mapping, not a copyable member. An effect surface like this one documents the success value and the fault variant in `<returns>` and states that it does not throw, so no `<exception>` tag appears.

```csharp rejected
/// <param name="symbol">A string.</param>
public static Result<Version> ResolveVersion(string symbol) => /* ... */;
```

```csharp conceptual
/// <param name="symbol">Fully qualified type or member name from the host catalog.</param>
public static Result<Version> ResolveVersion(string symbol) => /* ... */;
```

The first `<param>` restates the declared `string` type and is a reject; the second carries the origin and shape the type cannot state. Acceptance criterion: every `<param>` adds a unit, range, origin, or obligation the signature omits, or it is deleted.

## [7][TYPESCRIPT_COMMENTS]

Use TSDoc on every exported API. Map each tag to its semantic role and respect its cardinality. The implicit summary section is the text before the first block tag; `@remarks` ends the summary and opens detail. The `@param`, `@returns`, and `@typeParam` block tags carry no JSDoc-style type braces, because the TypeScript signature already owns the type.

| [INDEX] | [TAG]                               | [CARRIES]            | [CARDINALITY]        | [PROFILE]      |
| :-----: | :---------------------------------- | :------------------- | :------------------- | :------------- |
|   [1]   | Summary section                     | API purpose          | required             | all            |
|   [2]   | `@remarks`                          | invariant or example | optional             | any            |
|   [3]   | `@typeParam`                        | generic meaning      | repeatable           | all            |
|   [4]   | `@param`                            | meaning, unit, duty  | repeatable           | all            |
|   [5]   | `@returns`                          | value/effect/failure | required when useful | non-void       |
|   [6]   | `@throws`                           | actual thrown type   | repeatable           | throwing       |
|   [7]   | `{@link ...}` / `@see`              | resolvable reference | repeatable           | any            |
|   [8]   | `@inheritDoc`                       | inherited contract   | optional             | any            |
|   [9]   | `@deprecated` and release modifiers | release/deprecation  | optional per stage   | gated surfaces |

Where the project gates API maturity, the release modifier is contract content: an unstable surface documented without `@beta` or `@alpha` ships as if stable. Let the signature own the types. Do not use JSDoc type-expression syntax to restate a TypeScript type the signature already declares; TSDoc carries only the semantics the signature cannot.

Acceptance criterion: a `@param` block that restates the declared TypeScript type fails and must be replaced by the unit, range, or obligation the type omits, or removed.

## [8][PYTHON_DOCSTRINGS]

Use a PEP 257 triple-double-quoted docstring on every public module, package, class, function, and method. Lead with one summary line that states the contract as an imperative phrase ending in a period — `Resolve the host symbol`, not `Resolves the host symbol` or `Function that resolves`. For a multi-line docstring, follow the summary with a blank line, then the elaboration, and close the quotes on their own line. Add structured sections only when the summary cannot carry the full contract.

Use Google-style sections when a public object needs structured detail, in this order, one section per role:

- `Examples:` — runnable usage when the call shape is non-obvious.
- `Args:` — parameter meaning, units, ranges, or shape the annotation omits; omit `self`, list `*args` and `**kwargs` by name, and include a type only where no annotation exists.
- `Returns:` — return meaning, side effect, or typed result channel; omit it when the function returns `None` or when the imperative summary already states the return sufficiently. Document a tuple return as a single named tuple, never as faux multiple returns.
- `Yields:` — the per-item object a generator yields from `next()`.
- `Raises:` — an exception type relevant to the interface, with its cause. Do not document an exception raised only when the documented API is itself violated, because that would make violation behavior part of the contract.
- `Attributes:` — public attribute meaning not evident from the name; a `@property` docstring uses this attribute style, not `Returns the …`.

Acceptance criterion: a section earns its place only when it carries information the type annotation cannot, such as units, ranges, shape constraints, or protocol obligations. A `Returns:` section that restates the annotated return type fails this criterion and must be removed.

## [9][EFFECT_FAILURE_CHANNELS]

Document an effect or result carrier by its observable channels, and never collapse the carrier into a bare value. Every effect surface states items 1-3 unconditionally; items 4-5 are conditional and apply only when the surface has that shape. State them in order:

1. The success value and its observable side effect, when present.
2. Every failure variant, fault, or accumulated-validation meaning a caller can receive.
3. The cancellation, retry, resource, clock, IO, or runtime-context requirement the call imposes.
4. When the surface crosses an interop boundary, the boundary where native exceptions convert into typed failures.
5. When the surface defers execution, the terminal point where effects execute or collapse.

These three mandatory items match the Effect-surface row of the Documentation profiles table and the effect-surface row of the Review checklist; the two conditional items add no obligation when the surface neither crosses an interop boundary nor defers execution.

A worked effect surface that exercises all five items — an interop boundary that returns a deferred, cancellable effect:

```csharp conceptual
/// <summary>Streams host catalog entries that match a glob, deferring the host query until enumerated.</summary>
/// <param name="pattern">Catalog glob; matched case-insensitively against fully qualified names.</param>
/// <param name="ct">Cancellation token observed between yielded entries; cancellation surfaces as <c>OperationCanceled</c>.</param>
/// <returns>
///   (1) A lazy <see cref="IAsyncEnumerable{T}"/> of matched entries; iterating advances the host cursor.
///   (2) Yields a <c>CatalogUnavailable</c> fault entry when the host catalog is offline, or an
///       <c>OperationCanceled</c> fault when <paramref name="ct"/> trips.
///   (3) Requires a live host context and observes <paramref name="ct"/> on every step.
///   (4) The native COM <c>HRESULT</c> the host raises is converted to a <c>CatalogUnavailable</c>
///       fault at this boundary; no native exception escapes.
///   (5) The query is deferred: nothing executes until enumeration begins, and the host cursor
///       collapses when the enumerator is disposed.
/// </returns>
public static IAsyncEnumerable<Result<CatalogEntry>> StreamMatches(string pattern, CancellationToken ct) => /* ... */;
```

The block is `conceptual`: it maps each numbered item to a channel, not a copyable member. A pure in-process effect surface that neither crosses interop nor defers documents only items 1-3 and omits 4-5 entirely.

Document a thrown exception only on a throwing surface that actually throws it. An effect surface that returns its failures must not advertise a throw it never performs; mixing the two channels misleads the caller about how to handle every outcome.

Acceptance criterion: an effect-surface comment that states `returns a Result<T>` without enumerating its failure variants fails coverage item 2 and is incomplete; a comment that omits items 4-5 fails only when the surface actually crosses an interop boundary or defers execution.

## [10][CROSS_REFERENCES_INLINE]

Resolve every code reference through the toolchain that owns it, or omit the reference. A dangling reference is worse than none, because it fails generation and erodes trust in the surrounding contract.

- C# code references use `cref`, which the compiler verifies; a non-resolving `cref` fails the build.
- TypeScript code references use TSDoc `{@link ...}` or `@see`.
- Python references use the configured documentation generator's syntax, and only when the generated docs resolve it.

Reserve an inline comment for the one thing the code cannot say: why a non-obvious choice exists. An inline comment must not narrate the next line, preserve commented-out code, duplicate a name, or carry release history.

```csharp conceptual
// Accepted: states the non-obvious reason.
// Host catalog lists members lazily, so resolve eagerly here to surface a missing
// symbol at registration rather than at first call.
RegisterEager(symbol);
```

```csharp rejected
// Rejected: narrates the line and adds no reason a reader lacks.
// Register the symbol.
RegisterEager(symbol);
```

The first block is the accepted comment shape and earns its place; the second is `rejected` and must be deleted, because the symbol name already says what the line does.

## [11][ANTI_PATTERNS]

Reject these named low-value shapes; each carries a paired transformation from the rejected shape to the accepted one. They are the highest-frequency failures in source comments, and a comment that ships any of them fails review.

- Type-restating parameter — a `<param>`, `@param`, or `Args:` entry that echoes the declared type (`A string`, `An int`). Replace it with the unit, range, origin, or obligation the type omits, or delete it.
- Phantom exception — an `<exception>`, `@throws`, or `Raises:` entry for a type the symbol never throws, or for an exception raised only when the API contract is violated. Remove it; document a throw only where the symbol actually throws.
- Annotation-echoing return — a `<returns>` or `Returns:` that restates the annotated return type with no added meaning. Remove it, or replace it with the success value and side effect the annotation cannot state.
- Name-echo summary — a summary that paraphrases the symbol name (`Gets the value` on `GetValue`). Rewrite it to the per-kind lead-sentence shape so it carries intent.
- Line-narrating inline comment — an inline comment that restates the next statement. Delete it; reserve inline comments for the reason a non-obvious choice exists.

Acceptance criterion: each shipped comment satisfies its profile and matches no rejected anti-pattern in this catalog.

## [12][BOUNDARIES]

- [api.md](api.md) owns generated and contract-backed API reference, including the generated mirror of these same symbols, marked `generated` with its generation command; this standard owns the source comment that the generator consumes.
- [reference.md](reference.md) owns curated lookup facts that live outside source.
- [style-guide.md](../style-guide.md) owns prose mechanics inside a comment.
- [proof.md](../proof.md) owns the proof obligation that a source comment matches both source behavior and generated reference output; source comments carry no freshness fields, and the generated mirror carries the freshness metadata.
- [README.md](../README.md) routes document-type, placement, and lifecycle questions, including when a symbol's contract belongs in generated reference instead of a comment.

## [13][REVIEW_CHECKLIST]

- [ ] Profile is declared, and the symbol carries that profile's mandatory fields.
- [ ] Pure-surface symbol carries purpose, non-type parameter obligations, and return meaning.
- [ ] Effect-surface symbol carries the pure fields plus every failure variant and the runtime-context need.
- [ ] Throwing-surface symbol carries the pure fields plus each thrown type with its cause, and no phantom throws.
- [ ] No comment restates a declared type, return type, or nullability.
- [ ] The lead sentence uses the per-kind shape, not a name echo.
- [ ] Required tags or sections are present at their cardinality; optional ones earn their place.
- [ ] Cross-references resolve through the owning toolchain.
- [ ] Inline comments state a reason, not a narration or a dead name.
- [ ] No comment trips an entry in the anti-pattern catalog.
- [ ] A caller can use each symbol correctly from signature plus comment, without reading the body.
