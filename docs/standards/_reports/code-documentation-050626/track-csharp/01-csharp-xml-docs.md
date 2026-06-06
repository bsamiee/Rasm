# [CSHARP_XML_DOCS_RESEARCH]

This report audits the C# XML documentation capsule in `docs/standards/reference/code-documentation.md` against current Microsoft and DocFX primary sources. It is a research artifact only; active standards stay unchanged.

## [1][SCOPE]

Focus: C# 14 XML documentation comments, compiler XML generation, `cref` validation, nullable and exception documentation, DocFX behavior, and current Microsoft guidance.

Assigned output: `docs/standards/_reports/code-documentation-050626/track-csharp/01-csharp-xml-docs.md`.

Active standard read target: `docs/standards/reference/code-documentation.md`, especially `[6.1][C_SHARP_14]`.

Repository context:
- `Directory.Build.props` sets `LangVersion` to `14.0`, `Nullable` to `enable`, `TreatWarningsAsErrors` to `true`, and `GenerateDocumentationFile` to `true`.
- `Directory.Build.props` suppresses `CS1591` through `NoWarn`, so missing-comment churn is explicitly not a build-enforced documentation rule in Rasm.
- `docs/standards/reference/code-documentation.md` already rejects `CS1591`-driven churn where declarations carry caller truth.

## [2][SOURCES]

Maintained upstream sources:

