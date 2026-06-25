# [RASM_COMPUTE_API_GENETICSHARP]

`GeneticSharp` is the evolutionary/metaheuristic engine backing the `Solver/optimizer#OPTIMIZER_LANE` `OptimizerKind` rows the exact `Google.OrTools` CP-SAT/MILP lane does not reach — the population-based `Nsga2`/`CmaEs`/`Pso`/`RobustMinimax` rows. It supplies the full genetic-operator algebra (chromosome encodings, 13 crossovers, mutation/selection/reinsertion/termination catalogs) and the parallel-evaluation executor behind one `GeneticAlgorithm` engine. Every operator is a small interface (`IChromosome`/`ICrossover`/`IMutation`/`ISelection`/`IFitness`/`ITermination`/`IReinsertion`/`ITaskExecutor`), so the `Optimizer.Steps` fold composes the operator set a row declares rather than a per-algorithm engine; `IFitness.Evaluate` is the single coupling point to the `Solver/contract#SOLVE_CONTRACT` `evaluate` oracle (a full `SolveLane.Solve` or a `Surrogate.Predict`), and `ITaskExecutor` parallelizes fitness evaluation onto the bounded compute lanes. The solve fault lifts to `ComputeFault` at the boundary.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `GeneticSharp`
- package: `GeneticSharp` (two assemblies, version 3.1.4, direct pin)
- license: MIT (`giacomelli/GeneticSharp`; nuspec carries a `licenseUrl` to the repository LICENSE rather than an SPDX expression)
- assembly: `GeneticSharp.Domain` (the operator + engine surface) and `GeneticSharp.Infrastructure.Framework` (`ITaskExecutor`, randomization, helpers) → the `net10.0` consumer binds `lib/net6.0/` (the only `lib/` TFM; pure-managed AnyCPU IL, ALC-safe, no native asset)
- namespace: `GeneticSharp` (one flat namespace across both assemblies — chromosomes, operators, engine, executors, services)
- scope: real-coded + binary + integer GA, NSGA-style multi-objective via custom fitness, parallel/TPL fitness evaluation; the proposal kernel for the metaheuristic `OptimizerKind` rows
- rail: optimizer#OPTIMIZER_LANE (population-based rows)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: engine and operator contracts
- rail: optimizer#ENGINE
- note: the GA is assembled from these interfaces; each is a single-method (or near-single-method) contract, so the `OptimizerKind` row selects an operator set, never a subclassed engine.

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY]      | [RAIL]                                                       |
| :-----: | :------------------------------- | :----------------- | :---------------------------------------------------------- |
|  [01]   | `GeneticAlgorithm` / `IGeneticAlgorithm` | engine root | runs the evolve loop over the operator set                  |
|  [02]   | `IChromosome` / `ChromosomeBase` | genome contract    | gene encoding + `Fitness` + `CreateNew`/`Clone`             |
|  [03]   | `IFitness`                       | fitness contract   | `double Evaluate(IChromosome)` — the evaluate oracle seam   |
|  [04]   | `ISelection` / `SelectionBase`   | selection contract | `SelectChromosomes(int, Generation)`                        |
|  [05]   | `ICrossover` / `CrossoverBase`   | crossover contract | `Cross(IList<IChromosome>)` with parents/children arity     |
|  [06]   | `IMutation` / `MutationBase`     | mutation contract  | `Mutate(IChromosome, float probability)`                    |
|  [07]   | `IReinsertion` / `ReinsertionBase` | reinsertion contract | merges offspring + parents into the next generation     |
|  [08]   | `ITermination` / `TerminationBase` | termination contract | `bool HasReached(IGeneticAlgorithm)`                      |
|  [09]   | `IPopulation` / `Population` / `TplPopulation` | population | the generation history and `BestChromosome`        |
|  [10]   | `Generation`                     | generation value   | one evolved generation's chromosome set                     |
|  [11]   | `Gene`                           | gene value         | one chromosome locus value                                  |
|  [12]   | `IGenerationStrategy` / `PerformanceGenerationStrategy` / `TrackingGenerationStrategy` | generation policy | how much generation history is retained |
|  [13]   | `GeneticAlgorithmState`          | state enum         | `NotStarted`/`Started`/`Stopped`/`Resumed`/`TerminationReached` |

