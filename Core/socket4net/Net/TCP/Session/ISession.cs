using System.Net.Sockets;
#if NET45
using System.Threading.Tasks;
#endif

namespace socket4net
{
    public enum SessionCloseReason
    {
        ClosedByMyself,
        ClosedByRemotePeer,
        ReadError,
        WriteError,
        PackError,

        /// <summary>
        /// ����ͬһ�˺�ʹ�ò�ͬ�Ựʱ���ᶥ��֮ǰ�Ự
        /// </summary>
        Replaced,
    }

    public interface ISession : IUniqueObj<long>
    {
        Socket UnderlineSocket { get;}
        ushort ReceiveBufSize { get; }
        ushort PackageMaxSize { get; }

        void Close(SessionCloseReason reason);
        void InternalSend(NetPackage pack);

#if NET35
        void Dispatch(byte[] data);
#else
        Task Dispatch(byte[] data);
#endif
    }
}