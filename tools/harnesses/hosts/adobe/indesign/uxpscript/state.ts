import indesign, { app } from 'adobe:indesign';
import { Array, Equal, Function, Option, Predicate, Record, Result, Schema } from 'effect';
import { type Codec, enumerator, field, invoked, message, names, Strings } from '#/values';

// --- [CONSTANTS] -----------------------------------------------------------------------

const LIVE = ['activeDocument', 'activeWindow', 'layoutWindows', 'windows', 'selection'] as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const items = (value: unknown): readonly unknown[] => (Array.isArray(value) ? value : []);
const members = (enumeration: unknown) => Object.getOwnPropertyNames(Object.getPrototypeOf(enumeration)).filter((key) => enumerator(field(enumeration, key)) && String(field(enumeration, key)) === key);
const owned = (value: unknown) => Predicate.isObject(value) && Predicate.isFunction(field(value, 'everyItem'));
const keyOf = (list: unknown): Option.Option<string> =>
    Option.fromUndefinedOr(
        ['id', 'name'].find((key) => {
            try {
                return Array.isArray(field(invoked(list, 'everyItem', []), key));
            } catch {
                return false;
            }
        }),
    );
const preferences = (target: unknown) => Object.keys(Object.getOwnPropertyDescriptors(Object.getPrototypeOf(target))).filter((name) => name !== 'parent' && name !== 'properties' && !Array.contains<string>(LIVE, name));
const activeOf = () => Result.getSuccess(Result.try(() => app.activeDocument));
const pinned = <A>(work: () => A): A => {
    const unit = app.scriptPreferences.measurementUnit;
    app.scriptPreferences.measurementUnit = indesign.MeasurementUnits.POINTS;
    try {
        return work();
    } finally {
        app.scriptPreferences.measurementUnit = unit;
    }
};
const survey = () => {
    const settings = app.colorSettings;
    return {
        enumerations: Record.fromIterableWith(
            names.filter((name) => Reflect.get(indesign, name) instanceof indesign.Enumeration),
            (name) => [name, members(Reflect.get(indesign, name))],
        ),
        cmyk: settings.workingSpaceCMYKList,
        rgb: settings.workingSpaceRGBList,
        languages: Result.getOrThrow(Schema.decodeResult(Strings)(app.languagesWithVendors.everyItem().name)),
        host: { locale: String(app.locale), featureSet: String(app.featureSet), dom: String(app.scriptPreferences.version) },
    };
};
const scanned = () => {
    const preferred: string[] = [];
    const collections: { readonly name: string; readonly key: string }[] = [];
    const unread: string[] = [];
    const found = { preferences: preferred, collections, unread };
    preferences(app)
        .filter((name) => name !== 'scriptPreferences')
        .forEach((name) => {
            try {
                const value = field(app, name);
                if (owned(value)) {
                    const every = invoked(value, 'everyItem', []);
                    const closes = Predicate.isFunction(field(every, 'close')) || !Predicate.isFunction(field(every, 'remove'));
                    if (!closes) {
                        Option.match(keyOf(value), { onNone: () => found.unread.push(`${name}: items hold neither id nor name`), onSome: (key) => found.collections.push({ name, key }) });
                    }
                } else if (Predicate.isObject(value) && !Array.isArray(value) && !('id' in value) && Predicate.isObject(field(value, 'properties'))) {
                    found.preferences.push(name);
                }
            } catch (error) {
                found.unread.push(`${name}: ${message(error)}`);
            }
        });
    return { ...found, preferences: [...found.preferences, 'scriptPreferences'] };
};
const bound = (value: Schema.Json) => Predicate.isObject(value) && !Array.isArray(value) && String(field(value, '$')).startsWith('object:/document');
const snapshot = (found: ReturnType<typeof scanned>, { encoded }: Codec) => ({
    preferences: found.preferences.map((name) => {
        const target = field(app, name);
        const saved = Object.entries(new Object(field(target, 'properties'))).filter(([key, value]) => key !== 'measurementUnit' && !bound(encoded(value)));
        return { name, target, saved, encoded: new Map(saved.map(([key, value]) => [key, encoded(value)])) };
    }),
    collections: found.collections.map(({ name, key }) => ({ name, key, list: field(app, name), known: new Set(items(field(invoked(field(app, name), 'everyItem', []), key))) })),
});
const differences = (held: ReturnType<typeof snapshot>, { encoded }: Codec) => [
    ...held.preferences.flatMap(({ name, target, saved, encoded: was }) => {
        const current = field(target, 'properties');
        return saved
            .filter(([key]) => !(bound(encoded(field(current, key))) || Equal.equals(encoded(field(current, key)), was.get(key))))
            .map(([key, before]) => ({ member: `${name}.${key}`, before, after: field(current, key), undo: name === 'scriptPreferences' ? Reflect.set.bind(Reflect, new Object(target), key, before) : Function.constVoid }));
    }),
    ...held.collections.flatMap(({ name, key, list, known }) =>
        items(field(invoked(list, 'everyItem', []), key))
            .filter((id) => !known.has(id))
            .map((id) => {
                const item = invoked(list, key === 'id' ? 'itemByID' : 'itemByName', [id]);
                return { member: name, before: undefined, after: item, undo: invoked.bind(undefined, item, 'remove', []) };
            }),
    ),
];
const restore = (held: ReturnType<typeof snapshot>, codec: Codec): readonly string[] =>
    pinned(() => [
        ...differences(held, codec).flatMap(({ member, undo }) => {
            try {
                undo();
                return [];
            } catch (error) {
                return [`${member}: ${message(error)}`];
            }
        }),
        ...differences(held, codec).map(({ member }) => member),
    ]);
const unchanged = (was: PropertyDescriptor | undefined, now: PropertyDescriptor | undefined) => was !== undefined && now !== undefined && was.value === now.value && was.get === now.get && was.set === now.set;
const reset = (owner: object, key: PropertyKey, was: PropertyDescriptor | undefined): readonly string[] => {
    try {
        return (was === undefined ? Reflect.deleteProperty(owner, key) : Reflect.defineProperty(owner, key, was)) ? [] : [`global ${String(key)}`];
    } catch (error) {
        return [`global ${String(key)}: ${message(error)}`];
    }
};
const globals = () => {
    const owners: object[] = [globalThis, Object.prototype, globalThis.Array.prototype, String.prototype, Number.prototype, Boolean.prototype, globalThis.Function.prototype, Promise.prototype, Error.prototype];
    const held = owners.map((owner) => ({ owner, descriptors: new Map(Reflect.ownKeys(owner).map((key) => [key, Object.getOwnPropertyDescriptor(owner, key)] as const)) }));
    return (): readonly string[] => held.flatMap(({ owner, descriptors }) => Reflect.ownKeys(owner).flatMap((key) => (unchanged(descriptors.get(key), Object.getOwnPropertyDescriptor(owner, key)) ? [] : reset(owner, key, descriptors.get(key)))));
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { activeOf, differences, globals, LIVE, owned, pinned, preferences, restore, scanned, snapshot, survey };