[PUBLIC_TYPE_SCOPE]: chromosome encodings
- rail: optimizer#GENOME
- note: the encodings map onto the `optimizer#OPTIMIZER_LANE` `DesignVariable` cases — `Continuous`/`Density` → `FloatingPointChromosome`, `Integer` → `IntegerChromosome`, `Categorical`/combinatorial → a `ChromosomeBase` over discrete genes.

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY]      | [RAIL]                                                     |
| :-----: | :-------------------------- | :----------------- | :-------------------------------------------------------- |
|  [01]   | `FloatingPointChromosome`   | real-coded genome  | bit-encoded reals with `ToFloatingPoints()` decode        |
|  [02]   | `IntegerChromosome`         | integer genome     | a `[min, max]` integer gene                                |
|  [03]   | `BinaryChromosomeBase` / `IBinaryChromosome` | binary genome | bit-string chromosome base + `FlipGene`        |
|  [04]   | `ChromosomeBase`            | genome base        | the base a custom permutation/categorical genome derives   |
|  [05]   | `BinaryStringRepresentation` | bit codec        | real↔bit-string conversion the float genome uses          |
|  [06]   | `ChromosomeExtensions` / `IChromosomeOperator` | genome helper | gene-level operator helpers                       |

[PUBLIC_TYPE_SCOPE]: operator catalog
- rail: optimizer#OPERATORS
- note: the crossover row choice depends on the genome — ordered/position/cycle/PMX for permutation genomes, one/two/uniform/cut-and-splice for binary/real; a row is data on the operator set, never a new engine.

| [INDEX] | [SYMBOL]                                                                 | [TYPE_FAMILY]   | [RAIL]                               |
| :-----: | :---------------------------------------------------------------------- | :-------------- | :----------------------------------- |
|  [01]   | `OnePointCrossover` / `TwoPointCrossover` / `UniformCrossover`          | crossover       | binary/real positional crossover     |
|  [02]   | `CutAndSpliceCrossover` / `ThreeParentCrossover` / `VotingRecombinationCrossover` | crossover | variable-length / multi-parent  |
|  [03]   | `OrderedCrossover` / `OrderBasedCrossover` / `PositionBasedCrossover`   | crossover       | permutation-preserving crossover     |
|  [04]   | `CycleCrossover` / `PartiallyMappedCrossover` / `AlternatingPositionCrossover` | crossover | permutation cycle/PMX/AP          |
|  [05]   | `FlipBitMutation` / `UniformMutation`                                   | mutation        | binary/real gene mutation            |
|  [06]   | `TworsMutation` / `ReverseSequenceMutation` / `InsertionMutation` / `DisplacementMutation` / `PartialShuffleMutation` | mutation | permutation mutation (`SequenceMutationBase`) |
|  [07]   | `EliteSelection` / `RouletteWheelSelection` / `StochasticUniversalSamplingSelection` | selection | fitness-proportionate selection |
|  [08]   | `TournamentSelection` / `RankSelection` / `TruncationSelection`         | selection       | tournament/rank/truncation           |
|  [09]   | `ElitistReinsertion` / `FitnessBasedReinsertion` / `PureReinsertion` / `UniformReinsertion` | reinsertion | next-generation merge policy |
|  [10]   | `GenerationNumberTermination` / `FitnessThresholdTermination` / `FitnessStagnationTermination` / `TimeEvolvingTermination` | termination | convergence policy |
|  [11]   | `AndTermination` / `OrTermination` (`LogicalOperatorTerminationBase`)   | termination     | composite convergence over `params ITermination[]` |
|  [12]   | `FuncFitness`                                                           | fitness adapter | wraps a `Func<IChromosome, double>` oracle |

[PUBLIC_TYPE_SCOPE]: parallel execution and operator strategy
- rail: optimizer#EXECUTOR
- note: `ITaskExecutor` parallelizes fitness evaluation; `IOperatorsStrategy` parallelizes the crossover/mutation operator application — both are the seam to the bounded compute lanes.

| [INDEX] | [SYMBOL]                                                  | [TYPE_FAMILY]      | [RAIL]                                            |
| :-----: | :------------------------------------------------------- | :----------------- | :----------------------------------------------- |
|  [01]   | `ITaskExecutor` / `TaskExecutorBase`                     | executor contract  | `Add(Action)` / `Start` / `Stop` / `Timeout`     |
|  [02]   | `LinearTaskExecutor`                                     | serial executor    | sequential fitness evaluation                    |
|  [03]   | `ParallelTaskExecutor`                                   | thread executor    | thread-bounded parallel fitness evaluation       |
|  [04]   | `TplTaskExecutor`                                        | TPL executor       | `Task`-parallel fitness evaluation               |
|  [05]   | `IOperatorsStrategy` / `OperatorsStrategyBase`           | operator strategy  | how crossover/mutation are applied across parents |
|  [06]   | `DefaultOperatorsStrategy` / `TplOperatorsStrategy` / `TplPopulation` | operator strategy | serial vs TPL-parallel operator application |
|  [07]   | `IRandomization` / `BasicRandomization` / `FastRandomRandomization` / `RandomizationProvider` | RNG | the seedable global randomization source |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: engine assembly and run
- rail: optimizer#ENGINE
- note: the ctor binds the required operator quintet; the settable properties bind the optional operators and the executor; `Start`/`Stop`/`Resume` drive the loop and the events stream progress.

