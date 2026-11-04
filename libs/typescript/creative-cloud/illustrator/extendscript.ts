import { assign, file, handleEnvelope, illegal, indexed, read, step } from '@rasm/extendscript/extendscript';

// --- [TYPES] ---------------------------------------------------------------------------

interface ObjectType {
    readonly Document: Document;
    readonly Layer: Layer;
    readonly Artboard: Artboard;
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const is = <Name extends keyof ObjectType>(typename: Name, value: unknown): value is ObjectType[Name] => read(value, 'typename') === typename;
const isPageItem = (value: unknown): value is PageItem => read(value, 'layer') !== undefined && read(value, 'visibleBounds') !== undefined;
const owner = (value: unknown): Document | undefined => {
    if (value === undefined || value === app) {
        return;
    }
    return is('Document', value) ? value : owner(read(value, 'parent'));
};
const specifier = (value: unknown): string | undefined => {
    const [document, parent, uuid] = [owner(value), read(value, 'parent'), read(value, 'uuid')];
    if (document === undefined) {
        return undefined;
    }
    const reference = step('document', ['fullName', String(document.fullName)]);
    if (document === value) {
        return reference;
    }
    if (isPageItem(value) && typeof uuid === 'string') {
        return `${reference}${step('pageItem', ['uuid', uuid])}`;
    }
    const { kind, siblings } = is('Artboard', value) ? { kind: 'artboard', siblings: document.artboards } : { kind: 'layer', siblings: is('Document', parent) || is('Layer', parent) ? parent.layers : undefined };
    for (let index = 0; index < (siblings?.length ?? 0); index += 1) {
        if (siblings?.[index] === value) {
            return `${specifier(parent)}${indexed(kind, index)}`;
        }
    }
    return undefined;
};

// --- [COMPOSITION] ---------------------------------------------------------------------

export default handleEnvelope({
    indexBase: 0,
    dialogs: {
        suppress: () => {
            const level = app.userInteractionLevel;
            app.userInteractionLevel = UserInteractionLevel.DONTDISPLAYALERTS;
            return level;
        },
        restore: (level) => {
            app.userInteractionLevel = level;
        },
    },
    specifier,
    open: ({ file: url }) => app.open(file(url)),
    render: ({ target, longestSide, options, file: url }, resolve) => {
        const chain = resolve(target);
        const found = chain[chain.length - 1];
        const document = owner(found);
        const bounds = is('Document', found) || isPageItem(found) ? found.visibleBounds : undefined;
        const rect = is('Artboard', found) ? found.artboardRect : bounds;
        if (document === undefined || rect === undefined) {
            throw illegal(`${target} names a ${String(read(found, 'typename'))} with no bounds`);
        }
        const percent = 100;
        const scale = (longestSide / Math.max(rect[2] - rect[0], rect[1] - rect[3])) * percent;
        const { artboards } = document;
        const active = artboards.getActiveArtboardIndex();
        const added = artboards.add(rect);
        artboards.setActiveArtboardIndex(artboards.length - 1);
        try {
            document.exportFile(file(url), ExportType.PNG24, assign(new ExportOptionsPNG24(), options, { artBoardClipping: true, horizontalScale: scale, verticalScale: scale }));
        } finally {
            added.remove();
            artboards.setActiveArtboardIndex(active);
        }
    },
});
