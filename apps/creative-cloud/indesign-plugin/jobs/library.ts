// --- [IMPORTS] -------------------------------------------------------------------------

import { AssetType, app, type Book, type Document, ExportFormat, GlobalClashResolutionStrategy, ImportFormat, Library, LocationOptions, type PageItem, UIColors } from 'adobe:indesign';
import { thrown } from '@rasm/creative-cloud-server/client';
import { HostRejection } from '@rasm/creative-cloud-server/errors';
import { LibraryBodies, LibraryResults } from '@rasm/creative-cloud-server/indesign/library';
import { AbsolutePath } from '@rasm/creative-cloud-server/values';
import { Array, Effect, Exit, HashMap, Option, Record, Result, Schema, type Scope, Struct } from 'effect';
import { close, documentFor, fileFor, undoable } from '../host.ts';

// --- [NATIVE RESOURCES] -----------------------------------------------------------------

const libraryFor: (path: AbsolutePath, create: boolean) => Effect.Effect<Library, HostRejection, Scope.Scope> = Effect.fnUntraced(function* (path, create) {
    const file = yield* fileFor(path, create);
    const acquired = yield* Effect.acquireRelease(
        Effect.try({
            try: () => {
                const existing = Array.map([...app.libraries.everyItem().getElements(), ...app.documents.everyItem().getElements(), ...app.books.everyItem().getElements()], (open) =>
                    open.toSpecifier(),
                );
                const opened: Document | Book | Library = create ? app.libraries.add(file) : app.open(file, false);
                return { opened, borrowed: Array.contains(existing, opened.toSpecifier()) };
            },
            catch: thrown,
        }),
        ({ opened, borrowed }) => (borrowed ? Effect.void : close(opened).pipe(Effect.orDie)),
    );
    return yield* Effect.fromOption(
        Option.liftPredicate(acquired.opened, (opened) => opened instanceof Library),
        () => HostRejection.cases.malformedParams.make({ cause: new TypeError('The path did not open an InDesign library') }),
    );
});

const libraryData = (library: Library): Effect.Effect<typeof LibraryResults.fields.readLibrary.Type, HostRejection> =>
    Effect.tryPromise({
        try: async () => ({
            path: AbsolutePath.make((await library.fullName).nativePath),
            assets: Array.map(library.assets.everyItem().getElements(), (asset) => ({
                id: asset.id,
                name: asset.name,
                assetType: Schema.decodeUnknownSync(LibraryResults.fields.readLibrary.fields.assets.value.fields.assetType)(String(asset.assetType)),
                description: asset.description,
                label: asset.label,
                date: asset.date.toISOString(),
            })),
        }),
        catch: thrown,
    });

const resource = (item: Pick<PageItem, 'name' | 'toSpecifier'>): (typeof LibraryResults.fields.placeSnippet.Type)['items'][number] => ({
    specifier: item.toSpecifier(),
    name: item.name,
    type: item.constructor.name,
});

// --- [LIBRARIES] ------------------------------------------------------------------------

const readLibrary = (body: typeof LibraryBodies.fields.readLibrary.Type): Effect.Effect<typeof LibraryResults.fields.readLibrary.Type, HostRejection> =>
    Effect.scoped(Effect.flatMap(libraryFor(body.path, false), libraryData));

