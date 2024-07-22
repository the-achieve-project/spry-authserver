using System.Security.Cryptography;
#nullable disable

namespace Spry.AuthServer.Utility
{
    /// <summary>
    ///     This class can generate random strings and supports following settings:
    ///     1) 4 character sets (UpperCase, LowerCase, Numeric and Special characters)
    ///     2) Variable number of the character sets in use
    ///     3) Minimal number of each type of the characters
    ///     4) Pattern driven string generation
    ///     5) Unique string generation
    ///     6) Using each character only once
    ///     It can be easily used for generation of a password or an identificator.
    /// </summary>
    public class StringGenerator
    {
        /// <summary>
        ///     True if characters can be repeated.
        /// </summary>
        public readonly bool repeatCharacters;

        /// <summary>
        ///     True if it's not possible to create similar strings.
        /// </summary>
        public readonly bool uniqueStrings;

        public StringGenerator(bool useUpperCaseCharacters = true,
                               bool useLowerCaseCharacters = true,
                               bool useNumericCharacters = true,
                               bool useSpecialCharacters = true)
        {
            _useUpperCaseCharacters = useUpperCaseCharacters;
            _useLowerCaseCharacters = useLowerCaseCharacters;
            _useNumericCharacters = useNumericCharacters;
            _useSpecialCharacters = useSpecialCharacters;
            _currentGeneralCharacters = new char[0]; // avoiding null exceptions
            UpperCaseCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            LowerCaseCharacters = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
            NumericCharacters = "0123456789".ToCharArray();
            SpecialCharacters = "?@#$%&*".ToCharArray();
            MinUpperCaseCharacters = MinLowerCaseCharacters = MinNumericCharacters = MinSpecialCharacters = 0;
            repeatCharacters = true;
            _patternDriven = false;
            Pattern = "";
            _random = RandomNumberGenerator.Create();
            _existingStrings = [];
        }

        public StringGenerator UseIdentityDefaults()
        {
            this._useLowerCaseCharacters = true;
            this._useNumericCharacters = true;
            this._useUpperCaseCharacters = true;
            this._useSpecialCharacters = true;
            MinUpperCaseCharacters = MinLowerCaseCharacters = MinNumericCharacters = MinSpecialCharacters = 1;

            return this;
        }

        #region character sets managers

        /// <summary>
        ///     True if we need to use upper case characters
        /// </summary>
        public bool UseUpperCaseCharacters
        {
            get { return _useUpperCaseCharacters; }
            set
            {
                if (_currentUpperCaseCharacters != null)
                    _currentGeneralCharacters = _currentGeneralCharacters.Except(_currentUpperCaseCharacters).ToArray();
                if (value)
                    _currentGeneralCharacters = _currentGeneralCharacters.Concat(_currentUpperCaseCharacters).ToArray();
                _useUpperCaseCharacters = value;
            }
        }

        /// <summary>
        ///     Sets or gets upper case character set.
        /// </summary>
        public char[] UpperCaseCharacters
        {
            get { return _currentUpperCaseCharacters; }
            set
            {
                if (UseUpperCaseCharacters)
                {
                    if (_currentUpperCaseCharacters != null)
                        _currentGeneralCharacters = _currentGeneralCharacters.Except(_currentUpperCaseCharacters).ToArray();
                    _currentGeneralCharacters = _currentGeneralCharacters.Concat(value).ToArray();
                }
                _currentUpperCaseCharacters = value;
            }
        }

        /// <summary>
        ///     True if we need to use lower case characters
        /// </summary>
        public bool UseLowerCaseCharacters
        {
            get { return _useLowerCaseCharacters; }
            set
            {
                if (_currentLowerCaseCharacters != null)
                    _currentGeneralCharacters = _currentGeneralCharacters.Except(_currentLowerCaseCharacters).ToArray();
                if (value)
                    _currentGeneralCharacters = _currentGeneralCharacters.Concat(_currentLowerCaseCharacters).ToArray();
                _useLowerCaseCharacters = value;
            }
        }

        /// <summary>
        ///     Sets or gets lower case character set.
        /// </summary>
        public char[] LowerCaseCharacters
        {
            get { return _currentLowerCaseCharacters; }
            set
            {
                if (UseLowerCaseCharacters)
                {
                    if (_currentLowerCaseCharacters != null)
                        _currentGeneralCharacters = _currentGeneralCharacters.Except(_currentLowerCaseCharacters).ToArray();
                    _currentGeneralCharacters = _currentGeneralCharacters.Concat(value).ToArray();
                }
                _currentLowerCaseCharacters = value;
            }
        }

