import type { Tree } from '@nx/devkit';
import { Manifested } from '@rasm/nx-devkit/executor';
import { runtime, ToolError } from '@rasm/nx-devkit/process';
import ast from '@xml-tools/ast';
import xml, { type DocumentCstNode } from '@xml-tools/parser';
import { Array, Effect, identity, Option, Result, Schema, SemVer, String } from 'effect';

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({ projectFile: Schema.String, releases: Schema.Array(Schema.Struct({ shipped: Schema.String, unshipped: Schema.String })) });

// --- [OPERATIONS] ----------------------------------------------------------------------

const versioned = Effect.fnUntraced(function* (text: string, at: string) {
    const { cst, tokenVector, lexErrors, parseErrors } = xml.parse(text);
    const document = yield* Effect.mapError(Effect.fromOption(Option.liftPredicate(cst, (node): node is DocumentCstNode => node.name === 'document' && lexErrors.length + parseErrors.length === 0)), ToolError.of(at, 'does not parse as an MSBuild project file'));
    const groups = Array.fromNullishOr(ast.buildAst(document, tokenVector).rootElement).flatMap(({ subElements }) => subElements.filter(({ name }) => name === 'PropertyGroup'));
    const stated = Array.findLast(
        groups.flatMap(({ subElements }) => subElements.filter(({ name }) => name === 'Version')),
        ({ textContents }) => Array.findFirst(textContents, ({ text, position }) => Option.map(Option.fromNullOr(text), (version) => ({ version, position }))),
    );
    return yield* Effect.mapError(Effect.fromOption(stated), ToolError.of(at, 'states no Version property, which nx release reads and writes in the project file'));
});

const read = (tree: Tree, file: string) => Effect.mapError(Effect.fromNullishOr(tree.read(file, 'utf-8')), ToolError.of(file, 'is missing'));

const shipped = Effect.fnUntraced(function* (tree: Tree, version: string, files: (typeof Options.Type)['releases'][number]) {
    const [recorded, pending, release] = yield* Effect.all([read(tree, files.shipped), read(tree, files.unshipped), Effect.mapError(Effect.fromResult(SemVer.parse(version)), ToolError.of(version, 'is no semantic version'))]);
    const [rows, comments] = Array.partition(
        pending.split('\n'),
        Result.liftPredicate((line: string) => !line.trimStart().startsWith(';'), identity),
    );
    const moved = Option.filter(Option.liftPredicate(rows.join('\n').trim(), String.isNonEmpty), () => !SemVer.isPrerelease(release));
    return yield* Option.match(moved, {
        onNone: () => Effect.succeed([]),
        onSome: (lines) =>
            Effect.sync(() => {
                const section = `## Release ${SemVer.format(SemVer.release(release))}\n\n${lines}\n`;
                tree.write(files.shipped, Option.match(Option.liftPredicate(recorded.trimEnd(), String.isNonEmpty), { onNone: () => section, onSome: (existing) => `${existing}\n\n${section}` }));
                tree.write(files.unshipped, comments.map((line) => `${line}\n`).join(''));
                return [`Unshipped rules of ${files.unshipped} moved to ${files.shipped} under release ${version}`];
            }),
    });
});

// --- [SERVICES] ------------------------------------------------------------------------

class DotnetVersionActions extends Manifested {
    override get validManifestFilenames() {
        return Option.toArray(Option.map(Schema.decodeUnknownOption(Options)(this.finalConfigForProject.versionActionsOptions), ({ projectFile }) => projectFile));
    }

    override stated(text: string, at: string) {
        return Effect.map(versioned(text, at), ({ version }) => version);
    }

    override restated(text: string, version: string, at: string) {
        return Effect.map(versioned(text, at), ({ position }) => `${text.slice(0, position.startOffset)}${version}${text.slice(position.endOffset + 1)}`);
    }

    override updateProjectVersion(tree: Tree, newVersion: string) {
        const written = Effect.promise(() => super.updateProjectVersion(tree, newVersion));
        const tracked = Effect.flatMap(Schema.decodeUnknownEffect(Options)(this.finalConfigForProject.versionActionsOptions), ({ releases }) => Effect.forEach(releases, (files) => shipped(tree, newVersion, files)));
        return runtime.runPromise(Effect.zipWith(written, tracked, (logs, moved) => [...logs, ...moved.flat()]));
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export default DotnetVersionActions;
