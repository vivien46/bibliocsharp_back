using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Server.Models;
using Server.Database;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public BookController(DataContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostEnvironment;

        }

        [HttpPost("add")]
        public async Task<IActionResult> AddLivre([FromForm] Livre livre, [FromForm] IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                return BadRequest("Image is missing");

            // Assume que le chemin relatif vers le dossier Client est correctement défini dans le front-end
            var fileName = Path.GetFileName(imageFile.FileName);

            // Stockez uniquement le nom du fichier dans la base de données
            livre.ImageUrl = fileName;

            // Enregistrez le fichier dans le dossier du client
            var clientPath = Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\..\\Client\\public\\assets\\Images\\Livres");

            if (!Directory.Exists(clientPath))
                Directory.CreateDirectory(clientPath);

            var fullPath = Path.Combine(clientPath, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            // Ajoutez le livre à la base de données
            _context.Livres.Add(livre);
            await _context.SaveChangesAsync();

            return Ok(livre);
        }


        [HttpGet()]
        public async Task<IActionResult> GetLivres()
        {
            List<Livre> livres = await _context.Livres.OrderBy(Livre => Livre.Id).ToListAsync();
            return Ok(livres);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetLivre(int id)
        {
            var livre = await _context.Livres.FindAsync(id);
            if (livre == null)
            {
                return NotFound("Livre non trouvé");
            }

            return Ok(livre);
        }

        [HttpGet("image/{fileName}")]
        public IActionResult GetImage(string fileName)
        {
            var imagePath = Path.Combine(_hostingEnvironment.WebRootPath, "assets/Images/Livres", fileName);
            if (!System.IO.File.Exists(imagePath))
            {
                return NotFound("Image non trouvée");
            }

            var image = System.IO.File.OpenRead(imagePath);
            return File(image, "image/jpeg");
        }

        [HttpGet("previous/{id}")]
        public async Task<IActionResult> GetPreviousBook(int id)
        {
            var currentBook = await _context.Livres.FindAsync(id);
            if (currentBook == null)
            {
                return NotFound("Livre actuel non trouvé");
            }

            var livre = await _context.Livres.Where(l => l.Id < id).OrderByDescending(l => l.Id).FirstOrDefaultAsync();
            if (livre == null)
            {
                return NotFound("Livre précédent non trouvé");
            }

            return Ok(livre);
        }

        [HttpGet("next/{id}")]
        public async Task<IActionResult> GetNextBook(int id)
        {
            var currentBook = await _context.Livres.FindAsync(id);
            if (currentBook == null)
            {
                return NotFound("Livre actuel non trouvé");
            }

            var livre = await _context.Livres.Where(l => l.Id > id).OrderBy(l => l.Id).FirstOrDefaultAsync();
            if (livre == null)
            {
                return NotFound("Livre suivant non trouvé");
            }

            return Ok(livre);
        }



        [HttpPut("edit/{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] string titre, [FromForm] string auteur, [FromForm] string editeur, [FromForm] int annee, [FromForm] string isbn, [FromForm] IFormFile? image)
        {
            try
            {
                var bookToUpdate = await _context.Livres.FindAsync(id);

                if (bookToUpdate == null)
                {
                    return NotFound("Livre non trouvé");
                }

                if (image != null && image.Length > 0)
                {
                    var projectDirectory = Path.GetFullPath("..\\Client\\public");
                    Console.WriteLine($"projectDirectory: {projectDirectory}");

                    var imagePath = Path.Combine(projectDirectory, "assets\\Images\\Livres");
                    Console.WriteLine($"imagePath: {imagePath}");

                    if (!Directory.Exists(imagePath))
                        Directory.CreateDirectory(imagePath);

                    var fileName = Path.GetFileName(image.FileName);
                    var fullPath = Path.Combine(imagePath, fileName);

                    Console.WriteLine($"Tentative d'enregistrement de l'image à : {fullPath}");

                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }

                    Console.WriteLine($"Image enregistrée à : {fullPath}");

                    bookToUpdate.ImageUrl = $"{fileName}";
                }

                bookToUpdate.Titre = titre;
                bookToUpdate.Auteur = auteur;
                bookToUpdate.Editeur = editeur;
                bookToUpdate.Annee = annee;
                bookToUpdate.ISBN = isbn;

                await _context.SaveChangesAsync();

                return Ok(bookToUpdate);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur : {ex.Message}");
                return StatusCode(500, "Erreur interne du serveur");
            }
        }


        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var book = await _context.Livres.FindAsync(id);
            if (book == null)
            {
                return NotFound("Livre à supprimer non trouvé");
            }

            _context.Livres.Remove(book);
            await _context.SaveChangesAsync();

            return Ok(book);
        }
    }
}
