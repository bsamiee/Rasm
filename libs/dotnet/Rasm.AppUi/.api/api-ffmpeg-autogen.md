# [RASM_APPUI_API_FFMPEG_AUTOGEN]

`FFmpeg.AutoGen` owns the in-process video-encode rail: CppSharp-generated unsafe bindings expose the full libavcodec/libavformat/libavutil/libswscale C surface as one static `ffmpeg` facade over runtime-resolved function pointers, with the generated `AV*` struct and enum model. It turns the compositor/path-trace RGBA frame stream into an MP4/H.264 flythrough through the swscale RGBA→YUV420P convert, the encoder send/receive loop, and the muxer write — the encode counterpart to the `libmpv` decode/playback owner.

## [01]-[PUBLIC_TYPES]

[BINDING_HUB]: the static entrypoint, delegate-vector loader, and platform resolver.

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [CAPABILITY]                        |
| :-----: | :-------------------------- | :------------ | :---------------------------------- |
|  [01]   | `ffmpeg`                    | class         | static facade over the `av_*` C API |
|  [02]   | `DynamicallyLoadedBindings` | class         | delegate-vector loader              |
|  [03]   | `FunctionResolverFactory`   | class         | platform resolver selector          |
|  [04]   | `AVRational`                | struct        | time-base and frame-rate value      |

[HUB_MACROS]: encode-loop errno and macro members on `ffmpeg` — `AVERROR` `AVERROR_EOF` `MKTAG` `FFERRTAG`, errno `EAGAIN` `ENOMEM` `EINVAL` `EPIPE`.

[ENCODE_STRUCTS]: generated `unsafe partial struct` context and handle model.

| [INDEX] | [SYMBOL]            | [CAPABILITY]              |
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

[STRUCT_STATE]: caller-set fields on the encode context model.
- `AVCodecContext`: `width` `height` `pix_fmt` `time_base` `framerate` `bit_rate` `gop_size` `max_b_frames` `color_range` `colorspace`
- `AVFrame`: `data` `linesize` `width` `height` `format` `pts`
- `AVPacket`: `data` `size` `pts` `dts` `stream_index` `duration`
- `AVStream`: `time_base` `codecpar` `index`

[ENCODE_ENUMS]: generated `enum : int` vocabularies.

| [INDEX] | [SYMBOL]                        | [CAPABILITY]     |
| :-----: | :------------------------------ | :--------------- |
|  [01]   | `AVCodecID`                     | encoder identity |
|  [02]   | `AVPixelFormat`                 | pixel layout     |
|  [03]   | `AVMediaType`                   | stream kind      |
|  [04]   | `AVColorSpace`                  | color space      |
|  [05]   | `AVColorRange`                  | color range      |
|  [06]   | `AVColorTransferCharacteristic` | color transfer   |
|  [07]   | `AVColorPrimaries`              | color primaries  |
|  [08]   | `AVPictureType`                 | frame-type hint  |
|  [09]   | `SwsFlags`                      | scaler algorithm |

[ENUM_MEMBERS]: encode-path enum values.
- `AVCodecID`: `AV_CODEC_ID_H264` `AV_CODEC_ID_HEVC`
- `AVPixelFormat`: `AV_PIX_FMT_RGBA` `AV_PIX_FMT_BGRA` `AV_PIX_FMT_RGBA64LE` ingress, `AV_PIX_FMT_YUV420P` target
- `SwsFlags`: `SWS_FAST_BILINEAR` `SWS_BILINEAR` `SWS_BICUBIC` `SWS_LANCZOS` — `sws_getContext` takes the bitmask as a bare `int`, so a row casts; the hub publishes no `SWS_*` constant of its own
- `AVMediaType`: `AVMEDIA_TYPE_VIDEO`; `AVPictureType`: I/P/B-frame hints
- `AVColorSpace`: `AVCOL_SPC_BT709`; `AVColorRange`: `AVCOL_RANGE_MPEG` `AVCOL_RANGE_JPEG`

## [02]-[ENTRYPOINTS]

[ENCODE_PIPELINE]: the ordered libav\* encode fold on `ffmpeg`, `public static` facade methods in flythrough order — open, configure, allocate, convert, send, receive, mux, finalize, release.

