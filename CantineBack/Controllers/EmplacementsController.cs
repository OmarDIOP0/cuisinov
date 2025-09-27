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
  
    public class EmplacementsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        public EmplacementsController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/Emplacements
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<EmplacementReadDto>>> GetEmplacements()
        {
          if (_context.Emplacement == null)
          {
              return NotFound();
          }
            var l =await _context.Emplacement.ToListAsync();

            return Ok(_mapper.Map<IEnumerable<EmplacementReadDto>>( l));   
        }

        // GET: api/Emplacements/5
        [HttpGet("{id}")]
        [Authorize(Roles = IdentityData.AdminOrGerantUserRoles)]
        public async Task<ActionResult<Emplacement>> GetEmplacement(int id)
        {
          if (_context.Emplacement == null)
          {
              return NotFound();
          }
            var emplacement = await _context.Emplacement.FindAsync(id);

            if (emplacement == null)
            {
                return NotFound();
            }

            return emplacement;
        }

        // PUT: api/Emplacements/5
        [HttpPut("{id}")]
        [Authorize(Roles = IdentityData.AdminOrGerantUserRoles)]
        public async Task<IActionResult> PutEmplacement(int id, Emplacement emplacement)
        {
            if (id != emplacement.Id)
            {
                return BadRequest();
            }

            _context.Entry(emplacement).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmplacementExists(id))
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

        // POST: api/Emplacements
        [HttpPost]
        [Authorize(Roles = IdentityData.AdminOrGerantUserRoles)]
        public async Task<ActionResult<Emplacement>> PostEmplacement(Emplacement emplacement)
        {
            emplacement.Actif = true;
          if (_context.Emplacement == null)
          {
              return Problem("Entity set 'ApplicationDbContext.Emplacement'  is null.");
          }
            _context.Emplacement.Add(emplacement);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetEmplacement", new { id = emplacement.Id }, emplacement);
        }

        // DELETE: api/Emplacements/5
        [HttpDelete("{id}")]
        [Authorize(Roles = IdentityData.AdminOrGerantUserRoles)]
        public async Task<IActionResult> DeleteEmplacement(int id)
        {
            if (_context.Emplacement == null)
            {
                return NotFound();
            }
            var emplacement = await _context.Emplacement.FindAsync(id);
            if (emplacement == null)
            {
                return NotFound();
            }
            emplacement.Actif = false;
            _context.Entry(emplacement).Property(u => u.Actif).IsModified = true;
            // _context.Emplacement.Remove(emplacement);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EmplacementExists(int id)
        {
            return (_context.Emplacement?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
