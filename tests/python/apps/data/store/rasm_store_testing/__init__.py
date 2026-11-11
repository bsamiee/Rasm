"""Store fixtures for pytest: the session a test process holds on the store service and the stores a test makes, loaded beside `rasm_testing.plugin` through `pytest_plugins` of a suite's top conftest or `-p rasm_store_testing.plugin`."""

lazy from rasm_store_testing._stores import BUCKET, Bucket, CACHE, Cache, CLICKHOUSE, Clickhouse, Kind, Lease, POSTGRES, Postgres, Refusal, Store, Stores, Template, Templated

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["BUCKET", "CACHE", "CLICKHOUSE", "POSTGRES", "Bucket", "Cache", "Clickhouse", "Kind", "Lease", "Postgres", "Refusal", "Store", "Stores", "Template", "Templated"]
