import { execFileSync, spawnSync } from 'node:child_process';
import { pathToFileURL } from 'node:url';
import type { ExecutorContext } from '@nx/devkit';
import { Array, Config, Effect, Option, Path, Schema, String } from 'effect';
import { Metadata, refused, run } from '#/workspace';

// --- [MODELS] --------------------------------------------------------------------------

const Publish = Schema.Struct({
    dryRun: Schema.optionalKey(Schema.Boolean).annotate({ description: 'Prints each change and makes none, as NX_DRY_RUN=true does' }),
    cwd: Schema.String.annotate({ description: 'Cargo workspace root, relative to the Nx workspace root' }),
    package: Schema.String.annotate({ description: 'Crate to publish' }),
    registry: Schema.optionalKey(Schema.String).annotate({ description: 'Registry the crate names in its publish list, passed as --registry' }),
}).annotate({
    title: 'Release publish',
    description:
        'Publishes a crate with cargo publish where the registry token variable Cargo reads (CARGO_REGISTRY_TOKEN, or CARGO_REGISTRIES_<NAME>_TOKEN for a named registry) is set, and names the variable where it is not, under a dry run with cargo publish --dry-run over the crate and its workspace dependencies; runs through nx release publish',
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = Effect.fnUntraced(function* ({ dryRun, cwd: root, package: named, registry }: typeof Publish.Type, context: ExecutorContext) {
    const [path, stated] = yield* Effect.all([Path.Path, Config.withDefault(Config.Boolean('NX_DRY_RUN'), false)]);
    const dry = dryRun === true || stated;
    const cwd = path.resolve(context.root, root);
    const { packages } = yield* Effect.flatMap(Effect.try({ try: () => execFileSync('cargo', ['metadata', '--color', 'never', '--format-version', '1', '--no-deps'], { encoding: 'utf8', cwd, maxBuffer: Number.POSITIVE_INFINITY }), catch: refused(root, 'cargo metadata does not exit 0') }), Schema.decodeEffect(Metadata));
    const crate = yield* Effect.fromOption(Array.findFirst(packages, ({ name }) => name === named));
    const placed = new Map<string | undefined, string>(packages.map((member) => [path.dirname(member.manifest_path), member.name]));
    const reached = (names: ReadonlySet<string>): ReadonlySet<string> => {
        const needed = packages.filter(({ name }) => names.has(name)).flatMap(({ dependencies }) => dependencies.flatMap(({ path: at }) => Array.fromNullishOr(placed.get(at))));
        return names.isSupersetOf(new Set(needed)) ? names : reached(names.union(new Set(needed)));
    };
    const members = dry ? packages.filter(({ name }) => reached(new Set([crate.name])).has(name)) : [crate];
    const selected = members.flatMap((member) => ['--package', `path+${pathToFileURL(path.dirname(member.manifest_path)).href}#${member.name}`]);
    const publish = ['publish', ...selected, ...Array.fromNullishOr(registry).flatMap((held) => ['--registry', held]), ...(dry ? ['--dry-run'] : []), '--locked'];
    const credential = registry === undefined ? 'CARGO_REGISTRY_TOKEN' : `CARGO_REGISTRIES_${String.kebabToSnake(registry).toUpperCase()}_TOKEN`;
    const token = Option.filter(yield* Config.option(Config.String(credential)), String.isNonEmpty);
    return yield* dry || Option.isSome(token) ? Effect.sync(() => ({ success: spawnSync('cargo', publish, { cwd, stdio: 'inherit' }).status === 0 })) : Effect.as(Effect.logWarning(`Skipped cargo publish of ${crate.name}: ${credential} is not set`), { success: true });
}, run);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute, Publish };
