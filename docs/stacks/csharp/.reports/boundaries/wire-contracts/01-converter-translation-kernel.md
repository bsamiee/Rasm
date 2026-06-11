# Converter Translation Kernel

[NULL_TOKEN_DISPATCH_CONTRACT]:
- The base constructor derives null handling from `default(T)`, not from intent: a converter over a non-nullable value type or struct receives `HandleNullOnRead = true` automatically because `default(T) != null`, so `Read` is invoked on a `JsonTokenType.Null` token and must decide its own projection; a converter over a reference or nullable type receives `HandleNullOnRead = false`, and the framework short-circuits the null token to `default` before `Read` is ever entered. A reference-typed wire projection that must distinguish wire-`null` from wire-absence therefore cannot observe the null token through `Read` at all unless it overrides `HandleNull => true`.
- Overriding `HandleNull` to return a constant flips both read and write handling together — there is no independent read-only or write-only override; the constructor reads the virtual once and sets `HandleNullOnRead` and `HandleNullOnWrite` from the same value. A converter that must accept a null token on read but emit nothing rather than `null` on write cannot express the asymmetry through `HandleNull`; the write asymmetry lives on the property contract via the missing-key condition, the read asymmetry lives inside `Read`.
- Reading the base `HandleNull` getter has a side effect: it sets an internal `UsesDefaultHandleNull` marker that selects the `default(T) != null` derivation. A subclass that overrides `HandleNull` must not call `base.HandleNull`; doing so silently re-arms the default derivation path it was overriding.

[READER_POSITION_AND_STRATEGY_INVARIANT]:
- A converter declares one of two strategies implicitly. A value converter — one whose `Read`/`Write` consume and emit a single token — is read-ahead-buffered: the framework guarantees the complete value is materialized in the reader before `Read` is entered, so the converter never sees a value straddling a pipeline segment boundary. An object or array converter — one that drives the reader through a structured body — is not buffered and must itself loop the reader, tolerating mid-structure suspension on streaming inputs.
- The framework verifies reader displacement after every value-converter `Read`: a value converter must leave the reader on the same logical token it received, and any change in bytes-consumed is a contract violation that throws a serialization-converter-read fault. An object-strategy converter is verified differently — it must terminate with the reader positioned on the closing `EndObject`/`EndArray` at the same depth it started; ending early or consuming past the structure throws. A converter that reads one extra token "to be safe" corrupts the parent's read loop and trips this verifier rather than failing silently.
- The symmetric write verifier asserts that writer depth on exit equals depth on entry. A converter that opens an object and returns before the matching close, or that emits a property name with no value, leaves the writer in an unbalanced state that the verifier converts into a fault at the boundary, not deep in a downstream consumer.

[CLOSED_FAMILY_OWNED_BY_ONE_CONVERTER]:
- A single converter over the closed-family root owns the entire wire projection for that concept: `Read` peeks the discriminant token, then dispatches to the case projection; `Write` matches on the case and emits the case-specific shape plus the discriminant. The family stays one type with one converter rather than a converter per case, and the domain hierarchy and the wire hierarchy never reference each other's schema — the converter is the only place both shapes are simultaneously visible.
- Discriminant-first reading requires reading the discriminant property before the body is known, which is incompatible with positional-constructor binding unless the converter buffers. The structural choice is to read the whole object into a detached element, extract the discriminant, then deserialize the remainder against the resolved case contract; the alternative is a hand-rolled forward scan that records the discriminant and replays the body, which only pays off at extreme throughput where the double-parse cost dominates.

```csharp
public sealed class FamilyConverter : JsonConverter<Family>
{
    public override Family Read(ref Utf8JsonReader reader, Type t, JsonSerializerOptions o)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;
        return root.GetProperty("$kind").GetString() switch
        {
            "a" => new Family.A(root.Deserialize(FamilyContext.Default.CaseA)!),
            "b" => new Family.B(root.Deserialize(FamilyContext.Default.CaseB)!),
            _   => throw new JsonException("unrecognized discriminant"),
        };
    }
    public override void Write(Utf8JsonWriter w, Family value, JsonSerializerOptions o) =>
        value.Switch(
            a => Emit(w, "a", a.Payload, FamilyContext.Default.CaseA),
            b => Emit(w, "b", b.Payload, FamilyContext.Default.CaseB));
}
```

- `JsonDocument.ParseValue(ref reader)` consumes exactly the current value subtree and leaves the reader on its final token, satisfying the object-strategy displacement invariant; `Deserialize(JsonTypeInfo<T>)` on the resulting element stays metadata-clean by routing through the generated context rather than reflection. The converter throws `JsonException` for an unrecognized discriminant so the throw is caught and path-annotated by the framework's read wrapper before the rail bridge translates it.