| [INDEX] | [SOURCE]                                                                                                                                                              | [DATE_SIGNAL]                                                        | [USE]                                                                              |
| :-----: | :-------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :------------------------------------------------------------------- | :--------------------------------------------------------------------------------- |
|   [1]   | [Microsoft Learn: XML API documentation comments](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/)                                         | last updated page visible through Microsoft Learn; opened 2026-06-05 | compiler XML generation, `CS1591`, signature matching, XML output purpose          |
|   [2]   | [Microsoft Learn: Recommended XML documentation tags](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/recommended-tags)                     | page surfaced as published 4 months ago; opened 2026-06-05           | validated tags, `cref`, `href`, `inheritdoc`, `include`, `exception`, tag catalog  |
|   [3]   | [C# language specification: Documentation comments](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/documentation-comments) | page surfaced as published 8 months ago; opened 2026-06-05           | normative shape, generator duties, generic `cref` escaping, XML-file limits        |
|   [4]   | [Microsoft Learn: MSBuild properties for Microsoft.NET.Sdk](https://learn.microsoft.com/en-us/dotnet/core/project-sdk/msbuild-props)                                  | page surfaced as published 3 months ago; opened 2026-06-05           | `GenerateDocumentationFile`, `DocumentationFile`, XML publish behavior             |
|   [5]   | [Microsoft Learn: C# compiler output options](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-options/output)                             | last updated 2025-05-31 in page footer; opened 2026-06-05            | `DocumentationFile`, XML file placement, IntelliSense file-name requirement        |
|   [6]   | [Microsoft Learn: C# compiler errors and warnings options](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-options/errors-warnings)       | last updated 2025-05-31 in page footer; opened 2026-06-05            | `TreatWarningsAsErrors`, `NoWarn`, warning-as-error interaction                    |
|   [7]   | [Microsoft Learn: Compiler warning CS1591](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-messages/cs1591)                               | last updated 2022-12-14 in page footer; opened 2026-06-05            | missing XML comment warning definition                                             |
|   [8]   | [Microsoft Learn: Nullable reference types](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/null-safety/nullable-reference-types)                        | last updated 2026-05-26 in page footer; opened 2026-06-05            | nullable annotations, null-state analysis, API contract attributes                 |
|   [9]   | [Microsoft Learn: Nullable static analysis attributes](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/attributes/nullable-analysis)               | page surfaced as published 4 months ago; opened 2026-06-05           | `AllowNull`, `DisallowNull`, `MaybeNull`, `NotNull`, conditional postconditions    |
|  [10]   | [Microsoft Learn: Create and throw exceptions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/exceptions/creating-and-throwing-exceptions)              | older stable guidance; opened 2026-06-05                             | exception contract distinction and async exception timing                          |
|  [11]   | [Microsoft Learn: Exception-handling statements](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/statements/exception-handling-statements)         | page surfaced as published 4 months ago; opened 2026-06-05           | current exception statement behavior and helper methods                            |
|  [12]   | [DocFX: Basic Concepts](https://dotnet.github.io/docfx/docs/basic-concepts.html)                                                                                      | current docs, no visible page date; opened 2026-06-05                | DocFX metadata step, project input, Roslyn source navigation, YAML output          |
|  [13]   | [DocFX: .NET API Docs](https://dotnet.github.io/docfx/docs/dotnet-api-docs.html)                                                                                      | current docs, no visible page date; opened 2026-06-05                | supported XML tags, Markdown parsing default, `shouldSkipMarkup`, public filtering |
|  [14]   | [DocFX CLI: `docfx metadata`](https://dotnet.github.io/docfx/reference/docfx-cli-reference/docfx-metadata.html)                                                       | crawled 2 days ago by search; opened 2026-06-05                      | `--warningsAsErrors`, `--shouldSkipMarkup`, `--noRestore`, metadata options        |
|  [15]   | [dotnet/docfx release v2.78.5](https://github.com/dotnet/docfx/releases/tag/v2.78.5)                                                                                  | released 2026-02-24                                                  | current release signal; .NET 10 target-framework support                           |

Repository sources:

| [INDEX] | [SOURCE]                                         | [EVIDENCE]                                                                                   |
| :-----: | :----------------------------------------------- | :------------------------------------------------------------------------------------------- |
|   [1]   | `Directory.Build.props`                          | lines 63-68: `AnalysisLevel`, `TreatWarningsAsErrors`, `GenerateDocumentationFile`, `NoWarn` |
|   [2]   | `docs/standards/reference/code-documentation.md` | lines 160-188: current C# capsule                                                            |
|   [3]   | `docs/standards/reference/code-documentation.md` | lines 344-345: C# references use `cref` where compiler validation can check them             |
|   [4]   | `docs/standards/AGENTS.md`                       | lines 21-40: owner routing and artifact contract                                             |
|   [5]   | `docs/standards/proof.md`                        | lines 20-26 and 30-38: evidence hierarchy and conflict handling                              |

Context7 check:

Library: `/dotnet/docfx`
Question: DocFX .NET API docs, XML documentation comments, supported XML tags, metadata input behavior, Markdown parsing, and `shouldSkipMarkup`.
Result: Context7 returned DocFX official docs snippets matching the opened DocFX pages: DocFX supports Microsoft recommended XML tags, parses XML comments as Markdown by default, can skip markup through `shouldSkipMarkup`, and generates .NET API docs through metadata and build stages.

## [3][TRANSCRIPT]

Repository reads:
- Read `CLAUDE.md`.
- Read root `AGENTS.md` from the user-provided instruction payload.
- Read `docs/standards/README.md`.
- Read `docs/standards/AGENTS.md`.
- Read `docs/standards/agentic-documentation.md`.
- Read `docs/standards/information-structure.md`.
- Read `docs/standards/proof.md`.
- Read `docs/standards/style-guide.md`.
- Read `docs/standards/formatting.md`.
- Read all visible portions of `docs/standards/reference/code-documentation.md`, including `[6.1][C_SHARP_14]`, lifecycle references, anti-patterns, boundaries, and validation.
- Listed active standards with `fd -H . docs/standards -t f -e md | sort`.
- Checked nearest standards overlay with `fd -H 'AGENTS(\\.override)?\\.md' docs/standards/reference docs/standards -t f | sort`.
- Checked Rasm C# build and warning settings with `rg -n "GenerateDocumentationFile|DocumentationFile|NoWarn|TreatWarningsAsErrors|WarningsAsErrors|CS15|CS157|CS158|CS159|Documentation" ...`.
- Read `Directory.Build.props` and relevant numbered lines.
- Checked existing DocFX and XML-doc references under `docs`, `libs`, `tests`, and `tools`.

External research:
- Searched Microsoft Learn for C# XML documentation comments, recommended tags, compiler messages, MSBuild documentation properties, nullable reference guidance, nullable attributes, and exception guidance.
- Opened Microsoft Learn pages for XML docs, recommended tags, language specification documentation comments, MSBuild documentation-file properties, compiler output options, compiler warning and warning-as-error options, `CS1591`, nullable references, nullable analysis attributes, and current exception guidance.
- Searched and opened DocFX official docs for .NET API docs, basic concepts, metadata CLI, and the latest GitHub release.
- Queried Context7 for `/dotnet/docfx`.

Workspace safety:
- `git status --short -- docs/standards/_reports/code-documentation-050626 docs/standards/reference/code-documentation.md Directory.Build.props` showed `docs/standards/reference/code-documentation.md` already modified in the worktree and `general-05-lifecycle.md` already untracked. This report does not edit those files.

## [4][FINDINGS]

### [4.1][COMPILER_XML]

Finding: The current standard is directionally correct that XML comments in `.cs` are source truth and compiler XML is a generated mirror, but the DocFX path needs a sharper distinction.

Evidence:
- Microsoft states that enabling `GenerateDocumentationFile` or `DocumentationFile` makes the compiler create XML documentation output from XML-tagged comment fields.
- Microsoft states that enabling documentation output generates `CS1591` for publicly visible members without XML documentation comments.
- Microsoft also states that the XML documentation file does not provide full type/member information and must be combined with reflection or another type/member source.
- DocFX states that project-based metadata reads source projects and uses Roslyn to navigate the supplied codebase, not the compiler `.xml` output. DocFX also supports compiled `.dll` plus `.xml` input.

Recommendation: Change the C# capsule wording from a single linear chain to a forked generated-reference model.

Suggested standard direction:
- Source comments in `.cs` carry semantic comment truth.
- Compiler XML is the build-generated XML mirror when `GenerateDocumentationFile` or `DocumentationFile` is enabled.
- DocFX is the .NET generated-reference profile when configured; project input uses Roslyn metadata extraction, while compiled input can consume DLL and XML pairs.

Confidence: HIGH.

### [4.2][LOCAL_ENFORCEMENT]

Finding: Rasm's local enforcement supports the standard's rejection of missing-comment churn and makes unresolved XML-doc warnings materially stronger than the upstream default.

Evidence:
- `Directory.Build.props` sets `TreatWarningsAsErrors` to `true`.
- `Directory.Build.props` sets `GenerateDocumentationFile` to `true`.
- `Directory.Build.props` suppresses `CS1591`.
- Microsoft states `TreatWarningsAsErrors` reports warnings as errors and stops output generation.
- Microsoft states `NoWarn` suppresses named warning IDs and should be used only when a warning is understood.

Recommendation: Add an explicit local-setting note to the C# capsule or validation section:
- Rasm generates XML documentation files, treats unsuppressed C# warnings as errors, and suppresses `CS1591`.
- Therefore, malformed XML, invalid validated tags, invalid `cref`, and signature-mismatched validated tags are build-relevant, while missing-comment coverage is a review question instead of an automatic comment requirement.

Confidence: HIGH.

### [4.3][VALIDATED_TAGS]

Finding: The current capsule should distinguish compiler-validated tags from renderer-supported or copied tags.

Evidence:
- Microsoft recommended-tags guidance says the compiler copies any valid XML tags to the output file.
- Microsoft identifies the compiler-verified tags with `*`: `<param>`, `<exception>`, `<include>`, `<see>`, `<seealso>`, and `<typeparam>`.
- Microsoft says `cref` attached to any tag is checked against code elements in the current compilation environment.
- Microsoft says Visual Studio provides IntelliSense for additional tags, including `<inheritdoc>` and `<example>`.
- The C# specification says a conforming compiler is not required to check documentation syntax, but the documentation generator for CLI-targeting implementations has special duties for `param` and `cref`.

Recommendation: Add a compact checked-vs-supported split:
- Checked by compiler/documentation generator: well-formed XML, `cref` resolution, `<param>` existence/coverage, `<typeparam>` existence, `<exception cref>`, `<include>` path/XPath shape, `<see>`, and `<seealso>`.
- Supported or copied but not necessarily compiler-validated: `<summary>`, `<remarks>`, `<returns>`, `<value>`, `<para>`, `<list>`, `<c>`, `<code>`, `<example>`, `<paramref>`, `<typeparamref>`, `<inheritdoc>`, `href`, and `langword`.

Change recommendation: Replace the phrase "`<see cref>`, `<seealso cref>`, `<see href>`, `<see langword>`, `<typeparamref>`, and `<paramref>` provide checked or stable routes" with a more precise split:
- "`cref` routes are compiler-checked when the referenced code element is available in the compilation; `href` and `langword` are stable external or keyword routes; `<paramref>` and `<typeparamref>` are name-reference formatting tags and must not be overclaimed as compiler-checked unless local compiler output proves it."

Confidence: HIGH for the general split; MEDIUM for exact behavior of every nonstarred tag because Microsoft only marks the compiler-verified subset and does not enumerate all non-validation warning details on the page.

### [4.4][CREF_AND_HREF]

Finding: The standard should keep `cref` as the internal code-reference route and should add the external-link distinction.

Evidence:
- Microsoft says `cref` references code elements and the compiler checks the element exists.
- Microsoft says the compiler respects `using` directives while resolving `cref`.
- Microsoft says generic code references must use escaped angle brackets or braces, such as `List&lt;T&gt;` or `List{T}`.
- Microsoft says `href` should be used for external web pages; `cref` is for code references and does not create clickable links for external URLs.
- Microsoft says documentation tools such as DocFX use `cref` attributes to generate internal hyperlinks.

Recommendation: Add external-link guidance:
- Use `cref` only for code elements available to the compilation or generated-reference resolver.
- Use `href` for external URLs.
- Use escaped angle brackets or brace syntax for generic `cref` targets.
- Omit the cross-reference when the controlling toolchain cannot resolve it.

Confidence: HIGH.

### [4.5][INHERITDOC]

Finding: The current standard's "`<inheritdoc>` applies only when inherited semantics are exact" is correct but should mention the compiler XML distinction.

Evidence:
- Microsoft says Visual Studio can automatically show inherited XML documentation in IntelliSense for undocumented overrides or implementations, but that automatic inheritance does not affect the XML documentation file generated by the compiler.
- Microsoft says distributed public APIs should explicitly use `<inheritdoc>` or provide complete documentation so the generated XML documentation file includes necessary information.
- DocFX supports Microsoft recommended XML tags.

Recommendation: Keep the current exact-semantics constraint and add:
- Automatic Visual Studio inherited Quick Info is not a generated XML-file guarantee.
- Use explicit `<inheritdoc>` only when the inherited public contract is exact and the generated reference consumer needs the inherited text.
- Do not use `<inheritdoc>` to hide a changed contract, narrowed failure, new side effect, or different resource/cancellation obligation.

Confidence: HIGH.

### [4.6][NULLABLE]

Finding: The current nullable rule is correct and should be strengthened with nullable static-analysis attributes. No current primary source supports a special C# XML `<nullable>` tag as the owner of null-state.

Evidence:
- Microsoft's current nullable reference type guidance says nullable reference types are compile-time features, annotations express intent, null-state analysis produces warnings, and API attributes describe nuanced contracts.
- Microsoft says `string` and `string?` have the same runtime type, and `?` informs the compiler of design intent.
- Microsoft's nullable attributes page says attributes give the compiler more API-contract information but do not enable more implementation checks.
- Nullable attributes include `AllowNull`, `DisallowNull`, `MaybeNull`, `NotNull`, `MaybeNullWhen`, `NotNullWhen`, `NotNullIfNotNull`, `MemberNotNull`, `MemberNotNullWhen`, `DoesNotReturn`, and `DoesNotReturnIf`.

Recommendation: Keep "Nullable annotations own null-state" and add:
- Nullable attributes own nuanced API null-state contracts when `?` is insufficient.
- XML comments state domain absence, sentinel/default pitfalls, boundary conversion, or null propagation only when the type annotation and nullable attributes cannot express the caller-visible meaning.
- Do not introduce a source-comment `<nullable>` tag as a standard C# null-state carrier.

Confidence: HIGH.

### [4.7][EXCEPTIONS]

Finding: The current `<exception>` rule is aligned with Microsoft and Rasm's rail doctrine. It should distinguish actual thrown exceptions from returned error data and async task storage.

Evidence:
- Microsoft recommended-tags guidance says `<exception cref="member">` identifies exceptions that can be thrown and applies to methods, properties, events, and indexers.
- Microsoft says the compiler checks the referenced exception exists and translates the member to canonical element name in output XML.
- Microsoft exception guidance says exceptions indicate operations that cannot complete their defined functionality and that async task-returning methods store exceptions in the returned task until awaited.
- Rasm's current standard says fake `<exception>` tags for typed rails are rejected and rail surfaces document observable failure data in result/returns/remarks instead.

Recommendation: Keep "actual thrown type and cause on a throwing surface only" and add:
- Use `<exception>` for supported thrown exception contracts only, including synchronous validation exceptions and task-stored exceptions when they are part of the public contract.
- Do not use `<exception>` for `Fin<T>`, `Validation<Error,T>`, `Eff<RT,T>`, `IO<T>`, `Result`, or other returned failure rails.
- For async or effect surfaces, document terminal boundary and observation point in `<returns>` or `<remarks>` unless the surface intentionally exposes thrown exceptions.

Confidence: HIGH.

### [4.8][DOCFX]

Finding: The current DocFX guidance is right to reject Markdown-heavy XML unless the generated renderer is the only consumer, and it should add the exact DocFX default.

Evidence:
- DocFX says it supports Microsoft recommended XML tags.
- DocFX warns that it parses XML documentation comments as Markdown by default and that Markdown in XML comments may cause rendering problems in places that do not support Markdown, such as Visual Studio IntelliSense.
- DocFX says `shouldSkipMarkup` disables Markdown processing for XML tags.
- DocFX says it shows only public accessible types and methods callable from another assembly by default, with default filtering rules and optional `includePrivateMembers`.
- DocFX latest release v2.78.5, released 2026-02-24, adds .NET 10 target-framework support, which matters to Rasm's `net10.0` baseline if DocFX generation is adopted.

Recommendation: Add DocFX renderer guidance:
- DocFX parses XML comments as Markdown by default.
- If comments must serve IntelliSense and DocFX, keep XML comments XML-native and light on Markdown, or configure `shouldSkipMarkup`.
- Treat DocFX UID/anchor generation and filtering as generated-reference behavior, not source-comment policy.
- If Rasm adopts a DocFX config, the standard should cite the config path and generation command rather than naming DocFX generically.

Confidence: HIGH.

### [4.9][INCLUDE_FILES]

Finding: The current rejection of `<include>` files without maintained source is correct and should stay strict.

Evidence:
- Microsoft says `<include>` pulls XML comments from another file by path and XPath.
- Microsoft frames include files as an alternative to inline comments and a source-control artifact.
- Rasm's standards reject generated or duplicated catalogs and require source truth close to the claim.

Recommendation: Keep the current reject clause. If active standards are revised, add that `<include>` requires a maintained source file, a local ownership route, and proof that generated XML includes the intended tags. Do not allow external include files as a way to create a hidden parallel documentation source.

Confidence: HIGH.

### [4.10][PUBLIC_COVERAGE]

Finding: Microsoft broadly recommends documenting public APIs, but Rasm's source-comment standard intentionally narrows the obligation to caller-visible semantics the declaration cannot express. This is a justified repo-local divergence.

Evidence:
- Microsoft recommended-tags guidance says document all publicly visible types and public members for consistency and at minimum include `<summary>`.
- Microsoft says enabling documentation output emits `CS1591` for publicly visible members without XML docs.
- Rasm suppresses `CS1591` and the active standard says public visibility creates a documentation question, not an automatic comment requirement.

Recommendation: No change to Rasm's doctrine. Add only a proof note if desired: Rasm intentionally suppresses `CS1591`, so upstream public-coverage advice is lower precedence than repo-local semantic-comment policy.

Confidence: HIGH.

## [5][ADD_RECOMMENDATIONS]

Add these facts to the active standard only if the editor pass changes the C# capsule:
- Rasm setting: `GenerateDocumentationFile=true`, `TreatWarningsAsErrors=true`, and `NoWarn` includes `CS1591`.
- Compiler validation split: checked tags and `cref` routes versus renderer-supported or copied tags.
- `href` route: external URLs use `href`, not `cref`.
- Generic `cref`: use escaped angle brackets or brace syntax.
- Explicit `<inheritdoc>`: needed for generated XML when inherited docs should appear outside Visual Studio automatic Quick Info.
- Nullable attributes: use `System.Diagnostics.CodeAnalysis` attributes for nuanced null-state contracts before prose.
- No standard `<nullable>` XML tag: nullable annotations and attributes own null-state.
- DocFX input split: project metadata uses Roslyn over source; compiled metadata can use DLL/XML input.
- DocFX Markdown default: XML comments are parsed as Markdown by default unless `shouldSkipMarkup` is set.
- DocFX config proof: if Rasm adopts DocFX, cite actual `docfx.json`, command, version, and output path.

## [6][CHANGE_RECOMMENDATIONS]

Change these current claims for precision:
- "compiler XML is the generated mirror, and DocFX is the generated-reference profile" should become "compiler XML is one generated mirror; DocFX is a generated-reference profile whose input path depends on configuration."
- "`<see cref>`, `<seealso cref>`, `<see href>`, `<see langword>`, `<typeparamref>`, and `<paramref>` provide checked or stable routes" should split checked `cref` from stable `href`/`langword` and name `<paramref>`/`<typeparamref>` as name-reference formatting tags.
- "Markdown-heavy XML unless the generated renderer is the only consumer" should mention DocFX's Markdown parsing default and IntelliSense incompatibility risk.

## [7][REMOVE_RECOMMENDATIONS]

Remove or avoid these overclaims:
- Any implication that DocFX always consumes compiler XML for project input.
- Any implication that all listed XML tags are compiler validated.
- Any implication that nullable semantics belong in XML comments rather than annotations and nullable analysis attributes.
- Any implication that `<exception>` documents returned typed failure rails.
- Any automatic public-member comment requirement derived from `CS1591`.

## [8][NO_CHANGE_CONFIRMATIONS]

Keep these current rules:
- Source comments document caller-visible semantics the declaration cannot express.
- `<summary>` should carry controlling purpose, not name echo.
- `<param>` and `<typeparam>` should carry semantic role or caller obligation, not type echo.
- `<returns>` should carry success, failure-rail, runtime, terminal, or resource semantics, not carrier echo.
- `<exception>` should name actual thrown type and cause on throwing surfaces only.
- Nullable annotations own null-state; comments only add observable domain absence or boundary nuance.
- Missing-comment churn from `CS1591` is rejected when declarations already carry caller truth.
- Unresolved `cref` is rejected.
- Fake `<exception>` tags for typed rails are rejected.
- `<include>` files without maintained source are rejected.
- Lifecycle wrappers for internal greenfield surfaces should be deleted or replaced rather than documented.

## [9][CONFIDENCE]

Overall confidence: HIGH.

Basis:
- Microsoft Learn XML-doc pages are current within the repo's research window or stable official language guidance.
- Microsoft Learn nullable reference guidance was updated 2026-05-26.
- DocFX official docs and Context7 agreed on supported tags, metadata flow, Markdown parsing default, and `shouldSkipMarkup`.
- Repository settings directly prove local warning and XML generation behavior.

Residual gaps:
- I did not run a live Rasm C# build or synthesize XML-doc warning fixtures, because the assignment is docs research and active standards were not to be edited.
- I did not validate every Roslyn diagnostic ID for XML documentation comments individually. The recommendations rely on Microsoft's current validated-tag guidance and Rasm's local warning-as-error settings.
- I did not inspect a Rasm `docfx.json` because no current DocFX config appeared in the scoped search. If one exists outside the searched paths, generated-reference recommendations should be refreshed against it.

## [10][VALIDATION]

- [x] Active standards read scope satisfied for a narrow research artifact.
- [x] Current primary sources used for Microsoft XML docs, nullable guidance, compiler settings, and DocFX behavior.
- [x] Active standards left unchanged.
- [x] Assigned report file created.
- [x] `git diff --check -- docs/standards/_reports/code-documentation-050626/track-csharp/01-csharp-xml-docs.md` passed.
