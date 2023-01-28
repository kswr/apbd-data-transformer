namespace StudentDataTransformer;

public static class TransformerApp
{
    public static void Main(string[] args)
    {
        try
        {
            var arguments = new ProgramArguments(args);
            var studentDetails = StudentDetailsRepository.Get(arguments.SourceFile);
            StudentDetailsRepository.Save(studentDetails, arguments.TargetFile, arguments.Format);
        }
        catch (Exception e)
        {
            LogWriter.Log(e);
        }
    }
}

public class ProgramArguments
{
    private static readonly string JsonFormat = "json";

    public string SourceFile { get; }

    public string TargetFile { get; }

    public string Format { get; }

    public ProgramArguments(string[] args)
    {
        if (args is null) throw new ArgumentException("Nie podano argumentów do programu");
        if (args.Length != 3) throw new ArgumentException("Podano niewłaściwą ilość argumentów do programu");
        SourceFile = ValidateFile(args[0]);
        TargetFile = ValidateDirectory(args[1]);
        Format = ValidateFormat(args[2]);
    }

    private static string ValidateFile(string path)
    {
        try
        {
            if (File.GetAttributes(path).HasFlag(FileAttributes.Directory))
            {
                throw new ArgumentException("Plik źródłowy nie może być folderem");
            }

            return path;
        }
        catch (FileNotFoundException e)
        {
            throw new ArgumentException($"Plik {path} nie istnieje", e);
        }
    }

    private static string ValidateDirectory(string path)
    {
        try
        {
            if (!File.GetAttributes(path).HasFlag(FileAttributes.Directory))
            {
                throw new ArgumentException("Folder docelowy nie może być plikiem");
            }

            return path;
        }
        catch (FileNotFoundException e)
        {
            throw new ArgumentException($"Folder {path} nie istnieje", e);
        }
    }

    private static string ValidateFormat(string format)
    {
        if (!JsonFormat.Equals(format)) throw new ArgumentException("Niepoprawny format pliku wyjściowego");
        return format;
    }
}