import { createDependencies, createNodes } from '@rasm/nx-apple';
import { it as nx } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = nx.extend('signed', { scope: 'file' }, async ({ fileWorkspace }) => {
    const graph = await fileWorkspace
        .write({
            'packages/signer/Package.swift': '// swift-tools-version: 6.4\nimport PackageDescription\n\nlet package = Package(name: "Signer", products: [.executable(name: "signer-cli", targets: ["Signer"])], targets: [.executableTarget(name: "Signer", exclude: ["Info.plist", "Signer.entitlements"])])\n',
            'packages/signer/Sources/Signer/main.swift': 'print("signer")\n',
            'packages/signer/Sources/Signer/Info.plist': '<?xml version="1.0" encoding="UTF-8"?>\n<plist version="1.0"><dict><key>CFBundleShortVersionString</key><string>1.0.0</string></dict></plist>\n',
            'packages/signer/Sources/Signer/Signer.entitlements': '<?xml version="1.0" encoding="UTF-8"?>\n<plist version="1.0"><dict/></plist>\n',
        })
        .graph({ createNodes, createDependencies }, {});
    return graph.nodes['Signer']?.data;
});

describe('SwiftPM executable products named apart from the module they build', { timeout: 120_000 }, () => {
    it('signs the product with the entitlements file the module holds, the file that made it entitled', ({ signed }) => {
        expect([signed?.targets?.['install--swift-package']?.options, signed?.targets?.['pack--ditto']?.options]).toStrictEqual([
            { package: 'packages/signer', product: 'signer-cli', entitlements: 'packages/signer/Sources/Signer/Signer.entitlements' },
            { products: [{ package: 'packages/signer', product: 'signer-cli', folder: 'packages/signer/.build/out/Products/Release', bin: 'packages/signer/.build/out/Products/Release', entitlements: 'packages/signer/Sources/Signer/Signer.entitlements' }] },
        ]);
    });

    it('hashes and versions the product by the Info.plist the module holds', ({ signed }) => {
        expect([signed?.targets?.['pack--ditto']?.inputs?.[0], signed?.release]).toStrictEqual(['{workspaceRoot}/packages/signer/Sources/Signer/Info.plist', { version: { versionActions: '@rasm/nx-apple/release/version-actions', versionActionsOptions: { manifest: 'Sources/Signer/Info.plist' } } }]);
    });
});
