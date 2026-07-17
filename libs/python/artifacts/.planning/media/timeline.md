# [PY_ARTIFACTS_MEDIA_TIMELINE]

`Timeline` is the non-linear-editing layer on top of the container/filtergraph spine — the owner over a closed `TimelineOp` family (`Trim`/`Concat`/`Segment`/`Xfade`/`Speed`/`Reverse` plus generated `Effect`) that assembles content-keyed clips into one deliverable. `Timeline` opens a container only inside its own packet workers and implements no filter of its own: re-encode arms compose the `media/container#CONTAINER` capsule primitives (`_decode_video`/`_decode_window`/`_encode_video`/`_mux_av`/`_transcode`, the segmented sink), while `Effect` carries an ordered `FilterNode` tuple whose existing closed vocabulary generates scale, crop, frame-rate, format, grade, denoise, sharpen, transpose, pad, deinterlace, text, and subtitle operations through one case. A multi-clip timeline is a DAG — each `Clip` carries its parent `ContentKey`, `Timeline.parents` projects them, and `emit()` wires that projection into `ArtifactWork.parents`, so the `core/plan#PLAN` `ArtifactPipeline` schedules the constituent clip producers as upstream nodes and elides an already-rendered clip on a warm replay.

Two ops derive a lossless-versus-re-encode strategy from the clip streams, never a knob. `Concat` routes `_packet_concat` — every source stream cloned by template and every packet re-stamped onto a per-stream monotonic offset, bit-exact with no decode — when `_params` proves stream identity across the clips, else `_filter_concat` decodes, joins video frames and same-rate audio samples, and re-encodes; differing audio rates remain a typed `MediaFault.invalid`, never mistimed samples. `Trim` routes `_packet_trim` — a packet-copy window — when the in-point lands on a lead keyframe AND both boundaries seat on every cloned stream's own packet grid `_keyframes` reads off the packet stream, else `_filter_trim` re-encodes the `[in, out)` window zero-based with its audio sample-sliced. Every re-encode arm closes through one `_sealed` fold: frames plus optional audio blocks enter `_mux_av` when the source carries audio and `_encode_video` when it does not. `Xfade` blends the overlapping window through the filtergraph `wired` dissolve arm or the timeline-owned `_MASK` table and cross-fades audio by overlap-add. `MediaFault` carries every worker and provider cause; `_crossed` maps the lane's terminal `BoundaryFault` through `_lapsed` and flattens the worker rail without raising.

## [01]-[INDEX]

- [01]-[TIMELINE]: the `Timeline` owner over the closed `TimelineOp` family — structural edits plus one generated `Effect` case over ordered `FilterNode` values, all content-keyed DAG nodes projecting over the container capsule and the filtergraph `wired` entrypoint and folding into the shared `ArtifactReceipt.Media` case.

## [02]-[TIMELINE]

- Owner: `Timeline` discriminates modality over the closed `TimelineOp` union, each case carrying its own typed payload — never a shared erased bag, a per-op subclass, or a parallel `trim`/`concat`/`xfade` trio. `Clip` carries `data: bytes` and `key: ContentKey`, so a clip is a content-keyed node and an op's dependency is its clips' keys, never a path or a re-minted key. Strategy is never a field — packet-versus-filter choice derives from stream identity for `Concat` and keyframe alignment for `Trim`. `Effect` carries the ordered `FilterNode` program directly, so every single-input filtergraph member is one data value under the existing grammar rather than another timeline case. `Transition` is the closed transition vocabulary the `Xfade` payload carries. `MediaFault` remains the one fault rail across the media plane.
- Cases: `Trim` re-encodes zero-based or packet-copies when the in-point seats tick-exact on a keyframe packet and the out-point on a packet boundary; an audio-only clip qualifies only on real packet boundaries, so a boundary-seated lossless audio trim or a param-equal audio concat packet-copies while the re-encode fallback stays video-bound and rails `MediaFault.invalid` on an audio-only source. `Segment` composes the spine's segmented encode. `Xfade` blends video through the filtergraph `wired` dissolve arm or a `_MASK` plane and audio through overlap-add. `Speed` retimes by index-pick and rate-resamples audio, while `Reverse` flips frame and sample order. `Effect` delegates the ordered `FilterNode` program to `_transcode`, so capability grows at the filtergraph vocabulary rather than through timeline case proliferation.
- Entry: `Timeline.parents` is the pure projection of the op's clip `ContentKey`s and `emit()` passes it as `ArtifactWork.parents`, so the planner wires upstream producers without inspecting `TimelineOp` internals. Pre-run identity includes deterministic encodings of both `TimelineOp` and `MediaProfile`. `_crossed` dispatches each worker through `self.lane.offload(Kernel.of(worker, KernelTrait.HOSTILE, idempotent=...))` — idempotency derived structurally from the crossing's segmented-profile argument, never a per-arm convention — maps the outer `BoundaryFault` through `_lapsed`, and flattens the worker's `Result` without an exception bridge.
- Auto: `_packet_concat` clones every source stream from the head clip's template and advances one offset per stream by its last `pts + duration` in that stream's own `time_base`, so the joined timeline stays monotonic across video and audio. `_sealed` is the one re-encode close every structural arm shares: `_mux_av` for `Some(Voice)` through the container-keyed `_VOICE_CODEC` policy and `_encode_video` for `Nothing`, with `_voice` preserving malformed-audio faults on `Result` rather than masking them as absence. Packet evidence composes `_probe`, so duration, frame count, and bit rate describe the delivered bytes rather than clip count or requested profile values.
- Receipt: `_keyed` threads the PRE-RUN node key as the receipt slot — the `core/receipt#RECEIPT` elision law — and demotes the assembled-bytes content address to the `address` band fact; each op adds its timeline facts to the shared `Media` band — `Trim` `{clips, trimmed_seconds, strategy}`, `Concat` `{clips, strategy}`, `Segment` `{clips, segments}`, `Xfade` `{clips, dissolve_frames, transition}`, `Speed` `{clips, factor}`, `Reverse` `{clips}`, and `Effect` `{filter_nodes}` — one `ArtifactReceipt.Media` case across the whole plane.
- Packages: `av` supplies the demux/seek/re-stamp/mux capsule; the timeline's own workers open read/write containers only for the packet-copy arms, and every decode/encode rides the `media/container#CONTAINER` primitives. Members settled against the folder `.api`.
- Growth: a structural NLE operation is one `TimelineOp` case plus one total `_mux` arm and one worker composing the spine; a single-input visual operation is one `FilterNode` member consumed unchanged by `Effect`; a concat strategy is one stream-identity axis; a transition is one `Transition` member plus one `_MASK` row; a nested timeline is one `Clip` whose `key` is the nested product; an evidence fact is one band key with no receipt edit.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from enum import StrEnum
from typing import Literal, assert_never

