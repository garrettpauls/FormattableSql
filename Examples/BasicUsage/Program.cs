using FormattableSql.Core;
using System;
using System.Linq;

namespace BasicUsage
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            _Main();
        }

        private static async void _Main()
        {
            var sql = FormattableSqlFactory.ForConnectionString("main");

            // Create a table
            await sql.ExecuteAsync($@"
DROP TABLE IF EXISTS Person;
CREATE TABLE Person
( Id           INTEGER PRIMARY KEY AUTOINCREMENT
, Name         TEXT NOT NULL
, FavoriteWord TEXT NOT NULL
);");

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

            Console.WriteLine("The following people have a favorite word containing 'e':");
            foreach (var person in results)
            {
                Console.WriteLine($"{person.Name}: {person.FavoriteWord}");
                person.FavoriteWord = person.FavoriteWord + "!";
            }

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

            personCount = await sql.ExecuteScalarAsync<long>($"SELECT COUNT(*) FROM Person");
            Console.WriteLine($"Total number of people after delete: {personCount}");

            Console.Write("Press enter to exit...");
            Console.ReadLine();
        }
    }

    public class Person
    {
        public string FavoriteWord { get; set; }
        public long Id { get; set; }
        public string Name { get; set; }
    }
}
