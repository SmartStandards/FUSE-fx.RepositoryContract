//using System;
//using System.Collections.Generic;
//using System.Linq.Expressions;


//namespace kPers {

//  /// <summary>
//  /// comes from: https://github.com/KornSW/kPers/blob/master/kPers/kPers/IRepository.cs
//  /// </summary>
//  /// <typeparam name="TItem"></typeparam>
//  public interface IRepository<TItem> {

//    int Count(Expression<Func<TItem, bool>> filterExpression);
//    bool Contains(Expression<Func<TItem, bool>> filterExpression);

//    IEnumerable<TItem> LoadAll(Expression<Func<TItem, bool>> filterExpression, object orderExpression, int skip = 0, int take = -1);

//    /// <summary>
//    /// returns false, if there is no item
//    /// throws an exception, if there is more than one item
//    /// </summary>
//    /// <param name="filterExpression"></param>
//    /// <param name=""></param>
//    /// <param name="skip"></param>
//    /// <param name="item"></param>
//    /// <returns></returns> 
//    bool TryLoadSingleItem(Expression<Func<TItem, bool>> filterExpression, object orderExpression, out TItem item);


//    // returns false, if there is no item
//    bool TryLoadFirstItem(Expression<Func<TItem, bool>> filterExpression, object orderExpression, out TItem item);

//    bool CanDeleteItem(TItem item);

//    bool TryDeleteItem(TItem item);
//    bool TryDeleteItems(Expression<Func<TItem, bool>> filterExpression);

//    bool CanAddItem(TItem item);
//    bool TryAddItem(TItem item);

//    bool CanUpateItem(TItem item);
//    bool TryUpateItem(TItem item);

//    bool CanAddOrUpdateItem(TItem item);
//    bool TryAddOrUpdateItem(TItem item);

//    IEnumerable<TItem> LoadAllItemsForRelation<TRelated>(Expression<Func<TRelated, TItem>> navExpression, object orderExpression, int skip = 0, int take = -1);

//    bool TryLoadSingleItemForRelation<TRelated>(Expression<Func<TRelated, TItem>> navExpression, object orderExpression, out TItem item);

//    bool TryLoadFirstItemForRelation<TRelated>(Expression<Func<TRelated, TItem>> navExpression, object orderExpression,  out TItem item);

//  }


//  /// <summary>
//  /// comes from: https://github.com/KornSW/kPers/blob/master/kPers/kPers/IRepository.cs
//  /// </summary>
//  public static class Extensions {

//    public static Expression<Func<TLocal, TRelated>> NavExpression<TLocal, TRelated>(this TLocal extendee, Expression<Func<TLocal, object>> localKeySelector, Expression<Func<TRelated, object>> relatedKeySelector) {
//      throw new NotImplementedException();
//    }

//    public static IEnumerable<TRelated> LoadAllRelatedItemsFrom<TLocal, TRelated>(this Expression<Func<TLocal, TRelated>> navExpression, IRepository<TRelated> repo, object orderExpression, int skip = 0, int take = -1) {
//      throw new NotImplementedException();
//    }

//    /// <summary>
//    /// returns false, if there is no item
//    /// throws an exception, if there is more than one item
//    /// </summary>
//    /// <param name="filterExpression"></param>
//    /// <param name=""></param>
//    /// <param name="skip"></param>
//    /// <param name="item"></param>
//    /// <returns></returns> 
//    public static bool TryLoadSingleRelatedItemFrom<TLocal, TRelated>(this Expression<Func<TLocal, TRelated>> navExpression, IRepository<TRelated> repo,object orderExpression, out TRelated item) {
//      throw new NotImplementedException ();
//    }

//    /// returns false, if there is no item
//    /// </summary>
//    /// <param name="filterExpression"></param>
//    /// <param name=""></param>
//    /// <param name="skip"></param>
//    /// <param name="item"></param>
//    /// <returns></returns>
//    public static bool TryLoadFirstRelatedItemFrom<TLocal, TRelated>(this Expression<Func<TLocal, TRelated>> navExpression, IRepository<TRelated> repo, object orderExpression,  out TRelated item) {
//      throw new NotImplementedException();
//    }

//  }


//  //SAMPLE ############################################################################################################

//  //  [Dependent]
//  //  public static Expression<Func<Person, Adresse>> Adressen(this Person person) {
//  //    return person.NavExpression<Person, Adresse>(
//  //      (p) => p.Id,
//  //      (a) => a.PersonId
//  //    );
//  //  }

//  //  //einer gezwungen, danach paramarray zusätzlicher (als string)
//  //  [Principal]
//  //  public static Expression<Func<Adresse, Person>> Person (this Adresse adresse) {


//  //    //muss hier entstehen und zieht später um in die EnityAnnotations
//  //    return adresse.GenerateNavigationExpression<Adresse, Person>(nameof(Person)); //<< holt die key-infors anhand der attribute!!!



//  //    return adresse.NavExpression<Adresse, Person>(
//  //      (a) => a.PersonId,
//  //      (p) => p.Id
//  //    );
//  //  }


//  //  public static void AssociateToPerson(this Adresse adresse, Person newTarget) {
//  //     adresse.PersonId = newTarget.Id;
//  //  }

//  //  /// <summary>
//  //  /// only for optionals
//  //  /// </summary>
//  //  /// <param name="adresse"></param>
//  //  public static void UnAssociateFromPerson(this Adresse adresse) {
//  //    adresse.PersonId =0;
//  //  }

//  //}

//  //public class Person {

//  //  public int Id { get; set; } =0;

//  //}

//  //public class Adresse {

//  //  public Guid Id { get; set; }

//  //  public int PersonId { get; set; } = 0;

//  //}

//  //public class Demo {

//  //public void Test( IRepository<Person> personen, IRepository<Adresse > addressen) {

//  //  Person p = null;
//  //  Adresse a = null;


//  //  //REPO-CENTRIC

//  //  var x = addressen.LoadAllItemsForRelation(p.Adressen(), null);

//  //  Person z = null;
//  //  var y = personen.TryLoadSingleItemForRelation(a.Person(), null,out z);

//  //  //ENTITY-CENTRIC

//  //  var xx = p.Adressen().LoadAllRelatedItemsFrom(addressen,null);

//  //  Person zz = null;
//  //  var y = a.Person().TryLoadSingleRelatedItemFrom(personen, null, out zz);

//  //}


//}
