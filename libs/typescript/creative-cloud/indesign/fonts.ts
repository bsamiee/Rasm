import type { Font as NativeFont } from 'adobe:indesign';
import { Source } from '@rasm/typography/source';
import { Uxp } from '@rasm/uxp/runtime';
import { Array, Cause, Effect, flow, Option, Predicate, Schema, Struct } from 'effect';
import { FindChange } from './find.ts';
import { Model } from './model.ts';
import { active, font, Reference, read, Specifier } from './references.ts';

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
    static readonly viewed = ({ owning }: Model['Service']) => flow(Option.liftPredicate(owning<NativeFont>('postscriptName')), Option.map(FontView.of));
    static readonly of = Effect.fn('of')(function* (face: NativeFont) {
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
                        query: Option.none(),
                        save: Option.none(),
                    }).search(),
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
        return yield* read(FontView, () => ({
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
class Font extends Schema.Class<Font, { readonly brand: unique symbol }>('Font')(
    {
        name: Schema.String.annotate({ description: 'Native font name, PostScript name, or full name; a family selects its Regular face' }),
        text: Schema.String.annotate({ description: 'Characters a native subset holds when the original file is unreadable' }),
    },
    { description: 'Reads one font with the bytes of its face' },
) {
    static readonly Result = Schema.Struct({
        view: FontView,
        source: Schema.OptionFromOptionalKey(Source.annotate({ description: 'Face bytes of an installed font' })),
    });
    run() {
        const { name, text } = this;
        return Effect.gen(function* () {
            const [face, { FontStatus }, { fs, temporary }] = yield* Effect.all([font(name), Model, Uxp]);
            const bytes = (path: string) => Effect.try(() => new Uint8Array(fs.readFileSync(path)));
            const subset = Effect.flatMap(temporary('font.otf'), (entry) =>
                Effect.andThen(
                    Effect.try(() => face.createSubsetFont(text, entry.nativePath)),
                    Effect.map(bytes(entry.url), (data) => Option.some({ data, postscriptName: Option.none<string>() })),
                ),
            );
            const source = face.status.equals(FontStatus.INSTALLED)
                ? yield* bytes(`file:${face.location}`).pipe(
                      Effect.map((data) => Option.some({ data, postscriptName: Option.some(face.postscriptName) })),
                      Effect.catchIf(
                          ({ cause }) => Schema.is(Schema.Struct({ code: Schema.Literal(-2) }))(cause),
                          () => subset,
                      ),
                  )
                : Option.none();
            return { view: yield* FontView.of(face), source };
        }).pipe(Effect.scoped);
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Font, FontView };
