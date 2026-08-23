# [PY_CAD_API_ZSTANDARD]

`zstandard` supplies the compressor and streaming decompressor behind Connect's zstd codec, which the OCCT provider negotiates ahead of gzip because STEP and GLB bodies dominate its traffic in both directions. Connect owns negotiation, envelope framing, and the identity fallback; this package contributes the codec alone and no fence compresses a body by hand.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `zstandard` (BSD-3-Clause)
- module: `zstandard`
- namespaces: `zstandard`
- abi: C-extension binding over the bundled libzstd
- role: implementation behind `connectrpc.compression.zstd.ZstdCompression`, never imported by a provider fence
- rail: wire compression

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: codec owners

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [CAPABILITY]                                                      |
| :-----: | :----------------- | :------------ | :---------------------------------------------------------------- |
|  [01]   | `ZstdCompressor`   | class         | level-parameterized compressor minting one frame per body         |
|  [02]   | `ZstdDecompressor` | class         | decompressor reading frames written without a content-size header |
|  [03]   | `ZstdError`        | class         | codec refusal a malformed or truncated frame raises               |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the composed Connect codec

| [INDEX] | [SURFACE]                                | [SHAPE]  | [CAPABILITY]                                        |
| :-----: | :--------------------------------------- | :------- | :-------------------------------------------------- |
|  [01]   | `ZstdCompression(level=3)`               | ctor     | Connect codec satisfying the `Compression` protocol |
|  [02]   | `ZstdCompression.name()`                 | instance | negotiated content-coding token `zstd`              |
|  [03]   | `ZstdCompressor(level=).compress(data)`  | instance | one complete frame over a whole body                |
|  [04]   | `ZstdDecompressor().stream_reader(data)` | instance | frame read tolerating an absent content size        |

- `ZstdCompression`: declared at `connectrpc.compression.zstd`, not in this package's own namespace.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Negotiation runs per call and assumes nothing: Connect defaults to gzip beside identity, so zstd reaches the wire only where mount and dial both carry it.
- Identity always survives negotiation, so a peer holding no zstd codec still completes every call and a zstd-only roster stays unrepresentable.
- Decode reads through the streaming reader, never a sized one-shot, because a frame written without a declared content size refuses the sized path.
- Level is a policy value on the codec instance, so one roster fixes it for every rpc rather than each call site choosing its own.

[STACKING]:
- `connectrpc`(`libs/python/.api/connectrpc.md`): `ZstdCompression()` and `GzipCompression()` enter the generated ASGI application through `compressions` and the dialing client through `accept_compression`, one policy row read by every mount and dial.
- within-lib `service/spool` owner: seats the ordered roster on `ProviderPolicy.compressions`, and `service/provider` hands that roster to the application so no fence names a codec twice.

[LOCAL_ADMISSION]:
- `zstandard` is admitted as direct closure because Connect's zstd codec imports it and the provider requires that codec; no provider fence imports `zstandard` itself.

[RAIL_LAW]:
- Package: `zstandard`
- Owns: Zstandard frame compression and streaming decompression beneath the Connect content-coding rail
- Accept: whole request and response bodies Connect hands the codec after envelope framing
- Reject: hand-compressed protobuf bodies, a zstd-only roster, per-method compression switches, a second frame codec beside Connect's, and a sized decompress over frames carrying no declared content size
