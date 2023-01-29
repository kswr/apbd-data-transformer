namespace StudentDataTransformer;

public static class TransformerApp
{
    public static void Main(string[] args)
    {
        LogWriter.Cleanup();
        try
        {
            var arguments = new ProgramArguments(args);
            var studentDetails = StudentDetailsRepository.Get(arguments.SourceFile);
            StudentDetailsRepository.Save(studentDetails, arguments.TargetDirectory, arguments.Format);
        }
        catch (Exception e)
        {
            LogWriter.Log(e);
            throw;
        }
    }
}

public class ProgramArguments
{
    private static readonly string JsonFormat = "json";

    public string SourceFile { get; }

    public string TargetDirectory { get; }

    public string Format { get; }

    public ProgramArguments(string[] args)
    {
        if (args is null) throw new ArgumentException("Nie podano argumentów do programu");
        if (args.Length != 3) throw new ArgumentException("Podano niewłaściwą ilość argumentów do programu");
        SourceFile = ValidateFile(Sanitize(args[1]));
        TargetDirectory = ValidateDirectory(Sanitize(args[0]));
        Format = ValidateFormat(args[2]);
    }

    private static string Sanitize(string path)
    {
        return path
            .Replace("\"", "")
            .Replace("„", "")
            .Replace("”", "");
    }

    private static string ValidateFile(string path)
    {
        Console.WriteLine(path);
        var absolutePath = AbsolutePath(path);
        Console.WriteLine(absolutePath);
        try
        {
            if (File.GetAttributes(absolutePath).HasFlag(FileAttributes.Directory))
            {
                throw new ArgumentException("Plik źródłowy nie może być folderem");
            }

            return absolutePath;
        }
        catch (FileNotFoundException e)
        {
            throw new ArgumentException($"Plik {absolutePath} z {path} nie istnieje", e);
        }
    }

    private static string ValidateDirectory(string path)
    {
        var absolutePath = AbsolutePath(path);
        try
        {
            if (!File.GetAttributes(absolutePath).HasFlag(FileAttributes.Directory))
            {
                throw new ArgumentException("Folder docelowy nie może być plikiem");
            }

            return absolutePath;
        }
        catch (FileNotFoundException e)
        {
            throw new ArgumentException($"Folder {absolutePath} z {path} nie istnieje", e);
        }
    }

    private static string AbsolutePath(string path)
    {
        if (!path.StartsWith(".")) return path;
        var relative = path.Remove(0, 1);
        var currentDir = Directory.GetCurrentDirectory();
        if (relative.StartsWith("\\") || relative.StartsWith("/"))
        {
            return $"{currentDir}{relative}";
        }

        return $"{currentDir}\\{relative}";
    }

    private static string ValidateFormat(string format)
    {
        if (!JsonFormat.Equals(format)) throw new ArgumentException("Niepoprawny format pliku wyjściowego");
        return format;
    }
}