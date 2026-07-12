# [RASM_IDENTITY]

The determinism owner (`Rasm.Domain`). This page owns the two reproducibility surfaces of the kernel: `ContentHash` — the federation content key, seed-zero `XxHash128` over canonical bytes — and `Deterministic` — the ONE splitmix64 derivation owner supplying order keys, unit-interval draws, and signed-unit streams to every reproducible algorithm. Identity and derivation are distinct concerns with one law each: `ContentHash` answers "is this the same content" across packages and runtimes; `Deterministic` answers "in what reproducible order, with what reproducible draw" inside one algorithm. Neither is cryptographic; neither has a sibling.

## [01]-[INDEX]

- [02]-[CONTENT_KEY]: `ContentHash` — the one federation content-identity entry, seed-zero `XxHash128` → `UInt128`, verbatim contract.
- [03]-[DETERMINISTIC_DERIVATION]: `Deterministic` — the one splitmix64 owner: finalizer, gamma-advance streams, coordinate order keys, clamped unit intervals.

## [02]-[CONTENT_KEY]

- Owner: `ContentHash` static class — one member, one algorithm, one seed. The caller owns the canonical byte projection; this owns the digest, so identity is byte-stable across packages and runtimes.
- Entry: `Of(ReadOnlySpan<byte> canonicalBytes)` → `XxHash128.HashToUInt128(canonicalBytes)` → `UInt128`. Seed is the `HashToUInt128` default of zero — never overridden, never parameterized.
- Law: THE federation content key, verbatim contract — every content hash in the federation composes this entry: the geometry content hash, the `Rasm.Element` projection/address seam, the `Rasm.Persistence` snapshot spine and artifact index, the python `runtime/evidence` peer, the typescript `kernel` peer, the rhino-bridge `CargoManifest.ContentHash`, and `Rasm.Rhino` block identity. One algorithm, one seed, no second hasher: a second hashing path anywhere in the federation forks identity and is the deleted form.
- Law: canonicalization is the caller's proof — this entry hashes the bytes it is handed; byte-stable member order, numeric normalization, and encoding are the projecting owner's obligations, stated where the projection lives. Two semantically equal values with divergent canonical projections are two identities, by design.
- Packages: System.IO.Hashing (`XxHash128.HashToUInt128` — the static one-shot; MIT, managed, no native dependency).
- Growth: streaming identity over large payloads is the same algorithm's incremental lifecycle (`XxHash128` `Append` + `GetCurrentHashAsUInt128`, seed zero) landing as one member on this owner when a consumer needs it — never a second algorithm, never a width change.
- Boundary: `UInt128` is the identity currency; wire and storage encodings (hex, two-lane `ulong`, byte order) are boundary projections owned at the consuming seam, never here.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.IO.Hashing;
using Rasm.Csp;

namespace Rasm.Domain;

// --- [OPERATIONS] ---------------------------------------------------------------------------
// The kernel's ONE content-identity entry: seed-zero XxHash128 over canonical bytes -> UInt128.
// Every federation content hash composes THIS entry — one algorithm, one seed, no second hasher.
public static class ContentHash {
    [BoundaryAdapter] public static UInt128 Of(ReadOnlySpan<byte> canonicalBytes) => XxHash128.HashToUInt128(canonicalBytes);
}
```

## [03]-[DETERMINISTIC_DERIVATION]

- Owner: `Deterministic` static class — the one splitmix64 owner. The finalizer `Mix` and the golden-gamma stream `Advance` are the private mechanism; the public family is the draws and keys — the unit projections `NextUnit`/`NextSignedUnit`/`NextSignedComplexUnit`, the coordinate `OrderKey`, and the clamped `UnitInterval` — one derivation family over one mixing function, with the mixer unreachable outside the owner so every new derivation lands HERE.
- Entry: two modalities discriminated by input shape — stream sampling advances a `ref ulong state` seeded by the consuming algorithm's named policy seed (`NextSignedUnit(ref state)` for real bases, `NextSignedComplexUnit(ref state)` for Hermitian bases); coordinate keying is stateless (`OrderKey(coordinates, seed)` with the `Point3d` overload routing into the span floor, `UnitInterval(point, salt, seed)` for per-point reproducible draws).
- Law: ONE mixing function — the splitmix64 finalizer with its published constants (`0x9E3779B97F4A7C15` gamma, `0xBF58476D1CE4E5B9`/`0x94D049BB133111EB` mixers) — under every member: the matrix eigensolver's basis streams and the sampler's coordinate keys are derivations of this one finalizer. The mature pair of private PRNGs — the eigensolver's local splitmix64 beside the sampler's ad-hoc coordinate mix — is the collapsed form; a mix minted outside this owner is the deleted form.
- Law: coordinate keys normalize the signed zero — `-0.0` projects to `+0.0` before bit extraction, so the two zeros key identically — and the seed widens unsigned (`(uint)seed`) so a negative seed never sign-extends into the state.
- Law: unit projections take the top 53 bits (`>> 11`, scaled by `2^-53`) so every draw is an exact double; `UnitInterval` clamps to `[RhinoMath.SqrtEpsilon, 1 - RhinoMath.SqrtEpsilon]` so log-weighted rejection draws (`-log(u) / weight`) stay finite at both ends.
- Law: derivation is not identity — `ContentHash` owns content equality; `Deterministic` owns reproducible algorithm-internal ordering and sampling. A content key built from `OrderKey`, or a sampler seeded from a content hash, crosses the concerns and is rejected.
- Cases: consumers by member — `matrix.md` LOBPCG deterministic starting bases (`NextSignedUnit`/`NextSignedComplexUnit` under its named basis-seed policy rows) · `sample.md` candidate ordering, active-set rotation, annulus draws, weighted-rejection keys (`OrderKey`/`UnitInterval`) · any reproducible tie-break in the processing suite (`OrderKey` over the deciding coordinates).
- Growth: a new reproducible draw shape (a 2D unit pair, a unit sphere direction, an index shuffle) is one member on this owner composing `Advance`/`OrderKey` — never a local generator, never `System.Random`.
- Boundary: the span fold in `OrderKey` and the state-advancing `ref` members are the named kernel exemption; determinism is the contract — no member reads time, thread identity, or process state.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Numerics;
using Rhino;

namespace Rasm.Domain;

// --- [OPERATIONS] ---------------------------------------------------------------------------
// The ONE splitmix64 owner: reproducible order keys, unit draws, and signed-unit streams.
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
        return Math.Clamp(value: unit, min: RhinoMath.SqrtEpsilon, max: 1.0 - RhinoMath.SqrtEpsilon);
    }
    // Signed zeros key identically: -0.0 normalizes before bit projection.
    private static ulong Bits(double value) => BitConverter.DoubleToUInt64Bits(value: value == 0.0 ? 0.0 : value);
}
```

## [04]-[DENSITY_BAR]

Two owners, two concerns, zero siblings; every reproducibility need in the corpus resolves to one of these members.

| [INDEX] | [CONCERN]               | [OWNER]         | [KIND]                                                                 | [RAIL]                                     | [CASES] |
| :-----: | :---------------------- | :-------------- | :--------------------------------------------------------------------- | :----------------------------------------- | :-----: |
|  [01]   | Content identity        | `ContentHash`   | static entry over seed-zero `XxHash128`                                | `ReadOnlySpan<byte> → UInt128`             |    1    |
|  [02]   | Reproducible derivation | `Deterministic` | the one splitmix64 family: streams + keys + draws over a private mixer | `ref ulong → double`/`coordinates → ulong` |    6    |
