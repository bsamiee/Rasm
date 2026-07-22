# [PY_ARTIFACTS_API_AV]

`av` (PyAV) supplies the FFmpeg-backed media surface for the artifacts MEDIA rail: `av.open(file, mode)` returns an `InputContainer` (demux/decode) or `OutputContainer` (encode/mux), `add_stream` mints typed `VideoStream`/`AudioStream` owners, and the `VideoFrame.from_ndarray` -> `stream.encode` -> `container.mux` loop drives a frame sequence into MP4/WebM/GIF — the inverse `container.demux` -> `stream.decode` -> `frame.to_ndarray` loop reads it back. The package owner composes `av.open`, `OutputContainer.add_stream`, `VideoFrame.from_ndarray`, `VideoStream.encode`, `OutputContainer.mux`, the `av.filter.Graph` filter-chain, and the `BitStreamFilterContext` remux path into the polymorphic `MediaOp` (Encode/Decode/Transcode/Remux/Filter). The package bundles FFmpeg (`libavcodec`/`libavformat`/`libavfilter`/`libswscale`/`libswresample` plus `libx264`/`libx265`/`libvpx`/`libSvtAv1Enc`), so it removes any `subprocess` shell-out to a system `ffmpeg` binary, and it never re-implements muxing, container packetization, the filter graph, or the codec layer FFmpeg already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `av`
- package: `av`
- import: `av`
- owner: `artifacts`
- rail: media
- version: `17.1.0`
- bundled libs: `libavutil 60`, `libavcodec 62`, `libavformat 62`, `libavdevice 62`, `libavfilter 11`, `libswscale 9`, `libswresample 6` (read at runtime via `av.library_versions` / `av.ffmpeg_version_info`)
- entry points: console script `pyav` (`av.__main__:main`); library use is import-only

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: container, stream, frame, and codec roots
- rail: media

`av.open(file, mode)` returns an `InputContainer` (`demux`/`decode`/`seek`) or `OutputContainer` (`add_stream`/`mux`); `add_stream` returns a `VideoStream`/`AudioStream`/`SubtitleStream`/`DataStream` whose `codec_context` is a `VideoCodecContext`/`AudioCodecContext`. `VideoFrame`/`AudioFrame` carry the pixel/sample buffers; `Packet` is the muxed unit demux yields and encode produces. `av.filter.Graph` is the `libavfilter` node graph; `BitStreamFilterContext` rewrites packet bitstreams without re-encoding; `AudioFifo` rebuffers samples to a fixed frame size. `ContainerFormat` names the muxer, `Codec` a registered encoder/decoder. All failures raise the typed `FFmpegError` hierarchy — a `tag`/`errno`-keyed tree where lookup, decode, and protocol faults are distinct exception classes, never raw return codes.

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]        | [RAIL]                                                                          |
| :-----: | :----------------------- | :------------------- | :------------------------------------------------------------------------------ |
|  [01]   | `OutputContainer`        | container            | write-mode; owns `add_stream`/`mux`/`mux_one`/`start_encoding`/`add_mux_stream` |
|  [02]   | `InputContainer`         | container            | read-mode; owns `demux`/`decode`/`seek`/`streams`/`chapters`/`metadata`         |
|  [03]   | `VideoStream`            | stream               | video stream; owns `encode`/`decode`/`set_display_rotation`/`frames`            |
|  [04]   | `AudioStream`            | stream               | audio stream; owns `encode`/`decode`                                            |
|  [05]   | `SubtitleStream`         | stream               | subtitle stream (demux/mux passthrough)                                         |
|  [06]   | `DataStream`             | stream               | opaque data stream (`add_data_stream`)                                          |
|  [07]   | `VideoCodecContext`      | codec context        | video codec state (`pix_fmt`/`gop_size`/`bit_rate`/`thread_type`/`hwaccel`)     |
|  [08]   | `AudioCodecContext`      | codec context        | audio codec state                                                               |
|  [09]   | `CodecContext`           | codec context (base) | base encoder/decoder state; `create(codec, mode)` factory; `parse`              |
|  [10]   | `VideoFrame`             | frame                | one video frame; NumPy / DLPack / Pillow / raw-bytes interchange                |
|  [11]   | `AudioFrame`             | frame                | one audio frame; NumPy interchange                                              |
|  [12]   | `Packet`                 | packet               | muxed encoded unit; demux yields, encode/bsf produce, `mux` consumes            |
|  [13]   | `av.filter.Graph`        | filter graph         | `libavfilter` node graph; `add`/`add_buffer`/`link_nodes`/`push`/`pull`         |
|  [14]   | `av.filter.Filter`       | filter descriptor    | one registered libavfilter (`scale`/`crop`/`overlay`/`fps`/`loudnorm`)          |
|  [15]   | `BitStreamFilterContext` | bitstream filter     | packet-bitstream rewrite without re-decode (`h264_mp4toannexb`, etc.)           |
|  [16]   | `AudioFifo`              | sample fifo          | rebuffer audio to a fixed `frame_size` for encoders that require it             |
|  [17]   | `AudioResampler`         | resampler            | `swresample` format/rate/layout resample owner                                  |
|  [18]   | `Codec`                  | codec descriptor     | registered encoder/decoder metadata, capability flags                           |
|  [19]   | `ContainerFormat`        | format descriptor    | muxer/demuxer descriptor (mp4/webm/gif)                                         |
|  [20]   | `VideoFormat`            | pixel-format value   | pixel-format descriptor (yuv420p/rgb24/rgba)                                    |
|  [21]   | `AudioFormat`            | sample-format value  | sample-format descriptor (s16/fltp)                                             |
|  [22]   | `AudioLayout`            | channel-layout value | channel-layout descriptor (mono/stereo)                                         |
|  [23]   | `FFmpegError`            | error (base)         | FFmpeg failure root carrying `tag`/`errno`/`strerror`                           |
|  [24]   | `EncoderNotFoundError`   | error                | requested encoder is unregistered                                               |
|  [25]   | `DecoderNotFoundError`   | error                | requested decoder is unregistered                                               |
|  [26]   | `MuxerNotFoundError`     | error                | requested muxer is unregistered                                                 |
|  [27]   | `DemuxerNotFoundError`   | error                | requested demuxer is unregistered                                               |
|  [28]   | `FilterNotFoundError`    | error                | requested libavfilter is unregistered                                           |
|  [29]   | `BSFNotFoundError`       | error                | requested bitstream filter is unregistered                                      |
|  [30]   | `InvalidDataError`       | error                | malformed/invalid frame or stream data                                          |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: container open and stream creation
- rail: media

