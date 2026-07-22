# [RASM_COMPUTE_API_GENETICSHARP]

`GeneticSharp` owns the genetic-algorithm engine behind the `optimizer#OPTIMIZER_LANE` `Nsga2` and `RobustMinimax` rows the exact CP-SAT/MILP lane cannot reach: the full genetic-operator algebra and the parallel-evaluation executor behind one `GeneticAlgorithm`. Every operator is a single-method interface, so one `Optimizer.Steps` fold composes the operator set a row declares; `IFitness.Evaluate` is the sole coupling to the solve `evaluate` oracle, `ITaskExecutor` parallelizes fitness onto the bounded compute lanes, and the solve fault lifts to `ComputeFault`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `GeneticSharp`
- package: `GeneticSharp` (MIT, `giacomelli`)
- assembly: `GeneticSharp.Domain` (operator + engine surface) + `GeneticSharp.Infrastructure.Framework` (executor, randomization) — pure-managed AnyCPU IL on `lib/net6.0`, ALC-safe, no native asset
- namespace: `GeneticSharp`
- rail: optimizer#OPTIMIZER_LANE (population-based rows)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: engine and operator contracts — a row selects an operator set, never a subclassed engine

| [INDEX] | [SYMBOL]                                                | [TYPE_FAMILY]     | [CAPABILITY]                             |
| :-----: | :------------------------------------------------------ | :---------------- | :--------------------------------------- |
|  [01]   | `GeneticAlgorithm` / `IGeneticAlgorithm`                | class / interface | engine root — runs the evolve loop       |
|  [02]   | `IChromosome` / `ChromosomeBase`                        | interface / class | genome — encoding + `CreateNew`/`Clone`  |
|  [03]   | `IFitness`                                              | interface         | fitness — the objective oracle seam      |
|  [04]   | `ISelection` / `SelectionBase`                          | interface / class | selection — chooses parents by fitness   |
|  [05]   | `ICrossover` / `CrossoverBase`                          | interface / class | crossover — recombines parents, w/ arity |
|  [06]   | `IMutation` / `MutationBase`                            | interface / class | mutation — perturbs a gene by rate       |
|  [07]   | `IReinsertion` / `ReinsertionBase`                      | interface / class | reinsertion — merges offspring + parents |
|  [08]   | `ITermination` / `TerminationBase`                      | interface / class | termination — convergence test           |
|  [09]   | `IPopulation` / `Population` / `TplPopulation`          | interface / class | population — history + `BestChromosome`  |
|  [10]   | `Generation`                                            | class             | one evolved generation's chromosome set  |
|  [11]   | `Gene`                                                  | struct            | one chromosome locus value               |
|  [12]   | `IGenerationStrategy` / `PerformanceGenerationStrategy` | interface / class | history policy — bounded retention       |
|  [13]   | `TrackingGenerationStrategy`                            | class             | history policy — full retention          |
|  [14]   | `GeneticAlgorithmState`                                 | enum              | engine lifecycle state                   |

- `GeneticAlgorithmState`: `NotStarted` `Started` `Stopped` `Resumed` `TerminationReached`.

[PUBLIC_TYPE_SCOPE]: chromosome encodings — the genome the `DesignVariable` case selects

| [INDEX] | [SYMBOL]                                       | [TYPE_FAMILY]     | [CAPABILITY]                                         |
| :-----: | :--------------------------------------------- | :---------------- | :--------------------------------------------------- |
|  [01]   | `FloatingPointChromosome`                      | real-coded genome | bit-encoded reals with `ToFloatingPoints()` decode   |
|  [02]   | `IntegerChromosome`                            | integer genome    | a `[min, max]` integer gene                          |
|  [03]   | `BinaryChromosomeBase` / `IBinaryChromosome`   | binary genome     | bit-string chromosome base + `FlipGene`              |
|  [04]   | `ChromosomeBase`                               | genome base       | base a custom permutation/categorical genome derives |
|  [05]   | `BinaryStringRepresentation`                   | bit codec         | real↔bit-string conversion the float genome uses     |
|  [06]   | `ChromosomeExtensions` / `IChromosomeOperator` | genome helper     | gene-level operator helpers                          |

[PUBLIC_TYPE_SCOPE]: operator catalog — a row is data on the operator set, keyed by genome family

