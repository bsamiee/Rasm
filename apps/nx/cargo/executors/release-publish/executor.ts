import { pathToFileURL } from 'node:url';
import { executor, Released, released } from '@rasm/nx-devkit/executor';
import { printed } from '@rasm/nx-devkit/process';
import { Array, Effect, Equivalence, Match, Option, Path, Schema, String, Struct } from 'effect';
import { FetchHttpClient, HttpClient, HttpClientResponse } from 'effect/http';
import { ran } from '#/executors/cargo/executor';
import { Metadata } from '#/workspace';

// --- [CONSTANTS] -----------------------------------------------------------------------

const LEVEL = 2;

// --- [MODELS] --------------------------------------------------------------------------

const Publish = Released.mapFields(
    Struct.assign({
        cwd: Schema.String.annotate({ description: 'Cargo workspace root, relative to the Nx workspace root' }),
        package: Schema.String.annotate({ description: 'Crate to publish' }),
    }),
).annotate({ title: 'Release publish', description: 'Publishes a crate with cargo publish, skipping a version the registry already holds; runs through nx release publish' });
const Published = Schema.fromJsonString(Schema.Struct({ vers: Schema.String, yanked: Schema.optionalKey(Schema.NullOr(Schema.Boolean)) }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const releases = Effect.fnUntraced(function* (name: string) {
    const lower = name.toLowerCase();
    const folder = Match.value(lower.length).pipe(
        Match.when(
            (length) => length <= LEVEL,
            (length) => `${length}`,
        ),
        Match.when(LEVEL + 1, (length) => `${length}/${String.takeLeft(lower, 1)}`),
        Match.orElse(() => `${String.takeLeft(lower, LEVEL)}/${lower.slice(LEVEL, 2 * LEVEL)}`),
    );
    const url = `https://index.crates.io/${folder}/${lower}`;
    const lines = yield* HttpClientResponse.matchStatus(yield* HttpClient.get(url), {
        404: () => Effect.succeed<readonly string[]>([]),
        orElse: (response) =>
            HttpClientResponse.filterStatusOk(response).pipe(
                Effect.flatMap((ok) => ok.text),
                Effect.map((text) => text.split('\n').filter(String.isNonEmpty)),
            ),
    });
    const published = yield* Effect.forEach(lines, (line) => Schema.decodeEffect(Published)(line));
    return { url, published };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = executor(
    Publish,
    Effect.fnUntraced(function* ({ cwd: root, package: named, ...stated }, context, project) {
        const { registry, firstRelease } = stated;
        const [path, { dry, unversioned }] = yield* Effect.all([Path.Path, released(stated, context, project)]);
        const cwd = path.resolve(context.root, root);
        const { packages } = yield* Effect.flatMap(printed('cargo', ['metadata', '--format-version', '1', '--no-deps'], { cwd }), Schema.decodeEffect(Metadata));
        const crate = yield* Effect.fromOption(Array.findFirst(packages, ({ name }) => name === named));
        const unpublished = (member: typeof crate) => (registry === undefined && firstRelease !== true ? Effect.map(releases(member.name), ({ published }) => !published.some(({ vers }) => vers === member.version)) : Effect.succeed(true));
        const local = (member: typeof crate): readonly (typeof crate)[] => member.dependencies.flatMap(({ path: at }) => packages.filter(({ manifest_path }) => at !== undefined && path.dirname(manifest_path) === at)).flatMap((dependency) => [dependency, ...local(dependency)]);
        const skipped = unversioned || !(yield* unpublished(crate));
        const overlay = dry ? yield* Effect.filter(Array.dedupeWith(local(crate), Equivalence.Struct({ name: Equivalence.String })), (member) => unpublished(member)) : [];
        return skipped
            ? yield* Effect.as(Effect.logInfo(`Skipped ${crate.name}@${crate.version}, the registry already holds it or nx release left it unchanged`), true)
            : yield* Effect.map(
                  ran(cwd, ['publish', ...[crate, ...overlay].flatMap((member) => ['--package', `path+${pathToFileURL(path.dirname(member.manifest_path)).href}#${member.name}`]), ...Option.toArray(Option.fromUndefinedOr(registry)).flatMap((held) => ['--registry', held]), ...(dry ? ['--dry-run'] : []), '--locked']),
                  ({ code }) => Option.contains(code, 0),
              );
    }, Effect.provide(FetchHttpClient.layer)),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute, releases };
