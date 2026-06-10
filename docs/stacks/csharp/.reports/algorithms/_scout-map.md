# Universal Concern Map — Numerical Geometry / Linear Algebra Surface

## [DENSE_DECOMPOSITION]

- Dense factorization suite (lower-upper, orthogonal, singular-value, symmetric-definite, symmetric and general eigendecomposition) delegated to an external numerics library, with live factor handles retained inside result carriers so factors can be reused for repeated right-hand sides.
- Rank, conditioning, determinant, pseudo-inverse, and spectral norm derived from singular values; squareness, dimension-agreement, and all-entries-finite admission gates precede every factorization.
- Residual-capped direct solves: success is demoted to failure when an independently recomputed relative residual exceeds a square-root-of-machine-epsilon-scale cap; least-squares routes record full-rank evidence instead of capping.
- Packed symmetric storage with triangular index arithmetic; immutable persistent-array carriers whose per-entry mutation is copy-on-write — safe for evidence records, pathological if ever used inside assembly loops.
- Small dense probes as validity instruments: positive-definiteness checked by attempting a factorization; per-pair precision matrices via small factor-and-solve-identity; moment systems solved through small normal-equation factorizations.

## [SPARSE_STORAGE_AND_ASSEMBLY]

- One canonical compressed-row sparse format constructed exclusively from triplet lists with duplicate summation, exact-zero dropping, row-major sorting, and shared generic compression machinery.
- Structural invariants (monotone row pointers, strictly increasing in-row columns, all-finite values) expressed as executable properties and re-validated on every hot-path access — repeated linear-in-nonzeros validation cost per operation.
- Upper-triangular Hermitian sibling format with a real-diagonal invariant enforced at assembly; the lower triangle reconstructed by conjugate transpose at multiply time; shared structural validators parameterized by a minimum-column rule.
- Symmetry carried as per-call revalidation (fold-to-upper-triangle with mirrored-entry consistency tolerance) rather than as a typed witness on the matrix — repeated superlinear work across every symmetric solve, factorization, eigen, and residual path.
- Three transient representations of one sparse operator coexist (canonical compressed-row, the iterative-solver library's storage, the direct-factorization library's column storage) — a conversion tax paid at every solve, residual recomputation, and factorization boundary.
- Decompress-edit-recompress round-trips for small structural edits (pinning, regularization) replay full triplet-assembly validation; at least three near-identical instances of the loop exist.

## [SPARSE_DIRECT_AND_ITERATIVE]

- Sparse symmetric-positive-definite direct factorization under a fill-reducing ordering, wrapped in a factor carrier exposing fill-in counts as evidence and recomputing the relative residual against the source operator after every solve.
- Iterative solve policy ladder: a preconditioned Krylov method under a stacked stop-criteria policy (failure, divergence, residual with hold-below count, iteration cap scaled to problem size); convergence is double-checked — solver status AND an independently recomputed residual — before acceptance, with a receipted fallback to a direct solve when the iterative route fails. The fallback is recorded, never hidden.
- Factor caching as a first-class concern: weakly keyed per-domain-object caches memoize factorizations, operator bundles, and derived solutions keyed by physical parameters; caching is success-only (failures recompute); parameter-dependent factors deliberately bypass the cache.
- Gauge handling for singular systems encoded in four distinct dialects — large diagonal penalty, row/column elimination with unit diagonal, designated gauge-node identity row, and mass-matrix regularization shift — one concern, four encodings; the clearest unharmonized numerical-slack policy on the surface.

## [SPECTRAL_EIGEN]

- Five eigensolution routes under one generic receipt shape: dense symmetric, dense general complex, sparse block-preconditioned iterative in real and Hermitian variants, and generalized symmetric-definite via triangular congruence reduction.
- A hand-written locally optimal block-preconditioned eigeniteration: deterministic seeded orthonormal start blocks, diagonal-inverse preconditioning with a unit fallback on degenerate diagonals, modified Gram–Schmidt with one reorthogonalization pass and rank-collapsed columns left zero, previous-direction dropout gated on a column-norm floor, and Rayleigh–Ritz over a stacked subspace via a dense generalized eigendecomposition. Iteration exhaustion returns finite diagnostic pairs explicitly flagged unusable rather than failing or lying.
- Real/complex polymorphism achieved by wide delegate-bundle parameterization (roughly ten injected functions) substituting for an absent numeric typeclass; further collapse is blocked by the platform lacking mixed real-by-complex generic arithmetic — a recognized ceiling, not accidental sprawl.
- Generalized problems reduced by triangular congruence with explicit re-symmetrization to absorb asymmetry drift and back-transform through adjoint solves; receipts label the route sparse while execution densifies the operator — route naming drifting from actual execution density is a live pressure.
- Eigen-residuals always recomputed against the original operator and folded to a maximum through one generic core fronted by four thin representation adapters.
- Spectral bases over-computed to a fixed default count, truncated on demand, with the receipt rewritten to the truncated request and cache-hit provenance recorded.

