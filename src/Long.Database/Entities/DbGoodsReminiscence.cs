
namespace Long.Database.Entities
{
    [Table("cq_goods_reminiscence")]
    public class DbGoodsReminiscence
    {
        [Key][Column("id")] public virtual uint Identity { get; set; }
        [Column("ownerid")] public virtual uint OwnerIdentity { get; set; }
        [Column("itemtype")] public virtual uint Itemtype { get; set; }
        [Column("paytype")] public virtual uint PayType { get; set; }
        [Column("srvmask")] public virtual uint SrvMask { get; set; }
    }
}
