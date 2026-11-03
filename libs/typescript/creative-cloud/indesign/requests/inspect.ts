import type { Application, InDesignDOMObject } from 'adobe:indesign';
import { HostError, ObjectModel } from '@rasm/automation/requests';
import { Unmatched } from '@rasm/search/unmatched';
import { Uxp } from '@rasm/uxp/runtime';
import { Array, Cache, Effect, Match, Option, pipe, Record, Result, Schema, Struct } from 'effect';
import { ApplicationView, Inspect } from '#/contract';
import { documentView, layerView, linkView, overset, pageView, spreadView, summary, swatchView } from '#/documents/document';
import { overrides, storyView, styleView, textView } from '#/documents/text';
import { Dictionary } from '#/dom/dictionary';
import { Model } from '#/dom/model';
import type { Slice } from '#/dom/slice';
import { fontView } from '#/requests/font';
import { preflightView } from '#/requests/preflight';

// --- [MODELS] --------------------------------------------------------------------------

const Contents = Schema.Struct({ contents: Schema.Json });

// --- [OPERATIONS] ----------------------------------------------------------------------

const applicationView = Effect.fnUntraced(function* (app: Application) {
    const { overridable, storied } = yield* Model;
    const story = (item: unknown) => Option.map(Option.liftPredicate(item, storied), ({ parentStory }) => parentStory.toSpecifier());
    const summaries = yield* Effect.forEach(app.documents.everyItem().getElements(), summary);
    const opened = yield* Effect.option(Model.current);
    const selection = yield* Effect.forEach(Option.match(opened, { onNone: Array.empty, onSome: () => app.selection }), (item) =>
        Effect.all({ own: Model.reference(item), overrides: Effect.transposeOption(Option.map(Option.liftPredicate(item, overridable), overrides)) }).pipe(
            Effect.map(({ own, overrides: overridden }) => ({ ...own, contents: Option.map(Schema.decodeUnknownOption(Contents)(item), Struct.get('contents')), parentStory: story(item), overrides: overridden })),
        ),
    );
    const window = Option.liftThrowable(() => app.activeWindow.toSpecifier())();
    return yield* Model.read(ApplicationView, app.toSpecifier(), () => ({
        _tag: 'Application',
        ...Model.referenceUnsafe(app),
        version: app.version,
        tool: app.toolBoxTools.currentTool,
        panels: Array.getSomes(Array.map(Array.range(0, app.panels.length - 1), (index) => Option.map(Option.filter(Option.fromUndefinedOr(app.panels.item(index)), Struct.get('visible')), Struct.get('name')))),
        documents: summaries,
        current: Option.map(opened, (document) => ({
            document: document.toSpecifier(),
            window,
            selection,
            overset: overset(document),
            undo: { mode: app.activeScriptUndoMode, name: document.undoName, history: document.undoHistory },
        })),
    }));
});
const accessors = (found: InDesignDOMObject<unknown>) =>
    pipe(
        Array.flatMap(Uxp.members(Object.getPrototypeOf(found)), ({ fields }) => fields),
        Array.dedupeWith((left, right) => left.name === right.name),
    ).filter(({ name }) => name !== Dictionary.aggregate);
const values = Effect.fn('values')(function* (found: InDesignDOMObject<unknown>, wanted: readonly string[]) {
    const { declarations } = yield* Model;
    const { fields } = yield* Cache.get(declarations, found.constructorName);
    const readable = accessors(found);
    const [named, missing] = Array.partition(wanted, (each) =>
        Result.fromOption(
            Array.findFirst(readable, ({ name }) => Unmatched.named(name, each)),
            () => each,
        ),
    );
    yield* Array.isReadonlyArrayEmpty(missing) ? Effect.void : Unmatched.make({ subject: `field of ${found.toSpecifier()}`, query: Option.some(missing.join(', ')), candidates: Array.map(readable, Struct.get('name')) });
    return yield* Effect.forEach(
        named,
        Effect.fnUntraced(function* (accessor) {
            const subject = `${found.toSpecifier()} ${accessor.name}`;
            const row = yield* Option.match(Record.get(fields, accessor.name), { onNone: () => Dictionary.undeclared(ObjectModel.Field, accessor), onSome: Effect.succeed });
            const refusal = Effect.transposeOption(Option.map(Dictionary.unavailable(found.constructorName, accessor.name, Option.some(found), false), (because) => HostError.refused(`${subject}: ${because}`)()));
            const held = Option.contains(row.type, 'File') ? Model.file(found, accessor.name) : Effect.map(Effect.try({ try: (): unknown => Reflect.get(found, accessor.name), catch: HostError.refused(subject) }), Option.fromNullishOr);
            const value = yield* Effect.result(Effect.andThen(refusal, Effect.map(held, Option.getOrNull)));
            return { ...row, value };
        }),
    );
});
const object = Effect.fn('object')(function* (found: InDesignDOMObject<unknown>, fields: Option.Option<readonly string[]>, slice: Slice) {
    const model = yield* Model;
    const { fields: own } = yield* Cache.get(model.declarations, found.constructorName);
    const view = yield* Effect.transposeOption(
        Match.value<unknown>(found).pipe(
            Match.when(model.application, applicationView),
            Match.when(model.document, documentView),
            Match.when(model.page, pageView),
            Match.whenOr(model.spread, model.masterSpread, spreadView),
            Match.when(model.layer, layerView),
            Match.when(model.style, styleView),
            Match.when(model.swatch, swatchView),
            Match.when(model.link, linkView),
            Match.when(model.story, storyView),
            Match.whenOr(model.text, model.frame, (text) => textView(text, slice)),
            Match.when(model.font, fontView),
            Match.when(model.preflightProcess, preflightView),
            Match.option,
        ),
    );
    const entries = yield* values(
        found,
        Option.getOrElse(fields, () => (Option.isSome(view) ? Record.keys(Record.filter(own, model.collectionField)) : Array.map(accessors(found), Struct.get('name')))),
    );
    const [collections, scalars] = Array.partition(entries, (entry) =>
        pipe(
            Result.getSuccess(entry.value),
            Option.filter(model.collection),
            Option.map((items) => ({ name: entry.name, type: items.constructor.name, count: items.length, specifier: items.everyItem().toSpecifier() })),
            Result.fromOption(() => entry),
        ),
    );
    const reference = yield* Model.reference(found);
    return Inspect.Object.make({ ...reference, view, fields: yield* Model.read(Inspect.Readings, found.toSpecifier(), () => scalars), collections });
});
const inspect = Effect.fn('inspect')(function* ({ target, fields, slice }: Inspect) {
    const [found, { domObject }, path] = yield* Effect.all([Model.resolve(target), Model, target.path]);
    if (domObject(found)) {
        return yield* object(found, fields, slice);
    }
    const items = yield* Effect.forEach(slice.of(found), (item) => Effect.zipWith(Model.reference(item), values(item, Option.getOrElse(fields, Array.empty)), (own, entries) => ({ ...own, fields: entries })));
    return yield* Model.read(Inspect.Items, path, () => ({ _tag: 'Items', specifier: path, total: found.length, items }));
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { inspect };
