namespace teams_assignment_ics_host.Entities;

public class Assignment {
    public string Id {get;set;} = String.Empty;
    public string DisplayName {get;set;} = String.Empty;
    public DateTime DueDateTime {get;set;}
}