using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data.Fuse.Convenience;
using System.Data.ModelDescription;
using System.Linq;
using System.Reflection;

namespace System.Data.Fuse.Convenience.Aggregation {

  [TestClass]
  public sealed class ExpressionTreeSplitterTests {

    #region " Preparation & Helpers "

    private static SchemaRoot CreateSchemaRoot() {

      // Uses reflection to avoid hard dependency on specific ModelReader API surface.

      return SchemaRootFactory.CreateFromTypes(
        new Type[] {
          typeof(PrimaryEntity),
          typeof(SecondaryEntity),
          typeof(SampleAggregatedEntity)
        }
      );

    }

    private static PredicateRoutingMap CreateRouting() {
      // Primary-owned fields (appear in PrimaryEntity/SampleView)
      // Secondary-owned fields (appear in SecondaryEntity/SampleView)
      return new PredicateRoutingMap(
        new string[] { "Id", "SecondaryId", "PrimaryNumber", "PrimaryText", "PrimaryId" },
        new string[] { "SecondaryNumber", "SecondaryText" }
      );
    }

    private static SampleAggregatedEntity[] CreateAggregatedEntities() {
      SchemaRoot schemaRoot = CreateSchemaRoot();

      InMemoryRepository<PrimaryEntity, int> primaryRepo =
        new InMemoryRepository<PrimaryEntity, int>(schemaRoot);

      InMemoryRepository<SecondaryEntity, int> secondaryRepo =
        new InMemoryRepository<SecondaryEntity, int>(schemaRoot);

      PrimaryEntity[] primaryData = TestDataFactory.CreatePrimary();
      SecondaryEntity[] secondaryData = TestDataFactory.CreateSecondary();

      int i = 0;
      while (i < primaryData.Length) {
        primaryRepo.AddOrUpdateEntity(primaryData[i]);
        i++;
      }

      int s = 0;
      while (s < secondaryData.Length) {
        secondaryRepo.AddOrUpdateEntity(secondaryData[s]);
        s++;
      }

      PredicateRoutingMap routing = CreateRouting();

      SampleAggregatedRepository repo =
        new SampleAggregatedRepository(primaryRepo, secondaryRepo, routing, 2);

      return repo.GetEntities(ExpressionTree.Empty(), new string[0], int.MaxValue, 0);
    }

    private static SampleAggregatedEntity[] Filter(SampleAggregatedEntity[] data, ExpressionTree filter) {
      List<SampleAggregatedEntity> list = new List<SampleAggregatedEntity>();

      int i = 0;
      while (i < data.Length) {
        if (LinqEvaluator.Matches(data[i], filter)) {
          list.Add(data[i]);
        }
        i++;
      }

      return list.ToArray();
    }

    private static SampleAggregatedEntity[] ApplyRecomposed(SampleAggregatedEntity[] data, ExpressionTreeSplitResult split) {
      SampleAggregatedEntity[] result = data;

      if (split.PrimaryPushdown != null) {
        result = Filter(result, split.PrimaryPushdown);
      }
      if (split.SecondaryPushdown != null) {
        result = Filter(result, split.SecondaryPushdown);
      }
      if (split.Residual != null) {
        result = Filter(result, split.Residual);
      }

      return result;
    }

    #endregion

    [TestMethod]
    public void Split_AND_PrimaryAndSecondary_IsEquivalent() {
      SampleAggregatedEntity[] data = CreateAggregatedEntities();
      PredicateRoutingMap routing = CreateRouting();

      ExpressionTree filter = ExpressionTree.Empty();
      filter.MatchAll = true;
      filter.Negate = false;

      filter.Predicates.Add(new FieldPredicate { FieldName = "PrimaryNumber", Operator = FieldOperators.Equal, Value = 10 });
      filter.Predicates.Add(new FieldPredicate { FieldName = "SecondaryText", Operator = FieldOperators.Equal, Value = "A" });

      ExpressionTreeSplitResult split = ExpressionTreeSplitter.Split(filter, routing);

      SampleAggregatedEntity[] original = Filter(data, filter);
      SampleAggregatedEntity[] recomposed = ApplyRecomposed(data, split);

      Assert.AreEqual(original.Length, recomposed.Length);
    }

