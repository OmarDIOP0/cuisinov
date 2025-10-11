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
using CantineBack.Models.Dtos;
using CantineBack.Identity;
using Microsoft.AspNetCore.Authorization;

namespace CantineBack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class PaymentMethodsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        public PaymentMethodsController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/PaymentMethods
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<PaymentMethodReadDto>>> GetPaymentMethods()
        {
          if (_context.PaymentMethods == null)
          {
              return NotFound();
          }
            var l= await _context.PaymentMethods.Where(pm=>pm.Actif).ToListAsync();
            return Ok(_mapper.Map<IEnumerable<PaymentMethodReadDto>>(l));
        }

        // GET: api/PaymentMethods/5
        [HttpGet("{id}")]
        [Authorize(Roles = IdentityData.AdminOrGerantUserRoles)]
        public async Task<ActionResult<PaymentMethod>> GetPaymentMethod(int id)
        {
          if (_context.PaymentMethods == null)
          {
              return NotFound();
          }
            var categorie = await _context.PaymentMethods.FindAsync(id);

            if (categorie == null)
            {
                return NotFound();
            }

            return categorie;
        }

        // PUT: api/PaymentMethods/5
        [HttpPut("{id}")]
        [Authorize(Roles = IdentityData.AdminOrGerantUserRoles)]
        public async Task<IActionResult> PutPaymentMethod(int id, PaymentMethod payment)
        {
            if (id != payment.Id)
            {
                return BadRequest();
            }

            _context.Entry(payment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PaymentMethodExists(id))
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

        // POST: api/PaymentMethods
        [HttpPost]
        [Authorize(Roles = IdentityData.AdminOrGerantUserRoles)]
        public async Task<ActionResult<PaymentMethod>> PostPaymentMethod(PaymentMethod request)
        {
          if (_context.PaymentMethods == null)
          {
              return Problem("Entity set 'ApplicationDbContext.PaymentMethods'  is null.");
          }
            PaymentMethod payment = new PaymentMethod()
            {
                Name = request.Name.ToUpper(),
                Code = request.Code.ToUpper(),
                Actif = request.Actif,
            };
            if(request.Code == null)
            {
                payment.Code = GenerateCode(request.Code);
            }
            _context.PaymentMethods.Add(payment);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPaymentMethod", new { id = payment.Id }, payment);
        }

        // DELETE: api/PaymentMethods/5
        [HttpDelete("{id}")]
        [Authorize(Roles = IdentityData.AdminOrGerantUserRoles)]
        public async Task<IActionResult> DeletePaymentMethod(int id)
        {
            if (_context.PaymentMethods == null)
            {
                return NotFound();
            }
            var payment = await _context.PaymentMethods.FindAsync(id);
            if (payment == null)
            {
                return NotFound();
            }

            _context.PaymentMethods.Remove(payment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PaymentMethodExists(int id)
        {
            return (_context.PaymentMethods?.Any(e => e.Id == id)).GetValueOrDefault();
        }
        private string GenerateCode(string nomPayment)
        {
            string cleaned = new string(nomPayment
                          .Where(char.IsLetter)
                          .ToArray())
                           .ToUpper();

            return cleaned.Length >= 3 ? cleaned.Substring(0,3) : cleaned;
        }
    }
}