| [INDEX] | [SYMBOL]                                                              | [TYPE_FAMILY] | [CAPABILITY]                         |
| :-----: | :-------------------------------------------------------------------- | :------------ | :----------------------------------- |
|  [01]   | `OnePointCrossover` / `TwoPointCrossover` / `UniformCrossover`        | crossover     | binary/real positional               |
|  [02]   | `CutAndSpliceCrossover` / `ThreeParentCrossover`                      | crossover     | variable-length / multi-parent       |
|  [03]   | `VotingRecombinationCrossover`                                        | crossover     | multi-parent voting recombination    |
|  [04]   | `OrderedCrossover` / `OrderBasedCrossover` / `PositionBasedCrossover` | crossover     | permutation-preserving               |
|  [05]   | `CycleCrossover` / `PartiallyMappedCrossover`                         | crossover     | permutation cycle / PMX              |
|  [06]   | `AlternatingPositionCrossover`                                        | crossover     | permutation alternating-position     |
|  [07]   | `FlipBitMutation` / `UniformMutation`                                 | mutation      | binary / real gene mutation          |
|  [08]   | `TworsMutation` / `ReverseSequenceMutation` / `InsertionMutation`     | mutation      | permutation (`SequenceMutationBase`) |
|  [09]   | `DisplacementMutation` / `PartialShuffleMutation`                     | mutation      | permutation (`SequenceMutationBase`) |
|  [10]   | `EliteSelection` / `RouletteWheelSelection`                           | selection     | fitness-proportionate                |
|  [11]   | `StochasticUniversalSamplingSelection`                                | selection     | stochastic universal sampling        |
|  [12]   | `TournamentSelection` / `RankSelection` / `TruncationSelection`       | selection     | tournament / rank / truncation       |
|  [13]   | `ElitistReinsertion` / `FitnessBasedReinsertion`                      | reinsertion   | next-generation merge policy         |
|  [14]   | `PureReinsertion` / `UniformReinsertion`                              | reinsertion   | next-generation merge policy         |
|  [15]   | `GenerationNumberTermination` / `FitnessThresholdTermination`         | termination   | count / fitness convergence          |
|  [16]   | `FitnessStagnationTermination` / `TimeEvolvingTermination`            | termination   | stall / time convergence             |
|  [17]   | `AndTermination` / `OrTermination`                                    | termination   | composite (`params ITermination[]`)  |
|  [18]   | `FuncFitness`                                                         | fitness       | wraps a `Func<IChromosome, double>`  |

[PUBLIC_TYPE_SCOPE]: parallel execution and operator strategy — the seam to the bounded compute lanes

| [INDEX] | [SYMBOL]                                                              | [TYPE_FAMILY]     | [CAPABILITY]                                 |
| :-----: | :-------------------------------------------------------------------- | :---------------- | :------------------------------------------- |
|  [01]   | `ITaskExecutor` / `TaskExecutorBase`                                  | executor contract | `Add(Action)` / `Start` / `Stop` / `Timeout` |
|  [02]   | `LinearTaskExecutor`                                                  | serial executor   | sequential fitness evaluation                |
|  [03]   | `ParallelTaskExecutor`                                                | thread executor   | thread-bounded parallel fitness evaluation   |
|  [04]   | `TplTaskExecutor`                                                     | TPL executor      | `Task`-parallel fitness evaluation           |
|  [05]   | `IOperatorsStrategy` / `OperatorsStrategyBase`                        | operator strategy | how crossover/mutation apply across parents  |
|  [06]   | `DefaultOperatorsStrategy` / `TplOperatorsStrategy` / `TplPopulation` | operator strategy | serial vs TPL-parallel operator application  |
|  [07]   | `IRandomization` / `BasicRandomization`                               | RNG               | seedable randomization base                  |
|  [08]   | `FastRandomRandomization` / `RandomizationProvider`                   | RNG               | fast RNG + global provider                   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: engine assembly and run — the ctor binds the required operator quintet, the rest settable

| [INDEX] | [SURFACE]                                                                        | [SHAPE]  | [CAPABILITY]                          |
| :-----: | :------------------------------------------------------------------------------- | :------- | :------------------------------------ |
|  [01]   | `new GeneticAlgorithm(IPopulation, IFitness, ISelection, ICrossover, IMutation)` | ctor     | assembles the engine, operator set    |
|  [02]   | `Reinsertion` / `Termination` / `TaskExecutor` / `OperatorsStrategy { set }`     | property | optional operators + executor         |
|  [03]   | `CrossoverProbability` / `MutationProbability { get; set }`                      | property | probability, default `0.75`/`0.1`     |
|  [04]   | `Start()` / `Resume()` / `Stop()`                                                | instance | runs/continues/halts the loop         |
|  [05]   | `BestChromosome` / `GenerationsNumber` / `TimeEvolving`                          | property | best genome, gen count, elapsed       |
|  [06]   | `GenerationRan` / `TerminationReached` / `Stopped`                               | event    | per-generation/convergence/stop hooks |
|  [07]   | `State { get }`                                                                  | property | engine lifecycle state                |

