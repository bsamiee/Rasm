// --- [CONSTANTS] -----------------------------------------------------------------------

const GLOBALS = ['app', 'event', 'security', 'spell', 'util', 'Report'] as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const HostScript = {
    suite: 'CARO',
    engine: { firefox: '24' },
    offset: 1,
    program: (expression: string) => `var body = function () { return (function (${GLOBALS.join(', ')}) { return ${expression}; })(${GLOBALS.map((name) => `membrane.root.${name}`).join(', ')}).call(membrane.wrap(this)); };`,
    wrap: (source: string, host: string, dialogs: string) =>
        ['(function (self) {', source, dialogs, 'var membrane = dialogs.suppressDialogs(function () { return this; }());', host, 'return encodeURIComponent(host.reply(function () { return { result: body.call(self) }; }, { reference: function () { return undefined; }, replace: membrane.unwrap }));', '})(this)'].join(
            '\n',
        ),
} as const;

// --- [EXPORTS] -------------------------------------------------------------------------

export { HostScript };
