/// <reference path="./prelude.ts"/>

// --- [PRELUDE] -------------------------------------------------------------------------

const { collect, present, run }: Prelude = $.evalFile(new File(`${new File($.fileName).path}/prelude.jsx`));

// --- [ENTRY] ---------------------------------------------------------------------------

const execute = (request: { readonly commands: string[] }): Reading<JsonObject> => present({ kind: 'values', values: collect(request.commands, (command): string => String($.global.eval(command))) });

run(execute);
