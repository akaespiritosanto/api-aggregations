namespace api_aggregations.Services;

using System.IO.Compression;
using System.Xml.Linq;

public sealed partial class RelatorioExcelExportService
{
    private static void CorrigirCelulasUnidas(string caminhoFicheiro, int totalColunas)
    {
        // A .xlsx file is a zip file with XML files inside it.
        // We only edit the worksheet XML and leave the rest of the workbook alone.
        using var arquivoExcel = ZipFile.Open(caminhoFicheiro, ZipArchiveMode.Update);
        var ficheiroFolha = arquivoExcel.GetEntry("xl/worksheets/sheet1.xml");

        if (ficheiroFolha == null)
        {
            return;
        }

        var folha = LerXmlDaFolha(ficheiroFolha);
        XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        var worksheet = folha.Root;

        if (worksheet == null)
        {
            return;
        }

        TrocarMergeCells(worksheet, ns, totalColunas);
        RegravarFolha(arquivoExcel, folha);
    }

    private static XDocument LerXmlDaFolha(ZipArchiveEntry ficheiroFolha)
    {
        using var stream = ficheiroFolha.Open();
        return XDocument.Load(stream);
    }

    private static void TrocarMergeCells(XElement worksheet, XNamespace ns, int totalColunas)
    {
        var celulasUnidas = worksheet.Element(ns + "mergeCells");
        celulasUnidas?.Remove();

        // A1:A2 is the "Mês" header. B1:last column is the "Resumo" header.
        celulasUnidas = new XElement(ns + "mergeCells", new XAttribute("count", "2"));
        celulasUnidas.Add(new XElement(ns + "mergeCell", new XAttribute("ref", "A1:A2")));
        celulasUnidas.Add(new XElement(ns + "mergeCell", new XAttribute("ref", $"B1:{ObterLetraColuna(totalColunas)}1")));

        var pageMargins = worksheet.Element(ns + "pageMargins");

        if (pageMargins == null)
        {
            worksheet.Add(celulasUnidas);
            return;
        }

        pageMargins.AddBeforeSelf(celulasUnidas);
    }

    private static void RegravarFolha(ZipArchive arquivoExcel, XDocument folha)
    {
        var ficheiroFolha = arquivoExcel.GetEntry("xl/worksheets/sheet1.xml");
        ficheiroFolha?.Delete();

        ficheiroFolha = arquivoExcel.CreateEntry("xl/worksheets/sheet1.xml");

        using var outputStream = ficheiroFolha.Open();
        folha.Save(outputStream);
    }

    private static string ObterLetraColuna(int numeroColuna)
    {
        // Converts 1, 2, 27 into A, B, AA. Excel uses letters in merge references.
        var letras = string.Empty;

        while (numeroColuna > 0)
        {
            var resto = (numeroColuna - 1) % 26;
            letras = (char)('A' + resto) + letras;
            numeroColuna = (numeroColuna - 1) / 26;
        }

        return letras;
    }
}
