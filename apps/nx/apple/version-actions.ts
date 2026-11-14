import { execFile } from 'node:child_process';
import { extname } from 'node:path';
import { promisify } from 'node:util';
import type { Tree } from '@nx/devkit';
import { Array, Effect, Option, Schema } from 'effect';
import { VersionActions } from 'nx/release';
import { analyzer, refused } from '#/targets';

// --- [CONSTANTS] -----------------------------------------------------------------------

const SHORT_VERSION = 'CFBundleShortVersionString';

// --- [MODELS] --------------------------------------------------------------------------

const Stated = Schema.Struct({ manifest: Schema.String });
const Versioned = Schema.fromJsonString(Schema.Struct({ version: Schema.NullOr(Schema.String), project: Schema.String }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const read = (tree: Tree, at: string) => Effect.mapError(Effect.fromNullishOr(tree.read(at, 'utf-8')), refused(at, 'is missing'));

const piped = (command: string, args: readonly string[], input: string) =>
    Effect.tryPromise({
        try: (signal) => {
            const running = promisify(execFile)(command, args, { encoding: 'utf8', maxBuffer: Number.POSITIVE_INFINITY, signal });
            running.child.stdin?.end(input);
            return running;
        },
        catch: refused(command, 'does not exit 0'),
    }).pipe(Effect.map(({ stdout }) => stdout));

const versioned = (text: string, marketing: readonly string[]) => Effect.flatMap(analyzer, (core) => piped(core, ['version', ...marketing], text)).pipe(Effect.flatMap(Schema.decodeEffect(Versioned)));

const stated = Effect.fnUntraced(function* (tree: Tree, at: string) {
    const text = yield* read(tree, at);
    return extname(at) === '.plist' ? (yield* piped('plutil', ['-extract', SHORT_VERSION, 'raw', '-o', '-', '-'], text)).trim() : yield* Effect.mapError(Effect.fromNullishOr((yield* versioned(text, [])).version), refused(at, 'states no MARKETING_VERSION'));
});

const restated = Effect.fnUntraced(function* (tree: Tree, at: string, version: string) {
    const text = yield* read(tree, at);
    tree.write(at, extname(at) === '.plist' ? yield* piped('plutil', ['-replace', SHORT_VERSION, '-string', version, '-o', '-', '-'], text) : (yield* versioned(text, ['--marketing-version', version])).project);
    return `New version ${version} written to manifest: ${at}`;
});

// --- [SERVICES] ------------------------------------------------------------------------

class AppleVersionActions extends VersionActions {
    override readonly excludeManifestsFromFormatting = true;

    override get validManifestFilenames(): string[] {
        return Option.toArray(Option.map(Schema.decodeUnknownOption(Stated)(this.finalConfigForProject.versionActionsOptions), ({ manifest }) => manifest));
    }

    readCurrentVersionFromSourceManifest(tree: Tree) {
        const located = Effect.mapError(Effect.fromOption(Array.head(this.manifestsToUpdate)), refused(this.projectGraphNode.name, 'names no manifest in its versionActionsOptions'));
        return Effect.runPromise(Effect.flatMap(located, ({ manifestPath }) => Effect.all({ currentVersion: stated(tree, manifestPath), manifestPath: Effect.succeed(manifestPath) })));
    }

    override readCurrentVersionFromRegistry() {
        return Promise.resolve(null);
    }

    override readCurrentVersionOfDependency() {
        return Promise.resolve({ currentVersion: null, dependencyCollection: null });
    }

    updateProjectVersion(tree: Tree, newVersion: string) {
        return Effect.runPromise(Effect.forEach(this.manifestsToUpdate, ({ manifestPath }) => restated(tree, manifestPath, newVersion)));
    }

    override updateProjectDependencies() {
        return Promise.resolve([]);
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export default AppleVersionActions;
