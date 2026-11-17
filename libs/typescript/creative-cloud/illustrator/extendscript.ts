import { assign, declaration, illegalArgument, preserve } from '@rasm/extendscript/host';
import { type Containment, read } from '@rasm/object-model/host';

// --- [TYPES] ---------------------------------------------------------------------------

declare global {
    interface XML {
        readonly [index: number]: XML | undefined;
    }
}

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
const named = (node: XML, name: string): XML[] => {
    const list = node.descendants();
    const items: XML[] = [];
    for (let index = 0; index < list.length(); index += 1) {
        const item = list[index];
        if (item?.localName() === name) {
            items.push(item);
        }
    }
    return items;
};
const parsed = (path: string): XML | undefined => {
    const file = new File(path);
    file.encoding = 'utf-8';
    const text = file.exists && file.open('r') ? file.read() : '';
    file.close();
    return text === '' ? undefined : new XML(text.slice(text.indexOf('<dictionary')));
};
const declared = (name: string | undefined): unknown => {
    const library = `${app.name} ${app.version.split('.')[0]} Type Library`;
    const found: Record<string, unknown> = {};
    for (const folder of new Folder(`${Folder.appData.fsName}/Adobe/Scripting Dictionaries CC`).getFiles()) {
        const dictionary = parsed(`${folder.fsName}/omv.xml`);
        const held = dictionary !== undefined && named(dictionary, 'map').some((map) => String(map.attribute('title')) === library);
        for (const definition of held ? named(dictionary, 'classdef').filter((each) => String(each.attribute('enumeration')) === 'true') : []) {
            const enumeration = String(definition.attribute('name'));
            found[enumeration] = { name: enumeration, properties: [], methods: [], staticProperties: named(definition, 'property').map((member) => ({ name: String(member.attribute('name')), description: String(named(member, 'shortdesc')[0] ?? '') })) };
        }
    }
    return declaration($.global, app, 0, name, found);
};
const session = () => preserve(app, { userInteractionLevel: UserInteractionLevel.DONTDISPLAYALERTS });
const open = (path: string): object => app.open(new File(path));
const rasterize = (found: object, longestSide: number, options: Readonly<Record<string, unknown>> | undefined, path: string): void => {
    const document = owner(found);
    const rect = is('Artboard')(found) ? found.artboardRect : is('Document')(found) || placed(found) ? found.visibleBounds : undefined;
    if (document === undefined || rect === undefined) {
        throw illegalArgument(`${String(read(found, 'typename'))} has no bounds, rasterize takes a Document, Artboard, or page item`);
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

export { containment, declared, open, rasterize, session };
