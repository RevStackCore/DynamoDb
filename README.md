# RevStackCore.DynamoDb

[![Build status](https://ci.appveyor.com/api/projects/status/i416rbu5hbxjr52x?svg=true)](https://ci.appveyor.com/project/tachyon1337/dynamodb)


A DynamoDb implementation of the RevStackCore repository pattern

# Nuget Installation

``` bash
Install-Package RevStackCore.DynamoDb

```

# Repositories

```cs
public interface IRepository<TEntity, TKey> where TEntity : class, IEntity<TKey>
{
    IEnumerable<TEntity> Get();
    TEntity GetById(TKey id);
    IQueryable<TEntity> Find(Expression<Func<TEntity, bool>> predicate);
    TEntity Add(TEntity entity);
    TEntity Update(TEntity entity);
    void Delete(TEntity entity);
}
 public interface IDynamoDbRepository<TEntity,TKey> : IRepository<TEntity,TKey> where TEntity:class, IEntity<TKey>
 {
     IPocoDynamo DbClient { get;  }
     TEntity GetByHashId(object id);
     TEntity GetById(object id, object range);
     void Delete(object id);
     void Delete(object id, object range);
     ScanExpression<TEntity> Scan(Expression<Func<TEntity, bool>> filterExpressionPredicate);
     QueryExpression<TEntity> Query(Expression<Func<TEntity, bool>> keyConditionPredicate);
     QueryExpression<TEntity> Query(DynamoGlobalIndex index, Expression<Func<TEntity, bool>> keyConditionPredicate);
     QueryExpression<TEntity> Query(DynamoLocalIndex index, Expression<Func<TEntity, bool>> keyConditionPredicate);
     QueryExpression<TEntity> QueryComposite(Expression<Func<TEntity, bool>> keyConditionPredicate);
 }
 public class DynamoDbRepository<TEntity, TKey> : IDynamoDbRepository<TEntity, TKey> where TEntity : class, IEntity<TKey>
```

# Implementations
DynamoDbRepository<TEntity,Tkey> implements IRepository<TEntity,TKey> for basic Crud operations and Find
DynamoDbRepository<TEntity,Tkey> implements IDynamoDbRepository<TEntity,TKey> for Crud + DynamoDb specific query operations

## Notes on Implementations
Get(), i.e, Get All, and Find() rely on scans. Because the AWS service cost for DynamoDb is based on provisioned throughput, scans are generally regarded as a last resort practice for querying DynamoDb. A phonebook perhaps is the simplest way to analogize DynamoDb. If you know the last name & first name(i.e, composite key), it is a simple lookup to find the phone number associated with that piece of information. However, if one instead wanted to query all phone numbers in a given city, the default method would have to rely on "scanning" the entire phone book to properly build the return data set. Note: for nontrivially sized tables, this can become expensive. 

# Key Schemas

## Primary Key Schema
Although DynamoDb is referred to as "schemaless," it does enforce a primary key schema. Every item in a table is uniquely identified by its primary key. The primary key must be included with every item that is written to a DynamoDB table.  A simple primary key, the hash key, uses a single attribute to identify an item.

A composite primary key uses a combination of two attributes to identify a particular item. The first attribute is the hash key. The second attribute is the range key which is used to order items with the same hash key. In a composite primary key schema, the hash keys do not have to be unique.

## Secondary Key Schemas
Table lookups by primary key schemas are limited by the primary key uniqueness constraint, which limits the query capabilities. Although this limitation can be circumvented by scanning, a table scan is an inefficient(and expensive) way to query data. Furtunately, DynamoDb can be queried using a secondary key schema. Unlike primary keys, secondary keys, or secondary indexes, do not have to be unique.

## Local Secondary Indexes
LSIs essentially are an additonal sort key for a primary key lookup. They are only useful if the Table has a composite key defined where the hash keys are not expected to be unique. For example, consider a DynamoDb Table, UserComments. UserId==hash key, Timestamp==range key. UserId + Timestamp is a unique value, but UserId can be duplicates. A hash key lookup will return a List. You can sort the list by Timestamp. However,if you wanted another sort order on the hash key lookup--or wanted a key condition filter--by, say, Rating, you can define a local secondary index on Rating. So, a LSI (hash key=UserId, range key=Rating) lookup will return comments by UserId sorted by Rating, or it can serve as a key condition expression to return only, say, the user's comments rated greater than 3 stars. Like the primary key schema, LSIs have to be defined at Table creation time.


## Global Secondary Indexes
GSIs essentially are provisioned copies of the underlying Table using non-key attributes to define a new partition. Like the primary key schema, a global secondary key schema can be simple or composite. But unlike a primary key schema, there are no unqiuness constraints on the secondary key, be it a simple hash key or composite. Unlike a local secondary key, a global secondary hash key can be any table attribute. Each Global secondary index has separate privisioned Read/Write capacity and because item writes are asynchronously replicated to global secondary indexes, read operations are required to be "eventual consistenct."

As an example, consider a a DynamoDb Table, User, with simple hash primary key, UserId. To do lookups on,say, a City attribute, define a new GSI with the City attribute as the hash key. Then you can query on this secondary key schema(or index) using city as the key condition.

Unlike a local secondary index, global secondary indexes can be created at any time. However, there is a limit of 5 global secondary indexes per DynamoDb table.


# POCO First
Poco First, or Code First, means we create and define our DynamoDb tables from appropriately annotated C# classes. Consistent with the repository pattern, all Poco items must implement an Id prop(TKey Id). By default, Id will be the Table primary hash key. But it doesn't have to be. Use the [HashKey] attribute to explicitly set the primary hash key.

For example:
```cs
[ProvisionedThroughput(ReadCapacityUnits =10,WriteCapacityUnits =10)]
public class ProfileTable : IEntity<string>
{
    [HashKey]
    public string Id { get; set; }
    public string Name { get; set; }
    [GlobalSecondaryIndexHashKey("Name")]
    public string City { get; set; }
    public string State { get; set; }
    public string Email { get; set; }

    public ProfileTable()
    {
        Id = Guid.NewGuid().ToString();
    }
}
```

## Data Annotations
```cs
  //Table Read/Write throughput capacity table attribute
  [ProvisionedThroughput(ReadCapacityUnits =10,WriteCapacityUnits =10)]

  //Hash Key property attribute
  [HashKey]
  
  //Range Key property attribute
  [RangeKey]

  //Local Secondary Index property attribute
  [Index]

  //Global Secondary Index property attribute
  [GlobalSecondaryIndexHashKey]

  //Global Secondary Index with Range Key
  [GlobalSecondaryIndexHashKey("RangeKeyProperty)]
```


## Register Table
Register your tables at startup
```cs
private static void RegisterTables(IPocoDynamo db)
{
    db.RegisterTable<MyTable>();
    db.InitSchema();
}
```
## Secondary Key Schema Pattern
Code First InitSchema() will create local and global secondary indexes according to the following pattern
```cs

//LSI
TableName-LSI-HashKey-Prop-Index
//ex: MyTable-LSI-Id-Rating-Index

//GSI
TableName-LSI-HashKey-RangeKey-Index
//ex: MyTable-GSI-City-ZipCode-Index
```

# CRUD

```cs
//add
_repository.Add(item);

//update
_repository.Update(item);

//delete
_repository.Delete(item)

//Get Item By Id, if TKey Id==hash key
_repository.GetById(id)

//Get Item by composite key
_repository.GetById(hashKey, rangeKey)

//Get Item by hash key
_repository.GetByHashId(hashKey);


```

# Queries
DynamoDbRepository 4 public methods for querying 

```cs
QueryExpression<TEntity> Query(Expression<Func<TEntity, bool>> keyConditionPredicate);
QueryExpression<TEntity> Query(DynamoGlobalIndex index, Expression<Func<TEntity, bool>> keyConditionPredicate);
QueryExpression<TEntity> Query(DynamoLocalIndex index, Expression<Func<TEntity, bool>> keyConditionPredicate);
QueryExpression<TEntity> QueryComposite(Expression<Func<TEntity, bool>> keyConditionPredicate);

```
QueryExpression<T> inherits from AWS SDK QueryRequest class and provides a fluent api for lazy evaluation.

```cs
Query(Expression<Func<TEntity, bool>> keyConditionPredicate)
```

Sets up a query on the global secondary index specified by the keyCondition predicate. To execute the query, call the Exec([limit]) extension method. Exec returns the IEnumerable<T> results.

```cs
var result=_repository.Query(x=>x.State=="GA").Exec(); //==>List<T>

//fluent api
var result=_repository.Query(x=>x.State=="GA").Filter(x=>x.City=="Atlanta").OrderByDescending().Exec(5); //==>sorted List<T>, 5 results
```

```cs
Query(DynamoGlobalIndex index, Expression<Func<TEntity, bool>> keyConditionPredicate)
```

If the global secondary index doesn't match the Poco First pattern(e.g, you manually created it in the AWS console), you will need to pass an instance of DynamoGlobalIndex in the Query overload method.

```cs
var index=new DynamoGlobalIndex { Name="MyGlobaIndexSchemaName"};
var result=_repository.Query(index,x=>x.State=="GA").Exec(); //==>List<T>

```

```cs
Query(DynamoLocalIndex index, Expression<Func<TEntity, bool>> keyConditionPredicate)
```

```cs
var index=new DynamoLocalIndex { Property="IndexProp"}; 
//var index=new DynamoLocalIndex { Name="MyLocalIndexSchemaName"}; // local secondary index doesn't match the Poco First pattern

//key condition predicate is on the primary hash key
var result=_repository.Query(index,x=>x.UserId=="12345").OrderByDescending().Exec(); //==>List<T>

```

```cs
QueryComposite(Expression<Func<TEntity, bool>> keyConditionPredicate)
```

```cs
//key condition predicate is on the primary hash key
var result=_repository.QueryComposite(index,x=>x.UserId=="12345").Exec(); //==>List<T>
```

## Paging
Implement paging by calling the ExecPage(limit,[Dictionary<string, AttributeValue> lastEvaluatedKey]) extension method. ExecPage returns a DynamoDbResult.

```cs
public class DynamoDbResult<T> 
{
    public IEnumerable<T> Data { get; set; }
    public Dictionary<string,AttributeValue> LastEvaluatedKey { get; set; }
}
```

```cs
var result = _repository.Query(x => x.City == "Charlotte").ExecPage(10); //result.Data==1st page of List<T> 10 items
var result2 = _repository.Query(x => x.City == "Charlotte").ExecPage(10,result.LastEvaluatedKey); //result.Data==Next page of List<T> 10 items

```
# Scans 
DynamoDbRepository exposes 1 public method for lazy evaluation of scan expressions

```cs
ScanExpression<TEntity> Scan(Expression<Func<TEntity, bool>> filterExpressionPredicate)
```

```cs
var result=_repository.Scan(x=>x.Referrer=="GOOGLE").Exec();
```



# Usage

## AmazonDynamoDBClient
Inject an instance of AmazonDynamoDBClient with your AWS credentials and Region information
```cs
var awsDbClient = new AmazonDynamoDBClient(AWS_ACCESS_KEY, AWS_SECRET_KEY, RegionEndpoint.USEast1);
```

## Dependency Injection

```cs
using Amazon.DynamoDBv2;
using RevStackCore.DynamoDb.Client;
using RevStackCore.Pattern;
using RevStackCore.DynamoDb;

class Program
{
    static void main(string[] args)
    {
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var dataService = serviceProvider.GetService<IDataService>();
        var myRepository = serviceProvider.GetService<IDynamoDbRepository<MyTable, string>>();
        var db = myRepository.DbClient;
        RegisterTables(db);
        dataService.myMethod();
    }
    private static void ConfigureServices(IServiceCollection services)
    {
        services
        .AddSingleton(x => new AmazonDynamoDBClient(AWS_ACCESS_KEY, AWS_SECRET_KEY, RegionEndpoint.USEast1))
        .AddSingleton<IDynamoDbRepository<MyTable, string>, DynamoDbRepository<MyTable, string>>()
        .AddSingleton<IDataService, DataService>()
    }

    private static void RegisterTables(IPocoDynamo db)
    {
        db.RegisterTable<MyTable>();
        db.InitSchema();
    }
}

```

# AspNetCore Identity framework
DynamoDbRepository can be plugged into the RevStackCore generic implementation of the AspNetCore Identity framework
https://github.com/RevStackCore/Identity

# Asynchronous Services
```cs
IPocoDynamo DbClient { get; }
TEntity GetByHashId(object id);
TEntity GetById(object id, object range);
void Delete(object id);
void Delete(object id, object range);
ScanExpression<TEntity> Scan(Expression<Func<TEntity, bool>> predicate);
QueryExpression<TEntity> Query(Expression<Func<TEntity, bool>> keyConditionPredicate);
QueryExpression<TEntity> Query(DynamoGlobalIndex index, Expression<Func<TEntity, bool>> keyConditionPredicate);
QueryExpression<TEntity> Query(DynamoLocalIndex index, Expression<Func<TEntity, bool>> keyConditionPredicate);
QueryExpression<TEntity> QueryComposite(Expression<Func<TEntity, bool>> keyConditionPredicate);

Task<TEntity> GetByHashIdAsync(object id);
Task<TEntity> GetByIdAsync(Object id, object range);
Task DeleteAsync(object id);
Task DeleteAsync(object id, object range);
Task<ScanExpression<TEntity>> ScanAsync(Expression<Func<TEntity, bool>> predicate);
Task<QueryExpression<TEntity>> QueryAsync(Expression<Func<TEntity, bool>> keyConditionPredicate);
Task<QueryExpression<TEntity>> QueryAsync(DynamoGlobalIndex index, Expression<Func<TEntity, bool>> keyConditionPredicate);
Task<QueryExpression<TEntity>> QueryAsync(DynamoLocalIndex index, Expression<Func<TEntity, bool>> keyConditionPredicate);
Task<QueryExpression<TEntity>> QueryCompositeAsync(Expression<Func<TEntity, bool>> keyConditionPredicate);

```

# Implementations
DynamoDbService<TEntity,Tkey> implements IService<TEntity,TKey> for basic Async Crud operations and FindAsync
DynamoDbService<TEntity,Tkey> implements IDynamoDbService<TEntity,TKey> for Async Crud + DynamoDb query operations


















