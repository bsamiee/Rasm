# CSHARP 5 API Style Research

Report path: `docs/standards/_reports/code-documentation-050626/track-csharp/05-csharp-api-style.md`
Focus: modern C# public API documentation style in the Microsoft/.NET ecosystem: XML summaries, remarks, analyzers, examples, inherited docs, exceptions, security, cancellation, and resource comments.
Date: 2026-06-05.

## Scope

This report is source material for a later active-standard edit. It does not edit `docs/standards/reference/code-documentation.md` or any other active standard.

The controlling local question is whether the C# capsule in `code-documentation.md` needs sharper API-style guidance for public XML comments under Rasm's current build posture: XML documentation files are generated, warnings are errors, and `CS1591` is explicitly suppressed.

## Read Transcript

Local instruction and target files read fully:

- `CLAUDE.md`
- `AGENTS.md`
- `docs/standards/AGENTS.md`
- `docs/standards/README.md`
- `docs/standards/reference/code-documentation.md`

Shared governing standards read fully:

- `docs/standards/information-structure.md`
- `docs/standards/proof.md`
- `docs/standards/style-guide.md`
- `docs/standards/formatting.md`

Adjacent/local scans:

- `fd -H . docs/standards -t f -e md`
- `fd -H . docs/standards/reference -t f -e md`
- `nl -ba docs/standards/reference/code-documentation.md | sed -n '156,224p'`
- `rg -n "GenerateDocumentationFile|DocumentationFile|TreatWarningsAsErrors|WarningsAsErrors|NoWarn|CS1591|CA1200|Documentation" Directory.Build.props Directory.Build.targets Directory.Packages.props .editorconfig libs tests tools -g '*.props' -g '*.targets' -g '*.csproj' -g '.editorconfig'`
- `rg -n "///|<summary>|<remarks>|<inheritdoc|<exception|<returns>|<param|<see cref|<seealso" libs/csharp -g '*.cs'`

External primary-source checks:

- Microsoft Learn, C# XML documentation comments overview.
- Microsoft Learn, recommended C# XML documentation tags.
- Microsoft Learn, C# language specification Annex D documentation comments.
- Microsoft Learn, C# XML documentation tutorial.
- Microsoft Learn, `CS1591`.
- Microsoft Learn, .NET documentation code-analysis rules.
- Microsoft Learn, `CA1200`.
- Microsoft Learn, C# 14 extension members.
- DocFX .NET API docs.
- `dotnet/dotnet-api-docs` README.
- Microsoft Learn, .NET exception best practices.
- Microsoft Learn, .NET cancellation in managed threads.
- Microsoft Learn, .NET `Dispose` and `DisposeAsync` implementation guidance.
- Microsoft Learn, .NET secure coding guidelines and BinaryFormatter security guide.

## Local Source Notes

- `code-documentation.md` already frames C# XML comments as semantic source truth, compiler XML as the generated mirror, and DocFX as the generated-reference profile when .NET API documentation is produced.
- The current C# capsule already assigns `<summary>`, `<typeparam>`, `<param>`, `<returns>`, `<value>`, `<remarks>`, `<para>`, and `<list>` to caller-visible semantics, obligations, rails, resources, security, interop, and lifecycle.
- The current C# capsule already rejects unresolved `cref`, fake `<exception>` tags for typed rails, generated-member catalogs, `<include>` files without maintained source, and lifecycle wrappers for internal surfaces that should be deleted or replaced.
- `Directory.Build.props` sets `AnalysisLevel` to `latest-all`, `TreatWarningsAsErrors` to `true`, and `GenerateDocumentationFile` to `true`.
- `Directory.Build.props` suppresses `CS1591`, so Rasm intentionally rejects ecosystem-style missing-comment churn even though compiler XML output is enabled.
- The scan of `libs/csharp` found effectively no public XML-comment surface. That supports treating this standard as a decision rule for rare public semantic contracts, not as a blanket requirement to comment every public declaration.

## External Source Notes

Microsoft C# documentation comments:

