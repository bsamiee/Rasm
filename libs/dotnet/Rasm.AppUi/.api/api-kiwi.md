# [RASM_APPUI_API_KIWI]

`Kiwi` is the managed C# port of the Cassowary incremental linear-arithmetic constraint solver: `Variable` values feed `Term`/`Expression` linear forms, a `Constraint` binds an expression to a `RelationalOperator` at a `Strength`, and `Solver` keeps the constraint system satisfied incrementally — `AddConstraint`/`RemoveConstraint` edit the system, `AddEditVariable`/`SuggestValue` drive runtime values through the dual-simplex, and `UpdateVariables` writes solved values into each variable's `IVariableStore`. Its pure-managed Apache-2.0 surface serves the AppUi Shell/Layout rail with no native dependency.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Kiwi`
- package: `Kiwi` (Apache-2.0)
- assembly: `Kiwi`
- namespace: `Nanoray.Kiwi`
- asset: runtime library (pure managed, zero native dependency)
- build-floor: ships `lib/net5.0` + `lib/net6.0`; the `net10.0` consumer binds `lib/net6.0` (highest TFM ≤ consumer), which is the surface documented here
- rail: layout

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: solver and constraint owners
- rail: layout

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]         | [RAIL]                            |
| :-----: | :------------------- | :-------------------- | :-------------------------------- |
|  [01]   | `Solver`             | sealed class          | incremental constraint system     |
|  [02]   | `Constraint`         | handle-equality class | expression-operator-strength bind |
|  [03]   | `Strength`           | static class          | constraint priority constants     |
|  [04]   | `RelationalOperator` | enum                  | equality/inequality selector      |

[PUBLIC_TYPE_SCOPE]: linear expression carriers
- rail: layout

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]          | [RAIL]                          |
| :-----: | :--------------- | :--------------------- | :------------------------------ |
|  [01]   | `Variable`       | sealed class           | solved unknown, operator source |
|  [02]   | `Term`           | readonly record struct | variable times coefficient      |
|  [03]   | `Expression`     | readonly struct        | terms plus constant linear form |
|  [04]   | `IVariableStore` | interface              | variable value storage contract |
|  [05]   | `VariableStore`  | sealed class           | default in-memory value store   |

[PUBLIC_TYPE_SCOPE]: operator cases
- rail: layout

| [INDEX] | [SYMBOL]                                | [TYPE_FAMILY] | [RAIL]                      |
| :-----: | :-------------------------------------- | :------------ | :-------------------------- |
|  [01]   | `RelationalOperator.Equal`              | enum case     | both sides equal            |
|  [02]   | `RelationalOperator.LessThanOrEqual`    | enum case     | left bounded above by right |
|  [03]   | `RelationalOperator.GreaterThanOrEqual` | enum case     | left bounded below by right |

[PUBLIC_TYPE_SCOPE]: solver failure rails
- rail: layout

| [INDEX] | [SYMBOL]                           | [TYPE_FAMILY] | [RAIL]                            |
| :-----: | :--------------------------------- | :------------ | :-------------------------------- |
|  [01]   | `DuplicateConstraintException`     | exception     | constraint already added          |
|  [02]   | `UnknownConstraintException`       | exception     | remove of an unadded constraint   |
|  [03]   | `UnsatisfiableConstraintException` | exception     | required constraint cannot hold   |
|  [04]   | `DuplicateEditVariableException`   | exception     | edit variable already attached    |
|  [05]   | `UnknownEditVariableException`     | exception     | suggest/remove without edit       |
|  [06]   | `NonLinearExpressionException`     | exception     | nonlinear constraint expression   |
|  [07]   | `InternalSolverException`          | exception     | internal simplex invariant breach |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: solver constraint and edit-variable operations
- rail: layout

| [INDEX] | [SURFACE]                                    | [ENTRY_FAMILY]   | [RAIL]                          |
| :-----: | :------------------------------------------- | :--------------- | :------------------------------ |
|  [01]   | `AddConstraint(Constraint)`                  | throwing mutate  | add constraint to system        |
|  [02]   | `TryAddConstraint(Constraint)`               | guarded mutate   | non-throwing add, `bool` result |
|  [03]   | `RemoveConstraint(Constraint)`               | throwing mutate  | remove constraint from system   |
|  [04]   | `TryRemoveConstraint(Constraint)`            | guarded mutate   | non-throwing remove             |
|  [05]   | `HasConstraint(Constraint)`                  | membership query | constraint presence test        |
|  [06]   | `AddEditVariable(Variable, double strength)` | throwing mutate  | attach edit constraint          |
|  [07]   | `TryAddEditVariable(Variable, double)`       | guarded mutate   | non-throwing edit attach        |
|  [08]   | `RemoveEditVariable(Variable)`               | throwing mutate  | detach edit constraint          |
|  [09]   | `TryRemoveEditVariable(Variable)`            | guarded mutate   | non-throwing edit detach        |
|  [10]   | `HasEditVariable(Variable)`                  | membership query | edit-variable presence test     |
|  [11]   | `SuggestValue(Variable, double value)`       | throwing suggest | drive edit variable to value    |
|  [12]   | `TrySuggestValue(Variable, double)`          | guarded suggest  | non-throwing suggest            |

