# [CODE_DOCUMENTATION_STANDARDS]

Code documentation is source-level reference for public symbols. It records the semantic contract a signature cannot express: intent, caller obligations, effect and failure channels, invariants, lifecycle, resource use, and non-obvious rationale. The signature carries types, arity, nullability, and generic bounds; comments repeat none of that unless the language lacks the type information.

## [1][USE_WHEN]

Apply this standard when writing or reviewing source-level comments on:
- public visible types, members, functions, methods, modules, packages, and properties.
- public error, fault, result, validation, or effect variants and their observable channels.
- generic type parameters whose meaning or constraint is not evident from the bound.
- lifecycle, resource, concurrency, interop, or runtime-context obligations a caller must honor.
- inline rationale for a non-obvious implementation choice.

Skip comments that restate the signature, obvious accessors, private implementation details, or names the type already makes unambiguous. Generated reference pages route through [api.md](api.md); curated lookup facts outside source route through [reference.md](reference.md); scope-local README maps route through [readme.md](readme.md); prose mechanics inside comments route through [style-guide.md](../style-guide.md).

[AUTHORING_CONTRACT]:
- Agent use: decide whether a public symbol needs source documentation and whether that documentation changes generated reference, README, architecture, support, or task docs.
- Required produced structure: symbol profile, semantic comment shape, generated-reference handoff, maintenance triggers, language tag rules, boundaries, and checklist.
- Section cardinality: one public-symbol decision per comment, one generated-reference handoff only when adjacent output changes, and language-specific rules only where the source language applies.
- Adjacent checks: API docs for generated reference, README for public entrypoints, reference for lookup facts, architecture for invariants, support matrix for lifecycle, how-to/tutorial for usage paths.
- Maintenance triggers: public symbol added, removed, renamed, retyped, made visible, given a new failure carrier, changed side effect, changed generated-reference anchor, or changed caller-visible invariant.
- Stale prevention: comments document semantics the signature cannot express; generated catalogs are regenerated or linked, not rebuilt by hand.

## [2][REQUIRED_STRUCTURE]

Choose the symbol profile during authoring and review; do not emit a profile label in the source comment unless the language toolchain carries that tag. Each profile answers one question: how does the caller handle every observable outcome?

| [INDEX] | [PROFILE]        | [APPLIES]                | [SEMANTICS]              | [FAILURE]              |
| :-----: | :--------------- | :----------------------- | :----------------------- | :--------------------- |
|   [1]   | Pure surface     | total functions, values  | purpose, obligations     | no failure channel     |
|   [2]   | Effect surface   | result, effect, status   | success, faults, runtime | no phantom throws      |
|   [3]   | Throwing surface | actual throws or interop | real throws              | only real thrown types |

A comment is complete when a caller can use the symbol correctly and handle every outcome from the signature plus comment, without reading the body.

Render the semantic contract through the toolchain's syntax. The field set is the same across languages; tags and section names differ.

- Summary or first sentence: required, single; states the controlling purpose in symbol-kind shape.
- Parameter entries: repeatable; include only meaning, unit, range, origin, or caller obligation the signature omits.
- Type-parameter entries: repeatable; include only generic meaning or relationship the bound omits.
- Return or yield entry: conditional; required when the symbol returns a value, effect, validation result, generator item, status, or failure channel whose semantics are not obvious from the type.
- Side-effect entry: conditional; required when the symbol mutates state, writes artifacts, observes time or IO, allocates or disposes resources, starts work, or changes external state.
- Cancellation/resource entry: conditional; required when a token, deadline, async iterator, runtime context, disposable, or retry policy changes caller obligations.
- Value or attribute entry: conditional; required for a public property or attribute whose value carries a caller constraint.
- Remarks or elaboration: optional; include only invariants, lifecycle, resource, concurrency, examples, or interop boundaries that do not fit the summary.
- Exception or throw entries: repeatable only for actual thrown types on a throwing surface.
- Cross-references: optional and repeatable; include only references the controlling toolchain resolves.
- Inline comments: optional; state why a non-obvious implementation choice exists, never what the next line does.

A parameter entry that says only `A string`, a return entry that restates `Fin<T>` or another carrier name, or a summary that echoes the symbol name fails this cardinality model.

Folder-level code catalogs and "mini API" pages are not source comments. They aggregate current public surface for agents and maintenance routes, so they belong in the scope README, a generated API reference, or a reference leaf. This standard defines only the source-comment side of that handoff: source comments feed generated reference, and the source document consumes a compact route record without becoming a second symbol catalog.

