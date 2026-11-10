import type { ExecutorContext } from '@nx/devkit';
import { printed, runtime } from '@rasm/nx-devkit/process';
import { Array, Config, Effect, FileSystem, flow, Option, Order, Path, Schema, String } from 'effect';
import { isGreaterThan, sortReversed, tryParse } from 'verkit';
import { Composer, CST, Parser, Scalar, visit } from 'yaml';

// --- [CONSTANTS] -----------------------------------------------------------------------

const STAGES = ['alpha', 'beta', 'rc'] as const;
const UNTAGGED = /[@$]/u;
const CONTEXT = 'docker-image://';
const DOTTED = /^(?<numbers>\d+(?:\.\d+)+)(?<rest>.*)$/u;

// --- [MODELS] --------------------------------------------------------------------------

const Upgrade = Schema.Struct({ files: Schema.Array(Schema.String), dryRun: Schema.optionalKey(Schema.Boolean) });

// --- [OPERATIONS] ----------------------------------------------------------------------

const formed = (tag: string) =>
    Option.map(Option.fromNullOr(tryParse(tag)), ({ prerelease = [] }) => [tag.startsWith('v'), ...prerelease.flatMap((identifier) => (String.isString(identifier) ? identifier.split('-') : [])).filter((part) => Number.isNaN(Number(part)) && !STAGES.some((stage) => part.startsWith(stage)))].join(' '));

const dotted = (tag: string) => Option.map(Option.fromNullishOr(DOTTED.exec(tag)?.groups), ({ numbers = '', rest = '' }) => ({ parts: numbers.split('.').map(Number), rest }));

const byParts = Order.mapInput(Array.makeOrder(Order.Number), ({ parts }: { readonly parts: readonly number[] }) => parts);

const newest = (current: string, tags: readonly string[]) => {
    const semantic = Option.flatMap(formed(current), (form) => Array.head(sortReversed(tags.filter((tag) => Option.contains(formed(tag), form) && isGreaterThan(tag, current)))));
    const read = Array.getSomes(tags.map((tag) => Option.map(dotted(tag), (parsed) => ({ tag, ...parsed }))));
    const numeric = Option.flatMap(dotted(current), (stated) => {
        const later = read.filter((each) => each.rest === stated.rest && each.parts.length === stated.parts.length && byParts(each, stated) > 0);
        return Array.isReadonlyArrayNonEmpty(later) ? Option.some(Array.max(later, byParts).tag) : Option.none();
    });
    return Option.orElse(semantic, () => numeric);
};

const upgraded = Effect.fnUntraced(function* (file: string, dry: boolean) {
    const fs = yield* FileSystem.FileSystem;
    const tokens = [...new Parser().parse(yield* fs.readFileString(file))];
    const images = yield* Effect.sync(() => {
        const found: { readonly node: Scalar; readonly text: string }[] = [];
        new Composer({ keepSourceTokens: true }).compose(tokens).forEach((document) =>
            visit(document, {
                Pair: (_, { key, value }) => {
                    if (value instanceof Scalar && String.isString(value.value) && (value.value.startsWith(CONTEXT) || (key instanceof Scalar && key.value === 'image'))) {
                        found.push({ node: value, text: value.value });
                    }
                },
            }),
        );
        return found;
    });
    const stated = Array.getSomes(
        images.map(({ node, text }) => {
            const at = text.lastIndexOf(':');
            return Option.liftPredicate({ node, repository: text.slice(0, at), tag: text.slice(at + 1) }, () => at > text.lastIndexOf('/') && !UNTAGGED.test(text));
        }),
    );
    const pulled = (repository: string) => (repository.startsWith(CONTEXT) ? repository.slice(CONTEXT.length) : repository);
    const listed = yield* Effect.forEach(Array.dedupe(stated.map(({ repository }) => pulled(repository))), (repository) => Effect.map(printed('crane', ['ls', repository]), (list) => [repository, list.split('\n').filter(String.isNonEmpty)] as const), { concurrency: 8 });
    const tags = new Map(listed);
    const moves = Array.getSomes(stated.map(({ node, repository, tag }) => Option.map(newest(tag, tags.get(pulled(repository)) ?? []), (next) => ({ node, repository, tag, next }))));
    yield* Effect.forEach(
        stated.filter(({ tag }) => Option.isNone(formed(tag)) && Option.isNone(dotted(tag))),
        ({ repository, tag }) => Effect.logInfo(`Kept ${repository}:${tag} in ${file}, its tag states no version to order`),
        { discard: true },
    );
    yield* Effect.forEach(moves, ({ repository, tag, next }) => Effect.logInfo(`${dry ? 'Would move' : 'Moved'} ${repository} ${tag} -> ${next} in ${file}`), { discard: true });
    yield* Effect.sync(() => moves.forEach(({ node, repository, next }) => (node.srcToken === undefined ? undefined : CST.setScalarValue(node.srcToken, `${repository}:${next}`))));
    return yield* dry ? Effect.void : fs.writeFileString(file, tokens.map((token) => CST.stringify(token)).join(''));
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = flow(
    Effect.fnUntraced(function* (options: unknown, context: ExecutorContext) {
        const [path, { files, dryRun }, dry] = yield* Effect.all([Path.Path, Schema.decodeUnknownEffect(Upgrade)(options), Config.Boolean('NX_DRY_RUN').pipe(Config.withDefault(false))]);
        yield* Effect.forEach(files, (file) => upgraded(path.join(context.root, file), dryRun ?? dry), { discard: true });
        return { success: true };
    }),
    runtime.runPromise,
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
