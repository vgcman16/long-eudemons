namespace Long.Database.Entities
{
    [Table("cq_monster_magictype")]
    public class DbMonsterTypeMagic
    {
        [Key][Column("id")] public uint Id { get; set; }
        [Column("monster_type")] public uint MonsterType { get; set; }
        [Column("trigger_interval")] public virtual byte TriggerInterval { get; set; }

        [Column("magic1")] public virtual uint Magic1 { get; set; }
        [Column("trigger1")] public virtual byte Trigger1 { get; set; }
        [Column("data1")] public virtual int Data1 { get; set; }
        [Column("odds1")] public virtual ushort Odds1 { get; set; }
        [Column("action1")] public virtual uint Action1 { get; set; }

        [Column("magic2")] public virtual uint Magic2 { get; set; }
        [Column("trigger2")] public virtual byte Trigger2 { get; set; }
        [Column("data2")] public virtual int Data2 { get; set; }
        [Column("odds2")] public virtual ushort Odds2 { get; set; }
        [Column("action2")] public virtual uint Action2 { get; set; }

        [Column("magic3")] public virtual uint Magic3 { get; set; }
        [Column("trigger3")] public virtual byte Trigger3 { get; set; }
        [Column("data3")] public virtual int Data3 { get; set; }
        [Column("odds3")] public virtual ushort Odds3 { get; set; }
        [Column("action3")] public virtual uint Action3 { get; set; }

        [Column("magic4")] public virtual uint Magic4 { get; set; }
        [Column("trigger4")] public virtual byte Trigger4 { get; set; }
        [Column("data4")] public virtual int Data4 { get; set; }
        [Column("odds4")] public virtual ushort Odds4 { get; set; }
        [Column("action4")] public virtual uint Action4 { get; set; }

        [Column("magic5")] public virtual uint Magic5 { get; set; }
        [Column("trigger5")] public virtual byte Trigger5 { get; set; }
        [Column("data5")] public virtual int Data5 { get; set; }
        [Column("odds5")] public virtual ushort Odds5 { get; set; }
        [Column("action5")] public virtual uint Action5 { get; set; }

        [Column("magic6")] public virtual uint Magic6 { get; set; }
        [Column("trigger6")] public virtual byte Trigger6 { get; set; }
        [Column("data6")] public virtual int Data6 { get; set; }
        [Column("odds6")] public virtual ushort Odds6 { get; set; }
        [Column("action6")] public virtual uint Action6 { get; set; }

        [Column("magic7")] public virtual uint Magic7 { get; set; }
        [Column("trigger7")] public virtual byte Trigger7 { get; set; }
        [Column("data7")] public virtual int Data7 { get; set; }
        [Column("odds7")] public virtual ushort Odds7 { get; set; }
        [Column("action7")] public virtual uint Action7 { get; set; }

        [Column("magic8")] public virtual uint Magic8 { get; set; }
        [Column("trigger8")] public virtual byte Trigger8 { get; set; }
        [Column("data8")] public virtual int Data8 { get; set; }
        [Column("odds8")] public virtual ushort Odds8 { get; set; }
        [Column("action8")] public virtual uint Action8 { get; set; }

        [Column("life1")] public virtual byte Life1 { get; set; }
        [Column("magic_atk1")] public virtual uint MagicAtk1 { get; set; }

        [Column("life2")] public virtual byte Life2 { get; set; }
        [Column("magic_atk2")] public virtual uint MagicAtk2 { get; set; }

        [Column("life3")] public virtual byte Life3 { get; set; }
        [Column("magic_atk3")] public virtual uint MagicAtk3 { get; set; }
    }
}