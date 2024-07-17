namespace Long.Database.Entities
{
    [Table("cq_eudemon_rbn_type")]
    public class DbEudemonRbnType
    {
        [Key][Column("id")] public virtual uint Id { get; set; }
        [Column("life")] public virtual bool Life { get; set; }
        [Column("phy_atk")] public virtual bool PhyAtk { get; set; }
        [Column("phy_def")] public virtual bool PhyDef { get; set; }
        [Column("mgc_atk")] public virtual bool MgcAtk { get; set; }
        [Column("mgc_def")] public virtual bool MgcDef { get; set; }
    }
}
