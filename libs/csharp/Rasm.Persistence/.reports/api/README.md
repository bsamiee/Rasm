# [RASM_PERSISTENCE_API]

This folder carries the Persistence package API catalogue. It maps store, provider, snapshot, redaction, and functional APIs to one store-profile rail.

## [1]-[SURFACES]

This table routes catalogue pages by API family.

| [INDEX] | [READ_FOR]             | [OPEN]              |
| :-----: | :--------------------- | :------------------ |
|   [1]   | store APIs             | [store](api-store.md) |
|   [2]   | provider APIs          | [provider](api-provider.md) |
|   [3]   | snapshot APIs          | [snapshots](api-snapshots.md) |
|   [4]   | redaction APIs         | [redaction](api-redaction.md) |
|   [5]   | functional substrate   | [functional](api-functional.md) |

## [2]-[API_LOCATORS]

This table is a lookup by locator family.

| [INDEX] | [FAMILY]  | [API_LOCATOR]                         | [LOCAL_RAIL] |
| :-----: | :-------- | :------------------------------------ | :----------- |
|   [1]   | NuGet     | `.cache/nuget/packages/<package>/`    | package      |
|   [2]   | Decompile | `tools.assay api query` or `ilspycmd` | inspection   |

## [3]-[CAPABILITIES]

This table maps catalogue pages to store rails.

| [INDEX] | [PAGE]     | [CAPABILITY]                        |
| :-----: | :--------- | :---------------------------------- |
|   [1]   | store      | EF SQLite, raw SQLite, migrations   |
|   [2]   | provider   | PostgreSQL provider and profile     |
|   [3]   | snapshots  | JSON, MessagePack, hashing, LZ4     |
|   [4]   | redaction  | classification and support export   |
|   [5]   | functional | rails and generated shapes          |

## [4]-[REJECTED]

Rejected provider and codec packages appear on the catalogue page that owns the rail.