| [INDEX] | [SURFACE]                                                                                        | [ENTRY_FAMILY] | [RAIL]                                          |
| :-----: | :---------------------------------------------------------------------------------------------- | :------------- | :--------------------------------------------- |
|  [01]   | `new GeneticAlgorithm(IPopulation population, IFitness fitness, ISelection selection, ICrossover crossover, IMutation mutation)` | ctor | assembles the engine over its operator set |
|  [02]   | `Reinsertion` / `Termination` / `TaskExecutor` / `OperatorsStrategy` { get; set; }              | operator bind  | optional reinsertion/termination/executor/strategy |
|  [03]   | `CrossoverProbability` / `MutationProbability` { get; set; }                                    | rate           | per-operator probability (defaults `0.75`/`0.1`) |
|  [04]   | `void Start()` / `void Resume()` / `void Stop()`                                                | control        | runs / continues / halts the evolve loop       |
|  [05]   | `IChromosome BestChromosome` / `int GenerationsNumber` / `TimeSpan TimeEvolving`                | result         | best genome, generation count, elapsed time    |
|  [06]   | `event GenerationRan` / `TerminationReached` / `Stopped`                                        | progress       | per-generation, convergence, and stop hooks    |
|  [07]   | `GeneticAlgorithmState State`                                                                   | state          | the engine lifecycle state                     |

[ENTRYPOINT_SCOPE]: population, genome, and fitness construction
- rail: optimizer#GENOME
- note: `Population` seeds from an `adamChromosome` prototype (`CreateNew` clones it); the float genome decodes bit-encoded reals to `double[]` for the objective.

| [INDEX] | [SURFACE]                                                                                          | [ENTRY_FAMILY] | [RAIL]                                       |
| :-----: | :------------------------------------------------------------------------------------------------ | :------------- | :------------------------------------------- |
|  [01]   | `new Population(int minSize, int maxSize, IChromosome adamChromosome)`                             | ctor           | bounded population from a prototype genome   |
|  [02]   | `new FloatingPointChromosome(double[] minValue, double[] maxValue, int[] totalBits, int[] fractionDigits)` | ctor | per-variable real-coded genome (vector)  |
|  [03]   | `new FloatingPointChromosome(double minValue, double maxValue, int fractionDigits)`                | ctor           | scalar real-coded genome                     |
|  [04]   | `FloatingPointChromosome.ToFloatingPoints()` / `ToFloatingPoint()`                                 | decode         | genome → `double[]` / `double` for the objective |
|  [05]   | `new IntegerChromosome(int minValue, int maxValue)`                                                | ctor           | a `[min, max]` integer genome                |
|  [06]   | `new FuncFitness(Func<IChromosome, double> func)`                                                  | ctor           | adapts the `evaluate` oracle to `IFitness`   |
|  [07]   | `IChromosome.GenerateGene(int)` / `ReplaceGene(int, Gene)` / `GetGenes()` / `CreateNew()` / `Clone()` | genome ops   | the gene-level genome contract a custom encoding implements |

[ENTRYPOINT_SCOPE]: operator instances and termination composition
- rail: optimizer#OPERATORS
- note: the selection/crossover/mutation/termination instances are the operator set the engine ctor and properties take; logical terminations compose any set.

