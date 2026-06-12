# [BEDROCK] validation / fluentvalidation-law

## validator-as-owner

- `AbstractValidator<T>` is the one rule-graph owner per boundary shape: it implements `IValidator<T>` and `IEnumerable<IValidationRule>` simultaneously — the same declaration validates and enumerates its own rules, so the graph is inspectable data, not opaque behavior.
- `InlineValidator<T>` carries `public void Add<TProperty>(Func<InlineValidator<T>, IRuleBuilderOptions<T, TProperty>> ruleCreator)` — collection-initializer declaration is legal, so a `static readonly` validator field on the shape it validates is a complete allocation-once owner with zero class ceremony.
- The `Add` lambda must return `IRuleBuilderOptions` — a bare `RuleFor` with no attached rule is unrepresentable in initializer form.
- `protected virtual bool PreValidate(ValidationContext<T>, ValidationResult)` is the only pre-rule advice point: returning `false` skips every rule yet still returns (and can still throw on) failures added inside it — the seat for whole-payload precondition rejection, never per-property logic.
- A null root instance throws `InvalidOperationException` from `Validate`/`ValidateAsync` before any rule runs — the validator never models root absence; absence resolves before invocation, so the rule graph's domain is always a present raw value.
- `protected virtual void OnRuleAdded(IValidationRule<T>)` fires per `RuleFor`/`RuleForEach` registration — one base-class override applies a uniform policy (code minting, severity) to every rule of every derived validator without touching declarations.
- `RaiseValidationException` and `CreateDescriptor` are `virtual` — exception shape and metadata projection are override points on the owner, not free functions.
- Rules execute in declaration order — the rule list is positional, so class-level `Stop` semantics, dependent-fact ordering, and failure ordering in the outcome are all functions of source order; reordering declarations is a behavior change, not a formatting change.
- No transform surface exists on the rule graph: rules validate, never reshape — value pre-shaping is the materialization stage's concern, and a `Must` that mutates is a defect, not a technique.
- `IValidator.CanValidateInstancesOfType(Type)` answers assignability — the non-generic dispatch guard when validators are driven heterogeneously.
- Non-generic `IValidator.Validate(IValidationContext)` rehydrates the typed context internally — one untyped driver can run any validator against any context, which is what makes a seam-generic validation driver writable without reflection.
- `IRuleBuilder<T, out TProperty>` is covariant in the property — one extension targeting a base-typed builder applies to every derived property type, the variance that makes shared rule-pack extensions writable once.

## execution-and-context

- `Validate(instance)` and `ValidateAsync(instance, token)` build a fresh context under the global default selector; `Validate(instance, opt => ...)` builds it from a strategy — three entrypoints, one internal path.
- `ValidationContext<T>.CreateWithOptions(instance, Action<ValidationStrategy<T>>)` is the reusable-context factory when one context must be constructed once and passed explicitly.
- `RootContextData` (`IDictionary<string, object>`) is the per-run side channel: populated before the run, readable inside conditions, providers, and custom rules via the context overloads — run-scoped policy enters here, never through captured mutable fields on the validator.
- `RootContextData` flows into child-validator runs unchanged — one policy write at the seam reaches every nested validator.
- Context rehydration from the untyped contract preserves failures, root data, the throw flag, and the async flag — adapter-built contexts lose nothing crossing the generic boundary.
- The sync path wraps the async-invoked throw and rethrows it enriched with the concrete validator type — the diagnostic names the offending validator, never the call site.
- The async run loop checks cancellation between rules; every async primitive takes the `CancellationToken` at the rule site — long accumulating runs abort between rules, not mid-validator.

## cascade-algebra

- Cascade is a 2x2 policy matrix: `ClassLevelCascadeMode` (between rules) x `RuleLevelCascadeMode` (within one chain), both defaulting `Continue`.
- `Stop` at class level breaks the rule loop the moment the shared failure count grows; `Stop` at rule level abandons only the remaining validators of the failing chain.
- Class-level `Stop` is atomic per rule: the failure count is sampled before and after each whole rule, so a rule emitting several failures (a `Continue` chain, a collection rule across elements) completes its own accumulation before the between-rules break fires.
- `CascadeMode` has exactly `Continue = 0` and `Stop = 2` — the numeric gap is evidence that no third mode exists.
- Both validator-level properties are delegate-backed reads of the global defaults until explicitly assigned, and each rule captures `() => RuleLevelCascadeMode` rather than a value — cascade resolves at validation time, so global or validator-level changes apply retroactively to already-declared rules.
- `.Cascade(mode)` exists only on `IRuleBuilderInitial`/`IRuleBuilderInitialCollection` — it must be the first call in a chain.
- Dependent-step chains under `Cascade(CascadeMode.Stop)` beside independent field rules on `Continue` realize abort-vs-accumulate inside one validator with zero branching.

