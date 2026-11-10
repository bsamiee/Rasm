use core::num::NonZero;

use secrecy::ExposeSecret as _;
use sqlx::error::DatabaseError;
use sqlx::pool::PoolConnection;
use sqlx::postgres::{PgArguments, PgConnection, PgPool};
use sqlx::query::QueryScalar;
use sqlx::{AssertSqlSafe, ConnectOptions as _, Connection as _};

use crate::stores::{Create, Login, Server, Template, decoded, encoded, named};
use crate::{Kind, Label, Operation, ProvisioningError, Refusal, Removal, StoreId};

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Debug, Clone)]
pub struct Postgres {
    pub admin: PgPool,
    pub connections: NonZero<u32>,
}

impl Postgres {
    async fn login(&self, id: StoreId, label: &Label, owned: &'static str, operation: Operation) -> Result<(PoolConnection<sqlx::Postgres>, Login), ProvisioningError> {
        const LOGIN: &str = "SELECT unnest(ARRAY[format('CREATE ROLE %I LOGIN CREATEROLE CONNECTION LIMIT %s PASSWORD %L', $1::text, $2::int8, $3::text), \
                             format('ALTER ROLE %I SET createrole_self_grant = %L', $1::text, 'set, inherit'), format($4::text, $1::text, $1::text), \
                             format('REVOKE CONNECT ON DATABASE %I FROM PUBLIC', $1::text), format('COMMENT ON DATABASE %I IS %L', $1::text, $5::text)])";
        let refused = |source| ProvisioningError::Postgres { operation, source };
        let (granted, comment) = (Login::new(id), encoded(id, label)?);
        let mut admin = self.admin.acquire().await.map_err(refused)?;
        let script = sqlx::query_scalar(LOGIN).bind(id.to_string()).bind(NonZero::<i64>::from(self.connections)).bind(granted.password.expose_secret()).bind(owned).bind(comment);
        scripted(&mut admin, script).await.map_err(refused)?;
        Ok((admin, granted))
    }
}

impl Server for Postgres {
    fn kind(&self) -> Kind { Kind::Postgres }

    async fn limit(&self) -> Result<Option<u32>, ProvisioningError> {
        const LIMIT: &str = "SELECT GREATEST((current_setting('max_connections')::int8 - current_setting('superuser_reserved_connections')::int8 \
                             - current_setting('reserved_connections')::int8 - $1::int8) / $2::int8, 0)::int4";
        let admin = i64::from(self.admin.options().get_max_connections()).saturating_mul(2);
        let stores = sqlx::query_scalar::<_, i32>(LIMIT).bind(admin).bind(NonZero::<i64>::from(self.connections)).fetch_one(&self.admin).await;
        stores.map(|stores| Some(stores.unsigned_abs())).map_err(|source| ProvisioningError::Postgres { operation: Operation::Limit, source })
    }

    async fn stores(&self) -> Result<Vec<(StoreId, Option<Label>)>, ProvisioningError> {
        const LIST: &str = "SELECT datname::text, shobj_description(oid, 'pg_database') FROM pg_database";
        let listed = sqlx::query_as::<_, (String, Option<String>)>(LIST).fetch_all(&self.admin).await.map_err(|source| ProvisioningError::Postgres { operation: Operation::List, source })?;
        Ok(listed.into_iter().filter_map(|(name, comment)| named(&name).map(|id| (id, comment.and_then(|comment| decoded(id, comment.as_bytes()))))).collect())
    }

    async fn label(&self, id: StoreId, label: &Label) -> Result<(), ProvisioningError> {
        const LABEL: &str = "SELECT format('COMMENT ON DATABASE %I IS %L', $1::text, $2::text)";
        const SEAL: &str = "SELECT unnest(ARRAY[format('ALTER ROLE %I NOLOGIN', $1::text), format('ALTER DATABASE %I OWNER TO CURRENT_USER', $1::text), \
                            format('ALTER DATABASE %I ALLOW_CONNECTIONS false IS_TEMPLATE true', $1::text), format('COMMENT ON DATABASE %I IS %L', $1::text, $2::text)])";
        const CLOSE: &str = "SELECT count(pg_terminate_backend(pid)) FROM pg_stat_activity WHERE datname = $1::text";
        let (name, comment) = (id.to_string(), encoded(id, label)?);
        let labeled = async {
            let mut admin = self.admin.acquire().await?;
            match label {
                Label::Template { digest: _ } => {
                    let mut sealing = admin.begin().await?;
                    scripted(&mut sealing, sqlx::query_scalar(SEAL).bind(&name).bind(comment)).await?;
                    sealing.commit().await?;
                    sqlx::query_scalar::<_, i64>(CLOSE).bind(&name).fetch_one(&mut *admin).await.map(drop)
                }
                Label::Held { session: _ } | Label::Kept { session: _ } | Label::Build { session: _, digest: _ } | Label::Ready { digest: _ } =>
                    scripted(&mut admin, sqlx::query_scalar(LABEL).bind(&name).bind(comment)).await,
            }
        };
        labeled.await.map_err(|source| ProvisioningError::Postgres { operation: Operation::Label(id), source })
    }

