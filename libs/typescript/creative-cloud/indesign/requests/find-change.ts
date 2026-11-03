import type { Application, Document, InDesignDOMObject, PageItem, Text } from 'adobe:indesign';
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
    Text: { find: 'findText', change: 'changeText', preferences: ['findTextPreferences', 'findChangeTextOptions', 'changeTextPreferences'] },
    Grep: { find: 'findGrep', change: 'changeGrep', preferences: ['findGrepPreferences', 'findChangeGrepOptions', 'changeGrepPreferences'] },
    Glyph: { find: 'findGlyph', change: 'changeGlyph', preferences: ['findGlyphPreferences', 'findChangeGlyphOptions', 'changeGlyphPreferences'] },
    Object: { find: 'findObject', change: 'changeObject', preferences: ['findObjectPreferences', 'findChangeObjectOptions', 'changeObjectPreferences'] },
    Color: { find: 'findColor', change: 'changeColor', preferences: ['findColorPreferences', 'findChangeColorOptions', 'changeColorPreferences'] },
    Transliterate: { find: 'findTransliterate', change: 'changeTransliterate', preferences: ['findTransliteratePreferences', 'findChangeTransliterateOptions', 'changeTransliteratePreferences'] },
} as const satisfies Record<FindChange['mode'], { readonly find: keyof Document; readonly change: keyof Document; readonly preferences: readonly [keyof Application, keyof Application, keyof Application] }>;

// --- [OPERATIONS] ----------------------------------------------------------------------

const held = Effect.fnUntraced(function* (member: (typeof modes)[FindChange['mode']]['preferences'][number]) {
    const model = yield* Model;
    const { app, NothingEnum } = model;
    const target = app[member];
    const { fields } = yield* Cache.get(model.declarations, target.constructorName);
    const current = (name: string) => Option.filter(Option.liftThrowable((): unknown => Reflect.get(target, name))(), Predicate.not(model.nothing));
    const writable = Record.filter(fields, (row, name) => row.settable && !model.collectionField(row) && Option.isNone(row.unavailable) && name !== Dictionary.aggregate);
    const reset = Effect.try({ try: () => Object.assign(app, { [member]: NothingEnum.NOTHING })[member], catch: HostError.refused(`reset of ${member}`) });
    return yield* Effect.acquireRelease(Effect.sync(() => Record.getSomes(Record.map(writable, (_, name) => current(name)))).pipe(Effect.tap(() => reset)), (saved) =>
        reset.pipe(
            Effect.orDie,
            Effect.flatMap((cleared) => Uxp.restore(cleared, saved)),
        ),
    );
});
const search = Effect.fnUntraced(function* ({ mode, find: sought, change: replacing, options }: Pick<FindChange, 'mode' | 'find' | 'change' | 'options'>, located: unknown) {
    const { find, change, preferences } = modes[mode];
    const model = yield* Model;
    const { app, enumerator } = model;
    yield* Effect.forEach(preferences, held, { discard: true });
    const counted = Match.type<number | readonly unknown[]>().pipe(Match.when(Predicate.isNumber, identity<number>), Match.orElse(Array.length));
    const rendered = (contents: unknown): string => Array.map(Array.ensure(contents), (each) => (enumerator(each) ? `<${String(each)}>` : String(each))).join('');
    const searched = model.owning<InDesignDOMObject<unknown> & Pick<Document, typeof find | typeof change>>(change);
    const scopes = yield* Effect.forEach(
        Array.ensure(located),
        flow(
            Option.liftPredicate(searched),
            Effect.fromOption(() => Unmatched.make({ subject: `${mode} search target`, query: Option.map(Option.liftPredicate(located, model.domObject), (target) => target.toSpecifier()) })),
        ),
    );
    yield* Effect.forEach(Array.zip(preferences, [sought, options, replacing]), ([target, fields]) => Effect.transposeOption(Option.map(fields, (values) => write(app[target], values))), { discard: true });
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
