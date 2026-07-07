# [PY_ARTIFACTS_MEDIA_TIMELINE]

The non-linear-editing (NLE) layer on top of the container/filtergraph spine — the one `Timeline` owner over the closed `TimelineOp` family (`Trim`/`Concat`/`Segment`/`Xfade`) that assembles multiple content-keyed clips into one deliverable. `Timeline` opens no container and implements no filter of its own; every arm COMPOSES the `media/container#CONTAINER` capsule primitives (`_decode_video`/`_decode_window` read, `_encode_video` re-encode, `_seek` random-access, `_open_sink` segmented sink, `_deployment`/`_media_fault` evidence and fault rails) and the `media/filtergraph#FILTER` builders (`cross_dissolve` for the transition, `link_clips` for the native concat), so the NLE modality is a projection over the spine, never a second FFmpeg surface. `Trim` seeks to the keyframe at/before the in-point through `_seek(reader, stream, in_point)` (`InputContainer.seek` + `CodecContext.flush_buffers`, so no stale reference frame bleeds across the cut), decodes the frame-accurate `[in, out)` window through `_decode_window`, and re-encodes it zero-based through `_encode_video` (the fresh 0-based `pts` the shared `_drive` stamps IS the `setpts=PTS-STARTPTS` zero-base, no filter needed). `Concat` is ONE polymorphic arm over TWO verified strategies keyed by source-parameter match, never a knob: when every clip shares codec/resolution/pix_fmt/time_base the lossless `_packet_concat` demuxes each source and re-stamps every `Packet.dts`/`pts`/`duration` onto a monotonic timeline over one `add_stream_from_template` cloned stream (no re-decode, bit-exact), else the re-encoding `_filter_concat` decodes each clip and re-encodes the joined frame stream. `Segment` composes the container `SEGMENT`-muxer segmented sink with `segment_times` at the cut boundaries, so the split is the spine's own `io_open` `UPath` egress producing N segments and one manifest-keyed artifact. `Xfade` decodes both clips, cross-dissolves the overlapping `window = round(duration · rate)` frames through `media/filtergraph#FILTER` `cross_dissolve` (the native `xfade` filter refuses in-process `configure()` on `av 17.1.0`, so the numpy ramp is the arm, not a parallel graph), and re-encodes. A multi-clip timeline is a DAG — each clip carries its own parent `ContentKey`, and `Timeline.parents` exposes them so the `core/plan#PLAN` `ArtifactPipeline` schedules the constituent clip producers as upstream `ArtifactWork` nodes and elides an already-rendered clip on a warm replay: the media plane's strongest `ArtifactPipeline`/CPM exemplar, where a concat of five independently-keyed clips is a five-parent front the planner drives concurrently before the join. `TimelineFault` reuses the `media/container#CONTAINER` `MediaFault` closed vocabulary — one media cause rail across the plane. `emit()` IS the `core/plan#PLAN` `ArtifactWork.work` thunk, returning `the `emit()`/`_emit` node contract` keyed over the assembled bytes, and contributes the shared `core/receipt#RECEIPT` `ArtifactReceipt.Media` case whose `facts` band carries the clip and segment counts. Every worker crossing runs over the `WORKER_BAND`-bounded the runtime process lane lane under the `/`_emit` node contract` keyed over the assembled bytes, the `Clip` value carrying its source bytes AND its parent `ContentKey` so `Timeline.parents` projects the `core/plan#PLAN` `ArtifactPipeline` upstream-clip dependency set, the `Trim` seek-window-reencode arm (`media/container#CONTAINER` `_seek` + `_decode_window` + `_encode_video`, the fresh 0-based `_drive` `pts` the zero-base), the `Concat` two-strategy arm (`_packet_concat` lossless `Packet` re-stamp over `add_stream_from_template` when params match, `_filter_concat` decode-reencode otherwise, the strategy DERIVED from the clip-param match never a knob), the `Segment` arm composing the container `SEGMENT` segmented sink with `segment_times`, and the `Xfade` arm composing `media/filtergraph#FILTER` `cross_dissolve` over the overlapping window; `av` `InputContainer.seek`/`demux`/`decode`/`streams.best`, `CodecContext.flush_buffers`, `OutputContainer.add_stream_from_template`/`mux_one`, `Packet.dts`/`pts`/`duration`/`time_base`, `VideoFrame.time`/`to_ndarray`, `VideoStream.average_rate` — all verified present on `av 17.1.0` and all reached THROUGH the `media/container#CONTAINER` primitives, never re-opened here. This page owns the `TimelineOp` family, the `Clip` value, the `Timeline.parents` DAG projection, and the `_trim`/`_concat`/`_packet_concat`/`_filter_concat`/`_segment`/`_xfade` workers; it composes the container capsule, the filtergraph builders, and the `media/audio#MEDIA` `_decode_audio` inverse, minting the shared `ArtifactReceipt.Media` case.

