using AutoMapper;
using CantineBack.Identity;
using CantineBack.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CantineBack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EntreprisesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;
        private readonly ITokenService _tokenService;

        public EntreprisesController(ApplicationDbContext context, IMapper mapper, IConfiguration config, ITokenService tokenService)
        {
            _context = context;
            _mapper = mapper;
            _config = config;
            _tokenService = tokenService;
        }
        // GET: api/<EntreprisesController>
        [HttpGet]
        [Authorize(Policy = IdentityData.AdminUserPolicyName)]
        public async Task<ActionResult<IEnumerable<Entreprise>>> GetEntreprises()
        {
           if(_context.Entreprises == null)
           {
                return NotFound();
            }
            return await _context.Entreprises
                .Where(e => e.Actif == true)
                .OrderBy(e=> e.Nom)
                .ToListAsync();
        }
        [HttpGet("All")]
        [Authorize(Policy = IdentityData.AdminUserPolicyName)]
        public async Task<ActionResult<IEnumerable<Entreprise>>> GetAllEntreprises()
        {
            if (_context.Entreprises == null)
            {
                return NotFound();
            }
            return await _context.Entreprises.ToListAsync();
        }

        // GET api/<EntreprisesController>/5
        [HttpGet("{id}")]
        [Authorize(Policy = IdentityData.AdminUserPolicyName)]
        public async Task<ActionResult<Entreprise>> GetEntreprise(int id)
        {
           if(_context.Entreprises == null)
           {
                return NotFound();
            }
            var entreprise = await _context.Entreprises.FindAsync(id);
            if (entreprise == null)
            {
                return NotFound();
            }

            return entreprise;
        }

        // POST api/<EntreprisesController>
        [Authorize(Policy = IdentityData.AdminUserPolicyName)]
        [HttpPost]
        public async Task<ActionResult<Entreprise>> Post([FromBody] Entreprise request)
        {
            
            if (_context.Entreprises == null)
            {
                throw new Exception("Entity set 'ApplicationDbContext.Entreprises'  is null.");
            }
            try
            {
                Entreprise entreprise = new Entreprise()
                {
                    Actif = request.Actif,
                    Addresse = request.Addresse,
                    Email = request.Email,
                    Nom = request.Nom,
                    Phone = request.Phone,
                    Code = GenerateCode(request.Nom)
                };
                _context.Entreprises.Add(entreprise);
                await _context.SaveChangesAsync();
                return Ok(entreprise);
            }
            catch (Exception ex)
            {
                return BadRequest("Erreur : " + ex.Message);
            }
            }

        // PUT api/<EntreprisesController>/5
        [HttpPut("{id}")]
        [Authorize(Policy = IdentityData.AdminUserPolicyName)]
        public async Task<IActionResult> PutEntreprise(int id, [FromBody] Entreprise entreprise)
        {
            if (id != entreprise.Id)
            {
                return BadRequest("L'identifiant de l'entreprise ne correspond pas.");
            }

            // Vérifier si l'entreprise existe
            var entrepriseExistante = await _context.Entreprises.FindAsync(id);
            if (entrepriseExistante == null)
            {
                return NotFound("Entreprise introuvable.");
            }

            // Vérifier unicité du Nom (hors lui-même)
            bool nomDejaPris = await _context.Entreprises
                .AnyAsync(e => e.Nom == entreprise.Nom && e.Id != id);

            if (nomDejaPris)
            {
                return BadRequest("Une autre entreprise avec le même nom existe déjà.");
            }

            // Mise à jour des champs
            entrepriseExistante.Nom = entreprise.Nom;
            entrepriseExistante.Email = entreprise.Email;
            entrepriseExistante.Phone = entreprise.Phone;
            entrepriseExistante.Addresse = entreprise.Addresse;
            entrepriseExistante.Actif = entreprise.Actif;

            try
            {
                await _context.SaveChangesAsync();
                return Ok($"Entreprise {entrepriseExistante.Nom.ToUpper()} modifiée avec succès.");
            }
            catch (Exception ex)
            {
                return BadRequest("Erreur : " + ex.Message);
            }
        }


        // DELETE api/<EntreprisesController>/5
        [HttpDelete("{id}")]
        [Authorize(Policy = IdentityData.AdminUserPolicyName)]
        public async Task<IActionResult> DeleteEntreprise(int id)
        {
            if(_context.Entreprises == null)
            {
                throw new Exception("Entity set 'ApplicationDbContext.Users'  is null.");
            }
            var entreprise = await _context.Entreprises.FindAsync(id);
            if (entreprise == null)
            {
                return NotFound();
            }
            entreprise.Actif = false;
            _context.Entry(entreprise).Property(u => u.Actif).IsModified = true;
            await _context.SaveChangesAsync();
            return Ok("Entreprise " + entreprise.Nom.ToUpper() + " supprimée avec success");
        }

        private string GenerateCode(string nomEntreprise)
        {
            // Exemple : CUISINOV_A1B2C3
            string prefix = nomEntreprise.ToUpper().Replace(" ", "");
            string uniquePart = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
            return $"{prefix}_{uniquePart}";
        }

    }
}
