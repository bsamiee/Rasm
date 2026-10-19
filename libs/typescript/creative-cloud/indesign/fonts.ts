import type { Font } from 'adobe:indesign';
import { single } from '@rasm/search';
import { Array, Cause, Effect, Option, Predicate, Schema, Struct } from 'effect';
import { Host } from './host.ts';
import type { Classes } from './objects/classes.ts';
import { active, read, Specifier } from './objects/specifiers.ts';
import { preferences } from './search.ts';

// --- [MODELS] --------------------------------------------------------------------------

class FontView extends Schema.TaggedClass<FontView, { readonly brand: unique symbol }>()('Font', {
    name: Schema.String,
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
            location: Schema.String,
            styles: Schema.Array(Schema.String).annotate({ description: 'Styles of the family InDesign lists' }),
            coordinates: Schema.OptionFromOptionalKey(Schema.NonEmptyArray(Schema.Finite).annotate({ description: 'Design axis values of a variable instance' })),
            allows: Schema.Struct(Struct.Record(['outlines', 'pdfEmbedding', 'editableEmbedding', 'restrictedPrinting'], Schema.Boolean)),
        }),
    ),
}) {
    static readonly guard = ({ owning }: Classes['Service']) => owning<Font>('postscriptName');
    static readonly named = Effect.fn('named')(function* (name: string) {
        const { indesign } = yield* Host;
        const every = indesign.app.fonts.everyItem();
        const unavailable = Array.flatMap(Option.toArray(yield* Effect.option(active)), (open) =>
            open.fonts
                .everyItem()
                .getElements()
                .filter(({ status }) => !status.equals(indesign.FontStatus.INSTALLED)),
        );
        const fonts = Array.dedupeWith([...every.getElements(), ...unavailable], (first, second) => first.equals(second));
        return yield* single('font', name)(fonts, { names: (each) => [each.name, each.postscriptName, each.fullName, each.toSpecifier(), ...(each.fontStyleName === 'Regular' ? [each.fontFamily] : [])], label: Struct.get('name') });
    });
    static readonly of = Effect.fn('of')(function* (font: Font) {
        const { indesign } = yield* Host;
        const { app } = indesign;
        const every = app.fonts.everyItem();
        const family = font.fontFamily;
        const style = Option.liftThrowable(() => font.fontStyleName)();
        const uses = yield* Option.match(yield* Effect.option(active), {
            onNone: () => Effect.succeed([]),
            onSome: (open) =>
                Effect.andThen(
                    preferences('Text'),
                    Effect.try({
                        try: () => {
                            Object.assign(app.findTextPreferences, { appliedFont: font });
                            Object.assign(app.findChangeTextOptions, { includeFootnotes: true, includeHiddenLayers: true, includeLockedLayersForFind: true, includeLockedStoriesForFind: true, includeMasterPages: true });
                            return [...open.findText(), ...[...open.allParagraphStyles, ...open.allCharacterStyles].filter(({ appliedFont, fontStyle }) => Option.contains(style, fontStyle) && (Predicate.isString(appliedFont) ? appliedFont : appliedFont?.fontFamily) === family)];
                        },
                        catch: (cause) => new Cause.UnknownError(cause, `uses of ${font.name}`),
                    }),
                ).pipe(Effect.scoped),
        });
        const installed = Option.map(
            Option.liftPredicate(font, ({ status }) => status.equals(indesign.FontStatus.INSTALLED)),
            (face) => ({
                postscriptName: face.postscriptName,
                fullName: face.fullName,
                version: face.version,
                type: face.fontType,
                location: face.location,
                styles: Array.dedupe(Array.flatMap(Array.zip(every.fontFamily, every.fontStyleName), ([each, named]) => (each === family ? [named] : []))),
                coordinates: Option.getOrUndefined(Option.filter(Option.liftThrowable(() => face.designAxesValues)(), Array.isArrayNonEmpty)),
                allows: { outlines: face.allowOutlines, pdfEmbedding: face.allowPDFEmbedding, editableEmbedding: face.allowEditableEmbedding, restrictedPrinting: face.restrictedPrinting },
            }),
        );
        return yield* read(FontView, () => ({ _tag: 'Font', name: font.name, family, style: Option.getOrUndefined(style), status: font.status, uses, installed: Option.getOrUndefined(installed) }));
    });
    static readonly source = Effect.fn('source')(function* (font: Font, text: string) {
        const { indesign, fs, uxp } = yield* Host;
        return yield* Effect.try(() => new Uint8Array(fs.readFileSync(`file:${font.location}`))).pipe(
            Effect.map((data) => ({ data, subset: false })),
            Effect.catchIf(
                ({ cause }) => Schema.is(Schema.Struct({ code: Schema.Literal(-2) }))(cause),
                Effect.fnUntraced(function* () {
                    const [folder, id] = yield* Effect.all([Effect.tryPromise(() => uxp.storage.localFileSystem.getTemporaryFolder()), Effect.fiberId]);
                    const file = yield* Effect.acquireRelease(
                        Effect.tryPromise(() => folder.createFile(`font-${id}.otf`, { overwrite: true })),
                        (created) => Effect.promise(created.delete.bind(created)),
                    );
                    yield* Effect.try(() => font.createSubsetFont(text, file.nativePath));
                    return { data: yield* Effect.try(() => new Uint8Array(fs.readFileSync(file.url))), subset: true };
                }, Effect.scoped),
            ),
            Effect.when(Effect.succeed(font.status.equals(indesign.FontStatus.INSTALLED))),
        );
    });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { FontView };
