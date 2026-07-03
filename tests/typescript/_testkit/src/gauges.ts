import { FileSystem, Path } from '@effect/platform';
import { Array, Data, Effect, Option, pipe } from 'effect';
import ts from 'typescript';

// --- [TYPES] -----------------------------------------------------------------------------

declare namespace Imports {
    type Specifier = { readonly specifier: string; readonly typeOnly: boolean };
    type Module = { readonly path: string; readonly specifiers: ReadonlyArray<Specifier> };
    type Violation = {
        readonly path: string;
        readonly specifier: string;
        readonly kind: 'banned' | 'edge';
        readonly typeOnly: boolean;
    };
    type Rules = {
        readonly banned: ReadonlyArray<RegExp>;
        readonly zone: (path: string) => Option.Option<string>;
        readonly zoneOf: (specifier: string, from: string) => Option.Option<string>;
        readonly permitted: ReadonlyArray<readonly [from: string, to: string]>;
    };
}

type Audit = Data.TaggedEnum<{
    Unsupported: { readonly reason: string };
    Audited: { readonly scanned: number; readonly violations: ReadonlyArray<Imports.Violation> };
}>;
const Audit: Data.TaggedEnum.Constructor<Audit> = Data.taggedEnum<Audit>();

// --- [CONSTANTS] -------------------------------------------------------------------------

const _SNAP = { home: '__snapshots__', extension: '.snap' } as const;

// --- [ERRORS] ----------------------------------------------------------------------------

class GaugeFault extends Data.TaggedError('GaugeFault')<{ readonly reason: 'unreadable'; readonly detail: string }> {}

// --- [OPERATIONS] ------------------------------------------------------------------------

const _walked = (root: string): Effect.Effect<ReadonlyArray<string>, GaugeFault, FileSystem.FileSystem> =>
    Effect.mapError(
        Effect.flatMap(FileSystem.FileSystem, (fs) => fs.readDirectory(root, { recursive: true })),
        (fault) => new GaugeFault({ reason: 'unreadable', detail: fault.message }),
    );

const _specifiers = (path: string, text: string): ReadonlyArray<Imports.Specifier> => {
    // BOUNDARY ADAPTER: the compiler walk is a platform callback seam; the accumulator detaches immutable at the return.
    const source = ts.createSourceFile(path, text, ts.ScriptTarget.Latest, false);
    const found: Array<Imports.Specifier> = [];
    ts.forEachChild(source, (node) => {
        if (ts.isImportDeclaration(node) && ts.isStringLiteral(node.moduleSpecifier)) {
            found.push({ specifier: node.moduleSpecifier.text, typeOnly: node.importClause?.isTypeOnly === true });
        } else if (ts.isExportDeclaration(node) && node.moduleSpecifier !== undefined && ts.isStringLiteral(node.moduleSpecifier)) {
            found.push({ specifier: node.moduleSpecifier.text, typeOnly: node.isTypeOnly });
        }
    });
    return found;
};

// Standing snapshot-hygiene gauge: a snapshot whose owning spec no longer exists is stale evidence, flagged for deletion.
const Snapshots = {
    audit: (
        root: string,
    ): Effect.Effect<{ readonly scanned: number; readonly orphans: ReadonlyArray<string> }, GaugeFault, FileSystem.FileSystem | Path.Path> =>
        Effect.gen(function* () {
            const path = yield* Path.Path;
            const entries = yield* _walked(root);
            const present = new Set(entries);
            const snapshots = Array.filter(entries, (entry) => entry.endsWith(_SNAP.extension) && entry.includes(_SNAP.home));
            const orphans = Array.filter(snapshots, (snapshot) => {
                const owner = path.join(path.dirname(path.dirname(snapshot)), path.basename(snapshot, _SNAP.extension));
                return !present.has(owner);
            });
            return { scanned: snapshots.length, orphans };
        }),
} as const;

// Import-graph gauge engine: pure over supplied sources, parameterized by zone projection and permitted-edge rows.
const Imports = {
    scan: (sources: ReadonlyArray<{ readonly path: string; readonly text: string }>): ReadonlyArray<Imports.Module> =>
        Array.map(sources, (source) => ({ path: source.path, specifiers: _specifiers(source.path, source.text) })),
    load: (root: string): Effect.Effect<ReadonlyArray<Imports.Module>, GaugeFault, FileSystem.FileSystem | Path.Path> =>
        Effect.gen(function* () {
            const fs = yield* FileSystem.FileSystem;
            const path = yield* Path.Path;
            const entries = yield* _walked(root);
            const files = Array.filter(entries, (entry) => entry.endsWith('.ts') && !entry.endsWith('.d.ts'));
            return Imports.scan(
                yield* Effect.forEach(files, (file) =>
                    Effect.map(
                        Effect.mapError(
                            fs.readFileString(path.join(root, file)),
                            (fault) => new GaugeFault({ reason: 'unreadable', detail: fault.message }),
                        ),
                        (text) => ({ path: file, text }),
                    ),
                ),
            );
        }),
    // An empty module set is UNSUPPORTED, never a vacuous green: the gauge names what it could not audit.
    verdict: (modules: ReadonlyArray<Imports.Module>, rules: Imports.Rules): Audit =>
        Array.isNonEmptyReadonlyArray(modules)
            ? Audit.Audited({
                  scanned: modules.length,
                  violations: Array.flatMap(modules, (module) =>
                      Array.filterMap(module.specifiers, (entry) =>
                          Array.some(rules.banned, (pattern) => pattern.test(entry.specifier))
                              ? Option.some<Imports.Violation>({
                                    path: module.path,
                                    specifier: entry.specifier,
                                    kind: 'banned',
                                    typeOnly: entry.typeOnly,
                                })
                              : pipe(
                                    Option.all([rules.zone(module.path), rules.zoneOf(entry.specifier, module.path)]),
                                    Option.filter(
                                        ([from, to]) =>
                                            from !== to && !Array.some(rules.permitted, ([source, target]) => source === from && target === to),
                                    ),
                                    Option.map(
                                        (): Imports.Violation => ({
                                            path: module.path,
                                            specifier: entry.specifier,
                                            kind: 'edge',
                                            typeOnly: entry.typeOnly,
                                        }),
                                    ),
                                ),
                      ),
                  ),
              })
            : Audit.Unsupported({ reason: 'zero modules scanned' }),
} as const;

// --- [EXPORTS] ---------------------------------------------------------------------------

export { Audit, GaugeFault, Imports, Snapshots };
