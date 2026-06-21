# [RASM_APPUI_API_KIWI]

`Kiwi` is the managed C# port of the Cassowary incremental linear-arithmetic constraint solver: `Variable` values feed `Term`/`Expression` linear forms, a `Constraint` binds an expression to a `RelationalOperator` at a `Strength`, and `Solver` keeps the constraint system satisfied incrementally — `AddConstraint`/`RemoveConstraint` edit the system, `AddEditVariable`/`SuggestValue` drive runtime values through the dual-simplex, and `UpdateVariables` writes the solved values back into each variable's `IVariableStore`. The whole surface is pure-managed Apache-2.0 with no native dependency, serving the AppUi Shell/Layout rail as the layout constraint-solving engine.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Kiwi`
- package: `Kiwi`
- assembly: `Kiwi`
- namespace: `Nanoray.Kiwi`
- asset: runtime library (pure managed)
- license: Apache-2.0
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

| [INDEX] | [SURFACE]                                                       | [ENTRY_FAMILY]   | [RAIL]                               |
| :-----: | :-------------------------------------------------------------- | :--------------- | :----------------------------------- |
|  [01]   | `new Variable(string? name)`                                    | constructor      | unknown over default `VariableStore` |
|  [02]   | `Variable.Value` / `Name` / `Store`                             | property         | read solved value, label, storage    |
|  [03]   | `new Term(Variable, double Coefficient = 1.0)`                  | record ctor      | single weighted variable             |
|  [04]   | `new Expression(IReadOnlyCollection<Term>, double constant)`    | constructor      | terms plus constant linear form      |
|  [05]   | `Variable`/`Term`/`Expression` `+ - * /` operators              | operator algebra | fluent linear-form construction      |
|  [06]   | `Expression.Value` / `Terms` / `Constant` / `IsConstant`        | property         | expression inspection                |
|  [07]   | `Strength.Required` / `Strong` / `Medium` / `Weak` / `Disabled` | static field     | named priority constants             |
|  [08]   | `Strength.Create(a, b, c, w)` / `Clip(value)`                   | static method    | lexicographic strength assembly      |

## [04]-[IMPLEMENTATION_LAW]

[KIWI_TOPOLOGY]:
- Linear-form assembly is operator-driven: `Variable * double` yields a `Term`, `Term + double` and `Variable + Variable` yield an `Expression`, and every arithmetic operator (`+`, `-`, `*`, `/`, unary `-`) on `Variable`/`Term`/`Expression` returns one of those three carriers, so a layout expression composes through C# operators rather than builder calls.
- `Term` is a `readonly record struct (Variable Variable, double Coefficient = 1.0)`; `Expression` is a `readonly struct` holding an `ImmutableArray<Term>` plus a `double Constant`, with `Value` summing `Term.Value` (each `Variable.Value * Coefficient`) and the constant.
- `Constraint` binds a reduced `Expression` to a `RelationalOperator` at a clipped `double` strength; the constraint normalizes both sides into `expression <op> 0`, so `Equal`/`LessEqual`/`GreaterEqual`/`Make` all subtract `rhs` from `lhs` before constructing.
- `Constraint` identity is handle-based: `Equals`, `==`, and `GetHashCode` compare the internal shared `ConstraintData` instance, not structural expression/operator/strength equality, so two separately constructed but value-equivalent constraints are not equal and the clone constructor without a new strength shares the same data handle.
- `Strength` is a lexicographic packing of strong/medium/weak components scaled by `1e6`/`1e3`/`1`: `Required = Create(1000, 1000, 1000)`, `Strong = Create(1, 0, 0)`, `Medium = Create(0, 1, 0)`, `Weak = Create(0, 0, 1)`, `Disabled = 0.0`; `Clip` bounds any strength to `[Disabled, Required]`.
- `Solver` keeps the constraint system in a dual-simplex tableau; `AddConstraint`/`RemoveConstraint` incrementally edit rows, `AddEditVariable` attaches an edit row whose value `SuggestValue` drives, `Solve` runs `DualOptimize` then `UpdateVariables`, and `UpdateVariables` writes each solved row constant back into the variable's `IVariableStore.Value` after flushing zero-reference variables.
- Variable storage is indirected through `IVariableStore`: `Variable.Value` reads and writes `Store.Value`, the default `VariableStore` is a plain in-memory `double` cell, and a custom `IVariableStore` lets a layout node observe solved values without polling.

[LOCAL_ADMISSION]:
- Layout geometry edges (panel left/top/width/height) are `Variable` values; layout rules compose as `Expression` operator algebra and bind through `Constraint.Equal`/`LessEqual`/`GreaterEqual` at the matching `Strength`, never through hand-built tableau rows.
- Fixed structural rules use `Strength.Required`; competing preferences use `Strong`/`Medium`/`Weak` so the dual-simplex relaxes the lower-priority constraint instead of throwing.
- Runtime drag, resize, and content-size changes flow through `AddEditVariable` plus `SuggestValue`; the layout pass calls `Solve` once and reads solved positions from each `Variable.Value` (or a custom `IVariableStore` bound to the visual node).
- Boundary intake of constraint edits uses the `Try*` family (`TryAddConstraint`, `TryRemoveEditVariable`, `TrySuggestValue`); `UnsatisfiableConstraintException` and the duplicate/unknown rails never cross the layout-update boundary as exceptions.

[RAIL_LAW]:
- Package: `Kiwi`
- Owns: incremental linear-arithmetic constraint solving — `Variable`/`Term`/`Expression` linear-form algebra, `Constraint` binding at named `Strength` priorities, and the `Solver` dual-simplex with edit-variable `SuggestValue` runtime drive.
- Accept: operator-composed `Expression` constraints bound through `Equal`/`LessEqual`/`GreaterEqual`/`Make`; guarded `Try*` edits at the boundary; solved values read from `IVariableStore`.
- Reject: hand-rolled layout arithmetic outside the constraint system; structural value-equality on `Constraint` (identity is handle-based); a second managed solver wrapper renaming the `Solver`/`Constraint`/`Strength` surface.
