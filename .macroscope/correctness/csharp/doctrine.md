---
include:
  - "libs/csharp/**"
  - "**/*.cs"
---

# [CSHARP_DOCTRINE]

`docs/stacks/csharp/` is the floor for every C# surface — fence code judged as production on C# 14 / `net10.0`, nullable-enabled, warnings-as-errors, overflow-checked. Extreme ADT collapse through Thinktecture generated owners, LanguageExt rails, and one polymorphic entry per concern rule the shape; a conformant-but-weak form is a finding when a stronger form exists.

## [01]-[GENERATED_OWNERS]

- One concept owns exactly one closed family — `[Union]`, `[SmartEnum<TKey>]`, `[ValueObject<T>]`, `[ComplexValueObject]` — never sibling types; a flat `record` beside a generated owner sharing its identity regime, a per-relation class roster mirroring a foreign schema, and a success-or-failure `[Union]` (rails own outcomes) are findings.
- A uniform-payload `[Union]` whose every case carries one identical field collapses to the entrypoint argument plus a `[SmartEnum]` key.
- `Validate` is the admission factory bridging to rails via `[ValidationError<TCustom>]`; hand-written `Equals`/`GetHashCode` on a generated owner, a language `switch` ladder over smart-enum items where generated `Switch` exists, and `default`-minting a generated owner are findings.
- Smart-enum behavior rides `[UseDelegateFromConstructor]` columns; a row-to-row column reference is deferred (`static () => Row`), never eager.
- Each `Switch` arm constructs its own `Op.Of(name: nameof(CaseName))` provenance; a hoisted single `Op` shared across arms is the bug, not duplication to consolidate.

## [02]-[RAILS]

- Carrier ladder is `Option` < `Fin` < `Validation` < `Eff`/`IO`, chosen once at admission, threaded unchanged, collapsed only at the host, UI, native, command, or wire edge; `try`/`catch` around a rail transform and mid-pipeline `.Match`/`.Value` collapse inside a pure projection are findings.
- Independent faults accumulate applicatively (`.Apply`/`.Traverse`); a `Bind`/`from..select` chain over independent operands silently drops all but the first fault. `Validation<Seq<Error>, T>` and `Validation<string, T>` miss the required `Monoid<E>` and are findings.
- A `Fin` to `Option` to `Fin` round trip stamps `Errors.None` over the original fault; ad-hoc delay and retry loops where `Schedule` composes are findings.
- Composition-time policy — retry, recover, bracket — attaches as effect transformers whose stack order is a value (a `Concern` union with a rank column folded in order); a bracket-retry-catch tower hand-spelled per call site is the finding.

## [03]-[DISPATCH_AND_BOUNDARIES]

- A verb family is one request `[Union]` under one total generated `Switch`; arity collapses into `params ReadOnlySpan<T>`; sibling `Create`/`Update`/`CreateMany` methods, per-arity overload families, and `bool`/`mode`/`batch` parameters beside a value are findings. A `_` arm over a closed owned family is forbidden — a missing case must break the build; `_` is legal only over an open structural shape.
- Optional context is one `Option<T> x = default` consumed via `IfNone(policy)`; a nullable flag tail is the finding.
- Foreign material crosses exactly once: only the seam names a provider type, catches a provider exception, or holds a native lifetime, and every native crossing mints a closed `Fault` union case — a bare `Error.New(ex)` flattening a multi-cause crossing is a finding. `Some(null!)`, sentinels riding past the seam, and stored `Span<T>`/`ref struct` views are findings.
- Owner-to-DTO projection rides `[Mapper]`/`[MapProperty]` generation; a hand-written field-by-field projection where the generator owns it is a finding. `CultureInfo.InvariantCulture` pins wire and persisted seams.
- Numeric operands admit once through a finite/symmetry/singularity gate; operand shape selects the factorization via one `Route` `[SmartEnum]`; results leave as domain receipts carrying route, scale-derived tolerance, and recomputed true relative residual — never a `Matrix<T>` or factorization instance. A bare absolute tolerance literal and a `bool computeVectors` knob are findings.
- A runtime API replaces a local loop only when it owns the concern: hand-built static `Regex`, `string.Split` on a parse path, `TryGetValue`-then-`Add` double probes, `BitConverter` endian frames, and `[DllImport]` are findings — `[GeneratedRegex]`, `SearchValues<T>`, `FrozenDictionary`, `CollectionsMarshal`, `XxHash3`, and `[LibraryImport]` are the owners.

## [04]-[FORBIDDEN_SHAPES]

Shims, compat aliases, `[Obsolete]` layers, migration surfaces, alias-to-constant-to-enum-to-class chains, forwarding and helper shells, convenience wrappers, a function calling exactly one other function, and the same wrapper stack recurring across owners are findings — capability weaves into the owner as if always there, and a name resolves to its semantics in one hop. A member identifier equal to a BCL simple name (`Encoding`, `Path`, `Version`) qualifies inside its owner.
