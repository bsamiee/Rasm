import { join } from 'node:path';
import { layer } from '@effect/platform-node/NodeServices';
import type { Tree } from '@nx/devkit';
import { Completed } from '@rasm/process';
import ast from '@xml-tools/ast';
import xml, { type DocumentCstNode } from '@xml-tools/parser';
import { Array, Cause, Effect, identity, Option, Result, Schema, SemVer, String, Struct } from 'effect';
import { ChildProcess } from 'effect/process';
import { VersionActions } from 'nx/release';

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({ manifest: Schema.String, releases: Schema.Array(Schema.Struct({ shipped: Schema.String, unshipped: Schema.String })) });
const Found = Schema.fromJsonString(Schema.Struct({ searchResult: Schema.Array(Schema.Struct({ packages: Schema.Array(Schema.Struct({ version: Schema.String })) })) }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const refused = (subject: string, detail: string) => (cause?: unknown) => new Cause.UnknownError(cause, `${subject}: ${detail}`);

const read = (tree: Tree, file: string) => Effect.mapError(Effect.fromNullishOr(tree.read(file, 'utf-8')), refused(file, 'is missing'));

const versioned = Effect.fnUntraced(function* (tree: Tree, at: string) {
    const text = yield* read(tree, at);
    const { cst, tokenVector, lexErrors, parseErrors } = xml.parse(text);
    const document = yield* Effect.mapError(Effect.fromOption(Option.liftPredicate(cst, (node): node is DocumentCstNode => node.name === 'document' && lexErrors.length + parseErrors.length === 0)), refused(at, 'does not parse as an MSBuild project file'));
    const groups = Array.fromNullishOr(ast.buildAst(document, tokenVector).rootElement).flatMap(({ subElements }) => subElements.filter(({ name }) => name === 'PropertyGroup'));
    const stated = Array.findLast(
        groups.flatMap(({ subElements }) => subElements.filter(({ name }) => name === 'Version')),
        ({ textContents }) => Array.findFirst(textContents, ({ text: version, position }) => Option.all({ version: Option.fromNullOr(version), position: Option.some(position) })),
    );
    return { text, ...(yield* Effect.mapError(Effect.fromOption(stated), refused(at, 'states no Version property, which nx release reads and writes in the project file'))) };
});

const shipped = Effect.fnUntraced(function* (tree: Tree, version: string, files: (typeof Options.Type)['releases'][number]) {
    const [recorded, pending, release] = yield* Effect.all([read(tree, files.shipped), read(tree, files.unshipped), Effect.mapError(Effect.fromResult(SemVer.parse(version)), refused(version, 'is no semantic version'))]);
    const [rows, comments] = Array.partition(
        pending.split('\n'),
        Result.liftPredicate((row: string) => !row.trimStart().startsWith(';'), identity),
    );
    const moved = Option.filter(Option.liftPredicate(rows.join('\n').trim(), String.isNonEmpty), () => !SemVer.isPrerelease(release));
    return yield* Option.match(moved, {
        onNone: () => Effect.succeed([]),
        onSome: (lines) =>
            Effect.sync(() => {
                const section = `## Release ${SemVer.format(SemVer.release(release))}\n\n${lines}\n`;
                tree.write(files.shipped, Option.match(Option.liftPredicate(recorded.trimEnd(), String.isNonEmpty), { onNone: () => section, onSome: (existing) => `${existing}\n\n${section}` }));
                tree.write(files.unshipped, comments.map((comment) => `${comment}\n`).join(''));
                return [`Unshipped rules of ${files.unshipped} moved to ${files.shipped} under release ${version}`];
            }),
    });
});

// --- [SERVICES] ------------------------------------------------------------------------

class DotnetVersionActions extends VersionActions {
    readonly #stated = Schema.decodeUnknownOption(Options)(this.finalConfigForProject.versionActionsOptions);
    override readonly validManifestFilenames = Option.toArray(Option.map(this.#stated, Struct.get('manifest')));
    override readonly excludeManifestsFromFormatting = true;

    readCurrentVersionFromSourceManifest(tree: Tree) {
        const manifestPath = join(this.projectGraphNode.data.root, ...this.validManifestFilenames);
        return Effect.runPromise(Effect.map(versioned(tree, manifestPath), ({ version }) => ({ currentVersion: version, manifestPath })));
    }

    override readCurrentVersionFromRegistry(tree: Tree) {
        const search = ['package', 'search', this.projectGraphNode.name, '--exact-match', '--format', 'json'];
        const listed = Effect.flatMap(Completed.output(ChildProcess.make('dotnet', search, { cwd: tree.root, stdin: 'ignore' })), Schema.decodeEffect(Found));
        return Effect.runPromise(
            Effect.provide(
                Effect.map(listed, ({ searchResult }) =>
                    Option.getOrNull(
                        Option.map(
                            Array.findLast(searchResult, ({ packages }) => Array.last(packages)),
                            ({ version }) => ({ currentVersion: version, logText: `dotnet ${search.join(' ')}` }),
                        ),
                    ),
                ),
                layer,
            ),
        );
    }

    override readCurrentVersionOfDependency() {
        return Effect.runPromise(Effect.succeed({ currentVersion: null, dependencyCollection: null }));
    }

    updateProjectVersion(tree: Tree, newVersion: string) {
        return Effect.runPromise(
            Effect.zipWith(
                Effect.forEach(this.manifestsToUpdate, ({ manifestPath }) =>
                    Effect.map(versioned(tree, manifestPath), ({ text, position }) => {
                        tree.write(manifestPath, `${text.slice(0, position.startOffset)}${newVersion}${text.slice(position.endOffset + 1)}`);
                        return `New version ${newVersion} written to manifest: ${manifestPath}`;
                    }),
                ),
                Effect.forEach(Option.toArray(this.#stated).flatMap(Struct.get('releases')), (files) => shipped(tree, newVersion, files)),
                (logs, moved) => [...logs, ...moved.flat()],
            ),
        );
    }

    override updateProjectDependencies() {
        return Effect.runPromise(Effect.succeed(Array.empty<string>()));
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export default DotnetVersionActions;
