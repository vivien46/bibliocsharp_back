using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using Server.Models;
using Server.Database;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmpruntController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public EmpruntController(DataContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostEnvironment;

        }

        [HttpGet]
        public async Task<ActionResult<List<Emprunt>>> GetEmprunts()
        {
            var emprunts = await _context.Emprunts
                .Include(e => e.User)
                .Include(e => e.Livre)
                .ToListAsync();
            return Ok(emprunts);

        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Emprunt>> GetEmprunt(int id)
        {
            var emprunt = await _context.Emprunts
                .Include(e => e.User)
                .Include(e => e.Livre)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (emprunt == null)
            {
                return NotFound("Emprunt non trouvé");
            }

            return Ok(emprunt);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddEmprunt([FromForm] string dateEmprunt, [FromForm] string dateRetour, [FromForm] int livreId, [FromForm] int userId)
        {
            try
            {
        // Conversion des dates du format jj/mm/aaaa en DateTime
        DateTime dateEmpruntConverted = DateTime.ParseExact(dateEmprunt, "dd/MM/yyyy", CultureInfo.InvariantCulture);

        DateTime dateRetourConverted;
        if (!string.IsNullOrEmpty(dateRetour))
        {
            dateRetourConverted = DateTime.ParseExact(dateRetour, "dd/MM/yyyy", CultureInfo.InvariantCulture);
        }
        else
        {
            // Si la date de retour n'est pas spécifiée, ajouter 14 jours à la date d'emprunt
            dateRetourConverted = dateEmpruntConverted.AddDays(14);
        }

        // Forcer les dates en UTC
        dateEmpruntConverted = DateTime.SpecifyKind(dateEmpruntConverted, DateTimeKind.Utc);
        dateRetourConverted = DateTime.SpecifyKind(dateRetourConverted, DateTimeKind.Utc);

        // Création de l'emprunt
        Emprunt emprunt = new Emprunt
        {
            DateEmprunt = dateEmpruntConverted,
            DateRetour = dateRetourConverted,
            LivreId = livreId,
            UserId = userId
        };

        // Sauvegarde dans la base de données
        _context.Emprunts.Add(emprunt);
        await _context.SaveChangesAsync();

        var empruntCreated = await _context.Emprunts
            .Include(e => e.User)
            .Include(e => e.Livre)
            .FirstOrDefaultAsync(e => e.Id == emprunt.Id);

        // Retourner une réponse avec l'emprunt créé
        return CreatedAtAction("GetEmprunt", new { id = emprunt.Id }, emprunt);
    }
    catch (Exception ex)
    {
        return BadRequest(new { error = ex.Message });
    }
}


        [HttpPut("edit/{id}")]
        public async Task<ActionResult> UpdateEmprunt(int id, [FromForm] Emprunt emprunt)
        {
            if (id != emprunt.Id)
            {
                return BadRequest("ID de l'emprunt ne correspond pas.");
            }

            var existingEmprunt = await _context.Emprunts.FindAsync(id);
            if (existingEmprunt == null)
            {
                return NotFound("Emprunt non trouvé.");
            }

            existingEmprunt.LivreId = emprunt.LivreId;
            existingEmprunt.UserId = emprunt.UserId;
            existingEmprunt.DateEmprunt = emprunt.DateEmprunt;
            existingEmprunt.DateRetour = emprunt.DateRetour;

            _context.Entry(existingEmprunt).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmpruntExists(id))
                {
                    return NotFound("Emprunt non trouvé.");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteEmprunt(int id)
        {
            var emprunt = await _context.Emprunts.FindAsync(id);
            if (emprunt == null)
            {
                return NotFound("Emprunt non trouvé.");
            }

            _context.Emprunts.Remove(emprunt);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EmpruntExists(int id)
        {
            return _context.Emprunts.Any(e => e.Id == id);
        }

    }
}