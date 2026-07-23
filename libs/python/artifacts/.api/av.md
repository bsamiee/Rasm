# [PY_ARTIFACTS_API_AV]

`av` (PyAV) binds the FFmpeg media surface for the artifacts MEDIA rail: `av.open` selects a read `InputContainer` or write `OutputContainer`, `add_stream` mints the typed `VideoStream`/`AudioStream` owners, and one `from_ndarray`/`encode`/`mux` loop drives a frame sequence into MP4/WebM/GIF while the inverse `demux`/`decode`/`to_ndarray` loop reads it back. One polymorphic `MediaOp` folds encode, decode, transcode, remux, and filter through the bundled FFmpeg libraries, so the branch never shells out to a system `ffmpeg` binary nor re-implements the container, filter, or codec layer.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `av`
- package: `av` (BSD-3-Clause)
- import: `av`
- owner: `artifacts`
- rail: media
- bundled: FFmpeg shared libraries — `libavcodec`/`libavformat`/`libavfilter`/`libswscale`/`libswresample` with the `libx264`/`libx265`/`libvpx`/`libSvtAv1Enc` encoders, read at runtime through `av.library_versions`/`av.ffmpeg_version_info`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: container, stream, frame, codec, filter, and typed-error roots

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]        | [CAPABILITY]                                                                    |
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

| [INDEX] | [SURFACE]                                  | [CALL_SHAPE]                                         | [CAPABILITY]                      |
| :-----: | :----------------------------------------- | :--------------------------------------------------- | :-------------------------------- |
|  [01]   | `av.open`                                  | `open(file, mode="w", ...) -> OutputContainer`       | open a write container            |
|  [02]   | `av.open`                                  | `open(file, mode="r", *, hwaccel) -> InputContainer` | open a read container (`hwaccel`) |
|  [03]   | `OutputContainer.add_stream`               | `add_stream(codec_name, rate, options, hwaccel)`     | typed a/v/subtitle stream         |
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

- `OutputContainer.add_stream`: a `hwaccel` device uploads software frames passed to `encode` to the GPU automatically, and `VideoCodecContext.sw_format` sets the pre-upload pixel format the hardware encoder consumes.

[ENTRYPOINT_SCOPE]: container demux, decode, and seek (read side)

`demux` and `decode` discriminate one stream, a stream tuple, or a `video=`/`audio=` selector — `demux` yields typed `Packet`s, `decode` yields `VideoFrame`/`AudioFrame`, one polymorphic read surface over both.

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

`pts`/`time_base`/`pix_fmt` are assigned on the frame before `encode(frame)`; the loop muxes each returned `Packet` and flushes with `encode(None)` at end-of-stream.

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

- `VideoFrame.reformat`: a transfer/primaries conversion runs only when `dst_color_trc`/`dst_color_primaries` is passed; reserved or unsupported source `color_primaries`/`color_trc` tags (VP9, NVDEC) no longer raise on `reformat`/`to_ndarray`/`to_rgb`/`to_image`.
- `VideoStream.encode`: a DLPack CUDA frame encodes directly under `pix_fmt="cuda"`, the encoder adopting the frame's `hw_frames_ctx`; downloading a hardware frame to system memory preserves `pts`/`time_base`/colorspace.

[ENTRYPOINT_SCOPE]: filter graph, bitstream filter, and resample

`av.filter.loudnorm.stats` runs the two-pass EBU R128 measurement over an `AudioStream`, the gated integrated-LUFS read a single-pass encode cannot expose.

