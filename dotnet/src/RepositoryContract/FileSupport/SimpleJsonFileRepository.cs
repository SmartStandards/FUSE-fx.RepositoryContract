#if NETCOREAPP
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Data.Fuse;
using System.Data.ModelDescription;
using System.Data.Fuse.Convenience;

namespace System.Data.Fuse.FileSupport {
  public class SimpleJsonFileRepository<TEntity, TKey> : IRepository<TEntity, TKey> where TEntity : class, new() {
    private readonly string _BasePath;
    private readonly SchemaRoot _SchemaRoot;

    public SimpleJsonFileRepository(string basePath, SchemaRoot schemaRoot) {
      _BasePath = Path.Combine(basePath, typeof(TEntity).Name);
      Directory.CreateDirectory(_BasePath);
      this._SchemaRoot = schemaRoot;
    }

    public TEntity AddOrUpdateEntity(TEntity entity) {
      TKey key = GetEntityKey(entity);
      string filePath = GetFilePathForKey(key);
      string json = JsonSerializer.Serialize(entity);
      File.WriteAllText(filePath, json);
      return entity;
    }

    public bool ContainsKey(TKey key) {
      string filePath = GetFilePathForKey(key);
      return File.Exists(filePath);
    }

    public int Count(Expression<Func<TEntity, bool>> filter) {
      return GetAllEntities().Count(filter.Compile());
    }

    public int CountAll() {
      return Directory.GetFiles(_BasePath).Length;
    }

    public TEntity[] GetEntities(Expression<Func<TEntity, bool>> filter, string[] sortedBy = null, int limit = 100, int skip = 0) {
      var query = GetAllEntities().AsQueryable();
      if (filter != null) {
        query = query.Where(filter);
      }
      // Implement sorting and paging as needed
      return query.Skip(skip).Take(limit).ToArray();
    }

    public TEntity TryUpdateEntity(TEntity entity) {
      TKey key = GetEntityKey(entity);
      if (ContainsKey(key)) {
        AddOrUpdateEntity(entity);
        return entity;
      }
      return null;
    }

    public bool TryDeleteEntity(TKey key) {
      string filePath = GetFilePathForKey(key);
      if (File.Exists(filePath)) {
        File.Delete(filePath);
        return true;
      }
      return false;
    }

    private IEnumerable<TEntity> GetAllEntities() {
      foreach (var filePath in Directory.GetFiles(_BasePath)) {
        string json = File.ReadAllText(filePath);
        yield return JsonSerializer.Deserialize<TEntity>(json);
      }
    }

    private string GetFilePathForKey(TKey key) {
      string keyString = ConvertKeyToString(key);
      return Path.Combine(_BasePath, keyString + ".json");
    }

    private string ConvertKeyToString(TKey key) {
      if (key is ICompositeKey compositeKey) {
        return string.Join("|", compositeKey.GetFields().Select(k => k.ToString()));
      }
      return key.ToString();
    }

    private TKey GetEntityKey(TEntity entity) {
      return (TKey)ConversionHelper.GetKey(entity, _SchemaRoot);
    }

    public string GetOriginIdentity() {
      throw new NotImplementedException();
    }

    public RepositoryCapabilities GetCapabilities() {
      throw new NotImplementedException();
    }

    public EntityRef<TKey>[] GetEntityRefs(ExpressionTree filter, string[] sortedBy, int limit = 100, int skip = 0) {
      throw new NotImplementedException();
    }

    public EntityRef<TKey>[] GetEntityRefsBySearchExpression(string searchExpression, string[] sortedBy, int limit = 100, int skip = 0) {
      throw new NotImplementedException();
    }

    public EntityRef<TKey>[] GetEntityRefsByKey(TKey[] keysToLoad) {
      throw new NotImplementedException();
    }

    public TEntity[] GetEntities(ExpressionTree filter, string[] sortedBy, int limit = 100, int skip = 0) {
      throw new NotImplementedException();
    }

    public TEntity[] GetEntitiesBySearchExpression(string searchExpression, string[] sortedBy, int limit = 100, int skip = 0) {
      throw new NotImplementedException();
    }

    public TEntity[] GetEntitiesByKey(TKey[] keysToLoad) {
      throw new NotImplementedException();
    }

    public Dictionary<string, object>[] GetEntityFields(ExpressionTree filter, string[] includedFieldNames, string[] sortedBy, int limit = 100, int skip = 0) {
      throw new NotImplementedException();
    }

    public Dictionary<string, object>[] GetEntityFieldsBySearchExpression(string searchExpression, string[] includedFieldNames, string[] sortedBy, int limit = 100, int skip = 0) {
      throw new NotImplementedException();
    }

    public Dictionary<string, object>[] GetEntityFieldsByKey(TKey[] keysToLoad, string[] includedFieldNames) {
      throw new NotImplementedException();
    }

    public int Count(ExpressionTree filter) {
      throw new NotImplementedException();
    }

    public int CountBySearchExpression(string searchExpression) {
      throw new NotImplementedException();
    }

    public Dictionary<string, object> AddOrUpdateEntityFields(Dictionary<string, object> fields) {
      throw new NotImplementedException();
    }

    public Dictionary<string, object> TryUpdateEntityFields(Dictionary<string, object> fields) {
      throw new NotImplementedException();
    }

    public TKey TryAddEntity(TEntity entity) {
      throw new NotImplementedException();
    }

    public TKey[] MassupdateByKeys(TKey[] keysToUpdate, Dictionary<string, object> fields) {
      throw new NotImplementedException();
    }

    public TKey[] Massupdate(ExpressionTree filter, Dictionary<string, object> fields) {
      throw new NotImplementedException();
    }

    public TKey[] MassupdateBySearchExpression(string searchExpression, Dictionary<string, object> fields) {
      throw new NotImplementedException();
    }

    public TKey[] TryDeleteEntities(TKey[] keysToDelete) {
      throw new NotImplementedException();
    }

    public bool TryUpdateKey(TKey currentKey, TKey newKey) {
      throw new NotImplementedException();
    }

    // Implement other IRepository methods as needed...
  }
}
#endif