import { Effect, Schema } from 'effect';
import { Element } from '#/server/element';
import { Event } from '#/server/event';
import { ApplicationFailure } from '#/server/failure';
import { Instance } from '#/server/instance';

// --- [MODELS] --------------------------------------------------------------------------

const Frame = Schema.Struct({
    png: Schema.Uint8Array,
    width: Schema.Int,
    height: Schema.Int,
    frame: Element.Frame,
    windows: Schema.Array(Element.WindowNumber),
    backingStore: Schema.Boolean,
    document: Schema.optionalKey(Schema.String),
    presentation: Schema.optionalKey(Instance.Presentation),
    at: Schema.optionalKey(Schema.DateTimeUtcFromString),
    changed: Schema.optionalKey(Element.Frame),
    markers: Schema.Union([Schema.TaggedStruct('step', { index: Schema.Int, at: Schema.DateTimeUtcFromString }), Schema.TaggedStruct('event', Event.Changed.fields)]).pipe(Schema.Array, Schema.optionalKey),
});
class Capture extends Schema.Opaque<Capture>()(
    Schema.Struct({
        source: Schema.TaggedUnion({ windows: { numbers: Schema.NonEmptyArray(Element.WindowNumber) }, element: { ref: Element.Ref }, region: { frame: Element.Frame } }),
        longestSide: Schema.OptionFromOptionalKey(Schema.Int.check(Schema.isGreaterThan(0))),
        mode: Schema.TaggedUnion({ still: { marks: Schema.Array(Schema.Struct({ frame: Element.Frame, label: Schema.String })).pipe(Schema.withConstructorDefault(Effect.succeed([]))) }, recording: { keyFrameLimit: Schema.Int.check(Schema.isGreaterThan(0)), duration: Schema.DurationFromMillis, movie: Schema.optionalKey(Schema.URL) } }),
    }),
) {
    static readonly Frame = Frame;
    static readonly Frames = Schema.Array(ApplicationFailure.read(Frame));
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Capture };
