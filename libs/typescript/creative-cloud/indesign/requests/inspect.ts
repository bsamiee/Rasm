import type { InDesignDOMObject, Text } from 'adobe:indesign';
import { Unmatched } from '@rasm/search/unmatched';
import type { Uxp } from '@rasm/uxp/runtime';
import { Array, Cache, Effect, Option, pipe, Record, Result, Schema, Struct } from 'effect';
import { DocumentView, LayerView, LinkView, PageView, SpreadView, SwatchView } from '#/documents/document';
import { StoryView, StyleView, TextView } from '#/documents/text';
import { Field, unavailable, undeclared } from '#/dom/dictionary';
import { Model } from '#/dom/model';
import { Reference, Specifier } from '#/dom/specifier';
import { read } from '#/dom/value';
import { InDesignError } from '#/error';
import { FontView } from '#/requests/font';
import { PreflightProcessView } from '#/requests/preflight';

// --- [MODELS] --------------------------------------------------------------------------

class ApplicationView extends Schema.TaggedClass<ApplicationView, { readonly brand: unique symbol }>()('Application', {
    version: Schema.String,
    tool: Schema.String,
    panels: Schema.Array(Schema.String),
    documents: Schema.Array(DocumentView.Row),
    active: Schema.OptionFromOptionalKey(
        Schema.Struct({
            document: Schema.String,
            window: Schema.String,
            selection: Schema.Array(Reference.mapFields(Struct.assign({ contents: Schema.OptionFromOptionalKey(Schema.Json), parentStory: Schema.OptionFromOptionalKey(Schema.String), overrides: Schema.OptionFromOptionalKey(TextView.fields.overrides) }))),
            overset: Schema.Array(Schema.String),
            undo: Schema.Struct({ mode: Schema.String, name: Schema.String, history: Schema.Array(Schema.String) }),
        }),
    ),
}) {
    static readonly current = Effect.gen(function* () {
        const { app, owning } = yield* Model;
        const overridable = owning<Text>('textHasOverrides');
        const rows = yield* Effect.forEach(app.documents.everyItem().getElements(), DocumentView.row);
        const opened = yield* Effect.option(Model.active);
        const selection = yield* Effect.forEach(Option.match(opened, { onNone: Array.empty, onSome: () => app.selection }), (item) =>
            pipe(
                Option.liftPredicate(item, overridable),
                Option.map(TextView.overrides),
                Effect.transposeOption,
                Effect.map((overrides) => ({
                    ...Reference.of(item),
                    contents: Option.fromUndefinedOr('contents' in item ? item.contents : undefined),
                    parentStory: 'parentStory' in item ? Option.some(item.parentStory.toSpecifier()) : Option.none(),
                    overrides,
                })),
            ),
        );
        const panels = Array.getSomes(
            Array.makeBy(app.panels.length, (index) =>
                Option.map(
                    Option.filter(Option.fromUndefinedOr(app.panels.item(index)), ({ visible }) => visible),
                    Struct.get('name'),
                ),
            ),
        );
        return yield* read(ApplicationView, 'application', () => ({
            _tag: 'Application',
            version: app.version,
            tool: app.toolBoxTools.currentTool,
            panels,
            documents: rows,
            active: Option.map(opened, (document) => ({
                document: document.toSpecifier(),
                window: app.activeWindow.toSpecifier(),
                selection,
                overset: DocumentView.overset(document),
                undo: { mode: app.activeScriptUndoMode, name: document.undoName, history: document.undoHistory },
            })),
        }));
    });
}
const View = Schema.Union([DocumentView, PageView, SpreadView, LayerView, StyleView, SwatchView, LinkView, StoryView, TextView, FontView, PreflightProcessView]);
const Read = Field.mapFields(Struct.omit(['unavailable'])).pipe(Schema.fieldsAssign({ value: Schema.Result(Schema.OptionFromNullOr(Schema.Json), Schema.Defect()) }));
class Inspect extends Schema.Class<Inspect, { readonly brand: unique symbol }>('Inspect')({
    target: Schema.OptionFromOptionalKey(Specifier.Path.annotate({ description: 'Object or collection specifier, application when absent' })),
    fields: Schema.OptionFromOptionalKey(Schema.NonEmptyArray(Schema.String).annotate({ description: 'Fields to read, collections of viewed objects or every field when absent' })),
}) {
    static readonly Object = Schema.TaggedStruct('Object', {
        ...Reference.fields,
        view: Schema.OptionFromOptionalKey(View),
        fields: Schema.Array(Read),
        collections: Schema.Array(Schema.Struct({ name: Schema.String, type: Schema.String, count: Schema.Int, specifier: Schema.String })),
    });
    static readonly Items = Schema.TaggedStruct('Items', { specifier: Schema.String, total: Schema.Int, items: Schema.Array(Reference.mapFields(Struct.assign({ fields: Schema.Array(Read) }))) });
    static readonly Result = Schema.Union([ApplicationView, Inspect.Object, Inspect.Items]);

    run() {
        const { target, fields } = this;
        return Option.match(target, {
            onNone: () => ApplicationView.current,
            onSome: Effect.fnUntraced(function* (specifier) {
                const [found, { host }] = yield* Effect.all([Specifier.resolve(specifier), Model]);
                return yield* host(found)
                    ? object(found, fields)
                    : Effect.flatMap(
                          Effect.forEach(found, (item) => Effect.map(values(item, Option.getOrElse(fields, Array.empty)), (entries) => ({ ...Reference.of(item), fields: entries }))),
                          (items) => read(Inspect.Items, specifier.path, () => ({ _tag: 'Items', specifier: specifier.path, total: found.length, items })),
                      );
            }),
        });
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const getters = (found: InDesignDOMObject<unknown>) => Array.flatMap(Object.entries(Object.getOwnPropertyDescriptors(Object.getPrototypeOf(found))), ([name, { get }]) => (get === undefined || name === 'properties' ? [] : [name]));
const values = Effect.fn('values')(function* (found: InDesignDOMObject<unknown>, wanted: readonly string[]) {
    const { rows } = yield* Model;
    const { fields } = yield* Cache.get(rows, found.constructorName);
    const prototype = Object.getPrototypeOf(found);
    return yield* Effect.forEach(
        wanted,
        Effect.fnUntraced(function* (name) {
            yield* Object.getOwnPropertyDescriptor(prototype, name)?.get === undefined ? Effect.fail(Unmatched.make({ subject: `field of ${found.toSpecifier()}`, query: Option.some(name), candidates: getters(found) })) : Effect.void;
            const subject = `${found.toSpecifier()} ${name}`;
            const row = yield* Option.match(Record.get(fields, name), { onNone: () => undeclared(Field, name), onSome: Effect.succeed });
            const refusal = Effect.transposeOption(Option.map(unavailable(found.constructorName, name, Option.some(found), false), (because) => InDesignError.make({ reason: { _tag: 'Unavailable', because }, subject })));
            const held = Option.contains(row.type, 'File') ? Model.file(found, name) : Effect.map(Effect.try({ try: (): unknown => Reflect.get(found, name), catch: InDesignError.refused(subject) }), Option.fromUndefinedOr);
            const value = yield* Effect.result(Effect.andThen(refusal, held));
            return { ...Struct.omit(row, ['unavailable']), value };
        }),
    );
});
const object = Effect.fn('object')(function* (found: InDesignDOMObject<unknown>, fields: Option.Option<readonly string[]>) {
    const model = yield* Model;
    const { fields: own } = yield* Cache.get(model.rows, found.constructorName);
    const view = yield* Effect.transposeOption(Option.firstSomeOf(Array.map(View.members, (kind): Option.Option<Effect.Effect<typeof View.Type, InDesignError | Unmatched | Schema.SchemaError, Model | Uxp>> => kind.viewed(model)(found))));
    const entries = yield* values(
        found,
        Option.getOrElse(fields, () => (Option.isSome(view) ? Record.keys(Record.filter(own, model.plural)) : getters(found))),
    );
    const [collections, scalars] = Array.partition(entries, (entry) =>
        pipe(
            Result.getSuccess(entry.value),
            Option.flatten,
            Option.filter(model.collection),
            Option.map((items) => ({ name: entry.name, type: items.constructor.name, count: items.length, specifier: items.everyItem().toSpecifier() })),
            Result.fromOption(() => entry),
        ),
    );
    return Inspect.Object.make({ ...Reference.of(found), view, fields: yield* read(Schema.Array(Read), found.toSpecifier(), () => scalars), collections });
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Inspect };
