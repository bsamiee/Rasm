# [RASM_COMPUTE_API_ORTOOLS]

`Google.OrTools` supplies the CP-SAT constraint-programming model and solver, the LinearSolver MIP/LP exact-optimization wrapper across pluggable backends, and the ConstraintSolver routing engine, with per-RID native solver libraries resolved transitively for the Compute solver/optimizer rails behind the `OptimizerKind` rows. The wire-level model/response carriers (`CpModelProto`, `CpSolverResponse`, `MPModelProto`) are `api-protobuf` messages, so the proto vocabulary stacks directly onto the `Runtime/wire` remote lane; the solve fault lifts to `ComputeFault` at the boundary.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Google.OrTools`
- package: `Google.OrTools` (meta-package, version 9.15.6755, direct pin)
- license: Apache-2.0 (`google/or-tools`)
- assembly: `Google.OrTools` → the `net10.0` consumer binds `lib/net8.0/Google.OrTools.dll` (the package also ships `lib/net462`; only `net8.0` is the bound asset)
- namespace: `Google.OrTools.Sat`, `Google.OrTools.LinearSolver`, `Google.OrTools.ConstraintSolver`, `Google.OrTools.Graph`, `Google.OrTools.Util`, `Google.OrTools.OperationsResearch`
- asset: managed SWIG/protobuf wrapper plus per-RID native solver libraries (`Google.OrTools.runtime.{osx-arm64,osx-x64,linux-arm64,linux-x64,win-x64}`; `osx-arm64` verified) — a solve with no matching RID payload faults at native load
- transitive: bundles `Google.Protobuf` 3.33.1 transitively for the proto carriers; the central 3.35.1 pin (`api-protobuf`) wins resolution and is binary-compatible
- rail: solver

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: CP-SAT model and value contracts
- rail: solver#CP_SAT

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]      | [RAIL]                            |
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

[PUBLIC_TYPE_SCOPE]: CP-SAT structural constraint families
- rail: solver#CP_SAT
- note: each is returned by the matching `CpModel.Add*` factory and refines `Constraint` with constraint-specific builders.

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY]    | [RAIL]                          |
| :-----: | :-------------------------- | :--------------- | :------------------------------ |
|  [01]   | `CircuitConstraint`         | graph constraint | single-circuit arc set          |
|  [02]   | `MultipleCircuitConstraint` | graph constraint | multi-circuit arc set           |
|  [03]   | `TableConstraint`           | table constraint | allowed/forbidden tuple set     |
|  [04]   | `AutomatonConstraint`       | automaton        | regular-language transition set |
|  [05]   | `ReservoirConstraint`       | reservoir        | level events over time          |
|  [06]   | `CumulativeConstraint`      | scheduling       | capacity over intervals         |
|  [07]   | `NoOverlap2dConstraint`     | scheduling       | 2D rectangle non-overlap        |

[PUBLIC_TYPE_SCOPE]: LinearSolver MIP/LP contracts
- rail: solver#LINEAR

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY]      | [RAIL]                              |
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

[PUBLIC_TYPE_SCOPE]: Graph network-flow contracts
- rail: solver#GRAPH
- note: `Google.OrTools.Graph` — the specialized min-cut/max-flow/assignment engines (bound-assembly-verified in `lib/net8.0`); each is `IDisposable` over a native graph and takes `int` node/arc indices with `long` capacities/costs. The `Analysis/circulation` exit-capacity solver, outranking a managed Edmonds-Karp minted for the same concern.

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]      | [RAIL]                              |
| :-----: | :--------------------- | :----------------- | :---------------------------------- |
|  [01]   | `MaxFlow`              | max-flow engine    | push-relabel max-flow / min-cut     |
|  [02]   | `MaxFlow.Status`       | status enum        | `OPTIMAL`/`POSSIBLE_OVERFLOW`/`BAD_INPUT`/`BAD_RESULT` |
|  [03]   | `MinCostFlow`          | min-cost-flow engine | `: MinCostFlowBase` — min-cost flow / max-flow-at-min-cost |
|  [04]   | `MinCostFlowBase`      | flow base          | shared node/arc accessors + `Status` enum |
|  [05]   | `LinearSumAssignment`  | assignment engine  | optimal bipartite min-cost assignment |

[PUBLIC_TYPE_SCOPE]: ConstraintSolver routing contracts
- rail: solver#ROUTING

| [INDEX] | [SYMBOL]                               | [TYPE_FAMILY]      | [RAIL]                           |
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

[ENTRYPOINT_SCOPE]: CP-SAT model construction
- rail: solver#CP_SAT
- note: all are instance methods on `CpModel`; `Add*` factories return the constraint family in section 2.

| [INDEX] | [SURFACE]                                                                      | [ENTRY_FAMILY]   | [RAIL]                           |
| :-----: | :----------------------------------------------------------------------------- | :--------------- | :------------------------------- |
|  [01]   | `NewIntVar(long lb, long ub, string name)`                                     | variable factory | integer variable on `[lb, ub]`   |
|  [02]   | `NewIntVarFromDomain(Domain domain, string name)`                              | variable factory | integer variable on a domain     |
|  [03]   | `NewConstant(long value)`                                                      | variable factory | constant integer variable        |
|  [04]   | `NewBoolVar(string name)`                                                      | variable factory | Boolean variable                 |
|  [05]   | `TrueLiteral()` / `FalseLiteral()`                                             | literal factory  | constant literals                |
|  [06]   | `NewIntervalVar<S,D,E>(S start, D size, E end, name)`                          | interval factory | scheduling interval              |
|  [07]   | `NewOptionalIntervalVar<S,D,E>(.., ILiteral is_present, name)`                 | interval factory | presence-gated interval          |
|  [08]   | `AddLinearConstraint(LinearExpr expr, long lb, long ub)`                       | constraint add   | `lb ≤ expr ≤ ub`                 |
|  [09]   | `AddLinearExpressionInDomain(LinearExpr expr, Domain d)`                       | constraint add   | expression in a domain           |
|  [10]   | `Add(BoundedLinearExpression lin)`                                             | constraint add   | relational-operator constraint   |
|  [11]   | `AddAllDifferent(IEnumerable<LinearExpr> exprs)`                               | constraint add   | all-different                    |
|  [12]   | `AddElement(LinearExpr index, exprs, LinearExpr target)`                       | constraint add   | indexed-element equality         |
|  [13]   | `AddCircuit()` / `AddMultipleCircuit()`                                        | constraint add   | circuit constraints              |
|  [14]   | `AddAllowedAssignments` / `AddForbiddenAssignments(exprs)`                     | constraint add   | table constraints                |
|  [15]   | `AddAutomaton(expressions, starting_state, final_states)`                      | constraint add   | automaton constraint             |
|  [16]   | `AddInverse(IEnumerable<IntVar> direct, reverse)`                              | constraint add   | inverse permutation              |
|  [17]   | `AddReservoirConstraint(long minLevel, long maxLevel)`                         | constraint add   | reservoir constraint             |
|  [18]   | `AddImplication(ILiteral a, ILiteral b)`                                       | constraint add   | literal implication              |
|  [19]   | `AddBoolOr` / `AddBoolAnd` / `AddBoolXor(literals)`                            | constraint add   | Boolean clause constraints       |
|  [20]   | `AddAtLeastOne` / `AddAtMostOne` / `AddExactlyOne(literals)`                   | constraint add   | cardinality clause constraints   |
|  [21]   | `AddMinEquality` / `AddMaxEquality(target, exprs)`                             | constraint add   | min/max equality                 |
|  [22]   | `AddAbsEquality(LinearExpr target, LinearExpr expr)`                           | constraint add   | absolute-value equality          |
|  [23]   | `AddDivisionEquality<T,N,D>` / `AddModuloEquality<T,V,M>`                      | constraint add   | integer division/modulo equality |
|  [24]   | `AddMultiplicationEquality(target, left, right)`                               | constraint add   | product equality                 |
|  [25]   | `AddNoOverlap(IEnumerable<IntervalVar> intervals)`                             | constraint add   | 1D interval non-overlap          |
|  [26]   | `AddNoOverlap2D()` / `AddCumulative<C>(C capacity)`                            | constraint add   | 2D non-overlap and cumulative    |
|  [27]   | `Minimize(LinearExpr obj)` / `Maximize(LinearExpr obj)`                        | objective set    | objective direction              |
|  [28]   | `AddDecisionStrategy(vars, var_str, dom_str)`                                  | search hint      | branching strategy               |
|  [29]   | `AddHint(IntVar var, long value)` / `AddHint(ILiteral, bool)` / `ClearHints()` | search hint      | solution hints                   |
|  [30]   | `AddAssumption` / `AddAssumptions` / `ClearAssumptions`                        | assumption set   | infeasibility-core assumptions   |
|  [31]   | `ModelStats()` / `Validate()` / `ExportToFile(string)`                         | model inspection | stats, validation, proto export  |

[ENTRYPOINT_SCOPE]: CP-SAT constraint reification and structural-family builders
- rail: solver#CP_SAT
- note: every `CpModel.Add*` returns either a base `Constraint` (reifiable through `OnlyEnforceIf`) or a refined family that exposes its own fluent builder. These are the methods a discretization/clash design page composes — the table in section 2 names the types, this names the live builders.

| [INDEX] | [SURFACE]                                                                 | [ENTRY_FAMILY]   | [RAIL]                                          |
| :-----: | :------------------------------------------------------------------------ | :--------------- | :---------------------------------------------- |
|  [01]   | `Constraint.OnlyEnforceIf(ILiteral lit)` / `OnlyEnforceIf(ILiteral[] lits)` | reification      | half-reifies any constraint on a literal/and-set |
|  [02]   | `Constraint.Proto` / `Constraint.Index`                                   | constraint value | the underlying `ConstraintProto` and model index |
|  [03]   | `ReservoirConstraint.AddEvent<T,L>(T time, L levelChange)`                 | reservoir build  | adds a fixed level event                        |
|  [04]   | `ReservoirConstraint.AddOptionalEvent<T,L>(T time, L levelChange, ILiteral)` | reservoir build  | adds a presence-gated level event               |
|  [05]   | `CumulativeConstraint.AddDemand<D>(IntervalVar interval, D demand)`        | cumulative build | binds one interval's resource demand            |
|  [06]   | `CumulativeConstraint.AddDemands<D>(IEnumerable<IntervalVar>, IEnumerable<D>)` | cumulative build | binds a batch of interval demands               |
|  [07]   | `NoOverlap2dConstraint.AddRectangle(IntervalVar xInterval, IntervalVar yInterval)` | 2D pack build | adds an x/y interval rectangle to the packing   |
|  [08]   | `AutomatonConstraint.AddTransition(int tail, int head, long label)`       | automaton build  | adds one labeled state transition               |

[ENTRYPOINT_SCOPE]: `Domain` value algebra
- rail: solver#CP_SAT
- note: `Domain` (`Google.OrTools.Util`) is the discrete-domain value type behind `NewIntVarFromDomain`/`AddLinearExpressionInDomain`; it is a full set algebra, not an opaque token, and is `IDisposable`.

| [INDEX] | [SURFACE]                                                                 | [ENTRY_FAMILY]   | [RAIL]                                          |
| :-----: | :------------------------------------------------------------------------ | :--------------- | :---------------------------------------------- |
|  [01]   | `new Domain(long value)` / `new Domain(long left, long right)`            | ctor             | singleton or single-interval domain             |
|  [02]   | `Domain.FromValues(long[])` / `FromIntervals(long[][])` / `FromFlatIntervals(long[])` | static factory | explicit value set or interval set              |
|  [03]   | `Domain.AllValues()` / `LowerOrEqual(long)` / `GreaterOrEqual(long)`      | static factory   | half-open and universal domains                 |
|  [04]   | `Domain.Complement()` / `IntersectionWith` / `UnionWith`                  | set algebra      | domain set operations                           |
|  [05]   | `Domain.Contains(long)` / `IsIncludedIn(Domain)` / `OverlapsWith(Domain)` | set predicate    | membership and containment tests                |
|  [06]   | `Domain.Min()` / `Max()` / `Size()` / `IsEmpty()` / `FlattenedIntervals()` | domain inspect   | bounds, cardinality, and the flat interval form |

[ENTRYPOINT_SCOPE]: CP-SAT solve and result projection
- rail: solver#CP_SAT
- note: all are members of `CpSolver`; `Value`/`BooleanValue` read the best solution after `Solve`.

| [INDEX] | [SURFACE]                                                                 | [ENTRY_FAMILY]  | [RAIL]                                |
| :-----: | :------------------------------------------------------------------------ | :-------------- | :------------------------------------ |
|  [01]   | `Solve(CpModel model, SolutionCallback? cb)`                              | solve call      | runs CP-SAT, returns `CpSolverStatus` |
|  [02]   | `StringParameters { get; set; }`                                          | parameter set   | proto-text `SatParameters` string     |
|  [03]   | `StopSearch()`                                                            | control call    | asynchronous search stop              |
|  [04]   | `Value(IntVar)` / `Value(LinearExpr)`                                     | result read     | integer solution value                |
|  [05]   | `BooleanValue(ILiteral literal)`                                          | result read     | literal solution value                |
|  [06]   | `ObjectiveValue` / `BestObjectiveBound`                                   | result property | objective and bound                   |
|  [07]   | `Response`                                                                | result property | full `CpSolverResponse`               |
|  [08]   | `NumBranches()` / `NumConflicts()` / `WallTime()`                         | solve stats     | search statistics                     |
|  [09]   | `SufficientAssumptionsForInfeasibility()`                                 | result read     | infeasibility-core assumption subset  |
|  [10]   | `ResponseStats()` / `SolveLog()` / `SolutionInfo()`                       | solve report    | response text, log, solution info     |
|  [11]   | `SetLogCallback(StringToVoidDelegate)` / `ClearLogCallback()`             | callback set    | log streaming                         |
|  [12]   | `SetBestBoundCallback(DoubleToVoidDelegate)` / `ClearBestBoundCallback()` | callback set    | bound streaming                       |
|  [13]   | `Dispose()`                                                               | lifetime call   | releases native solver handles        |

[ENTRYPOINT_SCOPE]: LinearExpr term algebra
- rail: solver#CP_SAT
- note: static factories build `LinearExpr`; operators build `LinearExpr` (arithmetic) or `BoundedLinearExpression` (relational) for intake by `CpModel.Add`.

| [INDEX] | [SURFACE]                                           | [ENTRY_FAMILY] | [RAIL]                           |
| :-----: | :-------------------------------------------------- | :------------- | :------------------------------- |
|  [01]   | `Sum(IEnumerable<LinearExpr\|ILiteral\|BoolVar>)`   | static factory | sum of terms                     |
|  [02]   | `WeightedSum(exprs, IEnumerable<int\|long> coeffs)` | static factory | weighted sum                     |
|  [03]   | `Term(LinearExpr\|ILiteral\|BoolVar, long coeff)`   | static factory | scaled single term               |
|  [04]   | `Affine(expr, long coeff, long offset)`             | static factory | `coeff * expr + offset`          |
|  [05]   | `Constant(long value)`                              | static factory | constant expression              |
|  [06]   | `NewBuilder(int sizeHint = 2)`                      | static factory | mutable `LinearExprBuilder`      |
|  [07]   | `+` `-` `*` (unary `-`)                             | operator       | arithmetic over `LinearExpr`     |
|  [08]   | `==` `!=` `>=` `>` `<=` `<`                         | operator       | builds `BoundedLinearExpression` |

[ENTRYPOINT_SCOPE]: LinearSolver model and solve
- rail: solver#LINEAR
- note: all are members of `Solver`; `MakeVar*`/`MakeConstraint` build the variable and constraint handles.

| [INDEX] | [SURFACE]                                                    | [ENTRY_FAMILY]     | [RAIL]                              |
| :-----: | :----------------------------------------------------------- | :----------------- | :---------------------------------- |
|  [01]   | `CreateSolver(string solver_id)`                             | static factory     | backend solver by id string         |
|  [02]   | `Solver(string name, OptimizationProblemType problem_type)`  | ctor               | backend solver by enum              |
|  [03]   | `SupportsProblemType(OptimizationProblemType)`               | static query       | backend availability                |
|  [04]   | `MakeNumVar` / `MakeIntVar` / `MakeBoolVar(.., name)`        | variable factory   | continuous/integer/boolean variable |
|  [05]   | `MakeVar(double lb, double ub, bool integer, string name)`   | variable factory   | typed variable                      |
|  [06]   | `MakeVarArray` / `MakeNumVarMatrix` / `MakeBoolVarMatrix`    | variable factory   | bulk variable arrays and matrices   |
|  [07]   | `MakeConstraint(double lb, double ub, string name)`          | constraint factory | bounded constraint row              |
|  [08]   | `Add(LinearConstraint constraint)`                           | constraint add     | relational-expression constraint    |
|  [09]   | `Objective()`                                                | objective access   | the model objective handle          |
|  [10]   | `Minimize` / `Maximize(LinearExpr\|Variable)`                | objective set      | objective direction                 |
|  [11]   | `Solve()` / `Solve(MPSolverParameters param)`                | solve call         | runs solve, returns `ResultStatus`  |
|  [12]   | `Value(Variable)` / `ReducedCost(Variable)`                  | result read        | primal value and reduced cost       |
|  [13]   | `DualValue(LinearConstraint)` / `Activity(LinearConstraint)` | result read        | dual value and row activity         |
|  [14]   | `ObjectiveValue` / `BestObjectiveBound`                      | result property    | objective and bound                 |
|  [15]   | `SetTimeLimit(long ms)` / `SetNumThreads(int)`               | solve control      | time limit and thread count         |
|  [16]   | `SetHint(MPVariableVector, double[])`                        | solve control      | warm-start hint                     |
|  [17]   | `InterruptSolve()` / `EnableOutput()` / `SuppressOutput()`   | solve control      | interrupt and logging               |
|  [18]   | `Iterations()` / `Nodes()` / `WallTime()`                    | solve stats        | simplex/branch statistics           |
|  [19]   | `ExportModelAsLpFormat(bool obfuscated)` / `ExportModelAsMpsFormat(bool fixed_format, bool obfuscated)` | model export | LP/MPS text export    |
|  [20]   | `VerifySolution(double tolerance, bool log_errors)`          | solve check        | feasibility verification            |
|  [21]   | `SetSolverSpecificParametersAsString(string)` / `SolverVersion()` | solve control  | backend-native parameter passthrough + version |

[ENTRYPOINT_SCOPE]: ConstraintSolver routing model and solve
- rail: solver#ROUTING
- note: `RoutingIndexManager` maps caller nodes to solver indices; callbacks register through index-typed delegates.

| [INDEX] | [SURFACE]                                                                                                     | [ENTRY_FAMILY]   | [RAIL]                         |
| :-----: | :------------------------------------------------------------------------------------------------------------ | :--------------- | :----------------------------- |
|  [01]   | `RoutingIndexManager(int num_nodes, int num_vehicles, int depot)`                                             | ctor             | single-depot index manager     |
|  [02]   | `RoutingIndexManager(num_nodes, num_vehicles, int[] starts, int[] ends)`                                      | ctor             | multi-depot index manager      |
|  [03]   | `RoutingModel(RoutingIndexManager index_manager)`                                                             | ctor             | routing model                  |
|  [04]   | `RegisterTransitCallback(LongLongToLong callback)`                                                            | callback set     | arc-cost/transit evaluator     |
|  [05]   | `RegisterUnaryTransitCallback(LongToLong callback)`                                                           | callback set     | unary transit evaluator        |
|  [06]   | `RegisterTransitMatrix(long[][] values)`                                                                      | callback set     | matrix transit evaluator       |
|  [07]   | `SetArcCostEvaluatorOfAllVehicles(int evaluator_index)`                                                       | cost set         | global arc cost                |
|  [08]   | `SetArcCostEvaluatorOfVehicle(int evaluator_index, int vehicle)`                                              | cost set         | per-vehicle arc cost           |
|  [09]   | `AddDimension(int evaluator_index, long slack_max, long capacity, bool fix_start_cumul_to_zero, string name)` | dimension add    | capacity dimension             |
|  [10]   | `AddDimensionWithVehicleCapacity(.., long[] vehicle_capacities, ..)`                                          | dimension add    | per-vehicle capacity dimension |
|  [11]   | `GetDimensionOrDie(string dimension_name)`                                                                    | dimension access | dimension lookup               |
|  [12]   | `AddDisjunction(long[] indices, long penalty)`                                                                | constraint add   | optional-visit disjunction     |
|  [13]   | `AddPickupAndDelivery(long pickup, long delivery)`                                                            | constraint add   | pickup-delivery pairing        |
|  [14]   | `SetFixedCostOfVehicle(long cost, int vehicle)`                                                               | cost set         | per-vehicle fixed cost         |
|  [15]   | `SolveWithParameters(RoutingSearchParameters search_parameters)`                                              | solve call       | runs routing search            |
|  [16]   | `SetVisitType` / `AddHardTypeIncompatibility(int type1, int type2)`                                           | constraint add   | visit-type regulations         |
|  [17]   | `AddResourceGroup()`                                                                                          | constraint add   | shared-resource group          |

[ENTRYPOINT_SCOPE]: Graph max-flow / min-cost-flow / assignment
- rail: solver#GRAPH
- note: build the arc set imperatively (each `Add*` returns the arc index), `Solve` returns the `Status`, then read the optimal value and per-arc flow. The `Analysis/circulation` exit-capacity solve maps occupant-load supplies onto space nodes and door/corridor capacities onto arcs of the concrete `ElementGraph` space-adjacency subgraph.

| [INDEX] | [SURFACE]                                                        | [ENTRY_FAMILY]   | [RAIL]                                          |
| :-----: | :--------------------------------------------------------------- | :--------------- | :---------------------------------------------- |
|  [01]   | `MaxFlow.AddArcWithCapacity(int tail, int head, long capacity)`  | arc build        | adds one capacitated arc, returns its index     |
|  [02]   | `MaxFlow.SetArcCapacity(int arc, long capacity)`                 | arc mutate       | reset an arc capacity for an incremental re-solve |
|  [03]   | `MaxFlow.Solve(int source, int sink)`                           | solve call       | runs push-relabel, returns `MaxFlow.Status`     |
|  [04]   | `MaxFlow.OptimalFlow()` / `Flow(int arc)`                       | result read      | the max-flow value / per-arc flow (the min-cut is read via `Flow == Capacity` saturated arcs) |
|  [05]   | `MinCostFlow(int reserveNodes[, int reserveArcs])`             | ctor             | pre-sized min-cost-flow engine                  |
|  [06]   | `MinCostFlow.AddArcWithCapacityAndUnitCost(tail, head, cap, unitCost)` | arc build  | adds one capacitated, priced arc                |
|  [07]   | `MinCostFlow.SetNodeSupply(int node, long supply)`             | node build       | sets a node's supply (+) / demand (−)           |
|  [08]   | `MinCostFlow.Solve()` / `SolveMaxFlowWithMinCost()`           | solve call       | min-cost flow / max-flow-at-min-cost, returns `Status` |
|  [09]   | `MinCostFlow.OptimalCost()` / `MaximumFlow()` / `Flow(int arc)` | result read     | optimal cost / total flow / per-arc flow        |
|  [10]   | `LinearSumAssignment.AddArcWithCost(left, right, long cost)`   | arc build        | adds one left→right assignment arc              |
|  [11]   | `LinearSumAssignment.Solve()` / `OptimalCost()` / `RightMate(int left)` / `AssignmentCost(int left)` | solve + read | optimal assignment, total cost, per-left-node mate + cost |

## [04]-[IMPLEMENTATION_LAW]

[SOLVER_TOPOLOGY]:
- namespaces: `Google.OrTools.Sat` (CP-SAT), `Google.OrTools.LinearSolver` (MIP/LP), `Google.OrTools.ConstraintSolver` (routing), `Google.OrTools.Util` (`Domain`, `OptionalBoolean`)
- CP-SAT: `CpModel` builds the proto and returns typed constraint families; `CpSolver.Solve` runs the native solver and projects results by `Value`/`BooleanValue`; status is `CpSolverStatus { Unknown, ModelInvalid, Feasible, Infeasible, Optimal }`
- expression algebra: arithmetic operators build `LinearExpr`; relational operators (`==`, `!=`, `>=`, `>`, `<=`, `<`) build `BoundedLinearExpression`; `CpModel.Add(BoundedLinearExpression)` accepts the relational carrier
- variable hierarchy: `BoolVar : IntVar : LinearExpr`; `BoolVar.Not()` returns a `NotBoolVar` literal, so every Boolean variable composes directly into integer expressions and clauses
- LinearSolver: `Solver` selects a backend via `Solver.OptimizationProblemType` (`GLOP`, `CLP`, `PDLP`, `SCIP`, `CBC`, `BOP`, `SAT`, plus optional `GUROBI`, `CPLEX`, `XPRESS`, `GLPK`) or `CreateSolver(solver_id)`; status is `Solver.ResultStatus { OPTIMAL, FEASIBLE, INFEASIBLE, UNBOUNDED, ABNORMAL, MODEL_INVALID, NOT_SOLVED }`
- LinearSolver duals: continuous relaxations expose `ReducedCost`, `DualValue`, `Activity`, and `Solver.BasisStatus { FREE, AT_LOWER_BOUND, AT_UPPER_BOUND, FIXED_VALUE, BASIC }`
- routing: `RoutingIndexManager` maps node ids to solver indices; `RoutingModel` registers transit/arc-cost callbacks, adds dimensions and disjunctions, and solves through `SolveWithParameters(RoutingSearchParameters)`; first-solution and improvement engines are `FirstSolutionStrategy.Types.Value` and `LocalSearchMetaheuristic.Types.Value`; outcome is `RoutingSearchStatus.Types.Value`
- reification: `Constraint.OnlyEnforceIf(ILiteral)` half-reifies any constraint; the structural families refine `Constraint` with fluent builders (`ReservoirConstraint.AddEvent`, `CumulativeConstraint.AddDemand`/`AddDemands`, `NoOverlap2dConstraint.AddRectangle`, `AutomatonConstraint.AddTransition`), so a scheduling/packing model composes the builder rather than a parallel constraint type
- domain algebra: `Domain` is a closed set value (`FromValues`/`FromIntervals`/`Complement`/`IntersectionWith`/`UnionWith`/`Contains`/`IsIncludedIn`); a discretization page expresses a variable's admissible set through the algebra, never an external bit-set beside it
- native binding: the managed wrapper is a SWIG/protobuf surface; `IDisposable` roots (`CpSolver`, `Solver`, `RoutingModel`, `RoutingIndexManager`, `MPSolverParameters`, `Objective`, `Domain`) own native handles released by `Dispose`
- proto carriers: `CpModel.Model` (`CpModelProto`), `CpSolver.Response` (`CpSolverResponse`), `Constraint.Proto` (`ConstraintProto`), and `MPModelProto`/`MPModelRequest` in `Google.OrTools.OperationsResearch` are `api-protobuf` messages — the wire-level model and response carriers

[INTEGRATION_STACK]:
- proto wire: the `CpModelProto`/`CpSolverResponse`/`MPModelProto` carriers are `Google.Protobuf` messages, so a solve request/response crosses the `Runtime/wire#PROTO_VOCABULARY` lane on the `api-protobuf` codec and stages its serialized bytes through the `api-recyclable-stream` pool — no managed solve DTO beside the proto.
- optimizer rows: the `Solver/optimizer#OPTIMIZER_LANE` `OptimizerKind` axis carries `cp-sat`/`milp` rows that lower the typed `DesignProblem` to a `CpModel`/`Solver` through the typed model-builder API (`NewIntVar`/`MakeIntVar`/`MakeNumVar` over the `DesignVariable` cases, never a string-parsed model); one `Optimize` fold discriminates on the row, so a per-backend solver owner is the collapsed form.
- backend policy: `Solver.OptimizationProblemType`, a `solver_id` string, the `SatParameters` proto-text (`CpSolver.StringParameters`), and `SetSolverSpecificParametersAsString` are policy DATA carried on the row, never branched inside a solve helper.
- time budget: `CpModel`/`Solver` time limits accept the deadline the `Runtime/scheduling` budget folds (NodaTime `Duration` → ms via `Solver.SetTimeLimit(long)` / the `max_time_in_seconds` `SatParameters` key); the solve elapsed (`WallTime()`) stamps the typed receipt.
- streaming callbacks: `CpSolver.SetLogCallback`/`SetBestBoundCallback` and a `SolutionCallback` subclass stream search progress to the `Stats`/`Runtime/progress` sink; `StopSearch()`/`InterruptSolve()` honor cooperative cancellation from the channel deadline.
- graph-flow circulation: the `Analysis/circulation` egress runner composes `Google.OrTools.Graph.MaxFlow` for exit-capacity — occupant-load supplies map onto space nodes, door/corridor widths onto arc capacities of the concrete `ElementGraph` space-adjacency subgraph, and `Solve(source, sink)` returns the evacuation throughput while saturated arcs (`Flow == Capacity`) name the min-cut bottleneck; `MinCostFlow` distributes occupant load at least travel cost. The path/topology algebra (Dijkstra/A*/betweenness) is `QuikGraph`'s, the planar side (isovist, medial-axis) is `NetTopologySuite`/`Clipper2`'s — the flow concern alone is this Graph module's, zero new central pins.

