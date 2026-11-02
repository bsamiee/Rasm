import { handle, illegal } from '@rasm/cep/extendscript';

// --- [OPERATIONS] ----------------------------------------------------------------------

const owner = (found: PageItem): Document | undefined => {
    let above: unknown = found.layer;
    while (typeof above === 'object' && above !== null && 'typename' in above && above.typename === 'Layer' && 'parent' in above) {
        above = above.parent;
    }
    for (const document of app.documents) {
        if (document === above) {
            return document;
        }
    }
    return undefined;
};
const located = (found: unknown): { readonly document: Document; readonly index: number } | undefined => {
    for (const document of app.documents) {
        if (document === found) {
            return { document, index: document.artboards.getActiveArtboardIndex() };
        }
        for (let index = 0; index < document.artboards.length; index += 1) {
            if (document.artboards[index] === found) {
                return { document, index };
            }
        }
    }
    return undefined;
};

// --- [COMPOSITION] ---------------------------------------------------------------------

// biome-ignore lint/style/noDefaultExport: the CEP bundle assigns the default export to global handle
export default handle({
    base: 0,
    suppressed: (body) => {
        const level = app.userInteractionLevel;
        app.userInteractionLevel = UserInteractionLevel.DONTDISPLAYALERTS;
        try {
            return body();
        } finally {
            app.userInteractionLevel = level;
        }
    },
    undone: (_label, body) => body(),
    open: ({ file: url }, { path }) => {
        const document = app.open(new File(path(url)));
        return { type: document.typename, specifier: `/document[@name="${document.name}"]`, name: document.name, valid: true };
    },
    render: ({ target, longestSide, options, file: url }, { resolve, assign, path }) => {
        const found = resolve(target);
        const percent = 100;
        const pointsPerInch = 72;
        const extent = (bounds: Rect) => Math.max(bounds[2] - bounds[0], bounds[1] - bounds[3]);
        const item = (value: unknown): value is PageItem => typeof value === 'object' && value !== null && 'layer' in value && 'visibleBounds' in value;
        if (item(found)) {
            const owning = owner(found);
            if (owning === undefined) {
                throw illegal(`${target} lies on no document`);
            }
            const capture = assign(assign(new ImageCaptureOptions(), { antiAliasing: true, transparency: true, resolution: (longestSide * pointsPerInch) / extent(found.visibleBounds) }), options ?? {});
            owning.imageCapture(new File(path(url)), found.visibleBounds, capture);
            return;
        }
        const placed = located(found);
        const rect = placed?.document.artboards[placed.index]?.artboardRect;
        if (placed === undefined || rect === undefined) {
            throw illegal(`${target} names no document, artboard, or page item`);
        }
        const scale = (longestSide / extent(rect)) * percent;
        const png = assign(assign(new ExportOptionsPNG24(), { artBoardClipping: true, horizontalScale: scale, verticalScale: scale }), options ?? {});
        const active = placed.document.artboards.getActiveArtboardIndex();
        placed.document.artboards.setActiveArtboardIndex(placed.index);
        try {
            placed.document.exportFile(new File(path(url)), ExportType.PNG24, png);
        } finally {
            placed.document.artboards.setActiveArtboardIndex(active);
        }
    },
});
