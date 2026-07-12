# [RASM_PERSISTENCE_API_EF_NAMING]

`EFCore.NamingConventions` rewrites the relational model's table, column, key, index, and constraint
names through a single EF Core convention plugin so the schema names follow one policy
(snake_case, lower, upper, camelCase, UPPER_SNAKE_CASE) instead of the CLR PascalCase default. The
consumption surface is the `Use*NamingConvention` extensions on `DbContextOptionsBuilder`; the
rewriters, the plugin, and the convention live under `EFCore.NamingConventions.Internal` and are
extension points, not direct call surfaces. Policy is applied as schema facts at model-build time, so
generated migrations carry the rewritten names.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `EFCore.NamingConventions`
- package: `EFCore.NamingConventions`
- license: Apache-2.0
- target: `net10.0`
- assembly: `EFCore.NamingConventions`
- namespace (consumption): `Microsoft.EntityFrameworkCore` (`Use*NamingConvention` builder extensions), `Microsoft.Extensions.DependencyInjection` (`NamingConventionsServiceCollectionExtensions`)
- namespace (extension points): `EFCore.NamingConventions.Internal` (`INameRewriter`, the rewriter rows, `NamingConvention`, `NamingConventionSetPlugin`, `NameRewritingConvention`, `NamingConventionsOptionsExtension`)
- asset: runtime library
- rail: schema-tooling

## [02]-[PUBLIC_TYPES]

[NAMING_TYPES]: convention plugin, rewriter contract, and rewriter rows
- rail: schema-tooling

The rewriter classes are `public` but `Internal`-namespaced — consumers extend the policy through the
`INameRewriter` contract plus the DI plugin, never by referencing a sealed rewriter type. The five
built-in rewriters are the `NamingConvention` enum cases the `Use*` extensions select.

| [INDEX] | [SYMBOL]                     | [NAMESPACE]                         | [PACKAGE_ROLE]         | [CAPABILITY]                                                            |
| :-----: | :--------------------------- | :---------------------------------- | :--------------------- | :---------------------------------------------------------------------- |
|  [01]   | `INameRewriter`              | `EFCore.NamingConventions.Internal` | rewriter contract      | `string RewriteName(string name)` — the extension point                 |
|  [02]   | `NamingConvention`           | `EFCore.NamingConventions.Internal` | policy enum            | `None`/`SnakeCase`/`LowerCase`/`CamelCase`/`UpperCase`/`UpperSnakeCase` |
|  [03]   | `NamingConventionSetPlugin`  | `EFCore.NamingConventions.Internal` | `IConventionSetPlugin` | injects `NameRewritingConvention` into the convention set               |
|  [04]   | `NameRewritingConvention`    | `EFCore.NamingConventions.Internal` | model convention       | applies the rewriter across entity/property/key/FK/index add events     |
|  [05]   | `SnakeCaseNameRewriter`      | `EFCore.NamingConventions.Internal` | rewriter row           | writes snake_case (culture-aware)                                       |
|  [06]   | `LowerCaseNameRewriter`      | `EFCore.NamingConventions.Internal` | rewriter row           | writes lowercase                                                        |
|  [07]   | `UpperCaseNameRewriter`      | `EFCore.NamingConventions.Internal` | rewriter row           | writes UPPERCASE                                                        |
|  [08]   | `CamelCaseNameRewriter`      | `EFCore.NamingConventions.Internal` | rewriter row           | writes camelCase                                                        |
|  [09]   | `UpperSnakeCaseNameRewriter` | `EFCore.NamingConventions.Internal` | rewriter row           | writes UPPER_SNAKE_CASE (`: SnakeCaseNameRewriter`)                     |

`INameRewriter` contract:
- `string RewriteName(string name)` — the single rewrite hook every built-in rewriter implements and a custom convention overrides; `NameRewritingConvention` calls it once per emitted table/column/key/index/constraint identifier.

Each built-in rewriter takes a `CultureInfo culture` constructor argument, so casing is culture-aware (the Turkish-I problem is parameterizable, not hardcoded to the invariant culture).

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: naming policy operations (namespace `Microsoft.EntityFrameworkCore`)
- rail: schema-tooling

Every policy extension carries an optional `CultureInfo? culture = null` argument and a generic
`<TContext>` mirror; the non-generic form returns `DbContextOptionsBuilder` and the generic form
returns `DbContextOptionsBuilder<TContext>`, so both `OnConfiguring` and a typed `AddDbContext<T>`
registration apply the policy without a cast. Applying a convention adds (or amends) one
`NamingConventionsOptionsExtension` on the options.

