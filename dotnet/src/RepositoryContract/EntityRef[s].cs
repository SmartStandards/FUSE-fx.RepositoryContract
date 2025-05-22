using System.Diagnostics;

namespace System.Data.Fuse {

  /// <summary>
  /// (from 'FUSE-fx.RepositoryContract')
  /// EntityRef (UNTYPED)
  /// </summary>
  [DebuggerDisplay("EntityRef {Label}")]
  public class EntityRef {

    public EntityRef() {
    }

    public EntityRef(object key, string label) {
      this.Key = key;
      this.Label = label;
    }

    public virtual object Key { get; set; } = null;

    public string Label { get; set; } = string.Empty;

    /// <summary> ONLY THE LABEL! </summary>
    public override string ToString() {
      if (this.Label == null) {
        return string.Empty;
      }
      return this.Label;
    }

    /// <summary> Based ONLY ON THE KEY (the label is not relevant) </summary>
    public override int GetHashCode() {
      if (this.Key == null) {
        return 0;
      }
      return this.Key.GetHashCode();
    }

    /// <summary> Based ONLY ON THE KEY (the label is not relevant) </summary>
    public override bool Equals(object obj) {
      return this.GetHashCode().Equals(obj?.GetHashCode());
    }

  }

  /// <summary>
  /// (from 'FUSE-fx.RepositoryContract')
  /// EntityRef with Typed Key (generic)
  /// </summary>
  [DebuggerDisplay("EntityRef {Label}")]
  public class EntityRef<TKey> : EntityRef {

    private bool _TypedKeyWasSet;

    public EntityRef() {
    }

    public EntityRef(TKey key, string label) {
      this.Key = key;
      this.Label = label;
    }

    private TKey _Key = default(TKey);

    //NOTE: the 'new' keyword is used to do a socalled 'SHADOWING'
    //which is an override with typechange!
    public new TKey Key {
      get {
        if (!_TypedKeyWasSet) {
          _Key = (TKey)base.Key;
        } 
        return _Key;
      }
      set {
        _Key = value;
        base.Key = value;
        _TypedKeyWasSet = true;
      }
    }

    /// <summary>
    /// Creates an generic EntityRef[TKey] from an untyped EntityRef.
    /// NOTE: a type missmatch between the runtime type within the untyped input
    /// and the given declarative type will cause an conversion exception!
    /// </summary>
    public static EntityRef<TKey> From(EntityRef entityRef) {
      return new EntityRef<TKey>(((TKey)entityRef.Key), entityRef.Label);
    }

    /// <summary> Based ONLY ON THE KEY (the label is not relevant) </summary>
    public override int GetHashCode() {
      if (this.Key == null) {
        return 0;
      }
      return this.Key.GetHashCode();
    }
  }

}
