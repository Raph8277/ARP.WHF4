using Microsoft.Win32;
using System.IO;
using System.Windows;

namespace MdTools;

public partial class MainWindow : Window
{
    private string? _pdfPath;
    private string? _markdown;

    public MainWindow() => InitializeComponent();

    // ── Drag & Drop ──────────────────────────────────────────────────────────

    private void Window_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop)
            ? DragDropEffects.Copy
            : DragDropEffects.None;
        e.Handled = true;
    }

    private void Window_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetData(DataFormats.FileDrop) is string[] files)
        {
            var pdf = files.FirstOrDefault(f =>
                f.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase));
            if (pdf is not null) LoadPdf(pdf);
        }
    }

    // ── Browse ────────────────────────────────────────────────────────────────

    private void BtnBrowse_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog
        {
            Title  = "Sélectionner un fichier PDF",
            Filter = "Fichiers PDF (*.pdf)|*.pdf|Tous les fichiers|*.*"
        };
        if (dlg.ShowDialog() == true) LoadPdf(dlg.FileName);
    }

    private void LoadPdf(string path)
    {
        _pdfPath = path;
        TxtPdfPath.Text = path;
        TxtPdfPath.Foreground = System.Windows.Media.Brushes.Black;
        BtnConvert.IsEnabled = true;
        TxtStatus.Text = $"PDF chargé : {Path.GetFileName(path)}";
        _markdown = null;
        BtnSave.IsEnabled = false;
        TxtPreview.Clear();
    }

    // ── Convert ───────────────────────────────────────────────────────────────

    private async void BtnConvert_Click(object sender, RoutedEventArgs e)
    {
        if (_pdfPath is null) return;

        BtnConvert.IsEnabled = false;
        BtnSave.IsEnabled    = false;
        TxtPreview.Clear();
        Progress.Value = 0;

        var progress = new Progress<(int page, int total)>(t =>
        {
            Progress.Value = (double)t.page / t.total * 100;
            TxtStatus.Text = $"Page {t.page} / {t.total}…";
        });

        try
        {
            _markdown = await PdfConverter.ConvertAsync(_pdfPath, progress);
            TxtPreview.Text = _markdown;
            TxtStatus.Text  = "Conversion terminée.";
            BtnSave.IsEnabled = true;
            Progress.Value = 100;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur : {ex.Message}", "Conversion échouée",
                MessageBoxButton.OK, MessageBoxImage.Error);
            TxtStatus.Text = "Erreur lors de la conversion.";
        }
        finally
        {
            BtnConvert.IsEnabled = true;
        }
    }

    // ── Copy / Save ───────────────────────────────────────────────────────────

    private void BtnCopy_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(TxtPreview.Text))
            Clipboard.SetText(TxtPreview.Text);
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        if (_markdown is null) return;

        var dlg = new SaveFileDialog
        {
            Title      = "Enregistrer le fichier Markdown",
            Filter     = "Fichiers Markdown (*.md)|*.md|Tous les fichiers|*.*",
            FileName   = Path.GetFileNameWithoutExtension(_pdfPath) + ".md",
            DefaultExt = ".md"
        };

        if (dlg.ShowDialog() == true)
        {
            File.WriteAllText(dlg.FileName, _markdown, System.Text.Encoding.UTF8);
            TxtStatus.Text = $"Enregistré : {dlg.FileName}";
        }
    }
}
