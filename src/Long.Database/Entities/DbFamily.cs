namespace Long.Database.Entities
{
    [Table("cq_family")]
    public class DbFamily
    {
        [Key][Column("id")] public uint Identity { get; set; }
        [Column("family_name")] public string Name { get; set; }
        [Column("rank")] public byte Rank { get; set; }
        [Column("leader_name")] public string LeaderName { get; set; }
        [Column("leader_id")] public uint LeaderIdentity { get; set; }
        [Column("announce")] public string Announcement { get; set; }
        [Column("money")] public ulong Money { get; set; }
        [Column("repute")] public uint Repute { get; set; }
        [Column("amount")] public byte Amount { get; set; }
        [Column("enemy_family0_id")] public uint EnemyFamily0 { get; set; }
        [Column("enemy_family1_id")] public uint EnemyFamily1 { get; set; }
        [Column("enemy_family2_id")] public uint EnemyFamily2 { get; set; }
        [Column("enemy_family3_id")] public uint EnemyFamily3 { get; set; }
        [Column("enemy_family4_id")] public uint EnemyFamily4 { get; set; }
        [Column("ally_family0_id")] public uint AllyFamily0 { get; set; }
        [Column("ally_family1_id")] public uint AllyFamily1 { get; set; }
        [Column("ally_family2_id")] public uint AllyFamily2 { get; set; }
        [Column("ally_family3_id")] public uint AllyFamily3 { get; set; }
        [Column("ally_family4_id")] public uint AllyFamily4 { get; set; }
        [Column("create_date")] public uint CreationDate { get; set; }
        [Column("create_name")] public string CreateName { get; set; }
        [Column("del_flag")] public uint DeletionFlag { get; set; }
        [Column("star_tower")] public byte StarTower { get; set; }
        [Column("challenge_map")] public uint ChallengeMap { get; set; }
        [Column("family_map")] public uint FamilyMap { get; set; }
        [Column("truce")] public byte Truce { get; set; }
    }
}