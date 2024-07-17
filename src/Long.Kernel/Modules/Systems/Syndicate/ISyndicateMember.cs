using Long.Database.Entities;
using Long.Kernel.Modules.Systems.Nobility;
using Long.Kernel.States.User;

namespace Long.Kernel.Modules.Systems.Syndicate
{
    public interface ISyndicateMember
    {
        uint AssistantIdentity { get; set; }
        uint Emoney { get; set; }
        int Gender { get; }
        uint GuideDonation { get; set; }
        bool IsOnline { get; }
        DateTime? LastLogout { get; set; }
        byte Level { get; set; }
        uint LookFace { get; }
        uint MasterIdentity { get; set; }
        uint MateIdentity { get; }
        NobilityRank NobilityRank { get; }
        int PkDonation { get; set; }
        DateTime? PositionExpiration { get; set; }
        int Profession { get; set; }
        SyndicateRank Rank { get; set; }
        string RankName { get; }
        uint RedRoseDonation { get; set; }
        uint WhiteRoseDonation { get; set; }
        int Silvers { get; set; }
        uint SyndicateIdentity { get; }
        string SyndicateName { get; }
        int TotalDonation { get; }
        Character User { get; }
        uint UserIdentity { get; }
        string UserName { get; }

        void ChangeName(string newName);
        Task<bool> CreateAsync(Character user, ISyndicate syn, SyndicateRank rank);
        Task<bool> CreateAsync(DbSyndicateAttr attr, ISyndicate syn);
        Task<bool> DeleteAsync();
        Task<bool> SaveAsync();

        public enum SyndicateRank : ushort
        {
            GuildLeader = 1000,
            LeaderSpouse = 920,

            DeputyLeader = 990,
            DeputyLeaderSpouse = 620,

            HonoraryDeputyLeader = 980,

            Manager = 890,
            HonoraryManager = 880,

            Capitain = 850,
            HonoraryCapitain = 840,

            Lieutenant = 690,
            HonoraryLieutenant = 680,

            Secretary = 603,
            LeaderSecretary = 602,

            SeniorMember = 490,

            Level1Member = 240,
            Level2Member = 230,
            Level3Member = 220,

            JuniorMember = 201,
            Member = 200,
            Reservlist = 100,

            Star0 = 4,
            Star1 = 400,
            Star2 = 500,
            Star3 = 600,
            Star4 = 3,
            Star5 = 2,
            Star6 = 900,
            Star7 = 1,

            None = 0
        }
    }
}
/*Amount=37
9999=Legion
1000=Leader
990=Deputy
980=HonorDeputy
920=Leader'sSpouse
900=6-star
890=General
880=HonorGeneral
850=Captain
840=HonorCaptain
690=Lieutenant
680=HonorLieut.
620=Deputy'sSpouse
611=Deputy'sAide
610=LeadSpouseAide
603=Secretary
602=LeaderSecretary
600=3-star
590=Sergeant
521=Captain'sSpouse
520=General'sSpouse
511=Captain'sAide
510=General'sAide
500=2-star
490=SeniorMember
420=Lieut.'sSpouse
400=1-star
240=Level1Member
230=Level2Member
220=Level3Member
201=JuniorMember
200=CommonMember
100=Reservist
4=0-star
3=4-star
2=5-star
1=7-star*/