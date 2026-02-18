using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace JobApplicator2.Services
{
    // --- MODELS ---
    public class HtmlTemplate
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = "Neues Design";
        public string HtmlDeckblatt { get; set; } = "<h1>Deckblatt</h1>";
        public string HtmlAnschreiben { get; set; } = "<p>Ihr Text...</p>";
        public string HtmlLebenslauf { get; set; } = "<h1>Lebenslauf</h1>";
        public string CustomCss { get; set; } = "body { font-family: Arial; }";
    }

    public class AiConfig
    {
        [Key]
        public int Id { get; set; }
        public string SelectedProvider { get; set; } = "Ollama";
        public string OllamaUrl { get; set; } = "http://localhost:11434";
        public string DefaultOllamaModel { get; set; } = "llama3.2";
        public string GeminiApiKey { get; set; } = "";
        public string GeminiModel { get; set; } = "gemini-1.5-flash";
        public double Temperature { get; set; } = 0.7;
    }

    public class ResumeData
    {
        [Key]
        public int Id { get; set; }
        public PersonalData Personal { get; set; } = new();
        public List<Skill> Skills { get; set; } = new();
        public List<string> Competences { get; set; } = new();
        public List<Experience> Experiences { get; set; } = new();
        public List<Education> EducationHistory { get; set; } = new();
    }

    public class PersonalData
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Title { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Location { get; set; } = "";
        public string Website { get; set; } = "";
        public string ProfilePictureBase64 { get; set; } = "";
    }

    public class Skill
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Level { get; set; } = "";
    }

    public class Experience
    {
        [Key]
        public int Id { get; set; }
        public string Company { get; set; } = "";
        public string Period { get; set; } = "";
        public string Role { get; set; } = "";
        public string Description { get; set; } = "";
    }

    public class Education
    {
        [Key]
        public int Id { get; set; }
        public string School { get; set; } = "";
        public string Period { get; set; } = "";
        public string Degree { get; set; } = "";
        public string Notes { get; set; } = "";
    }

    public class ApplicationRecord
{
    [Key]
    public int Id { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? AppliedAt { get; set; }

    public DateTime? StatusChangedAt { get; set; }
        // Status für das Dashboard (Entwurf, Versendet, Abgesagt, Zusage)
    public string Status { get; set; } = "Entwurf";
    
    // Basis-Informationen für die Liste
    public string JobTitle { get; set; } = "";
    public string Company { get; set; } = "";
    
    // Kontaktdaten
    public string? ContactPerson { get; set; } = "";
    public string? Street { get; set; } = "";
    public string? ZipCode { get; set; } = "";
    public string? City { get; set; } = "";
    public string? FullAddress { get; set; } = "";
    
    // Dashboard-Details & Filter
    public string? SalaryInfo { get; set; } = "";
    public string? WorkTimeModel { get; set; } = "";
    
    // Listen (werden via JsonSerializer in der DB gespeichert)
    public List<string> Requirements { get; set; } = new();
    public List<string> Benefits { get; set; } = new();
    
    // Die ausführliche Beschreibung
    public string? FullJobDescription { get; set; } = "";
    public string? JobSummary { get; set; }

        // Die fertigen HTML-Blöcke (Nullable für bestehende Datensätze)
    public string? HtmlCover { get; set; }
    public string? HtmlLetter { get; set; } 
    public string? HtmlResume { get; set; }
    public string? HtmlAttachments { get; set; }

    public string? AppliedCss { get; set; }

    // Der reine Text vom KI-Modell (ohne HTML-Tags)
    public string? RawLetterText { get; set; }
}

    // --- CONTEXT ---

