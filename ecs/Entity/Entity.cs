using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using socket4net;

namespace ecs
{
    public class EntityArg : UniqueObjArg<long>
    {
        public EntityArg(IObj parent, long key)
            : base(parent, key)
        {
        }
    }

    /// <summary>
    ///     �����Ϣ֪ͨ����
    /// </summary>
    public class Message
    {
    }

    /// <summary>
    ///     �첽��Ϣ
    /// </summary>
    public class AsyncMsg
    {
    }

    /// <summary>
    ///     ����������Ϣ
    /// </summary>
    public class NetReqMsg : AsyncMsg
    {
        public short Ops { get; set; }
        public byte[] Data { get; set; }
    }

    /// <summary>
    ///     ����������Ϣ
    /// </summary>
    public class NetPushMsg : AsyncMsg
    {
        public short Ops { get; set; }
        public byte[] Data { get; set; }
    }

    public interface IEntity : IUniqueObj<long>, IProperty
    {
        T GetComponent<T>() where T : Component;
        T GetComponent<T>(short cpId) where T : Component;
        void Listen(Action<IEntity, IBlock> handler, params short[] pids);
        void Unlisten(Action<IEntity, IBlock> handler, params short[] pids);
        void SendMessage(Message msg);
    }

    /// <summary>
    ///    E(ECS)
    /// </summary>
    public partial class Entity : UniqueObj<long>, IEntity, IEnumerable<Component>
    {
        /// <summary>
        ///     �������
        /// </summary>
        protected virtual void SpawnComponents()
        {
        }
        
        #region ��ʼ����ж��

        /// <summary>
        ///     ��ʼ��
        /// </summary>
        /// <param name="arg"></param>
        protected override void OnInit(ObjArg arg)
        {
            base.OnInit(arg);

            // ������
            SpawnComponents();

            // �����������������Ϊ�գ�
            _property = GetComponent<PropertyComponent>();
        }

        /// <summary>
        ///     ����
        /// </summary>
        protected override void OnStart()
        {
            base.OnStart();

            // �����ʼ
            Components.Start();
        }

        /// <summary>
        /// ����
        /// </summary>
        protected override void OnReset()
        {
            base.OnReset();

            // �������
            Components.Reset();
        }

        /// <summary>
        ///     ж��
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();

            // �������
            Components.Destroy();
        }

        #endregion

        public void SendMessage(Message msg)
        {
            OnMessage(msg);

            foreach (var component in Components)
            {
                component.OnMessage(msg);
            }
        }

        public virtual void OnMessage(Message msg)
        {
        }

#if NET45
        public virtual Task<RpcResult> OnMessageAsync(AsyncMsg msg)
        {
            return Task.FromResult(RpcResult.Failure);
        }
#else
        public virtual void OnMessageAsync(AsyncMsg msg, Action<RpcResult> cb)
        {
        }
#endif

        #region �����ʵ��

        private ComponentsMgr _components;
        public ComponentsMgr Components
        {
            get
            {
                return _components ??
                       (_components =
                           Create<ComponentsMgr>(new UniqueMgrArg(this)));
            }
        }

        public Component GetComponent(short key)
        {
            return Components.Get(key);
        }

        public bool RemoveComponent(short key)
        {
            return Components.Destroy(key);
        }

        public T GetComponent<T>() where T : Component
        {
            return Components.GetFirst<T>();
        }

        public T GetComponent<T>(short cpId) where T : Component
        {
            return Components.Get(cpId) as T;
        }

        public List<T> GetComponents<T>() where T : Component
        {
            return Components.Get<T>();
        }

        public T AddComponent<T>() where T : Component, new()
        {
            return Components.AddComponent<T>();
        }

        public Component AddComponent(Type cpType)
        {
            return Components.AddComponent(cpType);
        }

        public List<short> RemoveComponent<T>() where T : Component
        {
            return Components.Destroy<T>();
        }

        public IEnumerator GetEnumerator()
        {
            return Components.GetEnumerator();
        }

        IEnumerator<Component> IEnumerable<Component>.GetEnumerator()
        {
            return (IEnumerator<Component>)GetEnumerator();
        }

        #endregion

        #region ����

        public void Listen(Action<IEntity, IBlock> handler, params short[] pids)
        {
            _property.Listen(handler, pids);
        }

        public void Unlisten(Action<IEntity, IBlock> handler, params short[] pids)
        {
            _property.Unlisten(handler, pids);
        }

        #endregion
    }
}