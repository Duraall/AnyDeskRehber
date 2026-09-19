using System.Collections.ObjectModel;
using AnyDeskRehber.WinUI3.Models;
using AnyDeskRehber.WinUI3.Services;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI;

namespace AnyDeskRehber.WinUI3;

public sealed partial class MainWindow : Window
{
    private readonly ContactStore _store = new();
    private readonly AnyDeskService _anyDesk = new();
    private readonly ObservableCollection<Contact> _contacts = new();
    private readonly ObservableCollection<string> _groups = new();
    private string? _selectedGroup;
    private bool _favoritesOnly;
    private bool _dark = true;
    private CancellationTokenSource? _toastCts;

    public MainWindow()
    {
        InitializeComponent();
        Title = "AnyDesk Rehber";
        ContactsRepeater.ItemsSource = _contacts;
        ApplyTheme();
        Activated += MainWindow_Activated;
    }

    private async void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
        Activated -= MainWindow_Activated;
        foreach (var item in await _store.LoadAsync())
        {
            if (string.IsNullOrWhiteSpace(item.Status)) item.Status = "unknown";
            _contacts.Add(item);
        }
        foreach (var group in await _store.LoadGroupsAsync(_contacts))
            _groups.Add(group);
        RefreshUi();
    }

    private IEnumerable<Contact> FilteredContacts()
    {
        var q = SearchBox.Text.Trim();
        return _contacts.Where(c =>
            (!_favoritesOnly || c.Favorite) &&
            (_selectedGroup is null || string.Equals(c.Group, _selectedGroup, StringComparison.OrdinalIgnoreCase)) &&
            (string.IsNullOrWhiteSpace(q) ||
             c.Name.Contains(q, StringComparison.OrdinalIgnoreCase) ||
             c.Address.Contains(q, StringComparison.OrdinalIgnoreCase) ||
             c.Group.Contains(q, StringComparison.OrdinalIgnoreCase) ||
             c.Note.Contains(q, StringComparison.OrdinalIgnoreCase)));
    }

    private void RefreshUi()
    {
        ContactsRepeater.ItemsSource = FilteredContacts().ToList();
        AllCountText.Text = _contacts.Count.ToString();
        FavoriteCountText.Text = _contacts.Count(c => c.Favorite).ToString();

        PageTitle.Text = _favoritesOnly ? "Favoriler" : (_selectedGroup ?? "Bilgisayarlar");
        PageIcon.Text = _favoritesOnly ? "★" : (_selectedGroup is null ? "▣" : "▦");
        PageSubtitle.Text = _favoritesOnly
            ? "Sık kullandığın uzak bilgisayarlar."
            : _selectedGroup is null
                ? "Kayıtlı uzak bilgisayarlarını hızlıca yönet."
                : $"{_selectedGroup} grubundaki bilgisayarlar.";

        var active = (Brush)Application.Current.Resources["SidebarActiveBrush"];
        var transparent = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
        AllButton.Background = (!_favoritesOnly && _selectedGroup is null) ? active : transparent;
        FavoritesButton.Background = _favoritesOnly ? active : transparent;
        AllActiveIndicator.Visibility = (!_favoritesOnly && _selectedGroup is null) ? Visibility.Visible : Visibility.Collapsed;
        FavoritesActiveIndicator.Visibility = _favoritesOnly ? Visibility.Visible : Visibility.Collapsed;
        BuildGroups();
    }

    private void BuildGroups()
    {
        GroupPanel.Children.Clear();
        foreach (var group in _groups.OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
        {
            var button = new Button
            {
                Style = (Style)Application.Current.Resources["SidebarButtonStyle"],
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Background = string.Equals(_selectedGroup, group, StringComparison.OrdinalIgnoreCase)
                    ? (Brush)Application.Current.Resources["SidebarActiveBrush"]
                    : new SolidColorBrush(Colors.Transparent),
                Tag = group,
                Content = BuildGroupContent(group)
            };
            button.Click += GroupButton_Click;
            button.ContextFlyout = BuildGroupMenu(group);
            GroupPanel.Children.Add(button);
        }
    }

    private MenuFlyout BuildGroupMenu(string group)
    {
        var menu = new MenuFlyout();
        var rename = new MenuFlyoutItem { Text = "Grubu yeniden adlandır", Tag = group };
        rename.Click += RenameGroup_Click;
        var delete = new MenuFlyoutItem { Text = "Grubu sil", Tag = group };
        delete.Click += DeleteGroup_Click;
        menu.Items.Add(rename);
        menu.Items.Add(delete);
        return menu;
    }

    private async void RenameGroup_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuFlyoutItem { Tag: string oldName }) return;
        var input = new TextBox { Text = oldName, Header = "Grup adı", Width = 360, SelectionStart = oldName.Length };
        var dialog = new ContentDialog
        {
            Title = "Grubu yeniden adlandır", Content = input, PrimaryButtonText = "Kaydet",
            CloseButtonText = "İptal", DefaultButton = ContentDialogButton.Primary, XamlRoot = RootGrid.XamlRoot
        };
        if (await dialog.ShowAsync() != ContentDialogResult.Primary) return;
        var newName = input.Text.Trim();
        if (string.IsNullOrWhiteSpace(newName) || string.Equals(newName, oldName, StringComparison.OrdinalIgnoreCase)) return;
        if (_groups.Any(g => string.Equals(g, newName, StringComparison.OrdinalIgnoreCase)))
        {
            await ShowMessageAsync("Grup zaten var", $"‘{newName}’ adında bir grup zaten mevcut.");
            return;
        }
        var index = _groups.IndexOf(oldName);
        if (index >= 0) _groups[index] = newName;
        foreach (var contact in _contacts.Where(c => string.Equals(c.Group, oldName, StringComparison.OrdinalIgnoreCase)))
            contact.Group = newName;
        if (string.Equals(_selectedGroup, oldName, StringComparison.OrdinalIgnoreCase)) _selectedGroup = newName;
        await _store.SaveAsync(_contacts);
        await _store.SaveGroupsAsync(_groups);
        RefreshUi();
    }

    private async void DeleteGroup_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuFlyoutItem { Tag: string group }) return;
        var count = _contacts.Count(c => string.Equals(c.Group, group, StringComparison.OrdinalIgnoreCase));
        var message = count == 0
            ? "Bu boş grup kalıcı olarak silinecek."
            : $"Bu grupta {count} bilgisayar var. Grup silinecek ancak bilgisayarlar silinmeyecek; yalnızca gruptan çıkarılacak.";
        var dialog = new ContentDialog
        {
            Title = $"‘{group}’ grubu silinsin mi?", Content = new TextBlock { Text = message, TextWrapping = TextWrapping.Wrap },
            PrimaryButtonText = "Grubu sil", CloseButtonText = "İptal", DefaultButton = ContentDialogButton.Close, XamlRoot = RootGrid.XamlRoot
        };
        if (await dialog.ShowAsync() != ContentDialogResult.Primary) return;
        foreach (var contact in _contacts.Where(c => string.Equals(c.Group, group, StringComparison.OrdinalIgnoreCase)))
            contact.Group = "";
        var existing = _groups.FirstOrDefault(g => string.Equals(g, group, StringComparison.OrdinalIgnoreCase));
        if (existing is not null) _groups.Remove(existing);
        if (string.Equals(_selectedGroup, group, StringComparison.OrdinalIgnoreCase)) _selectedGroup = null;
        await _store.SaveAsync(_contacts);
        await _store.SaveGroupsAsync(_groups);
        RefreshUi();
    }

    private Grid BuildGroupContent(string group)
    {
        var grid = new Grid { Height = 44 };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(42) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(32) });

        var icon = new TextBlock
        {
            Text = "▦",
            Foreground = (Brush)Application.Current.Resources["SidebarTextBrush"],
            FontSize = 15,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(icon, 0); grid.Children.Add(icon);

        var name = new TextBlock
        {
            Text = group,
            Foreground = (Brush)Application.Current.Resources["SidebarTextBrush"],
            FontSize = 13,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(name, 1); grid.Children.Add(name);

        var count = new TextBlock
        {
            Text = _contacts.Count(c => string.Equals(c.Group, group, StringComparison.OrdinalIgnoreCase)).ToString(),
            Foreground = (Brush)Application.Current.Resources["SidebarMutedBrush"],
            FontSize = 11,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(count, 2); grid.Children.Add(count);
        return grid;
    }

    private void GroupButton_Click(object sender, RoutedEventArgs e)
    {
        _favoritesOnly = false;
        _selectedGroup = (sender as Button)?.Tag as string;
        RefreshUi();
    }

    private void AllButton_Click(object sender, RoutedEventArgs e)
    {
        _favoritesOnly = false;
        _selectedGroup = null;
        RefreshUi();
    }

    private void FavoritesButton_Click(object sender, RoutedEventArgs e)
    {
        _favoritesOnly = true;
        _selectedGroup = null;
        RefreshUi();
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs args) => RefreshUi();

    private async void AddButton_Click(object sender, RoutedEventArgs e) => await ShowContactDialogAsync(null);

    private async void Edit_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: Contact contact })
            await ShowContactDialogAsync(contact);
    }

    private async void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: Contact contact }) return;

        var dialog = new ContentDialog
        {
            Title = $"{contact.Name} silinsin mi?",
            Content = new TextBlock
            {
                Text = "Bu bilgisayar rehberden kalıcı olarak kaldırılacak. Bu işlem geri alınamaz.",
                TextWrapping = TextWrapping.Wrap
            },
            PrimaryButtonText = "Sil",
            CloseButtonText = "İptal",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = RootGrid.XamlRoot
        };

        if (await dialog.ShowAsync() != ContentDialogResult.Primary) return;
        _contacts.Remove(contact);
        await _store.SaveAsync(_contacts);
        RefreshUi();
    }

    private async void Connect_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: Contact contact }) return;
        if (!_anyDesk.Connect(contact.Address, out var error))
            await ShowMessageAsync("AnyDesk", error);
    }

    private async void Copy_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: Contact contact }) return;
        var package = new DataPackage();
        package.SetText(contact.Address);
        Clipboard.SetContent(package);
        await ShowCopyToastAsync();
    }

    private async Task ShowCopyToastAsync()
    {
        _toastCts?.Cancel();
        _toastCts = new CancellationTokenSource();
        var token = _toastCts.Token;
        CopyToast.Opacity = 1;
        try
        {
            await Task.Delay(1100, token);
            CopyToast.Opacity = 0;
        }
        catch (TaskCanceledException) { }
    }

    private async void Favorite_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: Contact contact })
        {
            contact.Favorite = !contact.Favorite;
            await _store.SaveAsync(_contacts);
            RefreshUi();
        }
    }

    private void ThemeButton_Click(object sender, RoutedEventArgs e)
    {
        _dark = !_dark;
        RootGrid.RequestedTheme = _dark ? ElementTheme.Dark : ElementTheme.Light;
        ApplyTheme();
    }

    private void ApplyTheme()
    {
        RootGrid.RequestedTheme = _dark ? ElementTheme.Dark : ElementTheme.Light;
        SetBrush("PageBackgroundBrush", _dark ? "#111827" : "#F8FAFD");
        SetBrush("CardBackgroundBrush", _dark ? "#1B2433" : "#FFFFFF");
        SetBrush("TextPrimaryBrush", _dark ? "#F3F6FB" : "#162033");
        SetBrush("TextSecondaryBrush", _dark ? "#B7C2D3" : "#5E6B7D");
        SetBrush("MutedBrush", _dark ? "#8290A5" : "#7A8798");
        SetBrush("BorderBrush", _dark ? "#2B3749" : "#DCE2EA");
        SetBrush("AccentBrush", _dark ? "#4A90FF" : "#1769E0");
        SetBrush("AccentSoftBrush", _dark ? "#1B355A" : "#E5F0FF");
        SetBrush("SidebarBrush", _dark ? "#0B172A" : "#E7EDF6");
        SetBrush("SidebarActiveBrush", _dark ? "#1A3457" : "#DDEBFF");
        SetBrush("SidebarTextBrush", _dark ? "#F4F7FC" : "#1C2A3A");
        SetBrush("SidebarMutedBrush", _dark ? "#A9BCD9" : "#5F7188");
        SetBrush("SidebarSectionBrush", _dark ? "#7690B4" : "#64748B");
        SetBrush("SidebarDividerBrush", _dark ? "#294567" : "#C8D2E1");
        SetBrush("ToastBrush", _dark ? "#243247" : "#25344A");
        ThemeIcon.Text = _dark ? "☼" : "☾";
    }

    private static void SetBrush(string key, string hex)
    {
        if (Application.Current.Resources[key] is SolidColorBrush brush)
            brush.Color = ParseHex(hex);
    }

    private static Color ParseHex(string hex)
    {
        hex = hex.TrimStart('#');
        byte r = Convert.ToByte(hex.Substring(0, 2), 16);
        byte g = Convert.ToByte(hex.Substring(2, 2), 16);
        byte b = Convert.ToByte(hex.Substring(4, 2), 16);
        return Color.FromArgb(255, r, g, b);
    }

    private async Task ShowContactDialogAsync(Contact? editing)
    {
        var isEdit = editing is not null;
        var name = new TextBox { Header = "Bilgisayar adı", PlaceholderText = "Örn. Muhasebe PC", Text = editing?.Name ?? "" };
        var address = new TextBox { Header = "AnyDesk ID / Alias", PlaceholderText = "Örn. 123 456 789", Text = editing?.Address ?? "" };

        var groups = _groups.OrderBy(g => g, StringComparer.OrdinalIgnoreCase).ToList();
        var group = new ComboBox { Header = "Grup", PlaceholderText = "Listeden grup seç", Width = 420 };
        foreach (var g in groups) group.Items.Add(g);
        group.Items.Add("＋ Yeni grup...");
        if (!string.IsNullOrWhiteSpace(editing?.Group))
            group.SelectedItem = groups.FirstOrDefault(g => string.Equals(g, editing.Group, StringComparison.OrdinalIgnoreCase)) ?? editing.Group;

        var newGroup = new TextBox { Header = "Yeni grup adı", PlaceholderText = "Örn. Yönetim", Width = 420, Visibility = Visibility.Collapsed };
        group.SelectionChanged += (_, _) =>
        {
            newGroup.Visibility = group.SelectedItem?.ToString() == "＋ Yeni grup..." ? Visibility.Visible : Visibility.Collapsed;
        };

        var status = new ComboBox { Header = "Durum", Width = 420 };
        status.Items.Add("Bilinmiyor");
        status.Items.Add("Online");
        status.Items.Add("Offline");
        status.SelectedIndex = editing?.Status switch
        {
            "online" => 1,
            "offline" => 2,
            _ => 0
        };

        var note = new TextBox { Header = "Not", PlaceholderText = "Kısa açıklama...", Text = editing?.Note ?? "", AcceptsReturn = true, Height = 90, TextWrapping = TextWrapping.Wrap };
        var favorite = new CheckBox { Content = "Favorilere ekle", IsChecked = editing?.Favorite ?? false };

        var panel = new StackPanel { Spacing = 12, Width = 420 };
        panel.Children.Add(name);
        panel.Children.Add(address);
        panel.Children.Add(group);
        panel.Children.Add(newGroup);
        panel.Children.Add(status);
        panel.Children.Add(note);
        panel.Children.Add(favorite);

        var dialog = new ContentDialog
        {
            Title = isEdit ? "Bilgisayarı düzenle" : "Yeni bilgisayar",
            Content = new ScrollViewer { Content = panel, MaxHeight = 560 },
            PrimaryButtonText = isEdit ? "Kaydet" : "Ekle",
            CloseButtonText = "Vazgeç",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = RootGrid.XamlRoot
        };

        if (await dialog.ShowAsync() != ContentDialogResult.Primary) return;
        if (string.IsNullOrWhiteSpace(name.Text) || string.IsNullOrWhiteSpace(address.Text))
        {
            await ShowMessageAsync("Eksik bilgi", "Bilgisayar adı ve AnyDesk ID / Alias alanları zorunlu.");
            return;
        }

        var normalizedAddress = NormalizeAddress(address.Text);
        var duplicate = _contacts.FirstOrDefault(c => !ReferenceEquals(c, editing) && NormalizeAddress(c.Address) == normalizedAddress);
        if (duplicate is not null)
        {
            await ShowMessageAsync("Kayıt zaten var", $"Bu AnyDesk ID / Alias zaten ‘{duplicate.Name}’ kaydında kullanılıyor.");
            return;
        }

        var selectedGroup = group.SelectedItem?.ToString() ?? "";
        var groupName = selectedGroup == "＋ Yeni grup..." ? newGroup.Text.Trim() : selectedGroup.Trim();
        if (!string.IsNullOrWhiteSpace(groupName) && !_groups.Any(g => string.Equals(g, groupName, StringComparison.OrdinalIgnoreCase)))
            _groups.Add(groupName);
        var statusValue = status.SelectedIndex switch
        {
            1 => "online",
            2 => "offline",
            _ => "unknown"
        };

        if (editing is null)
        {
            _contacts.Add(new Contact
            {
                Name = name.Text.Trim(),
                Address = address.Text.Trim(),
                Group = groupName,
                Note = note.Text.Trim(),
                Favorite = favorite.IsChecked == true,
                Status = statusValue
            });
        }
        else
        {
            editing.Name = name.Text.Trim();
            editing.Address = address.Text.Trim();
            editing.Group = groupName;
            editing.Note = note.Text.Trim();
            editing.Favorite = favorite.IsChecked == true;
            editing.Status = statusValue;
        }

        await _store.SaveAsync(_contacts);
        await _store.SaveGroupsAsync(_groups);
        RefreshUi();
    }

    private static string NormalizeAddress(string value)
    {
        value = value.Trim();
        // Numeric AnyDesk IDs are commonly written with spaces or hyphens.
        if (value.All(ch => char.IsDigit(ch) || char.IsWhiteSpace(ch) || ch == '-'))
            return new string(value.Where(char.IsDigit).ToArray());
        return value.ToLowerInvariant();
    }

    private async Task ShowMessageAsync(string title, string message)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = new TextBlock { Text = message, TextWrapping = TextWrapping.Wrap },
            CloseButtonText = "Tamam",
            XamlRoot = RootGrid.XamlRoot
        };
        await dialog.ShowAsync();
    }
}