const writeLibrary: (body: typeof LibraryBodies.fields.writeLibrary.Type) => Effect.Effect<typeof LibraryResults.fields.writeLibrary.Type, HostRejection> = Effect.fnUntraced(function* (body) {
    const document = yield* documentFor(body.documentId);
    const ids = Array.dedupe(Array.flatMap(body.assets, Struct.get('itemIds')));
    const found = yield* Effect.forEach(ids, (itemId) =>
        Effect.filterOrFail(Effect.try({ try: () => document.pageItems.itemByID(itemId), catch: thrown }), Struct.get('isValid'), () => HostRejection.cases.itemNotFound.make({ itemId })),
    );
    const items = HashMap.fromIterable(Array.zip(ids, found));
    const assets = Array.map(body.assets, (row) => ({
        source: Array.getSomes(Array.map(row.itemIds, (id) => HashMap.get(items, id))),
        properties: { ...Struct.omit(row, ['itemIds', 'assetType']), ...Record.getSomes({ assetType: Option.map(row.assetType, (type) => AssetType[type]) }) },
    }));
    const library = yield* libraryFor(body.path, body.create);
    const [rejected, stored] = yield* Effect.partition(assets, ({ source, properties }, index) =>
        Effect.acquireUseRelease(
            Effect.try({ try: () => library.store(source), catch: thrown }),
            (asset) => Effect.as(Effect.try({ try: Object.assign.bind(Object, asset, properties), catch: thrown }), { index, id: asset.id }),
            (asset, exit) => (Exit.isFailure(exit) ? Effect.try({ try: asset.remove.bind(asset), catch: thrown }) : Effect.void),
        ).pipe(Effect.mapError((reason) => ({ index, reason }))),
    );
    return yield* Effect.map(libraryData(library), (data) => ({ ...data, stored, rejected }));
}, Effect.scoped);

const placeLibraryAsset: (body: typeof LibraryBodies.fields.placeLibraryAsset.Type) => Effect.Effect<typeof LibraryResults.fields.placeLibraryAsset.Type, HostRejection> = Effect.fnUntraced(function* (
    body,
) {
    const document = yield* documentFor(body.documentId);
    const target = Option.match(body.insertion, { onNone: () => document, onSome: ({ storyId, index }) => document.stories.itemByID(storyId).insertionPoints.item(index) });
    const library = yield* libraryFor(body.path, false);
    return yield* undoable('place_library_asset', LibraryResults.fields.placeLibraryAsset, () => ({ items: Array.map(library.assets.itemByID(body.assetId).placeAsset(target), resource) }));
}, Effect.scoped);

// --- [SNIPPETS] -------------------------------------------------------------------------

const writeSnippet: (body: typeof LibraryBodies.fields.writeSnippet.Type) => Effect.Effect<typeof LibraryResults.fields.writeSnippet.Type, HostRejection> = Effect.fnUntraced(function* (body) {
    const document = yield* documentFor(body.documentId);
    const item = yield* Effect.filterOrFail(Effect.try({ try: () => document.pageItems.itemByID(body.itemId), catch: thrown }), Struct.get('isValid'), () =>
        HostRejection.cases.itemNotFound.make({ itemId: body.itemId }),
    );
    const file = yield* fileFor(body.path, true);
    return yield* Effect.try({
        try: () => {
            item.exportFile(ExportFormat.INDESIGN_SNIPPET, file, false);
            return { path: body.path, item: resource(item) };
        },
        catch: thrown,
    });
});

const placeSnippet: (body: typeof LibraryBodies.fields.placeSnippet.Type) => Effect.Effect<typeof LibraryResults.fields.placeSnippet.Type, HostRejection> = Effect.fnUntraced(function* (body) {
    const document = yield* documentFor(body.documentId);
    const page = yield* Effect.filterOrFail(Effect.try({ try: () => document.pages.item(body.pageIndex), catch: thrown }), Struct.get('isValid'), () =>
        HostRejection.cases.pageOutOfRange.make({ index: body.pageIndex, count: document.pages.length }),
    );
    const file = yield* fileFor(body.path, false);
    return yield* undoable('place_snippet', LibraryResults.fields.placeSnippet, () => ({
        items: Array.map(
            page.place(file, Option.getOrUndefined(Option.map(body.position, Array.fromIterable)), Option.getOrUndefined(Option.map(body.layerId, (id) => document.layers.itemByID(id))), false),
            resource,
        ),
    }));
});

// --- [STYLES] ---------------------------------------------------------------------------

