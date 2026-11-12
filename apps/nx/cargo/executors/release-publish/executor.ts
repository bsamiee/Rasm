import { pathToFileURL } from 'node:url';
import { Dry, dried, executor } from '@rasm/nx-devkit/executor';
import { printed, ran } from '@rasm/nx-devkit/process';
import { Array, Effect, Equivalence, Path, Schema, Struct } from 'effect';
import { Metadata } from '#/workspace';

// --- [MODELS] --------------------------------------------------------------------------

const Publish = Dry.mapFields(
    Struct.assign({
        cwd: Schema.String.annotate({ description: 'Cargo workspace root, relative to the Nx workspace root' }),
        package: Schema.String.annotate({ description: 'Crate to publish' }),
        registry: Schema.optionalKey(Schema.String).annotate({ description: 'Registry the crate names in its publish list, passed as --registry' }),
    }),
).annotate({ title: 'Release publish', description: 'Publishes a crate with cargo publish, under a dry run with cargo publish --dry-run over the crate and its workspace dependencies; runs through nx release publish' });

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = executor(
    Publish,
    Effect.fnUntraced(function* ({ cwd: root, package: named, registry, ...stated }, context) {
        const [path, dry] = yield* Effect.all([Path.Path, dried(stated)]);
        const cwd = path.resolve(context.root, root);
        const { packages } = yield* Effect.flatMap(printed('cargo', ['metadata', '--format-version', '1', '--no-deps'], { cwd }), Schema.decodeEffect(Metadata));
        const crate = yield* Effect.fromOption(Array.findFirst(packages, ({ name }) => name === named));
        const local = (member: typeof crate): readonly (typeof crate)[] => member.dependencies.flatMap(({ path: at }) => packages.filter(({ manifest_path }) => at !== undefined && path.dirname(manifest_path) === at)).flatMap((dependency) => [dependency, ...local(dependency)]);
        const members = dry ? Array.dedupeWith([crate, ...local(crate)], Equivalence.Struct({ name: Equivalence.String })) : [crate];
        const selected = members.flatMap((member) => ['--package', `path+${pathToFileURL(path.dirname(member.manifest_path)).href}#${member.name}`]);
        return yield* Effect.map(ran('cargo', ['publish', ...selected, ...Array.fromNullishOr(registry).flatMap((held) => ['--registry', held]), ...(dry ? ['--dry-run'] : []), '--locked'], { cwd, stdin: 'inherit', stdout: 'inherit', stderr: 'inherit' }), ({ code }) => code === 0);
    }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
