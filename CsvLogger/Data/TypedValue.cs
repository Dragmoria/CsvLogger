namespace CsvLogger.Data
{
    public class TypedValue<T>
    {
        private T _value;

        public TypedValue()
        {
            _value = default;
        }

        public T GetValue() => _value;

        public void SetValue(T newValue)
        {
            _value = newValue;
        }

        public override string ToString()
        {
            return _value?.ToString();
        }
    }
}