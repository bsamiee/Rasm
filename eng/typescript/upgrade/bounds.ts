import { Array, Effect, Option, Record, Schema } from 'effect';
import { compare } from 'verkit';
import { stdout } from '#/eng/typescript/process';

// --- [MODELS] --------------------------------------------------------------------------

const Capture = Schema.Struct({ text: Schema.String });

// --- [ERRORS] --------------------------------------------------------------------------

class NoRelease extends Schema.TaggedError<NoRelease, { readonly brand: unique symbol }>()('NoRelease', { url: Schema.String }) {
    override get message(): string {
        return `${this.url} lists no release tag`;
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const newest = Effect.fn('newest')(function* (url: string, parse: (tag: string) => Option.Option<string>) {
    const tags = Array.getSomes((yield* stdout('git', ['ls-remote', '--tags', '--refs', url])).split('\n').map((line) => Option.flatMap(Option.fromUndefinedOr(line.split('refs/tags/')[1]), parse)));
    return yield* Array.match(tags, { onEmpty: () => Effect.fail(NoRelease.make({ url })), onNonEmpty: (all) => Effect.succeed(Array.max(all, compare)) });
});
const inline = (rules: Record.ReadonlyRecord<string, string>) => Record.collect(rules, (id, rule) => `{id: ${id}, severity: info, ${rule}}`).join('\n---\n');
const scan = Effect.fn('scan')(function* <Single extends Schema.Struct.Fields>(rules: Record.ReadonlyRecord<string, string>, single: Single, args: readonly string[]) {
    const text = yield* stdout('ast-grep', ['scan', '--inline-rules', inline(rules), '--json=compact', ...args]);
    return yield* Schema.decodeEffect(Schema.fromJsonString(Schema.Array(Schema.Struct({ metaVariables: Schema.Struct({ single: Schema.Struct(single) }) }))))(text);
});
const rewrite = (rules: Record.ReadonlyRecord<string, string>, args: readonly string[]) => stdout('ast-grep', ['scan', '--inline-rules', inline(rules), '--update-all', ...args]);

// --- [EXPORTS] -------------------------------------------------------------------------

export { Capture, newest, rewrite, scan };
