// Boot-edge runner setup: structural Equal.equals equality for Effect data in every toEqual across the estate.
import { addEqualityTesters } from '@effect/vitest';

addEqualityTesters();

// VM container runtimes expose the daemon socket at /var/run/docker.sock inside the VM while DOCKER_HOST
// names the host-side path Ryuk cannot mount; the daemon-side default holds for every runtime, env wins.
process.env['TESTCONTAINERS_DOCKER_SOCKET_OVERRIDE'] ??= '/var/run/docker.sock';