- Microsoft states that XML documentation comments produce structured API documentation, and the compiler combines source structure with comment text into one XML document.
- Microsoft states that the compiler verifies comments against API signatures for relevant tags. It also validates well-formed XML, `param`, `typeparam`, `exception`, `include`, `see`, `seealso`, and `cref`-style references in the documented ways.
- Microsoft recommends documenting all publicly visible types and public members, says a minimum public member comment has `<summary>`, and says comments should be complete sentences. This is an ecosystem default, not automatically the Rasm local rule because Rasm suppresses `CS1591`.
- Microsoft warns that XML comments on private members can expose inner or confidential library workings. This supports Rasm's public-surface-only and security-sensitive comment stance.
- Microsoft says `<summary>` appears in Visual Studio IntelliSense and Object Browser; `<remarks>` supplements the summary; `<param>`, `<returns>`, `<value>`, `<exception>`, `<example>`, `<see>`, `<seealso>`, `<inheritdoc>`, and `<include>` each carry specific generated-reference behavior.
- Microsoft says comments cannot be applied to namespaces.
- The C# specification says documentation comments must immediately precede the type or member they annotate, including before attributes, and malformed XML generates a warning in the produced documentation file.
- The C# specification says the documentation file does not carry full type/member information and must be paired with reflection or equivalent metadata. This supports Rasm's rule that signatures own machine shape and comments own semantics.

Inherited documentation:

- Microsoft documents `<inheritdoc>` for inheriting comments from base classes, interfaces, and similar methods.
- Microsoft says Visual Studio can show inherited docs for undocumented overrides or implementations in IntelliSense without an explicit `<inheritdoc>`, but that IDE-only inheritance does not affect compiler-generated XML output.
- Microsoft recommends explicit `<inheritdoc>` or complete comments for distributed public APIs so the generated XML documentation file contains the necessary consumer documentation.
- This supports the current Rasm rule that `<inheritdoc>` applies only when inherited semantics are exact. It also suggests adding the generated-XML caveat: if generated API docs are consumed, IDE-only inherited Quick Info is not enough.

Analyzers and compiler warnings:

- `CS1591` is the compiler warning for a missing XML comment when documentation file output is enabled.
- Microsoft documentation rules support externally visible API docs through correct XML documentation comments; the only listed documentation quality rule in that page is `CA1200`.
- `CA1200` is in the Documentation category, applies to C# and Visual Basic, and is not enabled by default in .NET 10. It rejects prefixed `cref` forms because prefixes prevent compiler verification and Visual Studio refactoring support.
- This supports a narrower Rasm recommendation: keep `CS1591` suppressed, but prefer compiler-verifiable `cref` syntax and treat unresolved references as real defects when a comment exists.

Generated API documentation:

- DocFX converts XML documentation comments into rendered HTML documentation.
- DocFX can generate .NET API docs from assemblies by reflecting the assembly and reading the side-by-side XML documentation file.
- DocFX can generate metadata from `.csproj`, `.sln`, `.slnf`, and `.slnx`; the `.slnx` support line matters to Rasm because the repo uses `Workspace.slnx`.
- DocFX supports Microsoft's recommended C# XML tags and warns that Markdown inside XML comments can render poorly in consumers that do not support Markdown, such as Visual Studio IntelliSense.
- `dotnet/dotnet-api-docs` says some .NET API reference docs are maintained in assembly source comments and that edits for those APIs should update source comments, not the generated XML mirror.
- This supports Rasm's current source-truth order: source comments are the semantic owner; compiler XML and DocFX output are generated mirrors.

Exceptions:

- Microsoft exception guidance says callers should be able to assume no side effects when an exception is thrown from a method.
- Microsoft recommends handling common conditions to avoid exceptions and using predefined exception types when throwing is appropriate.
- Microsoft says argument validation exceptions should be thrown synchronously.
- For documentation style, `<exception>` should name actual thrown exception types and causes on throwing public surfaces. It should not document typed result rails, validation errors returned as data, or internal helper throws that are not part of the public contract.

Cancellation:

- Microsoft's cancellation model is cooperative: the caller passes a `CancellationToken`, the requester is distinct from the listener, and listeners respond in an appropriate and timely way.
- Microsoft says library code that provides cancelable operations should provide public methods accepting an external cancellation token.
- Microsoft says cancellation refers to operations, not objects, and `CancellationTokenSource` implements `IDisposable`.
- Public API comments should document cancellation only when it changes caller obligations or observable behavior: token ownership, whether cancellation is observed before starting work, whether cancellation propagates to child operations, whether `OperationCanceledException` can be thrown, and what cleanup/resource release happens when cancellation wins.

Resources and disposal:

