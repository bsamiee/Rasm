import { execFileSync } from 'node:child_process';
import { join } from 'node:path';
import { execute } from '@rasm/nx-apple/executors/package';
import { it } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('package', () => {
    it('zips the application an archive states with the bundle at the root of the zip, where no notary key is set', async ({ workspace, onTestFinished }) => {
        ['ASC_API_KEY_P8', 'ASC_API_KEY_ID', 'ASC_API_ISSUER_ID'].forEach((key) => {
            vi.stubEnv(key, undefined);
        });
        onTestFinished(() => {
            vi.unstubAllEnvs();
        });
        workspace.write({
            'Viewer.xcarchive/Info.plist': '<?xml version="1.0" encoding="UTF-8"?>\n<plist version="1.0"><dict><key>ApplicationProperties</key><dict><key>ApplicationPath</key><string>Applications/Viewer.app</string></dict></dict></plist>\n',
            'Viewer.xcarchive/Products/Applications/Viewer.app/Contents/Info.plist': '<?xml version="1.0" encoding="UTF-8"?>\n<plist version="1.0"><dict><key>CFBundleName</key><string>Viewer</string></dict></plist>\n',
            'Viewer.xcarchive/Products/Applications/Viewer.app/Contents/MacOS/Viewer': '#!/bin/sh\n',
        });

        const { success } = await execute({ exports: [{ archive: 'Viewer.xcarchive', folder: 'dist', run: ['mkdir', '-p', 'dist'] }] }, workspace.context({ nodes: {}, dependencies: {} }, 'Viewer', 'pack--ditto'));

        expect({
            success,
            zipped: execFileSync('zipinfo', ['-1', join(workspace.root, 'dist/Viewer.zip')], { encoding: 'utf-8' })
                .split('\n')
                .filter(Boolean)
                .toSorted(),
        }).toStrictEqual({
            success: true,
            zipped: ['Viewer.app/', 'Viewer.app/Contents/', 'Viewer.app/Contents/Info.plist', 'Viewer.app/Contents/MacOS/', 'Viewer.app/Contents/MacOS/Viewer'],
        });
    });
});
