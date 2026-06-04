# [CODE_DOCUMENTATION_STANDARDS]

Code documentation is source-level reference for public symbols. It records the semantic contract a signature cannot express: intent, caller obligations, effect and failure channels, invariants, lifecycle, resource use, and non-obvious rationale. The signature owns types, arity, nullability, and generic bounds; comments repeat none of that unless the language lacks the type information.

## [1][USE_WHEN]

Apply this standard when writing or reviewing source-level comments on:

- public visible types, members, functions, methods, modules, packages, and properties;
- public error, fault, result, validation, or effect variants and their observable channels;
- generic type parameters whose meaning or constraint is not evident from the bound;
- lifecycle, resource, concurrency, interop, or runtime-context obligations a caller must honor;
- inline rationale for a non-obvious implementation choice.

Skip comments that restate the signature, obvious accessors, private implementation details, or names the type already makes unambiguous. Generated reference pages route through [api.md](api.md); curated lookup facts outside source route through [reference.md](reference.md); owner-local README maps route through [readme.md](readme.md); prose mechanics inside comments route through [style-guide.md](../style-guide.md).

## [2][TOOLCHAIN_BASELINES]

The language toolchain owns comment syntax, tag validation, reference resolution, and generated output. This standard owns semantic completeness.

C# XML documentation
    Source of truth: [Microsoft C# documentation comments](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/documentation-comments).
    Last verified: 2026-06-04
    Review trigger: C# documentation-comment specification or repository warning policy changes.

.NET XML documentation tags
    Source of truth: [.NET recommended XML documentation tags](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/recommended-tags), including tags such as `<inheritdoc>`.
    Last verified: 2026-06-04
    Review trigger: Microsoft XML tag guidance or repository generator support changes.

TSDoc
    Source of truth: [TSDoc specification and tag reference](https://tsdoc.org/).
    Last verified: 2026-06-04
    Review trigger: TSDoc tag syntax, release modifiers, deprecation tags, or parser behavior changes.

Python docstrings
    Source of truth: `pyproject.toml` `[tool.ruff.lint.pydocstyle].convention = "google"`; [PEP 257](https://peps.python.org/pep-0257/) and [Google-style docstrings](https://google.github.io/styleguide/pyguide.html#381-docstrings) provide the external syntax baseline.
    Last verified: 2026-06-04
    Review trigger: `pyproject.toml` pydocstyle settings, Python docstring policy, or configured generator dialect changes.

Repository semantic overlay
    Source of truth: `CLAUDE.md`, language skills, `Directory.Build.props`, `pyproject.toml`, and this standard for `Fin<T>`, `Validation<Error,T>`, `Eff<RT,T>`, effects, validation, and typed failure-channel semantics.
    Review trigger: repo language policy, warning policy, docstring configuration, or result and effect carrier policy changes.

In Rasm C# builds, documentation warnings are build-failing unless explicitly suppressed: `Directory.Build.props` sets `TreatWarningsAsErrors` and `GenerateDocumentationFile`, suppresses `CS1591`, and does not suppress unresolved `cref` diagnostics such as `CS1574`. A dangling reference in any language is still a documentation defect because it breaks generated reference trust.

## [3][REQUIRED_STRUCTURE]

Choose the symbol profile during authoring and review; do not emit a profile label in the source comment unless the language toolchain owns that tag. Each profile answers one question: how does the caller handle every observable outcome?

| [INDEX] | [PROFILE]        | [APPLIES]                | [SEMANTICS]             | [FAILURE]              |
| :-----: | :--------------- | :----------------------- | :---------------------- | :--------------------- |
|   [1]   | Pure surface     | total functions, values  | purpose, obligations    | no failure channel     |
|   [2]   | Effect surface   | result, effect, status   | success, faults, runtime | no phantom throws     |
|   [3]   | Throwing surface | actual throws or interop | real throws             | only real thrown types |

A comment is complete when a caller can use the symbol correctly and handle every outcome from the signature plus comment, without reading the body.

Render the semantic contract through the toolchain's syntax. The field set is the same across languages; tags and section names differ.

- Summary or first sentence: required, single; states the controlling purpose in symbol-kind shape.
- Parameter entries: repeatable; include only meaning, unit, range, origin, or caller obligation the signature omits.
- Type-parameter entries: repeatable; include only generic meaning or relationship the bound omits.
- Return or yield entry: conditional; required when the symbol returns a value, effect, validation result, generator item, status, or failure channel whose semantics are not obvious from the type.
- Value or attribute entry: conditional; required for a public property or attribute whose value carries a caller constraint.
- Remarks or elaboration: optional; include only invariants, lifecycle, resource, concurrency, examples, or interop boundaries that do not fit the summary.
- Exception or throw entries: repeatable only for actual thrown types on a throwing surface.
- Cross-references: optional and repeatable; include only references the owning toolchain resolves.
- Inline comments: optional; state why a non-obvious implementation choice exists, never what the next line does.

A parameter entry that says only `A string`, a return entry that restates `Fin<T>` or another carrier name, or a summary that echoes the symbol name fails this cardinality model.

Folder-level code catalogs and "mini API" pages are not source comments. They aggregate current public surface for agents and maintainers, so they belong in the owner README, a generated API reference, or a reference leaf. When a folder catalog is useful, it may carry this record shape:

```text template
Public surface: `<entrypoint, type family, command family, or generated reference anchor>`
Owner paths: `<current paths that implement the surface>`
Failure carriers: `<typed result, fault, receipt, or status; omit when absent>`
Receipts: `<proof or receipt type a caller receives; omit when absent>`
Generated reference: `<api.md-owned generated page; omit when absent>`
Common misuse: `<one misuse the source comments do not prevent; omit when absent>`
Route-away: `<architecture, roadmap, how-to, tutorial, or support; omit when absent>`
Review trigger: public symbol, generated reference, owner path, or failure-carrier change.
```

The record is a handoff. Omit absent optional fields instead of filling `none`; keep an explicit `none` value only when absence itself changes reader behavior. It does not replace symbol comments, generated reference, or architecture codemaps, and it appears only where a maintained owner document consumes it.

## [4][LEAD_SHAPE]

Lead with the contract the symbol kind needs:

- Type or class: a noun phrase opening with `Represents` or `Provides` that names the modeled concept, not its fields.
- Method or function: an effect clause that names the call's observable action.
- Boolean return: `true` when the condition holds and `false` otherwise, using the language's comment syntax.
- Property or Python `@property`: an attribute-style noun phrase naming what the value is, never `Gets` or `Returns` when the property itself is the value.
- Module or package: a purpose sentence naming the surface the module provides and the boundary it does not cross.

For Python functions and methods, the repo-preferred form follows PEP 257's imperative shape where it reads naturally. For Python modules, classes, and properties, use the symbol-kind shape above so type and attribute documentation does not become command prose.

## [5][C_XML_COMMENTS]

Use XML documentation tags by semantic role. Cardinality is per symbol.

| [INDEX] | [TAG]                 | [MAPS_TO]              | [TOOLCHAIN_CAVEAT]                  |
| :-----: | :-------------------- | :--------------------- | :---------------------------------- |
|   [1]   | `<summary>`           | controlling purpose    | required for documented symbols     |
|   [2]   | `<param>`             | parameter meaning      | one per meaningful parameter entry  |
|   [3]   | `<typeparam>`         | generic relationship   | one per meaningful type parameter   |
|   [4]   | `<returns>`           | value, effect, failure | non-void surfaces only              |
|   [5]   | `<value>`             | property meaning       | property value contracts            |
|   [6]   | `<remarks>`           | invariant or lifecycle | optional elaboration                |
|   [7]   | `<exception>`         | actual thrown type     | throwing surfaces only              |
|   [8]   | `<see>` / `<seealso>` | resolvable reference   | compiler-checked `cref` where used  |
|   [9]   | `<inheritdoc>`        | inherited contract     | valid only when inherited text fits |

Use `<paramref>` for parameter references, `<c>` for inline symbols, and `<code>` for multiline examples. Use `<include>` only when the external XML file is maintained beside the source member.

```csharp conceptual
/// <summary>Resolves a host symbol to its loaded assembly version.</summary>
/// <param name="symbol">Fully qualified type or member name from the host catalog.</param>
/// <returns>
///   A <c>Fin&lt;Version&gt;</c> carrying the resolved version on success, or a
///   <c>SymbolNotFound</c> fault when the catalog has no matching entry. Does not
///   throw for a missing symbol.
/// </returns>
public static Fin<Version> ResolveVersion(string symbol) => /* ... */;
```

The example is conceptual: it shows the effect-surface channel mapping, not copyable production code. It documents the success value and fault variant in `<returns>` and does not advertise a phantom exception.

## [6][TYPESCRIPT_COMMENTS]

Use TSDoc for exported TypeScript APIs. The implicit summary is the text before the first block tag; `@remarks` opens detail. Block tags carry semantics only, not JSDoc type-expression braces that duplicate the TypeScript signature.

| [INDEX] | [TAG]                  | [MAPS_TO]              | [TOOLCHAIN_CAVEAT]                     |
| :-----: | :--------------------- | :--------------------- | :------------------------------------- |
|   [1]   | Summary section        | API purpose            | text before the first block tag        |
|   [2]   | `@remarks`             | invariant or example   | optional elaboration                   |
|   [3]   | `@typeParam`           | generic relationship   | use TSDoc syntax, not JSDoc type forms |
|   [4]   | `@param`               | parameter meaning      | omit type-expression braces            |
|   [5]   | `@returns`             | value, effect, fault   | non-void surfaces only                 |
|   [6]   | `@throws`              | actual thrown type     | throwing surfaces only                 |
|   [7]   | `{@link ...}` / `@see` | resolvable reference   | generator must resolve the target      |
|   [8]   | `@inheritDoc`          | inherited contract     | valid only when inherited text fits    |
|   [9]   | release modifiers      | maturity contract      | `@alpha`, `@beta`, `@public`, and peers |
|  [10]   | `@deprecated`          | deprecation contract   | block tag; name replacement guidance   |

Where the project gates API maturity, release modifiers such as `@alpha`, `@beta`, and `@public` are contract content. A deprecated exported surface uses `@deprecated` with replacement guidance; it is not a release-stage modifier.

```typescript conceptual
/**
 * Runs the checked plan against the active target.
 *
 * @param plan - Reviewed plan whose target matches the active workspace.
 * @returns Success with an execution receipt, or `TargetMismatch` when the plan
 * target does not match the active workspace. Does not throw for validation
 * failure.
 */
export declare const applyPlan: (
  plan: Plan,
) => Effect.Effect<Receipt, TargetMismatch>;
```

The example is conceptual: it shows a TypeScript effect surface documenting the success receipt and typed failure instead of restating `Effect` generics or adding a phantom `@throws`.

## [7][PYTHON_DOCSTRINGS]

Use PEP 257 triple-double-quoted docstrings on public modules, packages, classes, functions, methods, and properties. Rasm's Ruff configuration selects Google-style structured sections, so keep section names and ordering consistent with that dialect until `pyproject.toml` changes.

Python cardinality:

- Summary line: required; function and method summaries may use imperative form, while module, class, and property summaries use the symbol-kind shape.
- Blank line after summary: required only for multiline docstrings.
- `Args:`: conditional; include only parameter meaning, units, ranges, or protocol obligations the annotation omits.
- `Returns:`: conditional; include return meaning, side effect, or typed result channel; omit when the function returns `None` or the summary fully states the result.
- `Yields:`: conditional for generators.
- `Raises:`: repeatable only for exception types that are part of the interface.
- `Attributes:`: conditional for public attribute meaning; `@property` docstrings use attribute-style wording.
- `Examples:`: optional; include only where call shape is non-obvious.

Do not document an exception raised only when the caller violates the documented API contract; that turns violation behavior into the public contract.

## [8][EFFECT_FAILURE_CHANNELS]

For repo effect, `Fin<T>`, `Validation<Error,T>`, `Eff<RT,T>`, validation, and status carriers, document observable channels instead of collapsing the carrier into a bare value. This is a repository semantic overlay, not a claim that C#, TSDoc, or PEP 257 requires the model.

Every effect surface states:

1. The success value and observable side effect, when present.
2. Every failure variant, fault, or accumulated-validation meaning a caller can receive.
3. The cancellation, retry, resource, clock, IO, or runtime-context requirement the call imposes.

When the surface has the shape, also state:

- the interop boundary where native exceptions convert into typed failures;
- the terminal point where deferred effects execute or collapse.

An effect-surface comment that says only `returns Fin<T>` or names another typed carrier without naming failure variants is incomplete. A comment that omits interop or deferral details fails only when the surface actually crosses an interop boundary or defers execution.

## [9][CROSS_REFERENCES_INLINE]

Resolve every code reference through the toolchain that owns it, or omit the reference.

- C# references use `cref` where compiler documentation validation can check them.
- TypeScript references use TSDoc `{@link ...}` or `@see`.
- Python references use the configured documentation generator's resolvable syntax.

Reserve inline comments for the reason a non-obvious choice exists:

```csharp conceptual
// Host catalog lists members lazily, so resolve eagerly here to surface a
// missing symbol at registration rather than at first call.
RegisterEager(symbol);
```

Reject narration:

```csharp rejected
// Register the symbol.
RegisterEager(symbol);
```

## [10][ANTI_PATTERNS]

Reject these shapes:

- Type-restating parameter: a `<param>`, `@param`, or `Args:` entry that echoes the declared type. Replace it with unit, range, origin, or obligation, or delete it.
- Phantom exception: an `<exception>`, `@throws`, or `Raises:` entry for a type the symbol never throws. Remove it.
- Annotation-echoing return: a `<returns>`, `@returns`, or `Returns:` entry that restates the annotated return type. Replace it with success, effect, or failure semantics, or remove it.
- Name-echo summary: a summary that paraphrases the symbol name. Rewrite it to the symbol-kind lead shape.
- Profile label leakage: a review-only profile label emitted into a source comment without a toolchain-owned tag. Remove the label and keep the semantics.
- Line-narrating inline comment: an inline comment that restates the next statement. Delete it.

## [11][BOUNDARIES]

- [api.md](api.md) owns generated and contract-backed API reference, including generated mirrors of source comments.
- [reference.md](reference.md) owns curated lookup facts that live outside source.
- [readme.md](readme.md) owns owner-local entry maps that point readers to public surfaces without cataloging every symbol.
- [architecture.md](../explanation/architecture.md) owns current owner blocks, invariants, and codemaps; code comments do not carry folder architecture.
- [style-guide.md](../style-guide.md) owns prose mechanics inside comments.
- [proof.md](../proof.md) owns proof that source comments match source behavior and generated reference output; source comments carry no freshness fields unless a language-specific generator consumes them.
- [README.md](../README.md) routes document-type, placement, and lifecycle questions.

## [12][REVIEW_CHECKLIST]

- [ ] The reviewer chose one symbol profile during review, without leaking review labels into source comments.
- [ ] The comment carries the required semantic fields for that profile.
- [ ] No comment restates declared type, return type, nullability, or arity.
- [ ] The lead sentence matches the symbol kind and carries contract, not name echo.
- [ ] Effect surfaces name success, every failure variant, and runtime-context requirements.
- [ ] Throwing surfaces list only actual thrown types with their causes.
- [ ] Python, TypeScript, and C# comments use the syntax and tags their toolchains parse.
- [ ] C# reference-resolution language reflects Rasm's warning-as-error and documentation-generation settings.
- [ ] Cross-references resolve through the owning toolchain.
- [ ] Inline comments state a reason, not narration.
- [ ] Folder-level public-surface catalogs route to README, generated API reference, architecture, or reference leaves instead of leaking into source comments.
- [ ] No anti-pattern remains.
