//using System;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Linq.Dynamic.Core;
//using System.Reflection;
//using System.Collections.Generic;

//namespace System.Data {

//  /// <summary>
//  /// comes from: https://github.com/KornSW/DynamicRepository
//  /// </summary>
//  public static class DynamicLinqExtensions {

//    public static String FreetextSearchStringToFilterExpression(String filterExpressionOrFreetextSearchString, String[] exactMatchPropNames, String[] freetextPropNames) {

//      //the input is already a expression
//      if ((filterExpressionOrFreetextSearchString.Contains("=") || filterExpressionOrFreetextSearchString.Contains("<") || filterExpressionOrFreetextSearchString.Contains(">"))) {
//        return filterExpressionOrFreetextSearchString;
//      }

//      //TODO: Escaping!!!
//      string[] escaptedBlocks = filterExpressionOrFreetextSearchString.Split(' ').Select(s => "\"" + s.Trim() + "\"").ToArray();

//      string newQuery = null;

//      bool generateBraces = (((exactMatchPropNames.Length + freetextPropNames.Length) > 1) && (escaptedBlocks.Length > 1));

//      //AND!!!!
//      foreach (string escaptedBlock in escaptedBlocks) {

//        string orExpr = null;

//        //OR
//        foreach (string exactMatchPropName in exactMatchPropNames) {
//          if ((orExpr == null)) {
//            orExpr = exactMatchPropName + " == " + escaptedBlock.ToLower() + ")";
//          }
//          else {
//            orExpr = orExpr + " or " + exactMatchPropName + " == " + escaptedBlock.ToLower() + ")";
//          }
//        }
//        foreach (string freetextPropName in freetextPropNames) {
//          if ((orExpr == null)) {
//            orExpr = freetextPropName + ".ToLower().Contains(" + escaptedBlock.ToLower() + ")";
//          }
//          else {
//            orExpr = orExpr + " or " + freetextPropName + ".ToLower().Contains(" + escaptedBlock.ToLower() + ")";
//          }
//        }

//        if ((generateBraces)) {
//          orExpr = "(" + orExpr + ")";
//        }

//        if ((newQuery == null)) {
//          newQuery = orExpr;
//        }
//        else {
//          newQuery = newQuery + " and " + orExpr;
//        }

//      }

//      //can be null
//      return newQuery;
//    }

//    public static IQueryable<TEntity> DynamicallyFiltered<TEntity>(this IQueryable<TEntity> extendee, String filterExpression) {
//      var expr = DynamicExpressionParser.ParseLambda<TEntity, Boolean>(ParsingConfig.Default, false, filterExpression);
//      return extendee.Where(expr);
//    }

//    public static IOrderedQueryable<TEntity> DynamicallySorted<TEntity>(this IQueryable<TEntity> extendee, String sortExpression) {
//      return DynamicLinqSortingExecutor.ForType<TEntity>().ApplyTo(extendee, sortExpression);
//    }

//  }

//  internal class DynamicLinqSortingExecutor {

//    #region Singleton


//    private static Dictionary<Type, DynamicLinqSortingExecutor> _Instances = new Dictionary<Type, DynamicLinqSortingExecutor>();
//    public static DynamicLinqSortingExecutor ForType<TEntity>() {
//      return ForType(typeof(TEntity));
//    }

//    public static DynamicLinqSortingExecutor ForType(Type entityType) {
//      DynamicLinqSortingExecutor instance;
//      lock (_Instances) {
//        if ((_Instances.ContainsKey(entityType))) {
//          instance = _Instances[entityType];
//        }
//        else {
//          instance = new DynamicLinqSortingExecutor(entityType);
//          _Instances.Add(entityType, instance);
//        }
//      }
//      return instance;
//    }

//    #endregion

//    #region Fields & Constructor

