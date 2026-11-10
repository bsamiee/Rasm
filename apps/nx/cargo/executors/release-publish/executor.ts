import { pathToFileURL } from 'node:url';
import { layer } from '@effect/platform-node/NodeServices';
import type { ExecutorContext } from '@nx/devkit';
import { isCI } from '@nx/devkit/internal';
import { Array, Config, Effect, Equivalence, flow, Layer, Match, Option, Path, Schema, String } from 'effect';
import { FetchHttpClient, HttpClient, HttpClientResponse } from 'effect/http';
import { ran } from '#/executors/cargo/executor';
import { Metadata, spawned } from '#/plugins/workspace';

// --- [CONSTANTS] -----------------------------------------------------------------------

const LEVEL = 2;

// --- [MODELS] --------------------------------------------------------------------------

const Publish = Schema.Struct({
    cwd: Schema.String,
    package: Schema.String,
    registry: Schema.optionalKey(Schema.String),
    dryRun: Schema.optionalKey(Schema.Boolean),
    firstRelease: Schema.optionalKey(Schema.Boolean),
    nxReleaseVersionData: Schema.optionalKey(Schema.Record(Schema.String, Schema.Struct({ newVersion: Schema.NullOr(Schema.String) }))),
});
const Released = Schema.fromJsonString(Schema.Struct({ vers: Schema.String, yanked: Schema.optionalKey(Schema.NullOr(Schema.Boolean)) }));

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
    const published = yield* Effect.forEach(lines, (line) => Schema.decodeEffect(Released)(line));
    return { url, published };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = flow(
    Effect.fnUntraced(
        function* (options: unknown, context: ExecutorContext) {
            const [path, publish, dry, project] = yield* Effect.all([Path.Path, Schema.decodeUnknownEffect(Publish)(options), Config.Boolean('NX_DRY_RUN').pipe(Config.withDefault(false)), Effect.fromNullishOr(context.projectName)]);
            const dryRun = publish.dryRun === true || dry;
            const cwd = path.resolve(context.root, publish.cwd);
            const { packages } = yield* Effect.flatMap(spawned('cargo', ['metadata', '--format-version', '1', '--no-deps'], cwd), Schema.decodeEffect(Metadata));
            const crate = yield* Effect.fromOption(Array.findFirst(packages, ({ name }) => name === publish.package));
            const unpublished = (member: typeof crate) => (publish.registry === undefined && publish.firstRelease !== true ? Effect.map(releases(member.name), ({ published }) => !published.some(({ vers }) => vers === member.version)) : Effect.succeed(true));
            const local = (member: typeof crate): readonly (typeof crate)[] => member.dependencies.flatMap(({ path: at }) => packages.filter(({ manifest_path }) => at !== undefined && path.dirname(manifest_path) === at)).flatMap((dependency) => [dependency, ...local(dependency)]);
            const skipped = publish.nxReleaseVersionData?.[project]?.newVersion === null || !(yield* unpublished(crate));
            const overlay = dryRun ? yield* Effect.filter(Array.dedupeWith(local(crate), Equivalence.Struct({ name: Equivalence.String })), (member) => unpublished(member)) : [];
            return skipped
                ? yield* Effect.as(Effect.logInfo(`Skipped ${crate.name}@${crate.version}, the registry already holds it or nx release left it unchanged`), { success: true })
                : yield* Effect.map(
                      ran(cwd, [
                          'publish',
                          ...[crate, ...overlay].flatMap((member) => ['--package', `path+${pathToFileURL(path.dirname(member.manifest_path)).href}#${member.name}`]),
                          ...Option.toArray(Option.fromUndefinedOr(publish.registry)).flatMap((registry) => ['--registry', registry]),
                          ...(dryRun ? ['--dry-run'] : []),
                          ...(isCI() === false ? [] : ['--locked']),
                      ]),
                      ({ code }) => ({ success: Option.contains(code, 0) }),
                  );
        },
        Effect.provide(Layer.mergeAll(layer, FetchHttpClient.layer)),
    ),
    Effect.runPromise,
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute, releases };
