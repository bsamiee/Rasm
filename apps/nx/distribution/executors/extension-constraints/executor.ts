import { patch } from '@decimalturn/toml-patch';
import { layer } from '@effect/platform-node/NodeServices';
import type { ExecutorContext } from '@nx/devkit';
import { Array, Effect, FileSystem, Order, Path, Record, Schema, String } from 'effect';
import { probed, toml } from '#/executors/extension-build/executor';
import { DistributionError } from '#/plugins/host-packages';

// --- [CONSTANTS] -----------------------------------------------------------------------

const SEPARATORS = /[-_.]+/gu;

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({ manifest: Schema.String });
const Table = Schema.Record(Schema.String, Schema.Unknown);
const Rows = Schema.optionalKey(Schema.Array(Schema.String));
const Root = Schema.StructWithRest(
    Schema.Struct({
        tool: Schema.StructWithRest(Schema.Struct({ uv: Schema.StructWithRest(Schema.Struct({ 'constraint-dependencies': Rows, 'build-constraint-dependencies': Rows }), [Table]) }), [Table]),
    }),
    [Table],
);

// --- [OPERATIONS] ----------------------------------------------------------------------

const canonical = (name: string) => name.toLowerCase().replaceAll(SEPARATORS, '-');

const constrained = Effect.fnUntraced(function* (options: unknown, context: ExecutorContext) {
    const [path, fs, project, { manifest }] = yield* Effect.all([Path.Path, FileSystem.FileSystem, Effect.fromNullishOr(context.projectName), Schema.decodeUnknownEffect(Options)(options)]);
    const file = path.join((yield* Effect.fromNullishOr(context.projectGraph.nodes[project])).data.root, manifest);
    const [{ distributions, undeclared }, text] = yield* Effect.all([probed(context.root), fs.readFileString(path.join(context.root, file))]);
    const root = yield* Effect.flatMap(toml(text, file), Schema.decodeUnknownEffect(Root)).pipe(Effect.catchTag('SchemaError', DistributionError.of(file, 'states no [tool.uv] table a uv root reads constraints from')));
    const exact = '==';
    const bundled = Record.collect({ ...distributions, ...undeclared }, (name, version) => [canonical(name), [name, version].join(exact)] as const);
    const pinned = Record.fromEntries(bundled);
    const { 'constraint-dependencies': held = [], 'build-constraint-dependencies': builds } = root.tool.uv;
    const rows = Array.sortWith(bundled, ([name]) => name, Order.String).map(([, row]) => row);
    const built = builds?.map((row) => pinned[canonical(Array.headNonEmpty(String.split(row, exact)))] ?? row);
    const written = patch(text, { ...root, tool: { ...root.tool, uv: { ...root.tool.uv, 'constraint-dependencies': rows, ...(built === undefined ? {} : { 'build-constraint-dependencies': built }) } } });
    const [stated, wanted] = [Array.dedupe([...held, ...(builds ?? [])]), Array.dedupe([...rows, ...(built ?? [])])];
    yield* written === text
        ? Effect.logInfo(`${file} already states each distribution Blender bundles`)
        : Effect.andThen(fs.writeFileString(path.join(context.root, file), written), Effect.logInfo(`${file} adds ${Array.difference(wanted, stated).join(', ') || 'no row'} and drops ${Array.difference(stated, wanted).join(', ') || 'no row'}`));
    return { success: true };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = (options: unknown, context: ExecutorContext) => constrained(options, context).pipe(Effect.provide(layer), Effect.runPromise);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
