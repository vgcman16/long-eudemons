using Long.Shared.Helpers;
using Long.World.Map;
using Serilog;
using System.Collections.Concurrent;
using System.Drawing;

namespace Long.World
{
    public class MapDataManager
    {
        private static readonly ILogger logger = Log.ForContext<MapDataManager>();

        public static async Task LoadDataAsync()
        {
            IniFileReader file = new IniFileReader(Path.Combine(Environment.CurrentDirectory, "ini", "GameMap.ini"));
            var mapList = file.GetAll();
            logger.Debug("Loading {0} maps...", mapList.Count);
            foreach(var info in mapList)
            {
                uint.TryParse(info.Key.Remove(0, 3), out var idMap);
                int.TryParse(file.GetValue(info.Key, "Alpha"), out var alpha);
                int.TryParse(file.GetValue(info.Key, "Red"), out var red);
                int.TryParse(file.GetValue(info.Key, "Green"), out var green);
                int.TryParse(file.GetValue(info.Key, "Blue"), out var blue);
                int.TryParse(file.GetValue(info.Key, "switch"), out var switchFlag);

                Color day = Color.Empty;
                Color nightfall = Color.Empty;
                Color night = Color.Empty;
                Color dawn = Color.Empty;
                string colordata = file.GetValue(info.Key, "day");
                if (colordata != null && colordata != string.Empty)
                    day = ColorTranslator.FromHtml("#" + colordata.Substring(4));
                colordata = file.GetValue(info.Key, "nightfall");
                if (colordata != null && colordata != string.Empty)
                    nightfall = ColorTranslator.FromHtml("#" + colordata.Substring(4));
                colordata = file.GetValue(info.Key, "night");
                if (colordata != null && colordata != string.Empty)
                    night = ColorTranslator.FromHtml("#" + colordata.Substring(4));
                colordata = file.GetValue(info.Key, "dawn");
                if (colordata != null && colordata != string.Empty)
                    dawn = ColorTranslator.FromHtml("#" + colordata.Substring(4));

                string upLevData = file.GetValue(info.Key, "UpLev");
                int levMin = 0, levMax = 0;
                if (upLevData != null && upLevData != string.Empty)
                {
                    string[] upLev = upLevData.Split('-');
                    int.TryParse(upLev[0], out levMin);
                    int.TryParse(upLev[1], out levMax);
                }
                var mapDoc = new MapData()
                {
                    ID = idMap,
                    Color = Color.FromArgb(alpha, red, green, blue),
                    Switch = switchFlag,
                    Day = day,
                    NightFall = nightfall,
                    Night = night,
                    Dawn = dawn,
                    UpLevMin = levMin,
                    UpLevMax = levMax,
                    Name = file.GetValue(info.Key, "Name"),
                    FilePath = file.GetValue(info.Key, "File")
                };

                mapData.TryAdd(idMap, mapDoc);
            }
        }

        public static GameMapData GetMapData(uint idDoc)
        {
            if (!MapDataManager.mapData.TryGetValue(idDoc, out MapData value))
            {
                return null;
            }

            GameMapData mapData = new(idDoc);
            if (mapData.Load(value.FilePath.Replace("\\", Path.DirectorySeparatorChar.ToString())))
            {
                return mapData;
            }
            return null;
        }

        private struct MapData
        {
            public uint ID;
            public string Name;
            public string FilePath;
            public Color Color;
            public Color Day;
            public Color NightFall;
            public Color Night;
            public Color Dawn;
            public int Snow;
            public int FireBug;
            public int Dragon;
            public int Dandelion;
            public int Switch;
            public int UpLevMin;
            public int UpLevMax;
        }

        private static readonly ConcurrentDictionary<uint, MapData> mapData = new();
    }
}
