# [PY_ARTIFACTS_API_AV]

`av` (PyAV) supplies the FFmpeg-backed media surface for the artifacts MEDIA rail: `av.open(file, mode)` returns an `InputContainer` (demux/decode) or `OutputContainer` (encode/mux), `add_stream` mints typed `VideoStream`/`AudioStream` owners, and the `VideoFrame.from_ndarray` -> `stream.encode` -> `container.mux` loop drives a frame sequence into MP4/WebM/GIF — the inverse `container.demux` -> `stream.decode` -> `frame.to_ndarray` loop reads it back. The package owner composes `av.open`, `OutputContainer.add_stream`, `VideoFrame.from_ndarray`, `VideoStream.encode`, `OutputContainer.mux`, the `av.filter.Graph` filter-chain, and the `BitStreamFilterContext` remux path into the polymorphic `MediaOp` (Encode/Decode/Transcode/Remux/Filter). The wheel bundles FFmpeg (`libavcodec`/`libavformat`/`libavfilter`/`libswscale`/`libswresample` plus `libx264`/`libx265`/`libvpx`/`libSvtAv1Enc`), so it removes any `subprocess` shell-out to a system `ffmpeg` binary, and it never re-implements muxing, container packetization, the filter graph, or the codec layer FFmpeg already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `av`
- package: `av`
- import: `av`
- owner: `artifacts`
- rail: media
- version: `17.1.0`; license `BSD-3-Clause` (bindings); bundled FFmpeg `8.1.1` is `LGPL`/`GPL` by build config — GPL encoders (`libx264`/`libx265`) make the deployed wheel GPL-effective
- abi: `cp311-abi3` wheel (one stable-ABI binary covers cp311 through cp315; manifest carries `av` markerless, cp315-clean); delocate-vendored native libs, no system FFmpeg
- bundled libs: `libavutil 60`, `libavcodec 62`, `libavformat 62`, `libavdevice 62`, `libavfilter 11`, `libswscale 9`, `libswresample 6` (read at runtime via `av.library_versions` / `av.ffmpeg_version_info`)
- entry points: console script `pyav` (`av.__main__:main`); library use is import-only
- capability: FFmpeg container read/write, frame-sequence encode to MP4/WebM/GIF via codec streams (`h264`/`libx264`, `hevc`/`libx265`, `vp8`/`vp9`/`libvpx`, `av1`/`libsvtav1`, `gif`), demux/decode of any FFmpeg-supported input, NumPy ndarray <-> `VideoFrame`/`AudioFrame` interchange, DLPack zero-copy GPU/torch frame import, `swscale` pixel-format reformat, `swresample` audio resample, the `libavfilter` filter graph (scale/crop/overlay/fps/loudnorm), bitstream-filter remux (no re-encode), `AudioFifo` rebuffering, codec-context option control (CRF/preset/threads/hwaccel), and packet-level mux/demux with a wheel-bundled FFmpeg (no system binary)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: container, stream, frame, and codec roots
- rail: media

