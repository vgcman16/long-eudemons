namespace Long.Database.Entities
{
    [Table("cq_announce")]
    public class DbAnnounce
    {
        [Key][Column("id")] public virtual uint Id { get; set; }
        [Column("user_id")] public virtual uint UserId { get; set; }
        [Column("name")] public virtual string UserName { get; set; }
        [Column("level")] public virtual byte Level { get; set; }
        [Column("teacherlevel")] public virtual byte TeacherLevel { get; set; }
        [Column("profession")] public virtual byte Profession { get; set; }
        [Column("title")] public virtual string Title { get; set; }
        [Column("content")] public virtual string Content { get; set; }
    }
}
