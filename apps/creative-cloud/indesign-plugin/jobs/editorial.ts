// --- [IMPORTS] -------------------------------------------------------------------------

import { app, type Character, type Document, ListType, NothingEnum, PageBindingOptions, type Paragraph, SearchModes, type Story, StoryDirectionOptions } from 'adobe:indesign';
import { read } from '@rasm/creative-cloud-server/client';
import { type HostRejection, WriteRejection } from '@rasm/creative-cloud-server/errors';
import { members } from '@rasm/creative-cloud-server/indesign';
import { type Applied, EditorialError, type Input, NativeConstant, Output } from '@rasm/creative-cloud-server/indesign/editorial';
import { Array, Effect, Equal, HashMap, Match, Option, Record, Result, Schema, Struct } from 'effect';
import { constant, documentFor, type Live, live, translate, undoable } from '../host.ts';
import { rightToLeft } from '../typography.ts';
import { render, writeProperty } from './preferences.ts';

// --- [NATIVE SELECTIONS] ---------------------------------------------------------------

interface Write {
    readonly target: object;
    readonly path: string;
    readonly values: Readonly<Record<string, unknown>>;
}

const _story = (document: Document, id: number): Effect.Effect<Story, typeof EditorialError.Type> =>
    Effect.filterOrFail(
        Effect.sync(() => document.stories.itemByID(id)),
        Struct.get('isValid'),
        () => EditorialError.cases.selectionMissing.make({ collection: 'stories', id }),
    );

const _range: (
    document: Document,
    collection: 'paragraphs' | 'characters',
    range: { readonly storyId: number; readonly from: number; readonly to: number },
) => Effect.Effect<readonly (Paragraph | Character)[], typeof EditorialError.Type> = Effect.fnUntraced(function* (document, collection, { storyId, from, to }) {
    const story = yield* _story(document, storyId);
    const selected = story[collection];
    if (to >= selected.length) {
        return yield* Effect.fail(EditorialError.cases.rangeOutOfBounds.make({ storyId, collection, from, to, count: selected.length }));
    }
    return selected.itemByRange(from, to).getElements();
});

// --- [READBACK] ------------------------------------------------------------------------

const _apply = (table: Live, targets: readonly Write[]): typeof Applied.Type => {
    const entries = Array.flatMap(targets, ({ target, path, values }) => Array.map(Record.toEntries(values), ([key, value]) => ({ target, path, key, value })));
    const committed = Array.map(entries, ({ target, path, key, value }) => ({
        path: `${path}.${key}`,
        result: Result.gen(function* () {
            const { writable, property } = yield* Result.fromOption(
                Option.flatMap(
                    Array.findFirst(table.owners, ({ constructor }) => target instanceof constructor),
                    (owner) => Option.all({ writable: Record.get(owner.members, key), property: Record.get(owner.properties, key) }),
                ),
                () => WriteRejection.cases.unknownKey.make({}),
            );
            if (!writable || Record.has<string, boolean>(members.Preference, key)) {
                return yield* Result.fail(WriteRejection.cases.readOnly.make({}));
            }
            const assigned = yield* value instanceof NativeConstant
                ? Result.map(
                      Result.fromOption(HashMap.get(table.lookup.exact, [value.enumeration, value.constant]), () =>
                          WriteRejection.cases.threw.make({ cause: EditorialError.cases.nativeConstantMissing.make({ property: key, constant: value.constant }) }),
                      ),
                      Struct.get('enumerator'),
                  )
                : Result.succeed(Option.match(constant(table, target, key, value), { onNone: () => value, onSome: Struct.get('enumerator') }));
            const from = yield* Result.mapError(read(render)(target, key), (cause) => WriteRejection.cases.threw.make({ cause }));
            const intended = yield* Result.try(() => render(assigned)).pipe(
                Result.mapError((cause) => WriteRejection.cases.threw.make({ cause })),
                Result.flatMap(Result.fromOption(() => WriteRejection.cases.threw.make({ cause: assigned }))),
                Result.flatMap((rendered) => writeProperty(target, key, assigned, rendered, property.types)),
            );
            return { target, key, from, intended };
        }),
    }));
    const [rejected, applied] = Array.separate(
        Array.map(committed, ({ path, result }) =>
            Result.flatMap(result, ({ target, key, from, intended }) =>
                read(render)(target, key).pipe(
                    Result.mapError((cause) => WriteRejection.cases.threw.make({ cause })),
                    Result.filterOrFail(Equal.equals(intended), () => WriteRejection.cases.unchanged.make({})),
                    Result.map((to) => ({ from, to })),
                ),
            ).pipe(Result.mapBoth({ onSuccess: (changed) => ({ path, ...changed }), onFailure: (reason) => ({ path, reason }) })),
        ),
    );
    return { kind: 'applied', applied, rejected };
};