## [02]-[TIMELINE]

- Owner: `Timeline` the one NLE-assembly owner discriminating modality over the closed `TimelineOp` family; `TimelineOp` an `expression.tagged_union` whose every case carries its own typed payload, never a shared erased bag nor a per-op `Timeline` subclass nor a parallel `trim`/`concat`/`xfade` function trio; `Clip` the frozen source value carrying `data: bytes` (the encoded clip) and `key: ContentKey` (the parent artifact the clip producer minted), so a clip IS a content-keyed node and a timeline op's dependency IS its clips' keys, never a path or a re-minted key; `ConcatStrategy` NOT a field — the packet-vs-filter choice is DERIVED once from `_params(clip)` equality across the clips (all-match -> lossless packet copy, mismatch -> re-encode), the discriminant recoverable from the clip streams themselves, so a `lossless: bool` knob the body re-derives is the rejected form; `TimelineFault` the reused `media/container#CONTAINER` `MediaFault` closed vocabulary (`unregistered`/`invalid`/`codec`/`provision`/`worker`/`contract`), never a parallel timeline fault rail; `MediaProfile`/`MediaEvidence`/`ContainerFormat`/`SegmentSpec` the shared `media/container#CONTAINER` family this page reads and never re-declares; `Result[(ContentKey, ArtifactReceipt), MediaFault]` the one carrier every arm returns, keyed over the assembled bytes through `ContentIdentity.of(container.value, blob)` so an identical assembly at an identical profile is a cache hit by reference. The op owns no container open, no encoder, and no filter — it composes the spine's decode/encode/seek primitives and the filtergraph builders, the assembly the projection.
- Cases: `TimelineOp` cases — `Trim(clip, in_point, out_point)` (the frame-accurate cut: `_decode_window(clip.data, in_point, out_point)` seeks to the keyframe at/before the in-point, flushes the decoder, and keeps only the frames whose `VideoFrame.time` lands in `[in_point, out_point)`, then `_encode_video(frames, profile)` re-encodes them zero-based — the fresh index-derived `pts` the shared `_drive` stamps IS the `setpts=PTS-STARTPTS` zero-base) · `Concat(clips)` (the join over the two DERIVED strategies — `_packet_concat` when `_params` matches across every clip, demuxing each source and re-stamping each `Packet.dts`/`pts`/`duration` onto the running offset over one `add_stream_from_template` cloned stream for a bit-exact lossless copy with no re-decode, else `_filter_concat` decoding each clip through `_decode_video` and re-encoding the joined frame stream through `_encode_video` when codecs differ — never a re-encode where a lossless copy is admissible) · `Segment(clip, cuts)` (the split: `_decode_video(clip.data)` then a `SEGMENT`-container re-encode whose `SegmentSpec.options` carries `segment_times={cuts}`, so the container's own `io_open` `UPath` segmented sink produces N segments and one manifest-keyed artifact — the split IS the spine's segmented encode, not a re-implemented muxer) · `Xfade(under, over, offset, duration, transition)` (the transition: `_decode_video` both clips, `cross_dissolve(under_frames, over_frames, window=round(duration · rate))` linearly ramps the overlapping frames — the `media/filtergraph#FILTER` numpy substitute since the native `xfade` refuses in-process wiring — then `_encode_video` re-encodes the joined stream, the audio leg composing `media/audio#MEDIA` `_decode_audio` + a `filtergraph` `acrossfade` a growth axis) — matched by one total `match`/`case`, the four-op modality recovered from the discriminant, never a name suffix; every arm is a module-level worker the `_mux` dispatch drives over the `to_process` lane by qualified name.
- Entry: `emit()` is `async` over the runtime `async_boundary`, returning `the `emit()`/`_emit` node contract` and IS the `core/plan#PLAN` `ArtifactWork.work` thunk the pipeline schedules; `_emit` maps the `_mux` outcome through `_keyed` deriving `ContentIdentity.of(...)` over the assembled bytes, and `_mux` dispatches each `TimelineOp` case onto the runtime process lane wrapped in the spine's `_WORKER_RETRY` (imported, never re-minted), catching `BrokenWorkerProcess` -> `MediaFault(worker=...)` and `BeartypeCallHintViolation` -> `MediaFault(contract=...)` exactly as the container `_mux` does. Each `@beartype`-woven worker composes the spine's `_decode_*`/`_encode_video`/`_seek` primitives (which own the `av.open` capsule and the `av.error.FFmpegError` -> `_media_fault` capture), so a timeline worker adds no second `try`/`except FFmpegError` beyond the composed primitives' own, and returns `Result[(bytes, MediaEvidence), MediaFault]` as picklable data across the `to_process` seam. `Timeline.parents` is the pure projection of the op's clip `ContentKey`s the composition root reads to wire the `ArtifactWork.parents` upstream set, so a five-clip concat plans as a five-parent dependency front.
- Auto: the clip DAG is content-addressed end to end — each `Clip.key` is the `ContentKey` the producing `media/container#CONTAINER`/`scene/render#SCENE` arm already minted, so `Timeline.parents` re-mints nothing and a warm replay elides an already-rendered clip; the `Concat` strategy is derived once by `_params(clip.data)` (open the reader, read `(codec_name, width, height, pix_fmt, str(time_base))` off `streams.best("video")`) — all-equal routes `_packet_concat`, any mismatch `_filter_concat`, the discriminant the clip streams themselves; `_packet_concat` opens one writer, clones the first clip's stream through `add_stream_from_template`, and for each clip demuxes its packets re-stamping `packet.dts`/`pts += offset` and advancing `offset` by the clip's last `pts + duration` so the joined timeline is monotonic (the `Packet` re-stamp verified settable), muxing each through `mux_one` with no decode; `_filter_concat` decodes each clip through `_decode_video` (a per-clip `FilterNode.Scale` to the target composed when resolutions differ) and re-encodes the concatenated frame tuple through `_encode_video`; `_trim` decodes the `[in, out)` window through `_decode_window` and re-encodes it 0-based; `_segment` decodes the clip and re-encodes through a `SEGMENT`-container profile whose `segment_times` the cuts fill; `_xfade` ramps the overlapping window through `cross_dissolve` and re-encodes; every re-encode folds the shared `_deployment` facts plus the timeline-specific clip/segment counts through `MediaEvidence` once via `msgspec.structs.replace(evidence, facts=evidence.facts | extra)`.
- Receipt: each timeline op contributes `core/receipt#RECEIPT` `ArtifactReceipt.Media` — the shared eight-slot case — keyed by the content key `_keyed` derives over the assembled bytes; the `facts` band carries the timeline-specific evidence beside the spine's deployment facts: `Trim` adds `{"clips": 1, "trimmed_seconds": out - in}`, `Concat` adds `{"clips": len(clips), "strategy": "packet"|"filter"}`, `Segment` adds `{"clips": 1, "segments": len(cuts) + 1}`, and `Xfade` adds `{"clips": 2, "dissolve_frames": window}` — one `ArtifactReceipt.Media` case across the whole media plane (container HDR/segment facts, filtergraph filter-node count, audio LUFS, timeline clip/segment counts all on the one shared band), never a parallel timeline receipt rail. The `strategy` fact makes the lossless-vs-reencode decision observable evidence a downstream consumer reads off one structured-log line.
- Growth: a new NLE operation is one `TimelineOp` case plus one `_mux` arm plus one worker composing the spine primitives — the `assert_never` breaking the dispatch at type-check until the arm exists; a new concat strategy is one `_params` axis and one branch in the derived selection (a hardware-accelerated stream-copy path, an audio-aware concat), never a knob; a new transition is one `transition` value the `cross_dissolve` ramp or a future in-process `xfade` reads; a clip that is itself a timeline (a nested edit) is one `Clip` whose `key` is the nested `emit()` product, so the `ArtifactPipeline` DAG nests without a new surface; a new evidence fact is one `facts` band key with ZERO receipt edit; the audio cross-fade leg is one `_xfade` growth composing `media/audio#MEDIA` `_decode_audio` + `media/filtergraph#FILTER` `acrossfade`; zero new surface — the modality space is the four ops on one owner, every addition a case, arm, or worker.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from typing import Literal, assert_never

