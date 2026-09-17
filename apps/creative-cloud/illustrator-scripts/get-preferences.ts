/// <reference path="./prelude.ts"/>

// --- [HOST] ----------------------------------------------------------------------------

declare const $: $;
declare const app: Application;

// --- [PRELUDE] -------------------------------------------------------------------------

const { all, each, reference, run }: Prelude = $.evalFile(new File(`${new File($.fileName).path}/prelude.jsx`));

// --- [READERS] -------------------------------------------------------------------------

const ABSENT_INTEGER = 207_192_349;

const readPreference = (key: string, kind: 'boolean' | 'integer' | 'real' | 'string'): Json => {
    if (kind === 'boolean') {
        return app.preferences.getBooleanPreference(key);
    }
    if (kind === 'real') {
        return app.preferences.getRealPreference(key);
    }
    if (kind === 'string') {
        return app.preferences.getStringPreference(key);
    }
    const value = app.preferences.getIntegerPreference(key);
    if (value === ABSENT_INTEGER) {
        throw new Error(`Integer preference ${key} answers the absent sentinel`);
    }
    return value;
};

// --- [ENTRY] ---------------------------------------------------------------------------

const getPreferences = (request: { readonly keys: { readonly key: string; readonly kind: 'boolean' | 'integer' | 'real' | 'string' }[] }, at: Site): Reading<JsonObject> =>
    all(at, [
        ['kind', (site): Reading<Json> => reference('preferences', site)],
        [
            'rows',
            (site): Reading<Json> =>
                each(site, request.keys, (row, inner): Reading<Json> => {
                    if (!app.preferences.preferenceExists(row.key)) {
                        throw new Error(`Preference ${row.key} does not exist`);
                    }
                    return all(inner, [
                        ['key', (deeper): Reading<Json> => reference(row.key, deeper)],
                        ['value', (deeper): Reading<Json> => reference(readPreference(row.key, row.kind), deeper)],
                    ]);
                }),
        ],
    ]);

run(getPreferences);
