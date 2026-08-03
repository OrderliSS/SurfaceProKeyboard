# Supabase Schema Drift Resolution Skill

## Purpose
Ensure database integrity by identifying and resolving schema drift between staging and production environments in Supabase prior to generating or applying database migrations.

## Instructions
1. **Pre-Migration Check**:
   - Always compare and verify schema parity between staging and production Supabase environments.
   - Pay special attention to hardware identifier (HWID) tracking tables (e.g. `hwid_logs`, `device_registrations`, or equivalent HWID tracking tables).
2. **Drift Detection**:
   - Audit column definitions, index constraints, foreign keys, and RLS (Row Level Security) policies across environments.
   - Confirm HWID column data types (e.g. `UUID`, `VARCHAR`, or `TEXT`) match identically in both staging and production.
3. **Migration Generation & Application**:
   - Do not generate or apply any new database migrations until schema drift is fully resolved or explicitly accounted for in the migration script.
   - Test migrations in staging first, validating HWID tracking table behavior before applying to production.
