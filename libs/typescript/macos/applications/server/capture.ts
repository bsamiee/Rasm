import { Schema } from 'effect';
import { Element } from '#/server/element';
import { Event } from '#/server/event';
import { Instance } from '#/server/instance';

// --- [MODELS] --------------------------------------------------------------------------

const Screenshot = Schema.Struct({
    png: Schema.Uint8Array,
    width: Schema.Int,
    height: Schema.Int,
    frame: Element.Frame,
    windows: Schema.Array(Element.WindowNumber),
    backingStore: Schema.Boolean,
    document: Schema.optionalKey(Schema.String),
    presentation: Schema.optionalKey(Instance.Presentation),
});
class Capture extends Schema.Opaque<Capture>()(
    Schema.Struct({
        source: Schema.TaggedUnion({ windows: { numbers: Schema.NonEmptyArray(Element.WindowNumber) }, element: { ref: Element.Ref }, region: { frame: Element.Frame } }),
        longestSide: Schema.OptionFromOptionalKey(Schema.Int.check(Schema.isGreaterThan(0))),
    }),
) {
    static readonly Screenshot = Screenshot;
    static readonly Stills = Schema.Array(Schema.Result(Screenshot, Schema.ErrorInstance()));
    static readonly Recording = Schema.Struct({ ...Capture.fields, frames: Schema.Int.check(Schema.isGreaterThan(0)) });
    static readonly Recorded = Schema.Struct({
        keyFrames: Schema.Array(
            Schema.Struct({
                at: Schema.DateTimeUtcFromString,
                changed: Schema.optionalKey(Element.Frame),
                screenshot: Screenshot,
                markers: Schema.Array(Schema.Union([Schema.TaggedStruct('step', { index: Schema.Int, at: Schema.DateTimeUtcFromString }), Schema.TaggedStruct('event', Event.fields)])),
            }),
        ),
        presentation: Schema.optionalKey(Instance.Presentation),
    });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Capture };
