using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.Data.SqlClient;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks; // This is required for Task<>
using System.Collections.Generic;


[ApiController]
[Route("api/[controller]")]
public class NL2SQLController : ControllerBase
{
    private readonly IConfiguration _config;
    public NL2SQLController(IConfiguration config)
    {
        _config = config;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] QueryRequest request)
    {
        var builder = Kernel.CreateBuilder();
        builder.AddAzureOpenAIChatCompletion(
            deploymentName: _config["AzureOpenAI:Deployment"],
            endpoint: _config["AzureOpenAI:Endpoint"],
            apiKey: _config["AzureOpenAI:ApiKey"]
        );
        var kernel = builder.Build();

        var prompt = $@"
You are a SQL assistant. Convert the following natural language request into a SQL query.

Use the following schema (T-SQL syntax for Microsoft SQL Server):

Tables:
Departments(ID, Name)  
Employees(ID, Name, Title, DepartmentID)  
Salaries(ID, EmployeeID, Salary, Date)  
Projects(ID, Name, Budget)  
EmployeeProjects(EmployeeID, ProjectID)

Requirements:
- Use proper T-SQL syntax.
- Do NOT use backticks or markdown formatting.
- Only return the raw SQL query.

Request: {request.Question}

SQL:
";

        var result = await kernel.InvokePromptAsync(prompt);
        string sqlQuery = result.ToString()
            .Replace("```sql", "")
            .Replace("```", "")
            .Replace("`", "")
            .Trim();

        ////Console.WriteLine("Generated SQL: " + sqlQuery);

        var rows = new List<Dictionary<string, object>>();
        string connStr = _config.GetConnectionString("SqlServer");

        try
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                await conn.OpenAsync();
                SqlCommand cmd = new SqlCommand(sqlQuery, conn);
                SqlDataReader reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var row = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row[reader.GetName(i)] = reader.GetValue(i);
                    }
                    rows.Add(row);
                }
            }
        }
        catch (SqlException ex)
        {
            ////Console.WriteLine("SQL Error: " + ex.Message);
            return BadRequest(new { error = "Invalid SQL generated.", details = ex.Message, sql = sqlQuery });
        }

        return Ok(new
        {
            sql = sqlQuery,
            data = rows
        });
    }
}

public class QueryRequest
{
    public string Question { get; set; }
}
