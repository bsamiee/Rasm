import { join } from 'node:path';
import type { Tree } from '@nx/devkit';
import ast from '@xml-tools/ast';
import xml, { type DocumentCstNode } from '@xml-tools/parser';
import { Array, Effect, identity, Option, Result, Schema, SemVer, String, Struct } from 'effect';
import { VersionActions } from 'nx/release';
import { DotnetError, printed, run } from '#/targets';

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({ manifest: Schema.String, releases: Schema.Array(Schema.Struct({ shipped: Schema.String, unshipped: Schema.String })) });
const Found = Schema.fromJsonString(Schema.Struct({ searchResult: Schema.Array(Schema.Struct({ packages: Schema.Array(Schema.Struct({ version: Schema.String })) })) }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const read = (tree: Tree, file: string) => Effect.mapError(Effect.fromNullishOr(tree.read(file, 'utf-8')), DotnetError.of({ _tag: 'Missing', what: 'file' }, file));

const versioned = Effect.fnUntraced(function* (tree: Tree, at: string) {
    const text = yield* read(tree, at);
    const { cst, tokenVector, lexErrors, parseErrors } = xml.parse(text);
    const document = yield* Effect.mapError(Effect.fromOption(Option.liftPredicate(cst, (node): node is DocumentCstNode => node.name === 'document' && lexErrors.length + parseErrors.length === 0)), DotnetError.of({ _tag: 'Undecoded', form: 'an MSBuild project file' }, at));
    const groups = Array.fromNullishOr(ast.buildAst(document, tokenVector).rootElement).flatMap(({ subElements }) => subElements.filter(({ name }) => name === 'PropertyGroup'));
    const stated = Array.findLast(
        groups.flatMap(({ subElements }) => subElements.filter(({ name }) => name === 'Version')),
        ({ textContents }) => Array.findFirst(textContents, ({ text: version, position }) => Option.all({ version: Option.fromNullOr(version), position: Option.some(position) })),
    );
    return { text, ...(yield* Effect.mapError(Effect.fromOption(stated), DotnetError.of({ _tag: 'Missing', what: 'Version property, which nx release reads and writes in the project file' }, at))) };
});

const shipped = Effect.fnUntraced(function* (tree: Tree, version: string, files: (typeof Options.Type)['releases'][number]) {
    const [recorded, pending, release] = yield* Effect.all([read(tree, files.shipped), read(tree, files.unshipped), Effect.mapError(Effect.fromResult(SemVer.parse(version)), DotnetError.of({ _tag: 'Undecoded', form: 'a semantic version' }, version))]);
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

// --- [COMPOSITION] ---------------------------------------------------------------------

class DotnetVersionActions extends VersionActions {
    readonly #stated = Schema.decodeUnknownOption(Options)(this.finalConfigForProject.versionActionsOptions);
    override readonly validManifestFilenames = Option.toArray(Option.map(this.#stated, Struct.get('manifest')));
    override readonly excludeManifestsFromFormatting = true;

    readCurrentVersionFromSourceManifest(tree: Tree) {
        const manifestPath = join(this.projectGraphNode.data.root, ...this.validManifestFilenames);
        return run(Effect.map(versioned(tree, manifestPath), ({ version }) => ({ currentVersion: version, manifestPath })));
    }

    override readCurrentVersionFromRegistry(tree: Tree) {
        const search = ['package', 'search', this.projectGraphNode.name, '--exact-match', '--format', 'json'];
        const listed = Effect.flatMap(printed('dotnet', search, { cwd: tree.root }), Schema.decodeEffect(Found));
        return run(Effect.map(listed, ({ searchResult }) => Option.getOrNull(Array.findLast(searchResult, ({ packages }) => Array.last(packages)).pipe(Option.map(({ version }) => ({ currentVersion: version, logText: `dotnet ${search.join(' ')}` }))))));
    }

    override readCurrentVersionOfDependency() {
        return Promise.resolve({ currentVersion: null, dependencyCollection: null });
    }

    updateProjectVersion(tree: Tree, newVersion: string) {
        return run(
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
        return Promise.resolve([]);
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export default DotnetVersionActions;