| [INDEX] | [SURFACE]                                                                                              | [CAPABILITY]                |
| :-----: | :----------------------------------------------------------------------------------------------------- | :-------------------------- |
|  [01]   | `avformat_alloc_output_context2(AVFormatContext**, AVOutputFormat*, string, string) -> int`            | allocate MP4 muxer context  |
|  [02]   | `avcodec_find_encoder(AVCodecID) -> AVCodec*`                                                          | resolve encoder by id       |
|  [03]   | `avcodec_find_encoder_by_name(string) -> AVCodec*`                                                     | resolve encoder by name     |
|  [04]   | `avcodec_alloc_context3(AVCodec*) -> AVCodecContext*`                                                  | allocate encoder context    |
|  [05]   | `avcodec_open2(AVCodecContext*, AVCodec*, AVDictionary**) -> int`                                      | open encoder with options   |
|  [06]   | `avformat_new_stream(AVFormatContext*, AVCodec*) -> AVStream*`                                         | add output stream           |
|  [07]   | `avcodec_parameters_from_context(AVCodecParameters*, AVCodecContext*) -> int`                          | copy codec params to stream |
|  [08]   | `av_frame_alloc() -> AVFrame*`                                                                         | allocate frame              |
|  [09]   | `av_frame_get_buffer(AVFrame*, int) -> int`                                                            | back frame planes           |
|  [10]   | `av_frame_make_writable(AVFrame*) -> int`                                                              | unshare frame buffer        |
|  [11]   | `av_packet_alloc() -> AVPacket*`                                                                       | allocate packet             |
|  [12]   | `sws_getContext(int, int, AVPixelFormat, int, int, AVPixelFormat, int, ...) -> SwsContext*`            | RGBA→YUV420P scaler         |
|  [13]   | `sws_scale(SwsContext*, byte*[], int[], int, int, byte*[], int[]) -> int`                              | convert one frame           |
|  [14]   | `av_image_fill_arrays(ref byte_ptrArray4, ref int_array4, byte*, AVPixelFormat, int, int, int) -> int` | wrap RGBA as planes         |
|  [15]   | `avio_open(AVIOContext**, string, int) -> int`                                                         | bind file output sink       |
|  [16]   | `avio_open_dyn_buf(AVIOContext**) -> int`                                                              | bind in-memory output sink  |
|  [17]   | `avformat_write_header(AVFormatContext*, AVDictionary**) -> int`                                       | open container              |
|  [18]   | `avcodec_send_frame(AVCodecContext*, AVFrame*) -> int`                                                 | submit frame to encoder     |
|  [19]   | `avcodec_receive_packet(AVCodecContext*, AVPacket*) -> int`                                            | drain encoded packet        |
|  [20]   | `av_rescale_q(long, AVRational, AVRational) -> long`                                                   | rescale to stream base      |
|  [21]   | `av_interleaved_write_frame(AVFormatContext*, AVPacket*) -> int`                                       | mux a packet interleaved    |
|  [22]   | `av_write_frame(AVFormatContext*, AVPacket*) -> int`                                                   | mux a packet direct         |
|  [23]   | `av_write_trailer(AVFormatContext*) -> int`                                                            | finalize container          |
|  [24]   | `avio_closep(AVIOContext**) -> int`                                                                    | close file sink             |
|  [25]   | `avio_close_dyn_buf(AVIOContext*, byte** buffer) -> int`                                               | take dyn-buf bytes + size   |
|  [26]   | `av_frame_unref(AVFrame*) -> void`                                                                     | reset a frame               |
|  [27]   | `av_frame_free(AVFrame**) -> void`                                                                     | release a frame             |
|  [28]   | `av_packet_unref(AVPacket*) -> void`                                                                   | reset a packet              |
|  [29]   | `av_packet_free(AVPacket**) -> void`                                                                   | release a packet            |
|  [30]   | `avformat_free_context(AVFormatContext*) -> void`                                                      | release muxer context       |
|  [31]   | `av_free(void*) -> void`                                                                               | release a dyn-buf buffer    |

[RUNTIME_BINDING]: native provisioning and version probe on the loader hub.