from builtins import frozendict
from expression import Error, Result, case, tag, tagged_union
from msgspec import Struct

from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.faults import BoundaryFault, RuntimeRail, async_boundary
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.workers import Kernel, KernelTrait

from rasm.artifacts.core.plan import Admission, ArtifactWork
from rasm.artifacts.core.receipt import ArtifactReceipt
from rasm.artifacts.media.container import HwAccel, MediaEvidence, MediaFault, MediaProfile, Produced, _lapsed
from rasm.artifacts.media.filtergraph import FilterNode

# --- [TYPES] ----------------------------------------------------------------------------

type TimelineOpTag = Literal["trim", "concat", "segment", "xfade", "speed", "reverse", "effect"]

# --- [CONSTANTS] --------------------------------------------------------------------------


class Transition(StrEnum):
    FADE = "fade"  # the filtergraph `wired` dissolve ramp
    WIPE_LEFT = "wipe_left"  # _MASK spatial sweeps owned here
    WIPE_RIGHT = "wipe_right"
    IRIS = "iris"


# --- [MODELS] ---------------------------------------------------------------------------


class Clip(Struct, frozen=True):
    data: bytes  # the encoded clip bytes a worker decodes/demuxes
    key: ContentKey  # the parent artifact key the clip producer minted; Timeline.parents projects it


@tagged_union(frozen=True)
class TimelineOp:
    tag: TimelineOpTag = tag()
    trim: tuple[Clip, float, float] = case()  # (clip, in_point, out_point) seconds
    concat: tuple[Clip, ...] = case()  # ordered clips joined lossless-or-reencoded by param match
    segment: tuple[Clip, tuple[float, ...]] = case()  # (clip, cut boundaries)
    xfade: tuple[Clip, Clip, float, Transition] = case()  # (under, over, overlap seconds, transition)
    speed: tuple[Clip, float] = case()  # (clip, retime factor; >1 faster, <1 slower)
    reverse: Clip = case()
    effect: tuple[Clip, tuple[FilterNode, ...]] = case()

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
    def Xfade(under: Clip, over: Clip, duration: float, transition: Transition = Transition.FADE, /) -> "TimelineOp":
        return TimelineOp(xfade=(under, over, duration, transition))

    @staticmethod
    def Speed(clip: Clip, factor: float, /) -> "TimelineOp":
        return TimelineOp(speed=(clip, factor))

    @staticmethod
    def Reverse(clip: Clip, /) -> "TimelineOp":
        return TimelineOp(reverse=clip)

    @staticmethod
    def Effect(clip: Clip, nodes: tuple[FilterNode, ...], /) -> "TimelineOp":
        return TimelineOp(effect=(clip, nodes))

    @property
    def clips(self) -> tuple[Clip, ...]:
        match self:
            case (
                TimelineOp(tag="trim", trim=(clip, *_))
                | TimelineOp(tag="segment", segment=(clip, _))
                | TimelineOp(tag="speed", speed=(clip, _))
                | TimelineOp(tag="effect", effect=(clip, _))
            ):
                return (clip,)
            case TimelineOp(tag="reverse", reverse=clip):
                return (clip,)
            case TimelineOp(tag="concat", concat=clips):
                return clips
            case TimelineOp(tag="xfade", xfade=(under, over, *_)):
                return (under, over)
            case _ as unreachable:
                assert_never(unreachable)