| [INDEX] | [SURFACE]                        | [CALL_SHAPE]                                                     | [CAPABILITY]                      |
| :-----: | :------------------------------- | :--------------------------------------------------------------- | :-------------------------------- |
|  [01]   | `Graph.add_buffer`               | `add_buffer(template=None, ...) -> FilterContext`                | create a video source node        |
|  [02]   | `Graph.add_abuffer`              | `add_abuffer(template=None, ...) -> FilterContext`               | create an audio source node       |
|  [03]   | `Graph.add`                      | `add(filter, args=None, **kwargs) -> FilterContext`              | add a named libavfilter node      |
|  [04]   | `Graph.link_nodes`               | `link_nodes(*nodes) -> Graph`                                    | chain filter contexts in order    |
|  [05]   | `Graph.push`/`.vpush`/`.pull`    | `push(frame, at=-1)`; `vpush(vframe, at=-1)`; `pull() -> Frame`  | drive frames; `at` picks a source |
|  [06]   | `Graph.configure`                | `configure(auto_buffer=True, force=False) -> None`               | validate/configure before pull    |
|  [07]   | `BitStreamFilterContext`         | `BitStreamFilterContext(filter_description, ...)`                | bitstream rewrite                 |
|  [08]   | `BitStreamFilterContext.filter`  | `filter(packet=None) -> list[Packet]`; `flush() -> None`         | rewrite a packet bitstream        |
|  [09]   | `AudioResampler`                 | `AudioResampler(format, layout, rate, frame_size, options=None)` | swresample resample owner         |
|  [10]   | `AudioResampler.resample`        | `resample(frame_or_None) -> list[AudioFrame]`                    | resample a frame; `None` flushes  |
|  [11]   | `AudioFifo.write` / `.read`      | `write(frame)`; `read(samples=-1, partial=False)`                | rebuffer to a fixed frame size    |
|  [12]   | `CodecContext.create`            | `create(codec, mode=None, hwaccel=None) -> CodecContext`         | standalone codec context          |
|  [13]   | `CodecContext.parse`             | `parse(raw_input=None) -> list[Packet]`                          | split a raw elementary stream     |
|  [14]   | `av.filter.loudnorm.stats`       | `stats(loudnorm_args: str, stream: AudioStream) -> bytes`        | two-pass EBU R128 stats JSON      |
|  [15]   | `Graph.pull` (drain signals)     | raises `BlockingIOError` (needs input) / `EOFError` (EOF)        | drain-loop terminal signals       |
|  [16]   | `AudioFormat.packed`/`is_planar` | property -> `AudioFormat` / `bool`                               | planar/packed format twins        |

- `Graph.push`/`.vpush`: `at` selects one buffer source by index for a multi-input filter (`overlay`), where the single-source default (`at=-1`) cannot disambiguate.
- `BitStreamFilterContext`: `in_stream` accepts a `Codec` or a codec-name `str`, pinning the input codec without a full `Stream`.
- `AudioResampler`: `options` passes `libswresample` knobs (`resampler`, `filter_size`, `cutoff`) to the underlying resample graph.

[ENTRYPOINT_SCOPE]: build registries, capability probes, and per-context filter wiring

Multi-input filters wire per context: `FilterContext.link_to` binds explicit pads and `FilterContext.push`/`pull` drive one source among several where the single-source `Graph.push` cannot disambiguate.

| [INDEX] | [SURFACE]                              | [CALL_SHAPE]                                         | [CAPABILITY]                            |
| :-----: | :------------------------------------- | :--------------------------------------------------- | :-------------------------------------- |
|  [01]   | `av.codecs_available`                  | module attr -> `set[str]`                            | registered encoder/decoder names        |
|  [02]   | `av.bitstream_filters_available`       | module attr -> `set[str]`                            | registered bitstream-filter names       |
|  [03]   | `av.filter.filters_available`          | module attr -> `set[str]`                            | registered libavfilter names            |
|  [04]   | `av.codec.hwaccel.hwdevices_available` | `hwdevices_available() -> list[str]`                 | hardware decode device-type names       |
|  [05]   | `av.codec.hwaccel.HWAccel`             | `HWAccel(device_type, allow_software_fallback, ...)` | GPU decode/encode ctx (`hwaccel=`)      |
|  [06]   | `FilterContext.link_to`                | `link_to(input_, output_idx=0, input_idx=0) -> None` | explicit-pad multi-input wiring         |
|  [07]   | `FilterContext.push` / `.pull`         | `push(frame) -> None`; `pull() -> Frame`             | per-source drive in a multi-input graph |
|  [08]   | `av.library_versions`                  | module attr -> `dict[str, tuple]`                    | bundled libav majors                    |
|  [09]   | `ffmpeg_version_info`                  | module attr -> `str`                                 | ffmpeg build string                     |
|  [10]   | `av.time_base`                         | module attr -> `int` (1_000_000)                     | container timestamp denominator         |
|  [11]   | `Frame.time`                           | property -> `float \| None`                          | presentation seconds `pts * time_base`  |
|  [12]   | `OutputContainer.metadata`             | `dict[str, str]` (mutable before header write)       | container tags (title/artist/comment)   |

