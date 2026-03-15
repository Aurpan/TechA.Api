using System.ComponentModel.DataAnnotations;

namespace TechA.Core.Requests.Profile;

public class UpdateProfileRequest
{
    [MaxLength(50)]
    public string? FirstName { get; set; }

    [MaxLength(50)]
    public string? LastName { get; set; }

    [MaxLength(500)]
    public string? ProfilePictureUrl { get; set; }

    [MaxLength(20)]
    public string? ContactNumber { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    [MaxLength(500)]
    public string? Bio { get; set; }

    [MaxLength(250)]
    public string? Address { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(100)]
    public string? State { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }

    [MaxLength(20)]
    public string? ZipCode { get; set; }
}
