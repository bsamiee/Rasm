import { join } from 'node:path';
import { execute } from '@rasm/nx-apple/executors/run';
import { it, shimmed } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';

// --- [CONSTANTS] -----------------------------------------------------------------------

const EMPTY = { nodes: {}, dependencies: {} };

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('run coverage report', () => {
    it.for([
        { stated: 'no result bundle pattern', runs: [['true']], report: { profiles: ['*.profdata'], objects: ['Suite.xctest'], file: 'lcov.info' }, refusal: /^No profile matches \*\.profdata after the invocations$/u },
        { stated: 'no instrumented binary', runs: [['touch', 'default.profdata']], report: { profiles: ['*.profdata'], results: ['*.xcresult'], file: 'lcov.info' }, refusal: /^No instrumented binary is stated or reported by a result bundle matching \*\.xcresult after the invocations$/u },
    ])('reads a report with $stated as one stating none, and names what the invocations left unwritten', async ({ runs, report, refusal }, { workspace }) => {
        await expect(execute({ runs, report }, workspace.context(EMPTY, 'Viewer', 'test--xcodebuild'))).rejects.toThrow(refusal);
    });
});

describe('run install notice', () => {
    it.for([
        { case: 'runs from inside its bundle', running: '/Applications/Viewer.app/Contents/MacOS/Viewer', notices: ['Viewer runs the build it launched from, relaunch it to run this one'] },
        { case: 'runs from a bundle whose name ends in its name', running: '/Applications/MyViewer.app/Contents/MacOS/Viewer', notices: [] },
    ])('asks for a relaunch after an install only where the installed application $case', async ({ running, notices }, { workspace, onTestFinished }) => {
        const log = vi.spyOn(console, 'log').mockReturnValue(undefined);
        onTestFinished(() => {
            log.mockRestore();
        });
        shimmed(join(workspace.root, 'bin'), { ps: `printf '%s\\n' /sbin/launchd '${running}'` });

        const { success } = await execute({ runs: [['true']], applications: ['Viewer.app'] }, workspace.context(EMPTY, 'Viewer', 'install--xcodebuild'));

        expect({ success, notices: log.mock.calls.map((call) => String(call.at(-1))) }).toStrictEqual({ success: true, notices });
    });
});
