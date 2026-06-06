# [CSHARP_04_GENERATORS_THINKTECTURE]

Research scope: C# source generators and generated public APIs for Rasm code-documentation standards, with focus on `Thinktecture.Runtime.Extensions` value objects, complex value objects, smart enums, unions, generated factories, `[Obsolete]`, generated-reference interactions, and what source comments should own. This report is research input only; it does not edit active standards.

## [1][EXECUTIVE_FINDINGS]

Rasm should treat Thinktecture generated members as machine shape, not as comment-owned prose. Source comments on the handwritten partial type, case type, property, factory-admission hook, or public wrapper own only caller-visible semantics the generated shape does not express: domain invariant, normalization, invalid-state prevention, failure rail mapping, item-specific behavior, case semantics, equality consequence, lifecycle signal, or generated-reference route.

The current `docs/standards/reference/code-documentation.md` C# capsule already has the right center of gravity: XML comments are source semantic truth, compiler XML is a generated mirror, DocFX is the generated-reference profile when adopted, and Thinktecture shapes document invariant and factory failure rather than generated member catalogs. The strongest refinement is to make the generator boundary sharper: comments should document the declaring source surface and generated behavior consequences, while API reference, package/source-map docs, or generated XML/DocFX output should own generated member inventories.

## [2][LOCAL_REPO_TRUTH]

Rasm has a live Thinktecture dependency and XML documentation generation, but it intentionally suppresses missing-comment churn.

Local package and build facts:
- `Directory.Packages.props` pins `Thinktecture.Runtime.Extensions` to `10.2.0`.
- `libs/csharp/Rasm/packages.lock.json` resolves `Thinktecture.Runtime.Extensions`, `Thinktecture.Runtime.Extensions.Analyzers`, `Thinktecture.Runtime.Extensions.Refactorings`, and `Thinktecture.Runtime.Extensions.SourceGenerator` all at `10.2.0`.
- `.cache/nuget/packages/thinktecture.runtime.extensions/10.2.0/thinktecture.runtime.extensions.nuspec` records upstream repository commit `68b861511c419f06ed80bc590df0e460f60e0e48`.
- `.cache/nuget/packages/thinktecture.runtime.extensions/10.2.0/buildTransitive/Thinktecture.Runtime.Extensions.props` exposes source-generator MSBuild properties through `CompilerVisibleProperty`.
- `Directory.Build.props` sets `GenerateDocumentationFile` to `true`, `TreatWarningsAsErrors` to `true`, and suppresses `CS1591`.
- `Directory.Build.props` globally references `Thinktecture.Runtime.Extensions` when workspace libraries are enabled and adds global `Thinktecture` usings.
- `.editorconfig` suppresses `CA1034` because nested types are intentional for Thinktecture regular unions and suppresses `TTRESG1000` as a Thinktecture source-generator self-warning.
- Coverage excludes `**/obj/**`, `**/*.Generated.cs`, `**/*.g.cs`, `GeneratedCodeAttribute`, `CompilerGeneratedAttribute`, and related generated-code attributes.

Existing Rasm source-map docs already own most generated-shape facts:
- `docs/external-libs/thinktecture/api.md` routes package version, generated support API assets, package nuspec, official wiki, and Context7.
- `docs/external-libs/thinktecture/objects.md` records generated value-object factory methods, validation bridging, complex value object constraints, `AllowDefaultStructs`, `SkipFactoryMethods`, and custom validation errors.
- `docs/external-libs/thinktecture/enums.md` records `Items`, `Get`, `TryGet`, generated parsing, delegate-backed behavior, and the anti-pattern against parallel constants or dictionaries.
- `docs/external-libs/thinktecture/unions.md` records regular, ad-hoc, and generic union shapes, generated `Switch`/`Map`, and boundary serialization policy.
- `docs/external-libs/thinktecture/union-attributes.md` records `SwitchMapStateParameterName`, `SwitchMethods`, `MapMethods`, `UnionSwitchMapOverload`, factory generation, conversion generation, `ObjectFactory`, and the absence of generated union `operator +` or `operator |`.

Implication: active code-documentation standards should point authors to generated-shape owners instead of repeating the package catalog in XML comments.

## [3][PRIMARY_SOURCES]

Upstream and platform sources used:

