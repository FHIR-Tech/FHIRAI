namespace FHIRAI.Domain.Events;

public class PatientCreatedEvent : BaseEvent
{
    public PatientCreatedEvent(Patient patient)
    {
        Patient = patient;
    }

    public Patient Patient { get; }
}
