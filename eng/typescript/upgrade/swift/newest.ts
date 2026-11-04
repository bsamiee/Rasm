import { Array, Cause, Effect, flow, Option, Schema, Tuple } from 'effect';
import { compare } from 'verkit';
import { runCommand } from '#/eng/typescript/run-command';

// --- [MODELS] --------------------------------------------------------------------------

const Ref = Schema.TemplateLiteralParser([Schema.String, '\trefs/tags/', Schema.String]);

// --- [OPERATIONS] ----------------------------------------------------------------------

const newest = Effect.fn('newest')(function* (url: string, parse: (tag: string) => Option.Option<string>) {
    const tags = Array.getSomes(Array.map(yield* runCommand.records('\n', 'git', ['ls-remote', '--tags', '--refs', url]), (line) => Option.flatMap(Schema.decodeUnknownOption(Ref)(line), flow(Tuple.get(2), parse))));
    const all = yield* Effect.fromOption(Option.liftPredicate(tags, Array.isArrayNonEmpty), () => new Cause.NoSuchElementError(`${url} lists no release`));
    return Array.max(all, compare);
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { newest };
