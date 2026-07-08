# [RASM_APPUI_API_FFMPEG_AUTOGEN]

`FFmpeg.AutoGen` is the in-process video-encode owner — CppSharp-generated unsafe bindings over the FFmpeg 8.x C API under one root namespace `FFmpeg.AutoGen`. It carries the full libavcodec/libavformat/libavutil/libswscale surface as one static `ffmpeg` hub (thousands of `av_*`/`avcodec_*`/`avformat_*`/`sws_*` facade methods delegating to runtime-resolved function pointers) plus the generated `AV*` struct and enum model. It is the encode counterpart to the `HanumanInstitute.LibMpv` playback owner: `libmpv` owns media DECODE + on-screen render, `FFmpeg.AutoGen` owns the encode path that turns an RGBA frame stream (the compositor/path-trace output) into an MP4/H.264 flythrough deliverable through the swscale RGBA→YUV420P convert, the encoder send/receive loop, and the muxer write. The native FFmpeg shared libraries are provisioned at the app-host distribution layer, never bundled with this assembly.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `FFmpeg.AutoGen`
- package: `FFmpeg.AutoGen` (the self-contained meta binding — it ships its OWN generated bindings and dynamic loader; the `FFmpeg.AutoGen.Abstractions` + `FFmpeg.AutoGen.Bindings.*` split is the MAUI-oriented alternative, NOT admitted)
- license: MIT (expression) — the binding is MIT; the native FFmpeg build carries its own GPL/LGPL license
- assembly: `FFmpeg.AutoGen`
- namespace: `FFmpeg.AutoGen` (every generated binding, struct, enum, and the `ffmpeg`/`DynamicallyLoadedBindings`/`FunctionResolverFactory` hub)
- target: `lib/netstandard2.1` (bound by the `net10.0` consumer) + `lib/netstandard2.0`; `AllowUnsafeBlocks`
- native: P/Invoke over FFmpeg 8.x shared libraries — `ffmpeg.LibraryVersionMap` pins avcodec 62 / avformat 62 / avutil 60 / swscale 9 / swresample 6 / avfilter 11 / avdevice 62; the binaries are external
- rail: encode

## [02]-[PUBLIC_TYPES]

[BINDING_HUB]: the static entrypoint + loader — rail: encode

| [INDEX] | [SYMBOL]                     | [KIND]                                                                             |
| :-----: | :--------------------------- | :-------------------------------------------------------------------------------- |
|  [01]   | `ffmpeg`                     | `public static unsafe partial class` — the whole libav* facade (RootPath, `av_*` methods, `EAGAIN`/`ENOMEM`/`EINVAL`/`EPIPE` errno, `AVERROR`/`MKTAG`/`FFERRTAG`/`AV_VERSION_*` macros); its static ctor auto-runs `DynamicallyLoadedBindings.Initialize()` |
|  [02]   | `DynamicallyLoadedBindings`  | loader — `Initialize()` wires the `vectors.*` delegates; `ThrowErrorIfFunctionNotFound` fails loudly on a missing symbol; `FunctionResolver` field |
|  [03]   | `FunctionResolverFactory`    | `GetPlatformId()` / `Create()` → the `Mac`/`Linux`/`Windows` `IFunctionResolver` that `dlopen`s `lib{name}.{version}.dylib` from `ffmpeg.RootPath` |
|  [04]   | `AVRational`                 | numerator/denominator time-base + frame-rate value struct                          |

[MUX_ENCODE_STRUCTS]: generated `public unsafe partial struct` context/handle model — rail: encode

| [INDEX] | [SYMBOL]                     | [KIND]                                                                             |
| :-----: | :--------------------------- | :-------------------------------------------------------------------------------- |
|  [01]   | `AVFormatContext`            | the muxer context — output format, streams, IO context, header/trailer state       |
|  [02]   | `AVOutputFormat`             | the container format descriptor (mp4 / matroska / …)                               |
|  [03]   | `AVStream`                   | an output stream — `time_base`, `codecpar`, index                                  |
|  [04]   | `AVCodec`                    | an encoder descriptor (H.264 / HEVC / …)                                           |
|  [05]   | `AVCodecContext`             | the encoder context — `width`/`height`/`pix_fmt`/`time_base`/`framerate`/`bit_rate`/`gop_size`/`max_b_frames`/`color_range`/`colorspace` |
|  [06]   | `AVCodecParameters`          | serialized codec params copied onto the stream (`avcodec_parameters_from_context`)  |
|  [07]   | `AVFrame`                    | a raw picture — `data`/`linesize` planes, `width`/`height`/`format`/`pts`           |
|  [08]   | `AVPacket`                   | an encoded packet — `data`/`size`/`pts`/`dts`/`stream_index`/`duration`             |
|  [09]   | `SwsContext`                 | the swscale rescale/convert context (RGBA → YUV420P)                                |
|  [10]   | `AVIOContext`                | the byte-IO sink behind the muxer (`avio_open`/`avio_closep`)                       |
|  [11]   | `AVDictionary`               | the options bag (`av_dict_set` — x264 `preset`/`crf`/`profile`)                     |
|  [12]   | `AVBufferRef`                | a reference-counted native buffer backing frame/packet data                        |

