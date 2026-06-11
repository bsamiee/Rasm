# [RASM_PERSISTENCE_API_SNAPSHOTS]

Snapshot APIs supply semantic time serialization, JSON envelopes, MessagePack binary payloads, analyzer coverage, hashing, and LZ4 compression.

## [1]-[SURFACES]

This table is a lookup by snapshot package or platform surface.

| [INDEX] | [PACKAGE]                         | [ASSEMBLY]                         | [LOCAL_RAIL] |
| :-----: | :-------------------------------- | :--------------------------------- | :----------- |
|   [1]   | `NodaTime`                        | `NodaTime`                         | clock        |
|   [2]   | `NodaTime.Serialization.SystemTextJson` | `NodaTime.Serialization.SystemTextJson` | json |
|   [3]   | BCL JSON                          | `System.Text.Json`                 | json         |
|   [4]   | `MessagePack`                     | `MessagePack`                      | binary       |
|   [5]   | `MessagePackAnalyzer`             | analyzer package                   | binary       |
|   [6]   | `System.IO.Hashing`               | `System.IO.Hashing`                | checksum     |
|   [7]   | `K4os.Compression.LZ4`            | `K4os.Compression.LZ4`             | compression  |

## [2]-[API_LOCATORS]

This table is a lookup by assembly.

| [INDEX] | [ASSEMBLY]                         | [NAMESPACE]                       | [USING]                            | [API_LOCATOR] |
| :-----: | :--------------------------------- | :-------------------------------- | :--------------------------------- | :------------ |
|   [1]   | `NodaTime`                         | `NodaTime`                        | `NodaTime`                         | `.cache/nuget/packages/nodatime/` |
|   [2]   | `NodaTime.Serialization.SystemTextJson` | `NodaTime.Serialization.SystemTextJson` | `NodaTime.Serialization.SystemTextJson` | `.cache/nuget/packages/nodatime.serialization.systemtextjson/` |
|   [3]   | `System.Text.Json`                 | `System.Text.Json`                | `System.Text.Json`                 | shared framework |
|   [4]   | `MessagePack`                      | `MessagePack`                     | `MessagePack`                      | `.cache/nuget/packages/messagepack/` |
|   [5]   | `System.IO.Hashing`                | `System.IO.Hashing`               | `System.IO.Hashing`                | `.cache/nuget/packages/system.io.hashing/` |
|   [6]   | `K4os.Compression.LZ4`             | `K4os.Compression.LZ4`            | `K4os.Compression.LZ4`             | `.cache/nuget/packages/k4os.compression.lz4/` |

## [3]-[CAPABILITIES]

This table is a lookup by type family.

| [INDEX] | [TYPE_FAMILY]             | [ENTRY_SURFACE]              | [LOCAL_RAIL] |
| :-----: | :------------------------ | :--------------------------- | :----------- |
|   [1]   | `Instant`                 | snapshot timestamp           | clock        |
|   [2]   | JSON source generation    | readable snapshot envelope   | json         |
|   [3]   | MessagePack attributes    | binary snapshot contract     | binary       |
|   [4]   | MessagePack analyzer      | contract coverage            | binary       |
|   [5]   | `XxHash3`                 | checksum identity            | checksum     |
|   [6]   | LZ4 codec APIs            | compressed payload           | compression  |

## [4]-[REJECTED]

This table is a lookup by rejected package.

| [INDEX] | [REJECT]             | [LOCAL_RAIL] | [REASON]              |
| :-----: | :------------------- | :----------- | :-------------------- |
|   [1]   | `MessagePack.Generator` | binary    | analyzer route owns coverage |
|   [2]   | `MemoryPack`         | binary       | second binary codec   |