[FACTORY_DISPATCH_FOR_OPEN_GENERICS]:
- A factory converter answers `CanConvert` over an open shape — any closed generic of one definition, any type carrying a marker interface — and synthesizes the closed converter in `CreateConverter`. Its `Type` is null and its strategy is none; it never reads or writes, and the framework calls every value/object operation on the converter it produces, not on the factory. A factory that returns null or returns another factory throws immediately, so the synthesis must always resolve to a concrete converter.
- The closed converter is built per type via reflection over the closed generic arguments inside `CreateConverter`, then cached by the options instance keyed on the resolved type; the reflection cost is paid once per type per options, not per message. This is the one sanctioned reflection site for a generated-context boundary, because it runs at contract-resolution time, not on the serialization hot path.
- A built-in string-enum converter is itself a factory closing over a naming policy and an integer-allowance flag, producing a per-options enum converter; the same shape — a factory holding policy state, emitting a stateless per-type converter — is the model for any vocabulary-keyed family whose key spelling is policy, not type.

[PER_MEMBER_CONVERTER_SYNTHESIS]:
- The converter-selection attribute is subclassable with a virtual `CreateConverter(Type)` and a parameterless constructor. A subclass can synthesize a converter parameterized by the attributed member's declared type without a separate registration, making the attribute the per-property converter selector: the same domain type serializes one way on a redacted field and another on an audit field, selected at the property declaration site, with no per-field converter type proliferation.
- A member-level converter attribute is resolved into the property's effective converter when the contract is built, taking precedence over any matching converter in the options-level converter list for that property. Property-converter ownership is therefore single-sited at the declaration, and a policy that must apply uniformly across a whole type belongs on a resolver modifier that sets the property converter centrally, not on per-member attributes that a future field can silently omit.

[DICTIONARY_KEY_SEAM_IS_DISTINCT]:
- A type used as a dictionary key serializes through `WriteAsPropertyName`/`ReadAsPropertyName`, a seam entirely separate from `Write`/`Read`. The default implementations throw a key-type-not-supported fault for any non-string key unless a fallback simple converter exists, so a value-object or smart-enum key that round-trips as a value but appears as a map key requires both seams overridden, not just the value seam.
- The property-name write seam is verified to leave the writer on a `PropertyName` token at unchanged depth: it must emit exactly one property name and no value, the inverse of the value-write balance check. A key converter that emits a full object as a key, or a value plus a name, trips this verifier. The read seam likewise must consume exactly the property-name token with zero net byte displacement.

[NUMBER_HANDLING_INTERSECTS_THE_KERNEL]:
- Number-handling policy does not route through `Read`/`Write` for a custom converter unless the converter is recognized as a number-type converter internally; for an ordinary custom converter, the contextual `JsonNumberHandling` is invisible and the converter must implement any string-encoded-numeric tolerance itself by branching on the incoming token type. A measurement field that must reject string-encoded numbers enforces that inside its converter `Read` by demanding `JsonTokenType.Number`, not by relying on a strict number-handling flag that a custom converter never observes.
- The numeric custom-handling hooks throw by default and are internal extension points; a public custom converter cannot opt into the per-context number-handling pipeline and must treat number tolerance as part of its own token contract.

[POLYMORPHIC_REENTRY_GATE]:
- The polymorphic write path is gated by an init-only capability flag on the converter, set at construction. When set, the framework resolves the runtime subtype's converter before calling the declared converter, so a base-typed converter can be bypassed entirely for a derived instance. A hand-written family converter that wants to own all cases itself, including derived ones, must not advertise this capability, or the framework's resolution will intercept derived instances before the converter's own `Write` discriminant-emission runs.
- The read re-entry path branches to the structured `OnTryRead` only for reference types that advertise polymorphism; a sealed value-family converter takes the direct value path with full read-ahead. This is why a closed union projected as a value rather than a reference graph stays on the buffered, displacement-verified path and never enters the continuation machinery, simplifying its streaming-suspension contract to nothing.

[THROW_TRANSLATION_AT_THE_KERNEL]:
- Every exception escaping `Read`/`Write` is caught by the framework's core wrapper and re-thrown with JSON path information attached: a bare `JsonException` with a null path is enriched in place; a `FormatException`, `InvalidOperationException`, or marked rethrowable is re-thrown with path; a `NotSupportedException` lacking a path marker is converted to a path-annotated not-supported fault. A converter that throws a domain-specific exception type that is none of these escapes unwrapped and loses path evidence, so the converter must throw `JsonException` for any wire-shape rejection it wants path-anchored before the rail bridge captures it.
- Because the path-enrichment is performed by the core wrapper around the public `Read`/`Write`, a converter that catches and swallows its own exception to return a sentinel value defeats the boundary's failure visibility entirely; rejection must propagate as a throw so the path annotation and the downstream exceptional-error translation both fire.