`av.open(file, mode)` returns an `InputContainer` (`demux`/`decode`/`seek`) or `OutputContainer` (`add_stream`/`mux`); `add_stream` returns a `VideoStream`/`AudioStream`/`SubtitleStream`/`DataStream` whose `codec_context` is a `VideoCodecContext`/`AudioCodecContext`. `VideoFrame`/`AudioFrame` carry the pixel/sample buffers; `Packet` is the muxed unit demux yields and encode produces. `av.filter.Graph` is the `libavfilter` node graph; `BitStreamFilterContext` rewrites packet bitstreams without re-encoding; `AudioFifo` rebuffers samples to a fixed frame size. `ContainerFormat` names the muxer, `Codec` a registered encoder/decoder. All failures raise the typed `FFmpegError` hierarchy — a `tag`/`errno`-keyed tree where lookup, decode, and protocol faults are distinct exception classes, never raw return codes.

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]        | [RAIL]                                                                  |
| :-----: | :------------------------ | :------------------- | :--------------------------------------------------------------------- |
|  [01]   | `OutputContainer`         | container            | write-mode; owns `add_stream`/`mux`/`mux_one`/`start_encoding`/`add_mux_stream` |
|  [02]   | `InputContainer`          | container            | read-mode; owns `demux`/`decode`/`seek`/`streams`/`chapters`/`metadata` |
|  [03]   | `VideoStream`             | stream               | video stream; owns `encode`/`decode`/`set_display_rotation`/`frames`   |
|  [04]   | `AudioStream`             | stream               | audio stream; owns `encode`/`decode`                                   |
|  [05]   | `SubtitleStream`          | stream               | subtitle stream (demux/mux passthrough)                                |
|  [06]   | `DataStream`              | stream               | opaque data stream (`add_data_stream`)                                 |
|  [07]   | `VideoCodecContext`       | codec context        | video codec state (`pix_fmt`/`gop_size`/`bit_rate`/`thread_type`/`hwaccel`) |
|  [08]   | `AudioCodecContext`       | codec context        | audio codec state                                                      |
|  [09]   | `CodecContext`            | codec context (base) | base encoder/decoder state; `create(codec, mode)` factory; `parse`     |
|  [10]   | `VideoFrame`              | frame                | one video frame; NumPy / DLPack / Pillow / raw-bytes interchange       |
|  [11]   | `AudioFrame`              | frame                | one audio frame; NumPy interchange                                     |
|  [12]   | `Packet`                  | packet               | muxed encoded unit; demux yields, encode/bsf produce, `mux` consumes   |
|  [13]   | `av.filter.Graph`         | filter graph         | `libavfilter` node graph; `add`/`add_buffer`/`link_nodes`/`push`/`pull` |
|  [14]   | `av.filter.Filter`        | filter descriptor    | one registered libavfilter (`scale`/`crop`/`overlay`/`fps`/`loudnorm`) |
|  [15]   | `BitStreamFilterContext`  | bitstream filter     | packet-bitstream rewrite without re-decode (`h264_mp4toannexb`, etc.)  |
|  [16]   | `AudioFifo`               | sample fifo          | rebuffer audio to a fixed `frame_size` for encoders that require it    |
|  [17]   | `AudioResampler`          | resampler            | `swresample` format/rate/layout resample owner                        |
|  [18]   | `Codec`                   | codec descriptor     | registered encoder/decoder metadata, capability flags                  |
|  [19]   | `ContainerFormat`         | format descriptor    | muxer/demuxer descriptor (mp4/webm/gif)                                |
|  [20]   | `VideoFormat`             | pixel-format value   | pixel-format descriptor (yuv420p/rgb24/rgba)                           |
|  [21]   | `AudioFormat`             | sample-format value  | sample-format descriptor (s16/fltp)                                    |
|  [22]   | `AudioLayout`             | channel-layout value | channel-layout descriptor (mono/stereo)                                |
|  [23]   | `FFmpegError`             | error (base)         | FFmpeg failure root carrying `tag`/`errno`/`strerror`                  |
|  [24]   | `EncoderNotFoundError`    | error                | requested encoder is unregistered                                      |
|  [25]   | `DecoderNotFoundError`    | error                | requested decoder is unregistered                                      |
|  [26]   | `MuxerNotFoundError`      | error                | requested muxer is unregistered                                        |
|  [27]   | `DemuxerNotFoundError`    | error                | requested demuxer is unregistered                                      |
|  [28]   | `FilterNotFoundError`     | error                | requested libavfilter is unregistered                                  |
|  [29]   | `BSFNotFoundError`        | error                | requested bitstream filter is unregistered                             |
|  [30]   | `InvalidDataError`        | error                | malformed/invalid frame or stream data                                 |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: container open and stream creation
- rail: media

