using System.Diagnostics;
using System.Linq;

namespace System.Data.Fuse.SchemaResolving {

  /// <summary>
  /// Provides functionality to get a concrete Type from a entity name (as used in the Schema/ModelDescription).
  /// The 'ListBasedEntityResolver' provides the known types just by holding an array of known entities.
  /// (from 'FUSE-fx.RepositoryContract')
  /// </summary>
  [DebuggerDisplay("ListBasedEntityResolver")]
  public class ListBasedEntityResolver : IEntityResolver {

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    private Type[] _KnownEntityTypes = null;

    /// <summary>
    /// Provides functionality to get a concrete Type from a entity name (as used in the Schema/ModelDescription).
    /// The 'ListBasedEntityResolver' provides the known types just by holding an array of known entities.
    /// </summary>
    public ListBasedEntityResolver(params Type[] knownEntityTypes) {
      _KnownEntityTypes = knownEntityTypes;
    }

    /// <summary>
    /// Adds the given type to the list.
    /// Already present types will be skipped.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public void Add<TEntity>() {
      this.Add(typeof(TEntity));
    }

    /// <summary>
    /// Adds the given type(s) to the list.
    /// Already present types will be skipped.
    /// </summary>
    /// <param name="knownEntityTypes"></param>
    public void Add(params Type[] knownEntityTypes) {
      _KnownEntityTypes = _KnownEntityTypes.Union(knownEntityTypes).Distinct().ToArray();
    }

    public Type[] GetWellknownTypes() {
      return _KnownEntityTypes;
    }

    /// <summary>
    /// Runs a looup over the list of known entity types to find one with the given name.
    /// Otherwiese it will return null.
    /// </summary>
    /// <param name="entityTypeName">Name as provided from the Schema/ModelDescription</param>
    /// <returns></returns>
    public Type TryResolveEntityTypeByName(string entityTypeName) {

      if(_KnownEntityTypes == null) {
        return null;
      }

      foreach(Type t in _KnownEntityTypes) {
        if(t.Name.Equals(entityTypeName,StringComparison.CurrentCultureIgnoreCase)) { 
          return t;
        }
      }

      foreach (Type t in _KnownEntityTypes) {
        if (t.FullName.Equals(entityTypeName, StringComparison.CurrentCultureIgnoreCase)) {
          return t;
        }
      }

      return null;
    }

  }

}