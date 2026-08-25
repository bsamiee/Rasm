import { addEqualityTesters } from '@effect/vitest';

addEqualityTesters();

if (typeof process !== 'undefined') {
    process.env['TESTCONTAINERS_DOCKER_SOCKET_OVERRIDE'] ??= '/var/run/docker.sock';
}
