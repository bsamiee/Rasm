import { fileURLToPath } from 'node:url';
import { runInNewContext } from 'node:vm';
import { assert, it } from '@effect/vitest';
import { Completion } from '@rasm/extendscript';
import { script } from '@rasm/extendscript/vite';
import { ScriptHost } from '@rasm/indesign';
import { UxpManifest } from '@rasm/uxp';
import { Array, Cause, Context, Effect, Formatter, Layer, Option, Schema, Struct } from 'effect';
import { build } from 'vite';

// --- [CONSTANTS] -----------------------------------------------------------------------

const LABEL = 'Run suite body';
const PAGE = { constructorName: 'Page', isValid: true, name: '1', toSpecifier: () => '/document[@id=1]//page[@id=205]' };
const CLOSED = { constructorName: 'Page', isValid: false, name: 'gone', toSpecifier: () => '/document[@id=1]//page[@id=206]' };

// --- [MODELS] --------------------------------------------------------------------------

const Inlined = Schema.Struct({ output: Schema.NonEmptyArray(Schema.Struct({ code: Schema.String })) });
const Answer = Schema.fromJsonString(Completion);

// --- [SERVICES] ------------------------------------------------------------------------

class Runtime extends Context.Service<Runtime, string>()('@rasm/test-indesign/script.test/Runtime') {
    static readonly layer = Effect.gen(function* () {
        const host = fileURLToPath(import.meta.resolve('@rasm/extendscript/host'));
        const built = yield* Effect.tryPromise({
            try: () => build({ configFile: false, logLevel: 'silent', plugins: [script(host, 'runtime')], build: { write: false, rolldownOptions: { input: `${host}?inline`, preserveEntrySignatures: 'exports-only', output: { format: 'es' } } } }),
            catch: (cause) => new Cause.UnknownError(cause, Formatter.format(cause)),
        });
        const { code } = Array.headNonEmpty((yield* Schema.decodeUnknownEffect(Inlined)(built)).output);
        const inlined = yield* Effect.promise((): Promise<{ readonly default: unknown }> => import(`data:text/javascript,${encodeURIComponent(code)}`));
        return yield* Schema.decodeUnknownEffect(Schema.String)(inlined.default);
    }).pipe(Layer.effect(Runtime));
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const scripted = Effect.fnUntraced(function* (body: string, ran: (step: () => void) => void) {
    const runtime = yield* Runtime;
    const steps: { readonly language: unknown; readonly mode: unknown; readonly name: unknown; readonly level: unknown }[] = [];
    const replies: string[] = [];
    const scriptPreferences = { userInteractionLevel: 'INTERACT_WITH_ALL' };
    const doScript = (step: () => void, language: unknown, _: unknown, mode: unknown, name: unknown): void => {
        steps.push({ language, mode, name, level: scriptPreferences.userInteractionLevel });
        ran(step);
    };
    const indesign = { app: { scriptPreferences, doScript, page: PAGE, closed: CLOSED }, ScriptLanguage: { UXPSCRIPT: 'UXPSCRIPT' }, UndoModes: { ENTIRE_SCRIPT: 'ENTIRE_SCRIPT' }, UserInteractionLevels: { NEVER_INTERACT: 'NEVER_INTERACT' } };
    const uxp = { script: { setResult: (reply: string) => replies.push(decodeURIComponent(reply)) } };
    runInNewContext(UxpManifest.script(`(function body() {\n${body}\n})`, { runtime, host: ScriptHost, label: LABEL }), { require: (id: string) => ({ uxp, indesign })[id] ?? raising(new Error(`Cannot find module '${id}'`)) });
    const answers = yield* Effect.forEach(replies, (reply) => Schema.decodeEffect(Answer)(reply));
    return { steps, answers, level: scriptPreferences.userInteractionLevel };
});
const raising = (error: Error): never => {
    throw error;
};
const rolledBack =
    (seen: unknown[]) =>
    (step: () => void): void => {
        try {
            step();
        } catch (error) {
            seen.push(error);
            throw new Error('Uncaught JavaScript exception', { cause: error });
        }
    };
const ran = (step: () => void): void => {
    step();
};
const skipped = (): void => undefined;

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(Runtime.layer)('InDesign do script body', (methods) => {
    methods.effect(
        'runs as one undo step under its caller label with dialogs off, restores the interaction level, and answers its value',
        Effect.fnUntraced(function* () {
            assert.deepStrictEqual(yield* scripted('return 7;', ran), { steps: [{ language: 'UXPSCRIPT', mode: 'ENTIRE_SCRIPT', name: LABEL, level: 'NEVER_INTERACT' }], answers: [{ _tag: 'Value', value: { result: 7 } }], level: 'INTERACT_WITH_ALL' });
        }),
    );

    methods.effect(
        'answers a returned InDesign object as its type, specifier, validity, and a valid one by name, and a plain object as itself',
        Effect.fnUntraced(function* () {
            const [page, closed, plain] = yield* Effect.all([scripted("return require('indesign').app.page;", ran), scripted("return require('indesign').app.closed;", ran), scripted('return { held: [1, "two"] };', ran)]);
            assert.deepStrictEqual(
                [page.answers, closed.answers, plain.answers],
                [
                    [{ _tag: 'Value', value: { result: { type: 'Page', specifier: '/document[@id=1]//page[@id=205]', valid: true, name: '1' } } }],
                    [{ _tag: 'Value', value: { result: { type: 'Page', specifier: '/document[@id=1]//page[@id=206]', valid: false } } }],
                    [{ _tag: 'Value', value: { result: { held: [1, 'two'] } } }],
                ],
            );
        }),
    );

    methods.effect(
        'answers what the body threw at its body line in place of the host rollback error, and restores the interaction level',
        Effect.fnUntraced(function* () {
            const seen: unknown[] = [];
            const { answers, level } = yield* scripted("const first = 1;\nthrow new Error('boom ' + first);", rolledBack(seen));
            assert.deepStrictEqual([answers, level, seen.length], [[{ _tag: 'Thrown', message: 'boom 1', number: Option.none(), line: Option.some(3 + UxpManifest.script.offset) }], 'INTERACT_WITH_ALL', 1]);
        }),
    );

    methods.effect(
        'refuses a body returning a promise, as do script runs a body synchronously',
        Effect.fnUntraced(function* () {
            const { answers } = yield* scripted('return Promise.resolve(7);', ran);
            assert.deepStrictEqual(Array.map(answers, Struct.get('_tag')), ['IllegalArgument']);
        }),
    );

    methods.effect(
        'answers a failure when the host returns before it ran the step',
        Effect.fnUntraced(function* () {
            const { answers } = yield* scripted('return 7;', skipped);
            assert.deepStrictEqual(answers, [{ _tag: 'Thrown', message: 'doScript returned before its function ran', number: Option.none(), line: Option.none() }]);
        }),
    );
});
