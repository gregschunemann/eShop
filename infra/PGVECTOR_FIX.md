# PostgreSQL pgvector Extension Fix

## Issue
The Bicep deployment failed with the error:
```
Value 'vector' is invalid for server parameter 'shared_preload_libraries'. 
Allowed values are ',age,anon,auto_explain,azure_storage,pg_cron,pg_duckdb,pg_failover_slots,pg_hint_plan,pg_partman_bgw,pg_prewarm,pg_squeeze,pg_stat_statements,pgaudit,pglogical,timescaledb,wal2json'.
```

## Root Cause
The `vector` extension (pgvector) was incorrectly configured as a shared preloaded library. In Azure Database for PostgreSQL Flexible Server, the `pgvector` extension is not loaded via `shared_preload_libraries` but is instead enabled at the database level using SQL commands.

## Solution
1. **Removed** the incorrect configuration:
   ```bicep
   resource postgreSqlExtension 'Microsoft.DBforPostgreSQL/flexibleServers/configurations@2023-12-01-preview' = {
     parent: postgreSqlServer
     name: 'shared_preload_libraries'
     properties: {
       value: 'vector'
       source: 'user-override'
     }
   }
   ```

2. **Added** documentation comment explaining the proper approach:
   ```bicep
   // PostgreSQL Extensions - pgvector is enabled at database level, not shared_preload_libraries
   // The vector extension will be created via SQL: CREATE EXTENSION IF NOT EXISTS vector;
   ```

## Post-Deployment Steps
After the infrastructure is deployed, you'll need to connect to each PostgreSQL database and manually enable the pgvector extension:

```sql
-- Connect to each database and run:
CREATE EXTENSION IF NOT EXISTS vector;
```

This should be done for:
- Catalog database
- Identity database  
- Ordering database
- Webhooks database

## Alternative Approach
Consider adding the extension creation to your application's database migration scripts or startup procedures to automate this process.

## References
- [Azure Database for PostgreSQL - Extensions](https://docs.microsoft.com/en-us/azure/postgresql/flexible-server/concepts-extensions)
- [pgvector Extension Documentation](https://github.com/pgvector/pgvector)