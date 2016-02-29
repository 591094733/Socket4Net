using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using socket4net;

namespace node
{
    public class NodeArg : UniqueObjArg<Guid>
    {
        public NodeArg(IObj parent, Guid key, NodeElement cfg)
            : base(parent, key)
        {
            Config = cfg;
        }

        public NodeElement Config { get; private set; }
    }

    public interface INode : IObj
    {
        IEnumerable<T> GetSession<T>() where T : ISession;
        T GetSession<T>(long sid) where T : class, ISession;
        IEnumerable<T> GetSession<T>(Predicate<T> condition) where T : ISession;
        T GetFirstSession<T>(Predicate<T> condition) where T : ISession;
        T GetFirstSession<T>() where T : ISession;

        Task<RpcResult> RequestAsync<T>(long sessionId, byte targetServer, long playerId, short ops,
            T proto, long objId, short componentId);

        Task<RpcResult> RequestAsync(long sessionId, byte targetServer, long playerId, short ops,
            byte[] data, long objId, short componentId);

        void RequestAsync<T>(long sessionId, byte targetServer, long playerId, short ops,
            T proto, long objId, short componentId, Action<bool, byte[]> cb);

        void RequestAsync(long sessionId, byte targetServer, long playerId, short ops,
            byte[] data, long objId, short componentId, Action<bool, byte[]> cb);

        bool Push<T>(long sessionId, byte targetServer, long playerId, short ops, T proto, long objId,
            short componentId);

        bool Push(long sessionId, byte targetServer, long playerId, short ops, byte[] data, long objId,
            short componentId);
    }

    /// <summary>
    ///     �ڵ�
    /// </summary>
    public abstract class Node : UniqueObj<Guid>, INode
    {
        /// <summary>
        ///     ��
        /// </summary>
        public override string Name
        {
            get
            {
                return string.Format("{0}:{1}:{2}", GetType().Name, Config.Name,
                    Peer != null ? string.Format("{0}:{1}", Peer.Ip, Peer.Port) : "��");
            }
        }

        /// <summary>
        ///     �ڵ�����
        /// </summary>
        public string Type
        {
            get { return Config.Type; }
        }

        /// <summary>
        ///     Peer
        /// </summary>
        public IPeer Peer { get; protected set; }

        /// <summary>
        ///     ����
        /// </summary>
        public NodeElement Config { get; private set; }

        /// <summary>
        ///     ��ȡ����
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetConfig<T>() where T : NodeElement
        {
            return Config as T;
        }

        /// <summary>
        ///     ��ȡ�Ự��
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="condition"></param>
        /// <returns></returns>
        public IEnumerable<T> GetSession<T>() where T : ISession
        {
            return Peer == null ? null : Peer.SessionMgr.Get<T>();
        }

        public T GetSession<T>(long sid) where T : class, ISession
        {
            return Peer == null ? null : Peer.SessionMgr.Get(sid) as T;
        }

        public IEnumerable<T> GetSession<T>(Predicate<T> condition) where T : ISession
        {
            return Peer == null ? null : Peer.SessionMgr.Get<T>().Where(x => condition(x));
        }

        public T GetFirstSession<T>(Predicate<T> condition) where T : ISession
        {
            var sessions = GetSession(condition);
            return sessions == null ? default(T) : sessions.FirstOrDefault();
        }

        public T GetFirstSession<T>() where T : ISession
        {
            var sessions = GetSession<T>();
            return sessions == null ? default(T) : sessions.FirstOrDefault();
        }

        protected override void OnInit(ObjArg arg)
        {
            base.OnInit(arg);

            Config = arg.As<NodeArg>().Config;
        }

        #region

        public async Task<RpcResult> RequestAsync<T>(long sessionId, byte targetServer, long playerId, short ops,
            T proto, long objId, short componentId)
        {
            return
                await
                    RequestAsync(sessionId, targetServer, playerId, ops, PiSerializer.Serialize(proto), objId,
                        componentId);
        }

        public async Task<RpcResult> RequestAsync(long sessionId, byte targetServer, long playerId, short ops,
            byte[] data, long objId, short componentId)
        {
            var session = Peer.SessionMgr.Get(sessionId) as IRpcSession;
            if (session == null) return RpcResult.Failure;
            return await session.RequestAsync(targetServer, playerId, ops, data, objId, componentId);
        }

        public void RequestAsync<T>(long sessionId, byte targetServer, long playerId, short ops,
            T proto, long objId, short componentId, Action<bool, byte[]> cb)
        {
            RequestAsync(sessionId, targetServer, playerId, ops, PiSerializer.Serialize(proto), objId, componentId, cb);
        }

        public void RequestAsync(long sessionId, byte targetServer, long playerId, short ops,
            byte[] data, long objId, short componentId, Action<bool, byte[]> cb)
        {
            var session = Peer.SessionMgr.Get(sessionId) as IRpcSession;
            if (session == null)
            {
                cb(false, null);
                return;
            }

            session.RequestAsync((byte)targetServer, playerId, ops, data, objId, componentId, cb);
        }

        public bool Push<T>(long sessionId, byte targetServer, long playerId, short ops, T proto, long objId,
            short componentId)
        {
            return Push(sessionId, targetServer, playerId, ops, PiSerializer.Serialize(proto), objId, componentId);
        }

        public bool Push(long sessionId, byte targetServer, long playerId, short ops, byte[] data, long objId,
            short componentId)
        {
            var session = Peer.SessionMgr.Get(sessionId) as IRpcSession;
            if (session == null) return false;
            session.Push((byte)targetServer, playerId, ops, data, objId, componentId);
            return true;
        }

        #endregion
    }

    public abstract class Node<TSession> : Node where TSession : class, IRpcSession, new()
    {
        protected override void OnStart()
        {
            base.OnStart();

            Peer.EventSessionClosed += OnDisconnected;
            Peer.EventSessionEstablished += OnConnected;
            Peer.EventErrorCatched += OnError;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            Peer.EventSessionClosed -= OnDisconnected;
            Peer.EventSessionEstablished -= OnConnected;
            Peer.EventErrorCatched -= OnError;
        }

        private void OnConnected(ISession session)
        {
            OnConnected((TSession)session);
        }

        private void OnDisconnected(ISession session, SessionCloseReason reason)
        {
            OnDisconnected((TSession)session, reason);
        }

        protected virtual void OnConnected(TSession session)
        {
        }

        protected virtual void OnDisconnected(TSession session, SessionCloseReason reason)
        {
        }

        protected virtual void OnError(string msg)
        {
        }
    }
}