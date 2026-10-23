import { Unmatched } from '@rasm/search';
import { Array, Cache, Cause, Effect, Option, Record, Schema, Struct } from 'effect';
import { decoder } from '#/dom/decoding';
import { active, resolveObject } from '#/dom/location';
import { Model } from '#/dom/model';
import { leaf, Reference, Specifier } from '#/dom/reference';
import { Mutation } from '#/dom/transactions';
import { Change } from '#/requests/change';

// --- [MODELS] --------------------------------------------------------------------------

class Create extends Mutation.extend<Create>('Create')(
    {
        owner: Schema.OptionFromOptionalKey(Specifier.annotate({ description: 'Collection owner, active document when absent' })),
        collection: Schema.String.annotate({ description: 'Collection field (pages, paragraphStyles, rectangles, nestedGrepStyles, tabStops, swatches)' }),
        arguments: Schema.OptionFromOptionalKey(Schema.Array(Schema.Json).annotate({ description: 'Positional add arguments as change values, kind lists add signatures' })),
        properties: Schema.OptionFromOptionalKey(Schema.JsonObject.annotate({ description: 'New object fields as change values' })),
    },
    { description: 'Adds collection member in one undo step' },
) {
    override get label(): string {
        return `Create ${this.collection} in ${Option.match(this.owner, { onNone: () => 'active document', onSome: leaf })}`;
    }

    run() {
        const { owner: given, collection: field, arguments: positional, properties } = this;
        return Effect.flatMap(Option.match(given, { onNone: () => active, onSome: resolveObject }), (found) =>
            this.commit(
                Effect.gen(function* () {
                    const model = yield* Model;
                    const { fields } = yield* Cache.get(model.rows, found.constructorName);
                    const items = yield* Effect.fromOption(Option.liftPredicate(Reflect.get(found, field), model.collection), () => Unmatched.make({ subject: `collection of ${found.toSpecifier()}`, query: Option.some(field), candidates: Record.keys(Record.filter(fields, model.plural)) }));
                    const { methods } = yield* Cache.get(model.rows, items.constructor.name);
                    const add = yield* Effect.fromOption(Record.get(methods, 'add'), () => Unmatched.make({ subject: `method of ${items.constructor.name}`, query: Option.some('add'), candidates: Record.keys(methods) }));
                    const ordered = Option.getOrElse(positional, Array.empty);
                    yield* ordered.length > add.parameters.length ? new Cause.IllegalArgumentError(`${items.constructor.name} add takes ${add.parameters.length} parameters (${Array.map(add.parameters, Struct.get('name')).join(', ')}), given ${ordered.length}`) : Effect.void;
                    const positioned = yield* Effect.forEach(Array.zip(ordered, add.parameters), ([value, parameter]) => decoder(found, Option.some(parameter))(value));
                    const made = yield* Effect.try({ try: (): unknown => Reflect.apply(items.add, items, positioned), catch: (cause) => new Cause.UnknownError(cause, `${items.everyItem().toSpecifier()} add`) }).pipe(
                        Effect.flatMap((result) => Option.match(Array.head(Array.ensure<unknown>(result).filter(model.host)), { onNone: () => Effect.die(result), onSome: Effect.succeed })),
                    );
                    yield* Effect.transposeOption(Option.map(properties, (supplied) => Change.write(made, supplied)));
                    return Reference.of(made);
                }),
            ),
        );
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Create };
