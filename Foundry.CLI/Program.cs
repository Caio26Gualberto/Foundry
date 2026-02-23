using System.IO.Compression;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace BoilerplateCustomizer
{
    class Program
    {
        private static string? projectName;
        private static List<string> entityNames = new();
        private static bool enableMultitenancy = true;
        private static bool enableRedis = true;
        private static string? destinationPath;
        private static string? tempExtractPath;

        private static string ExtractTemplates()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream("templates.zip")
                ?? throw new InvalidOperationException("Embedded templates.zip not found in assembly.");

            tempExtractPath = Path.Combine(Path.GetTempPath(), "BoilerplateCustomizer", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempExtractPath);

            using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
            archive.ExtractToDirectory(tempExtractPath);

            return tempExtractPath;
        }

        private static void CleanupTempFiles()
        {
            if (tempExtractPath != null && Directory.Exists(tempExtractPath))
            {
                try
                {
                    Directory.Delete(tempExtractPath, true);
                }
                catch
                {
                    // Best-effort cleanup
                }
            }
        }

        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Boilerplate Customizer ===");
            Console.WriteLine();

            try
            {
                Console.WriteLine("Extracting templates...");
                string templatesPath = ExtractTemplates();

                // Collect user input
                await CollectUserInput();

                // Select template folder
                string templateSubfolder = enableMultitenancy ? "multi-tenancy" : "single-tenancy";
                string backendTemplatePath = Path.Combine(templatesPath, templateSubfolder, "Boilerplate");
                string frontendTemplatePath = Path.Combine(templatesPath, templateSubfolder, "react-boilerplate");

                if (!Directory.Exists(backendTemplatePath))
                {
                    Console.WriteLine($"Error: Backend template not found at: {backendTemplatePath}");
                    return;
                }

                // Determine base directory
                string baseDir = destinationPath ?? Directory.GetCurrentDirectory();

                // Create parent project folder
                string parentFolder = Path.Combine(baseDir, projectName!);

                if (Directory.Exists(parentFolder))
                {
                    Console.Write($"Folder '{parentFolder}' already exists. Overwrite? (y/n): ");
                    var overwrite = Console.ReadLine()?.ToLower();
                    if (overwrite != "y" && overwrite != "yes")
                    {
                        Console.WriteLine("Operation cancelled.");
                        return;
                    }
                    Directory.Delete(parentFolder, true);
                }

                Directory.CreateDirectory(parentFolder);

                // Copy backend template
                string backendPath = Path.Combine(parentFolder, projectName!);
                Console.WriteLine("Copying backend template...");
                CopyDirectory(backendTemplatePath, backendPath);

                // Copy frontend template
                string? frontendPath = null;
                if (Directory.Exists(frontendTemplatePath))
                {
                    Console.WriteLine("Copying frontend template...");
                    frontendPath = Path.Combine(parentFolder, $"react-{projectName!.ToLower()}");
                    CopyDirectory(frontendTemplatePath, frontendPath, skipNodeModules: true);
                }
                else
                {
                    Console.WriteLine("Warning: Frontend template not found. Frontend will not be copied.");
                }

                // Apply customizations
                Console.WriteLine("Applying customizations...");
                await ApplyCustomizations(backendPath, frontendPath);

                Console.WriteLine();
                Console.WriteLine($"Project '{projectName}' created successfully!");
                Console.WriteLine($"  Project folder: {parentFolder}");
                Console.WriteLine($"  Backend: {backendPath}");
                if (frontendPath != null)
                    Console.WriteLine($"  Frontend: {frontendPath}");
                Console.WriteLine();
                Console.WriteLine("Next steps:");
                Console.WriteLine("1. Open the backend project in Visual Studio");
                Console.WriteLine("2. Run Entity Framework migrations");
                Console.WriteLine("3. Configure the connection string in appsettings.json");
                if (frontendPath != null)
                {
                    Console.WriteLine($"4. In the frontend, run: npm install && npm run dev");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
            finally
            {
                CleanupTempFiles();
                Console.WriteLine();
                Console.WriteLine("Press any key to close...");
                Console.ReadKey();
            }
        }

        private static async Task CollectUserInput()
        {
            // Project name
            Console.Write("Enter the project name: ");
            projectName = Console.ReadLine()?.Trim();

            while (string.IsNullOrEmpty(projectName) || !IsValidProjectName(projectName))
            {
                Console.WriteLine("Invalid name. Use only letters, numbers and underscore, starting with a letter.");
                Console.Write("Enter the project name: ");
                projectName = Console.ReadLine()?.Trim();
            }

            // Multi-tenancy
            Console.Write("Enable multi-tenancy? (y/n): ");
            var multitenancyResponse = Console.ReadLine()?.ToLower();
            enableMultitenancy = multitenancyResponse == "y" || multitenancyResponse == "yes";

            // Redis
            Console.Write("Enable Redis for caching? (y/n): ");
            var redisResponse = Console.ReadLine()?.ToLower();
            if (redisResponse != "y" && redisResponse != "yes")
            {
                Console.WriteLine();
                Console.WriteLine("Redis provides distributed caching and rate limiting with better performance.");
                Console.WriteLine("It's recommended for production and multi-instance deployments.");
                Console.WriteLine("Without Redis, the application will use InMemoryRateLimitService instead.");
                Console.WriteLine();
                Console.Write("Are you sure you don't want Redis? (y/n): ");
                var confirmNoRedis = Console.ReadLine()?.ToLower();
                enableRedis = confirmNoRedis != "y" && confirmNoRedis != "yes";
            }

            // Number of entities
            Console.Write("How many initial entities do you want to create? ");
            int entityCount;
            while (!int.TryParse(Console.ReadLine(), out entityCount) || entityCount < 0)
            {
                Console.Write("Enter a valid number (0 or greater): ");
            }

            // Entity names
            for (int i = 0; i < entityCount; i++)
            {
                Console.Write($"Enter the name of entity {i + 1}: ");
                string? entityName = Console.ReadLine()?.Trim();

                while (string.IsNullOrEmpty(entityName) || !IsValidEntityName(entityName))
                {
                    Console.WriteLine("Invalid name. Use only letters, starting with uppercase.");
                    Console.Write($"Enter the name of entity {i + 1}: ");
                    entityName = Console.ReadLine()?.Trim();
                }

                entityNames.Add(entityName);
            }

            // Destination path
            Console.WriteLine();
            Console.Write("Enter the full path to create the project (or press Enter for default): ");
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
            // 1. Replace project names
            await ReplaceProjectNames(projectPath);
            if (frontendPath != null)
                await ReplaceFrontendProjectNames(frontendPath);

            // 2. Generate JWT Secret Key
            await GenerateJwtSecretKey(projectPath);

            // 3. Create entities from Entity1 template
            await CreateEntitiesFromTemplate(projectPath);

            // 4. Remove Entity1 references from DbContext (only if entities were created)
            if (entityNames.Count > 0)
            {
                await RemoveEntity1FromDbContext(projectPath);
            }

            // 5. Remove Redis if disabled
            if (!enableRedis)
            {
                await RemoveRedis(projectPath);
            }
        }

        private static async Task ReplaceFrontendProjectNames(string frontendPath)
        {
            Console.WriteLine("Replacing project names in frontend...");

            var filesToProcess = Directory.GetFiles(frontendPath, "*", SearchOption.AllDirectories)
                .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}node_modules{Path.DirectorySeparatorChar}"))
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
                    Console.WriteLine($"Warning: Could not process {filePath}: {ex.Message}");
                }
            }

            // Rename directories containing "Boilerplate"
            var dirsToRename = Directory.GetDirectories(frontendPath, "*Boilerplate*", SearchOption.AllDirectories)
                .Where(d => !d.Contains($"{Path.DirectorySeparatorChar}node_modules{Path.DirectorySeparatorChar}"))
                .OrderByDescending(d => d.Length)
                .ToList();
            foreach (var dir in dirsToRename)
            {
                string dirName = Path.GetFileName(dir);
                string newDirName = dirName.Replace("Boilerplate", projectName);
                string newPath = Path.Combine(Path.GetDirectoryName(dir)!, newDirName);
                if (dir != newPath && Directory.Exists(dir))
                {
                    Directory.Move(dir, newPath);
                    Console.WriteLine($"  Renamed directory: {dirName} -> {newDirName}");
                }
            }
        }

        private static async Task ReplaceProjectNames(string projectPath)
        {
            Console.WriteLine("Replacing project names...");

            var filesToProcess = Directory.GetFiles(projectPath, "*", SearchOption.AllDirectories)
                .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}") && !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"))
                .Where(f => Path.GetExtension(f).ToLower() is ".cs" or ".csproj" or ".sln" or ".json")
                .ToList();

            foreach (string filePath in filesToProcess)
            {
                try
                {
                    string content = await File.ReadAllTextAsync(filePath);

                    content = content.Replace("Boilerplate", projectName);
                    content = content.Replace("boilerplate", projectName!.ToLower());
                    content = content.Replace("BOILERPLATE", projectName!.ToUpper());

                    await File.WriteAllTextAsync(filePath, content);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Could not process {filePath}: {ex.Message}");
                }
            }

            // Rename directories and files
            await RenameDirectories(projectPath);
        }

        private static async Task RenameDirectories(string projectPath)
        {
            var directories = Directory.GetDirectories(projectPath, "*", SearchOption.AllDirectories)
                .Where(d => Path.GetFileName(d).Contains("Boilerplate"))
                .OrderByDescending(d => d.Length)
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
                    Console.WriteLine($"Warning: Could not rename directory {dir}: {ex.Message}");
                }
            }

            // Rename files
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
                    Console.WriteLine($"Warning: Could not rename file {file}: {ex.Message}");
                }
            }
        }

        private static async Task GenerateJwtSecretKey(string projectPath)
        {
            Console.WriteLine("Generating JWT Secret Key...");

            using var rng = RandomNumberGenerator.Create();
            byte[] keyBytes = new byte[32];
            rng.GetBytes(keyBytes);
            string secretKey = Convert.ToHexString(keyBytes).ToLower();

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
                    Console.WriteLine($"Warning: Could not update JWT SecretKey: {ex.Message}");
                }
            }
        }

        // ========================================================================
        // ENTITY CREATION FROM TEMPLATE
        // ========================================================================

        private static async Task CreateEntitiesFromTemplate(string projectPath)
        {
            if (entityNames.Count == 0) return;

            Console.WriteLine($"Creating {entityNames.Count} entity(ies)...");

            foreach (string entityName in entityNames)
            {
                await CreateEntityFromEntity1Template(projectPath, entityName);
            }

            // After creating all entities, remove Entity1 files
            await RemoveEntity1Files(projectPath);
        }

        private static async Task CreateEntityFromEntity1Template(string projectPath, string entityName)
        {
            // Find all files containing "Entity1"
            var entity1Files = Directory.GetFiles(projectPath, "*", SearchOption.AllDirectories)
                .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}") && !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") && !f.Contains($"{Path.DirectorySeparatorChar}.vs{Path.DirectorySeparatorChar}") && !f.Contains($"{Path.DirectorySeparatorChar}packages{Path.DirectorySeparatorChar}"))
                .Where(f => Path.GetExtension(f).ToLower() is ".cs" or ".csproj" or ".json")
                .Where(f => Path.GetFileName(f).Contains("Entity1"))
                .ToList();

            // Also find .cs files that reference Entity1 in content
            var additionalFiles = Directory.GetFiles(projectPath, "*.cs", SearchOption.AllDirectories)
                .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}") && !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") && !f.Contains($"{Path.DirectorySeparatorChar}.vs{Path.DirectorySeparatorChar}") && !f.Contains($"{Path.DirectorySeparatorChar}packages{Path.DirectorySeparatorChar}"))
                .Where(f => !Path.GetFileName(f).Contains("Entity1"))
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
                catch { }
            }

            foreach (string templateFile in entity1Files)
            {
                try
                {
                    string content = await File.ReadAllTextAsync(templateFile);

                    if (!content.Contains("Entity1")) continue;

                    // Create new file based on template
                    string newFileName = Path.GetFileName(templateFile).Replace("Entity1", entityName);
                    string newFilePath = Path.Combine(Path.GetDirectoryName(templateFile)!, newFileName);

                    // Replace Entity1 with new entity name
                    string newContent = content.Replace("Entity1", entityName);
                    newContent = newContent.Replace("entity1", entityName.ToLower());

                    // Ensure entity inherits from EntityBase
                    if (templateFile.Contains($"{Path.DirectorySeparatorChar}Domain{Path.DirectorySeparatorChar}Entities{Path.DirectorySeparatorChar}") && templateFile.EndsWith(".cs"))
                    {
                        if (!newContent.Contains(": EntityBase") && newContent.Contains($"class {entityName}"))
                        {
                            newContent = newContent.Replace($"class {entityName}", $"class {entityName} : EntityBase");
                        }
                    }

                    await File.WriteAllTextAsync(newFilePath, newContent);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Could not create entity {entityName} from {templateFile}: {ex.Message}");
                }
            }

            // Update DependencyInjection.cs
            await UpdateDependencyInjection(projectPath, entityName);
        }

        private static async Task RemoveEntity1Files(string projectPath)
        {
            Console.WriteLine("Removing Entity1 template files...");

            var entity1Files = Directory.GetFiles(projectPath, "*Entity1*", SearchOption.AllDirectories)
                .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}") && !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") && !f.Contains($"{Path.DirectorySeparatorChar}.vs{Path.DirectorySeparatorChar}") && !f.Contains($"{Path.DirectorySeparatorChar}packages{Path.DirectorySeparatorChar}"))
                .ToList();

            foreach (string file in entity1Files)
            {
                try
                {
                    File.Delete(file);
                    Console.WriteLine($"  Removed: {Path.GetFileName(file)}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Could not remove {file}: {ex.Message}");
                }
            }

            // Also remove Entity1 references in remaining files
            await RemoveEntity1References(projectPath);
        }

        private static async Task RemoveEntity1References(string projectPath)
        {
            var filesToProcess = Directory.GetFiles(projectPath, "*.cs", SearchOption.AllDirectories)
                .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}") && !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") && !f.Contains($"{Path.DirectorySeparatorChar}.vs{Path.DirectorySeparatorChar}") && !f.Contains($"{Path.DirectorySeparatorChar}packages{Path.DirectorySeparatorChar}"))
                .ToList();

            foreach (string filePath in filesToProcess)
            {
                try
                {
                    string content = await File.ReadAllTextAsync(filePath);
                    bool modified = false;

                    // Remove using statements for Entity1
                    if (content.Contains($"using {projectName}.Domain.Entities.Entity1"))
                    {
                        content = Regex.Replace(content, $@"using {projectName}\.Domain\.Entities\.Entity1.*\n", "");
                        modified = true;
                    }

                    // Remove Entity1 registrations in DependencyInjection
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
                    Console.WriteLine($"Warning: Could not process {filePath} for Entity1 removal: {ex.Message}");
                }
            }
        }

        private static async Task RemoveEntity1FromDbContext(string projectPath)
        {
            string dbContextPath = Path.Combine(projectPath, $"{projectName}.Infra.Data", "Context", $"{projectName}DbContext.cs");
            if (!File.Exists(dbContextPath)) return;

            string content = await File.ReadAllTextAsync(dbContextPath);
            bool modified = false;

            // Remove DbSet<Entity1> line
            if (content.Contains("DbSet<Entity1>"))
            {
                content = RemoveLineContaining(content, "DbSet<Entity1>");
                modified = true;
            }

            // Remove Entity1 using statement if present
            if (content.Contains("Entity1"))
            {
                content = Regex.Replace(content, @".*using.*Entity1.*\n", "");
                modified = true;
            }

            if (modified)
            {
                await File.WriteAllTextAsync(dbContextPath, content);
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

                    // Add service registration
                    string serviceRegistration = $"            services.AddScoped<I{entityName}Service, {entityName}Service>();";
                    string jobSchedulerRegistration = $"            services.AddScoped<I{entityName}JobScheduler, {entityName}Wrapper>();";
                    string jobExecutorRegistration = $"            services.AddScoped<I{entityName}JobExecutor, {entityName}Executor>();";

                    // Find where to insert new registrations
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
                    Console.WriteLine($"Warning: Could not update DependencyInjection for {entityName}: {ex.Message}");
                }
            }
        }

        // ========================================================================
        // REDIS REMOVAL
        // ========================================================================

        private static async Task RemoveRedis(string projectPath)
        {
            Console.WriteLine("Removing Redis configuration...");

            // 1. Delete Redis folder
            string redisFolder = Path.Combine(projectPath, $"{projectName}.Infra.Data", "Redis");
            if (Directory.Exists(redisFolder))
            {
                Directory.Delete(redisFolder, true);
                Console.WriteLine("  Deleted Redis folder");
            }

            // 2. Delete Caching folder
            string cachingFolder = Path.Combine(projectPath, $"{projectName}.Infra.Data", "Services", "Caching");
            if (Directory.Exists(cachingFolder))
            {
                Directory.Delete(cachingFolder, true);
                Console.WriteLine("  Deleted Caching folder");
            }

            // 3. Delete RateLimitService.cs (keep InMemoryRateLimitService.cs)
            string rateLimitServicePath = Path.Combine(projectPath, $"{projectName}.Infra.Data", "Services", "RateLimit", "RateLimitService.cs");
            if (File.Exists(rateLimitServicePath))
            {
                File.Delete(rateLimitServicePath);
                Console.WriteLine("  Deleted RateLimitService.cs");
            }

            // 4. Delete docker-compose.yml
            string dockerComposePath = Path.Combine(projectPath, "docker-compose.yml");
            if (File.Exists(dockerComposePath))
            {
                File.Delete(dockerComposePath);
                Console.WriteLine("  Deleted docker-compose.yml");
            }

            // 5. Modify DependencyInjection.cs - Remove Redis code block and related usings
            string diPath = Path.Combine(projectPath, $"{projectName}.Infra.IoC", "DependencyInjection.cs");
            if (File.Exists(diPath))
            {
                try
                {
                    string content = await File.ReadAllTextAsync(diPath);

                    // Remove Redis-related using statements
                    content = RemoveLineContaining(content, $"using {projectName}.Infra.Data.Redis;");
                    content = RemoveLineContaining(content, $"using {projectName}.Infra.Data.Services.RateLimit;");
                    content = RemoveLineContaining(content, $"using {projectName}.Infra.Data.Services.Caching;");
                    content = RemoveLineContaining(content, "using StackExchange.Redis;");

                    // Replace the Redis conditional block with just InMemoryRateLimitService
                    string redisBlockPattern = @"\s*var redisSection = configuration\.GetSection\(""Redis""\);.*?services\.AddScoped<IRateLimitService, InMemoryRateLimitService>\(\);\s*\}";
                    content = Regex.Replace(content, redisBlockPattern, "\n            services.AddScoped<IRateLimitService, InMemoryRateLimitService>();", RegexOptions.Singleline);

                    // Remove CacheService injection
                    content = RemoveLineContaining(content, "services.AddScoped<ICacheService, CacheService>();");

                    await File.WriteAllTextAsync(diPath, content);
                    Console.WriteLine("  Updated DependencyInjection.cs");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Could not update DependencyInjection.cs: {ex.Message}");
                }
            }

            // 6. Modify appsettings.Example.json - Remove Redis section
            string appsettingsPath = Path.Combine(projectPath, $"{projectName}.Api", "appsettings.Example.json");
            if (File.Exists(appsettingsPath))
            {
                try
                {
                    string content = await File.ReadAllTextAsync(appsettingsPath);
                    
                    // Remove the Redis JSON section
                    string redisJsonPattern = @",?\s*""Redis"":\s*\{[^}]*\}";
                    content = Regex.Replace(content, redisJsonPattern, "");
                    
                    // Clean up any double commas that might result
                    content = Regex.Replace(content, @",\s*,", ",");
                    content = Regex.Replace(content, @",\s*\}", "}");

                    await File.WriteAllTextAsync(appsettingsPath, content);
                    Console.WriteLine("  Updated appsettings.Example.json");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Could not update appsettings.Example.json: {ex.Message}");
                }
            }

            // 7. Modify .csproj - Remove StackExchange.Redis package reference
            string csprojPath = Path.Combine(projectPath, $"{projectName}.Infra.Data", $"{projectName}.Infra.Data.csproj");
            if (File.Exists(csprojPath))
            {
                try
                {
                    string content = await File.ReadAllTextAsync(csprojPath);
                    content = RemoveLineContaining(content, "StackExchange.Redis");
                    await File.WriteAllTextAsync(csprojPath, content);
                    Console.WriteLine("  Updated .csproj (removed StackExchange.Redis)");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Could not update .csproj: {ex.Message}");
                }
            }
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
