using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Server.Models;

public class Emprunt
{
    public int Id { get; set; }
    public DateTime DateEmprunt { get; set; }
    public DateTime DateRetour { get; set; }
    public int LivreId { get; set; }
    public Livre Livre { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
}