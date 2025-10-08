# Boilerplate Customizer

Este executável automatiza a customização do boilerplate de backend, aplicando as seguintes transformações baseadas nas escolhas do usuário:

## Funcionalidades

### 1. **Substituição de Nomes do Projeto**
- Substitui todas as ocorrências de "Boilerplate" pelo nome do projeto fornecido
- Mantém consistência em namespaces, referências internas, nomes de arquivos e diretórios
- Suporta variações de case (Boilerplate, boilerplate, BOILERPLATE)

### 2. **Criação de Entidades Iniciais**
- Usa `Entity1` como template para gerar novas entidades
- Cria automaticamente:
  - Classes de entidade no Domain
  - Controllers na API
  - Services e Interfaces na Application
  - Job Executors e Schedulers
  - Registros no DependencyInjection

### 3. **Configuração de Multi-tenancy**
- **Se habilitado**: Mantém toda a estrutura de multi-tenancy existente
- **Se desabilitado**: Remove automaticamente:
  - Entidade `Tenant` e suas referências
  - Propriedades `TenantId` de outras entidades
  - Lógica de multi-tenant no DbContext
  - Query filters relacionados a tenant

### 4. **Geração de JWT SecretKey**
- Gera uma chave secreta aleatória de 256 bits
- Atualiza automaticamente o `appsettings.json`
- Garante segurança criptográfica adequada

### 5. **Criação do Projeto Final**
- Copia todo o boilerplate para uma nova pasta
- **NUNCA modifica ou apaga a pasta original do boilerplate**
- Todas as transformações ocorrem apenas na nova pasta

## Como Usar

### Pré-requisitos
- .NET 9.0 ou superior
- Pasta `Boilerplate` no mesmo nível do executável

### Estrutura de Pastas Esperada
```
📁 Sua Pasta de Trabalho/
├── 📁 Boilerplate/          # Pasta com o boilerplate original
│   └── 📁 Boilerplate/      # Projeto do boilerplate
└── 📁 Executavel/           # Esta pasta com o executável
    ├── BoilerplateCustomizer.exe
    └── README.md
```

### Execução

1. **Compile o projeto** (se necessário):
   ```bash
   dotnet build
   ```

2. **Execute o programa**:
   ```bash
   dotnet run
   ```
   ou
   ```bash
   BoilerplateCustomizer.exe
   ```

3. **Siga as instruções interativas**:
   - Digite o nome do projeto (ex: "MinhaEmpresa")
   - Informe quantas entidades iniciais deseja criar
   - Para cada entidade, forneça o nome (ex: "Produto", "Cliente")
   - Escolha se deseja ativar multi-tenancy (s/n)

### Exemplo de Execução
```
=== Boilerplate Customizer ===

Digite o nome do projeto: MinhaEmpresa
Quantas entidades iniciais deseja criar? 2
Digite o nome da entidade 1: Produto
Digite o nome da entidade 2: Cliente
Deseja ativar multi-tenancy? (n): n

Copiando arquivos do boilerplate...
Aplicando customizações...
Substituindo nomes do projeto...
Gerando JWT Secret Key...
Criando 2 entidade(s)...
Removendo funcionalidades de multi-tenancy...

✅ Projeto 'MinhaEmpresa' criado com sucesso!
📁 Localização: C:\Projetos\MinhaEmpresa

Próximos passos:
1. Abra o projeto no Visual Studio
2. Execute as migrations do Entity Framework
3. Configure a string de conexão no appsettings.json
```

## Resultado

Após a execução, você terá:

- **Nova pasta** com o nome do seu projeto
- **Namespaces atualizados** para o nome do projeto
- **Entidades criadas** baseadas no template Entity1
- **JWT SecretKey segura** gerada automaticamente
- **Multi-tenancy configurado** conforme sua escolha
- **Boilerplate original preservado** e intocado

## Validações

O executável inclui validações para:
- **Nome do projeto**: Deve começar com letra e conter apenas letras, números e underscore
- **Nome das entidades**: Deve começar com letra maiúscula e conter apenas letras e números
- **Existência da pasta Boilerplate**: Verifica se o boilerplate está disponível
- **Sobrescrita de projetos**: Pergunta antes de sobrescrever pastas existentes

## Tratamento de Erros

- Avisos são exibidos para arquivos que não puderam ser processados
- O processo continua mesmo com erros não-críticos
- Erros críticos interrompem a execução com mensagem clara

## Arquitetura Suportada

O executável foi projetado para trabalhar com a seguinte arquitetura de boilerplate:

- **API Layer**: Controllers, Program.cs, appsettings
- **Application Layer**: Services, Interfaces, Job Executors
- **Domain Layer**: Entities, Interfaces
- **Infrastructure Layer**: Data Context, Repositories, IoC
- **Job Server**: Hangfire wrappers

---

**Importante**: Sempre mantenha um backup do seu boilerplate original antes de fazer modificações manuais.
