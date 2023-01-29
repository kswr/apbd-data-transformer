using System.Text.Json;
using System.Text.RegularExpressions;

namespace StudentDataTransformer;

public static class StudentDetailsRepository
{
    public static HashSet<StudentDetails> Get(string sourceFile)
    {
        return StudentCsvAdapter.Read(sourceFile);
    }

    public static void Save(
        HashSet<StudentDetails> details,
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

public static class StudentJsonAdapter
{
    public static void Save(HashSet<StudentDetails> details, string targetFile)
    {
        var json = JsonSerializer.Serialize(details);
        File.WriteAllText(targetFile, json);
    }
}

public static class StudentCsvAdapter
{
    public static HashSet<StudentDetails> Read(string sourceFile)
    {
        var studentDetails = new HashSet<StudentDetails>();
        var lines = File.ReadAllLines(sourceFile);
        foreach (var line in lines)
        {
            try
            {
                var details = StudentDetails.OfCsv(line);
                if (!studentDetails.Add(details))
                {
                    LogWriter.Log($"Duplicate data: {line}");
                }
            }
            catch (Exception e)
            {
                LogWriter.Log(e);
            }
        }

        return studentDetails;
    }
}

public class StudentDetails
{
    public string IndexNumber { get; }
    public string FName { get; }
    public string LName { get; }
    public DateOnly Birthdate { get; private set; }
    public string Email { get; private set; }
    public string MothersName { get; private set; }
    public string FathersName { get; private set; }
    public Studies Studies { get; private set; }

    private StudentDetails(string index, string fName, string lName)
    {
        IndexNumber = index;
        FName = fName;
        LName = lName;
    }
    public static StudentDetails OfCsv(string row)
    {
        if (row is null) throw new ArgumentException("Row of data is null");
        var fields = row.Split(",");
        Sanitize(fields);
        if (fields.Length != 9) throw new ArgumentException("Incorrect number of columns in CSV row");
        var index = Index(fields[5]);
        var fname = Name(fields[0]);
        var lname = Name(fields[1]);
        return new StudentDetails(index, fname, lname)
        {
            Studies = Studies.Of(fields[2], fields[3]),
            Email = EmailFrom(fields[6]),
            Birthdate = DateOnly.Parse(fields[5]),
            MothersName = Name(fields[7]),
            FathersName = Name(fields[8])
        };
    }

    private static void Sanitize(string[] fields)
    {
        if (fields.Any(field => field.Equals("")))
        {
            throw new ArgumentException("Empty field");
        }
    }

    private static string Index(string indexNum)
    {
        if (!indexNum.All(char.IsDigit) || indexNum.Length > 10) throw new ArgumentException($"Index {indexNum} format incorrect");
        return $"s{indexNum}";
    }

    private static string Name(string name)
    {
        if (name.Length > 30) throw new ArgumentException($"Name {name} to long");
        return Regex.Replace(name, @"[\d-]", string.Empty); 
    }

    private static string EmailFrom(string email)
    {
        var regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
        if (!regex.Match(email).Success) throw new ArgumentException($"Incorrect email {email}");
        return email;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        var other = (StudentDetails) obj;
        return IndexNumber.Equals(other.IndexNumber)
               && FName.Equals(other.FName)
               && LName.Equals(other.LName);
    }

    public override int GetHashCode()
    {
        return IndexNumber.GetHashCode() * 17 + FName.GetHashCode() * 7 + LName.GetHashCode();
    }
}

public class Studies
{
    public StudiesName Name { get; }
    public StudiesMode Mode { get; }

    private Studies(StudiesName name, StudiesMode mode)
    {
        Name = name;
        Mode = mode;
    }
    public static Studies Of(string name, string mode)
    {
        return new(StudiesName.Of(name), StudiesMode.Of(mode));
    }
}

public class StudiesName
{
    public static readonly StudiesName ComputerScience = new("Computer Science");
    public static readonly StudiesName NewMediaArt = new("New Media Art");

    private const string NewMediaArtString = "Sztuka Nowych Mediów";
    private const string ComputerScienceString = "Informatyka";
    private string Name { get; }

    private StudiesName(string name)
    {
        Name = name;
    }

    public static StudiesName Of(string name)
    {
        if (name is null) throw new ArgumentException("Null studies name");
        if (name.Contains(NewMediaArtString)) return NewMediaArt;
        if (name.Contains(ComputerScienceString)) return ComputerScience;
        throw new ArgumentException($"Unknown studies name {name}");
    }

    public override string ToString()
    {
        return Name;
    }
}

public class StudiesMode
{
    private const string FullTimeString = "Dzienne";
    private const string PartTimeString = "Zaoczne";

    private static readonly StudiesMode FullTime = new(FullTimeString);
    private static readonly StudiesMode PartTime = new(PartTimeString);
    
    private string Mode { get; }

    private StudiesMode(string mode)
    {
        Mode = mode;
    }

    public static StudiesMode Of(string mode)
    {
        return mode switch
        {
            FullTimeString => FullTime,
            PartTimeString => PartTime,
            _ => throw new ArgumentException($"Unknown studies mode {mode}")
        };
    }

    public override string ToString()
    {
        return Mode;
    }
}
