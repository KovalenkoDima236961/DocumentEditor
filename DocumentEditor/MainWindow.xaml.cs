using System.Windows;
using System.IO;
using System.Windows.Shapes;
using Microsoft.Win32;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace DocumentEditor
{
    public partial class MainWindow : Window
    {
        private List<string> selectedFiles = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void FileList_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }
        
        private void FileList_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                var invalidFiles = files.Where(file => System.IO.Path.GetExtension(file).ToLower() != ".pdf").ToList();

                if (invalidFiles.Any())
                {
                    MessageBox.Show("Only PDF files are supported.", "Invalid File", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                foreach (var file in files.Where(file => System.IO.Path.GetExtension(file).ToLower() == ".pdf"))
                {
                    if (!FileList.Items.Contains(file))
                    {
                        FileList.Items.Add(file);
                        selectedFiles.Add(file);
                    }
                }
            }
        }
        
        // Open File Event
        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                selectedFiles.AddRange(openFileDialog.FileNames);
                foreach (var file in openFileDialog.FileNames)
                {
                    FileList.Items.Add(file);
                }
            }
        }

        // Save As Event
        private void SaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                MessageBox.Show($"File saved to: {saveFileDialog.FileName}");
            }
        }

        // Merge PDFs Event
        private void MergePDFs_Click(object sender, RoutedEventArgs e)
        {
            if (selectedFiles.Count < 2)
            {
                MessageBox.Show("Please select at least two files to merge.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    PdfDocument outputDocument = new PdfDocument();

                    foreach (var file in selectedFiles)
                    {
                        PdfDocument inputDocument = PdfReader.Open(file, PdfDocumentOpenMode.Import);
                        for (int i = 0; i < inputDocument.PageCount; i++)
                        {
                            outputDocument.AddPage(inputDocument.Pages[i]);
                        }
                    }

                    outputDocument.Save(saveFileDialog.FileName);
                    MessageBox.Show($"Merged PDF saved to: {saveFileDialog.FileName}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error merging PDFs: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Extract Pages Event
        private void ExtractPages_Click(object sender, RoutedEventArgs e)
        {
            if (selectedFiles.Count != 1)
            {
                MessageBox.Show("Please select one file to extract pages from.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var file = selectedFiles[0];

            // Read the number of pages to extract from the input field
            if (!int.TryParse(PagesToExtractInput.Text, out int numberOfPages) || numberOfPages <= 0)
            {
                MessageBox.Show("Please enter a valid positive number for pages to extract.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    PdfDocument inputDocument = PdfReader.Open(file, PdfDocumentOpenMode.Import);
                    PdfDocument outputDocument = new PdfDocument();

                    // Extract the specified number of pages, ensuring we do not exceed the total page count
                    for (int i = 0; i < Math.Min(numberOfPages, inputDocument.PageCount); i++)
                    {
                        outputDocument.AddPage(inputDocument.Pages[i]);
                    }

                    outputDocument.Save(saveFileDialog.FileName);
                    MessageBox.Show($"Extracted {Math.Min(numberOfPages, inputDocument.PageCount)} pages saved to: {saveFileDialog.FileName}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error extracting pages: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Add Watermark Event
        private void AddWatermark_Click(object sender, RoutedEventArgs e)
        {
            if (selectedFiles.Count != 1)
            {
                MessageBox.Show("Please select one file to add a watermark to.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var file = selectedFiles[0];
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    PdfDocument inputDocument = PdfReader.Open(file, PdfDocumentOpenMode.Modify);

                    foreach (var page in inputDocument.Pages)
                    {
                        // Add a watermark (example: simple text)
                        var gfx = PdfSharp.Drawing.XGraphics.FromPdfPage(page);

                        // Set up font and brush for watermark
                        var font = new PdfSharp.Drawing.XFont("Arial", 40);
                        var brush = new PdfSharp.Drawing.XSolidBrush(PdfSharp.Drawing.XColor.FromArgb(50, PdfSharp.Drawing.XColors.Red)); // Semi-transparent red

                        // Get the page dimensions
                        var width = page.Width;
                        var height = page.Height;

                        // Position the watermark (e.g., diagonally across the page)
                        gfx.RotateTransform(-45); // Rotate the text
                        gfx.DrawString("WATERMARK", font, brush, new PdfSharp.Drawing.XPoint(width / 2, height / 2),
                            PdfSharp.Drawing.XStringFormats.Center);
                        gfx.RotateTransform(45); // Reset rotation
                    }

                    inputDocument.Save(saveFileDialog.FileName);
                    MessageBox.Show($"Watermark added and saved to: {saveFileDialog.FileName}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error adding watermark: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
