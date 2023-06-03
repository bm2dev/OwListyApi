# OwListyAPI (Em Desenvolvimento / In Development)

A OwListy API é o backend do app de listas compartilhadas. Ela possui com rotas protegidas e autenticação com JWT. A API é desenvolvida em C# utilizando o framework ASP.NET Core e o Entity Framework Core como ORM para acesso aos dados.

## Autenticação e Autorização

A OwListy API suporta autenticação e autorização para proteger os recursos da API. Ela utiliza o esquema de autenticação JWT (JSON Web Token) para autenticar os usuários e controlar o acesso aos endpoints. Para obter um token de acesso, é necessário fazer uma solicitação de autenticação fornecendo as credenciais de usuário. O token de acesso retornado deve ser incluído no cabeçalho `Authorization` de todas as solicitações subsequentes.

## Configuração

Para executar a API do OwListy, siga as etapas abaixo:

1. Certifique-se de ter o [.NET Core SDK](https://dotnet.microsoft.com/download) instalado em sua máquina.

2. Clone o repositório do OwListy API para o seu ambiente local.

   ```bash
   git clone https://github.com/bm2dev/OwListyApi.git
   ```

3. Acesse o diretório do projeto.

   ```bash
   cd OwListyApi
   ```

4. Abra o arquivo `Settings.example.cs` apaque o `.example` e descomente o código e configure a sua "ConnectionString" com o banco de dados e defina um "Token" para a criptografia do JWT.

5. Execute as migrações para criar o banco de dados e as tabelas necessárias.

   ```bash
   dotnet ef database update
   ```

6. Inicie a aplicação.

   ```bash
   dotnet run
   ```

7. A API do OwListy estará disponível em `https://localhost:7213` (ou em outra porta, caso especificada).

## Suporte

Se você tiver alguma dúvida, sugestão ou encontrar algum problema com a OwListy API, fique à vontade para abrir uma issue no repositório ou entrar em contato com comigo.