[ENTRYPOINT_SCOPE]: stream and codec-context configuration

Each row is set after `add_stream` and before the first `encode`, reading the shown type unless a call shape is given; the frame-rate reads mirror the live `rate` on `add_stream`, the encode rate target is `codec_context.framerate`.

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
|  [16]   | `VideoCodecContext.sw_format`                           | `VideoFormat \| None` (settable, str) | HW-encoder software pixel format     |
|  [17]   | `Frame.pts`                                             | `int \| None`                         | presentation timestamp (`time_base`) |
|  [18]   | `Frame.time_base`                                       | `Fraction \| None`                    | timestamp unit for the frame         |
|  [19]   | `VideoFrame.colorspace` / `color_range`                 | attribute                             | per-frame color metadata (BT.709)    |
|  [20]   | `VideoFrame.color_primaries` / `color_trc`              | attribute                             | per-frame primaries / transfer curve |
|  [21]   | `VideoFrame.pict_type` / `key_frame` / `rotation`       | attribute                             | picture type (I/P/B), keyframe, rot  |
|  [22]   | `AudioStream.sample_rate`                               | `int`                                 | audio sample rate                    |
|  [23]   | `AudioStream.layout`                                    | `AudioLayout`                         | channel layout                       |
|  [24]   | `AudioCodecContext.frame_size`                          | `int`                                 | encoder fixed frame size (0 = free)  |
|  [25]   | `AudioCodecContext.format`/`.layout`/`.rate`            | settable attributes                   | sample format / layout / sample rate |
|  [26]   | `AudioFrame.rate`                                       | `int` (settable)                      | sample-rate stamp before resample    |
|  [27]   | `AudioFrame.to_ndarray`                                 | `to_ndarray() -> ndarray`             | extract samples to NumPy             |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One polymorphic `MediaOp` discriminates Encode / Decode / Transcode / Remux / Filter; the read side (`demux`/`decode`/`seek`) and the write side (`add_stream`/`encode`/`mux`) are arms of one owner, a transcode being `decode` then optional `Graph` then `encode` then `mux`.
- One `av.open` opens every container as a context manager (trailer written, IO released); `mode` selects `InputContainer` vs `OutputContainer`, `format` selects the muxer (`mp4`/`webm`/`gif`) as a call row, and `io_open` supplies a Python I/O callback for protocol/segment sinks.
- `add_stream(codec_name, rate)` is the single typed stream factory: `codec_name` discriminates video (`h264`/`libx264`/`vp9`/`libvpx-vp9`/`libsvtav1`/`gif`) from audio (`aac`/`libopus`), `add_mux_stream`/`add_stream_from_template` carry the remux/clone rows, codec knobs ride `options`/`**kwargs`, and a `hwaccel` device (`h264_nvenc`/`h264_vaapi`/`h264_videotoolbox`) routes hardware encode, `VideoCodecContext.sw_format` fixing the pre-upload pixel format.
- `VideoFrame.from_ndarray` ingests a host array and `from_dlpack` a zero-copy CUDA-context tensor; a DLPack CUDA frame encodes directly under `pix_fmt="cuda"` (the encoder adopting its `hw_frames_ctx`) while a software frame passed to a `hwaccel` encoder uploads automatically, and `reformat` is the swscale resize/colorspace-convert row over the assigned `pts`/`time_base`/`pix_fmt`.
- `av.filter.Graph` is the single libavfilter owner: scale/crop/overlay/fps/format/loudnorm are `add(...)` rows wired with `link_nodes`, driven with `push`/`pull`, running between `decode` and `encode`.
- `BitStreamFilterContext` rewrites a packet bitstream (annexb, extradata) for a `demux` then `bsf.filter` then `mux` copy that changes container or codec without re-decoding.
- `stream.encode(frame)` then `container.mux(packets)` is the one encode-then-mux row, flushed with `encode(None)`; `mux_one(packet)` serves one-at-a-time backpressure and `AudioFifo` rebuffers to the fixed `frame_size` encoders like AAC demand.
- `FFmpegError` is a typed `tag`/`errno` tree whose lookup, decode, and protocol faults are distinct exception classes, never raw return codes.
- Each op captures container format, codec, `pix_fmt`, frame count, resolution, frame rate, bit rate, GOP size, filter-graph chain, `ffmpeg_version_info`, and input/output byte length as a media receipt.

