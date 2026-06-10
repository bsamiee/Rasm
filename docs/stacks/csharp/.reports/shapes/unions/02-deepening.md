# Discriminated Unions — Case-Kind Regimes, Wire Admission, and Failure Surfaces

[CASE_KIND_REGIME]:
- The owner's declaration kind is family-global by language law — records derive only from records, classes only from classes — so `abstract partial record` versus `abstract partial class` on the root fixes every case's kind at once: equality, `ToString`, `with`, and `Deconstruct` are a root-level regime decision, never a per-case choice.
- An abstract record owner synthesizes `==`/`!=` at the owner static type routing through virtual `Equals` plus `EqualityContract`: two owner-typed references compare by runtime case and payload — whole-family structural equality with zero hand-written code. An abstract class owner leaves owner-typed `==` as reference identity, and no structural comparison exists anywhere in the family unless hand-written, because the class root forbids record cases outright.
- `EqualityContract` makes cross-case equality constant-false even when two cases carry identical payload shapes, so a record family needs no discriminator guard before comparing — distinct folds, set membership, and memo keys are case-exact for free.
- Record-rooted families are flat by analyzer law (every record case sealed, no nested unions inside records); depth — intermediate abstract cases, nested dispatch surfaces, stop-at boundaries — requires a class root and surrenders generated structural equality. The trade is exactly evidence family (record root: value semantics, flat) versus intent tree (class root: depth, reference identity).
- `with` is legal at the owner static type because the synthesized clone is virtual, but it can rebind only owner-declared members there; rebinding case payloads demands the case static type — non-destructive payload mutation lives behind a type pattern, and its result stays inside the family.
- Positional record cases ship `Deconstruct`, so boundary probes compose recursive patterns (`x is Owner.CaseA(var p, _)`) without touching dispatch; interior code still owes totality to generated `Switch`, because language `switch` over a class hierarchy never proves exhaustiveness and the silencing `_` arm is precisely the hole the generated surface exists to close.
- Record cases print case-name-plus-payload; ad-hoc unions forward the raw member's `ToString`, erasing which alternative is active from every log line — two unions unequal by case can render identically. Where the active case matters observationally, project through `Map` to labeled text; `ToString` on an ad-hoc union is a payload printer, not an identity printer.

[WIRE_ADMISSION_FACTORY]:
- `[ObjectFactory<TValue>]` on a union owner declares a second admission route with analyzer-enforced shape: a user-written `static TError? Validate(TValue value, IFormatProvider? provider, out T item)` (TTRESG061), plus an instance `TValue ToValue()` the moment serialization or persistence is requested (TTRESG062). The generator then implements the static-abstract factory interface and the conversion interface, and publishes a factory-metadata row carrying the value type and per-consumer routing flags.
- The factory's failure channel is typed, not stringly: it defaults to the library validation-error and swaps to any type implementing the static-`Create(string)` error interface, so wire rejection speaks the same typed failure vocabulary as the rest of the boundary — one message-to-error constructor everywhere, declared once on the owner.
- `HasCorrespondingConstructor = true` is a proof obligation, not a hint: the owner must expose a single-argument constructor of the factory value type (TTRESG059), which metadata then exposes as a constructor-shaped conversion expression — an expression-tree admission path persistence providers can compile and translate where a method-call `Validate` is opaque.
- Multiple factory attributes partition by concern under analyzer guard: overlapping serialization-framework flags are an error, at most one factory may claim persistence, and at most one may claim model binding — every downstream consumer resolves exactly one admission grammar, mechanically.
- `[ObjectFactory<string>]` additionally generates the parse pair: `Parse` funnels through `Validate` and throws a format exception carrying the validation error's text; `TryParse` is the non-throwing rail (`false` plus `default` out). Both are public statics, so convention-based endpoint parameter binding discovers them with no registration; classic controller binding still wants the binder provider inserted at the boundary.
- Deserialization is admission: every generated converter routes the wire value through the same `Validate`, so hostile or stale payloads fail into the serializer's error channel and a half-built union cannot exist in memory — interior code never re-checks wire trust.

