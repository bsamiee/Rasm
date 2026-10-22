import type { Font } from 'adobe:indesign';
import { Array, Cause, Effect, flow, Option, Predicate, Schema, Struct } from 'effect';
import { read } from '#/dom/encoding';
import { active } from '#/dom/location';
import { Model } from '#/dom/model';
import { Reference, Specifier } from '#/dom/reference';
import { FindChange } from '#/requests/find-change';

// --- [MODELS] --------------------------------------------------------------------------

class FontView extends Schema.TaggedClass<FontView, { readonly brand: unique symbol }>()('Font', {
    ...Reference.fields,
    family: Schema.String,
    style: Schema.OptionFromOptionalKey(Schema.String),
    status: Schema.String,
    uses: Schema.Array(Specifier).annotate({ description: 'Text and styles of the active document set in this face' }),
    installed: Schema.OptionFromOptionalKey(
        Schema.Struct({
            postscriptName: Schema.String,
            fullName: Schema.String,
            version: Schema.String,
            type: Schema.String,
            location: Schema.String.annotate({ description: 'File path, or a label for a face with no readable file' }),
            styles: Schema.Array(Schema.String).annotate({ description: 'Styles of the family InDesign lists' }),
            axes: Schema.OptionFromOptionalKey(Schema.Array(Schema.Struct({ name: Schema.String, value: Schema.Finite })).annotate({ description: 'Design axes of a variable instance in fvar order, by the axis name InDesign shows, values rounded to two decimals' })),
            features: Schema.Array(Schema.String).annotate({ description: 'OpenTypeFeature members the face holds in GSUB or GPOS' }),
            allows: Schema.Struct(Struct.Record(['outlines', 'pdfEmbedding', 'editableEmbedding', 'restrictedPrinting'], Schema.Boolean)),
        }),
    ),
}) {
    static readonly viewed = ({ owning }: Model['Service']) => flow(Option.liftPredicate(owning<Font>('postscriptName')), Option.map(FontView.of));
    static readonly of = Effect.fn('FontView.of')(function* (face: Font) {
        const { app, FontStatus, OpenTypeFeature, members } = yield* Model;
        const [family, installed] = [face.fontFamily, face.status.equals(FontStatus.INSTALLED)];
        const style = installed ? Option.some(face.fontStyleName) : Option.none();
        const uses = yield* Option.match(yield* Effect.option(active), {
            onNone: () => Effect.succeed(Array.empty<string>()),
            onSome: (open) =>
                Effect.map(
                    FindChange.make({
                        mode: 'Text',
                        find: Option.some({ appliedFont: { type: face.constructorName, specifier: face.toSpecifier() } }),
                        options: Option.some({ includeFootnotes: true, includeHiddenLayers: true, includeLockedLayersForFind: true, includeLockedStoriesForFind: true, includeMasterPages: true }),
                        change: Option.none(),
                        scope: Option.none(),
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
            catch: (cause) => new Cause.UnknownError(cause, `faces of ${face.name}`),
        });
        const axes = Option.liftThrowable(() => Array.zipWith(face.designAxesName, face.designAxesValues, (name, value) => ({ name, value })))();
        return yield* read(FontView, face.toSpecifier(), () => ({
            _tag: 'Font',
            ...Reference.of(face),
            family,
            style,
            status: face.status,
            uses,
            installed: installed
                ? Option.some({
                      postscriptName: face.postscriptName,
                      fullName: face.fullName,
                      version: face.version,
                      type: face.fontType,
                      location: face.location,
                      styles,
                      axes,
                      features,
                      allows: { outlines: face.allowOutlines, pdfEmbedding: face.allowPDFEmbedding, editableEmbedding: face.allowEditableEmbedding, restrictedPrinting: face.restrictedPrinting },
                  })
                : Option.none(),
        }));
    });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { FontView };
