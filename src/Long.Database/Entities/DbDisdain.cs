using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Long.Database.Entities
{
    [Table("cq_disdain")]
    public class DbDisdain
    {
        [Key][Column("id")] public uint Identity { get; set; }
        [Column("type")] public byte Type { get; set; }
        [Column("delta_lev")] public int DeltaLev { get; set; }
        [Column("usr_atk_mst")] public int UsrAtkMst { get; set; }
        [Column("usr_atk_usr")] public int UsrAtkUsr { get; set; }
        [Column("usr_atk_usrx")] public int UsrAtkUsrX { get; set; }
        [Column("usrx_atk_usr")] public int UsrXAtkUsr { get; set; }
        [Column("usrx_atk_usrx")] public int UsrXAtkUsrX { get; set; }
        [Column("mst_atk")] public int MstAtk { get; set; }
        [Column("max_atk")] public int MaxAtk { get; set; }
        [Column("max_xp_atk")] public int MaxXpAtk { get; set; }
        [Column("dex_factor")] public int DexFactor { get; set; }
        [Column("pk_dex_factor")] public int PkDexFactor { get; set; }
        [Column("exp_factor")] public int ExpFactor { get; set; }
        [Column("xp_exp_factor")] public int XpExpFactor { get; set; }
        [Column("usr_atk_usr_min")] public int UsrAtkUsrMin { get; set; }
        [Column("usr_atk_usr_max")] public int UsrAtkUsrMax { get; set; }
        [Column("usr_atk_usr_overadj")] public int UsrAtkUsrOveradj { get; set; }
        [Column("usr_atk_usrx_min")] public int UsrAtkUsrxMin { get; set; }
        [Column("usr_atk_usrx_max")] public int UsrAtkUsrxMax { get; set; }
        [Column("usr_atk_usrx_overadj")] public int UsrAtkUsrxOveradj { get; set; }
        [Column("usrx_atk_usr_min")] public int UsrxAtkUsrMin { get; set; }
        [Column("usrx_atk_usr_max")] public int UsrxAtkUsrMax { get; set; }
        [Column("usrx_atk_usr_overadj")] public int UsrxAtkUsrOveradj { get; set; }
        [Column("usrx_atk_usrx_max")] public int UsrxAtkUsrxMax { get; set; }
        [Column("usrx_atk_usrx_min")] public int UsrxAtkUsrxMin { get; set; }
        [Column("usrx_atk_usrx_overadj")] public int UsrxAtkUsrxOveradj { get; set; }
        [Column("estoppel")] public int Estoppel { get; set; }
        [Column("mst_atk_usr_min")] public int MstAtkUsrMin { get; set; }
        [Column("mst_atk_usr_max")] public int MstAtkUsrMax { get; set; }
        [Column("mst_atk_usr_overadj")] public int MstAtkUsrOveradj { get; set; }
        [Column("usr_atk_mst_min")] public int UsrAtkMstMin { get; set; }
        [Column("usr_atk_mst_max")] public int UsrAtkMstMax { get; set; }
        [Column("usr_atk_mst_overadj")] public int UsrAtkMstOveradj { get; set; }
    }
}
