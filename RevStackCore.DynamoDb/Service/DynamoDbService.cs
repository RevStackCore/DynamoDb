using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using RevStackCore.DynamoDb.Client;
using RevStackCore.Pattern;

namespace RevStackCore.DynamoDb
{
    public class DynamoDbService<TEntity, TKey> : IDynamoDbService<TEntity, TKey> where TEntity : class, IEntity<TKey>
    {
        private readonly IDynamoDbRepository<TEntity, TKey> _repository;
        public DynamoDbService(IDynamoDbRepository<TEntity, TKey> repository)
        {
            _repository = repository;
        }

        public IPocoDynamo DbClient
        {
            get
            {
                return _repository.DbClient;
            }
        }

        public TEntity Add(TEntity entity)
        {
            return _repository.Add(entity);
        }

        public Task<TEntity> AddAsync(TEntity entity)
        {
            return Task.FromResult(Add(entity));
        }

        public void Delete(TEntity entity)
        {
            _repository.Delete(entity);
        }

        public void Delete(TKey id)
        {
            _repository.Delete(id);
        }

        public void Delete(TKey id, object range)
        {
            _repository.Delete(id, range);
        }

        public Task DeleteAsync(TEntity entity)
        {
            return Task.Run(() => Delete(entity));
        }

        public Task DeleteAsync(TKey id)
        {
            return Task.Run(() => Delete(id));
        }

        public Task DeleteAsync(TKey id, object range)
        {
            return Task.Run(() => Delete(id,range));
        }

        public IQueryable<TEntity> Find(Expression<Func<TEntity, bool>> predicate)
        {
            return _repository.Find(predicate);
        }

        public Task<IQueryable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return Task.FromResult(Find(predicate));
        }

        public IEnumerable<TEntity> Get()
        {
            return _repository.Get();
        }

        public Task<IEnumerable<TEntity>> GetAsync()
        {
            return Task.FromResult(Get());
        }

        public TEntity GetById(TKey id)
        {
            return _repository.GetById(id);
        }

        public TEntity GetById(TKey id, object range)
        {
            return _repository.GetById(id, range);
        }

        public Task<TEntity> GetByIdAsync(TKey id)
        {
            return Task.FromResult(GetById(id));
        }

        public Task<TEntity> GetByIdAsync(TKey id, object range)
        {
            return Task.FromResult(GetById(id, range));
        }

        public QueryExpression<TEntity> Query(Expression<Func<TEntity, bool>> keyConditionPredicate)
        {
            return _repository.Query(keyConditionPredicate);
        }

        public QueryExpression<TEntity> Query(DynamoGlobalIndex index, Expression<Func<TEntity, bool>> keyConditionPredicate)
        {
            return _repository.Query(index,keyConditionPredicate);
        }

        public QueryExpression<TEntity> Query(DynamoLocalIndex index, Expression<Func<TEntity, bool>> keyConditionPredicate)
        {
            return _repository.Query(index,keyConditionPredicate);
        }

        public Task<QueryExpression<TEntity>> QueryAsync(Expression<Func<TEntity, bool>> keyConditionPredicate)
        {
            return Task.FromResult(Query(keyConditionPredicate));
        }

        public Task<QueryExpression<TEntity>> QueryAsync(DynamoGlobalIndex index, Expression<Func<TEntity, bool>> keyConditionPredicate)
        {
            return Task.FromResult(Query(index, keyConditionPredicate));
        }

        public Task<QueryExpression<TEntity>> QueryAsync(DynamoLocalIndex index, Expression<Func<TEntity, bool>> keyConditionPredicate)
        {
            return Task.FromResult(Query(index, keyConditionPredicate));
        }

        public QueryExpression<TEntity> QueryComposite(Expression<Func<TEntity, bool>> keyConditionPredicate)
        {
            return _repository.QueryComposite(keyConditionPredicate);
        }

        public Task<QueryExpression<TEntity>> QueryCompositeAsync(Expression<Func<TEntity, bool>> keyConditionPredicate)
        {
            return Task.FromResult(QueryComposite(keyConditionPredicate));
        }

        public ScanExpression<TEntity> Scan(Expression<Func<TEntity, bool>> predicate)
        {
            return _repository.Scan(predicate);
        }

        public Task<ScanExpression<TEntity>> ScanAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return Task.FromResult(Scan(predicate));
        }

        public TEntity Update(TEntity entity)
        {
            return _repository.Update(entity);
        }

        public Task<TEntity> UpdateAsync(TEntity entity)
        {
            return Task.FromResult(Update(entity));
        }
    }
}
