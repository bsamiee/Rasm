import { loadAndParseConfig } from '@cloudflare/config';
import type { Tree } from '@nx/devkit';
import { Array, Effect, Option, Path, Record, Schema, Struct } from 'effect';
import { CLOUDFLARE_CONFIG, glob } from '#/tools/nx/internal/open-workspace';
import { runPlugin } from '#/tools/nx/internal/plugin-runtime';

// --- [CONSTANTS] -----------------------------------------------------------------------

const HOSTING_FILE = '.openai/hosting.json';
const HOSTED_KINDS = ['d1', 'r2'] as const;

// --- [MODELS] --------------------------------------------------------------------------

const Hosting = Schema.fromJsonString(Schema.Record(Schema.String, Schema.Json), { space: 4 });

// --- [OPERATIONS] ----------------------------------------------------------------------

const hostedNames = (env: Record.ReadonlyRecord<string, { readonly type: string }>) =>
    Array.getSomes(
        HOSTED_KINDS.map((kind) =>
            Option.map(
                Record.findFirst(env, ({ type }) => type === kind),
                ([binding]) => [kind, binding] as const,
            ),
        ),
    );
const hostingRow = Effect.fnUntraced(function* (tree: Tree, file: string) {
    const path = yield* Path.Path;
    const text = yield* Effect.fromNullishOr(tree.read(file, 'utf-8'));
    const { result } = yield* Effect.promise(() => loadAndParseConfig(path.join(tree.root, file.slice(0, -HOSTING_FILE.length), CLOUDFLARE_CONFIG), { mode: 'production', isPreview: false }));
    const { worker } = result.success ? result.data : yield* Effect.fail(result.error);
    const next = yield* Schema.encodeEffect(Hosting)({ ...Object.fromEntries(hostedNames(worker?.env ?? {})), ...Struct.omit(yield* Schema.decodeEffect(Hosting)(text), HOSTED_KINDS) });
    return { file, text, next: `${next}\n` };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const hostingBindings = Effect.fnUntraced(function* (tree: Tree) {
    const rows = yield* Effect.forEach(yield* glob(tree.root, [`**/${HOSTING_FILE}`]), (file) => hostingRow(tree, file));
    const drifted = rows.filter(({ text, next }) => next !== text);
    yield* Effect.forEach(drifted, ({ file, next }) => Effect.sync(tree.write.bind(tree, file, next)), { discard: true });
    return Array.match(drifted, { onEmpty: () => undefined, onNonEmpty: (changed) => ({ outOfSyncMessage: `${HOSTING_FILE} binding names differ from ${CLOUDFLARE_CONFIG}`, outOfSyncDetails: changed.map(Struct.get('file')) }) });
}, runPlugin);

// --- [EXPORTS] -------------------------------------------------------------------------

export { hostingBindings };
