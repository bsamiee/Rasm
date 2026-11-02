import type { Document, InDesignDOMObject, PageItem, Text } from 'adobe:indesign';
import { HostError } from '@rasm/automation/requests';
import { Unmatched } from '@rasm/search/unmatched';
import { Array, Cache, Effect, flow, Number, Option, Predicate, Record, Schema, Struct } from 'effect';
import { Model } from '#/dom/model';
import { Specifier } from '#/dom/specifier';
import { commit, preserve } from '#/dom/transactions';
import { read, Slice, Values } from '#/dom/value';
import { Change } from '#/requests/change';

// --- [CONSTANTS] -----------------------------------------------------------------------

const modes = {
    Text: { find: 'findText', change: 'changeText', preferences: { find: 'findTextPreferences', change: 'changeTextPreferences', options: 'findChangeTextOptions' } },
    Grep: { find: 'findGrep', change: 'changeGrep', preferences: { find: 'findGrepPreferences', change: 'changeGrepPreferences', options: 'findChangeGrepOptions' } },
    Glyph: { find: 'findGlyph', change: 'changeGlyph', preferences: { find: 'findGlyphPreferences', change: 'changeGlyphPreferences', options: 'findChangeGlyphOptions' } },
    Object: { find: 'findObject', change: 'changeObject', preferences: { find: 'findObjectPreferences', change: 'changeObjectPreferences', options: 'findChangeObjectOptions' } },
    Color: { find: 'findColor', change: 'changeColor', preferences: { find: 'findColorPreferences', change: 'changeColorPreferences', options: 'findChangeColorOptions' } },
    Transliterate: { find: 'findTransliterate', change: 'changeTransliterate', preferences: { find: 'findTransliteratePreferences', change: 'changeTransliteratePreferences', options: 'findChangeTransliterateOptions' } },
} as const;

// --- [MODELS] --------------------------------------------------------------------------

class FindChange extends Schema.Class<FindChange, { readonly brand: unique symbol }>('FindChange')({
    mode: Schema.Literals(Struct.keys(modes)),
    find: Schema.OptionFromOptionalKey(Values.annotate({ description: 'Find preference fields (pattern, format criteria)' })),
    change: Schema.OptionFromOptionalKey(Values.annotate({ description: 'Change preference fields for every match, absent to find only' })),
    options: Schema.OptionFromOptionalKey(Values.annotate({ description: 'Find/change option fields' })),
    scope: Schema.OptionFromOptionalKey(Specifier.Path.annotate({ description: 'Document, story, text, or page item to search, current document when absent' })),
    slice: Slice.field,
}) {
    static readonly Result = Schema.Struct({
        count: Schema.Struct({ matches: Schema.Int, changes: Schema.OptionFromOptionalKey(Schema.Int.annotate({ description: 'Color mode change count, absent otherwise' })) }),
        matches: Schema.Array(Schema.Struct({ specifier: Schema.String, text: Schema.OptionFromOptionalKey(Schema.String), page: Schema.OptionFromOptionalKey(Schema.String) })).annotate({ description: 'Matches with text before changes, empty for Color' }),
    });
    static readonly handle = (payload: FindChange) =>
        Option.match(payload.scope, { onSome: Specifier.resolve, onNone: () => Model.document }).pipe(
            Effect.flatMap((found: unknown) => (Option.isSome(payload.change) ? commit(payload.label, payload.search(found)) : payload.search(found))),
            Effect.flatMap(({ count, matches }) => read(FindChange.Result, payload.label, () => ({ count, matches: payload.slice.of(matches) }))),
        );

    get label(): string {
        return `${Option.isSome(this.change) ? 'Change' : 'Find'} ${this.mode}`;
    }

    search(located: unknown) {
        const { mode, find: sought, change: replacing, options, scope } = this;
        const { find, change, preferences } = modes[mode];
        return Effect.gen(function* () {
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
            const rendered = (contents: unknown): string => Array.map(Array.ensure(contents), (each) => (enumerator(each) ? `<${String(each)}>` : String(each))).join('');
            const scopes = yield* Effect.forEach(
                Array.ensure(located),
                flow(
                    Option.liftPredicate(model.owning<InDesignDOMObject<unknown> & Pick<Document, typeof find | typeof change>>(change)),
                    Effect.fromOption(() => Unmatched.make({ subject: `${mode} search target`, query: Option.map(scope, Struct.get('path')), candidates: [] })),
                ),
            );
            yield* Effect.forEach(
                [
                    [preferences.find, sought],
                    [preferences.options, options],
                    [preferences.change, replacing],
                ] as const,
                ([target, fields]) => Effect.transposeOption(Option.map(fields, (values) => Change.write(app[target], values))),
                { discard: true },
            );
            const describe = (hit: Text | PageItem) => {
                const frame = model.text(hit) ? hit.parentTextFrames[0] : undefined;
                return model.text(hit) ? { specifier: hit.toSpecifier(), text: rendered(hit.contents), page: frame !== undefined && 'parentPage' in frame ? frame.parentPage?.name : undefined } : { specifier: hit.toSpecifier(), page: model.item(hit) ? hit.parentPage?.name : undefined };
            };
            const found = yield* Effect.forEach(
                scopes,
                Effect.fnUntraced(function* (each) {
                    yield* Effect.transposeOption(Option.map(model.owner(each), (context) => preserve(app, { activeDocument: context })));
                    return yield* Effect.try({
                        try: () => {
                            const hits = each[find]();
                            const matches = Predicate.isNumber(hits) ? [] : hits.map(describe);
                            return { count: { matches: Predicate.isNumber(hits) ? hits : hits.length, changes: Option.flatMap(replacing, () => Option.liftPredicate(each[change](), Predicate.isNumber)) }, matches };
                        },
                        catch: HostError.refused(`${mode} search of ${each.toSpecifier()}`),
                    });
                }, Effect.scoped),
            );
            const changes = Array.getSomes(found.map(({ count }) => count.changes));
            return { count: { matches: Number.sumAll(found.map(({ count }) => count.matches)), changes: Option.map(Option.liftPredicate(changes, Array.isReadonlyArrayNonEmpty), Number.sumAll) }, matches: Array.flatMap(found, Struct.get('matches')) };
        }).pipe(Effect.scoped);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const held = Effect.fnUntraced(function* (member: (typeof modes)[keyof typeof modes]['preferences']['find' | 'change' | 'options']) {
    const model = yield* Model;
    const { app, enumerator, NothingEnum } = model;
    const target = app[member];
    const { fields } = yield* Cache.get(model.rows, target.constructorName);
    const current = (name: string) => Option.filter(Option.liftThrowable((): unknown => Reflect.get(target, name))(), (value) => !(enumerator(value) && NothingEnum.NOTHING.equals(value)));
    const writable = Record.filter(fields, (row, name) => row.settable && !model.plural(row) && Option.isNone(row.unavailable) && name !== 'properties');
    return yield* Effect.acquireRelease(
        Effect.sync(() => Record.getSomes(Record.map(writable, (_, name) => current(name)))),
        (saved) => Effect.sync(() => Object.assign(Object.assign(app, { [member]: NothingEnum.NOTHING })[member], saved)),
    );
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { FindChange };
