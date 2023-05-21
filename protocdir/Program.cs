using Newtonsoft.Json;

namespace protocdir;

class Program
{
    private static string _currentDir;
    private static string _protoc;
    
    public static void Main(string[] args)
    {
        // Console.WriteLine($"CommandLine: {Environment.CommandLine}");
        try
        {
            RunInternal(args);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        
        Console.WriteLine("All done. press any key to close.");
        Console.ReadKey();
    }

    static void RunInternal(string[] args)
    {
        _currentDir = Path.GetDirectoryName(Environment.ProcessPath);
        if (string.IsNullOrEmpty(_currentDir))
        {
            Console.WriteLine("Failed to get currentDir");
            return;
        }

        Console.WriteLine($"currentDir: {_currentDir}");

        _protoc = Path.Combine(_currentDir, "protoc.exe");
        if (!File.Exists(_protoc))
        {
            Console.WriteLine($"protoc not found: {_protoc}");
            return;
        }
        
        if (!GetConfigFiles(args, out var builds, out var errors))
        {
            foreach (var error in errors)
            {
                Console.WriteLine($"build config error: {error}");
            }
            return;
        }
        
        var builder = new ProtoBuilder(_currentDir, _protoc);
        // Console.WriteLine($"build count: {builds.Length}");
        foreach (var build in builds)
        {
            // Console.WriteLine($"build {Newtonsoft.Json.JsonConvert.SerializeObject(build, Formatting.Indented)}");
            builder.BuildCfg(build);
        }
    }

    static bool GetConfigFiles(string[] args, out Build[] builds, out string[] errors)
    {
        var buildList = new List<Build>();
        var errorList = new List<string>();
        
        if (args.Length == 0)
        {
            var file = Path.Combine(_currentDir, "build.json");
            if (!File.Exists(file))
            {
                errorList.Add("No input build.json");
                builds = null;
                errors = errorList.ToArray();
                return errorList.Count == 0;
            }

            if (File2Build(file, out var build, out var error))
            {
                buildList.Add(build);
            }
            else
            {
                errorList.Add(error);
            }

            builds = buildList.ToArray();
            errors = errorList.ToArray();
            return errorList.Count == 0;
        }

        foreach (var arg in args)
        {
            var file = arg;
            if (!File.Exists(file))
            {
                file = Path.Combine(_currentDir, arg);
            }

            if (!File.Exists(file))
            {
                errorList.Add($"Not found build config: {arg}");
                continue;
            }

            if (!File2Build(file, out var build, out var error))
            {
                errorList.Add(error);
                continue;
            }

            buildList.Add(build);
        }
  
        builds = buildList.ToArray();
        errors = errorList.ToArray();
        return errorList.Count == 0;
    }

    private static bool File2Build(string file, out Build build, out string error)
    {
        try
        {
            var json = File.ReadAllText(file);
            build = Newtonsoft.Json.JsonConvert.DeserializeObject<Build>(json);
            error = null;
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            
            build = null;
            error = e.Message;
            return false;
        }
    }
}