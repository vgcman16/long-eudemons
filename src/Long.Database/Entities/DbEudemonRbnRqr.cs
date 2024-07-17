using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Long.Database.Entities
{
    [Table("cq_eudemon_rbn_rqr")]
    public class DbEudemonRbnRqr
    {
        [Key][Column("id")] public virtual uint Id { get; set; }
        [Column("min")] public virtual int Min { get; set; }
        [Column("max")] public virtual int Max { get; set; }
        [Column("rand_type")] public virtual int RandType { get; set; }
        [Column("suc_percent")] public virtual int SucPercent { get; set; }
        [Column("sacrifice_lev")] public virtual byte SacrificeLev { get; set; }
        [Column("sacrifice_fidelity")] public virtual int SacrificeFidelity { get; set; }
        [Column("sacrifice_starlev")] public virtual int SacrificeStarLev { get; set; }
        [Column("sacrifice_dmg_type")] public virtual bool SacrificeDamageType { get; set; }
        [Column("rbn_rqr_type")] public virtual int SacrificeRbnReqType { get; set; }
        [Column("Sacrifice_lev_god")] public virtual bool SacrificeLevGod { get; set; }
    }
}
