namespace protocdir;

[System.Serializable]
public class Build
{
    [System.Serializable]
    public class Out
    {
        public string language;
        public string path;
    }

    public string name;
    public string[] input;
    public Out[] output;
}