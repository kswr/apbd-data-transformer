namespace StudentDataTransformer;

public static class StudentDetailsRepository
{
    public static List<StudentDetails> Get(string sourceFile)
    {
        return StudentCsvAdapter.Read(sourceFile);
    }

    public static void Save(
        List<StudentDetails> details,
        string targetFile, 
        string format)
    {
        if ("json".Equals(format))
        {
            StudentJsonAdapter.Save(details, targetFile);
        }
        else
        {
            LogWriter.Log("Nie znaleziono odpowiedniego formatu docelowego");
        }
    }
}

public class StudentJsonAdapter
{
    public static void Save(List<StudentDetails> details, string targetFile)
    {
        throw new NotImplementedException();
    }
}

public class StudentCsvAdapter
{
    public static List<StudentDetails> Read(string sourceFile)
    {
        throw new NotImplementedException();
    }
}

public class StudentDetails
{
    
}