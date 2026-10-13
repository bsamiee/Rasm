import type { Font } from 'adobe:indesign';
import indesign from 'adobe:indesign';
import { Array, Effect, Option, Predicate, pipe, String, Struct } from 'effect';
import type { Rpc } from 'effect/rpc';
import { Face, type Subset } from './contract.ts';
import { active, missing, read, scope, Temporary, thrown } from './dom.ts';
import { scripted } from './execute.ts';
import { find } from './find.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

const face = Effect.fnUntraced(function* ({ name }: Rpc.Payload<typeof Face>) {
    const { app } = indesign;
    yield* scope;
    const document = yield* Effect.option(active);
    const every = app.fonts.everyItem();
    const held = Array.flatMap(Option.toArray(document), ({ fonts }) => Array.zip(fonts.everyItem().name, fonts.everyItem().status));
    const listing = every.name;
    const names = [
        ...listing,
        ...Array.difference(
            Array.map(held, ([used]) => used),
            listing,
        ),
    ];
    const key = name.toLowerCase();
    const hits = (keys: readonly string[], value: string): Option.Option<Array.NonEmptyArray<{ readonly listed: string; readonly index: number }>> =>
        Option.liftPredicate(
            Array.flatMap(Array.zip(names, keys), ([each, candidate], at) => (candidate.toLowerCase() === value ? [{ listed: each, index: at }] : [])),
            Array.isArrayNonEmpty,
        );
    const { listed, index } = yield* Option.match(
        pipe(
            hits(names, key),
            Option.orElse(() => hits(names, `${key}\tregular`)),
            Option.orElse(() => hits(every.postscriptName, key)),
            Option.orElse(() => hits(every.fullName, key)),
        ),
        {
            onNone: () => {
                const family = Array.filter(names, (each) => each.toLowerCase().split('\t')[0] === key.split('\t')[0]);
                return Effect.fail(missing(`font ${name}`, name, Array.isReadonlyArrayNonEmpty(family) ? family : names));
            },
            onSome: ([only, ...shared]) =>
                Array.match(shared, {
                    onEmpty: () => Effect.succeed(only),
                    onNonEmpty: () =>
                        Effect.fail(
                            missing(
                                `single font ${name}`,
                                name,
                                Array.filter(every.postscriptName, (_, at) => Array.some([only, ...shared], (each) => each.index === at)),
                            ),
                        ),
                }),
        },
    );
    const [family, ...rest] = String.split(listed, '\t');
    const style = Array.join(rest, '\t');
    const uses = yield* Option.match(document, {
        onNone: () => Effect.succeed([]),
        onSome: Effect.fnUntraced(function* (open) {
            const request = {
                description: `Finding uses of ${name}`,
                preview: Option.some(true as const),
                mode: 'Text' as const,
                find: Option.some({ appliedFont: family, fontStyle: style }),
                action: Option.none(),
                options: Option.some({ includeFootnotes: true, includeHiddenLayers: true, includeLockedLayersForFind: true, includeLockedStoriesForFind: true, includeMasterPages: true }),
                scope: Option.none(),
                query: Option.none(),
                save: Option.none(),
            };
            const [{ matches }] = yield* Effect.flatMap(find(request, Option.none()), (job) => scripted(request, job));
            return [
                ...Array.map(matches, Struct.get('specifier')),
                ...Array.filter([...open.allParagraphStyles, ...open.allCharacterStyles], (each) => {
                    const applied: Font | string | null = each.appliedFont;
                    return each.fontStyle === style && (Predicate.isString(applied) ? applied : applied?.fontFamily) === family;
                }),
            ];
        }),
    });
    return yield* read(Face.successSchema, () => ({
        family,
        style,
        status: Option.match(
            Array.findFirst(held, ([used]) => used === listed),
            { onSome: ([, status]) => status, onNone: () => app.fonts.item(index).status },
        ),
        uses,
        autoLeading: Option.getOrElse(document, () => app).textDefaults.autoLeading,
        installed: pipe(
            Option.liftPredicate(index, (at) => at < listing.length),
            Option.map((at) => app.fonts.item(at)),
            Option.map((font) => ({
                postscriptName: font.postscriptName,
                fullName: font.fullName,
                version: font.version,
                type: font.fontType,
                location: font.location,
                styles: Array.dedupe(Array.flatMap(listing, (each) => (each.startsWith(`${family}\t`) ? [each.slice(family.length + 1)] : []))),
                axes: Option.getOrElse(Option.liftThrowable(() => font.designAxesName)(), Array.empty),
                coordinates: Option.getOrElse(Option.liftThrowable(() => font.designAxesValues)(), Array.empty),
                allows: { outlines: font.allowOutlines, pdfEmbedding: font.allowPDFEmbedding, editableEmbedding: font.allowEditableEmbedding, restrictedPrinting: font.restrictedPrinting },
            })),
            Option.getOrUndefined,
        ),
    }));
}, Effect.scoped);
const subset = Effect.fnUntraced(function* ({ postscriptName, text }: Rpc.Payload<typeof Subset>) {
    const { app } = indesign;
    const folder = yield* Temporary;
    yield* scope;
    const at = yield* Effect.fromOption(
        Array.findFirstIndex(app.fonts.everyItem().postscriptName, (each) => each === postscriptName),
        () => missing(`installed face ${postscriptName}`, postscriptName, []),
    );
    const file = `${folder}${postscriptName}.otf`;
    yield* Effect.try({ try: () => app.fonts.item(at).createSubsetFont(text, file), catch: thrown });
    return file;
}, Effect.scoped);

// --- [EXPORTS] -------------------------------------------------------------------------

export { face, subset };
