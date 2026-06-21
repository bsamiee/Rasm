# [PY_ARTIFACTS_API_AV]

`av` (PyAV) supplies the FFmpeg-backed encode surface for the artifacts MEDIA rail: a single `av.open(file, mode="w")` factory returns an `OutputContainer` whose `add_stream` mints typed `VideoStream`/`AudioStream` owners, and a `VideoFrame.from_ndarray` -> `stream.encode` -> `container.mux` loop drives a frame sequence into MP4/WebM/GIF. The package owner composes `av.open`, `OutputContainer.add_stream`, `VideoFrame.from_ndarray`, `VideoStream.encode`, and `OutputContainer.mux` into the `MediaOp.Encode` path; the wheel bundles ffmpeg (`libavcodec`/`libavformat`/`libswscale` plus `libx264`/`libx265`/`libvpx`/`libSvtAv1Enc`), so it removes any `subprocess` shell-out to a system `ffmpeg` binary, and it never re-implements muxing, container packetization, or the codec layer FFmpeg already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `av`
- package: `av`
- import: `av`
- owner: `artifacts`
- rail: media
- installed: `17.1.0` reflected via `assay api` on cp315
- entry points: console script `pyav` (CLI); library use is import-only
- capability: FFmpeg container read/write, frame-sequence encode to MP4/WebM/GIF via codec streams (`h264`/`libx264`, `hevc`/`libx265`, `vp8`/`vp9`/`libvpx`, `av1`/`libsvtav1`, `gif`), NumPy ndarray <-> `VideoFrame`/`AudioFrame` interchange, pixel-format reformatting through `swscale`, audio resampling through `swresample`, codec-context option control, and packet-level mux/demux with a wheel-bundled ffmpeg (no system binary)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: container, stream, frame, and codec roots
- rail: media

`av.open(file, mode="w")` returns an `OutputContainer`; `add_stream` returns a `VideoStream` or `AudioStream` whose `codec_context` is a `VideoCodecContext` or `AudioCodecContext`. `VideoFrame`/`AudioFrame` carry the pixel/sample buffers fed to `encode`; `Packet` is the muxed unit. `ContainerFormat` names the muxer; `Codec` describes a registered encoder/decoder. Lookup and encode failures raise the typed `FFmpegError` hierarchy (`EncoderNotFoundError`, `MuxerNotFoundError`, `InvalidDataError`).

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]        | [RAIL]                                                         |
| :-----: | :--------------------- | :------------------- | :------------------------------------------------------------- |
|  [01]   | `OutputContainer`      | container            | write-mode container; owns `add_stream`/`mux`/`start_encoding` |
|  [02]   | `InputContainer`       | container            | read-mode container returned by `open(mode="r")`               |
|  [03]   | `VideoStream`          | stream               | video output stream; owns `encode`/`encode_lazy`               |
|  [04]   | `AudioStream`          | stream               | audio output stream; owns `encode`                             |
|  [05]   | `VideoCodecContext`    | codec context        | video codec state (`pix_fmt`/`gop_size`/`bit_rate`/`encode`)   |
|  [06]   | `AudioCodecContext`    | codec context        | audio codec state                                              |
|  [07]   | `CodecContext`         | codec context (base) | base encoder/decoder state; `create` factory                   |
|  [08]   | `VideoFrame`           | frame                | one video frame; NumPy/image interchange                       |
|  [09]   | `AudioFrame`           | frame                | one audio frame; NumPy interchange                             |
|  [10]   | `Codec`                | codec descriptor     | registered encoder/decoder metadata                            |
|  [11]   | `ContainerFormat`      | format descriptor    | muxer/demuxer descriptor (mp4/webm/gif)                        |
|  [12]   | `VideoFormat`          | pixel-format value   | pixel-format descriptor (yuv420p/rgb24/rgba)                   |
|  [13]   | `AudioFormat`          | sample-format value  | sample-format descriptor (s16/fltp)                            |
|  [14]   | `AudioLayout`          | channel-layout value | channel-layout descriptor (mono/stereo)                        |
|  [15]   | `AudioResampler`       | resampler            | format/rate/layout resample owner                              |
|  [16]   | `Packet`               | packet               | muxed encoded unit fed to `mux`                                |
|  [17]   | `FFmpegError`          | error (base)         | FFmpeg failure root                                            |
|  [18]   | `EncoderNotFoundError` | error                | requested encoder is unregistered                              |
|  [19]   | `MuxerNotFoundError`   | error                | requested muxer is unregistered                                |
|  [20]   | `InvalidDataError`     | error                | malformed/invalid frame or stream data                         |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: container open and stream creation
- rail: media

