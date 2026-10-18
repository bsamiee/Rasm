import fs from 'adobe:fs';
import indesign, { type Document, type PageItem, type Text } from 'adobe:indesign';
import { Unmatched } from '@rasm/search';
import { Array, Effect, flow, Iterable, Match, Number, Option, Predicate, Record, Schema, Struct } from 'effect';
import { active, host, owning, preserve, read, resolve, transacted, write } from '../document/objects.ts';
import { InDesignError, Operations } from '../models.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [PREFERENCES]
const preferences = Effect.fn('preferences')(function* (mode: (typeof Operations.find_change.payload.Type)['mode']) {
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
    yield* Effect.acquireRelease(Effect.try({ try: () => app.saveFindChangeQuery(name, search), catch: InDesignError.refused(Option.some(name)) }), () =>
        Effect.try({
            try: () => {
                app.loadFindChangeQuery(name, search);
                app.deleteFindChangeQuery(name, search);
            },
            catch: InDesignError.refused(Option.some(name)),
        }).pipe(Effect.orDie),
    );
    yield* Effect.try({ try: () => Record.map(fields, (field) => Object.assign(app, { [field]: NothingEnum.NOTHING })), catch: InDesignError.refused(Option.none()) });
    return {
        preferences: fields,
        search,
        find,
        change,
        scope: owning<Pick<Document, typeof find | typeof change>>(change),
    };
});

// --- [FIND]
const searched = Effect.fn('searched')(function* ({ mode, find: sought, change: replacing, options, scope, query, save }: typeof Operations.find_change.payload.Type) {
    const { app } = indesign;
    const [textual, placed, document] = [owning<Text>('parentTextFrames'), owning<PageItem>('itemLayer'), owning<Document>('createTOC')];
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
        return textual(hit)
            ? {
                  specifier: hit.toSpecifier(),
                  text: rendered(hit.contents),
                  page: frame !== undefined && 'parentPage' in frame ? frame.parentPage?.name : undefined,
              }
            : { specifier: hit.toSpecifier(), page: placed(hit) ? hit.parentPage?.name : undefined };
    };
    const found = yield* Effect.forEach(
        scopes,
        Effect.fnUntraced(function* (each) {
            const ancestors = Iterable.unfold(each, (owner: unknown) => (host(owner) && owner.constructorName !== 'Application' ? Option.some([owner, owner.parent] as const) : Option.none()));
            const context = yield* Effect.try({ try: () => Iterable.findFirst(ancestors, document), catch: InDesignError.refused(scope) });
            yield* Effect.forEach(Option.toArray(context), (owner) => preserve(app, { activeDocument: owner }), { discard: true });
            yield* Effect.forEach(Option.toArray(query), (name) => Effect.try({ try: () => app.loadFindChangeQuery(name, search), catch: InDesignError.refused(Option.some(name)) }), { discard: true });
            yield* Effect.forEach(
                [
                    [owners.find, sought],
                    [owners.options, options],
                    [owners.change, replacing],
                ] as const,
                ([owner, fields]) => Option.match(fields, { onNone: () => Effect.void, onSome: (values) => write(app[owner], values) }),
                { discard: true },
            );
            const result = yield* read(Operations.find_change.success, () => {
                const hits = each[find]();
                const matches = Predicate.isNumber(hits) ? [] : hits.map(describe);
                const changes = Option.isSome(replacing) ? each[change]() : undefined;
                return { count: { matches: Predicate.isNumber(hits) ? hits : hits.length, changes: Predicate.isNumber(changes) ? changes : undefined }, matches };
            });
            yield* Effect.forEach(Option.toArray(save), (name) => Effect.try({ try: () => app.saveFindChangeQuery(name, search), catch: InDesignError.refused(Option.some(name)) }), { discard: true });
            return result;
        }, Effect.scoped),
    );
    const changes = Array.getSomes(found.map(({ count }) => count.changes));
    return { count: { matches: Number.sumAll(found.map(({ count }) => count.matches)), changes: Array.isReadonlyArrayNonEmpty(changes) ? Option.some(Number.sumAll(changes)) : Option.none() }, matches: Array.flatMap(found, Struct.get('matches')) };
});
const find = (payload: typeof Operations.find_change.payload.Type) => (Option.isSome(payload.change) ? transacted(payload.description, Effect.scoped(searched(payload))) : Effect.scoped(searched(payload)));

// --- [QUERIES]
const savedQueries = Effect.gen(function* () {
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
    const { origin, mode } = Operations.queries.success.value.fields;
    const listed = yield* Effect.forEach(origin.literals, (from) =>
        Effect.map(
            read(Schema.Array(Schema.Tuple([Schema.String, Schema.String, Schema.String])), () => app.doScript(listing, ScriptLanguage.JAVASCRIPT, [from === 'factory'])),
            Array.map(([folder, query, path]: readonly [string, string, string]) => ({ folder, query, path, from })),
        ),
    );
    const entries = Array.flatten(listed).flatMap(({ folder, query, path, from }) =>
        Array.map(
            mode.literals.filter((kind) => kind.toLowerCase() === folder.toLowerCase()),
            (kind) => ({ name: query, mode: kind, origin: from, path }),
        ),
    );
    return Array.dedupeWith(entries, (left, right) => left.name === right.name && left.mode === right.mode && left.origin === right.origin);
});
const queries = Effect.fn('queries')(function* ({ name }: typeof Operations.queries.payload.Type) {
    const rows = yield* savedQueries;
    const chosen = Option.match(name, { onNone: () => rows, onSome: (query) => rows.filter((row) => row.name === query) });
    yield* Option.isSome(name) && Array.isReadonlyArrayEmpty(chosen) ? Effect.fail(Unmatched.make({ subject: 'saved query', query: name.value, candidates: Array.map(rows, Struct.get('name')) })) : Effect.void;
    return yield* Effect.forEach(chosen, (row) => read(Operations.queries.success.value, () => ({ ...row, content: Option.isSome(name) ? fs.readFileSync(`file:${row.path}`, { encoding: 'utf-8' }) : undefined })));
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { find, preferences, queries, savedQueries };
