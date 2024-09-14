namespace CsvLogger.Data
{
    public class TypedValue<T>
    {
        private T _value = default;

        public T GetValue() => _value;

        public T Value
        {
            get => _value;
            set => _value = value;
        }

        public void SetValue(T newValue)
        {
            _value = newValue;
        }

        public override string ToString()
        {
            return _value?.ToString();
        }

        // Implicit conversion from T to TypedValue<T>
        public static implicit operator TypedValue<T>(T value)
        {
            return new TypedValue<T> { Value = value };
        }

        // Implicit conversion from TypedValue<T> to T
        public static implicit operator T(TypedValue<T> typedValue)
        {
            return typedValue._value;
        }
    }
}