import { Catalog } from '@rasm/database/catalog';
import { Relation } from '@rasm/database/relation';
import { Array, Effect, HashMap, Option, Record, Schema, SchemaGetter, SchemaIssue, Struct } from 'effect';
import { Description } from '#/description/description';
import { Act } from '#/server/act';
import { Element } from '#/server/element';

// --- [MODELS] --------------------------------------------------------------------------

const Tally = Schema.Struct({
    seen: Schema.Natural,
    missed: Schema.Natural,
    first: Schema.DateTimeUtcFromString,
    last: Schema.DateTimeUtcFromString,
    epoch: Schema.DateTimeUtcFromString,
    since: Schema.OptionFromOptionalKey(Schema.String),
    verified: Schema.OptionFromOptionalKey(Schema.String),
    persistent: Schema.Boolean,
    unexplained: Schema.Array(Schema.DateTimeUtcFromString),
});
const entry = <Atoms extends Schema.Top>(atoms: Atoms) =>
    Schema.Struct({
        container: Schema.Struct({
            ...Struct.pick(Element.Observation.fields, ['kind', 'title', 'modal', 'layer', 'size', 'default', 'cancel', 'close']),
            openers: Schema.Array(Schema.Struct({ ...Element.Opener.fields, tally: Tally })),
            tally: Tally,
            scopes: Schema.Array(Schema.Struct({ at: Schema.DateTimeUtcFromString, atoms })),
        }),
        controls: Schema.Record(
            Element.Locator,
            Schema.Struct({
                ...Struct.omit(Element.Shape.fields, ['options']),
                options: Schema.OptionFromOptionalKey(Schema.Union([Schema.Array(Schema.String), Schema.Struct({ count: Schema.Natural })])),
                frame: Schema.OptionFromOptionalKey(Element.Frame),
                value: Schema.OptionFromOptionalKey(Element.Value),
                presence: Schema.OptionFromOptionalKey(Schema.Struct({ with: atoms, against: atoms })),
                tally: Tally,
                retired: Schema.OptionFromOptionalKey(Schema.String),
                route: Schema.OptionFromOptionalKey(Schema.Struct({ build: Schema.OptionFromOptionalKey(Schema.String), passed: Schema.OptionFromOptionalKey(Act.Route.Kind), failed: Schema.Array(Act.Route.Kind) })),
            }),
        ),
    });
const Entry = Element.Condition.pipe(Schema.Array, entry);
const Interned = Schema.Struct({ conditions: Schema.Array(Element.Condition), ...Schema.Natural.pipe(Schema.Array, entry).fields }).pipe(
    Schema.decodeTo(Schema.toType(Entry), {
        decode: SchemaGetter.transformEffect((interned, options) => {
            const { conditions, container, controls } = interned;
            const atoms = (positions: readonly number[]) => Effect.forEach(positions, (at) => Effect.fromOption(Array.get(conditions, at)));
            return Effect.all({
                container: Effect.map(
                    Effect.forEach(container.scopes, (scope) => Effect.map(atoms(scope.atoms), (held) => ({ ...scope, atoms: held }))),
                    (scopes) => ({ ...container, scopes }),
                ),
                controls: Effect.all(
                    Record.map(controls, (control) =>
                        control.presence.pipe(
                            Option.map(({ with: seen, against }) => Effect.all({ with: atoms(seen), against: atoms(against) })),
                            Effect.transposeOption,
                            Effect.map((presence) => ({ ...control, presence })),
                        ),
                    ),
                ),
            }).pipe(Effect.mapError(() => new SchemaIssue.InvalidValue({ message: `Expected condition positions below ${conditions.length}` }, interned, options)));
        }),
        encode: SchemaGetter.transform(({ container, controls }) => {
            const conditions = Array.dedupe([...container.scopes.flatMap(Struct.get('atoms')), ...Record.values(controls).flatMap(({ presence }) => Option.match(presence, { onNone: Array.empty, onSome: ({ with: seen, against }) => [...seen, ...against] }))]);
            const positions = HashMap.fromIterable(conditions.map((atom, at) => [atom, at] as const));
            const place = Array.flatMap((atom: typeof Element.Condition.Type) => Option.toArray(HashMap.get(positions, atom)));
            return {
                conditions,
                container: { ...container, scopes: container.scopes.map((scope) => ({ ...scope, atoms: place(scope.atoms) })) },
                controls: Record.map(controls, (control) => ({ ...control, presence: Option.map(control.presence, Struct.evolve({ with: place, against: place })) })),
            };
        }),
    }),
);

// --- [COMPOSITION] ---------------------------------------------------------------------

const applications = Relation.make(
    'applications',
    {
        bundle: Schema.String,
        language: Schema.String,
        body: Schema.fromJsonString(
            Schema.Struct({
                builds: Schema.Array(Schema.String),
                options: Schema.Record(Schema.String, Schema.Array(Schema.Tuple([Description.Locator, Description.fields.containers.value.fields.controls.value.fields.options.schema.value.mapFields(Struct.omit(['locator']))]))).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
                cursors: Schema.Record(Schema.String, Schema.Record(Schema.String, Schema.Record(Schema.String, Schema.Natural))).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
            }),
        ),
    },
    { key: ['bundle', 'language'], documents: { body: [] } },
);
const containers = Relation.make('containers', { ...Struct.pick(applications.schema.fields, ['bundle', 'language']), locator: Element.Locator, body: Schema.fromJsonString(Interned) }, { key: ['bundle', 'language', 'locator'], documents: { body: [Schema.fromJsonString(Entry)] } });
const catalog = Catalog.make({ tables: [applications, containers] });

// --- [EXPORTS] -------------------------------------------------------------------------

export { applications, catalog, containers };
