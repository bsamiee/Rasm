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

The hub resolves FFmpeg shared-library members from `ffmpeg.RootPath`, and `ffmpeg` static construction runs `DynamicallyLoadedBindings.Initialize()`.

| [INDEX] | [SYMBOL]                    | [KIND]                                      |
| :-----: | :-------------------------- | :------------------------------------------ |
|  [01]   | `ffmpeg`                    | `public static unsafe partial class` facade |
|  [02]   | `DynamicallyLoadedBindings` | loader                                      |
|  [03]   | `FunctionResolverFactory`   | resolver factory                            |
|  [04]   | `AVRational`                | time-base and frame-rate value struct       |

[FFMPEG_FACADE]: `ffmpeg` owns `RootPath`, the `av_*` methods, the `EAGAIN`/`ENOMEM`/`EINVAL`/`EPIPE` errno values, and the `AVERROR`/`MKTAG`/`FFERRTAG`/`AV_VERSION_*` macros.

[DYNAMIC_LOADER]: `Initialize()` wires the `vectors.*` delegates through `FunctionResolver`, and `ThrowErrorIfFunctionNotFound` faults on a missing symbol.

[RESOLVER_FACTORY]: `GetPlatformId()` and `Create()` select the `Mac`, `Linux`, or `Windows` `IFunctionResolver` that loads `lib{name}.{version}.dylib`.

[MUX_ENCODE_STRUCTS]: generated `public unsafe partial struct` context/handle model — rail: encode

| [INDEX] | [SYMBOL]            | [KIND]                    |
| :-----: | :------------------ | :------------------------ |
|  [01]   | `AVFormatContext`   | muxer context             |
|  [02]   | `AVOutputFormat`    | container descriptor      |
|  [03]   | `AVStream`          | output stream             |
|  [04]   | `AVCodec`           | encoder descriptor        |
|  [05]   | `AVCodecContext`    | encoder context           |
|  [06]   | `AVCodecParameters` | serialized codec settings |
|  [07]   | `AVFrame`           | raw picture               |
|  [08]   | `AVPacket`          | encoded packet            |
|  [09]   | `SwsContext`        | pixel-conversion context  |
|  [10]   | `AVIOContext`       | muxer byte sink           |
|  [11]   | `AVDictionary`      | options bag               |
|  [12]   | `AVBufferRef`       | native buffer reference   |

[FORMAT_STATE]: `AVFormatContext` carries the output format, streams, IO context, and header/trailer state; `AVOutputFormat` identifies formats such as MP4 and Matroska.

[CODEC_STATE]: `AVCodecContext` carries `width`, `height`, `pix_fmt`, `time_base`, `framerate`, `bit_rate`, `gop_size`, `max_b_frames`, `color_range`, and `colorspace`.

[STREAM_STATE]: `AVStream` carries `time_base`, `codecpar`, and the stream index; `avcodec_parameters_from_context` copies `AVCodecParameters` onto it.

[PICTURE_STATE]: `AVFrame` carries `data` and `linesize` planes plus `width`, `height`, `format`, and `pts`; `AVPacket` carries `data`, `size`, `pts`, `dts`, `stream_index`, and `duration`.

[CONVERSION_STATE]: `SwsContext` converts RGBA to YUV420P, while `AVIOContext` binds `avio_open` and `avio_closep` to the muxer sink.

[OPTION_STATE]: `AVDictionary` carries the `av_dict_set` x264 `preset`, `crf`, and `profile` values, and `AVBufferRef` retains native frame or packet storage.

[ENUM_VOCABULARY]: generated `public enum : int` — rail: encode

| [INDEX] | [SYMBOL]                        | [KIND]           |
| :-----: | :------------------------------ | :--------------- |
|  [01]   | `AVCodecID`                     | encoder identity |
|  [02]   | `AVPixelFormat`                 | pixel layout     |
|  [03]   | `AVMediaType`                   | stream kind      |
|  [04]   | `AVColorSpace`                  | color space      |
|  [05]   | `AVColorRange`                  | color range      |
|  [06]   | `AVColorTransferCharacteristic` | color transfer   |
|  [07]   | `AVColorPrimaries`              | color primaries  |
|  [08]   | `AVPictureType`                 | frame-type hint  |

[ENCODER_VALUES]: `AVCodecID` includes `AV_CODEC_ID_H264` (`27`) and `AV_CODEC_ID_HEVC`, while `AVMediaType` includes `AVMEDIA_TYPE_VIDEO`.

[PIXEL_VALUES]: `AVPixelFormat` identifies `AV_PIX_FMT_RGBA` (`26`) at encode ingress and `AV_PIX_FMT_YUV420P` (`0`) at the encode target.

[COLOR_VALUES]: `AVColorSpace` includes `AVCOL_SPC_BT709`, and `AVColorRange` includes `AVCOL_RANGE_MPEG` and `AVCOL_RANGE_JPEG`; transfer and primaries carry the corresponding signalled metadata.

[PICTURE_VALUES]: `AVPictureType` carries I-, P-, or B-frame hints.

## [03]-[ENTRYPOINTS]

[ENCODE_PIPELINE]: the ordered libav\* encode fold on `ffmpeg`
- rail: encode
- Verified `public static` facade methods appear in flythrough-encode order: open, configure, allocate, convert, send, receive, mux, finalize, and release.

