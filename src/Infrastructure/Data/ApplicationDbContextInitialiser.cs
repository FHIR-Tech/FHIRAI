using FHIRAI.Domain.Constants;
using FHIRAI.Domain.Entities;
using FHIRAI.Domain.Enums;
using FHIRAI.Infrastructure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace FHIRAI.Infrastructure.Data;

public static class InitialiserExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();

        await initialiser.InitialiseAsync();
        await initialiser.SeedAsync();
    }
}

public class ApplicationDbContextInitialiser
{
    private readonly ILogger<ApplicationDbContextInitialiser> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public ApplicationDbContextInitialiser(ILogger<ApplicationDbContextInitialiser> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task InitialiseAsync()
    {
        try
        {
            // Apply migrations to create the database schema
            await _context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    public async Task TrySeedAsync()
    {
        // Default roles
        var administratorRole = new IdentityRole(Roles.Administrator);

        if (_roleManager.Roles.All(r => r.Name != administratorRole.Name))
        {
            await _roleManager.CreateAsync(administratorRole);
        }

        // Default users
        var administrator = new ApplicationUser { UserName = "administrator@localhost", Email = "administrator@localhost" };

        if (_userManager.Users.All(u => u.UserName != administrator.UserName))
        {
            await _userManager.CreateAsync(administrator, "Administrator1!");
            if (!string.IsNullOrWhiteSpace(administratorRole.Name))
            {
                await _userManager.AddToRolesAsync(administrator, new [] { administratorRole.Name });
            }
        }

        // Default data
        // Seed, if necessary
        if (!_context.TodoLists.Any())
        {
            _context.TodoLists.Add(new TodoList
            {
                Title = "Todo List",
                Items =
                {
                    new TodoItem { Title = "Make a todo list 📃" },
                    new TodoItem { Title = "Check off the first item ✅" },
                    new TodoItem { Title = "Realise you've already done two things on the list! 🤯"},
                    new TodoItem { Title = "Reward yourself with a nice, long nap 🏆" },
                }
            });

            await _context.SaveChangesAsync();
        }

        // Seed Healthcare/FHIR data
        await SeedHealthcareDataAsync();
    }

    private async Task SeedHealthcareDataAsync()
    {
        // Seed FhirResources if none exist
        if (!_context.FhirResources.Any())
        {
            await SeedFhirResourcesAsync();
        }

        // Note: PatientAccess requires actual Patient entities to exist
        // We'll skip this for now since we only have FHIR resources
        // PatientAccess can be created when actual Patient entities are added
    }

    private async Task SeedFhirResourcesAsync()
    {
        var fhirResources = new List<FhirResource>();

        // Create sample FHIR Patient resources
        var samplePatients = new[]
        {
            new { id = "patient-001", firstName = "John", lastName = "Smith", gender = "male", birthDate = "1985-03-15" },
            new { id = "patient-002", firstName = "Maria", lastName = "Garcia", gender = "female", birthDate = "1992-07-22" },
            new { id = "patient-003", firstName = "David", lastName = "Johnson", gender = "male", birthDate = "1978-11-08" }
        };

        foreach (var patient in samplePatients)
        {
            // Create FHIR Patient resource
            var patientResource = new
            {
                resourceType = "Patient",
                id = patient.id,
                meta = new
                {
                    versionId = "1",
                    lastUpdated = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    profile = new[] { "http://hl7.org/fhir/StructureDefinition/Patient" }
                },
                identifier = new[]
                {
                    new { system = "http://hospital.example.com/patients", value = patient.id },
                    new { system = "http://hl7.org/fhir/sid/us-ssn", value = "123-45-6789" }
                },
                active = true,
                name = new[]
                {
                    new
                    {
                        use = "official",
                        family = patient.lastName,
                        given = new[] { patient.firstName }
                    }
                },
                telecom = new[]
                {
                    new { system = "phone", value = "+1-555-0123", use = "home" },
                    new { system = "email", value = $"{patient.firstName.ToLower()}.{patient.lastName.ToLower()}@email.com", use = "home" }
                },
                gender = patient.gender,
                birthDate = patient.birthDate,
                address = new[]
                {
                    new
                    {
                        use = "home",
                        type = "physical",
                        line = new[] { "123 Main Street" },
                        city = "New York",
                        state = "NY",
                        postalCode = "10001",
                        country = "US"
                    }
                },
                maritalStatus = new { coding = new[] { new { system = "http://terminology.hl7.org/CodeSystem/v3-MaritalStatus", code = "M", display = "Married" } } }
            };

            fhirResources.Add(new FhirResource
            {
                ResourceType = "Patient",
                FhirId = patient.id,
                VersionId = 1,
                ResourceJson = JsonSerializer.Serialize(patientResource),
                Status = "active",
                LastUpdated = DateTime.UtcNow,
                FhirCreated = DateTime.UtcNow,
                SearchParameters = JsonSerializer.Serialize(new
                {
                    name = $"{patient.firstName} {patient.lastName}",
                    gender = patient.gender,
                    birthdate = patient.birthDate
                }),
                SecurityLabels = JsonSerializer.Serialize(new[]
                {
                    new { system = "http://terminology.hl7.org/CodeSystem/v3-Confidentiality", code = "R", display = "Restricted" }
                }),
                Tags = JsonSerializer.Serialize(new[]
                {
                    new { system = "http://terminology.hl7.org/CodeSystem/v3-ActCode", code = "AMB", display = "Ambulatory" }
                })
            });

            // Create FHIR Observation resource for blood type
            var bloodTypeObservation = new
            {
                resourceType = "Observation",
                id = $"obs-{patient.id}-bloodtype",
                meta = new
                {
                    versionId = "1",
                    lastUpdated = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    profile = new[] { "http://hl7.org/fhir/StructureDefinition/Observation" }
                },
                status = "final",
                category = new[]
                {
                    new { coding = new[] { new { system = "http://terminology.hl7.org/CodeSystem/observation-category", code = "laboratory", display = "Laboratory" } } }
                },
                code = new { coding = new[] { new { system = "http://loinc.org", code = "883-9", display = "ABO group [Type] in Blood" } } },
                subject = new { reference = $"Patient/{patient.id}" },
                effectiveDateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                issued = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                valueCodeableConcept = new { coding = new[] { new { system = "http://terminology.hl7.org/CodeSystem/v2-0411", code = "O+", display = "Blood Type O+" } } }
            };

            fhirResources.Add(new FhirResource
            {
                ResourceType = "Observation",
                FhirId = $"obs-{patient.id}-bloodtype",
                VersionId = 1,
                ResourceJson = JsonSerializer.Serialize(bloodTypeObservation),
                Status = "final",
                LastUpdated = DateTime.UtcNow,
                FhirCreated = DateTime.UtcNow,
                SearchParameters = JsonSerializer.Serialize(new
                {
                    subject = $"Patient/{patient.id}",
                    category = "laboratory",
                    code = "883-9"
                }),
                SecurityLabels = JsonSerializer.Serialize(new[]
                {
                    new { system = "http://terminology.hl7.org/CodeSystem/v3-Confidentiality", code = "R", display = "Restricted" }
                })
            });

            // Create FHIR MedicationRequest resource
            var medicationRequest = new
            {
                resourceType = "MedicationRequest",
                id = $"med-{patient.id}-001",
                meta = new
                {
                    versionId = "1",
                    lastUpdated = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    profile = new[] { "http://hl7.org/fhir/StructureDefinition/MedicationRequest" }
                },
                status = "active",
                intent = "order",
                medicationCodeableConcept = new
                {
                    coding = new[]
                    {
                        new
                        {
                            system = "http://www.nlm.nih.gov/research/umls/rxnorm",
                            code = "197361",
                            display = "Acetaminophen 500 MG Oral Tablet"
                        }
                    }
                },
                subject = new { reference = $"Patient/{patient.id}" },
                authoredOn = DateTime.UtcNow.AddDays(-7).ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                requester = new { reference = "Practitioner/practitioner-001" },
                dosageInstruction = new[]
                {
                    new
                    {
                        text = "Take 1 tablet every 4-6 hours as needed for pain",
                        timing = new
                        {
                            repeat = new
                            {
                                frequency = 1,
                                period = 6,
                                periodUnit = "h"
                            }
                        },
                        route = new
                        {
                            coding = new[]
                            {
                                new
                                {
                                    system = "http://snomed.info/sct",
                                    code = "26643006",
                                    display = "Oral route"
                                }
                            }
                        },
                        doseAndRate = new[]
                        {
                            new
                            {
                                type = new
                                {
                                    coding = new[]
                                    {
                                        new
                                        {
                                            system = "http://terminology.hl7.org/CodeSystem/dose-rate-type",
                                            code = "ordered",
                                            display = "Ordered"
                                        }
                                    }
                                },
                                doseQuantity = new
                                {
                                    value = 1,
                                    unit = "tablet",
                                    system = "http://unitsofmeasure.org",
                                    code = "TAB"
                                }
                            }
                        }
                    }
                }
            };

            fhirResources.Add(new FhirResource
            {
                ResourceType = "MedicationRequest",
                FhirId = $"med-{patient.id}-001",
                VersionId = 1,
                ResourceJson = JsonSerializer.Serialize(medicationRequest),
                Status = "active",
                LastUpdated = DateTime.UtcNow,
                FhirCreated = DateTime.UtcNow,
                SearchParameters = JsonSerializer.Serialize(new
                {
                    subject = $"Patient/{patient.id}",
                    status = "active",
                    intent = "order"
                }),
                SecurityLabels = JsonSerializer.Serialize(new[]
                {
                    new { system = "http://terminology.hl7.org/CodeSystem/v3-Confidentiality", code = "R", display = "Restricted" }
                })
            });
        }

        _context.FhirResources.AddRange(fhirResources);
        await _context.SaveChangesAsync();
    }


}