class Timeline(Struct, frozen=True):
    op: TimelineOp
    profile: MediaProfile = MediaProfile()
    # `lane` arrives projected via LanePolicy.of(context) at the composition root — a capacity literal has no owner.
    lane: LanePolicy

    @property
    def parents(self) -> tuple[ContentKey, ...]:
        return tuple(clip.key for clip in self.op.clips)

    def emit(self, /) -> ArtifactWork:
        # clip keys ARE the node's parent edges, so the planner schedules the clip producers and elides a warm replay.
        return ArtifactWork(key=self._key, work=self._emit, parents=self.parents, admission=Admission(keyed=None), cost=1.0)

    @property
    def _key(self) -> ContentKey:
        return ContentIdentity.key(f"media.timeline-{self.op.tag}", _CANON.encode((self.op, self.profile)))

    async def _emit(self) -> RuntimeRail[ArtifactReceipt]:
        # member MediaFault folds into the boundary fault (Work[ArtifactReceipt] forbids an inner Result).
        railed = await async_boundary(f"media.timeline.{self.op.tag}", self._folded)
        return railed.bind(
            lambda res: res.map_error(lambda fault: BoundaryFault(boundary=(f"media.timeline.{self.op.tag}", f"{fault.tag}:{fault}")))
        )

    async def _folded(self) -> Result[ArtifactReceipt, MediaFault]:
        return (await self._mux()).map(self._keyed)

    def _keyed(self, produced: Produced, /) -> ArtifactReceipt:
        # receipt.slot threads the PRE-RUN node key (the core/receipt elision law); the product address rides the band.
        blob, evidence = produced
        address = ContentIdentity.key(evidence.container.value, blob)
        return ArtifactReceipt.Media(
            self._key,
            evidence.container.value,
            evidence.codec,
            evidence.duration,
            evidence.byte_count,
            evidence.frame_count,
            evidence.bit_rate,
            evidence.facts | {"address": address.hex},
        )

    async def _crossed(self, worker: "Callable[..., Result[Produced, MediaFault]]", /, *args: object) -> Result[Produced, MediaFault]:
        # a segmented profile's io_open sink writes a manifest plus segment set — external state a worker-death replay must
        # never repeat — so idempotency derives structurally from the crossing's own profile argument, never a per-arm convention.
        replayable = not any(isinstance(a, MediaProfile) and a.container.segmented and a.segment is not None for a in args)
        outcome = await self.lane.offload(Kernel.of(worker, KernelTrait.HOSTILE, idempotent=replayable), *args)
        return outcome.map_error(_lapsed).bind(lambda inner: inner)

    async def _mux(self) -> Result[Produced, MediaFault]:
        match self.op:
            case TimelineOp(tag="trim", trim=(clip, in_point, out_point)):
                return await self._crossed(_trim, clip.data, in_point, out_point, self.profile)
            case TimelineOp(tag="concat", concat=clips):
                return await self._crossed(_concat, tuple(c.data for c in clips), self.profile)
            case TimelineOp(tag="segment", segment=(clip, cuts)):
                # the EFFECTIVE segmented profile derives loop-side, so `_crossed`'s structural replay probe reads
                # the profile the worker actually muxes with — never the pre-derivation original it would misread as replayable.
                match _segmented(cuts, self.profile):
                    case Result(tag="error", error=fault):
                        return Error(fault)
                    case Result(tag="ok", ok=segment_profile):
                        return await self._crossed(_segment, clip.data, cuts, segment_profile)
                    case _ as unreachable:
                        assert_never(unreachable)
            case TimelineOp(tag="xfade", xfade=(under, over, duration, transition)):
                return await self._crossed(_xfade, under.data, over.data, duration, transition, self.profile)
            case TimelineOp(tag="speed", speed=(clip, factor)):
                return await self._crossed(_speed, clip.data, factor, self.profile)
            case TimelineOp(tag="reverse", reverse=clip):
                return await self._crossed(_reverse, clip.data, self.profile)
            case TimelineOp(tag="effect", effect=(clip, nodes)):
                return await self._crossed(_transcode, clip.data, self.profile, nodes)
            case _ as unreachable:
                assert_never(unreachable)
