# [CSHARP_LANGUAGE]

C# 14.0 on `net10.0` is the active language surface. `Directory.Build.props` owns `TargetFramework`, `LangVersion`, `Nullable`, and `ImplicitUsings`; this page is the language-feature law for choosing syntax, type, member, pattern, and expression forms before adding a local abstraction.

## [01]-[ACTIVE_SURFACE]

[ACTIVE_SURFACE]:
- Target framework: `net10.0`
- Language version: `14.0` explicit; floating values are not allowed
- Nullable: `enable`
- Implicit usings: `enable`
- Compiler: .NET 10 SDK C# compiler
- Analyzer authoring: `Microsoft.CodeAnalysis.CSharp` with project analyzer packages

Treat source files as modern C#, not compatibility layers. Replace older syntax, overload families, wrapper types, and backing-field ceremony whenever the active surface carries the concept directly.

## [02]-[CANONICAL_CHOOSER]

Use the active C# surface directly. Replace older spellings and local machinery when syntax, the type system, or member shape owns the behavior.

[DECLARATION_AND_MEMBER_FORMS]: which declaration, member, or callable form carries the concept.

| [INDEX] | [CONCERN]                     | [USE]                                         | [REPLACE]                            |
| :-----: | :---------------------------- | :-------------------------------------------- | :----------------------------------- |
|  [01]   | receiver-owned members        | extension blocks                              | static helper classes, wrapper types |
|  [02]   | operators on foreign receiver | extension operators (non-conversion)          | named arithmetic helper methods      |
|  [03]   | property-local invariant      | `field` accessors                             | manual backing fields                |
|  [04]   | mandatory initialization      | `required` members                            | constructor telescoping              |
|  [05]   | immutable carrier             | `record` and `readonly record struct`         | hand-written equality classes        |
|  [06]   | nondestructive update         | `with` expressions                            | copy constructors, builders          |
|  [07]   | constructor boilerplate       | primary constructors                          | assign-only constructor bodies       |
|  [08]   | generated or hand splits      | partial members, constructors, events         | wrapper forwarding splits            |
|  [09]   | file-private machinery        | `file` types                                  | name-mangled `internal` types        |
|  [10]   | attribute payload type        | generic attributes                            | `typeof` constructor arguments       |
|  [11]   | construction abstraction      | static abstract and virtual interface members | reflection factories                 |
|  [12]   | numeric abstraction           | generic math constraints                      | per-type arithmetic overload copies  |
|  [13]   | compound mutation operators   | user-defined compound assignment              | expanded operator round trips        |
|  [14]   | generic identity              | `nameof` with unbound generics                | string literals, dummy closed names  |
|  [15]   | dense signature alias         | `using` alias for any type                    | parallel domain type names           |
|  [16]   | synchronous gate              | `System.Threading.Lock`                       | private `object` locks               |
|  [17]   | delegate adapter shape        | default and modifier-only lambda parameters   | wrapper adapter methods              |
|  [18]   | capture-free intent           | `static` anonymous functions                  | accidental closure allocation        |
|  [19]   | direct method reference       | natural-type method groups                    | identity lambda wrappers             |

[PATTERN_AND_EXPRESSION_FORMS]: value decisions, shape probes, and null flow stated as one expression.

| [INDEX] | [CONCERN]                  | [USE]                               | [REPLACE]                             |
| :-----: | :------------------------- | :---------------------------------- | :------------------------------------ |
|  [01]   | value-returning decision   | switch expressions                  | `if`/`else` ladders, statement switch |
|  [02]   | shape probe                | property patterns                   | getter chains with null checks        |
|  [03]   | range and sign law         | relational and logical patterns     | comparison chains                     |
|  [04]   | sequence shape             | list and slice patterns             | count and index guard code            |
|  [05]   | span text dispatch         | constant string patterns over spans | `ToString` comparisons                |
|  [06]   | type test with binding     | declaration and recursive patterns  | `as` plus null check                  |
|  [07]   | tuple dispatch             | positional patterns                 | nested conditionals                   |
|  [08]   | null test                  | `is null` and `is not null`         | `==` null with operator hazard        |
|  [09]   | exhaustiveness             | total switch over the closed owner  | default arms hiding cases             |
|  [10]   | inline result binding      | declaration patterns and `out var`  | pre-declared locals                   |
|  [11]   | repeated type spelling     | target-typed `new()`                | duplicated constructor type names     |
|  [12]   | null-coalescing update     | `??=`                               | if-null assignment blocks             |
|  [13]   | nullable mutation boundary | null-conditional assignment         | if-not-null mutation blocks           |
|  [14]   | end-relative access        | index `^` and range `..` operators  | length arithmetic                     |

[CONSTRUCTION_AND_KERNEL_FORMS]: how values are constructed, passed across arity and span boundaries, and laid out in measured kernels.

