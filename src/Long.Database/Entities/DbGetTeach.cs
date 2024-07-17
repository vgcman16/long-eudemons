namespace Long.Database.Entities
{
    [Table("cq_getteach")]
    public class DbGetTeach
    {
        [Key][Column("id")] public virtual uint Id { get; set; }
        [Column("teacher_id")] public virtual uint TeacherId { get; set; }
        [Column("student_id")] public virtual uint StudentId { get; set; }
        [Column("times")] public virtual int Times { get; set; }
    }
}
