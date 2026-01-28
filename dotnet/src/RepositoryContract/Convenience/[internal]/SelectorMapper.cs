using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Data.Fuse.Convenience {

  internal static class SelectorMapper {

    //produces a system.Linq.Expression that represents selecting the given fields from TEntity
    public static Expression<Func<TEntity, TEntity>> CreateDynamicSelectorExpression<TEntity>(string[] selectedFieldNames) {
      if (selectedFieldNames == null || selectedFieldNames.Length == 0) {
        throw new ArgumentException("No fields specified for selection.", nameof(selectedFieldNames));
      }

      // Create a parameter expression for the TEntity type
      ParameterExpression parameter = Expression.Parameter(typeof(TEntity), "e");

      // Create bindings for each selected field
      IEnumerable<MemberAssignment> bindings = selectedFieldNames.Select(fieldName => {
        MemberExpression property = Expression.Property(parameter, fieldName);
        return Expression.Bind(typeof(TEntity).GetProperty(fieldName), property);
      });

      // Create a new expression for the selected fields
      NewExpression newExpression = Expression.New(typeof(TEntity));
      MemberInitExpression memberInit = Expression.MemberInit(newExpression, bindings);

      return Expression.Lambda<Func<TEntity, TEntity>>(memberInit, parameter);
    }

    /// <summary>
    /// Extracts a list of field names referenced by the selector.
    /// </summary>
    public static string[] ExtractSelectorFieldNames<TEntity, TSelectedFields>(Expression<Func<TEntity, TSelectedFields>> selector) {
      
      Expression body = selector.Body;

      List<string> fields = new List<string>();

      NewExpression newExpression = body as NewExpression;
      if (newExpression != null) {
        for (int i = 0; i < newExpression.Arguments.Count; i++) {
          MemberExpression member = newExpression.Arguments[i] as MemberExpression;
          if (member == null) {
            throw new NotSupportedException(
                "Only direct member projections are supported in anonymous selectors."
            );
          }

          string fieldName = ExtractMemberName(member);
          fields.Add(fieldName);
        }

        return fields.ToArray();
      }

      MemberInitExpression init = body as MemberInitExpression;
      if (init != null) {
        foreach (MemberBinding binding in init.Bindings) {
          MemberAssignment assignment = binding as MemberAssignment;
          if (assignment == null) {
            throw new NotSupportedException("Unsupported member binding in projection.");
          }

          MemberExpression member = assignment.Expression as MemberExpression;
          if (member == null) {
            throw new NotSupportedException("Only direct field assignment in projections is supported.");
          }

          string fieldName = ExtractMemberName(member);
          fields.Add(fieldName);
        }

        return fields.ToArray();
      }

      throw new NotSupportedException("Unsupported selector expression type: " + body.NodeType.ToString());
    }

    /// <summary>
    /// Extracts the simple field name (no nested properties supported for projections).
    /// </summary>
    private static string ExtractMemberName(MemberExpression member) {
      return member.Member.Name;
    }

    /// <summary>
    /// Maps a dictionary row array into instances of TSelectedFields.
    /// </summary>
    public static TSelectedFields[] Map<TEntity, TSelectedFields>(
        Dictionary<string, object>[] rows,
        Expression<Func<TEntity, TSelectedFields>> selector) {
      if (rows == null) {
        return new TSelectedFields[0];
      }

      Func<Dictionary<string, object>, TSelectedFields> projector =
          BuildProjector<TEntity, TSelectedFields>(selector);

      TSelectedFields[] result = new TSelectedFields[rows.Length];

      for (int i = 0; i < rows.Length; i++) {
        result[i] = projector(rows[i]);
      }

      return result;
    }

    /// <summary>
    /// Converts a projection object (which may be an anonymous type or DTO)
    /// into a dictionary { fieldName -> value }.
    /// Used for mass update operations.
    /// </summary>
    /// <typeparam name="TSelectedFields">The type of the projection object.</typeparam>
    /// <param name="instance">The populated projection instance.</param>
    /// <returns>A dictionary mapping property names to values.</returns>
    /// <exception cref="ArgumentNullException">Thrown if instance is null.</exception>
    /// <exception cref="NotSupportedException">Thrown if type contains unsupported members.</exception>
    public static Dictionary<string, object> MapToDict<TSelectedFields>(TSelectedFields instance) {
      if (instance == null) {
        throw new ArgumentNullException("instance");
      }

      Dictionary<string, object> dict = new Dictionary<string, object>();

      Type type = typeof(TSelectedFields);

      // Anonymous types have compiler-generated constructors & get-only properties.
      // DTOs also work naturally with this.
      PropertyInfo[] props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);

      if (props == null || props.Length == 0) {
        throw new NotSupportedException("Projection type contains no public readable properties.");
      }

      for (int i = 0; i < props.Length; i++) {
        PropertyInfo prop = props[i];

        if (!prop.CanRead) {
          throw new NotSupportedException(
              "Projection type contains unreadable property: " + prop.Name
          );
        }

        string name = prop.Name;
        object value = prop.GetValue(instance, null);

        dict[name] = value;
      }

      return dict;
    }

    /// <summary>
    /// Creates a dictionary-to-object projector from the selector expression.
    /// </summary>
    private static Func<Dictionary<string, object>, TSelectedFields> BuildProjector<TEntity, TSelectedFields>(
      Expression<Func<TEntity, TSelectedFields>> selector
    ) {
      Expression body = selector.Body;

      NewExpression newExpr = body as NewExpression;
      if (newExpr != null) {
        return BuildAnonymousProjector<TEntity, TSelectedFields>(newExpr);
      }

      MemberInitExpression initExpr = body as MemberInitExpression;
      if (initExpr != null) {
        return BuildObjectInitProjector<TEntity, TSelectedFields>(initExpr);
      }

      throw new NotSupportedException("Unsupported projection selector expression: " + body.NodeType.ToString());
    }

    /// <summary>
    /// Builds a projector for anonymous type new { X = p.Age, ... }.
    /// </summary>
    private static Func<Dictionary<string, object>, TSelectedFields> BuildAnonymousProjector<TEntity, TSelectedFields>(
      NewExpression newExpr
    ) {
      
      return (Dictionary<string, object> dict) => {
        object[] args = new object[newExpr.Arguments.Count];

        for (int i = 0; i < newExpr.Arguments.Count; i++) {
          MemberExpression member = newExpr.Arguments[i] as MemberExpression;
          if (member == null) {
            throw new NotSupportedException("Anonymous projections only allow direct field accesses.");
          }

          string fieldName = member.Member.Name;
          object value = dict.ContainsKey(fieldName) ? dict[fieldName] : null;
          args[i] = value;
        }

        object anonymous = newExpr.Constructor.Invoke(args);
        return (TSelectedFields)anonymous;
      };
    }

    /// <summary>
    /// Builds a projector for DTO projections: new Dto { X = p.A, Y = p.B }.
    /// </summary>
    private static Func<Dictionary<string, object>, TSelectedFields> BuildObjectInitProjector<TEntity, TSelectedFields>(
        MemberInitExpression initExpr
    ) {

      ConstructorInfo ctor = initExpr.NewExpression.Constructor;
      if (ctor == null) {
        throw new NotSupportedException("DTO projection requires a constructor.");
      }

      return (Dictionary<string, object> dict) => {
        object instance = ctor.Invoke(new object[0]);

        foreach (MemberBinding binding in initExpr.Bindings) {
          MemberAssignment assign = binding as MemberAssignment;
          if (assign == null) {
            throw new NotSupportedException("Unsupported member binding in DTO projection.");
          }

          MemberExpression member = assign.Expression as MemberExpression;
          if (member == null) {
            throw new NotSupportedException("DTO projections only support direct field assignment.");
          }

          string fieldName = member.Member.Name;
          object value = dict.ContainsKey(fieldName) ? dict[fieldName] : null;

          PropertyInfo prop = assign.Member as PropertyInfo;
          if (prop == null) {
            throw new NotSupportedException("DTO projection requires property assignment.");
          }

          prop.SetValue(instance, value, null);
        }

        return (TSelectedFields)instance;
      };
    }

  }

}