`open` selects `InputContainer` or `OutputContainer` by `mode` (`"r"` default, `"w"` for write); `format` overrides the extension-inferred muxer (`"mp4"`, `"webm"`, `"gif"`); `options`/`container_options`/`stream_options` carry private muxer/demuxer knobs, `metadata_encoding`/`metadata_errors`/`buffer_size`/`timeout` tune the write path, `io_open` supplies a custom Python I/O callback for segmented/protocol output, and `hwaccel` requests GPU decode on read. `add_stream` returns a typed `VideoStream`/`AudioStream`/`SubtitleStream`. `add_stream` is the single typed stream factory: the `codec_name` row discriminates video/audio/subtitle, `rate` is the frame rate (video) or sample rate (audio), and codec options flow through `options` or `**kwargs`. `add_mux_stream` mints a passthrough stream for remux (copy without re-encode); `add_stream_from_template` clones an input stream's parameters; `add_data_stream`/`add_attachment` carry opaque/embedded payloads.

| [INDEX] | [SURFACE]                                  | [CALL_SHAPE]                                         | [CAPABILITY]                      |
| :-----: | :----------------------------------------- | :--------------------------------------------------- | :-------------------------------- |
|  [01]   | `av.open`                                  | `open(file, mode="w", ...) -> OutputContainer`       | open a write container            |
|  [02]   | `av.open`                                  | `open(file, mode="r", *, hwaccel) -> InputContainer` | open a read container (`hwaccel`) |
|  [03]   | `OutputContainer.add_stream`               | `add_stream(codec_name, rate=None, options=None)`    | typed video/audio/subtitle stream |
|  [04]   | `OutputContainer.add_mux_stream`           | `add_mux_stream(codec_name, rate=None, **kwargs)`    | passthrough stream for remux      |
|  [05]   | `OutputContainer.add_stream_from_template` | `add_stream_from_template(template, opaque=None)`    | clone an existing stream's params |
|  [06]   | `OutputContainer.add_attachment`           | `add_attachment(name, mimetype, data)`               | embed a font/cover/file           |
|  [07]   | `OutputContainer.add_data_stream`          | `add_data_stream(codec_name=None, options=None)`     | opaque/timed-metadata data stream |
|  [08]   | `OutputContainer.start_encoding`           | `start_encoding() -> None`                           | finalize params, write header     |
|  [09]   | `OutputContainer.mux`                      | `mux(packets) -> None`                               | write one packet or a sequence    |
|  [10]   | `OutputContainer.mux_one`                  | `mux_one(packet) -> None`                            | write a single packet             |
|  [11]   | `OutputContainer.default_video_codec`      | property -> `str`                                    | muxer default video codec         |
|  [12]   | `OutputContainer.supported_codecs`         | property -> `set[str]`                               | codecs the active muxer admits    |
|  [13]   | `Container.close`                          | `close() -> None`; `__enter__`/`__exit__`            | flush trailer; use `with av.open` |
|  [14]   | `Container.set_chapters`                   | `set_chapters(chapters: list[Chapter]) -> None`      | bind the chapter list on a remux  |
|  [15]   | `Stream.metadata`                          | `dict[str, str]` (mutable before header write)       | per-stream tags (title/language)  |

