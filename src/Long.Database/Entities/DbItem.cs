namespace Long.Database.Entities
{
    [Table("cq_item")]
    public class DbItem
    {
        [Key][Column("id")] public virtual uint Id { get; set; }
        [Column("type")] public virtual uint Type { get; set; }
        [Column("owner_id")] public virtual uint OwnerId { get; set; }
        [Column("player_id")] public virtual uint PlayerId { get; set; }
        [Column("amount")] public virtual ushort Amount { get; set; }
        [Column("amount_limit")] public virtual ushort AmountLimit { get; set; }
        [Column("ident")] public virtual byte Ident { get; set; }
        [Column("position")] public virtual byte Position { get; set; }
        [Column("gem1")] public virtual byte Gem1 { get; set; }
        [Column("gem2")] public virtual byte Gem2 { get; set; }
        [Column("magic1")] public virtual ushort Magic1 { get; set; }
        [Column("magic2")] public virtual byte Magic2 { get; set; }
        [Column("magic3")] public virtual byte Magic3 { get; set; }
        [Column("data")] public virtual uint Data { get; set; }
        [Column("warghostexp")] public virtual uint WarGhostExp { get; set; }
        [Column("gemtype")] public virtual uint GemType { get; set; }
        [Column("availabletime")] public virtual uint AvailableTime { get; set; }
        [Column("eudemon_attack1")] public virtual byte EudemonAttack1 { get; set; }
        [Column("eudemon_attack2")] public virtual byte EudemonAttack2 { get; set; }
        [Column("eudemon_attack3")] public virtual byte EudemonAttack3 { get; set; }
        [Column("eudemon_attack4")] public virtual byte EudemonAttack4 { get; set; }
        [Column("special_effect")] public virtual byte SpecialEffect { get; set; }
        [Column("forgename")] public virtual string ForgenName { get; set; } = string.Empty;
        [Column("chksum")] public virtual uint ChkSum { get; set; }
        [Column("locked")] public virtual uint Locked { get; set; }
        [Column("plunder")] public virtual ushort Plunder { get; set; }
        [Column("monopoly")] public virtual byte Monopoly { get; set; }
        [Column("gem3")] public virtual byte Gem3 { get; set; }
        [Column("progress")] public virtual uint Progress { get; set; }
        [Column("extra_addi")] public virtual uint ExtraAddi { get; set; }
        [Column("active_time")] public virtual uint ActiveTime { get; set; }
        [Column("del_time")] public virtual int DeleteTime { get; set; }
    }
}