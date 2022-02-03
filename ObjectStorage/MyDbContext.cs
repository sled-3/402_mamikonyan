using System.Data.Entity;

namespace ObjectStorage
{
    public class MyDbContext : DbContext
    {
        public MyDbContext() : base("ConnStr")
        {

        }
        public DbSet<Foto> Fotos { get; set; }
        public DbSet<YoloObject> YoloObjects { get; set; }
    }
}
