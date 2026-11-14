import { pathToFileURL } from 'node:url';
import type { ExecutorContext } from '@nx/devkit';
import { Completed } from '@rasm/process';
import { Array, Config, Effect, Path, Schema } from 'effect';
import { ChildProcess } from 'effect/process';
import { Metadata, run } from '#/workspace';

// --- [MODELS] --------------------------------------------------------------------------

const Publish = Schema.Struct({
    dryRun: Schema.optionalKey(Schema.Boolean).annotate({ description: 'Prints each change and makes none, as NX_DRY_RUN=true does' }),
    cwd: Schema.String.annotate({ description: 'Cargo workspace root, relative to the Nx workspace root' }),
    package: Schema.String.annotate({ description: 'Crate to publish' }),
    registry: Schema.optionalKey(Schema.String).annotate({ description: 'Registry the crate names in its publish list, passed as --registry' }),
}).annotate({ title: 'Release publish', description: 'Publishes a crate with cargo publish, under a dry run with cargo publish --dry-run over the crate and its workspace dependencies; runs through nx release publish' });

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = Effect.fnUntraced(function* ({ dryRun, cwd: root, package: named, registry }: typeof Publish.Type, context: ExecutorContext) {
    const [path, stated] = yield* Effect.all([Path.Path, Config.withDefault(Config.Boolean('NX_DRY_RUN'), false)]);
    const cwd = path.resolve(context.root, root);
    const { packages } = yield* Effect.flatMap(Completed.output(ChildProcess.make('cargo', ['metadata', '--color', 'never', '--format-version', '1', '--no-deps'], { cwd, stdin: 'ignore' })), Schema.decodeEffect(Metadata));
    const crate = yield* Effect.fromOption(Array.findFirst(packages, ({ name }) => name === named));
    const placed = new Map<string | undefined, string>(packages.map((member) => [path.dirname(member.manifest_path), member.name]));
    const reached = (names: ReadonlySet<string>): ReadonlySet<string> => {
        const needed = packages.filter(({ name }) => names.has(name)).flatMap(({ dependencies }) => dependencies.flatMap(({ path: at }) => Array.fromNullishOr(placed.get(at))));
        return names.isSupersetOf(new Set(needed)) ? names : reached(names.union(new Set(needed)));
    };
    const members = dryRun === true || stated ? packages.filter(({ name }) => reached(new Set([crate.name])).has(name)) : [crate];
    const selected = members.flatMap((member) => ['--package', `path+${pathToFileURL(path.dirname(member.manifest_path)).href}#${member.name}`]);
    const publish = ['publish', ...selected, ...Array.fromNullishOr(registry).flatMap((held) => ['--registry', held]), ...(dryRun === true || stated ? ['--dry-run'] : []), '--locked'];
    const { code } = yield* Completed.run(ChildProcess.make('cargo', publish, { cwd, detached: false, stdin: 'inherit', stdout: 'inherit', stderr: 'inherit' }));
    return { success: code === 0 };
}, run);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute, Publish };
