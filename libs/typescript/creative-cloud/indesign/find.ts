import type { Document, InDesignDOMObject, PageItem, Text } from 'adobe:indesign';
import { Unmatched } from '@rasm/search';
import { Array, Cause, Effect, flow, Number, Option, Predicate, Record, Schema, String, Struct } from 'effect';
import { Model } from './model.ts';
import { write } from './mutations.ts';
import { active, owner, read, resolve, Specifier } from './references.ts';
import { Mutation, preserve } from './transactions.ts';

// --- [MODELS] --------------------------------------------------------------------------

class FindChange extends Mutation.extend<FindChange>('FindChange')(
    {
        mode: Schema.Literals(['Text', 'Grep', 'Glyph', 'Object', 'Color']),
        find: Schema.OptionFromOptionalKey(Schema.JsonObject.annotate({ description: "Fields of the mode's find preferences: the pattern and the format criteria" })),
        change: Schema.OptionFromOptionalKey(Schema.JsonObject.annotate({ description: "Fields of the mode's change preferences applied to every match; absent, the call only finds" })),
        options: Schema.OptionFromOptionalKey(Schema.JsonObject.annotate({ description: "Fields of the mode's find/change options" })),
        scope: Schema.OptionFromOptionalKey(Specifier.annotate({ description: 'Specifier of the document, story, text, or page item to search; the active document when absent' })),
    },
    {
        description:
            "Finds or changes native text, GREP, glyph, object, or color matches in one undo step. Search preferences restore after each call. Values decode as change decodes them: enumeration members and named objects by name, {type, specifier} for native objects. Inspect the mode's preference class for its fields",
    },
) {
    static readonly Result = Schema.Struct({
        count: Schema.Struct({ matches: Schema.Int, changes: Schema.OptionFromOptionalKey(Schema.Int.annotate({ description: 'Native numeric change result, returned by Color operations. Absent for array returns and search-only calls; array length does not count edits' })) }),
        matches: Schema.Array(Schema.Struct({ specifier: Specifier, text: Schema.OptionFromOptionalKey(Schema.String), page: Schema.OptionFromOptionalKey(Schema.String) })).annotate({
            description: 'Native matches with their text before changes; inspect their specifiers for surrounding content. Color returns counts alone',
        }),
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
            yield* Effect.acquireRelease(Effect.try({ try: () => app.saveFindChangeQuery(held, search), catch: (cause) => new Cause.UnknownError(cause, `${mode} preferences held as query ${held}`) }), () =>
                Effect.sync(() => {
                    app.loadFindChangeQuery(held, search);
                    app.deleteFindChangeQuery(held, search);
                }),
            );
            yield* Effect.try({ try: () => Record.map(owners, (field) => Object.assign(app, { [field]: NothingEnum.NOTHING })), catch: (cause) => new Cause.UnknownError(cause, `${mode} preference reset`) });
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
                ([target, fields]) => Effect.transposeOption(Option.map(fields, (values) => write(app[target], values))),
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
