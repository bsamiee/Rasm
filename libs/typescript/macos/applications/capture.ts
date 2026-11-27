import { Array, DateTime, Schema } from 'effect';
import { Event } from '#/instances';
import { Frame, Instant, Presentation, Ref, WindowID } from '#/interface';
import { Outline } from '#/outline';

// --- [MODELS] --------------------------------------------------------------------------

const LongestSide = Schema.Int.check(Schema.isGreaterThan(0)).annotate({ description: 'Longest side of an image in pixels, never upscaled' });
class Capture extends Schema.Opaque<Capture>()(
    Schema.Struct({
        source: Schema.TaggedUnion({
            windows: { numbers: Schema.NonEmptyArray(WindowID).annotate({ description: 'Each window an image of its own with its child windows' }) },
            element: { ref: Ref.annotate({ description: 'Control or container cropped from its window, with the windows of the application above it' }) },
            region: { frame: Frame.annotate({ description: 'Screen region the windows of the application show' }) },
        }),
        longestSide: Schema.OptionFromOptionalKey(LongestSide.annotate({ description: 'Longest side in pixels of each image, never upscaled, full resolution when absent' })),
    }),
) {
    static readonly Screenshot = Schema.Struct({
        png: Schema.Uint8Array.annotate({ description: 'PNG image' }),
        width: Schema.Int.annotate({ description: 'Image width in pixels' }),
        height: Schema.Int.annotate({ description: 'Image height in pixels' }),
        frame: Frame.annotate({ description: 'Screen area the image shows' }),
        windows: Schema.Array(WindowID).annotate({ description: 'Windows the image composites' }),
        backingStore: Schema.Boolean.annotate({
            description: 'The image is the backing store the window server keeps for a window off screen (hidden, minimized, or on another Space), its last drawn content read through CGSHWCaptureWindowList in place of ScreenCaptureKit',
        }),
        document: Schema.optionalKey(Schema.String.annotate({ description: 'AXDocument file URL of the first window showing one' })),
        presentation: Schema.optionalKey(Presentation.annotate({ description: 'How the menus the image shows were opened for it, absent when it opened none' })),
    });
    static readonly Result = Schema.Array(Schema.Result(Capture.Screenshot, Schema.ErrorInstance())).annotate({ description: 'One image per window of a windows source in its order, else one' });
}
class Recording extends Schema.Opaque<Recording>()(
    Schema.Struct({
        ...Capture.fields,
        frames: Schema.Int.check(Schema.isGreaterThan(0)).annotate({ description: 'Most key frames kept: 1 the last state, 2 the first and last, more each state the source settled in for a beat between them, the smallest change merged into the next where more settle' }),
    }),
) {
    static readonly Marker = Schema.Union([Schema.TaggedStruct('step', { index: Schema.Int.annotate({ description: 'Step of the act call, counted from 0' }), at: Instant.annotate({ description: 'When the step began' }) }), Schema.TaggedStruct('event', Event.fields)]).annotate({
        description: 'Step begun or instance event read after a key frame appeared and before the next',
    });
    static readonly Result = Schema.Struct({
        keyFrames: Schema.Array(
            Schema.Struct({
                at: Instant.annotate({ description: 'When the source first showed the state, on the clock of event and read times' }),
                changed: Schema.optionalKey(Frame.annotate({ description: 'Screen area that differs from the key frame before, absent on the first' })),
                screenshot: Capture.Screenshot.annotate({ description: 'The whole source on the first and last key frames, the changed area alone on those between' }),
                markers: Schema.Array(Recording.Marker).annotate({ description: 'Steps begun and instance events read from when this state appeared until the next did, in order' }),
            }),
        ).annotate({
            description:
                'The state when recording began, each state the source settled in, and the last, kept to the frames asked for, the windows the application opened meanwhile included; a state the source left and returned to with no step or event between is dropped, and a frame showing what the one before shows is merged into it',
        }),
        presentation: Schema.optionalKey(Presentation.annotate({ description: 'How the menus the frames show were opened for them, absent when they opened none' })),
    });
    static readonly leads = ({ keyFrames }: typeof Recording.Result.Type) =>
        Array.match(keyFrames, {
            onEmpty: Array.empty,
            onNonEmpty: (frames) => {
                const first = Array.headNonEmpty(frames);
                const [[x, y], [across, down]] = first.screenshot.frame;
                const begun = frames.map(({ markers }) => markers.flatMap((marker) => (marker._tag === 'step' ? [marker.index + 1] : [])));
                return frames.map(({ at, changed }, index) => [
                    index === 0 ? 'at the start' : `${Outline.duration(DateTime.distance(first.at, at))} in`,
                    ...Array.match(begun[index - 1] ?? [], { onEmpty: Array.empty, onNonEmpty: (listed) => [`after ${Outline.list(listed.map((step) => `step ${step}`))}`] }),
                    ...Array.fromNullishOr(changed).map(([[left, top], [wide, high]]) => `changed [${[((left - x) * first.screenshot.width) / across, ((top - y) * first.screenshot.height) / down, (wide * first.screenshot.width) / across, (high * first.screenshot.height) / down].map(Math.round).join(',')}]`),
                    ...Array.match(index === frames.length - 1 ? (begun[index] ?? []) : [], { onEmpty: Array.empty, onNonEmpty: (listed) => [`then ${Outline.list(listed.map((step) => `step ${step}`))}, which changed nothing shown`] }),
                ]);
            },
        });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Capture, LongestSide, Recording };
