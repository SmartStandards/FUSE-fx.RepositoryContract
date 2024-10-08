using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace System.Data.Fuse.SchemaResolving {

  /// <summary>
  /// Provides functionality to get a concrete Type from a entity name (as used in the Schema/ModelDescription).
  /// The 'AssemblySearchEntityResolver' provides the known types from a internally evaluated list, which
  /// is created by crawling the existing types within the given assembly, filtered by the given namespace.
  /// (from 'FUSE-fx.RepositoryContract')
  /// </summary>
  [DebuggerDisplay("AssemblySearchEntityResolver ({_NamespaceSeachPrefix}* in {_Assembly.FullName})")]
  public class AssemblySearchEntityResolver : IEntityResolver {

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    private Type[] _KnownEntityTypes = null;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private Assembly _Assembly = null;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string _NamespaceSeachPrefix = null;

    /// <summary>
    /// Provides functionality to get a concrete Type from a entity name (as used in the Schema/ModelDescription).
    /// The 'AssemblySearchEntityResolver' provides the known types from a internally evaluated list, which
    /// is created by crawling the existing types within the given assembly, filtered by the given namespace.
    /// </summary>
    public AssemblySearchEntityResolver(Assembly assembly, string namespaceToSearch) {
      _Assembly = assembly;
      _NamespaceSeachPrefix = namespaceToSearch;
      if(!_NamespaceSeachPrefix.EndsWith(".")) {
        _NamespaceSeachPrefix = _NamespaceSeachPrefix + ".";
      }
    }

    /// <summary>
    /// Runs a looup over the list of known entity types to find one with the given name.
    /// Otherwiese it will return null.
    /// </summary>
    /// <param name="entityTypeName">Name as provided from the Schema/ModelDescription</param>
    /// <returns></returns>
    public Type TryResolveEntityTypeByName(string entityTypeName) {

      foreach (Type t in this.GetWellknownTypes()) {
        if (t.Name.Equals(entityTypeName, StringComparison.CurrentCultureIgnoreCase)) {
          return t;
        }
      }

      foreach (Type t in this.GetWellknownTypes()) {
        if (t.FullName.Equals(entityTypeName, StringComparison.CurrentCultureIgnoreCase)) {
          return t;
        }
      }

      return null;
    }

    public Type[] GetWellknownTypes() {
      if (_KnownEntityTypes == null) {
        Type[] availableTypes;
        try {
          availableTypes = _Assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex) {
          availableTypes = ex.Types.Where((t) => t != null).ToArray();
        }
        _KnownEntityTypes = availableTypes.Where(
          (t) => (t.Namespace + ".").StartsWith(_NamespaceSeachPrefix, StringComparison.CurrentCultureIgnoreCase)
         ).ToArray();
      }
      return _KnownEntityTypes;
    }

  }

}