[ENTRYPOINT_SCOPE]: container demux, decode, and seek (read side)
- rail: media

`InputContainer.demux(stream)` is the polymorphic packet iterator — it is overloaded on a `VideoStream`/`AudioStream`/`SubtitleStream`/`DataStream` (or a tuple, or a `video=`/`audio=` selector kwarg) and yields typed `Packet` objects; `decode(...)` is the same selector but yields decoded `VideoFrame`/`AudioFrame` directly. `streams` exposes the typed stream list and `.video`/`.audio`/`.best(...)` selectors. `seek(offset, *, stream=...)` positions the demuxer by timestamp; `chapters`/`metadata`/`duration`/`bit_rate` read container structure.

| [INDEX] | [SURFACE]                 | [CALL_SHAPE]                                              | [CAPABILITY]                               |
| :-----: | :------------------------ | :-------------------------------------------------------- | :----------------------------------------- |
|  [01]   | `InputContainer.demux`    | `demux(stream=None, *, video, audio) -> Iterator[Packet]` | polymorphic packet iterator over stream(s) |
|  [02]   | `InputContainer.decode`   | `decode(stream=None, *, video, audio) -> Iterator[Frame]` | decode stream(s) to frames                 |
|  [03]   | `InputContainer.seek`     | `seek(offset, *, backward=True, any_frame=False) -> None` | seek to a timestamp (stream time_base)     |
|  [04]   | `InputContainer.streams`  | `-> StreamContainer` (`.video`/`.audio`/`.best(...)`)     | typed stream selectors                     |
|  [05]   | `Stream.decode`           | `decode(packet=None) -> list[VideoFrame \| AudioFrame]`   | decode a demuxed packet; `None` flushes    |
|  [06]   | `InputContainer.chapters` | `chapters() -> list[Chapter]`; `.metadata`/`.duration`    | chapter/metadata/duration structure        |
|  [07]   | `Chapter`                 | `TypedDict` — `id`/`start`/`end`/`time_base`/`metadata`   | one navigation chapter row                 |

[ENTRYPOINT_SCOPE]: frame-sequence encode
- rail: media

`VideoFrame.from_ndarray` lifts a NumPy buffer to a frame; `pts`/`time_base`/`format`/`pix_fmt` are assigned before `encode`. `encode(frame)` returns a `Packet` list; `encode(None)` flushes the encoder. The encode loop muxes each returned packet, then flushes with a `None` frame at end-of-stream. `from_dlpack` imports a frame zero-copy from a DLPack producer (torch/cupy/jax) including from CUDA device memory via `cuda_context`, the GPU-native ingest that avoids a host round-trip; `from_dlpack` also carries `width`/`height`/`stream`/`device_id`/`primary_ctx`; `reformat` carries `src_colorspace`/`dst_colorspace`/`interpolation`/`color_range`/`color_trc`/`color_primaries`/`threads`. `from_bytes` lifts a raw RGBA buffer with `flip_horizontal`/`flip_vertical`; `to_rgb`/`to_ndarray`/`to_image` are the egress mirrors; `make_writable` forces a copy before mutation of a shared buffer. `Packet(bytes)` constructs a raw packet whose `stream`/`time_base`/`pts`/`dts`/`duration` slots admit a timed subtitle payload; `CodecContext.extradata` carries the subtitle codec header, and `Container.format.name` identifies the source muxer before a `BytesIO` output opens with an explicit `format=`.