| [INDEX] | [SURFACE]                                                                                            | [RETURN]          |
| :-----: | :--------------------------------------------------------------------------------------------------- | :---------------- |
|  [01]   | `avformat_alloc_output_context2(AVFormatContext**, AVOutputFormat*, string format, string filename)` | `int`             |
|  [02]   | `avcodec_find_encoder(AVCodecID)`                                                                    | `AVCodec*`        |
|  [03]   | `avcodec_find_encoder_by_name(string)`                                                               | `AVCodec*`        |
|  [04]   | `avcodec_alloc_context3(AVCodec*)`                                                                   | `AVCodecContext*` |
|  [05]   | `avcodec_open2(AVCodecContext*, AVCodec*, AVDictionary**)`                                           | `int`             |
|  [06]   | `avformat_new_stream(AVFormatContext*, AVCodec*)`                                                    | `AVStream*`       |
|  [07]   | `avcodec_parameters_from_context(AVCodecParameters*, AVCodecContext*)`                               | `int`             |
|  [08]   | `av_frame_alloc()`                                                                                   | `AVFrame*`        |
|  [09]   | `av_frame_get_buffer(AVFrame*, int align)`                                                           | `int`             |
|  [10]   | `av_frame_make_writable(AVFrame*)`                                                                   | `int`             |
|  [11]   | `av_packet_alloc()`                                                                                  | `AVPacket*`       |
|  [12]   | `sws_getContext(w,h,AV_PIX_FMT_RGBA, w,h,AV_PIX_FMT_YUV420P, flags, ...)`                            | `SwsContext*`     |
|  [13]   | `sws_scale(SwsContext*, srcData, srcStride, 0, h, dstData, dstStride)`                               | `int`             |
|  [14]   | `av_image_fill_arrays(data, linesize, buffer, AVPixelFormat, w, h, align)`                           | `int`             |
|  [15]   | `avio_open(AVIOContext**, string url, int flags)`                                                    | `int`             |
|  [16]   | `avformat_write_header(AVFormatContext*, AVDictionary**)`                                            | `int`             |
|  [17]   | `avcodec_send_frame(AVCodecContext*, AVFrame*)`                                                      | `int`             |
|  [18]   | `avcodec_receive_packet(AVCodecContext*, AVPacket*)`                                                 | `int`             |
|  [19]   | `av_rescale_q(long, AVRational, AVRational)`                                                         | `long`            |
|  [20]   | `av_interleaved_write_frame(AVFormatContext*, AVPacket*)`                                            | `int`             |
|  [21]   | `av_write_frame(...)`                                                                                | `int`             |
|  [22]   | `av_write_trailer(AVFormatContext*)`                                                                 | `int`             |
|  [23]   | `avio_closep(AVIOContext**)`                                                                         | `int`             |
|  [24]   | `av_frame_unref`                                                                                     | `void`            |
|  [25]   | `av_frame_free`                                                                                      | `void`            |
|  [26]   | `av_packet_unref`                                                                                    | `void`            |
|  [27]   | `av_packet_free`                                                                                     | `void`            |
|  [28]   | `avformat_free_context`                                                                              | `void`            |

[OPEN]: `avformat_alloc_output_context2` creates the MP4 muxer context, `avcodec_find_encoder*` resolves H.264, and `avcodec_open2` consumes options set through `av_dict_set`.

[STREAM]: `avformat_new_stream` adds the stream, and `avcodec_parameters_from_context` copies its codec settings.

[FRAME]: `av_frame_alloc`, `av_frame_get_buffer`, and `av_frame_make_writable` allocate, back, and unshare the picture buffer; `av_packet_alloc` creates the encoded-packet holder.

[CONVERT]: `av_image_fill_arrays` wraps RGBA bytes as `data` and `linesize`, while `sws_getContext` and `sws_scale` convert RGBA to YUV420P.

[MUX]: `avio_open` binds the sink, `avformat_write_header` opens the container, `AVERROR(EAGAIN)` and `AVERROR_EOF` drive send/receive draining, `av_rescale_q` converts packet timestamps to stream time-base, and `av_interleaved_write_frame` or `av_write_frame` muxes each packet.

[FINALIZE]: `av_write_trailer` closes the container contract, `avio_closep` closes the sink, and the unref/free surfaces release every native handle.

[RUNTIME_BINDING]: native provisioning + version probe on the hub
- rail: encode

| [INDEX] | [SURFACE]                      | [SURFACE_ROOT]              |
| :-----: | :----------------------------- | :-------------------------- |
|  [01]   | `RootPath { get; set; }`       | `ffmpeg`                    |
|  [02]   | `av_version_info()`            | `ffmpeg`                    |
|  [03]   | `LibraryVersionMap`            | `ffmpeg`                    |
|  [04]   | `Initialize()`                 | `DynamicallyLoadedBindings` |
|  [05]   | `ThrowErrorIfFunctionNotFound` | `DynamicallyLoadedBindings` |
|  [06]   | `Create()`                     | `FunctionResolverFactory`   |
|  [07]   | `GetPlatformId()`              | `FunctionResolverFactory`   |

[ROOT_PATH]: `RootPath` identifies the native-library directory, including the Homebrew `lib` directory on macOS.

[VERSION_PROBE]: `av_version_info()` returns the linked FFmpeg build string, while `LibraryVersionMap` pins the FFmpeg 8.x native majors.

[BINDING_POLICY]: `ffmpeg` construction runs `Initialize()`, `ThrowErrorIfFunctionNotFound` faults on an absent function, and the factory selects the platform resolver.

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
