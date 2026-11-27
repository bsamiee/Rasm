import { mkdirSync, readFileSync, writeFileSync } from 'node:fs';
import { join } from 'node:path';
import { execute } from '@rasm/nx-apple/executors/run';
import { it, shimmed } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';

// --- [CONSTANTS] -----------------------------------------------------------------------

const EMPTY = { nodes: {}, dependencies: {} };
const ARCHIVED = 'mkdir -p "$2/Products$0" && printf new > "$2/Products$0/Viewer"';

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('run install', () => {
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

    it.for([
        { case: 'a later invocation fails after the bundle was written', installed: true, failing: true, executable: 'old' },
        { case: 'every invocation succeeds', installed: true, failing: false, executable: 'new' },
        { case: 'every invocation succeeds with no bundle installed before', installed: false, failing: false, executable: 'new' },
    ])('installs the bundle its invocations archive only as a whole, where $case', async ({ installed, failing, executable }, { workspace }) => {
        const contents = join(workspace.root, 'Applications', 'Viewer.app', 'Contents', 'MacOS');
        if (installed) {
            mkdirSync(contents, { recursive: true });
            writeFileSync(join(contents, 'Viewer'), 'old');
        }

        const { success } = await execute({ runs: [['sh', '-c', ARCHIVED, contents], ...(failing ? [['false']] : [])], applications: ['Viewer.app'] }, workspace.context(EMPTY, 'Viewer', 'install--xcodebuild'));

        expect({ success, executable: readFileSync(join(contents, 'Viewer'), 'utf-8') }).toStrictEqual({ success: !failing, executable });
    });
});
