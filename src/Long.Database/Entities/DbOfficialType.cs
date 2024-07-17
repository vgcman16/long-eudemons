namespace Long.Database.Entities
{
    [Table("cq_official_type")]
    public class DbOfficialType
    {
        [Key][Column("id")] public virtual uint Id { get; set; }
        [Column("type")] public virtual uint OfficialType { get; set; }
        [Column("req_lev")] public virtual int ReqLevel { get; set; }
        [Column("data0")] public virtual int Data0 { get; set; }
        [Column("data1")] public virtual int Data1 { get; set; }
        [Column("data2")] public virtual int Data2 { get; set; }
        [Column("data3")] public virtual int Data3 { get; set; }
    }
}
