# Comparer-Accessor Typeclass Algebra

[ACCESSOR_AS_STATIC_TYPECLASS]:
- Identity policy is carried by a type, not a value: a comparer accessor is a class or struct whose only obligation is one or two static-abstract properties — the equality-comparer accessor and the ordering-comparer accessor — threaded purely as a type argument on the policy attribute. The generated `Equals`, `GetHashCode`, `CompareTo`, and every relational operator reach the comparer through the constraint, so policy resolves at generic-instantiation time with no field, no constructor parameter, and no per-owner allocation: the comparison surface is a typeclass dictionary passed at compile time.
- Both accessor interfaces are contravariant (`in T`), so an accessor written over a base or interface type satisfies the constraint for every key assignable to it — one accessor class supplies the policy for a whole inheritance family of keys, never one accessor per concrete key.
- The interfaces place no `class` constraint on implementers: a `readonly struct` accessor is legal, and because generated code reaches the comparer only through the static member, the accessor type is never instantiated — its sole runtime footprint is the comparer object the property returns, shared across every comparison the owner performs.
- The two concerns are split into two interfaces precisely so they can be satisfied independently — a key needing only ordering takes the ordering accessor while equality stays default, and vice versa — while the pairing diagnostics re-couple them at the surface; the type system keeps the concerns orthogonal so an owner opts into exactly the surface its domain needs.

[STATIC_TO_INSTANCE_HOP]:
- The accessor returns a comparer instance, not a static method, so every generated comparison is a two-hop dispatch: a static-abstract property read resolved at compile time, then a virtual `Equals`/`Compare` on the returned object. The first hop is free; the second is the cost of expressing policy as a reusable comparer object rather than inlined arithmetic.
- The property body is evaluated on every comparison: a custom accessor that constructs a fresh comparer per get manufactures a new object on each `Equals` inside a hot dictionary probe. The correct shape stores the comparer in a `static readonly` field and returns it — the typeclass contract rewards a singleton comparer because the type is the dictionary and the comparer is its only state.

```csharp
public sealed class CodepointAccessor : IEqualityComparerAccessor<string>, IComparerAccessor<string>
{
   private static readonly IEqualityComparer<string> EqInstance =
      new ProjectionEqualityComparer<string, string>(static s => s.Normalize().Trim(), StringComparer.OrdinalIgnoreCase);
   public static IEqualityComparer<string> EqualityComparer => EqInstance;
   public static IComparer<string> Comparer => StringComparer.Ordinal;
}
```

[PROJECTION_COMPOSITION]:
- The projection comparer composes its inner comparer the same way the owner composes the accessor: the selector picks a sub-value, then defers to an inner comparer for that sub-value, which can itself be a string-policy comparer. A two-level policy — equal when the normalized form of a derived substring matches case-insensitively — is one accessor whose field stacks selector over inner comparer, and the owner inherits the full stack through a single type argument; its hash arm returns zero for a null projection, so null-projecting elements bucket together rather than throwing.
- A member-level accessor on a multi-field owner is the same one-property class attached per member, so the member list carries a heterogeneous vector of typeclass policies — one field case-insensitive, another ordinal, another sequence-equated — all resolved statically inside the single generated `Equals`. The accessor tier is the only tier that overrides a member's intrinsic semantics: no owner-wide switch reaches a single member without an accessor, so correcting one wrong member equality is always a per-member accessor promotion, never a global setting.

[ORDERING_CONVERGENCE]:
- `CompareTo` and the relational operators are independent derivations that agree only through an accessor: `CompareTo` defaults to casting the key to its comparable interface while the operators forward the key's own operator methods — two routes that can disagree when a key implements one but not the other. A configured ordering accessor forces both through the accessor's `Compare`, restoring a single ordering definition; this convergence is the structural reason the pairing diagnostics treat equality-and-ordering accessors as one policy, because only with the accessor present do the two ordering surfaces provably agree.
- The relational key-type overloads compare the owner directly against an unadmitted raw bound through the same accessor — a range gate at the boundary runs the owner's collation against a raw threshold without admitting it and without casting the owner out to the key. This is the sanctioned spelling for raw-threshold gating: the threshold stays raw, the comparison stays the owner's policy, and no second owner is materialized.

[HASH_POLICY_ALIGNMENT]:
- The keyed owner's `GetHashCode` is the accessor's hash of the key member, never the member's own hash — so the `Equals`-implies-same-hash contract is structurally guaranteed under any custom equivalence, not reviewed. A reference-typed key from a type parameter null-coalesces its hash to zero rather than throwing, so a null-keyed generic owner is hashable in the degenerate case the comparer would otherwise reject.
