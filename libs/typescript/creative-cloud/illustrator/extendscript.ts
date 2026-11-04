import { assignProperties, fileFromUrl, handleEnvelope, illegalArgument, readProperty, specifierStep } from '@rasm/extendscript/extendscript';

// --- [TYPES] ---------------------------------------------------------------------------

interface Typenames {
    readonly Document: Document;
    readonly Layer: Layer;
    readonly Artboard: Artboard;
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const is = <Typename extends keyof Typenames>(typename: Typename, value: unknown): value is Typenames[Typename] => readProperty(value, 'typename') === typename;
const isPageItem = (value: unknown): value is PageItem => readProperty(value, 'layer') !== undefined && readProperty(value, 'visibleBounds') !== undefined;
const owner = (value: unknown): Document | undefined => {
    if (value === undefined || value === app) {
        return undefined;
    }
    return is('Document', value) ? value : owner(readProperty(value, 'parent'));
};
const member = (container: string, kind: string, siblings: Artboards | Layers, value: unknown): string | undefined => {
    for (let index = 0; index < siblings.length; index += 1) {
        if (siblings[index] === value) {
            return `${container}${specifierStep(kind, [index])}`;
        }
    }
    return undefined;
};
const specifier = (value: unknown): string | undefined => {
    const document = owner(value);
    if (document === undefined) {
        return undefined;
    }
    const [reference, parent, uuid] = [specifierStep('document', ['fullName', String(document.fullName)]), readProperty(value, 'parent'), readProperty(value, 'uuid')];
    if (value === document) {
        return reference;
    }
    if (isPageItem(value) && typeof uuid === 'string') {
        return `${reference}${specifierStep('pageItem', ['uuid', uuid])}`;
    }
    if (is('Artboard', value)) {
        return member(reference, 'artboard', document.artboards, value);
    }
    if (!(is('Document', parent) || is('Layer', parent))) {
        return undefined;
    }
    const container = specifier(parent);
    return container === undefined ? undefined : member(container, 'layer', parent.layers, value);
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
    open: (payload) => app.open(fileFromUrl(payload.file)),
    render: (payload, resolve) => {
        const chain = resolve(payload.target);
        const found = chain[chain.length - 1];
        const document = owner(found);
        const bounds = is('Document', found) || isPageItem(found) ? found.visibleBounds : undefined;
        const rect = is('Artboard', found) ? found.artboardRect : bounds;
        if (document === undefined || rect === undefined) {
            throw illegalArgument(`${payload.target} names a ${String(readProperty(found, 'typename'))} with no bounds`);
        }
        const percent = 100;
        const scale = (payload.longestSide / Math.max(rect[2] - rect[0], rect[1] - rect[3])) * percent;
        const { artboards } = document;
        const active = artboards.getActiveArtboardIndex();
        const added = artboards.add(rect);
        artboards.setActiveArtboardIndex(artboards.length - 1);
        try {
            document.exportFile(fileFromUrl(payload.file), ExportType.PNG24, assignProperties(new ExportOptionsPNG24(), payload.options, { artBoardClipping: true, horizontalScale: scale, verticalScale: scale }));
        } finally {
            added.remove();
            artboards.setActiveArtboardIndex(active);
        }
    },
});