//    private static MethodInfo _OrderByMethod = typeof(Queryable).GetMethods().Single(method => method.Name == nameof(Queryable.OrderBy) && method.IsGenericMethodDefinition && method.GetGenericArguments().Length == 2 && method.GetParameters().Length == 2);
//    private static MethodInfo _ThenByMethod = typeof(Queryable).GetMethods().Single(method => method.Name == nameof(Queryable.ThenBy) && method.IsGenericMethodDefinition && method.GetGenericArguments().Length == 2 && method.GetParameters().Length == 2);
//    private static MethodInfo _OrderByDescendingMethod = typeof(Queryable).GetMethods().Single(method => method.Name == nameof(Queryable.OrderByDescending) && method.IsGenericMethodDefinition && method.GetGenericArguments().Length == 2 && method.GetParameters().Length == 2);

//    private static MethodInfo _ThenByDescendingMethod = typeof(Queryable).GetMethods().Single(method => method.Name == nameof(Queryable.ThenByDescending) && method.IsGenericMethodDefinition && method.GetGenericArguments().Length == 2 && method.GetParameters().Length == 2);

//    private string _FirstPropertyName;
//    private DynamicLinqSortingExecutor(Type entityType) {
//      _FirstPropertyName = entityType.GetProperties().First().Name;
//    }

//    #endregion

//    #region Execution

//    public IOrderedQueryable<TEntity> ApplyTo<TEntity>(IQueryable<TEntity> input, String sortExpression) {
//      IQueryable<TEntity> chain = input;

//      var alreadyAppliedFields = new List<String>();
//      bool first = true;
//      foreach (string token in sortExpression.Split(',').Select((s) => s.Trim()).Where((s) => !String.IsNullOrWhiteSpace(s))) {
//        bool desc = false;
//        string field = token;
//        if (token.EndsWith("^")) {
//          desc = true;
//          field = token.Substring(0, token.Length - 1).Trim();
//        }
//        if (!alreadyAppliedFields.Contains(field)) {
//          chain = this.ApplySinglePropertyOrder(chain, field, desc, !first);
//          alreadyAppliedFields.Add(field);
//          first = false;
//        }
//      }

//      if (first) {
//        throw new Exception("At least one property name is required as sorting expression!");
//      }

//      return (IOrderedQueryable<TEntity>)chain;
//    }

//    private Dictionary<string, Func<IQueryable, IOrderedQueryable>> _OrderByMethodsPerField = new Dictionary<string, Func<IQueryable, IOrderedQueryable>>();
//    private Dictionary<string, Func<IQueryable, IOrderedQueryable>> _ThenByMethodPerField = new Dictionary<string, Func<IQueryable, IOrderedQueryable>>();
//    private Dictionary<string, Func<IQueryable, IOrderedQueryable>> _OrderByDescendingMethodsPerField = new Dictionary<string, Func<IQueryable, IOrderedQueryable>>();
//    private Dictionary<string, Func<IQueryable, IOrderedQueryable>> _ThenByDescendingMethodsPerField = new Dictionary<string, Func<IQueryable, IOrderedQueryable>>();

//    private IOrderedQueryable<TEntity> ApplySinglePropertyOrder<TEntity>(IQueryable<TEntity> source, string propertyName, bool descending, bool append) {
//      Func<IQueryable, IOrderedQueryable> orderMethod;

//      if ((append)) {
//        if ((descending)) {
//          orderMethod = this.GetSinglePropertyOrderMethod(typeof(TEntity), propertyName, _ThenByDescendingMethod, _ThenByDescendingMethodsPerField);
//        }
//        else {
//          orderMethod = this.GetSinglePropertyOrderMethod(typeof(TEntity), propertyName, _ThenByMethod, _ThenByMethodPerField);
//        }
//      }
//      else {
//        if ((descending)) {
//          orderMethod = this.GetSinglePropertyOrderMethod(typeof(TEntity), propertyName, _OrderByDescendingMethod, _OrderByDescendingMethodsPerField);
//        }
//        else {
//          orderMethod = this.GetSinglePropertyOrderMethod(typeof(TEntity), propertyName, _OrderByMethod, _OrderByMethodsPerField);
//        }
//      }

//      if ((orderMethod == null)) {
//        throw new Exception($"Cant apply sorting to resultset because Type '{typeof(TEntity).Name}' has no property named '{propertyName}'!");
//      }

//      return (IOrderedQueryable<TEntity>)orderMethod.Invoke(source);
//    }

