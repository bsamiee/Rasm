import type { Document, InDesignDOMObject, PageItem, Text } from 'adobe:indesign';
import { matching, Unmatched } from '@rasm/search';
import { Uxp } from '@rasm/uxp/runtime';
import { Array, Cause, Effect, flow, Number, Option, Predicate, pipe, Record, Schema, String, Struct } from 'effect';
import { Dom } from './dom.ts';
import { Model } from './model.ts';
import { write } from './mutations.ts';
import { active, owner, read, resolve, Specifier } from './references.ts';
import { Mutation, preserve } from './transactions.ts';

// --- [MODELS] --------------------------------------------------------------------------

const Mode = Schema.Literals(['Text', 'Grep', 'Glyph', 'Object', 'Color']);
const Replacement = Schema.TaggedUnion({
    Fields: { values: Schema.JsonObject.annotate({ description: "Fields of the mode's change preferences" }) },
    Query: {},
}).annotate({ description: "Fields applies the given change preferences to every match; Query applies the loaded query's changes; absent, the call only finds" });
class FindChange extends Mutation.extend<FindChange>('FindChange')(
    {
        mode: Mode,
        find: Schema.OptionFromOptionalKey(Schema.JsonObject.annotate({ description: "Fields of the mode's find preferences: the pattern and the format criteria" })),
        change: Schema.OptionFromOptionalKey(Replacement),
        options: Schema.OptionFromOptionalKey(Schema.JsonObject.annotate({ description: "Fields of the mode's find/change options" })),
        scope: Schema.OptionFromOptionalKey(Specifier.annotate({ description: 'Specifier of the document, story, text, or page item to search; the active document when absent' })),
        query: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'Saved query loaded before find, change, and options apply' })),
        save: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'Name to save the query under once the call succeeds' })),
    },
    {
        description:
            "Finds or changes native text, GREP, glyph, object, or color matches in one undo step. Search preferences restore after each call. Values decode as change decodes them: enumeration members and named objects by name, {type, specifier} for native objects. Inspect the mode's preference class for its fields",
    },
) {
    static readonly Own = Schema.Struct({
        count: Schema.Struct({ matches: Schema.Int, changes: Schema.OptionFromOptionalKey(Schema.Int.annotate({ description: 'Native numeric change result, returned by Color operations. Absent for array returns and search-only calls; array length does not count edits' })) }),
        matches: Schema.Array(Schema.Struct({ specifier: Specifier, text: Schema.OptionFromOptionalKey(Schema.String), page: Schema.OptionFromOptionalKey(Schema.String) })).annotate({
            description: 'Native matches with their text before changes; inspect their specifiers for surrounding content. Color returns counts alone',
        }),
    });
    static readonly Result = Mutation.summarized(FindChange.Own);

    override get label(): string {
        return `${Option.isSome(this.change) ? 'Change' : 'Find'} ${this.mode}${Option.match(this.query, { onNone: () => '', onSome: (name) => ` query ${name}` })}`;
    }

    run() {
        return this.commit(FindChange.Own, this.search(), Effect.succeed);
    }

    search() {
        const { mode, find: sought, change: replacing, options, scope, query, save } = this;
        return Effect.gen(function* () {
            const [indesign, { owning, text, item }] = yield* Effect.all([Dom, Model]);
            const { app, NothingEnum, SearchModes } = indesign;
            const [find, change] = [`find${mode}`, `change${mode}`] as const;
            const owners = { find: `${find}Preferences`, change: `${change}Preferences`, options: `findChange${mode}Options` } as const;
            const search = SearchModes[`${String.toLowerCase(mode)}search` as const];
            const held = `find-change-${yield* Effect.fiberId}`;
            yield* Effect.acquireRelease(
                Effect.try(() => app.saveFindChangeQuery(held, search)),
                () =>
                    Effect.orDie(
                        Effect.try(() => {
                            app.loadFindChangeQuery(held, search);
                            app.deleteFindChangeQuery(held, search);
                        }),
                    ),
            );
            yield* Effect.try(() => Record.map(owners, (field) => Object.assign(app, { [field]: NothingEnum.NOTHING })));
            const rendered = (contents: unknown): string => Array.map(Array.ensure(contents), (each) => (each instanceof indesign.Enumerator ? `<${globalThis.String(each)}>` : globalThis.String(each))).join('');
            const scopes = yield* Effect.forEach(
                Array.ensure<unknown>(yield* Option.match(scope, { onSome: resolve, onNone: () => active })),
                flow(
                    Option.liftPredicate(owning<InDesignDOMObject<unknown> & Pick<Document, typeof find | typeof change>>(change)),
                    Effect.fromOption(() => Unmatched.make({ subject: `${mode} search target`, query: Option.getOrElse(scope, () => 'active document'), candidates: [] })),
                ),
            );
            const refused = (name: string) => (cause: unknown) => new Cause.UnknownError(cause, `query ${name}`);
            yield* Effect.transposeOption(Option.map(query, (name) => Effect.try({ try: () => app.loadFindChangeQuery(name, search), catch: refused(name) })));
            yield* Effect.forEach(
                [
                    [owners.find, sought],
                    [owners.options, options],
                    [owners.change, Option.flatMap(replacing, Replacement.match({ Fields: ({ values }) => Option.some(values), Query: () => Option.none() }))],
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
                            const changes = Option.isSome(replacing) ? each[change]() : undefined;
                            return { count: { matches: Predicate.isNumber(hits) ? hits : hits.length, changes: Option.liftPredicate(changes, Predicate.isNumber) }, matches };
                        },
                        catch: (cause) => new Cause.UnknownError(cause, `${mode} search of ${each.toSpecifier()}`),
                    });
                }, Effect.scoped),
            );
            yield* Effect.transposeOption(Option.map(save, (name) => Effect.try({ try: () => app.saveFindChangeQuery(name, search), catch: refused(name) })));
            const changes = Array.getSomes(found.map(({ count }) => count.changes));
            return { count: { matches: Number.sumAll(found.map(({ count }) => count.matches)), changes: Option.map(Option.liftPredicate(changes, Array.isReadonlyArrayNonEmpty), Number.sumAll) }, matches: Array.flatMap(found, Struct.get('matches')) };
        }).pipe(Effect.scoped);
    }
}
class Queries extends Schema.Class<Queries, { readonly brand: unique symbol }>('Queries')(
    { name: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'Saved query name, in any case; the query then includes its native XML' })) },
    { description: "Saved Find/Change queries of every mode, factory queries in the application's locale; a name includes the native XML" },
) {
    static readonly Query = Schema.Struct({ name: Schema.String, mode: Mode, origin: Schema.Literals(['user', 'factory']), content: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'Native query XML with complete patterns, formatting, effects, and options' })) });
    static readonly Result = Schema.Array(Queries.Query);

    run() {
        const { name } = this;
        return Effect.gen(function* () {
            const [{ app, ScriptLanguage }, { fs }] = yield* Effect.all([Dom, Uxp]);
            const listing = `(function (factory) {
    var rows = [], modes = Folder(factory ? app.filePath + '/Presets/Find-Change Queries' : app.scriptPreferences.scriptsFolder.parent.parent + '/Find-Change Queries').getFiles() || [];
    for (var at = 0; at < modes.length; at++) {
        if (!(modes[at] instanceof Folder)) continue;
        var files = Folder(modes[at].fsName + (factory ? '/' + $.locale : '')).getFiles('*.xml') || [];
        for (var each = 0; each < files.length; each++) rows.push([modes[at].displayName, files[each].displayName.replace(/\\.xml$/i, ''), files[each].fsName]);
    }
    return rows;
})(arguments[0])`;
            const found = yield* Effect.forEach(Queries.Query.fields.origin.literals, (origin) =>
                Effect.map(
                    read(Schema.Array(Schema.Tuple([Schema.String, Schema.String, Schema.String])), () => app.doScript(listing, ScriptLanguage.JAVASCRIPT, [origin === 'factory'])),
                    Array.map(([folder, query, path]) => ({ folder, name: query, origin, path })),
                ),
            );
            const rows = Array.flatMap(found.flat(), ({ folder, ...row }) =>
                Option.toArray(
                    Option.map(
                        Array.findFirst(Mode.literals, (mode) => String.toLowerCase(mode) === String.toLowerCase(folder)),
                        (mode) => ({ ...row, mode }),
                    ),
                ),
            );
            const chosen = yield* matching('saved query', name)(rows, { names: (row) => [row.name], label: Struct.get('name') });
            return yield* Effect.forEach(chosen, ({ path, ...row }) =>
                pipe(
                    name,
                    Option.as(Effect.try(() => fs.readFileSync(`file:${path}`, { encoding: 'utf-8' }))),
                    Effect.transposeOption,
                    Effect.map((content) => ({ ...row, content })),
                ),
            );
        });
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { FindChange, Queries };