    [TestMethod]
    public void Split_OR_MixedSources_BecomesResidual_IsEquivalent() {
      SampleAggregatedEntity[] data = CreateAggregatedEntities();
      PredicateRoutingMap routing = CreateRouting();

      ExpressionTree filter = ExpressionTree.Empty();
      filter.MatchAll = false; // OR
      filter.Negate = false;

      filter.Predicates.Add(new FieldPredicate { FieldName = "PrimaryNumber", Operator = FieldOperators.Equal, Value = 10 });
      filter.Predicates.Add(new FieldPredicate { FieldName = "SecondaryText", Operator = FieldOperators.Equal, Value = "B" });

      ExpressionTreeSplitResult split = ExpressionTreeSplitter.Split(filter, routing);

      SampleAggregatedEntity[] original = Filter(data, filter);
      SampleAggregatedEntity[] recomposed = ApplyRecomposed(data, split);

      Assert.AreEqual(original.Length, recomposed.Length);
    }

    [TestMethod]
    public void Split_OR_SingleSource_Pushdown_IsEquivalent() {
      SampleAggregatedEntity[] data = CreateAggregatedEntities();
      PredicateRoutingMap routing = CreateRouting();

      ExpressionTree filter = ExpressionTree.Empty();
      filter.MatchAll = false; // OR
      filter.Negate = false;

      filter.Predicates.Add(new FieldPredicate { FieldName = "SecondaryText", Operator = FieldOperators.Equal, Value = "A" });
      filter.Predicates.Add(new FieldPredicate { FieldName = "SecondaryText", Operator = FieldOperators.Equal, Value = "B" });

      ExpressionTreeSplitResult split = ExpressionTreeSplitter.Split(filter, routing);

      SampleAggregatedEntity[] original = Filter(data, filter);
      SampleAggregatedEntity[] recomposed = ApplyRecomposed(data, split);

      Assert.AreEqual(original.Length, recomposed.Length);
    }

    [TestMethod]
    public void Split_NEGATE_Mixed_BecomesResidual_IsEquivalent() {
      SampleAggregatedEntity[] data = CreateAggregatedEntities();
      PredicateRoutingMap routing = CreateRouting();

      ExpressionTree filter = ExpressionTree.Empty();
      filter.MatchAll = true;
      filter.Negate = true;

      filter.Predicates.Add(new FieldPredicate { FieldName = "PrimaryText", Operator = FieldOperators.Contains, Value = "a" });
      filter.Predicates.Add(new FieldPredicate { FieldName = "SecondaryNumber", Operator = FieldOperators.Greater, Value = 100 });

      ExpressionTreeSplitResult split = ExpressionTreeSplitter.Split(filter, routing);

      SampleAggregatedEntity[] original = Filter(data, filter);
      SampleAggregatedEntity[] recomposed = ApplyRecomposed(data, split);

      Assert.AreEqual(original.Length, recomposed.Length);
    }

    [TestMethod]
    public void Split_AND_DuplicateFieldGroup_IsKeptIntact_IsEquivalent() {
      SampleAggregatedEntity[] data = CreateAggregatedEntities();
      PredicateRoutingMap routing = CreateRouting();

      ExpressionTree filter = ExpressionTree.Empty();
      filter.MatchAll = true; // AND with duplicate field => OR group per field
      filter.Negate = false;

      filter.Predicates.Add(new FieldPredicate { FieldName = "PrimaryNumber", Operator = FieldOperators.Equal, Value = 10 });
      filter.Predicates.Add(new FieldPredicate { FieldName = "PrimaryNumber", Operator = FieldOperators.Equal, Value = 30 });

      ExpressionTreeSplitResult split = ExpressionTreeSplitter.Split(filter, routing);

      SampleAggregatedEntity[] original = Filter(data, filter);
      SampleAggregatedEntity[] recomposed = ApplyRecomposed(data, split);

      Assert.AreEqual(original.Length, recomposed.Length);
    }