| [INDEX] | [SURFACE]                                                                          | [ENTRY_FAMILY] | [RAIL]                                       |
| :-----: | :-------------------------------------------------------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `new TournamentSelection(int size, bool allowWinnerCompeteNextTournament = …)`     | selection      | tournament selection of the given size       |
|  [02]   | `new EliteSelection()` / `new RouletteWheelSelection()` / `new RankSelection()`    | selection      | the parameterless selection operators        |
|  [03]   | `ICrossover.Cross(IList<IChromosome> parents)` / `ParentsNumber` / `ChildrenNumber` | crossover     | applies a crossover row; arity is operator data |
|  [04]   | `new UniformMutation(bool allGenesMutable)` / `new UniformMutation(params int[] mutableGenesIndexes)` | mutation | real-gene uniform mutation               |
|  [05]   | `IMutation.Mutate(IChromosome chromosome, float probability)`                      | mutation       | applies a mutation row at a probability      |
|  [06]   | `new GenerationNumberTermination(int n)` / `new FitnessThresholdTermination(double f)` / `new FitnessStagnationTermination(int g)` / `new TimeEvolvingTermination(TimeSpan t)` | termination | the leaf convergence policies |
|  [07]   | `new AndTermination(params ITermination[] terminations)` / `new OrTermination(params ITermination[])` | termination | composite convergence over a set       |
|  [08]   | `IReinsertion.SelectChromosomes(IPopulation, IList<IChromosome> offspring, IList<IChromosome> parents)` / `CanCollapse` / `CanExpand` | reinsertion | next-generation merge with arity guards |

[ENTRYPOINT_SCOPE]: parallel evaluation
- rail: optimizer#EXECUTOR
- note: the executor batches fitness evaluations; `Timeout` bounds a generation, `IsRunning`/`Start`/`Stop` drive the batch.

| [INDEX] | [SURFACE]                                                          | [ENTRY_FAMILY] | [RAIL]                                       |
| :-----: | :--------------------------------------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `new ParallelTaskExecutor()` / `new TplTaskExecutor()` / `new LinearTaskExecutor()` | ctor   | the parallel/TPL/serial fitness executor     |
|  [02]   | `ITaskExecutor.Add(Action task)` / `Start()` / `Stop()` / `Clear()`               | executor ops | enqueue, run, halt, and reset the batch      |
|  [03]   | `ITaskExecutor.Timeout { get; set; }` / `IsRunning { get; }`                       | bound        | per-generation deadline and running flag     |
|  [04]   | `RandomizationProvider.Current { get; set; }`                                      | RNG seed     | the global randomization source (seed for reproducibility) |

## [04]-[IMPLEMENTATION_LAW]

[ENGINE_TOPOLOGY]:
- `GeneticAlgorithm(population, fitness, selection, crossover, mutation)` is the one engine; `Reinsertion`, `Termination`, `TaskExecutor`, and `OperatorsStrategy` are settable, so the evolve loop is configured from an operator set, never specialized by subclass
- the genome is `IChromosome` (`GenerateGene`/`ReplaceGene`/`GetGenes`/`Resize`/`CreateNew`/`Clone` + a nullable `Fitness`); `FloatingPointChromosome` bit-encodes a real vector (`ToFloatingPoints()` decodes), `IntegerChromosome` a bounded integer, `BinaryChromosomeBase` a bit string, and a custom `ChromosomeBase` a permutation/categorical genome
- the operators are interfaces: `ISelection.SelectChromosomes(int, Generation)`, `ICrossover.Cross(IList<IChromosome>)` (with `ParentsNumber`/`ChildrenNumber`/`MinChromosomeLength`), `IMutation.Mutate(IChromosome, float)`, `IReinsertion.SelectChromosomes(IPopulation, offspring, parents)` (with `CanCollapse`/`CanExpand`), `ITermination.HasReached(IGeneticAlgorithm)`
- termination composes: `AndTermination`/`OrTermination` take `params ITermination[]`, so a generation-count AND fitness-stagnation convergence is one composite, not branching
- `IFitness.Evaluate(IChromosome)` is the only objective coupling; `FuncFitness(Func<IChromosome,double>)` adapts an inline oracle
- parallelism has two seams: `ITaskExecutor` (`LinearTaskExecutor`/`ParallelTaskExecutor`/`TplTaskExecutor`, `Add(Action)`/`Start`/`Stop`/`Timeout`) batches fitness evaluations, and `IOperatorsStrategy` (`DefaultOperatorsStrategy` serial vs `TplOperatorsStrategy`/`TplPopulation` parallel) applies the genetic operators across parents
- `RandomizationProvider.Current` is the global RNG (`BasicRandomization`/`FastRandomRandomization`); seeding it makes a run reproducible