| [INDEX] | [SURFACE]                      | [CALL_SHAPE]                                              | [CAPABILITY]                            |
| :-----: | :----------------------------- | :-------------------------------------------------------- | :-------------------------------------- |
|  [01]   | `VideoFrame.from_ndarray`      | `from_ndarray(array, format="rgb24", channel_last=False)` | lift a NumPy array to a video frame     |
|  [02]   | `VideoFrame.from_numpy_buffer` | `from_numpy_buffer(array, format="rgb24", width=0)`       | zero-copy wrap a contiguous buffer      |
|  [03]   | `VideoFrame.from_dlpack`       | `from_dlpack(planes, format="nv12", ...)`                 | zero-copy DLPack/CUDA import            |
|  [04]   | `VideoFrame.from_bytes`        | `from_bytes(data, width, height, format="rgba", ...)`     | lift a raw pixel byte buffer            |
|  [05]   | `VideoFrame.from_image`        | `from_image(img)`                                         | lift a Pillow image to a video frame    |
|  [06]   | `VideoFrame.__init__`          | `VideoFrame(width=0, height=0, format="yuv420p")`         | allocate an empty video frame           |
|  [07]   | `VideoFrame.reformat`          | `reformat(width, height, format, ...) -> VideoFrame`      | swscale resize / pixfmt / color convert |
|  [08]   | `VideoFrame.to_ndarray`        | `to_ndarray(channel_last=False, **kwargs) -> ndarray`     | extract a frame to a NumPy array        |
|  [09]   | `VideoFrame.to_rgb`            | `to_rgb(**kwargs) -> VideoFrame`                          | convert to an rgb24 frame               |
|  [10]   | `VideoFrame.make_writable`     | `make_writable() -> None`                                 | copy a shared buffer before mutation    |
|  [11]   | `VideoStream.encode`           | `encode(frame=None) -> list[Packet]`                      | encode a frame; `None` flushes          |
|  [12]   | `Packet`                       | attrs `dts`/`pts`/`duration`/`is_keyframe`/`stream`       | per-packet mux; `decode()` in place     |
|  [13]   | `AudioFrame.from_ndarray`      | `from_ndarray(array, format="s16", layout="stereo")`      | lift a NumPy array to an audio frame    |
|  [14]   | `AudioStream.encode`           | `encode(frame=None) -> list[Packet]`                      | encode an audio frame; `None` flushes   |
|  [15]   | `Packet.__init__`              | `Packet(input: int \| bytes \| None = None)`              | construct a raw timed packet            |
|  [16]   | `Packet.time_base`             | writable `Fraction`                                       | timestamp unit for `pts`/`dts`          |
|  [17]   | `CodecContext.extradata`       | writable `bytes \| None`                                  | codec-private header payload            |
|  [18]   | `Container.format.name`        | property -> `str`                                         | active muxer identifier                 |

[ENTRYPOINT_SCOPE]: filter graph, bitstream filter, and resample
- rail: media

`av.filter.Graph` is the single `libavfilter` owner: `add_buffer(template, width, height, format, name, time_base)`/`add_abuffer(template, sample_rate, format, layout, channels, name, time_base)` create the source node from a stream template, `add(name, args, **kwargs)` adds a named filter (`scale`/`crop`/`overlay`/`fps`/`format`/`loudnorm`), `link_nodes(*nodes)` chains them, and `push(frame)`/`pull()` drive frames through — never a hand-rolled scale/crop/overlay loop. `BitStreamFilterContext(description, in_stream, out_stream)` rewrites a packet bitstream (`h264_mp4toannexb`, `hevc_mp4toannexb`, `extract_extradata`) for a remux without decode/re-encode. `AudioResampler.resample` converts format/rate/layout; `AudioFifo.read(samples=-1, partial=False)` rebuffers samples to a fixed `frame_size` for encoders (AAC) that require exact frame sizes. `CodecContext.create` returns a `VideoCodecContext`/`AudioCodecContext`/`CodecContext` by codec. `av.filter.loudnorm.stats(loudnorm_args, stream)` runs the two-pass EBU R128 measurement over an `AudioStream` and returns the loudnorm JSON (`input_i`/`input_tp`/`input_lra`/`input_thresh`) as bytes — the gated integrated-LUFS read a single-pass encode cannot expose.