`open` selects `InputContainer` or `OutputContainer` by `mode` (`"r"` default, `"w"` for write); `format` overrides the extension-inferred muxer (`"mp4"`, `"webm"`, `"gif"`); `options`/`container_options` carry private muxer/demuxer knobs; `io_open` supplies a custom Python I/O callback for segmented/protocol output; `hwaccel` requests GPU decode on read. `add_stream` is the single typed stream factory: the `codec_name` row discriminates video/audio/subtitle, `rate` is the frame rate (video) or sample rate (audio), and codec options flow through `options` or `**kwargs`. `add_mux_stream` mints a passthrough stream for remux (copy without re-encode); `add_stream_from_template` clones an input stream's parameters; `add_data_stream`/`add_attachment` carry opaque/embedded payloads.

| [INDEX] | [SURFACE]                                  | [CALL_SHAPE]                                                                                                                                                                                  | [CAPABILITY]                                       |
| :-----: | :----------------------------------------- | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :------------------------------------------------- |
|  [01]   | `av.open`                                  | `open(file, mode="w", format=None, options=None, container_options=None, stream_options=None, metadata_encoding="utf-8", metadata_errors="strict", buffer_size=32768, timeout=None, io_open=None) -> OutputContainer` | open a write container (file path or file object)  |
|  [02]   | `av.open`                                  | `open(file, mode="r", ..., timeout=None, io_open=None, hwaccel=None) -> InputContainer`                                                                                                       | open a read container (`hwaccel` for GPU decode)   |
|  [03]   | `OutputContainer.add_stream`               | `add_stream(codec_name, rate=None, options=None, **kwargs) -> VideoStream \| AudioStream \| SubtitleStream`                                                                                  | mint a typed output stream by codec name           |
|  [04]   | `OutputContainer.add_mux_stream`           | `add_mux_stream(codec_name, rate=None, **kwargs) -> Stream`                                                                                                                                  | mint a passthrough stream for remux (no re-encode) |
|  [05]   | `OutputContainer.add_stream_from_template` | `add_stream_from_template(template, opaque=None, **kwargs) -> Stream`                                                                                                                        | mint a stream copying an existing stream's params  |
|  [06]   | `OutputContainer.add_attachment`           | `add_attachment(name, mimetype, data) -> AttachmentStream`                                                                                                                                    | embed a font/cover/file attachment                 |
|  [07]   | `OutputContainer.add_data_stream`          | `add_data_stream(codec_name=None, options=None) -> DataStream`                                                                                                                                | add an opaque/timed-metadata data stream           |
|  [08]   | `OutputContainer.start_encoding`           | `start_encoding() -> None`                                                                                                                                                                    | finalize parameters and write the container header |
|  [09]   | `OutputContainer.mux`                      | `mux(packets) -> None`                                                                                                                                                                        | write one packet or a packet sequence              |
|  [10]   | `OutputContainer.mux_one`                  | `mux_one(packet) -> None`                                                                                                                                                                     | write a single packet                              |
|  [11]   | `OutputContainer.default_video_codec`      | property -> `str`                                                                                                                                                                             | muxer's default video codec (`default_audio_codec` mirror) |
|  [12]   | `OutputContainer.supported_codecs`         | property -> `set[str]`                                                                                                                                                                        | codec names the active muxer admits                |
|  [13]   | `Container.close`                          | `close() -> None`; context-manager `__enter__`/`__exit__`                                                                                                                                     | flush trailer / release; use `with av.open(...)`   |

[ENTRYPOINT_SCOPE]: container demux, decode, and seek (read side)
- rail: media

`InputContainer.demux(stream)` is the polymorphic packet iterator — it is overloaded on a `VideoStream`/`AudioStream`/`SubtitleStream`/`DataStream` (or a tuple, or a `video=`/`audio=` selector kwarg) and yields typed `Packet` objects; `decode(...)` is the same selector but yields decoded `VideoFrame`/`AudioFrame` directly. `streams` exposes the typed stream list and `.video`/`.audio`/`.best(...)` selectors. `seek(offset, *, stream=...)` positions the demuxer by timestamp; `chapters`/`metadata`/`duration` read container structure.