## conditions

- Chain-level `When`/`Unless`/`WhenAsync`/`UnlessAsync` default to `ApplyConditionTo.AllValidators`: the predicate guards every validator already attached to the chain — `NotNull().When(p).Length(1, 9)` guards only `NotNull`; guarding the adjacent validator alone requires `ApplyConditionTo.CurrentValidator`. This default is the most common silent-scope defect in rule graphs.
- Validator-level `When(predicate, action)` returns `IConditionBuilder`; `Otherwise(action)` attaches the complementary branch.
- Validator-level `Unless` is literally `When` over the negated predicate — one semantics, two spellings; no behavioral asymmetry exists between them.
- A block predicate evaluates once per root instance and is memoized on the validation context, shared by every rule in the block — side-effecting predicates observe first-call state only, and the memo survives across rule sets within one run.
- The memo keys on the instance and engages only for non-null instances — a context rehydrated without an instance re-evaluates the predicate per rule, the one path where a shared condition runs more than once.
- Every condition form has a `(T, ValidationContext<T>)` overload — context-driven conditions read `RootContextData` instead of closing over state.
- `DependentRules(Action)` runs nested rules only when the parent chain passed — the declaration-order spelling of "no cross-field facts over broken parts" inside an accumulating validator.
- `RuleForEach(...).Where(predicate)` / `.WhereAsync` filter which elements are validated at all — element applicability is rule-graph data, not an `if` inside `Must`.

## rule-sets-and-selectors

- `RuleSet("a,b; c", action)` splits the name on `,` and `;` and assigns the trimmed set to every rule declared inside — one block publishes rules into several named seam variants; all ruleset matching is `OrdinalIgnoreCase`.
- The default selector executes only rules whose set list is empty or contains `"default"` — moving a rule into a ruleset removes it from plain `Validate` silently.
- `"default"` is a reserved addressable name (`RulesetValidatorSelector.DefaultRuleSetName`); `"*"` (`WildcardRuleSetName`) selects everything.
- Strategy parts: `IncludeRuleSets(names)`, `IncludeRulesNotInRuleSet()` (appends `"default"`), `IncludeAllRuleSets()` (appends `"*"`), `IncludeProperties(strings | expressions)`, `UseCustomSelector(IValidatorSelector)`, `ThrowOnFailures()`.
- Multiple strategy parts compose through a selector whose verdict is `Any` — properties plus rulesets is a union, never an intersection; intersection requires one custom selector.
- Property selection matches exact path, dotted prefix in both directions, and indexer prefix, and normalizes literal `[]` against concrete indices — `"items[].code"` selects every element's rule.
- Inside a child-validator context a selector whose member names contain no `.` approves everything: undotted `IncludeProperties` filters top-level rules but runs entire child validators; reaching into a child requires the dotted path.
- Rules created by `Include` are approved unconditionally by both ruleset and member-name selectors; the included validator's own rules are then re-filtered individually — includes never block partial validation, and rulesets inside included validators still apply.
- `ValidationResult.RuleSetsExecuted` is the run receipt — the distinct set names actually selected (`"default"` when no ruleset selector was active); merging results through the combining constructor unions failures and receipts distinct, so executed-variant evidence survives aggregation.
- The receipt records selection, not success — an executed set name means its rules were eligible and ran, never that they passed; pass/fail lives only in the failures.

## async-law

- `ValidateAsync` flags the context async; synchronous `Validate` over a graph containing any async component (`MustAsync`, `CustomAsync`, `SetAsyncValidator`, async conditions) throws `AsyncValidatorInvokedSynchronouslyException` naming the validator type — async capability is a graph property enforced at run time, a published contract rather than an implementation detail.
- Sync rules run unchanged under `ValidateAsync` — an all-sync validator serves both paths; one async rule poisons the sync path permanently.
- Quarantining I/O rules in a ruleset does not lift the poison: the throw fires when the async component executes, so the partition that matters is per-validator, not per-ruleset — I/O-bearing rules live in validators that only ever run async.
- Async conditions count: a `WhenAsync` block or chain condition makes every guarded rule an async component even when the guarded validators are all sync — condition asynchrony poisons the sync path exactly like rule asynchrony.

