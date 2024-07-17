using Long.Database.Entities;

namespace Long.Kernel.Database.Repositories
{
    public static class GradeRepository
    {
        public static List<DbGrade> Get()
        {
            using var ctx = new ServerDbContext();
            return ctx.Grades.ToList();
        }
    }
}
