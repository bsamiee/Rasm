use clickhouse::Client;
use clickhouse::sql::Identifier;
use secrecy::ExposeSecret as _;

use crate::stores::{Create, Login, Server, Template, decoded, encoded, named};
use crate::{Kind, Label, Operation, ProvisioningError, Refusal, Removal, StoreId};

// --- [CONSTANTS] -----------------------------------------------------------------------

const DROP_USER: &str = "DROP USER IF EXISTS ?";
const CREATE: &str = "CREATE DATABASE ?";

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Debug, Clone)]
pub struct Clickhouse {
    pub admin: Client,
}

impl Clickhouse {
    async fn login(&self, id: StoreId, comment: &str) -> Result<Login, clickhouse::error::Error> {
        let (name, granted) = (id.to_string(), Login::new(id));
        self.admin.query("CREATE USER ? IDENTIFIED BY ?").bind(Identifier(&name)).bind(granted.password.expose_secret()).execute().await?;
        self.admin
            .query("GRANT ALL ON ?.*, SELECT ON system.data_skipping_indices, SELECT ON system.projections, SELECT ON system.constraints TO ?")
            .bind(Identifier(&name))
            .bind(Identifier(&name))
            .execute()
            .await?;
        self.comment(id, comment).await.map(|()| granted)
    }

    async fn comment(&self, id: StoreId, comment: &str) -> Result<(), clickhouse::error::Error> {
        self.admin.query("ALTER DATABASE ? MODIFY COMMENT ?").bind(Identifier(&id.to_string())).bind(comment).execute().await
    }
}

impl Server for Clickhouse {
    fn kind(&self) -> Kind { Kind::Clickhouse }

    async fn limit(&self) -> Result<Option<u32>, ProvisioningError> {
        let stated = self.admin.query("SELECT toUInt32(value) FROM system.server_settings WHERE name = 'max_database_num_to_throw'").fetch_one::<u32>().await;
        stated.map(|limit| Some(limit).filter(|limit| *limit > 0)).map_err(|source| ProvisioningError::Clickhouse { operation: Operation::Limit, source })
    }

    async fn stores(&self) -> Result<Vec<(StoreId, Option<Label>)>, ProvisioningError> {
        let listed = self.admin.query("SELECT name, comment FROM system.databases").fetch_all::<(String, String)>().await;
        let listed = listed.map_err(|source| ProvisioningError::Clickhouse { operation: Operation::List, source })?;
        Ok(listed.into_iter().filter_map(|(name, comment)| named(&name).map(|id| (id, decoded(id, comment.as_bytes())))).collect())
    }

    async fn label(&self, id: StoreId, label: &Label) -> Result<(), ProvisioningError> {
        let comment = encoded(id, label)?;
        let labeled = async {
            if let Label::Template { digest: _ } = label {
                self.admin.query(DROP_USER).bind(Identifier(&id.to_string())).execute().await?;
            }
            self.comment(id, &comment).await
        };
        labeled.await.map_err(|source| ProvisioningError::Clickhouse { operation: Operation::Label(id), source })
    }

    async fn remove(&self, id: StoreId, removal: Removal) -> Result<(), ProvisioningError> {
        let (name, refused) = (id.to_string(), |source| ProvisioningError::Clickhouse { operation: Operation::Remove(id), source });
        let open = match removal {
            Removal::Idle => self.admin.query("SELECT count() FROM system.processes WHERE user = ?").bind(&name).fetch_one::<u64>().await.map_err(refused)?,
            Removal::Forced => 0,
        };
        (open == 0).ok_or(ProvisioningError::Store { id, refusal: Refusal::InUse })?;
        self.admin.query(DROP_USER).bind(Identifier(&name)).execute().await.map_err(refused)?;
        self.admin.query("DROP DATABASE IF EXISTS ? SYNC").bind(Identifier(&name)).execute().await.map_err(refused)
    }
}

impl Create for Clickhouse {
    type Access = Login;

    async fn create(&self, id: StoreId, label: &Label) -> Result<Login, ProvisioningError> {
        let comment = encoded(id, label)?;
        let created = async {
            self.admin.query(CREATE).bind(Identifier(&id.to_string())).execute().await?;
            self.login(id, &comment).await
        };
        created.await.map_err(|source| ProvisioningError::Clickhouse { operation: Operation::Create(id), source })
    }
}

impl Template for Clickhouse {
    async fn copy(&self, id: StoreId, template: StoreId, label: &Label) -> Result<(), ProvisioningError> {
        const OBJECTS: &str = "SELECT name, CAST(endsWith(engine, 'MergeTree') AS Bool), replaceAll(create_table_query, ?, ?) FROM system.tables \
                               WHERE database = ? AND NOT startsWith(name, '.inner') ORDER BY engine IN ('View', 'MaterializedView'), metadata_modification_time";
        let (name, source, comment) = (id.to_string(), template.to_string(), encoded(id, label)?);
        let copied = async {
            self.admin.query(CREATE).bind(Identifier(&name)).execute().await?;
            self.comment(id, &comment).await?;
            for (object, cloned, statement) in self.admin.query(OBJECTS).bind(&source).bind(&name).bind(&source).fetch_all::<(String, bool, String)>().await? {
                let made = if cloned {
                    self.admin.query("CREATE TABLE ?.? CLONE AS ?.?").bind(Identifier(&name)).bind(Identifier(&object)).bind(Identifier(&source)).bind(Identifier(&object))
                } else {
                    self.admin.query_raw(&statement)
                };
                made.execute().await?;
            }
            Ok(())
        };
        copied.await.map_err(|source| ProvisioningError::Clickhouse { operation: Operation::Copy(id), source })
    }

    async fn grant(&self, id: StoreId, _: StoreId, label: &Label) -> Result<Login, ProvisioningError> {
        self.login(id, &encoded(id, label)?).await.map_err(|source| ProvisioningError::Clickhouse { operation: Operation::Grant(id), source })
    }
}
