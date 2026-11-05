import { loadAndParseConfig } from '@cloudflare/config';
import type { Tree } from '@nx/devkit';
import { Array, Effect, Option, Path, Record, Schema, Struct } from 'effect';
import { CLOUDFLARE_CONFIG, glob } from '#/tools/nx/internal/open-workspace';
import { syncGenerator } from '#/tools/nx/internal/plugin-runtime';

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
    return { file, next: `${next}\n` };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const hostingBindings = syncGenerator(`${HOSTING_FILE} binding names differ from ${CLOUDFLARE_CONFIG}`, (tree) =>
    Effect.flatMap(
        glob(tree.root, [`**/${HOSTING_FILE}`]),
        Effect.forEach((file) => hostingRow(tree, file)),
    ),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { hostingBindings };
