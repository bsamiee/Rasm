/// <reference path="./prelude.ts"/>

// --- [HOST] ----------------------------------------------------------------------------

declare const $: $;

// --- [PRELUDE] -------------------------------------------------------------------------

const { collect, run }: Prelude = $.evalFile(new File(`${new File($.fileName).path}/prelude.jsx`));

// --- [ENTRY] ---------------------------------------------------------------------------

const execute = (request: { readonly commands: string[] }, _at: Site): Reading<JsonObject> => ({
    value: { kind: 'values', values: collect(request.commands, (command): string => String($.global.eval(command))) },
    unavailable: [],
});

run(execute);
