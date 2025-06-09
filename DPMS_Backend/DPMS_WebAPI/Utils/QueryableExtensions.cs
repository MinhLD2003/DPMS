using DPMS_WebAPI.Models;
using DPMS_WebAPI.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;

namespace DPMS_WebAPI.Utils
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> ApplyFiltering<T>(this IQueryable<T> query, Dictionary<string, string> filters)
        {
            if (filters == null || !filters.Any())
                return query;

            var parameter = Expression.Parameter(typeof(T), "x");
            Expression? filterExpression = null;

            // Group filters by property name for range operations
            var rangeFilters = new Dictionary<string, Dictionary<string, string>>();

            foreach (var filter in filters)
            {
                // Skip empty filter values
                if (string.IsNullOrWhiteSpace(filter.Value))
                    continue;

                // Check if this is a range filter (property_from or property_to)
                if (filter.Key.EndsWith("_from") || filter.Key.EndsWith("_to"))
                {
                    string propertyName = filter.Key.EndsWith("_from")
                        ? filter.Key.Substring(0, filter.Key.Length - 5)
                        : filter.Key.Substring(0, filter.Key.Length - 3);

                    string rangeType = filter.Key.EndsWith("_from") ? "from" : "to";

                    if (!rangeFilters.ContainsKey(propertyName))
                        rangeFilters[propertyName] = new Dictionary<string, string>();

                    rangeFilters[propertyName][rangeType] = filter.Value;
                    continue;
                }

                // Process regular filters (no change to original code)
                string[] propertyPath = filter.Key.Split('.');
                PropertyInfo? property = null;
                Expression? propertyAccess = parameter;

                // Navigate through the property chain
                foreach (var propertyName in propertyPath)
                {
                    Type currentType = propertyAccess.Type;
                    property = currentType.GetProperty(propertyName,
                        BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                    if (property == null)
                        break;

                    propertyAccess = Expression.Property(propertyAccess, property);
                }

                if (property == null || propertyAccess == null)
                    continue;

                // Create constant with appropriate type conversion
                object? typedValue = ConvertValue(filter.Value, property.PropertyType);
                var constant = Expression.Constant(typedValue);

                // For string properties, use Contains method
                Expression comparison;
                if (property.PropertyType == typeof(string))
                {
                    // Check for null value before calling Contains
                    var notNullCheck = Expression.NotEqual(propertyAccess, Expression.Constant(null));
                    var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) })!;
                    var containsCall = Expression.Call(propertyAccess, containsMethod, constant);
                    comparison = Expression.AndAlso(notNullCheck, containsCall);
                }
                else
                {
                    // For non-string properties, use equality comparison
                    comparison = Expression.Equal(propertyAccess, Expression.Convert(constant, property.PropertyType));
                }

                // Combine expressions with And operator
                filterExpression = filterExpression == null
                    ? comparison
                    : Expression.AndAlso(filterExpression, comparison);
            }

            // Process range filters
            foreach (var rangeFilter in rangeFilters)
            {
                string propertyName = rangeFilter.Key;
                var rangeValues = rangeFilter.Value;

                // Handle nested properties (e.g. "User.CreatedAt")
                string[] propertyPath = propertyName.Split('.');
                PropertyInfo? property = null;
                Expression? propertyAccess = parameter;

                // Navigate through the property chain
                foreach (var propName in propertyPath)
                {
                    Type currentType = propertyAccess.Type;
                    property = currentType.GetProperty(propName,
                        BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                    if (property == null)
                        break;

                    propertyAccess = Expression.Property(propertyAccess, property);
                }

                if (property == null || propertyAccess == null)
                    continue;

                // Apply "from" filter (greater than or equal to)
                if (rangeValues.TryGetValue("from", out string? fromValue) && !string.IsNullOrWhiteSpace(fromValue))
                {
                    object? typedFromValue = ConvertValue(fromValue, property.PropertyType);
                    if (typedFromValue != null)
                    {
                        var fromConstant = Expression.Constant(typedFromValue);
                        var greaterThanOrEqual = Expression.GreaterThanOrEqual(
                            propertyAccess,
                            Expression.Convert(fromConstant, property.PropertyType)
                        );

                        filterExpression = filterExpression == null
                            ? greaterThanOrEqual
                            : Expression.AndAlso(filterExpression, greaterThanOrEqual);
                    }
                }

                // Apply "to" filter (less than or equal to)
                if (rangeValues.TryGetValue("to", out string? toValue) && !string.IsNullOrWhiteSpace(toValue))
                {
                    object? typedToValue = ConvertValue(toValue, property.PropertyType);
                    if (typedToValue != null)
                    {
                        var toConstant = Expression.Constant(typedToValue);
                        var lessThanOrEqual = Expression.LessThanOrEqual(
                            propertyAccess,
                            Expression.Convert(toConstant, property.PropertyType)
                        );

                        filterExpression = filterExpression == null
                            ? lessThanOrEqual
                            : Expression.AndAlso(filterExpression, lessThanOrEqual);
                    }
                }
            }

            // Apply filter if any were valid
            if (filterExpression != null)
            {
                var lambda = Expression.Lambda<Func<T, bool>>(filterExpression, parameter);
                query = query.Where(lambda);
            }

            return query;
        }

        public static IQueryable<T> ApplySorting<T>(this IQueryable<T> query, string sortBy, string sortDirection)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
                return query;

            // Handle nested properties (e.g. "User.Name")
            string[] propertyPath = sortBy.Split('.');
            PropertyInfo? property = null;
            var parameter = Expression.Parameter(typeof(T), "x");
            Expression? propertyAccess = parameter;

            // Navigate through the property chain
            foreach (var propertyName in propertyPath)
            {
                Type currentType = propertyAccess.Type;
                property = currentType.GetProperty(propertyName,
                    BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (property == null)
                    return query;

                propertyAccess = Expression.Property(propertyAccess, property);
            }

            if (property == null || propertyAccess == null)
                return query;

            // Create lambda expression for sorting
            var lambdaType = typeof(Func<,>).MakeGenericType(typeof(T), propertyAccess.Type);
            var lambda = Expression.Lambda(lambdaType, propertyAccess, parameter);

            // Determine sort method based on direction
            string methodName = sortDirection.ToLower() == "desc" ? "OrderByDescending" : "OrderBy";

            // Create method call expression
            var methodCall = Expression.Call(
                typeof(Queryable),
                methodName,
                new[] { typeof(T), propertyAccess.Type },
                query.Expression,
                Expression.Quote(lambda)
            );

            // Execute the sorting
            return query.Provider.CreateQuery<T>(methodCall);
        }

        public static IQueryable<T> ApplyIncludes<T>(this IQueryable<T> query, params Expression<Func<T, object>>[]? includes) where T : class
        {
            if (includes == null || !includes.Any())
                return query;

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return query;
        }


        public static PagedResponse<T> ToPagedResponse<T>(this IQueryable<T> query, int pageNumber, int pageSize)
        {
            if (typeof(T) == typeof(Feature))
            {
                // Cast query to Feature IQueryable
                var featureQuery = query as IQueryable<Feature>;
                if (featureQuery == null)
                    throw new InvalidOperationException("Invalid query type for Feature paging.");

                // Only page through root-level features (ParentId == null)
                var rootFeaturesQuery = featureQuery.Where(f => f.ParentId == null);

                var totalRootCount = rootFeaturesQuery.Count();
                var totalPages = (int)Math.Ceiling(totalRootCount / (double)pageSize);

                var pagedRoots = rootFeaturesQuery
                    .OrderBy(f => f.FeatureName) // You may customize sorting
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // Now include their children
                var featureIds = pagedRoots.Select(f => f.Id).ToList();

                // Get all children of these roots (and possibly grandchildren if needed)
                var allFeatures = featureQuery
                    .Where(f => f.ParentId == null || featureIds.Contains(f.ParentId.Value))
                    .ToList();

                return new PagedResponse<T>
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = totalPages,
                    TotalRecords = totalRootCount,
                    Data = allFeatures.Cast<T>().ToList()
                };
            }
            else
            {
                // Regular flat paging
                var totalCount = query.Count();
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                var data = query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return new PagedResponse<T>
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = totalPages,
                    TotalRecords = totalCount,
                    Data = data
                };
            }
        }


        private static object? ConvertValue(string value, Type targetType)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            // Handle nullable types
            Type underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            // Handle enum types (case insensitive)
            if (underlyingType.IsEnum)
            {
                // Try direct parsing (case insensitive)
                if (Enum.TryParse(underlyingType, value, true, out object? result))
                    return result;

                // Try parsing as integer value of enum
                if (int.TryParse(value, out int intValue))
                {
                    // Verify the integer is a valid enum value
                    if (Enum.IsDefined(underlyingType, intValue))
                        return Enum.ToObject(underlyingType, intValue);
                }

                // If we get here, the enum value couldn't be parsed
                return null;
            }

            // Handle other common types
            try
            {
                if (underlyingType == typeof(string))
                    return value;
                else if (underlyingType == typeof(int))
                    return int.Parse(value);
                else if (underlyingType == typeof(long))
                    return long.Parse(value);
                else if (underlyingType == typeof(double))
                    return double.Parse(value);
                else if (underlyingType == typeof(decimal))
                    return decimal.Parse(value);
                else if (underlyingType == typeof(DateTime))
                    return DateTime.Parse(value);
                else if (underlyingType == typeof(bool))
                    return bool.Parse(value);
                else if (underlyingType == typeof(Guid))
                    return Guid.Parse(value);

                // For any other type, try using Convert.ChangeType
                return Convert.ChangeType(value, underlyingType);
            }
            catch
            {
                // If conversion fails, return null
                return null;
            }
        }
    }
}
