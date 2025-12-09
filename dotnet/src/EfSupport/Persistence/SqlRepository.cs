using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Fuse.Convenience;
using System.Data.Fuse.Ef.InstanceManagement;
using System.Data.Fuse.Sql.InstanceManagement;
using System.Data.ModelDescription;
using System.Data.ModelDescription.Convenience;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
#if NETCOREAPP
using System.Text.Json;
using System.Windows.Markup;
#endif

namespace System.Data.Fuse.Sql {

  /// <summary>
  /// SQL implementation of the IRepository interface that uses ADO.NET instead of Entity Framework
  /// </summary>
  public class SqlRepository<TEntity, TKey> : IRepository<TEntity, TKey> where TEntity : class {

    #region " SchemaRoot/Metadata Caching "

    private static SchemaRoot _SchemaRoot = null;
    public SchemaRoot GetSchemaRoot() {
      //if (_SchemaRoot == null) {
      //  _SchemaRoot = SchemaCache.GetSchemaRootForType(typeof(TEntity));
      //}
      return _SchemaRoot;
    }

    public class NavigationInfo {
      public EntitySchema OtherType { get; set; } = null;
      public FieldSchema[] Fields { get; set; } = null;
      public RelationSchema RelationSchema { get; set; } = null;
      public PropertyInfo NavigationProperty { get; set; } = null;
      public string JoinTemplate { get; set; } = null;
      public FieldSchema LabelField { get; set; } = null;
    }

    private static Dictionary<string, NavigationInfo[]> _NavigationInfoByFullTypeName;

    protected NavigationInfo[] GetNavigationInfo(Type type) {
      string fullTypeName = type.FullName;
      if (_NavigationInfoByFullTypeName == null) {
        _NavigationInfoByFullTypeName = new Dictionary<string, NavigationInfo[]>();
      }
      if (!_NavigationInfoByFullTypeName.TryGetValue(fullTypeName, out var info)) {
        info = LoadNavigationInfo(type);
        _NavigationInfoByFullTypeName[fullTypeName] = info;
      }
      return info;
    }

    private NavigationInfo[] LoadNavigationInfo(Type type) {
      EntitySchema schema = this.GetSchemaRoot().GetSchema(type.Name);
      IEnumerable<RelationSchema> relations = this.GetSchemaRoot().Relations.Where(
        (r) => r.ForeignEntityName == type.Name || r.PrimaryEntityName == type.Name
      );

      if (relations.Count() == 0) {
        return Array.Empty<NavigationInfo>();
      }
      List<NavigationInfo> result = new List<NavigationInfo>();
      foreach (RelationSchema relation in relations) {
        if (relation.ForeignEntityName == type.Name) {
          if (string.IsNullOrEmpty(relation.ForeignNavigationName)) continue;
          PropertyInfo navigationProp = type.GetProperty(relation.ForeignNavigationName);
          if (navigationProp == null) continue;
          NavigationInfo info = new NavigationInfo();
          EntitySchema otherSchema = this.GetSchemaRoot().GetSchema(relation.PrimaryEntityName);
          if (otherSchema == null) continue;
          info.OtherType = otherSchema;
          info.RelationSchema = relation;
          info.NavigationProperty = navigationProp;
          if (typeof(EntityRef).IsAssignableFrom(navigationProp.PropertyType)) {
            IndexSchema keyIndexSchema = otherSchema.GetIndex(otherSchema.PrimaryKeyIndexName);
            List<FieldSchema> fields = keyIndexSchema.MemberFieldNames
              .Select((f) => otherSchema.Fields.Single((fs) => fs.Name == f))
              .ToList();
            FieldSchema labelField = otherSchema.Fields.FirstOrDefault((f) => f.IdentityLabel);
            info.LabelField = labelField;
            if (labelField != null) {
              fields.Add(labelField);
            }
            info.Fields = fields.ToArray();
          } else {
            info.Fields = otherSchema.Fields.ToArray();
          }
          IndexSchema foreignKeyIndexSchema = schema.GetIndex(relation.ForeignKeyIndexName);
          IndexSchema primaryIndexSchema = otherSchema.GetIndex(otherSchema.PrimaryKeyIndexName);
          if (primaryIndexSchema.MemberFieldNames.Length == 0) { continue; }
          if (foreignKeyIndexSchema.MemberFieldNames.Length == 0) { continue; }
          for (int i = 0; i < foreignKeyIndexSchema.MemberFieldNames.Length; ++i) {
            info.JoinTemplate += $"t0.{foreignKeyIndexSchema.MemberFieldNames[i]} = t{{0}}.{primaryIndexSchema.MemberFieldNames[i]}";
          }
          result.Add(info);
        } else if (relation.PrimaryEntityName == type.Name) {
          //TODO
        }
      }
      return result.ToArray();
    }

    #endregion

    private readonly IDbConnectionProvider _ConnectionProvider;
    private readonly bool _OwnsConnection;
    private readonly string _TableName = null;
    private readonly string _SchemaName = null;
    private string SchemaAndTableName => string.IsNullOrEmpty(_SchemaName) ? _TableName : $"{_SchemaName}.{_TableName}";

    public IDbConnectionProvider ConnectionProvider {
      get {
        return _ConnectionProvider;
      }
    }

    //public SqlRepository(IDbConnectionProvider connectionProvider) {
    //  _connectionProvider = connectionProvider;
    //  _tableName = GetTableName(typeof(TEntity));
    //  _ownsConnection = false;
    //}

    public SqlRepository(
      IDbConnectionProvider connectionProvider,
      SchemaRoot schemaRoot,
      string tableName = null,
      string schemaName = null
    ) {
      _ConnectionProvider = connectionProvider;
      _SchemaRoot = schemaRoot;
      _OwnsConnection = false;
      EntitySchema schema = _SchemaRoot.GetSchema(typeof(TEntity).Name);
      this._TableName = string.IsNullOrEmpty(tableName) ? schema.NamePlural : tableName;
      this._SchemaName = schemaName;
    }

    //[Obsolete("This overload is unsafe because it doesn't care about lifetime management of the connection!")]
    //public SqlRepository(IDbConnection connection) {
    //  _connectionProvider = new LongLivingConnectionProvider(connection);
    //  _tableName = GetTableName(typeof(TEntity));
    //  _ownsConnection = true;
    //}

    private static List<List<PropertyInfo>> _UniqueKeySets;
    private static List<PropertyInfo> _PrimaryKeySet;

    protected List<List<PropertyInfo>> Keysets {
      get {
        if (_UniqueKeySets == null) {
          _UniqueKeySets = InitUniqueKeySets();
        }
        return _UniqueKeySets;
      }
    }

