namespace Long.Database.Entities
{
    [Table("cq_soul_value_lev")]
    public class DbSoulValueLev
    {
        [Key][Column("id")] public virtual uint Identity { get; set; }
        [Column("soul_lev")] public virtual byte SoulLevel { get; set; }
        [Column("req_lev")] public virtual byte ReqLevel { get; set; }
        [Column("soul_max")] public virtual int SoulMaxValue { get; set; }
        [Column("prof_sort")] public virtual byte ProfSort { get; set; }
    }
}