## child-and-inheritance-composition

- `SetValidator` lives on `IRuleBuilder<T, TProperty>` itself in three forms — instance `(IValidator<TProperty>, params string[] ruleSets)`, provider `(Func<T, TValidator>, params string[])`, value-aware provider `(Func<T, TProperty, TValidator>, params string[])`.
- The `ruleSets` tail forwards into the child run by swapping the child's selector to a ruleset selector for exactly those names — parent selector verdicts still govern the parent rule itself.
- The child adaptor skips silently on a null property value and on a null resolved validator — `SetValidator` carries no implicit `NotNull`; required children pair `NotNull().SetValidator(...)` explicitly.
- The child adaptor's own verdict is unconditionally success: child failures land in the shared failure list but never trip rule-level cascade in the parent chain — a failing child does not stop subsequent validators on the same parent chain even under `Stop`.
- The child run shares the parent's failure list, root context data, and message formatter, and extends the property chain with the parent property's raw name — child failure paths compose mechanically, nothing re-prefixes them.
- Nullable-struct properties have dedicated `SetValidator`/`SetAsyncValidator` overloads accepting validators of the underlying value type — lifted-struct shapes need no unwrap ceremony.
- `ChildRules(v => ...)` declares an inline child validator for one property; its container records the parent's rulesets and re-applies them to nested child rules — `ChildRules` inside `RuleSet` blocks stays inside the variant at any nesting depth.
- `Include(validator)` / `Include(t => validator)` splices another validator over the same `T` as a first-class rule — trait-shaped rule packs compose by inclusion, cascade still governed by the including validator.
- `SetInheritanceValidator(v => v.Add<TDerived>(...))` configures a `PolymorphicValidator<T, TProperty>`: a per-derived-type table with instance, `Func<T, IValidator<TDerived>>`, and `Func<T, TProperty, IValidator<TDerived>>` registrations plus per-type `ruleSets` forwarding.
- Polymorphic lookup is `value.GetType()` against the table — exact runtime type, no hierarchy walk, no assignability scan: an unregistered concrete subtype, including a new leaf under a registered intermediate base, passes silently.
- A `protected` non-generic `Add(Type, IValidator, params string[])` exists for table-driven registration and guards with `CanValidateInstancesOfType` — the seat for registering a whole closed family from its case enumeration in one loop.

## custom-validators-and-built-ins

