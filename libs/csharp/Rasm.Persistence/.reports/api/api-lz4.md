# [RASM_PERSISTENCE_API_LZ4]

`K4os.Compression.LZ4` supplies block compression and decompression for snapshot payloads.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `K4os.Compression.LZ4`
- package: `K4os.Compression.LZ4`
- assembly: `K4os.Compression.LZ4`
- namespace: `K4os.Compression.LZ4`
- asset: runtime library
- rail: compression

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: compression family
- rail: compression

- capability: anchors compression contract

| [INDEX] | [SYMBOL]            | [PACKAGE_ROLE]     |
| :-----: | :------------------ | :----------------- |
|   [1]   | `LZ4Codec`          | block codec        |
|   [2]   | `LZ4Level`          | compression level  |
|   [3]   | `LZ4Pickler`        | object compressor  |
|   [4]   | `LZ4Pickler.Pickle` | compressed payload |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: compression operations
- rail: compression

| [INDEX] | [SURFACE]           | [CALL_SHAPE]      | [CAPABILITY]             |
| :-----: | :------------------ | :---------------- | :----------------------- |
|   [1]   | `Encode`            | operation call    | executes operation       |
|   [2]   | `Decode`            | operation call    | executes operation       |
|   [3]   | `MaximumOutputSize` | capacity query    | bounds buffer allocation |
|   [4]   | `Pickle`            | compress object   | compresses object        |
|   [5]   | `Unpickle`          | decompress object | decompresses object      |

## [4]-[IMPLEMENTATION_LAW]

[RAIL_LAW]:
- Package: `K4os.Compression.LZ4`
- Owns: snapshot compression
- Accept: compression receipts record size and hash
- Reject: opaque compressed blobs

