namespace Long.Database.Entities
{
    [Table("cq_ad_queue")]
    public class DbPigeonQueue
    {
        [Key][Column("id")] public virtual uint Identity { get; protected set; }
        [Column("next_id")] public virtual uint NextIdentity { get; set; }
        [Column("user_id")] public virtual uint UserIdentity { get; set; }
        [Column("user_name")] public virtual string UserName { get; set; }
        [Column("create_time")] public virtual int CreateTime { get; set; }
        [Column("addition")] public virtual ushort Addition { get; set; }
        [Column("words")] public virtual string Message { get; set; }
    }
}
