using System.Diagnostics;

namespace System.Data.Fuse {

  /// <summary>
  /// (from 'FUSE-fx.RepositoryContract')
  /// </summary>
  public interface ICompositeKey : IEquatable<ICompositeKey> {
    object[] GetFields();
  }

  /// <summary>
  /// (from 'FUSE-fx.RepositoryContract')
  /// 
  /// Helper-TUMPLE to represent a composite key
  /// represented by 2 fields.
  /// </summary>
  [DebuggerDisplay("{Field1}|{Field2}")]
  public class CompositeKey2<TKey1,TKey2> : ICompositeKey, IEquatable<CompositeKey2<TKey1, TKey2>> {

    public CompositeKey2() {
    }
    public CompositeKey2(TKey1 field1, TKey2 field2) {
      this.Field1 = field1;
      this.Field2 = field2;
    }

    public TKey1 Field1 { get; set; }
    public TKey2 Field2 { get; set; }

    public override string ToString() {
      return $"{Field1}|{Field2}";
    }

    public override int GetHashCode() {
      return this.ToString().GetHashCode();
    }

    public override bool Equals(object obj) {
      return this.GetHashCode().Equals(obj?.GetHashCode());
    }

    public object[] GetFields() {
      return new object[] { Field1, Field2 };
    }
    public bool Equals(ICompositeKey other) {
      return this.GetHashCode().Equals(other?.GetHashCode());
    }

    public bool Equals(CompositeKey2<TKey1, TKey2> other) {
      return this.GetHashCode().Equals(other.GetHashCode());
    }

  }

  /// <summary>
  /// (from 'FUSE-fx.RepositoryContract')
  /// Helper-TUMPLE to represent a composite key
  /// represented by 3 fields.
  /// </summary>
  [DebuggerDisplay("{Field1}|{Field2}|{Field3}")]
  public class CompositeKey3<TKey1, TKey2, TKey3> : ICompositeKey, IEquatable<CompositeKey3<TKey1, TKey2, TKey3>> {

    public CompositeKey3() {
    }
    public CompositeKey3(TKey1 field1, TKey2 field2, TKey3 field3) {
      this.Field1 = field1;
      this.Field2 = field2;
      this.Field3 = field3;
    }

    public TKey1 Field1 { get; set; }
    public TKey2 Field2 { get; set; }
    public TKey3 Field3 { get; set; }

    public override string ToString() {
      return $"{Field1}|{Field2}|{Field3}";
    }

    public override int GetHashCode() {
      return this.ToString().GetHashCode();
    }

    public override bool Equals(object obj) {
      return this.GetHashCode().Equals(obj?.GetHashCode());
    }

    public object[] GetFields() {
      return new object[] { Field1, Field2, Field3 };
    }
    public bool Equals(ICompositeKey other) {
      return this.GetHashCode().Equals(other?.GetHashCode());
    }

    public bool Equals(CompositeKey3<TKey1, TKey2, TKey3> other) {
      return this.GetHashCode().Equals(other.GetHashCode());
    }

  }

  /// <summary>
  /// (from 'FUSE-fx.RepositoryContract')
  /// Helper-TUMPLE to represent a composite key
  /// represented by 4 fields.
  /// </summary>
  [DebuggerDisplay("{Field1}|{Field2}|{Field3}|{Field4}")]
  public class CompositeKey4<TKey1, TKey2, TKey3, TKey4> : ICompositeKey, IEquatable<CompositeKey4<TKey1, TKey2, TKey3, TKey4>> {

    public CompositeKey4() {
    }
    public CompositeKey4(TKey1 field1, TKey2 field2, TKey3 field3, TKey4 field4) {
      this.Field1 = field1;
      this.Field2 = field2;
      this.Field3 = field3;
      this.Field4 = field4;
    }

    public TKey1 Field1 { get; set; }
    public TKey2 Field2 { get; set; }
    public TKey3 Field3 { get; set; }
    public TKey4 Field4 { get; set; }

    public override string ToString() {
      return $"{Field1}|{Field2}|{Field3}|{Field4}";
    }

    public override int GetHashCode() {
      return this.ToString().GetHashCode();
    }

    public override bool Equals(object obj) {
      return this.GetHashCode().Equals(obj?.GetHashCode());
    }

    public object[] GetFields() {
      return new object[] { Field1, Field2, Field3, Field4 };
    }
    public bool Equals(ICompositeKey other) {
      return this.GetHashCode().Equals(other?.GetHashCode());
    }

    public bool Equals(CompositeKey4<TKey1, TKey2, TKey3, TKey4> other) {
      return this.GetHashCode().Equals(other.GetHashCode());
    }

  }

  /// <summary>
  /// (from 'FUSE-fx.RepositoryContract')
  /// Helper-TUMPLE to represent a composite key
  /// represented by 5 fields.
  /// </summary>
  [DebuggerDisplay("{Field1}|{Field2}|{Field3}|{Field4}|{Field5}")]
  public class CompositeKey5<TKey1, TKey2, TKey3, TKey4, TKey5> : ICompositeKey, IEquatable<CompositeKey5<TKey1, TKey2, TKey3, TKey4, TKey5>> {

    public CompositeKey5() {
    }
    public CompositeKey5(TKey1 field1, TKey2 field2, TKey3 field3, TKey4 field4, TKey5 field5) {
      this.Field1 = field1;
      this.Field2 = field2;
      this.Field3 = field3;
      this.Field4 = field4;
      this.Field5 = field5;
    }

    public TKey1 Field1 { get; set; }
    public TKey2 Field2 { get; set; }
    public TKey3 Field3 { get; set; }
    public TKey4 Field4 { get; set; }
    public TKey5 Field5 { get; set; }

    public override string ToString() {
      return $"{Field1}|{Field2}|{Field3}|{Field4}|{Field5}";
    }

    public override int GetHashCode() {
      return this.ToString().GetHashCode();
    }

    public override bool Equals(object obj) {
      return this.GetHashCode().Equals(obj?.GetHashCode());
    }

    public object[] GetFields() {
      return new object[] { Field1, Field2, Field3, Field4, Field5 };
    }

    public bool Equals(ICompositeKey other) {
      return this.GetHashCode().Equals(other?.GetHashCode());
    }

    public bool Equals(CompositeKey5<TKey1, TKey2, TKey3, TKey4, TKey5> other) {
      return this.GetHashCode().Equals(other.GetHashCode());
    }

  }

}
