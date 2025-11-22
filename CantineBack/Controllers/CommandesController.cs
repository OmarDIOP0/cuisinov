using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CantineBack;
using CantineBack.Models;
using CantineBack.Models.Dtos;
using CantineBack.Models.Enums;
using CantineBack.Helpers;
using AutoMapper;
using NuGet.Versioning;
using CantineBack.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using DpworldDkr.Helpers;

namespace CantineBack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class CommandesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        ILogger<CommandesController> _logger;
        public CommandesController(ApplicationDbContext context, IMapper mapper, ILogger<CommandesController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger =logger;
        }
        [HttpPost("GetSoldAmountByPaymentMethod")]
        [Authorize(Roles = IdentityData.AdminOrGerantUserRoles)]
        public  ActionResult<SoldAmountReport> GetSoldAmountByPaymentMethod(SoldAmountRequest soldAmountRequest)
        {
            // return (await _context.Commandes.Where(c => c.IsDelivered == false && c.IsRejected == false).CountAsync()).ToString();

            List<SoldAmountResponseItem> soldAmountResponseItems = new List<SoldAmountResponseItem>();
            if (soldAmountRequest.PaymentMethods != null)
            {
                foreach (var item in soldAmountRequest.PaymentMethods)
                {
                    int amount = 0;
                    try
                    {
                        if (soldAmountRequest.UseDateTimeFilter)
                        {
                            amount = _context.Commandes.Where(c => c.IsDelivered == true && c.IsRejected != true && c.IsDeleted != true && c.Date >= soldAmountRequest.FromDate && c.Date <= soldAmountRequest.ToDate).Include(c => c.LigneCommandesNavigation).Include(c => c.PaymentMethodNavigation).Where(c => c.PaymentMethodNavigation.Code.ToUpper() == item.ToString().ToUpper()).Sum(c => c.Montant);

                        }
                        else
                        {
                            amount = _context.Commandes.Where(c => c.IsDelivered == true && c.IsRejected != true && c.IsDeleted != true && c.Date.Value.Date >= soldAmountRequest.FromDate.Date && c.Date.Value.Date <= soldAmountRequest.ToDate.Date).Include(c => c.LigneCommandesNavigation).Include(c => c.PaymentMethodNavigation).Where(c => c.PaymentMethodNavigation.Code.ToUpper() == item.ToString().ToUpper()).Sum(c => c.Montant);

                        }
                        var label = item.ToString();
                        var code = item.ToString();
                        switch (item)
                        {
                            case PaymentMethodsEnum.CASH:
                                label = item.ToString();
                                break;

                            case PaymentMethodsEnum.WAVE:
                                label = item.ToString();
                                break;
                            case PaymentMethodsEnum.OM:
                                label = "ORANGE MONEY";
                                break;
                            default:
                                break;
                        }
                  
                        soldAmountResponseItems.Add(new SoldAmountResponseItem { Code=code,Label= label, Amount=amount});
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message);
                    }
                }

            }
            return Ok(new SoldAmountReport {StartDate=soldAmountRequest.FromDate,EndDate=soldAmountRequest.ToDate  ,Items= soldAmountResponseItems ,UseDateTimeFilter=soldAmountRequest.UseDateTimeFilter});
        }

        [HttpGet("GetPendingCommandesCount")]
        [Authorize(Roles = IdentityData.AdminOrGerantUserRoles)]
        public async Task<ActionResult<string>> GetPendingCommandesCount()
        {
            return (await _context.Commandes.Where(c => c.IsDelivered == false && c.IsRejected == false).CountAsync()).ToString();
        }
        // GET: api/Commandes
        //[HttpGet]
        //[Authorize(Roles = IdentityData.AdminOrGerantUserRoles)]
        //public async Task<ActionResult<IEnumerable<CommandeReadDto>>> GetCommandes(CommandStateEnum? state, DateTime? startDate, DateTime? endDate)
        //{
        //    if (_context.Commandes == null)
        //    {
        //        return NotFound();
        //    }
        //    if (startDate.HasValue)
        //    {
        //        if(endDate==null ) endDate=DateTime.Now;

        //        var r = await _context.Commandes.Where(c => c.Date >= startDate && c.Date <= endDate).Include(c => c.LigneCommandesNavigation).ThenInclude(x => x.ArticleNavigation).Include(c => c.UserNavigation).Include(c => c.EmplacementNavigation).Include(c => c.PaymentMethodNavigation).OrderByDescending(c => c.Date).ToListAsync();
        //        return Ok(_mapper.Map<IEnumerable<CommandeReadDto>>(r));
        //    }
        //    if (state == null)
        //    {
        //        startDate = DateTime.Now.AddDays(-1);
        //        endDate = DateTime.Now.AddDays(1);
        //        var r = await _context.Commandes.Where(c => c.Date >= startDate && c.Date <= endDate).Include(c => c.LigneCommandesNavigation).ThenInclude(x => x.ArticleNavigation).Include(c => c.UserNavigation).Include(c => c.EmplacementNavigation).Include(c => c.PaymentMethodNavigation).OrderByDescending(c => c.Date).ToListAsync();
        //        return Ok(_mapper.Map<IEnumerable<CommandeReadDto>>(r));
        //    }
        //    else
        //    {
        //        if (state == CommandStateEnum.Delivered)
        //        {
        //            var r = await _context.Commandes.Where(c => c.IsDelivered == true && c.IsRejected == false).Include(c => c.LigneCommandesNavigation).ThenInclude(x => x.ArticleNavigation)
        //                .Include(c => c.UserNavigation).Include(c => c.EmplacementNavigation).Include(c => c.PaymentMethodNavigation).OrderByDescending(c => c.Date).ToListAsync();
        //            return Ok(_mapper.Map<IEnumerable<CommandeReadDto>>(r));
        //        }
        //        else
        //        {
        //            try
        //            {
        //                var r = await _context.Commandes.Where(c => c.IsDelivered == false && c.IsRejected == false).Include(c => c.LigneCommandesNavigation).ThenInclude(x => x.ArticleNavigation).Include(c => c.UserNavigation).Include(c => c.EmplacementNavigation).Include(c => c.PaymentMethodNavigation).OrderBy(c => c.Date).ToListAsync();
        //                var l = _mapper.Map<IEnumerable<CommandeReadDto>>(r);
        //                return Ok(l);
        //            }
        //            catch (Exception ex)
        //            {
        //                return null;
        //            }
        //        }
        //    }
        //}
        [HttpGet]
        [Authorize(Roles = IdentityData.AdminOrGerantUserRoles)]
        public async Task<ActionResult<IEnumerable<CommandeReadDto>>> GetCommandes(
                             CommandStateEnum? state,
                             DateTime? startDate,
                             DateTime? endDate,
                             int? entrepriseId = null,
                             int? emplacementId = null)
        {
            if (_context.Commandes == null)
                return NotFound();

            if (startDate.HasValue && !endDate.HasValue)
                endDate = DateTime.Now;

            if (!startDate.HasValue && state == null)
            {
                startDate = DateTime.Now.AddDays(-1);
                endDate = DateTime.Now.AddDays(1);
            }
         
            var query = _context.Commandes
                .Include(c => c.LigneCommandesNavigation)
                    .ThenInclude(l => l.ArticleNavigation)
                .Include(c => c.UserNavigation)
                .Include(c => c.EmplacementNavigation)
                    .ThenInclude(e => e.Entreprise)
                .Include(c => c.PaymentMethodNavigation)
                .AsQueryable();

            if (entrepriseId.HasValue)
            {
                query = query.Where(c => c.EmplacementNavigation != null &&
                                         c.EmplacementNavigation.Entreprise != null &&
                                         c.EmplacementNavigation.Entreprise.Id == entrepriseId.Value);
            }

            if (emplacementId.HasValue)
            {
                query = query.Where(c => c.EmplacementNavigation != null &&
                                         c.EmplacementNavigation.Id == emplacementId.Value);
            }


            if (startDate.HasValue && endDate.HasValue)
                query = query.Where(c => c.Date >= startDate && c.Date <= endDate);

            if (state.HasValue)
            {
                switch (state.Value)
                {
                    case CommandStateEnum.Delivered:
                        query = query.Where(c => c.IsDelivered && !c.IsRejected);
                        break;
                    case CommandStateEnum.Pending:
                        query = query.Where(c => !c.IsDelivered && !c.IsRejected);
                        break;
                    case CommandStateEnum.Rejected:
                        query = query.Where(c => c.IsRejected);
                        break;
                }
            }

            var commandes = await query.OrderByDescending(c => c.Date).ToListAsync();
            return Ok(_mapper.Map<IEnumerable<CommandeReadDto>>(commandes));
        }


        // GET: api/Commandes
        [HttpGet("GetMyCommands/{id}")]
        [Authorize(Roles = IdentityData.AdminOrUserRoles)]
        public async Task<ActionResult<IEnumerable<CommandeReadDto>>> GetMyCommands(int id, DateTime? startDate, DateTime? endDate)
        {
            if (_context.Commandes == null)
            {
                return NotFound();
            }

            if (id > 0)
            {
                if (startDate.HasValue && endDate.HasValue)
                {

                    var r = await _context.Commandes.Where(c => c.UserId == id && c.IsDeleted != true && c.Date >= startDate && c.Date <= endDate).Include(c => c.LigneCommandesNavigation).ThenInclude(x => x.ArticleNavigation).Include(c => c.UserNavigation).Include(c => c.EmplacementNavigation).Include(c => c.PaymentMethodNavigation).OrderByDescending(c => c.Date).ToListAsync();
                    return Ok(_mapper.Map<IEnumerable<CommandeReadDto>>(r));
                }
                else
                {
                    startDate = DateTime.Now.AddDays(-1);
                    endDate = DateTime.Now.AddDays(1);
                    var r = await _context.Commandes.Where(c => c.UserId == id && c.IsDeleted != true && c.Date >= startDate && c.Date <= endDate).Include(c => c.LigneCommandesNavigation).ThenInclude(x => x.ArticleNavigation).Include(c => c.UserNavigation).Include(c => c.EmplacementNavigation).Include(c => c.PaymentMethodNavigation).OrderByDescending(c => c.Date).ToListAsync();
                    return Ok(_mapper.Map<IEnumerable<CommandeReadDto>>(r));
                }
            }
            else
            {
                return NotFound();
            }



        }

        // GET: api/Commandes/5
        [HttpGet("{id}", Name = "GetCommande")]
        [Authorize]
        public async Task<ActionResult<CommandeReadDto>> GetCommande(int id)
        {
            if (_context.Commandes == null)
            {
                return NotFound();
            }
            var commande = await _context.Commandes!.Include(c => c.EmplacementNavigation).
                Include(c => c.LigneCommandesNavigation).ThenInclude(x => x.ArticleNavigation).
                Include(c => c.UserNavigation).Include(c => c.EmplacementNavigation).Where(c => c.Id == id).FirstOrDefaultAsync();

            if (commande == null)
            {
                return NotFound();
            }
            var c = _mapper.Map<CommandeReadDto>(commande);
            return c;
        }

        // PUT: api/Commandes/ChangeState/5
        [HttpPut("ChangeState/{id}")]
        [Authorize(Roles = IdentityData.AdminOrGerantUserRoles)]
        public async Task<ActionResult<Commande>> ChangeState(int id)
        {
            if (_context.Commandes == null)
            {
                return NotFound();
            }

            var commande = await _context.Commandes.FindAsync(id);

            if (commande == null)
            {
                return NotFound();
            }


            if (commande.UserId != null)
            {

                var user = await _context.Users.FindAsync(commande.UserId);
                string userPhoneNumber = user?.Telephone;
                string userEmail = user?.Email;
                string userFullName = user?.Login;


                //if (!String.IsNullOrWhiteSpace(userPhoneNumber))
                //{
                //    try
                //    {
                //        SmsManager.SendSMS(userPhoneNumber, $"Bonjour {userFullName} votre commande du  {commande.Date?.ToString("dd-MM-yyyy HH:mm")} a ete livree Montant Total : {commande.Montant}. Votre nouveau solde est : {Functions.FormatMonetaire(user.Solde).Replace("\t", " ")} F CFA");
                //    }
                //    catch (Exception ex)
                //    {

                //    }

                //}

                //if (!String.IsNullOrWhiteSpace(userEmail))
                //{
                //    try
                //    {
                //        var listAriclesStr = "";

                //        var lignesCommands = await _context.LigneCommandes.Where(lc=>lc.CommandeId==commande.Id).AsNoTracking().Include(lc=>lc.ArticleNavigation).ToListAsync();
                //        if (lignesCommands?.Any()??false)
                //        {
                //            listAriclesStr = $"\n\nArticle                             Quantité    Prix        Prix*Quantite     \n";
                //            listAriclesStr += "-------------------------------------------------------------------------------\n";
                //            foreach (var ligneCommand in lignesCommands)
                //            {
                //                int totalLength= ligneCommand.ArticleNavigation?.Title?.Length??0;
                //                int len = Math.Min(totalLength, 30);
                //                string concat = totalLength > 30 ? "..." : "";
                //                //int prixPad=
                //                string articleStr = (ligneCommand.ArticleNavigation.Title.Substring(0, len - concat.Length) + concat).ToLower().PadRight(30);
                //                listAriclesStr += $"{articleStr}{ligneCommand.Quantite.ToString().PadRight(12)}{ligneCommand.ArticleNavigation.PrixDeVente.ToString().PadRight(12)} {ligneCommand.PrixTotal} F CFA \n";
           
                //            }
                //            listAriclesStr += $"{" ".PadRight(43)} Montant Total = {commande.Montant} F CFA \n";
                //        }
               
                //        string nouveauSolde = $"\n Votre nouveau solde est : {Functions.FormatMonetaire( user.Solde)} F CFA";

                //        EmailManager.SendEmail(userEmail, "COMMANDE LIVREÉ", $"Bonjour {userFullName} votre commande du  {commande.Date?.ToString("dd-MM-yyyy HH:mm")} a été livrée Montant Total : {commande.Montant} {listAriclesStr} {nouveauSolde}", null, null);
                //    }
                //    catch (Exception ex)
                //    {

                //    }

                //}

            }


            commande.IsDelivered = true;

            try
            {
                _context.Commandes.Update(commande);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CommandeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }


        // PUT: api/Commandes/ChangeState/5
        [HttpPost("Reject/{id}")]
        [Authorize(Roles = IdentityData.AdminOrGerantUserRoles)]
        public async Task<ActionResult<Commande>> Reject(int id, RejectCommandRequest rejectCommand)
        {


            if (id != rejectCommand.Id)
            {
                return BadRequest();
            }
            string userPhoneNumber = String.Empty;
            string userEmail = String.Empty;
            string userFullName = String.Empty;
            string profilUserAction = User.FindFirstValue(ClaimTypes.Role);
            //  int userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId);
            if (_context.Commandes == null)
            {
                return NotFound();
            }

            // var commande = await _context.Commandes.FindAsync(id);
            var commande = await _context.Commandes.Where(c => c.Id == id).Include(c => c.LigneCommandesNavigation).FirstOrDefaultAsync();
            if (commande == null)
            {
                return Problem("Commande introuvable");
                // return NotFound();
            }
            if (commande.IsDelivered)
            {
                return Problem("Cette commande a été déjà livrée");
            }
            else
            {
                var lignes = commande.LigneCommandesNavigation?.ToList();
                if (lignes.Any())
                {
                    foreach (var ligne in lignes)
                    {
                        var a = await _context.Articles.FindAsync(ligne.ArticleId);
                        if (a != null)
                        {
                            a.QuantiteStock += Math.Abs(ligne.Quantite);
                        }
                    }
                    if (profilUserAction != "admin" && profilUserAction != "gerant")
                    {
                        return Forbid();
                    }
                    var user = await _context.Users.FindAsync(commande.UserId);
                    userPhoneNumber = user?.Telephone;
                    userEmail = user?.Email;
                    userFullName = user?.Prenom + " " + user.Nom;
                    var paymentMethod = await _context.PaymentMethods.FindAsync(commande.PaymentMethodId);
                    if (user != null && paymentMethod?.Code == "QRCODE")
                    {
                        user.Solde += commande.Montant;

                    }

                }
            }
            commande.IsRejected = true;
            commande.RejectComment = rejectCommand.Reason;

            commande.IsDelivered = true;

            try
            {
                //_context.Commandes.Update(commande);

                _context.Commandes.Entry(commande).Property(c => c.IsRejected).IsModified = true;
                _context.Commandes.Entry(commande).Property(c => c.RejectComment).IsModified = true;
                await _context.SaveChangesAsync();
                if (!String.IsNullOrWhiteSpace(userPhoneNumber))
                {
                    try
                    {
                        //SmsManager.SendSMS(userPhoneNumber, $"Bonjour {userFullName} votre commande du: {commande.Date?.ToString("dd-MM-yyyy HH:mm")} a ete annulee par le prestataire Motif : {rejectCommand.Reason}");
                    }
                    catch (Exception ex)
                    {

                    }

                }

                if (!String.IsNullOrWhiteSpace(userEmail))
                {
                    try
                    {
                        EmailManager.SendEmail(userEmail, "COMMANDE ANNULÉE", $"Bonjour {userFullName} votre commande du: {commande.Date?.ToString("dd-MM-yyyy HH:mm")} a été annulée par le prestataire \nMotif : {rejectCommand.Reason}", null, null);
                    }
                    catch (Exception ex)
                    {

                    }

                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CommandeExists(id))
                {
                    return Problem("Commande introuvable");
                    //  return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }
        [HttpPost("CommandFeedBack/{id}")]
        [Authorize(Roles = IdentityData.AdminOrUserRoles)]
        public async Task<ActionResult<Commande>> CommandFeedBack(int id, CommandRatingRequest feedBack)
        {
            if (id != feedBack.Id)
            {
                return BadRequest();
            }
            string userPhoneNumber = String.Empty;
            string profilUserAction = User.FindFirstValue(ClaimTypes.Role);
            //  int userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId);
            if (_context.Commandes == null)
            {
                return NotFound();
            }

            var commande = await _context.Commandes.FindAsync(id);

            if (commande == null)
            {
                return Problem("Commande introuvable");
                // return NotFound();
            }
            if (commande.IsRating)
            {
                return Problem("Vous avez dèja donné votre impression sur cette commande!");
            }

            commande.IsRating = true;
            commande.CustomerFeedback = feedBack.CustomerFeedback;

            commande.Rate = feedBack.Rate;

            try
            {
                //_context.Commandes.Update(commande);

                _context.Commandes.Entry(commande).Property(c => c.IsRating).IsModified = true;
                _context.Commandes.Entry(commande).Property(c => c.CustomerFeedback).IsModified = true;
                _context.Commandes.Entry(commande).Property(c => c.Rate).IsModified = true;
                await _context.SaveChangesAsync();


            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CommandeExists(id))
                {
                    return Problem("Commande introuvable");
                    //  return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }
        // POST: api/Commandes
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<Commande>> PostCommande(PostCommandDto commandDto)
        {
            if (_context.Commandes == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Commandes' is null.");
            }

            if (commandDto == null)
            {
                return Problem("Data is null");
            }

            if (commandDto.LigneCommands == null || commandDto.LigneCommands.Any() == false)
            {
                return Problem("No data was found inside the command object");
            }

            int amountCommand = 0;

            // Validation des articles et calcul du montant
            foreach (var ligneCommand in commandDto.LigneCommands)
            {
                var article = _context.Articles.FirstOrDefault(a => a.Id == ligneCommand.ArticleId);
                if (article == null)
                {
                    return Problem("Un des articles commandé est introuvable");
                }

                if ((article.QuantiteStock < ligneCommand.Quantite) && article.ControlStockQuantity)
                {
                    return Problem($"La quantité en stock de l'article {article.Title} est insuffisant, quantité disponible = {article.QuantiteStock}");
                }
                else
                {
                    article.QuantiteStock -= ligneCommand.Quantite;
                    article.QuantiteStock = article.QuantiteStock < 0 ? 0 : article.QuantiteStock;
                }

                amountCommand += article.PrixDeVente * ligneCommand.Quantite;
            }

            PaymentMethod? paymentMethod = await _context.PaymentMethods.FindAsync(commandDto.PaymentMethodId);
            if (paymentMethod == null)
            {
                return Problem("Payment Method for this command is unknown");
            }

            string profilUserAction = User.FindFirstValue(ClaimTypes.Role);
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId);
            var userAction = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == userId);
            Emplacement? emplacement = null;
            var emplacementId = 0;

            if (!commandDto.CommandeADistance)
            {
                emplacement = await _context.Emplacement
                    .FirstOrDefaultAsync(e =>
                        e.EntrepriseId == userAction.EntrepriseId &&
                        EF.Functions.Like(e.Name.ToUpper(), "%SURPLACE%"));

                if (emplacement == null)
                {
                    emplacement = await _context.Emplacement
                        .FirstOrDefaultAsync(e => e.Id == Common.ShopID);
                }
                emplacementId = emplacement.Id;
            }
            else
            {
                emplacementId = commandDto.EmplacementId > 0 ? commandDto.EmplacementId : Common.ShopID;
            }

                //var emplacementId = commandDto.CommandeADistance ? Common.ShopID : emplacement.Id;

            Commande newCommande = new Commande
            {
                Date = DateTime.Now,
                IsDelivered = false,
                CommandeADistance = commandDto.CommandeADistance,
                EmplacementId = emplacementId,
                UserId = commandDto.UserId,
                PaymentMethodId = commandDto.PaymentMethodId,
                Montant = amountCommand,
                Comment = commandDto.Comment
            };

            _context.Commandes.Add(newCommande);
            try
            {
                if (await _context.SaveChangesAsync() > 0)
                {
                    foreach (var ligneCommand in commandDto.LigneCommands)
                    {
                        LigneCommande ligneCommande = new()
                        {
                            ArticleId = ligneCommand.ArticleId,
                            CommandeId = newCommande.Id,
                            PrixTotal = ligneCommand.PrixTotal,
                            Quantite = ligneCommand.Quantite
                        };

                        _context.LigneCommandes.Add(ligneCommande);
                    }

                    try
                    {
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        return Problem(detail: $"Erreur interne : {ex.Message}", statusCode: StatusCodes.Status500InternalServerError);
                    }

                }
            }
            catch (Exception ex)
            {
                return Problem($"Erreur lors de la sauvegarde initiale : {ex.Message}");
            }



            return Ok((await GetCommande(newCommande.Id))?.Value);
        }

        // DELETE: api/Commandes/5
        [HttpDelete("{id}")]
        [Authorize(Roles = IdentityData.AdminOrUserRoles)]
        public async Task<IActionResult> DeleteCommande(int id)
        {

            string profilUserAction = User.FindFirstValue(ClaimTypes.Role);
            //  int userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId);
            if (_context.Commandes == null)
            {
                return NotFound();
            }
            var commande = await _context.Commandes.Where(c => c.Id == id).Include(c => c.LigneCommandesNavigation).FirstOrDefaultAsync();
            if (commande == null)
            {
                return NotFound();
            }
            if (commande.IsDelivered)
            {
                return BadRequest();
            }
            else
            {
                var lignes = commande.LigneCommandesNavigation?.ToList();
                if (lignes.Any())
                {
                    foreach (var ligne in lignes)
                    {
                        var a = await _context.Articles.FindAsync(ligne.ArticleId);
                        if (a != null)
                        {
                            a.QuantiteStock += Math.Abs(ligne.Quantite);
                        }
                    }
                    if (profilUserAction != "admin" && userId != commande.UserId)
                    {
                        return Forbid();
                    }
                    var user = await _context.Users.FindAsync(commande.UserId);

                    var paymentMethod = await _context.PaymentMethods.FindAsync(commande.PaymentMethodId);
                    if (user != null && paymentMethod?.Code == "QRCODE")
                    {
                        user.Solde += commande.Montant;

                    }

                    //var user = await _context.Users.FindAsync(commande.UserId);
                    //if (user != null)
                    //{
                    //    user.Solde += commande.Montant;
                    //}

                }
            }
            if (commande.LigneCommandesNavigation != null)
            {

                _context.LigneCommandes.RemoveRange(commande.LigneCommandesNavigation!);
            }

            _context.Commandes.Remove(commande);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CommandeExists(int id)
        {
            return (_context.Commandes?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
