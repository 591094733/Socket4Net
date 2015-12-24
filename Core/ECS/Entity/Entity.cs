using System;
using System.Collections;
using System.Collections.Generic;

namespace socket4net
{
    public class EntityArg : UniqueObjArg<long>
    {
        public EntityArg(IObj parent, long key, short group)
            : base(parent, key)
        {
            Group = group;
        }

        public short Group { get; private set; }
    }

    public interface IEntity : IUniqueObj<long>, IData, IScheduler
    {
        short Group { get; }
        T GetComponent<T>() where T : Component;
        T GetComponent<T>(short cpId) where T : Component;
        void Listen(Action<IEntity, IBlock> handler, params short[] pids);
        void Unlisten(Action<IEntity, IBlock> handler, params short[] pids);
        void SendMessage(object msg);
    }

    /// <summary>
    ///    E(ECS)
    /// </summary>
    public sealed partial class Entity : UniqueObj<long>, IEntity, IEnumerable<Component>
    {
        /// <summary>
        ///     ��
        /// </summary>
        public short Group { get; private set; }

        /// <summary>
        ///     �������
        /// </summary>
        private void SpawnComponents()
        {
            var cps = ComponentsCache.Instance.Get(GetType());
            if (cps.IsNullOrEmpty()) return;

            foreach (var type in cps)
            {
                AddComponent(type);
            }
        }
        
        #region ��ʼ����ж��

        /// <summary>
        ///     ��ʼ��
        /// </summary>
        /// <param name="objArg"></param>
        protected override void OnInit(ObjArg objArg)
        {
            base.OnInit(objArg);

            // ��
            Group = objArg.As<EntityArg>().Group;

            // ������
            SpawnComponents();

            // �����������������Ϊ�գ�
            _data = GetComponent<DataComponent>();
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

        public void SendMessage(object msg)
        {
            foreach (var component in Components)
            {
                component.OnMessage(msg);
            }
        }

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
            return (IEnumerator<Component>) GetEnumerator();
        }

        #endregion

        #region ����
        public void Listen(Action<IEntity, IBlock> handler, params short[] pids)
        {
            _data.Listen(handler, pids);
        }

        public void Unlisten(Action<IEntity, IBlock> handler, params short[] pids)
        {
            _data.Unlisten(handler, pids);
        }

        #endregion
    }
}