- Microsoft says `Dispose` primarily releases unmanaged resources, but it also covers releasing owned `IDisposable` members, allocated memory, collection registrations, and acquired locks.
- Microsoft says a class that creates and stores an owned `IDisposable` is typically responsible for cleanup.
- Microsoft says `Dispose` should be idempotent.
- Microsoft recommends `SafeHandle` for unmanaged resources and says finalizers are only required for direct unmanaged resource ownership.
- Microsoft says `IAsyncDisposable.DisposeAsync` supports asynchronous cleanup, and that implementing async disposal without sync disposal can leak resources when consumers call only `Dispose`.
- Public API comments should document resource ownership, lifetime, idempotence, caller disposal responsibility, async cleanup, finalization caveats, and DI/lifetime ownership only when the declaration and type shape do not make those obligations obvious.

Security and data exposure:

- Microsoft secure coding guidance says code should protect and limit access to resources, especially when using or invoking code of unknown origin.
- Microsoft's BinaryFormatter security guide is the sharper current API-documentation model: it names affected types, states the unsafe behavior, distinguishes trusted-input assumptions from actual trust boundaries, and routes users to alternatives.
- Public API comments should document security and data exposure when the API crosses a trust boundary, processes untrusted input, logs or exposes sensitive data, invokes native or external code, changes tenant/auth scope, or relies on a security-sensitive resource. They should not hide secrets, exploit details, credentials, or operational attack paths in comments.

C# 14 extension members:

- Microsoft's C# 14 extension members page says extension blocks define multiple extension members for a type or an instance, including methods, properties, and operators, and can declare instance or static extensions.
- This supports the current C# capsule's note that extension blocks need receiver invariants, static or instance extension semantics, receiver type-parameter meaning, allocation/resource behavior, side effects, and failure rail where caller-visible.

## Findings

### Finding 1: Rasm should preserve selective semantic comments over blanket public-comment coverage.

Microsoft's ecosystem default says public APIs should be documented and `<summary>` is the minimum. Rasm's local build explicitly suppresses `CS1591` while still generating XML docs and treating warnings as errors. That combination means the local standard should not adopt "comment every public member" as a gate. The better Rasm rule is already present: public visibility creates a review question, and a source comment appears only when the declaration cannot express the caller-visible semantic contract.

Confidence: high.

Recommendation: no broad rewrite. Preserve `CS1591` rejection language and add, at most, a local-enforcement sentence that distinguishes generated XML output from missing-comment enforcement.

### Finding 2: The C# capsule should name the generated XML inheritance caveat.

The active standard already says `<inheritdoc>` applies only when inherited semantics are exact. Microsoft adds one important distinction: Visual Studio can display inherited docs in Quick Info without `<inheritdoc>`, but compiler-generated XML output does not get that IDE-only inheritance. A generated-reference consumer therefore needs either explicit `<inheritdoc>` or complete local documentation.

Confidence: high.

Recommendation: add a short clause to the existing `<inheritdoc>` bullet: exact inherited semantics are required, and generated XML consumers require explicit `<inheritdoc>` or complete source comments rather than relying on IDE-only inheritance.

### Finding 3: `cref` guidance should prefer compiler-verifiable syntax and mention `CA1200`.

The active standard already rejects unresolved `cref`. Microsoft and `CA1200` add a specific reason: prefixed `cref` forms can prevent compiler verification and refactoring support. `CA1200` is not enabled by default in .NET 10, so the local standard should not claim it fires unless local configuration proves it. It can still encode the practice as a C# comment rule.

Confidence: high.

Recommendation: add a narrow `cref` style sentence: use compiler-verifiable `cref` syntax without documentation-ID prefixes; mention `CA1200` only as the ecosystem rule behind the practice, not as a local active gate unless configured.

### Finding 4: Remarks should carry boundary semantics, not general background.

Microsoft frames `<remarks>` as supplemental information for a type or member. DocFX supports rendering XML comments into HTML, but it also warns that Markdown-heavy XML can be problematic for IntelliSense. Rasm's active capsule already routes invariants, generated-code behavior, resources, security/data exposure, interop, and lifecycle details into `<remarks>`.

Confidence: high.

Recommendation: keep the current `<remarks>` ownership and add no DocFX-specific Markdown pattern unless generated API docs are adopted. If tightened later, say `<remarks>` carries caller-visible boundary semantics that do not fit in `<summary>`, and avoid Markdown-heavy XML unless the configured generated renderer is the controlling consumer.

