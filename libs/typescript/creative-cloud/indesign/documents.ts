import type { Document, Layer, MasterSpread, Page, Spread } from 'adobe:indesign';
import { Array, Effect, Option, Predicate, Record, Schema, Struct } from 'effect';
import { Classes } from './objects/classes.ts';
import { Reference, read, Specifier } from './objects/specifiers.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Layout = Page | Spread | MasterSpread | Layer;

// --- [MODELS] --------------------------------------------------------------------------

const hostOwned = (name: string): boolean => name.startsWith('[') && name.endsWith(']');
class DocumentView extends Schema.TaggedClass<DocumentView, { readonly brand: unique symbol }>()('Document', {
    ...Reference.fields,
    saved: Schema.Boolean,
    modified: Schema.Boolean,
    path: Schema.OptionFromOptionalKey(Schema.String),
    intent: Schema.String,
    facingPages: Schema.Boolean,
    size: Schema.Struct({ width: Schema.Finite, height: Schema.Finite }).annotate({ description: 'Default page size in points' }),
    units: Schema.Struct({ horizontal: Schema.String, vertical: Schema.String, origin: Schema.String }).annotate({ description: 'Ruler units and origin the user sees; values this library returns stay in points' }),
    pages: Schema.Int,
    spreads: Schema.Int,
    parents: Schema.Array(Reference.pipe(Schema.fieldsAssign({ pages: Schema.Int }))).annotate({ description: 'Parent (master) spreads' }),
    sections: Schema.Array(Reference.pipe(Schema.fieldsAssign({ start: Specifier, length: Schema.Int, pageNumberStart: Schema.Int, continueNumbering: Schema.Boolean }))),
    layers: Schema.Array(Reference.pipe(Schema.fieldsAssign({ visible: Schema.Boolean, locked: Schema.Boolean, printable: Schema.Boolean }))),
    stories: Schema.Struct({ count: Schema.Int, overset: Schema.Array(Specifier) }),
    links: Schema.Record(Schema.String, Schema.Int).annotate({ description: 'Link count per LinkStatus member' }),
    fonts: Schema.Array(Schema.Struct({ name: Schema.String, status: Schema.String })).annotate({ description: 'Fonts the document uses with their FontStatus' }),
    swatches: Schema.Array(Reference.pipe(Schema.fieldsAssign({ hostOwned: Schema.Boolean.annotate({ description: 'A bracketed name marks a swatch InDesign owns and refuses to remove' }) }))),
    styles: Schema.Struct(Struct.Record(['paragraph', 'character', 'object', 'table', 'cell'], Schema.Int)).annotate({ description: 'Style counts, grouped styles included' }),
    preflight: Schema.Struct({ profile: Schema.String, enabled: Schema.Boolean }),
}) {
    static readonly guard = ({ document }: Classes['Service']) => document;
    static readonly of = Effect.fn('of')(function* (found: Document) {
        const { reference } = yield* Classes;
        const path = yield* found.saved ? Effect.asSome(Effect.promise(() => found.fullName)) : Effect.succeedNone;
        const { documentPreferences: preferences, viewPreferences: view, preflightOptions: preflight } = found;
        return yield* read(DocumentView, () => ({
            _tag: 'Document',
            ...reference(found),
            saved: found.saved,
            modified: found.modified,
            path: Option.getOrUndefined(Option.map(path, Struct.get('nativePath'))),
            intent: preferences.intent,
            facingPages: preferences.facingPages,
            size: { width: preferences.pageWidth, height: preferences.pageHeight },
            units: { horizontal: view.horizontalMeasurementUnits, vertical: view.verticalMeasurementUnits, origin: view.rulerOrigin },
            pages: found.pages.length,
            spreads: found.spreads.length,
            parents: found.masterSpreads
                .everyItem()
                .getElements()
                .map((each) => ({ ...reference(each), pages: each.pages.length })),
            sections: found.sections
                .everyItem()
                .getElements()
                .map((each) => ({ ...reference(each), start: each.pageStart, length: each.length, pageNumberStart: each.pageNumberStart, continueNumbering: each.continueNumbering })),
            layers: found.layers
                .everyItem()
                .getElements()
                .map((each) => ({ ...reference(each), visible: each.visible, locked: each.locked, printable: each.printable })),
            stories: { count: found.stories.length, overset: Array.flatMap(Array.zip(found.stories.everyItem().overflows, found.stories.everyItem().id), ([overflows, id]) => (overflows === true ? [found.stories.itemByID(id)] : [])) },
            links: Record.map(Array.groupBy(Array.ensure(found.links.everyItem().status), String), Array.length),
            fonts: Array.zipWith(found.fonts.everyItem().name, found.fonts.everyItem().status, (name, status) => ({ name, status })),
            swatches: found.swatches
                .everyItem()
                .getElements()
                .map((each) => ({ ...reference(each), hostOwned: hostOwned(each.name) })),
            styles: { paragraph: found.allParagraphStyles.length, character: found.allCharacterStyles.length, object: found.allObjectStyles.length, table: found.allTableStyles.length, cell: found.allCellStyles.length },
            preflight: { profile: Predicate.isString(preflight.preflightWorkingProfile) ? preflight.preflightWorkingProfile : preflight.preflightWorkingProfile.name, enabled: !preflight.preflightOff },
        }));
    });
}
class LayoutView extends Schema.TaggedClass<LayoutView, { readonly brand: unique symbol }>()('Layout', {
    ...Reference.fields,
    position: Schema.Int.annotate({ description: 'documentOffset of a page, index of a spread, parent, or layer' }),
    pages: Schema.Array(Reference.pipe(Schema.fieldsAssign({ side: Schema.String }))).annotate({ description: 'Pages of a spread or parent in order' }),
    parent: Schema.OptionFromOptionalKey(Reference).annotate({ description: 'Applied parent spread, or the parent a parent inherits from' }),
    items: Schema.Struct({ own: Schema.Int, inherited: Schema.OptionFromOptionalKey(Schema.Int) }).annotate({ description: 'Page items held, and parent items shown on a page' }),
    settings: Schema.JsonObject.annotate({ description: 'Native members by kind: page side, bounds, section, and margins; spread parent item display; parent prefix and base name; layer visibility, locking, printing, wrap, and color' }),
}) {
    static readonly guard = ({ owning }: Classes['Service']) => {
        const guards = [owning<Page>('documentOffset'), owning<Spread>('showMasterItems'), owning<MasterSpread>('namePrefix'), owning<Layer>('layerColor')];
        return (value: unknown): value is Layout => guards.some((guard) => guard(value));
    };
    static readonly of = Effect.fn('of')(function* (found: Layout) {
        const { reference, host, projected } = yield* Classes;
        const settings: Record<Layout['constructorName'], readonly string[]> = {
            Page: ['side', 'bounds', 'appliedSection', 'marginPreferences'],
            Spread: ['showMasterItems', 'allowPageShuffle'],
            MasterSpread: ['namePrefix', 'baseName'],
            Layer: ['visible', 'locked', 'printable', 'ignoreWrap', 'layerColor'],
        };
        return yield* read(LayoutView, () => ({
            _tag: 'Layout',
            ...reference(found),
            position: 'documentOffset' in found ? found.documentOffset : found.index,
            pages:
                'pages' in found
                    ? found.pages
                          .everyItem()
                          .getElements()
                          .map((each) => ({ ...reference(each), side: each.side }))
                    : [],
            parent: 'appliedMaster' in found && host(found.appliedMaster) ? reference(found.appliedMaster) : undefined,
            items: { own: found.pageItems.length, inherited: 'masterPageItems' in found ? found.masterPageItems.length : undefined },
            settings: Record.fromIterableWith(settings[found.constructorName], (name) => [name, projected(Reflect.get(found, name))]),
        }));
    });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { DocumentView, hostOwned, LayoutView };
