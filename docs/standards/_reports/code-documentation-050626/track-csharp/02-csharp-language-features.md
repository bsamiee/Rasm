# [CSHARP_02_LANGUAGE_FEATURES]

This report records current C# language-feature evidence for the `code-documentation.md` C# capsule. It does not edit active standards; it isolates current-source findings and the standard-ready implications for source XML documentation in C# 14 and .NET 10.

## [1][SCOPE]

Workspace: `/Users/bardiasamiee/Documents/99.Github/Rasm`
Assigned output: `docs/standards/_reports/code-documentation-050626/track-csharp/02-csharp-language-features.md`
Active standards edited: no
Research date: 2026-06-05
Primary source family: Microsoft Learn C# language reference, programming guide, feature specifications, and XML documentation pages.
Context7 cross-check: `/websites/learn_microsoft_en-us_dotnet` query for C# extension blocks, XML comments, records, primary constructors, nullable attributes, required members, and collection expressions.

Read scope:
- `CLAUDE.md`
- root `AGENTS.md` from the prompt context
- `docs/standards/AGENTS.md`
- `docs/standards/README.md`
- `docs/standards/reference/code-documentation.md`
- `docs/standards/information-structure.md`
- `docs/standards/style-guide.md`
- `docs/standards/proof.md`
- `docs/standards/formatting.md`

## [2][SOURCE_MAP]

Microsoft Learn is the controlling upstream source for these language-documentation facts because it is the maintained documentation for C# language behavior and XML documentation comments. Feature specifications remain useful where the final reference page does not yet expose every metadata or analysis detail, but the standard should cite the language reference or programming guide first when both cover the same behavior.

