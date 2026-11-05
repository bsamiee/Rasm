import type { Tree } from '@nx/devkit';
import { Array, Effect, Struct } from 'effect';
import Builder from 'fast-xml-builder';
import { XMLParser } from 'fast-xml-parser';
import { glob } from '#/tools/nx/internal/open-workspace';
import { runPlugin } from '#/tools/nx/internal/plugin-runtime';

// --- [CONSTANTS] -----------------------------------------------------------------------

const IMAGESET = /^(?<resources>.+)\/Assets\.xcassets\/(?<mark>.+)Symbol\.imageset\/Contents\.json$/u;
const SVG = { ignoreAttributes: false, preserveOrder: true, format: true, indentBy: '  ', suppressEmptyNode: true } as const;
const MENU_BAR_POINTS = '16';

// --- [OPERATIONS] ----------------------------------------------------------------------

const sized = (master: string): string =>
    `${new Builder(SVG).build(
        new XMLParser({
            ...SVG,
            updateTag: (tag, _, attributes) => {
                Object.assign(attributes, tag === 'svg' ? { '@_width': MENU_BAR_POINTS, '@_height': MENU_BAR_POINTS } : {});
                return tag;
            },
        }).parse(master),
    )}`.trimStart();
const glyphRow = Effect.fnUntraced(function* (tree: Tree, imageset: string) {
    const { resources, mark } = yield* Effect.fromNullishOr(IMAGESET.exec(imageset)?.groups);
    const master = yield* Effect.fromNullishOr(tree.read(`${resources}/AppIcon.icon/Assets/${mark}.svg`, 'utf-8'));
    const file = `${resources}/Assets.xcassets/${mark}Symbol.imageset/${mark}Symbol.svg`;
    return { file, text: tree.read(file, 'utf-8'), next: sized(master) };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const menuBarGlyph = Effect.fnUntraced(function* (tree: Tree) {
    const rows = yield* Effect.forEach(yield* glob(tree.root, ['**/Assets.xcassets/*Symbol.imageset/Contents.json']), (imageset) => glyphRow(tree, imageset));
    const drifted = rows.filter(({ text, next }) => next !== text);
    yield* Effect.forEach(drifted, ({ file, next }) => Effect.sync(tree.write.bind(tree, file, next)), { discard: true });
    return Array.match(drifted, { onEmpty: () => undefined, onNonEmpty: (changed) => ({ outOfSyncMessage: 'Menu bar glyph differs from its AppIcon.icon layer master', outOfSyncDetails: changed.map(Struct.get('file')) }) });
}, runPlugin);

// --- [EXPORTS] -------------------------------------------------------------------------

export { menuBarGlyph };