| [INDEX] | [CONCERN]                   | [USE]                                     | [REPLACE]                            |
| :-----: | :-------------------------- | :---------------------------------------- | :----------------------------------- |
|  [01]   | end-relative initialization | implicit index `[^1]` in initializers     | post-construction assignment loops   |
|  [02]   | literal composition         | collection expressions with spread        | `new[]`, list adds, concat chains    |
|  [03]   | call-site arity             | `params` collections                      | overload families                    |
|  [04]   | hot read-only input         | one `ReadOnlySpan<T>` boundary            | parallel array/string/span overloads |
|  [05]   | stack scratch               | `stackalloc` spans in measured kernels    | temporary heap arrays                |
|  [06]   | span-capable generics       | `allows ref struct` constraints           | boxed or duplicated span paths       |
|  [07]   | stack-only contracts        | `ref struct` interface implementations    | boxing interface conversions         |
|  [08]   | ref safety in coroutines    | `ref` and `unsafe` in iterators and async | extracted helper duplication         |
|  [09]   | struct construction freedom | auto-default structs                      | explicit `this = default` assignment |
|  [10]   | by-reference fields         | `ref` fields with `scoped` lifetimes      | pointer carriers                     |
|  [11]   | readonly by-ref contract    | `ref readonly` parameters                 | `in` and `ref` ambiguity             |
|  [12]   | residual overload ambiguity | `[OverloadResolutionPriority]`            | breaking renames, dummy parameters   |

[TEXT_LITERAL_FORMS]: structured text, wire constants, and terminal sequences stated as literals.

| [INDEX] | [CONCERN]                | [USE]                                     | [REPLACE]                      |
| :-----: | :----------------------- | :---------------------------------------- | :----------------------------- |
|  [01]   | embedded structured text | raw string literals                       | escape-laden concatenation     |
|  [02]   | UTF-8 wire constants     | `u8` literals                             | runtime UTF-8 encoding calls   |
|  [03]   | rich interpolation       | full expression grammar in interpolations | `string.Format`, concat chains |
|  [04]   | terminal escapes         | `\e` escape sequence                      | `\x1b` magic literals          |

## [03]-[LANGUAGE_FORM_CONTRACTS]

Use these contracts when the chooser names the form but code still needs a placement rule.

[EXTENSION_SURFACE_SITE]:
- Use when: behavior belongs to a receiver the declaring assembly does not own, or a wrapper type would only forward and decorate receiver behavior.
- Accept: extension blocks holding instance members, static members, and non-conversion operators on the receiver; one block per receiver shape.
- Reject: wrapper types that rename receiver behavior, static helper classes with receiver-first parameters, and extension conversion operators.
- Boundary: receiver-owned behavior that admits, validates, or dispatches domain state belongs to the owning domain shape, not to an extension surface.

```csharp conceptual
public static class VersionSurface {
    extension(Version source) {
        public bool Stable => source.Major >= 1;

        public Version Advanced(int generations) => new(source.Major + generations, 0);
    }

    extension(Version) {
        public static Version Origin => new(1, 0);

        public static Version operator +(Version left, int steps) =>
            new(left.Major, left.Minor + steps);
    }
}
```

[PATTERN_DISPATCH_SITE]:
- Use when: a value-returning decision can state its whole law as one total pattern expression.
- Accept: switch expressions over property, positional, relational, logical, list, slice, and constant-string-over-span patterns, with `var` bindings inside arms.
- Reject: `if`/`else` ladders, statement switches for value decisions, guard arms that leave cases unproved, and default arms that hide a missing case of a closed owner.
- Boundary: closed-family ownership and generated dispatch belong to the domain-shape owner; this site owns the pattern grammar itself.

```csharp conceptual
public readonly record struct Mark(string Key, int Rank);

public static class MarkPolicy {
    public static string Banded(ReadOnlySpan<Mark> marks) =>
        marks switch {
            [] => "<band-empty>",
            [(_, < 0), ..] => "<band-negative>",
            [.., { Rank: >= 9, Key: var key }] => $"<band-peak>:{key}",
            [{ Rank: >= 0 and < 9 } only] => $"<band-single>:{only.Key}",
            _ => "<band-mixed>",
        };

    public static int Routed(ReadOnlySpan<char> token) =>
        token switch {
            "<token-a>" => 1,
            "<token-b>" => 2,
            ['<', .. var body, '>'] => body.Length,
            _ => 0,
        };
}
```

[IMMUTABLE_CARRIER_SITE]:
- Use when: an inert carrier needs identity, mandatory initialization, or property-local invariants without a generated domain owner.
- Accept: `record` and `readonly record struct`, `required` plus `init`, `field` accessors for one-property invariants, and `with` expressions for nondestructive update.
- Reject: manual backing fields serving one accessor, constructor telescoping for mandatory members, copy constructors beside `with`, and hand-written equality on inert data.
- Boundary: carriers with admission, validation, vocabulary, or dispatch pressure graduate to generated domain owners.

