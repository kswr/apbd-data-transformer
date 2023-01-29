using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace StudentDataTransformer;

public static class StudentDetailsRepository
{
    public static HashSet<StudentDetails> Get(string sourceFile)
    {
        return StudentCsvAdapter.Read(sourceFile);
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
                LogWriter.Log(e, $"for record {line}");
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
    
    [JsonConverter(typeof(DateOnlyJsonConverter))]
    public DateOnly Birthdate { get; private init; }
    public string Email { get; private init; }
    public string MothersName { get; private init; }
    public string FathersName { get; private init; }
    public Studies Studies { get; private init; }

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
        var index = Index(fields[4]);
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

    public override string ToString()
    {
        return
            $"StudentDetails[indexNumber:{IndexNumber}, fName:{FName}, lName:{LName}, birthDate:{Birthdate}, email:{Email}, studies:{Studies}, mothersName:{MothersName}, fathersName:{FathersName}]";
    }
}

public class Studies
{
    public string Name { get; }
    public string Mode { get; }

    private Studies(StudiesName name, StudiesMode mode)
    {
        Name = name.Name;
        Mode = mode.Mode;
    }
    public static Studies Of(string name, string mode)
    {
        return new(StudiesName.Of(name), StudiesMode.Of(mode));
    }

    public override string ToString()
    {
        return $"Studies[name: {Name}, mode: {Mode}]";
    }
}

public class StudiesName
{
    public static readonly StudiesName ComputerScience = new("Computer Science");
    public static readonly StudiesName NewMediaArt = new("New Media Art");

    private const string NewMediaArtString = "Sztuka Nowych Mediów";
    private const string ComputerScienceString = "Informatyka";
    public string Name { get; }

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
        return $"StudiesName:[name: {Name}]";
    }
}

public class StudiesMode
{
    private const string FullTimeString = "Dzienne";
    private const string PartTimeString = "Zaoczne";
    private const string OnLineString = "Internetowe";

    private static readonly StudiesMode FullTime = new(FullTimeString);
    private static readonly StudiesMode PartTime = new(PartTimeString);
    private static readonly StudiesMode OnLine = new(OnLineString);
    
    public string Mode { get; }

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
            OnLineString => OnLine,
            _ => throw new ArgumentException($"Unknown studies mode {mode}")
        };
    }

    public override string ToString()
    {
        return $"StudiesMode[mode:{Mode}";
    }
}
