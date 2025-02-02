namespace Domain.Common
{
    public class Timeline<T> where T : ICloneable, new()
    {
        private readonly List<T> _entries = new();

        public IReadOnlyList<T> Entries => _entries;

        public int Count => _entries.Count;

        public T AddEntry(T entry)
        {
            _entries.Add(entry);
            return entry;
        }

        public T AddEntry()
        {
            T newEntry = _entries.Count == 0 ? new T() : (T)_entries[^1].Clone();
            _entries.Add(newEntry);
            return newEntry;
        }

        public T Last() => _entries.Count > 0 ? _entries[^1] : throw new InvalidOperationException("Timeline is empty");

        public T Get(int index)
        {
            if (index < 0 || index >= _entries.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
            }
            return _entries[index];
        }
    }
}
