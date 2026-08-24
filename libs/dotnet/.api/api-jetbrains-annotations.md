# [RASM_API_JETBRAINS_ANNOTATIONS]

`JetBrains.Annotations` carries the attribute vocabulary a declaration states its analyzer contract with: nullability, implicit reachability, declaration order, purity and result consumption, resource ownership, enumeration and collection effect, assertion and termination shape, and string-payload grammar. Every attribute is `[Conditional("JETBRAINS_ANNOTATIONS")]`, so a contract reaches metadata only where the consumer defines that symbol and no runtime dependency follows. `MeansImplicitUse` and `BaseTypeRequired` lift the vocabulary onto a repo marker, which projects its law onto every declaration it marks.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `JetBrains.Annotations`
- package: `JetBrains.Annotations` (MIT)
- assembly: `JetBrains.Annotations`, attribute and enum declarations alone; `net10.0` consumers bind the `netstandard2.0` asset
- namespace: `JetBrains.Annotations`
- rail: source-analysis

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: attributes declaring a member's analyzer contract, and the flag enums their arguments select.

| [INDEX] | [SYMBOL]                                  | [TYPE_FAMILY] | [CAPABILITY]                                        |
| :-----: | :---------------------------------------- | :------------ | :-------------------------------------------------- |
|  [01]   | `CanBeNullAttribute`                      | attribute     | nullable result, parameter, or member               |
|  [02]   | `NotNullAttribute`                        | attribute     | non-null result, parameter, or member               |
|  [03]   | `ItemCanBeNullAttribute`                  | attribute     | nullable sequence item, `Task.Result`, `Lazy.Value` |
|  [04]   | `ItemNotNullAttribute`                    | attribute     | non-null sequence item, `Task.Result`, `Lazy.Value` |
|  [05]   | `ValueRangeAttribute`                     | attribute     | inclusive integral interval                         |
|  [06]   | `NonNegativeValueAttribute`               | attribute     | integral floor at zero                              |
|  [07]   | `CannotApplyEqualityOperatorAttribute`    | attribute     | `==`/`!=` refused for the declaring type            |
|  [08]   | `DefaultEqualityUsageAttribute`           | attribute     | default-equality dependence on a generic parameter  |
|  [09]   | `BaseTypeRequiredAttribute`               | attribute     | base or interface required of a marker's targets    |
|  [10]   | `PublicAPIAttribute`                      | attribute     | preserved public surface                            |
|  [11]   | `NoReorderAttribute`                      | attribute     | authored member order held against IDE reordering   |
|  [12]   | `UsedImplicitlyAttribute`                 | attribute     | reflection, generator, or host-called usage         |
|  [13]   | `MeansImplicitUseAttribute`               | attribute     | implicit-use lift onto a local attribute            |
|  [14]   | `ImplicitUseKindFlags`                    | enum          | access, assignment, instantiation kinds             |
|  [15]   | `ImplicitUseTargetFlags`                  | enum          | reach across itself, members, inheritors            |
|  [16]   | `TestSubjectAttribute`                    | attribute     | test-to-subject binding                             |
|  [17]   | `MeansTestSubjectAttribute`               | attribute     | subject binding through a base generic parameter    |
|  [18]   | `ProvidesContextAttribute`                | attribute     | canonical value site for a context type             |
|  [19]   | `PureAttribute`                           | attribute     | observable-state-free method                        |
|  [20]   | `MustUseReturnValueAttribute`             | attribute     | result the caller consumes                          |
|  [21]   | `ContractAnnotationAttribute`             | attribute     | input-to-output nullability and halt grammar        |
|  [22]   | `TerminatesProgramAttribute`              | attribute     | method that never returns control                   |
|  [23]   | `InstantHandleAttribute`                  | attribute     | delegate or sequence consumed within the call       |
|  [24]   | `RequireStaticDelegateAttribute`          | attribute     | capture-free delegate argument                      |
|  [25]   | `MustDisposeResourceAttribute`            | attribute     | disposal ownership transferred to the caller        |
|  [26]   | `HandlesResourceDisposalAttribute`        | attribute     | disposal ownership accepted at the member           |
|  [27]   | `LinqTunnelAttribute`                     | attribute     | postponed-enumeration LINQ method                   |
|  [28]   | `NoEnumerationAttribute`                  | attribute     | sequence parameter left unenumerated                |
|  [29]   | `CollectionAccessAttribute`               | attribute     | collection read and mutation effect                 |
|  [30]   | `CollectionAccessType`                    | enum          | collection effect vocabulary                        |
|  [31]   | `AssertionMethodAttribute`                | attribute     | control-flow-halting assertion helper               |
|  [32]   | `AssertionConditionAttribute`             | attribute     | the asserted parameter                              |
|  [33]   | `AssertionConditionType`                  | enum          | asserted-value vocabulary                           |
|  [34]   | `StringFormatMethodAttribute`             | attribute     | positional format-string parameter                  |
|  [35]   | `StructuredMessageTemplateAttribute`      | attribute     | named-placeholder template parameter                |
|  [36]   | `InvokerParameterNameAttribute`           | attribute     | argument matching a caller parameter name           |
|  [37]   | `ValueProviderAttribute`                  | attribute     | completion values from a named static holder        |
|  [38]   | `RegexPatternAttribute`                   | attribute     | regex pattern payload                               |
|  [39]   | `UriStringAttribute`                      | attribute     | URI string payload                                  |
|  [40]   | `LanguageInjectionAttribute`              | attribute     | foreign-language string payload                     |
|  [41]   | `InjectedLanguage`                        | enum          | injected-language vocabulary                        |
|  [42]   | `NotifyPropertyChangedInvocatorAttribute` | attribute     | `INotifyPropertyChanged` raise method               |