    public class AppDbContext : DbContext
    {
        public DbSet<ResumeData> Profiles { get; set; }
        public DbSet<ApplicationRecord> Applications { get; set; }
        public DbSet<AiConfig> AiSettings { get; set; }
        public DbSet<HtmlTemplate> Templates { get; set; }
        public DbSet<Skill> Skills { get; set; }
    public DbSet<Experience> Experiences { get; set; }
    public DbSet<Education> EducationHistory { get; set; }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ResumeData>()
                .Property(e => e.Competences)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null) ?? new List<string>()
                );

            modelBuilder.Entity<ApplicationRecord>()
                .Property(e => e.Requirements)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null) ?? new List<string>()
                );

            modelBuilder.Entity<ApplicationRecord>()
                .Property(e => e.Benefits)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null) ?? new List<string>()
                );
        }
    }

    // --- HELPER CLASS FOR PLACEHOLDERS ---

    public static class TemplateEngine
    {
        public static string Render(string html, ResumeData? profile, ApplicationRecord? app = null)
        {
            if (string.IsNullOrEmpty(html)) return "";

            // --- 1. Persönliche Daten ---
            var p = profile?.Personal;
            string photoHtml = !string.IsNullOrEmpty(p?.ProfilePictureBase64)
                  ? p.ProfilePictureBase64
                  : "data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7";

            html = html
                .Replace("{{Name}}", p?.Name ?? "Name")
                .Replace("{{Title}}", p?.Title ?? "Titel")
                .Replace("{{Email}}", p?.Email ?? "E-Mail")
                .Replace("{{Phone}}", p?.Phone ?? "Telefon")
                .Replace("{{Location}}", p?.Location ?? "Ort")
                .Replace("{{Website}}", p?.Website ?? "Website")
                .Replace("{{Photo}}", photoHtml);

            // --- 2. Bewerbungsdaten ---
            html = html
                .Replace("{{JobTitle}}", app?.JobTitle ?? "Position")
                .Replace("{{Company}}", app?.Company ?? "Firma")
                .Replace("{{ContactPerson}}", app?.ContactPerson ?? "ContactPerson")
                .Replace("{{Street}}", app?.Street ?? "Straße")
                .Replace("{{ZipCode}}", app?.ZipCode ?? "PLZ")
                .Replace("{{City}}", app?.City ?? "Stadt")
                .Replace("{{Date}}", DateTime.Now.ToShortDateString())
                .Replace("{{LetterText}}", app?.RawLetterText ?? "Anschreiben-Text...");

            // --- 3. Komplexe Listen ---

            // Berufserfahrung
            var expHtml = new StringBuilder();

            if (profile?.Experiences != null && profile.Experiences.Any())
            {
                foreach (var exp in profile.Experiences)
                {
                    expHtml.Append($@"
        <div class='exp-item mb-3'>
            <div class='d-flex justify-content-between'>
                <strong contenteditable='true'>{exp.Role}</strong>
                <span class='text-muted' contenteditable='true'>{exp.Period}</span>
            </div>
            <div contenteditable='true'>{exp.Company}</div>
            <small class='text-secondary' contenteditable='true'>
                {exp.Description}
            </small>
        </div>");
                }
            }
            else
            {
                expHtml.Append("<div class='text-muted'>Keine Berufserfahrung hinterlegt.</div>");
            }

            html = html.Replace("{{CV_Experience}}", expHtml.ToString());

            // Ausbildung
            var eduHtml = new StringBuilder();

            if (profile?.EducationHistory != null && profile.EducationHistory.Any())
            {
                // HIER NEU: Sortierung nach dem String 'Period' 
                // Wir nehmen die letzten 4 Zeichen (meistens das Endjahr) für eine einfache Sortierung
                var sortedEducation = profile.EducationHistory
                    .OrderByDescending(e => e.Period.Length >= 4 ? e.Period.Substring(e.Period.Length - 4) : e.Period)
                    .ToList();

                foreach (var edu in sortedEducation) // Nutze hier die sortierte Liste
                {
                    eduHtml.Append($@"
            <div class='edu-item mb-3'>
                <div class='d-flex justify-content-between'>
                    <strong contenteditable='true'>{edu.School}</strong>
                    <span class='text-muted' contenteditable='true'>{edu.Period}</span>
                </div>
                <div contenteditable='true'>{edu.Degree}</div>
                {(string.IsNullOrEmpty(edu.Notes) ? "" : $"<small class='text-secondary' contenteditable='true'>{edu.Notes}</small>")}
            </div>");
                }
            }
            else
            {
                eduHtml.Append("<div class='text-muted'>Keine Ausbildung hinterlegt.</div>");
            }

            html = html.Replace("{{CV_Education}}", eduHtml.ToString());

            // Skills
            // 1. Start-Tag definieren
            var skillsOpening = "<ul style=\"padding-left: 15px; font-size: 9pt; color: #ccc; margin-bottom: 0;\">";

            // 2. Den Inhalt generieren (die <li> Elemente)
            var skillsItems = profile?.Skills != null && profile.Skills.Any()
                ? string.Join("", profile.Skills.Select(s => $"<li><span contenteditable=\"true\">{s.Name}</span></li>"))
                : "<li>Keine Skills</li>";

            // 3. Alles zusammenfügen: Start + Inhalt + Ende
            var fullSkillsHtml = skillsOpening + skillsItems + "</ul>";

            // 4. Den Platzhalter im Dokument ersetzen
            html = html.Replace("{{Skills_Content}}", fullSkillsHtml);


            // Kompetenzen
            // 1. Start-Tag (Style analog zu den Skills, damit es optisch passt)
            var compOpening = "<ul style=\"padding-left: 15px; font-size: 9pt; color: #ccc; margin-bottom: 0;\">";

            // 2. Den Inhalt generieren
            var compItems = (profile?.Competences != null && profile.Competences.Any())
                ? string.Join("", profile.Competences.Select(c => $"<li contenteditable='true'>{c}</li>"))
                : "<li contenteditable='true'>Keine Kompetenzen hinterlegt</li>";

            // 3. Alles zusammenbauen
            var fullCompHtml = compOpening + compItems + "</ul>";

            // 4. Ersetzen
            html = html.Replace("{{Competences_Content}}", fullCompHtml);

            return html;
        }
    }

    // --- SERVICES ---

    public class ProfileService
    {
        private readonly IDbContextFactory<AppDbContext> _dbFactory;

        public ProfileService(IDbContextFactory<AppDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
            using var context = _dbFactory.CreateDbContext();
            context.Database.EnsureCreated();
        }

        public async Task<ResumeData?> LoadProfileAsync()
        {
            using var context = _dbFactory.CreateDbContext();
            return await context.Profiles
                .Include(p => p.Personal)
                .Include(p => p.Skills)             // Wieder rein!
                .Include(p => p.Experiences)        // Wieder rein!
                .Include(p => p.EducationHistory)   // Wieder rein!
                .FirstOrDefaultAsync();
        }

        public async Task SaveProfileAsync(ResumeData data)
        {
            using var context = _dbFactory.CreateDbContext();
            var existing = await context.Profiles
                .Include(p => p.Personal)
                .Include(p => p.Skills)
                .Include(p => p.Experiences)
                .Include(p => p.EducationHistory)
                .FirstOrDefaultAsync();

            if (existing == null)
            {
                context.Profiles.Add(data);
            }
            else
            {
                context.Entry(existing).CurrentValues.SetValues(data);
                if (existing.Personal != null && data.Personal != null)
                    context.Entry(existing.Personal).CurrentValues.SetValues(data.Personal);

                // Listen-Update: Alt weg, neu her
                context.Skills.RemoveRange(existing.Skills);
                existing.Skills = data.Skills;

                context.Experiences.RemoveRange(existing.Experiences);
                existing.Experiences = data.Experiences;

                context.EducationHistory.RemoveRange(existing.EducationHistory);
                existing.EducationHistory = data.EducationHistory;
            }
            await context.SaveChangesAsync();
        }
    }

    public class DatabaseService
    {
        private readonly IDbContextFactory<AppDbContext> _dbFactory;

        public DatabaseService(IDbContextFactory<AppDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using var context = _dbFactory.CreateDbContext();
            // Erstellt die DB, falls sie noch gar nicht existiert
            context.Database.EnsureCreated();

            try
            {
                // 1. Check: Existiert die alte Spalte noch? Dann benenne sie um.
                // SQLite unterstützt RENAME COLUMN erst seit neueren Versionen (3.25+).
                // Falls das fehlschlägt, fügen wir die Spalte einfach neu hinzu.
                context.Database.ExecuteSqlRaw("ALTER TABLE Applications RENAME COLUMN GeneratedLetter TO HtmlLetter;");
            }
            catch
            {
                // Falls RENAME nicht geht (weil Spalte schon weg oder SQLite zu alt), 
                // versuchen wir sie neu anzulegen
                try { context.Database.ExecuteSqlRaw("ALTER TABLE Applications ADD COLUMN HtmlLetter TEXT;"); } catch { }
            }

            // 2. Weitere neue Spalten sicherstellen
            try { context.Database.ExecuteSqlRaw("ALTER TABLE Applications ADD COLUMN HtmlCover TEXT;"); } catch { }
            try { context.Database.ExecuteSqlRaw("ALTER TABLE Applications ADD COLUMN HtmlResume TEXT;"); } catch { }
            try { context.Database.ExecuteSqlRaw("ALTER TABLE Applications ADD COLUMN HtmlAttachments TEXT;"); } catch { }
            try { context.Database.ExecuteSqlRaw("ALTER TABLE Applications ADD COLUMN AppliedCss TEXT;"); } catch { }
            try { context.Database.ExecuteSqlRaw("ALTER TABLE Applications ADD COLUMN RawLetterText TEXT;"); } catch { }
            try { context.Database.ExecuteSqlRaw("ALTER TABLE Applications ADD COLUMN JobTitle TEXT;"); } catch { }
            try { context.Database.ExecuteSqlRaw("ALTER TABLE Applications ADD COLUMN Company TEXT;"); } catch { }
            try { context.Database.ExecuteSqlRaw("ALTER TABLE Applications ADD COLUMN Status TEXT DEFAULT 'Entwurf';"); } catch { }
            try { context.Database.ExecuteSqlRaw("ALTER TABLE Applications ADD COLUMN SalaryInfo TEXT;"); } catch { }
            try { context.Database.ExecuteSqlRaw("ALTER TABLE Applications ADD COLUMN WorkTimeModel TEXT;"); } catch { }
            try { context.Database.ExecuteSqlRaw("ALTER TABLE Applications ADD COLUMN FullJobDescription TEXT;"); } catch { }
            try { context.Database.ExecuteSqlRaw("ALTER TABLE Applications ADD COLUMN AppliedAt TEXT;"); } catch { }
            try { context.Database.ExecuteSqlRaw("ALTER TABLE Applications ADD COLUMN StatusChangedAt TEXT;"); } catch { }
            try { context.Database.ExecuteSqlRaw("ALTER TABLE Applications ADD COLUMN JobSummary TEXT;"); } catch { }

           
            // 3. Fix für andere Tabellen
            try { context.Database.ExecuteSqlRaw("ALTER TABLE AiSettings ADD COLUMN GeminiModel TEXT DEFAULT 'gemini-1.5-flash';"); } catch { }
            try { context.Database.ExecuteSqlRaw("CREATE TABLE IF NOT EXISTS Templates (Id INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT, HtmlDeckblatt TEXT, HtmlAnschreiben TEXT, HtmlLebenslauf TEXT, CustomCss TEXT);"); } catch { }


            try
            {
                context.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS Skills (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT, Level TEXT,
                ResumeDataId INTEGER,
                FOREIGN KEY (ResumeDataId) REFERENCES Profiles (Id) ON DELETE CASCADE
            );");
            }
            catch { }

            try
            {
                context.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS Experiences (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Company TEXT, Period TEXT, Role TEXT, Description TEXT,
                ResumeDataId INTEGER,
                FOREIGN KEY (ResumeDataId) REFERENCES Profiles (Id) ON DELETE CASCADE
            );");
            }
            catch { }

            try
            {
                context.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS EducationHistory (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                School TEXT, Period TEXT, Degree TEXT, Notes TEXT,
                ResumeDataId INTEGER,
                FOREIGN KEY (ResumeDataId) REFERENCES Profiles (Id) ON DELETE CASCADE
            );");
            }
            catch { }
        }

        public string RenderTemplate(string html, ResumeData? profile, ApplicationRecord? app = null)
        {
            return TemplateEngine.Render(html, profile, app);
        }
        public async Task<List<ApplicationRecord>> GetApplicationsAsync()
        {
            return await GetAllApplicationsAsync();
        }
        public async Task<int> SaveApplicationAsync(ApplicationRecord record)
        {
            using var context = _dbFactory.CreateDbContext();

            // "Sanitizing": Null-Werte in leere Strings umwandeln für Felder, die nicht null sein dürfen
            record.JobTitle ??= "";
            record.Company ??= "";
            record.Status ??= "Entwurf";
            record.ContactPerson ??= "";
            record.Street ??= "";
            record.ZipCode ??= "";
            record.City ??= "";
            record.FullAddress ??= "";
            record.SalaryInfo ??= "";
            record.WorkTimeModel ??= "";
            record.FullJobDescription ??= "";
            record.HtmlLetter ??= "";

            try
            {
                if (record.Id == 0)
                    context.Applications.Add(record);
                else
                    context.Applications.Update(record);

                await context.SaveChangesAsync();
                return record.Id;
            }
            catch (Exception ex)
            {
                // Hier kannst du den Fehler loggen, um die genaue Ursache zu sehen
                Console.WriteLine($"Fehler beim Speichern: {ex.Message}");
                throw; // Oder gib eine Fehlermeldung zurück
            }
        }

        public async Task<ApplicationRecord?> GetApplicationAsync(int id)
        {
            using var context = _dbFactory.CreateDbContext();
            return await context.Applications.FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<List<ApplicationRecord>> GetAllApplicationsAsync()
        {
            using var context = _dbFactory.CreateDbContext();
            return await context.Applications.OrderByDescending(a => a.CreatedAt).ToListAsync();
        }

        public async Task DeleteApplicationAsync(int id)
        {
            using var context = _dbFactory.CreateDbContext();
            var app = await context.Applications.FindAsync(id);
            if (app != null)
            {
                context.Applications.Remove(app);
                await context.SaveChangesAsync();
            }
        }

        public async Task<AiConfig> GetAiConfigAsync()
        {
            using var context = _dbFactory.CreateDbContext();
            var config = await context.AiSettings.FirstOrDefaultAsync();
            if (config == null)
            {
                config = new AiConfig();
                context.AiSettings.Add(config);
                await context.SaveChangesAsync();
            }
            return config;
        }

        public async Task SaveAiConfigAsync(AiConfig config)
        {
            using var context = _dbFactory.CreateDbContext();
            var existing = await context.AiSettings.FirstOrDefaultAsync();
            if (existing == null) context.AiSettings.Add(config);
            else context.Entry(existing).CurrentValues.SetValues(config);
            await context.SaveChangesAsync();
        }

        public async Task<List<HtmlTemplate>> GetAllTemplatesAsync()
        {
            using var context = _dbFactory.CreateDbContext();
            return await context.Templates.ToListAsync();
        }

        public async Task<HtmlTemplate?> GetTemplateAsync(int id)
        {
            using var context = _dbFactory.CreateDbContext();
            return await context.Templates.FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task SaveTemplateAsync(HtmlTemplate template)
        {
            using var context = _dbFactory.CreateDbContext();
            if (template.Id == 0) context.Templates.Add(template);
            else context.Templates.Update(template);
            await context.SaveChangesAsync();
        }

        public async Task DeleteTemplateAsync(int id)
        {
            using var context = _dbFactory.CreateDbContext();
            var t = await context.Templates.FindAsync(id);
            if (t != null)
            {
                context.Templates.Remove(t);
                await context.SaveChangesAsync();
            }
        }
    }
}