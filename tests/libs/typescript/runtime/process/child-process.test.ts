import { ExitError } from '@rasm/process';
import { crossed } from '@rasm/testing/errors';
import { describe, expect, it } from 'vitest';

describe('ExitError', () => {
    it('should keep the message it derives across structured clone and Nx serialization', () => {
        expect(crossed(ExitError.of({ command: 'uv sync', code: 3, stdout: '', stderr: 'refused\n' }))).toStrictEqual(['uv sync exited 3: refused', 'uv sync exited 3: refused']);
    });
});