[ENUM_VOCABULARY]: generated `public enum : int` — rail: encode

| [INDEX] | [SYMBOL]                            | [KIND]                                                                      |
| :-----: | :---------------------------------- | :------------------------------------------------------------------------- |
|  [01]   | `AVCodecID`                         | encoder identity — `AV_CODEC_ID_H264` (27), `AV_CODEC_ID_HEVC`, …          |
|  [02]   | `AVPixelFormat`                     | pixel layout — `AV_PIX_FMT_RGBA` (26, encode ingress), `AV_PIX_FMT_YUV420P` (0, encode target) |
|  [03]   | `AVMediaType`                       | stream kind — `AVMEDIA_TYPE_VIDEO`, …                                       |
|  [04]   | `AVColorSpace` / `AVColorRange` / `AVColorTransferCharacteristic` / `AVColorPrimaries` | signalled color metadata — `AVCOL_SPC_BT709`, `AVCOL_RANGE_MPEG`/`AVCOL_RANGE_JPEG`, transfer/primaries |
|  [05]   | `AVPictureType`                     | frame picture type (I/P/B) hint                                             |

## [03]-[ENTRYPOINTS]

[ENCODE_PIPELINE]: the ordered libav* encode fold on `ffmpeg`
- rail: encode
- Verified `public static` facade methods (return types shown). The order below is the flythrough-encode sequence: open the muxer, find and open the encoder, allocate the frame/packet, build the RGBA→YUV converter, then loop convert → send → receive → mux, and finalize with the trailer.

| [INDEX] | [SURFACE]                                                                                      | [RETURN]           | [RAIL]  |
| :-----: | :--------------------------------------------------------------------------------------------- | :----------------- | :------ |
|  [01]   | `avformat_alloc_output_context2(AVFormatContext**, AVOutputFormat*, string format, string filename)` | `int`      | open the mp4 muxer context |
|  [02]   | `avcodec_find_encoder(AVCodecID)` / `avcodec_find_encoder_by_name(string)`                      | `AVCodec*`         | resolve the H.264 encoder |
|  [03]   | `avcodec_alloc_context3(AVCodec*)` / `avcodec_open2(AVCodecContext*, AVCodec*, AVDictionary**)` | `AVCodecContext*` / `int` | allocate + open the encoder (options via `av_dict_set`) |
|  [04]   | `avformat_new_stream(AVFormatContext*, AVCodec*)` / `avcodec_parameters_from_context(AVCodecParameters*, AVCodecContext*)` | `AVStream*` / `int` | add the stream + copy codec params |
|  [05]   | `av_frame_alloc()` / `av_frame_get_buffer(AVFrame*, int align)` / `av_frame_make_writable(AVFrame*)` | `AVFrame*` / `int` / `int` | allocate + back + unshare the picture buffer |
|  [06]   | `av_packet_alloc()`                                                                             | `AVPacket*`        | allocate the encoded-packet holder |
|  [07]   | `sws_getContext(w,h,AV_PIX_FMT_RGBA, w,h,AV_PIX_FMT_YUV420P, flags, ...)` / `sws_scale(SwsContext*, srcData, srcStride, 0, h, dstData, dstStride)` | `SwsContext*` / `int` | build + run the RGBA→YUV420P convert |
|  [08]   | `av_image_fill_arrays(data, linesize, buffer, AVPixelFormat, w, h, align)`                      | `int`              | wrap an RGBA byte span as planar `data`/`linesize` |
|  [09]   | `avio_open(AVIOContext**, string url, int flags)` / `avformat_write_header(AVFormatContext*, AVDictionary**)` | `int` / `int` | open the file sink + write the container header |
|  [10]   | `avcodec_send_frame(AVCodecContext*, AVFrame*)` / `avcodec_receive_packet(AVCodecContext*, AVPacket*)` | `int` / `int` | the send/receive encode loop (`AVERROR(EAGAIN)`/`AVERROR_EOF` drive draining) |
|  [11]   | `av_rescale_q(long, AVRational, AVRational)`                                                    | `long`             | rescale `pts`/`dts` from encoder to stream time-base |
|  [12]   | `av_interleaved_write_frame(AVFormatContext*, AVPacket*)` / `av_write_frame(...)`               | `int`              | mux the packet |
|  [13]   | `av_write_trailer(AVFormatContext*)` / `avio_closep(AVIOContext**)`                             | `int` / `int`      | finalize the container + close the sink |
|  [14]   | `av_frame_unref`/`av_frame_free` / `av_packet_unref`/`av_packet_free` / `avformat_free_context` | `void`             | teardown every native handle |

