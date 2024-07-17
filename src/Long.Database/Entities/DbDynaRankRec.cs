namespace Long.Database.Entities
{
    [Table("cq_dyna_rank_rec")]
    public class DbDynaRankRec
    {
        [Key][Column("id")] public virtual uint Id { get; set; }
        [Column("type")] public virtual uint RankType { get; set; }
        [Column("value")] public virtual long Value { get; set; }
        [Column("obj_id")] public virtual uint ObjId { get; set; }
        [Column("dataStr")] public virtual string DataStr { get; set; }
        [Column("user_id")] public virtual uint UserId { get; set; }
        [Column("user_name")] public virtual string UserName { get; set; }
        [Column("eud_cardid")] public virtual uint EudCardId { get; set; }

        public virtual DbUser User { get; set; }
    }
}
