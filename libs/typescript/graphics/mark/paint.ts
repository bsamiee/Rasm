import { Resvg } from '@resvg/resvg-js';
import { load } from 'cheerio';
import { Cause, Effect, FileSystem } from 'effect';
import type { Fill } from '#/index';

// --- [OPERATIONS] ----------------------------------------------------------------------

const paint = Effect.fnUntraced(function* (master: string, { color, opacity = 1 }: Fill, width?: number) {
    const fs = yield* FileSystem.FileSystem;
    const document = load(yield* fs.readFileString(master), { xml: true });
    document
        .root()
        .children('svg')
        .attr({ color, opacity: `${opacity}` });
    const svg = yield* Effect.try({ try: () => new Resvg(document.xml(), { fitTo: width === undefined ? { mode: 'original' } : { mode: 'width', value: width } }), catch: (cause) => new Cause.UnknownError(cause, `resvg refused ${master}`) });
    return width === undefined ? new TextEncoder().encode(svg.toString()) : svg.render().asPng();
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { paint };
