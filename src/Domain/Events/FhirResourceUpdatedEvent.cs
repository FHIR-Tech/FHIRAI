namespace FHIRAI.Domain.Events;

public class FhirResourceUpdatedEvent : BaseEvent
{
    public FhirResourceUpdatedEvent(FhirResource fhirResource)
    {
        FhirResource = fhirResource;
    }

    public FhirResource FhirResource { get; }
}
