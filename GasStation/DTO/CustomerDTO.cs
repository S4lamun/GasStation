using System.ComponentModel.DataAnnotations;

namespace GasStation.DTO
{
	public class CustomerDTO
	{
		[Required(ErrorMessage = "NIP jest wymagany.")]
		[StringLength(10, MinimumLength = 10, ErrorMessage = "NIP musi składać się z dokładnie 10 cyfr.")]
		[RegularExpression("^[0-9]+$", ErrorMessage = "NIP może zawierać tylko cyfry.")] 		public string Nip { get; set; }

		[Required(ErrorMessage = "Nazwa firmy jest wymagana.")]
		[StringLength(100, ErrorMessage = "Nazwa firmy nie może przekraczać 100 znaków.")]
		public string CompanyName { get; set; }
	}

				}