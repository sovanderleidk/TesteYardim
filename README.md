# TesteYardim – Conversor JSON para CSV

Este projeto é uma implementação do conversor **JSON → CSV** conforme exigido no desafio técnico. A solução é **web-based**, desenvolvida em **C# com ASP.NET Core MVC**, e inclui:

- Interface moderna e responsiva com abas para conversão, exemplo e visualização em tabela
- Upload de JSON via textarea com validação em tempo real
- Escolha do separador CSV (`,` ou `;`)
- Visualização do resultado em CSV bruto **e** em tabela HTML com miniaturas das imagens
- Botão para exportar o CSV como arquivo
- Carregamento automático de um exemplo de produtos (com link de imagem válido)
- Código 100% documentado com XML Comments
- Projeto de testes unitários com xUnit e Moq (12 testes cobrindo todos os cenários)

>  **Nenhuma biblioteca de conversão externa foi usada** — toda a lógica de CSV é feita manualmente, conforme solicitado.

---

##  Estrutura do projeto
  ```
TesteYardim/
├── TesteYardim.CSharp/          # Aplicação principal (ASP.NET Core MVC)
│   ├── Controllers/HomeController.cs
│   ├── Models/JsonInputModel.cs
│   ├── Views/Home/Index.cshtml
│   ├── wwwroot/css/json2csv.css
│   └── Data/exemplo.json        # Dados carregados automaticamente
├── TesteYardim.CSharp.Tests/    # Testes unitários (xUnit + Moq)
└── Dockerfile                   # Configuração para Docker
  ```

##  Executar com Docker (recomendado)

Este projeto roda **totalmente em Docker**, sem necessidade de instalar .NET localmente.

### Construa a imagem Docker
    docker build -t teste-yardim .  

### Inicie o container   
    docker run -p 8080:8080 teste-yardim

### Para ver o projeto em execução 
    http://localhost:8080

### Pré-requisitos

- [Docker](https://www.docker.com/products/docker-desktop) instalado

### Passo a passo

1. **Clone este repositório**
   ```bash
   git clone https://github.com/seu-usuario/TesteYardim.git
   cd TesteYardim


