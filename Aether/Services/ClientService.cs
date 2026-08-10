using Aether.Models;
using System.Collections.Generic;

namespace Aether.Services
{
    /// <summary>
    /// Client verilerinin üretimi ve oluşturulmasından sorumlu servis katmanı.
    ///
    /// NOT: GenerateDefaultClients stateless bir üretim metodudur; instance state gerektirmez.
    /// Bu nedenle static olarak tasarlanmıştır.
    /// </summary>
    public static class ClientService
    {
        /// <summary>
        /// Varsayılan client listesini oluşturur.
        /// </summary>
        public static List<ClientModel> GenerateDefaultClients(int count = 10)
        {
            var list = new List<ClientModel>();
            for (int i = 1; i <= count; i++)
            {
                list.Add(new ClientModel(i, $"Client {i}"));
            }
            return list;
        }
    }
}
