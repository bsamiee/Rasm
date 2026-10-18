import fs from 'adobe:fs';
import indesign from 'adobe:indesign';
import { storage } from 'adobe:uxp';
import { Unmatched } from '@rasm/search';
import { Array, Effect, Equal, Option, Predicate, pipe, Schema, Struct } from 'effect';
import { active } from '../document/objects.ts';
import { InDesignError, type Operations } from '../models.ts';
import { preferences } from './search.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

const face = Effect.fn('face')(function* ({ name, text }: typeof Operations.font.payload.Type) {
    const { app } = indesign;
    const document = yield* Effect.option(active);
    const every = app.fonts.everyItem();
    const unavailable = Array.flatMap(Option.toArray(document), (open) =>
        open.fonts
            .everyItem()
            .getElements()
            .filter(({ status }) => !status.equals(indesign.FontStatus.INSTALLED)),
    );
    const fonts = [...every.getElements(), ...unavailable];
    const names = [...every.name, ...unavailable.map(Struct.get('name'))];
    const key = name.toLowerCase();
    const hits = (keys: readonly string[]) =>
        pipe(
            Array.zip(fonts, keys),
            Array.flatMap(([each, candidate]) => (candidate.toLowerCase() === key ? [each] : [])),
            Array.dedupeWith((first, second) => first.equals(second)),
            Array.match({ onEmpty: Option.none, onNonEmpty: Option.some }),
        );
    const [font] = yield* Effect.filterOrFail(
        Effect.fromOption(
            pipe(
                hits(names),
                Option.orElse(() => hits(every.postscriptName)),
                Option.orElse(() => hits(every.fullName)),
                Option.orElse(() => Option.flatMap(hits(every.fontFamily), (rows) => Array.match(rows.filter(Predicate.Struct({ fontStyleName: Equal.equals('Regular') })), { onEmpty: Option.none, onNonEmpty: Option.some }))),
                Option.orElse(() => hits(fonts.map((each) => each.toSpecifier()))),
            ),
            () => Unmatched.make({ subject: 'font', query: name, candidates: names }),
        ),
        (rows) => rows.length === 1,
        (rows) => Unmatched.make({ subject: 'single font', query: name, candidates: rows.map((each) => each.toSpecifier()) }),
    );
    const family = font.fontFamily;
    const style = Option.liftThrowable(() => font.fontStyleName)();
    const uses = yield* Option.match(document, {
        onNone: () => Effect.succeed([]),
        onSome: (open) =>
            Effect.scoped(
                Effect.andThen(
                    preferences('Text'),
                    Effect.try({
                        try: () => {
                            Object.assign(app.findTextPreferences, { appliedFont: font });
                            Object.assign(app.findChangeTextOptions, { includeFootnotes: true, includeHiddenLayers: true, includeLockedLayersForFind: true, includeLockedStoriesForFind: true, includeMasterPages: true });
                            return [...open.findText(), ...[...open.allParagraphStyles, ...open.allCharacterStyles].filter(({ appliedFont, fontStyle }) => Option.contains(style, fontStyle) && (Predicate.isString(appliedFont) ? appliedFont : appliedFont?.fontFamily) === family)].map((each) => each.toSpecifier());
                        },
                        catch: InDesignError.refused(Option.some(`uses of ${font.name}`)),
                    }),
                ),
            ),
    });
    const installed = yield* Effect.try(() => new Uint8Array(fs.readFileSync(`file:${font.location}`))).pipe(
        Effect.map((data) => ({ data, subset: false })),
        Effect.catchIf(
            ({ cause }) => Schema.is(Schema.Struct({ code: Schema.Literal(-2) }))(cause),
            Effect.fnUntraced(function* () {
                const folder = yield* Effect.tryPromise(() => storage.localFileSystem.getTemporaryFolder());
                const id = yield* Effect.fiberId;
                const file = yield* Effect.acquireRelease(
                    Effect.tryPromise(() => folder.createFile(`font-${id}.otf`, { overwrite: true })),
                    (created) => Effect.promise(created.delete.bind(created)),
                );
                yield* Effect.try(() => font.createSubsetFont(text, file.nativePath));
                return { data: yield* Effect.try(() => new Uint8Array(fs.readFileSync(file.url))), subset: true };
            }, Effect.scoped),
        ),
        Effect.mapError(InDesignError.refused(Option.some(font.name))),
        Effect.map((source) => ({
            postscriptName: font.postscriptName,
            fullName: font.fullName,
            version: font.version,
            type: font.fontType.toString(),
            location: font.location,
            styles: Array.dedupe(Array.flatMap(Array.zip(every.fontFamily, every.fontStyleName), ([each, named]) => (each === family ? [named] : []))),
            coordinates: Option.filter(Option.liftThrowable(() => font.designAxesValues)(), Array.isArrayNonEmpty),
            allows: { outlines: font.allowOutlines, pdfEmbedding: font.allowPDFEmbedding, editableEmbedding: font.allowEditableEmbedding, restrictedPrinting: font.restrictedPrinting },
            ...source,
        })),
        Effect.when(Effect.succeed(font.status.equals(indesign.FontStatus.INSTALLED))),
    );
    return {
        family,
        style,
        status: font.status.toString(),
        uses,
        installed,
    };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { face };
