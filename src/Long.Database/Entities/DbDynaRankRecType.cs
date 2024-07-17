namespace Long.Database.Entities
{
    [Table("cq_dyna_rank_type")]
    public class DbDynaRankRecType
    {
        [Key][Column("id")] public virtual uint Id { get; set; }
        [Column("type")] public virtual uint RankType { get; set; }
        [Column("rank_number")] public virtual int RankSize { get; set; }
    }
}
