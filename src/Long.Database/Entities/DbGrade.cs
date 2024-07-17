using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Long.Database.Entities
{
    [Table("cq_grade")]
    public class DbGrade
    {
        [Key][Column("id")] public virtual uint Id { get; set; }
        [Column("life_a")] public virtual int LifeA { get; set; }
        [Column("life_b")] public virtual int LifeB { get; set; }
        [Column("life_c")] public virtual int LifeC { get; set; }
        [Column("life_grow_a")] public virtual int LifeGrowA { get; set; }
        [Column("life_grow_b")] public virtual int LifeGrowB { get; set; }
        [Column("life_grow_c")] public virtual int LifeGrowC { get; set; }
        [Column("phy_min_a")] public virtual int PhyMinA { get; set; }
        [Column("phy_min_b")] public virtual int PhyMinB { get; set; }
        [Column("phy_min_c")] public virtual int PhyMinC { get; set; }
        [Column("phy_min_grow_a")] public virtual int PhyMinGrowA { get; set; }
        [Column("phy_min_grow_b")] public virtual int PhyMinGrowB { get; set; }
        [Column("phy_min_grow_c")] public virtual int PhyMinGrowC { get; set; }
        [Column("phy_max_a")] public virtual int PhyMaxA { get; set; }
        [Column("phy_max_b")] public virtual int PhyMaxB { get; set; }
        [Column("phy_max_c")] public virtual int PhyMaxC { get; set; }
        [Column("phy_max_grow_a")] public virtual int PhyMaxGrowA { get; set; }
        [Column("phy_max_grow_b")] public virtual int PhyMaxGrowB { get; set; }
        [Column("phy_max_grow_c")] public virtual int PhyMaxGrowC { get; set; }
        [Column("phy_def_a")] public virtual int PhyDefA { get; set; }
        [Column("phy_def_b")] public virtual int PhyDefB { get; set; }
        [Column("phy_def_c")] public virtual int PhyDefC { get; set; }
        [Column("phy_def_grow_a")] public virtual int PhyDefGrowA { get; set; }
        [Column("phy_def_grow_b")] public virtual int PhyDefGrowB { get; set; }
        [Column("phy_def_grow_c")] public virtual int PhyDefGrowC { get; set; }
        [Column("mgc_min_a")] public virtual int MagicMinA { get; set; }
        [Column("mgc_min_b")] public virtual int MagicMinB { get; set; }
        [Column("mgc_min_c")] public virtual int MagicMinC { get; set; }
        [Column("mgc_min_grow_a")] public virtual int MagicMinGrowA { get; set; }
        [Column("mgc_min_grow_b")] public virtual int MagicMinGrowB { get; set; }
        [Column("mgc_min_grow_c")] public virtual int MagicMinGrowC { get; set; }
        [Column("mgc_max_a")] public virtual int MagicMaxA { get; set; }
        [Column("mgc_max_b")] public virtual int MagicMaxB { get; set; }
        [Column("mgc_max_c")] public virtual int MagicMaxC { get; set; }
        [Column("mgc_max_grow_a")] public virtual int MagicMaxGrowA { get; set; }
        [Column("mgc_max_grow_b")] public virtual int MagicMaxGrowB { get; set; }
        [Column("mgc_max_grow_c")] public virtual int MagicMaxGrowC { get; set; }
        [Column("mgc_def_a")] public virtual int MagicDefA { get; set; }
        [Column("mgc_def_b")] public virtual int MagicDefB { get; set; }
        [Column("mgc_def_c")] public virtual int MagicDefC { get; set; }
        [Column("mgc_def_grow_a")] public virtual int MagicDefGrowA { get; set; }
        [Column("mgc_def_grow_b")] public virtual int MagicDefGrowB { get; set; }
        [Column("mgc_def_grow_c")] public virtual int MagicDefGrowC { get; set; }
        [Column("luck_a")] public virtual int LuckA { get; set; }
        [Column("luck_b")] public virtual int LuckB { get; set; }
        [Column("luck_c")] public virtual int LuckC { get; set; }
        [Column("dmg_type")] public virtual int DmgType { get; set; }
        [Column("rarity")] public virtual int Rarity { get; set; }
        [Column("metempsychosis")] public virtual int Metempsychosis { get; set; }
        [Column("reborn_limit")] public virtual int RebornLimit { get; set; }
        [Column("rbn_rqr_type")] public virtual int RebornRequireType { get; set; }
        [Column("auto_trade_type")] public virtual int AutoTradeType { get; set; }
        [Column("major_attr_a")] public virtual int MajorAttrA { get; set; }
        [Column("major_attr_b")] public virtual int MajorAttrB { get; set; }
        [Column("major_attr_c")] public virtual int MajorAttrC { get; set; }
        [Column("minor_attr1_a")] public virtual int MinnorAttr1A { get; set; }
        [Column("minor_attr1_b")] public virtual int MinnorAttr1B { get; set; }
        [Column("minor_attr1_c")] public virtual int MinnorAttr1C { get; set; }
        [Column("minor_attr2_a")] public virtual int MinnorAttr2A { get; set; }
        [Column("minor_attr2_b")] public virtual int MinnorAttr2B { get; set; }
        [Column("minor_attr2_c")] public virtual int MinnorAttr2C { get; set; }
        [Column("rbn_rqr_type2")] public virtual int RebornRequireType2 { get; set; }
    }
}
