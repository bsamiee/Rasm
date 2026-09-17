/// <reference path="./prelude.ts"/>

// --- [HOST] ----------------------------------------------------------------------------

declare const $: $;
declare const app: Application;

// --- [PRELUDE] -------------------------------------------------------------------------

const { run }: Prelude = $.evalFile(new File(`${new File($.fileName).path}/prelude.jsx`));

// --- [ENTRY] ---------------------------------------------------------------------------

const runMenuCommand = (request: { readonly command: string }, _at: Site): Reading<JsonObject> => {
    app.executeMenuCommand(request.command);
    return { value: { kind: 'menuCommandRun', command: request.command }, unavailable: [] };
};

run(runMenuCommand);
