# [PYTHON_SEAM_SPLITS]

| [SEAM]                | [MECHANICS_OWNER]           | [CONSUMER_CONSEQUENCE]                                    |
| :-------------------- | :-------------------------- | :-------------------------------------------------------- |
| host lifecycle        | `Rasm.AppHost`              | runtime accepts context and resource roots only           |
| product telemetry     | `Rasm.AppHost`              | runtime emits Python-local receipt facts only             |
| durable stores        | `Rasm.Persistence`          | data emits portable import/export bundles                 |
| query rails           | `Rasm.Persistence`          | data owns offline scan plans and query receipts           |
| production compute    | `Rasm.Compute`              | compute owns studies, assets, and handoff receipts        |
| benchmark claims      | `Rasm.Compute`              | compute emits research evidence only                      |
| live UI               | `Rasm.AppUi` and TypeScript | artifacts emits static files and visual specs             |
| Rhino/GH mutation     | bridge and C# host owners   | data reads or emits files only                            |
| Assay command surface | `tools/assay`               | libraries can be consumed internally without new commands |
| external API members  | package `.api` owner        | source uses named members only after evidence             |
