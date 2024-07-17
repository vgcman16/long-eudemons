namespace Long.Database.Entities
{
    [Table("cq_battle_limit_type")]
    public class DbTutorBattleLimitType
    {
        [Key][Column("id")] public virtual ushort Id { get; set; }
        [Column("tutor_limit")] public virtual ushort TutorLimit { get; set; }
        [Column("family_battle_limit")] public virtual ushort FamilyLimit { get; set; }
    }
}