[ImplicitUseKindFlags]: `Access` `Assign` `InstantiatedWithFixedConstructorSignature` `InstantiatedNoFixedConstructorSignature` `Default`
[ImplicitUseTargetFlags]: `Itself` `Members` `WithMembers` `WithInheritors` `Default`
[CollectionAccessType]: `None` `Read` `ModifyExistingContent` `UpdatedContent`
[AssertionConditionType]: `IS_TRUE` `IS_FALSE` `IS_NULL` `IS_NOT_NULL`
[InjectedLanguage]: `CSS` `HTML` `JAVASCRIPT` `JSON` `XML`

- `ImplicitUseKindFlags.Default`: `Access | Assign | InstantiatedWithFixedConstructorSignature`; `ImplicitUseTargetFlags.Default` is `Itself` and `WithMembers` is `Itself | Members`.
- `PublicAPIAttribute`: itself carries `[MeansImplicitUse(ImplicitUseTargetFlags.WithMembers)]`, so one application reaches the type and every member.
- `PureAttribute`: collides with `System.Diagnostics.Contracts.PureAttribute` wherever both namespaces are imported; the application qualifies or aliases.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: argument contracts an application binds — positional constructor overloads and the named properties an attribute exposes.

| [INDEX] | [SURFACE]                                                          | [SHAPE]  | [CAPABILITY]                             |
| :-----: | :----------------------------------------------------------------- | :------- | :--------------------------------------- |
|  [01]   | `[PublicAPI(string)]`                                              | ctor     | comment carried on the preserved surface |
|  [02]   | `[UsedImplicitly(ImplicitUseKindFlags)]`                           | ctor     | narrow the usage kind                    |
|  [03]   | `[UsedImplicitly(ImplicitUseTargetFlags)]`                         | ctor     | widen the reachable target               |
|  [04]   | `[UsedImplicitly(ImplicitUseKindFlags, ImplicitUseTargetFlags)]`   | ctor     | kind and target together                 |
|  [05]   | `[UsedImplicitly(Reason = string)]`                                | property | record the binding host                  |
|  [06]   | `[MeansImplicitUse(ImplicitUseKindFlags, ImplicitUseTargetFlags)]` | ctor     | lift kind and target onto a marker       |
|  [07]   | `[BaseTypeRequired(Type)]`                                         | ctor     | constrain a marker's target types        |
|  [08]   | `[ContractAnnotation(string)]`                                     | ctor     | nullability and halt grammar             |
|  [09]   | `[ContractAnnotation(string, bool)]`                               | ctor     | force full state coverage                |
|  [10]   | `[MustDisposeResource(bool)]`                                      | ctor     | clear ownership transfer on an overload  |
|  [11]   | `[MustUseReturnValue(string)]`                                     | ctor     | justification shown in the diagnostic    |
|  [12]   | `[MustUseReturnValue(IsFluentBuilderMethod = bool)]`               | property | fluent-builder chain member              |
|  [13]   | `[InstantHandle(RequireAwait = bool)]`                             | property | bind only under an `await` expression    |
|  [14]   | `[RequireStaticDelegate(IsError = bool)]`                          | property | escalate a capture to an error           |
|  [15]   | `[CollectionAccess(CollectionAccessType)]`                         | ctor     | declare the collection effect            |
|  [16]   | `[AssertionCondition(AssertionConditionType)]`                     | ctor     | select the asserted value                |
|  [17]   | `[ValueRange(long, long)]`                                         | ctor     | inclusive interval                       |
|  [18]   | `[ValueRange(long)]`                                               | ctor     | single admitted value                    |
|  [19]   | `[StringFormatMethod(string)]`                                     | ctor     | name the format parameter                |
|  [20]   | `[ValueProvider(string)]`                                          | ctor     | name the static value holder             |
|  [21]   | `[NotifyPropertyChangedInvocator(string)]`                         | ctor     | name the property-name parameter         |
|  [22]   | `[UriString(string)]`                                              | ctor     | name the HTTP verb the URI answers       |
|  [23]   | `[LanguageInjection(InjectedLanguage)]`                            | ctor     | select a known language                  |
|  [24]   | `[LanguageInjection(string)]`                                      | ctor     | name a language outside the enum         |
|  [25]   | `[LanguageInjection(Prefix = string)]`                             | property | literal prefix framing the fragment      |
|  [26]   | `[LanguageInjection(Suffix = string)]`                             | property | literal suffix framing the fragment      |
|  [27]   | `[TestSubject(Type)]`                                              | ctor     | bind the tested type                     |

