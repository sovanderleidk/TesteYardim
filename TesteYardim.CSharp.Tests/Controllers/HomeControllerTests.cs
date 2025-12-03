using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TesteYardim.CSharp.Controllers;
using TesteYardim.CSharp.Models;
using Xunit;

namespace TesteYardim.CSharp.Tests.Controllers;

/// <summary>
/// Conjunto de testes unitários para o controlador <see cref="HomeController"/>.
/// Valida o comportamento da conversão JSON para CSV, tratamento de erros e páginas auxiliares.
/// </summary>
public class HomeControllerTests
{
    private readonly Mock<IWebHostEnvironment> _mockEnv;
    private readonly HomeController _controller;

    /// <summary>
    /// Inicializa uma nova instância da classe de teste.
    /// Configura mocks para <see cref="IWebHostEnvironment"/> e <see cref="HttpContext"/>
    /// para garantir isolamento e evitar dependências externas.
    /// </summary>
    public HomeControllerTests()
    {
        _mockEnv = new Mock<IWebHostEnvironment>();
        _mockEnv.Setup(env => env.ContentRootPath).Returns("/fake/path");
        _controller = new HomeController(_mockEnv.Object);

        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    /// <summary>
    /// Verifica se a ação <see cref="HomeController.Index"/> retorna uma <see cref="ViewResult"/>.
    /// </summary>
    [Fact]
    public void Index_ReturnsViewResult()
    {
        var result = _controller.Index();
        Assert.IsType<ViewResult>(result);
    }

    /// <summary>
    /// Verifica se a conversão falha com código 400 quando o JSON de entrada é nulo.
    /// Mensagem esperada: "JSON vazio."
    /// </summary>
    [Fact]
    public void ConvertJsonToCsv_WithNullJson_ReturnsBadRequest()
    {
        var model = new JsonInputModel { Json = null };
        var result = _controller.ConvertJsonToCsv(model);
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("JSON vazio.", badRequest.Value);
    }

    /// <summary>
    /// Verifica se a conversão falha com código 400 quando o JSON de entrada contém apenas espaços em branco.
    /// Mensagem esperada: "JSON vazio."
    /// </summary>
    [Fact]
    public void ConvertJsonToCsv_WithEmptyJson_ReturnsBadRequest()
    {
        var model = new JsonInputModel { Json = "   " };
        var result = _controller.ConvertJsonToCsv(model);
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("JSON vazio.", badRequest.Value);
    }

    /// <summary>
    /// Verifica se a conversão falha com código 400 quando o JSON de entrada é sintaticamente inválido.
    /// Mensagem esperada: "JSON inválido."
    /// </summary>
    [Fact]
    public void ConvertJsonToCsv_WithInvalidJson_ReturnsBadRequest()
    {
        var model = new JsonInputModel { Json = "{ invalid json }" };
        var result = _controller.ConvertJsonToCsv(model);
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("JSON inválido.", badRequest.Value);
    }

    /// <summary>
    /// Verifica se a conversão falha com código 400 quando o JSON de entrada não é um array.
    /// Mensagem esperada: "JSON deve ser um array não vazio."
    /// </summary>
    [Fact]
    public void ConvertJsonToCsv_WithNonArrayJson_ReturnsBadRequest()
    {
        var model = new JsonInputModel { Json = "{ \"key\": \"value\" }" };
        var result = _controller.ConvertJsonToCsv(model);
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("JSON deve ser um array não vazio.", badRequest.Value);
    }

    /// <summary>
    /// Verifica se a conversão falha com código 400 quando o JSON de entrada é um array vazio.
    /// Mensagem esperada: "JSON deve ser um array não vazio."
    /// </summary>
    [Fact]
    public void ConvertJsonToCsv_WithEmptyArray_ReturnsBadRequest()
    {
        var model = new JsonInputModel { Json = "[]" };
        var result = _controller.ConvertJsonToCsv(model);
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("JSON deve ser um array não vazio.", badRequest.Value);
    }

    /// <summary>
    /// Verifica se a conversão falha com código 400 quando o primeiro item do array JSON não é um objeto.
    /// Mensagem esperada: "O primeiro item do array deve ser um objeto."
    /// </summary>
    [Fact]
    public void ConvertJsonToCsv_WithNonObjectFirstItem_ReturnsBadRequest()
    {
        var model = new JsonInputModel { Json = "[\"string\", {\"id\":1}]" };
        var result = _controller.ConvertJsonToCsv(model);
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("O primeiro item do array deve ser um objeto.", badRequest.Value);
    }

    /// <summary>
    /// Verifica se a conversão de um JSON válido gera um CSV correto sem aspas em campos simples,
    /// conforme a RFC 4180. Espaços no final do valor são preservados.
    /// </summary>
    [Fact]
    public void ConvertJsonToCsv_ValidJson_ReturnsCsvContent()
    {
        var json = """
            [
              {
                "id": 1,
                "nome": "Produto A",
                "linkImagem": "https://exemplo.com/img.jpg  "
              }
            ]
            """;
        var model = new JsonInputModel { Json = json };
        var result = _controller.ConvertJsonToCsv(model);
        var contentResult = Assert.IsType<ContentResult>(result);
        Assert.Equal("text/plain; charset=utf-8", contentResult.ContentType);
        Assert.Equal("id,nome,linkImagem\n1,Produto A,https://exemplo.com/img.jpg  ", contentResult.Content);
    }

    /// <summary>
    /// Verifica se a conversão respeita o separador personalizado (ex: ponto e vírgula) e não adiciona
    /// aspas em campos que não contêm delimitadores.
    /// </summary>
    [Fact]
    public void ConvertJsonToCsv_ValidJson_WithCustomSeparator_ReturnsCsvWithSemicolon()
    {
        var json = """
            [
              { "id": 1, "nome": "Produto" }
            ]
            """;
        var model = new JsonInputModel { Json = json, Separator = ";" };
        var result = _controller.ConvertJsonToCsv(model);
        var contentResult = Assert.IsType<ContentResult>(result);
        Assert.Equal("id;nome\n1;Produto", contentResult.Content);
    }

    /// <summary>
    /// Verifica se campos contendo vírgulas e aspas são corretamente escapados com aspas duplas,
    /// conforme a RFC 4180.
    /// </summary>
    [Fact]
    public void ConvertJsonToCsv_FieldWithCommaAndQuotes_EscapesCorrectly()
    {
        var json = """
            [
              { "descricao": "Item com \"aspas\" e vírgula, ok?" }
            ]
            """;
        var model = new JsonInputModel { Json = json };
        var result = _controller.ConvertJsonToCsv(model);
        var contentResult = Assert.IsType<ContentResult>(result);
        Assert.Contains("\"Item com \"\"aspas\"\" e vírgula, ok?\"", contentResult.Content);
    }

    /// <summary>
    /// Verifica se a ação <see cref="HomeController.Privacy"/> retorna uma <see cref="ViewResult"/>.
    /// </summary>
    [Fact]
    public void Privacy_ReturnsViewResult()
    {
        var result = _controller.Privacy();
        Assert.IsType<ViewResult>(result);
    }

    /// <summary>
    /// Verifica se a ação <see cref="HomeController.Error"/> retorna uma <see cref="ViewResult"/>
    /// sem lançar exceção, mesmo em contexto de teste unitário.
    /// </summary>
    [Fact]
    public void Error_ReturnsViewResult()
    {
        var result = _controller.Error();
        Assert.IsType<ViewResult>(result);
    }
}