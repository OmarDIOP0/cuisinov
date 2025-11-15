using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CantineBack;
using CantineBack.Models;
using NuGet.Protocol.Plugins;
using System.Reflection.PortableExecutable;
using AutoMapper;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using CantineBack.Models.Dtos;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using CantineBack.Identity;
using System.Reflection.Metadata;
using CantineBack.Helpers;
using Microsoft.AspNetCore.Identity;
using NuGet.Protocol;
using QRCoder;
using static System.Runtime.CompilerServices.RuntimeHelpers;
using System.Drawing.Printing;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using static QRCoder.PayloadGenerator.SwissQrCode;
using NuGet.Versioning;
using Coravel.Queuing.Interfaces;

namespace CantineBack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;
        private readonly ITokenService _tokenService;

        public UsersController(ApplicationDbContext context, IMapper mapper, IConfiguration config, ITokenService tokenService)
        {
            _context = context;
            _mapper = mapper;
            _config = config;
            _tokenService = tokenService;
        }

        // POST: api/Users
        [Authorize(Policy = IdentityData.AdminUserPolicyName)]
        [HttpPost]
        public async Task<ActionResult> PostUser([FromBody] User user)
        {
            if (_context.Users == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Users' est null.");
            }

            user.Profile = string.IsNullOrWhiteSpace(user.Profile) ? "USER" : user.Profile.ToUpper().Trim();

            // Vérifier unicité du login
            var existing = await _context.Users.FirstOrDefaultAsync(u => u.Login == user.Login);
            if (existing != null)
            {
                return Problem("Ce nom d'utilisateur existe déjà.");
            }

            // Mettre les valeurs par défaut
            user.Actif = true;
            user.Guid = Guid.NewGuid().ToString();

            // Si l'admin n'a pas fourni de mot de passe, on le génère avec le format ROLE_YYYY
            bool passwordWasGenerated = false;
            string plainPassword;
            if (string.IsNullOrWhiteSpace(user.Password))
            {
                plainPassword = GeneratePassword(user.Profile);
                passwordWasGenerated = true;
            }
            else
            {
                plainPassword = user.Password;
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(plainPassword);

            user.ResetPassword = passwordWasGenerated;

   
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUser", new { id = user.Id }, new
            {
                Message = "Utilisateur créé avec succès.",
                Username = user.Login,
                Password = plainPassword,
                ForceResetPassword = user.ResetPassword
            });
        }


        [AllowAnonymous]
        [HttpPost("Register")]
        public async Task<ActionResult<UserRegisterResponse>> Register(User request)
            {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (_context.Users.Any(u => u.Login.ToLower() == request.Login.ToLower()))
                return BadRequest("Un utilisateur avec ce login existe déjà.");

            if(_context.Users.Any(u => u.Telephone == request.Telephone))
                return BadRequest("Un utilisateur avec ce numéro de téléphone existe déjà.");

            if (string.IsNullOrEmpty(request.EntrepriseCode))
            {
                return BadRequest(new { Success = false, Message = "Le code entreprise est obligatoire." });
            }
            var entreprise = await _context.Entreprises
                .FirstOrDefaultAsync(e => e.Code == request.EntrepriseCode && e.Actif);

            if (entreprise == null)
            {
                return BadRequest(new { Success = false, Message = "Code entreprise invalide." });
            }

            // Remplacer le Code par l’ID
            request.EntrepriseId = entreprise.Id;

            var user = new User
            {
                Login = request.Login,
                Telephone = request.Telephone,
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Bureau = request.Bureau,
                EntrepriseId = request.EntrepriseId,
                Actif = true,
                Profile = "USER"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var userRead = _mapper.Map<UserReadDto>(user);

            // claims pour JWT
            var claims = new[]
            {
        new Claim(ClaimTypes.Name, userRead.Login!),
        new Claim(ClaimTypes.NameIdentifier, userRead.Id.ToString()),
        new Claim(ClaimTypes.Role, userRead.Profile!.ToLower()),
        new Claim(JwtRegisteredClaimNames.GivenName,$"{userRead.Login} {userRead.Telephone}"),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

            // génération accessToken
            var res = _tokenService.GenerateAccessToken(claims);
            string accessToken = res.Item1;
            DateTime expire = res.Item2;

            // génération refreshToken
            var refreshToken = _tokenService.GenerateRefreshToken();
            _ = int.TryParse(_config["Jwt:RefreshTokenValidityInDays"], out int refreshTokenValidityInDays);

            var userRefreshToken = new RefreshToken
            {
                UserName = user.Login,
                Refresh_Token = refreshToken,
                Created = DateTime.Now,
                Expires = DateTime.Now.AddDays(refreshTokenValidityInDays)
            };
            _context.RefreshTokens.Add(userRefreshToken);
            await _context.SaveChangesAsync();

            return Ok(new UserRegisterResponse
            {
                Token = accessToken,
                RefreshToken = refreshToken,
                TokenExpireAt = expire,
                User = userRead
            });
        }
        [AllowAnonymous]
        [HttpPost("Authenticate")]
        public async Task<ActionResult<UserLoginResponse>> Authenticate(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return BadRequest("Identifiants manquants.");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Login.ToLower() == username.ToLower());
            if (user == null || !user.Actif)
                return Unauthorized("Utilisateur introuvable ou inactif.");

            // vérification du password hashé
            bool validPassword = BCrypt.Net.BCrypt.Verify(password, user.Password);
            if (!validPassword)
                return Unauthorized("Mot de passe incorrect.");
            //if (user.ResetPassword == true)
            //    return Unauthorized("Mot de passe temporaire : veuillez le changer à la prochaine connexion.");
            var userRead = _mapper.Map<UserReadDto>(user);

            var claims = new[]
            {
        new Claim(ClaimTypes.Name, userRead.Login!),
        new Claim(ClaimTypes.NameIdentifier, userRead.Id.ToString()),
        new Claim(ClaimTypes.Role, userRead.Profile!.ToLower()),
        new Claim(JwtRegisteredClaimNames.GivenName,$"{userRead.Prenom} {userRead.Nom}"),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

            var res = _tokenService.GenerateAccessToken(claims);
            string accessToken = res.Item1;
            DateTime expire = res.Item2;

            var refreshToken = _tokenService.GenerateRefreshToken();
            _ = int.TryParse(_config["Jwt:RefreshTokenValidityInDays"], out int refreshTokenValidityInDays);

            var userRefreshToken = _context.RefreshTokens.FirstOrDefault(rt => rt.UserName == user.Login);
            if (userRefreshToken == null)
            {
                userRefreshToken = new RefreshToken { UserName = user.Login };
                _context.RefreshTokens.Add(userRefreshToken);
            }

            userRefreshToken.Refresh_Token = refreshToken;
            userRefreshToken.Created = DateTime.Now;
            userRefreshToken.Expires = DateTime.Now.AddDays(refreshTokenValidityInDays);

            await _context.SaveChangesAsync();

            return Ok(new UserLoginResponse
            {
                Token = accessToken,
                RefreshToken = refreshToken,
                TokenExpireAt = expire,
                User = userRead
            });
        }

        //[Authorize(Roles = "ADMIN")]
        [HttpPost("SetUserRole/{id}")]
        public async Task<IActionResult> SetUserRole(int id, [FromBody] string role)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            if (role != "ADMIN" && role != "GERANT" && role != "USER")
                return BadRequest("Rôle invalide");

            user.Profile = role;
            await _context.SaveChangesAsync();

            return Ok(new { Message = $"Rôle de {user.Login} mis à jour : {role}" });
        }

        // GET: api/Users
        [HttpGet]
        [Authorize(Policy = IdentityData.AdminUserPolicyName)]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            if (_context.Users == null)
            {
                return NotFound();
            }
            var r = await _context.Users.Include(u => u.Department).Include(e => e.Entreprise).ToListAsync();
            var l = _mapper.Map<IEnumerable<UserReadDto>>(r);
            return Ok(l);
        }
        [HttpGet("Gerant")]
        [Authorize(Policy = IdentityData.AdminUserPolicyName)]
        public async Task<ActionResult<IEnumerable<User>>> GetGerant()
        {
            if (_context.Users == null)
            {
                return NotFound();
            }
            var r = await _context.Users.Include(u => u.Department).Where(p => p.Profile == "GERANT").ToListAsync();
            var l = _mapper.Map<IEnumerable<UserReadDto>>(r);
            return Ok(l);
        }
        [HttpGet("Admin")]
        [Authorize(Policy = IdentityData.AdminUserPolicyName)]
        public async Task<ActionResult<IEnumerable<User>>> GetAdmins()
        {
            if (_context.Users == null)
            {
                return NotFound();
            }
            var r = await _context.Users.Include(u => u.Department).Where(p => p.Profile == "ADMIN").ToListAsync();
            var l = _mapper.Map<IEnumerable<UserReadDto>>(r);
            return Ok(l);
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            if (_context.Users == null)
            {
                return NotFound();
            }
            var user = await _context.Users.Include(u => u.Entreprise)
                                            .Include(d => d.Department)
                             .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }
        // GET: api/Users/5
        [AllowAnonymous]
        [HttpGet("UserByMatricule")]

        public async Task<ActionResult<User>> GetUser(string matricule)
        {
            if (_context.Users == null || string.IsNullOrWhiteSpace(matricule))
            {
                return NotFound();
            }
            var user = await _context.Users.Where(u => u.Matricule!.ToLower() == matricule.ToLower()).FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }
        // PUT: api/Users/5
        [Authorize(Policy = IdentityData.AdminUserPolicyName)]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, [FromBody] User user)
        {
            if (user == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Les données sont nulles"
                });
            }
            if (id != user.Id)
                return BadRequest("L'identifiant de l'utilisateur ne correspond pas.");

            // Vérifier si le login existe déjà pour un autre utilisateur
            var userExist = await _context.Users
                .FirstOrDefaultAsync(u => u.Login == user.Login && u.Id != id);

            if (userExist != null)
                return Problem("Ce nom d'utilisateur existe déjà.");

            // Récupérer l'utilisateur depuis la base
            var userBD = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (userBD == null)
                return NotFound("Utilisateur introuvable.");

            // Mise à jour des champs autorisés
            userBD.Login = user.Login;
            userBD.Telephone = user.Telephone;
            userBD.Email = user.Email;
            userBD.Profile = user.Profile?.ToUpper().Trim() ?? userBD.Profile;
            userBD.Actif = user.Actif;
            userBD.EntrepriseId = user.EntrepriseId;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "Utilisateur mis à jour avec succès" });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                    return NotFound("Utilisateur introuvable.");
                throw;
            }
        }
        [HttpPost("UpdateProfileUser")]
        public async Task<IActionResult> UpdateProfileUser([FromBody] UserProfilDto userRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    status = "error",
                    errors = ModelState
                        .Where(e => e.Value.Errors.Count > 0)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value.Errors.Select(e => e.ErrorMessage)
                        )
                });
            }

            var user = await _context.Users.FindAsync(userRequest.Id);
            if (user == null)
                return NotFound(new { success = false, message = "Utilisateur introuvable" });

            user.Login = userRequest.Login;
            user.Email = userRequest.Email;
            user.Telephone = userRequest.Telephone;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Profil mis à jour avec succès",
                data = user
            });
        }


        [Authorize]
        [HttpGet("GetSolde/{id}")]
        public async Task<IActionResult> GetSolde(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }


            return Ok(user.Solde);


        }
        [Authorize(Policy = IdentityData.AdminUserPolicyName)]
        [HttpPut("RechargerCompte/{id}")]

        public async Task<IActionResult> RechargerCompte(int id, int montant)
        {
            // get user action info from API Token 
            // and log action in Log table
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            if (user.Profile == "GERANT")
            {
                return BadRequest();
            }
            user.Solde += montant;
            user.LastRechargeAmount = montant;
            user.LastRechargeDate = DateTime.Now;
            _context.Entry(user).Property(u => u.Solde).IsModified = true;

            string fullnameUserAction = User.FindFirstValue(ClaimTypes.GivenName);
            string usernameUserAction = User.FindFirstValue(ClaimTypes.Name);
            string profilUserAction = User.FindFirstValue(ClaimTypes.Role);
            if (_context.Log != null)
            {
                _context.Log.Add(new Log
                {
                    ActionProfil = profilUserAction,
                    ActionType = "ChargerCompte",
                    ActionUser = usernameUserAction,
                    Entity = "User",
                    Date = DateTime.Now,
                    Comment = $"Chargement du compte de {user.Prenom + user.Nom + $" ({user.Matricule}) "} d'un montant de {montant}"
                });

            }
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok(_mapper.Map<UserReadDto>(user));
        }

        [Authorize(Policy = IdentityData.AdminUserPolicyName)]
        [HttpPut("GenerateAllQrCode")]
        public async Task<IActionResult> GenerateAllQrCode()
        {

            var listUsers = await _context.Users.Where(u => u.Profile != "GERANT").ToListAsync();
            bool update = false;
            if (listUsers != null)
            {
                foreach (var user in listUsers)
                {

                    if (String.IsNullOrWhiteSpace(user.Matricule))
                    {
                        continue;
                    }
                    if (!String.IsNullOrWhiteSpace(user.Matricule))
                    {
                        user.Matricule = user.Matricule.Trim();
                        int matNumber = Convert.ToInt32(Regex.Match(user.Matricule, @"\d+").Value);
                        int modulo = matNumber % 2;
                        user.QrCode = DPWorldEncryption.SecurityManager.EncryptAES($"{user.Matricule}{modulo}");
                        _context.Entry(user).Property(u => u.QrCode).IsModified = true;
                        update = true;
                    }
                }
                string fullnameUserAction = User.FindFirstValue(ClaimTypes.GivenName);
                string usernameUserAction = User.FindFirstValue(ClaimTypes.Name);
                string profilUserAction = User.FindFirstValue(ClaimTypes.Role);
                if (_context.Log != null)
                {
                    _context.Log.Add(new Log
                    {
                        ActionProfil = profilUserAction,
                        ActionType = "GenerateAllQrCode",
                        ActionUser = usernameUserAction,
                        Entity = "User",
                        Date = DateTime.Now,
                        Comment = $"Génération QR Code de tous les employés"
                    });
                }

                if (update)
                {
                    await _context.SaveChangesAsync();
                }
            }



            return NoContent();
        }

        [Authorize(Policy = IdentityData.AdminUserPolicyName)]
        [HttpPut("SendAllQrCode")]
        public async Task<IActionResult> SendAllQrCode()
        {

            var listUsers = await _context.Users.AsNoTracking().Where(u => u.Profile != "GERANT" && u.Actif).ToListAsync();

            if (listUsers != null)
            {
                foreach (var user in listUsers)
                {
                    if (!String.IsNullOrEmpty(user.QrCode) && !String.IsNullOrEmpty(user.Email))
                    {
                        var qrCode = GenerateQRCode(user.QrCode);

                        var fullname = user.Prenom + " " + user.Nom;
                        //  SaveByteArrayToFileWithStaticMethod(qrCode, "QRCode_" + fullname + ".png");
                        var message = String.Format(Common.QrCodeEmailMessage, fullname) + "\n\n\n" + String.Format(Common.QrCodeEmailMessageEn, fullname);
                        EmailManager.SendEmail(user.Email, "COFFEE SNACK CONTAINER", message, qrCode, "QRCode_" + fullname + ".png");
                    }
                }
            }
            return NoContent();
        }
        [Authorize(Policy = IdentityData.AdminUserPolicyName)]
        [HttpPut("SendUserQrCode/{id}")]
        public async Task<IActionResult> SendUserQrCode(int id)
        {

            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound();

            if (!String.IsNullOrEmpty(user.QrCode) && !String.IsNullOrEmpty(user.Email))
            {
                var qrCode = GenerateQRCode(user.QrCode);

                var fullname = user.Prenom + " " + user.Nom;
                //  SaveByteArrayToFileWithStaticMethod(qrCode, "QRCode_" + fullname + ".png");
                var message = String.Format(Common.QrCodeEmailMessage, fullname) + "\n\n\n" + String.Format(Common.QrCodeEmailMessageEn, fullname);
                var ok = EmailManager.SendEmail(user.Email, "COFFEE SNACK CONTAINER", message, qrCode, "QRCode_" + fullname + ".png");
                if (!ok)
                {
                    return Problem("Une erreur a été rencontrée lors de l'envoi du mail.");
                }
            }
            else
            {
                return Problem("Cet utilisateur n'a pas d'adresse email ou son QR Code n'est pas encore généré.");
            }

            return NoContent();
        }

        [Authorize(Policy = IdentityData.AdminUserPolicyName)]
        [HttpPost("RechargerMultiCompte")]
        public async Task<IActionResult> RechargerMultiCompte(RechargeComptesRequest rechargeComptesRequest)
        {


            if (!(rechargeComptesRequest.UserIds?.Any() ?? false))

            {
                bool exist = await _context.Log.Where(l => l.Date.Month == DateTime.Now.Month && l.Date.Year == DateTime.Now.Year && l.ActionType == "ChargerCompteAllUsers").AnyAsync();
                if (exist)
                {
                    return Problem("Vous avez déjà recharger tous les comptes pour ce mois-ci.");
                }
            }
            var allUsersCount = await _context.Users.AsNoTracking().Where(u => (u.Profile != "GERANT") && u.Actif).CountAsync();
            var all = !(rechargeComptesRequest.UserIds?.Any() ?? false);
            List<User>? listUsers;
            string comment = String.Empty;
            if (rechargeComptesRequest.UserIds?.Any() ?? false)
            {
                listUsers = await _context.Users.Where(u => rechargeComptesRequest.UserIds.Contains(u.Id) && (u.Profile != "GERANT") && u.Actif).ToListAsync();
                comment = $"Chargement des comptes de [List Ids] d'un montant de {rechargeComptesRequest.Montant}";
            }
            else
            {
                listUsers = await _context.Users.Where(u => u.Profile != "GERANT").ToListAsync();
                comment = $"Chargement de tous les comptes d'un montant de {rechargeComptesRequest.Montant}";
            }
            if (rechargeComptesRequest.UserIds.Count == allUsersCount && allUsersCount > 0)

            {
                bool exist = await _context.Log.Where(l => l.Date.Month == DateTime.Now.Month && l.Date.Year == DateTime.Now.Year && l.ActionType == "ChargerCompteAllUsers").AnyAsync();
                if (exist)
                {
                    return Problem("Vous avez déjà recharger tous les comptes pour ce mois-ci.");
                }
            }


            if (listUsers != null)
            {
                foreach (var user in listUsers)
                {


                    user.Solde += rechargeComptesRequest.Montant;
                    user.LastRechargeAmount = rechargeComptesRequest.Montant;
                    user.LastRechargeDate = DateTime.Now;
                    _context.Entry(user).Property(u => u.Solde).IsModified = true;
                    _context.Entry(user).Property(u => u.LastRechargeAmount).IsModified = true;
                    _context.Entry(user).Property(u => u.LastRechargeDate).IsModified = true;


                }
                string fullnameUserAction = User.FindFirstValue(ClaimTypes.GivenName);
                string usernameUserAction = User.FindFirstValue(ClaimTypes.Name);
                string profilUserAction = User.FindFirstValue(ClaimTypes.Role);
                if (_context.Log != null)
                {
                    _context.Log.Add(new Log
                    {
                        ActionProfil = profilUserAction,
                        ActionType = all ? "ChargerCompteAllUsers" : "ChargerCompteListUsers",
                        ActionUser = usernameUserAction,
                        Entity = "User",
                        Date = DateTime.Now,
                        Comment = comment

                    });
                }


                await _context.SaveChangesAsync();

            }

            try
            {
                _ = Task.Run(() =>
                {
                    try
                    {
                        if (listUsers != null)
                        {
                            foreach (var user in listUsers)
                            {

                                if (!String.IsNullOrWhiteSpace(user.Telephone))
                                {
                                    SmsManager.SendSMS(user.Telephone, $"Bonjour {user.Prenom} {user.Nom}, votre compte a ete recharge. Nouveau solde {user.Solde} F CFA");
                                }
                            }
                        }

                    }
                    catch (Exception)
                    {


                    }



                });

            }
            catch (Exception ex)
            {

            }

            return NoContent();
        }
        [Authorize]
        [HttpPost("ResetPassword")]
        public async Task<ActionResult<User>> ResetPassword([FromBody] UserResetPwdRequest utilisateurReset)
        {

            if (ModelState.IsValid)
            {

                string passwordHash = BCrypt.Net.BCrypt.HashPassword(utilisateurReset.OldPassword);
                var oldpwd = utilisateurReset.OldPassword;
                User? utilisateur = await _context.Users
                                                        .FirstOrDefaultAsync(u => u.Login == utilisateurReset.Login);


                if (utilisateur != null)
                {
                    bool verified = BCrypt.Net.BCrypt.Verify(oldpwd, utilisateur.Password);
                    if (verified)
                    {
                        utilisateur.Password = BCrypt.Net.BCrypt.HashPassword(utilisateurReset.NewPassword);
                        utilisateur.ResetPassword = false;
                        if(utilisateurReset.NewPassword != utilisateurReset.ConfirmPassword)
                        {
                            return BadRequest("Le mot de passe de confirmation ne correspond pas au nouveau mot de passe.");
                        }
                        _context.Entry(utilisateur).State = EntityState.Modified;
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        return Problem("L'ancien mot de passe saisi est incorrect.");
                    }

                }
                else
                {
                    return Problem("Nom d'utilisateur incorrect.");
                }
                return NoContent();

            }
            else
            {
                return Problem("Tous les champs sont obligatoires.");
            }
        }

        [AllowAnonymous]
        [HttpPost("ForgetPassword/{email}")]
        public async Task<ActionResult> ForgetPassword(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
                return Problem("Utilisateur introuvable.");

            if (!user.Actif)
                return Problem("Utilisateur inactif.");
            var resetToken = new PasswordResetToken
            {
                Token = Guid.NewGuid().ToString(),
                ExpireAt = DateTime.Now.AddMinutes(15),
                UserId = user.Id
            };

            _context.PasswordResetTokens.Add(resetToken);
            await _context.SaveChangesAsync();

            string linkFrontEnd = Common.FrontEndLink;
            string linkForgetPassword = $"{linkFrontEnd}/Login/ForgotPassword?token={resetToken.Token}";

            //Recuperation de la Queue de Coravel
            var queue = HttpContext.RequestServices.GetRequiredService<IQueue>();

            // Envoi du mail à l'utilisateur
            if (!string.IsNullOrEmpty(user.Email))
            {
                queue.QueueTask(() =>
                {
                    string message = $"Bonjour {user.Login},\n\n" +
                     $"Vous avez demandé à réinitialiser votre mot de passe.\n" +
                     $"Veuillez cliquer sur le lien suivant pour le faire : {linkForgetPassword}\n\n" +
                     $"Si vous n'êtes pas à l'origine de cette demande, ignorez ce message.\n\n" +
                     $"Cordialement,\nL'équipe Cuisinov.";

                        EmailManager.SendEmail(
                            user.Email!,
                            "Réinitialisation de votre mot de passe",
                            message,
                            null,
                            ""
                        );
                    });
            }

            // Notification aux administrateurs
            var adminUsers = await _context.Users
                .Where(u => u.Profile == "ADMIN" && u.Actif)
                .ToListAsync();

            foreach (var admin in adminUsers)
            {
                queue.QueueTask(() =>
                {
                    string messageAdmin = $"L'utilisateur '{user.Login}' a demandé une réinitialisation de mot de passe.";
                    EmailManager.SendEmail(admin.Email!, "Demande de réinitialisation de mot de passe", messageAdmin, null, "");
                });
            }

            return NoContent();
        }


        [AllowAnonymous]
        [HttpPost("ForgotPassword")]
        public async Task<ActionResult> ForgotPassword([FromBody] UserRequestForgotPassword utilisateurReset)
        {
            if (!ModelState.IsValid)
                return Problem("Tous les champs sont obligatoires.");

            var resetTokenEntry = await _context.PasswordResetTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == utilisateurReset.Token);

            if (resetTokenEntry == null || resetTokenEntry.ExpireAt < DateTime.UtcNow)
                return BadRequest("Lien de réinitialisation invalide ou expiré.");

            var utilisateur = resetTokenEntry.User;

            if (utilisateurReset.NewPassword != utilisateurReset.ConfirmPassword)
                return BadRequest("Le mot de passe de confirmation ne correspond pas.");

            utilisateur.Password = BCrypt.Net.BCrypt.HashPassword(utilisateurReset.NewPassword);
            utilisateur.ResetPassword = false;

            _context.PasswordResetTokens.Remove(resetTokenEntry);

            await _context.SaveChangesAsync();

            return NoContent();
        }



        [Authorize(Policy = IdentityData.AdminUserPolicyName)]
        [HttpPut("RenitializePassword/{id}")]
        public async Task<ActionResult> RenitializePassword(int id)
        {
            var user = await _context.Users.Where(u => u.Id == id).FirstOrDefaultAsync();

            if (user == null)
            {
                return Problem("Utilisateur introuvable .");
            }
            if (!user.Actif)
            {
                return Problem("Utilisateur inactif.");
            }
            string password = String.Empty;
            user.Guid = Guid.NewGuid().ToString();
            password = Common.GetRandomAlphanumericString(8);
            if (user.Profile == "GERANT" || user.Profile == "USER" || user.Profile == "ADMIN")
            {
                user.ResetPassword = true;
                user.Password = BCrypt.Net.BCrypt.HashPassword(password);
            }
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            if (user.Email != null)
            {
                var message = String.Format("L'administrateur a réinitialisé le mot de passe de l'utilisateur Nom d'Utilisateur:{0} - Mot de Passe: {1}", user.Login, password);
                EmailManager.SendEmail(user.Email!, "Réinitialisation de mot de passe", message,null,"");
            }
            var adminUser = _context.Users.Where(u => u.Profile == "ADMIN" && u.Actif).ToList();
            if(adminUser != null) {
                foreach(var admin in adminUser)
                {
                    var messageAdmin = String.Format("L'administrateur a réinitialisé le mot de passe de l'utilisateur Nom d'Utilisateur:{0} - Mot de Passe: {1}", user.Login,password);
                    EmailManager.SendEmail(admin.Email!, "Réinitialisation de mot de passe", messageAdmin, null, "");
                }
            }
            return NoContent();


        }


        [Authorize(Policy = IdentityData.AdminUserPolicyName)]
        [HttpPost("ResetPasswordMultiAccounts")]
        public async Task<IActionResult> ResetPasswordMultiAccounts(RechargeComptesRequest rechargeComptesRequest)
        {

            bool update = false;

            if (rechargeComptesRequest.UserIds?.Any() ?? false)
            {
                var ids = rechargeComptesRequest.UserIds;
                foreach (var id in ids)
                {

                    var user = await _context.Users.Where(u => u.Id == id).FirstOrDefaultAsync();

                    if (user == null)
                    {
                        continue;
                    }
                    if (!user.Actif)
                    {
                        continue;
                    }
                    string password = String.Empty;
                    user.Guid = Guid.NewGuid().ToString();
                    if (user.Profile == "GERANT")
                    {

                        if (String.IsNullOrWhiteSpace(user.Telephone))
                        {
                            continue;
                        }
                        user.ResetPassword = true;
                        password = Common.GetRandomAlphanumericString(8);
                        user.Password = BCrypt.Net.BCrypt.HashPassword(password);
                    }
                    else
                    {
                        user.ResetPassword = false;
                    }

                    if (user.Profile != "GERANT" && String.IsNullOrWhiteSpace(user.Matricule))
                    {

                        continue;
                    }

                    var message = String.Format("Reinitialisation de Mot de passe Username:{0} - Password: {1}", user.Login, password);

                    //SmsManager.SendSMS(user.Telephone, Common.CreateAccountMessage);
                    SmsManager.SendSMS(user.Telephone!, message);

                    update = true;
                }

                if (update)
                    await _context.SaveChangesAsync();

            }



            return NoContent();
        }



        // DELETE: api/Users/5
        [Authorize(Policy = IdentityData.AdminUserPolicyName)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (_context.Users == null)
            {
                return NotFound();
            }
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            user.Actif = false;
            _context.Entry(user).Property(u => u.Actif).IsModified = true;
            //_context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        private string GeneratePassword(string profile)
        {
            string prefix = profile.ToUpper().Replace(" ", "");
            string uniquePart = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
            return $"{prefix}_{uniquePart}";
        }


        //[AllowAnonymous]
        //[HttpGet("Authenticate")]
        //public async Task<UserLoginResponse?> Authenticate(string username, string password)
        //{
        //    if (_context.Users == null)
        //    {
        //        return null;
        //    }
        //    if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        //    {
        //        return null;
        //    }

        //    User? user = await _context.Users.Where(u => u.Login.ToLower() == username.ToLower()).FirstOrDefaultAsync();

        //    if (user == null)
        //    {
        //        return null;
        //    }

        //    if (!user.Actif)
        //    {
        //        return null;
        //    }
        //    try
        //    {

        //        bool isUsernamePasswordAuth = ((user.Profile == "GERANT") && BCrypt.Net.BCrypt.Verify(password, user.Password));
        //        bool isADUser = false;
        //        if (!isUsernamePasswordAuth)
        //        {
        //            //System.DirectoryServices.DirectoryEntry? Ldap = new System.DirectoryServices.DirectoryEntry("LDAP://" + "dpworld.sn", username, password);

        //            //if (Ldap.Guid == null)
        //            //{
        //            //    return null;
        //            //}
        //            isADUser = true;
        //        }
        //        if (isADUser || isUsernamePasswordAuth)
        //        {
        //            var userRead = _mapper.Map<UserReadDto>(user);
        //            if (_context.RefreshTokens == null)
        //            {
        //                return null;
        //            }
        //            var userRefreshToken = _context.RefreshTokens.Where(rut => rut.UserName == username).FirstOrDefault();

        //            _ = int.TryParse(_config["jwt:RefreshTokenValidityInDays"], out int refreshTokenValidityInDays);

        //            bool exist = userRefreshToken != null;
        //            if (userRefreshToken == null)
        //            {
        //                userRefreshToken = new RefreshToken();
        //            }
        //            var claims = new[] {
        //                    new Claim(ClaimTypes.Name, userRead.Login!),
        //                    new Claim(ClaimTypes.NameIdentifier, userRead.Id.ToString()),
        //                    new Claim(ClaimTypes.Role, userRead.Profile!.ToLower()!),
        //                    new Claim(JwtRegisteredClaimNames.GivenName,$"{userRead.Prenom} {userRead.Nom}"),
        //                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        //                };

        //            var res = _tokenService.GenerateAccessToken(claims);
        //            string accessToken = res.Item1;
        //            DateTime expire = res.Item2;

        //            var refreshToken = _tokenService.GenerateRefreshToken();
        //            userRefreshToken.Refresh_Token = refreshToken;
        //            userRefreshToken.UserName = username;
        //            userRefreshToken.Created = DateTime.Now;
        //            userRefreshToken.Expires = DateTime.Now.AddDays(refreshTokenValidityInDays);

        //            if (exist)
        //            {
        //                _context.Entry<RefreshToken>(userRefreshToken).State = EntityState.Modified;
        //            }
        //            else
        //            {
        //                _context.RefreshTokens.Add(userRefreshToken);
        //            }
        //            _context.SaveChanges();

        //            return new UserLoginResponse { Token = accessToken, RefreshToken = refreshToken, TokenExpireAt = expire, User = userRead };
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        return null;
        //    }

        //    return null;
        //}

        private bool UserExists(int id)
        {
            return (_context.Users?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        public static void SaveByteArrayToFileWithStaticMethod(byte[] data, string filePath)
  => System.IO.File.WriteAllBytes(filePath, data);

        protected byte[] GenerateQRCode(string code)
        {

            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(code, QRCodeGenerator.ECCLevel.Q);
            BitmapByteQRCode qrCode = new(qrCodeData);
            string base64String = Convert.ToBase64String(qrCode.GetGraphic(20));

            return qrCode.GetGraphic(20).ToArray();
        }

        //protected void GeneratePDF(object sender, EventArgs e)
        //{
        //    string base64 = Convert.ToBase64String(byteImage);
        //    byte[] imageBytes = Convert.FromBase64String(base64);
        //    iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(imageBytes);
        //    using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
        //    {
        //        Document document = new Document(PageSize.A4, 0f, 0f, 0f, 0f);
        //        PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);
        //        document.Open();
        //        document.Add(image);
        //        document.Close();
        //        byte[] bytes = memoryStream.ToArray();
        //        memoryStream.Close();
        //        Response.Clear();
        //        Response.ContentType = "application/pdf";
        //        Response.AddHeader("Content-Disposition", "attachment; filename=Image.pdf");
        //        Response.ContentType = "application/pdf";
        //        Response.Buffer = true;
        //        Response.Cache.SetCacheability(HttpCacheability.NoCache);
        //        Response.BinaryWrite(bytes);
        //        Response.End();
        //    }
        //}
    }
}
