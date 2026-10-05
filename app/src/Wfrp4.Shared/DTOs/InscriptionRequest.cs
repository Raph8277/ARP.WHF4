using System.ComponentModel.DataAnnotations;

namespace Wfrp4.Shared.DTOs;

public class InscriptionRequest
{
    [Required(ErrorMessage = "Le nom d'utilisateur est obligatoire.")]
    [RegularExpression(@"^[a-zA-Z0-9._\-]+$", ErrorMessage = "Le nom d'utilisateur ne peut contenir que des lettres, chiffres, points, tirets et underscores (pas d'espaces).")]
    public string NomUtilisateur { get; set; } = null!;

    [Required(ErrorMessage = "L'email est obligatoire.")]
    [EmailAddress(ErrorMessage = "L'email n'est pas valide.")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Le mot de passe est obligatoire.")]
    [MinLength(8, ErrorMessage = "Le mot de passe doit contenir au moins 8 caractères.")]
    public string MotDePasse { get; set; } = null!;

    [Compare(nameof(MotDePasse), ErrorMessage = "Les mots de passe ne correspondent pas.")]
    public string ConfirmationMotDePasse { get; set; } = null!;

    public string? Prenom { get; set; }
    public string? Nom { get; set; }
}