| [INDEX] | [SURFACE]                                                | [SHAPE]       | [CAPABILITY]                        |
| :-----: | :------------------------------------------------------- | :------------ | :---------------------------------- |
|  [01]   | `ffmpeg.RootPath { get; set; }`                          | property      | native-library directory            |
|  [02]   | `ffmpeg.av_version_info() -> string`                     | static method | linked FFmpeg build string          |
|  [03]   | `ffmpeg.LibraryVersionMap`                               | static field  | SONAME major pins                   |
|  [04]   | `DynamicallyLoadedBindings.Initialize()`                 | static method | wire the `vectors.*` delegate table |
|  [05]   | `DynamicallyLoadedBindings.ThrowErrorIfFunctionNotFound` | static field  | fault on an absent function         |
|  [06]   | `FunctionResolverFactory.Create() -> IFunctionResolver`  | static method | build the platform resolver         |
|  [07]   | `FunctionResolverFactory.GetPlatformId() -> PlatformID`  | static method | detect Mac, Linux, or Windows       |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every encode folds one ordered pass — muxer-context open, codec configure, buffer allocate, `SwsContext` RGBA→YUV420P convert, `avcodec_send_frame`/`avcodec_receive_packet` drain on `AVERROR(EAGAIN)`/`AVERROR_EOF`, `av_rescale_q` timestamp rescale to stream time-base, `av_interleaved_write_frame` mux, `av_write_trailer` finalize, then `av_*_free`/`avformat_free_context` release; the unsafe surface carries no finalizer, so every handle frees explicitly.
- `ffmpeg` static construction runs `DynamicallyLoadedBindings.Initialize()`, resolving members from `ffmpeg.RootPath` through the platform `IFunctionResolver` (`lib{name}.{version}.dylib` on macOS); `ThrowErrorIfFunctionNotFound` faults on an absent symbol.
- The in-memory mux pairs `avio_open_dyn_buf` on `AVFormatContext.pb` with `avio_close_dyn_buf`, which CONSUMES the io context and answers the written length beside a `byte*` the caller frees with `av_free`; `avformat_free_context` releases no io handle, so a fault path that skips the close leaks the whole buffer and a success path that leaves `pb` set closes it twice.
- `AVFrame.data` and `AVFrame.linesize` are the fixed `byte_ptrArray8`/`int_array8` carriers, each publishing an implicit conversion to the `byte*[]`/`int[]` managed arrays `sws_scale` takes, so a frame's planes cross to the scaler with no per-plane copy.

[STACKING]:
- `api-libmpv.md`(`.api/api-libmpv.md`): `libmpv` owns media decode and on-screen OpenGL render (the Editing MediaSurface); this owns the encode-out path (the Render capture deliverable), the two seamed encode-out versus decode-in.
- `api-avalonia-gpu-interop.md`(`.api/api-avalonia-gpu-interop.md`), `api-skiasharp.md`(`.api/api-skiasharp.md`): the compositor RGBA output these produce is the encode ingress `av_image_fill_arrays` wraps as `AVFrame` planes.
- `api-pdfsharp.md`(`.api/api-pdfsharp.md`): the MP4 motion export is the deliverable peer to the still-image and vector-PDF exports.
- `Render/capture.md` streams the compositor/path-trace RGBA frames into one encode owner that fills `AVFrame`, converts through the persistent `SwsContext`, runs the send/receive loop, and muxes MP4.

[LOCAL_ADMISSION]:
- External FFmpeg shared libraries (`libavcodec` `libavformat` `libavutil` `libswscale`) provision at the app-host distribution layer, located through `ffmpeg.RootPath`; this MIT binding ships no native binary.
- Native-build licensing follows the FFmpeg build, never this binding: a dynamically-linked LGPL build (`--enable-shared`, no `--enable-gpl`/`--enable-nonfree`) keeps the deliverable LGPL-compliant, while a `--enable-gpl` build (x264/x265) imposes copyleft on the distributed product — a distribution-policy decision.
- A missing native runtime faults through `DynamicallyLoadedBindings.ThrowErrorIfFunctionNotFound`, never a silent no-op.