    protected List<PropertyInfo> PrimaryKeySet {
      get {
        if (_PrimaryKeySet == null) {
          _PrimaryKeySet = InitPrimaryKeySet();
        }
        return _PrimaryKeySet;
      }
    }

    protected List<List<PropertyInfo>> InitUniqueKeySets() {
      return GetSchemaRoot().GetUniqueKeysetsProperties(typeof(TEntity));
    }

    protected List<PropertyInfo> InitPrimaryKeySet() {
      return GetSchemaRoot().GetPrimaryKeyProperties(typeof(TEntity)).ToList();
    }

    #region " Helper Methods "

    private string BuildSelectSql(
      bool includeNavigations,
      string whereClause = null,
      string orderByClause = null,
      int? limit = null, int? offset = null
    ) {
      NavigationInfo[] navigationInfos = GetNavigationInfo(typeof(TEntity));

      StringBuilder sql = new StringBuilder();

      //TODO select only the fields that are in the TEntity
      if (includeNavigations) {
        sql.Append($"SELECT t0.*");
      } else {
        sql.Append("SELECT *");
      }
      int t = 1;
      if (includeNavigations) {
        foreach (var info in navigationInfos) {
          if (info.Fields != null && info.Fields.Length > 0) {
            sql.Append(", ");
            sql.Append(string.Join(", ", info.Fields.Select((f) => $"t{t}.{f.Name} as 'nav_{t}.{f.Name}'")));
          }
          t++;
        }
        t = 1;
      }
      if (includeNavigations) {
        sql.Append($" FROM {SchemaAndTableName} t0 ");
      } else {
        sql.Append($" FROM {SchemaAndTableName}");
      }
      if (includeNavigations) {
        foreach (var info in navigationInfos) {
          if (info.Fields != null && info.Fields.Length > 0) {
            string otherTableName = string.IsNullOrEmpty(_SchemaName) ?
              info.OtherType.NamePlural :
              $"{_SchemaName}.{info.OtherType.NamePlural}";
            sql.Append($"left outer join {otherTableName} t{t} on {string.Format(info.JoinTemplate, t)}");
          }
          t++;
        }
      }

      if (!string.IsNullOrEmpty(whereClause)) {
        sql.Append(" WHERE ").Append(whereClause);
      }

      if (!string.IsNullOrEmpty(orderByClause)) {
        sql.Append(" ORDER BY ").Append(orderByClause);
      }

      // Add pagination if supported by the database provider
      if ((limit.HasValue && limit.Value > 0) || offset.HasValue) {
        // This is a simplified version - actual implementation should be database specific
        if (offset.HasValue) {
          sql.Append(" OFFSET ").Append(offset.Value).Append(" ROWS");
        }
        if (limit.HasValue && limit.Value > 0) {
          sql.Append(" FETCH NEXT ").Append(limit.Value).Append(" ROWS ONLY");
        }
      }

      return sql.ToString();
    }

    private string BuildSelectFieldsSql(string[] fieldNames, string whereClause = null, string orderByClause = null, int? limit = null, int? offset = null) {
      StringBuilder sql = new StringBuilder();
      sql.Append("SELECT ").Append(string.Join(", ", fieldNames)).Append(" FROM ").Append(SchemaAndTableName);

      if (!string.IsNullOrEmpty(whereClause)) {
        sql.Append(" WHERE ").Append(whereClause);
      }

      if (!string.IsNullOrEmpty(orderByClause)) {
        sql.Append(" ORDER BY ").Append(orderByClause);
      }

      // Add pagination if supported by the database provider
      if (limit.HasValue || offset.HasValue) {
        // This is a simplified version - actual implementation should be database specific
        if (offset.HasValue) {
          sql.Append(" OFFSET ").Append(offset.Value).Append(" ROWS");
        }
        if (limit.HasValue) {
          sql.Append(" FETCH NEXT ").Append(limit.Value).Append(" ROWS ONLY");
        }
      }

      return sql.ToString();
    }

    private string BuildCountSql(string whereClause = null) {
      StringBuilder sql = new StringBuilder();
      sql.Append("SELECT COUNT(*) FROM ").Append(SchemaAndTableName);

      if (!string.IsNullOrEmpty(whereClause)) {
        sql.Append(" WHERE ").Append(whereClause);
      }

      return sql.ToString();
    }

    private string BuildInsertSql(Dictionary<string, object> fields) {
      StringBuilder columnNames = new StringBuilder();
      StringBuilder parameterNames = new StringBuilder();

      bool first = true;
      foreach (var field in fields) {

        if (
          PrimaryKeySet.Count == 1 && PrimaryKeySet.Any(
            f => String.Compare(f.Name, field.Key, StringComparison.OrdinalIgnoreCase) == 0
          )
        ) {
          if (field.Value is int) {
            if ((int)field.Value == 0) {
              continue; // Skip primary key fields with default value
            }
          }
          continue; // Skip primary key fields
        }

        if (!first) {
          columnNames.Append(", ");
          parameterNames.Append(", ");
        }
        columnNames.Append(field.Key);
        parameterNames.Append("@").Append(field.Key);
        first = false;
      }

      return $"INSERT INTO {SchemaAndTableName} ({columnNames}) VALUES ({parameterNames}) SELECT SCOPE_IDENTITY()";
    }

    private string BuildUpdateSql(Dictionary<string, object> fields, string whereClause) {
      StringBuilder setClause = new StringBuilder();

      bool first = true;
      foreach (var field in fields) {
        if (!first) {
          setClause.Append(", ");
        }
        setClause.Append(field.Key).Append(" = @").Append(field.Key);
        first = false;
      }

      return $"UPDATE {SchemaAndTableName} SET {setClause} WHERE {whereClause}";
    }

    private string BuildDeleteSql(string whereClause) {
      return $"DELETE FROM {SchemaAndTableName} WHERE {whereClause}";
    }

    private string BuildWhereClauseForKey(TKey key) {
      StringBuilder whereClause = new StringBuilder();
      object[] keyValues = key.GetKeyFieldValues();

      bool first = true;
      for (int i = 0; i < PrimaryKeySet.Count; i++) {
        if (!first) {
          whereClause.Append(" AND ");
        }
        //if (useAlias) {
        //  whereClause.Append("t0.");
        //}
        whereClause.Append(PrimaryKeySet[i].Name).Append(" = @").Append(PrimaryKeySet[i].Name + "0");
        first = false;
      }

      return whereClause.ToString();
    }

