using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace BoilerplateCustomizer
{
    class Program
    {
        private static readonly string BoilerplatePath = GetBoilerplatePath();
        private static readonly string ReactBoilerplatePath = GetReactBoilerplatePath();
        private static string? projectName;
        private static List<string> entityNames = new();
        private static bool enableMultitenancy = true;
        private static string? destinationPath;

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

        private static string GetReactBoilerplatePath()
        {
            string boilerplateDir = Path.GetDirectoryName(GetBoilerplatePath()) ?? Directory.GetCurrentDirectory();
            return Path.Combine(boilerplateDir, "react-boilerplate");
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

                // Determinar diretório base (caminho informado pelo usuário ou padrão)
                string baseDir = destinationPath 
                    ?? Directory.GetParent(BoilerplatePath)?.FullName 
                    ?? Directory.GetCurrentDirectory();

                // Criar pasta mãe do projeto
                string parentFolder = Path.Combine(baseDir, projectName!);
                
                if (Directory.Exists(parentFolder))
                {
                    Console.WriteLine($"Pasta '{parentFolder}' já existe. Deseja sobrescrever? (s/n): ");
                    var overwrite = Console.ReadLine()?.ToLower();
                    if (overwrite != "s" && overwrite != "sim")
                    {
                        Console.WriteLine("Operação cancelada.");
                        return;
                    }
                    Directory.Delete(parentFolder, true);
                }

                Directory.CreateDirectory(parentFolder);

                // Copiar boilerplate backend para [parent]/[projectName]/
                string backendPath = Path.Combine(parentFolder, projectName!);
                Console.WriteLine("Copiando arquivos do boilerplate backend...");
                CopyDirectory(BoilerplatePath, backendPath);

                // Copiar boilerplate frontend para [parent]/react-[projectName]/
                string? frontendPath = null;
                if (Directory.Exists(ReactBoilerplatePath))
                {
                    Console.WriteLine("Copiando arquivos do boilerplate frontend...");
                    frontendPath = Path.Combine(parentFolder, $"react-{projectName!.ToLower()}");
                    CopyDirectory(ReactBoilerplatePath, frontendPath, skipNodeModules: true);
                }
                else
                {
                    Console.WriteLine("Aviso: Pasta 'react-boilerplate' não encontrada. Frontend não será copiado.");
                }

                // Aplicar customizações
                Console.WriteLine("Aplicando customizações...");
                await ApplyCustomizations(backendPath, frontendPath);

                Console.WriteLine();
                Console.WriteLine($"✅ Projeto '{projectName}' criado com sucesso!");
                Console.WriteLine($"📁 Pasta do projeto: {parentFolder}");
                Console.WriteLine($"   📁 Backend: {backendPath}");
                if (frontendPath != null)
                    Console.WriteLine($"   📁 Frontend: {frontendPath}");
                Console.WriteLine();
                Console.WriteLine("Próximos passos:");
                Console.WriteLine("1. Abra o projeto backend no Visual Studio");
                Console.WriteLine("2. Execute as migrations do Entity Framework");
                Console.WriteLine("3. Configure a string de conexão no appsettings.json");
                if (frontendPath != null)
                {
                    Console.WriteLine($"4. No frontend, execute: npm install && npm run dev");
                }
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

            // Caminho de destino
            Console.WriteLine();
            Console.Write("Informe o caminho completo para criar o projeto (ou pressione Enter para o padrão): ");
            var inputPath = Console.ReadLine()?.Trim();
            if (!string.IsNullOrEmpty(inputPath))
            {
                destinationPath = inputPath;
            }
        }

        private static bool IsValidProjectName(string name)
        {
            return Regex.IsMatch(name, @"^[A-Za-z][A-Za-z0-9_]*$");
        }

        private static bool IsValidEntityName(string name)
        {
            return Regex.IsMatch(name, @"^[A-Z][A-Za-z0-9]*$");
        }

        private static void CopyDirectory(string sourceDir, string destDir, bool skipNodeModules = false)
        {
            Directory.CreateDirectory(destDir);

            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string fileName = Path.GetFileName(file);
                string destFile = Path.Combine(destDir, fileName);
                File.Copy(file, destFile, true);
            }

            var skipDirs = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".vs", "bin", "obj" };
            if (skipNodeModules) skipDirs.Add("node_modules");

            foreach (string subDir in Directory.GetDirectories(sourceDir))
            {
                string dirName = Path.GetFileName(subDir);
                if (skipDirs.Contains(dirName))
                    continue;
                string destSubDir = Path.Combine(destDir, dirName);
                CopyDirectory(subDir, destSubDir, skipNodeModules);
            }
        }

        private static async Task ApplyCustomizations(string projectPath, string? frontendPath)
        {
            // 1. Substituir nomes do projeto
            await ReplaceProjectNames(projectPath);
            if (frontendPath != null)
                await ReplaceFrontendProjectNames(frontendPath);

            // 2. Gerar JWT Secret Key
            await GenerateJwtSecretKey(projectPath);

            // 3. Criar entidades baseadas em Entity1
            await CreateEntitiesFromTemplate(projectPath);

            // 4. Configurar multitenancy
            if (!enableMultitenancy)
            {
                await RemoveMultitenancyBackend(projectPath);
                if (frontendPath != null)
                    await RemoveMultitenancyFrontend(frontendPath);
            }
        }

        private static async Task ReplaceFrontendProjectNames(string frontendPath)
        {
            Console.WriteLine("Substituindo nomes do projeto no frontend...");

            var filesToProcess = Directory.GetFiles(frontendPath, "*", SearchOption.AllDirectories)
                .Where(f => !f.Contains("\\node_modules\\"))
                .Where(f => Path.GetExtension(f).ToLower() is ".ts" or ".tsx" or ".json" or ".html" or ".env" or ".md")
                .ToList();

            foreach (string filePath in filesToProcess)
            {
                try
                {
                    string content = await File.ReadAllTextAsync(filePath);
                    if (!content.Contains("Boilerplate") && !content.Contains("boilerplate") && !content.Contains("BOILERPLATE"))
                        continue;

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

            // Rename directories containing "Boilerplate" (e.g., BoilerplateDataGrid -> {projectName}DataGrid)
            var dirsToRename = Directory.GetDirectories(frontendPath, "*Boilerplate*", SearchOption.AllDirectories)
                .Where(d => !d.Contains("\\node_modules\\"))
                .OrderByDescending(d => d.Length) // Rename deepest first
                .ToList();
            foreach (var dir in dirsToRename)
            {
                string dirName = Path.GetFileName(dir);
                string newDirName = dirName.Replace("Boilerplate", projectName);
                string newPath = Path.Combine(Path.GetDirectoryName(dir)!, newDirName);
                if (dir != newPath && Directory.Exists(dir))
                {
                    Directory.Move(dir, newPath);
                    Console.WriteLine($"  Renomeado diretório: {dirName} -> {newDirName}");
                }
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
                    
                    // Substituir namespaces legados
                    content = content.Replace("NewLevel.Shared.DTOs.Auth", $"{projectName}.Application.DTOs.Auth");

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

        // ========================================================================
        // MULTI-TENANCY REMOVAL - BACKEND
        // ========================================================================

        private static async Task RemoveMultitenancyBackend(string projectPath)
        {
            Console.WriteLine("Removendo funcionalidades de multi-tenancy do backend...");

            // Phase 1: Delete purely-tenant files
            DeleteTenantBackendFiles(projectPath);

            // Phase 2: Rewrite complex files
            await RewriteDbContext(projectPath);
            await RewriteAuthenticateService(projectPath);
            await RewriteIAuthenticateService(projectPath);
            await RewriteAuthAppService(projectPath);
            await RewriteSeedData(projectPath);
            await RewriteSystemNotificationService(projectPath);

            // Phase 3: Modify simpler files with targeted edits
            await ModifyUserEntity(projectPath);
            await ModifyEntity1(projectPath);
            await ModifySystemNotificationEntity(projectPath);
            await ModifyApplicationUser(projectPath);
            await ModifyRefreshToken(projectPath);
            await ModifyDependencyInjection(projectPath);
            await ModifyRoles(projectPath);
            await ModifyICurrentUserContext(projectPath);
            await ModifyCurrentUserContext(projectPath);
            await ModifyIEmailService(projectPath);
            await ModifyEmailService(projectPath);
            await ModifyRegisterInputDto(projectPath);

            // Phase 4: Rewrite controllers and remaining services
            await RewriteAuthController(projectPath);
            await ModifySystemNotificationController(projectPath);
            await ModifyISystemNotificationService(projectPath);
            await CreateChangePasswordDto(projectPath);
            await RewriteUserService(projectPath);
            await ModifyIUserService(projectPath);
            await ModifyUserController(projectPath);
            await DeleteUserInviteDto(projectPath);
            await ModifyDesignTimeCurrentContextService(projectPath);

            // Phase 5: Cleanup unused usings across all .cs files
            await CleanupTenantUsings(projectPath);

            Console.WriteLine("Multi-tenancy removida do backend com sucesso.");
        }

        private static void DeleteTenantBackendFiles(string projectPath)
        {
            Console.WriteLine("Deletando arquivos exclusivos de multi-tenancy...");

            var filesToDelete = new[]
            {
                Path.Combine(projectPath, $"{projectName}.Domain", "Entities", "Tenant.cs"),
                Path.Combine(projectPath, $"{projectName}.Domain", "Entities", "TenantInvitation.cs"),
                Path.Combine(projectPath, $"{projectName}.Domain", "Enums", "EInviteStatus.cs"),
                Path.Combine(projectPath, $"{projectName}.Infra.Data", "Persistence", "Configuration", "TenantConfiguration.cs"),
                Path.Combine(projectPath, $"{projectName}.Api", "Controllers", "TenantController.cs"),
                Path.Combine(projectPath, $"{projectName}.Application", "Interfaces", "ITenantService.cs"),
                Path.Combine(projectPath, $"{projectName}.Application", "Dtos", "Auth", "AcceptTenantInvitationInputDto.cs"),
                Path.Combine(projectPath, $"{projectName}.Application", "Dtos", "Auth", "ValidateInvitationTokenInputDto.cs"),
                Path.Combine(projectPath, $"{projectName}.Application", "Dtos", "SystemNotification", "TenantNotificationDto.cs"),
            };

            foreach (var file in filesToDelete)
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                    Console.WriteLine($"  Deletado: {Path.GetFileName(file)}");
                }
            }

            // Delete entire directories
            var dirsToDelete = new[]
            {
                Path.Combine(projectPath, $"{projectName}.Application", "Services", "Tenants"),
                Path.Combine(projectPath, $"{projectName}.Application", "Dtos", "Tenants"),
                Path.Combine(projectPath, $"{projectName}.Infra.Data", "Migrations"),
            };

            foreach (var dir in dirsToDelete)
            {
                if (Directory.Exists(dir))
                {
                    Directory.Delete(dir, true);
                    Console.WriteLine($"  Deletado diretório: {Path.GetFileName(dir)}");
                }
            }
        }

        private static async Task RewriteDbContext(string projectPath)
        {
            string dbContextPath = Path.Combine(projectPath, $"{projectName}.Infra.Data", "Context", $"{projectName}DbContext.cs");
            if (!File.Exists(dbContextPath)) return;

            Console.WriteLine("  Reescrevendo DbContext sem multi-tenancy...");

            string content = $@"using {projectName}.Application.Interfaces.ICurrentUserContext;
using {projectName}.Domain.Entities;
using {projectName}.Infra.Data.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace {projectName}.Infra.Data.Context
{{
    public class {projectName}DbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
    {{
        private readonly ICurrentUserContext _currentUserContext;

        public {projectName}DbContext(DbContextOptions<{projectName}DbContext> options, ICurrentUserContext currentUserContext) : base(options) 
        {{ 
            _currentUserContext = currentUserContext;
        }}
        public DbSet<User> DomainUsers => Set<User>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<SystemNotification> SystemNotifications => Set<SystemNotification>();
        public DbSet<SystemNotificationUser> SystemNotificationUser => Set<SystemNotificationUser>();
        public DbSet<Entity1> Entity1s => Set<Entity1>();


        protected override void OnModelCreating(ModelBuilder builder)
        {{
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>().ToTable(""UsersIdentity"");
            builder.Entity<IdentityRole<int>>().ToTable(""Roles"");
            builder.Entity<IdentityUserRole<int>>().ToTable(""UserRoles"");
            builder.Entity<IdentityUserLogin<int>>().ToTable(""UserLogins"");
            builder.Entity<IdentityUserClaim<int>>().ToTable(""UserClaims"");
            builder.Entity<IdentityRoleClaim<int>>().ToTable(""RoleClaims"");
            builder.Entity<IdentityUserToken<int>>().ToTable(""UserTokens"");

            builder.Entity<SystemNotificationUser>()
                .HasKey(x => new {{ x.UserId, x.NotificationId }});

            builder.Entity<User>(b =>
            {{
                b.ToTable(""DomainUsers"");
                b.HasKey(u => u.Id);
            }});

            builder.Entity<ApplicationUser>(b =>
            {{
                b.HasOne(a => a.User)
                    .WithOne()
                    .HasForeignKey<ApplicationUser>(a => a.DomainUserId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasIndex(a => a.UserName).IsUnique();
            }});

        }}

        public override int SaveChanges()
        {{
            ApplyAuditing();
            return base.SaveChanges();
        }}

        public override async Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {{
            ApplyAuditing();
            return await base.SaveChangesAsync(cancellationToken);
        }}

        private void ApplyAuditing()
        {{
            var now = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries<EntityBase>())
            {{
                switch (entry.State)
                {{
                    case EntityState.Added:
                        entry.Entity.CreatedAt = now;
                        entry.Entity.CreatedBy = _currentUserContext.UserId;
                        break;

                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = now;
                        entry.Entity.UpdatedBy = _currentUserContext.UserId;
                        break;

                    case EntityState.Deleted:
                        entry.State = EntityState.Modified;
                        entry.Entity.IsDeleted = true;
                        entry.Entity.DeletedAt = now;
                        entry.Entity.DeletedBy = _currentUserContext.UserId;
                        break;
                }}
            }}
        }}
    }}
}}
";
            await File.WriteAllTextAsync(dbContextPath, content);
        }

        private static async Task RewriteAuthenticateService(string projectPath)
        {
            string authPath = Path.Combine(projectPath, $"{projectName}.Infra.Data", "Identity", "AuthenticateService", "AuthenticateService.cs");
            if (!File.Exists(authPath)) return;

            Console.WriteLine("  Reescrevendo AuthenticateService sem multi-tenancy...");

            string content = $@"using {projectName}.Domain.Entities;
using {projectName}.Domain.Interfaces.Authenticate;
using {projectName}.Infra.Data.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace {projectName}.Infra.Data.Identity.AuthenticateService
{{
    public class AuthenticateService : IAuthenticateService
    {{
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly {projectName}DbContext _context;

        public AuthenticateService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration, {projectName}DbContext context)
        {{
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _context = context;
        }}

        public async Task<(bool, bool)> Authenticate(string email, string password)
        {{
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
                return (false, false);

            var passwordValid = await _userManager.CheckPasswordAsync(user, password);

            return (passwordValid, user.IsNeededChangePassword);
        }}

        public async Task<(int, string)> Register(string email, string password, string name)
        {{
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {{
                var user = new User
                {{
                    Email = email,
                    Name = name
                }};

                _context.DomainUsers.Add(user);
                await _context.SaveChangesAsync();

                var applicationUser = new ApplicationUser
                {{
                    UserName = email,
                    Email = email,
                    DomainUserId = user.Id
                }};

                var result = await _userManager.CreateAsync(applicationUser, password);

                if (!result.Succeeded)
                {{
                    await transaction.RollbackAsync();
                    return (0, string.Empty);
                }}

                await _userManager.AddToRoleAsync(applicationUser, {projectName}.Domain.Constants.Roles.User);
                await transaction.CommitAsync();

                return (applicationUser.Id, applicationUser.Email);
            }}
            catch
            {{
                await transaction.RollbackAsync();
                throw;
            }}
        }}

        public async Task Logout()
        {{
            await _signInManager.SignOutAsync();
        }}

        public async Task<string?> GeneratePasswordResetTokenAsync(string email)
        {{
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return null;

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            return token;
        }}

        public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
        {{
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return false;

            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            return result.Succeeded;
        }}

        public async Task<string> GenerateJwtToken(string email, User domainUser)
        {{
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return string.Empty;
            IList<string> roles = await _userManager.GetRolesAsync(user);

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration[""JWT:SecretKey""] ?? ""your-secret-key-here"");

            var claims = new List<Claim>
            {{
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Name, domainUser.Name),
                new Claim(ClaimTypes.NameIdentifier, domainUser.Id.ToString()),
                new Claim(""userId"", domainUser.Id.ToString()),
            }};

            foreach (var role in roles)
            {{
                claims.Add(new Claim(ClaimTypes.Role, role));
            }}

            var subject = new ClaimsIdentity(claims);

            var tokenDescriptor = new SecurityTokenDescriptor
            {{
                Subject = subject,
                Expires = DateTime.UtcNow.AddHours(3),
                Issuer = _configuration[""JWT:Issuer""],
                Audience = _configuration[""JWT:Audience""],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            }};

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }}

        public string GenerateRefreshToken()
        {{
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            var base64Token = Convert.ToBase64String(randomNumber);
            return base64Token;
        }}

        public async Task<bool> ValidateRefreshToken(string refreshToken)
        {{
            var token = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken && !rt.IsRevoked && rt.ExpiryDate > DateTime.UtcNow);

            return token != null;
        }}

        public async Task<string?> GetEmailFromRefreshToken(string refreshToken)
        {{
            var token = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken && !rt.IsRevoked && rt.ExpiryDate > DateTime.UtcNow);

            return token?.Email;
        }}

        public async Task SaveRefreshToken(string email, string refreshToken)
        {{
            var existingTokens = _context.RefreshTokens.Where(rt => rt.Email == email);
            _context.RefreshTokens.RemoveRange(existingTokens);

            var newRefreshToken = new RefreshToken
            {{
                Token = refreshToken,
                Email = email,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow
            }};

            _context.RefreshTokens.Add(newRefreshToken);
            await _context.SaveChangesAsync();
        }}

        public async Task<bool> RemoveRefreshToken(string refreshToken)
        {{
            var token = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == refreshToken);
            if (token == null) return false;

            token.IsRevoked = true;
            await _context.SaveChangesAsync();
            return true;
        }}

        public async Task<bool> ConfirmEmail(string userId, string token)
        {{
            var applicationUser = await _userManager.FindByIdAsync(userId);
            if (applicationUser == null) return false;

            var result = await _userManager.ConfirmEmailAsync(applicationUser, token);
            return result.Succeeded;
        }}

        public async Task<bool> IsExpiredRefreshToken(string refreshToken)
        {{
            var token = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == refreshToken);
            if (token == null) return false;

            return token.ExpiryDate < DateTime.Now;
        }}

        public async Task<bool> ChangePassword(string email, string password)
        {{
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return false;

            await _userManager.RemovePasswordAsync(user);

            var result = await _userManager.AddPasswordAsync(user, password);
            if (!result.Succeeded)
                return false;

            user.IsNeededChangePassword = false;
            await _userManager.UpdateAsync(user);

            return true;
        }}
    }}
}}
";
            await File.WriteAllTextAsync(authPath, content);
        }

        private static async Task RewriteIAuthenticateService(string projectPath)
        {
            string interfacePath = Path.Combine(projectPath, $"{projectName}.Domain", "Interfaces", "Authenticate", "IAuthenticateService.cs");
            if (!File.Exists(interfacePath)) return;

            Console.WriteLine("  Reescrevendo IAuthenticateService sem multi-tenancy...");

            string content = $@"using {projectName}.Domain.Entities;

namespace {projectName}.Domain.Interfaces.Authenticate
{{
    public interface IAuthenticateService
    {{
        Task<(bool, bool)> Authenticate(string email, string password);
        Task<(int, string)> Register(string email, string password, string name);
        Task Logout();
        Task<string?> GeneratePasswordResetTokenAsync(string email);
        Task<bool> ResetPasswordAsync(string email, string token, string newPassword);
        Task<bool> ChangePassword(string email, string password);

        Task<bool> ConfirmEmail(string userId, string token);

        // JWT Methods
        Task<string> GenerateJwtToken(string email, User domainUser);
        string GenerateRefreshToken();
        Task<bool> ValidateRefreshToken(string refreshToken);
        Task<string?> GetEmailFromRefreshToken(string refreshToken);
        Task<bool> IsExpiredRefreshToken(string refreshToken);
        Task SaveRefreshToken(string email, string refreshToken);
        Task<bool> RemoveRefreshToken(string refreshToken);
    }}
}}
";
            await File.WriteAllTextAsync(interfacePath, content);
        }

        private static async Task RewriteAuthAppService(string projectPath)
        {
            string authAppPath = Path.Combine(projectPath, $"{projectName}.Application", "Services", "Auth", "AuthAppService.cs");
            if (!File.Exists(authAppPath)) return;

            Console.WriteLine("  Reescrevendo AuthAppService sem multi-tenancy...");

            string content = $@"using {projectName}.Application.Dtos.Auth;
using {projectName}.Application.DTOs.Auth;
using {projectName}.Application.Interfaces;
using {projectName}.Domain.Entities;
using {projectName}.Domain.Interfaces.Authenticate;
using {projectName}.Domain.Interfaces.Repositories;

namespace {projectName}.Application.Services.Auth
{{
    public class AuthAppService
    {{
        private readonly IAuthenticateService _authService;
        private readonly IEmailService _emailService;
        private readonly IRepository<User> _userRepository;

        public AuthAppService(IAuthenticateService authenticateService, IEmailService emailService, IRepository<User> userRepository)
        {{
            _authService = authenticateService;
            _emailService = emailService;
            _userRepository = userRepository;
        }}

        public async Task<bool> ConfirmEmail(string userId, string token)
        {{
            return await _authService.ConfirmEmail(userId, token);
        }}

        public async Task<LoginResponseDto> Authenticate(string email, string password)
        {{
            var user = _userRepository.GetAll().Where(x => x.Email == email).FirstOrDefault();
            if (user == null)
                throw new Exception(""Usuário não encontrado"");

            var (isAuthenticated, isNeededChangePassword) = await _authService.Authenticate(email, password);

            if (!isAuthenticated)
            {{
                return new LoginResponseDto
                {{
                    Tokens = null,
                    IsNeededChangePassword = isNeededChangePassword
                }};
            }}

            var accessToken = await _authService.GenerateJwtToken(email, user);
            var refreshToken = _authService.GenerateRefreshToken();

            await _authService.SaveRefreshToken(email, refreshToken);

            return new LoginResponseDto
            {{
                Tokens = new TokensDto
                {{
                    Token = accessToken,
                    RefreshToken = refreshToken
                }},
                IsNeededChangePassword = isNeededChangePassword
            }};
        }}

        public async Task<bool> ChangePassword(ChangePasswordDto input)
            => await _authService.ChangePassword(input.Email, input.Password);

        public async Task<RegisterResponseDto> Register(RegisterInputDto input)
        {{
            var (userId, email) = await _authService.Register(input.Email, input.Password, input.Nickname);

            if (string.IsNullOrEmpty(email))
            {{
                return new RegisterResponseDto
                {{
                    Result = false,
                    Message = ""Erro ao registrar usuário""
                }};
            }}

            return new RegisterResponseDto
            {{
                Result = true,
                Message = ""Usuário registrado com sucesso""
            }};
        }}

        public async Task<bool> Logout()
        {{
            await _authService.Logout();
            return true;
        }}

        public async Task<ForgotPasswordResponseDto> ForgotPassword(ForgotPasswordRequestDto request)
        {{
            var token = await _authService.GeneratePasswordResetTokenAsync(request.Email);

            if (string.IsNullOrEmpty(token))
            {{
                return new ForgotPasswordResponseDto
                {{
                    IsSuccess = false,
                    Message = ""Email não encontrado no sistema""
                }};
            }}

            var emailSent = await _emailService.SendPasswordResetEmailAsync(request.Email, token);

            if (!emailSent)
            {{
                return new ForgotPasswordResponseDto
                {{
                    IsSuccess = false,
                    Message = ""Erro ao enviar email de recuperação. Tente novamente mais tarde.""
                }};
            }}

            return new ForgotPasswordResponseDto
            {{
                IsSuccess = true,
                Message = ""Email de recuperação enviado com sucesso. Verifique sua caixa de entrada.""
            }};
        }}

        public async Task<ResetPasswordResponseDto> ResetPassword(ResetPasswordRequestDto request)
        {{
            var result = await _authService.ResetPasswordAsync(request.Email, request.Token, request.NewPassword);

            return new ResetPasswordResponseDto
            {{
                IsSuccess = result,
                Message = result ? ""Senha alterada com sucesso"" : ""Token inválido ou expirado""
            }};
        }}

        public async Task<TokensDto> RefreshTokens(string refreshToken)
        {{
            if (!await _authService.ValidateRefreshToken(refreshToken))
                return EmptyTokens();

            var email = await _authService.GetEmailFromRefreshToken(refreshToken);
            if (string.IsNullOrEmpty(email))
                return EmptyTokens();

            var user = _userRepository.GetAll().FirstOrDefault(x => x.Email == email);
            if (user == null)
                return EmptyTokens();

            if (!await _authService.IsExpiredRefreshToken(refreshToken))
            {{
                var newAccessToken = await _authService.GenerateJwtToken(email, user);
                return new TokensDto
                {{
                    Token = newAccessToken
                }};
            }}

            await _authService.RemoveRefreshToken(refreshToken);

            var accessToken = await _authService.GenerateJwtToken(email, user);
            var newRefreshToken = _authService.GenerateRefreshToken();

            await _authService.SaveRefreshToken(email, newRefreshToken);

            return new TokensDto
            {{
                Token = accessToken,
                RefreshToken = newRefreshToken
            }};
        }}

        private static TokensDto EmptyTokens() => new TokensDto
        {{
            Token = string.Empty,
            RefreshToken = string.Empty
        }};
    }}
}}
";
            await File.WriteAllTextAsync(authAppPath, content);
        }

        private static async Task RewriteSeedData(string projectPath)
        {
            string seedPath = Path.Combine(projectPath, $"{projectName}.Infra.Data", "Context", "Seeding", "SeedData.cs");
            if (!File.Exists(seedPath)) return;

            Console.WriteLine("  Reescrevendo SeedData sem multi-tenancy...");

            string content = $@"using {projectName}.Domain.Constants;
using {projectName}.Domain.Entities;
using {projectName}.Infra.Data.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace {projectName}.Infra.Data.Context.Seeding
{{
    public class SeedData
    {{
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly {projectName}DbContext _context;
        public SeedData(RoleManager<IdentityRole<int>> roleManager, UserManager<ApplicationUser> userManager, {projectName}DbContext context)
        {{
            _roleManager = roleManager;
            _userManager = userManager;
            _context = context;
        }}

        public async Task SeedAsync()
        {{
            var roles = new[] {{ Roles.AdminGlobal, Roles.GlobalManager, Roles.User, Roles.Guest }};
            foreach (var role in roles)
            {{
                if (!await _roleManager.RoleExistsAsync(role))
                    await _roleManager.CreateAsync(new IdentityRole<int>(role));
            }}

            var adminEmail = ""admin@{projectName!.ToLower()}.com"";
            if (await _userManager.FindByEmailAsync(adminEmail) == null)
            {{
                var domainUser = new User
                {{
                    Name = ""Admin123"",
                    Email = adminEmail,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }};
                _context.DomainUsers.Add(domainUser);
                await _context.SaveChangesAsync();

                var adminUser = new ApplicationUser
                {{
                    UserName = adminEmail,
                    Email = adminEmail,
                    DomainUserId = domainUser.Id
                }};

                await _userManager.CreateAsync(adminUser, ""admin123"");
                await _userManager.AddToRoleAsync(adminUser, Roles.AdminGlobal);
            }}
        }}
    }}
}}
";
            await File.WriteAllTextAsync(seedPath, content);
        }

        private static async Task RewriteSystemNotificationService(string projectPath)
        {
            string svcPath = Path.Combine(projectPath, $"{projectName}.Application", "Services", "SystemNotifications", "SystemNotificationService.cs");
            if (!File.Exists(svcPath)) return;

            Console.WriteLine("  Reescrevendo SystemNotificationService sem multi-tenancy...");

            string content = $@"using {projectName}.Application.Common.SystemNotifications;
using {projectName}.Application.Dtos.SystemNotification;
using {projectName}.Application.Interfaces;
using {projectName}.Application.Interfaces.ICurrentUserContext;
using {projectName}.Application.Services.SignalR;
using {projectName}.Domain.Entities;
using {projectName}.Domain.Interfaces.Repositories;
using {projectName}.Domain.Interfaces.Repositories.IUnitOfWork;
using Microsoft.AspNetCore.SignalR;

namespace {projectName}.Application.Services.Notifications
{{
    public class SystemNotificationService : ISystemNotificationService
    {{
        private readonly IRepository<SystemNotification> _repository;
        private readonly IRepository<SystemNotificationUser> _repositoryNotificationsUsers;
        private readonly IRepository<User> _userRepository;
        private readonly IHubContext<SystemNotificationHub> _hubContext;
        private readonly ICurrentUserContext _currentUserContext;
        private readonly IUnitOfWork _unitOfWork;
        public SystemNotificationService(IRepository<SystemNotification> repository, IRepository<SystemNotificationUser> repositoryNotificationsUsers,
            IRepository<User> userRepository, IHubContext<SystemNotificationHub> hubContext,
            ICurrentUserContext currentUserContext, IUnitOfWork unitOfWork)
        {{
            _repository = repository;
            _repositoryNotificationsUsers = repositoryNotificationsUsers;
            _userRepository = userRepository;
            _hubContext = hubContext;
            _currentUserContext = currentUserContext;
            _unitOfWork = unitOfWork;
        }}

        public async Task<SystemNotificationDto> CreateSystemNotification(CreateSystemNotificationDto input)
        {{
            List<User> users;

            if (input.UserIds.Count == 0)
            {{
                users = _userRepository.GetAll().ToList();
            }}
            else
            {{
                users = _userRepository
                    .GetAll()
                    .Where(x => input.UserIds.Contains(x.Id))
                    .ToList();
            }}

            if (users.Count == 0)
                throw new Exception(""No valid users found for the notification."");

            var notification = new SystemNotification
            {{
                Title = input.Title,
                Content = input.Content,
            }};
            await _unitOfWork.BeginTransactionAsync();
            await _repository.AddAsync(notification);
            await _unitOfWork.CommitAsync();

            var userNotifications = users.Select(user => new SystemNotificationUser
            {{
                UserId = user.Id,
                NotificationId = notification.Id,
                IsRead = false
            }}).ToList();

            await _repositoryNotificationsUsers.AddRangeAsync(userNotifications);
            await _unitOfWork.CommitAsync();

            await _hubContext.Clients
                .Users(users.Select(x => x.Id.ToString()).ToList())
                .SendAsync(SystemNotificationEvents.UpdateNotifications);

            return new SystemNotificationDto
            {{
                Id = notification.Id,
                Title = notification.Title,
                Content = notification.Content,
                IsRead = false,
                CreatedAt = notification.CreatedAt,
            }};
        }}

        public async Task<bool> DeleteAllMessages(ClearAllMessagesDto input)
        {{
            var notifications = _repositoryNotificationsUsers.GetAll()
                .Where(nu => input.NotificationIds.Contains(nu.NotificationId) && nu.UserId == _currentUserContext.UserId).ToList();

            if (!notifications.Any())
                throw new Exception(""No notifications found to delete."");

            foreach (var notification in notifications)
                await _repositoryNotificationsUsers.SoftDelete(notification);

            await _hubContext.Clients
                .User(_currentUserContext.UserId.ToString())
                .SendAsync(SystemNotificationEvents.UpdateNotifications);

            return true;
        }}

        public async Task<List<SystemNotificationDto>> GetAllNotifications()
        {{
            var notifications = _repositoryNotificationsUsers.GetAll(n => n.Notification).Where(nu => nu.UserId == _currentUserContext.UserId);
            return notifications.Select(n => new SystemNotificationDto
            {{
                Id = n.Notification.Id,
                Title = n.Notification.Title,
                Content = n.Notification.Content,
                IsRead = n.IsRead,
                CreatedAt = n.Notification.CreatedAt,
            }}).OrderByDescending(n => n.CreatedAt).ToList();
        }}

        public async Task<bool> MarkNotificationAsRead(int id, MarkAsReadDto input)
        {{
            var notificationUser = _repositoryNotificationsUsers
                .GetAll()
                .FirstOrDefault(nu => nu.NotificationId == id && nu.UserId == _currentUserContext.UserId);

            if (notificationUser == null)
                throw new Exception(""Notification not found for the user."");

            notificationUser.IsRead = input.IsRead;
            notificationUser.ReadAt = input.IsRead ? DateTime.UtcNow : null;

            await _repositoryNotificationsUsers.UpdateAsync(notificationUser);
            await _hubContext.Clients
                .User(_currentUserContext.UserId.ToString())
                .SendAsync(SystemNotificationEvents.UpdateNotifications);

            return true;
        }}
    }}
}}
";
            await File.WriteAllTextAsync(svcPath, content);
        }

        // --- Simple file modifications ---

        private static async Task ModifyUserEntity(string projectPath)
        {
            string path = Path.Combine(projectPath, $"{projectName}.Domain", "Entities", "User.cs");
            if (!File.Exists(path)) return;
            string content = await File.ReadAllTextAsync(path);
            content = RemoveLineContaining(content, "public int? TenantId { get; set; }");
            content = RemoveLineContaining(content, "public Tenant? Tenant { get; set; }");
            await File.WriteAllTextAsync(path, content);
        }

        private static async Task ModifyEntity1(string projectPath)
        {
            string path = Path.Combine(projectPath, $"{projectName}.Domain", "Entities", "Entity1.cs");
            if (!File.Exists(path)) return;
            string content = await File.ReadAllTextAsync(path);
            content = RemoveLineContaining(content, "public int TenantId { get; set; }");
            content = RemoveLineContaining(content, "public Tenant Tenant { get; set; }");
            await File.WriteAllTextAsync(path, content);
        }

        private static async Task ModifySystemNotificationEntity(string projectPath)
        {
            string path = Path.Combine(projectPath, $"{projectName}.Domain", "Entities", "SystemNotification.cs");
            if (!File.Exists(path)) return;
            string content = await File.ReadAllTextAsync(path);
            content = RemoveLineContaining(content, "public int? TenantId { get; set; }");
            await File.WriteAllTextAsync(path, content);
        }

        private static async Task ModifyApplicationUser(string projectPath)
        {
            string path = Path.Combine(projectPath, $"{projectName}.Infra.Data", "Identity", "ApplicationUser.cs");
            if (!File.Exists(path)) return;

            string content = $@"using {projectName}.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace {projectName}.Infra.Data.Identity
{{
    public class ApplicationUser : IdentityUser<int>
    {{
        public int DomainUserId {{ get; set; }}
        public User User {{ get; set; }}
        public bool IsNeededChangePassword {{ get; set; }} = false;
    }}
}}
";
            await File.WriteAllTextAsync(path, content);
        }

        private static async Task ModifyRefreshToken(string projectPath)
        {
            string path = Path.Combine(projectPath, $"{projectName}.Infra.Data", "Identity", "RefreshToken.cs");
            if (!File.Exists(path)) return;
            string content = await File.ReadAllTextAsync(path);
            content = RemoveLineContaining(content, "public int? TenantId { get; set; }");
            await File.WriteAllTextAsync(path, content);
        }

        private static async Task ModifyDependencyInjection(string projectPath)
        {
            string path = Path.Combine(projectPath, $"{projectName}.Infra.IoC", "DependencyInjection.cs");
            if (!File.Exists(path)) return;
            string content = await File.ReadAllTextAsync(path);
            content = RemoveLineContaining(content, "using " + projectName + ".Application.Services.Tenants;");
            content = RemoveLineContaining(content, "ITenantService, TenantService");
            await File.WriteAllTextAsync(path, content);
        }

        private static async Task ModifyRoles(string projectPath)
        {
            string path = Path.Combine(projectPath, $"{projectName}.Domain", "Constants", "Roles.cs");
            if (!File.Exists(path)) return;
            string content = await File.ReadAllTextAsync(path);
            content = RemoveLineContaining(content, "TenantAdmin");
            await File.WriteAllTextAsync(path, content);
        }

        private static async Task ModifyICurrentUserContext(string projectPath)
        {
            string path = Path.Combine(projectPath, $"{projectName}.Domain", "Interfaces", "CurrentUserContext", "ICurrentUserContext.cs");
            if (!File.Exists(path)) return;

            string content = $@"namespace {projectName}.Application.Interfaces.ICurrentUserContext
{{
    public interface ICurrentUserContext
    {{
        int UserId {{ get; }}
        bool IsAuthenticated {{ get; }}
        string Email {{ get; }}
    }}
}}
";
            await File.WriteAllTextAsync(path, content);
        }

        private static async Task ModifyCurrentUserContext(string projectPath)
        {
            string path = Path.Combine(projectPath, $"{projectName}.Application", "Utils", "CurrentUserContext", "CurrentUserContext.cs");
            if (!File.Exists(path)) return;

            string content = $@"using {projectName}.Application.Interfaces.ICurrentUserContext;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace {projectName}.Application.Utils.CurrentUserContext
{{
    public class CurrentUserContext : ICurrentUserContext
    {{
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserContext(IHttpContextAccessor httpContextAccessor)
        {{
            _httpContextAccessor = httpContextAccessor;
        }}

        private ClaimsPrincipal ActiveUser
        {{
            get
            {{
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null)
                    return new ClaimsPrincipal();

                return httpContext.User;
            }}
        }}

        public int UserId
        {{
            get
            {{
                var userIdClaim = ActiveUser.FindFirst(""userId"")?.Value;
                return userIdClaim != null ? int.Parse(userIdClaim) : 0;
            }}
        }}

        public string Email => ActiveUser.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;

        public bool IsAuthenticated => ActiveUser.Identity?.IsAuthenticated ?? false;
    }}
}}
";
            await File.WriteAllTextAsync(path, content);
        }

        private static async Task ModifyIEmailService(string projectPath)
        {
            string path = Path.Combine(projectPath, $"{projectName}.Application", "Interfaces", "IEmailService.cs");
            if (!File.Exists(path)) return;
            string content = await File.ReadAllTextAsync(path);
            content = RemoveLineContaining(content, "SendTenantInvitationEmailAsync");
            content = RemoveLineContaining(content, "using " + projectName + ".Domain.Entities;");
            await File.WriteAllTextAsync(path, content);
        }

        private static async Task ModifyEmailService(string projectPath)
        {
            string path = Path.Combine(projectPath, $"{projectName}.Application", "Services", "Email", "EmailService.cs");
            if (!File.Exists(path)) return;
            string content = await File.ReadAllTextAsync(path);
            // Remove the entire SendTenantInvitationEmailAsync method
            content = Regex.Replace(content,
                @"\s*public async Task<bool> SendTenantInvitationEmailAsync\(.*?\n\s*\{.*?\n\s*return true;\s*\n\s*\}\s*",
                "\n", RegexOptions.Singleline);
            content = RemoveLineContaining(content, "using " + projectName + ".Domain.Entities;");
            await File.WriteAllTextAsync(path, content);
        }

        private static async Task ModifyRegisterInputDto(string projectPath)
        {
            string path = Path.Combine(projectPath, $"{projectName}.Application", "Dtos", "Auth", "RegisterInputDto.cs");
            if (!File.Exists(path)) return;
            string content = await File.ReadAllTextAsync(path);
            content = RemoveLineContaining(content, "public int? TenantId { get; set; }");
            content = RemoveLineContaining(content, "public string Token { get; set; }");
            await File.WriteAllTextAsync(path, content);
        }

        private static async Task RewriteAuthController(string projectPath)
        {
            string path = Path.Combine(projectPath, $"{projectName}.Api", "Controllers", "AuthController.cs");
            if (!File.Exists(path)) return;

            Console.WriteLine("  Reescrevendo AuthController sem multi-tenancy...");

            string content = $@"using {projectName}.Api.ApiResponse;
using {projectName}.Application.Dtos.Auth;
using {projectName}.Application.DTOs.Auth;
using {projectName}.Application.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace {projectName}.Api.Controllers
{{
    [Route(""api/[controller]"")]
    [ApiController]
    public class AuthController : ControllerBase
    {{
        private readonly AuthAppService _authAppService;
        public AuthController(AuthAppService authAppService)
        {{
            _authAppService = authAppService;
        }}

        [HttpPost(""Login"")]
        public async Task<ActionResult<{projectName}Response<LoginResponseDto>>> Login(LoginInputDto input)
        {{
            var resultLogin = await _authAppService.Authenticate(input.Email, input.Password);

            return new {projectName}Response<LoginResponseDto>()
            {{
                IsSuccess = true,
                Data = new LoginResponseDto
                {{
                    Tokens = resultLogin.Tokens != null ? new TokensDto
                    {{
                        Token = resultLogin.Tokens.Token,
                        RefreshToken = resultLogin.Tokens.RefreshToken
                    }} : null,
                    IsNeededChangePassword = resultLogin.IsNeededChangePassword
                }}
            }};
        }}

        [HttpPost(""Register"")]
        public async Task<ActionResult<{projectName}Response<RegisterResponseDto>>> Register(RegisterInputDto input)
        {{
            var result = await _authAppService.Register(input);

            return Ok(new {projectName}Response<RegisterResponseDto>()
            {{
                Data = new RegisterResponseDto
                {{
                    Result = result.Result,
                    Message = result.Message,
                    UserId = result.UserId
                }},
                IsSuccess = result.Result,
                Message = result.Message
            }});
        }}

        [HttpGet(""Logout"")]
        public async Task<ActionResult<{projectName}Response<bool>>> Logout()
        {{
            var result = await _authAppService.Logout();

            if (result)
                return Ok(new {projectName}Response<bool>()
                {{
                    Data = result,
                    IsSuccess = result,
                    Message = ""Deslogado com sucesso""
                }});

            return StatusCode(500, new {projectName}Response<bool> {{ IsSuccess = false, Message = ""Algo deu errado, tente novamente mais tarde"" }});
        }}

        [HttpPost(""RefreshToken"")]
        public async Task<ActionResult<{projectName}Response<TokensDto>>> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {{
            var tokens = await _authAppService.RefreshTokens(request.RefreshToken);

            if (!string.IsNullOrEmpty(tokens.Token))
            {{
                return Ok(new {projectName}Response<TokensDto>
                {{
                    Data = new TokensDto
                    {{
                        Token = tokens.Token,
                        RefreshToken = tokens.RefreshToken
                    }},
                    IsSuccess = true,
                    Message = ""Tokens renovados com sucesso""
                }});
            }}

            return Unauthorized(new {projectName}Response<TokensDto>
            {{
                IsSuccess = false,
                Message = ""Refresh token inválido ou expirado""
            }});
        }}

        [HttpPost(""ForgotPassword"")]
        public async Task<ActionResult<{projectName}Response<bool>>> ForgotPassword(ForgotPasswordRequestDto request)
        {{
            var result = await _authAppService.ForgotPassword(request);

            return Ok(new {projectName}Response<bool>
            {{
                Data = result.IsSuccess,
                IsSuccess = result.IsSuccess,
                Message = result.Message
            }});
        }}

        [HttpPost(""ResetPassword"")]
        public async Task<ActionResult<{projectName}Response<bool>>> ResetPassword(ResetPasswordRequestDto request)
        {{
            var result = await _authAppService.ResetPassword(request);

            return Ok(new {projectName}Response<bool>
            {{
                Data = result.IsSuccess,
                IsSuccess = result.IsSuccess,
                Message = result.Message
            }});
        }}

        [HttpPost(""ChangePassword"")]
        public async Task<ActionResult<{projectName}Response<bool>>> ChangePassword(ChangePasswordDto request)
        {{
            var result = await _authAppService.ChangePassword(request);
            return Ok(new {projectName}Response<bool>
            {{
                IsSuccess = result,
                Data = result
            }});
        }}

        [HttpGet(""confirm-email"")]
        public async Task<ActionResult<{projectName}Response<bool>>> ConfirmEmail(string userId, string token)
        {{
            var result = await _authAppService.ConfirmEmail(userId, token);

            return Ok(new {projectName}Response<bool>
            {{
                IsSuccess = result,
                Message = result ? ""Email confirmado com sucesso."" : ""Falha ao confirmar o email."",
            }});
        }}
    }}
}}
";
            await File.WriteAllTextAsync(path, content);
        }

        private static async Task ModifySystemNotificationController(string projectPath)
        {
            string path = Path.Combine(projectPath, $"{projectName}.Api", "Controllers", "SystemNotificationController.cs");
            if (!File.Exists(path)) return;

            Console.WriteLine("  Reescrevendo SystemNotificationController sem multi-tenancy...");

            string content = $@"using {projectName}.Api.ApiResponse;
using {projectName}.Application.Dtos.SystemNotification;
using {projectName}.Application.Interfaces;
using {projectName}.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace {projectName}.Api.Controllers
{{
    [Route(""api/[controller]"")]
    [ApiController]
    [Authorize]
    public class SystemNotificationController : ControllerBase
    {{
        private readonly ISystemNotificationService _notificationService;
        public SystemNotificationController(ISystemNotificationService notificationService)
        {{
            _notificationService = notificationService;
        }}

        [HttpGet]
        public async Task<ActionResult<{projectName}Response<List<SystemNotificationDto>>>> GetAll()
        {{
            var notifications = await _notificationService.GetAllNotifications();
            return Ok(new {projectName}Response<List<SystemNotificationDto>>
            {{
                IsSuccess = true,
                Data = notifications
            }});
        }}

        [HttpPost]
        [Authorize(Roles = $""{{Roles.GlobalManager}},{{Roles.AdminGlobal}}"")]
        public async Task<ActionResult<{projectName}Response<SystemNotificationDto>>> Create(CreateSystemNotificationDto input)
        {{
            var notification = await _notificationService.CreateSystemNotification(input);
            return Ok(new {projectName}Response<SystemNotificationDto>
            {{
                IsSuccess = true,
                Data = notification
            }});
        }}

        [HttpPatch(""MarkAsRead/{{id}}"")]
        public async Task<ActionResult<{projectName}Response<bool>>> MarkAsRead(int id, [FromBody] MarkAsReadDto input)
        {{
            var result = await _notificationService.MarkNotificationAsRead(id, input);
            return Ok(new {projectName}Response<bool>
            {{
                IsSuccess = true,
                Data = result
            }});
        }}

        [HttpPost(""ClearAllMessages"")]
        public async Task<ActionResult<{projectName}Response<bool>>> ClearAllMessages(ClearAllMessagesDto input)
        {{
            var result = await _notificationService.DeleteAllMessages(input);
            return Ok(new {projectName}Response<bool>
            {{
                IsSuccess = true,
                Data = result
            }});
        }}
    }}
}}
";
            await File.WriteAllTextAsync(path, content);
        }

        private static async Task ModifyISystemNotificationService(string projectPath)
        {
            string path = Path.Combine(projectPath, $"{projectName}.Application", "Interfaces", "ISystemNotificationService.cs");
            if (!File.Exists(path)) return;

            Console.WriteLine("  Reescrevendo ISystemNotificationService sem multi-tenancy...");

            string content = $@"using {projectName}.Application.Dtos.SystemNotification;

namespace {projectName}.Application.Interfaces
{{
    public interface ISystemNotificationService
    {{
        Task<List<SystemNotificationDto>> GetAllNotifications();
        Task<SystemNotificationDto> CreateSystemNotification(CreateSystemNotificationDto input);
        Task<bool> MarkNotificationAsRead(int id, MarkAsReadDto input);
        Task<bool> DeleteAllMessages(ClearAllMessagesDto input);
    }}
}}
";
            await File.WriteAllTextAsync(path, content);
        }

        private static async Task CreateChangePasswordDto(string projectPath)
        {
            string path = Path.Combine(projectPath, $"{projectName}.Application", "Dtos", "Auth", "ChangePasswordDto.cs");
            if (File.Exists(path)) return;

            Console.WriteLine("  Criando ChangePasswordDto...");

            string content = $@"namespace {projectName}.Application.Dtos.Auth
{{
    public record ChangePasswordDto(
        string Email,
        string Password    
    );
}}
";
            await File.WriteAllTextAsync(path, content);
        }

        private static async Task RewriteUserService(string projectPath)
        {
            string path = Path.Combine(projectPath, $"{projectName}.Application", "Services", "Users", "UserService.cs");
            if (!File.Exists(path)) return;

            Console.WriteLine("  Reescrevendo UserService sem multi-tenancy...");

            string content = $@"using {projectName}.Application.Dtos.Users;
using {projectName}.Application.Interfaces;
using {projectName}.Application.Interfaces.ICurrentUserContext;
using {projectName}.Application.Utils.StaticUtils;
using {projectName}.Domain.Entities;
using {projectName}.Domain.Interfaces.ApplicationUserService;
using {projectName}.Domain.Interfaces.Repositories;
using {projectName}.Domain.Models;

namespace {projectName}.Application.Services.Users
{{
    public class UserService : IUserService
    {{
        private readonly IRepository<User> _repository;
        private readonly IApplicationUserService _applicationUserService;
        private readonly ICurrentUserContext _currentUserContext;
        public UserService(IRepository<User> repository, IApplicationUserService applicationUserService, 
            ICurrentUserContext currentUserContext)
        {{
            _repository = repository;
            _applicationUserService = applicationUserService;
            _currentUserContext = currentUserContext;
        }}

        public async Task<bool> DeleteUser(int id)
        {{
            var user = await _repository.GetByIdAsync(id);
            if (user == null)
                throw new Exception(""User not found"");

            await _repository.SoftDelete(user);
            return true;
        }}

        public async Task<List<UserDto>> GetAllUSers()
        {{
            var users = new List<UserDto>();
            var allUsers = _repository.GetAll().ToList();
            foreach (var u in allUsers) 
            {{
                users.Add(new UserDto
                {{
                    Id = u.Id,
                    Email = u.Email,
                    Name = u.Name,
                    Roles = await _applicationUserService.GetUserRole(u.Id)
                }});
            }}
            return users;
        }}

        public async Task<bool> UpdateUser(int id, UpdateUserDto input)
        {{
            var user = await _repository.GetByIdAsync(id);
            if (user == null)
                throw new NullReferenceException(""Usuário não encontrado para atualizar"");

            var result = await _applicationUserService.UpdateUserRoles(id, input.Roles);
            if (result == false)
                return false;

            {projectName}StaticUtils.ApplyChanges<User, UpdateUserDto>(user, input);
            await _repository.UpdateAsync(user);
            return true;
        }}
    }}
}}
";
            await File.WriteAllTextAsync(path, content);
        }

        private static async Task ModifyIUserService(string projectPath)
        {
            string path = Path.Combine(projectPath, $"{projectName}.Application", "Interfaces", "IUserService.cs");
            if (!File.Exists(path)) return;

            Console.WriteLine("  Reescrevendo IUserService sem multi-tenancy...");

            string content = $@"using {projectName}.Application.Dtos.Users;

namespace {projectName}.Application.Interfaces
{{
    public interface IUserService
    {{
        public Task<List<UserDto>> GetAllUSers();
        public Task<bool> DeleteUser(int id);
        public Task<bool> UpdateUser(int id, UpdateUserDto input);
    }}
}}
";
            await File.WriteAllTextAsync(path, content);
        }

        private static async Task ModifyUserController(string projectPath)
        {
            string path = Path.Combine(projectPath, $"{projectName}.Api", "Controllers", "UserController.cs");
            if (!File.Exists(path)) return;

            Console.WriteLine("  Reescrevendo UserController sem multi-tenancy...");

            string content = $@"using {projectName}.Api.ApiResponse;
using {projectName}.Application.Dtos.Users;
using {projectName}.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace {projectName}.Api.Controllers
{{
    [Route(""api/[controller]"")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {{
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {{
            _userService = userService;
        }}

        [HttpGet]
        public async Task<ActionResult<{projectName}Response<List<UserDto>>>> GetAll()
        {{
            var users = await _userService.GetAllUSers();
            return Ok(new {projectName}Response<List<UserDto>>
            {{
                IsSuccess = true,
                Data = users
            }});
        }}

        [HttpDelete(""{{id}}"")]
        public async Task<ActionResult<{projectName}Response<bool>>> Delete(int id)
        {{
            await _userService.DeleteUser(id);
            return NoContent();
        }}

        [HttpPatch(""{{id}}"")]
        public async Task<ActionResult<{projectName}Response<bool>>> Update(int id, UpdateUserDto input)
        {{
            await _userService.UpdateUser(id, input);
            return NoContent();
        }}
    }}
}}
";
            await File.WriteAllTextAsync(path, content);
        }

        private static async Task DeleteUserInviteDto(string projectPath)
        {
            string path = Path.Combine(projectPath, $"{projectName}.Application", "Dtos", "Users", "UserInviteDto.cs");
            if (File.Exists(path))
            {
                File.Delete(path);
                Console.WriteLine("  Deletado: UserInviteDto.cs");
            }
        }

        private static async Task ModifyDesignTimeCurrentContextService(string projectPath)
        {
            string path = Path.Combine(projectPath, $"{projectName}.Infra.Data", "Context", "Factory", "DesignTimeCurrentContextService.cs");
            if (!File.Exists(path)) return;

            Console.WriteLine("  Reescrevendo DesignTimeCurrentContextService sem multi-tenancy...");

            string content = $@"using {projectName}.Application.Interfaces.ICurrentUserContext;

namespace {projectName}.Infra.Data.Context.Factory
{{
    public class DesignTimeCurrentContextService : ICurrentUserContext
    {{
        public int UserId => 0;
        public bool IsAuthenticated => false;
        public string Email => string.Empty;
    }}
}}
";
            await File.WriteAllTextAsync(path, content);
        }

        private static async Task CleanupTenantUsings(string projectPath)
        {
            var filesToProcess = Directory.GetFiles(projectPath, "*.cs", SearchOption.AllDirectories)
                .Where(f => !f.Contains("\\bin\\") && !f.Contains("\\obj\\"))
                .ToList();

            foreach (string filePath in filesToProcess)
            {
                try
                {
                    string content = await File.ReadAllTextAsync(filePath);
                    bool modified = false;

                    var tenantUsings = new[]
                    {
                        $"using {projectName}.Domain.Entities.Tenant;",
                        $"using {projectName}.Domain.Entities.TenantInvitation;",
                        $"using {projectName}.Domain.Enums;",
                        $"using {projectName}.Application.Dtos.Tenants;",
                        $"using {projectName}.Application.Services.Tenants;",
                        $"using {projectName}.Infra.Data.Persistence.Configuration;",
                    };

                    foreach (var u in tenantUsings)
                    {
                        if (content.Contains(u))
                        {
                            // Only remove if the using is no longer needed
                            var typeName = u.Replace("using ", "").Replace(";", "").Split('.').Last();
                            // Simple heuristic: if the type isn't referenced elsewhere, remove it
                            var contentWithoutUsing = content.Replace(u, "");
                            if (!contentWithoutUsing.Contains(typeName) || typeName == "Configuration")
                            {
                                content = RemoveLineContaining(content, u);
                                modified = true;
                            }
                        }
                    }

                    if (modified)
                    {
                        await File.WriteAllTextAsync(filePath, content);
                    }
                }
                catch { }
            }
        }

        // ========================================================================
        // MULTI-TENANCY REMOVAL - FRONTEND
        // ========================================================================

        private static async Task RemoveMultitenancyFrontend(string frontendPath)
        {
            Console.WriteLine("Removendo funcionalidades de multi-tenancy do frontend...");

            // Phase 3: Delete purely-tenant frontend files
            DeleteTenantFrontendFiles(frontendPath);

            // Phase 4: Rewrite/modify shared frontend files
            await RewriteAppRouter(frontendPath);
            await RewriteProtectedRoute(frontendPath);
            await RewriteAuthContext(frontendPath);
            await RewriteTypesIndex(frontendPath);
            await RewriteConstants(frontendPath);
            await RewriteAuthHelpers(frontendPath);
            await RewriteContextsUtils(frontendPath);
            await RewriteSidebar(frontendPath);
            await RewriteHeader(frontendPath);
            await RewriteDashboard(frontendPath);
            await RewriteDashboardLayout(frontendPath);
            await ModifyApiClient(frontendPath);
            await ModifyFrontendUserTypes(frontendPath);
            await ModifyFrontendSystemNotificationTypes(frontendPath);
            await RewriteUsersPage(frontendPath);
            await ModifyEditUserModal(frontendPath);
            await ModifyComponentsIndex(frontendPath);
            await ModifyCreateNotificationModal(frontendPath);

            Console.WriteLine("Multi-tenancy removida do frontend com sucesso.");
        }

        private static void DeleteTenantFrontendFiles(string frontendPath)
        {
            Console.WriteLine("Deletando arquivos frontend exclusivos de multi-tenancy...");

            var filesToDelete = new[]
            {
                Path.Combine(frontendPath, "src", "pages", "TenantSelection.tsx"),
                Path.Combine(frontendPath, "src", "pages", "TenantSettings.tsx"),
                Path.Combine(frontendPath, "src", "pages", "AcceptInvitation.tsx"),
            };

            foreach (var file in filesToDelete)
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                    Console.WriteLine($"  Deletado: {Path.GetFileName(file)}");
                }
            }

            var dirsToDelete = new[]
            {
                Path.Combine(frontendPath, "src", "components", "tenants"),
                Path.Combine(frontendPath, "src", "types", "tenants"),
                Path.Combine(frontendPath, "src", "components", "users", "InviteUserModal"),
            };

            foreach (var dir in dirsToDelete)
            {
                if (Directory.Exists(dir))
                {
                    Directory.Delete(dir, true);
                    Console.WriteLine($"  Deletado diretório: {Path.GetFileName(dir)}");
                }
            }
        }

        private static async Task RewriteAppRouter(string frontendPath)
        {
            string path = Path.Combine(frontendPath, "src", "components", "AppRouter.tsx");
            if (!File.Exists(path)) return;

            string pn = projectName!;
            string content = @"import React from ""react"";
import { BrowserRouter, Routes, Route, Navigate } from ""react-router-dom"";
import { useAuth } from ""../contexts/Auth"";
import { ProtectedRoute } from ""./ProtectedRoute"";
import { DashboardLayout } from ""./Layout/DashboardLayout"";
import { Login } from ""../pages/Login"";
import { Dashboard } from ""../pages/Dashboard"";
import { ROUTES } from ""../utils/constants"";
import Users from ""../pages/Users"";
import ChangePassword from ""../pages/ChangePassword"";

export const AppRouter: React.FC = () => {
  const { user, token, isLoading } = useAuth();

  if (isLoading) {
    return <div>Loading...</div>;
  }

  const getDefaultRedirect = () => {
    if (!user || !token) {
      return ROUTES.LOGIN;
    }
    return ROUTES.DASHBOARD;
  };

  return (
    <BrowserRouter>
      <Routes>
          {/* Public Routes */}
          <Route
            path={ROUTES.LOGIN}
            element={
              user && token ? (
                <Navigate to={getDefaultRedirect()} replace />
              ) : (
                <Login />
              )
            }
          />

          {/* Change Password Route */}
          <Route
            path={ROUTES.CHANGE_PASSWORD}
            element={<ChangePassword />}
          />

          {/* Protected Dashboard Routes */}
          <Route
            path=""/dashboard/*""
            element={
              <ProtectedRoute requireAuth={true}>
                <DashboardLayout>
                  <Routes>
                    <Route index element={<Dashboard />} />
                    <Route path=""analytics"" element={<Dashboard />} />
                    <Route path=""users"" element={<Users />} />
                    <Route path=""reports"" element={<Dashboard />} />
                    <Route path=""security"" element={<Dashboard />} />
                    <Route path=""settings"" element={<Dashboard />} />
                    <Route
                      path=""*""
                      element={<Navigate to=""/dashboard"" replace />}
                    />
                  </Routes>
                </DashboardLayout>
              </ProtectedRoute>
            }
          />

          {/* Root redirect */}
          <Route
            path=""/""
            element={<Navigate to={getDefaultRedirect()} replace />}
          />

          {/* Catch all */}
          <Route
            path=""*""
            element={<Navigate to={getDefaultRedirect()} replace />}
          />
        </Routes>
    </BrowserRouter>
  );
};
";
            await File.WriteAllTextAsync(path, content);
        }

        private static async Task RewriteProtectedRoute(string frontendPath)
        {
            string path = Path.Combine(frontendPath, "src", "components", "ProtectedRoute.tsx");
            if (!File.Exists(path)) return;

            string content = @"import React from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { Box, CircularProgress } from '@mui/material';
import { useAuth } from '../contexts/Auth';
import { ROUTES } from '../utils/constants';

interface ProtectedRouteProps {
  children: React.ReactNode;
  requireAuth?: boolean;
}

export const ProtectedRoute: React.FC<ProtectedRouteProps> = ({
  children,
  requireAuth = true,
}) => {
  const { user, token, isLoading } = useAuth();
  const location = useLocation();

  if (isLoading) {
    return (
      <Box
        sx={{
          display: 'flex',
          justifyContent: 'center',
          alignItems: 'center',
          minHeight: '100vh',
        }}
      >
        <CircularProgress />
      </Box>
    );
  }

  if (requireAuth && (!user || !token)) {
    return <Navigate to={ROUTES.LOGIN} state={{ from: location }} replace />;
  }

  return <>{children}</>;
};
";
            await File.WriteAllTextAsync(path, content);
        }

        private static async Task RewriteAuthContext(string frontendPath)
        {
            string path = Path.Combine(frontendPath, "src", "contexts", "Auth", "AuthContext.tsx");
            if (!File.Exists(path)) return;

            string content = @"import React, { useEffect, useState, useCallback } from 'react';
import type { ReactNode } from 'react';
import { useSnackbar } from 'notistack';
import apiClient from '../../services/apiClient';
import { STORAGE_KEYS } from '../../utils/constants';
import type { AuthContextType, LoginResponseDto, TokensDto, User } from '../../types';
import { AuthContext } from './context';

interface AuthProviderProps {
  children: ReactNode;
}

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const { enqueueSnackbar } = useSnackbar();
  const [user, setUser] = useState<User | null>(null);
  const [token, setToken] = useState<string | null>(null);
  const [refreshToken, setRefreshToken] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

    async function decodeJWT(token: string | null): Promise<User | null> {
    try {
      if (!token) return null;
      const base64Url = token.split('.')[1];
      const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
      const jsonPayload = decodeURIComponent(
        atob(base64)
          .split('')
          .map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
          .join('')
      );

      const payload = JSON.parse(jsonPayload);
      
      const userData = {
        id: payload.sub || payload.userId || payload.id || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'],
        email: payload.email || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'],
        userName: payload.unique_name || payload.userName || payload.name || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'],
        roles: Array.isArray(payload.role) ? payload.role : payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ? [payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']] : [payload.role].filter(Boolean),
      };
      
      return userData;
    } catch (error) {
      console.error('Error decoding JWT:', error);
      return null;
    }
  }

  const logout = useCallback(async (): Promise<void> => {
    try {
      if (token) {
        await apiClient.get('/Auth/Logout');
      }
    } catch (error) {
      console.error('Logout API call failed:', error);
      enqueueSnackbar('Erro ao fazer logout', { variant: 'error' });
    } finally {
      setUser(null);
      setToken(null);
      setRefreshToken(null);
      localStorage.removeItem(STORAGE_KEYS.TOKEN);
      localStorage.removeItem(STORAGE_KEYS.REFRESH_TOKEN);
      localStorage.removeItem(STORAGE_KEYS.USER);
    }
  }, [token, enqueueSnackbar]);

  const refreshTokens = useCallback(async (): Promise<boolean> => {
    try {
      if (!refreshToken) {
        await logout();
        return false;
      }

      const response = await apiClient.get<TokensDto>('/Auth/RefreshToken');
      
      if (response) {
        const { token: newAccessToken, refreshToken: newRefreshToken } = response;
        
        setToken(newAccessToken);
        setRefreshToken(newRefreshToken);
        
        const userData = await decodeJWT(newAccessToken);
        if (userData) {
          setUser(userData);
          
          localStorage.setItem(STORAGE_KEYS.TOKEN, newAccessToken);
          localStorage.setItem(STORAGE_KEYS.REFRESH_TOKEN, newRefreshToken);
          localStorage.setItem(STORAGE_KEYS.USER, JSON.stringify(userData));
        }
        
        return true;
      } else {
        await logout();
        return false;
      }
    } catch (error) {
      console.error('Token refresh failed:', error);
      enqueueSnackbar('Sessão expirada. Faça login novamente.', { variant: 'warning' });
      await logout();
      return false;
    }
  }, [refreshToken, logout, enqueueSnackbar]);

  useEffect(() => {
    const initializeAuth = async () => {
      const storedToken = localStorage.getItem(STORAGE_KEYS.TOKEN);
      const storedRefreshToken = localStorage.getItem(STORAGE_KEYS.REFRESH_TOKEN);

      if (storedToken && storedRefreshToken) {
        try {
          setToken(storedToken);
          setRefreshToken(storedRefreshToken);
          
          const userData = await decodeJWT(storedToken);
          if (userData) {
            setUser(userData);
            localStorage.setItem(STORAGE_KEYS.USER, JSON.stringify(userData));
          } else {
            await refreshTokens();
          }
        } catch (error) {
          console.error('Token validation failed:', error);
          enqueueSnackbar('Erro ao validar token. Faça login novamente.', { variant: 'error' });
          await logout();
        }
      }
      
      setIsLoading(false);
    };

    initializeAuth();
  }, [enqueueSnackbar, refreshTokens, logout]);

  const login = async (email: string, password: string): Promise<{ isNeededChangePassword: boolean }> => {
    try {
      setIsLoading(true);
      const response = await apiClient.post<LoginResponseDto>('/Auth/Login', { email, password });
      
      if (response?.isNeededChangePassword) {
        return { isNeededChangePassword: true };
      }

      if (response?.tokens?.token && response?.tokens?.refreshToken) {
        const { token: accessToken, refreshToken: newRefreshToken } = response.tokens;
        setToken(accessToken);
        setRefreshToken(newRefreshToken);
        const userData = await decodeJWT(accessToken);
        if (userData) {
          setUser(userData);
          localStorage.setItem(STORAGE_KEYS.TOKEN, accessToken);
          localStorage.setItem(STORAGE_KEYS.REFRESH_TOKEN, newRefreshToken);
          localStorage.setItem(STORAGE_KEYS.USER, JSON.stringify(userData));
        }
        return { isNeededChangePassword: false };
      }

      throw new Error('Login failed - no tokens received');
    } catch (error) {
      console.error('Login failed:', error);
      const errorMessage = error instanceof Error ? error.message : 'Erro no login';
      enqueueSnackbar(errorMessage, { variant: 'error' });
      throw error;
    } finally {
      setIsLoading(false);
    }
  };

  const refreshUserFromToken = async (): Promise<void> => {
    try {
      setIsLoading(true);
      const storedToken = localStorage.getItem(STORAGE_KEYS.TOKEN);

      if (storedToken) {
        setToken(storedToken);
        const userData = await decodeJWT(storedToken);
        if (userData) {
          setUser(userData);
          localStorage.setItem(STORAGE_KEYS.USER, JSON.stringify(userData));
        }
      }
    } catch (error) {
      console.error('Error refreshing user from token:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const value: AuthContextType = {
    user,
    token,
    refreshToken,
    isLoading,
    login,
    logout,
    refreshTokens,
    refreshUserFromToken,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};
";
            await File.WriteAllTextAsync(path, content);
        }

        private static async Task RewriteTypesIndex(string frontendPath)
        {
            string path = Path.Combine(frontendPath, "src", "types", "index.ts");
            if (!File.Exists(path)) return;

            string content = $@"export interface User {{
  id: string;
  email: string;
  userName: string;
  roles: string[];
}}
export interface AuthContextType {{
  user: User | null;
  token: string | null;
  refreshToken: string | null;
  isLoading: boolean;
  login: (email: string, password: string) => Promise<{{ isNeededChangePassword: boolean }}>;
  logout: () => void;
  refreshTokens: () => Promise<boolean>;
  refreshUserFromToken: () => Promise<void>;
}}

export interface TokensDto {{
  token: string;
  refreshToken: string;
}}

export interface LoginResponseDto {{
  tokens: TokensDto | null;
  isNeededChangePassword: boolean;
}}

export interface {projectName}Response<T> {{
  isSuccess: boolean;
  message?: string;
  data: T;
}}

export interface LoginInputDto {{
  email: string;
  password: string;
}}

export interface RefreshTokenRequestDto {{
  refreshToken: string;
}}

export type UserRole = 'AdminGlobal' | 'GlobalManager' | 'User';

export interface ApiCallOptions {{
  errorMessage?: string;
  silent?: boolean;
}}
";

            await File.WriteAllTextAsync(path, content);
        }

        private static async Task RewriteConstants(string frontendPath)
        {
            string path = Path.Combine(frontendPath, "src", "utils", "constants.ts");
            if (!File.Exists(path)) return;

            string content = @"export const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'https://localhost:7265/api';

export const ROUTES = {
  LOGIN: '/login',
  CHANGE_PASSWORD: '/change-password',
  DASHBOARD: '/dashboard',
  USERS: '/dashboard/users',
  HOME: '/',
} as const;

export const USER_ROLES = {
  ADMIN_GLOBAL: 'AdminGlobal',
  GLOBAL_MANAGER: 'GlobalManager',
  USER: 'User',
} as const;

export const STORAGE_KEYS = {
  TOKEN: '" + projectName + @"_token',
  REFRESH_TOKEN: '" + projectName + @"_refresh_token',
  USER: 'user_data',
} as const;

export const SystemNotificationsEvents = {
  UpdateNotifications: ""UpdateNotifications""
} as const;
";
            await File.WriteAllTextAsync(path, content);
        }

        private static async Task RewriteAuthHelpers(string frontendPath)
        {
            string path = Path.Combine(frontendPath, "src", "utils", "authHelpers.ts");
            if (!File.Exists(path)) return;

            string content = @"import { USER_ROLES } from './constants';
import type { User } from '../types';

export const isAdminGlobal = (user: User | null): boolean => {
  return user?.roles.includes(USER_ROLES.ADMIN_GLOBAL) ?? false;
};

export const isGlobalManager = (user: User | null): boolean => {
  return user?.roles.includes(USER_ROLES.GLOBAL_MANAGER) ?? false;
};
";
            await File.WriteAllTextAsync(path, content);
        }

        private static async Task RewriteContextsUtils(string frontendPath)
        {
            string path = Path.Combine(frontendPath, "src", "contexts", "Utils.ts");
            if (!File.Exists(path)) return;

            string content = @"import type { User } from ""../types"";
import { USER_ROLES } from ""../utils/constants"";

export const isAdminGlobal = (user: User | null): boolean => {
    return user?.roles.includes(USER_ROLES.ADMIN_GLOBAL) ?? false;
  };
  
  export const isGlobalManager = (user: User | null): boolean => {
    return user?.roles.includes(USER_ROLES.GLOBAL_MANAGER) ?? false;
  };
";
            await File.WriteAllTextAsync(path, content);
        }

        private static async Task RewriteSidebar(string frontendPath)
        {
            string path = Path.Combine(frontendPath, "src", "components", "Layout", "Sidebar.tsx");
            if (!File.Exists(path)) return;

            Console.WriteLine("  Reescrevendo Sidebar sem multi-tenancy...");

            string content = $@"import React, {{ useState }} from 'react';
import {{
  Box,
  Drawer,
  List,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Typography,
  Divider,
  Avatar,
  Chip,
  IconButton,
  Collapse,
}} from '@mui/material';
import {{
  Home,
  Dashboard,
  People,
  Analytics,
  Security,
  ChevronRight,
  AccountCircle,
  ExpandLess,
  ExpandMore,
}} from '@mui/icons-material';
import {{ useAuth }} from '../../contexts/Auth';
import {{ useNavigate }} from 'react-router-dom';
import {{ translate }} from '../../i18n';

interface SidebarProps {{
  mobileOpen: boolean;
  desktopOpen: boolean;
  onMobileClose: () => void;
  onDesktopToggle: () => void;
  drawerWidth: number;
  collapsedWidth: number;
}}

interface MenuItem {{
  text: string;
  icon: React.ReactNode;
  path?: string;
  subItems?: MenuItem[];
  action?: () => void;
}}

const getMenuItems = (): MenuItem[] => [
  {{ text: translate('sidebar.home'), icon: <Home />, path: '/dashboard' }},
  {{ text: translate('sidebar.dashboard'), icon: <Dashboard />, path: '/dashboard/analytics' }},
  {{ text: translate('sidebar.users.title'), icon: <People />, path: '/dashboard/users' }},
  {{ text: translate('sidebar.reports'), icon: <Analytics />, path: '/dashboard/reports' }},
  {{ text: translate('sidebar.security'), icon: <Security />, path: '/dashboard/security' }},
];

export const Sidebar: React.FC<SidebarProps> = ({{ 
  mobileOpen, 
  desktopOpen, 
  onMobileClose, 
  onDesktopToggle, 
  drawerWidth, 
  collapsedWidth 
}}) => {{
  const {{ user, logout }} = useAuth();
  const navigate = useNavigate();
  const [openSubmenus, setOpenSubmenus] = useState<{{ [key: string]: boolean }}>({{}});

  const handleNavigation = (path: string) => {{
    navigate(path);
  }};

  const toggleSubmenu = (itemText: string) => {{
    setOpenSubmenus(prev => ({{
      ...prev,
      [itemText]: !prev[itemText]
    }}));
  }};

  const menuItems = getMenuItems();

  const getDrawerContent = (isCollapsed = false) => (
    <Box sx={{{{ height: '100%', display: 'flex', flexDirection: 'column' }}}}>
      {{/* Header */}}
      <Box sx={{{{ p: 2, display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}}}>
        {{!isCollapsed && (
          <Typography variant=""h6"" noWrap component=""div"" sx={{{{ fontWeight: 600 }}}}>
            {projectName}
          </Typography>
        )}}
        <IconButton 
          onClick={{isCollapsed ? onDesktopToggle : onMobileClose}} 
          sx={{{{ display: {{ sm: isCollapsed ? 'block' : 'none' }} }}}}
        >
          <ChevronRight />
        </IconButton>
      </Box>

      <Divider />

      {{/* User Info */}}
      {{!isCollapsed && (
        <Box sx={{{{ p: 2 }}}}>
          <Box sx={{{{ display: 'flex', alignItems: 'center', mb: 2 }}}}>
            <Avatar sx={{{{ width: 40, height: 40, mr: 2, bgcolor: 'primary.main' }}}}>
              <AccountCircle />
            </Avatar>
            <Box sx={{{{ flex: 1, minWidth: 0 }}}}>
              <Typography variant=""subtitle2"" noWrap>
                {{user?.userName}}
              </Typography>
              <Typography variant=""caption"" color=""text.secondary"" noWrap>
                {{user?.email}}
              </Typography>
            </Box>
          </Box>

          {{/* User Roles */}}
          <Box sx={{{{ display: 'flex', flexWrap: 'wrap', gap: 0.5, mb: 1 }}}}>
            {{user?.roles.map((role) => (
              <Chip
                key={{role}}
                label={{role}}
                size=""small""
                variant=""outlined""
                sx={{{{ fontSize: '0.7rem' }}}}
              />
            ))}}
          </Box>
        </Box>
      )}}

      {{/* Collapsed User Avatar */}}
      {{isCollapsed && (
        <Box sx={{{{ p: 1, display: 'flex', justifyContent: 'center' }}}}>
          <Avatar sx={{{{ width: 40, height: 40, bgcolor: 'primary.main' }}}}>
            <AccountCircle />
          </Avatar>
        </Box>
      )}}

      <Divider />

      {{/* Navigation Menu */}}
      <Box sx={{{{ flex: 1, overflow: 'auto' }}}}>
        <List>
          {{menuItems.map((item) => (
            <React.Fragment key={{item.text}}>
              <ListItem disablePadding>
                <ListItemButton
                  onClick={{() => {{
                    if (item.subItems && !isCollapsed) {{
                      toggleSubmenu(item.text);
                    }} else if (item.path) {{
                      handleNavigation(item.path);
                    }} else if (item.action) {{
                      item.action();
                    }}
                  }}}}
                  sx={{{{
                    minHeight: 48,
                    px: 2.5,
                    '&:hover': {{
                      backgroundColor: 'action.hover',
                    }},
                  }}}}
                >
                  <ListItemIcon sx={{{{ minWidth: isCollapsed ? 'auto' : 40, justifyContent: 'center' }}}}>
                    {{item.icon}}
                  </ListItemIcon>
                  {{!isCollapsed && (
                    <>
                      <ListItemText 
                        primary={{item.text}}
                        primaryTypographyProps={{{{
                          fontSize: '0.875rem',
                          fontWeight: 500,
                        }}}}
                      />
                      {{item.subItems && (
                        openSubmenus[item.text] ? <ExpandLess /> : <ExpandMore />
                      )}}
                    </>
                  )}}
                </ListItemButton>
              </ListItem>
              
              {{/* Submenu */}}
              {{item.subItems && !isCollapsed && (
                <Collapse in={{openSubmenus[item.text]}} timeout=""auto"" unmountOnExit>
                  <List component=""div"" disablePadding>
                    {{item.subItems.map((subItem) => (
                      <ListItem key={{subItem.text}} disablePadding>
                        <ListItemButton
                          onClick={{() => {{
                            if (subItem.path) {{
                              handleNavigation(subItem.path);
                            }} else if (subItem.action) {{
                              subItem.action();
                            }}
                          }}}}
                          sx={{{{
                            pl: 4,
                            minHeight: 40,
                            '&:hover': {{
                              backgroundColor: 'action.hover',
                            }},
                          }}}}
                        >
                          <ListItemIcon sx={{{{ minWidth: 32, justifyContent: 'center' }}}}>
                            {{subItem.icon}}
                          </ListItemIcon>
                          <ListItemText 
                            primary={{subItem.text}}
                            primaryTypographyProps={{{{
                              fontSize: '0.8rem',
                              fontWeight: 400,
                            }}}}
                          />
                        </ListItemButton>
                      </ListItem>
                    ))}}
                  </List>
                </Collapse>
              )}}
            </React.Fragment>
          ))}}
        </List>
      </Box>

      <Divider />

      {{/* Bottom Actions */}}
      <Box sx={{{{ p: 1 }}}}>
        <ListItemButton
          onClick={{logout}}
          sx={{{{ 
            borderRadius: 1,
            color: 'error.main',
            '&:hover': {{
              backgroundColor: 'error.light',
              color: 'error.contrastText',
            }},
          }}}}
        >
          <ListItemIcon sx={{{{ minWidth: isCollapsed ? 'auto' : 40, justifyContent: 'center', color: 'inherit' }}}}>
            <AccountCircle />
          </ListItemIcon>
          {{!isCollapsed && (
            <ListItemText 
              primary={{translate('sidebar.logout')}}
              primaryTypographyProps={{{{
                fontSize: '0.875rem',
              }}}}
            />
          )}}
        </ListItemButton>
      </Box>
    </Box>
  );

  const currentDesktopWidth = desktopOpen ? drawerWidth : collapsedWidth;

  return (
    <Box
      component=""nav""
      sx={{{{ width: {{ sm: currentDesktopWidth }}, flexShrink: {{ sm: 0 }} }}}}
    >
      {{/* Mobile drawer */}}
      <Drawer
        variant=""temporary""
        open={{mobileOpen}}
        onClose={{onMobileClose}}
        ModalProps={{{{
          keepMounted: true,
        }}}}
        sx={{{{
          display: {{ xs: 'block', sm: 'none' }},
          '& .MuiDrawer-paper': {{
            boxSizing: 'border-box',
            width: drawerWidth,
          }},
        }}}}
      >
        {{getDrawerContent(false)}}
      </Drawer>

      {{/* Desktop drawer */}}
      <Drawer
        variant=""permanent""
        sx={{{{
          display: {{ xs: 'none', sm: 'block' }},
          '& .MuiDrawer-paper': {{
            boxSizing: 'border-box',
            width: currentDesktopWidth,
            transition: 'width 0.3s ease',
            overflowX: 'hidden',
          }},
        }}}}
        open
      >
        {{getDrawerContent(!desktopOpen)}}
      </Drawer>
    </Box>
  );
}};
";
            await File.WriteAllTextAsync(path, content);
        }

        private static async Task RewriteHeader(string frontendPath)
        {
            string path = Path.Combine(frontendPath, "src", "components", "Layout", "Header.tsx");
            if (!File.Exists(path)) return;

            Console.WriteLine("  Reescrevendo Header sem multi-tenancy...");

            string content = @"import React from 'react';
import {
  AppBar,
  Toolbar,
  Typography,
  IconButton,
  Box,
  Avatar,
} from '@mui/material';
import {
  Menu as MenuIcon,
  AccountCircle,
} from '@mui/icons-material';
import { NotificationCenter } from '../NotificationCenter';
import LanguageSwitcher from '../common/LanguageSwitcher';
import { translate } from '../../i18n';

interface HeaderProps {
  onMenuClick: () => void;
  onDesktopMenuClick?: () => void;
  drawerWidth: number;
}
export const Header: React.FC<HeaderProps> = ({ onMenuClick, onDesktopMenuClick, drawerWidth }) => {
  return (
    <AppBar
      position=""fixed""
      sx={{
        width: { xs: '100%', sm: `calc(100% - ${drawerWidth}px)` },
        ml: { sm: `${drawerWidth}px` },
        bgcolor: 'background.paper',
        color: 'text.primary',
        boxShadow: '0 1px 3px rgba(0,0,0,0.1)',
        zIndex: (theme) => theme.zIndex.drawer + 1,
        transition: 'width 0.3s ease, margin-left 0.3s ease',
      }}
    >
      <Toolbar>
        {/* Mobile menu button */}
        <IconButton
          color=""inherit""
          aria-label=""open drawer""
          edge=""start""
          onClick={onMenuClick}
          sx={{ mr: 2, display: { sm: 'none' } }}
        >
          <MenuIcon />
        </IconButton>

        {/* Desktop menu button */}
        {onDesktopMenuClick && (
          <IconButton
            color=""inherit""
            aria-label=""toggle drawer""
            edge=""start""
            onClick={onDesktopMenuClick}
            sx={{ mr: 2, display: { xs: 'none', sm: 'block' } }}
          >
            <MenuIcon />
          </IconButton>
        )}

        <Typography variant=""h6"" noWrap component=""div"" sx={{ flexGrow: 1 }}>
          {translate('sidebar.dashboard')}
        </Typography>

        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
          {/* Notifications */}
          <NotificationCenter />

          {/* Language Switcher */}
          <LanguageSwitcher />

          {/* User Avatar */}
          <IconButton color=""inherit"">
            <Avatar sx={{ width: 32, height: 32, bgcolor: 'primary.main' }}>
              <AccountCircle />
            </Avatar>
          </IconButton>
        </Box>
      </Toolbar>
    </AppBar>
  );
};
";
            await File.WriteAllTextAsync(path, content);
        }

        private static async Task RewriteDashboard(string frontendPath)
        {
            string path = Path.Combine(frontendPath, "src", "pages", "Dashboard.tsx");
            if (!File.Exists(path)) return;

            Console.WriteLine("  Reescrevendo Dashboard sem multi-tenancy...");

            string content = @"import React from 'react';
import {
  Box,
  Grid,
  Card,
  CardContent,
  Typography,
  Avatar,
} from '@mui/material';
import {
  TrendingUp,
  People,
  Notifications,
} from '@mui/icons-material';
import { useAuth } from '../contexts/Auth';
import { translate } from '../i18n';

const StatCard: React.FC<{
  title: string;
  value: string;
  icon: React.ReactNode;
  color: string;
  change?: string;
}> = ({ title, value, icon, color, change }) => (
  <Card sx={{ height: '100%' }}>
    <CardContent>
      <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
        <Box>
          <Typography color=""text.secondary"" gutterBottom variant=""overline"">
            {title}
          </Typography>
          <Typography variant=""h4"" component=""div"" sx={{ fontWeight: 600 }}>
            {value}
          </Typography>
          {change && (
            <Typography variant=""body2"" sx={{ color: 'success.main', mt: 1 }}>
              {change}
            </Typography>
          )}
        </Box>
        <Avatar sx={{ bgcolor: color, width: 56, height: 56 }}>
          {icon}
        </Avatar>
      </Box>
    </CardContent>
  </Card>
);

export const Dashboard: React.FC = () => {
  const { user } = useAuth();

  return (
    <Box>
      {/* Welcome Section */}
      <Box sx={{ mb: 4 }}>
        <Typography variant=""h4"" component=""h1"" gutterBottom sx={{ fontWeight: 600 }}>
          {translate('dashboard.title', { userName: user?.userName })}
        </Typography>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, flexWrap: 'wrap' }}>
          <Typography variant=""body1"" color=""text.secondary"">
            {translate('dashboard.description')}
          </Typography>
        </Box>
      </Box>
      <Typography variant=""h6"" color=""text.primary"" sx={{ mb: 2 }}>
        {translate('dashboard.subtitle')}
      </Typography>

      {/* Statistics Cards */}
      <Grid container spacing={3} sx={{ mb: 4 }}>
        <Grid sx={{xs: 12, sm: 6, md: 3}}>
          <StatCard
            title=""Active Users""
            value=""1,234""
            icon={<People />}
            color=""primary.main""
            change=""+12% this month""
          />
        </Grid>
        <Grid sx={{xs: 12, sm: 6, md: 3}}>
          <StatCard
            title=""Revenue""
            value=""$45.2K""
            icon={<TrendingUp />}
            color=""success.main""
            change=""+8% this month""
          />
        </Grid>
        <Grid sx={{xs: 12, sm: 6, md: 3}}>
          <StatCard
            title=""Alerts""
            value=""12""
            icon={<Notifications />}
            color=""warning.main""
          />
        </Grid>
      </Grid>
    </Box>
  );
};
";
            await File.WriteAllTextAsync(path, content);
        }

        private static async Task RewriteDashboardLayout(string frontendPath)
        {
            string path = Path.Combine(frontendPath, "src", "components", "Layout", "DashboardLayout.tsx");
            if (!File.Exists(path)) return;

            Console.WriteLine("  Reescrevendo DashboardLayout sem multi-tenancy...");

            string content = @"import React, { useState } from 'react';
import { Box, Toolbar } from '@mui/material';
import { Header } from './Header';
import { Sidebar } from './Sidebar';

interface DashboardLayoutProps {
  children: React.ReactNode;
}

const DRAWER_WIDTH = 280;
const DRAWER_WIDTH_COLLAPSED = 64;

export const DashboardLayout: React.FC<DashboardLayoutProps> = ({ children }) => {
  const [mobileOpen, setMobileOpen] = useState(false);
  const [desktopOpen, setDesktopOpen] = useState(true);

  const handleDrawerToggle = () => {
    setMobileOpen(!mobileOpen);
  };

  const handleDesktopDrawerToggle = () => {
    setDesktopOpen(!desktopOpen);
  };

  const currentDrawerWidth = desktopOpen ? DRAWER_WIDTH : DRAWER_WIDTH_COLLAPSED;

  return (
    <Box sx={{ display: 'flex', width: '100%', minHeight: '100vh' }}>
      <Header 
        onMenuClick={handleDrawerToggle} 
        onDesktopMenuClick={handleDesktopDrawerToggle}
        drawerWidth={currentDrawerWidth} 
      />
      
      <Sidebar
        mobileOpen={mobileOpen}
        desktopOpen={desktopOpen}
        onMobileClose={handleDrawerToggle}
        onDesktopToggle={handleDesktopDrawerToggle}
        drawerWidth={DRAWER_WIDTH}
        collapsedWidth={DRAWER_WIDTH_COLLAPSED}
      />

      <Box
        component=""main""
        sx={{
          flexGrow: 1,
          p: 3,
          width: { xs: '100%', sm: `calc(100% - ${currentDrawerWidth}px)` },
          minHeight: '100vh',
          bgcolor: 'background.default',
          overflow: 'auto',
          transition: 'width 0.3s ease, margin-left 0.3s ease, padding 0.3s ease',
        }}
      >
        <Toolbar />
        {children}
      </Box>
    </Box>
  );
};
";
            await File.WriteAllTextAsync(path, content);
        }

        private static async Task ModifyApiClient(string frontendPath)
        {
            string path = Path.Combine(frontendPath, "src", "services", "apiClient.ts");
            if (!File.Exists(path)) return;
            string content = await File.ReadAllTextAsync(path);

            // Remove impersonated token handling in request interceptor
            content = Regex.Replace(content,
                @"const impersonatedToken = localStorage\.getItem\(STORAGE_KEYS\.IMPERSONATED_TOKEN\);\s*if \(impersonatedToken\) \{\s*config\.headers\.Authorization = `Bearer \$\{impersonatedToken\}`;\s*\}\s*else if",
                "if");

            // Remove impersonation block in 401 handler
            content = Regex.Replace(content,
                @"\s*// Verifica se existe token de impersonação[\s\S]*?return Promise\.reject\(new Error\('Impersonation session expired'\)\);\s*\}\s*",
                "\n", RegexOptions.Singleline);

            await File.WriteAllTextAsync(path, content);
        }

        private static async Task RewriteUsersPage(string frontendPath)
        {
            string path = Path.Combine(frontendPath, "src", "pages", "Users.tsx");
            if (!File.Exists(path)) return;

            Console.WriteLine("  Reescrevendo Users.tsx sem multi-tenancy...");

            string content = $@"import React, {{ useState, useEffect }} from ""react"";
import {{ Box, Typography, Container, Chip, Stack }} from ""@mui/material"";
import {{ useSnackbar }} from ""notistack"";
import type {{ GridColDef, GridRowParams, GridRowId }} from ""@mui/x-data-grid"";
import type {{ UserDto }} from ""../types/Users"";
import {projectName}DataGrid from ""../components/common/{projectName}DataGrid"";
import EditUserModal from ""../components/users/EditUserModal/EditUserModal"";
import {{ useConfirmation }} from ""../contexts/confirmationContext/ConfirmationProvider"";
import apiClient from ""../services/apiClient"";
import {{ translate }} from ""../i18n"";

export const Users: React.FC = () => {{
  const {{ enqueueSnackbar }} = useSnackbar();
  const confirm = useConfirmation();
  const [users, setUsers] = useState<UserDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("""");
  const [editModalOpen, setEditModalOpen] = useState(false);
  const [selectedUser, setSelectedUser] = useState<UserDto | null>(null);

  useEffect(() => {{
    fetchUsers();
  }}, []);

  const fetchUsers = async () => {{
    try {{
      setLoading(true);
      const usersData = await apiClient.get<UserDto[]>(""/User"");
      setUsers(usersData);
      setError("""");
    }} catch (err) {{
      console.error(""Error fetching users:"", err);
      const errorMessage =
        err instanceof Error ? err.message : ""Erro ao carregar usuários"";
      setError(errorMessage);
    }} finally {{
      setLoading(false);
    }}
  }};

  const handleEditUser = (id: GridRowId) => {{
    const userToEdit = users.find((u) => u.id === id) || null;
    setSelectedUser(userToEdit);
    setEditModalOpen(true);
  }};

  const handleEditSuccess = () => {{
    fetchUsers();
    setEditModalOpen(false);
    setSelectedUser(null);
  }};

  const handleDeleteUser = async (id: GridRowId) => {{
    const result = await confirm({{
      title: ""Excluir Usuário"",
      message: ""Tem certeza que deseja excluir este usuário?"",
    }});
    if (!result) return;

    try {{
      await apiClient.delete(`/User/${{id}}`);
      fetchUsers();
      enqueueSnackbar(""Usuário excluído com sucesso"", {{
        variant: ""success"",
      }});
    }} catch (error) {{
      console.error(""Error deleting user:"", error);
      enqueueSnackbar(""Erro ao excluir usuário"", {{ variant: ""error"" }});
    }}
  }};

  const handleRowClick = (params: GridRowParams) => {{
    console.log(""View user details:"", params.id);
  }};

  const columns: GridColDef[] = [
    {{
      field: ""name"",
      headerName: translate(""usersManagement.usersGrid.columns.name""),
      flex: 1,
      minWidth: 200,
    }},
    {{
      field: ""email"",
      headerName: translate(""usersManagement.usersGrid.columns.email""),
      flex: 1,
      minWidth: 250,
    }},
    {{
      field: ""roles"",
      headerName: translate(""usersManagement.usersGrid.columns.roles""),
      minWidth: 200,
      flex: 1,
      renderCell: (params) => {{
        const roles: string[] = params.value || [];

        if (!roles.length) {{
          return <Chip label=""User"" size=""small"" variant=""outlined"" />;
        }}

        return (
          <Stack direction=""row"" spacing={{0.5}} mt={{2}} sx={{{{ flexWrap: ""wrap"" }}}}>
            {{roles.map((role) => (
              <Chip key={{role}} label={{role}} size=""small"" variant=""outlined"" />
            ))}}
          </Stack>
        );
      }},
    }},
  ];

  const rows = users.map((user) => ({{
    id: user.id,
    name: user.name,
    email: user.email || ""-"",
    roles: user.roles || [],
  }}));

  return (
    <Container maxWidth=""lg"">
      <Box sx={{{{ py: 4 }}}}>
        <Box sx={{{{ mb: 4 }}}}>
          <Typography variant=""h4"" component=""h1"" gutterBottom>
            {{translate(""usersManagement.title"")}}
          </Typography>
          <Typography variant=""body1"" color=""text.secondary"">
            {{translate(""usersManagement.description"")}}
          </Typography>
        </Box>

        <Box sx={{{{ mb: 3 }}}}>
          <{projectName}DataGrid
            title={{translate(""usersManagement.usersGrid.title"")}}
            rows={{rows}}
            columns={{columns}}
            loading={{loading}}
            error={{error}}
            onEdit={{handleEditUser}}
            onDelete={{handleDeleteUser}}
            onRowClick={{handleRowClick}}
            height={{500}}
            pageSize={{10}}
          />
        </Box>

        <EditUserModal
          open={{editModalOpen}}
          user={{selectedUser}}
          onClose={{() => {{
            setEditModalOpen(false);
            setSelectedUser(null);
          }}}}
          onSuccess={{handleEditSuccess}}
        />
      </Box>
    </Container>
  );
}};

