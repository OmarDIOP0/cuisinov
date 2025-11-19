using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CantineBack;
using CantineBack.Models;
using AutoMapper;
using CantineBack.Identity;
using Microsoft.AspNetCore.Authorization;
using CantineBack.Models.Dtos;
using System.Security.Claims;
using CantineBack.Models.Enums;
using CantineBack.Helpers;

namespace CantineBack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class ArticlesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        public ArticlesController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        // GET: api/Articles
        [HttpGet]
        [Authorize(Roles = IdentityData.AdminOrGerantUserRoles)]
        public async Task<ActionResult<IEnumerable<Article>>> GetArticles(int? categorieId)
        {
            if (_context.Articles == null)
            {
                return NotFound();
            }
            if (categorieId == null)
            {
                return await _context.Articles.Include(a => a.CategorieNavigation).ToListAsync();
            }
            else
            {
                return await _context.Articles.Where(a => a.CategorieId == categorieId).Include(a => a.CategorieNavigation).ToListAsync();
            }

        }

        [HttpGet("GetArticleImages")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ArticleImageDto>>> GetArticleImages()
        {
            if (_context.Articles == null)
            {
                return NotFound();
            }
     
            
                return await _mapper.ProjectTo<ArticleImageDto>(_context.Articles).ToListAsync();
        }
        [HttpGet("GetMenu")]
        //[AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ArticleReadDto>>> GetMenu(int? categorieId)
        {
            if (_context.Articles == null)
                return NotFound();

            var userLogin = User.Identity.Name; 
            var currentUser = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Login == userLogin);

            if (currentUser == null)
                return Unauthorized();

            // Commencer la requête
            IQueryable<Article> query = _context.Articles
                .Include(a => a.CategorieNavigation)
                .Where(a => a.IsArticleOnMenu && a.IsApproved && a.QuantiteStock > 0);

            if (currentUser.Profile == "USER" || currentUser.Profile == "GERANT")
            {
                if (currentUser.EntrepriseId.HasValue)
                {
                    query = query.Where(a => a.EntrepriseId == currentUser.EntrepriseId.Value);
                }
                else
                {
                    return Ok(new List<ArticleReadDto>());
                }
            }

            if (categorieId.HasValue)
            {
                query = query.Where(a => a.CategorieId == categorieId.Value);
            }

            // ProjectTo avec AutoMapper
            var articlesDto = await _mapper.ProjectTo<ArticleReadDto>(query).ToListAsync();

            return Ok(articlesDto);
        }



        // GET: api/Articles/5
        [HttpGet("{id}")]
        [Authorize(Roles = IdentityData.AdminOrGerantUserRoles)]
        public async Task<ActionResult<Article>> GetArticle(int id)
        {




            if (_context.Articles == null)
            {
                return NotFound();
            }
            var article = await _context.Articles.FindAsync(id);

            if (article == null)
            {
                return NotFound();
            }

            return article;
        }

        // PUT: api/Articles/5
        [Authorize(Roles = IdentityData.AdminOrGerantUserRoles)]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutArticle(int id, Article article)
        {
            string userRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId);

            if (id != article.Id)
                return BadRequest("L'identifiant de l'article ne correspond pas.");

            var oldArticle = await _context.Articles
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id);

            if (oldArticle == null)
                return NotFound("Article introuvable.");

            // 🔹 Validation des données
            if (article.PrixDeVente <= 0)
                return Problem("Prix de vente invalide.");

            if (article.QuantiteStock < 0)
                return Problem("Quantité en stock invalide.");

            if (article.PrixDachat.HasValue && article.PrixDachat < 0)
                return Problem("Prix d'achat invalide.");

            // 🔹 Préparation de la mise à jour
            _context.Entry(article).State = EntityState.Modified;

            // 🔹 Gestion de l’approbation si le prix change
            if (oldArticle.PrixDeVente != article.PrixDeVente)
            {
                if (!userRole.Equals("admin", StringComparison.OrdinalIgnoreCase))
                {
                    article.IsApproved = false;

                    var userAction = await _context.Users
                        .AsNoTracking()
                        .FirstOrDefaultAsync(u => u.Id == userId);
                }
            }
            else
            {
                // 🔹 Empêche un non-admin de forcer IsApproved
                if (!userRole.Equals("admin", StringComparison.OrdinalIgnoreCase))
                {
                    _context.Entry(article).Property(a => a.IsApproved).IsModified = false;
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ArticleExists(id))
                    return NotFound("Article introuvable.");
                else
                    throw;
            }

            return NoContent();
        }

        [Route("UpdateStatus/{id}")]
        [Authorize(Roles = IdentityData.AdminOrGerantUserRoles)]
        [HttpPut]
        public async Task<ActionResult<Article?>> UpdateStatus(int id)
        {
            if (_context.Articles == null)
            {
                return BadRequest();
            }

            Article? article = await _context.Articles.FindAsync(id);
            if (article == null)
            {
                return NotFound();
            }

            article.IsArticleOnMenu = !article.IsArticleOnMenu;
            _context.Entry(article).Property(a => a.IsArticleOnMenu).IsModified = true;
            //_context.Articles.Update(article);
            if (await _context.SaveChangesAsync() > 0)
            {
                return Ok(article);
            }
            else
            {
                return BadRequest();
            }
        }

        [Route("Approve/{id}")]
        [Authorize(Policy = IdentityData.AdminUserPolicyName)]
        [HttpPut]
        public async Task<ActionResult<Article?>> Approve(int id)
        {
            if (_context.Articles == null)
            {
                return BadRequest();
            }

            Article? article = await _context.Articles.FindAsync(id);
            if (article == null)
            {
                return NotFound();
            }

            article.IsApproved = !article.IsApproved;
            _context.Entry(article).Property(a => a.IsApproved).IsModified = true;
            // _context.Articles.Update(article);
            if (await _context.SaveChangesAsync() > 0)
            {
                return Ok(article);
            }
            else
            {
                return BadRequest();
            }
        }
        // POST: api/Articles
        [HttpPost]
        [Authorize(Roles = IdentityData.AdminOrGerantUserRoles)]
        public async Task<ActionResult<Article>> PostArticle(Article article)
        {
            string profilUserAction = User.FindFirstValue(ClaimTypes.Role);
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId);

            article.Actif = true;
            article.IsArticleOnMenu = true;

            if (_context.Articles == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Articles'  is null.");
            }
            if (article.PrixDeVente <= 0)
            {
                return Problem("Prix de vente invalide.");
            }
            if (article.QuantiteStock < 0)
            {
                return Problem("Quantité en stock invalide.");
            }
            if (article.PrixDachat.HasValue)
            {
                if (article.PrixDachat < 0)
                {
                    return Problem("Prix d'achat invalide.");
                }
            }
            if (profilUserAction != "admin")
            {
                article.IsApproved = false;
            }
            if(profilUserAction == "gerant")
            {
                var userAction = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == userId);
                if (userAction != null && userAction.EntrepriseId.HasValue)
                {
                    article.EntrepriseId = userAction.EntrepriseId.Value;
                }
            }

            _context.Articles.Add(article);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetArticle", new { id = article.Id }, article);
        }

        // DELETE: api/Articles/5
        [HttpDelete("{id}")]
        [Authorize(Roles = IdentityData.AdminOrGerantUserRoles)]
        public async Task<IActionResult> DeleteArticle(int id)
        {
            if (_context.Articles == null)
            {
                return NotFound();
            }
            var article = await _context.Articles.FindAsync(id);
            if (article == null)
            {
                return NotFound();
            }

            //_context.Articles.Remove(article);
            article.Actif = false;
            article.IsApproved = false;
            article.IsArticleOnMenu = false;
            _context.Entry(article).Property(u => u.Actif).IsModified = true;
            _context.Entry(article).Property(u => u.IsApproved).IsModified = true;
            _context.Entry(article).Property(u => u.IsArticleOnMenu).IsModified = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }


        [Authorize(Roles = IdentityData.AdminOrGerantUserRoles)]
        [HttpPut("Mouvement/{id}")]

        public async Task<IActionResult> MouvementEntreeSortie(int id, int quantite, MouvementEnum mouvement)
        {
            // get user action info from API Token 
            // and log action in Log table
            var article = await _context.Articles.FindAsync(id);
            if (article == null)
            {
                return NotFound();
            }
            quantite = Math.Abs(quantite);

            if (mouvement == MouvementEnum.ENTREE)
            {
                article.QuantiteStock += quantite;

            }
            else
            {

                if (quantite > article.QuantiteStock) { quantite = article.QuantiteStock; }
                article.QuantiteStock -= quantite;
                quantite = -quantite;
            }

            _context.Entry(article).Property(u => u.QuantiteStock).IsModified = true;

            string fullnameUserAction = User.FindFirstValue(ClaimTypes.GivenName);
            string usernameUserAction = User.FindFirstValue(ClaimTypes.Name);
            string profilUserAction = User.FindFirstValue(ClaimTypes.Role);
            if (_context.Log != null)
            {
                _context.Log.Add(new Log
                {
                    ActionProfil = profilUserAction,
                    ActionType = mouvement.ToString().ToUpper(),
                    ActionUser = usernameUserAction,
                    Entity = "Article",
                    Date = DateTime.Now,
                    Comment = $"{mouvement.ToString()}  de l'article  <<{article.Title}>> , quantité mouvement ={quantite} , Nouvelle quantité en stock= {article.QuantiteStock}"
                });

            }
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ArticleExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok(_mapper.Map<ArticleCommandReadDto>(article));
        }



        private bool ArticleExists(int id)
        {
            return (_context.Articles?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
