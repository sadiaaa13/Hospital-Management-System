using System;

public class MedicalRecord
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public DateTime Date { get; set; }
    public string Diagnosis { get; set; }
    public string Prescription { get; set; }
    public string Notes { get; set; }
    // public virtual Patient Patient { get; set; }
}