    private string BuildWhereClauseForKeys(TKey[] keys) {
      if (keys.Length == 0) {
        return "1 = 0"; // No keys, no matches
      }

      StringBuilder whereClause = new StringBuilder();

      // For a single key, use simple equality
      if (keys.Length == 1) {
        return BuildWhereClauseForKey(keys[0]);
      }

      // For multiple keys, use IN clause or composite key logic
      if (PrimaryKeySet.Count == 1) {
        // Simple primary key - use IN clause
        whereClause.Append(PrimaryKeySet[0].Name).Append(" IN (");
        for (int i = 0; i < keys.Length; i++) {
          if (i > 0) whereClause.Append(", ");
          whereClause.Append("@").Append(PrimaryKeySet[0].Name).Append(i);
        }
        whereClause.Append(")");
      } else {
        // Composite primary key - use OR with AND clauses
        whereClause.Append("(");
        for (int keyIndex = 0; keyIndex < keys.Length; keyIndex++) {
          if (keyIndex > 0) whereClause.Append(" OR ");
          whereClause.Append("(");

          bool first = true;
          for (int i = 0; i < PrimaryKeySet.Count; i++) {
            if (!first) whereClause.Append(" AND ");
            whereClause.Append(PrimaryKeySet[i].Name).Append(" = @")
                       .Append(PrimaryKeySet[i].Name).Append(keyIndex);
            first = false;
          }

          whereClause.Append(")");
        }
        whereClause.Append(")");
      }

      return whereClause.ToString();
    }

    private void AddKeyParameters(IDbCommand command, TKey key, string suffix = "") {
      object[] keyValues = key.GetKeyFieldValues();

      for (int i = 0; i < PrimaryKeySet.Count; i++) {
        var param = command.CreateParameter();
        param.ParameterName = "@" + PrimaryKeySet[i].Name + suffix + "0";
        param.Value = keyValues[i] ?? DBNull.Value;
        command.Parameters.Add(param);
      }
    }

    private void AddKeysParameters(IDbCommand command, TKey[] keys) {
      if (PrimaryKeySet.Count == 1) {
        // Simple primary key
        for (int i = 0; i < keys.Length; i++) {
          var param = command.CreateParameter();
          param.ParameterName = "@" + PrimaryKeySet[0].Name + i;
          param.Value = keys[i].GetKeyFieldValues()[0] ?? DBNull.Value;
          command.Parameters.Add(param);
        }
      } else {
        // Composite primary key
        for (int keyIndex = 0; keyIndex < keys.Length; keyIndex++) {
          object[] keyValues = keys[keyIndex].GetKeyFieldValues();

          for (int i = 0; i < PrimaryKeySet.Count; i++) {
            var param = command.CreateParameter();
            param.ParameterName = "@" + PrimaryKeySet[i].Name + keyIndex;
            param.Value = keyValues[i] ?? DBNull.Value;
            command.Parameters.Add(param);
          }
        }
      }
    }

    private void AddFieldParameters(IDbCommand command, Dictionary<string, object> fields) {
      foreach (var field in fields) {
        var param = command.CreateParameter();
        param.ParameterName = "@" + field.Key;
        param.Value = field.Value ?? DBNull.Value;
        command.Parameters.Add(param);
      }
    }

    private TEntity DataRowToEntity(IDataReader reader) {
      TEntity entity = Activator.CreateInstance<TEntity>();

      NavigationInfo[] navigationInfos = GetNavigationInfo(typeof(TEntity));


      Dictionary<int, object> navigationObjectsByIndex = new Dictionary<int, object>();

      for (int i = 0; i < reader.FieldCount; i++) {
        string columnName = reader.GetName(i);

        if (columnName.StartsWith("nav_")) {
          int indexOfDot = columnName.IndexOf('.');
          string navIndexString = columnName.Substring(4, indexOfDot - 4);
          int navIndex = int.Parse(navIndexString) - 1;
          string fieldName = columnName.Substring(indexOfDot + 1);

          NavigationInfo navigationInfo = navigationInfos[navIndex];
          object navigationObject;
          if (!navigationObjectsByIndex.ContainsKey(navIndex)) {
            navigationObject = Activator.CreateInstance(navigationInfo.NavigationProperty.PropertyType);
            navigationObjectsByIndex.Add(navIndex, navigationObject);
            navigationInfo.NavigationProperty.SetValue(entity, navigationObject);
          } else {
            navigationObject = navigationObjectsByIndex[navIndex];
          }
          object value = reader.IsDBNull(i) ? null : reader.GetValue(i);
          if (typeof(EntityRef).IsAssignableFrom(navigationObject.GetType())) {
            EntityRef entityRef = (EntityRef)navigationObject;

            if (navigationInfo.LabelField != null && fieldName == navigationInfo.LabelField.Name) {
              entityRef.Label = value == null ? "null" : value.ToString();
            } else {
              var keyFields = navigationInfo.Fields.Where((f) => f.Name != navigationInfo.LabelField?.Name);
              if (keyFields.Count() == 1) {
                entityRef.Key = value;
              } else {
                //TODO
              }
            }
          } else {
            PropertyInfo fieldProperty = navigationInfo.NavigationProperty.PropertyType.GetProperty(fieldName);
            if (fieldProperty != null) {


              if (value != null && fieldProperty.PropertyType != value.GetType()) {
                value = ConvertToPropertyType(value, fieldProperty.PropertyType);
              }
              fieldProperty.SetValue(navigationObject, value);
            }
          }

        }

        PropertyInfo prop = typeof(TEntity).GetProperty(columnName);

        if (prop != null && prop.CanWrite) {
          object value = reader.IsDBNull(i) ? null : reader.GetValue(i);

          if (value != null && prop.PropertyType != value.GetType()) {
            value = ConvertToPropertyType(value, prop.PropertyType);
          }

          prop.SetValue(entity, value);
        }
      }

      return entity;
    }

    private Dictionary<string, object> DataRowToDictionary(IDataReader reader, string[] includedFieldNames) {
      Dictionary<string, object> dict = new Dictionary<string, object>();

      foreach (string fieldName in includedFieldNames) {
        int ordinal;
        try {
          ordinal = reader.GetOrdinal(fieldName);
        } catch (IndexOutOfRangeException) {
          continue; // Field not found in result set
        }

        object value = reader.IsDBNull(ordinal) ? null : reader.GetValue(ordinal);
        dict[fieldName] = value;
      }

      return dict;
    }

    private object ConvertToPropertyType(object value, Type targetType) {
      if (value == null) return null;
      if (targetType == null) return value;

      if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>)) {
        targetType = Nullable.GetUnderlyingType(targetType);
      }

      if (value.GetType() == targetType) return value;