```

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import io
from math import isfinite
from typing import TYPE_CHECKING

import msgspec
import numpy as np
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.collections import Block
from expression.extra.result import traverse

# workers stay module-level for qualified-name dispatch; decode/encode rides the container spine, the packet-copy
# arms alone open their own av read/write capsules.
lazy import av
lazy import av.error
lazy from rasm.artifacts.media.audio import _decode_audio
lazy from rasm.artifacts.media.container import (
    _CANON,
    ContainerFormat,
    _decode_video,
    _decode_window,
    _deployment,
    _encode_video,
    _media_fault,
    _mux_av,
    _probe,
    _transcode,
    _worker,
    Verification,
)
lazy from rasm.artifacts.media.filtergraph import wired

if TYPE_CHECKING:
    from collections.abc import Callable

    from numpy.typing import NDArray

    from rasm.artifacts.media.audio import Pcm
    from rasm.artifacts.media.container import Frames
    from rasm.artifacts.media.filtergraph import FilterNode

# --- [CONSTANTS] --------------------------------------------------------------------------

# progress t in [0,1] -> (h, w) float32 weight plane for the OVER frame; FADE routes the filtergraph ramp instead.
_MASK: frozendict[Transition, "Callable[[float, int, int], NDArray[np.float32]]"] = frozendict({
    Transition.WIPE_LEFT: lambda t, h, w: np.broadcast_to((np.arange(w) < t * w).astype(np.float32), (h, w)),
    Transition.WIPE_RIGHT: lambda t, h, w: np.broadcast_to((np.arange(w) >= (1.0 - t) * w).astype(np.float32), (h, w)),
    Transition.IRIS: lambda t, h, w: (
        ((np.arange(h)[:, None] - h / 2) ** 2 + (np.arange(w)[None, :] - w / 2) ** 2) <= (t * np.hypot(h, w) / 2) ** 2
    ).astype(np.float32),
})

_VOICE_CODEC: frozendict[ContainerFormat, str] = frozendict({
    ContainerFormat.MP4: "aac",
    ContainerFormat.WEBM: "libopus",
    ContainerFormat.MKV: "libopus",
    ContainerFormat.HLS: "aac",
    ContainerFormat.DASH: "aac",
    ContainerFormat.SEGMENT: "aac",
    ContainerFormat.MPEGTS: "aac",
})

# --- [OPERATIONS] -----------------------------------------------------------------------


def _augment(evidence: "MediaEvidence", extra: frozendict[str, float | str], /) -> "MediaEvidence":
    # fold the timeline facts onto the spine's deployment band, ONE MediaEvidence.
    return msgspec.structs.replace(evidence, facts=evidence.facts | extra)


type Voice = tuple[tuple["Pcm", ...], int, str]  # (blocks, rate, channel-layout name) — the decode contract keeps layout


def _voice(clip: bytes, /) -> Result[Option[Voice], "MediaFault"]:
    try:
        with av.open(io.BytesIO(clip), mode="r") as reader:
            present = bool(reader.streams.audio)
        return _decode_audio(clip).map(Some) if present else Ok(Nothing)
    except ImportError as exc:
        return Error(MediaFault(provision=str(exc)))
    except av.error.FFmpegError as exc:
        return Error(_media_fault("audio_probe", exc))


def _joined(blocks: "tuple[Pcm, ...]", /) -> "NDArray":
    return np.concatenate([np.asarray(block) for block in blocks], axis=-1)


def _video(clip: bytes, accel: "HwAccel | None" = None, /) -> Result[tuple[int, "Frames"], "MediaFault"]:
    # active profile's read-side acceleration policy threads every structural decode — REQUIRED refusal and
    # FALLBACK attempts stay the container owner's, never a timeline-local software-only bypass.
    try:
        rate, decoded = _decode_video(clip, accel)
        frames = tuple(decoded)
        return Ok((rate, frames)) if frames else Error(MediaFault(invalid="timeline source produced no video frames"))
    except ImportError as exc:
        return Error(MediaFault(provision=str(exc)))
    except av.error.FFmpegError as exc:
        return Error(_media_fault("decode_video", exc))


def _pcm(clip: bytes, voice: Option[Voice], template: "Pcm", rate: int, layout: str, /) -> Result["Pcm", "MediaFault"]:
    # total over the three-field Voice contract: rate then layout refuse before any block joins, absence stays silence.
    match voice:
        case Option(tag="some", some=(_, source_rate, _)) if source_rate != rate:
            return Error(MediaFault(invalid=f"audio rate {source_rate} does not match timeline rate {rate}"))
        case Option(tag="some", some=(_, _, source_layout)) if source_layout != layout:
            return Error(MediaFault(invalid=f"audio layout {source_layout} does not match timeline layout {layout}"))
        case Option(tag="some", some=(blocks, _, _)):
            return Ok(_joined(blocks))
        case Option(tag="nothing"):
            duration = _probe(clip)[0]
            return Ok(np.zeros((*template.shape[:-1], round(duration * rate)), dtype=template.dtype))
        case _ as unreachable:
            assert_never(unreachable)


def _voiced_profile(profile: "MediaProfile", arate: int, /) -> Result["MediaProfile", "MediaFault"]:
    codec = _VOICE_CODEC.get(profile.container)
    return (
        Ok(msgspec.structs.replace(profile, codec=codec, rate=arate))
        if codec is not None
        else Error(MediaFault(invalid=f"container {profile.container.value} cannot carry timeline audio"))
    )


def _sealed(
    frames: "Frames", voice: Option[Voice], profile: "MediaProfile", facts: frozendict[str, float | str], /
) -> Result[tuple[bytes, "MediaEvidence"], "MediaFault"]:
    match voice:
        case Option(tag="some", some=(blocks, rate, _layout)) if blocks:
            return _voiced_profile(profile, rate).bind(
                lambda audio: _mux_av(frames, blocks, profile, audio).map(lambda pair: (pair[0], _augment(pair[1], facts)))
            )
        case Option(tag="some"):
            return Error(MediaFault(invalid="empty audio sequence"))
        case Option(tag="nothing"):
            produced = _encode_video(frames, profile)
        case _ as unreachable:
            assert_never(unreachable)
    return produced.map(lambda pair: (pair[0], _augment(pair[1], facts)))


def _params(clip: bytes, /) -> tuple[tuple[str, ...], ...]:
    # concat-strategy discriminant: the full per-stream identity a lossless packet copy requires equal across clips.
    with av.open(io.BytesIO(clip), mode="r") as reader:
        return tuple(
            (str(s.index), s.type, s.codec_context.name, str(s.time_base), *((str(s.width), str(s.height), s.pix_fmt or "") if s.type == "video" else (str(s.sample_rate), s.layout.name)))
            for s in reader.streams
            if s.type in ("video", "audio")
        )


def _keyframes(clip: bytes, /) -> tuple[tuple[float, ...], tuple[tuple[tuple[float, ...], float], ...], bool] | None:
    # trim-strategy discriminant read off the packet stream (no decode): lead keyframe instants, then EVERY copied
    # stream's own (packet instants, tick) rows with the lead first — `_packet_trim` clones every video/audio
    # stream, so the caller proves the window against each stream's grid, never the lead's alone. An audio-only
    # clip reads its audio packets — every one a keyframe.
    with av.open(io.BytesIO(clip), mode="r") as reader:
        lead = reader.streams.best("video") or reader.streams.best("audio")
        if lead is None:
            return None
        ordered = (lead, *(s for s in reader.streams if s.type in ("video", "audio") and s.index != lead.index))
        marked: dict[int, list[tuple[float, bool]]] = {s.index: [] for s in ordered}
        for packet in reader.demux(*ordered):  # Exemption: one demux pass buckets per-stream packet instants in place
            if packet.pts is not None:
                marked[packet.stream.index].append((float(packet.pts * packet.stream.time_base), packet.is_keyframe))
        instants = tuple(at for at, keyed in marked[lead.index] if keyed)
        per_stream = tuple((tuple(at for at, _ in marked[s.index]), float(s.time_base)) for s in ordered)
        return instants, per_stream, lead.type == "video"


def _windowed(stamps: tuple[float, ...], tick: float, start: float, stop: float, /) -> bool:
    # a stream admits a lossless window when both boundaries land on its own packet grid — or outside it entirely,
    # where the cut removes nothing mid-packet; a stream with no timed packets constrains nothing.
    if not stamps:
        return True
    opened = start <= stamps[0] or any(abs(at - start) <= tick for at in stamps)
    closed = stop > stamps[-1] or any(abs(at - stop) <= tick for at in stamps)
    return opened and closed


def _lead_codec(delivered: object, /) -> str:
    lead = delivered.streams.best("video") or delivered.streams.best("audio")
    return lead.codec_context.name


@_worker
def _trim(clip: bytes, in_point: float, out_point: float, profile: "MediaProfile") -> Result[tuple[bytes, "MediaEvidence"], "MediaFault"]:
    # ONE polymorphic arm, the strategy DERIVED: a window whose in-point seats EXACTLY on a keyframe packet and
    # whose out-point on a packet boundary (tick-exact, never a half-period tolerance that admits a mid-GOP cut and
    # drops the anchoring keyframe) packet-copies — an audio-only clip qualifies only on real packet boundaries —
    # anything else re-encodes through the video filter path.
    if not (isfinite(in_point) and isfinite(out_point) and 0.0 <= in_point < out_point):
        return Error(MediaFault(invalid=f"trim window must be finite and ordered, got [{in_point}, {out_point})"))
    try:
        probed = _keyframes(clip)
        if probed is None:
            return Error(MediaFault(invalid="trim source carries no video or audio stream"))
        instants, per_stream, has_video = probed
        (stamps, tick), *companions = per_stream
        start = next((at for at in instants if abs(at - in_point) <= tick), None)
        stop = out_point if stamps and out_point > stamps[-1] else next((at for at in stamps if abs(at - out_point) <= tick), None)
        # `_packet_trim` clones EVERY video/audio stream, so both boundaries must seat on every companion's own
        # packet grid too — a lead-only proof admits a window that chops a companion stream mid-packet.
        aligned = start is not None and stop is not None and all(_windowed(marks, step, start, stop) for marks, step in companions)
        packetable = aligned and not profile.container.segmented and profile.verification is Verification.NONE
        if packetable:
            return _packet_trim(clip, start, stop, profile)
        return _filter_trim(clip, in_point, out_point, profile) if has_video else Error(MediaFault(invalid="filter-path trim requires a video stream"))
    except ImportError as exc:
        return Error(MediaFault(provision=str(exc)))
    except av.error.FFmpegError as exc:
        return Error(_media_fault("trim", exc))


def _filter_trim(clip: bytes, in_point: float, out_point: float, profile: "MediaProfile") -> Result[tuple[bytes, "MediaEvidence"], "MediaFault"]:
    _rate, frames = _decode_window(clip, in_point, out_point, profile.hwaccel)
    facts = frozendict({"clips": 1.0, "trimmed_seconds": out_point - in_point, "strategy": "filter"})
    return _voice(clip).bind(
        lambda voice: _sealed(
            frames,
            voice.map(lambda item: ((_joined(item[0])[..., int(in_point * item[1]) : int(out_point * item[1])],), item[1], item[2])),
            profile,
            facts,
        )
    )


def _packet_trim(clip: bytes, in_point: float, out_point: float, profile: "MediaProfile") -> Result[tuple[bytes, "MediaEvidence"], "MediaFault"]:
    # lossless window: clone every stream by template, keep packets timed inside the exact [in, out) window, re-stamp
    # from the selected keyframe origin to a zero base in each stream's own time_base — valid because the caller
    # snapped both boundaries onto real packet timestamps, so the first kept packet IS the anchoring keyframe.
    try:
        sink = io.BytesIO()
        with av.open(io.BytesIO(clip), mode="r") as reader, av.open(
            sink, mode="w", format=profile.container.value, container_options=dict(profile.container_options)
        ) as out:
            out.metadata.update(dict(profile.metadata))
            for attachment in profile.attachments:
                out.add_attachment(attachment.name, attachment.mimetype, attachment.data)
            cloned = {s.index: out.add_stream_from_template(s) for s in reader.streams if s.type in ("video", "audio")}
            for packet in reader.demux(*(reader.streams[i] for i in cloned)):  # Exemption: imperative packet re-stamp over one owned muxer
                source = packet.stream
                stamp = packet.dts if packet.pts is None else packet.pts  # decode order anchors the window when presentation is absent
                if stamp is None or not in_point <= float(stamp * source.time_base) < out_point:
                    continue
                shift = int(round(in_point / source.time_base))
                # PTS and DTS re-stamp independently and an absent one stays None — fabricating a DTS from PTS breaks
                # B-frame reorder — while every test is `is None`, never truthiness, so a legitimate 0 stamp survives.
                packet.stream = cloned[packet.stream.index]
                packet.pts = None if packet.pts is None else packet.pts - shift
                packet.dts = None if packet.dts is None else packet.dts - shift
                out.mux_one(packet)
        blob = sink.getvalue()
        duration, count, bit_rate, measured = _probe(blob)
        with av.open(io.BytesIO(blob), mode="r") as delivered:
            codec = _lead_codec(delivered)
        facts = _deployment(profile) | {"clips": 1.0, "trimmed_seconds": out_point - in_point, "strategy": "packet"}
        return Ok((blob, MediaEvidence.measure(profile.container, codec, duration, count, bit_rate, blob, facts | measured)))
    except av.error.FFmpegError as exc:
        return Error(_media_fault("trim", exc))


@_worker
def _concat(clips: tuple[bytes, ...], profile: "MediaProfile") -> Result[tuple[bytes, "MediaEvidence"], "MediaFault"]:
    # ONE polymorphic arm, the strategy DERIVED from full per-stream param equality; param-equal audio-only clips
    # packet-join lossless while the re-encode fallback stays video-bound.
    if not clips:
        return Error(MediaFault(invalid="concat requires at least one clip"))
    try:
        specs = tuple(_params(clip) for clip in clips)
        packetable = not profile.container.segmented and profile.verification is Verification.NONE and len(set(specs)) == 1
        if packetable:
            return _packet_concat(clips, profile)
        if any(all(row[1] != "video" for row in spec) for spec in specs):
            return Error(MediaFault(invalid="filter-path concat requires video streams"))
        return _filter_concat(clips, profile)
    except ImportError as exc:
        return Error(MediaFault(provision=str(exc)))
    except av.error.FFmpegError as exc:
        return Error(_media_fault("concat", exc))


def _packet_concat(clips: tuple[bytes, ...], profile: "MediaProfile") -> Result[tuple[bytes, "MediaEvidence"], "MediaFault"]:
    # lossless join over ONE owned writer: every stream cloned from the head clip's template, each source's packets
    # RE-BASED from that stream's own first timestamp then re-stamped onto a per-stream monotonic offset in the
    # stream's time_base (valid because `_params` proved stream identity) — a clip whose timeline starts nonzero
    # (edit-list head, negative-dts lead) would otherwise splice in shifted — no decode, bit-exact, both carried.
    sink = io.BytesIO()
    with av.open(sink, mode="w", format=profile.container.value, container_options=dict(profile.container_options)) as out:
        out.metadata.update(dict(profile.metadata))
        for attachment in profile.attachments:
            out.add_attachment(attachment.name, attachment.mimetype, attachment.data)
        with av.open(io.BytesIO(clips[0]), mode="r") as head:
            cloned = {s.index: out.add_stream_from_template(s) for s in head.streams if s.type in ("video", "audio")}
        offsets = dict.fromkeys(cloned, 0)
        for clip in clips:  # Exemption: imperative packet re-stamp over one owned muxer handle
            with av.open(io.BytesIO(clip), mode="r") as reader:
                origins: dict[int, int] = {}
                floors: dict[int, int] = {}
                last = dict(offsets)
                for packet in reader.demux(*(reader.streams[i] for i in cloned)):
                    stamp = packet.pts if packet.dts is None else packet.dts  # decode order anchors; presentation stands in when dts is absent
                    if stamp is None:
                        continue
                    index = packet.stream.index
                    origin = origins.setdefault(index, stamp)  # first demuxed stamp is the stream's own zero
                    stamped = stamp - origin + offsets[index]
                    if stamped < floors.get(index, stamped):  # normalized stamps must stay monotonic per stream, or the mux is torn
                        return Error(MediaFault(invalid=f"concat source packets are not stamp-monotonic on stream {index}"))
                    floors[index] = stamped
                    # PTS and DTS re-base independently and an absent one stays None — a fabricated stamp breaks B-frame reorder
                    packet.stream = cloned[index]
                    packet.pts = None if packet.pts is None else packet.pts - origin + offsets[index]
                    packet.dts = None if packet.dts is None else packet.dts - origin + offsets[index]
                    last[index] = max(last[index], (packet.pts if packet.pts is not None else stamped) + (packet.duration or 0))
                    out.mux_one(packet)
                offsets = last
    blob = sink.getvalue()
    duration, count, bit_rate, measured = _probe(blob)
    with av.open(io.BytesIO(blob), mode="r") as delivered:
        codec = _lead_codec(delivered)
    facts = _deployment(profile) | {"clips": float(len(clips)), "strategy": "packet"}
    return Ok((blob, MediaEvidence.measure(profile.container, codec, duration, count, bit_rate, blob, facts | measured)))


def _filter_concat(clips: tuple[bytes, ...], profile: "MediaProfile") -> Result[tuple[bytes, "MediaEvidence"], "MediaFault"]:
    # every clip decodes on the `_video` rail and the decoded metadata is the admission evidence: all clips must
    # agree on frame rate and frame shape BEFORE the streams flatten — a rate mismatch mistimes the joined
    # timeline and a shape mismatch reaches numpy broadcasting instead of the typed rail.
    def _flattened(decoded: Block[tuple[int, "Frames"]], /) -> Result["Frames", "MediaFault"]:
        rates = {rate for rate, _ in decoded}
        shapes = {frames[0].shape for _, frames in decoded}
        if len(rates) > 1:
            return Error(MediaFault(invalid=f"concat sources disagree on frame rate: {sorted(rates)}"))
        if len(shapes) > 1:
            return Error(MediaFault(invalid=f"concat sources disagree on frame shape: {sorted(shapes)}"))
        return Ok(tuple(frame for _, frames in decoded for frame in frames))

    facts = frozendict({"clips": float(len(clips)), "strategy": "filter"})
    return (
        traverse(lambda clip: _video(clip, profile.hwaccel), Block.of_seq(clips))
        .bind(_flattened)
        .bind(lambda frames: traverse(_voice, Block.of_seq(clips)).bind(lambda voices: _concatenated(frames, clips, tuple(voices), profile, facts)))
    )


def _concatenated(
    frames: "Frames", clips: tuple[bytes, ...], voices: tuple[Option[Voice], ...], profile: "MediaProfile", facts: frozendict[str, float | str], /
) -> Result[tuple[bytes, "MediaEvidence"], "MediaFault"]:
    present = tuple(item for voice in voices for item in voice.to_list())
    if not present:
        return _sealed(frames, Nothing, profile, facts)
    template, rate, layout = present[0][0][0], present[0][1], present[0][2]
    return traverse(lambda pair: _pcm(pair[0], pair[1], template, rate, layout), Block.of_seq(zip(clips, voices, strict=True))).bind(
        lambda tracks: _sealed(frames, Some(((np.concatenate(tuple(tracks), axis=-1),), rate, layout)), profile, facts)
    )


def _segmented(cuts: tuple[float, ...], profile: "MediaProfile") -> Result["MediaProfile", "MediaFault"]:
    # loop-side derivation of the EFFECTIVE segment profile: `_crossed` classifies replayability off its argument
    # list, so the segmented container must be visible there — a worker-side derivation would leave the crossing
    # marked replayable and a worker-death retry would re-write the manifest and segment set.
    if profile.segment is None:
        return Error(MediaFault(invalid="segment op needs profile.segment (a UPath root); none supplied"))
    if not cuts or not all(isfinite(cut) and cut > 0.0 for cut in cuts) or any(left >= right for left, right in zip(cuts, cuts[1:], strict=False)):
        return Error(MediaFault(invalid="segment cuts must be a non-empty ascending sequence of finite positive seconds"))
    seg = profile.segment
    return Ok(
        msgspec.structs.replace(
            profile,
            container=ContainerFormat.SEGMENT,
            segment=msgspec.structs.replace(seg, options=seg.options | {"segment_times": ",".join(f"{c:.3f}" for c in cuts)}),
        )
    )


@_worker
def _segment(clip: bytes, cuts: tuple[float, ...], profile: "MediaProfile") -> Result[tuple[bytes, "MediaEvidence"], "MediaFault"]:
    # split IS the container SEGMENT sink: the caller passed the `_segmented`-derived profile, so the crossing's
    # replay probe already read the segmented container off the argument list.
    facts = frozendict({"clips": 1.0, "segments": float(len(cuts) + 1)})
    return _video(clip, profile.hwaccel).bind(
        lambda decoded: _encode_video(decoded[1], profile).map(lambda produced: (produced[0], _augment(produced[1], facts)))
    )


def _audio_xfade(under: "NDArray", over: "NDArray", window: int, /) -> "NDArray":
    # overlap-add: the under tail fades out as the over head fades in over `window` samples.
    ramp = np.linspace(1.0, 0.0, window, dtype=np.float64)
    mixed = under[..., -window:] * ramp + over[..., :window] * (1.0 - ramp)
    return np.concatenate([under[..., :-window], mixed.astype(under.dtype), over[..., window:]], axis=-1)


@_worker
def _xfade(
    under: bytes, over: bytes, duration: float, transition: Transition, profile: "MediaProfile"
) -> Result[tuple[bytes, "MediaEvidence"], "MediaFault"]:
    if not isfinite(duration) or duration <= 0.0:
        return Error(MediaFault(invalid=f"transition duration must be finite and positive, got {duration}"))
    return _video(under, profile.hwaccel).map2(_video(over, profile.hwaccel), lambda left, right: (left, right)).bind(
        lambda decoded: _blended(under, over, decoded[0], decoded[1], duration, transition, profile)
    )


def _blended(
    under: bytes,
    over: bytes,
    under_video: tuple[int, "Frames"],
    over_video: tuple[int, "Frames"],
    duration: float,
    transition: Transition,
    profile: "MediaProfile",
    /,
) -> Result[tuple[bytes, "MediaEvidence"], "MediaFault"]:
    rate, under_frames = under_video
    over_rate, over_frames = over_video
    if not under_frames or not over_frames:
        return Error(MediaFault(invalid="transition sources must each contain video frames"))
    # crossfade admission: BOTH rates and BOTH frame shapes participate — a rate mismatch would mistime the
    # overlap window and a shape mismatch would reach numpy broadcasting instead of the typed rail.
    if over_rate != rate:
        return Error(MediaFault(invalid=f"transition sources disagree on frame rate: {rate} vs {over_rate}"))
    if under_frames[0].shape != over_frames[0].shape:
        return Error(MediaFault(invalid=f"transition sources disagree on frame shape: {under_frames[0].shape} vs {over_frames[0].shape}"))
    window = max(1, round(duration * rate))
    match transition:
        case Transition.FADE:
            # filtergraph `wired` dissolve arm: the xfade node + the (under, over) pair discriminate the modality;
            # the requested window clamps to the shorter source exactly as the spatial arm does, so video, audio,
            # and the recorded fact all carry one effective overlap and xfade never outruns its inputs.
            span = min(window, len(under_frames), len(over_frames))
            joined = wired(FilterNode(xfade=(0.0, span / rate, transition.value)), (under_frames, over_frames), window=span)
        case spatial:
            span = min(window, len(under_frames), len(over_frames))
            h, w = under_frames[0].shape[:2]
            planes = (_MASK[spatial]((i + 1) / span, h, w)[..., None] for i in range(span))
            blended = tuple(
                (u.astype(np.float32) * (1.0 - mask) + o.astype(np.float32) * mask).astype(np.uint8)
                for u, o, mask in zip(under_frames[-span:], over_frames[:span], planes, strict=True)
            )
            joined = (*under_frames[:-span], *blended, *over_frames[span:])
    # `dissolve_frames` records the EFFECTIVE blended span — the spatial arm clamps the requested window to the
    # shorter source, so the fact carries what actually blended, never the request.
    facts = frozendict({"clips": 2.0, "dissolve_frames": float(span), "transition": transition.value})
    # the audio window derives from the EFFECTIVE video overlap `span / rate`, never the requested duration —
    # a clamped spatial span would otherwise crossfade audio longer than the frames that actually blended.
    return _voice(under).map2(_voice(over), lambda a, b: (a, b)).bind(
        lambda voices: _xfaded(joined, under, over, voices[0], voices[1], span / rate, profile, facts)
    )


def _xfaded(
    frames: "Frames",
    under: bytes,
    over: bytes,
    under_voice: Option[Voice],
    over_voice: Option[Voice],
    overlap: float,
    profile: "MediaProfile",
    facts: frozendict[str, float | str],
    /,
) -> Result[tuple[bytes, "MediaEvidence"], "MediaFault"]:
    # `overlap` is the effective blended span in seconds; the sample window still clamps to the shorter voice below.
    present = (*under_voice.to_list(), *over_voice.to_list())
    if not present:
        return _sealed(frames, Nothing, profile, facts)
    template, rate, layout = present[0][0][0], present[0][1], present[0][2]
    window = max(1, round(overlap * rate))
    return _pcm(under, under_voice, template, rate, layout).map2(
        _pcm(over, over_voice, template, rate, layout),
        lambda left, right: Some(((_audio_xfade(left, right, min(window, left.shape[-1], right.shape[-1])),), rate, layout)),
    ).bind(lambda voice: _sealed(frames, voice, profile, facts))


@_worker
def _speed(clip: bytes, factor: float, profile: "MediaProfile") -> Result[tuple[bytes, "MediaEvidence"], "MediaFault"]:
    # index-pick retime: >1 drops frames, <1 duplicates; audio rate-resamples by the same factor (pitch shifts with rate).
    if not isfinite(factor) or factor <= 0.0:
        return Error(MediaFault(invalid=f"speed factor must be positive, got {factor}"))
    facts = frozendict({"clips": 1.0, "factor": factor})
    return _video(clip, profile.hwaccel).bind(
        lambda decoded: _voice(clip).bind(lambda voice: _sped(decoded[1], voice, factor, profile, facts))
    )


def _sped(
    frames: "Frames", voice: Option[Voice], factor: float, profile: "MediaProfile", facts: frozendict[str, float | str], /
) -> Result[tuple[bytes, "MediaEvidence"], "MediaFault"]:
    picks = np.clip(np.round(np.arange(0, len(frames), factor)).astype(int), 0, len(frames) - 1)

    def retimed(item: Voice, /) -> Voice:
        samples = _joined(item[0])
        indices = np.clip(np.round(np.arange(0, samples.shape[-1], factor)).astype(int), 0, samples.shape[-1] - 1)
        return (samples[..., indices],), item[1], item[2]

    return _sealed(tuple(frames[index] for index in picks), voice.map(retimed), profile, facts)


@_worker
def _reverse(clip: bytes, profile: "MediaProfile") -> Result[tuple[bytes, "MediaEvidence"], "MediaFault"]:
    return _video(clip, profile.hwaccel).bind(
        lambda decoded: _voice(clip).bind(
            lambda voice: _sealed(
                tuple(reversed(decoded[1])),
                voice.map(lambda item: (((_joined(item[0])[..., ::-1]),), item[1], item[2])),
                profile,
                frozendict({"clips": 1.0}),
            )
        )
    )
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