[CURRENT_PRIMARY]:
- C# 14 and .NET 10 support: [What's new in C# 14](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14). Evidence: C# 14 is listed as supported on .NET 10, and extension members are listed among the release features. Last verified: 2026-06-05. Review trigger: C# language version or .NET support page changes.
- Extension block reference: [Extension declaration](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/extension). Evidence: top-level nongeneric static classes may declare extension blocks; the page names XML-comment behavior for extension blocks. Last verified: 2026-06-05. Review trigger: extension keyword page or compiler XML generation behavior changes.
- Extension programming guide: [Extension members](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/extension-methods). Evidence: extension blocks support instance and static extensions, methods, properties, and operators; extension lookup does not override real type members. Last verified: 2026-06-05. Review trigger: extension binding or extension-member scope rules change.
- XML comment examples: [Example XML documentation comments](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/examples). Evidence: extension-block XML nodes are copied to members, and positional records use `<param>` for primary-constructor parameters while generated positional properties lack direct XML comments in current docs. Last verified: 2026-06-05. Review trigger: XML examples page or compiler generated-member documentation changes.
- XML comment reference: [Generate XML API documentation comments](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/) and [Recommended XML tags](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/recommended-tags). Evidence: compiler XML generation combines code structure with comments, validates relevant tags against signatures, and supports `<param>`, `<typeparam>`, `<inheritdoc>`, `cref`, and `href` rules. Last verified: 2026-06-05. Review trigger: compiler XML validation or recommended tag list changes.
- Nullable attributes: [Attributes interpreted by the compiler: Nullable static analysis](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/attributes/nullable-analysis). Evidence: nullable attributes refine compiler null-state preconditions and postconditions and improve caller warnings without adding implementation checks. Last verified: 2026-06-05. Review trigger: nullable attribute vocabulary or compiler analysis rules change.
- Required members: [required modifier](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/required) and [Required members feature specification](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-11.0/required-members). Evidence: `required` creates object-initializer obligations, uses `SetsRequiredMembersAttribute` for constructor contracts, and affects nullable analysis. Last verified: 2026-06-05. Review trigger: required-member reference or compiler metadata rules change.
- Records and primary constructors: [C# record types](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/types/records) and [What's new in C# 12](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-12). Evidence: records generate equality, `ToString`, copy semantics, and positional properties; primary constructors apply to classes and structs, but only records synthesize public properties from primary-constructor parameters. Last verified: 2026-06-05. Review trigger: record or primary-constructor docs change.
- Collection expressions: [Collection expressions](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/collection-expressions) and [What's new in C# 12](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-12). Evidence: collection expressions target many collection shapes, may realize `IEnumerable<T>` as an in-memory collection, and require well-behaved custom collection types for defined behavior. Last verified: 2026-06-05. Review trigger: collection expression conversion, builder, or well-behaved collection rules change.

## [3][FINDINGS]

### [3.1][EXTENSION_BLOCKS]

Current fact: C# 14 extension blocks can declare extension methods, properties, and operators inside top-level nongeneric static classes. They can extend instances or the type itself, and consumers call them as apparent instance or static members.

Documentation implication: the extension block itself is now a documentation-bearing surface. Current Microsoft examples put XML comments on the `extension(...)` block to describe the receiver and type parameters, then put member-specific comments on each extension member. The compiler copies XML nodes from the extension block to every member declared in that block, so block-level comments must be true for every member in the block.

Standard-ready rule: document receiver semantics at the extension block only when they apply to all members in the block. Put member result, failure, side-effect, laziness, allocation, resource, and operator semantics on the individual extension member. Split an extension block when one receiver-level comment would become false or misleading after compiler copying.

Standard-ready rule: use `<param>` on the extension block for a named receiver parameter and `<typeparam>` for extension-block type parameters only when their semantic role is not already obvious from constraints and receiver type. Use `<param>` on static extension operators for ordinary operands, and use `<paramref>` only for actual parameter references in prose.

Standard-ready rejection: do not use extension-member comments to imply overriding, privileged access, or modified encapsulation. The programming guide states that extension members do not override real members and do not bypass access rules, so comments may mention binding priority only where caller-visible ambiguity is the surface contract.

Proof gap: current docs state preferred XML style and copying behavior, but the report did not run a local Roslyn XML-output proof for Rasm. If the active standard adds a precise generated XML shape for extension blocks, run a minimal compiler XML output check or cite a checked-in generated artifact.

### [3.2][RECORDS_PRIMARY_CONSTRUCTORS]

Current fact: records are data-focused `class` or `struct` forms that synthesize value equality, formatted `ToString`, and nondestructive copy behavior. Positional record parameters define the constructor and generated properties; `record class` positional properties are `init`-only, while `record struct` positional properties are read-write unless the record struct is `readonly`.

Documentation implication: record declarations carry more machine shape than ordinary classes. XML comments should not restate value equality or property generation when that follows directly from `record`; comments should state domain equality consequences, copy invariants, normalization, validation, or invalid-state prevention when those semantics affect callers.

Current fact: C# 12 primary constructors are available on any `class` or `struct`. Their parameters are in scope throughout the type body, explicit constructors must chain through the primary constructor, and adding a primary constructor to a class prevents the implicit parameterless constructor. Only records synthesize public properties from primary-constructor parameters.

Documentation implication: non-record primary-constructor parameters are construction parameters, not public properties. A comment should not describe them as stable object state unless the type exposes or stores them through an explicit member. If a primary constructor normalizes, captures, disposes, validates, or aliases a dependency in a caller-visible way, the comment belongs on the type or constructor contract.

Current XML-comment fact: Microsoft XML examples state that generated properties for positional records do not currently have a direct XML-comment location, and that `<param>` describes the primary-constructor parameters. This matters because a generated reference may show constructor-parameter semantics but not independent property comments for the synthesized properties.

Standard-ready rule: for positional records, use `<param>` to document semantic parameter obligations that the signature cannot express. Add explicit properties only when the property itself needs a distinct public contract that cannot live on the positional parameter. Do not add duplicate comments to fight generated property behavior.

Standard-ready rule: record comments should document equality and copy semantics only when the domain deviates from "same component values mean equal" or when shallow copy, collection identity, normalization, or generated `with` behavior creates an observable caller obligation.

Proof gap: current Microsoft examples explicitly cover positional records; this report did not find an equally direct Microsoft page for XML comments on non-record primary constructors. Treat non-record primary-constructor XML shape as a local compiler-output proof item before adding a standard sentence that names exact generated XML for that case.

### [3.3][NULLABLE_ATTRIBUTES]

Current fact: nullable reference annotations and `System.Diagnostics.CodeAnalysis` attributes feed compiler static analysis. Attributes such as `AllowNull`, `DisallowNull`, `MaybeNull`, `NotNull`, `MaybeNullWhen`, `NotNullWhen`, `NotNullIfNotNull`, `MemberNotNull`, `MemberNotNullWhen`, `DoesNotReturn`, and `DoesNotReturnIf` describe null-state preconditions, postconditions, member initialization, and terminating control flow for callers and analyzers.

Documentation implication: nullability is machine shape. Comments should not restate `string?`, `T?`, or nullable attributes. Comments should add only domain absence semantics, sentinel meaning, default-value pitfalls, interop null conversion, null propagation, or a boundary condition the annotation cannot carry.

Standard-ready rule: if a `Try*` API uses `[NotNullWhen(true)]`, the XML comment should not repeat the null-state guarantee alone. It should state the success meaning of the `true` branch and the domain meaning of failure or absence. If the failure branch carries no domain detail beyond "not found," the declaration and attribute may be sufficient.

Standard-ready rule: use nullable attributes before prose when the caller-visible fact is a null-state fact. Use `<remarks>` only for facts attributes cannot encode, such as native interop returning null for a host-specific sentinel, or a `null` return that intentionally preserves caller fallback rather than indicating failure.

Standard-ready rejection: do not use XML comments as a substitute for nullable annotations or attributes. The nullable attributes page states that attributes improve caller warnings but do not add implementation checks, so comments cannot be treated as analysis enforcement.

Proof gap: nullable attributes are stable but still compiler-sensitive. If the active standard lists attribute names, keep them tied to the Microsoft nullable-analysis page rather than copying long attribute semantics into the standard.

### [3.4][REQUIRED_MEMBERS]

Current fact: `required` on a field or property makes object creation responsible for initializing that member unless the chosen constructor is attributed with `SetsRequiredMembersAttribute`. Required-member metadata includes compiler-recognized attributes, and nullable analysis treats required members specially at constructor boundaries.

Documentation implication: `required` is a machine-enforced initialization contract, so XML comments should not say "required" merely because the keyword already does. Comments should state the required member's domain meaning, valid range, normalization requirement, or invalid default hazard.

Standard-ready rule: document a `required` member with `<value>` only when the property value has caller-visible semantics beyond "must be initialized." For constructors marked `[SetsRequiredMembers]`, document the constructor's initialization guarantee only if callers rely on it to skip an object initializer or if the constructor supplies a domain-valid value.

Standard-ready rule: preserve the required-plus-nullability distinction. A required nullable member can require explicit initialization while still permitting `null`; a required non-nullable member can defer constructor initialization to the object initializer. Comments should state domain absence only where the required modifier and nullability cannot express it.

Standard-ready warning: `struct default` behavior can bypass required-member enforcement for default-created values according to the feature specification. Mention this in source comments only when the public struct's default value is observable and invalid, and prefer a type-level invariant or factory surface when possible.

Proof gap: the required-member feature spec contains metadata and nullable-analysis details that are more precise than the short keyword reference. If the standard mentions metadata attributes, cite the feature spec as the source and mark the claim as feature-spec-backed.

### [3.5][COLLECTION_EXPRESSIONS]

Current fact: collection expressions can target arrays, spans, many collection initializer types, collection-builder types, and several collection interfaces. A collection expression converted to `IEnumerable<T>` still creates a collection containing all elements, so it is not LINQ-style deferred generation.

Documentation implication: collection-expression syntax is usually implementation shape, not source-comment content. Comments should not explain `[a, b, ..c]` syntax. Comments may need to state materialization, ordering, allocation, safe-context, or builder invariants when a public API receives, stores, or returns a collection whose behavior is not obvious from its type.

Current fact: custom collection-builder support assumes well-behaved collection semantics. Microsoft's reference warns that behavior is undefined if a custom collection type is not well behaved for collection expressions.

Standard-ready rule: if Rasm exposes a custom collection type meant for collection expressions, document the type-level invariant that `Count` or `Length`, enumeration, and range-add behavior agree. Prefer a generated or source-level API reference for the collection builder contract rather than repeating the syntax in every consuming member.

Standard-ready rejection: do not add XML comments to local collection expressions or comments that teach C# syntax. Document only caller-visible materialization, ordering, allocation, mutation, or custom-builder behavior.

Proof gap: no Rasm custom collection-expression opt-in was inspected for this report. Treat collection expressions as a low-priority capsule addition unless a public custom collection builder exists or planned source changes introduce one.

## [4][CAPSULE_IMPLICATIONS]

The current C# capsule in `code-documentation.md` is directionally aligned with Microsoft Learn: XML documentation comments are the C# source-comment truth, compiler XML is the generated mirror, and comments should carry caller semantics rather than type echo. The strongest refinement opportunity is to sharpen the C# 14 extension-block rule because Microsoft now documents a specific XML-comment copying behavior for `extension` blocks.

Recommended additions to consider in the active standard:
- Extension blocks: state that XML nodes on the `extension` block are copied to every member declared in the block, so block comments describe only receiver-wide semantics.
- Extension members: state that instance and static extension members, extension properties, and extension operators need member-local comments only for semantic result, failure, side effect, laziness, resource, or allocation facts.
- Records: state that positional records use `<param>` for primary-constructor parameter semantics, and generated positional properties do not currently have a direct XML-comment target in Microsoft examples.
- Primary constructors: state that non-record primary-constructor parameters are not generated public properties; comments must not document them as properties unless explicit source exposes them.
- Nullable attributes: state that null-state attributes own compiler-visible null contracts, while comments own domain absence and boundary conversion semantics.
- Required members: state that `required` owns initialization obligation, while `<value>` owns domain value semantics and default-value hazards.
- Collection expressions: state that collection-expression syntax rarely belongs in comments; public custom collection-builder types document well-behaved collection invariants when caller-visible.

Recommended rejections to consider in the active standard:
- Receiver comments copied from an extension block that mention one member's result, failure, or side effect.
- `<param>` text that restates a primary-constructor parameter name or generated record property.
- Nullable XML prose that should be a nullable annotation or compiler-recognized attribute.
- Required-member comments that say only "must be set."
- Collection-expression comments that teach syntax rather than public materialization or builder behavior.

## [5][STANDARD_DELTA_CANDIDATE]

The following candidate wording is intentionally not applied to the active standard in this task.

```markdown template
C# 14 extension blocks: XML comments on an `extension` block describe the receiver and block type parameters, and the compiler copies those XML nodes to every member declared in the block. Put only receiver-wide semantics on the block. Put member-specific result, failure, side-effect, laziness, allocation, resource, operator, and property value semantics on the individual extension member. Split the block when one receiver comment would become false for any member after copying.

Records and primary constructors: positional records use `<param>` for primary-constructor parameter semantics; current Microsoft examples do not expose a direct XML-comment target for generated positional properties. Non-record primary-constructor parameters are construction parameters, not generated public properties, so comments must not describe them as stable public state unless explicit members expose that state.

Nullable and required contracts: nullable annotations, nullable analysis attributes, and `required` own machine-checkable null-state and initialization obligations. XML comments add only domain absence, sentinel, default-value, normalization, boundary-conversion, and value semantics that those declarations cannot express.
```

## [6][OPEN_CHECKS]

- [ ] Run a local minimal Roslyn XML-output check before publishing exact generated XML member names or compiler-output shape for C# 14 extension blocks.
- [ ] Run or cite a local compiler-output check before publishing exact XML behavior for non-record primary constructors.
- [ ] Search Rasm public C# surfaces for custom collection-builder opt-in before adding collection-expression-specific requirements to the active C# capsule.
- [ ] If `code-documentation.md` is edited later, run `git diff --check -- docs/standards/reference/code-documentation.md` and local path or anchor validation when links or headings change.

## [7][TRANSCRIPT]

Local governing read:
- `sed -n '1,240p' CLAUDE.md`
- prompt-provided root `AGENTS.md`
- `sed -n '1,260p' docs/standards/README.md`
- `sed -n '1,260p' docs/standards/AGENTS.md`
- `sed -n '1,260p' docs/standards/reference/code-documentation.md`
- `sed -n '220,520p' docs/standards/reference/code-documentation.md`
- `sed -n '1,260p' `
- `sed -n '1,300p' docs/standards/information-structure.md`
- `sed -n '301,520p' docs/standards/information-structure.md`
- `sed -n '1,300p' docs/standards/style-guide.md`
- `sed -n '1,300p' docs/standards/proof.md`
- `sed -n '1,260p' docs/standards/formatting.md`
- `fd -H . docs/standards -t f -e md | sort`

Current-source research:
- Web search and open against Microsoft Learn for C# 14, extension declarations, extension members, XML comments, XML examples, nullable attributes, required members, records, primary constructors, and collection expressions.
- Context7 library resolution selected `/websites/learn_microsoft_en-us_dotnet`.
- Context7 query confirmed the same Microsoft Learn surfaces for extension-block XML comments, positional records, and C# XML comment examples.

## [8][VALIDATION]

- [ ] Active standards remain unchanged.
- [ ] Source claims cite maintained Microsoft Learn pages or mark a proof gap.
- [ ] Recommendations stay in `_reports/` and are not presented as applied standard text.
- [ ] Drift-prone C# 14 and .NET 10 claims carry `Last verified: 2026-06-05`.
- [ ] Future active-standard edits rerun docs-only validation on the changed active path.
