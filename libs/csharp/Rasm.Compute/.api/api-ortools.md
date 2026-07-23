# [RASM_COMPUTE_API_ORTOOLS]

`Google.OrTools` owns three exact-solver rails behind the `OptimizerKind` rows: the CP-SAT constraint-programming model and solver, the LinearSolver MIP/LP optimizer across pluggable backends, and the ConstraintSolver vehicle-routing engine, over per-RID native solver libraries. Its wire carriers (`CpModelProto`, `CpSolverResponse`, `MPModelProto`) are `api-protobuf` messages, so the proto vocabulary stacks onto the `Runtime/wire` remote lane; a solve fault lifts to `ComputeFault` at the boundary.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Google.OrTools`
- package: `Google.OrTools` (Apache-2.0)
- assembly: `Google.OrTools` (managed SWIG/protobuf wrapper, `lib/net8.0` bound on `net10.0`)
- namespace: `Google.OrTools.Sat`, `Google.OrTools.LinearSolver`, `Google.OrTools.ConstraintSolver`, `Google.OrTools.Graph`, `Google.OrTools.Util`, `Google.OrTools.OperationsResearch`
- asset: managed wrapper + per-RID native solver libs (`Google.OrTools.runtime.{osx-arm64,osx-x64,linux-arm64,linux-x64,win-x64}`); a solve with no matching RID payload faults at native load
- transitive: bundles `Google.Protobuf` for the proto carriers, composing `api-protobuf`
- rail: solver

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: CP-SAT model and value contracts

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]      | [CAPABILITY]                      |
| :-----: | :------------------------ | :----------------- | :-------------------------------- |
|  [01]   | `CpModel`                 | model root         | builds the CP-SAT proto           |
|  [02]   | `CpSolver`                | solver root        | runs CP-SAT and reads results     |
|  [03]   | `CpSolverStatus`          | status enum        | classifies the solve outcome      |
|  [04]   | `LinearExpr`              | expression root    | linear term algebra and operators |
|  [05]   | `IntVar`                  | integer variable   | discrete-domain decision variable |
|  [06]   | `BoolVar`                 | Boolean variable   | literal-bearing decision variable |
|  [07]   | `NotBoolVar`              | negated literal    | carries `BoolVar.Not()`           |
|  [08]   | `ILiteral`                | literal interface  | Boolean literal contract          |
|  [09]   | `IntervalVar`             | interval variable  | scheduling interval variable      |
|  [10]   | `BoundedLinearExpression` | relational carrier | carries a constrained expression  |
|  [11]   | `Constraint`              | constraint handle  | base CP-SAT constraint            |
|  [12]   | `SolutionCallback`        | callback base      | per-solution observation hook     |
|  [13]   | `CpSolverResponse`        | response proto     | full solve response               |
|  [14]   | `Domain`                  | domain value       | discrete variable domain          |

[PUBLIC_TYPE_SCOPE]: CP-SAT structural constraint families — each returned by the matching `CpModel.Add*` factory, refining `Constraint` with its own builder

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY]    | [CAPABILITY]                    |
| :-----: | :-------------------------- | :--------------- | :------------------------------ |
|  [01]   | `CircuitConstraint`         | graph constraint | single-circuit arc set          |
|  [02]   | `MultipleCircuitConstraint` | graph constraint | multi-circuit arc set           |
|  [03]   | `TableConstraint`           | table constraint | allowed/forbidden tuple set     |
|  [04]   | `AutomatonConstraint`       | automaton        | regular-language transition set |
|  [05]   | `ReservoirConstraint`       | reservoir        | level events over time          |
|  [06]   | `CumulativeConstraint`      | scheduling       | capacity over intervals         |
|  [07]   | `NoOverlap2dConstraint`     | scheduling       | 2D rectangle non-overlap        |

[PUBLIC_TYPE_SCOPE]: LinearSolver MIP/LP contracts

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY]      | [CAPABILITY]                        |
| :-----: | :-------------------------------- | :----------------- | :---------------------------------- |
|  [01]   | `Solver`                          | solver root        | MIP/LP model and solve              |
|  [02]   | `Solver.OptimizationProblemType`  | backend enum       | selects the native solver backend   |
|  [03]   | `Solver.ResultStatus`             | status enum        | classifies the MIP/LP outcome       |
|  [04]   | `Solver.BasisStatus`              | basis enum         | simplex basis position              |
|  [05]   | `Variable`                        | decision variable  | continuous/integer/boolean variable |
|  [06]   | `Constraint`                      | linear constraint  | bounded coefficient row             |
|  [07]   | `Objective`                       | objective handle   | linear objective and direction      |
|  [08]   | `LinearExpr`                      | expression root    | LinearSolver term algebra           |
|  [09]   | `LinearConstraint`                | relational carrier | carries a constrained expression    |
|  [10]   | `RangeConstraint`                 | relational carrier | two-sided bound carrier             |
|  [11]   | `Equality`                        | relational carrier | expression equality carrier         |
|  [12]   | `MPSolverParameters`              | parameter handle   | double/integer solver parameters    |
|  [13]   | `MPSolverParameters.DoubleParam`  | parameter enum     | tolerance and gap parameter keys    |
|  [14]   | `MPSolverParameters.IntegerParam` | parameter enum     | presolve/scaling/algorithm keys     |

[PUBLIC_TYPE_SCOPE]: `Google.OrTools.Graph` network-flow engines — each `IDisposable` over a native graph, taking `int` node/arc indices with `long` capacities/costs

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]        | [CAPABILITY]                                               |
| :-----: | :-------------------- | :------------------- | :--------------------------------------------------------- |
|  [01]   | `MaxFlow`             | max-flow engine      | push-relabel max-flow / min-cut                            |
|  [02]   | `MaxFlow.Status`      | status enum          | `OPTIMAL`/`POSSIBLE_OVERFLOW`/`BAD_INPUT`/`BAD_RESULT`     |
|  [03]   | `MinCostFlow`         | min-cost-flow engine | `: MinCostFlowBase` — min-cost flow / max-flow-at-min-cost |
|  [04]   | `MinCostFlowBase`     | flow base            | shared node/arc accessors + `Status` enum                  |
|  [05]   | `LinearSumAssignment` | assignment engine    | optimal bipartite min-cost assignment                      |

[PUBLIC_TYPE_SCOPE]: ConstraintSolver routing contracts

| [INDEX] | [SYMBOL]                               | [TYPE_FAMILY]      | [CAPABILITY]                     |
| :-----: | :------------------------------------- | :----------------- | :------------------------------- |
|  [01]   | `RoutingModel`                         | routing root       | vehicle-routing model and solve  |
|  [02]   | `RoutingIndexManager`                  | index manager      | node/index/vehicle mapping       |
|  [03]   | `RoutingSearchParameters`              | search parameters  | first-solution and metaheuristic |
|  [04]   | `RoutingModelParameters`               | model parameters   | underlying CP solver parameters  |
|  [05]   | `RoutingDimension`                     | dimension handle   | accumulated quantity over routes |
|  [06]   | `RoutingSearchStatus.Types.Value`      | status enum        | classifies the routing outcome   |
|  [07]   | `FirstSolutionStrategy.Types.Value`    | strategy enum      | first-solution construction      |
|  [08]   | `LocalSearchMetaheuristic.Types.Value` | metaheuristic enum | local-search improvement engine  |
|  [09]   | `Assignment`                           | assignment value   | solved route assignment          |
|  [10]   | `RoutingModel.ResourceGroup`           | resource group     | shared-resource constraints      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: CP-SAT model construction — instance methods on `CpModel`; `Add*` factories return the section-2 constraint families

| [INDEX] | [SURFACE]                                                        | [SHAPE]  | [CAPABILITY]                     |
| :-----: | :--------------------------------------------------------------- | :------- | :------------------------------- |
|  [01]   | `NewIntVar(long, long, string)`                                  | factory  | integer variable on `[lb, ub]`   |
|  [02]   | `NewIntVarFromDomain(Domain, string)`                            | factory  | integer variable on a domain     |
|  [03]   | `NewConstant(long)`                                              | factory  | constant integer variable        |
|  [04]   | `NewBoolVar(string)`                                             | factory  | Boolean variable                 |
|  [05]   | `TrueLiteral() / FalseLiteral()`                                 | factory  | constant literals                |
|  [06]   | `NewIntervalVar<S,D,E>(S, D, E, string)`                         | factory  | scheduling interval              |
|  [07]   | `NewOptionalIntervalVar<S,D,E>(S, D, E, ILiteral, string)`       | factory  | presence-gated interval          |
|  [08]   | `AddLinearConstraint(LinearExpr, long, long)`                    | instance | `lb ≤ expr ≤ ub`                 |
|  [09]   | `AddLinearExpressionInDomain(LinearExpr, Domain)`                | instance | expression in a domain           |
|  [10]   | `Add(BoundedLinearExpression)`                                   | instance | relational-operator constraint   |
|  [11]   | `AddAllDifferent(IEnumerable<LinearExpr>)`                       | instance | all-different                    |
|  [12]   | `AddElement(LinearExpr, exprs, LinearExpr)`                      | instance | indexed-element equality         |
|  [13]   | `AddCircuit() / AddMultipleCircuit()`                            | instance | circuit constraints              |
|  [14]   | `AddAllowedAssignments / AddForbiddenAssignments(exprs)`         | instance | table constraints                |
|  [15]   | `AddAutomaton(exprs, startState, finalStates)`                   | instance | automaton constraint             |
|  [16]   | `AddInverse(IEnumerable<IntVar>, reverse)`                       | instance | inverse permutation              |
|  [17]   | `AddReservoirConstraint(long, long)`                             | instance | reservoir constraint             |
|  [18]   | `AddImplication(ILiteral, ILiteral)`                             | instance | literal implication              |
|  [19]   | `AddBoolOr / AddBoolAnd / AddBoolXor(literals)`                  | instance | Boolean clause constraints       |
|  [20]   | `AddAtLeastOne / AddAtMostOne / AddExactlyOne(literals)`         | instance | cardinality clause constraints   |
|  [21]   | `AddMinEquality / AddMaxEquality(target, exprs)`                 | instance | min/max equality                 |
|  [22]   | `AddAbsEquality(LinearExpr, LinearExpr)`                         | instance | absolute-value equality          |
|  [23]   | `AddDivisionEquality<T,N,D> / AddModuloEquality<T,V,M>`          | instance | integer division/modulo equality |
|  [24]   | `AddMultiplicationEquality(target, left, right)`                 | instance | product equality                 |
|  [25]   | `AddNoOverlap(IEnumerable<IntervalVar>)`                         | instance | 1D interval non-overlap          |
|  [26]   | `AddNoOverlap2D() / AddCumulative<C>(C)`                         | instance | 2D non-overlap and cumulative    |
|  [27]   | `Minimize(LinearExpr) / Maximize(LinearExpr)`                    | instance | objective direction              |
|  [28]   | `AddDecisionStrategy(vars, varStr, domStr)`                      | instance | branching strategy               |
|  [29]   | `AddHint(IntVar, long) / AddHint(ILiteral, bool) / ClearHints()` | instance | solution hints                   |
|  [30]   | `AddAssumption / AddAssumptions / ClearAssumptions`              | instance | infeasibility-core assumptions   |
|  [31]   | `ModelStats() / Validate() / ExportToFile(string)`               | instance | stats, validation, proto export  |

[ENTRYPOINT_SCOPE]: CP-SAT reification and structural-family builders — every `Add*` returns a base `Constraint` (reifiable through `OnlyEnforceIf`) or a refined family with its own fluent builder

| [INDEX] | [SURFACE]                                                                      | [SHAPE]  | [CAPABILITY]                       |
| :-----: | :----------------------------------------------------------------------------- | :------- | :--------------------------------- |
|  [01]   | `Constraint.OnlyEnforceIf(ILiteral) / OnlyEnforceIf(ILiteral[])`               | instance | half-reify on a literal/and-set    |
|  [02]   | `Constraint.Proto / Constraint.Index`                                          | property | underlying `ConstraintProto`/index |
|  [03]   | `ReservoirConstraint.AddEvent<T,L>(T, L)`                                      | instance | fixed level event                  |
|  [04]   | `ReservoirConstraint.AddOptionalEvent<T,L>(T, L, ILiteral)`                    | instance | presence-gated level event         |
|  [05]   | `CumulativeConstraint.AddDemand<D>(IntervalVar, D)`                            | instance | one interval's resource demand     |
|  [06]   | `CumulativeConstraint.AddDemands<D>(IEnumerable<IntervalVar>, IEnumerable<D>)` | instance | batch of interval demands          |
|  [07]   | `NoOverlap2dConstraint.AddRectangle(IntervalVar, IntervalVar)`                 | instance | x/y interval rectangle to the pack |
|  [08]   | `AutomatonConstraint.AddTransition(int, int, long)`                            | instance | one labeled state transition       |

[ENTRYPOINT_SCOPE]: `Domain` value algebra — the `Google.OrTools.Util` discrete-set type behind `NewIntVarFromDomain`/`AddLinearExpressionInDomain`, a full set algebra and `IDisposable`

| [INDEX] | [SURFACE]                                                                         | [SHAPE]  | [CAPABILITY]                    |
| :-----: | :-------------------------------------------------------------------------------- | :------- | :------------------------------ |
|  [01]   | `new Domain(long) / new Domain(long, long)`                                       | ctor     | singleton or single-interval    |
|  [02]   | `Domain.FromValues(long[]) / FromIntervals(long[][]) / FromFlatIntervals(long[])` | static   | explicit value or interval set  |
|  [03]   | `Domain.AllValues() / LowerOrEqual(long) / GreaterOrEqual(long)`                  | static   | half-open and universal domains |
|  [04]   | `Domain.Complement() / IntersectionWith / UnionWith`                              | instance | domain set operations           |
|  [05]   | `Domain.Contains(long) / IsIncludedIn(Domain) / OverlapsWith(Domain)`             | instance | membership and containment      |
|  [06]   | `Domain.Min() / Max() / Size() / IsEmpty() / FlattenedIntervals()`                | instance | bounds, cardinality, flat form  |

[ENTRYPOINT_SCOPE]: CP-SAT solve and result projection — `CpSolver` members; `Value`/`BooleanValue` read the best solution after `Solve`

| [INDEX] | [SURFACE]                                                               | [SHAPE]  | [CAPABILITY]                         |
| :-----: | :---------------------------------------------------------------------- | :------- | :----------------------------------- |
|  [01]   | `Solve(CpModel, SolutionCallback?) -> CpSolverStatus`                   | instance | runs CP-SAT, returns the status      |
|  [02]   | `StringParameters { get; set; }`                                        | property | proto-text `SatParameters` string    |
|  [03]   | `StopSearch()`                                                          | instance | asynchronous search stop             |
|  [04]   | `Value(IntVar) / Value(LinearExpr)`                                     | instance | integer solution value               |
|  [05]   | `BooleanValue(ILiteral)`                                                | instance | literal solution value               |
|  [06]   | `ObjectiveValue / BestObjectiveBound`                                   | property | objective and bound                  |
|  [07]   | `Response`                                                              | property | full `CpSolverResponse`              |
|  [08]   | `NumBranches() / NumConflicts() / WallTime()`                           | instance | search statistics                    |
|  [09]   | `SufficientAssumptionsForInfeasibility()`                               | instance | infeasibility-core assumption subset |
|  [10]   | `ResponseStats() / SolveLog() / SolutionInfo()`                         | instance | response text, log, solution info    |
|  [11]   | `SetLogCallback(StringToVoidDelegate) / ClearLogCallback()`             | instance | log streaming                        |
|  [12]   | `SetBestBoundCallback(DoubleToVoidDelegate) / ClearBestBoundCallback()` | instance | bound streaming                      |
|  [13]   | `Dispose()`                                                             | instance | releases native solver handles       |

[ENTRYPOINT_SCOPE]: `LinearExpr` term algebra — static factories and operators build `LinearExpr` (arithmetic) or `BoundedLinearExpression` (relational) for intake by `CpModel.Add`

| [INDEX] | [SURFACE]                                         | [SHAPE]  | [CAPABILITY]                     |
| :-----: | :------------------------------------------------ | :------- | :------------------------------- |
|  [01]   | `Sum(IEnumerable<LinearExpr\|ILiteral\|BoolVar>)` | static   | sum of terms                     |
|  [02]   | `WeightedSum(exprs, IEnumerable<int\|long>)`      | static   | weighted sum                     |
|  [03]   | `Term(LinearExpr\|ILiteral\|BoolVar, long)`       | static   | scaled single term               |
|  [04]   | `Affine(expr, long, long)`                        | static   | `coeff * expr + offset`          |
|  [05]   | `Constant(long)`                                  | static   | constant expression              |
|  [06]   | `NewBuilder(int)`                                 | static   | mutable `LinearExprBuilder`      |
|  [07]   | `+` `-` `*` (unary `-`)                           | operator | arithmetic over `LinearExpr`     |
|  [08]   | `==` `!=` `>=` `>` `<=` `<`                       | operator | builds `BoundedLinearExpression` |

[ENTRYPOINT_SCOPE]: LinearSolver model and solve — `Solver` members; `MakeVar*`/`MakeConstraint` build variable and constraint handles

| [INDEX] | [SURFACE]                                                          | [SHAPE]  | [CAPABILITY]                      |
| :-----: | :----------------------------------------------------------------- | :------- | :-------------------------------- |
|  [01]   | `CreateSolver(string)`                                             | static   | backend solver by id string       |
|  [02]   | `Solver(string, OptimizationProblemType)`                          | ctor     | backend solver by enum            |
|  [03]   | `SupportsProblemType(OptimizationProblemType)`                     | static   | backend availability              |
|  [04]   | `MakeNumVar / MakeIntVar / MakeBoolVar(.., string)`                | factory  | continuous/integer/boolean        |
|  [05]   | `MakeVar(double, double, bool, string)`                            | factory  | typed variable                    |
|  [06]   | `MakeVarArray / MakeNumVarMatrix / MakeBoolVarMatrix`              | factory  | bulk variable arrays and matrices |
|  [07]   | `MakeConstraint(double, double, string)`                           | factory  | bounded constraint row            |
|  [08]   | `Add(LinearConstraint)`                                            | instance | relational-expression constraint  |
|  [09]   | `Objective()`                                                      | instance | the model objective handle        |
|  [10]   | `Minimize / Maximize(LinearExpr\|Variable)`                        | instance | objective direction               |
|  [11]   | `Solve() / Solve(MPSolverParameters) -> ResultStatus`              | instance | runs solve, returns the status    |
|  [12]   | `Value(Variable) / ReducedCost(Variable)`                          | instance | primal value and reduced cost     |
|  [13]   | `DualValue(LinearConstraint) / Activity(LinearConstraint)`         | instance | dual value and row activity       |
|  [14]   | `ObjectiveValue / BestObjectiveBound`                              | property | objective and bound               |
|  [15]   | `SetTimeLimit(long) / SetNumThreads(int)`                          | instance | time limit and thread count       |
|  [16]   | `SetHint(MPVariableVector, double[])`                              | instance | warm-start hint                   |
|  [17]   | `InterruptSolve() / EnableOutput() / SuppressOutput()`             | instance | interrupt and logging             |
|  [18]   | `Iterations() / Nodes() / WallTime()`                              | instance | simplex/branch statistics         |
|  [19]   | `ExportModelAsLpFormat(bool) / ExportModelAsMpsFormat(bool, bool)` | instance | LP/MPS text export                |
|  [20]   | `VerifySolution(double, bool)`                                     | instance | feasibility verification          |
|  [21]   | `SetSolverSpecificParametersAsString(string) / SolverVersion()`    | instance | backend-native passthrough        |

[ENTRYPOINT_SCOPE]: ConstraintSolver routing model and solve — `RoutingIndexManager` maps caller nodes to solver indices; callbacks register through index-typed delegates

| [INDEX] | [SURFACE]                                             | [SHAPE]  | [CAPABILITY]                   |
| :-----: | :---------------------------------------------------- | :------- | :----------------------------- |
|  [01]   | `RoutingIndexManager(int, int, int)`                  | ctor     | single-depot index manager     |
|  [02]   | `RoutingIndexManager(int, int, int[], int[])`         | ctor     | multi-depot index manager      |
|  [03]   | `RoutingModel(RoutingIndexManager)`                   | ctor     | routing model                  |
|  [04]   | `RegisterTransitCallback(LongLongToLong)`             | instance | arc-cost/transit evaluator     |
|  [05]   | `RegisterUnaryTransitCallback(LongToLong)`            | instance | unary transit evaluator        |
|  [06]   | `RegisterTransitMatrix(long[][])`                     | instance | matrix transit evaluator       |
|  [07]   | `SetArcCostEvaluatorOfAllVehicles(int)`               | instance | global arc cost                |
|  [08]   | `SetArcCostEvaluatorOfVehicle(int, int)`              | instance | per-vehicle arc cost           |
|  [09]   | `AddDimension(int, long, long, bool, string)`         | instance | capacity dimension             |
|  [10]   | `AddDimensionWithVehicleCapacity(.., long[], ..)`     | instance | per-vehicle capacity dimension |
|  [11]   | `GetDimensionOrDie(string)`                           | instance | dimension lookup               |
|  [12]   | `AddDisjunction(long[], long)`                        | instance | optional-visit disjunction     |
|  [13]   | `AddPickupAndDelivery(long, long)`                    | instance | pickup-delivery pairing        |
|  [14]   | `SetFixedCostOfVehicle(long, int)`                    | instance | per-vehicle fixed cost         |
|  [15]   | `SolveWithParameters(RoutingSearchParameters)`        | instance | runs routing search            |
|  [16]   | `SetVisitType / AddHardTypeIncompatibility(int, int)` | instance | visit-type regulations         |
|  [17]   | `AddResourceGroup()`                                  | instance | shared-resource group          |

[ENTRYPOINT_SCOPE]: `Google.OrTools.Graph` max-flow / min-cost-flow / assignment — build the arc set imperatively (each `Add*` returns the arc index), `Solve` returns the `Status`, then read the optimal value and per-arc flow

| [INDEX] | [SURFACE]                                                                            | [SHAPE]  | [CAPABILITY]                    |
| :-----: | :----------------------------------------------------------------------------------- | :------- | :------------------------------ |
|  [01]   | `MaxFlow.AddArcWithCapacity(int, int, long)`                                         | instance | one capacitated arc, its index  |
|  [02]   | `MaxFlow.SetArcCapacity(int, long)`                                                  | instance | reset an arc for a re-solve     |
|  [03]   | `MaxFlow.Solve(int, int) -> Status`                                                  | instance | push-relabel over source/sink   |
|  [04]   | `MaxFlow.OptimalFlow() / Flow(int)`                                                  | instance | max-flow value / per-arc flow   |
|  [05]   | `MinCostFlow(int[, int])`                                                            | ctor     | pre-sized min-cost-flow engine  |
|  [06]   | `MinCostFlow.AddArcWithCapacityAndUnitCost(int, int, long, long)`                    | instance | one capacitated, priced arc     |
|  [07]   | `MinCostFlow.SetNodeSupply(int, long)`                                               | instance | node supply (+) / demand (−)    |
|  [08]   | `MinCostFlow.Solve() / SolveMaxFlowWithMinCost() -> Status`                          | instance | min-cost / max-flow-at-min-cost |
|  [09]   | `MinCostFlow.OptimalCost() / MaximumFlow() / Flow(int)`                              | instance | cost / total flow / per-arc     |
|  [10]   | `LinearSumAssignment.AddArcWithCost(int, int, long)`                                 | instance | one left→right assignment arc   |
|  [11]   | `LinearSumAssignment.Solve() / OptimalCost() / RightMate(int) / AssignmentCost(int)` | instance | assignment, cost, mate          |

- `MaxFlow.Flow`: saturated arcs (`Flow == Capacity`) read the min-cut.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- expression algebra: arithmetic operators build `LinearExpr`, relational operators (`==`/`!=`/`>=`/`>`/`<=`/`<`) build `BoundedLinearExpression`, and `CpModel.Add(BoundedLinearExpression)` intakes the relational carrier — a model composes the operator algebra, never a parallel constraint DSL.
- variable hierarchy: `BoolVar : IntVar : LinearExpr`, and `BoolVar.Not()` returns a `NotBoolVar` literal, so every Boolean variable folds directly into integer expressions and clauses.
- reification: `Constraint.OnlyEnforceIf(ILiteral)` half-reifies any constraint, and the structural families refine `Constraint` with fluent builders (`ReservoirConstraint.AddEvent`, `CumulativeConstraint.AddDemand`, `NoOverlap2dConstraint.AddRectangle`, `AutomatonConstraint.AddTransition`), so a scheduling/packing model composes the builder rather than a parallel constraint type.
- domain algebra: `Domain` is a closed set value (`FromValues`/`FromIntervals`/`Complement`/`IntersectionWith`/`UnionWith`/`Contains`/`IsIncludedIn`); a discretization expresses a variable's admissible set through the algebra, never an external bit-set beside it.
- backend selection: `Solver.OptimizationProblemType` picks the native backend (`GLOP`/`CLP`/`PDLP`/`SCIP`/`CBC`/`BOP`/`SAT`, optional `GUROBI`/`CPLEX`/`XPRESS`/`GLPK`) or `CreateSolver(id)`; LP relaxations expose `ReducedCost`/`DualValue`/`Activity` over `Solver.BasisStatus`.
- solve outcome: each rail classifies to its own status, never a generic success flag — CP-SAT `CpSolverStatus` (`Unknown`/`ModelInvalid`/`Feasible`/`Infeasible`/`Optimal`), LinearSolver `Solver.ResultStatus` (`OPTIMAL`/`FEASIBLE`/`INFEASIBLE`/`UNBOUNDED`/`ABNORMAL`/`MODEL_INVALID`/`NOT_SOLVED`), routing `RoutingSearchStatus.Types.Value`.
- native binding: the managed wrapper is a SWIG/protobuf surface, and every `IDisposable` root (`CpSolver`, `Solver`, `RoutingModel`, `RoutingIndexManager`, `MPSolverParameters`, `Objective`, `Domain`) owns native handles released by `Dispose`.
- proto carriers: `CpModel.Model` (`CpModelProto`), `CpSolver.Response` (`CpSolverResponse`), `Constraint.Proto` (`ConstraintProto`), and `MPModelProto`/`MPModelRequest` (`Google.OrTools.OperationsResearch`) are the `api-protobuf` wire model and response.

[STACKING]:
- `api-protobuf`(`libs/csharp/.api/api-protobuf.md`): `CpModelProto`/`CpSolverResponse`/`MPModelProto` are `Google.Protobuf` messages, so a solve crosses the `Runtime/wire#PROTO_VOCABULARY` lane on the `api-protobuf` codec and stages serialized bytes through the `api-recyclable-stream` pool — no managed solve DTO beside the proto.
- `api-recyclable-stream`(`api-recyclable-stream.md`): serialized solve bytes rent a pooled `RecyclableMemoryStream` at the wire edge, never an ad hoc array.
- `Solver/optimizer#OPTIMIZER_LANE`: `OptimizerKind` rows carry `cp-sat`/`milp`, lowering the typed `DesignProblem` to `CpModel`/`Solver` through the typed builder (`NewIntVar`/`MakeIntVar`/`MakeNumVar` over `DesignVariable` cases, never a string-parsed model); one `Optimize` fold discriminates on the row, and backend policy — `Solver.OptimizationProblemType`, the `solver_id` string, `CpSolver.StringParameters` `SatParameters` text, `SetSolverSpecificParametersAsString` — rides the row as data.
- `Runtime/scheduling`: a NodaTime `Duration` folds to `Solver.SetTimeLimit(long)` / the `max_time_in_seconds` `SatParameters` key, and the solve elapsed (`WallTime()`) stamps the typed receipt.
- `Runtime/progress`: `CpSolver.SetLogCallback`/`SetBestBoundCallback` and a `SolutionCallback` subclass stream search progress to the `Stats` sink, and `StopSearch()`/`InterruptSolve()` honor the channel deadline.
- `Analysis/circulation`: `Google.OrTools.Graph.MaxFlow` composes exit capacity for the egress runner — occupant-load supplies map onto space nodes, door/corridor widths onto arc capacities of the `ElementGraph` space-adjacency subgraph, `Solve(source, sink)` returns the evacuation throughput, and saturated arcs (`Flow == Capacity`) name the min-cut; `MinCostFlow` distributes load at least travel cost. Path/topology algebra is `QuikGraph`'s and the planar side `NetTopologySuite`/`Clipper2`'s; the flow concern alone is this module's, zero new central pins.

