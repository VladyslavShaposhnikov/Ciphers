using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using szyfry.Models;

namespace szyfry.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }
    
    private string alfabet = "aąbcćdeęfghijklłmnńoópqrsśtuvwxyzźż"; // length = 35
    private int y = 4; // random number
    private int z = 29586034; // random number

    public IActionResult Index()
    {
        return View();
    }
    
    public IActionResult Caesar()
    {
        return View();
    }
    [HttpPost]
    public IActionResult Caesar(string option, string text, int key)
    {
        if (option == "encrypt")
        {
            @ViewBag.Result = EncryptText(text.ToLower(), key);
            return View("CeaserResult");
        }
        else if (option == "decrypt")
        {
            @ViewBag.Result = DecryptText(text.ToLower(), key);
            return View("CeaserResult");
        }
        return View();
    }

    private string EncryptText(string input, int key)
    {
        string res = "";
        for (int i = 0; i < input.Length; i++)
        {
            int resultIndex;
            if (alfabet.Contains(input[i]))
            {
                int index = alfabet.IndexOf(input[i]);
                resultIndex = index + key;
                if (resultIndex >= alfabet.Length)
                {
                    resultIndex -= alfabet.Length;
                }
                res += alfabet[resultIndex];
            }
        }
        return res;
    }
    
    private string DecryptText(string input, int key)
    {
        string res = "";
        for (int i = 0; i < input.Length; i++)
        {
            int resultIndex;
            if (alfabet.Contains(input[i]))
            {
                int index = alfabet.IndexOf(input[i]);
                resultIndex = index - key;
                if (resultIndex < 0)
                {
                    resultIndex += alfabet.Length;
                }
                res += alfabet[resultIndex];
            }
        }
        return res;
    }

    public IActionResult Pobilius()
    {
        char[,] tab = RandomKeyGeneration(alfabet);
        
        string tabToString = "";
        for (int i = 0; i < tab.GetLength(0); i++)
        {
            for (int j = 0; j < tab.GetLength(1); j++)
            {
                tabToString += tab[i, j].ToString();
            }
        }
        @ViewBag.TabString = tabToString;
        return View();
    }

    [HttpPost]
    public IActionResult Pobilius(string option, string text, string key)
    {
        text = text.ToLower();
        char[,] tab = new char[5,7];
        int helper = 0;
        for (int i = 0; i < tab.GetLength(0); i++)
        {
            for (int j = 0; j < tab.GetLength(1); j++)
            {
                tab[i, j] = key[helper];
                helper++;
            }
        }
        if (option == "encrypt")
        {
            string result = "";
            for (int i = 0; i < text.Length; i++)
            {
                if (alfabet.Contains(text[i]))
                {
                    result += FindInt(text[i], tab); // replace chars with ints
                }
            }
            @ViewBag.PobiliusResult = Cipher(result);
            return View("PobiliusResult");
        }
        else if (option == "decrypt")
        {
            string backToNormal = GetStringOfInts(BigInteger.Parse(text));
            @ViewBag.PobiliusResult = BackToNormalWords(backToNormal, tab);
        }
        return View("PobiliusResult");
    }
    
    string BackToNormalWords(string toNorm, char[,] tab)
    {
        string toNormal = "";
        for (int i = 0; i < toNorm.Length; i += 2)
        {
            int one = int.Parse(toNorm[i].ToString());
            int two = int.Parse(toNorm[i + 1].ToString());
            toNormal += tab[--one, --two];
        }
        return toNormal;
    }
    
    string GetStringOfInts(BigInteger unknown)
    {
        BigInteger known = unknown - z;
        known /= y;
        known /= 2;
        return known.ToString();
    }
    BigInteger Cipher(string result)
    {
        BigInteger x;
        BigInteger unknown;

        try
        {
            checked
            {
                x = BigInteger.Parse(result);
                unknown = x * 2;
                unknown *= y;
                unknown += z;
                return unknown;
            }
        }
        catch (OverflowException)
        {
            Console.WriteLine("too long secret...");
            return 0;
        }
    }
    
    string FindInt(char ch, char[,] tab)
    {
        string resultStr = "";

        for (int i = 0; i < tab.GetLength(0); i++)
        {
            for (int j = 0; j < tab.GetLength(1); j++)
            {
                if (ch == tab[i, j])
                {
                    resultStr += (i + 1);
                    resultStr += (j + 1);
                    return resultStr;
                }
            }
        }
        return resultStr;
    }
    
    char[,] RandomKeyGeneration(string alf)
    {
        char[,] tabKey = new char[5, 7];

        char[] charArr = alf.ToCharArray();

        Random rand = new Random();

        int alfLength = alf.Length;

        for (int i = 0; i < tabKey.GetLength(0); i++)
        {
            for (int j = 0; j < tabKey.GetLength(1); j++)
            {
                char insert = charArr[rand.Next(0, alfLength)]; // random element

                MoveIndexInArray(charArr, insert); // moving selected element to the end of array

                alfLength--; // decrease length by 1 (to not select this element in next iteration)

                tabKey[i, j] = insert;


            }
        }
        return tabKey;
    }
    
    void MoveIndexInArray(char[] charArr, char insertedChar)
    {
        int moveIndex = Array.IndexOf(charArr, insertedChar);
        char temp = charArr[moveIndex];

        for (int item = moveIndex; item < charArr.Length - 1; item++)
        {
            charArr[item] = charArr[item + 1];
        }

        charArr[charArr.Length - 1] = temp;
    }

    public IActionResult Playfair()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Playfair(string option, string text, string key)
    {
        if (option == "encrypt")
        {
            string noDuplicates = RemoveDuplicates(key.ToLower().Replace(" ", ""));
            
            string newalf = noDuplicates; // vriable to hold new alfabet with key

            for (int i = 0; i < alfabet.Length; i++) 
            {
                if (!newalf.Contains(alfabet[i]))
                {
                    newalf += alfabet[i];
                }
            }
            
            char[,] arr = ToArr(newalf); // array with key
            
            string encode = text.ToLower().Replace(" ", ""); // text to encode with no whitespaces
            
            StringBuilder stringBuilder = new StringBuilder();

            for(int i = 0; i < encode.Length; i++)
            {
                stringBuilder.Append(encode[i]);
                if (i != encode.Length - 1 && encode[i] == encode[i + 1] && encode[i] != 'x')
                {
                    stringBuilder.Append('x');
                }
                else if (i != encode.Length - 1 && encode[i] == encode[i + 1] && encode[i] == 'x')
                {
                    stringBuilder.Append('y');
                }
            }

            if (stringBuilder.Length % 2 == 1 && stringBuilder[stringBuilder.Length-1] != 'x')
            {
                stringBuilder.Append('x');
            }
            else if (stringBuilder.Length % 2 == 1 && stringBuilder[stringBuilder.Length - 1] == 'x')
            {
                stringBuilder.Append('y');
            }
            
            string str = stringBuilder.ToString();

            Dictionary<char, int[]> dict = new Dictionary<char, int[]>();           // dictionary to hold each char from alfabet as a key 
                                                                                // and array of ints as value where first int is row and second is column
            for (int i = 0; i < arr.GetLength(0); i++)
            {
                for(int j = 0; j < arr.GetLength(1); j++)
                {
                    dict[arr[i, j]] = new int[] { i, j };
                }
            }

            StringBuilder encodedString = new StringBuilder();

            for (int i = 0; i <= str.Length-2; i += 2)
            {
                char c = str[i]; // first char
                char cc = str[i+1]; // second char

                if (dict[c][0] == dict[cc][0]) // for rows
                {
                    if (dict[c][1] + 1 > arr.GetLength(1) - 1) // is column exists
                    {
                        encodedString.Append(arr[dict[c][0], 0]);
                    }
                    else
                    { 
                        encodedString.Append(arr[dict[c][0], dict[c][1] + 1]);
                    }

                    if (dict[cc][1] + 1 > arr.GetLength(1) - 1) // is column exists
                    {
                        encodedString.Append(arr[dict[cc][0], 0]);
                    }
                    else
                    {
                        encodedString.Append(arr[dict[cc][0], dict[cc][1] + 1]);
                    }
                }
                else if (dict[c][1] == dict[cc][1])
                {
                    if (dict[c][0] + 1 > arr.GetLength(0) - 1) // is raw exists
                    {
                        encodedString.Append(arr[0, dict[c][1]]);
                    }
                    else
                    {
                        encodedString.Append(arr[dict[c][0] + 1, dict[c][1]]);
                    }

                    if (dict[cc][0] + 1 > arr.GetLength(0) - 1) // is raw exists
                    {
                        encodedString.Append(arr[0, dict[cc][1]]);
                    }
                    else
                    {
                        encodedString.Append(arr[dict[cc][0] + 1, dict[cc][1]]);
                    }
                }
                else
                {
                    encodedString.Append(arr[dict[c][0], dict[cc][1]]);
                    encodedString.Append(arr[dict[cc][0], dict[c][1]]);
                }
            }
            @ViewBag.Playfair = encodedString.ToString();
        }
        else if (option == "decrypt")
        {
            for (int i = 0; i < alfabet.Length; i++) 
            {
                if (!key.Contains(alfabet[i]))
                {
                    key += alfabet[i];
                }
            }
            
            char[,] arr = ToArr(key); // array with key
            
            string encode = text.ToLower().Replace(" ", ""); // text to encode with no whitespaces

            Dictionary<char, int[]> dict = new Dictionary<char, int[]>();           // dictionary to hold each char from alfabet as a key 
                                                                                // and array of ints as value where first int is row and second is column
            for (int i = 0; i < arr.GetLength(0); i++)
            {
                for(int j = 0; j < arr.GetLength(1); j++)
                {
                    dict[arr[i, j]] = new int[] { i, j };
                }
            }

            StringBuilder encodedString = new StringBuilder();

            for (int i = 0; i <= encode.Length-2; i += 2)
            {
                char c = encode[i]; // first char
                char cc = encode[i+1]; // second char

                if (dict[c][0] == dict[cc][0]) // for rows
                {
                    if (dict[c][1] - 1 < 0) // is column exists
                    {
                        encodedString.Append(arr[dict[c][0], arr.GetLength(1) - 1]);
                    }
                    else
                    { 
                        encodedString.Append(arr[dict[c][0], dict[c][1] - 1]);
                    }

                    if (dict[cc][1] - 1 < 0) // is column exists
                    {
                        encodedString.Append(arr[dict[cc][0], arr.GetLength(1) - 1]);
                    }
                    else
                    {
                        encodedString.Append(arr[dict[cc][0], dict[cc][1] - 1]);
                    }
                }
                else if (dict[c][1] == dict[cc][1])
                {
                    if (dict[c][0] - 1 < 0) // is raw exists
                    {
                        encodedString.Append(arr[arr.GetLength(0) - 1, dict[c][1]]);
                    }
                    else
                    {
                        encodedString.Append(arr[dict[c][0] - 1, dict[c][1]]);
                    }

                    if (dict[cc][0] - 1 < 0) // is raw exists
                    {
                        encodedString.Append(arr[arr.GetLength(0) - 1, dict[cc][1]]);
                    }
                    else
                    {
                        encodedString.Append(arr[dict[cc][0] - 1, dict[cc][1]]);
                    }
                }
                else
                {
                    encodedString.Append(arr[dict[c][0], dict[cc][1]]);
                    encodedString.Append(arr[dict[cc][0], dict[c][1]]);
                }
            }
            @ViewBag.Playfair = encodedString.ToString();
        }
        return View("PlayfairResult");
    }
    
    char[,] ToArr (string alf)
    {
        char[,] tab = new char[5, 7];
        int helper = 0;
        for (int i = 0; i < tab.GetLength(0); i++)
        {
            for (int j = 0; j < tab.GetLength(1); j++)
            {
                tab[i, j] = alf[helper];
                helper++;
            }
        }
        return tab;
    }
    
    string RemoveDuplicates(string input)
    {
        HashSet<char> seen = new HashSet<char>();
        StringBuilder result = new StringBuilder();

        foreach (char c in input)
        {
            if(!seen.Contains(c) && c != ' ')
            {
                seen.Add(c);
                result.Append(c);
            }
        }
        return result.ToString();
    }

    public IActionResult Vigener()
    {
        return View();
    }
    
    [HttpPost]
    public IActionResult Vigener(string option, string text, string key)
    {
        char[,] arr = VigenerArr();
        
        key = key.ToLower().Replace(" ", "");
        text = text.ToLower().Replace(" ", "");
        
        StringBuilder sb = new StringBuilder(); // variable to hold new keyword

        int textLen = text.Length;
        int keyLen = key.Length;

        if (textLen > keyLen)
        {
            int mult = textLen / keyLen;
            int rest = textLen % keyLen;

            for(int i = 0; i < mult; i++)
            {
                sb.Append(key);
            }
            sb.Append(key.Substring(0,rest));
        }
        else if (textLen == keyLen)
        {
            sb.Append(key);
        }
        else
        {
            sb.Append(key.Substring(0, textLen));
        }
        
        if (option == "encrypt")
        {
            StringBuilder res = new StringBuilder();

            for (int i = 0; i < textLen; i++)
            {
                int indexOne = alfabet.IndexOf(text[i]);
                int indexTwo = alfabet.IndexOf(sb[i]);
                res.Append(arr[indexOne, indexTwo]);
            }
            @ViewBag.Vigener = res.ToString();
        }
        else if (option == "decrypt")
        {
            StringBuilder res = new StringBuilder();

            for (int i = 0; i < textLen; i++)
            {
                for (int j = 0; j < arr.GetLength(1); j++)
                {
                    if (arr[j, alfabet.IndexOf(sb[i])] == text[i])
                    {
                        res.Append(arr[j, 0]);
                        break;
                    }
                }
            }
            @ViewBag.Vigener = res.ToString();
        }
        return View("VigenerResult");
    }

    public char[,] VigenerArr()
    {
        char[,] arr = new char[alfabet.Length,alfabet.Length];

        for (int i = 0; i < arr.GetLength(0); i++)
        {
            for (int j = 0; j < arr.GetLength(1); j++)
            {
                arr[i, j] = alfabet[j];
            }
            alfabet += alfabet[0];
            alfabet = alfabet.Substring(1);
        }
        return arr;
    }

    public IActionResult RSA()
    {
        return View();
    }

    [HttpPost]
    public IActionResult RSA(string option, string text)
    {
        int p = 99989;
        int q = 99991;

        BigInteger n = (BigInteger)p * q;
        BigInteger euler = (BigInteger)(p - 1) * (q - 1);
        BigInteger e = 65537;

        BigInteger d = ModularInverse(e, euler);

        BigInteger[] pub = { n, e };
        BigInteger[] priv = { n, d };
        
        StringBuilder sb = new StringBuilder();
        
        if (option == "encrypt")
        {
            foreach (char c in text.ToLower().Replace(" ", ""))
            {
                BigInteger m = (BigInteger)c;
                BigInteger encrypted = BigInteger.ModPow(m, pub[1], pub[0]);
                sb.Append(encrypted.ToString() + " "); 
            }
            ViewBag.RSA = sb.ToString();
            @ViewBag.RSA = sb.ToString();
        }
        else if (option == "decrypt")
        {
            string[] encryptedValues = text.Split(" ", StringSplitOptions.RemoveEmptyEntries);

            foreach (string value in encryptedValues)
            {
                // Obliczanie m = c^d mod n
                BigInteger c = BigInteger.Parse(value);
                BigInteger decrypted = BigInteger.ModPow(c, priv[1], priv[0]);
                sb.Append((char)decrypted); // Konwersja na znak
            }
            ViewBag.RSA = sb.ToString();
        }
        return View("RSAResalt");
    }
    
    static BigInteger ModularInverse(BigInteger e, BigInteger phi)
    {
        BigInteger t = 0, newT = 1;
        BigInteger r = phi, newR = e;

        while (newR != 0)
        {
            BigInteger quotient = r / newR;

            // Aktualizuj wartości t i r
            (t, newT) = (newT, t - quotient * newT);
            (r, newR) = (newR, r - quotient * newR);
        }

        // Jeśli NWD(e, phi) != 1, odwrotność nie istnieje
        if (r > 1) throw new ArgumentException("e i φ(n) nie są względnie pierwsze");

        // Upewnij się, że wynik jest dodatni
        if (t < 0) t += phi;

        return t;
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}