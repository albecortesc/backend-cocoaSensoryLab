using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Application.Interfaces;
using Infrastructure.Services;
using Domain.Entities;
using Domain.Enums;

namespace ConsoleApp
{
    internal class Program
    {
        private static async Task<int> Main(string[] args)
        {
            IUserService userService = new MongoUserService("mongodb://localhost:27017", "users_jwt", "users");

            while (true)
            {
                Console.Clear();
                Console.WriteLine("\n--- Usuario CLI ---");
                Console.WriteLine("1) Registrar usuario");
                Console.WriteLine("2) Login");
                Console.WriteLine("3) Listar usuarios");
                Console.WriteLine("4) Editar usuario");
                Console.WriteLine("5) Buscar usuario");
                Console.WriteLine("6) Borrar usuario");
                Console.WriteLine("0) Salir");
                Console.Write("Opción: ");
                var opt = Console.ReadLine();

                switch (opt)
                {
                    case "1":
                        Console.WriteLine();
                        Console.WriteLine("Registrar nuevo usuario:");
                        await Register(userService);
                        break;
                    case "2":
                        Console.Clear();
                        Console.WriteLine();
                        Console.WriteLine("Bienvenido al sistema, por favor ingrese sus credenciales: ");
                        await Authenticate(userService);
                        break;
                    case "3":
                        await ListUsers(userService);
                        break;
                    case "4":
                        await UpdateUser(userService);
                        break;
                    case "5":
                        await FindUser(userService);
                        break;
                    case "6":
                        await DeleteUser(userService);
                        break;
                    case "0":
                        return 0;
                    default:
                        Console.WriteLine("Opción no válida.");
                        break;
                }
            }
        } 

