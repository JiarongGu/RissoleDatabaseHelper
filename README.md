# RissoleDatabaseHelper - Alapa
Simple ORM database helper liabrary for .net core. 
To be easier use the basic connected method read/write with a range of database types

## Getting Started

Download and include this liabray into your project.

### Prerequisites

```
.net core 1.1+
```


## How To User

#### Create your class with "Flag attributes", binding the class object to matched table values.

```
[Table("examples")]
public class Example
{
    [Column(Name = "examples_id", IsComputed = true, DataType = typeof(Guid))]
    [PrimaryKey]
    public Guid ExamplesId { get; set; }

    [Column("content")]
    public string ExampleContent { get; set; }
```


#### Create RissoleEntity Object to access the database, you can use any dbconnection you want as long as they supports IDbconnection

```
IDbConnection connection = new DbConnection("{your connection strings}");
IRissoleEntity<Example> examples = new RissoleEntity<Account>(connection);

// insert new object
var example = New Example(){ ExampleContent = "My Example Content" };
example = examples.Insert(example).Exec();

// simple select query
var seletedExmaples = examples.Select(x => x).Where(x => x.ExampleContent.Contains("My Example Content")).Exec();
```

The RissoleCommand builds the normal script one by one based on the creative method it triggers, also generate the DbDataParameter.
The Exec 


## Other Examples

#### Join with other table

```
// join with single table
examples.Select(x => x)
    .Join<Test>((x, y) => x.ExampleId == y.ExampleId)
    .Where(x => x.ExampleId == {ExampleId})
    .Exec();
```

```
var command = examples.Select(x => x).Join<Test>((x, y) => x.ExampleId == y.ExampleId)；

// add another join
command = command.Join<Test, Unit>(x => x.TestId == TestId).Where(x => x.TestId == {TestId});

// execuate command
command.Exec();
```


#### Add custom command for more complex query building

```
examples.Select(x => x).Custom("ORDER BY 1 DESC");
```

```
List<IDbDataParameter> paramters = new ...;

examples.Select(x => x).Custom("WHERE examples_id == @ExampleId AND content LIKE '%@Content%'", parameters});
```

```
examples.Custom("{Your Custom Command}",parameter1, parameter2, parameter3 ...);

examples.Exec();
```

## Execution Methods

.ExecuteReader<T>() - Similar as normal ExecuteReader but returns a list of T

.ExecuteNonQuery<T>() - Similar as normal ExecuteNonQuery return number of row effected

.ExecuteScalar<T>() - Similar as normal ExecuteScalar return output value

.Exec<T>() - Default option to execute different type of command return based on type of command

.First<T>() - Execute and return first T

.FirstOrDefault<T>() - Execute and return first T or null

.ToList<T>() - Execute and return list of T

.BuildCommand() - Build normal IDbCommand
