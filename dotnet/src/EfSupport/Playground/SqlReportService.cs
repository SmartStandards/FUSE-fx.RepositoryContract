#if NETCOREAPP
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data.Fuse.Convenience;
using System.Data.ModelDescription;
using System.Data.SqlClient;
using System.Data.Common;
using System.Linq;

namespace System.Data.Fuse.Ef {

  public class SqlReportService<TEntity> : IReportService<TEntity> {
    private readonly SchemaRoot schemaRoot;
    private readonly string connectionString;
    private readonly string entityName;

    public SqlReportService(string connectionString, string entityName) {
      this.schemaRoot = ModelReader.GetSchema(typeof(TEntity).Assembly, new string[] { typeof(TEntity).Name });
      this.connectionString = connectionString;
      this.entityName = entityName;
    }

    public ReportResponse GenerateReport(
      ExpressionTree filter,
      string[]? groupBy = null,
      string[]? stackBy = null,
      string[]? reportValues = null,
      string[]? sortedBy = null,
      int limit = 100,
      int skip = 0
    ) {

      string[]? groupByUnion = groupBy == null ? stackBy : stackBy == null ? groupBy : groupBy.Union(stackBy).ToArray();

      if ((groupByUnion == null || groupByUnion.Count() == 0) && (reportValues == null || reportValues.Count() == 0)) {
        reportValues = new string[] { "count(1)" };
      }

      string sqlFiter = filter.CompileToSqlWhere(schemaRoot.Entities.First((x) => x.Name == this.entityName));
      string sqlReportValues = reportValues == null ? "" : string.Join(",", reportValues);

      string groupByValues = (groupByUnion == null || groupByUnion.Count() == 0) ?
        "" :
        string.Join(",", groupByUnion.Select((g) => $"[{g}]"));
      string sqlGroupBy = "";
      if (!string.IsNullOrEmpty(groupByValues)) {
        sqlGroupBy = "GROUP BY " + groupByValues;
      }
      string whereClase = "";
      if (sqlFiter != null && sqlFiter != "") {
        whereClase = $"WHERE {sqlFiter}";
      }
      string values = groupByValues;
      if (!string.IsNullOrEmpty(values) && !string.IsNullOrEmpty(sqlReportValues)) {
        values += ",";
      }
      values += sqlReportValues;
      string sql = $@"
        SELECT {values}
        FROM dbo.Reports 
        {whereClase}  
        {sqlGroupBy}"; // {schemaRoot.MainEntity.Name}
      SqlConnection connection = new SqlConnection(connectionString);
      connection.Open();
      SqlCommand command = new SqlCommand(sql, connection);
      SqlDataReader reader = command.ExecuteReader();
      List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
      while (reader.Read()) {
        Dictionary<string, object> row = new Dictionary<string, object>();

        for (int i = 0; i < groupByUnion?.Count(); i++) {
          row[groupByUnion[i]] = reader.GetValue(i);
        }
        int groupByCount = groupByUnion?.Count() ?? 0;
        for (int i = 0; i < reportValues?.Count(); i++) {
          if (double.TryParse(reader.GetValue(groupByCount + i).ToString(), out double val)) {
            row[reportValues[i]] = Math.Round(val, 2);
          } else {
            row[reportValues[i]] = 0;
          }
        }
        result.Add(row);
      }
      int totalCount = result.Count();
      result = ApplyStackBy(result, groupBy, stackBy, reportValues);

      connection.Close();
      return new ReportResponse { Page = result.ToArray(), TotalCount = totalCount };
    }

    private List<Dictionary<string, object>> ApplyStackBy(
      List<Dictionary<string, object>> input, string[]? groupBy, string[]? stackBy, string[]? reportValues
    ) {
      if (groupBy == null || stackBy == null || reportValues == null) {
        return input;
      }
      if (groupBy.Count() == 0 || stackBy.Count() == 0 || reportValues.Count() == 0) {
        return input;
      }
      List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
      List<string> finalKeys = new List<string>();
      foreach (Dictionary<string, object> entry in input) {
        Dictionary<string, object>? existingRow = result.FirstOrDefault((x) => {
          bool result = true;
          foreach (string key in groupBy) {
            result = result && x[key].Equals(entry[key]);
          }
          return result;
        });
        if (existingRow == null) {
          existingRow = new Dictionary<string, object>();
          foreach (string key in groupBy) {
            existingRow[key] = entry[key];
          }
          foreach (string key in reportValues) {
            string stackedKey = $"{key}";
            double stackValue = 0;
            if (entry.ContainsKey(key) && entry[key] != null) {
              stackValue = double.Parse(entry[key].ToString()!);
            }
            foreach (string stackKey in stackBy) {
              string stackColumn = (string)entry[stackKey];
              stackedKey += $"({stackColumn})";
            }
            existingRow[stackedKey] = stackValue;
            if (!finalKeys.Contains(stackedKey)) {
              finalKeys.Add(stackedKey);
            }
          }
          result.Add(existingRow);
        } else {
          foreach (string key in reportValues) {
            string stackedKey = $"{key}";
            double stackValue = 0;
            if (entry.ContainsKey(key) && entry[key] != null) {
              stackValue = double.Parse(entry[key].ToString()!);
            }
            foreach (string stackKey in stackBy) {
              string stackColumn = (string)entry[stackKey];
              stackedKey += $"({stackColumn})";
            }
            existingRow[stackedKey] = stackValue;
            if (!finalKeys.Contains(stackedKey)) {
              finalKeys.Add(stackedKey);
            }
          }
        }
      }

      foreach (string key in finalKeys) {
        foreach (Dictionary<string, object> entry in result) {
          if (!entry.ContainsKey(key)) {
            entry[key] = 0;
          }
        }
      }
      return result;
    }

    public EntitySchema GetEntitySchema() {
      return this.schemaRoot.Entities.First((x) => x.Name == this.entityName);
    }
  }
}
#endif