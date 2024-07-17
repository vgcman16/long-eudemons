namespace Long.Database.Entities
{
    [Table("cq_synattr")]
    public class DbSyndicateAttr
    {
        [Key][Column("id")] public virtual uint UserIdentity { get; set; }
        [Column("syn_id")] public virtual uint SynId { get; set; }
        [Column("rank")] public virtual ushort Rank { get; set; }
        [Column("proffer_money")] public virtual long Proffer { get; set; }
        /// <summary>
        /// the value limit is 600, probably an expball by being online?
        /// </summary>
        [Column("TimeOnline")] public ushort TimeOnline { get; set; }
        /// <summary>
        /// is a unixtime probably related to login time.
        /// </summary>
        [Column("TimeDetected")] public uint TimeDetected { get; set; }
        [Column("proffer_inc")] public virtual uint ProfferInc { get; set; }
        [Column("memberlevel")] public byte Level { get; set; }
        [Column("auto_exercise")] public byte AutoExercise { get; set; }
        [Column("assistant_id")] public uint AssistantIdentity { get; set; }
        [Column("proffer_exploit")] public virtual int Pk { get; set; }
        [Column("proffer_train")] public virtual uint ProfferTrain { get; set; }
        [Column("proffer_eudemon")] public virtual uint ProfferEudemon { get; set; }
        [Column("employtime")] public virtual uint EmployTime { get; set; }
        [Column("master_id")] public uint MasterId { get; set; }
        [Column("flower")] public uint Flower { get; set; }
        [Column("flower_w")] public uint WhiteFlower { get; set; }
        [Column("proffer_emoney")] public virtual uint Emoney { get; set; }
        /// <summary>
        /// is a date parsed..
        /// </summary>
        [Column("add_rank")] public virtual uint AddRank { get; set; }
    }
}
