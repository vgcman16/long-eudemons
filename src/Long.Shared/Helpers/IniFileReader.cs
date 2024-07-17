namespace Long.Shared.Helpers
{
    public class IniFileReader
    {
        private readonly Dictionary<string, Dictionary<string, string>> iniData = new();

        public IniFileReader(string filePath)
        {
            ReadFileData(filePath);
        }

        public Dictionary<string, Dictionary<string, string>> GetAll() {  return iniData; }

        private void ReadFileData(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return;
            }

            string currentSection = "";
            string[] lines = File.ReadAllLines(filePath);
            foreach (string line in lines)
            {
                string trimmedLine = line.Trim();
                if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
                {
                    currentSection = trimmedLine.Substring(1, trimmedLine.Length - 2);
                    iniData[currentSection] = new Dictionary<string, string>();
                }
                else if (!string.IsNullOrEmpty(trimmedLine) && !trimmedLine.StartsWith(";"))
                {
                    int index = trimmedLine.IndexOf('=');
                    if (index != -1)
                    {
                        string key = trimmedLine.Substring(0, index).Trim();
                        string value = trimmedLine.Substring(index + 1).Trim();
                        iniData[currentSection][key] = value;
                    }
                }
            }
        }

        public string GetValue(string section, string key)
        {
            if (iniData.ContainsKey(section) && iniData[section].ContainsKey(key))
            {
                return iniData[section][key];
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
