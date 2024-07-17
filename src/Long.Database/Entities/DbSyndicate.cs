namespace Long.Database.Entities
{
    [Table("cq_syndicate")]
    public class DbSyndicate
    {
        [Key][Column("id")] public virtual uint Identity { get; set; }
        [Column("name")] public virtual string Name { get; set; }
        [Column("announce")] public virtual string Announce { get; set; }
        [Column("member_title")] public virtual sbyte MemberTitle { get; set; }
        [Column("leader_id")] public virtual uint LeaderId { get; set; }
        [Column("leader_name")] public virtual string LeaderName { get; set; }
        [Column("money")] public virtual long Money { get; set; }
        [Column("fealty_syn")] public virtual uint FealtySyn { get; set; }
        [Column("del_flag")] public virtual byte DelFlag { get; set; }
        [Column("amount")] public virtual int Amount { get; set; }
        [Column("enemy0")] public virtual uint Enemy0 { get; set; }
        [Column("enemy1")] public virtual uint Enemy1 { get; set; }
        [Column("enemy2")] public virtual uint Enemy2 { get; set; }
        [Column("enemy3")] public virtual uint Enemy3 { get; set; }
        [Column("enemy4")] public virtual uint Enemy4 { get; set; }
        [Column("ally0")] public virtual uint Ally0 { get; set; }
        [Column("ally1")] public virtual uint Ally1 { get; set; }
        [Column("ally2")] public virtual uint Ally2 { get; set; }
        [Column("ally3")] public virtual uint Ally3 { get; set; }
        [Column("ally4")] public virtual uint Ally4 { get; set; }
        [Column("rank")] public virtual byte SynRank { get; set; }
        [Column("saint")] public virtual byte Saint { get; set; }
        [Column("mantle")] public virtual byte Mantle { get; set; }
        [Column("distime")] public virtual uint DisTime { get; set; }
        [Column("repute")] public virtual int Repute { get; set; }
        [Column("publish_time")] public virtual uint PublishTime { get; set; }
        [Column("symbol")] public virtual uint Symbol { get; set; }
        [Column("indexcode")] public virtual sbyte IndexCode { get; set; }
        [Column("kickoutmembertime1")] public virtual uint KickoutMemberTime1 { get; set; }
        [Column("kickoutmembertime2")] public virtual uint KickoutMemberTime2 { get; set; }
        [Column("kickoutmembertime3")] public virtual uint KickoutMemberTime3 { get; set; }
        [Column("totem_pole")] public virtual ulong TotemPole { get; set; }
        [Column("coach_user0")] public virtual uint CoachUser0 { get; set; }
        [Column("coach_user1")] public virtual uint CoachUser1 { get; set; }
        [Column("coach_user2")] public virtual uint CoachUser2 { get; set; }
        [Column("coach_user3")] public virtual uint CoachUser3 { get; set; }
        [Column("coach_user4")] public virtual uint CoachUser4 { get; set; }
        [Column("coach_user5")] public virtual uint CoachUser5 { get; set; }
        [Column("coach_user6")] public virtual uint CoachUser6 { get; set; }
        [Column("coach_user7")] public virtual uint CoachUser7 { get; set; }
        [Column("coach_user8")] public virtual uint CoachUser8 { get; set; }
        [Column("coach_date")] public virtual uint CoachDate { get; set; }
        [Column("coat_type")] public virtual byte CoatType { get; set; }
        [Column("emoney")] public virtual uint Emoney { get; set; }
        [Column("totem_pole_ex")] public virtual ulong TotemPoleEx0 { get; set; }
        [Column("kickout_num")] public virtual int KickoutNum { get; set; }
        [Column("totem_pole_ex1")] public virtual ulong TotemPoleEx1 { get; set; }
    }
}