[LOCAL_ADMISSION]:
- `OptimizerKind` rows select the rail — CP-SAT through `CpModel`/`CpSolver`, MIP/LP through `Solver`, routing through `RoutingModel`; one canonical solve discriminates on the row.
- Variables, constraints, objective, and solve each emit typed receipts, and the status enums classify the outcome.
- Native handles enter only through declared `IDisposable` roots and release deterministically; the SWIG `SWIGTYPE_p_*`/`*PINVOKE` interop types stay out of canonical owners.

[RAIL_LAW]:
- Package: `Google.OrTools`
- Owns: CP-SAT constraint programming (reification, structural-family builders, `Domain` algebra), MIP/LP exact optimization across pluggable backends, vehicle-routing search, and the `Google.OrTools.Graph` network-flow engines (max-flow/min-cut, min-cost-flow, linear-sum-assignment); the proto carriers are `api-protobuf` messages
- Accept: declared decision variables, typed constraints reified through `OnlyEnforceIf`, admissible sets as `Domain` algebra, and an objective solved to a classified status — stacked onto the `OptimizerKind` row, the proto wire, and the NodaTime deadline budget
- Reject: hand-rolled branch-and-bound, simplex, or routing search; float-equality feasibility outside the solver; per-backend solve entrypoints where one `Solve` discriminates on `OptimizerKind`; a managed solve DTO beside the proto carriers; SWIG `SWIGTYPE_p_*`/`*PINVOKE` types in canonical owners; a solve with no matching native RID payload
