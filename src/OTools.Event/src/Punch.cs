using OTools.Common;

namespace OTools.Events;

public sealed class Punch : IStorable
{
    public Guid Id { get; init; }

    public string CardNumber { get; set; }
    public ushort Code { get; set; }
    public DateTime TimeStamp { get; set; }

    public Punch(string cardNumber, ushort code, DateTime timeStamp)
    {
        Id = Guid.NewGuid();

        CardNumber = cardNumber;
        Code = code;
        TimeStamp = timeStamp;
    }

    public Punch(Guid id, string cardNumber, ushort code, DateTime timeStamp)
    {
        Id = id;

        CardNumber = cardNumber;
        Code = code;
        TimeStamp = timeStamp;
    }
}