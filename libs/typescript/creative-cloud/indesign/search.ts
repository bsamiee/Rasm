import type { Document, InDesignDOMObject, PageItem, Text } from 'adobe:indesign';
import { Unmatched } from '@rasm/search';
import { Array, Cause, Effect, flow, Match, Number, Option, Predicate, Record, Schema, Struct } from 'effect';
import { Host } from './host.ts';
import { Classes } from './objects/classes.ts';
import { active, read, resolve, Specifier } from './objects/specifiers.ts';
import { Mutation } from './objects/transactions.ts';
import { preserve, write } from './objects/values.ts';

// --- [MODELS] --------------------------------------------------------------------------

const Mode = Schema.Literals(['Text', 'Grep', 'Glyph', 'Object', 'Color']);
class FindChange extends Mutation.extend<FindChange>('FindChange')(
    {
        mode: Mode,
        find: Schema.OptionFromOptionalKey(Schema.JsonObject.annotate({ description: "Fields of the mode's find preferences: the pattern and the format criteria" })),
        change: Schema.OptionFromOptionalKey(Schema.JsonObject.annotate({ description: "Fields of the mode's change preferences, applied to every match. An empty object applies a saved query's changes; absent, the call only finds" })),
        options: Schema.OptionFromOptionalKey(Schema.JsonObject.annotate({ description: "Fields of the mode's find/change options" })),
        scope: Schema.OptionFromOptionalKey(Specifier.annotate({ identifier: undefined, description: 'Specifier of the document, story, text, or page item to search; the active document when absent' })),
        query: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'Saved query loaded before find, change, and options apply' })),
        save: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'Name to save the query under once the call succeeds' })),
    },
    {
        description:
            "Finds or changes native text, GREP, glyph, object, or color matches in one undo step. Search preferences restore after each call. Values decode as change decodes them: enumeration members and named objects by name, {type, specifier} for native objects. Inspect the mode's preference class for its fields",
    },
) {
    static readonly Result = Mutation.Result({
        count: Schema.Struct({ matches: Schema.Int, changes: Schema.OptionFromOptionalKey(Schema.Int.annotate({ description: 'Native numeric change result, returned by Color operations. Absent for array returns and search-only calls; array length does not count edits' })) }),
        matches: Schema.Array(Schema.Struct({ specifier: Specifier, text: Schema.OptionFromOptionalKey(Schema.String), page: Schema.OptionFromOptionalKey(Schema.String) })).annotate({
            description: 'Native matches with their text before changes; inspect their specifiers for surrounding content. Color returns counts alone',
        }),
    });

    override get label(): string {
        return `${Option.isSome(this.change) ? 'Change' : 'Find'} ${this.mode}${Option.match(this.query, { onNone: () => '', onSome: (name) => ` query ${name}` })}`;
    }

    run() {
        const { mode, find: sought, change: replacing, options, scope, query, save } = this;
        const searched = Effect.gen(function* () {
            const [{ indesign }, { owning, owner }] = yield* Effect.all([Host, Classes]);
            const { app } = indesign;
            const [textual, placed] = [owning<Text>('parentTextFrames'), owning<PageItem>('itemLayer')];
            const rendered = (contents: unknown): string => Array.map(Array.ensure(contents), (each) => (each instanceof indesign.Enumerator ? `<${globalThis.String(each)}>` : globalThis.String(each))).join('');
            const { preferences: owners, search, scope: searchable, find, change } = yield* preferences(mode);
            const [targets, subject] = [Array.ensure<unknown>(yield* Option.match(scope, { onSome: resolve, onNone: () => active })), Unmatched.make({ subject: `${mode} search target`, query: Option.getOrElse(scope, () => 'active document'), candidates: [] })];
            const scopes = yield* Effect.forEach(
                targets,
                flow(
                    Option.liftPredicate(searchable),
                    Effect.fromOption(() => subject),
                ),
            );
            const describe = (hit: Text | PageItem) => {
                const frame = textual(hit) ? hit.parentTextFrames[0] : undefined;
                return textual(hit) ? { specifier: hit.toSpecifier(), text: rendered(hit.contents), page: frame !== undefined && 'parentPage' in frame ? frame.parentPage?.name : undefined } : { specifier: hit.toSpecifier(), page: placed(hit) ? hit.parentPage?.name : undefined };
            };
            const found = yield* Effect.forEach(
                scopes,
                Effect.fnUntraced(function* (each) {
                    yield* Effect.forEach(Option.toArray(owner(each)), (context) => preserve(app, { activeDocument: context }), { discard: true });
                    yield* Effect.forEach(Option.toArray(query), (name) => Effect.try({ try: () => app.loadFindChangeQuery(name, search), catch: (cause) => new Cause.UnknownError(cause, `query ${name}`) }), { discard: true });
                    yield* Effect.forEach(
                        [
                            [owners.find, sought],
                            [owners.options, options],
                            [owners.change, replacing],
                        ] as const,
                        ([target, fields]) => Option.match(fields, { onNone: () => Effect.void, onSome: (values) => write(app[target], values) }),
                        { discard: true },
                    );
                    const result = yield* Effect.try({
                        try: () => {
                            const hits = each[find]();
                            const changes = Option.isSome(replacing) ? each[change]() : undefined;
                            return { count: { matches: Predicate.isNumber(hits) ? hits : hits.length, changes: Option.liftPredicate(changes, Predicate.isNumber) }, matches: Predicate.isNumber(hits) ? [] : hits.map(describe) };
                        },
                        catch: (cause) => new Cause.UnknownError(cause, `${mode} search of ${each.toSpecifier()}`),
                    });
                    yield* Effect.forEach(Option.toArray(save), (name) => Effect.try({ try: () => app.saveFindChangeQuery(name, search), catch: (cause) => new Cause.UnknownError(cause, `query ${name}`) }), { discard: true });
                    return result;
                }, Effect.scoped),
            );
            const changes = Array.getSomes(found.map(({ count }) => count.changes));
            return { count: { matches: Number.sumAll(found.map(({ count }) => count.matches)), changes: Option.map(Option.liftPredicate(changes, Array.isReadonlyArrayNonEmpty), Number.sumAll) }, matches: Array.flatMap(found, Struct.get('matches')) };
        }).pipe(Effect.scoped);
        const found = Effect.gen(function* () {
            const value = yield* searched;
            return { ...(yield* read(FindChange.Result.mapFields(Struct.omit(['preflight'])), () => value)), preflight: [] };
        });
        return Option.isSome(replacing) ? this.commit(FindChange.Result, searched) : found;
    }
}
class Query extends Schema.Class<Query, { readonly brand: unique symbol }>('Query')(
    { name: Schema.String, mode: Mode, origin: Schema.Literals(['user', 'factory']), content: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'Native query XML with complete patterns, formatting, effects, and options' })) },
    { description: 'Saved Find/Change queries of every mode; a name includes the native XML' },
) {
    static readonly listed = Effect.fn('listed')(function* (name: Option.Option<string>) {
        const { indesign, fs } = yield* Host;
        const { app, ScriptLanguage } = indesign;
        const listing = `(function (factory) {
    var rows = [], modes = Folder(factory ? app.filePath + '/Presets/Find-Change Queries' : app.scriptPreferences.scriptsFolder.parent.parent + '/Find-Change Queries').getFiles() || [];
    function walk(folder, mode) {
        var items = folder.getFiles() || [];
        for (var at = 0; at < items.length; at++) {
            if (items[at] instanceof Folder) walk(items[at], mode);
            else if (/\\.xml$/i.test(items[at].name)) rows.push([mode, items[at].displayName.replace(/\\.xml$/i, ''), items[at].fsName]);
        }
    }
    for (var at = 0; at < modes.length; at++) if (modes[at] instanceof Folder) walk(modes[at], modes[at].displayName);
    return rows;
})(arguments[0])`;
        const found = yield* Effect.forEach(Query.fields.origin.literals, (origin) =>
            Effect.map(
                read(Schema.Array(Schema.Tuple([Schema.String, Schema.String, Schema.String])), () => app.doScript(listing, ScriptLanguage.JAVASCRIPT, [origin === 'factory'])),
                Array.map(([folder, query, path]) => ({ origin, folder, query, path })),
            ),
        );
        const rows = Array.dedupeWith(
            found.flat().flatMap(({ origin, folder, query, path }) => Mode.literals.filter((mode) => mode.toLowerCase() === folder.toLowerCase()).map((mode) => ({ name: query, mode, origin, path }))),
            (left, right) => left.name === right.name && left.mode === right.mode && left.origin === right.origin,
        );
        const chosen = Option.match(name, { onNone: () => rows, onSome: (query) => rows.filter((row) => row.name === query) });
        yield* Option.isSome(name) && Array.isReadonlyArrayEmpty(chosen) ? Effect.fail(Unmatched.make({ subject: 'saved query', query: name.value, candidates: Array.map(rows, Struct.get('name')) })) : Effect.void;
        return yield* Effect.forEach(chosen, ({ path, ...row }) => read(Query, () => ({ ...row, content: Option.isSome(name) ? fs.readFileSync(`file:${path}`, { encoding: 'utf-8' }) : undefined })));
    });
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const preferences = Effect.fn('preferences')(function* (mode: typeof Mode.Type) {
    const [{ indesign }, { owning }] = yield* Effect.all([Host, Classes]);
    const { app, NothingEnum, SearchModes } = indesign;
    const [find, change] = [`find${mode}`, `change${mode}`] as const;
    const fields = { find: `${find}Preferences`, change: `${change}Preferences`, options: `findChange${mode}Options` } as const;
    const search = Match.value(mode).pipe(
        Match.when('Text', () => SearchModes.TEXT_SEARCH),
        Match.when('Grep', () => SearchModes.GREP_SEARCH),
        Match.when('Glyph', () => SearchModes.GLYPH_SEARCH),
        Match.when('Object', () => SearchModes.OBJECT_SEARCH),
        Match.when('Color', () => SearchModes.COLOR_SEARCH),
        Match.exhaustive,
    );
    const name = `find-change-${yield* Effect.fiberId}`;
    yield* Effect.acquireRelease(
        Effect.try(() => app.saveFindChangeQuery(name, search)),
        () =>
            Effect.orDie(
                Effect.try(() => {
                    app.loadFindChangeQuery(name, search);
                    app.deleteFindChangeQuery(name, search);
                }),
            ),
    );
    yield* Effect.try(() => Record.map(fields, (field) => Object.assign(app, { [field]: NothingEnum.NOTHING })));
    return { preferences: fields, search, find, change, scope: owning<InDesignDOMObject<unknown> & Pick<Document, typeof find | typeof change>>(change) };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { FindChange, preferences, Query };
