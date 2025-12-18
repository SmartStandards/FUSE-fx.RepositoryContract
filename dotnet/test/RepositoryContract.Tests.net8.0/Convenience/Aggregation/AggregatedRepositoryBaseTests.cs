using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data.Fuse.Convenience;
using System.Data.ModelDescription;
using System.Linq;

namespace System.Data.Fuse.Convenience.Aggregation {

  [TestClass]
  public sealed class AggregatedRepositoryBaseTests {

    #region " Preparation "

    private static SchemaRoot CreateSchemaRoot() {
      return SchemaRootFactory.CreateFromTypes(
        new Type[] {
          typeof(PrimaryEntity),
          typeof(SecondaryEntity),
          typeof(SampleAggregatedEntity)
        }
      );
    }

    private static PredicateRoutingMap CreateRouting() {
      return new PredicateRoutingMap(
        new string[] { "Id", "SecondaryId", "PrimaryNumber", "PrimaryText", "PrimaryId" },
        new string[] { "SecondaryNumber", "SecondaryText" }
      );
    }

    private static SampleAggregatedRepository CreateRepo(out SampleAggregatedEntity[] allViews) {
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

      allViews = repo.GetEntities(ExpressionTree.Empty(), new string[0], int.MaxValue, 0);
      return repo;
    }

    #endregion

    [TestMethod]
    public void AggregatedRepo_Result_Equals_LinqReference_ForMixedAnd() {
      SampleAggregatedEntity[] all;
      SampleAggregatedRepository repo = CreateRepo(out all);

      ExpressionTree filter = ExpressionTree.Empty();
      filter.MatchAll = true;
      filter.Negate = false;

      filter.Predicates.Add(new FieldPredicate { FieldName = "PrimaryNumber", Operator = FieldOperators.Equal, Value = 10 });
      filter.Predicates.Add(new FieldPredicate { FieldName = "SecondaryText", Operator = FieldOperators.Equal, Value = "A" });

      SampleAggregatedEntity[] aggregated = repo.GetEntities(filter, new string[0], int.MaxValue, 0);

      SampleAggregatedEntity[] linq =
        all.Where((v) => (v.PrimaryNumber == 10) && string.Equals(v.SecondaryText, "A", StringComparison.OrdinalIgnoreCase))
           .ToArray();

      Assert.AreEqual(linq.Length, aggregated.Length);
    }

    [TestMethod]
    public void AggregatedRepo_Result_Equals_LinqReference_ForOrMixedResidual() {
      SampleAggregatedEntity[] all;
      SampleAggregatedRepository repo = CreateRepo(out all);

      ExpressionTree filter = ExpressionTree.Empty();
      filter.MatchAll = false; // OR
      filter.Negate = false;

      filter.Predicates.Add(new FieldPredicate { FieldName = "PrimaryNumber", Operator = FieldOperators.Equal, Value = 40 });
      filter.Predicates.Add(new FieldPredicate { FieldName = "SecondaryText", Operator = FieldOperators.Equal, Value = "B" });

      SampleAggregatedEntity[] aggregated = repo.GetEntities(filter, new string[0], int.MaxValue, 0);

      SampleAggregatedEntity[] linq =
        all.Where((v) => (v.PrimaryNumber == 40) || string.Equals(v.SecondaryText, "B", StringComparison.OrdinalIgnoreCase))
           .ToArray();

      Assert.AreEqual(linq.Length, aggregated.Length);
    }

    [TestMethod]
    public void AggregatedRepo_Result_Equals_LinqReference_ForDuplicateFieldOrGroup() {
      SampleAggregatedEntity[] all;
      SampleAggregatedRepository repo = CreateRepo(out all);

      ExpressionTree filter = ExpressionTree.Empty();
      filter.MatchAll = true; // AND with duplicate field => OR group
      filter.Negate = false;

      filter.Predicates.Add(new FieldPredicate { FieldName = "PrimaryNumber", Operator = FieldOperators.Equal, Value = 10 });
      filter.Predicates.Add(new FieldPredicate { FieldName = "PrimaryNumber", Operator = FieldOperators.Equal, Value = 30 });

      SampleAggregatedEntity[] aggregated = repo.GetEntities(filter, new string[0], int.MaxValue, 0);

      SampleAggregatedEntity[] linq =
        all.Where((v) => (v.PrimaryNumber == 10) || (v.PrimaryNumber == 30))
           .ToArray();

      Assert.AreEqual(linq.Length, aggregated.Length);
    }

  }

}
