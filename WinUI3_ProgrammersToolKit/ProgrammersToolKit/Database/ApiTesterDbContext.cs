using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace ProgrammersToolKit.Database
{
    public class ApiCallEntity
    {
        public int Id { get; set; }
        public string Method { get; set; } = "GET";
        public string Url { get; set; } = string.Empty;
        public string HeadersJson { get; set; } = string.Empty; // JSON serialized headers
        public string JsonBody { get; set; } = string.Empty;
        public string? ExpectedStatus { get; set; }
        public string? ExpectedJson { get; set; }
        public string? ExpectedXml { get; set; }
        public string? ValueAssertionsJson { get; set; } // JSON serialized list of value assertions
        public string? TestName { get; set; } // Optional test name
    }

    public class ApiTesterDbContext : DbContext
    {
        public DbSet<ApiCallEntity> ApiCalls { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=apitester.db");
        }
    }
}
