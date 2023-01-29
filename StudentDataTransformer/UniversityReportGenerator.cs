using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StudentDataTransformer;

public static class UniversityReportGenerator
{
    public static void Generate(HashSet<StudentDetails> details, string targetDirectory, string format)
    {
        if ("json".Equals(format))
        {
            var report = new UniversityReport(details);
            JsonReportAdapter.Save(report, targetDirectory);
        }
        else
        {
            LogWriter.Log("Nie znaleziono odpowiedniego formatu docelowego");
        }
    }
}

public class UniversityReport
{

    [JsonConverter(typeof(DateOnlyJsonConverter))]
    public DateOnly CreatedAt { get; }
    public string Author { get; }
    public HashSet<StudentDetails> Studenci { get; }
    
    public HashSet<StudiesSummary> ActiveStudies { get; }
    public UniversityReport(HashSet<StudentDetails> details)
    {
        Studenci = details;
        Author = "Jan Kowalski";
        CreatedAt = DateOnly.FromDateTime(DateTime.Now);
        ActiveStudies = StudiesSummary.Of(details);
    }
}

public class StudiesSummary
{
    public string Name { get; }
    public int NumberOfStudents { get; }

    private StudiesSummary(string name, int noOfStudents)
    {
        Name = name;
        NumberOfStudents = noOfStudents;
    }

    public static HashSet<StudiesSummary> Of(HashSet<StudentDetails> students)
    {
        return students
            .GroupBy(student => new { student.Studies.Name })
            .Select(group => new { Name = group.Key, Count = group.Count() })
            .Select(x => new StudiesSummary(x.Name.Name, x.Count))
            .ToHashSet();
    }
}

public static class JsonReportAdapter
{
    public static void Save(UniversityReport universityReport, string targetDirectory)
    {
        var options = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true,
            IncludeFields = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
        var report = new Report(universityReport);
        var json = JsonSerializer.Serialize(report, options);
        File.WriteAllText(targetDirectory + "\\result.json", json);
    }
}

public class Report
{
    public Report(UniversityReport uczelnia)
    {
        Uczelnia = uczelnia;
    }

    public UniversityReport Uczelnia { get; }

}

public sealed class DateOnlyJsonConverter : JsonConverter<DateOnly>
{
    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return DateOnly.FromDateTime(reader.GetDateTime());
    }

    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
    {
        var isoDate = value.ToString("dd.MM.yyyy");
        writer.WriteStringValue(isoDate);
    }
}
