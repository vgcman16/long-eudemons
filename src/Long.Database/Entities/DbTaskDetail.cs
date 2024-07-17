namespace Long.Database.Entities
{
    [Table("cq_taskdetail")]
    public class DbTaskDetail
    {
        [Key][Column("id")] public virtual uint Identity { get; set; }
        [Column("userid")] public virtual uint UserIdentity { get; set; }
        [Column("taskid")] public virtual uint TaskIdentity { get; set; }
        [Column("taskphase")] public virtual int TaskPhase { get; set; }
        [Column("taskcompletenum")] public virtual int TaskCompleteNum { get; set; }
        [Column("taskbegintime")] public virtual uint TaskBeginTime { get; set; }
        [Column("taskbesttimes")] public virtual uint TaskBestTimes { get; set; }
        [Column("complete_times")] public virtual ushort CompleteTimes { get; set; }
        [Column("notice_flag")] public virtual byte NotifyFlag { get; set; }
        [Column("percent")] public virtual byte Percent { get; set; }
        [Column("account_id")] public virtual uint AccountId { get; set; }
        [Column("data1")] public virtual int Data1 { get; set; }
        [Column("data2")] public virtual int Data2 { get; set; }
        [Column("data3")] public virtual int Data3 { get; set; }
        [Column("data4")] public virtual int Data4 { get; set; }
        [Column("data5")] public virtual int Data5 { get; set; }
        [Column("data6")] public virtual int Data6 { get; set; }
        [Column("data7")] public virtual int Data7 { get; set; }
        [Column("data8")] public virtual int Data8 { get; set; }
    }
}