      try {

        // If direct conversion fails, try more specialized conversions
        if (targetType == typeof(Guid) && value is string) {
          return Guid.Parse((string)value);
        }

        if (targetType == typeof(DateTime) && value is string) {
          return DateTime.Parse((string)value);
        }

        // Handle enums 
        if (targetType.IsEnum) {
          if (value is string strValue) {
            return Enum.Parse(targetType, strValue, ignoreCase: true);
          } else {
            return Enum.ToObject(targetType, value);
          }
        }

        return Convert.ChangeType(value, targetType);
      } catch {

        // Return null if conversion is not possible
        return null;
      }
    }

    private string TranslateExpressionTreeToSql(ExpressionTree filter) {
      // This is a simplified implementation
      // A real implementation would need to translate the expression tree to SQL
      // For now, we'll just use the ToString() method as a placeholder
      if (filter == null) return null;
      return filter.CompileToSqlWhere(_SchemaRoot.GetSchema(typeof(TEntity).Name));
    }

    private string TranslateSearchExpressionToSql(string searchExpression) {
      if (string.IsNullOrWhiteSpace(searchExpression)) return null;

      string s = searchExpression.Trim();

      // Step 1: Convert double-quoted string literals to single-quoted SQL literals,
      // escaping single quotes inside the literal.
      s = Regex.Replace(s, "\"([^\"]*)\"", match => {
        var inner = match.Groups[1].Value.Replace("'", "''");
        return $"'{inner}'";
      });

      // Normalize logical operators
      s = s.Replace("&&", " AND ");
      s = s.Replace("||", " OR ");

      // Handle null comparisons: = null / == null  -> IS NULL, != null -> IS NOT NULL
      s = Regex.Replace(s, @"\b([A-Za-z0-9_\.]+)\s*(?:==|=)\s*null\b", @"$1 IS NULL", RegexOptions.IgnoreCase);
      s = Regex.Replace(s, @"\b([A-Za-z0-9_\.]+)\s*!=\s*null\b", @"$1 IS NOT NULL", RegexOptions.IgnoreCase);
      s = Regex.Replace(s, @"\b([A-Za-z0-9_\.]+)\s*<>\s*null\b", @"$1 IS NOT NULL", RegexOptions.IgnoreCase);

      // Handle string methods when argument is a literal:
      // Field.Contains('x') => (Field LIKE '%' + 'x' + '%')
      s = Regex.Replace(s, @"\b(?<field>[A-Za-z0-9_\.]+)\.Contains\(\s*(?<arg>'[^']*')\s*\)",
        m => $"({m.Groups["field"].Value} LIKE '%' + {m.Groups["arg"].Value} + '%')");

      // Field.StartsWith('x') => (Field LIKE 'x' + '%')
      s = Regex.Replace(s, @"\b(?<field>[A-Za-z0-9_\.]+)\.StartsWith\(\s*(?<arg>'[^']*')\s*\)",
        m => $"({m.Groups["field"].Value} LIKE {m.Groups["arg"].Value} + '%')");

      // Field.EndsWith('x') => (Field LIKE '%' + 'x')
      s = Regex.Replace(s, @"\b(?<field>[A-Za-z0-9_\.]+)\.EndsWith\(\s*(?<arg>'[^']*')\s*\)",
        m => $"({m.Groups["field"].Value} LIKE '%' + {m.Groups["arg"].Value})");

      // Handle inequality operators
      s = Regex.Replace(s, @"!=", " <> ");

      // Normalize equality operator (handle both == and single = with spaces)
      s = Regex.Replace(s, @"==", " = ");
      s = Regex.Replace(s, @"(?<!=)\s=\s(?!=)", " = ");

      // Convert boolean literals true/false to SQL BIT constants 1/0
      // Use word boundaries to avoid touching text inside strings (strings already quoted)
      s = Regex.Replace(s, @"\btrue\b", "1", RegexOptions.IgnoreCase);
      s = Regex.Replace(s, @"\bfalse\b", "0", RegexOptions.IgnoreCase);

      // Trim redundant whitespace
      s = Regex.Replace(s, @"\s+", " ").Trim();

      return s;
    }

    private string BuildOrderByClause(string[] sortedBy) {
      if (sortedBy == null || sortedBy.Length == 0) {
        return "1";
      }

      StringBuilder orderByClause = new StringBuilder();

      for (int i = 0; i < sortedBy.Length; i++) {
        string sortField = sortedBy[i];

        if (i > 0) {
          orderByClause.Append(", ");
        }

        if (sortField.StartsWith("^")) {
          string descSortField = sortField.Substring(1); // Remove the "^" prefix
          orderByClause.Append(descSortField).Append(" DESC");
        } else {
          orderByClause.Append(sortField).Append(" ASC");
        }
      }

      return orderByClause.ToString();
    }

    private Dictionary<string, object> ExtractFieldsFromEntity(TEntity entity) {
      Dictionary<string, object> fields = new Dictionary<string, object>();

      EntitySchema entitySchema = GetSchemaRoot().GetSchema(typeof(TEntity).Name);
      foreach (PropertyInfo prop in typeof(TEntity).GetProperties()) {
        FieldSchema fieldSchema = entitySchema.Fields.FirstOrDefault(
          f => String.Compare(f.Name, prop.Name, StringComparison.OrdinalIgnoreCase) == 0
        );
        if (fieldSchema == null) {
          continue; // Skip properties not in the schema
        }
        if (fieldSchema.SetabilityFlags == (int)Setability.Never) {
          continue; // Skip read-only fields
        }
        if (prop.CanRead) {
          fields[prop.Name] = prop.GetValue(entity);
        }
      }

      return fields;
    }

    private Dictionary<string, object> ExtractNonKeyFieldsFromEntity(TEntity entity) {
      Dictionary<string, object> fields = ExtractFieldsFromEntity(entity);

      // Remove key fields
      foreach (PropertyInfo keyProp in PrimaryKeySet) {
        fields.Remove(keyProp.Name);
      }

      return fields;
    }

    #endregion

    #region " IRepository Implementation "

    public TEntity AddOrUpdateEntity(TEntity entity) {
      return _ConnectionProvider.VisitCurrentConnection((connection) => {
        // Check if entity exists using primary key
        object[] keyValues = entity.GetValues(PrimaryKeySet);
        bool entityExists = false;
        TEntity existingEntity = null;

        // Try to find by primary key first
        using (var command = connection.CreateCommand()) {
          command.CommandText = BuildSelectSql(false, BuildWhereClauseForKey(keyValues.ToKey<TKey>()));
          AddKeyParameters(command, keyValues.ToKey<TKey>());

          using (var reader = command.ExecuteReader()) {
            if (reader.Read()) {
              entityExists = true;
              existingEntity = DataRowToEntity(reader);
            }
          }
        }

        // If not found by primary key, try unique keys
        if (!entityExists) {
          foreach (var keyset in Keysets) {
            object[] keysetValues = entity.GetValues(keyset);

            using (var command = connection.CreateCommand()) {
              StringBuilder whereClause = new StringBuilder();

              bool first = true;
              for (int i = 0; i < keyset.Count; i++) {
                if (!first) {
                  whereClause.Append(" AND ");
                }
                whereClause.Append(keyset[i].Name).Append(" = @").Append(keyset[i].Name);
                first = false;
              }

              command.CommandText = BuildSelectSql(false, whereClause.ToString());

              for (int i = 0; i < keyset.Count; i++) {
                var param = command.CreateParameter();
                param.ParameterName = "@" + keyset[i].Name;
                param.Value = keysetValues[i] ?? DBNull.Value;
                command.Parameters.Add(param);
              }

              using (var reader = command.ExecuteReader()) {
                if (reader.Read()) {
                  entityExists = true;
                  existingEntity = DataRowToEntity(reader);
                  break;
                }
              }
            }

            if (entityExists) break;
          }
        }

        // Perform insert or update
        if (entityExists) {
          // Update existing entity
          Dictionary<string, object> fields = ExtractNonKeyFieldsFromEntity(entity);

          using (var command = connection.CreateCommand()) {
            command.CommandText = BuildUpdateSql(fields, BuildWhereClauseForKey(existingEntity.GetValues(PrimaryKeySet).ToKey<TKey>()));
            AddFieldParameters(command, fields);
            AddKeyParameters(command, existingEntity.GetValues(PrimaryKeySet).ToKey<TKey>());

            command.ExecuteNonQuery();
          }

          // Update the fields of existingEntity with the new values
          foreach (var field in fields) {
            PropertyInfo prop = typeof(TEntity).GetProperty(field.Key);
            if (prop != null && prop.CanWrite) {
              prop.SetValue(existingEntity, field.Value);
            }
          }

          return existingEntity;
        } else {
          // Insert new entity
          Dictionary<string, object> fields = ExtractFieldsFromEntity(entity);

          using (var command = connection.CreateCommand()) {
            command.CommandText = BuildInsertSql(fields);
            AddFieldParameters(command, fields);

            var insertedId = command.ExecuteScalar();

            if (insertedId != null) {
              if (PrimaryKeySet.Count == 1 && PrimaryKeySet[0].PropertyType == typeof(int)) {
                insertedId = Convert.ToInt32(insertedId);
                PropertyInfo keyProp = typeof(TEntity).GetProperty(PrimaryKeySet[0].Name);
                if (keyProp != null && keyProp.CanWrite) {
                  keyProp.SetValue(entity, insertedId);
                }
              }

            }
          }
          return entity;
        }
      });
    }

    public Dictionary<string, object> AddOrUpdateEntityFields(Dictionary<string, object> fields) {
      return _ConnectionProvider.VisitCurrentConnection<Dictionary<string, object>>((connection) => {
        // Check for existing entity by primary key
        object[] primaryKeyValues = fields.TryGetValuesByFields(PrimaryKeySet);
        bool entityExists = false;
        TEntity existingEntity = null;

        // Try to find by primary key if values are provided
        if (primaryKeyValues != null) {
          using (var command = connection.CreateCommand()) {
            command.CommandText = BuildSelectSql(false, BuildWhereClauseForKey(primaryKeyValues.ToKey<TKey>()));
            AddKeyParameters(command, primaryKeyValues.ToKey<TKey>());

            using (var reader = command.ExecuteReader()) {
              if (reader.Read()) {
                entityExists = true;
                existingEntity = DataRowToEntity(reader);
              }
            }
          }
        }

        // If not found by primary key, check unique keys
        if (!entityExists) {
          foreach (var keyset in Keysets) {
            object[] keysetValues = fields.GetValuesFromDictionary(keyset);

            if (keysetValues != null) {
              using (var command = connection.CreateCommand()) {
                StringBuilder whereClause = new StringBuilder();

                bool first = true;
                for (int i = 0; i < keyset.Count; i++) {
                  if (!first) {
                    whereClause.Append(" AND ");
                  }
                  whereClause.Append(keyset[i].Name).Append(" = @").Append(keyset[i].Name);
                  first = false;
                }

                command.CommandText = BuildSelectSql(false, whereClause.ToString());

                for (int i = 0; i < keyset.Count; i++) {
                  var param = command.CreateParameter();
                  param.ParameterName = "@" + keyset[i].Name;
                  param.Value = keysetValues[i] ?? DBNull.Value;
                  command.Parameters.Add(param);
                }

                using (var reader = command.ExecuteReader()) {
                  if (reader.Read()) {
                    entityExists = true;
                    existingEntity = DataRowToEntity(reader);
                    break;
                  }
                }
              }
            }

            if (entityExists) break;
          }
        }

        // Perform insert or update
        if (entityExists) {
          // Create a dictionary of non-key fields for update
          Dictionary<string, object> updateFields = new Dictionary<string, object>(fields);
          foreach (PropertyInfo keyProp in PrimaryKeySet) {
            updateFields.Remove(keyProp.Name);
          }

          // Update existing entity
          using (var command = connection.CreateCommand()) {
            command.CommandText = BuildUpdateSql(updateFields, BuildWhereClauseForKey(existingEntity.GetValues(PrimaryKeySet).ToKey<TKey>()));
            AddFieldParameters(command, updateFields);
            AddKeyParameters(command, existingEntity.GetValues(PrimaryKeySet).ToKey<TKey>());

            command.ExecuteNonQuery();
          }

          // Update the fields of existingEntity with the new values
          foreach (var field in updateFields) {
            PropertyInfo prop = typeof(TEntity).GetProperty(field.Key);
            if (prop != null && prop.CanWrite) {
              prop.SetValue(existingEntity, field.Value);
            }
          }
        } else {
          // Insert new entity
          using (var command = connection.CreateCommand()) {
            command.CommandText = BuildInsertSql(fields);
            AddFieldParameters(command, fields);

            command.ExecuteNonQuery();
          }

          // Create a new entity with the inserted fields
          existingEntity = Activator.CreateInstance<TEntity>();
          foreach (var field in fields) {
            PropertyInfo prop = typeof(TEntity).GetProperty(field.Key);
            if (prop != null && prop.CanWrite) {
              prop.SetValue(existingEntity, field.Value);
            }
          }
        }

        // Create the dictionary of conflicting fields
        Dictionary<string, object> conflictingFields = new Dictionary<string, object>();
        foreach (var propertyInfo in typeof(TEntity).GetProperties()) {
          var updatedValue = propertyInfo.GetValue(existingEntity);
          if (
            !fields.TryGetValue(propertyInfo.Name, out var originalValue) ||
            !Equals(updatedValue, originalValue)
          ) {
            conflictingFields[propertyInfo.Name] = updatedValue;
          }
        }

        return conflictingFields;
      });
    }

    public bool ContainsKey(TKey key) {
      return _ConnectionProvider.VisitCurrentConnection((connection) => {
        using (var command = connection.CreateCommand()) {
          command.CommandText = BuildCountSql(BuildWhereClauseForKey(key));
          AddKeyParameters(command, key);

          return Convert.ToInt32(command.ExecuteScalar()) > 0;
        }
      });
    }

    public int Count(ExpressionTree filter) {
      return _ConnectionProvider.VisitCurrentConnection((connection) => {
        using (var command = connection.CreateCommand()) {
          command.CommandText = BuildCountSql(TranslateExpressionTreeToSql(filter));

          return Convert.ToInt32(command.ExecuteScalar());
        }
      });
    }

    public int CountAll() {
      return _ConnectionProvider.VisitCurrentConnection((connection) => {
        using (var command = connection.CreateCommand()) {
          command.CommandText = BuildCountSql();

          return Convert.ToInt32(command.ExecuteScalar());
        }
      });
    }

    public int CountBySearchExpression(string searchExpression) {
      return _ConnectionProvider.VisitCurrentConnection((connection) => {
        using (var command = connection.CreateCommand()) {
          command.CommandText = BuildCountSql(TranslateSearchExpressionToSql(searchExpression));

          return Convert.ToInt32(command.ExecuteScalar());
        }
      });
    }

    public RepositoryCapabilities GetCapabilities() {
      return RepositoryCapabilities.All;
    }

    public TEntity[] GetEntities(ExpressionTree filter, string[] sortedBy, int limit = 100, int skip = 0) {
      return _ConnectionProvider.VisitCurrentConnection((connection) => {
        List<TEntity> result = new List<TEntity>();

        using (var command = connection.CreateCommand()) {
          command.CommandText = BuildSelectSql(
            true,
            TranslateExpressionTreeToSql(filter),
            BuildOrderByClause(sortedBy),
            limit,
            skip
          );

          using (var reader = command.ExecuteReader()) {
            while (reader.Read()) {
              result.Add(DataRowToEntity(reader));
            }
          }
        }

        return result.ToArray();
      });
    }

    public TEntity[] GetEntitiesByKey(TKey[] keysToLoad) {
      return _ConnectionProvider.VisitCurrentConnection((connection) => {
        if (keysToLoad.Length == 0) {
          return new TEntity[0];
        }

        List<TEntity> result = new List<TEntity>();

        using (var command = connection.CreateCommand()) {
          command.CommandText = BuildSelectSql(true, BuildWhereClauseForKeys(keysToLoad));
          AddKeysParameters(command, keysToLoad);

          using (var reader = command.ExecuteReader()) {
            while (reader.Read()) {
              result.Add(DataRowToEntity(reader));
            }
          }
        }

        return result.ToArray();
      });
    }

    public TEntity[] GetEntitiesBySearchExpression(string searchExpression, string[] sortedBy, int limit = 100, int skip = 0) {
      return _ConnectionProvider.VisitCurrentConnection((connection) => {
        List<TEntity> result = new List<TEntity>();

        using (var command = connection.CreateCommand()) {
          command.CommandText = BuildSelectSql(
            true,
            TranslateSearchExpressionToSql(searchExpression),
            BuildOrderByClause(sortedBy),
            limit,
            skip
          );

          using (var reader = command.ExecuteReader()) {
            while (reader.Read()) {
              result.Add(DataRowToEntity(reader));
            }
          }
        }

        return result.ToArray();
      });
    }

    public Dictionary<string, object>[] GetEntityFields(ExpressionTree filter, string[] includedFieldNames, string[] sortedBy, int limit = 100, int skip = 0) {
      return _ConnectionProvider.VisitCurrentConnection((connection) => {
        List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

        using (var command = connection.CreateCommand()) {
          command.CommandText = BuildSelectFieldsSql(
            includedFieldNames,
            TranslateExpressionTreeToSql(filter),
            BuildOrderByClause(sortedBy),
            limit,
            skip
          );

          using (var reader = command.ExecuteReader()) {
            while (reader.Read()) {
              result.Add(DataRowToDictionary(reader, includedFieldNames));
            }
          }
        }

        return result.ToArray();
      });
    }

    public EntityRef<TKey>[] GetEntityRefs(ExpressionTree filter, string[] sortedBy, int limit = 100, int skip = 0) {
      return _ConnectionProvider.VisitCurrentConnection((connection) => {
        TEntity[] entities = GetEntities(filter, sortedBy, limit, skip);

        return entities.Select(e => new EntityRef<TKey>(
          e.GetValues(PrimaryKeySet).ToKey<TKey>(),
          ConversionHelper.GetLabel(e, this.GetSchemaRoot())
        )).ToArray();
      });
    }

    public EntityRef<TKey>[] GetEntityRefsByKey(TKey[] keysToLoad) {
      return _ConnectionProvider.VisitCurrentConnection((connection) => {
        TEntity[] entities = GetEntitiesByKey(keysToLoad);

        return entities.Select(e => new EntityRef<TKey>(
          e.GetValues(PrimaryKeySet).ToKey<TKey>(),
          ConversionHelper.GetLabel(e, this.GetSchemaRoot())
        )).ToArray();
      });
    }

    public EntityRef<TKey>[] GetEntityRefsBySearchExpression(string searchExpression, string[] sortedBy, int limit = 100, int skip = 0) {
      return _ConnectionProvider.VisitCurrentConnection((connection) => {
        TEntity[] entities = GetEntitiesBySearchExpression(searchExpression, sortedBy, limit, skip);

        return entities.Select(e => new EntityRef<TKey>(
          e.GetValues(PrimaryKeySet).ToKey<TKey>(),
          ConversionHelper.GetLabel(e, this.GetSchemaRoot())
        )).ToArray();
      });
    }

    public string GetOriginIdentity() {
      return $"SqlRepository {typeof(TEntity).Name}";
    }

    public TKey[] Massupdate(ExpressionTree filter, Dictionary<string, object> fields) {
      return MassupdateBySearchExpression(TranslateExpressionTreeToSql(filter), fields);
    }

    public TKey[] MassupdateByKeys(TKey[] keysToUpdate, Dictionary<string, object> fields) {
      return _ConnectionProvider.VisitCurrentConnection((connection) => {
        if (keysToUpdate.Length == 0) {
          return new TKey[0];
        }

        // Ensure that the fields to be updated do not include any key fields
        var keyFieldNames = PrimaryKeySet.Select(p => p.Name);
        if (fields.Keys.Intersect(keyFieldNames).Any()) {
          throw new ArgumentException("Update fields must not contain key fields.");
        }

        // First, retrieve all entities that will be updated to get their keys
        TEntity[] entitiesToUpdate = GetEntitiesByKey(keysToUpdate);

        // If no entities found, return empty array
        if (entitiesToUpdate.Length == 0) {
          return new TKey[0];
        }

        // Perform the update
        using (var command = connection.CreateCommand()) {
          command.CommandText = BuildUpdateSql(fields, BuildWhereClauseForKeys(keysToUpdate));
          AddFieldParameters(command, fields);
          AddKeysParameters(command, keysToUpdate);

          command.ExecuteNonQuery();
        }

        // Return the keys of the updated entities
        return entitiesToUpdate.Select(e => e.GetValues(PrimaryKeySet).ToKey<TKey>()).ToArray();
      });
    }

    public TKey[] MassupdateBySearchExpression(string searchExpression, Dictionary<string, object> fields) {
      return _ConnectionProvider.VisitCurrentConnection((connection) => {
        // Ensure that the fields to be updated do not include any key fields
        var keyFieldNames = PrimaryKeySet.Select(p => p.Name);
        if (fields.Keys.Intersect(keyFieldNames).Any()) {
          throw new ArgumentException("Update fields must not contain key fields.");
        }

        // First, retrieve all entities that will be updated to get their keys
        TEntity[] entitiesToUpdate = GetEntitiesBySearchExpression(searchExpression, null);

        // If no entities found, return empty array
        if (entitiesToUpdate.Length == 0) {
          return new TKey[0];
        }

        // Perform the update
        using (var command = connection.CreateCommand()) {
          command.CommandText = BuildUpdateSql(fields, TranslateSearchExpressionToSql(searchExpression));
          AddFieldParameters(command, fields);

          command.ExecuteNonQuery();
        }

        // Return the keys of the updated entities
        return entitiesToUpdate.Select(e => e.GetValues(PrimaryKeySet).ToKey<TKey>()).ToArray();
      });
    }

    public TKey TryAddEntity(TEntity entity) {
      return _ConnectionProvider.VisitCurrentConnection((connection) => {
        try {
          // Check if the entity already exists
          object[] keySetValues = entity.GetValues(PrimaryKeySet);
          bool entityExists = false;

          using (var command = connection.CreateCommand()) {
            command.CommandText = BuildCountSql(BuildWhereClauseForKey(keySetValues.ToKey<TKey>()));
            AddKeyParameters(command, keySetValues.ToKey<TKey>());

            entityExists = Convert.ToInt32(command.ExecuteScalar()) > 0;
          }

          // If the entity does not exist, add it
          if (!entityExists) {
            Dictionary<string, object> fields = ExtractFieldsFromEntity(entity);

            using (var command = connection.CreateCommand()) {
              command.CommandText = BuildInsertSql(fields);
              AddFieldParameters(command, fields);

              command.ExecuteNonQuery();
            }

            return entity.GetValues(PrimaryKeySet).ToKey<TKey>();
          }
        } catch (Exception) {
          // Ignore exceptions and return default(TKey)
        }

        // If the entity already exists or if an error occurs, return default(TKey)
        return default(TKey);
      });
    }

    public TKey[] TryDeleteEntities(TKey[] keysToDelete) {
      return _ConnectionProvider.VisitCurrentConnection((connection) => {
        List<TKey> deletedKeys = new List<TKey>();

        foreach (var key in keysToDelete) {
          try {
            // Check if the entity exists
            bool entityExists = false;

            using (var command = connection.CreateCommand()) {
              command.CommandText = BuildCountSql(BuildWhereClauseForKey(key));
              AddKeyParameters(command, key);

              entityExists = Convert.ToInt32(command.ExecuteScalar()) > 0;
            }

            // If the entity exists, delete it
            if (entityExists) {
              using (var command = connection.CreateCommand()) {
                command.CommandText = BuildDeleteSql(BuildWhereClauseForKey(key));
                AddKeyParameters(command, key);

                command.ExecuteNonQuery();
                deletedKeys.Add(key);
              }
            }
          } catch (Exception) {
            // Ignore exceptions and continue with the next key
          }
        }

        return deletedKeys.ToArray();
      });
    }

    public TEntity TryUpdateEntity(TEntity entity) {
      return _ConnectionProvider.VisitCurrentConnection((connection) => {
        try {
          // Check if the entity exists
          object[] keySetValues = entity.GetValues(PrimaryKeySet);
          TEntity existingEntity = null;

          using (var command = connection.CreateCommand()) {
            command.CommandText = BuildSelectSql(false, BuildWhereClauseForKey(keySetValues.ToKey<TKey>()));
            AddKeyParameters(command, keySetValues.ToKey<TKey>());

            using (var reader = command.ExecuteReader()) {
              if (reader.Read()) {
                existingEntity = DataRowToEntity(reader);
              }
            }
          }

          // If the entity exists, update it
          if (existingEntity != null) {
            Dictionary<string, object> fields = ExtractNonKeyFieldsFromEntity(entity);

            using (var command = connection.CreateCommand()) {
              command.CommandText = BuildUpdateSql(fields, BuildWhereClauseForKey(keySetValues.ToKey<TKey>()));
              AddFieldParameters(command, fields);
              AddKeyParameters(command, keySetValues.ToKey<TKey>());

              command.ExecuteNonQuery();
            }

            // Update the fields of existingEntity with the new values
            foreach (var field in fields) {
              PropertyInfo prop = typeof(TEntity).GetProperty(field.Key);
              if (prop != null && prop.CanWrite) {
                prop.SetValue(existingEntity, field.Value);
              }
            }

            return existingEntity;
          }
        } catch (Exception) {
          // Ignore exceptions and return null
        }

        // If the entity does not exist or if an error occurs, return null
        return null;
      });
    }

    public Dictionary<string, object> TryUpdateEntityFields(Dictionary<string, object> fields) {
      // Check if the dictionary contains all the key fields
      var keyFieldNames = PrimaryKeySet.Select(p => p.Name);
      if (!keyFieldNames.All(k => fields.ContainsKey(k))) {
        throw new ArgumentException("The given dictionary must contain all the key fields.");
      }

      return _ConnectionProvider.VisitCurrentConnection((connection) => {
        try {
          // Check if the entity exists
          object[] keySetValues = fields.TryGetValuesByFields(PrimaryKeySet);
          TEntity existingEntity = null;

          using (var command = connection.CreateCommand()) {
            command.CommandText = BuildSelectSql(false, BuildWhereClauseForKey(keySetValues.ToKey<TKey>()));
            AddKeyParameters(command, keySetValues.ToKey<TKey>());

            using (var reader = command.ExecuteReader()) {
              if (reader.Read()) {
                existingEntity = DataRowToEntity(reader);
              }
            }
          }

          // If the entity exists, update its fields
          if (existingEntity != null) {
            // Create a dictionary of non-key fields for update
            Dictionary<string, object> updateFields = new Dictionary<string, object>(fields);
            foreach (var keyField in keyFieldNames) {
              updateFields.Remove(keyField);
            }

            using (var command = connection.CreateCommand()) {
              command.CommandText = BuildUpdateSql(updateFields, BuildWhereClauseForKey(keySetValues.ToKey<TKey>()));
              AddFieldParameters(command, updateFields);
              AddKeyParameters(command, keySetValues.ToKey<TKey>());

              command.ExecuteNonQuery();
            }

            // Retrieve the updated entity to get any database-generated values
            using (var command = connection.CreateCommand()) {
              command.CommandText = BuildSelectSql(false, BuildWhereClauseForKey(keySetValues.ToKey<TKey>()));
              AddKeyParameters(command, keySetValues.ToKey<TKey>());

              using (var reader = command.ExecuteReader()) {
                if (reader.Read()) {
                  existingEntity = DataRowToEntity(reader);
                }
              }
            }

            // Build a dictionary of the fields that are different from the given values
            // Also include properties present on the existingEntity but not present in the given fields
            Dictionary<string, object> conflictingFields = new Dictionary<string, object>();
            foreach (var propertyInfo in typeof(TEntity).GetProperties()) {
              var updatedValue = propertyInfo.GetValue(existingEntity);
              if (
                !fields.TryGetValue(propertyInfo.Name, out var originalValue) ||
                !Equals(updatedValue, originalValue)
              ) {
                conflictingFields[propertyInfo.Name] = updatedValue;
              }
            }

            return conflictingFields;
          }
        } catch (Exception) {
          // Ignore exceptions and return null
        }

        // If the entity does not exist or if an error occurs, return null
        return null;
      });
    }

    public bool TryUpdateKey(TKey currentKey, TKey newKey) {
      return _ConnectionProvider.VisitCurrentConnection((connection) => {
        try {
          // Check if the entity with the current key exists
          bool currentEntityExists = false;

          using (var command = connection.CreateCommand()) {
            command.CommandText = BuildCountSql(BuildWhereClauseForKey(currentKey));
            AddKeyParameters(command, currentKey);

            currentEntityExists = Convert.ToInt32(command.ExecuteScalar()) > 0;
          }

          // Check if the entity with the new key already exists
          bool newEntityExists = false;

          using (var command = connection.CreateCommand()) {
            command.CommandText = BuildCountSql(BuildWhereClauseForKey(newKey));
            AddKeyParameters(command, newKey);

            newEntityExists = Convert.ToInt32(command.ExecuteScalar()) > 0;
          }

          // If the entity with the current key exists and the entity with the new key does not exist
          if (currentEntityExists && !newEntityExists) {
            // Get all fields of the entity with the current key
            TEntity entity = null;

            using (var command = connection.CreateCommand()) {
              command.CommandText = BuildSelectSql(false, BuildWhereClauseForKey(currentKey));
              AddKeyParameters(command, currentKey);

              using (var reader = command.ExecuteReader()) {
                if (reader.Read()) {
                  entity = DataRowToEntity(reader);
                }
              }
            }

            if (entity != null) {
              // Create a dictionary of all fields in the entity
              Dictionary<string, object> fields = ExtractFieldsFromEntity(entity);

              // Update the key fields with the new key values
              object[] newKeyValues = newKey.GetKeyFieldValues();
              for (int i = 0; i < PrimaryKeySet.Count; i++) {
                fields[PrimaryKeySet[i].Name] = newKeyValues[i];
              }

              // Begin a transaction for the key update
              using (var transaction = connection.BeginTransaction()) {
                try {
                  // Insert a new entity with the new key and all fields
                  using (var command = connection.CreateCommand()) {
                    command.Transaction = transaction;
                    command.CommandText = BuildInsertSql(fields);
                    AddFieldParameters(command, fields);

                    command.ExecuteNonQuery();
                  }

                  // Delete the entity with the old key
                  using (var command = connection.CreateCommand()) {
                    command.Transaction = transaction;
                    command.CommandText = BuildDeleteSql(BuildWhereClauseForKey(currentKey));
                    AddKeyParameters(command, currentKey);

                    command.ExecuteNonQuery();
                  }

                  transaction.Commit();
                  return true;
                } catch {
                  transaction.Rollback();
                  throw;
                }
              }
            }
          }
        } catch (Exception) {
          // Ignore exceptions and return false
        }

        return false;
      });
    }

    public Dictionary<string, object>[] GetEntityFieldsByKey(TKey[] keysToLoad, string[] includedFieldNames) {
      return _ConnectionProvider.VisitCurrentConnection((connection) => {
        if (keysToLoad.Length == 0) {
          return new Dictionary<string, object>[0];
        }

        List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

        using (var command = connection.CreateCommand()) {
          command.CommandText = BuildSelectFieldsSql(
            includedFieldNames,
            BuildWhereClauseForKeys(keysToLoad)
          );
          AddKeysParameters(command, keysToLoad);

          using (var reader = command.ExecuteReader()) {
            while (reader.Read()) {
              result.Add(DataRowToDictionary(reader, includedFieldNames));
            }
          }
        }

        return result.ToArray();
      });
    }

    public Dictionary<string, object>[] GetEntityFieldsBySearchExpression(string searchExpression, string[] includedFieldNames, string[] sortedBy, int limit = 100, int skip = 0) {
      return _ConnectionProvider.VisitCurrentConnection((connection) => {
        List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

        using (var command = connection.CreateCommand()) {
          command.CommandText = BuildSelectFieldsSql(
            includedFieldNames,
            TranslateSearchExpressionToSql(searchExpression),
            BuildOrderByClause(sortedBy),
            limit,
            skip
          );

          using (var reader = command.ExecuteReader()) {
            while (reader.Read()) {
              result.Add(DataRowToDictionary(reader, includedFieldNames));
            }
          }
        }
        return result.ToArray();
      });
    }

    #endregion

  }
}
