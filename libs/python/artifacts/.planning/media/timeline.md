# [PY_ARTIFACTS_MEDIA_TIMELINE]

`Timeline` is the non-linear-editing layer on top of the container/filtergraph spine — the owner over a closed `TimelineOp` family (`Trim`/`Concat`/`Segment`/`Xfade`) that assembles multiple content-keyed clips into one deliverable. `Timeline` opens no container and implements no filter of its own: every arm composes the `media/container#CONTAINER` capsule primitives (decode/encode, `_seek` random-access, the segmented sink) and the `media/filtergraph#FILTER` builders (`cross_dissolve`, `link_clips`), so the NLE modality is a projection over the spine, never a second FFmpeg surface. A multi-clip timeline is a DAG — each `Clip` carries its own parent `ContentKey`, and `Timeline.parents` exposes them so the `core/plan#PLAN` `ArtifactPipeline` schedules the constituent clip producers as upstream `ArtifactWork` nodes and elides an already-rendered clip on a warm replay: the media plane's strongest CPM exemplar, a five-clip concat planning as a five-parent front the planner drives concurrently before the join.

`Concat` is one polymorphic arm over two verified strategies derived from source-parameter match, never a knob — `_packet_concat` re-stamps every packet onto a monotonic timeline over one cloned stream (bit-exact, no re-decode) when every clip shares codec/resolution/pix_fmt/time_base, else `_filter_concat` decodes and re-encodes the joined frame stream. `Trim` seeks to the keyframe at/before the in-point and re-encodes the `[in, out)` window zero-based; `Segment` composes the container `SEGMENT` sink with `segment_times` at the cut boundaries; `Xfade` cross-dissolves the overlapping window through `media/filtergraph#FILTER` `cross_dissolve`, the numpy ramp the arm because the native `xfade` filter refuses in-process `configure()`. `TimelineFault` reuses `media/container#CONTAINER`'s `MediaFault` — one media cause rail across the plane. This page owns the `TimelineOp` family, the `Clip` value, the `Timeline.parents` DAG projection, and the `_trim`/`_concat`/`_packet_concat`/`_filter_concat`/`_segment`/`_xfade` workers; it composes the container capsule, the filtergraph builders, and `media/audio#MEDIA` `_decode_audio`, contributing the shared `core/receipt#RECEIPT` `ArtifactReceipt.Media` case whose `facts` band carries the clip and segment counts.

## [01]-[INDEX]

- [01]-[TIMELINE]: the `Timeline` owner over the closed `TimelineOp` family — `Trim`/`Concat`/`Segment`/`Xfade` as content-keyed DAG nodes projecting over the container capsule and filtergraph builders, folding into the shared `ArtifactReceipt.Media` case.

## [02]-[TIMELINE]