        /// <summary>
        ///     True if we need to use numeric characters
        /// </summary>
        public bool UseNumericCharacters
        {
            get { return _useNumericCharacters; }
            set
            {
                if (_currentNumericCharacters != null)
                    _currentGeneralCharacters = _currentGeneralCharacters.Except(_currentNumericCharacters).ToArray();
                if (value)
                    _currentGeneralCharacters = _currentGeneralCharacters.Concat(_currentNumericCharacters).ToArray();
                _useNumericCharacters = value;
            }
        }

        /// <summary>
        ///     Sets or gets numeric character set.
        /// </summary>
        public char[] NumericCharacters
        {
            get { return _currentNumericCharacters; }
            set
            {
                if (UseNumericCharacters)
                {
                    if (_currentNumericCharacters != null)
                        _currentGeneralCharacters = _currentGeneralCharacters.Except(_currentNumericCharacters).ToArray();
                    _currentGeneralCharacters = _currentGeneralCharacters.Concat(value).ToArray();
                }
                _currentNumericCharacters = value;
            }
        }

        /// <summary>
        ///     True if we need to use special characters
        /// </summary>
        public bool UseSpecialCharacters
        {
            get { return _useSpecialCharacters; }
            set
            {
                if (_currentSpecialCharacters != null)
                    _currentGeneralCharacters = _currentGeneralCharacters.Except(_currentSpecialCharacters).ToArray();
                if (value)
                    _currentGeneralCharacters = _currentGeneralCharacters.Concat(_currentSpecialCharacters).ToArray();
                _useSpecialCharacters = value;
            }
        }

        /// <summary>
        ///     Sets or gets special character set.
        /// </summary>
        public char[] SpecialCharacters
        {
            get { return _currentSpecialCharacters; }
            set
            {
                if (UseSpecialCharacters)
                {
                    if (_currentSpecialCharacters != null)
                        _currentGeneralCharacters = _currentGeneralCharacters.Except(_currentSpecialCharacters).ToArray();
                    _currentGeneralCharacters = _currentGeneralCharacters.Concat(value).ToArray();
                }
                _currentSpecialCharacters = value;
            }
        }

        #endregion

        #region character limits

        /// <summary>
        ///     Sets or gets minimal number of the upper case characters in generated strings.
        /// </summary>
        public int MinUpperCaseCharacters { get; set; }

        /// <summary>
        ///     Sets or gets minimal number of the lower case characters in generated strings.
        /// </summary>
        public int MinLowerCaseCharacters { get; set; }

        /// <summary>
        ///     Sets or gets minimal number of the numeric characters in generated strings.
        /// </summary>
        public int MinNumericCharacters { get; set; }

        /// <summary>
        ///     Sets or gets minimal number of the special characters in generated strings.
        /// </summary>
        public int MinSpecialCharacters { get; set; }

        #endregion

        #region pattern

        private string _pattern;

        /// <summary>
        ///     Defines the pattern to be followed to generate a string.
        ///     This value is ignored if it equals empty string.
        ///     Patterns are:
        ///     L - for upper case letter
        ///     l - for lower case letter
        ///     n - for number
        ///     s - for special character
        ///     * - for any character
        /// </summary>
        private string Pattern
        {
            get { return _pattern; }
            set
            {
                if (!value.Equals(String.Empty))
                    _patternDriven = true;
                else
                    _patternDriven = false;
                _pattern = value;
            }
        }

        #endregion

        #region generators

        /// <summary>
        ///     Generate a string which follows the pattern.
        ///     Possible characters are:
        ///     L - for upper case letter
        ///     l - for lower case letter
        ///     n - for number
        ///     s - for special character
        ///     * - for any character
        /// </summary>
        /// <param name="pattern">The pattern to follow while generation</param>
        /// <returns>A random string which follows the pattern</returns>
        public string Generate(string pattern)
        {
            this.Pattern = pattern;
            string res = GenerateString(pattern.Length);
            this.Pattern = "";
            return res;
        }