## [DISCRETE_OPERATORS_AND_DIFFUSION]

- Discrete exterior-calculus operator bundle (incidence operators plus diagonal metric stars derived from intrinsic edge lengths) with exactness of the boundary composition enforced numerically as a residual gate rather than symbolically.
- Three discrete Laplacian flavors behind one dispatch surface (extrinsic-weight, intrinsic-retriangulated, mollified-intrinsic) sharing triplet assembly; the extrinsic route hard-fails on negative weights, steering callers to the intrinsic route; mollification deltas are counted as evidence.
- An intrinsic remeshing kernel: a mutable builder frozen into immutable arrays, queue-driven edge flipping under a capped budget with exhaustive post-verification, and length-only trigonometry with clamped square roots throughout.
- Hermitian connection operators realified through a two-by-two block embedding so a real-only sparse direct factorization can factor them — a structural constraint imposed by the dependency, not an accident; the complex form is retained for the iterative eigensolver.
- Diffusion-based distance and transport family: heat solve, normalized gradient, divergence scatter, pinned potential solve; diffusion time scaled to squared characteristic element length; scalar, vector-valued, and signed-distance variants spanning surface, edge-element, volumetric-grid, and tetrahedral finite-element regimes — parallel pipelines that share shape but not code across surface and volumetric domains.
- Harmonic subspace extraction densifies a quadratic-in-edges normal-equation operator and runs a dense eigendecomposition inside otherwise sparse machinery — the clearest scalability cliff (quadratic memory on topologically nontrivial inputs).
- Per-cell metric inverses re-derived in every pass (assembly, divergence, interpolation) instead of cached once — repeated small-matrix inversion work across three consumers.
- Holonomy and potential decomposition restricted by topology with typed unsupported failures; conservation-style global checks (defect sums against prescribed indices) gate admission.

## [SPECTRAL_DESCRIPTORS_AND_FILTERS]

- Filter algebra as a closed union (heat, wave, biharmonic, diffusion, commute-time, identity) with per-eigenvalue weight dispatch, partial-monoid composition, zero-mode cutoffs at epsilon floors, and log-Gaussian bandwidth floors with normalized-weight-sum evidence.
- Descriptor evaluation as dense double loops over elements and modes with explicit buffer-reuse boundary escapes; normalization policies (scale by first nonzero mode, L1/L2/z-score energy) fail on degenerate spread rather than clamping; ranking through standard distance metrics with deterministic tie-breaking.

## [FIELDS_AND_IMPLICIT_SURFACES]

- Recursive scalar/vector/tensor field payload algebras (tens of cases each) dispatched through single generated switches; differential operators derived solely by central finite differences with the step epsilon carried per case rather than globally.
- Compactly supported kernel families as data rows carrying value plus radial derivatives and a smoothness status — an explicit non-claim of spatial gradients and Hessians.
- Signed-distance primitive catalog as data rows each carrying a Lipschitz constant; blend algebra with per-blend Lipschitz erosion factors folded recursively; smooth-minimum variants table-driven.
- Anisotropic metrics validated positive-definite by attempting a small dense factorization as the probe; nonfinite or nonpositive quadratic forms rejected.
- Scattered-data reconstruction: kernel interpolation by square direct solve, smoothing by stacked regularized least squares; moving-least-squares evaluation pays one orthogonal factorization plus one singular-value decomposition per query with rank, condition, and orientation-agreement acceptance gates — a heavy per-sample cost; declared-but-unexecutable reconstruction modes return typed failure evidence instead of faking success.
- Procedural noise (permutation-table gradient, simplex, cellular) with normalization gates and smoothness-aware caution propagation; derivative-based composition rejects non-smooth potentials at the factory.
- Volumetric signed-heat: node-capped regular grids, all-nodes-by-all-sources integration flagged as the hot numeric loop, softened kernels, one-sided boundary stencils, a gauge node, and calibration by source-mean shift plus interior sign flip; point location by linear cell scan with no spatial index — a noted asymptotic soft spot.

## [SAMPLING_AND_POINT_PROCESSES]