| [INDEX] | [SURFACE]                  | [CALL_SHAPE]                                                                                          | [CAPABILITY]                                          |
| :-----: | :------------------------- | :--------------------------------------------------------------------------------------------------- | :---------------------------------------------------- |
|  [01]   | `InputContainer.demux`     | `demux(stream=None, /, *, video=..., audio=...) -> Iterator[Packet]`                                 | polymorphic packet iterator over selected stream(s)   |
|  [02]   | `InputContainer.decode`    | `decode(stream=None, /, *, video=int, audio=int) -> Iterator[VideoFrame \| AudioFrame]`              | decode selected stream(s) straight to frames          |
|  [03]   | `InputContainer.seek`      | `seek(offset, *, backward=True, any_frame=False, stream=None) -> None`                               | seek the demuxer to a timestamp (in stream time_base) |
|  [04]   | `InputContainer.streams`   | property -> `StreamContainer` (`.video`/`.audio`/`.get(...)`/`.best("video")`)                       | typed stream selectors                                |
|  [05]   | `Stream.decode`            | `decode(packet=None) -> list[VideoFrame \| AudioFrame]`                                              | decode a demuxed packet; `None` flushes the decoder   |
|  [06]   | `InputContainer.chapters`  | `chapters() -> list[Chapter]`; `.metadata` / `.duration` / `.bit_rate`                               | container chapter/metadata/duration structure         |

[ENTRYPOINT_SCOPE]: frame-sequence encode
- rail: media

`VideoFrame.from_ndarray` lifts a NumPy buffer to a frame; `pts`/`time_base`/`format`/`pix_fmt` are assigned before `encode`. `encode(frame)` returns a `Packet` list; `encode(None)` flushes the encoder. The encode loop muxes each returned packet, then flushes with a `None` frame at end-of-stream. `from_dlpack` imports a frame zero-copy from a DLPack producer (torch/cupy/jax) including from CUDA device memory via `cuda_context`, the GPU-native ingest that avoids a host round-trip; `from_bytes` lifts a raw RGBA buffer; `to_rgb`/`to_ndarray`/`to_image` are the egress mirrors; `make_writable` forces a copy before mutation of a shared buffer.

| [INDEX] | [SURFACE]                      | [CALL_SHAPE]                                                                                                                              | [CAPABILITY]                               |
| :-----: | :----------------------------- | :--------------------------------------------------------------------------------------------------------------------------------------- | :----------------------------------------- |
|  [01]   | `VideoFrame.from_ndarray`      | `from_ndarray(array, format="rgb24", channel_last=False) -> VideoFrame`                                                                  | lift a NumPy array to a video frame        |
|  [02]   | `VideoFrame.from_numpy_buffer` | `from_numpy_buffer(array, format="rgb24", width=0) -> VideoFrame`                                                                        | zero-copy wrap a contiguous NumPy buffer   |
|  [03]   | `VideoFrame.from_dlpack`       | `from_dlpack(planes, format="nv12", width=0, height=0, stream=None, device_id=None, primary_ctx=True, cuda_context=None) -> VideoFrame`  | zero-copy import from a DLPack/CUDA producer |
|  [04]   | `VideoFrame.from_bytes`        | `from_bytes(data, width, height, format="rgba", flip_horizontal=False, flip_vertical=False) -> VideoFrame`                              | lift a raw pixel byte buffer               |
|  [05]   | `VideoFrame.from_image`        | `from_image(img) -> VideoFrame`                                                                                                          | lift a Pillow image to a video frame       |
|  [06]   | `VideoFrame.__init__`          | `VideoFrame(width=0, height=0, format="yuv420p")`                                                                                        | allocate an empty video frame              |
|  [07]   | `VideoFrame.reformat`          | `reformat(width=None, height=None, format=None, src_colorspace=None, dst_colorspace=None, interpolation=None, *_color_range/_trc/_primaries, threads=None) -> VideoFrame` | swscale resize / pixel-format / color convert |
|  [08]   | `VideoFrame.to_ndarray`        | `to_ndarray(channel_last=False, **kwargs) -> ndarray`                                                                                    | extract a frame to a NumPy array           |
|  [09]   | `VideoFrame.to_rgb`            | `to_rgb(**kwargs) -> VideoFrame`                                                                                                         | convert to an rgb24 frame                  |
|  [10]   | `VideoFrame.make_writable`     | `make_writable() -> None`                                                                                                                | copy a shared buffer before mutation       |
|  [11]   | `VideoStream.encode`           | `encode(frame=None) -> list[Packet]`                                                                                                     | encode a frame; `None` flushes the encoder |
|  [12]   | `Packet` (`dts`/`pts`/`duration`/`is_keyframe`/`stream`) | `mux_one(packet)` consumes one; `packet.decode() -> list[VideoFrame \| AudioFrame]`                                          | per-packet mux granularity for backpressure; a demuxed packet decodes in place |
|  [13]   | `AudioFrame.from_ndarray`      | `from_ndarray(array, format="s16", layout="stereo") -> AudioFrame`                                                                       | lift a NumPy array to an audio frame       |
|  [14]   | `AudioStream.encode`           | `encode(frame=None) -> list[Packet]`                                                                                                     | encode an audio frame; `None` flushes      |