| [INDEX] | [SOURCE]                                                                                                                                                                     | [CURRENT_SIGNAL]                                     | [CLAIM_USED]                                                                                                                                                    |
| :-----: | :--------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :--------------------------------------------------- | :-------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|   [1]   | [Thinktecture.Runtime.Extensions GitHub repository](https://github.com/PawelGerr/Thinktecture.Runtime.Extensions)                                                            | release `10.2.0` shown as latest on 2026-04-10       | package is a Roslyn source-generator, analyzer, and code-fix library for smart enums, value objects, and discriminated unions                                   |
|   [2]   | [Thinktecture Smart Enums wiki](https://github.com/PawelGerr/Thinktecture.Runtime.Extensions/wiki/Smart-Enums)                                                               | edited 2026-04-06                                    | generated `Items`, `Get`, `TryGet`, `Validate`, `Parse`, `TryParse`, `Switch`, `Map`, constructor, equality, key conversion, and delegate-backed behavior       |
|   [3]   | [Thinktecture Value Objects wiki](https://github.com/PawelGerr/Thinktecture.Runtime.Extensions/wiki/Value-Objects)                                                           | current upstream wiki                                | generated `Create`, `TryCreate`, `Validate`, validation hook, equality, hash, string formatting, default-struct handling, complex-object cross-field validation |
|   [4]   | [Thinktecture Discriminated Unions wiki](https://github.com/PawelGerr/Thinktecture.Runtime.Extensions/wiki/Discriminated-Unions)                                             | current upstream wiki                                | ad-hoc and regular union generated conversions, type checks, accessors, equality, `Switch`, `Map`, and factory methods for generic or special member cases      |
|   [5]   | [Thinktecture Source Generator Configuration wiki](https://github.com/PawelGerr/Thinktecture.Runtime.Extensions/wiki/Source-Generator-Configuration)                         | edited 2026-02-09                                    | generator-level MSBuild properties, logging behavior, and generated JetBrains annotation option                                                                 |
|   [6]   | [Microsoft C# XML documentation comments](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/)                                                        | last updated 2026-01-20 in search result             | compiler produces XML from structured source comments; XML docs are not assembly metadata; DocFX and similar tools consume the XML                              |
|   [7]   | [Microsoft C# documentation-comments spec annex](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/documentation-comments)           | current language specification page                  | documentation comments precede declarations, generate flat member ID strings, and `cref` references are checked by the documentation generator                  |
|   [8]   | [Microsoft XML documentation tutorial](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/tutorials/xml-documentation)                                             | recent Microsoft Learn tutorial                      | `GenerateDocumentationFile` emits XML for public comments; avoid type/name echo; document intentional exceptions only when public contract                      |
|   [9]   | [DocFX .NET API docs](https://dotnet.github.io/docfx/docs/dotnet-api-docs.html)                                                                                              | crawled 2026-05 by search; current docs              | DocFX can generate API docs from assemblies with side-by-side XML or from `.csproj`/`.slnx`; it supports standard XML tags and filters public APIs              |
|  [10]   | [Microsoft `Obsolete` compiler attribute reference](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/attributes/general#obsolete-and-deprecated-attribute) | published 2026-03 by search                          | `[Obsolete]` marks APIs as no longer recommended and causes compiler warnings or errors                                                                         |
|  [11]   | [Microsoft `ObsoleteAttribute` API](https://learn.microsoft.com/en-us/dotnet/api/system.obsoleteattribute?view=net-10.0)                                                     | .NET 10 API reference                                | `Message`, `IsError`, `DiagnosticId`, and `UrlFormat` are machine-visible lifecycle fields                                                                      |
|  [12]   | [ASP.NET Core OpenAPI XML comments](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/openapi-comments?view=aspnetcore-10.0)                                | recent Microsoft Learn page                          | XML comments can be consumed by a compile-time OpenAPI source generator when that package/configuration is adopted                                              |
|  [13]   | Context7 `/pawelgerr/thinktecture.runtime.extensions`                                                                                                                        | resolved as high-reputation source with 674 snippets | confirmed upstream generated surfaces and factory behavior against Thinktecture docs                                                                            |

## [4][THINKTECTURE_GENERATED_SURFACES]

Generated public APIs fall into two groups: generated machine shape and caller-visible semantic consequence. Generated shape belongs to source, compiler XML, generated API reference, or external-library reference. Semantic consequence belongs in XML comments only when the declaration does not already express it.

| [INDEX] | [SHAPE]                              | [GENERATED_SURFACE]                                                                                                                                                                                           | [COMMENT_DECISION]                                                                                                                                                                                            |
| :-----: | :----------------------------------- | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
|   [1]   | `[ValueObject<T>]`                   | key member, constructor policy, `Create`, `TryCreate`, `Validate`, parsing interfaces where supported, equality, hash, string conversion, optional operators                                                  | document domain invariant, unit, normalization, invalid raw values, boundary conversion, and failure rail mapping; omit generated factory inventory                                                           |
|   [2]   | `[ComplexValueObject]`               | generated constructor and factory methods, equality across included properties, validation hook, `ToString`, hash                                                                                             | document cross-field invariant, normalization order, equality consequence, default-struct risk, resource/security semantics when applicable; omit property echo                                               |
|   [3]   | `[SmartEnum]` or `[SmartEnum<TKey>]` | private constructor, singleton items, `Items`, `Get`, `TryGet`, `Validate`, `Parse`, `TryParse`, equality, key conversion, `Switch`, `Map`, optional partial dispatch, delegate-backed partial methods        | document closed vocabulary meaning, item-owned behavior, key semantics, protocol mapping, invalid-key behavior, exhaustive-dispatch route; omit generated registry catalog when `Items` is enough             |
|   [4]   | `[Union]` regular union              | nested case support, equality, generated `Switch`, `Map`, optional partial overloads, state parameter, constructor access                                                                                     | document case semantics, payload obligations, invalid state prevention, exhaustive dispatch expectation, failure branch meaning; omit hand-written case catalogs when generated API reference can mirror them |
|   [5]   | `[Union<T1,...>]` and `[AdHocUnion]` | conversions where C# permits them, `Is*`, `As*`, `Value`, equality, hash, `Switch`, `Map`, generated factory methods for type parameters, interfaces, `object`, duplicate types, or explicit factory settings | document why ad-hoc shape is public, member semantic roles, nullable/stateless meaning, and boundary conversion; avoid using it as public replacement for named domain variants                               |
|   [6]   | `[ObjectFactory<T>]`                 | external conversion factory contract                                                                                                                                                                          | document external binding protocol only when Rasm pins and consumes that boundary; otherwise route to external-library reference                                                                              |
|   [7]   | generator MSBuild properties         | logging path, log level, unique log path setting, generated JetBrains annotations                                                                                                                             | document only in package/tool reference or troubleshooting route; source comments should not own generator operations                                                                                         |

## [5][COMMENT_OWNERSHIP_RULES]

For Thinktecture public shapes, the comment target is the handwritten declaration, not every generated member. The declaration plus attribute is the machine-shape source; XML comments add domain semantics.

Source comments should own:
- the invariant admitted by `Create`, `TryCreate`, or `Validate`;
- normalization that changes caller-visible value meaning before construction succeeds;
- the domain meaning of a validation failure after it is mapped to `Error`, `Fin<T>`, `Validation<Error,T>`, or another Rasm rail;
- equality consequences that are not obvious from the generated shape, such as key comparer choice, ignored member, case identity, or cross-field identity law;
- smart-enum key semantics, protocol mapping, display or persistence key, and item-owned behavior;
- union case meaning, payload obligations, stateless/nullability semantics, and the exhaustive dispatch route callers should use;
- actual boundary exceptions only when the public surface intentionally throws rather than returning a typed rail;
- lifecycle markers only when an external caller or generated reference consumes them.

Source comments should not own:
- generated factory inventories like `Create`, `TryCreate`, `Validate`, `Items`, `Get`, `TryGet`, `Switch`, `Map`, `Parse`, `TryParse`, `Is*`, `As*`, conversion operators, equality operators, or hash methods when those are already generated by package shape;
- generated-source file paths under `obj`;
- full smart-enum item catalogs that are derivable from `Items`;
- full union case catalogs that generated API reference can expose from source;
- package-version facts, generator settings, analyzer diagnostics, or source-generator troubleshooting;
- lifecycle wrappers for internal stale surfaces that should be deleted in Rasm's greenfield posture;
- XML tags that fake exception behavior for typed rails.

## [6][GENERATED_REFERENCE_INTERACTIONS]

C# XML documentation has three relevant layers:
1. Source XML comments live beside declarations and carry semantic source truth.
2. The compiler-generated XML file combines comments with source structure into a flat generated mirror keyed by member IDs.
3. DocFX or another generated-reference tool consumes the assembly and side-by-side XML, or performs a design-time build from `.csproj`/`.slnx`, to render public API pages.

For Thinktecture, generated public members complicate layer 2 and layer 3. The package generator emits members during compilation; generated reference tooling can see compiled public members, but Rasm source comments cannot reasonably own every generated member. If a generated API page exposes `Create`, `TryCreate`, `Items`, `Switch`, or `Map`, the page should either render generator-provided XML or leave generated boilerplate minimally documented. Rasm comments should be placed on the handwritten type, case, property, or externally maintained route that explains semantics.

DocFX-specific consequence: DocFX can include generated public APIs because it loads projects through MSBuild design-time build or reflects assemblies with side-by-side XML. It also parses XML comments as Markdown by default, which supports the current standard's rejection of Markdown-heavy XML unless the generated renderer is the only consumer.

ASP.NET Core OpenAPI consequence: Microsoft documents a compile-time OpenAPI source generator that can consume XML documentation comments and generate descriptions. Rasm should not generalize this into a source-comment obligation unless an ASP.NET/OpenAPI package and configuration are actually adopted. If adopted, endpoint/API generated comments route through `api.md`, not through the Thinktecture capsule.

## [7][OBSOLETE_AND_LIFECYCLE]

`[Obsolete]` is machine-visible lifecycle metadata, not a prose comment substitute. Microsoft documents that using an obsolete API produces a warning or error; `ObsoleteAttribute` carries `Message`, `IsError`, `DiagnosticId`, and `UrlFormat`.

Recommended Rasm rule:
- Use `[Obsolete]` only when an external support contract, generated reference, package consumer, or compatibility route must see the lifecycle signal.
- Put replacement path, behavioral delta, and removal or migration condition in the attribute message or adjacent XML comment only when the generated reference consumes it.
- Prefer deletion or direct replacement for internal greenfield stale surfaces.
- Do not use `[Obsolete]` to preserve generated Thinktecture factories, old union cases, old smart-enum items, or wrapper-only APIs that Rasm can remove.

Generated Thinktecture interaction:
- If a handwritten smart-enum item or union case is externally visible and must be deprecated, mark the source declaration that the generator consumes, not an emitted generated member.
- If a generated factory name changes because a source type or property changes, update callers and generated reference; do not add obsolete wrappers unless a real external package contract requires it.
- If generator-provided public members themselves are obsolete upstream, the package XML or generated reference should surface that upstream marker; Rasm source comments should not duplicate upstream lifecycle prose.

## [8][RECOMMENDED_STANDARD_DELTA]

The active standard already says:
- C# XML comments are semantic source truth.
- compiler XML is the generated mirror.
- DocFX is the generated-reference profile when adopted.
- Thinktecture value objects, complex value objects, smart enums, and unions document invariant, normalization, invalid-state prevention, factory failure, equality consequence, closed vocabulary, case semantics, and exhaustive dispatch route.
- lifecycle tags serve external support contracts only.

Potential tightening for the C# capsule:
- Add that Thinktecture-generated member catalogs route to generated API reference or `docs/external-libs/thinktecture`, not XML comments.
- Add that generated factory methods are documented at the declaring type only when the generated signature omits domain failure semantics or normalization.
- Add that smart-enum `Items` is the source for item catalogs; comments should state closed vocabulary meaning and item-owned behavior, not list every item.
- Add that union `Switch`/`Map` comments should state exhaustive dispatch expectation only where callers need the route; generated overload inventory belongs in generated reference.
- Add that `[Obsolete]` belongs on the handwritten source declaration that owns the external lifecycle signal, never on a wrapper invented to preserve a stale generated name.

Suggested capsule sentence:

```markdown conceptual
Thinktecture-generated members such as `Create`, `TryCreate`, `Validate`, `Items`, `Get`, `TryGet`, `Switch`, `Map`, conversion operators, and case accessors are machine shape; XML comments on the handwritten partial type, case, property, or boundary method document only the domain invariant, normalization, failure rail, equality consequence, closed-vocabulary meaning, case semantics, lifecycle signal, or generated-reference route the generated signature does not express.
```

Suggested reject addition:

```markdown conceptual
Reject generated-member inventories for Thinktecture factories, smart-enum registries, and union dispatch overloads; route those to generated API reference or `docs/external-libs/thinktecture`, and keep source comments on caller-visible semantics.
```

## [9][EDGE_CASES]

### [9.1][CUSTOM_VALIDATION_ERROR]

If `[ValidationError<TCustom>]` changes the generated validation error type, XML comments should state the public failure meaning only when callers observe that type or when Rasm maps it into a domain rail. The generated generic shape and static `Create(string)` contract belong to package/source-map docs.

### [9.2][SKIP_FACTORY_METHODS]

If `SkipFactoryMethods` removes generated `Create`, `TryCreate`, and `Validate`, the public construction rail changes. Document the replacement admission route because callers can no longer rely on the standard generated factories. This is one of the few cases where a generated-surface absence is comment-relevant.

### [9.3][ALLOW_DEFAULT_STRUCTS]

If `AllowDefaultStructs = true`, comments should document the domain meaning of `default(T)` or the generated default instance because it creates a caller-visible value that bypasses ordinary validation expectations. If zero-init is not domain-valid, the standard should push authors away from the option rather than documenting around it.

### [9.4][KEYLESS_SMART_ENUMS]

Keyless smart enums cannot carry default key-based serialization or model-binding semantics without an object factory. Comments should state the absence or boundary conversion only when the type is public across a serialization, API, UI, or persistence boundary.

### [9.5][AD_HOC_UNIONS]

Ad-hoc unions can be public, but Rasm's docs already prefer named regular unions for domain variants. Comments should explain why type identity alone is semantically sufficient or why the ad-hoc shape is restricted to a local boundary. Generated `Is*`, `As*`, conversion, and factory methods should remain generated-reference facts.

### [9.6][PARTIAL_DISPATCH]

Partial `Switch`/`Map` overloads intentionally admit defaults. If public callers rely on partial dispatch, comments should document the default behavior and the condition where exhaustive dispatch is still required. Otherwise, generated overload details belong to package docs.

### [9.7][UNRESOLVED_CREF]

Rasm treats warnings as errors, but `CS1591` is suppressed. Unresolved `cref` remains a bad source-comment route because compiler/doc generation can validate references. Prefer `cref` only for stable public symbols and omit routes that generated tooling cannot resolve.

## [10][TRANSCRIPT]

Read and verification sequence:
1. Read `CLAUDE.md`, root `AGENTS.md` from the prompt, `docs/standards/README.md`, `docs/standards/AGENTS.md`, and the full `docs/standards/reference/code-documentation.md`.
2. Read shared governing standards: `proof.md`, `information-structure.md`, `style-guide.md`, `agentic-documentation.md`, and `formatting.md`.
3. Searched memory for docs-standards context and used it only to confirm active-corpus and proof-owner expectations.
4. Searched local repo for `Thinktecture`, `GenerateDocumentationFile`, `CS1591`, `Obsolete`, `DocFX`, generated-file exclusions, and generated-source settings.
5. Read existing Rasm Thinktecture source-map docs: `docs/external-libs/thinktecture/api.md`, `objects.md`, `enums.md`, `unions.md`, and `union-attributes.md`.
6. Read local build/package truth: `Directory.Packages.props`, `Directory.Build.props`, `.editorconfig`, `libs/csharp/Rasm/packages.lock.json`, Thinktecture `.nuspec` files, and Thinktecture `buildTransitive` props.
7. Verified upstream primary sources through web search and direct opens: Thinktecture GitHub repo, Thinktecture wiki pages, Microsoft Learn XML docs, C# documentation-comments spec, Microsoft Learn `[Obsolete]` docs, ASP.NET Core OpenAPI XML comments, and DocFX API docs.
8. Resolved Thinktecture in Context7 and queried generated API behavior for value objects, smart enums, unions, factories, and validation.
9. Observed an unrelated dirty active-standard file: `docs/standards/reference/code-documentation.md`. It was read for context and left untouched.
10. Created only this assigned report file.

## [11][PROOF_GAPS]

- No local C# build was run. This research did not need to mutate `obj` outputs, and the user constrained edits to the assigned report file.
- No DocFX generation was run. Rasm search did not find a local `docfx` configuration, so DocFX behavior is sourced from current DocFX docs and treated as an available profile, not a configured Rasm gate.
- No generated Thinktecture source was emitted in-repo during this task. Generated-member claims come from the pinned package metadata, upstream Thinktecture docs, Context7, and existing Rasm source-map docs.

## [12][CLOSE_CHECK]

- [x] Active standards and governing shared standards were read.
- [x] Current primary sources were used for changing external behavior.
- [x] Local package and build truth were checked before upstream synthesis.
- [x] Active standards were not edited.
- [x] The report distinguishes generated machine shape from source-comment semantics.
- [x] Proof gaps are stated instead of claiming unrun generated-reference gates.
