import { Extension, Grant, Provider, type ProviderArgs, Role, type RoleArgs, Schema } from '@pulumi/postgresql';
import type { CustomResourceOptions } from '@pulumi/pulumi';
import { generatePassword } from '#/infra/generate-password';

// --- [OPERATIONS] ----------------------------------------------------------------------

const provisionDatabase = (resource: string, server: ProviderArgs & { readonly database: string }, protect: boolean, options?: CustomResourceOptions) => {
    const provider = new Provider(resource, server, options);
    const login = (role: string, settings: RoleArgs) => {
        const { result } = generatePassword(`${resource}-${role}`);
        return { name: new Role(`${resource}-${role}`, { name: `${resource}-${role}`, login: true, password: result, ...settings }, { provider }).name, password: result };
    };
    const owner = login('migrator', { idleInTransactionSessionTimeout: 60_000 });
    const schema = new Schema(resource, { name: owner.name, owner: owner.name, database: server.database }, { provider, protect });
    const runtime = login('runtime', { statementTimeout: 15_000, idleInTransactionSessionTimeout: 30_000, searchPaths: [schema.name] });
    const statistics = new Extension(`${resource}-pg-stat-statements`, { name: 'pg_stat_statements', database: server.database }, { provider });
    return {
        migrator: { user: new Grant(`${resource}-migrator`, { database: server.database, role: schema.owner, objectType: 'database', privileges: ['CREATE'] }, { provider, dependsOn: statistics }).role, password: owner.password },
        runtime: { user: new Grant(`${resource}-runtime`, { database: server.database, role: runtime.name, objectType: 'schema', schema: schema.name, privileges: ['USAGE'] }, { provider }).role, password: runtime.password },
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { provisionDatabase };
