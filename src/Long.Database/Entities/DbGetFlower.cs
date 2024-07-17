namespace Long.Database.Entities
{
    [Table("cq_getflower")]
    public class DbGetFlower
    {
        [Key][Column("id")] public virtual uint Id { get; set; }
        [Column("present_id")] public virtual uint PresentId { get; set; }
        [Column("accept_id")] public virtual uint AcceptId { get; set; }
        [Column("times")] public virtual int Times { get; set; }
    }
}
