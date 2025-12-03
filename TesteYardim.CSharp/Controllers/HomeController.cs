using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using TesteYardim.CSharp.Models;

namespace TesteYardim.CSharp.Controllers;

/// <summary>
/// Controlador principal da aplicação JSON2CSV.
/// Fornece a interface de conversão de JSON para CSV, carregamento de exemplo e páginas auxiliares.
/// </summary>
public class HomeController : Controller
{
    private readonly IWebHostEnvironment _env;

    /// <summary>
    /// Inicializa uma nova instância do <see cref="HomeController"/>.
    /// </summary>
    /// <param name="env">O ambiente de hospedagem da aplicação, usado para acessar o caminho raiz do projeto.</param>
    public HomeController(IWebHostEnvironment env)
    {
        _env = env;
    }

    /// <summary>
    /// Exibe a página inicial do conversor JSON para CSV.
    /// Tenta carregar um arquivo de exemplo (Data/exemplo.json) e o passa para a view.
    /// </summary>
    /// <returns>A view da página inicial com ou sem dados de exemplo.</returns>
    public IActionResult Index()
    {
        string jsonExemplo = "";
        try
        {
            var filePath = Path.Combine(_env.ContentRootPath, "Data", "exemplo.json");
            if (System.IO.File.Exists(filePath))
            {
                jsonExemplo = System.IO.File.ReadAllText(filePath);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao carregar exemplo.json: {ex.Message}");
        }

        ViewBag.JsonExemplo = jsonExemplo;
        return View();
    }

    /// <summary>
    /// Exibe a página de política de privacidade.
    /// </summary>
    /// <returns>A view da política de privacidade.</returns>
    public IActionResult Privacy()
    {
        return View();
    }

    /// <summary>
    /// Exibe uma página de erro amigável em caso de falhas não tratadas.
    /// </summary>
    /// <returns>A view de erro com o identificador da requisição.</returns>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    /// <summary>
    /// Converte um JSON válido em formato CSV.
    /// </summary>
    /// <param name="model">O modelo de entrada contendo o JSON e o separador desejado.</param>
    /// <returns>
    /// Um conteúdo de texto com o CSV gerado (200 OK) ou uma mensagem de erro (400 Bad Request).
    /// </returns>
    /// <remarks>
    /// <para>Restrições:</para>
    /// <list type="bullet">
    /// <item>O JSON deve ser um array não vazio.</item>
    /// <item>O primeiro item do array deve ser um objeto.</item>
    /// <item>Campos com vírgulas, aspas ou quebras de linha são automaticamente escapados.</item>
    /// <item>O separador padrão é vírgula (`,`), mas pode ser personalizado.</item>
    /// </list>
    /// </remarks>
    [HttpPost]
    public IActionResult ConvertJsonToCsv([FromBody] JsonInputModel model)
    {
        if (string.IsNullOrWhiteSpace(model?.Json))
        {
            return BadRequest("JSON vazio.");
        }

        var separator = string.IsNullOrWhiteSpace(model.Separator) ? "," : model.Separator;

        try
        {
            // ✅ Validação correta: primeiro parse genérico
            using var doc = JsonDocument.Parse(model.Json);

            if (doc.RootElement.ValueKind != JsonValueKind.Array)
            {
                return BadRequest("JSON deve ser um array não vazio.");
            }

            var array = doc.RootElement.EnumerateArray().ToArray();

            if (array.Length == 0)
            {
                return BadRequest("JSON deve ser um array não vazio.");
            }

            if (array[0].ValueKind != JsonValueKind.Object)
            {
                return BadRequest("O primeiro item do array deve ser um objeto.");
            }

            var headers = array[0].EnumerateObject().Select(p => p.Name).ToArray();

            var csvLines = new List<string>
            {
                string.Join(separator, headers.Select(EscapeCsvField))
            };

            foreach (var item in array)
            {
                if (item.ValueKind != JsonValueKind.Object)
                {
                    csvLines.Add(string.Join(separator, headers.Select(_ => "")));
                    continue;
                }

                var values = headers.Select(header =>
                {
                    if (item.TryGetProperty(header, out var prop))
                    {
                        return EscapeCsvField(prop.ToString());
                    }
                    return "";
                });

                csvLines.Add(string.Join(separator, values));
            }

            return Content(string.Join("\n", csvLines), "text/plain; charset=utf-8");
        }
        catch (JsonException)
        {
            return BadRequest("JSON inválido.");
        }
        catch (Exception ex)
        {
            return BadRequest($"Erro interno: {ex.Message}");
        }
    }

    /// <summary>
    /// Escapa um campo de texto para uso seguro em CSV, conforme a RFC 4180.
    /// </summary>
    /// <param name="input">O valor do campo a ser escapado.</param>
    /// <returns>
    /// O campo original se não contiver caracteres especiais, 
    /// ou o campo envolvido em aspas duplas com aspas internas duplicadas.
    /// </returns>
    private static string EscapeCsvField(string input)
    {
        if (string.IsNullOrEmpty(input))
            return "";

        if (input.Contains('"') || input.Contains(',') || input.Contains('\n') || input.Contains('\r'))
        {
            return $"\"{input.Replace("\"", "\"\"")}\"";
        }
        return input;
    }
}