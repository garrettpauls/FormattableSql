# FormattableSql

A simple library for using C# 6 FormattableString to execute SQL commands.

[![Build status](https://ci.appveyor.com/api/projects/status/1e3kn4qy7xkgiaxk?svg=true)](https://ci.appveyor.com/project/garrettpauls/formattablesql)

# Supported backends

This library should work with any backend that supports ADO.NET and query parameters,
but it's only been tested with [System.Data.SQLite](https://www.nuget.org/packages/System.Data.SQLite.Core/).
You're on your own with any other providers.

# Examples

The following code illustrates simple usage.
A working example can be found in [Examples/BasicUsage](Examples/BasicUsage).
Take note of the connection string and provider stored in the app.config.

	var sql = FormattableSqlFactory.ForConnectionString("main");
	await sql.ExecuteAsync(@"
		CREATE TABLE IF NOT EXISTS Person
		( Id           INTEGER PRIMARY KEY AUTOINCREMENT
		, Name         TEXT NOT NULL
		, FavoriteWord TEXT NOT NULL
		);
	");

	// Insert data
	await sql.ExecuteManyParamsAsync(
		person => $"INSERT INTO Person (Name, FavoriteWord) VALUES ({person.Name}, {person.FavoriteWord})",
		new Person
		{
			Name = "Samantha",
			FavoriteWord = "Orthogonal"
		},
		new Person
		{
			Name = "Johannes",
			FavoriteWord = "Pythagorean"
		});

	// Query data
	var wordQuery = "%e%";
	var results = await sql.QueryAsync(
		$"SELECT Id, Name, FavoriteWord FROM Person WHERE FavoriteWord like {wordQuery}",
		async row => new Person
		{
			Id = await row.GetValueAsync<long>("Id"),
			Name = await row.GetValueAsync<string>("Name"),
			FavoriteWord = await row.GetValueAsync<string>("FavoriteWord")
		});

	// Update data
	await sql.ExecuteManyAsync(
		person => $"UPDATE Person SET FavoriteWord = {person.FavoriteWord} WHERE Id = {person.Id}",
		results);

	// Execute scalar
	var personCount = await sql.ExecuteScalarAsync<long>($"SELECT COUNT(*) FROM Person");
	Console.WriteLine($"Total number of people: {personCount}");

	// Delete data
	await sql.ExecuteManyAsync(
		id => $"DELETE FROM Person WHERE Id = {id}",
		results.Select(person => person.Id));

# Usage

FormattableSql is powered by the `IFormattableSqlProvider` interface
and `FormattableSqlProvider` implementation.

`FormattableSqlProvider` requires an instance of `ISqlProvider`
which has several implementations for varying uses in the
`FormattableSql.Core.Data.Provider` namespace.

## `SqlProvider`

`SqlProvider` is an abstract base class that can be used to fulfill
most functionality of `ISqlProvider`.

## `AdoNetSqlProvider`

This implementation integrates with ADO.NET and can be created
with just a connection string name.
It integrates with ADO.NET and supports app.config based configurations.

## `GenericSqlProvider`

This implementation requires a single method to act as a connection factory
which returns a new `DbConnection`. If you have to integrate with
a non-ADO.NET provider, this is the simplest approach.
