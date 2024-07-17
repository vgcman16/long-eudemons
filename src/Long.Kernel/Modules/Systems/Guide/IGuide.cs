namespace Long.Kernel.Modules.Systems.Guide
{
    public interface IGuide
    {
        public ITutor Tutor { get; set; }

        int MentorExpTime { get; set; }
        int TutorExp { get; set; }
        ushort MentorAddLevelExp { get; set; }
        ushort MentorGodTimes { get; set; }
        ushort MentorResetTimes { get; set; }

        int ApprenticeCount { get; }

        bool AddStudent(ITutor student);
        ITutor GetStudent(uint idStudent);
        Task InitializeAsync();
        bool IsApprentice(uint idGuide);
        bool IsTutor(uint idApprentice);
        Task OnLogoutAsync();
        void RemoveApprentice(uint idApprentice);
        Task SaveAsync();
        Task SynchroApprenticesSharedBattlePowerAsync();
        Task SynchroStudentsAsync();
        Task SynchroDetailsAsync();
        Task SendGuideHistoryAsync(byte mode = 1);
        Task SendStudentGuideHistoryAsync(uint userId);
    }
}
