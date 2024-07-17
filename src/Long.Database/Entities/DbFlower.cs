namespace Long.Database.Entities
{
    [Table("cq_flower")]
    public class DbFlower
    {
        [Key]
        [Column("id")] public uint Identity { get; set; }
        [Column("player_id")] public uint UserId { get; set; }
        [Column("flower")] public uint RedRose { get; set; }
        [Column("flower_w")] public uint WhiteRose { get; set; }

        public virtual DbUser User { get; set; }

        [NotMapped] public virtual int RedRosePos { get; set; }
        [NotMapped] public virtual int WhiteRosePos { get; set; }
        [NotMapped] public virtual byte Level { get; set; }
    }
}
