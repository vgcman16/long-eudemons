namespace Long.Database.Entities
{
    [Table("cq_auction")]
    public class DbAuction
    {
        [Key][Column("id")] public uint Id { get; set; }
        [Column("auction_id")] public uint UserId { get; set; }
        [Column("auction_player")] public string UserName { get; set; }
        [Column("item_id")] public uint ItemId { get; set; }
        [Column("value")] public uint Price { get; set; }
        [Column("state")] public byte State { get; set; }
        [Column("timeout")] public int TimeOut { get; set; }
    }
}
