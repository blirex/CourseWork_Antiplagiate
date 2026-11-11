
using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;
using DocumentFormat.OpenXml.Packaging;
using UglyToad.PdfPig;
using SharpCompress.Archives.Zip;

namespace CourseWork_Zubarev
{
    public static class DocumentReader
    {
        public static string ReadText(string path)
        {
            switch (Path.GetExtension(path).ToLowerInvariant())
            {
                case ".txt":
                    return File.ReadAllText(path, Encoding.UTF8);

                case ".rtf":
                    return ReadRtf(path);

                case ".docx":
                    return ReadDocx(path);

                case ".doc":
                    return ReadDoc(path);

                case ".pdf":
                    return ReadPdf(path);

                case ".odt":
                    return ReadOdt(path);

                default:
                    throw new NotSupportedException(
                        "Цей формат файлу не підтримується.");
            }
        }

        private static string ReadDocx(string path)
        {
            using (WordprocessingDocument doc =
                   WordprocessingDocument.Open(path, false))
            {
                var body = doc.MainDocumentPart.Document.Body;
                return body == null ? "" : body.InnerText;
            }
        }

        private static string ReadPdf(string path)
        {
            StringBuilder text = new StringBuilder();

            using (var pdf = PdfDocument.Open(path))
            {
                foreach (var page in pdf.GetPages())
                    text.AppendLine(page.Text);
            }

            return text.ToString();
        }

        private static string ReadRtf(string path)
        {
            using (var richText = new System.Windows.Forms.RichTextBox())
            {
                richText.LoadFile(path,
                    System.Windows.Forms.RichTextBoxStreamType.RichText);

                return richText.Text;
            }
        }

        private static string ReadDoc(string path)
        {
            Type wordType = Type.GetTypeFromProgID("Word.Application");

            if (wordType == null)
            {
                throw new InvalidOperationException(
                    "Для відкриття DOC-файлів потрібно встановити Microsoft Word. " +
                    "Можна також зберегти файл у форматі DOCX.");
            }

            dynamic word = null;
            dynamic doc = null;

            try
            {
                word = Activator.CreateInstance(wordType);
                word.Visible = false;
                word.DisplayAlerts = 0;

                doc = word.Documents.Open(
                    Path.GetFullPath(path),
                    ReadOnly: true,
                    AddToRecentFiles: false);

                return (string)doc.Content.Text;
            }
            finally
            {
                if (doc != null)
                    doc.Close(false);

                if (word != null)
                    word.Quit(false);

                if (doc != null)
                    System.Runtime.InteropServices.Marshal
                        .ReleaseComObject(doc);

                if (word != null)
                    System.Runtime.InteropServices.Marshal
                        .ReleaseComObject(word);
            }
        }


        private static string ReadOdt(string path)
        {
            using (var file = System.IO.File.OpenRead(path))
            using (var archive = new System.IO.Compression.ZipArchive(
                file, System.IO.Compression.ZipArchiveMode.Read))
            {
                var entry = archive.GetEntry("content.xml");

                if (entry == null)
                    throw new InvalidDataException(
                        "В файле ODT не найден content.xml.");

                using (var stream = entry.Open())
                {
                    var xml = System.Xml.Linq.XDocument.Load(stream);

                    return string.Join(
                        Environment.NewLine,
                        xml.Descendants()
                           .Where(e =>
                               e.Name.LocalName == "p" ||
                               e.Name.LocalName == "h")
                           .Select(e => e.Value));
                }
            }
        }

    }
}
