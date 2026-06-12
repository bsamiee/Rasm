# [RASM_PERSISTENCE_API_LZ4]

`K4os.Compression.LZ4` supplies block compression, fast and high-compression
levels, pickled payloads, streaming encoders, streaming decoders, and buffer
helpers for snapshot payload profiles.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `K4os.Compression.LZ4`
- package: `K4os.Compression.LZ4`
- assembly: `K4os.Compression.LZ4`
- namespace: `K4os.Compression.LZ4`
- asset: runtime library
- rail: snapshot-codec

## [2]-[PUBLIC_TYPES]

[CODEC_TYPES]: compression surfaces
- rail: snapshot-codec

| [INDEX] | [SYMBOL]       | [PACKAGE_ROLE]   | [CAPABILITY]           |
| :-----: | :------------- | :--------------- | :--------------------- |
|   [1]   | `LZ4Codec`     | codec root       | compresses block data  |
|   [2]   | `LZ4Pickler`   | pickle codec     | wraps framed payloads  |
|   [3]   | `LZ4Level`     | level classifier | selects compression    |
|   [4]   | `PickleHeader` | frame metadata   | describes pickle frame |

[ENCODER_TYPES]: stream encoder surfaces
- rail: snapshot-codec

| [INDEX] | [SYMBOL]               | [PACKAGE_ROLE]    | [CAPABILITY]             |
| :-----: | :--------------------- | :---------------- | :----------------------- |
|   [1]   | `ILZ4Encoder`          | encoder contract  | writes compressed blocks |
|   [2]   | `ILZ4Decoder`          | decoder contract  | reads compressed blocks  |
|   [3]   | `LZ4Encoder`           | encoder root      | writes stream payloads   |
|   [4]   | `LZ4Decoder`           | decoder root      | reads stream payloads    |
|   [5]   | `LZ4BlockEncoder`      | block encoder     | writes block payloads    |
|   [6]   | `LZ4BlockDecoder`      | block decoder     | reads block payloads     |
|   [7]   | `LZ4ChainDecoder`      | chain decoder     | reads chained payloads   |
|   [8]   | `LZ4EncoderExtensions` | encoder extension | extends encoder writes   |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: block operations
- rail: snapshot-codec

| [INDEX] | [SURFACE]           | [CALL_SHAPE] | [CAPABILITY]          |
| :-----: | :------------------ | :----------- | :-------------------- |
|   [1]   | `Encode`            | codec call   | compresses bytes      |
|   [2]   | `Decode`            | codec call   | decompresses bytes    |
|   [3]   | `MaximumOutputSize` | sizing call  | bounds output buffer  |
|   [4]   | `Pickle`            | pickle call  | writes framed payload |
|   [5]   | `Unpickle`          | pickle call  | reads framed payload  |
|   [6]   | `DecodeHeader`      | frame call   | reads frame metadata  |

[ENTRYPOINT_SCOPE]: stream operations
- rail: snapshot-codec

| [INDEX] | [SURFACE]     | [CALL_SHAPE] | [CAPABILITY]            |
| :-----: | :------------ | :----------- | :---------------------- |
|   [1]   | `EncodeBlock` | encoder call | writes compressed block |
|   [2]   | `DecodeBlock` | decoder call | reads compressed block  |
|   [3]   | `Drain`       | encoder call | flushes encoded payload |
|   [4]   | `Topup`       | decoder call | fills decoder input     |
|   [5]   | `Inject`      | decoder call | injects source payload  |

## [4]-[IMPLEMENTATION_LAW]

[COMPRESSION_PROFILE]:
- namespace: `K4os.Compression.LZ4`
- codec root: `LZ4Codec`
- frame root: `LZ4Pickler`
- level root: `LZ4Level`
- stream root: encoders and decoders

[LOCAL_ADMISSION]:
- LZ4 is a compression codec inside snapshot profiles.
- Compression level, frame shape, input bounds, and output bounds are receipt data.
- Encoders and decoders require deterministic buffer ownership.
- Compression cannot obscure redaction, retention, or schema receipts.

[RAIL_LAW]:
- Package: `K4os.Compression.LZ4`
- Owns: LZ4 compression for snapshots
- Accept: profile-declared compression
- Reject: hidden compression wrappers