| [INDEX] | [SURFACE]                         | [CALL_SHAPE]                                                | [CAPABILITY]                          |
| :-----: | :-------------------------------- | :---------------------------------------------------------- | :------------------------------------ |
|  [01]   | `Graph.add_buffer`                | `add_buffer(template=None, ...) -> FilterContext`           | create a video source node            |
|  [02]   | `Graph.add_abuffer`               | `add_abuffer(template=None, ...) -> FilterContext`          | create an audio source node           |
|  [03]   | `Graph.add`                       | `add(filter, args=None, **kwargs) -> FilterContext`         | add a named libavfilter node          |
|  [04]   | `Graph.link_nodes`                | `link_nodes(*nodes) -> Graph`                               | link a sequence of filter contexts    |
|  [05]   | `Graph.push` / `Graph.pull`       | `push(frame) -> None`; `pull() -> VideoFrame \| AudioFrame` | drive frames through the graph        |
|  [06]   | `Graph.configure`                 | `configure(auto_buffer=True, force=False) -> None`          | validate/configure before pull        |
|  [07]   | `BitStreamFilterContext`          | `BitStreamFilterContext(filter_description, ...)`           | bitstream rewrite (annexb/extradata)  |
|  [08]   | `BitStreamFilterContext.filter`   | `filter(packet=None) -> list[Packet]`; `flush() -> None`    | rewrite a packet bitstream            |
|  [09]   | `AudioResampler`                  | `AudioResampler(format, layout, rate, frame_size)`          | format/rate/layout resample owner     |
|  [10]   | `AudioResampler.resample`         | `resample(frame_or_None) -> list[AudioFrame]`               | resample a frame; `None` flushes      |
|  [11]   | `AudioFifo.write` / `.read`       | `write(frame)`; `read(samples=-1, partial=False)`           | rebuffer to a fixed frame size        |
|  [12]   | `CodecContext.create`             | `create(codec, mode=None, hwaccel=None) -> CodecContext`    | standalone codec context              |
|  [13]   | `CodecContext.parse`              | `parse(raw_input=None) -> list[Packet]`                     | split a raw elementary stream         |
|  [14]   | `av.filter.loudnorm.stats`        | `stats(loudnorm_args: str, stream: AudioStream) -> bytes`   | two-pass EBU R128 measurement JSON    |
|  [15]   | `Graph.pull` (drain signals)      | raises `BlockingIOError` (needs input) / `EOFError` (EOF)   | drain-loop terminal signals           |
|  [16]   | `AudioFormat.packed`/`.is_planar` | property -> `AudioFormat` / `bool`                          | planar and packed sample-format twins |

[ENTRYPOINT_SCOPE]: build registries, capability probes, and per-context filter wiring
- rail: media

The linked FFmpeg build publishes its registered names as module-level sets — `av.codecs_available`, `av.bitstream_filters_available`, and `av.filter.filters_available` — so codec, bsf, and filter admission is a membership probe before `add_stream`/`BitStreamFilterContext`/`Graph.add`, never a deep `*NotFoundError` raise. `av.codec.hwaccel.hwdevices_available()` is a CALL returning the hardware device-type name list, and `HWAccel(device_type, allow_software_fallback)` is the decode-acceleration context `av.open(hwaccel=)` consumes. Multi-input filters wire per context: `FilterContext.link_to(input_, output_idx, input_idx)` binds explicit pads where `Graph.link_nodes` raises `ArgumentError 22`, and `FilterContext.push`/`pull` drive one source among several where the single-source `Graph.push` cannot disambiguate; a dynamic-input filter (`amix`) reports an empty static `Filter(name).inputs` tuple, so arity travels as caller data. `av.time_base` is the 1e6 container timestamp denominator and `Frame.time` the derived presentation seconds (`pts * time_base`); `OutputContainer.metadata` accepts container tags before the header writes.