- Owner: `Timeline` discriminates modality over the closed `TimelineOp` union, each case carrying its own typed payload — never a shared erased bag, a per-op subclass, or a parallel `trim`/`concat`/`xfade` trio. `Clip` carries `data: bytes` and `key: ContentKey` (the parent artifact the producer minted), so a clip IS a content-keyed node and an op's dependency IS its clips' keys, never a path or a re-minted key. `ConcatStrategy` is not a field — the packet-versus-filter choice is derived once from `_params(clip)` equality across the clips, the discriminant recoverable from the clip streams, so a `lossless: bool` knob the body re-derives is the rejected form. `TimelineFault` reuses `media/container#CONTAINER`'s `MediaFault`, never a parallel rail; the `MediaProfile`/`MediaEvidence`/`ContainerFormat`/`SegmentSpec` family is read, never re-declared.
- Cases: `Trim` re-encodes zero-based — the fresh index-derived `pts` the shared `_drive` stamps IS the `setpts=PTS-STARTPTS` zero-base, no filter needed. `Segment`'s split IS the spine's segmented encode, not a re-implemented muxer. `Xfade` uses the `cross_dissolve` numpy ramp because the native `xfade` filter refuses in-process wiring; its audio leg composing `media/audio#MEDIA` `_decode_audio` + a `filtergraph` `acrossfade` is a growth axis.
- Entry: `Timeline.parents` is the pure projection of the op's clip `ContentKey`s the composition root reads to wire `ArtifactWork.parents`, so a five-clip concat plans as a five-parent front. Each `@beartype`-woven worker composes the spine's `_decode_*`/`_encode_video`/`_seek` primitives (which own the `av.open` capsule and the `FFmpegError` -> `_media_fault` capture), so a timeline worker adds no second `try`/`except FFmpegError` beyond the composed primitives' own.
- Auto: the `Concat` strategy is derived once by `_params(clip.data)` — all clips equal routes `_packet_concat`, any mismatch `_filter_concat`, the discriminant the clip streams themselves; `_packet_concat` advances the offset by each clip's last `pts + duration` so the joined timeline stays monotonic. Every re-encode folds the shared deployment facts plus the timeline-specific clip/segment counts through `MediaEvidence` once via `_augment`.
- Receipt: each op keys through `_keyed` over the assembled bytes and adds its timeline facts to the shared `Media` band — `Trim` `{clips, trimmed_seconds}`, `Concat` `{clips, strategy}`, `Segment` `{clips, segments}`, `Xfade` `{clips, dissolve_frames}` — one `ArtifactReceipt.Media` case across the whole plane, never a parallel timeline rail; the `strategy` fact makes the lossless-versus-reencode decision observable evidence off one structured-log line.
- Packages: `av` supplies the demux/seek/re-stamp/mux capsule, reached only through the `media/container#CONTAINER` primitives — timeline opens no container. Members settled against the folder `.api`.
- Growth: a new NLE operation is one `TimelineOp` case plus one `_mux` arm plus one worker composing the spine primitives, `assert_never` breaking the dispatch until the arm exists; a new concat strategy one `_params` axis and one branch; a new transition one `transition` value the `cross_dissolve` ramp reads; a nested timeline is one `Clip` whose `key` is the nested `emit()` product, so the DAG nests without a new surface; a new evidence fact one band key (ZERO receipt edit); the audio cross-fade leg one `_xfade` growth composing `media/audio#MEDIA` `_decode_audio` + `media/filtergraph#FILTER` `acrossfade`.

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
)  # the shared spine family + the transient-death retry policy, imported not re-minted

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
        # the upstream clip keys the ArtifactPipeline wires as this node's parents; a warm replay elides an already-rendered clip.
        return tuple(clip.key for clip in self.op.clips)

    def emit(self, /) -> ArtifactWork:
        return ArtifactWork(key=self._key, work=self._emit, parents=(), admission=Admission(keyed=None), cost=1.0)

    @property
    def _key(self) -> ContentKey:
        # key over the INPUT op minted pre-run; the output's own address rides the receipt facts.
        return ContentIdentity.of(f"timeline-{self.op.tag}", self.op, policy=CANONICAL_POLICY)

    async def _emit(self) -> RuntimeRail[ArtifactReceipt]:
        # the member MediaFault folds into the boundary fault (Work[ArtifactReceipt] forbids an inner Result).
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

# workers stay module-level for qualified-name dispatch; every av capsule op is reached THROUGH the container spine — timeline opens no container.
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
    # the concat-strategy discriminant: the stream identity a lossless packet-copy requires equal; a mismatch routes the filter path.
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
    # lossless join over ONE owned writer: re-stamp each source's packets onto a monotonic offset (valid because `_params`
    # proved a shared time_base) — no decode, bit-exact; the offset advances by each clip's last pts+duration.
    try:
        sink = io.BytesIO()
        with av.open(sink, mode="w", format=profile.container.value, container_options=dict(profile.container_options)) as out:
            with av.open(io.BytesIO(clips[0]), mode="r") as head:
                stream = out.add_stream_from_template(head.streams.best("video"))
            offset = 0
            for clip in clips:  # Exemption: imperative packet re-stamp over one owned muxer handle
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
    # ONE polymorphic arm, the strategy DERIVED from clip-param equality.
    try:
        return _packet_concat(clips, profile) if len({_params(clip) for clip in clips}) == 1 else _filter_concat(clips, profile)
    except av.error.FFmpegError as exc:
        return Error(_media_fault("concat", exc))


@beartype
def _segment(clip: bytes, cuts: tuple[float, ...], profile: MediaProfile) -> Result[tuple[bytes, MediaEvidence], MediaFault]:
    # the split IS the container SEGMENT sink: a profile whose SegmentSpec carries segment_times.
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
    # cross-dissolve the overlapping window through the filtergraph substitute (native xfade refuses in-process configure).
    under_frames, rate = _decode_video(under)
    over_frames, _ = _decode_video(over)
    window = max(1, round(duration * rate))
    joined = cross_dissolve(under_frames, over_frames, window)  # filtergraph: under[-window:] fades out under over[:window]
    facts = frozendict({"clips": 2.0, "dissolve_frames": float(window), "transition": transition})
    return _encode_video(joined, profile).map(lambda produced: (produced[0], _augment(produced[1], facts)))
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