`open` selects `InputContainer` or `OutputContainer` by `mode`; `format` overrides the extension-inferred muxer (`"mp4"`, `"webm"`, `"gif"`). `add_stream` is the single typed stream factory: the `codec_name` row discriminates video vs audio, `rate` is the frame rate (video) or sample rate (audio), and codec options flow through `options` or `**kwargs`.

| [INDEX] | [SURFACE]                                  | [CALL_SHAPE]                                                                                                                                       | [CAPABILITY]                                       |
| :-----: | :----------------------------------------- | :------------------------------------------------------------------------------------------------------------------------------------------------- | :------------------------------------------------- |
|  [01]   | `av.open`                                  | `open(file, mode="w", format=None, options=None, container_options=None, stream_options=None, buffer_size=32768, timeout=None) -> OutputContainer` | open a write container (file path or file object)  |
|  [02]   | `av.open`                                  | `open(file, mode="r", format=None, options=None, buffer_size=32768, timeout=None, hwaccel=None) -> InputContainer`                                 | open a read container                              |
|  [03]   | `OutputContainer.add_stream`               | `add_stream(codec_name, rate=None, options=None, **kwargs) -> VideoStream \| AudioStream \| SubtitleStream`                                        | mint a typed output stream by codec name           |
|  [04]   | `OutputContainer.add_stream_from_template` | `add_stream_from_template(template, opaque=None, **kwargs) -> Stream`                                                                              | mint a stream copying an existing stream's params  |
|  [05]   | `OutputContainer.start_encoding`           | `start_encoding() -> None`                                                                                                                         | finalize parameters and write the container header |
|  [06]   | `OutputContainer.mux`                      | `mux(packets) -> None`                                                                                                                             | write one packet or a packet sequence              |
|  [07]   | `OutputContainer.mux_one`                  | `mux_one(packet) -> None`                                                                                                                          | write a single packet                              |
|  [08]   | `OutputContainer.close`                    | `close() -> None`                                                                                                                                  | flush trailer and release the container            |
|  [09]   | `OutputContainer.supported_codecs`         | property -> `set[str]`                                                                                                                             | codec names the active muxer admits                |

[ENTRYPOINT_SCOPE]: frame-sequence encode
- rail: media

`VideoFrame.from_ndarray` lifts a NumPy buffer to a frame; `pts`/`time_base`/`format`/`pix_fmt` are assigned before `encode`. `encode(frame)` returns a `Packet` list; `encode(None)` flushes the encoder. The encode loop muxes each returned packet, then flushes with a `None` frame at end-of-stream.

| [INDEX] | [SURFACE]                      | [CALL_SHAPE]                                                                      | [CAPABILITY]                               |
| :-----: | :----------------------------- | :-------------------------------------------------------------------------------- | :----------------------------------------- |
|  [01]   | `VideoFrame.from_ndarray`      | `from_ndarray(array, format="rgb24", channel_last=False) -> VideoFrame`           | lift a NumPy array to a video frame        |
|  [02]   | `VideoFrame.from_numpy_buffer` | `from_numpy_buffer(array, format="rgb24", width=0) -> VideoFrame`                 | zero-copy wrap a NumPy buffer              |
|  [03]   | `VideoFrame.from_image`        | `from_image(img) -> VideoFrame`                                                   | lift a Pillow image to a video frame       |
|  [04]   | `VideoFrame.__init__`          | `VideoFrame(width=0, height=0, format="yuv420p")`                                 | allocate an empty video frame              |
|  [05]   | `VideoFrame.reformat`          | `reformat(width=None, height=None, format=None, ..., threads=None) -> VideoFrame` | swscale resize / pixel-format convert      |
|  [06]   | `VideoFrame.to_ndarray`        | `to_ndarray(channel_last=False, **kwargs) -> ndarray`                             | extract a frame to a NumPy array           |
|  [07]   | `VideoStream.encode`           | `encode(frame=None) -> list[Packet]`                                              | encode a frame; `None` flushes the encoder |
|  [08]   | `VideoStream.encode_lazy`      | `encode_lazy(frame=None) -> Iterator[Packet]`                                     | encode a frame lazily as a packet iterator |
|  [09]   | `AudioFrame.from_ndarray`      | `from_ndarray(array, format="s16", layout="stereo") -> AudioFrame`                | lift a NumPy array to an audio frame       |
|  [10]   | `AudioStream.encode`           | `encode(frame=None) -> list[Packet]`                                              | encode an audio frame; `None` flushes      |