```text template
Changed fact: `<public symbol behavior, source-comment contract, generated anchor, failure carrier, or public surface>`
Consumed by: `<README, generated API reference, reference leaf, architecture codemap, support matrix, or task doc>`
Use in this document: `<caller behavior, generated reference text, route, invariant, failure handling, or safe edit>`
Update when: `<public symbol, source comment, generated output, failure carrier, or adjacent route changes>`
Close when: `<generated reference and consuming route are updated, or route-away explicitly declines the fact>`
Route-away: `<README, api, reference, architecture, roadmap, how-to, tutorial, or support body that stays in its route>`
Public surface: `<entrypoint, type family, command family, or generated reference anchor>`
Source paths: `<current paths that implement the surface>`
Generated reference: `<api.md-local output path or anchor; omit when absent>`
Generation command: `<exact command or API-page generation record; omit when the API page carries it>`
Failure carriers: `<typed result, fault, receipt, or status; omit when absent>`
```

The record is a handoff. Omit absent optional fields instead of filling `none`; keep an explicit `none` value only when absence itself changes reader behavior. It does not replace symbol comments, generated reference, or architecture codemaps, and it appears only where a maintained source document consumes it. When an API page already carries `Generation`, this handoff links the generated anchor and avoids duplicating proof fields.

When a public symbol comment changes contract behavior, regenerate or compare the generated API reference first. Then update the scope README entrypoint, curated reference facts, architecture codemap, support matrix, how-to, or tutorial only when the changed public surface, invariant, lifecycle, failure carrier, side effect, cancellation, or usage path changes that route's reader action. A comment-only clarity fix that preserves behavior does not force adjacent document churn.

## [3][LEAD_SHAPE]

Lead with the contract the symbol kind needs:
- Type or class: a noun phrase opening with `Represents` or `Provides` that names the modeled concept, not its fields.
- Method or function: an effect clause that names the call's observable action.
- Boolean return: `true` when the condition holds and `false` otherwise, using the language's comment syntax.
- Property or Python `@property`: an attribute-style noun phrase naming what the value is, never `Gets` or `Returns` when the property itself is the value.
- Module or package: a purpose sentence naming the surface the module provides and the boundary it does not cross.

For Python functions and methods, the repo-preferred form follows PEP 257's imperative shape where it reads naturally. For Python modules, classes, and properties, use the symbol-kind shape above so type and attribute documentation does not become command prose.

## [4][EFFECT_FAILURE_CHANNELS]

For repo effect, `Fin<T>`, `Validation<Error,T>`, `Eff<RT,T>`, validation, and status carriers, document observable channels instead of collapsing the carrier into a bare value. This is a repository semantic overlay, not a claim that C#, TSDoc, or PEP 257 requires the model.

Every effect surface states:
1. The success value and observable side effect, when present.
2. Every failure variant, fault, or accumulated-validation meaning a caller can receive.
3. The cancellation, retry, resource, clock, IO, or runtime-context requirement the call imposes.

When the surface has the shape, also state:
- the interop boundary where native exceptions convert into typed failures.
- the terminal point where deferred effects execute or collapse.

An effect-surface comment that says only `returns Fin<T>` or names another typed carrier without naming failure variants is incomplete. A comment that omits interop or deferral details fails only when the surface actually crosses an interop boundary or defers execution.

## [5][TOOLCHAIN_BASELINES]

The language toolchain carries comment syntax, tag validation, reference resolution, and generated output. This standard carries semantic completeness.

Use the C# documentation-comment specification and the .NET recommended XML documentation tags for XML comment syntax, including tags such as `<inheritdoc>`. Use the TSDoc specification and tag reference for exported TypeScript API comments. Use `pyproject.toml` `[tool.ruff.lint.pydocstyle].convention = "google"` with PEP 257 and Google-style docstring guidance for Python docstrings.

Repository semantic overlays come from `CLAUDE.md`, language skills, `Directory.Build.props`, `pyproject.toml`, and this standard for `Fin<T>`, `Validation<Error,T>`, `Eff<RT,T>`, effects, validation, and typed failure-channel semantics. Apply [proof.md](../proof.md) when a source-comment claim needs claim-level proof handling.

In Rasm C# builds, documentation warnings are build-failing unless explicitly suppressed: `Directory.Build.props` sets `TreatWarningsAsErrors` and `GenerateDocumentationFile`, suppresses `CS1591`, and does not suppress unresolved `cref` diagnostics such as `CS1574`. A dangling reference in any language is still a documentation defect because it breaks generated reference trust.

## [6][C_XML_COMMENTS]