        // Función para registrar un nuevo usuario con validaciones
        private static async Task Register(IUserService svc)
        {
            try
            {
                // Validar Nombres
                var nombres = ValidateNombres("Nombres");
                
                // Validar Apellidos
                var apellidos = ValidateApellidos("Apellidos");
                
                // Validar Cédula
                var cedula = ValidateCedula("Cedula (solo numeros)");
                
                // Validar Teléfono
                var telefono = ValidateTelefono("Telefono (solo numeros)");
                
                // Validar Email
                var email = ValidateEmail("Email");
                
                // Validar Contraseña
                var password = ValidatePassword("Password");

                var role = ValidateRole();

                var user = User.CreateWithPassword(nombres, apellidos, cedula, telefono, email, role, password);
                await svc.UserRegister(user);
                Console.Clear();
                Console.WriteLine();
                Console.WriteLine("\n✓ Usuario registrado correctamente.");
                Console.WriteLine();
                Console.WriteLine("Presione cualquier tecla para continuar...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.Clear();
                Console.WriteLine();
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        // Funciones de validación para cada campo
        private static string ValidateNombres(string prompt)
        {
            while (true)
            {
                Console.Write($"{prompt}: ");
                var input = Console.ReadLine() ?? string.Empty;
                
                if (string.IsNullOrWhiteSpace(input))
                {
                    Console.WriteLine("❌ Los nombres no pueden estar vacíos. Intente de nuevo.");
                    continue;
                }
                
                if (input.Length < 1)
                {
                    Console.WriteLine("❌ Los nombres deben tener al menos 1 carácter. Intente de nuevo.");
                    continue;
                }
                
                return input.Trim();
            }
        }

        private static string ValidateApellidos(string prompt)
        {
            while (true)
            {
                Console.Write($"{prompt}: ");
                var input = Console.ReadLine() ?? string.Empty;
                
                if (string.IsNullOrWhiteSpace(input))
                {
                    Console.WriteLine("❌ Los apellidos no pueden estar vacíos. Intente de nuevo.");
                    continue;
                }
                
                if (input.Length < 1)
                {
                    Console.WriteLine("❌ Los apellidos deben tener al menos 1 carácter. Intente de nuevo.");
                    continue;
                }
                
                return input.Trim();
            }
        }

        private static long ValidateCedula(string prompt)
        {
            while (true)
            {
                Console.Write($"{prompt}: ");
                var input = Console.ReadLine() ?? string.Empty;
                
                if (!long.TryParse(input, out var cedula))
                {
                    Console.WriteLine("❌ La cédula debe ser un número válido. Intente de nuevo.");
                    continue;
                }
                
                if (cedula < 1000000)
                {
                    Console.WriteLine("❌ La cédula debe tener al menos 7 dígitos. Intente de nuevo.");
                    continue;
                }
                
                return cedula;
            }
        }

        private static long ValidateTelefono(string prompt)
        {
            while (true)
            {
                Console.Write($"{prompt}: ");
                var input = Console.ReadLine() ?? string.Empty;
                
                if (!long.TryParse(input, out var telefono))
                {
                    Console.WriteLine("❌ El teléfono debe ser un número válido. Intente de nuevo.");
                    continue;
                }
                
                if (telefono < 1000000000)
                {
                    Console.WriteLine("❌ El teléfono debe tener al menos 10 dígitos. Intente de nuevo.");
                    continue;
                }
                
                return telefono;
            }
        }

        private static string ValidateEmail(string prompt)
        {
            var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            
            while (true)
            {
                Console.Write($"{prompt}: ");
                var input = Console.ReadLine() ?? string.Empty;
                
                if (string.IsNullOrWhiteSpace(input))
                {
                    Console.WriteLine("❌ El email no puede estar vacío. Intente de nuevo.");
                    continue;
                }
                
                if (!Regex.IsMatch(input, emailPattern))
                {
                    Console.WriteLine("❌ El formato del email no es válido. Debe ser: usuario@dominio.com. Intente de nuevo.");
                    continue;
                }
                
                return input.Trim();
            }
        }

        private static string ValidatePassword(string prompt)
        {
            while (true)
            {
                Console.Write($"{prompt}: ");
                var password = ReadPassword();
                
                if (string.IsNullOrWhiteSpace(password))
                {
                    Console.WriteLine("❌ La contraseña no puede estar vacía. Intente de nuevo.");
                    continue;
                }
                
                if (password.Length < 5)
                {
                    Console.WriteLine("❌ La contraseña debe tener mínimo 5 caracteres. Intente de nuevo.");
                    continue;
                }
                
                return password;
            }
        }

        private static Role ValidateRole()
        {
            while (true)
            {
                Console.WriteLine("Role: 0=Root 1=Admin 2=User 3=Invited 4=Farmer 5=Taster (enter para User)");
                Console.Write("Role: ");
                var roleOpt = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(roleOpt))
                {
                    return Role.User;
                }

                if (int.TryParse(roleOpt, out var r) && Enum.IsDefined(typeof(Role), r))
                {
                    return (Role)r;
                }

                Console.WriteLine("❌ Role inválido. Debe ser 0, 1, 2, 3, 4, 5 o enter para User. Intente de nuevo.");
            }
        }

        // Función para autenticar o ingresar al sistema un usuario
        private static async Task Authenticate(IUserService svc)
        {
            Console.Write("Email: ");
            var email = Console.ReadLine() ?? string.Empty;
            Console.Write("Password: ");
            var password = ReadPassword();

            try
            {
                var ok = await svc.UserAuthenticate(email, password);
                Console.WriteLine();
                Console.WriteLine(ok ? "Autenticación correcta." : "Credenciales inválidas.");
        
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine($"Error: {ex.Message}");
            }
            Console.WriteLine();    
            Console.WriteLine("Presione cualquier tecla para continuar...");
            Console.ReadKey();
        }

        // Función para listar todos los usuarios registrados
        private static async Task ListUsers(IUserService svc)
        {
            try
            {
                var all = await svc.UserGetAll();
                Console.WriteLine(); 
                Console.WriteLine("Usuarios:");
                foreach (var u in all)
                {
                    Console.WriteLine($"- {u.Id ?? "(no id)"} | {u.Email} | {u.Nombres} {u.Apellidos} | Role: {u.Role}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al listar usuarios: {ex.Message}");
            }
            Console.WriteLine();    
            Console.WriteLine("Presione cualquier tecla para continuar...");
            Console.ReadKey();
        }


////La función UpdateUser no esta funcionando correctamente, no actualiza el usuario y 
/// no muestra ningún error, simplemente no hace nada. He probado con usuarios que existen 
/// y con usuarios que no existen, pero en ambos casos no pasa nada. He revisado el código 
/// y no veo ningún error evidente, pero no se si es un problema del servicio o del código 
/// de la función. Podrías ayudarme a identificar el problema y solucionarlo?

        // Función para actualizar o editar un usuario existente
        private static async Task UpdateUser(IUserService svc)
        {
            try
            {
                string? cedulaStr = null;
                string? email = null;
                bool hasCedulaInput = false;
                bool hasEmailInput = false;
                while (true)
                {
                    Console.Clear();
                    Console.WriteLine();
                    Console.WriteLine("Editar usuario por cedula o email.");
                    Console.WriteLine();
                    Console.WriteLine("Como desea identificar al usuario?");
                    Console.WriteLine("1) Cedula");
                    Console.WriteLine("2) Email");
                    Console.Write("Opcion: ");
                    var choice = Console.ReadLine();

                    if (choice == "1")
                    {
                        Console.WriteLine();
                        Console.Write("Cedula: ");
                        cedulaStr = Console.ReadLine();
                        hasCedulaInput = !string.IsNullOrWhiteSpace(cedulaStr);
                        hasEmailInput = false;
                    }
                    else if (choice == "2")
                    {
                        Console.WriteLine();
                        Console.Write("Email: ");
                        email = Console.ReadLine();
                        hasEmailInput = !string.IsNullOrWhiteSpace(email);
                        hasCedulaInput = false;
                    }
                    else
                    {
                        Console.WriteLine("Opcion invalida. Intente de nuevo.");
                        Console.WriteLine();
                        Console.WriteLine("Presione cualquier tecla para continuar...");
                        Console.ReadKey();
                        continue;
                    }

                    if (!hasCedulaInput && !hasEmailInput)
                    {
                        Console.WriteLine("Debe ingresar un valor.");
                        Console.WriteLine();
                        Console.WriteLine("Presione cualquier tecla para continuar...");
                        Console.ReadKey();
                        continue;
                    }

                    break;
                }

                long? cedula = null;
                if (hasCedulaInput)
                {
                    if (!long.TryParse(cedulaStr, out var c))
                    {
                        Console.WriteLine("Cedula inválida.");
                        Console.WriteLine();    
                        Console.WriteLine("Presione cualquier tecla para continuar...");
                        Console.ReadKey();
                        return;
                    }
                    cedula = c;
                }

                var existing = await svc.UserFindByCedulaOrEmail(cedula, email);
                if (existing == null)
                {
                    Console.WriteLine("Usuario no encontrado.");
                    Console.WriteLine();    
                    Console.WriteLine("Presione cualquier tecla para continuar...");
                    Console.ReadKey();
                    return;
                }

                Console.WriteLine();
                Console.WriteLine("Datos actuales:");
                Console.WriteLine($"Nombres: {existing.Nombres}");
                Console.WriteLine($"Apellidos: {existing.Apellidos}");
                Console.WriteLine($"Cedula: {existing.Cedula}");
                Console.WriteLine($"Telefono: {existing.Telefono}");
                Console.WriteLine($"Email: {existing.Email}");
                Console.WriteLine($"Role: {existing.Role}");
                Console.WriteLine();

                Console.WriteLine("Datos nuevos del usuario:");
                Console.Write($"Nombres (actual: {existing.Nombres}) [enter para mantener]: ");
                var nombresInput = Console.ReadLine();
                var nombres = string.IsNullOrWhiteSpace(nombresInput) ? existing.Nombres : nombresInput.Trim();

                Console.Write($"Apellidos (actual: {existing.Apellidos}) [enter para mantener]: ");
                var apellidosInput = Console.ReadLine();
                var apellidos = string.IsNullOrWhiteSpace(apellidosInput) ? existing.Apellidos : apellidosInput.Trim();

                long newCedula;
                while (true)
                {
                    Console.Write($"Cedula (actual: {existing.Cedula}) [enter para mantener]: ");
                    var newCedulaStr = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(newCedulaStr))
                    {
                        newCedula = existing.Cedula;
                        break;
                    }
                    if (!long.TryParse(newCedulaStr, out var parsedCedula))
                    {
                        Console.WriteLine("Cedula inválida.");
                        continue;
                    }
                    if (parsedCedula < 1000000)
                    {
                        Console.WriteLine("❌ La cédula debe tener al menos 7 dígitos. Intente de nuevo.");
                        continue;
                    }
                    newCedula = parsedCedula;
                    break;
                }

                long telefono;
                while (true)
                {
                    Console.Write($"Telefono (actual: {existing.Telefono}) [enter para mantener]: ");
                    var telefonoStr = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(telefonoStr))
                    {
                        telefono = existing.Telefono;
                        break;
                    }
                    if (!long.TryParse(telefonoStr, out var parsedTelefono))
                    {
                        Console.WriteLine("Telefono inválido.");
                        continue;
                    }
                    if (parsedTelefono < 1000000000)
                    {
                        Console.WriteLine("❌ El teléfono debe tener al menos 10 dígitos. Intente de nuevo.");
                        continue;
                    }
                    telefono = parsedTelefono;
                    break;
                }

                string newEmail;
                while (true)
                {
                    Console.Write($"Email (actual: {existing.Email}) [enter para mantener]: ");
                    var emailInput = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(emailInput))
                    {
                        newEmail = existing.Email;
                        break;
                    }
                    var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                    if (!Regex.IsMatch(emailInput, emailPattern))
                    {
                        Console.WriteLine("❌ El formato del email no es válido. Debe ser: usuario@dominio.com. Intente de nuevo.");
                        continue;
                    }
                    newEmail = emailInput.Trim();
                    break;
                }

                Role role;
                while (true)
                {
                    Console.WriteLine($"Role actual: {existing.Role}. Nuevo role: 0=Root 1=Admin 2=User 3=Invited 4=Farmer 5=Taster (enter para mantener)");
                    Console.Write("Role: ");
                    var roleOpt = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(roleOpt))
                    {
                        role = existing.Role;
                        break;
                    }
                    if (int.TryParse(roleOpt, out var r) && Enum.IsDefined(typeof(Role), r))
                    {
                        role = (Role)r;
                        break;
                    }
                    Console.WriteLine("❌ Role inválido. Debe ser 0, 1, 2, 3, 4, 5 o enter para mantener. Intente de nuevo.");
                }

                Console.Write("Password (enter para mantener): ");
                var password = ReadPassword();

                var passwordChanged = !string.IsNullOrWhiteSpace(password);

                Console.WriteLine();
                Console.WriteLine("Resumen de cambios:");
                Console.WriteLine($"Nombres: {existing.Nombres} -> {nombres}");
                Console.WriteLine($"Apellidos: {existing.Apellidos} -> {apellidos}");
                Console.WriteLine($"Cedula: {existing.Cedula} -> {newCedula}");
                Console.WriteLine($"Telefono: {existing.Telefono} -> {telefono}");
                Console.WriteLine($"Email: {existing.Email} -> {newEmail}");
                Console.WriteLine($"Role: {existing.Role} -> {role}");
                Console.WriteLine($"Password: {(passwordChanged ? "[cambiada]" : "[sin cambios]")}");
                Console.Write("Presione (s) guardar - cualquier otra tecla para cancelar: ");
                var confirm = Console.ReadLine();
                if (!string.Equals(confirm, "s", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine();
                    Console.WriteLine("Actualizacion cancelada.");
                    Console.WriteLine();
                    Console.WriteLine("Presione cualquier tecla para continuar...");    
                    Console.ReadKey();
                    return;
                }

                User updatedUser;
                if (!passwordChanged)
                {
                    updatedUser = new User(nombres, apellidos, newCedula, telefono, newEmail, role, existing.PasswordHash);
                }
                else
                {
                    updatedUser = User.CreateWithPassword(nombres, apellidos, newCedula, telefono, newEmail, role, password);
                }
                await svc.UserUpdateByCedulaOrEmail(cedula, email, updatedUser);
                Console.Clear();
                Console.WriteLine();
                Console.WriteLine("Usuario actualizado correctamente.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al editar usuario: {ex.Message}");
                Console.WriteLine();    
                Console.WriteLine("Presione cualquier tecla para continuar...");
                Console.ReadKey();
            }

            Console.WriteLine();    
            Console.WriteLine("Presione cualquier tecla para continuar...");
            Console.ReadKey();
        }

