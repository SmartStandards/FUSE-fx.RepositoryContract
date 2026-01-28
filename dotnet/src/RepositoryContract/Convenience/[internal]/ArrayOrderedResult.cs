using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Fuse.LinqSupport;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Data.Fuse.Convenience {

  /// <summary>
  /// A helper class to collect result items by key, preserving the order of requested keys.
  /// </summary>
  /// <typeparam name="TKey"></typeparam>
  /// <typeparam name="TResultItem"></typeparam>
  internal class ArrayOrderedResult<TKey, TResultItem> {

    private class ResultEntry {
      public TKey Key;
      public TResultItem ResultItem = default(TResultItem);
      public bool WasSet = false;
    }

    private ResultEntry[] _Result;

    public ArrayOrderedResult(IEnumerable<TKey> keysInRequestedOrder) {
      _Result = keysInRequestedOrder.Select((key) => new ResultEntry { Key = key }).ToArray();
    }

    public void SetResultItem(TKey key, TResultItem resultEntry) {
      ResultEntry entry = _Result.FirstOrDefault(re => EqualityComparer<TKey>.Default.Equals(re.Key, key));
      if (entry != null) {
        entry.ResultItem = resultEntry;
        entry.WasSet = true;
      }
      else {
        throw new KeyNotFoundException("The provided key was not part of the requested keys.");
      }
    }

    public IEnumerable<TResultItem> GetResultItems(bool keepEmptyEntriesForMissingResults) {
      return _Result.SelectOrSkip(
        (ResultEntry entry, ref TResultItem resultItem) => {
          if (entry.WasSet || keepEmptyEntriesForMissingResults) {
            resultItem = entry.ResultItem;
            return true;
          }
          return false; ;
        }
      );
    }

    /// <summary></summary>
    /// <param name="unprovidedResulthandler">
    /// Used to specify the behaviour when there is an requested key left, having no result-item provided.
    /// Default behaviour is to keep an empty field!
    /// Mostly common is one of the following:
    ///   a) return false to skip the item (will not be present within the result array any more)
    ///   b) return true to keep the item (may be in addition with assigning dummy values to the 
    ///     'dummyItemToProvide' paramter in order to avoid exeptions inside of the 'aggregationSelector')
    /// </param>
    /// <returns></returns>
    public IEnumerable<TResultItem> GetResultItems(UnprovidedResultHandlingDelegate unprovidedResulthandler) {
      return _Result.SelectOrSkip(
        (ResultEntry entry, ref TResultItem resultItem) => {
          if (entry.WasSet || unprovidedResulthandler == null) {
            resultItem = entry.ResultItem;
            return true;
          }
          return unprovidedResulthandler(ref resultItem);
        }
      );
    }

    /// <summary>
    /// Used to specify the behaviour when there is an requested key left, having no result-item provided.
    /// Mostly common is one of the following:
    ///   a) return false to skip the item (will not be present within the result array any more)
    ///   b) return true to keep the item (may be in addition with assigning dummy values to the 
    ///     'dummyItemToProvide' paramter in order to avoid exeptions inside of the 'aggregationSelector')
    /// </summary>
    /// <param name="dummyItemToProvide">
    ///  Can be used to assign dummy values
    /// </param>
    /// <returns></returns>
    public delegate bool UnprovidedResultHandlingDelegate(ref TResultItem dummyItemToProvide);

  }

}
