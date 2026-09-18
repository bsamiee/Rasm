/// <reference path="./prelude.ts"/>

// --- [PRELUDE] -------------------------------------------------------------------------

const { present, run }: Prelude = $.evalFile(new File(`${new File($.fileName).path}/prelude.jsx`));

// --- [ENTRY] ---------------------------------------------------------------------------

const runMenuCommand = (request: { readonly command: string }): Reading<JsonObject> => {
    app.executeMenuCommand(request.command);
    return present({ kind: 'applied', command: request.command });
};

run(runMenuCommand);