- All randomness is hash-derived and deterministic (coordinate-bit mixing with seeds; unit-interval mapping clamped away from endpoints) — no platform random generator anywhere in the sampling rail.
- Blue-noise family: active-list dart throwing with annulus checks; variable-radius variants in dual rails (candidate-set and continuous-domain with background-grid conflict scanning) sharing vocabulary and receipts but no code; weighted sample elimination with measure-derived radius bounds and deterministic tie-breaks; farthest-point and swap-improvement selection; relaxation and capacity-constrained variants with rejected-fraction residuals driving typed stop states.
- Spectrum validation as an advisory rail: sample indicators projected onto a truncated eigenbasis with a low-frequency suppression ceiling; validation failure degrades gracefully and never blocks sampling output.
- Quadratic pairwise spacing statistics and capacity-assignment scans without spatial indices — accepted asymptotic soft spots, fenced by explicit caps.

## [INTEGRATION_AND_TRAJECTORIES]

- Integrator catalog as tableau data (fixed and embedded adaptive pairs); the tableau itself is receipt-validated against structural laws and order-moment quadrature conditions under a coefficient tolerance.
- Dense output derived generically from stage derivatives through a small moment normal-equation solve, with honest order downgrade for repeated abscissae and endpoint re-verification against the step.
- Adaptive stepping with safety-factor scaling, clamped step ratios, and reject budgets; event localization through an endpoint-touch taxonomy, sign-change bracketing, and bisection over the dense-output curve when present else a linear chord — the localization kind is recorded as evidence.
- The trace loop is a deliberately bounded imperative escape with an explicit iteration cap, threading monadic state per step.

## [STATISTICS_REGISTRATION_TRANSPORT]

- Moment and covariance accumulation owned by one shared statistical engine; principal-component analysis with eigenvalue clamp floors and rank-clamp counting; clamped covariance rebuilt positive-definite by construction from the clamped spectrum.
- Curvature recovery by local quadric least squares in a principal frame under full-rank and residual gates, followed by a small shape-operator eigendecomposition; classification by tolerance bands with rejection causes split by kind.
- Rigid registration as one outer correspondence loop with per-variant inner solves dispatched by delegate rows: closed-form orthogonal alignment with determinant-sign correction; small-angle linearized and symmetric variants through least squares; robust reweighting with median-absolute-deviation scaling computed in log space against underflow; a generalized variant with per-pair precision matrices, damped normal equations, and a backtracking line search. Linearized increments are re-exponentiated exactly — a linearize-solve/exact-compose split.
- Entropic optimal transport fully in log space (max-shifted log-sum-exp, underflow clamps), balanced versus mass-relaxed update exponents, residual semantics forked by mode and named as data, debiasing via paired self-transport solves; the dense coupling is rematerialized each iteration purely for the balanced residual — quadratic per-iteration cost duplicated against the final build; nonconvergence is hard failure, never best-effort.

## [RECEIPTS_AND_EVIDENCE]

- Roughly forty-five algorithm-specific typed receipts; no generic evidence abstraction anywhere. Receipts nest hierarchically — composite receipts embed child solve, eigen, assembly, and sampling receipts.
- Receipts carry self-checking validity predicates encoding arithmetic conservation laws (accepted plus rejected equals input; rank plus nullity equals dimension; step counts tied to trail lengths; mass-conservation deltas) — the receipt is a postcondition gate re-checked at construction, so solver output cannot escape unaudited.
- Stored receipts double as tamper evidence: payload-carrying cases replay multi-conjunct cross-checks of receipt against payload and policy on every re-admission.
- Honesty markers systematically distinguish approximation tiers — approximate-versus-exact algorithm enumerations, linearization flags, normalization flags, fixed native constants surfaced as facts — claims are encoded as data, and diagnostic-but-unusable success is kept distinct from both success and failure.
- Counting-not-crashing degeneracy: every silently skipped degenerate element increments a receipt counter (degenerate elements, mollified edges, clamped eigenvalues, rejected weights, fallback edges, plateau rejects).
- Constructor-width pressure: receipts in the twenty-to-thirty-field range yield single-line construction sites hundreds of characters long; at least one helper exists solely to default-fill an oversized receipt; one optimizer receipt is constructed three times with near-identical argument lists.

## [ERROR_RAILS_AND_DISPATCH]