```csharp
[Union]
[ObjectFactory<string>(UseForSerialization = SerializationFrameworks.All, UseForModelBinding = true)]
public abstract partial record Channel
{
    public sealed record Mail(string Address) : Channel;
    public sealed record Page(string Number) : Channel;

    internal static ValidationError? Validate(string? value, IFormatProvider? provider, out Channel? item)
    {
        item = value?.Split(':') switch
        {
            ["mail", var address] => new Mail(address),
            ["page", var number] => new Page(number),
            _ => null,
        };
        return item is null ? new ValidationError($"<unparsable: {value}>") : null;
    }

    public string ToValue() => this.Switch(
        mail: static m => $"mail:{m.Address}",
        page: static p => $"page:{p.Number}");
}

// One grammar serves every consumer: JSON converters, persistence conversion, and
// endpoint binding all route through Validate; Parse failure carries the error text.
var direct = Channel.Parse("page:411", provider: null);
```

[WIRE_SHAPE_PRESSURE]:
- Regular-union cases are real types, so polymorphic JSON is the platform's own: per-leaf `[JsonDerivedType]` rows on the owner. Resolution is exact-runtime-type — an unregistered leaf fails serialization rather than degrading, and abstract intermediates are registrable only as targets of the opt-in nearest-ancestor fallback. That fallback is the wire mirror of stop-at dispatch overloads: coarse consumers read a whole case subtree as one shape while the family stays closed.
- The polymorphism discriminator must lead the JSON object unless out-of-order metadata reading is enabled on the serializer options; producers that reorder or stream properties break round-tripping silently — pin the option at the boundary owner, never per call site.
- Discriminator strings restate case identity the program already knows; derive them from `nameof(Owner.Case)` or the published member list so renaming a case breaks compilation, not production decoding.
- Ad-hoc unions never carry a discriminator by design — single-value factory admission is the only wire route. The per-serializer converters are emitted only when that serializer's integration assembly is referenced: converter generation is keyed off the dependency graph at definition time, so adding the reference retroactively widens every factory-marked union with no source edit.
- The binary serializer's own `[Union]` attribute collides by simple name with the generator's; a boundary file importing both namespaces needs an alias to compile. That serializer has no regular-union integration at all, so binary wire shape for a case tree is either the canonical-string factory or hand-written formatters — decide before the family grows.
- One `[ObjectFactory<string>(UseForSerialization = All)]` on the owner collapses every wire concern — JSON, binary, persistence, binding — onto one canonical grammar with one parser and one printer; the cost is opacity (no per-property wire schema). Correct for identifier-like unions; wrong for document-like evidence payloads whose properties downstream systems must address.
- Persistence by inheritance: cases map as a single-table hierarchy with one discriminator column and a value row per leaf; conventions do not discover nested types, so each case is registered explicitly — the closed member list turns that registration into a fold over the family rather than a hand-enumerated block, and per-case columns are configured on the case entity, never the owner.

[DEFAULT_POISON_CHANNELS]:
- The generated failure taxonomy is exact and worth knowing cold: a subverted regular-union closure throws an argument-range error naming the runtime type's full name from the dispatch default arm; an uninitialized struct ad-hoc union throws invalid-operation ("not initialized") from `Switch`, `Map`, `ToString`, `GetHashCode`, and `Value`; an impossible discriminator index throws argument-range from dispatch but index-range from `Value`; `As{Member}` on the wrong case throws invalid-operation naming both the requested and the actual member kind. Each shape fingerprints a distinct bug class — closure subversion, default leakage, memory corruption, misrouted projection.
- The default-poison analyzer catches literal `default(T)`, bare `new T()`, and undeclared required fields — but `new T[n]` elements, `default(TGeneric)` flowing through unconstrained generics, and uninitialized struct fields nested inside other structs all evade it; the runtime throw set is the backstop, which means hashing or logging a poisoned union is itself the crash site, far from the leak.
- `Is{Member}` is the single total probe on a possibly-default struct union — a pure discriminator compare that cannot throw; guard rehydration, pooling, and array-scan seams with it before touching any other generated member.
- Struct versus class ad-hoc owners trade failure channels, not safety: struct buys allocation-free transport plus a poisoned default; class buys an ordinary null channel plus per-value allocation — choose by which absence discipline the surrounding code already polices, and never police both.

