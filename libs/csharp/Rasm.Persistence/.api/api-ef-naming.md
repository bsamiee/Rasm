# [RASM_PERSISTENCE_API_EF_NAMING]

`EFCore.NamingConventions` binds one casing policy to every relational identifier EF Core derives — table, view, column, JSON container column, key, foreign-key constraint, and index — as a model-build convention displacing the CLR PascalCase default, so the runtime model and the migrations scaffolded from it carry the same names with no second naming pass.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `EFCore.NamingConventions`
- package: `EFCore.NamingConventions` (`Apache-2.0`, Shay Rojansky)
- assembly: `EFCore.NamingConventions`
- namespace: `Microsoft.EntityFrameworkCore`, `Microsoft.Extensions.DependencyInjection` (consumption); `EFCore.NamingConventions.Internal` (extension points)
- rail: schema-tooling

## [02]-[PUBLIC_TYPES]

[NAMING_TYPES]: policy carrier, convention installer, and the rewrite contract — every `Internal` symbol ships `public` and `virtual`, and each rewriter row takes a `CultureInfo` constructor argument, so casing is locale-parameterized rather than pinned to the invariant culture and `UpperSnakeCaseNameRewriter` derives its result from `SnakeCaseNameRewriter`.

| [INDEX] | [SYMBOL]                                       | [TYPE_FAMILY] | [CAPABILITY]                                           |
| :-----: | :--------------------------------------------- | :------------ | :----------------------------------------------------- |
|  [01]   | `NamingConventionsExtensions`                  | class         | the `Use*NamingConvention` builder-extension family    |
|  [02]   | `NamingConventionsServiceCollectionExtensions` | class         | DI registration of the convention plugin               |
|  [03]   | `INameRewriter`                                | interface     | the one per-identifier rewrite hook                    |
|  [04]   | `NamingConvention`                             | enum          | the casing-policy case vocabulary                      |
|  [05]   | `NamingConventionsOptionsExtension`            | class         | `IDbContextOptionsExtension` carrying policy + culture |
|  [06]   | `NamingConventionSetPlugin`                    | class         | `IConventionSetPlugin` selecting the rewriter row      |
|  [07]   | `NameRewritingConvention`                      | class         | applies one rewriter across the model-build hooks      |

`[NamingConvention]`: `None` `SnakeCase` `LowerCase` `CamelCase` `UpperCase` `UpperSnakeCase`

`[INameRewriter rows]`: `SnakeCaseNameRewriter` `LowerCaseNameRewriter` `UpperCaseNameRewriter` `CamelCaseNameRewriter` `UpperSnakeCaseNameRewriter`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: policy admission and the extension points beneath it. Every `Use*NamingConvention` takes `(DbContextOptionsBuilder, CultureInfo? = null)`, returns the builder, and mirrors as `<TContext>` over `DbContextOptionsBuilder<TContext>`, so `OnConfiguring` and a typed `AddDbContext<T>` registration both apply the policy without a cast.

`[NamingConventionsExtensions]`: `UseSnakeCaseNamingConvention` `UseLowerCaseNamingConvention` `UseUpperCaseNamingConvention` `UseCamelCaseNamingConvention` `UseUpperSnakeCaseNamingConvention`

`[NamingConventionsOptionsExtension]`: `WithSnakeCaseNamingConvention` `WithLowerCaseNamingConvention` `WithUpperCaseNamingConvention` `WithCamelCaseNamingConvention` `WithUpperSnakeCaseNamingConvention` `WithoutNaming` — each `(CultureInfo? = null)` returning the re-minted extension, the disarming form taking none; a hand-composed `DbContextOptions` mounts policy here without the builder.

| [INDEX] | [SURFACE]                                                                          | [SHAPE]  | [CAPABILITY]                         |
| :-----: | :--------------------------------------------------------------------------------- | :------- | :----------------------------------- |
|  [01]   | `AddEntityFrameworkNamingConventions(IServiceCollection)`                          | static   | registers the plugin in DI           |
|  [02]   | `RewriteName(string) -> string`                                                    | instance | the per-identifier rewrite hook      |
|  [03]   | `ModifyConventions(ConventionSet) -> ConventionSet`                                | instance | installs the rewriting convention    |
|  [04]   | `NameRewritingConvention(ProviderConventionSetBuilderDependencies, INameRewriter)` | ctor     | binds a custom rewriter to the model |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every identifier the relational model derives passes `INameRewriter.RewriteName` once at model build, so migration DDL and the runtime model agree by construction.
- One `NamingConventionsOptionsExtension` carries the policy per options instance: a `Use*` call reads the extension already mounted and re-mints it, so a second call re-points the casing instead of stacking a second rewrite pass.
- Every rewrite lands at convention configuration source, so a name pinned through `ToTable`, `HasColumnName`, or `HasName` survives verbatim and `NamingConvention.None` returns the convention set untouched.
- `NamingConventionSetPlugin` maps the enum case to its rewriter row and admits no custom one, so a bespoke policy constructs `NameRewritingConvention` against its own `INameRewriter` and mounts it through a sibling `IConventionSetPlugin`.

[STACKING]:
- `api-npgsql-ef`(`.api/api-npgsql-ef.md`): rides the same `DbContextOptionsBuilder` as `UseNpgsql` — the provider decides the relational model and this convention rewrites the identifiers it emits, so one call covers every PostgreSQL table, column, key, and index.
- `api-ef-sqlite`(`.api/api-ef-sqlite.md`): the same one call binds the `UseSqlite` profile, so both store profiles carry one casing policy rather than a per-provider patch.
- `api-thinktecture-ef`(`.api/api-thinktecture-ef.md`): `UseThinktectureValueConverters` and `Use*NamingConvention` mount as peer options extensions on one builder — generated converters decide column types, this convention decides column names, and neither orders the other.
- `api-ef-design`(`.api/api-ef-design.md`): `OperationExecutor` scaffolds migrations and compiles models over already-rewritten names, so emitted DDL and the `Optimize` compiled model need no naming fixup.
- within-lib: `Element/identity` chains `UseSnakeCaseNamingConvention()` into `ConverterRail.Compose` on the one `IdentityContext`, leaving `IdentityShape`/`NodeCellShape` to carry only what conventions cannot derive.

[LOCAL_ADMISSION]:
- Store profiles share one policy, and a profile that diverges declares the override at its own options composition.
- A casing flip is a schema change rather than a formatting preference: the migration it generates renames real database objects.

[RAIL_LAW]:
- Package: `EFCore.NamingConventions`
- Owns: relational identifier casing across table, view, column, JSON container, key, constraint, and index names
- Accept: one `Use*` admission per options builder, culture-explicit, extended through `INameRewriter`
- Reject: hand-written provider naming patches and hand-rolled model-build naming conventions