[ENTRYPOINT_SCOPE]: solver evaluation
- rail: layout

| [INDEX] | [SURFACE]           | [ENTRY_FAMILY] | [RAIL]                                       |
| :-----: | :------------------ | :------------- | :------------------------------------------- |
|  [01]   | `Solve()`           | solve          | dual-optimize then write variable values     |
|  [02]   | `UpdateVariables()` | flush          | write solved row constants into each `Store` |

[ENTRYPOINT_SCOPE]: constraint construction
- rail: layout

| [INDEX] | [SURFACE]                                                          | [ENTRY_FAMILY] | [RAIL]                            |
| :-----: | :----------------------------------------------------------------- | :------------- | :-------------------------------- |
|  [01]   | `new Constraint(Expression, RelationalOperator, double? strength)` | constructor    | expression-operator-strength bind |
|  [02]   | `new Constraint(Constraint other, double? strength)`               | constructor    | clone with restrengthened copy    |
|  [03]   | `Constraint.Equal(lhs, rhs, strength)`                             | static factory | `==` constraint from two sides    |
|  [04]   | `Constraint.LessEqual(lhs, rhs, strength)`                         | static factory | `<=` constraint from two sides    |
|  [05]   | `Constraint.GreaterEqual(lhs, rhs, strength)`                      | static factory | `>=` constraint from two sides    |
|  [06]   | `Constraint.Make(lhs, op, rhs, strength)` (15 overloads)           | static factory | operator-selected mixed-arg bind  |
|  [07]   | `Constraint.Expression` / `Operator` / `Strength` / `Violated`     | property       | constraint inspection             |

[ENTRYPOINT_SCOPE]: expression assembly
- rail: layout

`Term.Coefficient` defaults to `1.0`, and `Expression.Constant` defaults to `0.0`.

| [INDEX] | [SURFACE]                                                          | [ENTRY_FAMILY]      | [RAIL]                     |
| :-----: | :----------------------------------------------------------------- | :------------------ | :------------------------- |
|  [01]   | `new Variable(string? name)`                                       | constructor         | default in-memory store    |
|  [02]   | `new Variable(IVariableStore, string?)`                            | constructor         | injected value store       |
|  [03]   | `Variable.Value`                                                   | property            | solved value access        |
|  [04]   | `Variable.Name`                                                    | property            | mutable label              |
|  [05]   | `Variable.Store`                                                   | property            | value-store access         |
|  [06]   | `new Term(Variable, double Coefficient = 1.0)`                     | record constructor  | weighted variable          |
|  [07]   | `Term.Value`                                                       | property            | evaluated weighted value   |
|  [08]   | `new Expression(IReadOnlyCollection<Term>, double constant = 0.0)` | constructor         | terms plus constant        |
|  [09]   | `new Expression(Term, double)`                                     | constructor         | single-term form           |
|  [10]   | `new Expression(double)`                                           | constructor         | constant-only form         |
|  [11]   | `implicit operator Expression(Variable)`                           | implicit conversion | variable to expression     |
|  [12]   | `implicit operator Expression(Term)`                               | implicit conversion | term to expression         |
|  [13]   | `implicit operator Expression(double)`                             | implicit conversion | scalar to expression       |
|  [14]   | `Variable`/`Term`/`Expression` operator overloads                  | operator algebra    | linear-form composition    |
|  [15]   | `Expression.Value`                                                 | property            | evaluated expression value |
|  [16]   | `Expression.Terms`                                                 | property            | `IReadOnlyList<Term>`      |
|  [17]   | `Expression.Constant`                                              | property            | constant term              |
|  [18]   | `Expression.IsConstant`                                            | property            | constant-form test         |
|  [19]   | `Strength.Required`                                                | static field        | required priority          |
|  [20]   | `Strength.Strong`                                                  | static field        | strong priority            |
|  [21]   | `Strength.Medium`                                                  | static field        | medium priority            |
|  [22]   | `Strength.Weak`                                                    | static field        | weak priority              |
|  [23]   | `Strength.Disabled`                                                | static field        | disabled priority          |
|  [24]   | `Strength.Create(a, b, c, w = 1.0)`                                | static method       | lexicographic assembly     |
|  [25]   | `Strength.Clip(value)`                                             | static method       | priority-range clamp       |

## [04]-[IMPLEMENTATION_LAW]

