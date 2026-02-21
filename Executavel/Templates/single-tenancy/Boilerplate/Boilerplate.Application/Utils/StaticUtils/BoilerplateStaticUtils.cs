namespace Boilerplate.Application.Utils.StaticUtils
{
    public static class BoilerplateStaticUtils
    {
        public static void ApplyChanges<TEntity, TDto>(TEntity entity, TDto dto)
        {
            var entityProps = typeof(TEntity).GetProperties();
            var dtoProps = typeof(TDto).GetProperties();

            foreach (var dtoProp in dtoProps)
            {
                if (dtoProp.Name == "Id")
                    continue;

                var dtoValue = dtoProp.GetValue(dto);
                if (dtoValue == null)
                    continue;

                var entityProp = entityProps.FirstOrDefault(p =>
                    p.Name == dtoProp.Name &&
                    p.PropertyType.IsAssignableFrom(dtoProp.PropertyType) &&
                    p.CanWrite);

                if (entityProp != null)
                {
                    var entityValue = entityProp.GetValue(entity);

                    if (!Equals(entityValue, dtoValue))
                    {
                        entityProp.SetValue(entity, dtoValue);
                    }
                }
            }
        }
    }
}
