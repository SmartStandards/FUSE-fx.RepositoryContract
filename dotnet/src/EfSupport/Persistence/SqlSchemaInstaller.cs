using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.ModelDescription;
using System.Linq;
using System.Text;

namespace System.Data.Fuse.Sql {

  public class SqlSchemaInstaller {

    public static void EnsureSchemaIsInstalled(DbConnection connection, SchemaRoot schemaRoot) {
      if (connection == null) throw new ArgumentNullException(nameof(connection));
      if (schemaRoot == null) throw new ArgumentNullException(nameof(schemaRoot));

      var opened = false;
      try {
        if (connection.State != ConnectionState.Open) {
          connection.Open();
          opened = true;
        }

        using (var tx = connection.BeginTransaction()) {
          try {
            // collect required table names
            var requiredTables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var entity in schemaRoot.Entities ?? Array.Empty<EntitySchema>()) {
              var tableName = GetTableName(entity);
              requiredTables.Add(tableName);

              if (!TableExists(connection, tx, tableName)) {
                // create table with columns and PK
                var createSql = BuildCreateTableSql(entity, schemaRoot, tableName);
                ExecuteNonQuery(connection, tx, createSql);
              } else {
                // sync columns
                SyncColumns(connection, tx, entity, schemaRoot, tableName);
              }

              // ensure primary key exists (may be created in create table)
              EnsurePrimaryKey(connection, tx, entity, schemaRoot, tableName);

              // ensure indices
              EnsureIndices(connection, tx, entity, tableName);
            }

            // ensure foreign keys from relations
            EnsureForeignKeys(connection, tx, schemaRoot);

            // drop tables that are present in DB but not in schemaRoot
            var existingTables = GetExistingTables(connection, tx);
            foreach (var existing in existingTables) {
              if (!requiredTables.Contains(existing)) {
                //ExecuteNonQuery(connection, tx, $"DROP TABLE [dbo].[{SqlEscape(existing)}]"); //TODO
              }
            }

            tx.Commit();
          } catch {
            try { tx.Rollback(); } catch { }
            throw;
          }
        }
      } finally {
        if (opened) {
          try { connection.Close(); } catch { }
        }
      }
    }

    private static string GetTableName(EntitySchema entity) {
      if (!string.IsNullOrEmpty(entity.NamePlural)) return entity.NamePlural;
      if (!string.IsNullOrEmpty(entity.Name)) return entity.Name;
      throw new InvalidOperationException("Entity has no name");
    }

    private static bool TableExists(DbConnection connection, DbTransaction tx, string tableName) {
      using (var cmd = connection.CreateCommand()) {
        cmd.Transaction = tx;
        cmd.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = @table";
        var p = cmd.CreateParameter();
        p.ParameterName = "@table";
        p.Value = tableName;
        cmd.Parameters.Add(p);
        var cnt = Convert.ToInt32(cmd.ExecuteScalar());
        return cnt > 0;
      }
    }

    private static List<string> GetExistingTables(DbConnection connection, DbTransaction tx) {
      var result = new List<string>();
      using (var cmd = connection.CreateCommand()) {
        cmd.Transaction = tx;
        cmd.CommandText = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_TYPE = 'BASE TABLE'";
        using (var rdr = cmd.ExecuteReader()) {
          while (rdr.Read()) result.Add(rdr.GetString(0));
        }
      }
      return result;
    }

    private static string BuildCreateTableSql(EntitySchema entity, SchemaRoot schemaRoot, string tableName) {
      var sb = new StringBuilder();
      sb.Append($"CREATE TABLE [dbo].[{SqlEscape(tableName)}] (");

      var fields = entity.Fields ?? Array.Empty<FieldSchema>();
      var pkFieldNames = GetPrimaryKeyFieldNames(entity, schemaRoot);

      var first = true;
      foreach (var f in fields) {
        if (!first) sb.Append(", ");
        first = false;

        var col = BuildColumnDefinition(f, pkFieldNames);
        sb.Append(col);
      }

      // add primary key constraint if not identity single int handled already
      if (pkFieldNames != null && pkFieldNames.Count > 0) {
        // If PK was already created as identity NOT NULL inside column definition for single int, still add constraint
        sb.Append(", CONSTRAINT [PK_").Append(SqlEscape(tableName)).Append("] PRIMARY KEY (");
        sb.Append(string.Join(", ", pkFieldNames.Select(n => $"[{SqlEscape(n)}]")));
        sb.Append(")");
      }

      sb.Append(")");
      return sb.ToString();
    }

