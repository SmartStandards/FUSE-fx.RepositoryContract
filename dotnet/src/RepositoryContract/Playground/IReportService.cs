#if NETCOREAPP
using System.Collections.Generic;
using System.Data.ModelDescription;

namespace System.Data.Fuse {

  public interface IReportService {

    ReportResponse GenerateReport(
      ExpressionTree filter, 
      string[]? groupBy = null,
      string[]? stackBy = null,
      string[]? reportValues = null,
      string[]? sortedBy = null,
      int limit = 500,
      int skip = 0
    );

    EntitySchema GetEntitySchema();
  }

  public interface IReportService<TEntity> : IReportService {
  }
}
#endif