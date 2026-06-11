# Ad-Hoc Struct Union Representation Physics

[FIELD_COLLAPSE_ALGEBRA]:
- The backing-field set is computed by an exact predicate, not one-per-member: the shared object slot is emitted once and absorbs every reference member when the count of stateful, non-type-parameter, non-duplicate reference members reaches two; below that threshold each reference member keeps its own typed field. Adding a third reference case can therefore flip the entire family's storage shape from typed fields to one boxed slot — a silent representation change observable only in the memory diagram, with zero source or behavior difference.
- Struct members never join the shared slot: each stateful value-type member retains a dedicated inline field, so the owner sizes to its tag plus its widest struct-member cluster plus one shared pointer once the collapse fires — never to the sum of all payloads — and the auto layout emitted whenever no layout is user-declared hands the runtime permission to overlap-pack and reorder the heterogeneous fields and the tag, deleting the padding holes a hand-declared sequential layout would freeze in.
- A stateless member is a zero-width alternative: no field, accessor returning the default, intra-case equality constant-true, the case's entire runtime identity its index — strictly cheaper than a nullable reference marker because it has no null channel to mis-handle.

[POISON_INVISIBLE_TO_FLOW_ANALYSIS]:
- The default-struct poison hides inside a fully-inhabited static type: a default-constructed union satisfies not-null annotations, flows through non-nullable parameters, and lives in arrays and uninitialized fields with zero diagnostic, because the invalid state is a discriminant value the type system cannot name. The channel the language polices for reference absence has no analogue here — the only proof of validity is a runtime probe, never an annotation.
- The crash site is the first observation, not the minting site: array allocation, an unconstrained generic default, or a zero-filled outer struct manufactures the poison far from where hashing, logging, or comparing the value later throws — the fault surfaces at whichever innocent read touches a value-bearing member first, and triage must walk back to the allocation, not the throw.
- The discriminator probe is the one generated member total over the poisoned state: a pure index compare that reads zero as "not this case" rather than as an error. Every rehydration, pooling, array-scan, and deserialization seam guards with the probe before touching any value-bearing member, and the disjunction of all member probes reconstructs "is this value initialized at all" — the typestate test the language refuses to express, recovered as a fold over probes.

```csharp
[Union<Anchor, Drift>(T2IsStateless = true)]
public readonly partial struct Cursor;

public readonly record struct Drift;

// A scan over possibly zero-filled slots guards on the throw-free probe,
// never on Value/Switch, which fault on the poisoned default.
public static Seq<Anchor> Anchors(ReadOnlySpan<Cursor> window)
{
    var live = Seq<Anchor>.Empty;
    foreach (var c in window)               // span kernel: the named statement exemption
        live = (c.IsAnchor, c.IsDrift) switch
        {
            (true, _) => live.Add(c.AsAnchor),
            _ => live,                       // index 0 and Drift both fall here, no throw
        };
    return live;
}
```

[EXCEPTION_TAXONOMY_AS_TRIAGE]:
- The generated throw set is a three-tier taxonomy that localizes which invariant broke from the exception type alone, without a stack walk: invalid-operation for the poisoned default and for wrong-case projection on a correctly initialized value; index-out-of-range for a corrupted discriminant reaching the value and switch surfaces; argument-out-of-range for the same corruption reaching the map surfaces. The corrupted-discriminant arms are reachable only by binary corruption, torn struct reads, or reflection-built instances bypassing the constructors — they exist to convert silent memory corruption into a named exception at first dispatch rather than a wrong-arm execution.

[EQUALITY_OVER_TAGGED_BITS]:
- Equality is discriminant-gated then field-dispatched: index inequality short-circuits before any field read, so cross-case comparison never touches a payload. Under the shared-slot collapse the emitted arm re-checks the index before the field compare, because one object field serves several cases and only the index disambiguates which member's equality semantics apply to the stored reference.
- Reference-member arms are null-safe by structural emission, not operator dispatch — a null-pattern test before the member's own equality — so a genuinely-null payload and a stateless-derived null compare structurally without faulting; type-parameter members route both equality and hash through the default comparer of the closed instantiation, computed without boxing the value type.

[FAILURE_CHANNEL_RELOCATION]:
- The struct-versus-class choice relocates the absence discipline rather than removing it, and the two invalid states differ in nameability: the struct's poison is unnameable but free to construct, the class's null is nameable and policed by the entire existing nullability apparatus. Where the union lives in large arrays or hot value-type flow, the allocation win selects the struct; where it must round-trip reflection, serialization, or any path that zero-fills storage, the class's honest null is worth the per-value allocation precisely because those paths mint the struct's poison invisibly.
- The ref-struct owner is the extreme of the value form: boxed equality hard-returns false, the equatable and operator interfaces are skipped entirely, and stack-only type parameters on the owner itself are rejected — an equality-less, stack-bound carrier whose sole purpose is moving one of several span-shaped payloads through a single dispatch without heap escape.
