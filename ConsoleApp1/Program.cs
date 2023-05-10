

using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using ConsoleApp1;


        if (args.Length < 3)
        {
            throw new ArgumentOutOfRangeException("Za mało argumentów. Potrzebne są ścieżki do plików: wejściowego CSV, wyjściowego JSON i pliku logów.");
        }

        string csvPath = args[0];
        string jsonPath = args[1];
        string logsPath = args[2];

        if (!File.Exists(csvPath))
        {
            throw new FileNotFoundException($"Plik {csvPath} nie istnieje.");
        }

        if (!Directory.Exists(Path.GetDirectoryName(jsonPath)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(jsonPath));
        }

        if (!Directory.Exists(Path.GetDirectoryName(logsPath)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(logsPath));
        }

        var lines = File.ReadLines(csvPath);
        var students = new List<Student>();
        var activeStudiesDir = new Dictionary<string, int>();
        var logs = File.CreateText(logsPath);

        lines.ToList().ForEach(line =>
{
    var splitted = line.Split(',');
    if(splitted.Length != 9)
    {
        logs.WriteLine($"Wiersz nie posiada odpowiedniej ilości kolumn: {line}");
        return;
    }
    if(splitted.Any(e => e.Trim() == ""))
    {
        logs.WriteLine($"Wiersz nie może posiadać pustych kolumn: {line}");
        return;
    }
    var newStudies = new Studies
    {
        Name = splitted[2],
        Mode = splitted[3]
    };
    var newStudent = new Student
    {
        IndexNumber = splitted[4],
        Fname = splitted[0],
        Lname = splitted[1],
        Birthdate = DateOnly.Parse(splitted[5]),
        Email = splitted[6],
        MothersName = splitted[7],
        FathersName= splitted[8],
        Studies = newStudies
    };
    if (students.Any(student => 
        student.Fname == newStudent.Fname &&
        student.Lname == newStudent.Lname &&
        student.IndexNumber == newStudent.IndexNumber
    ))
    {
        logs.WriteLine($"Duplikat: {line}");
        return;
    }

    activeStudiesDir[newStudies.Name] = !activeStudiesDir.ContainsKey(newStudies.Name) ? 1 : activeStudiesDir[newStudies.Name] + 1;
    students.Add(newStudent);
});
        
        File.WriteAllText(jsonPath, JsonSerializer.Serialize(
            new UczelniaWrapper
            {
                Uczelnia = new Uczelnia
                {
                    CreatedAt = DateOnly.FromDateTime(DateTime.Now),
                    Author = "Jan Kowalski",
                    Students = students,
                    ActiveStudies = activeStudiesDir.Select(e => new ActiveStudies { Name = e.Key, NumberOfStudents = e.Value })
                }
            },
            new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }
        ));

class Studies
{
    public string Name { get; set; }
    public string Mode { get; set; }
}

class Uczelnia
{
    public DateOnly CreatedAt { get; set; }
    public string Author { get; set; }
    public IEnumerable<Student> Students { get; set; }
    public IEnumerable<ActiveStudies> ActiveStudies { get; set; }
}

class ActiveStudies
{
    public string Name { get; set; }
    public int NumberOfStudents { get; set; }
}

class UczelniaWrapper
{
    public Uczelnia Uczelnia { get; set; }
}