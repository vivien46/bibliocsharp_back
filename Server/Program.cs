using Microsoft.EntityFrameworkCore;
using Npgsql;
using Server.Models;
using Server.Database;

var builder = WebApplication.CreateBuilder(args);

// Utiliser la chaîne de connexion fournie directement
var databaseUrl = "postgresql://bibliocsharp_owner:lmJnwOY2T9UN@ep-ancient-grass-a25nwoyo.eu-central-1.aws.neon.tech/bibliocsharp?sslmode=require";

// Convertir la chaîne en Uri pour extraire les informations nécessaires
var databaseUri = new Uri(databaseUrl);
var userInfo = databaseUri.UserInfo.Split(':');

// Créer une chaîne de connexion compatible avec Npgsql
var connectionStringBuilder = new NpgsqlConnectionStringBuilder
{
    Host = databaseUri.Host,
    Port = databaseUri.IsDefaultPort ? 5432 : databaseUri.Port,
    Username = userInfo[0],
    Password = userInfo[1],
    Database = databaseUri.LocalPath.TrimStart('/'),
    SslMode = SslMode.Require,  // SslMode est requis pour Neon
};

// Ajouter cette chaîne de connexion dans Entity Framework
builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseNpgsql(connectionStringBuilder.ConnectionString);
});

// Autres configurations
var livresImagesPath = Path.Combine(AppContext.BaseDirectory, "..\\Client\\public\\assets\\Images\\Livres");
var fullImagesPath = Path.GetFullPath(livresImagesPath);

builder.Environment.WebRootPath = Path.GetFullPath("..\\Client\\public");

// Ajouter les services au conteneur
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
});

builder.Services.AddDistributedMemoryCache();

// Ajouter la gestion des sessions
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".BiblioSession.Session";
    options.IdleTimeout = TimeSpan.FromMinutes(15);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Ajouter la configuration CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("VercelSpecificOrigin", policy =>
    {
        policy.WithOrigins("https://mokarube46-biblio.vercel.app")
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});

// Configurer Kestrel pour l'écoute HTTP et HTTPS
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5263); // HTTP
    options.ListenAnyIP(7153, listenOptions =>
    {
        listenOptions.UseHttps(); // Utiliser le certificat de développement
    });
});

var app = builder.Build();

// Configurer le pipeline des requêtes HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCors("VercelSpecificOrigin");
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapControllers();
app.Run();