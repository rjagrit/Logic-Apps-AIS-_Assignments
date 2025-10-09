using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SupportTickets
{
    public class TicketFunction
    {
        private readonly ILogger _logger;

        public TicketFunction(ILogger<TicketFunction> logger)
        {
            _logger = logger;
        }

        [Function("CreateTicket")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            _logger.LogInformation("Processing ticket creation request...");

            // Read request body
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            if (string.IsNullOrEmpty(requestBody))
            {
                var badResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("Request body cannot be empty.");
                return badResponse;
            }

            // Parse JSON
            var data = JsonSerializer.Deserialize<TicketRequest>(requestBody);

            if (data == null || string.IsNullOrEmpty(data.Name) || string.IsNullOrEmpty(data.Email) || string.IsNullOrEmpty(data.Issue))
            {
                var badResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("Missing required fields: name, email, issue.");
                return badResponse;
            }

            // Generate Ticket ID
            string ticketId = $"TICKET-{Guid.NewGuid().ToString().Substring(0, 8)}";

            var ticketResponse = new TicketResponse
            {
                TicketId = ticketId,
                Name = data.Name,
                Email = data.Email,
                Issue = data.Issue,
                Status = "Created",
                CreatedAt = DateTime.UtcNow
            };

            var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");
            await response.WriteStringAsync(JsonSerializer.Serialize(ticketResponse));

            _logger.LogInformation($"Ticket created successfully: {ticketId}");

            return response;
        }
    }
    // Request model
    public class TicketRequest
    {
        [JsonPropertyName("name")] // <-- Tells the serializer to look for "name"
        public required string Name { get; set; }

        [JsonPropertyName("email")] // <-- Tells the serializer to look for "email"
        public required string Email { get; set; }

        [JsonPropertyName("issue")] // <-- Tells the serializer to look for "issue"
        public required string Issue { get; set; }
    }

    // Response model
    public class TicketResponse
    {
        // These fields are set when creating the response object
        public required string TicketId { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string Issue { get; set; }
        public required string Status { get; set; }
        public required DateTime CreatedAt { get; set; }
    }
}
