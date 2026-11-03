import { assign, file, handle, illegal, indexed, read, step } from '@rasm/extendscript/handle';

// --- [OPERATIONS] ----------------------------------------------------------------------

const is = <A>(typename: string, value: unknown): value is A => read(value, 'typename') === typename;
const isPageItem = (value: unknown): value is PageItem => read(value, 'layer') !== undefined && read(value, 'visibleBounds') !== undefined;
const owner = (value: unknown): Document | undefined => {
    if (value === undefined || value === app) {
        return;
    }
    return is<Document>('Document', value) ? value : owner(read(value, 'parent'));
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
    const [kind, siblings] = is<Artboard>('Artboard', value) ? (['artboard', document.artboards] as const) : (['layer', is<Document>('Document', parent) || is<Layer>('Layer', parent) ? parent.layers : undefined] as const);
    for (let index = 0; index < (siblings?.length ?? 0); index += 1) {
        if (siblings?.[index] === value) {
            return `${specifier(parent)}${indexed(kind, index)}`;
        }
    }
    return undefined;
};

// --- [COMPOSITION] ---------------------------------------------------------------------

export default handle({
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
        const [document] = chain;
        const found = chain[chain.length - 1];
        const bounds = is<Document>('Document', found) || isPageItem(found) ? found.visibleBounds : undefined;
        const rect = is<Artboard>('Artboard', found) ? found.artboardRect : bounds;
        if (!is<Document>('Document', document) || rect === undefined) {
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
