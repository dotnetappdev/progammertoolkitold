using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace ProgrammersToolKit.Database
{
    public class NoteEntity
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string EncryptedContent { get; set; } = string.Empty;
        public string Salt { get; set; } = string.Empty;
        public string IV { get; set; } = string.Empty;
    }

    public class NotepadDbContext : DbContext
    {
        public DbSet<NoteEntity> Notes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=notepad.db");
        }
    }
}
