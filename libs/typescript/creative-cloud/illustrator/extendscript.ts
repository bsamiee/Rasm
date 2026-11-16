import { assign, illegalArgument, preserve } from '@rasm/extendscript/host';
import { type Containment, read } from '@rasm/object-model/host';

// --- [OPERATIONS] ----------------------------------------------------------------------

const is =
    <Typename extends 'Document' | 'Layer' | 'Artboard'>(typename: Typename) =>
    (value: unknown): value is { Document: Document; Layer: Layer; Artboard: Artboard }[Typename] =>
        read(value, 'typename') === typename;
const placed = (value: unknown): value is PageItem => read(value, 'layer') !== undefined && read(value, 'visibleBounds') !== undefined;
const owner = (value: unknown): Document | undefined => (value === undefined || value === app ? undefined : is('Document')(value) ? value : owner(read(value, 'parent')));
const containment: Containment = {
    root: app,
    type: (value) => String(read(value, 'typename')),
    kinds: {
        document: { collection: 'documents', keys: ['fullName'], is: is('Document') },
        pageItem: { collection: 'pageItems', keys: ['uuid'], is: placed, owner },
        artboard: { collection: 'artboards', keys: [], is: is('Artboard'), owner },
        layer: { collection: 'layers', keys: [], is: is('Layer'), owner: (value) => read(value, 'parent') },
    },
};
const session = () => preserve(app, { userInteractionLevel: UserInteractionLevel.DONTDISPLAYALERTS });
const open = (path: string): object => app.open(new File(path));
const rasterize = (found: object, { target, longestSide, options }: { readonly target: string; readonly longestSide: number; readonly options?: Readonly<Record<string, unknown>> }, path: string): void => {
    const document = owner(found);
    const rect = is('Artboard')(found) ? found.artboardRect : is('Document')(found) || placed(found) ? found.visibleBounds : undefined;
    if (document === undefined || rect === undefined) {
        throw illegalArgument(`${target} names a ${String(read(found, 'typename'))} with no bounds`);
    }
    const percent = 100;
    const scale = (longestSide / Math.max(rect[2] - rect[0], rect[1] - rect[3])) * percent;
    const { artboards } = document;
    const active = artboards.getActiveArtboardIndex();
    const added = artboards.add(rect);
    artboards.setActiveArtboardIndex(artboards.length - 1);
    try {
        document.exportFile(new File(path), ExportType.PNG24, assign(new ExportOptionsPNG24(), options, { artBoardClipping: true, horizontalScale: scale, verticalScale: scale }));
    } finally {
        added.remove();
        artboards.setActiveArtboardIndex(active);
    }
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { containment, open, rasterize, session };
