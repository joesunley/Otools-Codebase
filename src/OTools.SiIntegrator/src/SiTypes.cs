
using OTools.Common;
using Sunley.Mathematics;

namespace OTools.SiIntegrator;

public struct SiCard
{
    public string Siid { get; set; }
    public SiCardPersonalData PersonalData { get; set; }

    public PunchData ClearPunch { get; set; }
    public PunchData ClearPunchReserve { get; set; }
    public PunchData CheckPunch { get; set; }
    public PunchData StartPunch { get; set; }
    public PunchData StartPunchReserve { get; set; }
    public PunchData FinishPunch { get; set; }
    public PunchData FinishPunchReserve { get; set; }

    public List<PunchData> ControlPunchList { get; set; }

    public static SiCard Parse(XMLDocument doc)
    {
        XMLNode pers = doc.Root.Children["PersonalData"];

        SiCardPersonalData personalData = new()
        {
            FirstName = pers.Attributes["firstName"],
            LastName = pers.Attributes["lastName"],
            Sex = pers.Attributes["sex"],
            DateOfBirth = pers.Attributes["dateOfBirth"],
            Class = pers.Attributes["class"],
            Club = pers.Attributes["club"],
            Email = pers.Attributes["email"],
            Phone = pers.Attributes["phone"],
            Street = pers.Attributes["street"],
            City = pers.Attributes["city"],
            Country = pers.Attributes["country"],
            ZipCode = pers.Attributes["zipCode"]
        };

        return new()
        {
            Siid = doc.Root.Attributes["siid"],
            PersonalData = personalData,
            ClearPunch = PunchData.Parse(doc.Root.Children["ClearPunch"]),
            ClearPunchReserve = PunchData.Parse(doc.Root.Children["ClearPunchReserve"]),
            CheckPunch = PunchData.Parse(doc.Root.Children["CheckPunch"]),
            StartPunch = PunchData.Parse(doc.Root.Children["StartPunch"]),
            StartPunchReserve = PunchData.Parse(doc.Root.Children["StartPunchReserve"]),
            FinishPunch = PunchData.Parse(doc.Root.Children["FinishPunch"]),
            FinishPunchReserve = PunchData.Parse(doc.Root.Children["FinishPunchReserve"]),
            ControlPunchList = doc.Root.Children["ControlPunches"].Children.Select(PunchData.Parse).ToList()
        };
    }
}

public struct PunchData
{
    public uint CodeNumber { get; set; }
    public string Siid { get; set; }

    public DayOfWeek DayOfWeek { get; set; }
    public DateTime PunchDateTime { get; set; }

    public static PunchData Parse(XMLNode node)
    {
        if (node.Attributes.Count == 0)
            return new();

        return new()
        {
            CodeNumber = node.Attributes["code"].Parse<uint>(),
            Siid = node.Attributes["siid"],
            DayOfWeek = node.Attributes["dayOfWeek"] switch
            {
                "Monday" => DayOfWeek.Monday,
                "Tuesday" => DayOfWeek.Tuesday,
                "Wednesday" => DayOfWeek.Wednesday,
                "Thursday" => DayOfWeek.Thursday,
                "Friday" => DayOfWeek.Friday,
                "Saturday" => DayOfWeek.Saturday,
                "Sunday" => DayOfWeek.Sunday,
                _ => throw new Exception("Invalid day of week")
            },
            PunchDateTime = new(node.Attributes["timeStamp"].Parse<long>()),
        };
    }
}

public struct SiCardPersonalData
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Sex { get; set; }
    public string DateOfBirth { get; set; }
    public string Class { get; set; }
    public string Club { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Street { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
    public string ZipCode { get; set; }
}