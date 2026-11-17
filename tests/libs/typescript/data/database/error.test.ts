import { ReconcileError } from '@rasm/database';
import { crossed } from '@rasm/testing/errors';
import { Effect } from 'effect';
import { describe, expect, it } from 'vitest';

describe('ReconcileError', () => {
    it('should keep the message it derives across structured clone and Nx serialization', () => {
        const raised = Effect.runSync(Effect.flip(ReconcileError.raise([{ table: 'users', columns: ['email'], reason: { _tag: 'Required' } }])));

        expect(crossed(raised)).toStrictEqual(['users (email) required, add default for existing rows', 'users (email) required, add default for existing rows']);
    });
});