[INTEGRATION_STACK]:
- optimizer rows: `GeneticAlgorithm` is the proposal kernel the `Solver/optimizer#OPTIMIZER_LANE` population-based `OptimizerKind` rows (`Nsga2`, `CmaEs`, `Pso`, `RobustMinimax`) bind on the `Optimizer.Steps` fold — one `Optimize` entry discriminates on the row, so a `GeneticEngine`/`Nsga2Engine` sibling beside the existing fold is the collapsed/deleted form per the optimizer-page Growth law
- genome ↔ design variables: the `optimizer#OPTIMIZER_LANE` `DesignVariable` cases map to the encodings — `Continuous`/`Density` → `FloatingPointChromosome` (vector ctor over per-variable `minValue`/`maxValue`/`totalBits`/`fractionDigits`), `Integer` → `IntegerChromosome`, `Categorical` → a discrete-gene `ChromosomeBase`, `Linked` resolved by `DesignProblem.Resolve` before encoding — never a stringly-typed genome
- fitness ↔ evaluate oracle: `IFitness.Evaluate` (via `FuncFitness`) calls the `Solver/contract#SOLVE_CONTRACT` `evaluate` oracle — a full `SolveLane.Solve` or, where the error bound holds, a `Surrogate.Predict` — so the GA search is contract-uniform with the gradient/Bayesian rows and never knows whether it ran FEA or a surrogate; the per-candidate `ComputeReceipt` and the `ParetoFront` accumulation are the optimizer page's, not re-minted here
- NSGA-II: the `Nsga2` row supplies a custom multi-objective `IFitness` + a `(rank, crowding)` binary `TournamentSelection`, with fast non-dominated sorting and crowding-distance assignment folded onto the `optimizer#OPTIMIZER_LANE` `ParetoFront` (GeneticSharp gives the population/operator machinery; the dominance/crowding comparator and the front are the optimizer page's)
- parallel lanes: `ParallelTaskExecutor`/`TplTaskExecutor` evaluate the population's fitness under the `Runtime/scheduling#WORK_LANES` bounded `WorkLane`/`Substrate`/`AllocationClass` budget; `ITaskExecutor.Timeout` takes the deadline the `Runtime/scheduling` budget folds (NodaTime `Duration` → `TimeSpan`), and `GeneticAlgorithm.Stop()` honors cooperative cancellation from the channel deadline
- progress: the `GenerationRan`/`TerminationReached`/`Stopped` events stream search progress to the `Stats`/`Runtime/progress#MONOTONIC_PROGRESS` sink; `TimeEvolving` and `GenerationsNumber` stamp the optimizer receipt
- reproducibility: `RandomizationProvider.Current` is seeded from the `OptimizerPolicy` so a robust-minimax or NSGA run is deterministic under a fixed seed, the seed riding the receipt

[LOCAL_ADMISSION]:
- the population-based `OptimizerKind` rows select the GA; one `Optimizer` fold drives `GeneticAlgorithm` over the row's operator set, never a per-algorithm engine entrypoint
- the genome encoding is chosen by the `DesignVariable` case (float/integer/binary/permutation), the operator set (selection/crossover/mutation/reinsertion) is row data, and termination is a composite `ITermination` — none branched inside a helper
- fitness is the `evaluate` oracle behind `FuncFitness`; a hand-rolled objective loop beside `IFitness` is the rejected form, and the surrogate/full-solve duality stays the optimizer page's contract
- parallel evaluation enters only through `ITaskExecutor` bound to the bounded compute lanes with a deadline `Timeout`; an unbounded `Parallel.For` over fitness beside the executor is the rejected form
- the GA gives the metaheuristic machinery only — the exact integer/combinatorial solve stays the `api-ortools` CP-SAT/MILP lane, and routing a genuinely exact problem through the GA where OR-Tools solves it exactly is the rejected pick

[RAIL_LAW]:
- Package: `GeneticSharp` (3.1.4, MIT, two pure-managed `lib/net6.0` AnyCPU IL assemblies, no native asset)
- Owns: the population-based GA engine, the chromosome encodings, the full crossover/mutation/selection/reinsertion/termination operator catalog, composite termination, and the parallel/TPL fitness executor — the proposal kernel for the metaheuristic `OptimizerKind` rows
- Accept: a `GeneticAlgorithm` assembled from an operator set a population-based `OptimizerKind` row declares, a genome encoding mapped from the `DesignVariable` cases, an `IFitness` bound to the `Solver/contract` evaluate oracle, parallel evaluation on the bounded compute lanes with a deadline, and the search progress/best-genome stamped onto the optimizer receipt and the `ParetoFront`
- Reject: a per-algorithm engine sibling where one `Optimizer` fold discriminates on `OptimizerKind`; a stringly-typed genome beside the `DesignVariable`-mapped encodings; a hand-rolled objective loop beside `IFitness`; an unbounded parallel fitness loop beside `ITaskExecutor`; routing an exact integer/combinatorial problem through the GA where the `api-ortools` CP-SAT/MILP lane solves it exactly; a re-minted Pareto-front/crowding owner (the optimizer page owns it)