[KIWI_TOPOLOGY]:
- Linear-form assembly is operator-driven: `Variable * double` yields a `Term`, `Term + double`/`Variable + Variable`/`Term + Variable` yield an `Expression`, and every arithmetic operator (`+`, `-`, `*`, `/`, unary `-`) on `Variable`/`Term`/`Expression` returns one of those three carriers, so layout expressions compose without builder calls.
- `Variable`, `Term`, and `double` each carry an `implicit operator Expression`, so a bare carrier passes directly into `Constraint.Equal`/`LessEqual`/`GreaterEqual`/`Make` and `AddEditVariable` without an explicit wrap.
- `Term` is a `readonly record struct (Variable Variable, double Coefficient = 1.0)` whose `Value` is `Variable.Value * Coefficient`; `Expression` is a `readonly struct` whose public `Terms` is `IReadOnlyList<Term>` (backed by an internal `ImmutableArray<Term>`) and whose `double Constant` joins each `Term.Value` in `Value`, with `IsConstant` true when no terms remain.
- `Constraint` binds a reduced `Expression` to a `RelationalOperator` at a clipped `double` strength; the constraint normalizes both sides into `expression <op> 0`, so `Equal`/`LessEqual`/`GreaterEqual`/`Make` all subtract `rhs` from `lhs` before constructing.
- `Constraint` identity is handle-based: `Equals`, `==`, and `GetHashCode` compare the internal shared `ConstraintData` instance, not structural expression/operator/strength equality, so two separately constructed but value-equivalent constraints are not equal and the clone constructor without a new strength shares the same data handle.
- `Strength` is a lexicographic packing of strong/medium/weak components scaled by `1e6`/`1e3`/`1`: `Required = Create(1000, 1000, 1000)`, `Strong = Create(1, 0, 0)`, `Medium = Create(0, 1, 0)`, `Weak = Create(0, 0, 1)`, `Disabled = 0.0`; `Clip` bounds any strength to `[Disabled, Required]`.
- `Solver` keeps the constraint system in a dual-simplex tableau; `AddConstraint`/`RemoveConstraint` incrementally edit rows, `AddEditVariable` attaches an edit row whose value `SuggestValue` drives, `Solve` runs `DualOptimize` then `UpdateVariables`, and `UpdateVariables` writes each solved row constant back into the variable's `IVariableStore.Value` after flushing zero-reference variables.
- Variable storage is indirected through `IVariableStore`: `Variable.Value` reads and writes `Store.Value`, the default `VariableStore` is a plain in-memory `double` cell, and a custom `IVariableStore` lets a layout node observe solved values without polling.

[LOCAL_ADMISSION]:
- Layout geometry edges (panel left/top/width/height) are `Variable` values; layout rules compose as `Expression` operator algebra and bind through `Constraint.Equal`/`LessEqual`/`GreaterEqual` at the matching `Strength`, never through hand-built tableau rows.
- Fixed structural rules use `Strength.Required`; competing preferences use `Strong`/`Medium`/`Weak` so the dual-simplex relaxes the lower-priority constraint instead of throwing.
- Runtime drag, resize, and content-size changes flow through `AddEditVariable` and `SuggestValue`; the layout pass calls `Solve` once and reads solved positions from each `Variable.Value` (or a custom `IVariableStore` bound to the visual node).
- Boundary intake of constraint edits uses the `Try*` family (`TryAddConstraint`, `TryRemoveEditVariable`, `TrySuggestValue`); `UnsatisfiableConstraintException` and the duplicate/unknown rails never cross the layout-update boundary as exceptions.

[STACKING]:
- Live drive from the data rail projects `DynamicData` drag and resize deltas from `SourceCache.Edit` through `Transform` into `(Variable, double)` pairs; the subscription calls `Solver.TrySuggestValue` per delta and `Solver.Solve` once per frame.
- One `Connect().Bind(...)` observable drives both the visual collection and `Solve()` pass, so each edit re-flows both and constraint solving remains the live-data rail's sole mutation sink.
- Custom `IVariableStore` as the observation seam: implement `IVariableStore` over an Avalonia visual node's geometry (or a `ReactiveUI` property) so `UpdateVariables` writes solved row constants straight into the bound property on `Solve`, eliminating the post-solve polling loop; the layout owner reads positions from the node, not from a side dictionary.
- Strength-priority composition mirrors UI intent ranking: required structural invariants (`Strength.Required`), then theme/preference rules (`Strong`/`Medium`/`Weak`) so the dual-simplex relaxes a soft preference rather than throwing — the `Strength.Create(a,b,c,w)` lexicographic packing lets a screen express a continuum of competing constraints that map onto the same priority order a token/theme rail already defines.

[RAIL_LAW]:
- Package: `Kiwi`
- Owns: incremental linear-arithmetic constraint solving — `Variable`/`Term`/`Expression` linear-form algebra, `Constraint` binding at named `Strength` priorities, and the `Solver` dual-simplex with edit-variable `SuggestValue` runtime drive.
- Accept: operator-composed `Expression` constraints bound through `Equal`/`LessEqual`/`GreaterEqual`/`Make`; guarded `Try*` edits at the boundary; solved values read from `IVariableStore`.
- Reject: hand-rolled layout arithmetic outside the constraint system; structural value-equality on `Constraint` (identity is handle-based); a second managed solver wrapper renaming the `Solver`/`Constraint`/`Strength` surface.
