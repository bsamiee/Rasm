import { Array, Effect, Option, Schema } from 'effect';
import { compare } from 'verkit';
import { records } from '#/eng/typescript/process';

// --- [MODELS] --------------------------------------------------------------------------

const Ref = Schema.TemplateLiteralParser([Schema.String, '\trefs/tags/', Schema.String]);

// --- [ERRORS] --------------------------------------------------------------------------

class NoRelease extends Schema.TaggedError<NoRelease, { readonly brand: unique symbol }>()('NoRelease', { subject: Schema.String }) {
    override get message(): string {
        return `${this.subject} lists no release`;
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const newest = Effect.fn('newest')(function* (url: string, parse: (tag: string) => Option.Option<string>) {
    const tags = Array.getSomes(Array.map(yield* records('\n', 'git', ['ls-remote', '--tags', '--refs', url]), (line) => Option.flatMap(Schema.decodeUnknownOption(Ref)(line), ([, , tag]) => parse(tag))));
    return yield* Array.match(tags, { onEmpty: () => Effect.fail(NoRelease.make({ subject: url })), onNonEmpty: (all) => Effect.succeed(Array.max(all, compare)) });
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { NoRelease, newest };
