import { isMembers, type Members, shaped } from '#/hosts/adobe/acrobat/engine/value';

// --- [TYPES] ---------------------------------------------------------------------------

interface Root extends Members {
    readonly app: Members;
    readonly global: Members;
    readonly util: Members;
    readonly console: Members;
    readonly acrohelp: unknown;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const FAULTS = { refused: 'Refused', action: 'Action', help: 'HelpError' } as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const compile = (scope: Members, body: string) => {
    const names = Object.keys(scope);
    // biome-ignore lint/nursery/noImpliedEval: case data and originals compile inside the host
    const made = new Function(...names, body);
    return (target: unknown): unknown =>
        made.apply(
            target,
            names.map((name) => scope[name]),
        );
};
const faultOf = (error: unknown) => {
    const held = isMembers(error) ? error : { message: String(error) };
    return {
        name: typeof held.name === 'string' ? held.name : 'Error',
        message: typeof held.message === 'string' ? held.message : '',
        extMessage: typeof held.extMessage === 'string' ? held.extMessage : undefined,
        lineNumber: typeof held.lineNumber === 'number' ? held.lineNumber : undefined,
        stack: typeof held.stack === 'string' ? held.stack : undefined,
    };
};
const root = shaped(compile({}, 'return this')(undefined), (value): value is Root => isMembers(value) && isMembers(value.app) && isMembers(value.global) && isMembers(value.util) && isMembers(value.console), 'this');
const base = ((): number => {
    try {
        compile({}, 'throw new Error("")')(undefined);
        return 0;
    } catch (error) {
        return (faultOf(error).lineNumber ?? 1) - 1;
    }
})();
const thrown = (error: unknown, offset = base) => {
    const fault = faultOf(error);
    const frames = (fault.stack ?? '')
        .split('\n')
        .filter((line) => line.split('@').length > 1)
        .map((line) => ({ name: line.slice(0, line.indexOf('@')), line: line.split(':').pop() }));
    const ending = frames.map(({ name }) => name).indexOf('anonymous');
    return {
        _tag: 'Thrown' as const,
        message: fault.name === FAULTS.help ? String(fault.extMessage) : `${fault.name}: ${fault.message}`,
        ...(fault.lineNumber === undefined ? {} : { line: fault.lineNumber - offset }),
        ...(ending < 0 ? {} : { stack: frames.slice(0, ending + 1).map(({ line }) => ({ line: Number(line) - offset })) }),
    };
};
const raised = (name: string, message: string, cause: unknown) => {
    const error = new Error(message);
    error.name = name;
    error.cause = cause;
    return error;
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { compile, FAULTS, faultOf, raised, root, thrown };
