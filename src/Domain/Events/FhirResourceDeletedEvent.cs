namespace FHIRAI.Domain.Events;

public class FhirResourceDeletedEvent : BaseEvent
{
    public FhirResourceDeletedEvent(FhirResource fhirResource)
    {
        FhirResource = fhirResource;
    }

    public FhirResource FhirResource { get; }
}
