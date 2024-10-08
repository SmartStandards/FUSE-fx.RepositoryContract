using System.Data.ModelDescription;

namespace System.Data.Fuse.SchemaResolving {

  /// <summary>
  /// Provides functionality to get a concrete Type from a entity name (as used in the Schema/ModelDescription).
  /// (from 'FUSE-fx.RepositoryContract')
  /// </summary>
  public interface IEntityResolver {

    /// <summary>
    /// Runs a looup over the list of known entity types to find one with the given name.
    /// Otherwiese it will return null.
    /// </summary>
    /// <param name="entityTypeName">Name as provided from the Schema/ModelDescription</param>
    /// <returns></returns>
    Type TryResolveEntityTypeByName(string entityTypeName);

    Type[] GetWellknownTypes();

  }

}