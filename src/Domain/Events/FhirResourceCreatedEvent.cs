namespace FHIRAI.Domain.Events;

public class FhirResourceCreatedEvent : BaseEvent
{
    public FhirResourceCreatedEvent(FhirResource fhirResource)
    {
        FhirResource = fhirResource;
    }

    public FhirResource FhirResource { get; }
}