// --- [HANDLER] -------------------------------------------------------------------------

const editDocument: (body: typeof Input.Type) => Effect.Effect<typeof Output.Type, HostRejection> = Effect.fnUntraced(
    function* ({ documentId, operation }: typeof Input.Type) {
        const table = yield* live;
        if (operation._tag === 'rtlDefaults' || operation._tag === 'rtlStyles') {
            const definitions = operation._tag === 'rtlDefaults' ? [Struct.pick(operation, ['role', 'languageId', 'overrides'])] : operation.styles;
            const [composer, languages] = yield* Effect.all([
                translate('$ID/HL Composer Optyca'),
                Effect.forEach(definitions, ({ languageId }) =>
                    Effect.filterOrFail(
                        Effect.sync(() => app.languagesWithVendors.itemByID(languageId)),
                        Struct.get('isValid'),
                        () => EditorialError.cases.selectionMissing.make({ collection: 'languagesWithVendors', id: languageId }),
                    ),
                ),
            ]);
            const policies = Array.zipWith(definitions, languages, ({ role, overrides }, language) => ({
                ...rightToLeft(role, language, composer),
                ...Record.fromIterableWith(overrides, ({ key, value }) => [key, value]),
            }));
            if (operation._tag === 'rtlDefaults') {
                const { owner, stories } =
                    operation.scope === 'application'
                        ? { owner: app, stories: [] }
                        : yield* Effect.map(documentFor(documentId), (open) => ({ owner: open, stories: open.stories.everyItem().getElements() }));
                const values = Array.getUnsafe(policies, 0);
                const storyPreferences = [owner.storyPreferences, ...Array.map(stories, Struct.get('storyPreferences'))];
                const defaults = operation.scope === 'application' ? [owner.textDefaults, owner.textDefaults.appliedParagraphStyle] : [owner.textDefaults];
                const rows: readonly Write[] = [
                    ...Array.map(defaults, (target) => ({ target, path: target.toSpecifier(), values })),
                    { target: owner.documentPreferences, path: 'documentPreferences', values: { pageBinding: PageBindingOptions.RIGHT_TO_LEFT } },
                    ...Array.map(storyPreferences, (target) => ({ target, path: target.toSpecifier(), values: { storyDirection: StoryDirectionOptions.RIGHT_TO_LEFT_DIRECTION } })),
                ];
                return yield* undoable('Apply RTL Defaults', Output, () => _apply(table, rows));
            }
            const document = yield* documentFor(documentId);
            const baseIds = Array.dedupe(Array.flatMap(operation.styles, ({ basedOn }) => Option.toArray(basedOn)));
            const bases = yield* Effect.forEach(baseIds, (id) =>
                Effect.filterOrFail(
                    Effect.sync(() => document.paragraphStyles.itemByID(id)),
                    Struct.get('isValid'),
                    () => EditorialError.cases.selectionMissing.make({ collection: 'paragraphStyles', id }),
                ),
            );
            const inherited = HashMap.fromIterable(Array.map(bases, (style) => [style.id, style] as const));
            const updates = Array.zipWith(operation.styles, policies, ({ name, basedOn }, policy) => {
                const values = { ...Record.getSomes({ basedOn: Option.flatMap(basedOn, (id) => HashMap.get(inherited, id)) }), ...policy };
                return {
                    try: () => {
                        const existing = document.paragraphStyles.itemByName(name);
                        const target = existing.isValid ? existing : document.paragraphStyles.add({ name });
                        return _apply(table, [{ target, path: target.toSpecifier(), values }]);
                    },
                    catch: (cause: unknown) => ({ path: `paragraphStyles.${name}`, reason: WriteRejection.cases.threw.make({ cause }) }),
                };
            });
            return yield* undoable('Apply RTL Styles', Output, (): typeof Output.Type => {
                const [creationRejected, completed] = Array.separate(Array.map(updates, Result.try<typeof Applied.Type, (typeof Applied.Type)['rejected'][number]>));
                const rejected = [...creationRejected, ...Array.flatMap(completed, Struct.get('rejected'))];
                const composed = operation.changeComposer && rejected.length === 0 ? Result.try(() => document.changeComposer()) : Result.void;
                return {
                    kind: 'applied',
                    applied: Array.flatMap(completed, Struct.get('applied')),
                    rejected: Result.isFailure(composed) ? [...rejected, { path: 'changeComposer', reason: WriteRejection.cases.threw.make({ cause: composed.failure }) }] : rejected,
                };
            });
        }
        const document = yield* documentFor(documentId);
        switch (operation._tag) {
            case 'direction': {
                const rows: readonly Write[] = yield* Match.value(operation.target).pipe(
                    Match.when({ _tag: 'document' }, ({ value }) => Effect.succeed([{ target: document.documentPreferences, path: 'documentPreferences', values: { pageBinding: value } }])),
                    Match.when({ _tag: 'story' }, ({ storyId, value }) =>
                        Effect.map(_story(document, storyId), (story) => [{ target: story.storyPreferences, path: story.toSpecifier(), values: { storyDirection: value } }]),
                    ),
                    Match.when({ _tag: 'paragraphs' }, ({ range, value }) =>
                        Effect.map(
                            _range(document, 'paragraphs', range),
                            Array.map((target) => ({ target, path: target.toSpecifier(), values: { paragraphDirection: value } })),
                        ),
                    ),
                    Match.when({ _tag: 'characters' }, ({ range, value }) =>
                        Effect.map(
                            _range(document, 'characters', range),
                            Array.map((target) => ({ target, path: target.toSpecifier(), values: { characterDirection: value } })),
                        ),
                    ),
                    Match.when({ _tag: 'table' }, ({ storyId, tableId, value }) =>
                        Effect.map(
                            Effect.filterOrFail(
                                Effect.map(_story(document, storyId), (story) => story.tables.itemByID(tableId)),
                                Struct.get('isValid'),
                                () => EditorialError.cases.selectionMissing.make({ collection: 'tables', id: tableId }),
                            ),
                            (target) => [{ target, path: target.toSpecifier(), values: { tableDirection: value } }],
                        ),
                    ),
                    Match.exhaustive,
                );
                return yield* undoable('Set Text Direction', Output, () => _apply(table, rows));
            }
            case 'numbering': {
                const { target } = operation;
                if (target._tag === 'paragraphs') {
                    const paragraphs = yield* _range(document, 'paragraphs', target.range);
                    return yield* undoable('Set Paragraph Numbering', Output, () => {
                        const list = Option.map(target.list, ({ name, acrossStories, acrossDocuments }) => {
                            const existing = document.numberingLists.itemByName(name);
                            const created = existing.isValid ? existing : document.numberingLists.add(name);
                            return { target: created, path: created.toSpecifier(), values: { continueNumbersAcrossStories: acrossStories, continueNumbersAcrossDocuments: acrossDocuments } };
                        });
                        const values = {
                            bulletsAndNumberingListType: ListType.NUMBERED_LIST,
                            numberingFormat: target.style,
                            ...Record.getSomes<string, unknown>({
                                numberingLevel: target.level,
                                numberingExpression: target.expression,
                                appliedNumberingList: Option.map(list, Struct.get('target')),
                            }),
                        };
                        return _apply(table, [
                            ...Option.toArray(list),
                            ...Array.map(paragraphs, (paragraph, index) => ({
                                target: paragraph,
                                path: paragraph.toSpecifier(),
                                values: {
                                    ...values,
                                    ...(Option.isSome(target.start) ? { numberingContinue: index > 0, ...(index === 0 ? { numberingStartAt: target.start.value } : {}) } : {}),
                                },
                            })),
                        ]);
                    });
                }
                const row: Write = yield* Match.value(target).pipe(
                    Match.when({ _tag: 'section' }, ({ sectionId, style }) =>
                        Effect.map(
                            Effect.filterOrFail(
                                Effect.sync(() => document.sections.itemByID(sectionId)),
                                Struct.get('isValid'),
                                () => EditorialError.cases.selectionMissing.make({ collection: 'sections', id: sectionId }),
                            ),
                            (section) => ({ target: section, path: section.toSpecifier(), values: { pageNumberStyle: style } }),
                        ),
                    ),
                    Match.when({ _tag: 'chapter' }, ({ style, source, number }) =>
                        Effect.succeed({
                            target: document.chapterNumberPreferences,
                            path: 'chapterNumberPreferences',
                            values: { chapterNumberFormat: style, ...Record.getSomes<string, unknown>({ chapterNumberSource: source, chapterNumber: number }) },
                        }),
                    ),
                    Match.when({ _tag: 'footnotes' }, ({ style }) => Effect.succeed({ target: document.footnoteOptions, path: 'footnoteOptions', values: { footnoteNumberingStyle: style } })),
                    Match.when({ _tag: 'endnotes' }, ({ style }) => Effect.succeed({ target: document.endnoteOptions, path: 'endnoteOptions', values: { endnoteNumberingStyle: style } })),
                    Match.exhaustive,
                );
                return yield* undoable('Set Numbering Style', Output, () => _apply(table, [row]));
            }
            case 'insertSpecialCharacter': {
                const story = yield* _story(document, operation.storyId);
                const count = story.insertionPoints.length;
                if (operation.offset >= count) {
                    return yield* Effect.fail(EditorialError.cases.rangeOutOfBounds.make({ storyId: story.id, collection: 'insertionPoints', from: operation.offset, to: operation.offset, count }));
                }
                const point = story.insertionPoints.item(operation.offset);
                const character = yield* Effect.fromOption(HashMap.get(table.lookup.exact, [operation.character.enumeration, operation.character.constant]), () =>
                    EditorialError.cases.nativeConstantMissing.make({ property: 'contents', constant: operation.character.constant }),
                );
                return yield* undoable('Insert Special Character', Output, (): typeof Output.Type => {
                    point.contents = character.enumerator;
                    return { kind: 'inserted', storyId: story.id, offset: operation.offset, character: character.constant, length: story.characters.length };
                });
            }
            case 'findText':
            case 'findGrep': {
                const scope = yield* Option.match(operation.storyId, { onNone: () => Effect.succeed(document), onSome: (id) => _story(document, id) });
                const preferences =
                    operation._tag === 'findText'
                        ? ({ find: 'findTextPreferences', change: 'changeTextPreferences', options: 'findChangeTextOptions', mode: SearchModes.TEXT_SEARCH } as const)
                        : ({ find: 'findGrepPreferences', change: 'changeGrepPreferences', options: 'findChangeGrepOptions', mode: SearchModes.GREP_SEARCH } as const);
                return yield* undoable('Find and Change', Output, (): typeof Output.Type => {
                    const current = { find: app[preferences.find], change: app[preferences.change], options: app[preferences.options] };
                    const saved = { find: current.find.properties, change: current.change.properties, options: current.options.properties };
                    try {
                        app[preferences.find] = NothingEnum.NOTHING;
                        app[preferences.change] = NothingEnum.NOTHING;
                        Array.forEach(Option.toArray(operation.query), (name) => app.loadFindChangeQuery(name, preferences.mode));
                        const configured = _apply(
                            table,
                            Array.map(Struct.keys(current), (key) => ({
                                target: current[key],
                                path: key,
                                values: Record.fromIterableWith(operation[key], ({ key: field, value }) => [field, value]),
                            })),
                        );
                        return configured.rejected.length > 0
                            ? { kind: 'rejected', error: EditorialError.cases.queryConfiguration.make({ rejected: configured.rejected }) }
                            : { kind: 'changed', count: (operation._tag === 'findText' ? scope.changeText(operation.reverseOrder) : scope.changeGrep(operation.reverseOrder)).length };
                    } finally {
                        app[preferences.find] = NothingEnum.NOTHING;
                        app[preferences.change] = NothingEnum.NOTHING;
                        current.find.properties = saved.find;
                        current.change.properties = saved.change;
                        current.options.properties = saved.options;
                    }
                });
            }
        }
    },
    Effect.catchIf(Schema.is(EditorialError), (error) => Effect.succeed({ kind: 'rejected' as const, error })),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { editDocument };
