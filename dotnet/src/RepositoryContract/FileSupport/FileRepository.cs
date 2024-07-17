#if NETCOREAPP

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text.Json;
using System.Linq.Expressions;
using System.Data.ModelDescription;
using System.Data.Fuse.Convenience;
using System.Reflection;
using System.Data.ModelDescription.Convenience;
using static System.Net.WebRequestMethods;
using System.Globalization;
using System.Formats.Asn1;
using CsvHelper;

namespace System.Data.Fuse.FileSupport {
  public class FileRepository<TEntity, TKey> : IRepository<TEntity, TKey> where TEntity : class, new() {

    public class IndexEntry {
      public string FilePath { get; set; } = "";
      public Metadata Metadata { get; set; }
    }

    private readonly string _basePath;
    private readonly SchemaRoot _SchemaRoot;
    private readonly string _indexFilePath;
    private readonly int _maxEntitiesPerFile = 1000; // Threshold for entities per file
    private Dictionary<TKey, IndexEntry> _index; // Maps TKey to file path and metadata

    private static List<List<PropertyInfo>> _UniqueKeySets;
    private static List<PropertyInfo> _PrimaryKeySet;

    public FileRepository(string basePath, SchemaRoot schemaRoot) {
      _basePath = basePath;
      this._SchemaRoot = schemaRoot;
      _indexFilePath = Path.Combine(_basePath, "index.json");

      Directory.CreateDirectory(_basePath);
      _index = LoadOrCreateIndex();
    }

    protected List<PropertyInfo> PrimaryKeySet {
      get {
        if (_PrimaryKeySet == null) {
          _PrimaryKeySet = InitPrimaeyKeySet();
        }
        return _PrimaryKeySet;
      }
    }
    protected List<PropertyInfo> InitPrimaeyKeySet() {
      return _SchemaRoot.GetPrimaryKeyProperties(typeof(TEntity)).ToList();
    }

    public TEntity AddOrUpdateEntity(TEntity entity) {
      TKey key = (TKey)ConversionHelper.GetKey(entity, this._SchemaRoot); // Assuming ConversionHelper is implemented
      string filePath;
      if (_index.TryGetValue(key, out IndexEntry? indexEntry)) {
        filePath = indexEntry.FilePath;
        List<TEntity> entities = LoadEntitiesFromFile(filePath);
        int entityIndex = entities.FindIndex(
          e => ConversionHelper.GetKey(e, this._SchemaRoot).Equals(key)
        );
        if (entityIndex >= 0) {
          entities[entityIndex] = entity; // Update
        } else {
          if (entities.Count >= _maxEntitiesPerFile) {
            filePath = CreateNewEntityFile(); // Create new file if threshold reached
            entities = new List<TEntity>(); // Start with a new list for the new file
          }
          entities.Add(entity); // Add
        }
        SaveEntitiesToFile(entities, filePath);
        UpdateIndexAndMetadata(key, filePath, entities); // Update index and metadata
      } else {
        this.AddNewEntity(key, entity);
      }
      return entity;
    }

    private void AddNewEntity(TKey key, TEntity entity) {
      string filePath = _index.LastOrDefault().Value?.FilePath ?? CreateNewEntityFile();
      List<TEntity> entities = LoadEntitiesFromFile(filePath);
      if (entities.Count >= _maxEntitiesPerFile) {
        filePath = CreateNewEntityFile(); // Create new file if threshold reached
        entities = new List<TEntity>(); // Start with a new list for the new file
      }
      entities.Add(entity); // Add
      SaveEntitiesToFile(entities, filePath);
      UpdateIndexAndMetadata(key, filePath, entities); // Update index and metadata   
    }

    public IEnumerable<TEntity> SearchEntities(
      string dynamicLinqExpression, string[] sortedBy, int limit, int skip
    ) {
      var results = new List<TEntity>();

      foreach (IndexEntry entry in GetDistinctIndexEntries()) {
        if (IsFilePotentiallyRelevant(entry.Metadata, dynamicLinqExpression)) {
          var entities = LoadEntitiesFromFile(entry.FilePath);
          if (dynamicLinqExpression == null) {
            results.AddRange(entities);
          } else {
            results.AddRange(entities.AsQueryable().Where(dynamicLinqExpression));
          }
        }
      }

      IQueryable<TEntity> queryableResults = results.AsQueryable();
      ApplySorting(sortedBy, queryableResults);
      ApplyPaging(limit, skip, queryableResults);
      return queryableResults;
    }

