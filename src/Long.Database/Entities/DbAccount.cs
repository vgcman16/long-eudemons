namespace Long.Database.Entities
{
    [Table("account")]
    public class DbAccount
    {
        [Key][Column("id")] public virtual uint Identity { get; set; }
        [Column("name")] public virtual string Username { get; set; }
        [Column("password")] public virtual string Password { get; set; }
        [Column("type")] public virtual byte Authority { get; set; }
    }
}