```csharp conceptual
public readonly record struct Patch(int Offset, int Length);

public sealed record Profile {
    public required string Key { get; init; }

    public required Patch Window { get; init; }

    public int Weight {
        get;
        init => field = int.Max(value, 0);
    } = 1;

    public Profile Shifted(int delta) => this with {
        Weight = Weight + delta,
        Window = Window with { Offset = Window.Offset + delta },
    };
}
```

[COLLECTION_COMPOSITION_SITE]:
- Use when: composition, arity, and end-relative placement can be stated at the call or initialization site.
- Accept: collection expressions with spread, `params` collections over span and interface element shapes, and implicit `^` index assignment inside object initializers.
- Reject: `new[]` plus `Concat` chains, list-add ceremony, overload families that differ only by collection kind, and post-construction index assignment loops.
- Law: one signature carries every call shape; a legacy overload set made ambiguous by implicit span conversions is deleted, with `[OverloadResolutionPriority]` only as the residual tiebreaker during the collapse.
- Boundary: immutable domain collection identity belongs to the rail owner; this site owns construction shape.

```csharp conceptual
public sealed class Board {
    public int[] Cells { get; init; } = new int[8];
}

public static class BoardOps {
    public static ImmutableArray<int> Merged(ReadOnlySpan<int> head, params ReadOnlySpan<int> tail) =>
        [.. head, 0, .. tail];

    public static Board Framed(params IReadOnlyList<int> edges) => new() {
        Cells = { [0] = edges[0], [^1] = edges[^1] },
    };
}
```

[GENERIC_ALGEBRA_SITE]:
- Use when: one algorithm must hold across every type that proves the same construction and operator laws.
- Accept: static abstract and virtual interface members, the narrowest generic math constraint that carries the invariant, and user-defined compound assignment where mutation-shaped operators are the domain law.
- Reject: reflection factories, per-type arithmetic overload copies, runtime type switches inside numeric algorithms, and compound assignment that diverges from its expanded operator.
- Boundary: matrix, solver, and factorization selection belongs to the algorithm owner; this site owns the type-level algebra.

```csharp conceptual
public interface IMeasured<TSelf> where TSelf : IMeasured<TSelf> {
    static abstract TSelf Zero { get; }
    static abstract TSelf operator +(TSelf left, TSelf right);

    static virtual TSelf Total(IEnumerable<TSelf> values) =>
        values.Aggregate(TSelf.Zero, static (sum, value) => sum + value);
}

public record struct Extent(double Span) : IMeasured<Extent> {
    public static Extent Zero => new(Span: 0d);

    public static Extent operator +(Extent left, Extent right) => new(left.Span + right.Span);

    public void operator +=(Extent other) => Span += other.Span;
}
```

[TEXT_LITERAL_SITE]:
- Use when: structured text, wire constants, or terminal sequences must be stated as literals instead of being assembled at runtime.
- Accept: raw string literals for embedded structure, `u8` literals for UTF-8 wire constants, full expression grammar inside interpolations, and `\e` for terminal escapes in processed strings.
- Reject: escape-laden concatenation, runtime UTF-8 encoding of constants, `string.Format` for local interpolation, and `\x1b` magic literals.
- Boundary: grammar compilation, parsing policy, and codec selection belong to the system API owner; raw string literals do not process escapes, so terminal sequences stay in processed strings.

```csharp conceptual
public static class Wire {
    public static ReadOnlySpan<byte> Preamble => "<frame-a>"u8;

    public static ReadOnlySpan<byte> Reset => "\e[0m"u8;

    public static string Highlighted(string body) => $"\e[1m{body.Trim()}\e[0m";

    public static string Manifest(string key, int count) => $$"""
        {
          "key": "{{key}}",
          "count": {{int.Max(count, 0)}}
        }
        """;
}
```

[STACK_KERNEL_SITE]:
- Use when: a measured algorithm needs stack-only values, by-reference state, or span traversal that rail combinators cannot carry without allocation.
- Accept: `ref struct` owners with primary constructors, `ref` fields with `scoped` lifetimes, `allows ref struct` type parameters, `ref struct` interface implementations, and statement loops confined to the kernel body.
- Reject: boxing stack-only values through interface conversions, carrying `ref struct` state across `await` or `yield`, pointer carriers where `ref` fields express the lifetime, and kernel-style statements leaking into domain flow.
- Boundary: kernels are the named statement exemption; their public surface returns ordinary values or rails, and the kernel never escapes the owner.

```csharp conceptual
public interface IStep<TState> {
    TState Folded(TState state, int value);
}

public ref struct Probe : IStep<int> {
    private readonly ref int peak;

    public Probe(ref int peak) => this.peak = ref peak;

    public int Folded(int state, int value) => peak = int.Max(state, value);
}

public readonly ref struct Frame(ReadOnlySpan<int> values) {
    private readonly ReadOnlySpan<int> values = values;

    public TState Folded<TState, TStep>(TState seed, scoped ref TStep step)
        where TStep : IStep<TState>, allows ref struct {
        TState current = seed;
        foreach (int value in values) {
            current = step.Folded(current, value);
        }

        return current;
    }
}
```
