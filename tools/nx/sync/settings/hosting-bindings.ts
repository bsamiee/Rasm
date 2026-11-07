import { loadAndParseConfig } from '@cloudflare/config';
import { globAsync, type Tree } from '@nx/devkit';
import { Cause, Effect, Path, Record, Schema, Struct } from 'effect';
import { syncGenerator } from '#/sync/tree.ts';
import { CLOUDFLARE_CONFIG } from '#/workspace.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const HOSTING_FILE = '.openai/hosting.json';
const HOSTED_KINDS = ['d1', 'r2'] as const;

// --- [MODELS] --------------------------------------------------------------------------

const Hosting = Schema.fromJsonString(Schema.Record(Schema.String, Schema.Json), { space: 4 });

// --- [OPERATIONS] ----------------------------------------------------------------------

const hostedNames = (file: string, env: Record.ReadonlyRecord<string, { readonly type: string }>) =>
    Effect.validate(HOSTED_KINDS, (kind) => {
        const names = Object.keys(Record.filter(env, ({ type }) => type === kind));
        return names.length <= 1 ? Effect.succeed([kind, names[0] ?? null] as const) : Effect.fail(new Cause.IllegalArgumentError(`${file} needs one ${kind} binding, found ${names.join(', ')}`));
    }).pipe(Effect.mapError((errors) => new AggregateError(errors, errors.map(({ message }) => message).join('\n'))));
const hostingRow = Effect.fnUntraced(function* (tree: Tree, file: string) {
    const path = yield* Path.Path;
    const text = yield* Effect.fromNullishOr(tree.read(file, 'utf-8'));
    const { result } = yield* Effect.promise(() => loadAndParseConfig(path.join(tree.root, file.slice(0, -HOSTING_FILE.length), CLOUDFLARE_CONFIG), { mode: 'production', isPreview: false }));
    const { worker } = result.success ? result.data : yield* Effect.fail(result.error);
    const [names, current] = yield* Effect.all([hostedNames(file, worker?.env ?? {}), Schema.decodeEffect(Hosting)(text)], { concurrency: 'unbounded' });
    const next = yield* Schema.encodeEffect(Hosting)({ ...Object.fromEntries(names), ...Struct.omit(current, HOSTED_KINDS) });
    return { file, next: `${next}\n` };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const hostingBindings = Object.assign(
    syncGenerator(`${HOSTING_FILE} binding names differ from ${CLOUDFLARE_CONFIG}`, (tree) =>
        Effect.flatMap(
            Effect.tryPromise(() => globAsync(tree, [`**/${HOSTING_FILE}`])),
            Effect.forEach((file) => hostingRow(tree, file)),
        ),
    ),
    { files: [`**/${HOSTING_FILE}`] },
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { hostingBindings };
