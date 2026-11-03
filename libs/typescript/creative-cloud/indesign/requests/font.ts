import { HostError, Reference } from '@rasm/automation/requests';
import { Source } from '@rasm/typography/source';
import { Uxp } from '@rasm/uxp/runtime';
import { Array, Effect, Option, Predicate, Schema, Struct } from 'effect';
import { Model } from '#/dom/model';
import { reference, Specifier } from '#/dom/specifier';
import { fontByName, picked, read, Slice } from '#/dom/value';
import { FindChange } from '#/requests/find-change';

// --- [MODELS] --------------------------------------------------------------------------

const Installed = Schema.Struct({
    postscriptName: Schema.String,
    fullName: Schema.String,
    version: Schema.String,
    type: Schema.String,
    location: Schema.String.annotate({ description: 'File path, or label without readable file' }),
    styles: Schema.Array(Schema.String).annotate({ description: 'Style names' }),
    axes: Schema.OptionFromOptionalKey(Schema.Array(Schema.Struct({ name: Schema.String, value: Schema.Finite })).annotate({ description: 'Instance axes by InDesign name in fvar order, values to 2 decimals' })),
    features: Schema.Array(Schema.String).annotate({ description: 'OpenTypeFeature members in GSUB or GPOS' }),
    allows: Schema.Struct(Struct.Record(['allowOutlines', 'allowPDFEmbedding', 'allowEditableEmbedding', 'restrictedPrinting'], Schema.Boolean)),
});
class FontView extends Schema.TaggedClass<FontView, { readonly brand: unique symbol }>()('Font', {
    ...Reference.fields,
    family: Schema.String,
    style: Schema.OptionFromOptionalKey(Schema.String),
    status: Schema.String,
    uses: Schema.Array(Schema.String).annotate({ description: 'Current document text and styles set in the face' }),
    installed: Schema.OptionFromOptionalKey(Installed),
}) {
    static readonly matches = (model: typeof Model.Service) => model.owning<import('adobe:indesign').Font>('postscriptName');
    static readonly of = Effect.fn('FontView.of')(function* (face: import('adobe:indesign').Font) {
        const { app, FontStatus, OpenTypeFeature, members } = yield* Model;
        const [family, installed] = [face.fontFamily, face.status.equals(FontStatus.INSTALLED)];
        const style = installed ? Option.some(face.fontStyleName) : Option.none();
        const specifier = yield* Effect.orDie(Schema.decodeUnknownEffect(Specifier.Path)(face.toSpecifier()));
        const uses = yield* Option.match(yield* Effect.option(Model.document), {
            onNone: () => Effect.succeed(Array.empty<string>()),
            onSome: (open) =>
                Effect.map(
                    FindChange.make({
                        mode: 'Text',
                        find: Option.some({ appliedFont: { type: face.constructorName, specifier } }),
                        options: Option.some({ includeFootnotes: true, includeHiddenLayers: true, includeLockedLayersForFind: true, includeLockedStoriesForFind: true, includeMasterPages: true }),
                        change: Option.none(),
                        scope: Option.none(),
                        slice: Slice.make({ offset: 0, limit: Option.none() }),
                    }).search(open),
                    ({ matches }) => [
                        ...Array.map(matches, Struct.get('specifier')),
                        ...Array.map(
                            [...open.allParagraphStyles, ...open.allCharacterStyles].filter(({ appliedFont, fontStyle }) => Option.contains(style, fontStyle) && (Predicate.isString(appliedFont) ? appliedFont : appliedFont?.fontFamily) === family),
                            (each) => each.toSpecifier(),
                        ),
                    ],
                ),
        });
        const every = app.fonts.everyItem();
        const [styles, features] = yield* Effect.try({
            try: () =>
                installed
                    ? [
                          Array.dedupe(Array.flatMap(Array.zip(every.fontFamily, every.fontStyleName), ([each, named]) => (each === family ? [named] : []))),
                          Array.map(
                              members(OpenTypeFeature).filter((feature) => face.checkOpenTypeFeature(feature)),
                              String,
                          ),
                      ]
                    : [[], []],
            catch: HostError.refused(`styles and features of ${face.name}`),
        });
        const axes = Option.liftThrowable(() => Array.zipWith(face.designAxesName, face.designAxesValues, (name, value) => ({ name, value })))();
        return yield* read(FontView, face.toSpecifier(), () => ({
            _tag: 'Font',
            ...reference(face),
            family,
            style,
            status: face.status,
            uses,
            installed: installed ? Option.some({ ...picked(Installed.fields)(face), type: face.fontType, styles, axes, features, allows: picked(Installed.fields.allows.fields)(face) }) : Option.none(),
        }));
    });
}
class Font extends Schema.Class<Font, { readonly brand: unique symbol }>('Font')({
    name: Schema.String.annotate({ description: 'Font, PostScript, or full name, family name selects its Regular or first face' }),
    text: Schema.String.annotate({ description: 'Subset characters for unreadable font files' }),
    file: Schema.String,
}) {
    static readonly Result = Schema.Struct({
        view: FontView,
        source: Schema.OptionFromOptionalKey(Source.mapFields(Struct.omit(['data'])).mapFields(Struct.assign({ file: Schema.String.annotate({ description: 'Installed face file, or subset written to the requested file' }) }))),
    });
    static readonly handle = Effect.fn('Font.handle')(function* ({ name, text, file }: Font) {
        const [face, { FontStatus }, { fs }] = yield* Effect.all([fontByName(name), Model, Uxp]);
        const subset = Effect.as(Effect.try({ try: () => face.createSubsetFont(text, file), catch: HostError.refused(`subset of ${face.name}`) }), Option.some({ file, postscriptName: Option.none<string>() }));
        const source = face.status.equals(FontStatus.INSTALLED)
            ? yield* Effect.try({ try: () => fs.lstatSync(`file:${face.location}`), catch: HostError.refused(face.location) }).pipe(
                  Effect.as(Option.some({ file: face.location, postscriptName: Option.some(face.postscriptName) })),
                  Effect.catchIf(
                      ({ cause }) => notFound(cause),
                      () => subset,
                  ),
              )
            : Option.none();
        return { view: yield* FontView.of(face), source };
    });
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const notFound = Schema.is(Schema.Struct({ code: Schema.Literal(-2) }));

// --- [EXPORTS] -------------------------------------------------------------------------

export { Font, FontView };