[ENTRYPOINT_SCOPE]: stream and codec-context configuration
- rail: media

The stream exposes its codec parameters directly and through `codec_context`. `width`/`height`/`pix_fmt`/`bit_rate`/`gop_size`/`framerate` are set after `add_stream` and before the first `encode`; `options` carries codec-private knobs (CRF, preset). `time_base` and per-frame `pts` set the presentation timeline.

| [INDEX] | [SURFACE]                   | [CALL_SHAPE]                     | [CAPABILITY]                                  |
| :-----: | :-------------------------- | :------------------------------- | :-------------------------------------------- |
|  [01]   | `VideoStream.width`         | attribute -> `int`               | coded frame width                             |
|  [02]   | `VideoStream.height`        | attribute -> `int`               | coded frame height                            |
|  [03]   | `VideoStream.pix_fmt`       | attribute -> `str \| None`       | output pixel format (e.g. `yuv420p`)          |
|  [04]   | `VideoStream.bit_rate`      | attribute -> `int \| None`       | target bit rate                               |
|  [05]   | `VideoStream.gop_size`      | attribute -> `int`               | keyframe (GOP) interval                       |
|  [06]   | `VideoStream.framerate`     | attribute -> `Fraction`          | encode frame rate                             |
|  [07]   | `VideoStream.codec_context` | attribute -> `VideoCodecContext` | underlying codec state                        |
|  [08]   | `CodecContext.options`      | attribute -> `dict[str, str]`    | codec-private option map (crf/preset)         |
|  [09]   | `CodecContext.thread_count` | attribute -> `int`               | encoder thread count                          |
|  [10]   | `Frame.pts`                 | attribute -> `int \| None`       | presentation timestamp (in `time_base` units) |
|  [11]   | `Frame.time_base`           | attribute -> `Fraction \| None`  | timestamp unit for the frame                  |
|  [12]   | `AudioStream.sample_rate`   | attribute -> `int`               | audio sample rate                             |
|  [13]   | `AudioStream.layout`        | attribute -> `AudioLayout`       | channel layout                                |

## [04]-[IMPLEMENTATION_LAW]

[MEDIA_ENCODE]:
- import: `import av` at boundary scope only; module-level import is banned by the manifest import policy.
- container axis: one `av.open` owns container open; `mode` selects `InputContainer` vs `OutputContainer` and `format` selects the muxer (`mp4`/`webm`/`gif`) as a call row, never a per-format container type; `open` is used as a context manager so `close` writes the trailer.
- stream axis: `add_stream(codec_name, rate)` is the single typed stream factory; video vs audio is the `codec_name` row (`h264`/`libx264`/`vp9`/`libvpx-vp9`/`libsvtav1`/`gif` vs `aac`/`libopus`), never a parallel `add_video_stream`/`add_audio_stream` pair; codec-private knobs are `options`/`**kwargs` rows, not bespoke builder types.
- frame axis: `VideoFrame.from_ndarray` is the single ndarray ingest row; `pts`/`time_base`/`pix_fmt` are assigned on the frame, and `reformat` is the swscale resize/convert row, never a hand-rolled pixel loop.
- encode axis: `stream.encode(frame)` -> `container.mux(packets)` is the one encode-then-mux row; the flush is `stream.encode(None)` muxed at end-of-stream, never a separate teardown surface; `encode_lazy` is the streaming-iterator row for backpressure-sensitive consumers.
- evidence: each encode captures container format, codec name, `pix_fmt`, frame count, resolution, frame rate, target bit rate, and output byte length as a media receipt.
- boundary: `av` owns FFmpeg container muxing and codec encode through the wheel-bundled ffmpeg with no system binary; NumPy is the frame-buffer interchange edge; Pillow routes through `from_image`/`to_image` only when an image owner already holds a `PIL.Image`; live playback and UI stay outside this package; failures surface as the typed `FFmpegError` hierarchy, never as raw return codes.

[RAIL_LAW]:
- Package: `av`
- Owns: FFmpeg container open/mux, frame-sequence encode to MP4/WebM/GIF via codec streams, NumPy <-> frame interchange, pixel-format reformat, and audio resample with a wheel-bundled ffmpeg
- Accept: frame-sequence encode and container muxing feeding the media and document owners
- Reject: wrapper-renames of `open`/`add_stream`/`encode`/`mux`; a `subprocess` shell-out to a system `ffmpeg` binary the wheel already bundles; a hand-rolled muxer, packetizer, or pixel-format converter FFmpeg owns; a parallel container or stream type per output format; identity or timeline minting the runtime owns
