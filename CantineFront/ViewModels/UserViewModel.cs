using CantineBack.Models;

namespace CantineFront.ViewModels
{
    public class UserViewModel
    {
        public User User { get; set; }
        public IEnumerable<Department>? Departments { get; set; }
        public IEnumerable<Entreprise>? Entreprises { get; set; }
    }
}
