using System;
using System.Linq.Expressions;
using RevStackCore.DynamoDb.Client;
using RevStackCore.Pattern;

namespace RevStackCore.DynamoDb
{
    public interface IDynamoDbRepository<TEntity, TKey> : IRepository<TEntity, TKey> where TEntity : class, IEntity<TKey>
    {
        IPocoDynamo DbClient { get;  }
        TEntity GetById(TKey id, object range);
        void Delete(TKey id);
        void Delete(TKey id, object range);
        ScanExpression<TEntity> Scan(Expression<Func<TEntity, bool>> predicate);
        QueryExpression<TEntity> Query(Expression<Func<TEntity, bool>> keyConditionPredicate);
        QueryExpression<TEntity> Query(DynamoGlobalIndex index, Expression<Func<TEntity, bool>> keyConditionPredicate);
        QueryExpression<TEntity> Query(DynamoLocalIndex index, Expression<Func<TEntity, bool>> keyConditionPredicate);
        QueryExpression<TEntity> QueryComposite(Expression<Func<TEntity, bool>> keyConditionPredicate);
    }
}