from anyio import BrokenWorkerProcess, to_process
from beartype.roar import BeartypeCallHintViolation
from expression import Error, Result, case, tag, tagged_union
from msgspec import Struct

from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.faults import BoundaryFault, RuntimeRail, async_boundary
from rasm.runtime.lanes import WORKER_BAND

from artifacts.core.plan import Admission, ArtifactWork
from artifacts.core.receipt import ArtifactReceipt
from artifacts.media.container import (
    MediaEvidence,
    MediaFault,
    MediaProfile,
    _WORKER_RETRY,
)  # the shared spine family + the ONE transient-death retry policy, imported not re-minted

# --- [TYPES] ----------------------------------------------------------------------------

type TimelineOpTag = Literal["trim", "concat", "segment", "xfade"]

# --- [MODELS] ---------------------------------------------------------------------------


class Clip(Struct, frozen=True):
    data: bytes  # the encoded clip bytes a worker decodes/demuxes
    key: ContentKey  # the parent artifact key the clip producer minted; Timeline.parents projects it


@tagged_union(frozen=True)
class TimelineOp:
    tag: TimelineOpTag = tag()
    trim: tuple[Clip, float, float] = case()  # (clip, in_point, out_point)
    concat: tuple[Clip, ...] = case()  # ordered clips joined lossless-or-reencoded by param match
    segment: tuple[Clip, tuple[float, ...]] = case()  # (clip, cut boundaries)
    xfade: tuple[Clip, Clip, float, float, str] = case()  # (under, over, offset, duration, transition)

    @staticmethod
    def Trim(clip: Clip, in_point: float, out_point: float, /) -> "TimelineOp":
        return TimelineOp(trim=(clip, in_point, out_point))

    @staticmethod
    def Concat(clips: tuple[Clip, ...], /) -> "TimelineOp":
        return TimelineOp(concat=clips)

    @staticmethod
    def Segment(clip: Clip, cuts: tuple[float, ...], /) -> "TimelineOp":
        return TimelineOp(segment=(clip, cuts))

    @staticmethod
    def Xfade(under: Clip, over: Clip, offset: float, duration: float, transition: str = "fade", /) -> "TimelineOp":
        return TimelineOp(xfade=(under, over, offset, duration, transition))

    @property
    def clips(self) -> tuple[Clip, ...]:
        match self:
            case TimelineOp(tag="trim", trim=(clip, *_)):
                return (clip,)
            case TimelineOp(tag="concat", concat=clips):
                return clips
            case TimelineOp(tag="segment", segment=(clip, _)):
                return (clip,)
            case TimelineOp(tag="xfade", xfade=(under, over, *_)):
                return (under, over)
            case _ as unreachable:
                assert_never(unreachable)


