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
            Console.WriteLine("No se encontró un archivo de cuentas. Por favor, cree una cuenta.");
            Console.WriteLine("Ingrese su usuario:");
            string nameUsu = Console.ReadLine();
            Console.WriteLine("Ingrese su contraseña:");
            string passwordUsu = ReadPassword();

            using (StreamWriter sw = File.AppendText(filePath))
            {
                sw.WriteLine(nameUsu);
                sw.WriteLine(passwordUsu);
            }
            Console.WriteLine("Cuenta creada con éxito.");
        }

        while (true)
        {
            Console.WriteLine("Login");
            Console.WriteLine("Ingrese su usuario:");
            string a = Console.ReadLine();
            Console.WriteLine("Ingrese su contraseña:");
            string b = ReadPassword();

            if (VerifyUser(filePath, a, b))
            {
                while (true)
                {
                    Console.WriteLine("1. Agregar Contraseñas\n2. Buscar Contraseña\n3. Modificar Contraseña\n4. Salir");
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
                                Console.WriteLine("Ingrese la descripción de la contraseña que desea buscar:");
                                string description = Console.ReadLine();
                                string encryptedPassword = program.GetEncryptedPassword(description);
                                if (encryptedPassword != null)
                                {
                                    string result = program.SearchPassword(encryptedPassword);
                                    Console.WriteLine("Contraseña desencriptada: " + result);
                                }
                                else
                                {
                                    Console.WriteLine("Descripción no encontrada.");
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
                                Console.WriteLine("Ingreso inválido");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Ocurrió un error: {ex.Message}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("a");
                        Console.WriteLine("Por favor, ingrese un número válido.");
                        Console.Clear();
                    }
                }
                break;
            }
            else
            {
                Console.WriteLine("Usuario o contraseña incorrectos. Intente nuevamente.");
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
        Console.WriteLine("Ingrese la descripción:");
        string description = Console.ReadLine();
        Console.WriteLine("Ingrese la contraseña:");
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

        Console.WriteLine("Contraseña agregada y encriptada.");
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
        Console.WriteLine("Ingrese la descripción de la contraseña a modificar:");
        string description = Console.ReadLine();
        Console.WriteLine("Ingrese la nueva contraseña:");
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
            Console.WriteLine("Contraseña modificada.");
            Console.Clear();
        }
        else
        {
            Console.WriteLine("Descripción no encontrada.");
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