| [INDEX] | [SURFACE]                                     | [CALL_SHAPE]                                         | [CAPABILITY]                             |
| :-----: | :-------------------------------------------- | :--------------------------------------------------- | :--------------------------------------- |
|  [01]   | `av.codecs_available`                         | module attr -> `set[str]`                            | registered encoder/decoder names         |
|  [02]   | `av.bitstream_filters_available`              | module attr -> `set[str]`                            | registered bitstream-filter names        |
|  [03]   | `av.filter.filters_available`                 | module attr -> `set[str]`                            | registered libavfilter names             |
|  [04]   | `av.codec.hwaccel.hwdevices_available`        | `hwdevices_available() -> list[str]`                 | hardware decode device-type names        |
|  [05]   | `av.codec.hwaccel.HWAccel`                    | `HWAccel(device_type, allow_software_fallback, ...)` | GPU decode context for `open(hwaccel=)`  |
|  [06]   | `FilterContext.link_to`                       | `link_to(input_, output_idx=0, input_idx=0) -> None` | explicit-pad multi-input wiring          |
|  [07]   | `FilterContext.push` / `.pull`                | `push(frame) -> None`; `pull() -> Frame`             | per-source drive in a multi-input graph  |
|  [08]   | `av.library_versions`                         | module attr -> `dict[str, tuple]`                    | bundled libav majors                     |
|  [09]   | `ffmpeg_version_info`                          | module attr -> `str`                                 | ffmpeg build string                      |
|  [10]   | `av.time_base`                                | module attr -> `int` (1_000_000)                     | container timestamp denominator          |
|  [11]   | `Frame.time`                                  | property -> `float \| None`                          | presentation seconds `pts * time_base`   |
|  [12]   | `OutputContainer.metadata`                    | `dict[str, str]` (mutable before header write)       | container tags (title/artist/comment)    |

[ENTRYPOINT_SCOPE]: stream and codec-context configuration
- rail: media

The stream exposes its codec parameters directly and through `codec_context`. `width`/`height`/`pix_fmt`/`bit_rate`/`gop_size`/`framerate` are set after `add_stream` and before the first `encode`; `options` carries codec-private knobs (CRF, preset, tune); `thread_count`/`thread_type` set parallel encode; `hwaccel`/`is_hwaccel` route to GPU; `flags`/`flags2`/`max_b_frames`/`qmin`/`qmax` tune rate control. `time_base` and per-frame `pts` set the presentation timeline; frames carry color and rotation metadata. Each row below is an attribute read of the shown type unless a call signature is given; the frame-rate reads mirror the live `rate` set on `add_stream`, while the encode rate target is `codec_context.framerate`.

| [INDEX] | [SURFACE]                                               | [CALL_SHAPE]                          | [CAPABILITY]                         |
| :-----: | :------------------------------------------------------ | :------------------------------------ | :----------------------------------- |
|  [01]   | `VideoStream.width`                                     | `int`                                 | coded frame width                    |
|  [02]   | `VideoStream.height`                                    | `int`                                 | coded frame height                   |
|  [03]   | `VideoStream.pix_fmt`                                   | `str \| None`                         | output pixel format (`yuv420p`)      |
|  [04]   | `VideoStream.bit_rate`                                  | `int \| None`                         | target bit rate                      |
|  [05]   | `VideoStream.gop_size`                                  | `int`                                 | keyframe (GOP) interval              |
|  [06]   | `VideoStream.average_rate`/`.base_rate`/`.guessed_rate` | `Fraction \| None`                    | demuxed/encode frame-rate reads      |
|  [07]   | `VideoStream.codec_context`                             | `VideoCodecContext`                   | underlying codec state               |
|  [08]   | `VideoStream.set_display_rotation`                      | `set_display_rotation(angle) -> None` | display-matrix rotation side-datum   |
|  [09]   | `CodecContext.options`                                  | `dict[str, str]`                      | codec-private options (crf/preset)   |
|  [10]   | `CodecContext.thread_count`                             | `int`                                 | encoder thread count                 |
|  [11]   | `CodecContext.thread_type`                              | `ThreadType` `FRAME`/`SLICE`/`AUTO`   | parallel-encode strategy             |
|  [12]   | `CodecContext.max_b_frames`                             | `int`                                 | max consecutive B-frames             |
|  [13]   | `CodecContext.flags` / `.flags2`                        | flag set                              | codec flag bitsets (`GLOBAL_HEADER`) |
|  [14]   | `CodecContext.flush_buffers`                            | `flush_buffers() -> None`             | reset codec state at a seek boundary |
|  [15]   | `CodecContext.hwaccel` / `is_hwaccel`                   | `HWAccel`/`bool`                      | hardware acceleration context        |
|  [16]   | `Frame.pts`                                             | `int \| None`                         | presentation timestamp (`time_base`) |
|  [17]   | `Frame.time_base`                                       | `Fraction \| None`                    | timestamp unit for the frame         |
|  [18]   | `VideoFrame.colorspace` / `color_range`                 | attribute                             | per-frame color metadata (BT.709)    |
|  [19]   | `VideoFrame.color_primaries` / `color_trc`              | attribute                             | per-frame primaries / transfer curve |
|  [20]   | `VideoFrame.pict_type` / `key_frame` / `rotation`       | attribute                             | picture type (I/P/B), keyframe, rot  |
|  [21]   | `AudioStream.sample_rate`                               | `int`                                 | audio sample rate                    |
|  [22]   | `AudioStream.layout`                                    | `AudioLayout`                         | channel layout                       |
|  [23]   | `AudioCodecContext.frame_size`                          | `int`                                 | encoder fixed frame size (0 = free)  |
|  [24]   | `AudioCodecContext.format`/`.layout`/`.rate`            | settable attributes                   | sample format / layout / sample rate |
|  [25]   | `AudioFrame.rate`                                       | `int` (settable)                      | sample-rate stamp before resample    |
|  [26]   | `AudioFrame.to_ndarray`                                 | `to_ndarray() -> ndarray`             | extract samples to NumPy             |

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

