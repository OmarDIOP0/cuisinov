using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace CantineBack.Models.Dtos
{
    public class UserProfilDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Le login est obligatoire")]
        public string Login { get; set; }

        [Required(ErrorMessage = "L'email est obligatoire")]
        [EmailAddress(ErrorMessage = "L'email n'est pas valide")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Le téléphone est obligatoire")]
        [Phone(ErrorMessage = "Le numéro de téléphone n'est pas valide")]
        public string Telephone { get; set; }

    }

}
