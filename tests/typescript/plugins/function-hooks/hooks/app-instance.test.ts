import { expect, it } from '@effect/vitest';
import { applyChange, createdPids, detachedQuit, outputChange, storedInstances } from '#/plugins/function-hooks/hooks/app-instance';

// --- [CONSTANTS] -----------------------------------------------------------------------

const LAUNCHED = '{"com.adobe.InDesign":{"64327":{"launched":{"created":true}}}}';
const REUSED = '{"com.adobe.InDesign":{"64327":{"launched":{"created":false}}}}';
const ASKING = '{"com.adobe.InDesign":{"64327":{"running":{"active":false,"asking":[{"kind":"modal","number":22354,"options":["Cancel","Don\'t Save","Save"],"text":["Save changes?"],"title":"Adobe InDesign"}],"hidden":true,"status":"responding","windows":[22351,22360]}}}}';
const BLOCKING = '{"com.adobe.InDesign":{"64327":{"running":{"active":false,"hidden":true,"process":"sleeping","status":"unresponsive","usage":{},"windows":[{"asking":true,"frame":[[653,277],[494,131]],"kind":"blocking","layer":8,"number":20917,"onScreen":false,"title":"Adobe InDesign"}]}}}}';
const EXITED = '{"com.apple.TextEdit":{"70001":{"exited":{"reporting":true,"status":0}}}}';
const FAILED = '{"watch-apps":{"fail":{"reason":"unmatched","subject":"TextEdit"}}}';
const INSTANCE = { pid: 64327, bundle: 'com.adobe.InDesign' };

// --- [COMPOSITION] ---------------------------------------------------------------------

it.each([
    ['launched created', LAUNCHED, [INSTANCE], []],
    ['launched reused', REUSED, [], []],
    ['asking outline', ASKING, [], []],
    ['blocking value', BLOCKING, [], []],
    ['exited', EXITED, [], [70001]],
    ['fail', FAILED, [], []],
    ['launch then states', [LAUNCHED, BLOCKING, ASKING].join('\n'), [INSTANCE], []],
    ['text around lines', `building\n${LAUNCHED}\nnot json {\n`, [INSTANCE], []],
] as const)('reads a line: %s', (_name, line, created, exited) => {
    const direct = outputChange(line);
    expect([direct.created, [...direct.exited]]).toStrictEqual([created, exited]);
    expect(outputChange({ stdout: line })).toStrictEqual(direct);
});

it.each([
    ['launch', { pid: 64327, created: true }, [64327]],
    ['launch reusing', { pid: 64327, created: false }, []],
    ['open launching', { document: 'a.indd', launched: { pid: 64327, created: true } }, [64327]],
    ['open reusing', { document: 'a.indd', launched: { pid: 64327, created: false } }, []],
    ['open over a link', { document: 'a.indd' }, []],
] as const)('reads an Atelier answer: %s', (_name, answer, pids) => {
    const blocks = [{ type: 'text', text: JSON.stringify(answer) }];
    expect(createdPids(blocks)).toStrictEqual(pids);
    expect(createdPids({ content: blocks })).toStrictEqual(pids);
});

it('keeps rows by pid until exit', () => {
    const rows = applyChange(storedInstances([{ ...INSTANCE, agent: 'a1' }, { pid: 70001, bundle: 'com.apple.TextEdit' }, { pid: 'x' }]), outputChange(EXITED));
    expect(rows).toStrictEqual([{ ...INSTANCE, agent: 'a1' }]);
});

it('forces a quit that fails, each pid detached', () => {
    expect(detachedQuit([64327, 70001])).toBe(['(watch-apps quit --pid 64327 || watch-apps quit --pid 64327 --force) </dev/null >/dev/null 2>&1 &', '(watch-apps quit --pid 70001 || watch-apps quit --pid 70001 --force) </dev/null >/dev/null 2>&1 &'].join('\n'));
});
