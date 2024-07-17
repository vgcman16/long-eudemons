using Long.Database.Entities;
using Long.Kernel.Database.Repositories;
using System.Collections.Concurrent;

namespace Long.Kernel.Managers
{
    public class MagicManager
    {
        private static readonly ILogger logger = Log.ForContext<MagicManager>();
        private static readonly ConcurrentDictionary<uint, DbMagictype> magicTypes = new();
        private static readonly ConcurrentDictionary<uint, DbTrack> trackTypes = new();

        public static readonly List<DbMagictype> AutoLearnMagics = new();

        public static async Task InitializeAsync()
        {
            logger.Information("Initializing Magic Manager");

            foreach (var trackType in await MagicTypeRepository.GetTracksAsync())
            {
                trackTypes.TryAdd(trackType.Id, trackType);
            }

            foreach (DbMagictype magicType in await MagicTypeRepository.GetAsync())
            {
                uint trackId = magicType.TrackId;
                int count = 0;
                while (trackId != 0 && count++ < 50)
                {
                    if (!trackTypes.TryGetValue(trackId, out var track))
                    {
                        logger.Error($"Track[{trackId}] not found for magictype {magicType.Id}");
                        break;
                    }

                    if (track.Direction < 0 || track.Direction > 7)
                    {
                        logger.Error($"Track[{trackId}] has invalid direction sector!");
                        break;
                    }

                    magicType.TrackS.Add(track);
                    trackId = track.IdNext;
                }

                trackId = magicType.TrackId2;
                count = 0;
                while (trackId != 0 && count++ < 50)
                {
                    if (!trackTypes.TryGetValue(trackId, out var track))
                    {
                        logger.Error($"Track[{trackId}] not found for magictype {magicType.Id}");
                        break;
                    }

                    if (track.Direction < 0 || track.Direction > 7)
                    {
                        logger.Error($"Track[{trackId}] has invalid direction sector!");
                        break;
                    }

                    magicType.TrackT.Add(track);
                    trackId = track.IdNext;
                }

                magicTypes.TryAdd(magicType.Id, magicType);
                if (magicType.LearnLevel > 0 && magicType.AutoLearn > 0)
                {
                    AutoLearnMagics.Add(magicType);
                }
            }
        }

        public static byte GetMaxLevel(uint idType)
        {
            return (byte)(magicTypes.Values.Where(x => x.Type == idType).OrderByDescending(x => x.Level)
                                      .FirstOrDefault()?.Level ?? 0);
        }

        public static DbMagictype GetMagictype(uint idType, ushort level)
        {
            return magicTypes.Values.FirstOrDefault(x => x.Type == idType && x.Level == level);
        }
    }
}
