import { spawn } from 'node:child_process';
import { once } from 'node:events';
import { constants, copyFileSync, mkdirSync, readFileSync, writeFileSync } from 'node:fs';
import { join } from 'node:path';
import { execPath } from 'node:process';
import { execute } from '@rasm/nx-apple/executors/run';
import { it } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';

// --- [CONSTANTS] -----------------------------------------------------------------------

const EMPTY = { nodes: {}, dependencies: {} };
const ARCHIVED = 'mkdir -p "$2/Products$0" && printf new > "$2/Products$0/Viewer"';

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('run install', () => {
    it.for([
        { case: 'runs from inside its bundle', bundle: 'Viewer.app', notices: ['Viewer runs the build it launched from, relaunch it to run this one'] },
        { case: 'runs from a bundle whose name ends in its name', bundle: 'MyViewer.app', notices: [] },
    ])('asks for a relaunch after an install only where the installed application $case', async ({ bundle, notices }, { workspace, onTestFinished }) => {
        const [contents, launched] = [join(workspace.root, 'Applications', 'Viewer.app', 'Contents', 'MacOS'), join(workspace.root, 'Applications', bundle, 'Contents', 'MacOS', 'Viewer')];
        mkdirSync(join(launched, '..'), { recursive: true });
        copyFileSync(execPath, launched, constants.COPYFILE_FICLONE);
        const [log, child] = [vi.spyOn(console, 'log').mockReturnValue(undefined), spawn(launched, ['-e', 'setTimeout(() => {}, 60_000)'], { stdio: 'ignore' })];
        onTestFinished(() => {
            child.kill();
            log.mockRestore();
        });
        await once(child, 'spawn');

        const { success } = await execute({ runs: [['sh', '-c', ARCHIVED, contents]], applications: ['Viewer.app'] }, workspace.context(EMPTY, 'Viewer', 'install--xcodebuild'));

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
