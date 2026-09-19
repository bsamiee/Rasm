// --- [IMPORTS] -------------------------------------------------------------------------

import { Array, Schema, Struct } from 'effect';
import { HostRejection } from '../errors.ts';
import { AbsolutePath, OptionalInt, PageIndex } from '../values.ts';
import { enumerations } from './indesign.ts';

// --- [MODELS] --------------------------------------------------------------------------

const Asset: Schema.Struct<{
    readonly id: Schema.Int;
    readonly name: Schema.String;
    readonly assetType: Schema.Literals<Array<keyof typeof enumerations.AssetType>>;
    readonly description: Schema.String;
    readonly label: Schema.String;
    readonly date: Schema.String;
}> = Schema.Struct({
    id: Schema.Int,
    name: Schema.String,
    assetType: Schema.Literals(Struct.keys(enumerations.AssetType)),
    description: Schema.String,
    label: Schema.String,
    date: Schema.String,
});

const Resource: Schema.Struct<{ readonly specifier: Schema.String; readonly name: Schema.String; readonly type: Schema.String }> = Schema.Struct({
    specifier: Schema.String,
    name: Schema.String,
    type: Schema.String,
});

const Tag: Schema.Struct<{ readonly exportType: Schema.NonEmptyString; readonly exportTag: Schema.String; readonly exportClass: Schema.String; readonly exportAttributes: Schema.String }> =
    Schema.Struct({
        exportType: Schema.NonEmptyString,
        exportTag: Schema.String,
        exportClass: Schema.String,
        exportAttributes: Schema.String,
    });

const channel: Schema.Number = Schema.Number.pipe(Schema.check(Schema.isBetween({ minimum: 0, maximum: 255 })));

const Layer: Schema.Struct<{
    readonly name: Schema.NonEmptyString;
    readonly layerColor: Schema.Union<readonly [Schema.Tuple<readonly [Schema.Number, Schema.Number, Schema.Number]>, Schema.Literals<Array<keyof typeof enumerations.UIColors>>]>;
    readonly locked: Schema.Boolean;
    readonly visible: Schema.Boolean;
    readonly printable: Schema.Boolean;
    readonly showGuides: Schema.Boolean;
    readonly lockGuides: Schema.Boolean;
    readonly ignoreWrap: Schema.Boolean;
}> = Schema.Struct({
    name: Schema.NonEmptyString,
    layerColor: Schema.Union([Schema.Tuple([channel, channel, channel]), Schema.Literals(Struct.keys(enumerations.UIColors))]),
    locked: Schema.Boolean,
    visible: Schema.Boolean,
    printable: Schema.Boolean,
    showGuides: Schema.Boolean,
    lockGuides: Schema.Boolean,
    ignoreWrap: Schema.Boolean,
});

// --- [JOBS] ----------------------------------------------------------------------------

