import { Resvg } from '@resvg/resvg-js';
import { load } from 'cheerio';
import { Effect, FileSystem, Formatter, Schema } from 'effect';

// --- [ERRORS] --------------------------------------------------------------------------

class MarkError extends Schema.TaggedError<MarkError, { readonly brand: unique symbol }>()('MarkError', {
    reason: Schema.TaggedUnion({ Unreadable: {} }),
    subject: Schema.String,
    cause: Schema.optionalKey(Schema.Defect()),
    message: Schema.String,
}) {
    static readonly of = (reason: MarkError['reason']['_tag'], subject: string) => (cause?: unknown) => MarkError.make({ reason: { _tag: reason }, subject, ...(cause === undefined ? {} : { cause }), message: [`${reason}: ${subject}`, ...(cause === undefined ? [] : [Formatter.format(cause)])].join(': ') });
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const paint = Effect.fnUntraced(function* (master: string, { color, opacity = 1 }: { readonly color: string; readonly opacity?: number | undefined }, width?: number) {
    const fs = yield* FileSystem.FileSystem;
    const document = load(yield* fs.readFileString(master), { xml: true });
    document
        .root()
        .children('svg')
        .attr({ color, opacity: `${opacity}` });
    const svg = yield* Effect.try({ try: () => new Resvg(document.xml(), { fitTo: width === undefined ? { mode: 'original' } : { mode: 'width', value: width } }), catch: MarkError.of('Unreadable', master) });
    return width === undefined ? new TextEncoder().encode(svg.toString()) : svg.render().asPng();
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { MarkError, paint };
