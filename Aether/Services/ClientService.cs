using Aether.Models;
using System.Collections.Generic;

namespace Aether.Services
{
    /// <summary>
    /// Client verilerinin yönetimi, üretimi ve filtrelenmesinden sorumlu servis katmanı.
    /// </summary>
    public class ClientService
    {
        private static readonly Lazy<ClientService> _instance = new Lazy<ClientService>(() => new ClientService());
        public static ClientService Instance => _instance.Value;

        /// <summary>
        /// Varsayılan 10 adet Client listesini oluşturur.
        /// </summary>
        public List<ClientModel> GenerateDefaultClients(int count = 10)
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