        /// <summary>
        ///     Generate a string of a variable length from MinLength to MaxLength. The possible
        ///     character sets should be defined before calling this function.
        /// </summary>
        /// <param name="minLength">Minimal length of a string</param>
        /// <param name="maxLength">Maximal length of a string</param>
        /// <returns>A random string from the selected range of length</returns>
        public string Generate(int minLength, int maxLength)
        {
            if (maxLength < minLength)
                throw new ArgumentException("Maximal length should be grater than minumal");
            int length = minLength + (GetRandomInt() % (maxLength - minLength));
            return GenerateString(length);
        }

        /// <summary>
        ///     Generate a string of a fixed length. The possible
        ///     character sets should be defined before calling this function.
        /// </summary>
        /// <param name="fixedLength">The length of a string</param>
        /// <returns>A random string of the desirable length</returns>
        public string Generate(int fixedLength)
        {
            return GenerateString(fixedLength);
        }

        /// <summary>
        ///     Main generation method which chooses the algorithm to use for the generation.
        ///     It checks some exceptional situations as well.
        /// </summary>
        private string GenerateString(int length)
        {
            if (length == 0)
                throw new ArgumentException("You can't generate a string of a zero length");
            if (!UseUpperCaseCharacters && !UseLowerCaseCharacters && !UseNumericCharacters && !UseSpecialCharacters)
                throw new ArgumentException("There should be at least one character set in use");
            if (!repeatCharacters && (_currentGeneralCharacters.Length < length))
                throw new ArgumentException("There is not enough characters to create a string without repeats");
            string result = ""; // This string will contain the result
            if (_patternDriven)
            {
                // Using the pattern to generate a string
                result = PatternDrivenAlgo(Pattern);
            }
            else if (MinUpperCaseCharacters == 0 && MinLowerCaseCharacters == 0 &&
                     MinNumericCharacters == 0 && MinSpecialCharacters == 0)
            {
                // Using the simpliest algorithm in this case
                result = SimpleGenerateAlgo(length);
            }
            else
            {
                // Paying attention to limits
                result = GenerateAlgoWithLimits(length);
            }
            // Support for unique strings
            // Recursion, but possibility of the stack overflow is low for big strings (> 3 chars).
            if (uniqueStrings && _existingStrings.Contains(result))
                return GenerateString(length);
            AddExistingString(result); // Saving history
            return result;
        }

        /// <summary>
        ///     Generate a random string following the pattern
        /// </summary>
        private string PatternDrivenAlgo(string pattern)
        {
            string result = "";
            var characters = new List<char>();
            foreach (char character in pattern)
            {
                char newChar = ' ';
                switch (character)
                {
                    case 'L':
                        {
                            newChar = GetRandomCharFromArray(_currentUpperCaseCharacters, characters);
                            break;
                        }
                    case 'l':
                        {
                            newChar = GetRandomCharFromArray(_currentLowerCaseCharacters, characters);
                            break;
                        }
                    case 'n':
                        {
                            newChar = GetRandomCharFromArray(_currentNumericCharacters, characters);
                            break;
                        }
                    case 's':
                        {
                            newChar = GetRandomCharFromArray(_currentSpecialCharacters, characters);
                            break;
                        }
                    case '*':
                        {
                            newChar = GetRandomCharFromArray(_currentGeneralCharacters, characters);
                            break;
                        }
                    default:
                        {
                            throw new Exception("The character '" + character + "' is not supported");
                        }
                }
                characters.Add(newChar);
                result += newChar;
            }
            return result;
        }

        /// <summary>
        ///     The simpliest algorithm of the random string generation. It doesn't pay attention to
        ///     limits and patterns.
        /// </summary>
        private string SimpleGenerateAlgo(int length)
        {
            string result = "";
            // No special limits
            for (int i = 0; i < length; i++)
            {
                char newChar = _currentGeneralCharacters[GetRandomInt() % _currentGeneralCharacters.Length];
                if (!repeatCharacters && result.Contains(newChar))
                {
                    do
                    {
                        newChar = _currentGeneralCharacters[GetRandomInt() % _currentGeneralCharacters.Length];
                    } while (result.Contains(newChar));
                }
                result += newChar;
            }
            return result;
        }

