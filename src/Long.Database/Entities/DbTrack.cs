namespace Long.Database.Entities
{
    [Table("cq_track")]
    public class DbTrack
    {
        [Key][Column("id")] public virtual uint Id { get; set; }
        [Column("id_next")] public virtual uint IdNext { get; set; }
        [Column("direction")] public virtual byte Direction { get; set; }
        [Column("step")] public virtual byte Step { get; set; }
        [Column("alt")] public virtual byte Alt { get; set; }
        [Column("action")] public virtual uint Action { get; set; }
        [Column("power")] public virtual int Power { get; set; }
        [Column("apply_ms")] public virtual int ApplyMs { get; set; }
    }
}
