// Plik: GasStation.DTO/CustomerDTO.cs
using System.ComponentModel.DataAnnotations;

namespace GasStation.DTO
{
	public class CustomerDTO
	{
		[Required(ErrorMessage = "NIP jest wymagany.")]
		[StringLength(10, MinimumLength = 10, ErrorMessage = "NIP musi składać się z 10 cyfr.")]
		// Poprawiony komunikat błędu z "Pesel" na "NIP"
		[RegularExpression(@"^\d{10}$", ErrorMessage = "NIP musi składać się z 10 cyfr.")]
		[Display(Name = "NIP")] // Dodane dla lepszych etykiet w widokach
		public string Nip { get; set; }

		// Możesz dodać [Required], jeśli nazwa firmy jest zawsze wymagana
		[Required(ErrorMessage = "Nazwa firmy jest wymagana.")]
		[StringLength(50, ErrorMessage = "Nazwa firmy nie może przekraczać 50 znaków.")]
		[Display(Name = "Nazwa Firmy")] // Dodane dla lepszych etykiet
		public string CompanyName { get; set; }
	}

	// Klasa CreateCustomerDTO nie jest już potrzebna, jeśli CustomerController
	// w metodzie Create() używa CustomerDTO i tam oczekuje walidacji.
	// Jeśli jednak planujesz różne zestawy pól lub walidacji dla tworzenia
	// i np. edycji, wtedy osobne DTOs mają sens, ale kontroler musi być dostosowany.
}