        /// <summary>
        ///     Generate a random string with specified number of minimal characters of each character set.
        /// </summary>
        private string GenerateAlgoWithLimits(int length)
        {
            // exceptional situations
            if (MinUpperCaseCharacters + MinLowerCaseCharacters +
                MinNumericCharacters + MinSpecialCharacters > length)
            {
                throw new ArgumentException("Sum of MinUpperCaseCharacters, MinLowerCaseCharacters," +
                                            " MinNumericCharacters and MinSpecialCharacters is greater than length");
            }
            if (!repeatCharacters && (MinUpperCaseCharacters > _currentUpperCaseCharacters.Length))
                throw new ArgumentException("Can't generate a string with this number of MinUpperCaseCharacters");
            if (!repeatCharacters && (MinLowerCaseCharacters > _currentLowerCaseCharacters.Length))
                throw new ArgumentException("Can't generate a string with this number of MinLowerCaseCharacters");
            if (!repeatCharacters && (MinNumericCharacters > _currentNumericCharacters.Length))
                throw new ArgumentException("Can't generate a string with this number of MinNumericCharacters");
            if (!repeatCharacters && (MinSpecialCharacters > _currentSpecialCharacters.Length))
                throw new ArgumentException("Can't generate a string with this number of MinSpecialCharacters");
            int allowedNumberOfGeneralChatacters = length - MinUpperCaseCharacters - MinLowerCaseCharacters
                                                   - MinNumericCharacters - MinSpecialCharacters;

            string result = "";
            // generation character set in order to support unique characters
            var characters = new List<char>();

            // adding chars to an array
            for (int i = 0; i < MinUpperCaseCharacters; i++)
                characters.Add(GetRandomCharFromArray(UpperCaseCharacters, characters));
            for (int i = 0; i < MinLowerCaseCharacters; i++)
                characters.Add(GetRandomCharFromArray(LowerCaseCharacters, characters));
            for (int i = 0; i < MinNumericCharacters; i++)
                characters.Add(GetRandomCharFromArray(NumericCharacters, characters));
            for (int i = 0; i < MinSpecialCharacters; i++)
                characters.Add(GetRandomCharFromArray(SpecialCharacters, characters));
            for (int i = 0; i < allowedNumberOfGeneralChatacters; i++)
                characters.Add(GetRandomCharFromArray(_currentGeneralCharacters, characters));

            // generating result
            for (int i = 0; i < length; i++)
            {
                int position = GetRandomInt() % characters.Count;
                char currentChar = characters[position];
                characters.RemoveAt(position);
                result += currentChar;
            }
            return result;
        }

        #endregion

        /// <summary>
        ///     Adding the string to the history array to support unique string generation.
        /// </summary>
        public void AddExistingString(string s)
        {
            _existingStrings.Add(s);
        }

        #region misc tools

        /// <summary>
        ///     A 16bit integer number generator.
        /// </summary>
        /// <returns>A random integer value from 0 to 65576</returns>
        private int GetRandomInt()
        {
            var buffer = new byte[2]; // 16 bit = 2^16 = 65576 (more than necessary)
            _random.GetNonZeroBytes(buffer);
            int index = BitConverter.ToInt16(buffer, 0);
            if (index < 0)
                index = -index; // manage negative random values
            return index;
        }

        /// <summary>
        ///     Get a random char from the selected array of chars. It pays attention to
        ///     the RepeatCharacters flag.
        /// </summary>
        /// <param name="array">Source of symbols</param>
        /// <param name="existentItems">Existing symbols. Can be null if RepeatCharacters flag is false</param>
        /// <returns>A random character from the array</returns>
        private char GetRandomCharFromArray(char[] array, List<char> existentItems)
        {
            char character = ' ';
            do
            {
                character = array[GetRandomInt() % array.Length];
            } while (!repeatCharacters && existentItems.Contains(character));
            return character;
        }

        #endregion

        #region internal state

        private readonly List<string> _existingStrings; // History
        private readonly RandomNumberGenerator _random;
        private char[] _currentGeneralCharacters; // All used characters
        private char[] _currentLowerCaseCharacters;
        private char[] _currentNumericCharacters;
        private char[] _currentSpecialCharacters;
        private char[] _currentUpperCaseCharacters;
        private bool _patternDriven;
        private bool _useLowerCaseCharacters;

        private bool _useNumericCharacters;
        private bool _useSpecialCharacters;

        private bool _useUpperCaseCharacters;

        #endregion
    }
}
