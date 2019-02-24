using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using RevStackCore.DynamoDb.Client;
using RevStackCore.Pattern;

namespace RevStackCore.DynamoDb
{
    public interface IDynamoDbService<TEntity,TKey> : IService<TEntity,TKey> where TEntity : class, IEntity<TKey>
    {
        IPocoDynamo DbClient { get; }
        TEntity GetById(TKey id, object range);
        void Delete(TKey id);
        void Delete(TKey id, object range);
        ScanExpression<TEntity> Scan(Expression<Func<TEntity, bool>> predicate);
        QueryExpression<TEntity> Query(Expression<Func<TEntity, bool>> keyConditionPredicate);
        QueryExpression<TEntity> Query(DynamoGlobalIndex index, Expression<Func<TEntity, bool>> keyConditionPredicate);
        QueryExpression<TEntity> Query(DynamoLocalIndex index, Expression<Func<TEntity, bool>> keyConditionPredicate);
        QueryExpression<TEntity> QueryComposite(Expression<Func<TEntity, bool>> keyConditionPredicate);

        Task<TEntity> GetByIdAsync(TKey id, object range);
        Task DeleteAsync(TKey id);
        Task DeleteAsync(TKey id, object range);
        Task<ScanExpression<TEntity>> ScanAsync(Expression<Func<TEntity, bool>> predicate);
        Task<QueryExpression<TEntity>> QueryAsync(Expression<Func<TEntity, bool>> keyConditionPredicate);
        Task<QueryExpression<TEntity>> QueryAsync(DynamoGlobalIndex index, Expression<Func<TEntity, bool>> keyConditionPredicate);
        Task<QueryExpression<TEntity>> QueryAsync(DynamoLocalIndex index, Expression<Func<TEntity, bool>> keyConditionPredicate);
        Task<QueryExpression<TEntity>> QueryCompositeAsync(Expression<Func<TEntity, bool>> keyConditionPredicate);

    }
}