[ENTRYPOINT_SCOPE]: filter graph, bitstream filter, and resample
- rail: media

`av.filter.Graph` is the single `libavfilter` owner: `add_buffer`/`add_abuffer` create the source node from a stream template, `add(name, args, **kwargs)` adds a named filter (`scale`/`crop`/`overlay`/`fps`/`format`/`loudnorm`), `link_nodes(*nodes)` chains them, and `push(frame)`/`pull()` drive frames through — never a hand-rolled scale/crop/overlay loop. `BitStreamFilterContext(description, in_stream, out_stream)` rewrites a packet bitstream (`h264_mp4toannexb`, `hevc_mp4toannexb`, `extract_extradata`) for a remux without decode/re-encode. `AudioResampler.resample` converts format/rate/layout; `AudioFifo` rebuffers samples to a fixed `frame_size` for encoders (AAC) that require exact frame sizes.

| [INDEX] | [SURFACE]                       | [CALL_SHAPE]                                                                                                            | [CAPABILITY]                                            |
| :-----: | :------------------------------ | :--------------------------------------------------------------------------------------------------------------------- | :------------------------------------------------------ |
|  [01]   | `Graph.add_buffer`              | `add_buffer(template=None, width=None, height=None, format=None, name=None, time_base=None) -> FilterContext`         | create a video source node                              |
|  [02]   | `Graph.add_abuffer`             | `add_abuffer(template=None, sample_rate=None, format=None, layout=None, channels=None, name=None, time_base=None) -> FilterContext` | create an audio source node                  |
|  [03]   | `Graph.add`                     | `add(filter, args=None, **kwargs) -> FilterContext`                                                                   | add a named libavfilter node                            |
|  [04]   | `Graph.link_nodes`              | `link_nodes(*nodes) -> Graph`                                                                                          | link a sequence of filter contexts                      |
|  [05]   | `Graph.push` / `Graph.pull`     | `push(frame_or_None) -> None`; `pull() -> VideoFrame \| AudioFrame`                                                   | drive frames through the configured graph               |
|  [06]   | `Graph.configure`               | `configure(auto_buffer=True, force=False) -> None`                                                                    | validate and configure the graph before pull            |
|  [07]   | `BitStreamFilterContext`        | `BitStreamFilterContext(filter_description, in_stream=None, out_stream=None)`                                          | bitstream rewrite filter (annexb, extradata)            |
|  [08]   | `BitStreamFilterContext.filter` | `filter(packet=None) -> list[Packet]`; `flush() -> None`                                                              | rewrite a packet's bitstream; `None` flushes            |
|  [09]   | `AudioResampler`                | `AudioResampler(format=None, layout=None, rate=None, frame_size=None)`                                                | format/rate/layout resample owner                       |
|  [10]   | `AudioResampler.resample`       | `resample(frame_or_None) -> list[AudioFrame]`                                                                         | resample a frame; `None` flushes the resampler          |
|  [11]   | `AudioFifo.write` / `.read`     | `write(frame) -> None`; `read(samples=-1, partial=False) -> AudioFrame \| None`                                       | rebuffer audio to a fixed encoder frame size            |
|  [12]   | `CodecContext.create`           | `create(codec, mode=None, hwaccel=None) -> VideoCodecContext \| AudioCodecContext \| CodecContext`                    | mint a standalone codec context (raw encode/decode)     |
|  [13]   | `CodecContext.parse`            | `parse(raw_input=None) -> list[Packet]`                                                                               | split a raw elementary stream into packets              |

