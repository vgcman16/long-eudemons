namespace Long.Database.Entities
{
    [Table("cq_partner")]
    public class DbPartner
    {
        [Key][Column("id")] public virtual uint Identity { get; set; }
        [Column("user_id")] public virtual uint UserId { get; set; }
        [Column("partner_id")] public virtual uint BusinessId { get; set; }
        [Column("partner_name")] public virtual string BusinessName { get; set; }
        [Column("date")] public virtual uint Date { get; set; }
    }
}