- One monadic failure rail with provenance keys; a closed error vocabulary (invalid input, invalid result, unsupported capability, missing context, caution); exceptions trapped exactly at external-library call boundaries and nowhere else.
- A tolerance taxonomy anchored on a host square-root-of-epsilon vocabulary plus a concentrated ledger of named magic constants (degenerate-area floors duplicated across two owners, penalty magnitudes, underflow cutoffs, robust-scale constants, bandwidth floors, iteration caps) and a residue of unnamed inline literals.
- Three-tier dispatch grammar: behavior-carrying enumerations with per-case delegates; closed unions with generated state-threaded switches; and runtime-type-directed output projection ladders — the projection ladder is structurally repeated across roughly twenty-five result owners, the largest repeated-switch family on the surface.
- Factory/admission duality: public validating factories paired with internal re-admission checks on already-constructed cases, including a rebuild-and-compare admission for domain carriers; constrained value objects gate every scalar entering the system.
- One shared validation vocabulary is load-bearing across nearly every module; polymorphic intake entry points discriminate on runtime shape instead of proliferating sibling factories.
- Success-only memoization over atomic immutable maps with structurally equal record keys, weakly rooted on native domain objects.

## [RESOURCE_AND_PERFORMANCE_BOUNDARIES]

- Native disposables threaded through lease capsules and scoped usings; ownership transfers annotated; enumerables defensively treated as disposable.
- Imperative loops confined to comment-flagged boundary kernels with stated reasons; monadic folds everywhere else; exactly one span-based surface, at the direct-factorization boundary.
- No SIMD or tensor-primitive usage anywhere: hot accumulation loops (descriptor evaluation, divergence scatter, log-sum-exp, grid integration) are scalar, bypassing both library vectorization and platform tensor primitives.
- Snapshot semantics on ingest (duplicate then own); a triangulation preamble (duplicate, convert quads, guard) repeated five-plus times; two literal small-function clones and one near-identical overload pair observed.
- Dual rails modeling one concept twice without shared code: candidate-set versus continuous-domain variable-radius sampling; surface versus volumetric finite-element pipelines; the four gauge dialects.

## [LANE_PRESSURE]

- **dense-algebra** — factorization suite with retained factor handles; residual-capped direct solves; singular-value-derived rank and conditioning; packed symmetric storage and its copy-on-write hazard; small dense probes (definiteness checks, precision matrices, moment systems); per-query factorization cost in moving least squares; quadratic densification cliffs (harmonic subspace normal operator; congruence routes that execute dense under sparse labels); registration inner solves (orthogonal alignment, damped normal equations, line search); absent SIMD/tensor primitives in dense hot loops.
- **sparse-systems** — canonical compressed-row format with executable invariants re-validated per access; triplet assembly with shared compression; symmetry-as-revalidation instead of typed witness; triple-representation conversion tax; direct factorization with fill evidence and fill-reducing ordering; preconditioned Krylov ladder with double-checked convergence and receipted fallback; four gauge-pinning dialects; weakly keyed success-only factor caching; decompress-edit-recompress round-trips; realified Hermitian block embedding driven by the real-only factorization constraint.
- **spectral-eigen** — five-route eigen receipt shape; hand-written block-preconditioned eigeniteration with delegate-bundle real/complex polymorphism at its typeclass ceiling; congruence reduction with re-symmetrization and route-name-versus-execution drift; independently recomputed eigen-residuals through one generic core; over-compute-and-truncate basis caching; filter algebra and descriptor normalization/ranking; harmonic subspace extraction and its quadratic cliff; advisory spectrum validation; unusable-but-finite exhaustion semantics.
- **fields-sampling-integration** — recursive field payload algebras with per-case finite-difference epsilons; kernel and signed-distance data rows with Lipschitz bookkeeping and erosion folds; reconstruction rails with typed unexecutable modes and heavy per-sample gates; procedural noise with smoothness propagation; volumetric and tetrahedral diffusion pipelines with capped grids, hot integration loops, gauge nodes, and calibration shifts; deterministic hash-derived sampling and the blue-noise family with dual rails; tableau-validated integrators, generic dense output, and evidence-carrying event localization; entropic transport iteration costs; missing spatial indices for point location and pairwise statistics.
- **receipts-and-rails** — algorithm-specific receipt catalog with conservation-law validity predicates and hierarchical nesting; tamper-evident re-admission replay; honesty markers and diagnostic-but-unusable statuses kept distinct from success and failure; the closed error vocabulary on one monadic rail with provenance and boundary-only exception trapping; factory/admission duality and constrained-value ingestion; the three-tier dispatch grammar and the twenty-five-site type-directed projection ladder; the named-constant tolerance ledger and its duplicated floors; success-only structural memoization; receipt constructor-width pressure.