public sealed class FavoriteVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var favorite = value is bool b && b;
        var invert = string.Equals(parameter?.ToString(), "Invert", StringComparison.OrdinalIgnoreCase);
        return (favorite ^ invert) ? Visibility.Visible : Visibility.Collapsed;
    }
    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotSupportedException();
}

public sealed class StatusBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value?.ToString()?.ToLowerInvariant() switch
        {
            "online" => new SolidColorBrush(Color.FromArgb(255, 34, 181, 115)),
            "offline" => new SolidColorBrush(Color.FromArgb(255, 222, 83, 83)),
            _ => new SolidColorBrush(Color.FromArgb(255, 145, 157, 175))
        };
    }
    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotSupportedException();
}

public sealed class StatusLabelConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value?.ToString()?.ToLowerInvariant() switch
        {
            "online" => "Online",
            "offline" => "Offline",
            _ => "Bilinmiyor"
        };
    }
    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotSupportedException();
}

public sealed class StatusTextBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value?.ToString()?.ToLowerInvariant() switch
        {
            "online" => new SolidColorBrush(Color.FromArgb(255, 34, 169, 104)),
            "offline" => new SolidColorBrush(Color.FromArgb(255, 210, 92, 92)),
            _ => new SolidColorBrush(Color.FromArgb(255, 145, 157, 175))
        };
    }
    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotSupportedException();
}
