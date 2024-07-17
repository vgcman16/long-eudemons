using Long.Database.Entities;
using System.Collections.Concurrent;

namespace Long.Kernel.Managers
{
    public class ExperienceManager
    {
        private static readonly ILogger logger = Log.ForContext<ExperienceManager>();

        private static readonly Dictionary<byte, Dictionary<byte, DbLevelExperience>> levelXpType = new();
        private static ConcurrentDictionary<uint, ExperienceMultiplierData> experienceMultiplierData = new();

        public static async Task InitializeAsync()
        {
            logger.Information("Starting experience manager");
            var file = File.ReadAllLines(Environment.CurrentDirectory + "/ini/LevelExp.ini");
            foreach (var line in file)
            {
                var args = line.Split(' ');
                if (args.Length < 5)
                {
                    logger.Error("Invalid levelxpparam parse!");
                }

                DbLevelExperience level = new()
                {
                    Level = byte.Parse(args[0]),
                    Exp = ulong.Parse(args[1]),
                    Type = byte.Parse(args[2]),
                    MentorUpLevTime = uint.Parse(args[3]),
                    UpLevTime = int.Parse(args[4]),
                };

                if (levelXpType.ContainsKey(level.Type))
                {
                    levelXpType[level.Type].Add(level.Level, level);
                }
                else
                {
                    levelXpType.Add(level.Type, new());
                    levelXpType[level.Type].Add(level.Level, level);
                }
            }
        }

        public static DbLevelExperience GetUserLevelXp(byte level)
        {
            return levelXpType[0].TryGetValue(level, out var value) ? value : null;
        }

        public static DbLevelExperience GetPetLevelXp(byte level)
        {
            return levelXpType[1].TryGetValue(level, out var value) ? value : null;
        }

        public static DbLevelExperience GetGodLevelXp(byte level)
        {
            return levelXpType[10].TryGetValue(level, out var value) ? value : null;
        }

        public static DbLevelExperience GetPetGodLevelXp(byte level)
        {
            return levelXpType[11].TryGetValue(level, out var value) ? value : null;
        }

        public static int GetLevelLimit()
        {
            return 143;
        }

        public static bool AddExperienceMultiplierData(uint idUser, float multiplier, int seconds)
        {
            ExperienceMultiplierData value = GetExperienceMultiplierData(idUser);
            if (!value.Equals(default) && value.ExperienceMultiplier > multiplier)
            {
                return false;
            }

            experienceMultiplierData.TryRemove(idUser, out _);

            value = new ExperienceMultiplierData
            {
                UserId = idUser,
                ExperienceMultiplier = multiplier,
                EndTime = DateTime.Now.AddSeconds(seconds)
            };
            return experienceMultiplierData.TryAdd(idUser, value);
        }

        public static ExperienceMultiplierData GetExperienceMultiplierData(uint idUser)
        {
            if (!experienceMultiplierData.TryGetValue(idUser, out var data))
            {
                return default;
            }
            if (DateTime.Now > data.EndTime)
            {
                experienceMultiplierData.TryRemove(idUser, out _);
                return default;
            }
            return data;
        }

        public struct ExperienceMultiplierData
        {
            public uint UserId { get; set; }
            public float ExperienceMultiplier { get; set; }
            public DateTime EndTime { get; set; }
            public readonly bool IsActive => EndTime > DateTime.Now;
            public readonly int RemainingSeconds => (int)(IsActive ? (EndTime - DateTime.Now).TotalSeconds : 0);
        }
    }
}
