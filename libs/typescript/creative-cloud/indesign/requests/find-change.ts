import type { Document, InDesignDOMObject, PageItem, Text } from 'adobe:indesign';
import { Unmatched } from '@rasm/search';
import { Array, Cause, Effect, flow, Number, Option, Predicate, Record, Schema, String, Struct } from 'effect';
import { read } from '#/dom/encoding';
import { active, owner, resolve } from '#/dom/location';
import { Model } from '#/dom/model';
import { Specifier } from '#/dom/reference';
import { Mutation, preserve } from '#/dom/transactions';
import { Change } from '#/requests/change';

// --- [MODELS] --------------------------------------------------------------------------

class FindChange extends Mutation.extend<FindChange>('FindChange')(
    {
        mode: Schema.Literals(['Text', 'Grep', 'Glyph', 'Object', 'Color']),
        find: Schema.OptionFromOptionalKey(Schema.JsonObject.annotate({ description: 'Find preference fields (pattern, format criteria)' })),
        change: Schema.OptionFromOptionalKey(Schema.JsonObject.annotate({ description: 'Change preference fields for every match, absent to find only' })),
        options: Schema.OptionFromOptionalKey(Schema.JsonObject.annotate({ description: 'Find/change option fields' })),
        scope: Schema.OptionFromOptionalKey(Specifier.annotate({ description: 'Document, story, text, or page item to search, active document when absent' })),
    },
    { description: 'Finds or changes matches in one undo step and restores search preferences. Values take change forms, kind lists preference fields' },
) {
    static readonly Result = Schema.Struct({
        count: Schema.Struct({ matches: Schema.Int, changes: Schema.OptionFromOptionalKey(Schema.Int.annotate({ description: 'Color mode change count, absent otherwise' })) }),
        matches: Schema.Array(Schema.Struct({ specifier: Specifier, text: Schema.OptionFromOptionalKey(Schema.String), page: Schema.OptionFromOptionalKey(Schema.String) })).annotate({ description: 'Matches with text before changes, empty for Color' }),
    });

    override get label(): string {
        return `${Option.isSome(this.change) ? 'Change' : 'Find'} ${this.mode}`;
    }

    run() {
        return Option.match(this.scope, { onSome: resolve, onNone: () => active }).pipe(
            Effect.flatMap((found: unknown) => Option.match(this.change, { onNone: () => this.search(found), onSome: () => this.commit(this.search(found)) })),
            Effect.flatMap((found) => read(FindChange.Result, this.label, () => found)),
        );
    }

    search(located: unknown) {
        const { mode, find: sought, change: replacing, options, scope } = this;
        return Effect.gen(function* () {
            const { app, Enumerator, NothingEnum, SearchModes, owning, text, item } = yield* Model;
            const [find, change] = [`find${mode}`, `change${mode}`] as const;
            const owners = { find: `${find}Preferences`, change: `${change}Preferences`, options: `findChange${mode}Options` } as const;
            const search = SearchModes[`${String.toLowerCase(mode)}search` as const];
            const held = `find-change-${yield* Effect.fiberId}`;
            yield* Effect.acquireRelease(Effect.try({ try: () => app.saveFindChangeQuery(held, search), catch: (cause) => new Cause.UnknownError(cause, `save of ${mode} preferences as query ${held}`) }), () =>
                Effect.sync(() => {
                    app.loadFindChangeQuery(held, search);
                    app.deleteFindChangeQuery(held, search);
                }),
            );
            yield* Effect.try({ try: () => Record.map(owners, (field) => Object.assign(app, { [field]: NothingEnum.NOTHING })), catch: (cause) => new Cause.UnknownError(cause, `reset of ${mode} preferences`) });
            const rendered = (contents: unknown): string => Array.map(Array.ensure(contents), (each) => (each instanceof Enumerator ? `<${globalThis.String(each)}>` : globalThis.String(each))).join('');
            const scopes = yield* Effect.forEach(
                Array.ensure(located),
                flow(
                    Option.liftPredicate(owning<InDesignDOMObject<unknown> & Pick<Document, typeof find | typeof change>>(change)),
                    Effect.fromOption(() => Unmatched.make({ subject: `${mode} search target`, query: scope, candidates: [] })),
                ),
            );
            yield* Effect.forEach(
                [
                    [owners.find, sought],
                    [owners.options, options],
                    [owners.change, replacing],
                ] as const,
                ([target, fields]) => Effect.transposeOption(Option.map(fields, (values) => Change.write(app[target], values))),
                { discard: true },
            );
            const describe = (hit: Text | PageItem) => {
                const frame = text(hit) ? hit.parentTextFrames[0] : undefined;
                return text(hit) ? { specifier: hit.toSpecifier(), text: rendered(hit.contents), page: frame !== undefined && 'parentPage' in frame ? frame.parentPage?.name : undefined } : { specifier: hit.toSpecifier(), page: item(hit) ? hit.parentPage?.name : undefined };
            };
            const found = yield* Effect.forEach(
                scopes,
                Effect.fnUntraced(function* (each) {
                    yield* Effect.transposeOption(Option.map(yield* owner(each), (context) => preserve(app, { activeDocument: context })));
                    return yield* Effect.try({
                        try: () => {
                            const hits = each[find]();
                            const matches = Predicate.isNumber(hits) ? [] : hits.map(describe);
                            return { count: { matches: Predicate.isNumber(hits) ? hits : hits.length, changes: Option.flatMap(replacing, () => Option.liftPredicate(each[change](), Predicate.isNumber)) }, matches };
                        },
                        catch: (cause) => new Cause.UnknownError(cause, `${mode} search of ${each.toSpecifier()}`),
                    });
                }, Effect.scoped),
            );
            const changes = Array.getSomes(found.map(({ count }) => count.changes));
            return { count: { matches: Number.sumAll(found.map(({ count }) => count.matches)), changes: Option.map(Option.liftPredicate(changes, Array.isReadonlyArrayNonEmpty), Number.sumAll) }, matches: Array.flatMap(found, Struct.get('matches')) };
        }).pipe(Effect.scoped);
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { FindChange };
