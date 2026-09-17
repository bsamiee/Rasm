/// <reference path="./prelude.ts"/>

// --- [HOST] ----------------------------------------------------------------------------

declare const $: $;
declare const app: Application;

// --- [PRELUDE] -------------------------------------------------------------------------

const { all, each, reference, run }: Prelude = $.evalFile(new File(`${new File($.fileName).path}/prelude.jsx`));

// --- [WRITERS] -------------------------------------------------------------------------

type Kind = 'boolean' | 'integer' | 'real' | 'string';

interface Row {
    readonly key: string;
    readonly kind: Kind;
    readonly value: boolean | number | string;
}

const readPreference = (key: string, kind: Kind): boolean | number | string => {
    if (kind === 'boolean') {
        return app.preferences.getBooleanPreference(key);
    }
    if (kind === 'real') {
        return app.preferences.getRealPreference(key);
    }
    if (kind === 'string') {
        return app.preferences.getStringPreference(key);
    }
    return app.preferences.getIntegerPreference(key);
};

const writePreference = (row: Row): void => {
    if (row.kind === 'boolean') {
        app.preferences.setBooleanPreference(row.key, row.value === true);
        return;
    }
    if (row.kind === 'real') {
        app.preferences.setRealPreference(row.key, Number(row.value));
        return;
    }
    if (row.kind === 'string') {
        app.preferences.setStringPreference(row.key, String(row.value));
        return;
    }
    app.preferences.setIntegerPreference(row.key, Number(row.value));
};

// --- [ENTRY] ---------------------------------------------------------------------------

const setPreferences = (request: { readonly rows: Row[] }, at: Site): Reading<JsonObject> =>
    all(at, [
        ['kind', (site): Reading<Json> => reference('preferencesApplied', site)],
        [
            'rows',
            (site): Reading<Json> =>
                each(site, request.rows, (row, inner): Reading<Json> => {
                    const before = readPreference(row.key, row.kind);
                    writePreference(row);
                    const after = readPreference(row.key, row.kind);
                    return all(inner, [
                        ['key', (deeper): Reading<Json> => reference(row.key, deeper)],
                        ['before', (deeper): Reading<Json> => reference(before, deeper)],
                        ['after', (deeper): Reading<Json> => reference(after, deeper)],
                        ['held', (deeper): Reading<Json> => reference(after === row.value, deeper)],
                    ]);
                }),
        ],
    ]);

run(setPreferences);