class Timeline(Struct, frozen=True):
    op: TimelineOp
    profile: MediaProfile = MediaProfile()

    @property
    def parents(self) -> tuple[ContentKey, ...]:
        # the upstream clip keys the core/plan#PLAN ArtifactPipeline wires as this node's ArtifactWork.parents, so a
        # multi-clip assembly is a dependency front the planner drives concurrently and elides on a warm replay.
        return tuple(clip.key for clip in self.op.clips)

    def emit(self, /) -> ArtifactWork:
        return ArtifactWork(key=self._key, work=self._emit, parents=(), admission=Admission(keyed=None), cost=1.0)

    @property
    def _key(self) -> ContentKey:
        # key-over-INPUT: the canonical frozen op minted PRE-RUN — the muxed output's own content
        # address rides the receipt facts, never the elision key.
        return ContentIdentity.of(f"timeline-{self.op.tag}", self.op, policy=CANONICAL_POLICY)

    async def _emit(self) -> RuntimeRail[ArtifactReceipt]:
        # the renamed private thunk — the inner Result[..., MediaFault] is DEAD: the member's MediaFault
        # folds into ITS OWN rail's boundary fault (Work[ArtifactReceipt] forbids an inner Result), and
        # the terminal receipt threads the PRE-RUN key (receipt.slot == node.key).
        railed = await async_boundary(f"timeline.{self.op.tag}", self._folded)
        return railed.bind(
            lambda res: res.map(lambda pair: pair[1]).map_error(
                lambda fault: BoundaryFault(boundary=(f"timeline.{self.op.tag}", f"{fault.tag}:{fault}"))
            )
        )

    async def _folded(self) -> Result[tuple[ContentKey, ArtifactReceipt], MediaFault]:
        return (await self._mux()).map(self._keyed)

    def _keyed(self, produced: tuple[bytes, MediaEvidence], /) -> tuple[ContentKey, ArtifactReceipt]:
        blob, evidence = produced
        key = ContentIdentity.of(evidence.container.value, blob)
        return key, ArtifactReceipt.Media(
            key,
            evidence.container.value,
            evidence.codec,
            evidence.duration,
            evidence.byte_count,
            evidence.frame_count,
            evidence.bit_rate,
            evidence.facts,
        )

    async def _mux(self) -> Result[tuple[bytes, MediaEvidence], MediaFault]:
        profile = self.profile
        try:
            match self.op:
                case TimelineOp(tag="trim", trim=(clip, in_point, out_point)):
                    return await _WORKER_RETRY(to_process.run_sync, _trim, clip.data, in_point, out_point, profile, limiter=WORKER_BAND)
                case TimelineOp(tag="concat", concat=clips):
                    return await _WORKER_RETRY(to_process.run_sync, _concat, tuple(c.data for c in clips), profile, limiter=WORKER_BAND)
                case TimelineOp(tag="segment", segment=(clip, cuts)):
                    return await _WORKER_RETRY(to_process.run_sync, _segment, clip.data, cuts, profile, limiter=WORKER_BAND)
                case TimelineOp(tag="xfade", xfade=(under, over, offset, duration, transition)):
                    return await _WORKER_RETRY(
                        to_process.run_sync, _xfade, under.data, over.data, offset, duration, transition, profile, limiter=WORKER_BAND
                    )
                case _:
                    assert_never(self.op)
        except BrokenWorkerProcess as broken:
            return Error(MediaFault(worker=str(broken)))
        except BeartypeCallHintViolation as violation:
            return Error(MediaFault(contract=type(violation).__name__))
