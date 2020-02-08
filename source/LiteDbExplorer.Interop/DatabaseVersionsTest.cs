extern alias v4;
extern alias v5;
using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using LiteDBv4 = v4::LiteDB;
using LiteDBv5 = v5::LiteDB;

namespace LiteDbExplorer.Interop
{
    public static class DatabaseVersionsTest
    {
        public static string ProgramFolder => Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);

        public static void CreateDatabases()
        {
            Console.WriteLine("Create v4 database");

            using(var db = new LiteDBv4.LiteDatabase(EnsureDataDirectory("DatabaseV4.db")))
            {
                // Get customer collection
                var col = db.GetCollection<Customer>("customers");

                // Create your new customer instance
                var customer = new Customer
                { 
                    Name = "John Doe", 
                    Phones = new string[] { "8000-0000", "9000-0000" }, 
                    Age = 39,
                    IsActive = true
                };

                // Create unique index in Name field
                // col.EnsureIndex(x => x.Name, true);

                col.Upsert(customer);

                // Use LINQ to query documents (with no index)
                var results = col.Find(x => x.Age > 20);

                Dump(results.ToList());
            }

            Console.WriteLine();
            Console.WriteLine("Create v5 database");
            using(var db = new LiteDBv5.LiteDatabase(EnsureDataDirectory("DatabaseV5.db")))
            {
                // var jsonInfo = JsonConvert.SerializeObject(LiteDBv5., Formatting.Indented);
                // Get customer collection
                var col = db.GetCollection<Customer>("customers");

                // Create your new customer instance
                var customer = new Customer
                { 
                    Name = "Julian Paulozzi", 
                    Phones = new string[] { "6000-0000", "7000-0000" }, 
                    Age = 24,
                    IsActive = false
                };

                // Create unique index in Name field
                // col.EnsureIndex(x => x.Name, true);

                col.Upsert(customer);

                // Use LINQ to query documents (with no index)
                var results = col.Find(x => x.Age > 20);

                Dump(results.ToList());
            }
            
        }


        private static string EnsureDataDirectory(string fileName)
        {
            var path = Path.Combine(ProgramFolder, "Data");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return Path.Combine(path, fileName);
        }

        private static void Dump(object value)
        {
            var json = JsonConvert.SerializeObject(value, Formatting.Indented);
            Console.WriteLine(json);
        }
    }

    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string[] Phones { get; set; }
        public bool IsActive { get; set; }
    }
}