[ENTRYPOINT_SCOPE]: stream and codec-context configuration
- rail: media

The stream exposes its codec parameters directly and through `codec_context`. `width`/`height`/`pix_fmt`/`bit_rate`/`gop_size`/`framerate` are set after `add_stream` and before the first `encode`; `options` carries codec-private knobs (CRF, preset, tune); `thread_count`/`thread_type` set parallel encode; `hwaccel`/`is_hwaccel` route to GPU; `flags`/`flags2`/`max_b_frames`/`qmin`/`qmax` tune rate control. `time_base` and per-frame `pts` set the presentation timeline; frames carry color and rotation metadata.

| [INDEX] | [SURFACE]                       | [CALL_SHAPE]                            | [CAPABILITY]                                       |
| :-----: | :------------------------------ | :-------------------------------------- | :------------------------------------------------- |
|  [01]   | `VideoStream.width`             | attribute -> `int`                      | coded frame width                                  |
|  [02]   | `VideoStream.height`            | attribute -> `int`                      | coded frame height                                 |
|  [03]   | `VideoStream.pix_fmt`           | attribute -> `str \| None`              | output pixel format (e.g. `yuv420p`)               |
|  [04]   | `VideoStream.bit_rate`          | attribute -> `int \| None`              | target bit rate                                    |
|  [05]   | `VideoStream.gop_size`          | attribute -> `int`                      | keyframe (GOP) interval                            |
|  [06]   | `VideoStream.average_rate` / `base_rate` / `guessed_rate` | attribute -> `Fraction \| None` | demuxed/encode frame rate reads (the live `rate` set on `add_stream`; the encode rate target is `codec_context.framerate`) |
|  [07]   | `VideoStream.codec_context`     | attribute -> `VideoCodecContext`        | underlying codec state                             |
|  [08]   | `VideoStream.set_display_rotation` | `set_display_rotation(angle) -> None` | write a display-matrix rotation side-datum         |
|  [09]   | `CodecContext.options`          | attribute -> `dict[str, str]`           | codec-private option map (crf/preset/tune)         |
|  [10]   | `CodecContext.thread_count`     | attribute -> `int`                      | encoder thread count                               |
|  [11]   | `CodecContext.thread_type`      | attribute -> `ThreadType` (`FRAME`/`SLICE`/`AUTO`) | parallel-encode strategy                |
|  [12]   | `CodecContext.max_b_frames`     | attribute -> `int`                      | max consecutive B-frames                           |
|  [13]   | `CodecContext.flags` / `.flags2`| attribute -> flag set                   | codec flag bitsets (`GLOBAL_HEADER`, low-delay)    |
|  [14]   | `CodecContext.flush_buffers`    | `flush_buffers() -> None`               | reset decoder/encoder state at a seek boundary     |
|  [15]   | `CodecContext.hwaccel`/`is_hwaccel` | attribute -> `HWAccel`/`bool`       | hardware acceleration context                      |
|  [16]   | `Frame.pts`                     | attribute -> `int \| None`              | presentation timestamp (in `time_base` units)      |
|  [17]   | `Frame.time_base`               | attribute -> `Fraction \| None`         | timestamp unit for the frame                       |
|  [18]   | `VideoFrame.colorspace`/`color_range`/`color_primaries`/`color_trc` | attribute | per-frame color metadata (BT.709 etc.) |
|  [19]   | `VideoFrame.pict_type`/`key_frame`/`rotation` | attribute             | picture type (I/P/B), keyframe flag, display rotation |
|  [20]   | `AudioStream.sample_rate`       | attribute -> `int`                      | audio sample rate                                  |
|  [21]   | `AudioStream.layout`            | attribute -> `AudioLayout`              | channel layout                                     |

