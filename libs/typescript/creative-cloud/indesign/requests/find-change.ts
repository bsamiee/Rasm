import type { Document, InDesignDOMObject, PageItem, Text } from 'adobe:indesign';
import { HostError } from '@rasm/automation/requests';
import { Unmatched } from '@rasm/search/unmatched';
import { Uxp } from '@rasm/uxp/runtime';
import { Array, Cache, Effect, flow, identity, Match, Number, Option, Predicate, Record, Struct } from 'effect';
import { FindChange } from '#/contract';
import { Dictionary } from '#/dom/dictionary';
import { Model } from '#/dom/model';
import { commit } from '#/dom/transactions';
import { write } from '#/requests/change';

// --- [CONSTANTS] -----------------------------------------------------------------------

const modes = {
    Text: { find: 'findText', change: 'changeText', preferences: { find: 'findTextPreferences', change: 'changeTextPreferences', options: 'findChangeTextOptions' } },
    Grep: { find: 'findGrep', change: 'changeGrep', preferences: { find: 'findGrepPreferences', change: 'changeGrepPreferences', options: 'findChangeGrepOptions' } },
    Glyph: { find: 'findGlyph', change: 'changeGlyph', preferences: { find: 'findGlyphPreferences', change: 'changeGlyphPreferences', options: 'findChangeGlyphOptions' } },
    Object: { find: 'findObject', change: 'changeObject', preferences: { find: 'findObjectPreferences', change: 'changeObjectPreferences', options: 'findChangeObjectOptions' } },
    Color: { find: 'findColor', change: 'changeColor', preferences: { find: 'findColorPreferences', change: 'changeColorPreferences', options: 'findChangeColorOptions' } },
    Transliterate: { find: 'findTransliterate', change: 'changeTransliterate', preferences: { find: 'findTransliteratePreferences', change: 'changeTransliteratePreferences', options: 'findChangeTransliterateOptions' } },
} as const satisfies Record<FindChange['mode'], { readonly find: string; readonly change: string; readonly preferences: Record<'find' | 'change' | 'options', string> }>;

// --- [OPERATIONS] ----------------------------------------------------------------------

const held = Effect.fnUntraced(function* (member: (typeof modes)[keyof typeof modes]['preferences']['find' | 'change' | 'options']) {
    const model = yield* Model;
    const { app, NothingEnum } = model;
    const target = app[member];
    const { fields } = yield* Cache.get(model.declarations, target.constructorName);
    const current = (name: string) => Option.filter(Option.liftThrowable((): unknown => Reflect.get(target, name))(), Predicate.not(model.nothing));
    const writable = Record.filter(fields, (row, name) => row.settable && !model.collectionField(row) && Option.isNone(row.unavailable) && name !== Dictionary.aggregate);
    return yield* Effect.acquireRelease(
        Effect.sync(() => Record.getSomes(Record.map(writable, (_, name) => current(name)))),
        (saved) =>
            Effect.try({ try: () => Object.assign(app, { [member]: NothingEnum.NOTHING })[member], catch: HostError.refused(`reset of ${member}`) }).pipe(
                Effect.orDie,
                Effect.flatMap((reset) => Uxp.restore(reset, saved)),
            ),
    );
});
const search = Effect.fnUntraced(function* ({ mode, find: sought, change: replacing, options }: Pick<FindChange, 'mode' | 'find' | 'change' | 'options'>, located: unknown) {
    const { find, change, preferences } = modes[mode];
    const model = yield* Model;
    const { app, enumerator, NothingEnum } = model;
    yield* Effect.forEach(Object.values(preferences), held, { discard: true });
    yield* Effect.try({
        try: () =>
            Object.assign(
                app,
                Record.mapEntries(preferences, (member) => [member, NothingEnum.NOTHING]),
            ),
        catch: HostError.refused(`reset of ${mode} preferences`),
    });
    const counted = Match.type<number | readonly unknown[]>().pipe(
        Match.when(Predicate.isNumber, identity<number>),
        Match.orElse((items) => items.length),
    );
    const rendered = (contents: unknown): string => Array.map(Array.ensure(contents), (each) => (enumerator(each) ? `<${String(each)}>` : String(each))).join('');
    const searched = model.owning<InDesignDOMObject<unknown> & Pick<Document, typeof find | typeof change>>(change);
    const scopes = yield* Effect.forEach(
        Array.ensure(located),
        flow(
            Option.liftPredicate(searched),
            Effect.fromOption(() => Unmatched.make({ subject: `${mode} search target`, query: Option.liftPredicate(located, model.domObject).pipe(Option.map((each) => each.toSpecifier())) })),
        ),
    );
    yield* Effect.forEach(
        [
            [preferences.find, sought],
            [preferences.options, options],
            [preferences.change, replacing],
        ] as const,
        ([target, fields]) => Effect.transposeOption(Option.map(fields, (values) => write(app[target], values))),
        { discard: true },
    );
    const describe = (hit: Text | PageItem) => {
        const frame = model.text(hit) ? hit.parentTextFrames[0] : undefined;
        return model.text(hit) ? { specifier: hit.toSpecifier(), text: rendered(hit.contents), page: frame !== undefined && frame.constructorName !== 'TextPath' ? frame.parentPage?.name : undefined } : { specifier: hit.toSpecifier(), page: model.item(hit) ? hit.parentPage?.name : undefined };
    };
    const found = yield* Effect.forEach(
        scopes,
        Effect.fnUntraced(function* (each) {
            yield* Effect.transposeOption(Option.map(model.owner(each), (context) => Uxp.preserve(app, { activeDocument: context })));
            return yield* Effect.try({
                try: () => {
                    const hits = each[find]();
                    const matches = Predicate.isNumber(hits) ? [] : hits.map(describe);
                    return { count: { matches: counted(hits), changes: Option.map(replacing, () => counted(each[change]())) }, matches };
                },
                catch: HostError.refused(`${mode} search of ${each.toSpecifier()}`),
            });
        }, Effect.scoped),
    );
    const changes = Array.getSomes(found.map(({ count }) => count.changes));
    return { count: { matches: Number.sumAll(found.map(({ count }) => count.matches)), changes: Option.map(Option.liftPredicate(changes, Array.isReadonlyArrayNonEmpty), Number.sumAll) }, matches: Array.flatMap(found, Struct.get('matches')) };
}, Effect.scoped);
const findChange = (payload: FindChange) =>
    Option.match(payload.scope, { onSome: Model.resolve, onNone: () => Model.current }).pipe(
        Effect.flatMap((found: unknown) => (Option.isSome(payload.change) ? commit(payload.label, search(payload, found)) : search(payload, found))),
        Effect.flatMap(({ count, matches }) => Model.read(FindChange.Result, payload.label, () => ({ count, matches: payload.slice.of(matches) }))),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { findChange, search };
