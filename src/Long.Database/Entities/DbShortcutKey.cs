namespace Long.Database.Entities
{
    [Table("cq_shortcut_key")]
    public class DbShortcutKey
    {
        [Key][Column("id")]public virtual uint Identity { get; set; }
        [Column("player_id")] public virtual uint PlayerId { get; set; }
        [Column("item_key")] public virtual string ItemKey { get; set; }
        [Column("magic_key")] public virtual string MagicKey { get; set; }
    }
}