    private static void SyncColumns(DbConnection connection, DbTransaction tx, EntitySchema entity, SchemaRoot schemaRoot, string tableName) {
      var existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
      using (var cmd = connection.CreateCommand()) {
        cmd.Transaction = tx;
        cmd.CommandText = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = @table";
        var p = cmd.CreateParameter();
        p.ParameterName = "@table";
        p.Value = tableName;
        cmd.Parameters.Add(p);
        using (var rdr = cmd.ExecuteReader()) {
          while (rdr.Read()) existing.Add(rdr.GetString(0));
        }
      }

      var fields = entity.Fields ?? Array.Empty<FieldSchema>();
      var desired = new HashSet<string>(fields.Select(f => f.Name), StringComparer.OrdinalIgnoreCase);

      // add missing columns
      foreach (var f in fields) {
        if (!existing.Contains(f.Name)) {
          var sqlType = MapFieldTypeToSql(f);
          var nullSpec = f.Required ? "NOT NULL" : "NULL";
          var addSql = $"ALTER TABLE [dbo].[{SqlEscape(tableName)}] ADD [{SqlEscape(f.Name)}] {sqlType} {nullSpec}";
          ExecuteNonQuery(connection, tx, addSql);
        }
      }

      // drop extra columns
      foreach (var col in existing.ToArray()) {
        if (!desired.Contains(col)) {
          var dropSql = $"ALTER TABLE [dbo].[{SqlEscape(tableName)}] DROP COLUMN [{SqlEscape(col)}]";
          ExecuteNonQuery(connection, tx, dropSql);
        }
      }
    }

    private static void EnsurePrimaryKey(DbConnection connection, DbTransaction tx, EntitySchema entity, SchemaRoot schemaRoot, string tableName) {
      var pkFieldNames = GetPrimaryKeyFieldNames(entity, schemaRoot);
      if (pkFieldNames == null || pkFieldNames.Count == 0) return;

      // check if primary key constraint exists
      using (var cmd = connection.CreateCommand()) {
        cmd.Transaction = tx;
        cmd.CommandText = @"
SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu ON tc.CONSTRAINT_NAME = kcu.CONSTRAINT_NAME
WHERE tc.TABLE_SCHEMA = 'dbo' AND tc.TABLE_NAME = @table AND tc.CONSTRAINT_TYPE = 'PRIMARY KEY'";
        var p = cmd.CreateParameter();
        p.ParameterName = "@table";
        p.Value = tableName;
        cmd.Parameters.Add(p);
        var cnt = Convert.ToInt32(cmd.ExecuteScalar());
        if (cnt == 0) {
          var cols = string.Join(", ", pkFieldNames.Select(n => $"[{SqlEscape(n)}]"));
          var sql = $"ALTER TABLE [dbo].[{SqlEscape(tableName)}] ADD CONSTRAINT [PK_{SqlEscape(tableName)}] PRIMARY KEY ({cols})";
          ExecuteNonQuery(connection, tx, sql);
        }
      }
    }

    private static void EnsureIndices(DbConnection connection, DbTransaction tx, EntitySchema entity, string tableName) {
      var indices = entity.Indices ?? Array.Empty<IndexSchema>();
      foreach (var idx in indices) {
        var indexName = idx.Name;
        if (string.IsNullOrEmpty(indexName)) {
          // create a name
          indexName = $"IX_{tableName}_{string.Join("_", idx.MemberFieldNames)}";
        }

        if (!IndexExists(connection, tx, tableName, indexName)) {
          var cols = string.Join(", ", idx.MemberFieldNames.Select(n => $"[{SqlEscape(n)}]"));
          var unique = idx.Unique ? "UNIQUE " : "";
          var sql = $"CREATE {unique}NONCLUSTERED INDEX [{SqlEscape(indexName)}] ON [dbo].[{SqlEscape(tableName)}] ({cols})";
          ExecuteNonQuery(connection, tx, sql);
        }
      }
    }