### Finding 5: Exception comments should stay tied to public throwing contracts.

Microsoft's `<exception>` tag identifies exceptions a method can throw, and compiler validation checks that the referenced exception exists. Microsoft exception guidance emphasizes predictable side effects, predefined exceptions, synchronous argument validation, and correct rethrowing. None of that requires Rasm to document typed rails as exceptions.

Confidence: high.

Recommendation: preserve the current rule that `<exception>` names actual thrown type and cause on throwing surfaces only. Add a small accepted/rejected contrast if later editors need an example.

Accepted: `<exception cref="InvalidOperationException">Thrown before any write when the builder is already committed.</exception>`
Rejected: `<exception cref="ImportFailure">Returned in the Fin&lt;ImportReceipt&gt; failure rail.</exception>`
Reason: thrown exceptions use `<exception>`; typed rail failures belong in `<returns>` or `<remarks>`.

### Finding 6: Cancellation comments need operation semantics and cleanup behavior.

Microsoft's cancellation model is cooperative and operation-scoped. The public API documentation question is not "does a parameter have type `CancellationToken`"; the question is what the caller can observe and must do. Rasm's current `<param>` bullet includes cancellation propagation, and the rail/resource section includes cancellation behavior. That is directionally correct but can be sharper.

Confidence: high.

Recommendation: add a C# cancellation sub-bullet or phrase that documents only caller-visible cancellation behavior: token propagation, pre-start cancellation, `OperationCanceledException` exposure, child operation cancellation, cleanup/finalizer behavior, and resource release on cancellation.

### Finding 7: Resource comments should state ownership and lifetime, not the dispose pattern.

Microsoft `Dispose` and `DisposeAsync` guidance is broad implementation guidance. Source comments should not paste that guidance. They should state the caller's ownership boundary: whether the API returns an owned disposable, borrows a resource, stores a disposable, requires caller disposal, cascades disposal, releases locks, or performs async cleanup.

Confidence: high.

Recommendation: keep `resource ownership` in `<returns>` and `resource scope` in `<remarks>`. Add one phrase for `IDisposable`, `IAsyncDisposable`, `SafeHandle`, locks, and DI-owned lifetime only when caller-visible.

### Finding 8: Security comments should name trust boundaries and data exposure, not exploit detail.

Microsoft security guidance and the BinaryFormatter security guide show the useful pattern: document affected surfaces, unsafe behavior, trust-boundary assumptions, and safe alternatives. For source comments, this means a public API should mention untrusted input, tenant/auth scope, sensitive logging, native interop, deserialization risk, and data exposure only where callers need that information to use the API safely.

Confidence: high.

Recommendation: preserve `security/data exposure` under `<remarks>`. Add a reject clause for secrets, credentials, exploit recipes, tenant IDs, and operational attack details in XML comments.

### Finding 9: `<include>` should remain exceptional under Rasm.

Microsoft says `<include>` is supported, and the .NET Runtime team uses it extensively. Rasm's current capsule rejects `<include>` files without maintained source. That rejection is good but could be misread as rejecting all `<include>` use.

Confidence: medium-high.

Recommendation: adjust only if needed: allow `<include>` when a maintained source file and generator path are proven; reject orphaned or unmaintained include files. Do not add include-file guidance unless Rasm actually adopts source-external XML comments.

### Finding 10: C# 14 extension-member comment guidance is current and useful.

Current Microsoft Learn C# 14 sources confirm extension blocks, instance extensions, static extensions, properties, and operators. The active C# capsule's extension-block sentence is appropriate for modern C# API docs because receiver semantics can be invisible from the generated member's call shape.

Confidence: high.

Recommendation: keep the extension-block rule. If tightened, explicitly include extension properties and operators beside methods.

## Add Recommendations

- Add a local-enforcement note in the C# capsule or validation: Rasm generates XML documentation files but suppresses `CS1591`, so missing-comment coverage is not proof of quality and comments are required only for omitted public semantics.
- Add a generated-output caveat to `<inheritdoc>`: exact inherited semantics are required, and generated XML consumers need explicit `<inheritdoc>` or complete source comments.
- Add a `cref` style sentence: prefer compiler-verifiable `cref` without documentation-ID prefixes; unresolved or unverifiable references are defects when a comment exists.
- Add a cancellation phrase: document token propagation, observable `OperationCanceledException`, cleanup, and resource release only when caller-visible.
- Add a resource phrase: document ownership, borrowed lifetime, disposal obligation, async cleanup, lock release, and DI lifetime only when the declaration does not already make the obligation obvious.
- Add a security phrase: document trust boundary, untrusted input, tenant/auth scope, sensitive log/data exposure, native/external execution, or unsafe format behavior when a caller must know it.