[DISPATCH_COST_AND_TOOLING]:
- Regular dispatch compiles to a most-derived-first type-test ladder: cost is linear in case count and case position; ad-hoc dispatch switches an integer index (constant); a case-owned virtual member is a constant vtable hop. A hot, wide regular family therefore pushes per-case behavior onto the cases as virtual members and reserves generated `Switch` for consumer seams where compile-time totality is the point — the dispatch forms differ in cost model, not just in who owns the behavior.
- `Map`'s eagerness amplifies allocation, not merely computation: every branch value is constructed per call, so a `Map` whose arms allocate builds N-1 dead values per dispatch — `Map` arms must be preallocated constants or cheap projections, and any allocating arm belongs in `Switch`.
- Generated members carry debugger-transparency attributes: stepping into a `Switch` lands directly in the chosen callback, and exception stacks omit generator plumbing — the first user frame above a dispatch throw is the call site, making the default arm's runtime-type message the entire triage signal for cross-assembly closure subversion.
- Callback parameters are annotated consumed-before-return for analysis tooling (declared under a pragma because the annotation type may arrive from several referenced assemblies), and IDE quick actions scaffold the complete named-argument set for `Switch`/`Map` and the partial forms — add the case, re-scaffold every red call site, and the compile break the addition caused becomes the repair loop.

[SURFACE_ABSORPTION]:
- Generated implicit conversions make an ad-hoc union a parameter-side absorber: one parameter of the union type replaces an overload per member, and collection expressions lift heterogeneous raw members element-wise through those conversions — a single `params ReadOnlySpan<U>` or array entrypoint accepts a mixed batch with zero ceremony at every call site.
- The absorption boundary is exactly the language's: members typed `object`, interface, or type parameter legally cannot own conversion operators, so their presence flips the whole family to factory-method admission — call shape degrades from juxtaposition to named construction the moment the member vocabulary abstracts, which is a real cost to weigh when choosing member types.

```csharp
[Union<string, long>(T1Name = "Tag", T2Name = "Serial")]
public partial struct Marker;

// Collection expressions lift mixed raw members through the generated implicit
// conversions: one entrypoint, no overload family, no per-element allocation.
Marker[] batch = ["alpha", 42L, "beta"];
var weight = batch.Sum(static m => m.Switch(tag: static t => t.Length, serial: static _ => 8));
```

[METADATA_SEAM]:
- Both families publish static-interface metadata — the closed type-member list for regular owners, the member-type list for ad-hoc — and factory owners publish a factory-metadata list whose rows carry the value type and per-consumer routing flags; a runtime lookup maps `Type` to metadata, and a filtered variant resolves the single conversion a given consumer should apply. That filtered lookup is the actual selection algorithm serializer and persistence adapters execute — custom infrastructure that consumes it inherits framework-identical routing for free.
- The metadata surface is itself a closed variant family with hand-written total `Switch`/`Map` over its kind cases — the package's own infrastructure speaks the dispatch grammar it generates, and consuming it obeys the same named-argument call-site laws.
- Direct use trips the internal-API diagnostic (TTRESG1000) everywhere outside generated code; the defensible consumption zone is family-law tests and infrastructure adapters — folding the published member list to assert every case owns a wire row, a discriminator mapping, or a render arm gives closed-family introspection without assembly scanning, and library reshaping surfaces as a test break instead of a production decode failure.