//    private Func<IQueryable, IOrderedQueryable> GetSinglePropertyOrderMethod(Type entity, string propertyName, MethodInfo genericOrderMethod, Dictionary<string, Func<IQueryable, IOrderedQueryable>> cache) {

//      lock (cache) {
//        if ((cache.ContainsKey(propertyName))) {
//          return cache[propertyName];
//        }

//        Func<IQueryable, IOrderedQueryable> typedOrderFunction;
//        string[] propertyParts = propertyName.Split('.');
//        Type propertyType = entity;

//        ParameterExpression arg = Expression.Parameter(propertyType, "p");
//        Expression expr = arg;
//        foreach (string prop in propertyParts) {
//          PropertyInfo pi = propertyType.GetProperty(prop);
//          if ((pi == null)) {
//            return null;
//          }
//          expr = Expression.Property(expr, pi);
//          propertyType = pi.PropertyType;
//        }


//        Type delegateType = typeof(Func<,>).MakeGenericType(entity, propertyType);
//        LambdaExpression lambda = Expression.Lambda(delegateType, expr, arg);
//        var typedOrderMethod = genericOrderMethod.MakeGenericMethod(entity, propertyType);
//        typedOrderFunction = (IQueryable input) => (IOrderedQueryable)typedOrderMethod.Invoke(null, new object[] {
//        input,
//        lambda
//      });

//        cache.Add(propertyName, typedOrderFunction);

//        return typedOrderFunction;
//      }
//    }
//    public static Expression<Func<TEntity, bool>> BuildFilterExpressionFromAnonymousObject<TEntity>(object anonymousObject, string wildcardValue = "*") {

//      Type i = anonymousObject.GetType();
//      Type e = typeof(TEntity);
//      var eps = e.GetProperties();

//      var paramExpr = Expression.Parameter(e);

//      Expression andExpr = null;

//      foreach (PropertyInfo ip in i.GetProperties()) {
//        PropertyInfo ep = eps.Where((p) => p.Name.Equals(ip.Name, StringComparison.InvariantCultureIgnoreCase)).Single();

//        object requiredValue = ip.GetValue(anonymousObject);

//        if ((wildcardValue != null && requiredValue != null && wildcardValue.Equals(requiredValue.ToString()))) {
//          //no filtering required
//        }
//        else {
//          MemberExpression propExpr = MemberExpression.Property(paramExpr, ep.Name);
//          ConstantExpression constValueExpr;

//          if (requiredValue == null) {
//            constValueExpr = Expression.Constant(null);
//          }
//          else {
//            if (requiredValue is string && ep.PropertyType != typeof(string)) {

//              if (ep.PropertyType == typeof(byte))
//                requiredValue = byte.Parse(requiredValue as string);
//              if (ep.PropertyType == typeof(Int16))
//                requiredValue = Int16.Parse(requiredValue as string);
//              if (ep.PropertyType == typeof(Int32))
//                requiredValue = Int32.Parse(requiredValue as string);
//              if (ep.PropertyType == typeof(Int64))
//                requiredValue = Int64.Parse(requiredValue as string);
//              if (ep.PropertyType == typeof(bool))
//                requiredValue = bool.Parse(requiredValue as string);
//              if (ep.PropertyType == typeof(DateTime))
//                requiredValue = DateTime.Parse(requiredValue as string);
//              if (ep.PropertyType == typeof(double))
//                requiredValue = double.Parse(requiredValue as string);
//              if (ep.PropertyType == typeof(decimal))
//                requiredValue = decimal.Parse(requiredValue as string);
//              if (ep.PropertyType == typeof(Guid))
//                requiredValue = Guid.Parse(requiredValue as string);
//            }

//            constValueExpr = Expression.Constant(requiredValue);

//          }

//          Expression eqlExpr = Expression.Equal(propExpr, constValueExpr);

//          if (andExpr == null) {
//            andExpr = eqlExpr;
//          }
//          else {
//            andExpr = Expression.AndAlso(andExpr, eqlExpr);
//          }

//        }

//      }
//      if (paramExpr == null) {
//        return null;
//      }
//      return paramExpr as Expression<Func<TEntity, bool>>;
//    }

//    #endregion

//  }

//}
