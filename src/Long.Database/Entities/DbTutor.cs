namespace Long.Database.Entities
{
    [Table("cq_tutor")]
    public class DbTutor
    {
        [Key][Column("id")] public virtual uint Identity { get; set; }
        [Column("user_id")] public virtual uint UserId { get; set; }
        [Column("user_name")] public virtual string UserName { get; set; }
        [Column("tutor_id")] public virtual uint TutorId { get; set; }
        [Column("tutor_name")] public virtual string TutorName { get; set; }
        [Column("betrayal_flag")] public virtual int BetrayalFlag { get; set; }
        [Column("join_date")] public virtual uint Date { get; set; }
    }
}
