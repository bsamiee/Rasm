import type { InDesignDOMObject } from 'adobe:indesign';
import { Unmatched } from '@rasm/search/unmatched';
import { Array, Cache, Cause, Effect, Option, Predicate, Record, Schema, Struct } from 'effect';
import { unavailable } from '#/dom/dictionary';
import { Model } from '#/dom/model';
import { Reference, Specifier } from '#/dom/specifier';
import { Mutation } from '#/dom/transactions';
import { decoder, record, Value, Values } from '#/dom/value';
import { InDesignError } from '#/error';

// --- [MODELS] --------------------------------------------------------------------------

class Change extends Mutation.extend<Change>('Change')({ target: Specifier.Path, values: Values }) {
    static readonly Result = Reference.mapFields(Struct.assign({ written: Schema.Array(Schema.String) }));
    static readonly write: (target: InDesignDOMObject<unknown>, values: Record.ReadonlyRecord<string, unknown>) => Effect.Effect<readonly string[], InDesignError | Unmatched | Schema.SchemaError, Model> = Effect.fnUntraced(function* (target, values) {
        const specifier = target.toSpecifier();
        const { rows, plural, host } = yield* Model;
        const { fields } = yield* Cache.get(rows, target.constructorName);
        return yield* Effect.validate(
            Object.entries(values),
            Effect.fnUntraced(function* ([field, value]: readonly [string, unknown]) {
                const subject = `${specifier} ${field}`;
                const refused = InDesignError.refused(subject);
                const row = yield* Effect.fromOption(Record.get(fields, field), () => Unmatched.make({ subject: `field of ${specifier}`, query: Option.some(field), candidates: Record.keys(Record.filter(fields, Predicate.not(plural))) }));
                const nested = record(value) && !row.settable;
                yield* Effect.transposeOption(Option.map(unavailable(target.constructorName, field, Option.some(target), !nested), (because) => InDesignError.make({ reason: { _tag: 'Unavailable', because }, subject })));
                const inner: unknown = nested ? yield* Effect.try({ try: () => Reflect.get(target, field), catch: refused }) : undefined;
                if (record(value) && host(inner)) {
                    return Array.map(yield* Change.write(inner, value), (name) => `${field}.${name}`);
                }
                const native = yield* decoder(target, Option.some(row))(value);
                yield* Effect.try({ try: () => Object.assign(target, { [field]: native }), catch: refused });
                return [field];
            }, Effect.sandbox),
        ).pipe(
            Effect.map(Array.flatten),
            Effect.catch((causes) => Effect.failCause(Cause.fromReasons(causes.flatMap(Struct.get('reasons'))))),
        );
    });

    override get label(): string {
        return `Change ${Record.keys(this.values).join(', ')} of ${this.target.leaf}`;
    }

    run() {
        const { values } = this;
        return Effect.flatMap(Specifier.object(this.target), (found) =>
            this.commit(
                Effect.gen(function* () {
                    return { ...Reference.of(found), written: yield* Change.write(found, values) };
                }),
            ),
        );
    }
}
class Create extends Mutation.extend<Create>('Create')({
    owner: Schema.OptionFromOptionalKey(Specifier.Path.annotate({ description: 'Collection owner, active document when absent' })),
    collection: Schema.String.annotate({ description: 'Collection field (pages, paragraphStyles, rectangles, nestedGrepStyles, tabStops, swatches)' }),
    arguments: Schema.OptionFromOptionalKey(Schema.Array(Value).annotate({ description: 'Positional add arguments' })),
    properties: Schema.OptionFromOptionalKey(Values.annotate({ description: 'New object fields' })),
}) {
    override get label(): string {
        return `Create ${this.collection} in ${Option.match(this.owner, { onNone: () => 'active document', onSome: Struct.get('leaf') })}`;
    }

    run() {
        const { owner: given, collection: field, arguments: positional, properties } = this;
        return Effect.flatMap(Option.match(given, { onNone: () => Model.active, onSome: Specifier.object }), (found) =>
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
                    const made = yield* Effect.try({ try: (): unknown => Reflect.apply(items.add, items, positioned), catch: InDesignError.refused(`${items.everyItem().toSpecifier()} add`) }).pipe(
                        Effect.flatMap((result) => Option.match(Array.head(Array.ensure<unknown>(result).filter(model.host)), { onNone: () => Effect.die(result), onSome: Effect.succeed })),
                    );
                    yield* Effect.transposeOption(Option.map(properties, (supplied) => Change.write(made, supplied)));
                    return Reference.of(made);
                }),
            ),
        );
    }
}
class Remove extends Mutation.extend<Remove>('Remove')({ target: Specifier.Path }) {
    override get label(): string {
        return `Remove ${this.target.leaf}`;
    }

    run() {
        const { target } = this;
        return Effect.flatMap(Specifier.object(target), (found) =>
            this.commit(
                Effect.gen(function* () {
                    const { rows, owning } = yield* Model;
                    const { methods } = yield* Cache.get(rows, found.constructorName);
                    const removable = yield* Effect.fromOption(Option.liftPredicate(found, owning<{ readonly remove: () => void }>('remove')), () => Unmatched.make({ subject: `method of ${found.constructorName}`, query: Option.some('remove'), candidates: Record.keys(methods) }));
                    const removed = Reference.of(found);
                    yield* Effect.try({ try: () => removable.remove(), catch: InDesignError.refused(target.path) });
                    return Reference.make({ ...removed, valid: found.isValid });
                }),
            ),
        );
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Change, Create, Remove };