| [INDEX] | [SURFACE]                           | [CALL_SHAPE]                                                           | [CAPABILITY]            |
| :-----: | :---------------------------------- | :--------------------------------------------------------------------- | :---------------------- |
|  [01]   | `UseSnakeCaseNamingConvention`      | `(DbContextOptionsBuilder, CultureInfo? = null)` + `<TContext>` mirror | snake_case schema names |
|  [02]   | `UseLowerCaseNamingConvention`      | `(DbContextOptionsBuilder, CultureInfo? = null)` + `<TContext>` mirror | lowercase schema names  |
|  [03]   | `UseUpperCaseNamingConvention`      | `(DbContextOptionsBuilder, CultureInfo? = null)` + `<TContext>` mirror | UPPERCASE schema names  |
|  [04]   | `UseCamelCaseNamingConvention`      | `(DbContextOptionsBuilder, CultureInfo? = null)` + `<TContext>` mirror | camelCase schema names  |
|  [05]   | `UseUpperSnakeCaseNamingConvention` | `(DbContextOptionsBuilder, CultureInfo? = null)` + `<TContext>` mirror | UPPER_SNAKE_CASE names  |

`NamingConventionsServiceCollectionExtensions.AddEntityFrameworkNamingConventions(this IServiceCollection) : IServiceCollection` (namespace `Microsoft.Extensions.DependencyInjection`) admits the convention services into the DI container — the registration path the `Use*` extensions rely on; a host wiring the convention through explicit service registration uses this surface rather than the options builder.

## [04]-[IMPLEMENTATION_LAW]

[NAMING_POLICY]:
- namespace (consumption): `Microsoft.EntityFrameworkCore`; namespace (extension points): `EFCore.NamingConventions.Internal`
- entry root: `DbContextOptionsBuilder` (or `DbContextOptionsBuilder<TContext>`) via one `Use*NamingConvention` call carrying an optional `CultureInfo?`
- convention root: `NamingConventionSetPlugin` injects `NameRewritingConvention`, which calls `INameRewriter.RewriteName` per identifier across the entity/property/key/FK/index/finalizing convention hooks
- options root: `NamingConventionsOptionsExtension` (one per options instance; `Use*` amends the existing extension rather than stacking)
- rewriter root: the five `*NameRewriter` rows (`Internal`), selected by the `NamingConvention` enum; culture-aware via the `CultureInfo` ctor argument
- schema role: relational table, column, key, index, and constraint names

[STACKING]:
- Owning pages: relational naming is a schema-identity fact on `Element/identity`, applied uniformly across the provider store profiles (`Store/provisioning`) from one `Use*` call rather than a per-provider patch.
- The naming convention applies ON TOP of the relational provider mapping: it rewrites the names EF Core emits for tables/columns/keys/indexes after the provider (`Npgsql.EntityFrameworkCore.PostgreSQL`) and the store-profile mapping decide the model, so snake_case lands uniformly across the Postgres and SQLite profiles from one `Use*` call rather than per-provider naming patches.
- A custom `INameRewriter` (e.g. a domain prefix scheme) composes through the same `NamingConventionSetPlugin` seam as a built-in rewriter, so an additional naming policy is one convention registration, not a hand-written `IEntityTypeAddedConvention`.
- Because the convention rewrites at model-build time, the generated EF migrations (`Microsoft.EntityFrameworkCore.Design`) carry the rewritten names as schema facts, so the migration DDL and the runtime model agree without a second naming pass.

[LOCAL_ADMISSION]:
- Naming convention is schema policy and cannot hide inside provider-specific setup; it enters through one `Use*NamingConvention` call on the shared options builder, with `CultureInfo?` explicit where casing is locale-sensitive.
- Store profiles share one naming policy unless a profile explicitly overrides it.
- Generated migrations reflect naming policy as schema facts, not formatting preferences.
- A custom rewriter extends `INameRewriter` and is admitted through the plugin; referencing a sealed `*NameRewriter` row directly is the rejected form.

[RAIL_LAW]:
- Package: `EFCore.NamingConventions`
- Owns: relational naming convention policy (table/column/key/index/constraint identifiers)
- Accept: unified schema naming via one `Use*` call, culture-explicit, with custom rewriters through `INameRewriter`
- Reject: hand-written provider naming patches and direct references to the `Internal` rewriter rows
