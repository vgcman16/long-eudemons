namespace Long.Database.Entities
{
    [Table("cq_point_allot")]
    public class DbPointAllot
    {
        [Key][Column("id")] public virtual uint Identity { get; set; }

        [Column("profession")] public virtual byte Profession { get; set; }
        [Column("level")] public virtual byte Level { get; set; }
        [Column("force")] public virtual ushort Force { get; set; }
        [Column("dexterity")] public virtual ushort Dexterity { get; set; }
        [Column("health")] public virtual ushort Health { get; set; }
        [Column("soul")] public virtual ushort Soul { get; set; }
    }
}