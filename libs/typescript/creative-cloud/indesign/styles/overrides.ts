import type { InDesignDOMObject } from 'adobe:indesign';
import { Cache, Effect, Record, Result, type Schema } from 'effect';
import { encoder } from '#/dom/encoding';
import { Model } from '#/dom/model';

// --- [OPERATIONS] ----------------------------------------------------------------------

const overrides = Effect.fnUntraced(function* (object: InDesignDOMObject<unknown>, own: object, base: (name: string) => unknown, except: Schema.Struct.Fields) {
    const [{ rows, plural }, encoded] = yield* Effect.all([Model, encoder]);
    const { fields } = yield* Cache.get(rows, object.constructorName);
    return Record.filterMap(fields, (row, name) => (row.settable && !plural(row) && !Object.hasOwn(except, name) && encoded(Reflect.get(own, name)) !== encoded(base(name)) ? Result.succeed<unknown>(Reflect.get(own, name)) : Result.failVoid));
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { overrides };
