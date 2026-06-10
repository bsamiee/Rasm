# Shape Decision Law — Frontier

[BREAK_TOPOLOGY]:
- Each owner kind carries a fixed compile-break signature, and owner selection is selection of where change detonates: a new union case lands at every exhaustive dispatch call site; a new smart-enum item with delegate rows lands at the item declaration itself, because the generated constructor demands every behavior column before the item compiles; a new complex-value-object member lands at every factory call; a key-type migration on a keyed owner lands only at conversion, serialization, and persistence seams, because interior consumers hold the owner and never the key — key encapsulation converts a pervasive break into a boundary break.
- Growth absorption is tunable to totality: a vocabulary whose behavior lives entirely in delegate rows and whose `Switch`/`Map` generation is set to none absorbs item growth with zero consumer breaks — the owner is the only file that changes — while the same vocabulary with generated dispatch pushes every addition outward to all call sites. Declaring the dispatch surface away is therefore a break-topology decision, not merely an encapsulation one.
- Invariant tightening on a value object is the one change with zero compile signal: the factory narrows silently, and values persisted under the old invariant now fail rehydration. The validation-bypassing rehydration constructor exists exactly for this seam — stored data outlives invariant drift — so an invariant change is a data-migration decision with a code rider, and the proof obligation moves entirely to tests because the type system stays quiet.
- Arm names and the dispatch state-parameter name are source contract, not style: arms bind by named argument, so renaming a case — or changing the owner-configured state parameter name, which defaults to `state` and is renameable per owner — is a deliberate source-breaking act with the same gravity as deleting a method.

[CASE_OWNED_DISPATCH]:
- Generated `Switch`, delegate rows, and frozen tables leave one dispatch form unowned: abstract members on a regular union's base with per-case overrides — the union analogue of smart-enum delegate rows. Selection follows break topology: behavior intrinsic to the case with a uniform signature rides an abstract member (a new case cannot compile until it answers, and no call site changes); behavior that varies by caller rides `Switch` (every call site re-answers). With dispatch generation set to none, case-owned members become the only consumption form and the family grows fully owner-locally.
- Recursive case payloads are unconstrained — a case may hold members of the union type itself — so recursive sums fold two ways on one owner without interference: case-owned recursion through overridden members, and call-site catamorphism through `Switch` re-entry.

```csharp
[Union]
public abstract partial class Expr
{
    private Expr() { }

    public abstract int Depth { get; }

    public sealed class Lit : Expr
    {
        public required decimal Value { get; init; }
        public override int Depth => 1;
    }

    public sealed class Add : Expr
    {
        public required Expr Left { get; init; }
        public required Expr Right { get; init; }
        public override int Depth => 1 + int.Max(Left.Depth, Right.Depth);
    }
}

static decimal Eval(Expr expr) =>
    expr.Switch(
        lit: static l => l.Value,
        add: static a => Eval(a.Left) + Eval(a.Right));
```

- An intermediate node that itself carries the union attribute generates its own `Switch`/`Map` hiding the root's — the generator suppresses the member-hiding warning deliberately — so the static type of a reference selects dispatch granularity: a branch-typed reference dispatches over that branch's cases, a root-typed reference over the root arms. Granularity is chosen at the declaration site, never by a call-site flag.
- Closure and case discovery share one language mechanism: the base's private constructor is accessible only to types nested inside it, so the lexical case list and the construction lock are the same declaration — a derived type outside the body fails both membership and compilation at once.

[MODALITY_CARRIERS]:
- An ad-hoc union reifies call modality: one entrypoint takes a carrier whose members are the scalar, the batch, and the query form; implicit value-to-union conversions preserve overload-call ergonomics while the chosen modality becomes a value that can be stored, queued, logged, and replayed — capabilities an overload set structurally lacks because the modality dies at overload resolution.
- Stateless members extend the carrier to pure modes at zero payload: an all-items or none mode is a discriminator-only member beside payload members, deleting the sentinel value or boolean flag that would otherwise re-open the input space.
- Conversion-trigger symmetry keeps the carrier honest: any member that cannot receive a conversion operator — duplicate type, interface, `object`, type-parameter member — flips factory generation on for all members, so ingress never splits into half conversion and half factory; forcing value-to-union conversion off makes every call site name its modality explicitly where silent coercion would mislead.
- When the plural form is canonical and a scalar arrives, single-item read-only set, dictionary, and lookup factories bridge the arity as a library primitive — the scalar never earns a second entrypoint.

[VOCABULARY_ORDER]:
- A vocabulary with intrinsic order earns generated comparison operators only when its key type itself carries the operator set: numeric-keyed rank ladders get `<`/`>=` plus heterogeneous key-typed overloads (`severity >= 3` compiles with no conversion), while string-coded vocabularies can never reach operator form — the axis declaration is silently inert over a key without the operators. Key choice therefore decides whether order is operator-expressible: a vocabulary whose consumers dispatch by range needs the numeric key even when a string code exists, and the code becomes a column, not the key.
- Comparison generation coerces equality-operator generation upward, and no configuration expresses ordered-but-not-equatable; the inverse stance — identity without order — is declared by skipping comparability with operators off and is then enforced at every sort and range expression.
- Items enumeration order is declaration order and key order is comparer order; the two are distinct axes — presentation sequence rides declaration, semantic rank rides the key — and conflating them couples display to identity. Range dispatch over an ordered vocabulary is total over future items where an arm-per-item dispatch re-opens at every addition: threshold semantics belong to the key, case semantics to the arms.