[STACKING]:
- `numpy`(`.api/numpy.md`): the frame seam is `VideoFrame.from_ndarray(arr, format)` in / `frame.to_ndarray()` out over a `uint8`/`float32` RGB24/RGBA/GRAY or 10-bit `yuv420p10le` buffer, `from_numpy_buffer` the zero-copy contiguous variant, `from_dlpack` the device edge a `torch`/`cupy`/`jax` CUDA tensor ingests with no host round-trip.
- `anyio`(`.api/anyio.md`): the synchronous `demux`/`decode`/`encode`/`mux` loop drives one container per `anyio.to_thread.run_sync` worker under a `CapacityLimiter` fan, only the finished bytes crossing back to the async caller.
- `expression`(`.api/expression.md`): the `FFmpegError` subtree maps at the boundary to `Result[MediaReceipt, MediaError]` — `EncoderNotFoundError`/`MuxerNotFoundError` the unregistered-codec arm, `InvalidDataError` the malformed-input arm, a successful `mux` trailer the `Ok` receipt.
- `structlog`(`.api/structlog.md`)/`opentelemetry`(`.api/opentelemetry-api.md`): the per-op evidence is the structured event/span payload, and `av.library_versions` read once at boundary init rides the receipt as a deployment fact.
- `pillow`(`.api/pillow.md`) is the still-image edge (`Image` to `VideoFrame.from_image`/`to_image`) for a poster or GIF frame, and the document owner (`pymupdf`/`reportlab`) embeds the produced MP4/GIF byte payload — each a `from_ndarray`/`from_image` row into the same `MediaOp`.
- within-library: the `MediaOp` composes `av.open`, `add_stream`, `from_ndarray`, `encode`, `mux`, the filter `Graph`, and the `BitStreamFilterContext` remux path into one owner, so encode, decode, transcode, remux, and filter are arms rather than sibling packages.

[LOCAL_ADMISSION]:
- `import av` at boundary scope only; the manifest import policy binds module-level import.
- Codec, muxer, bsf, and filter admission is a membership probe over `av.codecs_available`/`av.bitstream_filters_available`/`av.filter.filters_available` and `hwdevices_available` before `add_stream`/`BitStreamFilterContext`/`Graph.add`.

[RAIL_LAW]:
- Package: `av`
- Owns: the FFmpeg container/codec/filter media surface — `demux`/`decode`/`seek` read, `add_stream`/`encode`/`mux` write, the libavfilter graph, bitstream-filter remux, and swscale/swresample conversion.
- Accept: frame-sequence encode, decode, transcode, remux, and filter-graph pipelines feeding the media and document owners as `MediaOp` arms.
- Reject: a subprocess shell-out to a system `ffmpeg`; a hand-rolled muxer, packetizer, filter loop, or codec layer; a parallel reader/writer package or `add_video_stream`/`add_audio_stream` pair over the one `add_stream` factory; a raw pixel copy where `from_ndarray`/`from_dlpack` ingest.
