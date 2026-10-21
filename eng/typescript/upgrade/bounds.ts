import { Array, Effect, FileSystem, Option, Order, Record, Schema } from 'effect';
import { compare } from 'verkit';
import { stdout } from '../process.ts';

// --- [MODELS] --------------------------------------------------------------------------

const Span = Schema.Struct({ start: Schema.Int, end: Schema.Int });
const Capture = Schema.Struct({ text: Schema.String, range: Schema.Struct({ byteOffset: Span }) });

// --- [ERRORS] --------------------------------------------------------------------------

class NoRelease extends Schema.TaggedError<NoRelease, { readonly brand: unique symbol }>()('NoRelease', { url: Schema.String }) {
    override get message(): string {
        return `${this.url} lists no release tag`;
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const newest = Effect.fn('newest')(function* (url: string, parse: (tag: string) => Option.Option<string>) {
    const tags = (yield* stdout('git', ['ls-remote', '--tags', '--refs', url])).split('\n').flatMap((line) => Option.toArray(Option.flatMap(Option.fromUndefinedOr(line.split('refs/tags/')[1]), parse)));
    return yield* Array.match(tags, { onEmpty: () => Effect.fail(NoRelease.make({ url })), onNonEmpty: (all) => Effect.succeed(Array.max(all, compare)) });
});
const scan = Effect.fn('scan')(function* <Id extends string, Single extends Schema.Struct.Fields>(rules: Record.ReadonlyRecord<Id, string>, single: Single, args: readonly string[]) {
    const text = yield* stdout('ast-grep', [
        'scan',
        '--inline-rules',
        Object.entries(rules)
            .map(([id, rule]) => `{id: ${id}, severity: info, ${rule}}`)
            .join('\n---\n'),
        '--json=compact',
        ...args,
    ]);
    return yield* Schema.decodeEffect(Schema.fromJsonString(Schema.Array(Schema.Struct({ file: Schema.String, ruleId: Schema.Literals(Record.keys(rules)), metaVariables: Schema.Struct({ single: Schema.Struct(single) }) }))))(text);
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

export { Capture, newest, scan, splice };
