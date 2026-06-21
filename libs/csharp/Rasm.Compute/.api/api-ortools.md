# [RASM_COMPUTE_API_ORTOOLS]

`Google.OrTools` supplies the CP-SAT constraint-programming model and solver, the LinearSolver MIP/LP exact-optimization wrapper across pluggable backends, and the ConstraintSolver routing engine, with per-RID native solver libraries resolved transitively for the Compute solver/optimizer rails behind the `OptimizerKind` rows.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Google.OrTools`
- package: `Google.OrTools` (meta-package, version 9.15.6755)
- assembly: `Google.OrTools`
- namespace: `Google.OrTools.Sat`, `Google.OrTools.LinearSolver`, `Google.OrTools.ConstraintSolver`, `Google.OrTools.Util`
- asset: managed wrapper plus per-RID native solver libraries (`osx-arm64` verified)
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
|  [19]   | `ExportModelAsLpFormat` / `ExportModelAsMpsFormat(bool)`     | model export       | LP/MPS text export                  |
|  [20]   | `VerifySolution(double tolerance, bool log_errors)`          | solve check        | feasibility verification            |

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

## [04]-[IMPLEMENTATION_LAW]

[SOLVER_TOPOLOGY]:
- namespaces: `Google.OrTools.Sat` (CP-SAT), `Google.OrTools.LinearSolver` (MIP/LP), `Google.OrTools.ConstraintSolver` (routing), `Google.OrTools.Util` (`Domain`, `OptionalBoolean`)
- CP-SAT: `CpModel` builds the proto and returns typed constraint families; `CpSolver.Solve` runs the native solver and projects results by `Value`/`BooleanValue`; status is `CpSolverStatus { Unknown, ModelInvalid, Feasible, Infeasible, Optimal }`
- expression algebra: arithmetic operators build `LinearExpr`; relational operators (`==`, `!=`, `>=`, `>`, `<=`, `<`) build `BoundedLinearExpression`; `CpModel.Add(BoundedLinearExpression)` accepts the relational carrier
- variable hierarchy: `BoolVar : IntVar : LinearExpr`; `BoolVar.Not()` returns a `NotBoolVar` literal, so every Boolean variable composes directly into integer expressions and clauses
- LinearSolver: `Solver` selects a backend via `Solver.OptimizationProblemType` (`GLOP`, `CLP`, `PDLP`, `SCIP`, `CBC`, `BOP`, `SAT`, plus optional `GUROBI`, `CPLEX`, `XPRESS`, `GLPK`) or `CreateSolver(solver_id)`; status is `Solver.ResultStatus { OPTIMAL, FEASIBLE, INFEASIBLE, UNBOUNDED, ABNORMAL, MODEL_INVALID, NOT_SOLVED }`
- LinearSolver duals: continuous relaxations expose `ReducedCost`, `DualValue`, `Activity`, and `Solver.BasisStatus { FREE, AT_LOWER_BOUND, AT_UPPER_BOUND, FIXED_VALUE, BASIC }`
- routing: `RoutingIndexManager` maps node ids to solver indices; `RoutingModel` registers transit/arc-cost callbacks, adds dimensions and disjunctions, and solves through `SolveWithParameters(RoutingSearchParameters)`; first-solution and improvement engines are `FirstSolutionStrategy.Types.Value` and `LocalSearchMetaheuristic.Types.Value`; outcome is `RoutingSearchStatus.Types.Value`
- native binding: the managed wrapper is a SWIG/protobuf surface; `IDisposable` roots (`CpSolver`, `Solver`, `RoutingModel`, `RoutingIndexManager`, `MPSolverParameters`, `Objective`) own native handles released by `Dispose`
- proto carriers: `CpModel.Model` (`CpModelProto`), `CpSolver.Response` (`CpSolverResponse`), and `MPModelProto`/`MPModelRequest` in `OperationsResearch` are the wire-level model and response messages

[LOCAL_ADMISSION]:
- The `OptimizerKind` rows select the rail: CP-SAT through `CpModel`/`CpSolver`, MIP/LP through `Solver`, and routing through `RoutingModel`; one canonical solve operation discriminates on optimizer kind rather than parallel solver entrypoints.
- Backend selection is policy data carried by `Solver.OptimizationProblemType` or a `solver_id` string and `SatParameters` text, not hidden inside solve helpers.
- Variables, constraints, objective, and solve each emit typed receipts; status enums classify the outcome and never collapse into a generic success flag.
- Native solver handles enter only through declared `IDisposable` roots and release deterministically; the SWIG `SWIGTYPE_p_*` and `*PINVOKE` types are interop plumbing and stay out of canonical owners.

[RAIL_LAW]:
- Package: `Google.OrTools`
- Owns: CP-SAT constraint programming, MIP/LP exact optimization, and vehicle-routing search
- Accept: declared decision variables, typed constraints, and an objective solved to a classified status
- Reject: hand-rolled branch-and-bound, simplex, or routing search, and float-equality feasibility checks outside the solver
