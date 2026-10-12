using SmartClinic.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace SmartClinic.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PatientsController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public PatientsController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet]
    public IActionResult GetPatients()
    {
        var connectionString = _configuration
            .GetConnectionString("SmartClinicConnection");

        var patients = new List<object>();

        using var connection = new SqlConnection(connectionString);

        const string query = """
            SELECT id, name, gender, phone, address
            FROM patients
            ORDER BY id
            """;

        using var command = new SqlCommand(query, connection);

        connection.Open();

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            patients.Add(new
            {
                id = reader.GetInt32(0),
                name = reader.GetString(1),
                gender = reader.GetString(2).Trim(),
                phone = reader.IsDBNull(3) ? null : reader.GetString(3),
                address = reader.IsDBNull(4) ? null : reader.GetString(4)
            });
        }

        return Ok(patients);
    }


    [HttpGet("{id:int}")]
    public IActionResult GetPatientById(int id)
    {
        var connectionString = _configuration
            .GetConnectionString("SmartClinicConnection");

        using var connection = new SqlConnection(connectionString);

        const string query = """
        SELECT id, name, gender, phone, address
        FROM patients
        WHERE id = @id
        """;

        using var command = new SqlCommand(query, connection);
        command.Parameters.Add("@id", SqlDbType.Int).Value = id;

        connection.Open();

        using var reader = command.ExecuteReader();

        if (!reader.Read())
        {
            return NotFound(new
            {
                message = $"Pasien dengan ID {id} tidak ditemukan."
            });
        }

        var patient = new
        {
            id = reader.GetInt32(0),
            name = reader.GetString(1),
            gender = reader.GetString(2).Trim(),
            phone = reader.IsDBNull(3) ? null : reader.GetString(3),
            address = reader.IsDBNull(4) ? null : reader.GetString(4)
        };

        return Ok(patient);
    }


    [HttpPost]
    public IActionResult CreatePatient(
        [FromBody] PatientCreateRequest request)
    {
        var connectionString = _configuration
            .GetConnectionString("SmartClinicConnection");

        using var connection = new SqlConnection(connectionString);

        const string query = """
        INSERT INTO patients (name, gender, phone, address)
        OUTPUT INSERTED.id
        VALUES (@name, @gender, @phone, @address)
        """;

        using var command = new SqlCommand(query, connection);

        command.Parameters.Add("@name", SqlDbType.NVarChar, 100)
            .Value = request.Name;

        command.Parameters.Add("@gender", SqlDbType.Char, 1)
            .Value = request.Gender;

        command.Parameters.Add("@phone", SqlDbType.VarChar, 20)
            .Value = (object?)request.Phone ?? DBNull.Value;

        command.Parameters.Add("@address", SqlDbType.NVarChar, 200)
            .Value = (object?)request.Address ?? DBNull.Value;

        connection.Open();

        var newId = Convert.ToInt32(command.ExecuteScalar());

        return CreatedAtAction(
            nameof(GetPatientById),
            new { id = newId },
            new
            {
                id = newId,
                name = request.Name,
                gender = request.Gender,
                phone = request.Phone,
                address = request.Address
            });
    }


    [HttpPut("{id:int}")]
    public IActionResult UpdatePatient(
        int id,
        [FromBody] PatientCreateRequest request)
    {
        var connectionString = _configuration
            .GetConnectionString("SmartClinicConnection");

        using var connection = new SqlConnection(connectionString);

        const string query = """
        UPDATE patients
        SET name = @name,
            gender = @gender,
            phone = @phone,
            address = @address
        WHERE id = @id
        """;

        using var command = new SqlCommand(query, connection);

        command.Parameters.Add("@id", SqlDbType.Int).Value = id;
        command.Parameters.Add("@name", SqlDbType.NVarChar, 100)
            .Value = request.Name;
        command.Parameters.Add("@gender", SqlDbType.Char, 1)
            .Value = request.Gender;
        command.Parameters.Add("@phone", SqlDbType.VarChar, 20)
            .Value = (object?)request.Phone ?? DBNull.Value;
        command.Parameters.Add("@address", SqlDbType.NVarChar, 200)
            .Value = (object?)request.Address ?? DBNull.Value;

        connection.Open();

        int rowsAffected = command.ExecuteNonQuery();

        if (rowsAffected == 0)
        {
            return NotFound(new
            {
                message = $"Pasien dengan ID {id} tidak ditemukan."
            });
        }

        return Ok(new
        {
            message = "Data pasien berhasil diperbarui.",
            id
        });
    }


    [HttpDelete("{id:int}")]
    public IActionResult DeletePatient(int id)
    {
        var connectionString = _configuration
            .GetConnectionString("SmartClinicConnection");

        using var connection = new SqlConnection(connectionString);

        const string query = """
        DELETE FROM patients
        WHERE id = @id
        """;

        using var command = new SqlCommand(query, connection);

        command.Parameters.Add("@id", SqlDbType.Int).Value = id;

        connection.Open();

        int rowsAffected = command.ExecuteNonQuery();

        if (rowsAffected == 0)
        {
            return NotFound(new
            {
                message = $"Pasien dengan ID {id} tidak ditemukan."
            });
        }

        return Ok(new
        {
            message = "Data pasien berhasil dihapus.",
            id
        });
    }


}