    [TestMethod]
    public void Split_SubTrees_Mixed_IsEquivalent() {
      SampleAggregatedEntity[] data = CreateAggregatedEntities();
      PredicateRoutingMap routing = CreateRouting();

      ExpressionTree sub1 = ExpressionTree.Empty();
      sub1.MatchAll = true;
      sub1.Negate = false;
      sub1.Predicates.Add(new FieldPredicate { FieldName = "PrimaryNumber", Operator = FieldOperators.Equal, Value = 10 });

      ExpressionTree sub2 = ExpressionTree.Empty();
      sub2.MatchAll = true;
      sub2.Negate = false;
      sub2.Predicates.Add(new FieldPredicate { FieldName = "SecondaryText", Operator = FieldOperators.Equal, Value = "C" });

      ExpressionTree filter = ExpressionTree.Empty();
      filter.MatchAll = true;
      filter.Negate = false;
      filter.SubTree = new List<ExpressionTree>();
      filter.SubTree.Add(sub1);
      filter.SubTree.Add(sub2);

      ExpressionTreeSplitResult split = ExpressionTreeSplitter.Split(filter, routing);

      SampleAggregatedEntity[] original = Filter(data, filter);
      SampleAggregatedEntity[] recomposed = ApplyRecomposed(data, split);

      Assert.AreEqual(original.Length, recomposed.Length);
    }
  }

  internal static class SchemaRootFactory {

    /// <summary>
    /// Creates a SchemaRoot for the given entity types using reflection-based ModelReader invocation.
    /// This keeps tests compatible across slightly different API versions.
    /// </summary>
    public static SchemaRoot CreateFromTypes(Type[] types) {
      if (types == null) {
        throw new ArgumentNullException(nameof(types));
      }

      Assembly[] assemblies = new Assembly[] { types[0].Assembly };

      SchemaRoot schemaRoot = TryCreateUsingModelReader(types, assemblies);
      if (schemaRoot != null) {
        return schemaRoot;
      }

      throw new InvalidOperationException(
        "Could not create SchemaRoot. Please ensure ModelReader is referenced and provides a static method returning SchemaRoot for Assembly or Type[]."
      );
    }

    private static SchemaRoot TryCreateUsingModelReader(Type[] types, Assembly[] assemblies) {
      // Candidate type names for ModelReader
      string[] candidates = new string[] {
        "System.Data.ModelReader.ModelReader",
        "System.Data.Fuse.ModelReader.ModelReader",
        "System.Data.ModelReader.Convenience.ModelReader"
      };

      int c = 0;
      while (c < candidates.Length) {
        Type modelReaderType = Type.GetType(candidates[c], false);
        if (modelReaderType != null) {
          SchemaRoot root = TryInvokeReturningSchemaRoot(modelReaderType, types, assemblies);
          if (root != null) {
            return root;
          }
        }
        c++;
      }

      return null;
    }

    private static SchemaRoot TryInvokeReturningSchemaRoot(Type modelReaderType, Type[] types, Assembly[] assemblies) {
      MethodInfo[] methods = modelReaderType.GetMethods(BindingFlags.Public | BindingFlags.Static);

      int i = 0;
      while (i < methods.Length) {
        MethodInfo m = methods[i];

        if (m.ReturnType == typeof(SchemaRoot)) {
          ParameterInfo[] p = m.GetParameters();

          // Signature: SchemaRoot Xxx(Assembly)
          if (p.Length == 1 && p[0].ParameterType == typeof(Assembly)) {
            try {
              object result = m.Invoke(null, new object[] { assemblies[0] });
              SchemaRoot root = result as SchemaRoot;
              if (root != null) {
                return root;
              }
            }
            catch {
              // Ignore and continue
            }
          }

          // Signature: SchemaRoot Xxx(Assembly[])
          if (p.Length == 1 && p[0].ParameterType == typeof(Assembly[])) {
            try {
              object result = m.Invoke(null, new object[] { assemblies });
              SchemaRoot root = result as SchemaRoot;
              if (root != null) {
                return root;
              }
            }
            catch {
              // Ignore and continue
            }
          }

          // Signature: SchemaRoot Xxx(Type[])
          if (p.Length == 1 && p[0].ParameterType == typeof(Type[])) {
            try {
              object result = m.Invoke(null, new object[] { types });
              SchemaRoot root = result as SchemaRoot;
              if (root != null) {
                return root;
              }
            }
            catch {
              // Ignore and continue
            }
          }
        }

        i++;
      }

      return null;
    }

  }

}
