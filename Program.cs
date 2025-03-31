using System.Drawing;
using System.Text;

namespace WeaviateClient
{
    class Program
    {
        //Debes iniciar el servidor Weaviate antes de ejecutar este código
        //Es contenedor de Docker, puedes iniciar el servidor con el siguiente comando:
        //docker run -d --name weaviate -p 8080:8080 -e QUERY_DEFAULTS=vector_index -e AUTHENTICATION_ANONYMOUS_ACCESS_ENABLED=true semitechnologies/weaviate:latest
        //Tambien puedes usar la imagen de Docker Compose
        //docker-compose up -d

        static async Task Main(string[] args)
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:8080") // Dirección de tu servidor Weaviate
            };

            var userKeyPress = new ConsoleKeyInfo();

            while (userKeyPress.KeyChar != 'e')
            {
                userKeyPress = Console.ReadKey();

                switch (userKeyPress.KeyChar)
                {
                    case '0':
                        await CreateSchema(client);
                        break;
                    case '1':
                        await CreateDocument(client);
                        break;
                    case '2':
                        await CreateDocumentWithVector(client);
                        break;
                    case '3':
                        await ShowDocuments(client);
                        break;
                    default:
                        Console.WriteLine("Command not found.");
                        break;
                }

            }
        }

        private static async Task CreateSchema(HttpClient client)
        {
            var schemaJson = """       
            {
              "class": "Document",
              "description": "Stores text documents with embeddings",
              "vectorizer": "none",
              "properties": [
                {
                  "name": "text",
                  "dataType": ["text"]
                },
                {
                  "name": "title",
                  "dataType": ["string"]
                }
              ]
            }
            """;

            var content = new StringContent(schemaJson, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("/v1/schema", content);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Schema created successfully", Color.Azure);
            }
            else
            {
                Console.WriteLine($"Error: {response.StatusCode}", Color.Purple);
            }
        }

        private static async Task CreateDocument(HttpClient client)
        {
            var articleJson = """
            {
                "class": "Document",
                "properties": {
                    "title": "Artículo de prueba",
                    "content": "Este es el contenido del artículo de prueba."
                }
            }
            """;

            var content = new StringContent(articleJson, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/v1/objects", content);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Document created successfully", Color.Azure);
            }
            else
            {
                Console.WriteLine($"Error: {response.StatusCode}", Color.Purple);
            }
        }

        private static async Task CreateDocumentWithVector(HttpClient client)
        {
            var articleJson = """
            {
                "class": "Document",
                "properties": {
                    "title": "Artículo de prueba",
                    "content": "Este es el contenido del artículo de prueba.",
                    "vector": [0.1, 0.2, 0.3, 0.4, 0.5]
                }
            }
            """;
            var content = new StringContent(articleJson, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("/v1/objects", content);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Document created successfully", Color.Azure);
            }
            else
            {
                Console.WriteLine($"Error: {response.StatusCode}", Color.Purple);
            }
        }

        private static async Task ShowDocuments(HttpClient client)
        {
            var response = await client.GetAsync("/v1/objects");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine(content);
            }
            else
            {
                Console.WriteLine($"Error: {response.StatusCode}", Color.Purple);
            }
        }
    }

}
