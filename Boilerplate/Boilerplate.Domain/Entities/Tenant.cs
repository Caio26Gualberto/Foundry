namespace Boilerplate.Domain.Entities
{
    public class Tenant : EntityBase //<-- Se o usuário não quer multitenancy, pode remover isso
    {
        public string Name { get; set; }
        public List<User> Users { get; set; } = new();
    }
}
