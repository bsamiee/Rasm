# [RASM_IDENTITY]

`Rasm.Domain` owns the kernel's two reproducibility surfaces with no sibling between them: `ContentHash`, the federation content key over caller-canonical bytes, and `Deterministic`, the one splitmix64 owner supplying order keys, unit-interval draws, and signed-unit streams to every reproducible algorithm. Neither is cryptographic.

Identity and derivation never cross: a content key built from a `Deterministic` order key, or a sampler seeded from a `ContentHash`, is rejected by design. Every federation partner reproduces the zero-fixed seed byte-for-byte, so one content space addresses across packages and runtimes.

## [01]-[INDEX]

- [02]-[CONTENT_KEY]: `ContentHash` mints the seed-zero `XxHash128` federation content key.
- [03]-[DETERMINISTIC_DERIVATION]: `Deterministic` owns order keys, unit draws, and signed-unit streams off one splitmix64 finalizer.

## [02]-[CONTENT_KEY]

- Owner: `ContentHash` static class — one member, one algorithm, seed zero; THE federation content key every partner composes. Caller owns the canonical byte projection, this owns the digest, so identity is byte-stable across packages and runtimes.
- Entry: `Of(ReadOnlySpan<byte> canonicalBytes)` → `XxHash128.HashToUInt128(canonicalBytes)` → `UInt128`; seed is the `HashToUInt128` default zero.
- Law: canonicalization is the caller's proof — this entry hashes the bytes it is handed, so byte-stable member order, numeric normalization, and encoding are the projecting owner's obligation, and two semantically equal values with divergent canonical projections are two identities.
- Packages: `System.IO.Hashing` (`XxHash128.HashToUInt128` — the static one-shot; MIT, managed, no native asset).
- Growth: streaming identity over large payloads is the same algorithm's incremental lifecycle (`XxHash128.Append` + `GetCurrentHashAsUInt128`, seed zero), landing as one member on this owner.
- Boundary: `UInt128` is the identity currency; wire and storage encodings (hex, two-lane `ulong`, byte order) are boundary projections at the consuming seam.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.IO.Hashing;
using Rasm.Csp;

namespace Rasm.Domain;

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class ContentHash {
    [BoundaryAdapter] public static UInt128 Of(ReadOnlySpan<byte> canonicalBytes) => XxHash128.HashToUInt128(canonicalBytes);
}
```

## [03]-[DETERMINISTIC_DERIVATION]

- Owner: `Deterministic` static class — the one splitmix64 owner: `Mix` (finalizer) and `Advance` (golden-gamma stream) are the private mechanism, the public family is the unit draws, order keys, and clamped interval, and the mixer is unreachable outside the owner.
- Entry: two modalities by input shape — stream sampling advances a `ref ulong state` seeded by the consuming algorithm's named policy seed (`NextSignedUnit` for real bases, `NextSignedComplexUnit` for Hermitian); coordinate keying is stateless (`OrderKey(coordinates, seed)`, the `Point3d` overload routing into the span floor, `UnitInterval(point, salt, seed)` for per-point draws).
- Law: coordinate keys normalize the signed zero — `-0.0` projects to `+0.0` before bit extraction so the two zeros key identically, and the seed widens unsigned (`(uint)seed`) so a negative seed never sign-extends into the state.
- Law: unit projections take the top 53 bits (`>> 11`, scaled `2^-53`) for an exact double; `UnitInterval` clamps to `[EpsilonPolicy.SqrtEpsilon, 1 - EpsilonPolicy.SqrtEpsilon]` — the one named epsilon owner — so log-weighted rejection draws (`-log(u) / weight`) stay finite at both ends.
- Cases: consumers by member — the matrix eigensolver's LOBPCG starting bases (`NextSignedUnit`/`NextSignedComplexUnit` under its named basis-seed policy), the sampler's candidate ordering, active-set rotation, annulus, and weighted-rejection draws (`OrderKey`/`UnitInterval`), and any reproducible tie-break in the processing suite (`OrderKey`).
- Growth: a new reproducible draw shape is one member on this owner composing `Advance`/`OrderKey`.
- Boundary: the span fold in `OrderKey` and the state-advancing `ref` members are the named kernel exemption; no member reads time, thread identity, or process state.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Numerics;
using Rasm.Numerics;
using Rhino.Geometry;

namespace Rasm.Domain;

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class Deterministic {
    private const ulong Gamma = 0x9E3779B97F4A7C15UL;
    private static ulong Mix(ulong state) {
        ulong z = state;
        z = unchecked((z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL);
        z = unchecked((z ^ (z >> 27)) * 0x94D049BB133111EBUL);
        return z ^ (z >> 31);
    }
    private static ulong Advance(ref ulong state) => Mix(state: state = unchecked(state + Gamma));
    public static double NextUnit(ref ulong state) => (Advance(state: ref state) >> 11) * (1.0 / 9_007_199_254_740_992.0);
    public static double NextSignedUnit(ref ulong state) => (NextUnit(state: ref state) * 2.0) - 1.0;
    public static Complex NextSignedComplexUnit(ref ulong state) => new(real: NextSignedUnit(state: ref state), imaginary: NextSignedUnit(state: ref state));
    public static ulong OrderKey(Point3d point, int seed = 0) => OrderKey(coordinates: [point.X, point.Y, point.Z], seed: seed);
    public static ulong OrderKey(ReadOnlySpan<double> coordinates, int seed = 0) {
        ulong state = unchecked((uint)seed + Gamma);
        foreach (double coordinate in coordinates) {
            state = Mix(state: state ^ Bits(value: coordinate));
        }
        return state;
    }
    public static double UnitInterval(Point3d point, int salt, int seed = 0) {
        int mixed = unchecked((seed * 16_777_619) + salt);
        double unit = ((OrderKey(point: point, seed: mixed) >> 11) + 1.0) * (1.0 / 9_007_199_254_740_992.0);
        return Math.Clamp(value: unit, min: EpsilonPolicy.SqrtEpsilon, max: 1.0 - EpsilonPolicy.SqrtEpsilon);
    }
    // Signed zeros key identically: -0.0 normalizes before bit projection.
    private static ulong Bits(double value) => BitConverter.DoubleToUInt64Bits(value: value == 0.0 ? 0.0 : value);
}
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
