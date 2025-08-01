using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ProgrammersToolKit.Database;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace ProgrammersToolKit.Views
{
    public sealed partial class NotepadView : UserControl
    {
        private NotepadDbContext _db = new NotepadDbContext();
        private ObservableCollection<NoteEntity> _notes = new();
        private NoteEntity? _currentNote;
        private const int Iterations = 600000;
        private const int KeySize = 32; // 256 bits

        public NotepadView()
        {
            this.InitializeComponent();

            NewNoteBtn.Click += (s, e) => NewNote();
            SaveNoteBtn.Click += (s, e) => SaveNote();
            DeleteNoteBtn.Click += (s, e) => DeleteNote();
            RunMacroBtn.Click += (s, e) => RunMacro();
            NotesCombo.SelectionChanged += (s, e) => LoadSelectedNote();

            _db.Database.EnsureCreated();
            LoadNotes();
        }

        private void NewNote()
        {
            _currentNote = null;
            NoteEditor.Text = string.Empty;
            NotesCombo.SelectedIndex = -1;
            StatusBlock.Text = "New note";
        }

        private void SaveNote()
        {
            var text = NoteEditor.Text ?? string.Empty;
            var title = _currentNote?.Title ?? $"Note {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
            // Prompt for password (for demo, use a static password)
            var password = "SuperSecretPassword!";
            var salt = _currentNote?.Salt ?? Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
            var iv = _currentNote?.IV ?? Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
            var encrypted = Encrypt(text, password, salt, iv);
            if (_currentNote == null)
            {
                var note = new NoteEntity { Title = title, EncryptedContent = encrypted, Salt = salt, IV = iv };
                _db.Notes.Add(note);
                _currentNote = note;
            }
            else
            {
                _currentNote.EncryptedContent = encrypted;
                _currentNote.Salt = salt;
                _currentNote.IV = iv;
            }
            _db.SaveChanges();
            LoadNotes();
            StatusBlock.Text = "Note saved (encrypted)";
        }

        private void DeleteNote()
        {
            if (_currentNote == null) return;
            _db.Notes.Remove(_currentNote);
            _db.SaveChanges();
            _currentNote = null;
            LoadNotes();
            NoteEditor.Text = string.Empty;
            StatusBlock.Text = "Note deleted";
        }

        private void LoadNotes()
        {
            _notes.Clear();
            foreach (var n in _db.Notes.ToList())
                _notes.Add(n);
            NotesCombo.ItemsSource = _notes;
            NotesCombo.SelectedIndex = -1;
        }

        private void LoadSelectedNote()
        {
            if (NotesCombo.SelectedItem is NoteEntity note)
            {
                _currentNote = note;
                // Prompt for password (for demo, use static password)
                var password = "SuperSecretPassword!";
                var decrypted = Decrypt(note.EncryptedContent, password, note.Salt, note.IV);
                NoteEditor.Text = decrypted;
                StatusBlock.Text = $"Loaded: {note.Title}";
            }
        }

        private void RunMacro()
        {
            var code = MacroBox.Text ?? string.Empty;
            var text = NoteEditor.Text ?? string.Empty;
            // For demo: simple macros (replace, upper, lower)
            if (code.StartsWith("replace:", StringComparison.OrdinalIgnoreCase))
            {
                var parts = code.Substring(8).Split("=>");
                if (parts.Length == 2)
                    NoteEditor.Text = text.Replace(parts[0], parts[1]);
            }
            else if (code.Equals("upper", StringComparison.OrdinalIgnoreCase))
                NoteEditor.Text = text.ToUpper();
            else if (code.Equals("lower", StringComparison.OrdinalIgnoreCase))
                NoteEditor.Text = text.ToLower();
            else if (code.Equals("reverse", StringComparison.OrdinalIgnoreCase))
                NoteEditor.Text = new string(text.Reverse().ToArray());
            StatusBlock.Text = "Macro applied";
        }

        // AES-256 encryption with PBKDF2 (600,000 iterations)
        private string Encrypt(string plain, string password, string saltB64, string ivB64)
        {
            var salt = Convert.FromBase64String(saltB64);
            var iv = Convert.FromBase64String(ivB64);
            using var derive = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
            var key = derive.GetBytes(KeySize);
            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            using var encryptor = aes.CreateEncryptor();
            var plainBytes = Encoding.UTF8.GetBytes(plain);
            var cipher = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
            return Convert.ToBase64String(cipher);
        }

        private string Decrypt(string cipherB64, string password, string saltB64, string ivB64)
        {
            var salt = Convert.FromBase64String(saltB64);
            var iv = Convert.FromBase64String(ivB64);
            using var derive = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
            var key = derive.GetBytes(KeySize);
            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            using var decryptor = aes.CreateDecryptor();
            var cipher = Convert.FromBase64String(cipherB64);
            var plain = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);
            return Encoding.UTF8.GetString(plain);
        }
    }
}