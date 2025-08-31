namespace FHIRAI.Domain.Events;

public class PatientDeletedEvent : BaseEvent
{
    public PatientDeletedEvent(Patient patient)
    {
        Patient = patient;
    }

    public Patient Patient { get; }
}
