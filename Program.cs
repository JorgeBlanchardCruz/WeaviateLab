using System.Drawing;
using System.Text;
using System.Text.Json;

namespace WeaviateClient;

//Debes iniciar el servidor Weaviate antes de ejecutar este código
//Es contenedor de Docker, puedes iniciar el servidor con el siguiente comando:
//docker run -d --name weaviate -p 8080:8080 -e QUERY_DEFAULTS=vector_index -e AUTHENTICATION_ANONYMOUS_ACCESS_ENABLED=true semitechnologies/weaviate:latest
//Tambien puedes usar la imagen de Docker Compose
//docker-compose up -d

class Program
{
    static async Task Main(string[] args)
    {
        using var client = new HttpClient
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
                case '4':
                    await DeleteAllDocuments(client);
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

    private static async Task DeleteAllDocuments(HttpClient client)
    {
        try
        {
            // 1. Obtener los IDs de todos los objetos de la clase Document
            var getIdsQuery = new
            {
                query = @"
                {
                  Get {
                    Document {
                      _additional {
                        id
                      }
                    }
                  }
                }"
            };

            // Enviar la consulta GraphQL
            var content = new StringContent(
                JsonSerializer.Serialize(getIdsQuery),
                Encoding.UTF8,
                "application/json"
            );

            var response = await client.PostAsync("/v1/graphql", content);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error al obtener IDs: {await response.Content.ReadAsStringAsync()}");
                return;
            }

            // 2. Procesar la respuesta para extraer los IDs
            var responseJson = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(responseJson);
            var ids = new List<string>();

            var documentsElement = jsonDoc.RootElement
                .GetProperty("data")
                .GetProperty("Get")
                .GetProperty("Document");

            foreach (var doc in documentsElement.EnumerateArray())
            {
                string id = doc.GetProperty("_additional").GetProperty("id").GetString();
                ids.Add(id);
            }

            Console.WriteLine($"Se encontraron {ids.Count} documentos para eliminar");

            // 3. Eliminar cada documento por su ID
            int eliminados = 0;
            foreach (var id in ids)
            {
                var deleteResponse = await client.DeleteAsync($"/v1/objects/{id}");

                if (deleteResponse.IsSuccessStatusCode)
                {
                    eliminados++;
                }
                else
                {
                    Console.WriteLine($"Error al eliminar objeto {id}: {await deleteResponse.Content.ReadAsStringAsync()}");
                }
            }

            Console.WriteLine($"Se eliminaron {eliminados} documentos de {ids.Count} encontrados");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
