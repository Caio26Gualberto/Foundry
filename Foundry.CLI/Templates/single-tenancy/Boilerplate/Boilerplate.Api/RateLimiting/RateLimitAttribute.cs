namespace Boilerplate.Api.RateLimiting
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class RateLimitAttribute : Attribute
    {
        public Type PolicyType { get; }
        public RateLimitAttribute(Type policyType)
        {
            PolicyType = policyType;
        }
    }
}
