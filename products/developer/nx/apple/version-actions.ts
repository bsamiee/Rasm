import { extname } from 'node:path';
import type { Tree } from '@nx/devkit';
import { spawned } from '@rasm/nx-distribution/inference';
import { Array, Cause, Effect, Option, Schema, Stream } from 'effect';
import { VersionActions } from 'nx/release';
import { analyzer, run } from '#/targets.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const SHORT_VERSION = 'CFBundleShortVersionString';

// --- [MODELS] --------------------------------------------------------------------------

const Stated = Schema.Struct({ manifest: Schema.String });
const Versioned = Schema.fromJsonString(Schema.Struct({ version: Schema.NullOr(Schema.String), project: Schema.String }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const read = (tree: Tree, at: string) => Effect.mapError(Effect.fromNullishOr(tree.read(at, 'utf-8')), (cause) => new Cause.UnknownError(cause, 'the file is missing'));

const piped = (argv: Array.NonEmptyReadonlyArray<string>, input: string) => spawned(argv, { stdin: Stream.encodeText(Stream.make(input)) });

const versioned = (text: string, marketing: readonly string[]) => Effect.flatMap(analyzer, (core) => piped([core, 'version', ...marketing], text)).pipe(Effect.flatMap(Schema.decodeEffect(Versioned)));

const stated = Effect.fnUntraced(
    function* (tree: Tree, at: string) {
        const text = yield* read(tree, at);
        return extname(at) === '.plist' ? (yield* piped(['plutil', '-extract', SHORT_VERSION, 'raw', '-o', '-', '-'], text)).trim() : yield* Effect.mapError(Effect.fromNullishOr((yield* versioned(text, [])).version), (cause) => new Cause.UnknownError(cause, 'it states no MARKETING_VERSION'));
    },
    (effect, _, at) => Effect.catchCause(effect, (cause) => Effect.fail(new Cause.UnknownError(cause, `Cannot read the version of ${at}: ${Cause.pretty(cause)}`))),
);

const restated = Effect.fnUntraced(
    function* (tree: Tree, at: string, version: string) {
        const text = yield* read(tree, at);
        tree.write(at, extname(at) === '.plist' ? yield* piped(['plutil', '-replace', SHORT_VERSION, '-string', version, '-o', '-', '-'], text) : (yield* versioned(text, ['--marketing-version', version])).project);
        return `New version ${version} written to manifest: ${at}`;
    },
    (effect, _, at) => Effect.catchCause(effect, (cause) => Effect.fail(new Cause.UnknownError(cause, `Cannot update ${at}: ${Cause.pretty(cause)}`))),
);

// --- [SERVICES] ------------------------------------------------------------------------

class AppleVersionActions extends VersionActions {
    override readonly excludeManifestsFromFormatting = true;

    override get validManifestFilenames(): string[] {
        return Option.toArray(Option.map(Schema.decodeUnknownOption(Stated)(this.finalConfigForProject.versionActionsOptions), ({ manifest }) => manifest));
    }

    readCurrentVersionFromSourceManifest(tree: Tree) {
        const located = Effect.mapError(Effect.fromOption(Array.head(this.manifestsToUpdate)), (cause) => new Cause.UnknownError(cause, `${this.projectGraphNode.name} names no manifest in its versionActionsOptions`));
        return run(Effect.flatMap(located, ({ manifestPath }) => Effect.all({ currentVersion: stated(tree, manifestPath), manifestPath: Effect.succeed(manifestPath) })));
    }

    override readCurrentVersionFromRegistry() {
        return Promise.resolve(null);
    }

    override readCurrentVersionOfDependency() {
        return Promise.resolve({ currentVersion: null, dependencyCollection: null });
    }

    updateProjectVersion(tree: Tree, newVersion: string) {
        return run(Effect.forEach(this.manifestsToUpdate, ({ manifestPath }) => restated(tree, manifestPath, newVersion)));
    }

    override updateProjectDependencies() {
        return Promise.resolve([]);
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export default AppleVersionActions;