    private List<IndexEntry> GetDistinctIndexEntries() { 
      List<IndexEntry> distinctIndexEntries = new List<IndexEntry>();
      foreach (IndexEntry entry in _index.Values) {
        if (!distinctIndexEntries.Any(e => e.FilePath == entry.FilePath)) {
          distinctIndexEntries.Add(entry);
        }
      }
      return distinctIndexEntries;
    }

    private static IQueryable<TEntity> ApplySorting(string[] sortedBy, IQueryable<TEntity> entities) {
      foreach (var sortField in sortedBy) {
        if (sortField.StartsWith("^")) {
          string descSortField = sortField.Substring(1); // remove the "^" prefix
          entities = entities.OrderBy(descSortField + " descending");
        } else {
          entities = entities.OrderBy(sortField);
        }
      }

      return entities;
    }

    private static IQueryable<TEntity> ApplyPaging(int limit, int skip, IQueryable<TEntity> entities) {
      if (skip == 0 && limit == 0) {
        return entities;
      } else if (limit == 0) {
        return entities.Skip(skip);
      } else if (skip == 0) {
        return entities.Take(limit);
      } else {
        return entities.Skip(skip).Take(limit);
      }
    }

    private Dictionary<TKey, IndexEntry> LoadOrCreateIndex() {
      if (IO.File.Exists(_indexFilePath)) {
        string json = IO.File.ReadAllText(_indexFilePath);
        return JsonSerializer.Deserialize<Dictionary<TKey, IndexEntry>>(json);
      }
      return new Dictionary<TKey, IndexEntry>();
    }

    private void UpdateIndexAndMetadata(TKey key, string filePath, List<TEntity> entities) {
      Metadata metadata = CalculateMetadata(entities);
      _index[key] = new IndexEntry() { FilePath = filePath, Metadata = metadata };
      IO.File.WriteAllText(_indexFilePath, JsonSerializer.Serialize(_index));
    }

    private Metadata CalculateMetadata(List<TEntity> entities) {
      // Implement based on your specific metadata needs
      return new Metadata(); // Placeholder
    }

    private bool IsFilePotentiallyRelevant(Metadata metadata, string? dynamicLinqExpression) {
      return true;
      //// Example: Extracting a range from the predicate.
      //// This is a simplified example. Real predicate analysis can be complex and depends on your specific requirements.
      //if (TryExtractRange(predicate, out int minQueryValue, out int maxQueryValue)) {
      //  // Check if the query range overlaps with the file's metadata range.
      //  return minQueryValue <= metadata.MaxValue && maxQueryValue >= metadata.MinValue;
      //}
      //return false; // If the range can't be extracted or doesn't overlap, the file is not relevant.
    }

    private bool TryExtractRange(Expression<Func<TEntity, bool>> predicate, out int minQueryValue, out int maxQueryValue) {
      // Placeholder for logic to analyze the predicate and extract min/max values.
      // This is highly dependent on the structure of your predicates and entities.
      minQueryValue = 0; // Example values, replace with actual logic.
      maxQueryValue = 100; // Example values, replace with actual logic.
      return true; // Return true if extraction is successful, false otherwise.
    }

    private string CreateNewEntityFile() {
      string newFilePath = Path.Combine(_basePath, Guid.NewGuid().ToString() + ".json");
      IO.File.WriteAllText(newFilePath, "[]");
      return newFilePath;
    }

    private List<TEntity> LoadEntitiesFromFile1(string filePath) {
      string json = IO.File.ReadAllText(filePath);
      return JsonSerializer.Deserialize<List<TEntity>>(json) ?? new List<TEntity>();
    }

