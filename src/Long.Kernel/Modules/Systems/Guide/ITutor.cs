using Long.Kernel.States.User;

namespace Long.Kernel.Modules.Systems.Guide
{
    public interface ITutor
    {
        public uint GuideIdentity { get; }
        public string GuideName { get; }

        public uint StudentIdentity { get; }
        public string StudentName { get; }

        public uint MentorAddExpTime { get; }
        public uint MentorAddGodTime { get; }
        public uint MentorAddRebornTime { get; }

        public ulong TotalContribution { get; }

        public bool Betrayed { get; }
        public bool BetrayalCheck { get; }

        public Character Guide { get; }
        public Character Student { get; }

        int SharedBattlePower { get; }

        Task<bool> AwardTutorExperienceAsync(uint addExpTime);
        Task<bool> AwardTutorGodTimeAsync(ushort addGodTime);
        Task<bool> AwardTutorRebornTimesAsync(ushort addTime);

        Task BetrayAsync();
        Task SendTutorAsync();
        Task SendTutorOnlineAsync();
        Task SendTutorOfflineAsync();
        Task SendStudentAsync();
        Task SendStudentOnlineAsync();
        Task SendStudentOfflineAsync();
        Task BetrayalTimerAsync();

        Task<bool> SaveAsync();
        Task<bool> DeleteAsync();
    }
}
