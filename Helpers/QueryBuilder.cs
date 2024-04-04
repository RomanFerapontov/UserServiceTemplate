using System.Linq.Expressions;
using UserServiceTemplate.Models;

namespace UserServiceTemplate.Helpers;

public class QueryBuilder<T>(IQueryable<T> query) {
    private IQueryable<T> _query = query;

    public QueryBuilder<T> Filter(QueryObject parameters) {
        if (!string.IsNullOrWhiteSpace(parameters.SortBy)) {
            var parameterExpr = Expression.Parameter(typeof(T), string.Empty);
            var propertyExpr = Expression.Property(parameterExpr, parameters.SortBy);
            var orderByExpr = Expression.Lambda(propertyExpr, parameterExpr);

            string orderByMethod = string.IsNullOrEmpty(parameters.OrderBy) || !parameters.OrderBy.Equals("desc", StringComparison.CurrentCultureIgnoreCase) ?
                "OrderBy" : "OrderByDescending";

            var orderByCall = Expression.Call(
                typeof(Queryable),
                orderByMethod,
                [typeof(T), propertyExpr.Type],
                _query.Expression,
                Expression.Quote(orderByExpr)
            );
            _query = _query.Provider.CreateQuery<T>(orderByCall);
        }

        _query = _query.Skip(parameters.Offset).Take(parameters.Limit);
        return this;
    }

    public IQueryable<T> Build() {
        return _query;;
    }
}

