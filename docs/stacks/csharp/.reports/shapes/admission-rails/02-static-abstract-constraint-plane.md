# Static-Abstract Constraint Plane

[CONSTRAINT_LATTICE_MINIMALITY]:
- The generated vocabulary interface is an F-bounded static-abstract typeclass: `ISmartEnum<TKey, T, out TError> : ISmartEnum<TKey>, IObjectFactory<T, TKey, TError>, IObjectFactory<TKey>` with `where T : ISmartEnum<TKey>` — the self-reference is the dictionary witness, so a generic algorithm constrained on the one interface receives enumeration, lookup, admission, and key projection as static-abstract members callable through the type parameter, and the owner type itself is the passed-in typeclass instance with zero runtime witness object.
- The lattice is layered so each lower tier is satisfiable by strictly more shapes than the tier above: the key-only enum interface yields key projection and identity without lookup or admission; the trinary factory interface yields admission without enumeration and is satisfied by keyed value objects equally; the bare single-parameter factory marker carries only the value-type witness and exists to let `notnull, allows ref struct` flow without naming the error type. A kernel that only needs "buildable from this raw shape" constrains on the bare marker and stays error-type-agnostic.
- Selecting the minimal tier is collapse discipline applied to constraints: a projection-only algorithm that constrains on the full enum interface has over-specified and silently rejects every keyed value object that would have satisfied it, and widening a bridge from the enum interface to the factory interface is exactly the act that turns a vocabulary-specific bridge into a program-wide one.
- Enumeration is itself constraint-reachable — `static abstract Items` folds through the type parameter — so one generic startup probe walking every vocabulary through the constraint forces deferred lookup materialization for the whole program in one pass, and the duplicate-key fail-fast fires inside the generic call rather than at the first production admission of each concrete type:

```csharp
public static B FoldFamily<T, TKey, TError, B>(B seed, Func<B, T, B> step)
    where T : ISmartEnum<TKey, T, TError>
    where TKey : notnull
    where TError : class, IValidationError<TError> =>
    T.Items.AsIterable().Fold(seed, step);
```

[RELAY_AND_SPECIALIZATION_PHYSICS]:
- The language forbids invoking a static-abstract member on a bare type parameter outside a constrained generic context, which is the entire reason the shipped relays exist: aggressively inlined generic statics whose whole body is the constrained call (`T.Validate(...)`, `TError.Create(...)`), consumed by expression trees and boxed dispatch that hold the constraint but cannot name the member. Reusing them buys the package's exact dispatch instead of re-deriving a per-owner bridge.
- Static-abstract members resolve through constrained call, which the runtime monomorphizes per closed value-type instantiation and shares per reference-type instantiation: a generic admission method over a value-keyed owner specializes to a direct call with no interface dispatch and no allocation, while reference-keyed owners share one body reached by a constrained call the JIT devirtualizes when the exact type is known. The cost of the constraint plane over hand-written per-owner admission is therefore zero on value-keyed paths and one devirtualizable indirection otherwise — never a dictionary of witnesses.
- The error-construction plane is itself static-abstract: forwarding `TError.Create(message)` through the constraint manufactures the exact error subtype the owner declared — never a base placeholder — entirely from the type parameter, so a structured custom error vocabulary is honored by a fully generic bridge that never names it.

[SPAN_OVERLOAD_SPLIT]:
- The general static-abstract `Validate` takes its raw value in a `TValue?` nullable form that a byref-like type cannot inhabit, so span admission is not an instantiation of the general key path: the span route is a separately constrained entry pinned to `ReadOnlySpan<char>` against the span-factory shape (`T : IObjectFactory<T, ReadOnlySpan<char>, TError>`). An algorithm that must accept both a heap key and a span over one owner family declares two overloads mirroring this split — no single constraint spans both, by the nullable-form mechanics rather than by policy.
