import { join } from 'node:path';
import { execute } from '@rasm/nx-apple/executors/run';
import { it, shimmed } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';

// --- [CONSTANTS] -----------------------------------------------------------------------

const EMPTY = { nodes: {}, dependencies: {} };

// --- [COMPOSITION] ---------------------------------------------------------------------

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