    private static bool IndexExists(DbConnection connection, DbTransaction tx, string tableName, string indexName) {
      using (var cmd = connection.CreateCommand()) {
        cmd.Transaction = tx;
        cmd.CommandText = @"
SELECT COUNT(*) FROM sys.indexes i
JOIN sys.objects o ON o.object_id = i.object_id
WHERE o.object_id = OBJECT_ID(@obj) AND i.name = @name";
        var p1 = cmd.CreateParameter(); p1.ParameterName = "@obj"; p1.Value = $"dbo.{tableName}"; cmd.Parameters.Add(p1);
        var p2 = cmd.CreateParameter(); p2.ParameterName = "@name"; p2.Value = indexName; cmd.Parameters.Add(p2);
        var cnt = Convert.ToInt32(cmd.ExecuteScalar());
        return cnt > 0;
      }
    }

    private static void EnsureForeignKeys(DbConnection connection, DbTransaction tx, SchemaRoot schemaRoot) {
      var relations = schemaRoot.Relations ?? Array.Empty<RelationSchema>();
      foreach (var rel in relations) {
        // foreign table is rel.ForeignEntityName (the dependent)
        var foreignEntity = schemaRoot.Entities.FirstOrDefault(e => e.Name == rel.ForeignEntityName || e.NamePlural == rel.ForeignEntityName);
        var primaryEntity = schemaRoot.Entities.FirstOrDefault(e => e.Name == rel.PrimaryEntityName || e.NamePlural == rel.PrimaryEntityName);
        if (foreignEntity == null || primaryEntity == null) continue;

        // get index column names
        var fkIndex = foreignEntity.Indices?.FirstOrDefault(ix => ix.Name == rel.ForeignKeyIndexName);
        IndexSchema pkIndex = null; // primaryEntity.Indices?.FirstOrDefault(ix => ix.Name == rel.PrimaryKeyIndexName);

        // fallback: use PrimaryKeyIndexName if pkIndex null
        if (pkIndex == null && !string.IsNullOrEmpty(primaryEntity.PrimaryKeyIndexName)) {
          pkIndex = primaryEntity.Indices?.FirstOrDefault(ix => ix.Name == primaryEntity.PrimaryKeyIndexName);
        }

        if (fkIndex == null || pkIndex == null) continue;
        var foreignTable = GetTableName(foreignEntity);
        var primaryTable = GetTableName(primaryEntity);

        var fkCols = fkIndex.MemberFieldNames;
        var pkCols = pkIndex.MemberFieldNames;
        if (fkCols == null || fkCols.Length == 0 || pkCols == null || pkCols.Length == 0) continue;

        var fkName = $"FK_{foreignTable}_{primaryTable}_{string.Join("_", fkCols)}";

        if (!ForeignKeyExists(connection, tx, foreignTable, fkName)) {
          var fkColsList = string.Join(", ", fkCols.Select(c => $"[{SqlEscape(c)}]"));
          var pkColsList = string.Join(", ", pkCols.Select(c => $"[{SqlEscape(c)}]"));
          var onDelete = rel.CascadeDelete ? "ON DELETE CASCADE" : "";
          var sql = new StringBuilder();
          sql.Append($"ALTER TABLE [dbo].[{SqlEscape(foreignTable)}] ADD CONSTRAINT [{SqlEscape(fkName)}] FOREIGN KEY ({fkColsList})");
          sql.Append($" REFERENCES [dbo].[{SqlEscape(primaryTable)}] ({pkColsList}) {onDelete}");
          ExecuteNonQuery(connection, tx, sql.ToString());
        }
      }
    }

    private static bool ForeignKeyExists(DbConnection connection, DbTransaction tx, string tableName, string fkName) {
      using (var cmd = connection.CreateCommand()) {
        cmd.Transaction = tx;
        cmd.CommandText = @"
SELECT COUNT(*) FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE o.object_id = OBJECT_ID(@obj) AND fk.name = @name";
        var p1 = cmd.CreateParameter(); p1.ParameterName = "@obj"; p1.Value = $"dbo.{tableName}"; cmd.Parameters.Add(p1);
        var p2 = cmd.CreateParameter(); p2.ParameterName = "@name"; p2.Value = fkName; cmd.Parameters.Add(p2);
        var cnt = Convert.ToInt32(cmd.ExecuteScalar());
        return cnt > 0;
      }
    }

