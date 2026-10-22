import { Unmatched } from '@rasm/search/unmatched.ts';
import { Array, Cache, type Cause, Effect, flow, Match, Option, Predicate, pipe, Record, Schema, Struct } from 'effect';
import { declared, type Member } from './dictionary.ts';
import { record } from './encoding.ts';
import { active, owner, resolve } from './location.ts';
import { Model } from './model.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

const member = Effect.fnUntraced(function* (kind: string, name: string) {
    const { Enumerator, enumeration, members, enumerations } = yield* Model;
    const found = yield* Effect.fromOption(enumeration(kind), () => Unmatched.make({ subject: 'enumeration', query: Option.some(kind), candidates: enumerations }));
    return yield* Effect.fromOption(Option.liftPredicate(Reflect.get(found, name), Schema.is(Schema.instanceOf(Enumerator))), () => Unmatched.make({ subject: `${kind} member`, query: Option.some(name), candidates: Array.map(members(found), String) }));
});
const byName = Effect.fnUntraced(function* (type: string, name: string, scope: Option.Option<object>) {
    const model = yield* Model;
    const [{ fields }, parents] = yield* Effect.all([Cache.get(model.rows, type), model.lineage(type)]);
    const listing = Record.has(fields, 'name') && ![type, ...parents].includes('Swatch') ? yield* Cache.get(model.catalog, type) : Option.none();
    return yield* Option.match(Option.all([scope, listing]), {
        onNone: () => Effect.succeed<unknown>(name),
        onSome: ([document, field]) => {
            const listed: unknown = Reflect.get(document, field);
            return Unmatched.single(type, Option.some(name))((model.collection(listed) ? listed.everyItem().getElements() : Array.ensure(listed)).filter(model.named), (each) => [each.toSpecifier(), each.name]);
        },
    });
});
const fontByName = Effect.fn('fontByName')(function* (name: string) {
    const { app, FontStatus } = yield* Model;
    const every = app.fonts.everyItem();
    const installed = pipe(
        Array.zip(Array.zip(every.name, every.postscriptName), Array.zip(every.fullName, Array.zip(every.fontFamily, every.fontStyleName))),
        Array.map(([[label, postscript], [full, [family, style]]], index) => ({ postscript, names: [label, postscript, full, ...(style === 'Regular' ? [family] : [])] as const, font: () => app.fonts.item(index) })),
        Array.dedupeWith((a, b) => a.postscript === b.postscript),
    );
    const missing = Array.flatMap(Option.toArray(yield* Effect.option(active)), (open) => {
        const used = open.fonts.everyItem();
        return Array.flatMap(Array.zip(used.name, used.status), ([label, status], index) => (FontStatus.INSTALLED.equals(status) ? [] : [{ names: [label] as const, font: () => open.fonts.item(index) }]));
    });
    return (yield* Unmatched.single('font', Option.some(name))([...installed, ...missing], Struct.get('names'))).font();
});
const decoder = (target: unknown, row: Option.Option<Pick<typeof Member.Type, 'type' | 'description'>>): ((value: unknown) => Effect.Effect<unknown, Cause.UnknownError | Unmatched, Model>) => {
    const scope = Effect.map(Effect.all([owner(target), Effect.option(active)]), ([held, open]) => Option.orElse(held, () => open));
    const typed = (text: string) =>
        Effect.flatMap(Effect.all([Model, scope]), ([{ enumeration, classes }, container]) =>
            Match.value(Option.match(row, { onNone: Array.empty, onSome: declared })).pipe(
                Match.when(Schema.is(Schema.Tuple([Schema.String])), ([type]) =>
                    Match.value(type).pipe(
                        Match.when(flow(enumeration, Option.isSome), (each) => member(each, text)),
                        Match.when('Font', () => fontByName(text)),
                        Match.when(
                            (each) => classes.includes(each),
                            (each) => byName(each, text, container),
                        ),
                        Match.orElse(() => Effect.succeed<unknown>(text)),
                    ),
                ),
                Match.orElse((types) => Effect.firstSuccessOf([...types.filter(flow(enumeration, Option.isSome)).map((each) => member(each, text)), Effect.succeed<unknown>(text)])),
            ),
        );
    const decode: (value: unknown) => Effect.Effect<unknown, Cause.UnknownError | Unmatched, Model> = Match.type<unknown>().pipe(
        Match.when(Schema.is(Schema.Struct({ type: Schema.String, specifier: Schema.String })), ({ specifier }) => resolve(specifier)),
        Match.when(Schema.is(Schema.Struct({ type: Schema.String, member: Schema.String })), ({ type, member: name }) => member(type, name)),
        Match.when(Predicate.isString, typed),
        Match.when(Array.isArray, (items) => Effect.forEach(items, decode)),
        Match.when(record, (properties) => Effect.all(Record.map(properties, decoder(target, Option.none())))),
        Match.orElse(Effect.succeed),
    );
    return decode;
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { byName, decoder, fontByName };
