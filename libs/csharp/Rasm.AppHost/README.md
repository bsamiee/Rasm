# [RASM_APPHOST]

`Rasm.AppHost` is the host-neutral runtime spine for the Rasm app packages. One package owns
host variance, lifecycle and drain, time and deadlines, configuration and options, composition,
resource lanes, diagnostics and telemetry, health and degradation, support capture, outbound
resilience, and the seven cross-package runtime ports.

## [1]-[SCOPE]

- AppHost defines the runtime contract every sibling adapts to: AppUi, Compute, Persistence,
  app roots, companion processes, and bridge roots consume its ports and policy values; AppHost
  references none of them.
- The package has zero consumers; implementation is full-capability with no holding back. The
  `.planning/` corpus is decision-complete — implementation transcribes the finalized pages and
  never re-designs them.
- A new capability is a row or case on an existing axis owner, never a new surface beside the
  budgeted owners.
- It is not a domain service layer, job framework, DI wrapper, telemetry wrapper, UI package,
  persistence package, compute implementation, or host-boundary package.

## [2]-[DOCUMENTS]

| [INDEX] | [READ_FOR]                                        | [OPEN]                                            |
| :-----: | :------------------------------------------------ | :------------------------------------------------ |
|   [1]   | rails, axes, seams, package-API map               | [architecture](ARCHITECTURE.md)                   |
|   [2]   | implementation sequence and start gates           | [roadmap](ROADMAP.md)                             |
|   [3]   | product capability atlas                          | [features](FEATURES.md)                           |
|   [4]   | implementation charter: density bar, build order  | [planning charter](.planning/README.md)           |
|   [5]   | decision-complete concept pages                   | [.planning](.planning/README.md)                  |
|   [6]   | external package API catalogues                   | [.reports/api](.reports/api/README.md)            |
|   [7]   | C# stack doctrine                                 | [C# stack](../../../docs/stacks/csharp/README.md) |

The charter is the only linking file in the planning set; its PAGE_INDEX routes to all eleven
concept pages. Package-local documents state AppHost law and package API facts only.
