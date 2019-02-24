using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Amazon.DynamoDBv2;
using RevStackCore.Pattern;
using RevStackCore.DynamoDb.Client;

namespace RevStackCore.DynamoDb
{
    public class DynamoDbRepository<TEntity, TKey> : IDynamoDbRepository<TEntity, TKey> where TEntity : class, IEntity<TKey>
    {
        private readonly IPocoDynamo _db;
        public DynamoDbRepository(AmazonDynamoDBClient dbContext)
        {
            _db = new PocoDynamo(dbContext);
        }

        public IPocoDynamo DbClient
        {
            get
            {
                return _db;
            }
        }

        public TEntity Add(TEntity entity)
        {
            return _db.PutItem(entity);
        }

        public void Delete(TEntity entity)
        {
            _db.DeleteByItem<TEntity>(entity);
        }

        public void Delete(TKey id)
        {
            _db.DeleteItem<TEntity>(id);
        }

        public void Delete(TKey id, object range)
        {
            _db.DeleteItem<TEntity>(id,range);
        }

        public IQueryable<TEntity> Find(Expression<Func<TEntity, bool>> predicate)
        { 
            var result = _db.FromScan<TEntity>(predicate).Exec();
            return result.AsQueryable();
        }

        public ScanExpression<TEntity> Scan(Expression<Func<TEntity, bool>> predicate)
        {
            return _db.FromScan<TEntity>(predicate);
        }

        public QueryExpression<TEntity> Query(Expression<Func<TEntity, bool>> keyConditionPredicate)
        {
            return _db.QueryIndex(keyConditionPredicate);
        }

        public QueryExpression<TEntity> Query(DynamoGlobalIndex index,Expression<Func<TEntity, bool>> keyConditionPredicate)
        {
            return _db.QueryIndex(index,keyConditionPredicate);
        }

        public QueryExpression<TEntity> Query(DynamoLocalIndex index, Expression<Func<TEntity, bool>> keyConditionPredicate)
        {
            return _db.QueryIndex(index, keyConditionPredicate);
        }

        public QueryExpression<TEntity> QueryComposite(Expression<Func<TEntity, bool>> keyConditionPredicate)
        {
            return _db.QueryComposite(keyConditionPredicate);
        }

        public IEnumerable<TEntity> Get()
        {
            return _db.GetAll<TEntity>();
        }

        public TEntity GetById(TKey id)
        {
            return _db.GetItem<TEntity>(id);
        }

        public TEntity GetById(TKey id, object range)
        {
            return _db.GetItem<TEntity>(id,range);
        }

        public TEntity Update(TEntity entity)
        {
            _db.PutItem<TEntity>(entity);
            return entity;
        }
    }
}