- `PropertyValidator<T, TProperty>` / `AsyncPropertyValidator<T, TProperty>` are the reusable rule unit: abstract `Name` (the identity that becomes default error code and message key), abstract `IsValid(ValidationContext<T>, TProperty)` or `IsValidAsync(..., CancellationToken)`, and `GetDefaultMessageTemplate(string errorCode)` with the `Localized(errorCode, fallbackKey)` helper.
- A recurring predicate spelled as `Must` forfeits stable code, default message, and localization in one stroke — the typed validator gets all three from `Name`.
- Validators append placeholder arguments to the context's formatter at failure time (`MinLength`/`MaxLength`/`TotalLength`, `RegularExpression`, `ComparisonValue`, `CollectionIndex`) — custom validators join the same template economy by appending named arguments before returning false.
- `Custom((value, context) => ...)` emits failures via `context.AddFailure`; the message runs through the formatter and the property path is the chain-built current path — collection and child prefixes apply without manual assembly.
- `AddFailure` has three path semantics: the message-only overload uses the current property path; the `(propertyName, message)` overload builds the path through the chain (prefixes still apply); the raw `ValidationFailure` overload adds verbatim with no path building and no formatting — the only overload where the caller owns the entire failure record.
- `Custom` returns `IRuleBuilderOptionsConditions` — message/severity/code options do not chain after it; everything must be set on the emitted failure itself.
- Null-permissive law: every built-in except `NotNull`/`NotEmpty` (and the inverse `Null`/`Empty`) passes a null value — `Length`, `Matches`, `EmailAddress`, `IsInEnum` all return true on null. A shape rule on an optional field never doubles as a presence rule; presence and shape are separately declared concerns by design.
- `NotEmpty` rejects, in order: null, whitespace-only strings, zero-`Count` `ICollection`, empty `IEnumerable`, and `default(TProperty)` under the default equality comparer — zero numerics and default-valued structs fail `NotEmpty`, making it a default-sentinel gate on value types, not merely a string/collection rule.
- `EmailAddress` defaults to the single-`@`-not-at-either-end check; the regex mode is `[Obsolete]` — email shape is intentionally weak at the boundary, with real verification owned by delivery.
- `IsInEnum` honors `[Flags]`: composed values verify against the defined bit mask and undefined combinations fail — enum admission is fail-closed including flag arithmetic; `IsEnumName(type, caseSensitive)` is the string-side equivalent.
- `Length(min, max)` treats `-1` as the unbounded-max sentinel — `MinimumLength` is `Length(min, -1)` internally, so a hand-written `Length(n, -1)` and `MinimumLength(n)` are one rule, and the bound functions (`Func<T, int>` overloads) compute both ends per instance.
- Length placeholder arguments (`{MinLength}`/`{MaxLength}`/`{TotalLength}`) append only on failure — passing values leave no formatter residue.
- A `Matches(Func<T, Regex>)` provider returning null passes silently — pattern-by-policy rules must guarantee a non-null pattern or pair an explicit presence rule on the policy itself.
- `Must` carries `(TProperty)`, `(T, TProperty)`, and `(T, TProperty, ValidationContext<T>)` arities — context-arity predicates read run policy from `RootContextData` without closures, same as conditions.
- Comparison rules (`Equal`/`NotEqual`/`LessThan`/`GreaterThan(OrEqualTo)`) take a constant or `Expression<Func<T, TProperty>>` — cross-field ordering is declared, the referenced member's display name feeds `{ComparisonValue}`, and nullable-struct overloads skip null operands.
- `Equal`/`NotEqual` accept `IEqualityComparer<TProperty>`; `InclusiveBetween`/`ExclusiveBetween` accept `IComparer<TProperty>` — comparison semantics are injectable per rule.
- `PrecisionScale(precision, scale, ignoreTrailingZeros)` covers decimal shape; `CreditCard()` ships as a built-in string shape rule; `Matches` carries `[StringSyntax("Regex")]` overloads for literal, instance-derived, and pre-built `Regex` patterns with `RegexOptions`.
- `RuleForEach` chains element-typed rules with `{CollectionIndex}` available and indexed property paths built by the chain; `OverrideIndexer` swaps the bracket text per element (keyed paths instead of ordinals).
- `ForEach` inside a `RuleFor(collection)` chain reaches element rules without a second top-level rule — collection-level rules (count, non-empty) and element rules share one chain.
- `Configure(rule => ...)` and the static `Configurable(ruleBuilder)` expose the underlying `IValidationRule<T, TProperty>` / `ICollectionRule<T, TElement>` — the low-level mutation escape hatch for policy that the fluent surface cannot express; its presence means no requirement justifies abandoning the rule graph for hand-rolled validation.

## severity-api

- Severity is three-tier (`Error`, `Warning`, `Info`); the per-failure default comes from `ValidatorOptions.Global.Severity` (initial `Error`).
- `WithSeverity` overrides per rule as a constant or providers of arity `(T)`, `(T, TProperty)`, `(T, TProperty, ValidationContext<T>)` — severity is computable from instance, value, and run context, a policy output rather than an annotation.
- The severity provider sits on the rule component beside `CustomStateProvider` and `ErrorCode` — the triple (severity, state, code) is per-component data, so one chain mints differently-classified failures per validator.

## di-discovery-and-lifetimes

- `AddValidatorsFromAssemblies` / `AddValidatorsFromAssembly` / `AddValidatorsFromAssemblyContaining(Type)` / `AddValidatorsFromAssemblyContaining<T>` share one signature tail: `ServiceLifetime lifetime = ServiceLifetime.Scoped`, `Func<AssemblyScanResult, bool> filter = null`, `bool includeInternalTypes = false`.
- One root scan with the filter as the only admission gate; `includeInternalTypes` swaps `GetExportedTypes` for `GetTypes`.
- Registration is dual per hit: `TryAddEnumerable(new ServiceDescriptor(InterfaceType, ValidatorType, lifetime))` plus `TryAdd(ServiceDescriptor(ValidatorType, ValidatorType, lifetime))` — re-scanning is idempotent, the concrete type resolves directly, and a hand registration before the scan wins the concrete slot.
- The one `lifetime` argument applies to both registrations identically — interface and concrete resolutions can never disagree on lifetime, so lifetime drift between the two slots is unrepresentable.
- Several validators for one `T` coexist: `IEnumerable<IValidator<T>>` resolves all, the single resolve yields the last registered.
- The scanner keeps non-abstract, non-generic-definition types and takes `FirstOrDefault` of the closed `IValidator<>` interfaces — a validator implementing `IValidator<A>` and `IValidator<B>` registers under one interface only, determined by interface enumeration order; one validator class per validated shape is the law that avoids the trap.
- `AssemblyScanner` is itself public and enumerable (`FindValidatorsInAssembly`/`FindValidatorsInAssemblies` returning `IEnumerable<AssemblyScanResult>` of interface type plus implementation type, with `ForEach` for registration sweeps) — the same enumeration that feeds registration feeds coverage proofs, so discovery output is reusable data, not a hidden side effect.
- `FindValidatorsInAssemblies` flattens and `Distinct()`s the type set — overlapping assembly lists cannot double-register.
- Lifetime law: a validator with no scoped constructor dependencies belongs at singleton — the rule graph is immutable after construction, property accessors compile once into a static accessor cache (a disable knob exists, not recommended), and construction is the expensive phase; the scoped default exists for ctor-injected scoped dependencies, not because validators carry per-request state.
- Graph lifetimes must be uniform across `SetValidator(instance)` edges — a singleton parent freezes a ctor-injected child instance (captive dependency); `SetValidator(provider)` re-resolves per validation and tolerates mixing.
- Child validators enter parents by constructor injection plus `SetValidator` — the DI graph mirrors the shape graph one-to-one; validator factories and service-locator lookups inside rules are rejected forms.

