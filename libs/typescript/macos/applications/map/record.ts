import { Catalog } from '@rasm/database/catalog';
import { Relation } from '@rasm/database/relation';
import { Array, Effect, HashMap, Option, Record, Schema, SchemaGetter, SchemaIssue, Struct } from 'effect';
import { Description } from '#/description/description';
import { Act } from '#/server/act';
import { Element } from '#/server/element';

// --- [MODELS] --------------------------------------------------------------------------

const Build = Schema.String;
const Instant = Schema.DateTimeUtcFromString;
const Positions = Schema.Array(Schema.Natural);
class Tally extends Schema.Opaque<Tally>()(
    Schema.Struct({
        seen: Schema.Natural,
        missed: Schema.Natural,
        first: Instant,
        last: Instant,
        epoch: Instant,
        since: Schema.OptionFromOptionalKey(Build),
        verified: Schema.OptionFromOptionalKey(Build),
        persistent: Schema.Boolean,
        unexplained: Schema.Array(Instant),
    }),
) {}
const Presence = Schema.Struct({ with: Schema.Array(Element.Condition), against: Schema.Array(Element.Condition) });
class Application extends Schema.Opaque<Application>()(
    Schema.Struct({
        builds: Schema.Array(Build),
        options: Schema.Record(Build, Schema.Array(Schema.Tuple([Description.Locator, Description.Control.fields.options.schema.value.mapFields(Struct.omit(['locator']))]))).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
        cursors: Schema.Record(Build, Schema.Record(Schema.String, Schema.Record(Schema.String, Schema.Natural))).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
    }),
) {}
class Container extends Schema.Opaque<Container>()(
    Schema.Struct({
        ...Struct.pick(Element.Observation.fields, ['kind', 'title', 'modal', 'layer', 'size', 'default', 'cancel', 'close']),
        openers: Schema.Array(Schema.Struct({ ...Element.Opener.fields, tally: Tally })),
        tally: Tally,
        scopes: Schema.Array(Schema.Struct({ at: Instant, atoms: Schema.Array(Element.Condition) })),
    }),
) {}
const Control = Schema.Struct({
    ...Struct.omit(Element.Shape.fields, ['options']),
    options: Schema.OptionFromOptionalKey(Schema.Union([Schema.Array(Schema.String), Schema.Struct({ count: Schema.Natural })])),
    frame: Schema.OptionFromOptionalKey(Element.Frame),
    value: Schema.OptionFromOptionalKey(Element.Value),
    presence: Schema.OptionFromOptionalKey(Presence),
    tally: Tally,
    retired: Schema.OptionFromOptionalKey(Build),
    route: Schema.OptionFromOptionalKey(Schema.Struct({ build: Schema.OptionFromOptionalKey(Build), passed: Schema.OptionFromOptionalKey(Act.Rung), failed: Schema.Array(Act.Rung) })),
});
class Entry extends Schema.Opaque<Entry>()(Schema.Struct({ container: Container, controls: Schema.Record(Element.Locator, Control) })) {}
const Interned = Schema.Struct({
    conditions: Schema.Array(Element.Condition),
    container: Container.mapFields(Struct.assign({ scopes: Schema.Array(Container.fields.scopes.value.mapFields(Struct.assign({ atoms: Positions }))) })),
    controls: Schema.Record(Element.Locator, Control.mapFields(Struct.assign({ presence: Schema.OptionFromOptionalKey(Presence.mapFields(Struct.assign({ with: Positions, against: Positions }))) }))),
}).pipe(
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
class Key extends Schema.Opaque<Key>()(Schema.Struct({ bundle: Schema.String, language: Schema.String })) {}

// --- [COMPOSITION] ---------------------------------------------------------------------

const applications = Relation.make('applications', { ...Key.fields, body: Schema.fromJsonString(Application) }, { key: ['bundle', 'language'], documents: { body: [] } });
const containers = Relation.make('containers', { ...Key.fields, locator: Element.Locator, body: Schema.fromJsonString(Interned) }, { key: ['bundle', 'language', 'locator'], documents: { body: [Schema.fromJsonString(Entry)] } });
const catalog = Catalog.make({ tables: [applications, containers] });

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Application, applications, type Container, catalog, containers, type Entry, type Key, type Tally };
