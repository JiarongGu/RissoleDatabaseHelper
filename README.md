# RissoleDatabaseHelper
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
examples.Select(x => x).Join<Test>((x, y) => x.ExampleId == y.ExampleId).Where(x => x.ExampleId == {ExampleId}).Exec();
```

Also：

```
var command = examples.Select(x => x).Join<Test>((x, y) => x.ExampleId == y.ExampleId)
command = command.Join<Test, Unit>(x => x.TestId == TestId).Where(x => x.TestId == {TestId});
return command.Exec();
```

#### Add custom command for more complex query building

```
examples.Select(x => x).Custom("ORDER BY 1 DESC");

List<IDbDataParameter> paramters = new ...;
examples.Select(x => x).Custom("WHERE examples_id == @ExampleId AND content LIKE '%@Content%'", parameters});
```

Also：

```
examples.Select(x => x).Custom("WHERE examples_id == @ExampleId AND content LIKE '%@Content%'", parameter1, parameter2 ...);
```