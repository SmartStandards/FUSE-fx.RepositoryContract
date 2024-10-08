using System.Data.ModelDescription;

namespace System.Data.Fuse {

  /// <summary>
  /// (from 'FUSE-fx.RepositoryContract')
  /// </summary>
  public interface ISchemaProvider {

    SchemaRoot GetSchemaRoot();

  }

}