using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

class Program
{
    string filePath2 = "C:\\Users\\users\\OneDrive\\Documentos\\Ps.txt";

    static void Main()
    {
        Program program = new Program();
        string filePath = "C:\\Users\\users\\OneDrive\\Documentos\\contraseñas.txt";

        if (!File.Exists(filePath))
        {
            Console.WriteLine("No accounts file found. Please create an account.");
            Console.WriteLine("Enter your username:");
            string nameUsu = Console.ReadLine();
            Console.WriteLine("Enter your password:");
            string passwordUsu = ReadPassword();

            using (StreamWriter sw = File.AppendText(filePath))
            {
                sw.WriteLine(nameUsu);
                sw.WriteLine(passwordUsu);
            }
            Console.WriteLine("Account created successfully.");
        }

        while (true)
        {
            Console.WriteLine("Login");
            Console.WriteLine("Enter your username:");
            string a = Console.ReadLine();
            Console.WriteLine("Enter your password:");
            string b = ReadPassword();

            if (VerifyUser(filePath, a, b))
            {
                while (true)
                {
                    Console.WriteLine("1. Add Password\n2. Search Password\n3. Modify Password\n4. Exit");
                    string input = Console.ReadLine();

                    int c;
                    if (int.TryParse(input, out c))
                    {
                        try
                        {
                            if (c == 1)
                            {
                                program.AddPassword();
                            }
                            else if (c == 2)
                            {
                                Console.WriteLine("Enter the description of the password you want to search:");
                                string description = Console.ReadLine();
                                string encryptedPassword = program.GetEncryptedPassword(description);
                                if (encryptedPassword != null)
                                {
                                    string result = program.SearchPassword(encryptedPassword);
                                    Console.WriteLine("Decrypted Password: " + result);
                                }
                                else
                                {
                                    Console.WriteLine("Description not found.");
                                }
                            }
                            else if (c == 3)
                            {
                                program.ModifyPassword();
                            }
                            else if (c == 4)
                            {
                                break;
                            }
                            else
                            {
                                Console.WriteLine("Invalid input");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"An error occurred: {ex.Message}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid input");
                        Console.WriteLine("Please enter a valid number.");
                        Console.Clear();
                    }
                }
                break;
            }
            else
            {
                Console.WriteLine("Incorrect username or password. Please try again.");
            }
        }
    }

    static bool VerifyUser(string filePath, string user, string password)
    {
        string[] lines = File.ReadAllLines(filePath);
        for (int i = 0; i < lines.Length; i += 2)
        {
            if (lines[i] == user && lines[i + 1] == password)
            {
                return true;
            }
        }
        return false;
    }

    public void AddPassword()
    {
        Console.WriteLine("Enter description:");
        string description = Console.ReadLine();
        Console.WriteLine("Enter password:");
        string password = ReadPassword();

        byte[] data = UTF8Encoding.UTF8.GetBytes(password);
        using (MD5 md5 = MD5.Create())
        using (TripleDES tripleDES = TripleDES.Create())
        {
            tripleDES.Key = md5.ComputeHash(UTF8Encoding.UTF8.GetBytes(password));
            tripleDES.Mode = CipherMode.ECB;

            ICryptoTransform transform = tripleDES.CreateEncryptor();
            byte[] result = transform.TransformFinalBlock(data, 0, data.Length);

            using (StreamWriter sw = File.AppendText(filePath2))
            {
                sw.WriteLine($"{description}:{Convert.ToBase64String(result)}");
            }
        }

        Console.WriteLine("Password added and encrypted.");
    }

    public string SearchPassword(string encryptedPassword)
    {
        byte[] data = Convert.FromBase64String(encryptedPassword);

        using (MD5 md5 = MD5.Create())
        using (TripleDES tripleDES = TripleDES.Create())
        {
            tripleDES.Key = md5.ComputeHash(UTF8Encoding.UTF8.GetBytes(encryptedPassword));
            tripleDES.Mode = CipherMode.ECB;

            ICryptoTransform transform = tripleDES.CreateDecryptor();
            byte[] result = transform.TransformFinalBlock(data, 0, data.Length);

            return UTF8Encoding.UTF8.GetString(result);
        }
    }

    public string GetEncryptedPassword(string description)
    {
        string[] lines = File.ReadAllLines(filePath2);
        foreach (string line in lines)
        {
            if (line.StartsWith(description + ":"))
            {
                return line.Substring(description.Length + 1);
            }
        }
        return null;
    }

    public void ModifyPassword()
    {
        Console.WriteLine("Enter the description of the password to modify:");
        string description = Console.ReadLine();
        Console.WriteLine("Enter the new password:");
        string newPassword = ReadPassword();

        string[] lines = File.ReadAllLines(filePath2);
        bool found = false;

        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].StartsWith(description + ":"))
            {
                found = true;

                byte[] data = UTF8Encoding.UTF8.GetBytes(newPassword);
                using (MD5 md5 = MD5.Create())
                using (TripleDES tripleDES = TripleDES.Create())
                {
                    tripleDES.Key = md5.ComputeHash(UTF8Encoding.UTF8.GetBytes(newPassword));
                    tripleDES.Mode = CipherMode.ECB;

                    ICryptoTransform transform = tripleDES.CreateEncryptor();
                    byte[] result = transform.TransformFinalBlock(data, 0, data.Length);

                    lines[i] = $"{description}:{Convert.ToBase64String(result)}";
                }

                break;
            }
        }

        if (found)
        {
            File.WriteAllLines(filePath2, lines);
            Console.WriteLine("Password modified.");
            Console.Clear();
        }
        else
        {
            Console.WriteLine("Description not found.");
            Console.Clear();
        }
    }

    public static string ReadPassword()
    {
        StringBuilder password = new StringBuilder();
        ConsoleKeyInfo key;

        do
        {
            key = Console.ReadKey(true);
            if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
            {
                password.Append(key.KeyChar);
                Console.Write("*");
            }
            else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
            {
                password.Remove(password.Length - 1, 1);
                Console.Write("\b \b");
            }
        } while (key.Key != ConsoleKey.Enter);

        Console.WriteLine();
        return password.ToString();
    }
}