[LOCAL_ADMISSION]:
- The `OptimizerKind` rows select the rail: CP-SAT through `CpModel`/`CpSolver`, MIP/LP through `Solver`, and routing through `RoutingModel`; one canonical solve operation discriminates on optimizer kind rather than parallel solver entrypoints.
- Backend selection is policy data carried by `Solver.OptimizationProblemType` or a `solver_id` string and `SatParameters` text, not hidden inside solve helpers.
- Variables, constraints, objective, and solve each emit typed receipts; status enums classify the outcome and never collapse into a generic success flag.
- Native solver handles enter only through declared `IDisposable` roots and release deterministically; the SWIG `SWIGTYPE_p_*` and `*PINVOKE` types are interop plumbing and stay out of canonical owners.

[RAIL_LAW]:
- Package: `Google.OrTools` (9.15.6755, Apache-2.0, managed net8.0 + per-RID native)
- Owns: CP-SAT constraint programming (with reification + structural-family builders + `Domain` algebra), MIP/LP exact optimization across pluggable backends, vehicle-routing search, and the `Google.OrTools.Graph` specialized network-flow engines (max-flow/min-cut, min-cost-flow, linear-sum-assignment); the proto carriers are `api-protobuf` messages
- Accept: declared decision variables, typed constraints reified through `OnlyEnforceIf`, admissible sets expressed as `Domain` algebra, and an objective solved to a classified status — stacked onto the `OptimizerKind` row, the proto wire, and the NodaTime deadline budget
- Reject: hand-rolled branch-and-bound, simplex, or routing search; float-equality feasibility checks outside the solver; per-backend solve entrypoints where one `Solve` discriminates on `OptimizerKind`; a managed solve DTO beside the proto carriers; the SWIG `SWIGTYPE_p_*`/`*PINVOKE` interop types leaking into canonical owners; a solve with no matching native RID payload
