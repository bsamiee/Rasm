# [H1][RAILS_TOOLING]
>**Dictum:** *Each rail has one command path and one owner.*

<br>

## [1]-[MANAGED]

| [INDEX] | [COMMAND] | [USE] |
| :-----: | --------- | ----- |
| [1] | `<quality-router> static plan <paths...>` | Route preview for uncertain graph/tooling changes. |
| [2] | `<quality-router> static fix <paths...>` | Scoped whitespace/style/analyzer cleanup; mutates files. |
| [3] | `<quality-router> static report <paths...>` | Scoped whitespace/style/analyzer diagnostics; does not intentionally mutate source. |
| [4] | `<quality-router> static build <paths...>` | Routed build/analyzer/source-generation proof. |
| [5] | `<quality-router> static full` | Full-solution build/analyzer proof for trigger files. |
| [6] | `<test-runner>` | Default managed target: MTP unit tests only. |
| [7] | `<test-runner> --all` | All runnable MTP test projects. |
| [8] | `<test-runner> coverage` | Managed coverlet.MTP map. |
| [9] | `<test-runner> --mutation changed` | Changed-file Stryker MTP mutation. |
| [10] | `<test-runner> --mutation full` | Full managed Stryker MTP mutation. |
| [11] | `<test-runner> --target <project>` | Focused MTP project run. |

Managed gates isolate build output under the repo-owned artifact root. Mutation is explicit and fail-fast on the repo-owned mutation lock.

---
## [2]-[MUTATION]

Stryker is owned by the configured pure-managed project/test pair. Thresholds and reporters come from the repo mutation owner. Zero discovery fails the rail.

```bash
<quality-router> self-test
<test-runner> --mutation changed
```

---
## [3]-[RUNTIME]

| [INDEX] | [COMMAND] | [USE] |
| :-----: | --------- | ----- |
| [1] | `<runtime-runner> doctor` | Runtime health and endpoint proof. |
| [2] | `<runtime-runner> verify <scenario-or-glob>` | Runtime verification against the live host. |
| [3] | `<api-router> doctor` | Host assembly and reference map. |

Live host routes may be single-flight when the repo runtime owner declares one endpoint.
