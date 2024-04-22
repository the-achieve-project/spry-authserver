namespace Spry.Identity.Utility
{
    public class Hasher
    {
        private readonly int _spin;
        private List<int> _workingArray;

        public Hasher(long originalInteger, int spin = 0)
        {
            _spin = spin;
            string zeroPad = originalInteger.ToString().PadLeft(10, '0');
            _workingArray = zeroPad.Select(c => int.Parse(c.ToString())).ToList();
        }
        public string Hash()
        {
            Swap();
            Scatter();
            return CompletedString();
        }

        public string ReverseHash()
        {
            Unscatter();
            Unswap();
            return CompletedString();
        }

        private string CompletedString()
        {
            return string.Join("", _workingArray);
        }

        private List<int> SwapperMap(int index)
        {
            List<int> array = Enumerable.Range(0, 10).ToList();
            return Enumerable.Range(0, 10).Select(i =>
            {
                array.RotateLeft(index + i ^ _spin);
                return array.Pop();
            }).ToList();
        }

        private void Swap()
        {
            _workingArray = _workingArray.Select((digit, index) =>
            {
                return SwapperMap(index)[digit];
            }).ToList();
        }

        private void Unswap()
        {
            _workingArray = _workingArray.Select((digit, index) =>
            {
                return SwapperMap(index).LastIndexOf(digit);
            }).ToList();
        }

        private void Scatter()
        {
            int sumOfDigits = _workingArray.Sum();
            _workingArray = Enumerable.Range(0, 10).Select(i =>
            {
                _workingArray.RotateLeft(_spin ^ sumOfDigits);
                return _workingArray.Pop();
            }).ToList();
        }

        private void Unscatter()
        {
            var scatteredArray = new List<int>(_workingArray);
            int sumOfDigits = scatteredArray.Sum();
            _workingArray = [];
            Enumerable.Range(0, 10).ToList().ForEach(i =>
            {
                _workingArray.Add(scatteredArray.Pop());
                _workingArray.RotateRight(sumOfDigits ^ _spin);
            });
        }
    }

    public static class ScatterSwapExtensions
    {
        public static void RotateLeft<T>(this List<T> list, int count)
        {
            if (count < 0) throw new ArgumentException("count must be non-negative");

            count %= list.Count;
            if (count == 0) return;

            var first = list.Take(count).ToList();
            list.RemoveRange(0, count);
            list.AddRange(first);
        }

        public static void RotateRight<T>(this List<T> list, int count)
        {
            if (count < 0) throw new ArgumentException("count must be non-negative");

            count %= list.Count;
            if (count == 0) return;

            var last = list.TakeLast(count).ToList();
            list.RemoveRange(list.Count - count, count);
            list.InsertRange(0, last);
        }

        public static T Pop<T>(this List<T> list)
        {
            var item = list.Last();
            list.RemoveAt(list.Count - 1);
            return item;
        }
    }
}