    async fn remove(&self, id: StoreId, removal: Removal) -> Result<(), ProvisioningError> {
        const OPEN: &str = "SELECT count(*) FROM pg_stat_activity WHERE datname = $1::text AND backend_type = 'client backend'";
        const REMOVE: &str = "SELECT unnest(ARRAY(SELECT format('ALTER DATABASE %I IS_TEMPLATE false', datname) FROM pg_database WHERE datname = $1::text AND datistemplate) \
                              || format('DROP DATABASE IF EXISTS %I%s', $1::text, $2::text) \
                              || ARRAY(SELECT format('DROP ROLE IF EXISTS %I', made.rolname) FROM pg_auth_members held JOIN pg_roles made ON made.oid = held.roleid \
                              JOIN pg_roles login ON login.oid = held.member WHERE login.rolname = $1::text AND held.admin_option) || format('DROP ROLE IF EXISTS %I', $1::text))";
        let (name, refused) = (id.to_string(), |source| ProvisioningError::Postgres { operation: Operation::Remove(id), source });
        let mut admin = self.admin.acquire().await.map_err(refused)?;
        let (open, force) = match removal {
            Removal::Idle => (sqlx::query_scalar::<_, i64>(OPEN).bind(&name).fetch_one(&mut *admin).await.map_err(refused)?, ""),
            Removal::Forced => (0, " WITH (FORCE)"),
        };
        (open == 0).ok_or(ProvisioningError::Store { id, refusal: Refusal::InUse })?;
        scripted(&mut admin, sqlx::query_scalar(REMOVE).bind(&name).bind(force)).await.map_err(|source| match source.as_database_error().and_then(DatabaseError::code) {
            Some("55006") => ProvisioningError::Store { id, refusal: Refusal::InUse },
            Some(_) | None => refused(source),
        })
    }
}

impl Create for Postgres {
    type Access = Login;

    async fn create(&self, id: StoreId, label: &Label) -> Result<Login, ProvisioningError> {
        self.login(id, label, "CREATE DATABASE %I OWNER %I STRATEGY WAL_LOG", Operation::Create(id)).await.map(|(_, granted)| granted)
    }
}

impl Template for Postgres {
    async fn copy(&self, id: StoreId, template: StoreId, label: &Label) -> Result<(), ProvisioningError> {
        const COPY: &str = "SELECT unnest(ARRAY[format('CREATE DATABASE %I TEMPLATE %I STRATEGY WAL_LOG', $1::text, $2::text), format('COMMENT ON DATABASE %I IS %L', $1::text, $3::text)])";
        let comment = encoded(id, label)?;
        let copied = async { scripted(&mut *self.admin.acquire().await?, sqlx::query_scalar(COPY).bind(id.to_string()).bind(template.to_string()).bind(comment)).await };
        copied.await.map_err(|source| ProvisioningError::Postgres { operation: Operation::Copy(id), source })
    }

    async fn grant(&self, id: StoreId, template: StoreId, label: &Label) -> Result<Login, ProvisioningError> {
        const REASSIGN: &str = "SELECT format('REASSIGN OWNED BY %I TO %I', $1::text, $2::text)";
        let (_admin, granted) = self.login(id, label, "ALTER DATABASE %I OWNER TO %I", Operation::Grant(id)).await?;
        let reassigned = async {
            let mut inside = self.admin.connect_options().as_ref().clone().database(&id.to_string()).connect().await?;
            scripted(&mut inside, sqlx::query_scalar(REASSIGN).bind(template.to_string()).bind(id.to_string())).await?;
            inside.close().await
        };
        reassigned.await.map(|()| granted).map_err(|source| ProvisioningError::Postgres { operation: Operation::Grant(id), source })
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

async fn scripted(connection: &mut PgConnection, script: QueryScalar<'_, sqlx::Postgres, String, PgArguments>) -> Result<(), sqlx::Error> {
    for statement in script.fetch_all(&mut *connection).await? {
        sqlx::raw_sql(AssertSqlSafe(statement)).execute(&mut *connection).await?;
    }
    Ok(())
}