    private List<TEntity> LoadEntitiesFromFile(string filePath) {
      var entities = new List<TEntity>();
      var entityType = typeof(TEntity);
      var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                  .Where(p => p.CanRead && p.CanWrite)
                                  .ToArray();

      using (var reader = new StreamReader(filePath))
      using (var csvReader = new CsvHelper.CsvReader(reader, CultureInfo.InvariantCulture)) {
        csvReader.Read();
        csvReader.ReadHeader();
        while (csvReader.Read()) {
          var entity = new TEntity();
          foreach (var property in properties) {
            FieldSchema? fieldSchema = _SchemaRoot.GetSchema(entity.GetType().Name).Fields.FirstOrDefault(
              (f) => property.Name == f.Name
            );
            if (fieldSchema == null) {
              continue;
            }
            var value = csvReader.GetField(property.PropertyType, property.Name);
            property.SetValue(entity, value);
          }
          entities.Add(entity);
        }
      }

      return entities;
    }

    private void SaveEntitiesToFile1(List<TEntity> entities, string filePath) {
      string json = JsonSerializer.Serialize(entities);
      IO.File.WriteAllText(filePath, json);
    }

    private void SaveEntitiesToFile(List<TEntity> entities, string filePath) {
      var entityType = typeof(TEntity);
      var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                  .Where(p => p.CanRead)
                                  .ToArray();

      using (var writer = new StreamWriter(filePath))
      using (var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture)) {
        // Write header
        foreach (var property in properties) {
          csvWriter.WriteField(property.Name);
        }
        csvWriter.NextRecord();

        // Write records
        foreach (var entity in entities) {
          foreach (var property in properties) {
            FieldSchema? fieldSchema = _SchemaRoot.GetSchema(entity.GetType().Name).Fields.FirstOrDefault(
              (f) => property.Name == f.Name
            );
            if (fieldSchema == null) {
              continue;
            }
            var value = property.GetValue(entity);
            csvWriter.WriteField(value);
          }
          csvWriter.NextRecord();
        }
      }
    }

    public string GetOriginIdentity() {
      return "FileRepository_" + _basePath;
    }

    public RepositoryCapabilities GetCapabilities() {
      return RepositoryCapabilities.All;
    }

    public EntityRef<TKey>[] GetEntityRefs(
      ExpressionTree filter, string[] sortedBy, int limit = 100, int skip = 0
    ) {
      return this.GetEntityRefsBySearchExpression(
        filter.CompileToDynamicLinq(_SchemaRoot.GetSchema(typeof(TEntity).Name)),
        sortedBy, limit, skip
      );
    }

    public EntityRef<TKey>[] GetEntityRefsBySearchExpression(
      string searchExpression, string[] sortedBy, int limit = 100, int skip = 0
    ) {
      return this.SearchEntities(
       searchExpression,
       sortedBy, limit, skip
     ).Select(
       e => new EntityRef<TKey>(e.GetValues(PrimaryKeySet).ToKey<TKey>(), e.ToString())
     ).ToArray();
    }

    public EntityRef<TKey>[] GetEntityRefsByKey(TKey[] keysToLoad) {
      return GetEntitiesByKey(keysToLoad).Select(
       e => new EntityRef<TKey>(e.GetValues(PrimaryKeySet).ToKey<TKey>(), e.ToString())
     ).ToArray();
    }

    public TEntity[] GetEntities(ExpressionTree filter, string[] sortedBy, int limit = 100, int skip = 0) {
      return this.SearchEntities(
        filter.CompileToDynamicLinq(_SchemaRoot.GetSchema(typeof(TEntity).Name)), sortedBy, limit, skip
      ).ToArray();
    }

    public TEntity[] GetEntitiesBySearchExpression(string searchExpression, string[] sortedBy, int limit = 100, int skip = 0) {
      return this.SearchEntities(searchExpression, sortedBy, limit, skip).ToArray();
    }

    public TEntity[] GetEntitiesByKey(TKey[] keysToLoad) {
      return keysToLoad.Select(key => {
        if (_index.TryGetValue(key, out var indexEntry)) {
          return LoadEntitiesFromFile(indexEntry.FilePath).FirstOrDefault(
            e => ConversionHelper.GetKey(e, this._SchemaRoot).Equals(key)
          );
        }
        return null;
      }).Where(e => e != null).ToArray();
    }