```

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import io
from typing import TYPE_CHECKING

import msgspec
from beartype import beartype
from expression import Error, Ok, Result

# same `artifacts.media.timeline` module; the workers stay module-level so the runtime process lane dispatches them by
# qualified name. Every av capsule op is reached THROUGH the container spine primitives — timeline opens no container.
lazy import av
lazy import av.error
lazy from artifacts.media.container import (
    ContainerFormat,
    MediaEvidence,
    MediaFault,
    MediaProfile,
    SegmentSpec,
    _decode_video,
    _decode_window,
    _deployment,
    _encode_video,
    _media_fault,
)
lazy from artifacts.media.filtergraph import cross_dissolve

if TYPE_CHECKING:
    from numpy.typing import NDArray

# --- [OPERATIONS] -----------------------------------------------------------------------


def _augment(evidence: MediaEvidence, extra: frozendict[str, float | str], /) -> MediaEvidence:
    # fold the timeline-specific clip/segment/strategy facts onto the spine's deployment band, ONE MediaEvidence.
    return msgspec.structs.replace(evidence, facts=evidence.facts | extra)


def _params(clip: bytes, /) -> tuple[str, int, int, str, str]:
    # the concat-strategy discriminant: the stream identity every lossless packet-copy requires equal — a mismatch on
    # any axis routes the re-encoding filter path. Read once per clip through the container capsule, never re-opened after.
    with av.open(io.BytesIO(clip), mode="r") as reader:
        s = reader.streams.best("video")
        return s.codec_context.name, s.width, s.height, s.pix_fmt or "", str(s.time_base)


@beartype
def _trim(clip: bytes, in_point: float, out_point: float, profile: MediaProfile) -> Result[tuple[bytes, MediaEvidence], MediaFault]:
    frames, _rate = _decode_window(clip, in_point, out_point)  # container: _seek + flush + keep [in, out) — the frame-accurate window
    facts = frozendict({"clips": 1.0, "trimmed_seconds": out_point - in_point})
    return _encode_video(frames, profile).map(
        lambda produced: (produced[0], _augment(produced[1], facts))
    )  # 0-based _drive pts = setpts=PTS-STARTPTS


@beartype
def _packet_concat(clips: tuple[bytes, ...], profile: MediaProfile) -> Result[tuple[bytes, MediaEvidence], MediaFault]:
    # lossless join over ONE owned writer: clone the first clip's stream once, then re-stamp each source's packets onto a
    # monotonic offset (valid because `_params` proved every clip shares the time_base) — no decode, bit-exact. The
    # Exemption is the imperative demux/re-stamp/mux kernel; the offset advances by each clip's last pts+duration.
    try:
        sink = io.BytesIO()
        with av.open(sink, mode="w", format=profile.container.value, container_options=dict(profile.container_options)) as out:
            with av.open(io.BytesIO(clips[0]), mode="r") as head:
                stream = out.add_stream_from_template(head.streams.best("video"))
            offset = 0
            for clip in clips:  # Exemption: platform-forced imperative packet re-stamp over one owned muxer handle
                with av.open(io.BytesIO(clip), mode="r") as reader:
                    src = reader.streams.best("video")
                    last = 0
                    for packet in reader.demux(src):
                        if packet.dts is None:
                            continue
                        packet.stream, packet.dts, packet.pts = stream, (packet.dts or 0) + offset, (packet.pts or 0) + offset
                        last = max(last, (packet.pts or 0) + (packet.duration or 0))
                        out.mux_one(packet)
                    offset = last
        blob = sink.getvalue()
        return Ok((
            blob,
            MediaEvidence.measure(
                profile.container,
                profile.codec,
                float(offset),
                len(clips),
                int(profile.bit_rate or 0),
                blob,
                _deployment(profile) | {"clips": float(len(clips)), "strategy": "packet"},
            ),
        ))
    except ImportError as exc:
        return Error(MediaFault(provision=str(exc)))
    except av.error.FFmpegError as exc:
        return Error(_media_fault("concat", exc))


@beartype
def _filter_concat(clips: tuple[bytes, ...], profile: MediaProfile) -> Result[tuple[bytes, MediaEvidence], MediaFault]:
    joined: tuple["NDArray", ...] = tuple(frame for clip in clips for frame in _decode_video(clip)[0])  # re-encode path for mismatched codecs
    facts = frozendict({"clips": float(len(clips)), "strategy": "filter"})
    return _encode_video(joined, profile).map(lambda produced: (produced[0], _augment(produced[1], facts)))


@beartype
def _concat(clips: tuple[bytes, ...], profile: MediaProfile) -> Result[tuple[bytes, MediaEvidence], MediaFault]:
    # ONE polymorphic arm, the strategy DERIVED from clip-param equality — lossless copy when every clip matches, else
    # re-encode; a `lossless: bool` knob the body re-derives is the rejected form.
    try:
        return _packet_concat(clips, profile) if len({_params(clip) for clip in clips}) == 1 else _filter_concat(clips, profile)
    except av.error.FFmpegError as exc:
        return Error(_media_fault("concat", exc))


@beartype
def _segment(clip: bytes, cuts: tuple[float, ...], profile: MediaProfile) -> Result[tuple[bytes, MediaEvidence], MediaFault]:
    # the split IS the container SEGMENT-muxer segmented sink: re-encode the decoded clip through a profile whose
    # SegmentSpec carries segment_times, so the spine's own io_open UPath egress writes N segments + one manifest.
    if profile.segment is None:
        return Error(MediaFault(invalid="segment op needs profile.segment (a UPath root); none supplied"))
    frames, _rate = _decode_video(clip)
    seg = profile.segment
    segment_profile = msgspec.structs.replace(
        profile,
        container=ContainerFormat.SEGMENT,
        segment=msgspec.structs.replace(seg, options=seg.options | {"segment_times": ",".join(f"{c:.3f}" for c in cuts)}),
    )
    facts = frozendict({"clips": 1.0, "segments": float(len(cuts) + 1)})
    return _encode_video(frames, segment_profile).map(lambda produced: (produced[0], _augment(produced[1], facts)))


@beartype
def _xfade(
    under: bytes, over: bytes, offset: float, duration: float, transition: str, profile: MediaProfile
) -> Result[tuple[bytes, MediaEvidence], MediaFault]:
    # the transition: decode both clips, cross-dissolve the overlapping window through the filtergraph numpy substitute
    # (native xfade refuses in-process configure on av 17.1.0), re-encode the joined stream; the audio acrossfade leg grows here.
    under_frames, rate = _decode_video(under)
    over_frames, _ = _decode_video(over)
    window = max(1, round(duration * rate))
    joined = cross_dissolve(under_frames, over_frames, window)  # filtergraph: under[-window:] fades out under over[:window]
    facts = frozendict({"clips": 2.0, "dissolve_frames": float(window), "transition": transition})
    return _encode_video(joined, profile).map(lambda produced: (produced[0], _augment(produced[1], facts)))
```