    private static List<string> GetPrimaryKeyFieldNames(EntitySchema entity, SchemaRoot schemaRoot) {
      if (entity == null) return new List<string>();
      var pkIndexName = entity.PrimaryKeyIndexName;
      if (!string.IsNullOrEmpty(pkIndexName) && entity.Indices != null) {
        var idx = entity.Indices.FirstOrDefault(i => i.Name == pkIndexName);
        if (idx != null && idx.MemberFieldNames != null) return idx.MemberFieldNames.ToList();
      }

      // fallback: look for fields marked DbGeneratedIdentity or Name == "Id"
      var candidates = entity.Fields?.Where(f => f.DbGeneratedIdentity).Select(f => f.Name).ToList();
      if (candidates != null && candidates.Count > 0) return candidates;
      if (entity.Fields != null && entity.Fields.Any(f => string.Equals(f.Name, "Id", StringComparison.OrdinalIgnoreCase))) {
        return new List<string> { "Id" };
      }
      return new List<string>();
    }

    private static string BuildColumnDefinition(FieldSchema f, List<string> pkFieldNames) {
      var sb = new StringBuilder();
      var name = f.Name;
      var sqlType = MapFieldTypeToSql(f);
      var isPk = pkFieldNames != null && pkFieldNames.Contains(name);
      var notNull = f.Required || isPk ? "NOT NULL" : "NULL";

      // identity handling
      if (isPk && pkFieldNames.Count == 1 && f.DbGeneratedIdentity && (f.Type?.StartsWith("int", StringComparison.OrdinalIgnoreCase) == true || f.Type?.StartsWith("long", StringComparison.OrdinalIgnoreCase) == true)) {
        sb.Append($"[{SqlEscape(name)}] {sqlType} IDENTITY(1,1) {notNull}");
      } else {
        sb.Append($"[{SqlEscape(name)}] {sqlType} {notNull}");
      }

      return sb.ToString();
    }

    private static string MapFieldTypeToSql(FieldSchema f) {
      var t = (f?.Type ?? "").Trim().ToLowerInvariant();
      if (string.IsNullOrEmpty(t)) return "NVARCHAR(MAX)";

      if (t.StartsWith("int")) return "INT";
      if (t.StartsWith("long") || t.StartsWith("int64") || t.StartsWith("bigint")) return "BIGINT";
      if (t.StartsWith("short") || t.StartsWith("int16")) return "SMALLINT";
      if (t.StartsWith("bool") || t.StartsWith("boolean")) return "BIT";
      if (t.StartsWith("datetime") || t.StartsWith("date")) return "DATETIME2";
      if (t.StartsWith("guid") || t.StartsWith("uniqueidentifier")) return "UNIQUEIDENTIFIER";
      if (t.StartsWith("decimal")) return "DECIMAL(18,4)";
      if (t.StartsWith("double") || t.StartsWith("float64")) return "FLOAT";
      if (t.StartsWith("float") || t.StartsWith("single")) return "REAL";
      if (t.StartsWith("binary") || t.StartsWith("varbinary") || t.StartsWith("byte[]")) return "VARBINARY(MAX)";

      // treat as string, respect MaxLength if present
      if (f.MaxLength > 0 && f.MaxLength <= 4000) {
        return $"NVARCHAR({f.MaxLength})";
      }
      return "NVARCHAR(MAX)";
    }

    private static void ExecuteNonQuery(DbConnection connection, DbTransaction tx, string sql) {
      using (var cmd = connection.CreateCommand()) {
        cmd.Transaction = tx;
        cmd.CommandText = sql;
        cmd.CommandType = CommandType.Text;
        cmd.ExecuteNonQuery();
      }
    }

    private static void ExecuteNonQuery(DbConnection connection, string sql) {
      using (var cmd = connection.CreateCommand()) {
        cmd.CommandText = sql;
        cmd.CommandType = CommandType.Text;
        cmd.ExecuteNonQuery();
      }
    }

    private static string SqlEscape(string identifier) {
      return identifier.Replace("]", "]]");
    }
  } 

}