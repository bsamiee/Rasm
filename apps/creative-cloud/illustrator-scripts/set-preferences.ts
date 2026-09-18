/// <reference path="./prelude.ts"/>

// --- [PRELUDE] -------------------------------------------------------------------------

const { collect, preference, present, run, split }: Prelude = $.evalFile(new File(`${new File($.fileName).path}/prelude.jsx`));

// --- [ENTRY] ---------------------------------------------------------------------------

const written = <K extends keyof PreferenceValue>(row: { readonly key: string; readonly kind: K; readonly value: PreferenceValue[K] }): JsonObject => {
    const before = preference[row.kind].read(row.key);
    preference[row.kind].write(row.key, row.value);
    const after = preference[row.kind].read(row.key);
    return after === row.value ? { key: row.key, before, after } : { key: row.key, before, after, reason: 'readbackDiffers' };
};

const setPreferences = (request: {
    readonly rows: { readonly [K in keyof PreferenceValue]: { readonly key: string; readonly kind: K; readonly value: PreferenceValue[K] } }[keyof PreferenceValue][];
}): Reading<JsonObject> => present(split(collect(request.rows, written)));

run(setPreferences);