Use XML documentation tags by semantic route. Cardinality is per symbol.

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
/// <summary>Runs one validated geometry operation and returns every emitted value.</summary>
/// <typeparam name="TGeometry">Geometry type accepted by the operation.</typeparam>
/// <typeparam name="TOut">Value type emitted by supported operation runs.</typeparam>
/// <param name="geometry">Geometry instance already admitted by the caller's boundary checks.</param>
/// <returns>
///   A successful sequence of emitted values, or a failure whose fault explains
///   the geometry/output mismatch.
/// </returns>
public static Fin<Seq<TOut>> Run<TGeometry, TOut>(TGeometry geometry)
    where TGeometry : notnull;
```

The example is conceptual: it shows the effect-surface channel mapping, not copyable production code. It documents a real method return channel in `<returns>` and does not advertise a phantom exception.

## [7][TYPESCRIPT_COMMENTS]

Use TSDoc for exported TypeScript APIs. The implicit summary is the text before the first block tag; `@remarks` opens detail. Block tags carry semantics only, not JSDoc type-expression braces that duplicate the TypeScript signature.

| [INDEX] | [TAG]                  | [MAPS_TO]            | [TOOLCHAIN_CAVEAT]                      |
| :-----: | :--------------------- | :------------------- | :-------------------------------------- |
|   [1]   | Summary section        | API purpose          | text before the first block tag         |
|   [2]   | `@remarks`             | invariant or example | optional elaboration                    |
|   [3]   | `@typeParam`           | generic relationship | use TSDoc syntax, not JSDoc type forms  |
|   [4]   | `@param`               | parameter meaning    | omit type-expression braces             |
|   [5]   | `@returns`             | value, effect, fault | non-void surfaces only                  |
|   [6]   | `@throws`              | actual thrown type   | throwing surfaces only                  |
|   [7]   | `{@link ...}` / `@see` | resolvable reference | generator must resolve the target       |
|   [8]   | `@inheritDoc`          | inherited contract   | valid only when inherited text fits     |
|   [9]   | release modifiers      | maturity contract    | `@alpha`, `@beta`, `@public`, and peers |
|  [10]   | `@deprecated`          | deprecation contract | block tag; name replacement guidance    |

Where the project gates API maturity, release modifiers such as `@alpha`, `@beta`, and `@public` are contract content. A deprecated exported surface uses `@deprecated` with replacement guidance; it is not a release-stage modifier.

For `Promise`-returning APIs, `@returns` documents the resolved value and any returned status or failure envelope. Use `@throws` only for synchronous throws or documented promise rejection. If cancellation flows through `AbortSignal`, an Effect runtime, or another token, document the caller obligation in `@param` or `@remarks` and do not convert returned status values into thrown exceptions.

```typescript conceptual
/**
 * Queries generated host metadata for a Rhino or package symbol.
 *
 * @param key - Source key resolved by the API rail, such as `rhino-common` or `gh2`.
 * @param symbol - Namespace, type, member, or search text interpreted by the query engine.
 * @returns An Envelope whose `data` carries the query shape, signature, source,
 * counts, artifact paths, and preview records; `status: empty` means the source
 * resolved but no symbol or artifact matched.
 */
export declare const queryApiSurface: (
  key: string,
  symbol: string,
) => Promise<QualityEnvelope>;
```

The example is conceptual over the repo-real `tools.quality api query` contract. It documents output semantics rather than restating `Promise<QualityEnvelope>` or inventing a thrown exception.

## [8][PYTHON_DOCSTRINGS]

Use PEP 257 triple-double-quoted docstrings on public modules, packages, classes, functions, methods, and properties. Rasm's Ruff configuration selects Google-style structured sections, so keep section names and ordering consistent with that dialect until `pyproject.toml` changes.

[PYTHON_CARDINALITY]:
- Summary line: required; function and method summaries may use imperative form, while module, class, and property summaries use the symbol-kind shape.
- Blank line after summary: required only for multiline docstrings.
- `Args:`: conditional; include only parameter meaning, units, ranges, or protocol obligations the annotation omits.
- `Returns:`: conditional; include return meaning, side effect, or typed result channel; omit when the function returns `None` or the summary fully states the result.
- `Yields:`: conditional for generators.
- `Raises:`: repeatable only for exception types that are part of the interface.
- `Attributes:`: conditional for public attribute meaning; `@property` docstrings use attribute-style wording.
- `Examples:`: optional; include only where call shape is non-obvious.

Do not document an exception raised only when the caller violates the documented API contract; that turns violation behavior into the public contract.

```python conceptual
def query_api_surface(key: str, symbol: str) -> Result[ApiQuery, ApiFault]:
    """Query generated host or package metadata for one symbol.

    Args:
        key: API source key resolved by the quality API rail.
        symbol: Namespace, type, member, or search text interpreted by the query engine.

    Returns:
        Success with query shape, signature, source metadata, counts, artifact
        paths, and bounded preview records, or an API fault when the source key
        is unknown. A resolved source with no match returns an empty result.
    """
