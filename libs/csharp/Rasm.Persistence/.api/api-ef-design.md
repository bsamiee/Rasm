# [RASM_PERSISTENCE_API_EF_DESIGN]

`Microsoft.EntityFrameworkCore.Design` supplies design-time services, migration
scaffolding, model scaffolding, compiled-model generation, C# code generation,
operation reporting, and reverse-engineering assets for store-profile tooling.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.EntityFrameworkCore.Design`
- package: `Microsoft.EntityFrameworkCore.Design`
- assembly: `Microsoft.EntityFrameworkCore.Design`
- namespace: `Microsoft.EntityFrameworkCore.Design`
- asset: design-time library
- rail: schema-tooling

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: design-time service surfaces
- rail: schema-tooling

| [INDEX] | [SYMBOL]                                | [PACKAGE_ROLE]       | [CAPABILITY]               |
| :-----: | :-------------------------------------- | :------------------- | :------------------------- |
|   [1]   | `DesignTimeServicesBuilder`             | service builder      | composes design services   |
|   [2]   | `DesignTimeServiceCollectionExtensions` | service extension    | registers design services  |
|   [3]   | `OperationExecutor`                     | tool executor        | dispatches tool operations |
|   [4]   | `DbContextOperations`                   | context operations   | drives context tooling     |
|   [5]   | `MigrationsOperations`                  | migration operations | drives migration tooling   |
|   [6]   | `OperationReporter`                     | reporter             | emits tool messages        |

[PUBLIC_TYPE_SCOPE]: migration and model outputs
- rail: schema-tooling

| [INDEX] | [SYMBOL]                          | [PACKAGE_ROLE]     | [CAPABILITY]             |
| :-----: | :-------------------------------- | :----------------- | :----------------------- |
|   [1]   | `MigrationsScaffolder`            | migration tool     | scaffolds migrations     |
|   [2]   | `ReverseEngineerScaffolder`       | model tool         | scaffolds store model    |
|   [3]   | `ScaffoldedMigration`             | scaffold output    | carries migration files  |
|   [4]   | `ScaffoldedModel`                 | scaffold output    | carries model files      |
|   [5]   | `ScaffoldedFile`                  | file output        | carries generated file   |
|   [6]   | `ModelCodeGenerationOptions`      | generation policy  | configures model output  |
|   [7]   | `ModelReverseEngineerOptions`     | reverse policy     | configures store inspect |
|   [8]   | `MigrationsCodeGeneratorSelector` | generator selector | selects code generator   |

[PUBLIC_TYPE_SCOPE]: C# generation surfaces
- rail: schema-tooling

| [INDEX] | [SYMBOL]                            | [PACKAGE_ROLE] | [CAPABILITY]         |
| :-----: | :---------------------------------- | :------------- | :------------------- |
|   [1]   | `CSharpHelper`                      | C# helper      | emits C# fragments   |
|   [2]   | `CSharpMigrationOperationGenerator` | C# generator   | emits migration code |
|   [3]   | `CSharpSnapshotGenerator`           | C# generator   | emits model snapshot |
|   [4]   | `CSharpDbContextGeneratorBase`      | C# generator   | emits DbContext code |
|   [5]   | `CSharpEntityTypeGeneratorBase`     | C# generator   | emits entity code    |
|   [6]   | `MigrationsBundle`                  | bundle tool    | packages migrations  |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: design operations
- rail: schema-tooling

| [INDEX] | [SURFACE]                        | [CALL_SHAPE]   | [CAPABILITY]             |
| :-----: | :------------------------------- | :------------- | :----------------------- |
|   [1]   | `AddDbContextDesignTimeServices` | service call   | registers context design |
|   [2]   | `AddMigration`                   | tool operation | scaffolds migration      |
|   [3]   | `RemoveMigration`                | tool operation | removes migration        |
|   [4]   | `ScriptMigration`                | tool operation | scripts migrations       |
|   [5]   | `HasPendingModelChanges`         | tool operation | compares model state     |
|   [6]   | `GetMigrations`                  | tool operation | lists migrations         |
|   [7]   | `ScaffoldContext`                | tool operation | scaffolds context        |
|   [8]   | `Optimize`                       | tool operation | scaffolds compiled model |

## [4]-[IMPLEMENTATION_LAW]

[TOOL_ADMISSION]:
- dependency role: tool-only design package
- project role: private design asset
- schema role: migration and scaffold generation
- output role: generated files require owner-local placement before production source uses them

[LOCAL_ADMISSION]:
- Design-time services support store-profile schema work and do not become runtime dependencies.
- Scaffolding output is reviewed as generated shape before it enters source.
- Migration generation is provider-aware and vocabulary-neutral.
- Reverse engineering is an implementation aid, not a public store contract.

[RAIL_LAW]:
- Package: `Microsoft.EntityFrameworkCore.Design`
- Owns: EF design-time schema tooling
- Accept: private tool asset
- Reject: runtime dependency on design tooling
