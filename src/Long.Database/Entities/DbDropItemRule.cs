using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Long.Database.Entities
{
    [Table("cq_dropitemrule")]
    public class DbDropItemRule
    {
        [Key][Column("id")] public uint Identity { get; set; }
        [Column("group_id")] public uint GroupdId { get; set; }
        [Column("ruleid")] public uint RuleId { get; set; }
        [Column("chance")] public int Chance { get; set; }
        [Column("item0")] public uint Item0 { get; set; }
        [Column("item1")] public uint Item1 { get; set; }
        [Column("item2")] public uint Item2 { get; set; }
        [Column("item3")] public uint Item3 { get; set; }
        [Column("item4")] public uint Item4 { get; set; }
        [Column("item5")] public uint Item5 { get; set; }
        [Column("item6")] public uint Item6 { get; set; }
        [Column("item7")] public uint Item7 { get; set; }
        [Column("item8")] public uint Item8 { get; set; }
        [Column("item9")] public uint Item9 { get; set; }
        [Column("item10")] public uint Item10 { get; set; }
        [Column("item11")] public uint Item11 { get; set; }
        [Column("item12")] public uint Item12 { get; set; }
        [Column("item13")] public uint Item13 { get; set; }
        [Column("item14")] public uint Item14 { get; set; }
        [Column("data")] public uint Data { get; set; }

    }
}