## [04]-[IMPLEMENTATION_LAW]

[MEDIA_ENCODE]:
- import: `import av` at boundary scope only; module-level import is banned by the manifest import policy.
- modality axis: one polymorphic `MediaOp` discriminates Encode / Decode / Transcode / Remux / Filter; the read side (`demux`/`decode`/`seek`) and the write side (`add_stream`/`encode`/`mux`) are arms of the same owner, never two parallel reader/writer packages — a transcode is `decode` -> optional `Graph` -> `encode` -> `mux` in one pipeline.
- container axis: one `av.open` owns container open; `mode` selects `InputContainer` vs `OutputContainer` and `format` selects the muxer (`mp4`/`webm`/`gif`) as a call row, never a per-format container type; `open` is always a context manager so the trailer is written and the demuxer/IO is released; `io_open` supplies a Python I/O callback for protocol/segment sinks.
- stream axis: `add_stream(codec_name, rate)` is the single typed stream factory; video vs audio is the `codec_name` row (`h264`/`libx264`/`vp9`/`libvpx-vp9`/`libsvtav1`/`gif` vs `aac`/`libopus`), never a parallel `add_video_stream`/`add_audio_stream` pair; `add_mux_stream`/`add_stream_from_template` are the remux/clone rows; codec-private knobs are `options`/`**kwargs` rows, not bespoke builder types.
- frame axis: `VideoFrame.from_ndarray` is the host ndarray ingest row and `from_dlpack` the zero-copy GPU/torch row (CUDA-context aware, avoiding a device->host copy); `pts`/`time_base`/`pix_fmt` are assigned on the frame, and `reformat` is the swscale resize/colorspace-convert row, never a hand-rolled pixel loop.
- filter axis: `av.filter.Graph` is the single libavfilter owner; scale/crop/overlay/fps/format/loudnorm are `add(...)` node rows wired with `link_nodes` and driven with `push`/`pull`, never a NumPy resampling/compositing reimplementation; the graph runs between `decode` and `encode` in a transcode.
- remux axis: `BitStreamFilterContext` rewrites packet bitstreams (annexb, extradata) for a `demux` -> `bsf.filter` -> `mux` copy that never re-decodes; container/codec change without quality loss is a remux, never a needless re-encode.
- encode axis: `stream.encode(frame)` -> `container.mux(packets)` is the one encode-then-mux row; the flush is `stream.encode(None)` muxed at end-of-stream, never a separate teardown surface; backpressure-sensitive consumers mux `Packet`s one at a time with `container.mux_one(packet)` (each carries `dts`/`pts`/`duration`/`is_keyframe`), never a fictional lazy-encode iterator; `AudioFifo` rebuffers to a fixed `frame_size` for encoders (AAC) that require it.
- evidence: each op captures container format, codec name, `pix_fmt`, frame count, resolution, frame rate, target bit rate, GOP size, filter-graph node chain, bundled `libavcodec`/`ffmpeg_version_info`, and input/output byte length as a media receipt.
- boundary: `av` owns FFmpeg container muxing/demuxing, codec encode/decode, the filter graph, and bitstream remux through the wheel-bundled FFmpeg (`8.1.1`) with no system binary; NumPy is the host frame-buffer edge and DLPack the device edge; Pillow routes through `from_image`/`to_image` only when an image owner already holds a `PIL.Image`; live playback and UI stay outside this package; failures surface as the typed `FFmpegError` hierarchy (`EncoderNotFoundError`/`DecoderNotFoundError`/`MuxerNotFoundError`/`DemuxerNotFoundError`/`FilterNotFoundError`/`BSFNotFoundError`/`InvalidDataError`), never as raw return codes.