[STACK_INTEGRATION]:
- universal `numpy` tier (`libs/python/.api/numpy.md`): the frame seam is `VideoFrame.from_ndarray(arr, format)` in / `frame.to_ndarray()` out — a canonical `numpy` `uint8`/`float32` buffer (RGB24/RGBA/GRAY) is the one host pixel surface, so a `scikit-image`/`matplotlib`/`pillow`-produced frame array encodes without a bespoke pixel struct, and `from_numpy_buffer` is the zero-copy variant for a contiguous C-order buffer. The device edge is `from_dlpack` (a `torch`/`cupy`/`jax` CUDA tensor ingests with no host round-trip); the host array and the DLPack capsule are the only two ingest shapes.
- universal `anyio` tier (`libs/python/.api/anyio.md`): the synchronous `demux`/`decode`/`encode`/`mux` loop is CPU-bound C, so the boundary owner drives one container per `anyio.to_thread.run_sync` worker (or a `CapacityLimiter`-bounded fan over many inputs); a transcode farm is N bounded worker tasks each holding one `InputContainer`/`OutputContainer`, never an unbounded thread pool. The pull/push of `Packet`s stays inside the worker; only the finished bytes cross back to the async caller.
- universal `expression` tier (`libs/python/.api/expression.md`): the `FFmpegError` subtree maps at the boundary to a `Result[MediaReceipt, MediaError]` — `EncoderNotFoundError`/`MuxerNotFoundError` are a typed `Error` arm (the codec/muxer the spec named is not registered), `InvalidDataError` is the malformed-input arm, and a successful `mux` trailer yields the `Ok` receipt; the `try/except FFmpegError` lives only in the boundary adapter, never in the domain pipeline.
- universal `structlog`/`opentelemetry` tier: the per-op evidence (container format, codec, `pix_fmt`, frame count, `ffmpeg_version_info`, byte length) is the structured event/span payload; `av.library_versions` is read once at boundary init so the bundled `libavcodec`/`libavformat` majors ride the receipt as deployment facts.
- sibling artifacts libs: `pillow` (`Image` <-> `VideoFrame.from_image`/`to_image`) is the still-image edge for a single-frame poster or a GIF frame source; the document owner (`pymupdf`/`reportlab`) embeds a rendered MP4/GIF byte payload `av` produced; a `vl_convert`/`matplotlib` figure sequence rasterized to `numpy` frames is the chart-animation ingest — each is a `from_ndarray`/`from_image` row into the same `MediaOp`, never a parallel media writer.

[RAIL_LAW]:
- Package: `av`
- Accept: frame-sequence encode, decode, transcode, remux, and filter-graph pipelines feeding the media and document owners
