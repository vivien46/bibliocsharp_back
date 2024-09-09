namespace Server.Models
{
    public class Livre
    {
        public int Id { get; set; }
        public string Titre { get; set; }
        public string Auteur { get; set; }
        public string Editeur { get; set; }
        public int Annee { get; set; }
        public string ISBN { get; set; }
        public string? ImageUrl { get; set; }

        public ICollection<Emprunt> ?Emprunts { get; set; }
    }
}