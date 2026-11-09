import { pathToFileURL } from 'node:url';
import { layer } from '@effect/platform-node/NodeServices';
import type { ExecutorContext } from '@nx/devkit';
import { isCI } from '@nx/devkit/internal';
import { Array, Effect, Layer, Match, Option, Path, Schema } from 'effect';
import { FetchHttpClient, HttpClient, HttpClientResponse } from 'effect/http';
import { ran } from '#/executors/cargo/executor';
import { Metadata, spawned } from '#/plugins/workspace';

// --- [CONSTANTS] -----------------------------------------------------------------------

const LEVEL = 2;
const MISSING = 404;

// --- [MODELS] --------------------------------------------------------------------------

const Publish = Schema.Struct({
    cwd: Schema.String,
    package: Schema.String,
    registry: Schema.optionalKey(Schema.String),
    dryRun: Schema.optionalKey(Schema.Boolean),
    firstRelease: Schema.optionalKey(Schema.Boolean),
    nxReleaseVersionData: Schema.optionalKey(Schema.Record(Schema.String, Schema.Struct({ newVersion: Schema.NullOr(Schema.String) }))),
});
const Released = Schema.fromJsonString(Schema.Struct({ vers: Schema.String, yanked: Schema.Boolean }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const releases = (name: string) => {
    const lower = name.toLowerCase();
    const [first = '', second = ''] = Array.chunksOf(Array.fromIterable(lower), LEVEL).map((chunk) => chunk.join(''));
    const folder = Match.value(second.length).pipe(
        Match.when(LEVEL, () => `${first}/${second}`),
        Match.when(0, () => `${lower.length}`),
        Match.orElse(() => `${lower.length}/${lower.at(0) ?? ''}`),
    );
    const url = `https://index.crates.io/${folder}/${lower}`;
    return HttpClient.get(url).pipe(
        Effect.flatMap((response) => (response.status === MISSING ? Effect.succeed('') : Effect.flatMap(HttpClientResponse.filterStatusOk(response), (ok) => ok.text))),
        Effect.flatMap((text) =>
            Effect.forEach(
                text.split('\n').filter((line) => line.length > 0),
                (line) => Schema.decodeEffect(Released)(line),
            ),
        ),
        Effect.map((found) => ({ url, found })),
    );
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = (options: unknown, context: ExecutorContext) =>
    Effect.runPromise(
        Effect.gen(function* () {
            const [path, publish] = yield* Effect.all([Path.Path, Schema.decodeUnknownEffect(Publish)(options)]);
            const cwd = path.resolve(context.root, publish.cwd);
            const { packages } = yield* Effect.flatMap(spawned('cargo', ['metadata', '--format-version', '1', '--no-deps', '--offline'], cwd), Schema.decodeEffect(Metadata));
            const crate = yield* Effect.fromOption(Array.findFirst(packages, ({ name }) => name === publish.package));
            const unpublished = (held: typeof crate) => (publish.registry === undefined && publish.firstRelease !== true ? Effect.map(releases(held.name), ({ found }) => !found.some(({ vers }) => vers === held.version)) : Effect.succeed(true));
            const local = (held: typeof crate): readonly (typeof crate)[] => held.dependencies.flatMap(({ path: at }) => packages.filter(({ manifest_path }) => at !== undefined && path.dirname(manifest_path) === at)).flatMap((dependency) => [dependency, ...local(dependency)]);
            const skipped = publish.nxReleaseVersionData?.[context.projectName ?? '']?.newVersion === null || !(yield* unpublished(crate));
            const overlay =
                publish.dryRun === true
                    ? yield* Effect.filter(
                          Array.dedupeWith(local(crate), (left, right) => left.name === right.name),
                          (held) => unpublished(held),
                      )
                    : [];
            return skipped
                ? yield* Effect.as(Effect.logInfo(`Skipped ${crate.name}@${crate.version}, the registry already holds it or nx release left it unchanged`), { success: true })
                : yield* Effect.map(
                      ran(cwd, [
                          'publish',
                          ...[crate, ...overlay].flatMap((held) => ['--package', `path+${pathToFileURL(path.dirname(held.manifest_path)).href}#${held.name}`]),
                          ...Option.toArray(Option.fromUndefinedOr(publish.registry)).flatMap((registry) => ['--registry', registry]),
                          ...(publish.dryRun === true ? ['--dry-run'] : []),
                          ...(isCI() === false ? [] : ['--locked']),
                      ]),
                      ({ code }) => ({ success: code === 0 }),
                  );
        }).pipe(Effect.provide(Layer.mergeAll(layer, FetchHttpClient.layer))),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute, releases };
