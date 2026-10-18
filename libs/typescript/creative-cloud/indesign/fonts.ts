import indesign from 'adobe:indesign';
import { Unmatched } from '@rasm/mcp/link';
import { Temporary } from '@rasm/uxp/runtime';
import { Array, Effect, Option, Predicate, pipe, String, Struct } from 'effect';
import type { Rpc } from 'effect/rpc';
import { Font, InDesignError, type Subset } from './contract.ts';
import { active, read } from './dom.ts';
import { find } from './find.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

const face = Effect.fn('face')(function* ({ name }: Rpc.Payload<typeof Font>) {
    const { app } = indesign;
    const document = yield* Effect.option(active);
    const every = app.fonts.everyItem();
    const held = Array.flatMap(Option.toArray(document), ({ fonts }) => Array.zip(fonts.everyItem().name, fonts.everyItem().status));
    const listing = every.name;
    const names = [
        ...listing,
        ...Array.difference(
            Array.map(held, ([each]) => each),
            listing,
        ),
    ];
    const key = name.toLowerCase();
    const hits = (keys: readonly string[], value: string) =>
        Option.liftPredicate(
            Array.flatMap(Array.zip(names, keys), ([each, candidate], at) => (candidate.toLowerCase() === value ? [{ listed: each, index: at }] : [])),
            Array.isArrayNonEmpty,
        );
    const [{ listed, index }] = yield* Effect.filterOrFail(
        Effect.fromOption(
            pipe(
                hits(names, key),
                Option.orElse(() => hits(names, `${key}\tregular`)),
                Option.orElse(() => hits(every.postscriptName, key)),
                Option.orElse(() => hits(every.fullName, key)),
            ),
            () => {
                const family = names.filter((each) => each.toLowerCase().split('\t')[0] === key.split('\t')[0]);
                return Unmatched.make({ subject: 'font', query: name, candidates: Array.isReadonlyArrayNonEmpty(family) ? family : names });
            },
        ),
        (rows) => rows.length === 1,
        (rows) => {
            const indexes = new Set(Array.map(rows, Struct.get('index')));
            return Unmatched.make({ subject: 'single font', query: name, candidates: every.postscriptName.filter((_, at) => indexes.has(at)) });
        },
    );
    const [family, ...rest] = String.split(listed, '\t');
    const style = rest.join('\t');
    const uses = yield* Option.match(document, {
        onNone: () => Effect.succeed([]),
        onSome: Effect.fnUntraced(function* (open) {
            const { matches } = yield* find(
                {
                    description: `Finding uses of ${name}`,
                    preview: Option.some(true),
                    mode: 'Text',
                    find: Option.some({ appliedFont: family, fontStyle: style }),
                    action: Option.none(),
                    options: Option.some({ includeFootnotes: true, includeHiddenLayers: true, includeLockedLayersForFind: true, includeLockedStoriesForFind: true, includeMasterPages: true }),
                    scope: Option.none(),
                    query: Option.none(),
                    save: Option.none(),
                },
                Number.POSITIVE_INFINITY,
            );
            return [...Array.map(matches, Struct.get('specifier')), ...[...open.allParagraphStyles, ...open.allCharacterStyles].filter(({ appliedFont, fontStyle }) => fontStyle === style && (Predicate.isString(appliedFont) ? appliedFont : appliedFont?.fontFamily) === family)];
        }),
    });
    const used = Array.findFirst(held, ([each]) => each === listed);
    const installed = Option.map(
        Option.liftPredicate(app.fonts.item(index), () => index < listing.length),
        (font) => ({
            postscriptName: font.postscriptName,
            fullName: font.fullName,
            version: font.version,
            type: font.fontType,
            location: font.location,
            styles: Array.dedupe(Array.flatMap(listing, (each) => (each.startsWith(`${family}\t`) ? [each.slice(family.length + 1)] : []))),
            axes: Option.getOrElse(Option.liftThrowable(() => font.designAxesName)(), Array.empty),
            coordinates: Option.getOrElse(Option.liftThrowable(() => font.designAxesValues)(), Array.empty),
            allows: { outlines: font.allowOutlines, pdfEmbedding: font.allowPDFEmbedding, editableEmbedding: font.allowEditableEmbedding, restrictedPrinting: font.restrictedPrinting },
        }),
    );
    return yield* read(Font.successSchema, () => ({
        family,
        style,
        status: Option.isSome(used) ? used.value[1] : app.fonts.item(index).status,
        uses,
        autoLeading: Option.getOrElse(document, () => app).textDefaults.autoLeading,
        installed: Option.getOrUndefined(installed),
    }));
});
const subset = Effect.fn('subset')(function* ({ postscriptName, text }: Rpc.Payload<typeof Subset>) {
    const { app } = indesign;
    const at = yield* Effect.fromOption(
        Array.findFirstIndex(app.fonts.everyItem().postscriptName, (each) => each === postscriptName),
        () => Unmatched.make({ subject: 'installed face', query: postscriptName, candidates: app.fonts.everyItem().postscriptName }),
    );
    const { native } = (yield* Temporary)(`${postscriptName}.otf`);
    yield* Effect.try({ try: () => app.fonts.item(at).createSubsetFont(text, native), catch: InDesignError.thrown });
    return native;
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { face, subset };
