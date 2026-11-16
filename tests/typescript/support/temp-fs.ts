import { appendFileSync, mkdirSync, mkdtempSync, realpathSync, renameSync, rmSync, symlinkSync, unlinkSync, writeFileSync } from 'node:fs';
import { readFile } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import { dirname, join } from 'node:path';
import { Effect } from 'effect';
import { setWorkspaceRoot, workspaceRoot } from 'nx/src/utils/workspace-root';

// --- [SERVICES] ------------------------------------------------------------------------

class TempFs {
    readonly tempDir: string;
    readonly #previous = workspaceRoot;

    constructor(prefix: string, overrideWorkspaceRoot = true) {
        this.tempDir = realpathSync(mkdtempSync(join(tmpdir(), prefix)));
        if (overrideWorkspaceRoot) {
            setWorkspaceRoot(this.tempDir);
        }
    }

    // --- [FILES]

    createFiles(files: Readonly<Record<string, string>>) {
        return Effect.runPromise(Effect.sync(() => this.createFilesSync(files)));
    }
    createFilesSync(files: Readonly<Record<string, string>>) {
        Object.entries(files).forEach(([path, content]) => this.createFileSync(path, content));
    }
    createFile(path: string, content: string) {
        return Effect.runPromise(Effect.sync(() => this.createFileSync(path, content)));
    }
    createFileSync(path: string, content: string) {
        mkdirSync(dirname(join(this.tempDir, path)), { recursive: true });
        writeFileSync(join(this.tempDir, path), content);
    }
    createDirSync(path: string) {
        mkdirSync(join(this.tempDir, path), { recursive: true });
    }
    createSymlinkSync(target: string, path: string, type: 'dir' | 'file') {
        mkdirSync(dirname(join(this.tempDir, path)), { recursive: true });
        symlinkSync(join(this.tempDir, target), join(this.tempDir, path), type);
    }
    readFile(path: string) {
        return readFile(join(this.tempDir, path), 'utf-8');
    }
    removeFileSync(path: string) {
        unlinkSync(join(this.tempDir, path));
    }
    appendFile(path: string, content: string) {
        appendFileSync(join(this.tempDir, path), content);
    }
    writeFile(path: string, content: string) {
        writeFileSync(join(this.tempDir, path), content);
    }
    renameFile(from: string, to: string) {
        renameSync(join(this.tempDir, from), join(this.tempDir, to));
    }

    // --- [LIFECYCLE]

    cleanup() {
        rmSync(this.tempDir, { recursive: true, force: true, maxRetries: 5 });
        setWorkspaceRoot(this.#previous);
    }
    reset() {
        rmSync(this.tempDir, { recursive: true, force: true, maxRetries: 5 });
        mkdirSync(this.tempDir, { recursive: true });
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { TempFs };
