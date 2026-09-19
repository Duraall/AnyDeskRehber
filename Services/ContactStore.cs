using System.Text.Json;
using AnyDeskRehber.WinUI3.Models;

namespace AnyDeskRehber.WinUI3.Services;

public sealed class ContactStore
{
    private readonly JsonSerializerOptions _options = new() { WriteIndented = true, PropertyNameCaseInsensitive = true };
    private readonly string _filePath;
    private readonly string _backupPath;
    private readonly string _tempPath;
    private readonly string _groupsPath;
    private readonly string _groupsTempPath;

    public ContactStore()
    {
        var root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AnyDeskRehber");
        Directory.CreateDirectory(root);
        _filePath = Path.Combine(root, "address_book.json");
        _backupPath = Path.Combine(root, "address_book.backup.json");
        _tempPath = Path.Combine(root, "address_book.tmp");
        _groupsPath = Path.Combine(root, "groups.json");
        _groupsTempPath = Path.Combine(root, "groups.tmp");
    }

    public async Task<List<Contact>> LoadAsync()
    {
        var primary = await TryLoadAsync(_filePath);
        if (primary is not null) return primary;

        // If the primary file is damaged, automatically try the last known-good backup.
        var backup = await TryLoadAsync(_backupPath);
        return backup ?? new List<Contact>();
    }

    private async Task<List<Contact>?> TryLoadAsync(string path)
    {
        if (!File.Exists(path)) return null;
        try
        {
            await using var stream = File.OpenRead(path);
            return await JsonSerializer.DeserializeAsync<List<Contact>>(stream, _options) ?? new List<Contact>();
        }
        catch { return null; }
    }

    public async Task SaveAsync(IEnumerable<Contact> contacts)
    {
        // Write a complete temporary file first, then replace the live file.
        await using (var stream = new FileStream(_tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await JsonSerializer.SerializeAsync(stream, contacts, _options);
            await stream.FlushAsync();
        }

        if (File.Exists(_filePath))
            File.Copy(_filePath, _backupPath, true);

        File.Move(_tempPath, _filePath, true);
    }


    public async Task<List<string>> LoadGroupsAsync(IEnumerable<Contact> contacts)
    {
        List<string>? saved = null;
        if (File.Exists(_groupsPath))
        {
            try
            {
                await using var stream = File.OpenRead(_groupsPath);
                saved = await JsonSerializer.DeserializeAsync<List<string>>(stream, _options);
            }
            catch { }
        }

        // Migration: v1.2 and older derived groups from contacts. Import them once,
        // then keep groups independently so an empty group is not lost.
        var result = (saved ?? new List<string>())
            .Concat(contacts.Select(c => c.Group))
            .Where(g => !string.IsNullOrWhiteSpace(g))
            .Select(g => g.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(g => g, StringComparer.OrdinalIgnoreCase)
            .ToList();

        await SaveGroupsAsync(result);
        return result;
    }

    public async Task SaveGroupsAsync(IEnumerable<string> groups)
    {
        var clean = groups.Where(g => !string.IsNullOrWhiteSpace(g))
            .Select(g => g.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(g => g, StringComparer.OrdinalIgnoreCase)
            .ToList();

        await using (var stream = new FileStream(_groupsTempPath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await JsonSerializer.SerializeAsync(stream, clean, _options);
            await stream.FlushAsync();
        }
        File.Move(_groupsTempPath, _groupsPath, true);
    }

    public string FilePath => _filePath;
    public string BackupPath => _backupPath;
}