const LibraryBodies: Schema.Struct<{
    readonly readLibrary: Schema.Struct<{ readonly path: typeof AbsolutePath }>;
    readonly writeLibrary: Schema.Struct<{
        readonly path: typeof AbsolutePath;
        readonly create: Schema.Boolean;
        readonly documentId: typeof OptionalInt;
        readonly assets: Schema.NonEmptyArray<
            Schema.Struct<{
                readonly itemIds: Schema.NonEmptyArray<Schema.Int>;
                readonly name: Schema.NonEmptyString;
                readonly description: Schema.String;
                readonly label: Schema.String;
                readonly assetType: Schema.OptionFromOptionalKey<Schema.Literals<Array<keyof typeof enumerations.AssetType>>>;
            }>
        >;
    }>;
    readonly placeLibraryAsset: Schema.Struct<{
        readonly path: typeof AbsolutePath;
        readonly assetId: Schema.Int;
        readonly documentId: typeof OptionalInt;
        readonly insertion: Schema.OptionFromOptionalKey<Schema.Struct<{ readonly storyId: Schema.Int; readonly index: Schema.Int }>>;
    }>;
    readonly writeSnippet: Schema.Struct<{ readonly path: typeof AbsolutePath; readonly itemId: Schema.Int; readonly documentId: typeof OptionalInt }>;
    readonly placeSnippet: Schema.Struct<{
        readonly path: typeof AbsolutePath;
        readonly documentId: typeof OptionalInt;
        readonly pageIndex: typeof PageIndex;
        readonly position: Schema.OptionFromOptionalKey<Schema.Tuple<readonly [Schema.Finite, Schema.Finite]>>;
        readonly layerId: typeof OptionalInt;
    }>;
    readonly importStyles: Schema.Struct<{
        readonly path: typeof AbsolutePath;
        readonly documentId: typeof OptionalInt;
        readonly formats: Schema.NonEmptyArray<Schema.Literals<Array<keyof typeof enumerations.ImportFormat>>>;
        readonly strategy: Schema.Literals<Array<keyof typeof enumerations.GlobalClashResolutionStrategy>>;
    }>;
    readonly setExportTags: Schema.Struct<{
        readonly documentId: typeof OptionalInt;
        readonly styles: Schema.NonEmptyArray<
            Schema.Struct<{
                readonly kind: Schema.Literals<readonly ['paragraph', 'character']>;
                readonly id: Schema.Int;
                readonly mappings: Schema.NonEmptyArray<typeof Tag>;
            }>
        >;
    }>;
    readonly setLayers: Schema.Struct<{ readonly documentId: typeof OptionalInt; readonly layers: Schema.NonEmptyArray<typeof Layer> }>;
}> = Schema.Struct({
    readLibrary: Schema.Struct({ path: AbsolutePath }),
    writeLibrary: Schema.Struct({
        path: AbsolutePath,
        create: Schema.Boolean,
        documentId: OptionalInt,
        assets: Schema.NonEmptyArray(
            Schema.Struct({
                itemIds: Schema.NonEmptyArray(Schema.Int).check(Schema.isUnique()),
                name: Schema.NonEmptyString,
                description: Schema.String,
                label: Schema.String,
                assetType: Schema.OptionFromOptionalKey(Schema.Literals(Struct.keys(enumerations.AssetType))),
            }),
        ),
    }),
    placeLibraryAsset: Schema.Struct({
        path: AbsolutePath,
        assetId: Schema.Int,
        documentId: OptionalInt,
        insertion: Schema.OptionFromOptionalKey(Schema.Struct({ storyId: Schema.Int, index: PageIndex })),
    }),
    writeSnippet: Schema.Struct({ path: AbsolutePath, itemId: Schema.Int, documentId: OptionalInt }),
    placeSnippet: Schema.Struct({
        path: AbsolutePath,
        documentId: OptionalInt,
        pageIndex: PageIndex,
        position: Schema.OptionFromOptionalKey(Schema.Tuple([Schema.Finite, Schema.Finite])),
        layerId: OptionalInt,
    }),
    importStyles: Schema.Struct({
        path: AbsolutePath,
        documentId: OptionalInt,
        formats: Schema.NonEmptyArray(Schema.Literals(Struct.keys(enumerations.ImportFormat))).check(Schema.isUnique()),
        strategy: Schema.Literals(Struct.keys(enumerations.GlobalClashResolutionStrategy)),
    }),
    setExportTags: Schema.Struct({
        documentId: OptionalInt,
        styles: Schema.NonEmptyArray(
            Schema.Struct({
                kind: Schema.Literals(['paragraph', 'character']),
                id: Schema.Int,
                mappings: Schema.NonEmptyArray(Tag).check(
                    Schema.makeFilter((rows) => Array.dedupe(Array.map(rows, Struct.get('exportType'))).length === rows.length, { message: 'Each style maps each export type once.' }),
                ),
            }),
        ).check(
            Schema.makeFilter((rows) => Array.dedupeWith(rows, (first, second) => first.kind === second.kind && first.id === second.id).length === rows.length, {
                message: 'Each native style appears once.',
            }),
        ),
    }),
    setLayers: Schema.Struct({
        documentId: OptionalInt,
        layers: Schema.NonEmptyArray(Layer).check(Schema.makeFilter((rows) => Array.dedupe(Array.map(rows, Struct.get('name'))).length === rows.length, { message: 'Layer names must be unique.' })),
    }),
});

const LibraryData: Schema.Struct<{ readonly path: typeof AbsolutePath; readonly assets: Schema.$Array<typeof Asset> }> = Schema.Struct({ path: AbsolutePath, assets: Schema.Array(Asset) });
const Items: Schema.Struct<{ readonly items: Schema.$Array<typeof Resource> }> = Schema.Struct({ items: Schema.Array(Resource) });

const LibraryResults: Schema.Struct<{
    readonly readLibrary: typeof LibraryData;
    readonly writeLibrary: Schema.Struct<{
        readonly path: typeof AbsolutePath;
        readonly assets: Schema.$Array<typeof Asset>;
        readonly stored: Schema.$Array<Schema.Struct<{ readonly index: Schema.Int; readonly id: Schema.Int }>>;
        readonly rejected: Schema.$Array<Schema.Struct<{ readonly index: Schema.Int; readonly reason: typeof HostRejection }>>;
    }>;
    readonly placeLibraryAsset: Schema.Struct<{ readonly items: Schema.$Array<typeof Resource> }>;
    readonly writeSnippet: Schema.Struct<{ readonly path: typeof AbsolutePath; readonly item: typeof Resource }>;
    readonly placeSnippet: Schema.Struct<{ readonly items: Schema.$Array<typeof Resource> }>;
    readonly importStyles: Schema.Struct<{ readonly styles: Schema.$Array<typeof Resource> }>;
    readonly setExportTags: Schema.Struct<{ readonly styles: Schema.$Array<Schema.Struct<{ readonly style: typeof Resource; readonly mappings: Schema.$Array<typeof Tag> }>> }>;
    readonly setLayers: Schema.Struct<{ readonly layers: Schema.$Array<Schema.Struct<{ readonly id: Schema.Int; readonly index: Schema.Int; readonly properties: typeof Layer }>> }>;
}> = Schema.Struct({
    readLibrary: LibraryData,
    writeLibrary: Schema.Struct({
        ...LibraryData.fields,
        stored: Schema.Array(Schema.Struct({ index: Schema.Int, id: Schema.Int })),
        rejected: Schema.Array(Schema.Struct({ index: Schema.Int, reason: HostRejection })),
    }),
    placeLibraryAsset: Items,
    writeSnippet: Schema.Struct({ path: AbsolutePath, item: Resource }),
    placeSnippet: Items,
    importStyles: Schema.Struct({ styles: Items.fields.items }),
    setExportTags: Schema.Struct({ styles: Schema.Array(Schema.Struct({ style: Resource, mappings: Schema.Array(Tag) })) }),
    setLayers: Schema.Struct({ layers: Schema.Array(Schema.Struct({ id: Schema.Int, index: Schema.Int, properties: Layer })) }),
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { LibraryBodies, LibraryResults };