- `UsedImplicitly`, `MeansImplicitUse`: a parameterless application takes `ImplicitUseKindFlags.Default` with `ImplicitUseTargetFlags.Default`.
- `ValueRange`: `ulong` overloads mirror both `long` forms for unsigned domains, and the attribute repeats for non-intersecting intervals.
- `MustDisposeResource`: reaches a constructor through its declaring type's annotation, never through the base constructor it delegates to.
- `NoReorder`: the IDE honors it only where a member-reordering pattern names the attribute, so a load-bearing order states itself at the declaration rather than in a comment.
- `TerminatesProgram`: states unconditional termination to the analyzer where the inbox `[DoesNotReturn]` states it to nullable flow analysis, so a fail-fast entry carries both.
- `UriString`: the parameterless form marks any URI payload; the `string` form names the HTTP verb an endpoint route answers.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Each attribute's `AttributeUsage` fixes the declaration kinds it binds, so a mis-targeted contract fails at compile time.
- `MeansImplicitUse` and `BaseTypeRequired` are the lift seam: a repo marker carrying either projects its law onto every declaration the marker marks, and one lift replaces per-declaration annotation.
- `ContractAnnotation` states input-to-output nullability and termination as a grammar string, so a guard method's flow effect reads at the declaration rather than through the body.

[STACKING]:
- `Thinktecture.Runtime.Extensions`(`.api/api-thinktecture-runtime-extensions.md`): its assembly re-exports the `JetBrains.Annotations` namespace, so a generated `[Union]`, `[SmartEnum<TKey>]`, or `[ValueObject<T>]` member and its hand-written partial annotate against one attribute identity.
- `Generator.Equals`(`.api/api-generator-equals.md`): `[DefaultEqualityUsage]` marks the generic parameter whose element equality a generated comparer consumes, so a struct without an equality override surfaces at the call site instead of comparing by field layout.
- `Riok.Mapperly`(`.api/api-mapperly.md`): generated mapping methods reach the `partial` declaration's members with no hand-written call, so a mapped DTO carries `[PublicAPI]` and a mapper-only member carries `[UsedImplicitly]`.
- `LanguageExt.Core`(`.api/api-languageext.md`): a discarded `Fin<A>`, `Validation<F, A>`, or `Option<A>` loses the rail silently, so a rail-returning combinator carries `[MustUseReturnValue]`, a projection over one carries `[Pure]`, and a `[LinqTunnel]` method propagates `[InstantHandle]` inference down the chain.
- within-library: a native or `IDisposable` owner pairs `[MustDisposeResource]` at the factory with `[HandlesResourceDisposal]` at the field, property, or parameter that accepts the ownership, and `[RequireStaticDelegate]` closes the allocation path on a per-frame callback parameter.

[LOCAL_ADMISSION]:
- `[PublicAPI]` marks every public or generator-reachable declaration, and `[UsedImplicitly]` marks a member only a host, reflection lane, or generator calls, its kind and target flags naming that reach.
- `[MeansImplicitUse]` lifts reachability onto a repo-owned marker attribute and `[BaseTypeRequired(Type)]` states that marker's target contract.
- `[MustDisposeResource]` marks a factory returning an owned resource, and `[HandlesResourceDisposal]` marks the field, property, or parameter accepting that ownership.
- `[Pure]` or `[MustUseReturnValue]` marks every rail combinator, while `[InstantHandle]` or `[NoEnumeration]` fixes a sequence or delegate parameter's consumption point.
- `[RegexPattern]`, `[UriString]`, `[LanguageInjection]`, `[StringFormatMethod]`, and `[StructuredMessageTemplate]` declare a string parameter's foreign grammar at that parameter.
- `[NoReorder]` marks every owner whose declaration order is load-bearing — generated cases, dispatch rows, migration steps — and `[TerminatesProgram]` marks the fail-fast entry that never hands control back.

[RAIL_LAW]:
- Package: `JetBrains.Annotations`
- Owns: the compile-time contract vocabulary for nullability, reachability, declaration order, purity, resource ownership, enumeration and collection effect, assertion and termination shape, and string-payload grammar.
- Accept: contracts declared at the owning member and lifted onto repo markers through `[MeansImplicitUse]` and `[BaseTypeRequired]`, over one shared package identity.
- Reject: a local mirror of an attribute this package declares, and a suppression comment standing where an attribute states the contract.
