# [PYTHON_IDEAS]

The cross-package Python concert: higher-order concepts that couple two or more of the five packages or deepen a shared branch owner, distilled from the folder ideas. A concept grounded in one folder's domain stays on that folder's `IDEAS.md`; a concept crossing a language boundary stays in the cross-`libs/` `IDEAS.md` and is referenced as a wire seam, never restated here. `[1]-[OPEN]` holds the live concert as cards — a bracketed slug, the capability, what it unlocks, and the cross-package gap it draws on; `[2]-[CLOSED]` records dispositions.

## [1]-[OPEN]

[CONTENT_ADDRESSED_REUSE_FABRIC]:
- Fold the runtime `ContentIdentity` seed into the runtime lane admission so a unit of work admitted as a `(ContentKey, Work[T])` pair short-circuits when the key already carries a result, and key every consumer's most expensive output — a `compute` graduated sub-result, a `geometry` tessellated GLB, a `data` egress bundle, an `artifacts` document — by that same key.
- Unlocks by-reference reuse across a companion session and across packages: the identity seed becomes an execution-elision key, not only an interchange key, so determinism and cache-hit-by-reference are derivable from the key rather than re-declared per owner.
- Draws on the gap that identity keys artifacts and lanes drain work as independent runtime owners, with nothing folding the key into the lane admission decision; the four consumers each key their outputs by `ContentIdentity` already, so the elision fabric is one runtime fold the whole branch inherits.

[GEOMETRY_KERNEL_OFFLOAD_LANE]:
- Add a CPU-bound offload variant to the runtime `LanePolicy` over `anyio.to_interpreter.run_sync` that the geometry and compute siblings hand a caller-supplied numeric kernel to — the open3d/trimesh registration loops, the scipy/JAX solver kernels — executed per-subinterpreter under the same `CapacityLimiter` and `DrainReceipt`, the lane never importing the kernel.
- Unlocks true-parallel CPU geometry and numeric work on subinterpreter-capable runtimes without a process-pool pickle tax, keeping one lane spine for I/O-bound and CPU-bound work and freeing the companion event loop during heavy tessellation and registration.
- Draws on PEP 734 subinterpreters and `anyio.to_interpreter` as a lower-overhead isolation seam the lane has not absorbed; the geometry registration stack and the compute solver routes are exactly the heavy CPU kernels that today stall the loop or serialize through `to_process`.

[ONE_MEASURED_SIGNAL_STREAM]:
- Build the runtime `observability/metrics` instrument set — companion request-duration histogram, lane drain counters folded from `DrainReceipt`, `psutil` process gauges — against one `MeterProvider`, and route every package's measured-execution signal (graduation latency, tessellation throughput, egress volume, render duration) through that one stream rather than per-package log fields.
- Unlocks one measured-execution signal stream the host scrapes without per-package metric reinvention; lane saturation, retry exhaustion, and companion latency become first-class observable metrics shared by all five packages.
- Draws on the gap that runtime receipts emit logs and trace context but mint no metrics, while the companion server and lane spine across every consumer produce exactly the count/duration/gauge signals OTel metrics own, currently lost or stuffed into log fields.

[CROSS_PACKAGE_DRIFT_GUARD]:
- Extend the runtime `evidence` `Structural` query family into a same-language drift detector locating a re-minted canonical concept — a second content-identity seed, receipt rail, retry owner, or wire-projection name — across the `compute`, `data`, `geometry`, and `artifacts` sources, feeding the `assay code` rail the named drift defect the topology law forbids.
- Unlocks an automated guard on the one-owner-per-axis law inside the branch: a sibling re-minting a runtime shape is caught at the source before it lands, distinct from the cross-language guard that lives in the cross-`libs/` pool.
- Draws on the gap that `Structural.query` returns raw byte spans with no semantic over them and the drift defect is defined in law with no detector; tree-sitter S-expression queries over the one Python grammar already in the manifest are the surface to express the intra-branch case.

[ONE_GRADUATION_RAIL_OUTWARD]:
- Hold the compute `graduation` rail as the single evidence path every package's useful offline result crosses outward, with the `geometry` registration transforms, reconstructed meshes, topology graphs, network graphs, and form-finding reaching the managed owner system through the `HandoffAxis` geometry case rather than a parallel per-package handoff.
- Unlocks one outward contract: a result graduates the same way regardless of which package produced it, so the C# owner system consumes one rail and the determinism closure references one key, never a handoff family.
- Draws on the gap that geometry, compute, and the data egress each produce graduatable evidence while only the compute rail is designed to carry it outward; the geometry case on the compute rail already exists as the single crossing, leaving the law to hold the rail singular.

## [2]-[CLOSED]

No idea has closed.
