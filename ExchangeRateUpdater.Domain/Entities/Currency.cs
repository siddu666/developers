namespace ExchangeRateUpdater.Domain.Entities
{
    public class Currency
    {
        public string Code { get; }

        public Currency(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentException("Currency code cannot be null or whitespace.", nameof(code));
            }
            Code = code;
        }

        public override string ToString() => Code;

        public override bool Equals(object? obj)
        {
            if (obj is not Currency other) return false;
            return Code.Equals(other.Code, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(Code);
        }
    }
}