[ENTRYPOINT_SCOPE]: population, genome, and fitness construction

| [INDEX] | [SURFACE]                                                          | [SHAPE]  | [CAPABILITY]                                            |
| :-----: | :----------------------------------------------------------------- | :------- | :------------------------------------------------------ |
|  [01]   | `new Population(int, int, IChromosome)`                            | ctor     | bounded population from a prototype genome              |
|  [02]   | `new FloatingPointChromosome(double[], double[], int[], int[])`    | ctor     | per-variable real-coded genome (vector)                 |
|  [03]   | `new FloatingPointChromosome(double, double, int)`                 | ctor     | scalar real-coded genome                                |
|  [04]   | `FloatingPointChromosome.ToFloatingPoints()` / `ToFloatingPoint()` | instance | genome → `double[]` / `double`                          |
|  [05]   | `new IntegerChromosome(int, int)`                                  | ctor     | a `[min, max]` integer genome                           |
|  [06]   | `new FuncFitness(Func<IChromosome, double>)`                       | ctor     | adapts the `evaluate` oracle to `IFitness`              |
|  [07]   | `IChromosome.GenerateGene(int)` / `ReplaceGene(int, Gene)`         | instance | gene-level mutation/replacement contract                |
|  [08]   | `IChromosome.GetGenes()` / `CreateNew()` / `Clone()`               | instance | genome enumerate/copy contract                          |
|  [09]   | `Population.GenerationStrategy { get; set }`                       | property | retention (default `PerformanceGenerationStrategy(10)`) |

[ENTRYPOINT_SCOPE]: operator instances and termination composition

| [INDEX] | [SURFACE]                                                                            | [SHAPE]  | [CAPABILITY]                           |
| :-----: | :----------------------------------------------------------------------------------- | :------- | :------------------------------------- |
|  [01]   | `new TournamentSelection(int, bool)`                                                 | ctor     | tournament selection of the given size |
|  [02]   | `new EliteSelection()` / `RouletteWheelSelection()` / `RankSelection()`              | ctor     | parameterless selection operators      |
|  [03]   | `ICrossover.Cross(IList<IChromosome>)`                                               | instance | applies a crossover row                |
|  [04]   | `new UniformMutation(bool)` / `new UniformMutation(params int[])`                    | ctor     | real-gene uniform mutation             |
|  [05]   | `IMutation.Mutate(IChromosome, float)`                                               | instance | applies a mutation at a probability    |
|  [06]   | `new GenerationNumberTermination(int)` / `FitnessThresholdTermination(double)`       | ctor     | leaf convergence (count/fitness)       |
|  [07]   | `new FitnessStagnationTermination(int)` / `TimeEvolvingTermination(TimeSpan)`        | ctor     | leaf convergence (stall/time)          |
|  [08]   | `new AndTermination(params ITermination[])` / `OrTermination(params ITermination[])` | ctor     | composite convergence over a set       |
|  [09]   | `IReinsertion.SelectChromosomes(IPopulation, IList, IList)`                          | instance | next-generation merge                  |
|  [10]   | `new UniformCrossover(float)` / `new TwoPointCrossover()`                            | ctor     | crossover ctors a row binds            |

- `UniformCrossover.MixProbability`: defaults `0.5`; the parameterless crossover siblings construct from the section-2 rows.

[ENTRYPOINT_SCOPE]: parallel evaluation

| [INDEX] | [SURFACE]                                                                   | [SHAPE]  | [CAPABILITY]                                 |
| :-----: | :-------------------------------------------------------------------------- | :------- | :------------------------------------------- |
|  [01]   | `new ParallelTaskExecutor()` / `TplTaskExecutor()` / `LinearTaskExecutor()` | ctor     | the parallel/TPL/serial fitness executor     |
|  [02]   | `ITaskExecutor.Add(Action)` / `Start()` / `Stop()` / `Clear()`              | instance | enqueue, run, halt, reset the batch          |
|  [03]   | `ITaskExecutor.Timeout { get; set }` / `IsRunning { get }`                  | property | per-generation deadline + running flag       |
|  [04]   | `RandomizationProvider.Current { get; set }`                                | static   | global RNG source (seed for reproducibility) |
|  [05]   | `ParallelTaskExecutor.MinThreads` / `MaxThreads { get; set }`               | property | min/max worker threads (default `200`)       |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `GeneticAlgorithm` is the one engine, assembled from an operator set; `Reinsertion`/`Termination`/`TaskExecutor`/`OperatorsStrategy` are settable, so the evolve loop configures from an operator set, never a subclass.
- every operator is a single-method interface, so a row selects an operator set; `AndTermination`/`OrTermination` take `params ITermination[]`, composing convergence rather than branching it.
- `IFitness.Evaluate` is the sole objective coupling; `FuncFitness` adapts an inline `Func<IChromosome, double>` oracle.
- parallelism has two seams: `ITaskExecutor` batches fitness evaluations, `IOperatorsStrategy` applies genetic operators across parents.
- `RandomizationProvider.Current` is the global RNG (`BasicRandomization`/`FastRandomRandomization`); seeding it makes a run reproducible.