[RUNTIME_BINDING]: native provisioning + version probe on the hub
- rail: encode

| [INDEX] | [SURFACE]                                            | [SURFACE_ROOT]               | [RAIL] |
| :-----: | :--------------------------------------------------- | :--------------------------- | :----- |
|  [01]   | `RootPath { get; set; }`                             | `ffmpeg`                     | the directory native libs `dlopen` from (macOS: the homebrew `lib` dir) |
|  [02]   | `av_version_info()`                                  | `ffmpeg`                     | the linked FFmpeg build's version string |
|  [03]   | `LibraryVersionMap`                                  | `ffmpeg`                     | the pinned native major versions (FFmpeg 8.x) |
|  [04]   | `Initialize()` / `ThrowErrorIfFunctionNotFound`      | `DynamicallyLoadedBindings`  | wire the delegates (auto-run by the `ffmpeg` ctor) / fail-loud policy |
|  [05]   | `Create()` / `GetPlatformId()`                       | `FunctionResolverFactory`    | the per-platform resolver |

## [04]-[IMPLEMENTATION_LAW]

[ENCODE_LAW]:
- Package: `FFmpeg.AutoGen`
- Owns: the in-process video-encode deliverable — the libavformat muxer (`avformat_alloc_output_context2`/`avformat_write_header`/`av_interleaved_write_frame`/`av_write_trailer`), the libavcodec H.264 encoder (`avcodec_find_encoder`/`avcodec_alloc_context3`/`avcodec_open2`/`avcodec_send_frame`/`avcodec_receive_packet`), the libswscale RGBA→YUV420P convert (`sws_getContext`/`sws_scale`), and the `AVFrame`/`AVPacket`/`AVCodecContext` model plus the `av_dict_set` x264 option bag.
- Accept: `Render/capture.md` streams the compositor/path-trace RGBA frames into a single encode owner that fills an `AVFrame` (`av_image_fill_arrays` + `av_frame_make_writable`), converts to YUV420P through the persistent `SwsContext`, stamps `pts` on the encoder time-base, runs the `send_frame`/`receive_packet` loop, rescales with `av_rescale_q`, and muxes to MP4 — the flythrough deliverable; x264 `preset`/`crf`/`profile` are `av_dict_set` policy rows on `avcodec_open2`; `ThrowErrorIfFunctionNotFound = true` makes a missing native symbol a loud fault.
- Reject: shelling out to an `ffmpeg` CLI process where the in-process bindings own the pipeline; hand-rolling an MP4 muxer or an H.264 bitstream where libavformat/libavcodec own them; a second RGBA→YUV path where `SwsContext` owns the convert; leaking an `AVFrame`/`AVPacket`/`AVCodecContext`/`AVFormatContext` where the `av_*_free`/`avformat_free_context` teardown is mandatory (the unsafe surface has no finalizer); confusing this encode owner with the `libmpv` decode/playback owner — encode-out versus decode-in is the seam.

[STACKING]:
- Complements `api-libmpv.md`: `libmpv` owns media DECODE + on-screen OpenGL render (the Editing MediaSurface); `FFmpeg.AutoGen` owns the ENCODE-out path (the Render capture deliverable). Both bind external FFmpeg-family natives at the app-host distribution layer and never bundle them.
- Feeds the raster deliverable set: the RGBA ingress is the same compositor output that `api-avalonia-gpu-interop.md` / `api-skiasharp.md` produce; the MP4 output is the motion-deliverable peer to the still-image and vector-PDF exports (`api-pdfsharp.md`).

> [!IMPORTANT]
> The native FFmpeg 8.x shared libraries (`libavcodec.62` / `libavformat.62` / `libavutil.60` / `libswscale.9`) are provisioned at the app-host distribution layer (`brew install ffmpeg`, the system package manager, or a side-loaded build) and pointed at through `ffmpeg.RootPath`; this MIT binding ships no native binary. The native build's LICENSE is the FFmpeg build's, NOT the binding's: ship an LGPL-configured, dynamically-linked FFmpeg build (`--enable-shared`, no `--enable-gpl`/`--enable-nonfree`) so the deliverable stays LGPL-compliant; a GPL-enabled build (x264/x265 via `--enable-gpl`) imposes copyleft on the distributed product and is a distribution-policy decision, not a code default. A missing native runtime surfaces through `DynamicallyLoadedBindings.ThrowErrorIfFunctionNotFound`, never a silent no-op.
