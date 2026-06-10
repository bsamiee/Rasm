# Generated Value Objects — Frontier Surfaces

[DERIVATION_VISIBILITY]:
- Derived surfaces are emitted as one satellite declaration per capability — factory/identity core, then separate partials for `IComparable`, comparison operators, equality operators, each arithmetic family, `IFormattable`, `IParsable`, `ISpanParsable` — and each satellite exists only when the generator can see the key's corresponding capability on the key type's declared (not generated) surface.
- That visibility test has a structural blind spot: source generators cannot observe other generated output, including their own. A keyed owner whose key is itself a generated owner in the same compilation derives only the core — factories, equality, conversions, metadata — and silently loses comparison, parsing, formatting, and arithmetic, even though the inner owner demonstrably implements all of them after generation. The identical declaration with the inner owner in a referenced assembly derives the full surface, because compiled metadata is visible. Layering decision: owner-keyed owners reach maximal derivation only across an assembly boundary; composing them intra-assembly is a capability downgrade with no diagnostic.
- The blindness extends to nullability inference: the generated `ToString()` delegates to the key, and for a generated-owner key the generator resolves only `object.ToString()`, emitting `public override string? ToString()` — the nullable-annotated override is a visible fingerprint of an under-derived owner.
- Conversion operators do not compose transitively: an owner keyed by another owner gets exactly one hop in each direction; no outer→raw operator is manufactured through the chain, so double-cast spelling at call sites is the designed shape.
- The component-model converter on such an owner is closed over the inner owner as key — converter-driven binding chains through the inner owner's own converter, so string-shaped input crosses two admissions to reach the outer type.

[GENERIC_MATH_ADMISSION]:
- Every generated arithmetic family declares its generic-math interface on the owner — `IAdditionOperators<T,T,T>` through `IDivisionOperators<T,T,T>`, with `checked` variants satisfied inside the same interface — so admitted owners flow directly into operator-constrained generic algorithms. No identity-element interface is ever generated — no additive or multiplicative identity, no min-max carrier, no full numeric interface — because an identity element would be an unvalidated constant minted outside admission. Generic kernels over owners must take their seeds and bounds as admitted parameters; an algorithm written against a full-number constraint excludes owners by construction, and relaxing it to the operator families is the owner-compatible spelling.
- Comparison operators can exceed the key's own algebra: a string-keyed owner with a comparer accessor implements the full comparison-operator interface by routing `<` `<=` `>` `>=` through the accessor's `Comparer.Compare` — the owner becomes more generic-math-capable than its key, which has no relational operators at all. Policy attributes do not merely configure comparison; they synthesize operator algebra the key never had.
- The operators-generation vocabulary makes generation-on the zero state: the `None` member is `-1` and the `Default` member is `0`, so an unset attribute property means "generate" and opting out is an explicit negative assignment — tooling that reads attribute data and treats zero as "off" inverts the semantics.
- The equality/comparison pairing is self-healing at the attribute surface: the equality-operators property getter returns the maximum of itself and the comparison-operators setting, so requesting key-type overloads for comparison transparently grants them for equality in the generated output — while the divergence diagnostic still warns on the explicit spelling, demanding the two settings be written identically even where the clamp already reconciles them.

[COMPARER_TYPECLASS_SURFACE]:
- The accessor contract is one static abstract property per concern — `IEqualityComparerAccessor<T>.EqualityComparer` and `IComparerAccessor<T>.Comparer` — and generated code references the accessor statically at every comparison site: comparer identity travels as a type argument with zero instance plumbing, and a custom accessor is a one-property class. The shipped projection comparer (selector plus optional inner comparer) makes derived-field and collection-member semantics a one-line accessor body rather than a hand-written comparer class.
- Accessor/member type agreement is a compile error; on violation the generator still threads the mismatched accessor into `Equals`/`GetHashCode`, so the genuine diagnostic is followed by a type-mismatch cascade inside generated code — triage always starts from the first analyzer error, never from the generated-file errors below it.
- The generic default accessor closes the pairing rule for non-string keys: when an ordering accessor is required but only default equality is wanted (or vice versa), the default accessor satisfies the paired-policy diagnostics without inventing a comparer.
- Container-level identity override is a shipped surface: a generic comparer over any owner convertible to string exposes six prebuilt static instances (ordinal, ordinal-ignore-case, and both culture pairs) that compare owners by egressed key under the container's chosen policy — a case-sensitive set of case-insensitive owners, or a culture-ordered index over ordinal owners, without re-keying or touching the owner's intrinsic policy. It rides the universal explicit-interface egress, so it works for every string-keyed generated owner uniformly.

[COMPLEX_MEMBER_REIFICATION]:
- The reified member list on complex-owner metadata is the full assignable set — every stored factory member, extracted from the anonymous-type projection expression — not the equality-filtered set. Equality membership filters `Equals`, `GetHashCode`, and `ToString` only; metadata-driven frameworks binding through the member list see equality-ignored members too. The two views diverge by design: identity is opt-in, materialization is total.
- Complex admission is fail-first single-error by construction: the generated `Validate` returns on the first null reference member (with a per-member minted message), then runs the single hook, which can surface exactly one error object. Per-member error accumulation belongs inside the hook — it is the only point that sees all members simultaneously, post-null-gate and pre-capture — and the natural shape is an applicative sweep folded into one composite error:

```csharp
static partial void ValidateFactoryArguments(ref BundleError? validationError, ref string name, ref string code)
{
   name = name.Trim();
   code = code.Trim();
   var faults = Seq(
      name.Length is 0 ? Some("<message-a>") : None,
      code.Length > 12 ? Some("<message-b>") : None).Somes();
   validationError = faults.IsEmpty ? null : BundleError.Create(string.Join("; ", faults));
}
```

- A custom error type rewires the framework's own gates, not just the hook: the generated per-member null-rejection messages are minted through `TError.Create`, so a structured error type that parses its message string must tolerate the framework's prose grammar on those paths — or model itself as message-carrying with structure added only by its own factory call sites.
- The shipped default error type is value-semantic — equatable by message with message-returning `ToString` — so accumulated error collections deduplicate and compare by message content out of the box.

[CUSTOM_GRAMMAR_OWNERS]:
- Skipping factory methods while declaring a string object factory composes into a fully custom admission grammar on a generated chassis: the key-typed triad, the component-model converter, the key→owner cast, and key-typed parsing all vanish; the hand-written `Validate` over the foreign shape becomes the sole admission; and the object-factory weave contributes the factory interface, the egress obligation, the factory metadata row — and regenerates `IParsable` over the custom grammar, with `Parse`/`TryParse` dispatching into the hand-written `Validate` through the static-abstract invoker. Host parameter binding via the parsable interface is thereby restored after the master cut removed its key-typed form:

```csharp
[ValueObject<long>(SkipFactoryMethods = true)]
[ObjectFactory<string>(UseForSerialization = SerializationFrameworks.All)]
public partial class Ticket
{
   public static ValidationError? Validate(string? value, IFormatProvider? provider, out Ticket? item)
   {
      item = long.TryParse(value, provider, out var parsed) && parsed > 0 ? new Ticket(parsed) : null;
      return item is null ? ValidationError.Create("<message-a>") : null;
   }

   public string ToValue() => _value.ToString(System.Globalization.CultureInfo.InvariantCulture);
}
```

- The surviving surface matrix under the master cut is principled: equality, comparison, formatting, and owner→key egress conversions remain because none depends on a factory; everything that constructs from the key is gone. The owner still implements the keyed-object and convertible interfaces over its key type, so generic egress code is unaffected by the admission grammar swap.
- The required custom-factory signature and the egress obligation are enforced as errors with exact shapes — static `Validate(TValue, IFormatProvider?, out T)` returning the owner's error type, and instance `TValue ToValue()` whenever serialization or persistence participation is declared — so a custom grammar cannot drift from the triad contract that rail bridges and integrations compile against.

[METADATA_PULL_REGISTRY]:
- Metadata registration is pull, not push: there is no module initializer. Lookup reflects the explicit-interface static metadata property on first touch of a type, walks base types when needed, unwraps nullable-of-owner, and memoizes per type in a process-wide concurrent map — cold-start dispatch pays one reflection probe per type; steady-state pays a dictionary hit. Constructed generic owners each materialize their own metadata instance in their own static property, so per-closed-type lookup is automatic.
- The weakly-typed key bridge does not fail fast on foreign key types: it pattern-matches the boxed key and falls through to `default` on mismatch — for a struct-keyed owner a wrong-typed key degenerates into validating the zero value, which a permissive invariant will admit; for a class-keyed owner it degenerates into the null path. On the weak route, type mismatch is indistinguishable from zero-value admission; strongly-typed dispatch through the factory interface is the only route that makes mismatch unrepresentable.
- Factory-skipped owners keep their keyed metadata but null out the factory-routed delegates (from-key delegate, expression, weak try-get) while retaining the constructor-route expression — persistence materialization survives the master cut by design. Metadata consumers selecting a conversion route must filter on delegate presence, not on metadata kind.
- Conversion-metadata resolution selects the last declared object factory satisfying the filter — attribute declaration order is a semantic input wherever more than one factory can match a custom filter; only the serialization, persistence, and binding claims are analyzer-deduplicated.
- Internal-surface policing keys on the internal namespace or the internal marker attribute at warning severity; every generated owner carries a per-type suppression. The package also ships the generic admission arrows (keyed, span, and parse forms of a static-abstract `Validate` invoker, aggressively inlined), which its own generated parse surface calls; boundary code reusing them buys the package's exact dispatch at the cost of one auditable warning per use site.

[DECLARATION_EDGES]:
- Record declarations are rejected outright as owners — record-synthesized equality, formatting, and copy semantics would collide with every generated surface, so the positional-record spelling of a value object is unrepresentable rather than subtly wrong.
- Handing the key member to the author splits into two errors — member-not-found and member-type-mismatch — but the satellites still generate against the configured member name, so a missing member produces one true diagnostic followed by a missing-symbol cascade through every derived surface; the cascade size is a function of how arithmetic-capable the key is.
- Factory-name configuration fails silent: assigning whitespace or empty to `CreateFactoryMethodName` or `TryCreateFactoryMethodName` reverts to the canonical names instead of erroring — renames driven from computed constants must not assume an invalid value will be caught.
- Access-modifier configuration is a composable lattice (private, protected, internal, public, with the two composite protections as bitwise unions of their parts), applied uniformly across the constructor and key-member knobs — one vocabulary governs every generated accessibility decision on the owner.
- The serialization-framework gate is a closed three-bit flag set — two JSON families and one binary format, with a combined-JSON alias and an all-mask — materialized verbatim into each factory metadata row, so runtime integrations resolve the wire partition by flag intersection on metadata instead of re-reading attributes; any framework outside the vocabulary enters only through an object factory or the component-model converter, never through the flags.
