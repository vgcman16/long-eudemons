namespace Long.Database.Entities
{
    [Table("cq_itemaddition")]
    public class DbItemAddition
    {
        [Key][Column("id")] public virtual uint Identity { get; set; }
        [Column("typeid")] public virtual uint TypeId { get; set; }
        [Column("level")] public virtual byte Level { get; set; }
        [Column("life")] public virtual ushort Life { get; set; }
        [Column("attack_max")] public virtual ushort AttackMax { get; set; }
        [Column("attack_min")] public virtual ushort AttackMin { get; set; }
        [Column("defense")] public virtual ushort Defense { get; set; }
        [Column("mgcatk_max")] public virtual ushort MagicAtkMax { get; set; }
        [Column("mgcatk_min")] public virtual ushort MagicAtkMin { get; set; }
        [Column("magic_def")] public virtual ushort MagicDef { get; set; }
        [Column("dexterity")] public virtual ushort Dexterity { get; set; }
        [Column("dodge")] public virtual ushort Dodge { get; set; }
        [Column("flag")] public virtual byte Flag { get; set; }
        [Column("trumpdata")] public virtual ushort TrumpData { get; set; }
    }
}