[STACK_INTEGRATION]:
- universal `numpy` tier (`libs/python/.api/numpy.md`): the frame seam is `VideoFrame.from_ndarray(arr, format)` in / `frame.to_ndarray()` out — a canonical `numpy` `uint8`/`float32` buffer (RGB24/RGBA/GRAY) is the one host pixel surface, so a `scikit-image`/`matplotlib`/`pillow`-produced frame array encodes without a bespoke pixel struct, and `from_numpy_buffer` is the zero-copy variant for a contiguous C-order buffer. The device edge is `from_dlpack` (a `torch`/`cupy`/`jax` CUDA tensor ingests with no host round-trip); the host array and the DLPack capsule are the only two ingest shapes.
- universal `anyio` tier (`libs/python/.api/anyio.md`): the synchronous `demux`/`decode`/`encode`/`mux` loop is CPU-bound C, so the boundary owner drives one container per `anyio.to_thread.run_sync` worker (or a `CapacityLimiter`-bounded fan over many inputs); a transcode farm is N bounded worker tasks each holding one `InputContainer`/`OutputContainer`, never an unbounded thread pool. The pull/push of `Packet`s stays inside the worker; only the finished bytes cross back to the async caller.
- universal `expression` tier (`libs/python/.api/expression.md`): the `FFmpegError` subtree maps at the boundary to a `Result[MediaReceipt, MediaError]` — `EncoderNotFoundError`/`MuxerNotFoundError` are a typed `Error` arm (the codec/muxer the spec named is not registered), `InvalidDataError` is the malformed-input arm, and a successful `mux` trailer yields the `Ok` receipt; the `try/except FFmpegError` lives only in the boundary adapter, never in the domain pipeline.
- universal `structlog`/`opentelemetry` tier: the per-op evidence (container format, codec, `pix_fmt`, frame count, `ffmpeg_version_info`, byte length) is the structured event/span payload; `av.library_versions` is read once at boundary init so the bundled `libavcodec`/`libavformat` majors ride the receipt as deployment facts.
- sibling artifacts libs: `pillow` (`Image` <-> `VideoFrame.from_image`/`to_image`) is the still-image edge for a single-frame poster or a GIF frame source; the document owner (`pymupdf`/`reportlab`) embeds a rendered MP4/GIF byte payload `av` produced; a `vl_convert`/`matplotlib` figure sequence rasterized to `numpy` frames is the chart-animation ingest — each is a `from_ndarray`/`from_image` row into the same `MediaOp`, never a parallel media writer.

[RAIL_LAW]:
- Package: `av`
- Owns: FFmpeg container open/mux/demux, frame-sequence encode and decode to/from MP4/WebM/GIF via codec streams, the libavfilter filter graph, bitstream-filter remux, NumPy/DLPack frame interchange, pixel-format reformat, and audio resample/rebuffer with a wheel-bundled FFmpeg
- Accept: frame-sequence encode, decode, transcode, remux, and filter-graph pipelines feeding the media and document owners
- Reject: wrapper-renames of `open`/`add_stream`/`encode`/`mux`/`demux`/`decode`; a `subprocess` shell-out to a system `ffmpeg` binary the wheel already bundles; a hand-rolled muxer, packetizer, pixel-format converter, scaler, or compositor FFmpeg/libavfilter owns; a needless full decode+re-encode where a `BitStreamFilterContext` remux preserves quality; a parallel container or stream type per output format; a parallel reader package where the same owner demuxes; identity or timeline minting the runtime owns