## Remove Recommendations

- Remove any future wording that treats `CS1591` as a local quality gate.
- Remove any future wording that implies Visual Studio inherited Quick Info proves generated XML documentation completeness.
- Remove any future broad requirement to write `<param>` for every parameter, `<returns>` for every return type, or `<exception>` for every validation helper.
- Remove Markdown-heavy XML examples unless a configured renderer is the controlling consumer and IntelliSense compatibility is intentionally secondary.
- Remove any future security-comment pattern that includes secrets, exploit steps, credential routes, tenant IDs, or operational attack details.

## Change Recommendations

- Change the existing `<inheritdoc>` sentence from "applies only when inherited semantics are exact" to a form that also says explicit `<inheritdoc>` is needed when generated XML docs must carry inherited documentation.
- Change the existing reject sentence around `<include>` only if later editors want nuance:
  - current direction: reject `<include>` files without maintained source.
  - proposed direction: permit `<include>` only with maintained source and proven generation route; reject orphaned include files.
- Change validation item "C# reference-resolution language reflects Rasm's warning-as-error and documentation-generation settings" to explicitly mention `GenerateDocumentationFile`, `TreatWarningsAsErrors`, and `CS1591` suppression if the active standard needs local proof text.

## No-Change Confirmations

- Keep source declarations as machine shape and XML comments as semantic contract.
- Keep compiler XML and DocFX output as generated mirrors, not hand-maintained catalogs.
- Keep `<summary>` short and purpose-bearing; use `<remarks>` for boundary semantics that need more room.
- Keep `<exception>` for real thrown exceptions only.
- Keep typed result, validation, and effect failures in `<returns>` or `<remarks>`, not `<exception>`.
- Keep nullable annotations as null-state truth and use comments only for domain absence, sentinel, default-value, or boundary-conversion semantics.
- Keep greenfield internal stale APIs on the delete-or-replace path instead of preserving them with documentation.

## Source List

- Microsoft Learn, XML API documentation comments: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/
- Microsoft Learn, recommended XML documentation tags: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/recommended-tags
- Microsoft Learn, C# language specification documentation comments: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/documentation-comments
- Microsoft Learn, XML documentation tutorial: https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/tutorials/xml-documentation
- Microsoft Learn, `CS1591`: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-messages/cs1591
- Microsoft Learn, documentation code-analysis rules: https://learn.microsoft.com/en-ie/dotnet/fundamentals/code-analysis/quality-rules/documentation-warnings
- Microsoft Learn, `CA1200`: https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1200
- Microsoft Learn, C# extension members: https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/extension-methods
- Microsoft Learn, C# 14 extension member declarations: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/extension
- DocFX, .NET API docs: https://dotnet.github.io/docfx/docs/dotnet-api-docs.html
- `dotnet/dotnet-api-docs` README: https://github.com/dotnet/dotnet-api-docs
- Microsoft Learn, exception best practices: https://learn.microsoft.com/en-us/dotnet/standard/exceptions/best-practices-for-exceptions
- Microsoft Learn, cancellation in managed threads: https://learn.microsoft.com/en-us/dotnet/standard/threading/cancellation-in-managed-threads
- Microsoft Learn, implement `Dispose`: https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-dispose
- Microsoft Learn, implement `DisposeAsync`: https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-disposeasync
- Microsoft Learn, secure coding guidelines: https://learn.microsoft.com/en-us/dotnet/standard/security/secure-coding-guidelines
- Microsoft Learn, BinaryFormatter security guide: https://learn.microsoft.com/en-us/dotnet/standard/serialization/binaryformatter-security-guide

## Validation

- Report file created only under `_reports/`.
- Active standards not edited.
- Current primary sources checked for drift-prone Microsoft/.NET API documentation claims.
- Local C# build posture checked through `Directory.Build.props`.
- No static, test, bridge, or generated-reference rails run because this is a research report only.
