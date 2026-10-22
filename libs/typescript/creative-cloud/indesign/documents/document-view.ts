import type { Document } from 'adobe:indesign';
import { Array, Effect, flow, Option, Predicate, Record, Schema, Struct } from 'effect';
import { LayerView } from '#/documents/layer-view';
import { read } from '#/dom/encoding';
import { file } from '#/dom/files';
import type { Model } from '#/dom/model';
import { Reference, Specifier } from '#/dom/reference';
import { ParagraphAttributes } from '#/styles/paragraph-attributes';

// --- [MODELS] --------------------------------------------------------------------------

class DocumentView extends Schema.TaggedClass<DocumentView, { readonly brand: unique symbol }>()('Document', {
    ...Reference.fields,
    saved: Schema.Boolean,
    modified: Schema.Boolean,
    path: Schema.OptionFromOptionalKey(Schema.String),
    windows: Schema.Array(Specifier),
    intent: Schema.String,
    facingPages: Schema.Boolean,
    size: Schema.Struct({ width: Schema.Finite, height: Schema.Finite }).annotate({ description: 'Default page size in points' }),
    units: Schema.Struct({ horizontal: Schema.String, vertical: Schema.String, origin: Schema.String }).annotate({ description: 'Ruler units and origin the user sees; values this library returns stay in points' }),
    pages: Schema.Int,
    spreads: Schema.Int,
    parents: Schema.Array(Reference.mapFields(Struct.assign({ pages: Schema.Int }))).annotate({ description: 'Parent (master) spreads' }),
    sections: Schema.Array(Reference.mapFields(Struct.assign({ start: Specifier, length: Schema.Int, pageNumberStart: Schema.Int, continueNumbering: Schema.Boolean }))),
    layers: Schema.Array(Reference.mapFields(Struct.assign(Struct.pick(LayerView.fields, ['visible', 'locked', 'printable'])))),
    stories: Schema.Struct({ count: Schema.Int, overset: Schema.Array(Specifier) }),
    links: Schema.Record(Schema.String, Schema.Int).annotate({ description: 'Link count per LinkStatus member' }),
    fonts: Schema.Array(Schema.Struct({ name: Schema.String, status: Schema.String })).annotate({ description: 'Fonts the document uses with their FontStatus' }),
    swatches: Schema.Array(Reference),
    styles: Schema.Struct(Struct.Record(['paragraph', 'character', 'object', 'table', 'cell'], Schema.Int)).annotate({ description: 'Style counts, grouped styles included' }),
    preflightOptions: Schema.Struct({
        ...Struct.Record(['preflightOff', 'preflightIncludeObjectsOnPasteboard', 'preflightIncludeNonprintingObjects'], Schema.Boolean),
        ...Struct.Record(['preflightProfilePolicy', 'preflightWorkingProfile', 'preflightScope', 'preflightWhichLayers'], Schema.String),
    }),
    activeProcess: Schema.OptionFromOptionalKey(Reference).annotate({ description: 'Live preflight process, absent while live preflight is off' }),
    textDefaults: ParagraphAttributes.annotate({ description: 'Paragraph attributes of the document text defaults: tab stops, GREP, nested, and line styles, bullets and numbering' }),
}) {
    static readonly Row = Schema.Struct(Struct.pick(DocumentView.fields, ['type', 'specifier', 'name', 'valid', 'saved', 'modified', 'path', 'windows']));
    static readonly viewed = ({ document }: Model['Service']) => flow(Option.liftPredicate(document), Option.map(DocumentView.of));
    static readonly overset = ({ stories }: Document): readonly string[] => Array.flatMap(Array.zip(stories.everyItem().overflows, stories.everyItem().id), ([overflows, id]) => (overflows === true ? [stories.itemByID(id).toSpecifier()] : []));
    static readonly row = Effect.fn('DocumentView.row')(function* (document: Document) {
        const { saved } = document;
        const path = yield* Effect.when(file(document, 'fullName'), Effect.succeed(saved));
        return { ...Reference.of(document), saved, modified: document.modified, path: Option.flatten(path), windows: Array.map(document.windows.everyItem().getElements(), (window) => window.toSpecifier()) };
    });
    static readonly of = (found: Document) =>
        Effect.flatMap(DocumentView.row(found), (row) =>
            read(DocumentView, found.toSpecifier(), () => {
                const { documentPreferences: preferences, viewPreferences: view, preflightOptions: preflight } = found;
                return {
                    _tag: 'Document',
                    ...row,
                    intent: preferences.intent,
                    facingPages: preferences.facingPages,
                    size: { width: preferences.pageWidth, height: preferences.pageHeight },
                    units: { horizontal: view.horizontalMeasurementUnits, vertical: view.verticalMeasurementUnits, origin: view.rulerOrigin },
                    pages: found.pages.length,
                    spreads: found.spreads.length,
                    parents: Array.map(found.masterSpreads.everyItem().getElements(), (each) => ({ ...Reference.of(each), pages: each.pages.length })),
                    sections: Array.map(found.sections.everyItem().getElements(), (each) => ({ ...Reference.of(each), start: each.pageStart.toSpecifier(), length: each.length, pageNumberStart: each.pageNumberStart, continueNumbering: each.continueNumbering })),
                    layers: Array.map(found.layers.everyItem().getElements(), (each) => ({ ...Reference.of(each), visible: each.visible, locked: each.locked, printable: each.printable })),
                    stories: { count: found.stories.length, overset: DocumentView.overset(found) },
                    links: Record.map(Array.groupBy(found.links.everyItem().status, String), Array.length),
                    fonts: Array.zipWith(found.fonts.everyItem().name, found.fonts.everyItem().status, (name, status) => ({ name, status })),
                    swatches: found.swatches.everyItem().getElements(),
                    styles: { paragraph: found.allParagraphStyles.length, character: found.allCharacterStyles.length, object: found.allObjectStyles.length, table: found.allTableStyles.length, cell: found.allCellStyles.length },
                    preflightOptions: {
                        ...Record.fromIterableWith(Struct.keys(DocumentView.fields.preflightOptions.fields), (key) => [key, preflight[key]]),
                        preflightWorkingProfile: Predicate.isString(preflight.preflightWorkingProfile) ? preflight.preflightWorkingProfile : preflight.preflightWorkingProfile.name,
                    },
                    activeProcess: preflight.preflightOff ? Option.none() : Option.some(found.activeProcess),
                    textDefaults: ParagraphAttributes.of(found.textDefaults),
                };
            }),
        );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { DocumentView };
