using ProtoBuf;

namespace socket4net
{
    /// <summary>
    ///     ��װһ��Item
    /// </summary>
    [ProtoContract]
    public class ListItemRepresentation<T>
    {
        /// <summary>
        ///     �����Item��Id
        ///     �ڵ�ǰBlockΨһ
        /// </summary>
        [ProtoMember(1)]
        public int Id { get; set; }

        /// <summary>
        ///     Ŀ��Item
        /// </summary>
        [ProtoMember(2)]
        public T Item { get; set; }
    }
}