[COMBINABLE_VOCABULARY]:
- Combinable capability sets split by where per-flag behavior lives: a flags enum has nowhere to put it, so flag-test chains re-derive policy at every consumer, while vocabulary items held in a frozen or immutable set carry behavior as columns and fold — membership is set algebra, policy is a fold over member items. The flags form survives only at bit-level interop seams where a foreign ABI owns the bit layout.
- A language enum remains the correct owner in exactly three places, all boundaries: mirroring a wire schema a foreign protocol owns (converted to vocabulary at admission), bit-flag interop with host or native APIs, and ordinal array indexing inside measured kernels. Its disqualifying property is open admission — any integral cast produces an undefined value — so an enum that travels past the boundary smuggles unvalidated input inward; conversion to the closed vocabulary at the admission seam is the re-closing move.

[TABLE_INTEGRITY]:
- A frozen table keyed by generated vocabulary regains the closure the dictionary form lost: totality is provable at composition time by comparing table cardinality against the product of the vocabularies' item counts, converting the partial-function risk of lookup tables into a one-time startup invariant instead of a per-call gamble. Raw-primitive keys forfeit the proof — nothing enumerates their domain.

```csharp
[SmartEnum<string>]
public sealed partial class Region
{
    public static readonly Region East = new("east");
    public static readonly Region West = new("west");
}

[SmartEnum<string>]
public sealed partial class Tier
{
    public static readonly Tier Basic = new("basic");
    public static readonly Tier Premium = new("premium");
}

public static class Rates
{
    private static readonly FrozenDictionary<(Region, Tier), decimal> Table =
        new Dictionary<(Region, Tier), decimal>
        {
            [(Region.East, Tier.Basic)] = 0.10m,
            [(Region.East, Tier.Premium)] = 0.25m,
            [(Region.West, Tier.Basic)] = 0.12m,
            [(Region.West, Tier.Premium)] = 0.28m,
        }.ToFrozenDictionary() switch
        {
            { Count: var n } table when n == Region.Items.Count * Tier.Items.Count => table,
            _ => throw new InvalidOperationException("<rate-table-incomplete>"),
        };

    public static decimal Of(Region region, Tier tier) => Table[(region, tier)];
}
```

- Cross-product keys ride value tuples for positional pairs and graduate to a readonly record struct once slots deserve names; both hash structurally through the items' generated equality, so vocabulary values are table-key-safe by construction and never need a registered comparer.
- Frozen construction specializes the implementation to the key population at build time and is paid once at static initialization — the read-optimized trade is the declared reason a policy table is a static singleton and never rebuilt per scope; string-keyed tables additionally expose span-keyed alternate lookup, deleting the allocation on parse-shaped probe paths.

[MUTATION_REENTRY]:
- Generated owners delete the external update surface structurally: constructors private, `init` accessors forced private, members forced required and read-only — so object-initializer construction, member rebinding, and `with`-style cloning are inexpressible outside the owner, even on struct owners where the language otherwise grants `with` to every struct. Every derived state is a new admission: the only lawful update is an owner-published verb that routes through the factory and re-enters validation.
- Re-validation cost makes churn rate a placement discriminant: a value rewritten per frame or per solver iteration does not belong in a validated owner — accumulation lives in a plain record or struct, and admission happens once at the seam where the result becomes domain material. Admitting inside the hot loop pays the invariant on every step for evidence nothing consumes mid-loop; admitting never leaves the seam unguarded. The decision variable is reads-of-evidence per write, not type aesthetics.

[FAULT_VOCABULARY]:
- One `Validate` implementation fans out into four framework-native fault shapes, so boundary catch policy is owner-kind dispatch whether or not it is written that way: the throwing factory wraps the error text in the data-annotations validation exception, parse-interface routes wrap it in a format exception, the generated JSON converter wraps it in the JSON exception, and the throwing vocabulary lookup raises its own unknown-identifier exception carrying the enum type and offending value as data rather than flattened text. A boundary that catches by exception type around heterogeneous owners must enumerate all four or leak one route.
- Custom validation-error types are constrained by the message-only funnel: the static creation contract takes a bare string, and every framework-origin failure — parse, deserialization, conversion — arrives through it. Structured error payloads survive only on routes the owner itself populates inside factory validation, so an error vocabulary with mandatory structured fields lies on framework routes; every custom error type needs a designed degenerate text case, and code consuming the structured fields must treat them as optional evidence.

[WIRE_PRESENTATION]:
- Generated codecs attach as type-level converter attributes pointing at either a shared runtime converter factory or an owner-specific file-local converter class — codec machinery adds zero public API surface, and the file-local form is unreachable even within the same assembly. The owner's public surface stays purely domain-shaped while remaining fully wire-capable.
- Presentation policy stays host-owned even through generated converters: the structural-owner converter consults the serializer's default-ignore condition for null and default member emission and resolves per-member custom converters from serializer options at runtime — a structural owner's wire object can therefore differ per host configuration while its admitted shape cannot. The split is exact: evidence (which members exist, which values are legal) is decided at the owner; emission (which members appear, how values render) is decided at the edge — and any pressure to push emission policy onto the owner is the serialization-separation violation in its subtlest form.
