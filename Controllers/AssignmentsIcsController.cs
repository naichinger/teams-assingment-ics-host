using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Microsoft.AspNetCore.Mvc;
using teams_assignment_ics_host.Entities;

namespace teams_assignment_ics_host.Controllers;

[ApiController]
[Route("[controller]")]
public class AssignmentsIcsController : ControllerBase
{
    private readonly ILogger<AssignmentsIcsController> _logger;
    private readonly HttpClient httpClient;

    public AssignmentsIcsController(ILogger<AssignmentsIcsController> logger)
    {
        _logger = logger;
        
        httpClient = new HttpClient();
    }

    [HttpGet]
    public async Task<ActionResult> Get()
    {
        string token = new ConfigurationBuilder ()
                .AddEnvironmentVariables().Build ()["BearerToken"];

        using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, "https://assignments.onenote.com/api/v1.0/edu/me/work?$filter=( status eq microsoft.education.assignments.api.educationAssignmentStatus'assigned' and isCompleted eq false )"))
        {
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await httpClient.SendAsync(requestMessage);
            response.EnsureSuccessStatusCode();

            Assignments? assignments = await response.Content.ReadFromJsonAsync<Assignments>();
            if(assignments == null) {
                Console.WriteLine("IS NULL");
            }else {
                var calendar = new Ical.Net.Calendar();
                foreach(var assignment in assignments.Value) {
                    calendar.Events.Add(new CalendarEvent {
                        Class = "PUBLIC",
                        Summary = assignment.DisplayName,
                        Created = new CalDateTime(DateTime.Now),
                        Description = assignment.DisplayName,
                        Start = new CalDateTime(Convert.ToDateTime(assignment.DueDateTime)),
                        End = new CalDateTime(Convert.ToDateTime(assignment.DueDateTime)),
                        Sequence = 0,
                        Uid = assignment.Id,
                        Location = "Teams",
                    });
                }
                var serializer = new CalendarSerializer(new SerializationContext());
                var serializedCalendar = serializer.SerializeToString(calendar);
                var bytesCalendar = Encoding.UTF8.GetBytes(serializedCalendar);
                MemoryStream ms = new MemoryStream(bytesCalendar);

                return File(ms,"text/calendar","assignments.ics");
            }
        }
        return Ok();
    }
}
