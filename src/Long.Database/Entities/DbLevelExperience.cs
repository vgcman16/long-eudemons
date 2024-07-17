namespace Long.Database.Entities
{
    [Table("cq_levexp_x")]
    public class DbLevelExperience
    {
        [Column("type")] public virtual byte Type { get; set; }
        [Column("level")] public virtual byte Level { get; set; }
        [Column("exp")] public virtual ulong Exp { get; set; }
        [Column("stu_exp")] public virtual uint MentorUpLevTime { get; set; }
        [Column("PerAtk")] public virtual int PerAtk { get; set; }
        [Column("OverAdjAtk")] public virtual int OverAdjAtk { get; set; }
        [Column("PerXP")] public virtual int PerXP { get; set; }
        [Column("OverAdjXP")] public virtual int OverAdjXP { get; set; }
        [Column("PerXPTeam")] public virtual int PerXPTeam { get; set; }
        [Column("OverAdjXPTeam")] public virtual int OverAdjXPTeam { get; set; }
        [Column("PerKillBonus")] public virtual int PerKillBonus { get; set; }
        [Column("OverAdjKillBonus")] public virtual int OverAdjKillBonus { get; set; }
        [Column("UpLevTime")] public virtual int UpLevTime { get; set; }
        [Column("ExpBallMax")] public virtual ulong ExpBallMax { get; set; }
    }
}
