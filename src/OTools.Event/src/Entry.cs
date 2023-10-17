using OTools.Common;

namespace OTools.Events;

public sealed class Entry : IStorable
{
    public Guid Id { get; init; }

    public Person Person { get; set; }
    public Result? Result { get; set; }

    public Entry(Person person, Result? result)
    {
        Id = Guid.NewGuid();

        Person = person;
        Result = result;
    }

    public Entry(Guid id, Person person, Result? result)
    {
        Id = id;

        Person = person;
        Result = result;
    }
}