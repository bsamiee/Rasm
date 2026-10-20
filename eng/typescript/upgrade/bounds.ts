import { Array, Effect, FileSystem, Option, Order, Schema } from 'effect';
import { compare } from 'verkit';
import { stdout } from '../process.ts';

// --- [MODELS] --------------------------------------------------------------------------

const Span = Schema.Struct({ start: Schema.Int, end: Schema.Int });
const Capture = Schema.Struct({ text: Schema.String, range: Schema.Struct({ byteOffset: Span }) });

// --- [OPERATIONS] ----------------------------------------------------------------------

const newest = Effect.fn('newest')(function* (url: string, keep: (tag: string) => boolean) {
    const tags = (yield* stdout('git', ['ls-remote', '--tags', '--refs', url]))
        .split('\n')
        .flatMap((line) => Option.toArray(Option.fromUndefinedOr(line.split('refs/tags/')[1])))
        .filter(keep);
    return Array.match(tags, { onEmpty: Option.none, onNonEmpty: (all) => Option.some(Array.max(all, compare)) });
});
const splice = Effect.fn('splice')(function* (file: string, edits: ReadonlyArray<readonly [typeof Span.Type, string]>) {
    const fs = yield* FileSystem.FileSystem;
    const bytes = Buffer.from(yield* fs.readFile(file));
    yield* fs.writeFile(
        file,
        Array.sortWith(edits, ([{ start }]) => start, Order.flip(Order.Number)).reduce((text, [{ start, end }, value]) => Buffer.concat([text.subarray(0, start), Buffer.from(value), text.subarray(end)]), bytes),
    );
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Capture, newest, splice };