    public Dictionary<string, object>[] GetEntityFields(ExpressionTree filter, string[] includedFieldNames, string[] sortedBy, int limit = 100, int skip = 0) {
      return GetEntityFieldsBySearchExpression(
        filter.CompileToDynamicLinq(_SchemaRoot.GetSchema(typeof(TEntity).Name)),
        includedFieldNames, sortedBy, limit, skip
      );
    }

    public Dictionary<string, object>[] GetEntityFieldsBySearchExpression(
      string searchExpression,
      string[] includedFieldNames,
      string[] sortedBy,
      int limit = 100, int skip = 0
    ) {
      return GetEntitiesBySearchExpression(searchExpression, sortedBy, limit, skip).Select(
         e => {
           var dict = new Dictionary<string, object>();
           foreach (var fieldName in includedFieldNames) {
             dict[fieldName] = e.GetType().GetProperty(fieldName).GetValue(e);
           }
           return dict;
         }
       ).ToArray();
    }

    public Dictionary<string, object>[] GetEntityFieldsByKey(TKey[] keysToLoad, string[] includedFieldNames) {
      return GetEntitiesByKey(keysToLoad).Select(
               e => {
                 var dict = new Dictionary<string, object>();
                 foreach (var fieldName in includedFieldNames) {
                   dict[fieldName] = e.GetType().GetProperty(fieldName).GetValue(e);
                 }
                 return dict;
               }
      ).ToArray();
    }

    public int CountAll() {
      return _index.Count;
    }

    public int Count(ExpressionTree filter) {
      return CountBySearchExpression(
        filter.CompileToDynamicLinq(_SchemaRoot.GetSchema(typeof(TEntity).Name))
      );
    }

    public int CountBySearchExpression(string searchExpression) {
      return SearchEntities(searchExpression, new string[0], 0, 0).Count();
    }

    public bool ContainsKey(TKey key) {
      return _index.ContainsKey(key);
    }

    public Dictionary<string, object> AddOrUpdateEntityFields(Dictionary<string, object> fields) {
      throw new NotImplementedException();
    }

    public Dictionary<string, object> TryUpdateEntityFields(Dictionary<string, object> fields) {
      throw new NotImplementedException();
    }

    public TEntity TryUpdateEntity(TEntity entity) {
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
      return keysToDelete.Where(key => TryDeleteEntity(key)).ToArray();
    }

    public bool TryDeleteEntity(TKey keyToDelete) {
      // Check if the key exists in the index
      if (!_index.TryGetValue(keyToDelete, out var indexEntry)) {
        return false; // Entity not found
      }

      // Load entities from the file
      var entities = LoadEntitiesFromFile(indexEntry.FilePath);
      // Find and remove the entity with the given key
      var entityToRemove = entities.FirstOrDefault(e => ConversionHelper.GetKey(e, this._SchemaRoot).Equals(keyToDelete));
      if (entityToRemove == null) {
        return false; // Entity not found in the file
      }
      entities.Remove(entityToRemove);

      // Save the updated list of entities back to the file
      SaveEntitiesToFile(entities, indexEntry.FilePath);

      // If the file is empty after removal, consider deleting the file and removing the entry from the index
      if (entities.Count == 0) {
        IO.File.Delete(indexEntry.FilePath); // Delete the file if empty
        _index.Remove(keyToDelete); // Remove the entry from the index
      } else {
        // Update the index and metadata since the entity list has changed
        UpdateIndexAndMetadata(keyToDelete, indexEntry.FilePath, entities);
      }

      // Save the updated index to the index file
      IO.File.WriteAllText(_indexFilePath, JsonSerializer.Serialize(_index));

      return true;
    }

    public bool TryUpdateKey(TKey currentKey, TKey newKey) {
      throw new NotImplementedException();
    }

    // Placeholder for metadata structure. Define it based on what's useful for your queries.
    public struct Metadata {
      // Define metadata fields here
    }
  }
}
#endif