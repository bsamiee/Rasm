import type { Tree } from '@nx/devkit';
import { MiseLock, Package } from '@rasm/eng';
import { Array, Cause, Effect } from 'effect';
import XMLBuilder from 'fast-xml-builder';
import { applyEdits } from 'jsonc-parser';
import { decodeTree, syncGenerator } from '#/sync/tree.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const XML = new XMLBuilder();

// --- [OPERATIONS] ----------------------------------------------------------------------

const written =
    (anchor: (key: string) => RegExp, content: (key: string, value: string) => string = (_, value) => value) =>
    (values: Readonly<Record<string, string>>) =>
    (text: string, file: string) =>
        Effect.validate(Object.entries(values), ([key, value]) =>
            Effect.succeed([...text.matchAll(anchor(RegExp.escape(key)))]).pipe(
                Effect.filterOrFail(Array.isArrayNonEmpty, () => new Cause.IllegalArgumentError(`${file}: found no ${key} setting`)),
                Effect.map(Array.map((row) => ({ offset: row.index, length: row[0].length, content: content(key, value) }))),
            ),
        ).pipe(Effect.map((edits) => applyEdits(text, edits.flat())));
const propertyRows = written(
    (tag) => new RegExp(`<${tag}>[^<]*</${tag}>`, 'gu'),
    (tag, value) => XML.build({ [tag]: value }),
);
const packageRows = written(
    (name) => new RegExp(`(?<=releases/download/v)[^/]+/${name}@[^#]+(?=#)`, 'gu'),
    (name, value) => `${value}/${name}@${value}`,
);
const readSettings = Effect.fnUntraced(function* (tree: Tree) {
    const [manifest, locked] = yield* Effect.all([decodeTree(tree, Package.schema, Package.file), decodeTree(tree, MiseLock.schema, MiseLock.file)]);
    const [author, license, repository] = yield* Effect.all([Effect.fromNullishOr(manifest.author), Effect.fromNullishOr(manifest.license), Effect.fromNullishOr(manifest.repository)]);
    return { author, license, repository, hk: Array.headNonEmpty(locked.tools.hk).version };
});
const writers = {
    'Directory.Build.props': ({ author, license, repository }) => propertyRows({ Authors: author, PackageLicenseExpression: license, PackageProjectUrl: repository }),
    '.config/hk.pkl': ({ hk }) => packageRows({ hk }),
} as const satisfies Record<string, (held: Effect.Success<ReturnType<typeof readSettings>>) => (text: string, file: string) => Effect.Effect<string, unknown>>;
const rows = Effect.fnUntraced(function* (tree: Tree) {
    const held = yield* readSettings(tree);
    return yield* Effect.validate(
        Object.entries(writers),
        Effect.fnUntraced(function* ([file, write]) {
            return { file, next: yield* write(held)(yield* Effect.fromNullishOr(tree.read(file, 'utf-8')), file) };
        }),
    );
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const buildSettings = Object.assign(syncGenerator(`Build settings differ from package.json or ${MiseLock.file}`, rows), { files: [Package.file, MiseLock.file, ...Object.keys(writers)] });

// --- [EXPORTS] -------------------------------------------------------------------------

export { buildSettings };
