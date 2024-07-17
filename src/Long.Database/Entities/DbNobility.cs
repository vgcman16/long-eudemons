namespace Long.Database.Entities
{
    [Table("cq_donation_dynasort_rec")]
    public class DbNobility
    {
        [Key]
        [Column("id")] public virtual uint Id { get; set; }
        [Column("user_id")] public virtual uint UserId { get; set; }
        [Column("user_name")] public virtual string UserName { get; set; }
        [Column("value")] public virtual ulong Value { get; set; }
    }
}
