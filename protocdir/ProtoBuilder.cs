using System.Diagnostics;
using System.Text;

namespace protocdir;

public class ProtoBuilder
{
    private readonly string _currentDir;
    private readonly string _protoc;

    public ProtoBuilder(string currentDir, string protoc)
    {
        _currentDir = currentDir;
        _protoc = protoc;
    }

    public void BuildCfg(Build build)
    {
        Console.WriteLine($"Start build {build.name}:");

        // check input
        var inputs = new string[build.input.Length];
        for (int i = 0; i < inputs.Length; i++)
        {
            inputs[i] = Path.Combine(_currentDir, build.input[i]);

            if (!Directory.Exists(inputs[i]))
            {
                Console.WriteLine($"input dir {inputs[i]} not exist");
            }
        }

        // all out
        foreach (var buildOut in build.output)
        {
            var outputPath = Path.Combine(_currentDir, buildOut.path);
            ClearOutputPath(outputPath);
            ExecuteProtocDirs(inputs, buildOut.language, outputPath);
        }
        
        Console.WriteLine();
    }

    void ClearOutputPath(string outputPath)
    {
        if (Directory.Exists(outputPath))
        {
            Directory.Delete(outputPath, true);
        }

        Directory.CreateDirectory(outputPath);
    }

    void ExecuteProtocDirs(string[] inputs, string language, string outputPath)
    {
        foreach (var inputDir in inputs)
        {
            var protoFiles = Directory.GetFiles(inputDir, "*.proto", SearchOption.AllDirectories);
            foreach (var protoFilePath in protoFiles)
            {
                var protoFile = protoFilePath.Replace("\\", "/");
                // get the relative out path
                var protoRelative = protoFile.Substring(Path.GetDirectoryName(inputDir).Length).Replace("\\", "/");
                var currentOutPath = outputPath + protoRelative;
                var currentOutDir = Path.GetDirectoryName(currentOutPath);
                // Console.WriteLine(currentOutDir);
                if (!Directory.Exists(currentOutDir))
                {
                    Directory.CreateDirectory(currentOutDir);
                }
                
                ExecuteProtocFile(inputs, language, currentOutDir, protoFile);
            }
        }
    }
    
    void ExecuteProtocFile(string[] inputs, string language, string outputPath, string protoFile)
    {
        Console.WriteLine($"build protoFile: {protoFile}");
        
        var argsBuilder = new StringBuilder();
        //commandBuilder.Append($"\"{_protoc}\"");

        foreach (var importDir in inputs)
        {
            argsBuilder.Append($" --proto_path=\"{importDir}\"");
        }
        
        argsBuilder.Append($" --{language}_out=\"{outputPath}\"");
        argsBuilder.Append($" \"{protoFile}\"");

        var args = argsBuilder.ToString().Replace("\\", "/").Trim();
        //Console.WriteLine(args);
        var error = ExecuteCommand(_protoc, args);
        if (!string.IsNullOrEmpty(error))
        {
            Console.WriteLine($"error: {error}");
        }
    }

    string ExecuteCommand(string command, string commandArgs)
    {
        var processInfo = new ProcessStartInfo()
        {
            FileName = command,
            Arguments = commandArgs,
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
        };

        var process = Process.Start(processInfo);
        process.WaitForExit();

        var standardOut = process.StandardOutput.ReadToEnd();
        var errorOut = process.StandardError.ReadToEnd();
        
        // Console.WriteLine(standardOut);
        // Console.WriteLine(errorOut);
        return errorOut;
    }
}