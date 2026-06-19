# [RASM_PERSISTENCE_API_EF_DESIGN]

`Microsoft.EntityFrameworkCore.Design` supplies design-time services, migration
scaffolding, model scaffolding, compiled-model generation, C# code generation,
operation reporting, and reverse-engineering assets for store-profile tooling.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.EntityFrameworkCore.Design`
- package: `Microsoft.EntityFrameworkCore.Design`
- assembly: `Microsoft.EntityFrameworkCore.Design`
- namespace: `Microsoft.EntityFrameworkCore.Design`
- asset: design-time library
- rail: schema-tooling

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: design-time service surfaces
- rail: schema-tooling

| [INDEX] | [SYMBOL]                                | [PACKAGE_ROLE]       | [CAPABILITY]               |
| :-----: | :-------------------------------------- | :------------------- | :------------------------- |
|  [01]   | `DesignTimeServicesBuilder`             | service builder      | composes design services   |
|  [02]   | `DesignTimeServiceCollectionExtensions` | service extension    | registers design services  |
|  [03]   | `OperationExecutor`                     | tool executor        | dispatches tool operations |
|  [04]   | `DbContextOperations`                   | context operations   | drives context tooling     |
|  [05]   | `MigrationsOperations`                  | migration operations | drives migration tooling   |
|  [06]   | `OperationReporter`                     | reporter             | emits tool messages        |

[PUBLIC_TYPE_SCOPE]: migration and model outputs
- rail: schema-tooling

| [INDEX] | [SYMBOL]                          | [PACKAGE_ROLE]     | [CAPABILITY]             |
| :-----: | :-------------------------------- | :----------------- | :----------------------- |
|  [01]   | `MigrationsScaffolder`            | migration tool     | scaffolds migrations     |
|  [02]   | `ReverseEngineerScaffolder`       | model tool         | scaffolds store model    |
|  [03]   | `ScaffoldedMigration`             | scaffold output    | carries migration files  |
|  [04]   | `ScaffoldedModel`                 | scaffold output    | carries model files      |
|  [05]   | `ScaffoldedFile`                  | file output        | carries generated file   |
|  [06]   | `ModelCodeGenerationOptions`      | generation policy  | configures model output  |
|  [07]   | `ModelReverseEngineerOptions`     | reverse policy     | configures store inspect |
|  [08]   | `MigrationsCodeGeneratorSelector` | generator selector | selects code generator   |

[PUBLIC_TYPE_SCOPE]: C# generation surfaces
- rail: schema-tooling

| [INDEX] | [SYMBOL]                            | [PACKAGE_ROLE] | [CAPABILITY]         |
| :-----: | :---------------------------------- | :------------- | :------------------- |
|  [01]   | `CSharpHelper`                      | C# helper      | emits C# fragments   |
|  [02]   | `CSharpMigrationOperationGenerator` | C# generator   | emits migration code |
|  [03]   | `CSharpSnapshotGenerator`           | C# generator   | emits model snapshot |
|  [04]   | `CSharpDbContextGeneratorBase`      | C# generator   | emits DbContext code |
|  [05]   | `CSharpEntityTypeGeneratorBase`     | C# generator   | emits entity code    |
|  [06]   | `MigrationsBundle`                  | bundle tool    | packages migrations  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: design operations
- rail: schema-tooling

| [INDEX] | [SURFACE]                        | [CALL_SHAPE]   | [CAPABILITY]             |
| :-----: | :------------------------------- | :------------- | :----------------------- |
|  [01]   | `AddDbContextDesignTimeServices` | service call   | registers context design |
|  [02]   | `AddMigration`                   | tool operation | scaffolds migration      |
|  [03]   | `RemoveMigration`                | tool operation | removes migration        |
|  [04]   | `ScriptMigration`                | tool operation | scripts migrations       |
|  [05]   | `HasPendingModelChanges`         | tool operation | compares model state     |
|  [06]   | `GetMigrations`                  | tool operation | lists migrations         |
|  [07]   | `ScaffoldContext`                | tool operation | scaffolds context        |
|  [08]   | `Optimize`                       | tool operation | scaffolds compiled model |

## [04]-[IMPLEMENTATION_LAW]

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
