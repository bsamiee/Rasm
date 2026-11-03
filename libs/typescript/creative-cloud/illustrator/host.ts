import { handle } from '@rasm/extendscript/handle';

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
    specifier: (value, { step, indexed, read }) => {
        const isDocument = (above: unknown): above is Document => read(above, 'typename') === 'Document';
        const owner = (above: unknown): Document | undefined => {
            if (above === undefined || above === app) {
                return;
            }
            return isDocument(above) ? above : owner(read(above, 'parent'));
        };
        const document = owner(value);
        const uuid = read(value, 'layer') === undefined ? undefined : read(value, 'uuid');
        if (document === undefined) {
            return;
        }
        const reference = step('document', ['fullName', String(document.fullName)]);
        if (typeof uuid === 'string') {
            return `${reference}${step('pageItem', ['uuid', uuid])}`;
        }
        if (document === value) {
            return reference;
        }
        let found: string | undefined;
        for (const [kind, collection] of [
            ['artboard', document.artboards],
            ['layer', document.layers],
        ] as const) {
            for (let index = 0; index < collection.length && found === undefined; index += 1) {
                found = collection[index] === value ? `${reference}${indexed(kind, index)}` : undefined;
            }
        }
        return found;
    },
    open: ({ file: url }, { file }) => app.open(file(url)),
    render: ({ target, longestSide, options, file: url }, { resolve, assign, file, read, illegal }) => {
        const isDocument = (value: unknown): value is Document => read(value, 'typename') === 'Document';
        const isArtboard = (value: unknown): value is Artboard => read(value, 'typename') === 'Artboard';
        const isPageItem = (value: unknown): value is PageItem => read(value, 'layer') !== undefined && read(value, 'visibleBounds') !== undefined;
        const chain = resolve(target);
        const [document] = chain;
        const found = chain[chain.length - 1];
        const bounds = isDocument(found) || isPageItem(found) ? found.visibleBounds : undefined;
        const rect = isArtboard(found) ? found.artboardRect : bounds;
        if (!isDocument(document) || rect === undefined) {
            throw illegal(`${target} names a ${String(read(found, 'typename'))} with no bounds`);
        }
        const percent = 100;
        const longest = Math.max(rect[2] - rect[0], rect[1] - rect[3]);
        const scale = (longestSide / longest) * percent;
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
