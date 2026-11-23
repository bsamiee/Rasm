import { runInNewContext } from 'node:vm';
import { ScriptHost } from '@rasm/indesign';
import { UxpManifest } from '@rasm/uxp';
import { describe, expect, it } from 'vitest';

// --- [OPERATIONS] ----------------------------------------------------------------------

const rolledBack = (step: () => void): void => {
    try {
        step();
    } catch {
        throw new Error('Uncaught JavaScript exception');
    }
};
const scripted = (body: string, ran: (step: () => void) => void) => {
    const steps: { readonly language: unknown; readonly mode: unknown; readonly name: unknown; readonly level: unknown }[] = [];
    const answers: string[] = [];
    const scriptPreferences = { userInteractionLevel: 'INTERACT_WITH_ALL' };
    const doScript = (step: () => void, language: unknown, _: unknown, mode: unknown, name: unknown): void => {
        steps.push({ language, mode, name, level: scriptPreferences.userInteractionLevel });
        ran(step);
    };
    const page = { constructorName: 'Page', isValid: true, name: '1', toSpecifier: () => '/document[@id=1]//page[@id=205]' };
    const host = { app: { scriptPreferences, doScript, page }, ScriptLanguage: { UXPSCRIPT: 'UXPSCRIPT' }, UndoModes: { ENTIRE_SCRIPT: 'ENTIRE_SCRIPT' }, UserInteractionLevels: { NEVER_INTERACT: 'NEVER_INTERACT' } };
    const uxp = { script: { setResult: (reply: string) => answers.push(decodeURIComponent(reply)) } };
    const runtime = 'const runtime = { reply: (answer, encoder) => { try { const held = answer(); return JSON.stringify(typeof held.result === "object" ? { result: encoder.reference(held.result) ?? null } : held); } catch (error) { return JSON.stringify({ thrown: error.message }); } } };';
    runInNewContext(UxpManifest.script(`(function body() {\n${body}\n})`, { runtime, host: ScriptHost, label: 'Run suite body' }), { require: (id: string) => (id === 'uxp' ? uxp : host) });
    return { steps, answers, level: scriptPreferences.userInteractionLevel };
};

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('InDesign do script body', () => {
    it('runs as one UXP undo step named by the label its caller passed, with no dialogs, and answers its value', () => {
        expect(scripted('return 7;', (step) => step())).toStrictEqual({ steps: [{ language: 'UXPSCRIPT', mode: 'ENTIRE_SCRIPT', name: 'Run suite body', level: 'NEVER_INTERACT' }], answers: ['{"result":7}'], level: 'INTERACT_WITH_ALL' });
    });

    it('answers a returned host object as its type, specifier, validity, and name, and a plain object as no reference', () => {
        expect([scripted("return require('indesign').app.page;", (step) => step()).answers, scripted('return { held: 1 };', (step) => step()).answers]).toStrictEqual([['{"result":{"type":"Page","specifier":"/document[@id=1]//page[@id=205]","valid":true,"name":"1"}}'], ['{"result":null}']]);
    });

    it('answers what the body threw in place of the host rollback error, and restores the interaction level', () => {
        const { answers, level } = scripted("throw new Error('boom');", rolledBack);
        expect([answers, level]).toStrictEqual([['{"thrown":"boom"}'], 'INTERACT_WITH_ALL']);
    });

    it('answers a failure when the host returns before it ran the step', () => {
        expect(scripted('return 7;', () => undefined).answers).toStrictEqual(['{"thrown":"doScript returned before its function ran"}']);
    });
});
