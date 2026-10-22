import type { InDesignDOMObject } from 'adobe:indesign';
import { Unmatched } from '@rasm/search';
import type { Uxp } from '@rasm/uxp';
import { Array, Cache, Cause, Effect, flow, Option, pipe, Record, Result, Schema, Struct } from 'effect';
import { ApplicationView } from '#/application-view';
import { DocumentView } from '#/documents/document-view';
import { LayerView } from '#/documents/layer-view';
import { PageView } from '#/documents/page-view';
import { SpreadView } from '#/documents/spread-view';
import { bare, Field, unavailable } from '#/dom/dictionary';
import { read } from '#/dom/encoding';
import { file } from '#/dom/files';
import { resolve } from '#/dom/location';
import { Model } from '#/dom/model';
import { Reference, Specifier } from '#/dom/reference';
import { FontView } from '#/font-view';
import { LinkView } from '#/link-view';
import { PreflightProcessView } from '#/requests/preflight';
import { StyleView } from '#/styles/style-view';
import { SwatchView } from '#/swatch-view';
import { StoryView } from '#/text/story-view';
import { TextView } from '#/text/text-view';

// --- [MODELS] --------------------------------------------------------------------------

const View = Schema.Union([DocumentView, PageView, SpreadView, LayerView, StyleView, SwatchView, LinkView, StoryView, TextView, FontView, PreflightProcessView]);
const Row = Field.mapFields(Struct.omit(['unavailable']));
const Read = Row.pipe(Schema.fieldsAssign({ value: Schema.Result(Schema.OptionFromNullOr(Schema.Json), Schema.Defect()) }));
class Inspect extends Schema.Class<Inspect, { readonly brand: unique symbol }>('Inspect')(
    {
        target: Schema.OptionFromOptionalKey(Specifier.annotate({ description: 'Specifier of an object or collection; the application when absent' })),
        fields: Schema.OptionFromOptionalKey(Schema.NonEmptyArray(Schema.String).annotate({ description: 'Native fields to read; an object lists its collections and a collection its members unless fields are requested. Object values return specifiers to inspect further' })),
    },
    {
        description:
            "Reads InDesign's native object model without changing it. No target: the application, its documents, windows, selection, overset stories, and undo history. A specifier: a typed view of documents, pages, spreads, parents, layers, links, swatches, styles, stories, frames, text, fonts, and preflight processes, beside its native fields on request; other objects list fields, values, valid values, writable fields, and related collections. A collection: its members. Kind reads class and enumeration declarations",
    },
) {
    static readonly Object = Schema.TaggedStruct('Object', {
        ...Reference.fields,
        view: Schema.OptionFromOptionalKey(View),
        fields: Schema.Array(Read),
        collections: Schema.Array(Schema.Struct({ name: Schema.String, type: Schema.String, count: Schema.Int, specifier: Specifier })),
    });
    static readonly Items = Schema.TaggedStruct('Items', { specifier: Specifier, total: Schema.Int, items: Schema.Array(Reference.mapFields(Struct.assign({ fields: Schema.Array(Read) }))) });
    static readonly Result = Schema.Union([ApplicationView, Inspect.Object, Inspect.Items]);

    run() {
        const { target, fields } = this;
        return Option.match(target, {
            onNone: () => ApplicationView.current,
            onSome: Effect.fnUntraced(function* (specifier) {
                const [found, model] = yield* Effect.all([resolve(specifier), Model]);
                return yield* model.host(found)
                    ? object(found, fields)
                    : Effect.flatMap(
                          Effect.forEach(found, (item) => Effect.map(values(item, Option.getOrElse(fields, Array.empty)), (entries) => ({ ...Reference.of(item), fields: entries }))),
                          (items) => read(Inspect.Items, specifier, () => ({ _tag: 'Items', specifier, total: found.length, items })),
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
            const refused = (cause: unknown) => new Cause.UnknownError(cause, `${found.toSpecifier()} ${name}`);
            const row = yield* Option.match(Record.get(fields, name), { onNone: () => bare(name), onSome: Effect.succeed });
            const refusal = Effect.transposeOption(Option.map(unavailable(found.constructorName, name, Option.some(found), false), flow(refused, Effect.fail)));
            const held = Option.contains(row.type, 'File') ? file(found, name) : Effect.map(Effect.try({ try: (): unknown => Reflect.get(found, name), catch: refused }), Option.fromUndefinedOr);
            const value = yield* Effect.result(Effect.andThen(refusal, held));
            return { ...Struct.omit(row, ['unavailable']), value };
        }),
    );
});
const object = Effect.fn('object')(function* (found: InDesignDOMObject<unknown>, fields: Option.Option<readonly string[]>) {
    const model = yield* Model;
    const { fields: own } = yield* Cache.get(model.rows, found.constructorName);
    const view = yield* Effect.transposeOption(Option.firstSomeOf(Array.map(View.members, (kind): Option.Option<Effect.Effect<typeof View.Type, Cause.UnknownError | Unmatched, Model | Uxp>> => kind.viewed(model)(found))));
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
