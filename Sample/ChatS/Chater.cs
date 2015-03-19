using Core.RPC;
using Core.Serialize;
using Proto;

namespace ChatS
{
    /// <summary>
    /// �����ߣ���ÿ���Զ�һһ��Ӧ
    /// ����ʵ����RpcHost
    /// </summary>
    public class Chater : RpcHost
    {
        public Chater(RpcSession session) : base(session)
        {
        }

        /// <summary>
        /// ��д���෽��
        /// ��ȷ�Լ�������Щ����
        /// </summary>
        protected override void RegisterRpcHandlers()
        {
            RegisterNotifyHandler(RpcRoute.Chat, HandleChatNotify);
            RegisterRequestHandler(RpcRoute.GmCmd, HandleGmRequest);
        }

        /// <summary>
        /// ����RpcRoute.GmCmd
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        private object HandleGmRequest(byte[] bytes)
        {
            var msg = Serializer.Deserialize<Message2Server>(bytes);

            // ����Request���󣬻���ProtoBufʵ��
            // ��ʵ�����ձ��Զ������߽���
            return new Broadcast2Clients
            {
                From = Session.Id.ToString(),
                Message = "Gm command [" + msg.Message + "] Responsed"
            };
        }

        /// <summary>
        /// ����RpcRoute.Chat
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        private bool HandleChatNotify(byte[] bytes)
        {
            var msg = Serializer.Deserialize<Message2Server>(bytes);
            var proto = new Broadcast2Clients
            {
                From = Session.Id.ToString(),
                Message = msg.Message
            };

            // �㲥����������������
            Session.NotifyAll(RpcRoute.Chat, proto);

            // ����ֻ֪ͨ�Լ�
            //Session.Notify(RpcRoute.Chat, proto);

            return true;
        }
    }
}