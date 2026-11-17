import { SshError } from '@rasm/ssh';
import { crossed } from '@rasm/testing/errors';
import { describe, expect, it } from 'vitest';

describe('SshError', () => {
    it('should keep the message it derives across structured clone and Nx serialization', () => {
        expect(crossed(SshError.of({ _tag: 'Refused', stderr: 'bind: address in use' }, 'derby'))).toStrictEqual(['derby OpenSSH refused the forward, free its bind end: bind: address in use', 'derby OpenSSH refused the forward, free its bind end: bind: address in use']);
    });
});