export default Users;
";
            await File.WriteAllTextAsync(path, content);
        }

        private static async Task ModifyFrontendUserTypes(string frontendPath)
        {
            string path = Path.Combine(frontendPath, "src", "types", "Users", "index.ts");
            if (!File.Exists(path)) return;

            string content = @"export interface UserDto {
  id: string;
  name: string;
  email?: string;
  roles?: string[];
}
";
            await File.WriteAllTextAsync(path, content);
        }

        private static async Task ModifyFrontendSystemNotificationTypes(string frontendPath)
        {
            string path = Path.Combine(frontendPath, "src", "types", "systemNotifications", "index.ts");
            if (!File.Exists(path)) return;

            string content = @"export interface SystemNotificationDto {
    id: number;
    title: string;
    content: string;
    isRead: boolean;
    createdAt: Date;
}

export interface CreateNotificationDto {
    title: string;
    content: string;
    userIds: string[];
}
";
            await File.WriteAllTextAsync(path, content);
        }

        private static async Task ModifyEditUserModal(string frontendPath)
        {
            string path = Path.Combine(frontendPath, "src", "components", "users", "EditUserModal", "EditUserModal.tsx");
            if (!File.Exists(path)) return;
            string content = await File.ReadAllTextAsync(path);

            // Replace TENANT_ADMIN with just USER in availableRoles
            content = content.Replace(
                "const availableRoles = [USER_ROLES.USER, USER_ROLES.TENANT_ADMIN];",
                "const availableRoles = [USER_ROLES.USER];");

            await File.WriteAllTextAsync(path, content);
        }

        private static async Task ModifyComponentsIndex(string frontendPath)
        {
            string path = Path.Combine(frontendPath, "src", "components", "index.ts");
            if (!File.Exists(path)) return;
            string content = await File.ReadAllTextAsync(path);

            // Remove TenantCreateModal export
            content = RemoveLineContaining(content, "TenantCreateModal");

            await File.WriteAllTextAsync(path, content);
        }

        private static async Task ModifyCreateNotificationModal(string frontendPath)
        {
            string path = Path.Combine(frontendPath, "src", "components", "notifications", "CreateNotificationModal", "CreateNotificationModal.tsx");
            if (!File.Exists(path)) return;
            string content = await File.ReadAllTextAsync(path);

            // Replace tenant translation keys with simpler notification keys
            content = content.Replace("tenantSettings.tenantTabs.systemNotifications.", "notifications.");

            await File.WriteAllTextAsync(path, content);
        }

        // ========================================================================
        // UTILITY METHODS
        // ========================================================================

        private static string RemoveLineContaining(string content, string searchText)
        {
            var lines = content.Split('\n').ToList();
            lines.RemoveAll(line => line.Contains(searchText));
            return string.Join('\n', lines);
        }
    }
}
