/// <reference path="./prelude.ts"/>

// --- [PRELUDE] -------------------------------------------------------------------------

const { all, each, failure, preference, reference, run }: Prelude = $.evalFile(new File(`${new File($.fileName).path}/prelude.jsx`));

// --- [ENTRY] ---------------------------------------------------------------------------

const ABSENT_INTEGER = 207_192_349;

const getPreferences = (request: { readonly keys: { readonly key: string; readonly kind: keyof PreferenceValue }[] }, at: Site): Reading<JsonObject> => {
    const unreadable: JsonObject = {};
    const reading = all(at, [
        ['kind', (site): Reading<Json> => reference('preferences', site)],
        [
            'rows',
            (site): Reading<Json> =>
                each(site, request.keys, (row, inner): Reading<Json> => {
                    try {
                        if (!app.preferences.preferenceExists(row.key)) {
                            throw new Error(`Preference ${row.key} does not exist`);
                        }
                        const value = preference[row.kind].read(row.key);
                        if (row.kind === 'integer' && value === ABSENT_INTEGER) {
                            throw new Error(`Integer preference ${row.key} answers the absent sentinel`);
                        }
                        const result = all(inner, [
                            ['key', (deeper): Reading<Json> => reference(row.key, deeper)],
                            ['value', (deeper): Reading<Json> => reference(value, deeper)],
                        ]);
                        if (result.unavailable.length > 0) {
                            unreadable[row.key] = result.unavailable;
                        }
                        return result;
                    } catch (error) {
                        unreadable[row.key] = [failure(error, inner)];
                        throw error;
                    }
                }),
        ],
    ]);
    reading.value['unreadable'] = unreadable;
    return reading;
};

run(getPreferences);
