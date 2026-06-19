# [RASM_PERSISTENCE_API_LZ4]

`K4os.Compression.LZ4` supplies block compression, fast and high-compression
levels, pickled payloads, streaming encoders, streaming decoders, and buffer
helpers for snapshot payload profiles.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `K4os.Compression.LZ4`
- package: `K4os.Compression.LZ4`
- assembly: `K4os.Compression.LZ4`
- namespace: `K4os.Compression.LZ4`
- asset: runtime library
- rail: snapshot-codec

## [02]-[PUBLIC_TYPES]

[CODEC_TYPES]: compression surfaces
- rail: snapshot-codec

| [INDEX] | [SYMBOL]        | [PACKAGE_ROLE]    | [CAPABILITY]             |
| :-----: | :-------------- | :---------------- | :----------------------- |
|  [01]   | `LZ4Codec`      | codec root        | compresses block data    |
|  [02]   | `LZ4Pickler`    | pickle codec      | wraps framed payloads    |
|  [03]   | `LZ4Level`      | level classifier  | selects compression      |
|  [04]   | `EncoderAction` | action classifier | classifies encoder steps |

[ENCODER_TYPES]: stream encoder surfaces (namespace `K4os.Compression.LZ4.Encoders`)
- rail: snapshot-codec

| [INDEX] | [SYMBOL]               | [PACKAGE_ROLE]    | [CAPABILITY]             |
| :-----: | :--------------------- | :---------------- | :----------------------- |
|  [01]   | `ILZ4Encoder`          | encoder contract  | writes compressed blocks |
|  [02]   | `ILZ4Decoder`          | decoder contract  | reads compressed blocks  |
|  [03]   | `LZ4Encoder`           | encoder factory   | creates stream encoders  |
|  [04]   | `LZ4Decoder`           | decoder factory   | creates stream decoders  |
|  [05]   | `LZ4BlockEncoder`      | block encoder     | writes block payloads    |
|  [06]   | `LZ4BlockDecoder`      | block decoder     | reads block payloads     |
|  [07]   | `LZ4ChainDecoder`      | chain decoder     | reads chained payloads   |
|  [08]   | `LZ4EncoderExtensions` | encoder extension | extends encoder writes   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: block operations
- rail: snapshot-codec

| [INDEX] | [SURFACE]           | [CALL_SHAPE] | [CAPABILITY]          |
| :-----: | :------------------ | :----------- | :-------------------- |
|  [01]   | `Encode`            | codec call   | compresses bytes      |
|  [02]   | `Decode`            | codec call   | decompresses bytes    |
|  [03]   | `MaximumOutputSize` | sizing call  | bounds output buffer  |
|  [04]   | `Pickle`            | pickle call  | writes framed payload |
|  [05]   | `Unpickle`          | pickle call  | reads framed payload  |
|  [06]   | `UnpickledSize`     | frame call   | reads payload size    |

[ENTRYPOINT_SCOPE]: stream operations
- rail: snapshot-codec

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY]            |
| :-----: | :-------- | :----------- | :---------------------- |
|  [01]   | `Topup`   | encoder call | fills encoder input     |
|  [02]   | `Encode`  | encoder call | writes compressed block |
|  [03]   | `Decode`  | decoder call | reads compressed block  |
|  [04]   | `Inject`  | decoder call | injects decoded payload |
|  [05]   | `Drain`   | decoder call | reads decoded bytes     |

## [04]-[IMPLEMENTATION_LAW]

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
