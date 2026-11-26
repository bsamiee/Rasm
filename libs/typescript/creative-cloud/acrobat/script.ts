// --- [OPERATIONS] ----------------------------------------------------------------------

const HostScript = {
    suite: 'CARO',
    engine: { firefox: '24' },
    offset: 1,
    program: (expression: string) => `var body = function () { return (function (app, event, security, spell, util, Report) { return ${expression}; })(membrane.root.app, membrane.root.event, membrane.root.security, membrane.root.spell, membrane.root.util, membrane.root.Report).call(membrane.wrap(this)); };`,
    wrap: (source: string, host: string, dialogs: string) =>
        ['(function (self) {', source, dialogs, 'var membrane = dialogs.suppressDialogs(function () { return this; }());', host, 'return encodeURIComponent(host.reply(function () { return { result: body.call(self) }; }, { reference: function () { return undefined; }, replace: membrane.unwrap }));', '})(this)'].join(
            '\n',
        ),
} as const;

// --- [EXPORTS] -------------------------------------------------------------------------

export { HostScript };
