using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace BoilerplateCustomizer
{
    class Program
    {
        private static readonly string BoilerplatePath = GetBoilerplatePath();
        private static string? projectName;
        private static List<string> entityNames = new();
        private static bool enableMultitenancy = true;

        private static string GetBoilerplatePath()
        {
            // Primeiro, tenta no diretório atual (para quando executado da pasta Executavel)
            string currentDir = Directory.GetCurrentDirectory();
            string boilerplatePath = Path.Combine(currentDir, "Boilerplate");
            
            if (Directory.Exists(boilerplatePath))
                return boilerplatePath;
            
            // Se não encontrar, tenta no diretório pai (para quando executado da pasta publish)
            string parentDir = Directory.GetParent(currentDir)?.FullName ?? currentDir;
            boilerplatePath = Path.Combine(parentDir, "Boilerplate");
            
            if (Directory.Exists(boilerplatePath))
                return boilerplatePath;
            
            // Se ainda não encontrar, tenta dois níveis acima (publish -> Executavel -> Boilerplate)
            string grandParentDir = Directory.GetParent(parentDir)?.FullName ?? parentDir;
            boilerplatePath = Path.Combine(grandParentDir, "Boilerplate");
            
            return boilerplatePath;
        }

        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Boilerplate Customizer ===");
            Console.WriteLine();

            try
            {
                // Verificar se a pasta Boilerplate existe
                if (!Directory.Exists(BoilerplatePath))
                {
                    Console.WriteLine($"Erro: Pasta 'Boilerplate' não encontrada!");
                    Console.WriteLine($"Procurado em: {BoilerplatePath}");
                    Console.WriteLine($"Diretório atual: {Directory.GetCurrentDirectory()}");
                    Console.WriteLine();
                    Console.WriteLine("Estrutura esperada:");
                    Console.WriteLine("📁 E:\\Projetos\\Boilerplate\\");
                    Console.WriteLine("├── 📁 Boilerplate\\     # Pasta com o boilerplate");
                    Console.WriteLine("└── 📁 Executavel\\      # Pasta com o executável");
                    Console.WriteLine("    └── 📁 publish\\");
                    Console.WriteLine("        └── BoilerplateCustomizer.exe");
                    return;
                }

                // Coletar informações do usuário
                await CollectUserInput();

                // Criar nova pasta do projeto no mesmo nível das pastas Boilerplate e Executavel
                string baseDir = Directory.GetParent(BoilerplatePath)?.FullName ?? Directory.GetCurrentDirectory();
                string newProjectPath = Path.Combine(baseDir, projectName!);
                
                if (Directory.Exists(newProjectPath))
                {
                    Console.WriteLine($"Pasta '{projectName}' já existe. Deseja sobrescrever? (s/n): ");
                    var overwrite = Console.ReadLine()?.ToLower();
                    if (overwrite != "s" && overwrite != "sim")
                    {
                        Console.WriteLine("Operação cancelada.");
                        return;
                    }
                    Directory.Delete(newProjectPath, true);
                }

                // Copiar boilerplate
                Console.WriteLine("Copiando arquivos do boilerplate...");
                CopyDirectory(BoilerplatePath, newProjectPath);

                // Aplicar customizações
                Console.WriteLine("Aplicando customizações...");
                await ApplyCustomizations(newProjectPath);

                Console.WriteLine();
                Console.WriteLine($"✅ Projeto '{projectName}' criado com sucesso!");
                Console.WriteLine($"📁 Localização: {newProjectPath}");
                Console.WriteLine();
                Console.WriteLine("Próximos passos:");
                Console.WriteLine("1. Abra o projeto no Visual Studio");
                Console.WriteLine("2. Execute as migrations do Entity Framework");
                Console.WriteLine("3. Configure a string de conexão no appsettings.json");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro durante a execução: {ex.Message}");
                Console.WriteLine("Pressione qualquer tecla para sair...");
                Console.ReadKey();
            }
            finally
            {
                Console.WriteLine();
                Console.WriteLine("Pressione qualquer tecla para fechar...");
                Console.ReadKey();
            }
        }

        private static async Task CollectUserInput()
        {
            // Nome do projeto
            Console.Write("Digite o nome do projeto: ");
            projectName = Console.ReadLine()?.Trim();
            
            while (string.IsNullOrEmpty(projectName) || !IsValidProjectName(projectName))
            {
                Console.WriteLine("Nome inválido. Use apenas letras, números e underscore, começando com letra.");
                Console.Write("Digite o nome do projeto: ");
                projectName = Console.ReadLine()?.Trim();
            }

            // Número de entidades
            Console.Write("Quantas entidades iniciais deseja criar? ");
            int entityCount;
            while (!int.TryParse(Console.ReadLine(), out entityCount) || entityCount < 0)
            {
                Console.Write("Digite um número válido (0 ou maior): ");
            }

            // Nomes das entidades
            for (int i = 0; i < entityCount; i++)
            {
                Console.Write($"Digite o nome da entidade {i + 1}: ");
                string? entityName = Console.ReadLine()?.Trim();
                
                while (string.IsNullOrEmpty(entityName) || !IsValidEntityName(entityName))
                {
                    Console.WriteLine("Nome inválido. Use apenas letras, começando com maiúscula.");
                    Console.Write($"Digite o nome da entidade {i + 1}: ");
                    entityName = Console.ReadLine()?.Trim();
                }
                
                entityNames.Add(entityName);
            }

            // Multitenancy
            Console.Write("Deseja ativar multi-tenancy? (s/n): ");
            var multitenancyResponse = Console.ReadLine()?.ToLower();
            enableMultitenancy = multitenancyResponse == "s" || multitenancyResponse == "sim";
        }

        private static bool IsValidProjectName(string name)
        {
            return Regex.IsMatch(name, @"^[A-Za-z][A-Za-z0-9_]*$");
        }

        private static bool IsValidEntityName(string name)
        {
            return Regex.IsMatch(name, @"^[A-Z][A-Za-z0-9]*$");
        }

        private static void CopyDirectory(string sourceDir, string destDir)
        {
            Directory.CreateDirectory(destDir);

            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string fileName = Path.GetFileName(file);
                string destFile = Path.Combine(destDir, fileName);
                File.Copy(file, destFile, true);
            }

            foreach (string subDir in Directory.GetDirectories(sourceDir))
            {
                string dirName = Path.GetFileName(subDir);
                string destSubDir = Path.Combine(destDir, dirName);
                CopyDirectory(subDir, destSubDir);
            }
        }

        private static async Task ApplyCustomizations(string projectPath)
        {
            // 1. Substituir nomes do projeto
            await ReplaceProjectNames(projectPath);

            // 2. Gerar JWT Secret Key
            await GenerateJwtSecretKey(projectPath);

            // 3. Criar entidades baseadas em Entity1
            await CreateEntitiesFromTemplate(projectPath);

            // 4. Configurar multitenancy
            if (!enableMultitenancy)
            {
                await RemoveMultitenancy(projectPath);
            }
        }

        private static async Task ReplaceProjectNames(string projectPath)
        {
            Console.WriteLine("Substituindo nomes do projeto...");
            
            var filesToProcess = Directory.GetFiles(projectPath, "*", SearchOption.AllDirectories)
                .Where(f => !f.Contains("\\bin\\") && !f.Contains("\\obj\\"))
                .Where(f => Path.GetExtension(f).ToLower() is ".cs" or ".csproj" or ".sln" or ".json")
                .ToList();

            foreach (string filePath in filesToProcess)
            {
                try
                {
                    string content = await File.ReadAllTextAsync(filePath);
                    
                    // Substituir "Boilerplate" por nome do projeto (case sensitive)
                    content = content.Replace("Boilerplate", projectName);
                    content = content.Replace("boilerplate", projectName!.ToLower());
                    content = content.Replace("BOILERPLATE", projectName!.ToUpper());
                    
                    await File.WriteAllTextAsync(filePath, content);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Aviso: Não foi possível processar {filePath}: {ex.Message}");
                }
            }

            // Renomear diretórios
            await RenameDirectories(projectPath);
        }

        private static async Task RenameDirectories(string projectPath)
        {
            var directories = Directory.GetDirectories(projectPath, "*", SearchOption.AllDirectories)
                .Where(d => Path.GetFileName(d).Contains("Boilerplate"))
                .OrderByDescending(d => d.Length) // Processar subdiretórios primeiro
                .ToList();

            foreach (string dir in directories)
            {
                try
                {
                    string dirName = Path.GetFileName(dir);
                    string newDirName = dirName.Replace("Boilerplate", projectName);
                    string parentDir = Path.GetDirectoryName(dir)!;
                    string newDirPath = Path.Combine(parentDir, newDirName);
                    
                    if (dir != newDirPath)
                    {
                        Directory.Move(dir, newDirPath);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Aviso: Não foi possível renomear diretório {dir}: {ex.Message}");
                }
            }

            // Renomear arquivos
            var files = Directory.GetFiles(projectPath, "*", SearchOption.AllDirectories)
                .Where(f => Path.GetFileName(f).Contains("Boilerplate"))
                .ToList();

            foreach (string file in files)
            {
                try
                {
                    string fileName = Path.GetFileName(file);
                    string newFileName = fileName.Replace("Boilerplate", projectName);
                    string dir = Path.GetDirectoryName(file)!;
                    string newFilePath = Path.Combine(dir, newFileName);
                    
                    if (file != newFilePath)
                    {
                        File.Move(file, newFilePath);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Aviso: Não foi possível renomear arquivo {file}: {ex.Message}");
                }
            }
        }

        private static async Task GenerateJwtSecretKey(string projectPath)
        {
            Console.WriteLine("Gerando JWT Secret Key...");
            
            // Gerar chave de 256 bits (32 bytes)
            using var rng = RandomNumberGenerator.Create();
            byte[] keyBytes = new byte[32];
            rng.GetBytes(keyBytes);
            string secretKey = Convert.ToHexString(keyBytes).ToLower();

            // Atualizar appsettings.json
            string appsettingsPath = Path.Combine(projectPath, $"{projectName}.Api", "appsettings.json");
            
            if (File.Exists(appsettingsPath))
            {
                try
                {
                    string jsonContent = await File.ReadAllTextAsync(appsettingsPath);
                    using JsonDocument doc = JsonDocument.Parse(jsonContent);
                    
                    var options = new JsonWriterOptions { Indented = true };
                    using var stream = new MemoryStream();
                    using var writer = new Utf8JsonWriter(stream, options);
                    
                    writer.WriteStartObject();
                    
                    foreach (var property in doc.RootElement.EnumerateObject())
                    {
                        if (property.Name == "JWT")
                        {
                            writer.WriteStartObject("JWT");
                            writer.WriteString("SecretKey", secretKey);
                            
                            foreach (var jwtProperty in property.Value.EnumerateObject())
                            {
                                if (jwtProperty.Name != "SecretKey")
                                {
                                    jwtProperty.WriteTo(writer);
                                }
                            }
                            
                            writer.WriteEndObject();
                        }
                        else
                        {
                            property.WriteTo(writer);
                        }
                    }
                    
                    writer.WriteEndObject();
                    
                    string updatedJson = Encoding.UTF8.GetString(stream.ToArray());
                    await File.WriteAllTextAsync(appsettingsPath, updatedJson);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Aviso: Não foi possível atualizar JWT SecretKey: {ex.Message}");
                }
            }
        }

        private static async Task CreateEntitiesFromTemplate(string projectPath)
        {
            if (entityNames.Count == 0) return;

            Console.WriteLine($"Criando {entityNames.Count} entidade(s)...");

            foreach (string entityName in entityNames)
            {
                await CreateEntityFromEntity1Template(projectPath, entityName);
            }

            // Após criar todas as entidades, remover os arquivos Entity1
            await RemoveEntity1Files(projectPath);
        }

        private static async Task CreateEntityFromEntity1Template(string projectPath, string entityName)
        {
            // Encontrar todos os arquivos que contêm "Entity1", excluindo arquivos do Visual Studio e outros desnecessários
            var entity1Files = Directory.GetFiles(projectPath, "*", SearchOption.AllDirectories)
                .Where(f => !f.Contains("\\bin\\") && !f.Contains("\\obj\\") && !f.Contains("\\.vs\\") && !f.Contains("\\packages\\"))
                .Where(f => Path.GetExtension(f).ToLower() is ".cs" or ".csproj" or ".json")
                .Where(f => Path.GetFileName(f).Contains("Entity1"))
                .ToList();

            // Também procurar arquivos .cs que contenham "Entity1" no conteúdo
            var additionalFiles = Directory.GetFiles(projectPath, "*.cs", SearchOption.AllDirectories)
                .Where(f => !f.Contains("\\bin\\") && !f.Contains("\\obj\\") && !f.Contains("\\.vs\\") && !f.Contains("\\packages\\"))
                .Where(f => !Path.GetFileName(f).Contains("Entity1")) // Evitar duplicatas
                .ToList();

            foreach (string file in additionalFiles)
            {
                try
                {
                    string content = await File.ReadAllTextAsync(file);
                    if (content.Contains("Entity1") && !entity1Files.Contains(file))
                    {
                        entity1Files.Add(file);
                    }
                }
                catch
                {
                    // Ignorar arquivos que não podem ser lidos
                }
            }

            foreach (string templateFile in entity1Files)
            {
                try
                {
                    string content = await File.ReadAllTextAsync(templateFile);
                    
                    if (!content.Contains("Entity1")) continue;

                    // Criar novo arquivo baseado no template
                    string newFileName = Path.GetFileName(templateFile).Replace("Entity1", entityName);
                    string newFilePath = Path.Combine(Path.GetDirectoryName(templateFile)!, newFileName);

                    // Substituir Entity1 pelo nome da nova entidade
                    string newContent = content.Replace("Entity1", entityName);
                    newContent = newContent.Replace("entity1", entityName.ToLower());

                    // Se for um arquivo de entidade no Domain, garantir que herde de EntityBase
                    if (templateFile.Contains("\\Domain\\Entities\\") && templateFile.EndsWith(".cs"))
                    {
                        // Verificar se já herda de EntityBase, se não, adicionar
                        if (!newContent.Contains(": EntityBase") && newContent.Contains($"class {entityName}"))
                        {
                            newContent = newContent.Replace($"class {entityName}", $"class {entityName} : EntityBase");
                        }
                    }

                    await File.WriteAllTextAsync(newFilePath, newContent);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Aviso: Não foi possível criar entidade {entityName} baseada em {templateFile}: {ex.Message}");
                }
            }

            // Atualizar DependencyInjection.cs para incluir a nova entidade
            await UpdateDependencyInjection(projectPath, entityName);
        }

        private static async Task RemoveEntity1Files(string projectPath)
        {
            Console.WriteLine("Removendo arquivos template Entity1...");

            // Encontrar todos os arquivos que contêm "Entity1" no nome
            var entity1Files = Directory.GetFiles(projectPath, "*Entity1*", SearchOption.AllDirectories)
                .Where(f => !f.Contains("\\bin\\") && !f.Contains("\\obj\\") && !f.Contains("\\.vs\\") && !f.Contains("\\packages\\"))
                .ToList();

            foreach (string file in entity1Files)
            {
                try
                {
                    File.Delete(file);
                    Console.WriteLine($"Removido: {Path.GetFileName(file)}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Aviso: Não foi possível remover {file}: {ex.Message}");
                }
            }

            // Também remover referências a Entity1 em arquivos que não foram removidos
            await RemoveEntity1References(projectPath);
        }

        private static async Task RemoveEntity1References(string projectPath)
        {
            var filesToProcess = Directory.GetFiles(projectPath, "*.cs", SearchOption.AllDirectories)
                .Where(f => !f.Contains("\\bin\\") && !f.Contains("\\obj\\") && !f.Contains("\\.vs\\") && !f.Contains("\\packages\\"))
                .ToList();

            foreach (string filePath in filesToProcess)
            {
                try
                {
                    string content = await File.ReadAllTextAsync(filePath);
                    bool modified = false;

                    // Remover using statements para Entity1
                    if (content.Contains($"using {projectName}.Domain.Entities.Entity1"))
                    {
                        content = Regex.Replace(content, $@"using {projectName}\.Domain\.Entities\.Entity1.*\n", "");
                        modified = true;
                    }

                    // Remover registros de Entity1 no DependencyInjection
                    if (content.Contains("Entity1Service") || content.Contains("Entity1JobScheduler") || content.Contains("Entity1JobExecutor"))
                    {
                        content = Regex.Replace(content, @".*Entity1Service.*\n", "");
                        content = Regex.Replace(content, @".*Entity1JobScheduler.*\n", "");
                        content = Regex.Replace(content, @".*Entity1JobExecutor.*\n", "");
                        content = Regex.Replace(content, @".*Entity1Wrapper.*\n", "");
                        content = Regex.Replace(content, @".*Entity1Executor.*\n", "");
                        modified = true;
                    }

                    if (modified)
                    {
                        await File.WriteAllTextAsync(filePath, content);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Aviso: Não foi possível processar {filePath} para remoção de Entity1: {ex.Message}");
                }
            }
        }

        private static async Task UpdateDependencyInjection(string projectPath, string entityName)
        {
            string diPath = Path.Combine(projectPath, $"{projectName}.Infra.IoC", "DependencyInjection.cs");
            
            if (File.Exists(diPath))
            {
                try
                {
                    string content = await File.ReadAllTextAsync(diPath);
                    
                    // Adicionar using statements se necessário
                    if (!content.Contains($"using {projectName}.Application.Services.Interfaces;"))
                    {
                        content = content.Replace(
                            $"using {projectName}.Application.Services.Interfaces;",
                            $"using {projectName}.Application.Services.Interfaces;"
                        );
                    }

                    // Adicionar registro do serviço
                    string serviceRegistration = $"            services.AddScoped<I{entityName}Service, {entityName}Service>();";
                    string jobSchedulerRegistration = $"            services.AddScoped<I{entityName}JobScheduler, {entityName}Wrapper>();";
                    string jobExecutorRegistration = $"            services.AddScoped<I{entityName}JobExecutor, {entityName}Executor>();";

                    // Encontrar onde inserir os novos registros
                    if (content.Contains("//Application Services"))
                    {
                        content = content.Replace(
                            "//Application Services",
                            $"//Application Services\n{serviceRegistration}"
                        );
                    }

                    if (content.Contains("//Hangfire Job Scheduler Wrappers"))
                    {
                        content = content.Replace(
                            "//Hangfire Job Scheduler Wrappers",
                            $"//Hangfire Job Scheduler Wrappers\n{jobSchedulerRegistration}\n{jobExecutorRegistration}"
                        );
                    }

                    await File.WriteAllTextAsync(diPath, content);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Aviso: Não foi possível atualizar DependencyInjection para {entityName}: {ex.Message}");
                }
            }
        }

        private static async Task RemoveMultitenancy(string projectPath)
        {
            Console.WriteLine("Removendo funcionalidades de multi-tenancy...");

            // Remover arquivo Tenant.cs
            string tenantPath = Path.Combine(projectPath, $"{projectName}.Domain", "Entities", "Tenant.cs");
            if (File.Exists(tenantPath))
            {
                File.Delete(tenantPath);
            }

            // Processar arquivos para remover referências a Tenant
            var filesToProcess = Directory.GetFiles(projectPath, "*.cs", SearchOption.AllDirectories)
                .Where(f => !f.Contains("\\bin\\") && !f.Contains("\\obj\\"))
                .ToList();

            foreach (string filePath in filesToProcess)
            {
                try
                {
                    string content = await File.ReadAllTextAsync(filePath);
                    bool modified = false;

                    // Remover propriedades TenantId
                    if (content.Contains("public int TenantId { get; set; }"))
                    {
                        content = Regex.Replace(content, @"\s*public int TenantId \{ get; set; \}\s*", "\n");
                        modified = true;
                    }

                    // Remover referências a Tenant
                    if (content.Contains("public Tenant Tenant { get; set; }"))
                    {
                        content = Regex.Replace(content, @"\s*public Tenant Tenant \{ get; set; \}\s*", "\n");
                        modified = true;
                    }

                    // Remover linhas comentadas sobre multitenancy
                    content = Regex.Replace(content, @".*\/\/<-- Se o usuário não quer multitenancy.*\n", "");
                    
                    // Remover blocos específicos do DbContext
                    if (filePath.Contains("DbContext.cs"))
                    {
                        content = RemoveMultitenancyFromDbContext(content);
                        modified = true;
                    }

                    // Processar arquivos de autenticação (mais abrangente)
                    if (filePath.Contains("AuthService") || filePath.Contains("Authentication") || 
                        filePath.Contains("Service") || filePath.Contains("Repository"))
                    {
                        content = RemoveMultitenancyFromAuthService(content);
                        modified = true;
                    }

                    // Processar RefreshToken
                    if (filePath.Contains("RefreshToken"))
                    {
                        content = RemoveMultitenancyFromRefreshToken(content);
                        modified = true;
                    }

                    // Processar qualquer arquivo que contenha referências a TenantId
                    if (content.Contains("TenantId") && !modified)
                    {
                        content = RemoveMultitenancyFromAuthService(content);
                        modified = true;
                    }

                    if (modified)
                    {
                        await File.WriteAllTextAsync(filePath, content);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Aviso: Não foi possível processar {filePath} para remoção de multitenancy: {ex.Message}");
                }
            }
        }

        private static string RemoveMultitenancyFromDbContext(string content)
        {
            // Remover propriedades e métodos relacionados a tenant
            content = Regex.Replace(content, @"\s*public DbSet<Tenant> Tenants.*\n", "");
            content = Regex.Replace(content, @"\s*public int\? CurrentTenantId.*\n", "");
            content = Regex.Replace(content, @"\s*public void SetTenant.*\n", "");
            
            // Remover configurações de tenant no OnModelCreating - Tenant entity
            content = Regex.Replace(content, @"\s*builder\.Entity<Tenant>\(\).*?\.OnDelete\(DeleteBehavior\.Restrict\);\s*", "", RegexOptions.Singleline);
            
            // Remover configurações de relacionamento com Tenant em User
            content = Regex.Replace(content, @"\s*\.HasMany\(t => t\.Users\).*?\.OnDelete\(DeleteBehavior\.Restrict\);\s*", "", RegexOptions.Singleline);
            content = Regex.Replace(content, @"\s*b\.HasOne\(u => u\.Tenant\).*?\.OnDelete\(DeleteBehavior\.Restrict\);\s*", "", RegexOptions.Singleline);
            
            // Remover configurações de relacionamento com Tenant em ApplicationUser
            content = Regex.Replace(content, @"\s*b\.HasIndex\(a => new \{ a\.UserName, a\.TenantId \}\)\.IsUnique\(\);\s*", "");
            content = Regex.Replace(content, @"\s*// Tenant reference \(1:N\).*?\.OnDelete\(DeleteBehavior\.Restrict\);\s*", "", RegexOptions.Singleline);
            content = Regex.Replace(content, @"\s*\.WithMany\(\).*?\.OnDelete\(DeleteBehavior\.Restrict\);\s*", "", RegexOptions.Singleline);
            
            // Remover query filters
            content = Regex.Replace(content, @"\s*foreach \(var entityType in builder\.Model\.GetEntityTypes\(\)\).*?}\s*}", "", RegexOptions.Singleline);
            
            // Remover SaveChanges override - versões mais específicas para capturar blocos quebrados
            content = Regex.Replace(content, @"\s*public override int SaveChanges\(\)\s*\{[^}]*ApplyTenantId\(\);[^}]*return base\.SaveChanges\(\);[^}]*\}\s*", "", RegexOptions.Singleline);
            content = Regex.Replace(content, @"\s*\{\s*ApplyTenantId\(\);\s*return base\.SaveChanges\(\);\s*\}\s*", "", RegexOptions.Singleline);
            
            // Remover ApplyTenantId method - versões mais específicas para capturar blocos quebrados
            content = Regex.Replace(content, @"\s*private void ApplyTenantId\(\)\s*\{[^}]*CurrentTenantId[^}]*\}\s*", "", RegexOptions.Singleline);
            content = Regex.Replace(content, @"\s*\{\s*if \(CurrentTenantId is null\)\s*return;[^}]*\}\s*", "", RegexOptions.Singleline);
            
            // Remover blocos órfãos específicos que podem ter sobrado
            content = Regex.Replace(content, @"\s*\{\s*ApplyTenantId\(\);\s*return base\.SaveChanges\(\);\s*\}\s*", "");
            content = Regex.Replace(content, @"\s*\{\s*if \(CurrentTenantId is null\)\s*return;.*?tenantProp\.SetValue\(entry\.Entity, CurrentTenantId\);.*?\}\s*\}\s*", "", RegexOptions.Singleline);
            
            // Remover blocos vazios que podem ter sobrado
            content = Regex.Replace(content, @"\s*\{\s*\}\s*", "");
            
            // Corrigir índice único do ApplicationUser para usar apenas UserName
            content = content.Replace("b.HasIndex(a => new { a.UserName, a.TenantId }).IsUnique();", "b.HasIndex(a => a.UserName).IsUnique();");
            
            // Corrigir construtor quebrado - adicionar chaves faltantes se necessário
            content = Regex.Replace(content, @"(\w+DbContext\(DbContextOptions<\w+DbContext> options\) : base\(options\))(\s*public)", "$1 { }$2");

            return content;
        }

        private static string RemoveMultitenancyFromAuthService(string content)
        {
            // Remover parâmetros TenantId de métodos - versões mais específicas
            content = Regex.Replace(content, @",\s*int\s+tenantId", "");
            content = Regex.Replace(content, @"int\s+tenantId\s*,\s*", "");
            content = Regex.Replace(content, @"\(\s*int\s+tenantId\s*\)", "()");
            content = Regex.Replace(content, @"\(string\s+\w+,\s*string\s+\w+,\s*int\s+tenantId\)", "(string email, string password)");
            content = Regex.Replace(content, @"\(string\s+\w+,\s*string\s+\w+,\s*int\s+tenantId\)", "(string email, string refreshToken)");
            
            // Remover atribuições de TenantId
            content = Regex.Replace(content, @"\s*TenantId\s*=\s*tenantId\s*,?\s*", "");
            content = Regex.Replace(content, @"\s*,\s*TenantId\s*=\s*tenantId\s*", "");
            
            // Remover filtros por TenantId em queries - mais específico
            content = Regex.Replace(content, @"\.Where\(rt => rt\.Email == email && rt\.TenantId == tenantId\)", ".Where(rt => rt.Email == email)");
            content = Regex.Replace(content, @"\.FirstOrDefaultAsync\(u => u\.Email == email && u\.TenantId == tenantId\)", ".FirstOrDefaultAsync(u => u.Email == email)");
            content = Regex.Replace(content, @"\.Where\([^)]*TenantId[^)]*\)", "");
            content = Regex.Replace(content, @"&&\s*[^&]*TenantId[^&]*", "");
            content = Regex.Replace(content, @"[^&]*TenantId[^&]*\s*&&\s*", "");
            
            // Remover validações de TenantId
            content = Regex.Replace(content, @"\s*if\s*\([^)]*TenantId[^)]*\)[^}]*\{[^}]*\}\s*", "");
            
            return content;
        }

        private static string RemoveMultitenancyFromRefreshToken(string content)
        {
            // Remover propriedade TenantId da classe RefreshToken
            content = Regex.Replace(content, @"\s*public int TenantId \{ get; set; \}\s*", "\n");
            
            // Remover navegação para Tenant
            content = Regex.Replace(content, @"\s*public Tenant Tenant \{ get; set; \}\s*", "\n");
            
            // Remover atribuições de TenantId
            content = Regex.Replace(content, @"\s*TenantId\s*=\s*[^,\n]*,?\s*", "");
            content = Regex.Replace(content, @"\s*,\s*TenantId\s*=\s*[^,\n]*", "");
            
            return content;
        }
    }
}
