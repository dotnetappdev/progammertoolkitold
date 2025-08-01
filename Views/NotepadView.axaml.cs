using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using AvaloniaEdit;
using ProgrammersToolKit.Database;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace ProgrammersToolKit.Views
{
    public partial class NotepadView : UserControl
    {
        private ComboBox? _notesCombo;
        private TextEditor? _noteEditor;
        private TextBox? _macroBox;
        private Button? _newNoteBtn;
        private Button? _saveNoteBtn;
        private Button? _deleteNoteBtn;
        private Button? _runMacroBtn;
        private TextBlock? _statusBlock;
        private NotepadDbContext _db = new NotepadDbContext();
        private ObservableCollection<NoteEntity> _notes = new();
        private NoteEntity? _currentNote;
        private const int Iterations = 600000;
        private const int KeySize = 32; // 256 bits

        public NotepadView()
        {
            AvaloniaXamlLoader.Load(this);
            _notesCombo = this.FindControl<ComboBox>("NotesCombo");
            _noteEditor = this.FindControl<TextEditor>("NoteEditor");
            _macroBox = this.FindControl<TextBox>("MacroBox");
            _newNoteBtn = this.FindControl<Button>("NewNoteBtn");
            _saveNoteBtn = this.FindControl<Button>("SaveNoteBtn");
            _deleteNoteBtn = this.FindControl<Button>("DeleteNoteBtn");
            _runMacroBtn = this.FindControl<Button>("RunMacroBtn");
            _statusBlock = this.FindControl<TextBlock>("StatusBlock");

            if (_newNoteBtn != null) _newNoteBtn.Click += (s, e) => NewNote();
            if (_saveNoteBtn != null) _saveNoteBtn.Click += (s, e) => SaveNote();
            if (_deleteNoteBtn != null) _deleteNoteBtn.Click += (s, e) => DeleteNote();
            if (_runMacroBtn != null) _runMacroBtn.Click += (s, e) => RunMacro();
            if (_notesCombo != null) _notesCombo.SelectionChanged += (s, e) => LoadSelectedNote();

            _db.Database.EnsureCreated();
            LoadNotes();
        }

        private void NewNote()
        {
            _currentNote = null;
            if (_noteEditor != null) _noteEditor.Text = string.Empty;
            if (_notesCombo != null) _notesCombo.SelectedIndex = -1;
            if (_statusBlock != null) _statusBlock.Text = "New note";
        }

        private void SaveNote()
        {
            if (_noteEditor == null) return;
            var text = _noteEditor.Text ?? string.Empty;
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
            if (_statusBlock != null) _statusBlock.Text = "Note saved (encrypted)";
        }

        private void DeleteNote()
        {
            if (_currentNote == null) return;
            _db.Notes.Remove(_currentNote);
            _db.SaveChanges();
            _currentNote = null;
            LoadNotes();
            if (_noteEditor != null) _noteEditor.Text = string.Empty;
            if (_statusBlock != null) _statusBlock.Text = "Note deleted";
        }

        private void LoadNotes()
        {
            _notes.Clear();
            foreach (var n in _db.Notes.ToList())
                _notes.Add(n);
            if (_notesCombo != null)
            {
                _notesCombo.ItemsSource = _notes;
                _notesCombo.SelectedIndex = -1;
            }
        }

        private void LoadSelectedNote()
        {
            if (_notesCombo?.SelectedItem is NoteEntity note)
            {
                _currentNote = note;
                // Prompt for password (for demo, use static password)
                var password = "SuperSecretPassword!";
                var decrypted = Decrypt(note.EncryptedContent, password, note.Salt, note.IV);
                if (_noteEditor != null) _noteEditor.Text = decrypted;
                if (_statusBlock != null) _statusBlock.Text = $"Loaded: {note.Title}";
            }
        }

        private void RunMacro()
        {
            if (_noteEditor == null || _macroBox == null) return;
            var code = _macroBox.Text ?? string.Empty;
            var text = _noteEditor.Text ?? string.Empty;
            // For demo: simple macros (replace, upper, lower)
            if (code.StartsWith("replace:", StringComparison.OrdinalIgnoreCase))
            {
                var parts = code.Substring(8).Split("=>");
                if (parts.Length == 2)
                    _noteEditor.Text = text.Replace(parts[0], parts[1]);
            }
            else if (code.Equals("upper", StringComparison.OrdinalIgnoreCase))
                _noteEditor.Text = text.ToUpper();
            else if (code.Equals("lower", StringComparison.OrdinalIgnoreCase))
                _noteEditor.Text = text.ToLower();
            else if (code.Equals("reverse", StringComparison.OrdinalIgnoreCase))
                _noteEditor.Text = new string(text.Reverse().ToArray());
            if (_statusBlock != null) _statusBlock.Text = "Macro applied";
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