        // Función para buscar un usuario existente
        private static async Task FindUser(IUserService svc)
        {
            try
            {
                string? cedulaStr = null;
                string? email = null;
                bool hasCedulaInput = false;
                bool hasEmailInput = false;
                while (true)
                {
                    Console.Clear();
                    Console.WriteLine();
                    Console.WriteLine("Buscar usuario por cedula o email.");
                    Console.WriteLine();
                    Console.WriteLine("Como desea identificar al usuario?");
                    Console.WriteLine("1) Cedula");
                    Console.WriteLine("2) Email");
                    Console.Write("Opcion: ");
                    var choice = Console.ReadLine();

                    if (choice == "1")
                    {
                        Console.WriteLine();
                        Console.Write("Cedula: ");
                        cedulaStr = Console.ReadLine();
                        hasCedulaInput = !string.IsNullOrWhiteSpace(cedulaStr);
                        hasEmailInput = false;
                    }
                    else if (choice == "2")
                    {
                        Console.WriteLine();
                        Console.Write("Email: ");
                        email = Console.ReadLine();
                        hasEmailInput = !string.IsNullOrWhiteSpace(email);
                        hasCedulaInput = false;
                    }
                    else
                    {
                        Console.WriteLine();
                        Console.WriteLine("Opcion invalida. Intente de nuevo.");
                        Console.WriteLine();
                        Console.WriteLine("Presione cualquier tecla para continuar...");
                        Console.ReadKey();
                        continue;
                    }

                    if (!hasCedulaInput && !hasEmailInput)
                    {
                        Console.WriteLine("Debe ingresar un valor.");
                        Console.WriteLine();
                        Console.WriteLine("Presione cualquier tecla para continuar...");
                        Console.ReadKey();
                        continue;
                    }

                    break;
                }

                long? cedula = null;
                if (hasCedulaInput)
                {
                    if (!long.TryParse(cedulaStr, out var c))
                    {
                        Console.WriteLine();
                        Console.WriteLine("Cedula inválida.");
                        Console.WriteLine();
                        Console.WriteLine("Presione cualquier tecla para continuar...");
                        Console.ReadKey();
                        return;
                    }
                    cedula = c;
                }

                var user = await svc.UserFindByCedulaOrEmail(cedula, email);
                if (user == null)
                {
                    Console.WriteLine();
                    Console.WriteLine("Usuario no encontrado.");
                    Console.WriteLine();
                    Console.WriteLine("Presione cualquier tecla para continuar...");
                    Console.ReadKey();
                    return;
                }

                Console.WriteLine($"- {user.Id ?? "(no id)"} | {user.Email} | {user.Nombres} {user.Apellidos} | Cedula: {user.Cedula} | Telefono: {user.Telefono} | Role: {user.Role}");
                Console.WriteLine();
                Console.WriteLine("Presione cualquier tecla para continuar...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.Clear();
                Console.WriteLine($"Error al buscar usuario: {ex.Message}");
                Console.WriteLine();
                Console.WriteLine("Presione cualquier tecla para continuar...");    
                Console.ReadKey();
            }
        }

        // Función para borrar un usuario existente
        private static async Task DeleteUser(IUserService svc)
        {
            try
            {
                long? cedula = null;
                string? email = null;

                while (true)
                {
                    Console.Clear();
                    Console.WriteLine();
                    Console.WriteLine("Borrar usuario por cedula o email.");
                    Console.WriteLine();
                    Console.WriteLine("Como desea identificar al usuario?");
                    Console.WriteLine("1) Cedula");
                    Console.WriteLine("2) Email");
                    Console.Write("Opcion: ");
                    var choice = Console.ReadLine();

                    if (choice == "1")
                    {
                        Console.WriteLine();
                        Console.Write("Cedula: ");
                        var cedulaStr = Console.ReadLine();

                        if (string.IsNullOrWhiteSpace(cedulaStr))
                        {
                            Console.WriteLine("Debe ingresar una cedula.");
                            Console.WriteLine();
                            Console.WriteLine("Presione cualquier tecla para continuar...");
                            Console.ReadKey();
                            continue;
                        }

                        if (!long.TryParse(cedulaStr, out var c))
                        {
                            Console.WriteLine("Cedula inválida.");
                            Console.WriteLine();
                            Console.WriteLine("Presione cualquier tecla para continuar...");
                            Console.ReadKey();
                            continue;
                        }

                        cedula = c;
                        email = null;
                        break;
                    }

                    if (choice == "2")
                    {
                        Console.WriteLine();
                        Console.Write("Email: ");
                        var emailInput = Console.ReadLine();

                        if (string.IsNullOrWhiteSpace(emailInput))
                        {
                            Console.WriteLine("Debe ingresar un email.");
                            Console.WriteLine();
                            Console.WriteLine("Presione cualquier tecla para continuar...");
                            Console.ReadKey();
                            continue;
                        }

                        cedula = null;
                        email = emailInput.Trim();
                        break;
                    }

                    Console.WriteLine("Opcion invalida. Intente de nuevo.");
                    Console.WriteLine();
                    Console.WriteLine("Presione cualquier tecla para continuar...");
                    Console.ReadKey();
                }

                await svc.UserDeleteByCedulaOrEmail(cedula, email);
                Console.WriteLine("Usuario borrado correctamente.");
                Console.WriteLine();
                Console.WriteLine("Presione cualquier tecla para continuar...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al borrar usuario: {ex.Message}");
                Console.WriteLine();
                Console.WriteLine("Presione cualquier tecla para continuar...");
                Console.ReadKey();
            }
        }

        // Función para leer la contraseña sin mostrarla en consola
        private static string ReadPassword()
        {
            var sb = new System.Text.StringBuilder();
            while (true)
            {
                var key = Console.ReadKey(intercept: true);
                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    break;
                }
                if (key.Key == ConsoleKey.Backspace)
                {
                    if (sb.Length > 0)
                    {
                        sb.Length--;
                        Console.Write("\b \b");
                    }
                    continue;
                }
                if (!char.IsControl(key.KeyChar))
                {
                    sb.Append(key.KeyChar);
                    Console.Write('*');
                }
            }
            return sb.ToString();
        }
    }
}