[STACKING]:
- optimizer rows: `GeneticAlgorithm` is the proposal kernel the `optimizer#OPTIMIZER_LANE` `Nsga2` and `RobustMinimax` rows bind on the `Optimizer.Steps` table; one `Optimize` entry discriminates on the row, so an `Nsga2Engine` sibling beside the table is the collapsed form.
- genome ↔ design variables: the `DesignVariable` cases map to encodings — `Continuous`/`Density` → `FloatingPointChromosome` (per-variable vector ctor over `minValue`/`maxValue`/`totalBits`/`fractionDigits`), `Integer` → `IntegerChromosome`, `Categorical` → a discrete-gene `ChromosomeBase`, `Linked` resolved by `DesignProblem.Resolve` before encoding.
- fitness ↔ evaluate oracle: `IFitness.Evaluate` via `FuncFitness` calls the `Solver/contract#SOLVE_CONTRACT` `evaluate` oracle — a full `SolveLane.Solve` or a `Surrogate.Predict` — so the search is contract-uniform with the gradient/Bayesian rows; the `ComputeReceipt` and `ParetoFront` accumulation stay the optimizer page's.
- NSGA-II: the `Nsga2` row supplies a custom multi-objective `IFitness` and a `(rank, crowding)` `TournamentSelection`, folding non-dominated sorting + crowding onto the optimizer `ParetoFront`; GeneticSharp gives the population/operator machinery, the dominance comparator is the optimizer page's.
- parallel lanes: `ParallelTaskExecutor`/`TplTaskExecutor` evaluate the population under the `Runtime/scheduling#WORK_LANES` bounded budget with `MaxThreads` capped to the CPU lane; `ITaskExecutor.Timeout` takes the scheduling deadline (`Duration` → `TimeSpan`) and `GeneticAlgorithm.Stop()` honors cooperative cancellation.
- progress + reproducibility: the `GenerationRan`/`TerminationReached`/`Stopped` events stream to the `Runtime/progress#MONOTONIC_PROGRESS` sink and stamp `TimeEvolving`/`GenerationsNumber` on the receipt; `RandomizationProvider.Current` seeded from the `OptimizerPolicy` makes a run deterministic, the seed riding the receipt.

[LOCAL_ADMISSION]:
- `Nsga2` and the `RobustMinimax` outer search select the GA on the one `Optimizer.Steps` table; `CmaEs`/`Pso`/`SimulatedAnnealing` are genuine in-package covariance/swarm/annealing kernels no package owns, never this engine with swapped operators.
- genome encoding follows the `DesignVariable` case, the operator set is row data, and termination is a composite `ITermination` — none branched inside a helper.
- exact integer/combinatorial solve stays the `api-ortools` CP-SAT/MILP lane; routing a genuinely exact problem through the GA is the rejected pick.

[RAIL_LAW]:
- Package: `GeneticSharp`
- Owns: the GA engine, chromosome encodings, the full crossover/mutation/selection/reinsertion/termination operator catalog, composite termination, and the parallel/TPL fitness executor — the proposal kernel for the `Nsga2` row and the `RobustMinimax` outer GA.
- Accept: a `GeneticAlgorithm` assembled from an operator set a population-based `OptimizerKind` row declares, a genome mapped from the `DesignVariable` cases, an `IFitness` bound to the `Solver/contract` evaluate oracle, parallel evaluation on the bounded compute lanes with a deadline, and the best-genome/progress stamped onto the optimizer receipt and `ParetoFront`.
- Reject: a per-algorithm engine sibling where one `Optimizer` fold discriminates on `OptimizerKind`; a stringly-typed genome beside the `DesignVariable`-mapped encodings; a hand-rolled objective loop beside `IFitness`; an unbounded parallel fitness loop beside `ITaskExecutor`; an exact integer/combinatorial problem routed through the GA where `api-ortools` solves it; a re-minted Pareto-front/crowding owner.