## global-configuration

- `ValidatorOptions.Global` is the single process-wide `ValidatorConfiguration`: cascade defaults, `Severity`, `PropertyChainSeparator` (default `.`), `LanguageManager`, `MessageFormatterFactory`, `PropertyNameResolver`, `DisplayNameResolver`, `ErrorCodeResolver`, and the four selector factories (`DefaultValidatorSelectorFactory`, `MemberNameValidatorSelectorFactory`, `RulesetValidatorSelectorFactory`, `CompositeValidatorSelectorFactory`) — every cross-cutting validation policy has exactly one mutation point.
- Ordering hazard split: cascade and severity defaults are read at validation time (safe to set anytime), but `PropertyNameResolver`/`DisplayNameResolver` run at `RuleFor` declaration time — name policy set after any validator constructs has already missed those rules; globals are composition-root, before-first-validator configuration.
- Selector factories make partial-validation policy swappable system-wide — a custom default selector installs once and governs every bare `Validate` call.
- The global is mutable process state with no synchronization — write-once at the composition root is the only safe protocol; concurrent mutation during validation races against delegate-backed reads.

## divergent

- composition-depth — the everything-is-rule-graph-data law: one validator per boundary shape where severity (providers), applicability (conditions with per-instance memoized block predicates), variant membership (rulesets), composition (`Include` packs, child adaptors with ruleset forwarding, polymorphic tables), order dependence (`DependentRules`), and abort policy (the 2x2 cascade matrix) are all declared values on one graph; the only executable fragments are predicates and providers.
- composition-depth — rejected forms the law absorbs, one foreclosure per row:
  - hand `if` chains before mapping — the graph is the condition surface.
  - per-variant validator subclasses — rulesets are the variant axis.
  - repeated `Must` lambdas for one concept — a named `PropertyValidator` is the reuse unit and buys code, message, and localization together.
  - boolean strict-mode parameters threaded into validators — a severity provider plus a policy value reads the same fact through `RootContextData`.
  - split sibling validators for one shape — `Include` composes shared packs without inheritance.
- composition-depth — the silent-scope traps an expert misses, each rooted in the dispatch path:
  - chain conditions scope to ALL prior validators by default.
  - `Custom` terminates the options chain.
  - the composite strategy selector is a union — combining property and ruleset strategies widens, never narrows.
  - undotted property selection runs whole child validators.
  - one async rule or condition makes the validator async-only forever.
  - the polymorphic table matches exact runtime types only.
  - a failing child validator never stops the parent chain.
- di-discovery-lifetimes — the one-root-scan law: exactly one `AddValidatorsFromAssemblies` call at the composition root over the declared assembly set; the `filter` predicate is the only exclusion mechanism; manual per-validator registration lines beside a scan are drift the dedup makes invisible — the scan already found them. Growth lands as new validator classes only; discovery, registration, and resolution are all closed.
- di-discovery-lifetimes — multi-validator modality: several validators registered for one `T` (structural pack, capability pack) resolve as `IEnumerable<IValidator<T>>` and their outcomes merge through the result-combining constructor with distinct executed-set receipts — a seam fans validation across independently-owned units and still emits one outcome; prefer the `Include` spelling when packs share ownership, the enumerable spelling when packs ship from different modules.
