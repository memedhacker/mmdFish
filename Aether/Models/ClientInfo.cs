namespace Aether.Models
{
    /// <summary>
    /// State katmanında client kimlik ve adını taşıyan hafif veri transferi nesnesi.
    /// UI kontrolü olan ClientCard'dan bağımsızdır; katman sınırını korur.
    /// </summary>
    public sealed class ClientInfo
    {
        /// <summary> Client'ın benzersiz sayısal kimliği. </summary>
        public int Id { get; }

        /// <summary> Client'ın gösterilen adı. </summary>
        public string Name { get; }

        public ClientInfo(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public override string ToString() => $"Client #{Id}: {Name}";

        public override bool Equals(object? obj) => obj is ClientInfo other && Id == other.Id;
        public override int GetHashCode() => Id.GetHashCode();
    }
}
