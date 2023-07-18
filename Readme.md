# Pandatech.NpgsqlBinaryCopyHelper

Pandatech.NpgsqlBinaryCopyHelper is a NuGet package that simplifies the process of performing binary copy operations
using Npgsql in .NET applications. It provides a convenient and strongly typed API to streamline the insertion of data
into a PostgreSQL database using the binary copy feature.

## Features

- Supports strongly typed models for binary copy operations.
- Provides options to insert data with or without code-generated unique IDs for each model.
- Simplifies the process of writing raw SQL statements, opening and closing connections, and handling exceptions.
- Provides logging capabilities to help with debugging and troubleshooting.
- Supports asynchronous operations.

## Installation

You can install the Pandatech.NpgsqlBinaryCopyHelper package via NuGet package manager or by using the .NET CLI.

## Usage

The `Pandatech.NpgsqlBinaryCopyHelper` package enhances the native Npgsql binary copy functionality with strongly-typed
support. Follow the steps below to use the package effectively:

1. Import the `Pandatech.NpgsqlBinaryCopyHelper` namespace:

```csharp
using Pandatech.NpgsqlBinaryCopyHelper;
```

2. Set the logger (optional):

```csharp
CopyHelper.SetLogger(logger);
```

This allows you to specify a logger that will be used to log debug, informational, and error messages during the binary
copy process.

3. Perform the binary copy operation:

```csharp
await modelList.CopyAsync(context, insertWithPrimaryKeys: false, convertEnumToInt: true);
```

- modelList: A list of models to be inserted.
- context: The DbContext instance used to retrieve the database connection.
- insertWithPrimaryKeys (optional): A boolean value indicating whether to insert the models with their primary keys. If
  set to
  false, the primary keys will be ignored and the database will generate unique IDs for each model. The default
  is `false`.
- convertEnumToInt (optional): A boolean value indicating whether to convert enum values to integers. If set to true,
  the enum values will be converted to integers before being inserted into the database. If enum values are integers in
  postgres database, set this to true. The default is `true`.

Note: This is a heavy operation, and caution should be exercised when using it. Ensure that the list of models is not
null or empty and that the DbContext instance is properly initialized.

## Example

Assuming you have a model class named Person:

```csharp
public class Person
{
    public int Id { get; set; }
    public string Name { get; set; }
}
```

And you have a list of Person models:

```csharp
var personList = new List<Person>
{
    new Person { Id = 1, Name = "John" },
    new Person { Id = 2, Name = "Jane" }
};
```

To perform a binary copy using the `Pandatech.NpgsqlBinaryCopyHelper`:

```csharp
using (var context = new MyDbContext())
{
    await personList.CopyAsync(context, insertWithPrimaryKeys: true);
}
```

## Important Considerations

- This library assumes that you are using Entity Framework Core (EF Core) with the Code First approach. It relies on the
  DbContext to retrieve the database connection string and execute the binary copy operation.

- By default, this library uses the naming conventions retrieved from DbContext. If you have customized table or column
  names in the database, make sure they match the naming conventions in your code.

- Caution: The binary copy operation performs bulk inserts directly into the database and bypasses certain validations
  and constraints. Ensure that the data being inserted is valid and consistent with the database schema.

- It is recommended to thoroughly test the usage of this library and perform performance tests in your specific
  scenarios to ensure it meets your requirements.

- Make sure to handle exceptions appropriately when using this library. Any exceptions thrown during the binary copy
  process should be caught and handled accordingly.

## Troubleshooting

If you encounter any issues or have questions, feel free to reach out to the Pandatech.NpgsqlBinaryCopyHelper community
for support.

## License

This project is licensed under the MIT License.