const importStyles: (body: typeof LibraryBodies.fields.importStyles.Type) => Effect.Effect<typeof LibraryResults.fields.importStyles.Type, HostRejection> = Effect.fnUntraced(function* (body) {
    const document = yield* documentFor(body.documentId);
    const file = yield* fileFor(body.path, false);
    return yield* undoable('import_styles', LibraryResults.fields.importStyles, () => {
        Array.forEach(body.formats, (format) => document.importStyles(ImportFormat[format], file, GlobalClashResolutionStrategy[body.strategy]));
        return {
            styles: Array.map(
                [
                    ...document.allCharacterStyles,
                    ...document.allParagraphStyles,
                    ...document.allObjectStyles,
                    ...document.allTableStyles,
                    ...document.allCellStyles,
                    ...document.tocStyles.everyItem().getElements(),
                    ...document.strokeStyles.everyItem().getElements(),
                    ...document.namedGrids.everyItem().getElements(),
                ],
                resource,
            ),
        };
    });
});

const setExportTags: (body: typeof LibraryBodies.fields.setExportTags.Type) => Effect.Effect<typeof LibraryResults.fields.setExportTags.Type, HostRejection> = Effect.fnUntraced(function* (body) {
    const document = yield* documentFor(body.documentId);
    const styles = yield* Effect.forEach(body.styles, (row) =>
        Effect.map(
            Effect.filterOrFail(
                Effect.try({ try: () => (row.kind === 'paragraph' ? document.paragraphStyles.itemByID(row.id) : document.characterStyles.itemByID(row.id)), catch: thrown }),
                Struct.get('isValid'),
                () => HostRejection.cases.itemNotFound.make({ itemId: row.id }),
            ),
            (style) => ({ row, style }),
        ),
    );
    const mappings = Array.flatMap(styles, ({ row, style }) => Array.map(row.mappings, (mapping) => ({ mapping, style })));
    const [added, changed] = Array.partition(mappings, (entry) =>
        Option.match(
            Array.findFirst(entry.style.styleExportTagMaps.everyItem().getElements(), (existing) => existing.exportType === entry.mapping.exportType),
            { onNone: () => Result.fail(entry), onSome: (existing) => Result.succeed({ ...entry, existing }) },
        ),
    );
    return yield* undoable('set_export_tags', LibraryResults.fields.setExportTags, () => {
        Array.forEach(changed, ({ existing, mapping }) => Object.assign(existing, Struct.omit(mapping, ['exportType'])));
        Array.forEach(added, ({ style, mapping }) => style.styleExportTagMaps.add(mapping.exportType, mapping.exportTag, mapping.exportClass, mapping.exportAttributes));
        return {
            styles: Array.map(styles, ({ style }) => ({
                style: resource(style),
                mappings: Array.map(style.styleExportTagMaps.everyItem().getElements(), (mapping) =>
                    Schema.decodeUnknownSync(LibraryBodies.fields.setExportTags.fields.styles.value.fields.mappings.value)(mapping),
                ),
            })),
        };
    });
});

// --- [LAYERS] ---------------------------------------------------------------------------

const setLayers: (body: typeof LibraryBodies.fields.setLayers.Type) => Effect.Effect<typeof LibraryResults.fields.setLayers.Type, HostRejection> = Effect.fnUntraced(function* (body) {
    const document = yield* documentFor(body.documentId);
    return yield* undoable('set_layers', LibraryResults.fields.setLayers, () => {
        Array.forEach(Array.reverse(body.layers), (row) => {
            const existing = document.layers.itemByName(row.name);
            const layer = existing.isValid ? existing : document.layers.add({ name: row.name });
            layer.properties = { ...row, layerColor: typeof row.layerColor === 'string' ? UIColors[row.layerColor] : Array.fromIterable(row.layerColor) };
            layer.move(LocationOptions.AT_BEGINNING);
        });
        return {
            layers: Array.map(document.layers.everyItem().getElements(), (layer) => ({
                id: layer.id,
                index: layer.index,
                properties: Schema.decodeUnknownSync(LibraryBodies.fields.setLayers.fields.layers.value)({
                    ...Record.map(Struct.omit(LibraryBodies.fields.setLayers.fields.layers.value.fields, ['layerColor']), (_, key) => layer[key]),
                    layerColor: Array.isArray(layer.layerColor) ? layer.layerColor : String(layer.layerColor),
                }),
            })),
        };
    });
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { importStyles, placeLibraryAsset, placeSnippet, readLibrary, setExportTags, setLayers, writeLibrary, writeSnippet };