```

```python rejected
def query_api_surface(key: str, symbol: str) -> Result[ApiQuery, ApiFault]:
    """Query API.

    Args:
        key: A string.
        symbol: A string.

    Returns:
        Result[ApiQuery, ApiFault].

    Raises:
        LookupError: If the symbol is missing.
    """
```

The accepted docstring names the caller obligation and typed failure channel. The rejected docstring echoes types, hides the missing-symbol result, and advertises a phantom exception.

## [9][CROSS_REFERENCES_INLINE]

Resolve every code reference through the toolchain that carries it, or omit the reference.

- C# references use `cref` where compiler documentation validation can check them.
- TypeScript references use TSDoc `{@link ...}` or `@see`.
- Python references use the configured documentation generator's resolvable syntax.

Reserve inline comments for the reason a non-obvious choice exists:
```csharp conceptual
// Host catalog lists members lazily, so resolve eagerly here to surface a
// missing symbol at registration rather than at first call.
RegisterEager(symbol);
```

[REJECT_NARRATION]:
```csharp rejected
// Register the symbol.
RegisterEager(symbol);
```

## [10][ANTI_PATTERNS]

Reject these shapes:
- Type-restating parameter: a `<param>`, `@param`, or `Args:` entry that echoes the declared type. Replace it with unit, range, origin, or obligation, or delete it.
- Phantom exception: an `<exception>`, `@throws`, or `Raises:` entry for a type the symbol never throws. Remove it.
- Annotation-echoing return: a `<returns>`, `@returns`, or `Returns:` entry that restates the annotated return type. Replace it with success, effect, or failure semantics, or remove it.
- Hidden side effect or cancellation: a comment that describes only the returned value while the symbol mutates state, writes artifacts, starts work, observes time or IO, allocates/disposes resources, or requires cancellation handling. Add the observable obligation or route the detail to the controlling API/runbook/how-to page.
- Name-echo summary: a summary that paraphrases the symbol name. Rewrite it to the symbol-kind lead shape.
- Profile label leakage: a manual-only profile label emitted into a source comment without a toolchain-local tag. Remove the label and keep the semantics.
- Line-narrating inline comment: an inline comment that restates the next statement. Delete it.

## [11][BOUNDARIES]

- [api.md](api.md) carries generated and contract-backed API reference, including generated mirrors of source comments.
- [reference.md](reference.md) carries curated lookup facts that live outside source.
- [readme.md](readme.md) carries scope-local entry maps that point readers to public surfaces without cataloging every symbol.
- [architecture.md](../explanation/architecture.md) carries current route blocks, invariants, and codemaps; code comments do not carry folder architecture.
- [style-guide.md](../style-guide.md) carries prose mechanics inside comments.
- [proof.md](../proof.md) carries proof that source comments match source behavior and generated reference output; source comments carry no proof details unless a language-specific generator consumes them.
- Source-map pages such as external-library, system API, and testing-tool references may link generated reference or public-symbol behavior, but they must not become source-comment standards or folder-level mini API catalogs.
- [README.md](../README.md) routes document-type, placement, and lifecycle questions.

## [12][CHECKLIST]

- [ ] The validator chose one symbol profile during review, without leaking review labels into source comments.
- [ ] The comment carries the required semantic fields for that profile.
- [ ] No comment restates declared type, return type, nullability, or arity.
- [ ] The lead sentence matches the symbol kind and carries contract, not name echo.
- [ ] Effect surfaces name success, every failure variant, and runtime-context requirements.
- [ ] Throwing surfaces list only actual thrown types with their causes.
- [ ] Generated-reference handoffs use `Changed fact`, `Consumed by`, `Use in this document`, `Update when`, `Close when`, and `Route-away` before local fields.
- [ ] Public symbol behavior, failure-carrier, source-comment, generated-anchor, README-entrypoint, and architecture-invariant changes trigger the adjacent checks named in the authoring contract.
- [ ] Python, TypeScript, and C# comments use the syntax and tags their toolchains parse.
- [ ] C# reference-resolution language reflects Rasm's warning-as-error and documentation-generation settings.
- [ ] Cross-references resolve through the controlling toolchain.
- [ ] Inline comments state a reason, not narration.
- [ ] Folder-level public-surface catalogs route to README, generated API reference, architecture, or reference leaves instead of leaking into source comments.
- [ ] No anti-pattern remains.
