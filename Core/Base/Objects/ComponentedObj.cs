using System;
using System.Collections;
using System.Collections.Generic;
using socket4net.Util;

namespace socket4net
{
    public class ComponentedObjArg<TPKey> : PropertiedObjArg<TPKey>
    {
        public ComponentedObjArg(IObj parent, long key, IEnumerable<Pair<TPKey, byte[]>> properties,
            Func<PropertyBody<TPKey>, TPKey, IBlock<TPKey>> blockMaker) : base(parent, key, properties, blockMaker)
        {
        }
    }

    public interface IComponentedObj<TPKey> : IPropertiedObj<TPKey>
    {
        T GetComponent<T>() where T : Component<TPKey>;
        T GetComponent<T>(short cpId) where T : Component<TPKey>;
    }

    /// <summary>
    ///    ���������
    /// </summary>
    public abstract class ComponentedObj<TPKey> : PropertiedObj<TPKey>,IComponentedObj<TPKey>,
        IEnumerable<Component<TPKey>>
    {
        /// <summary>
        ///     �������
        /// </summary>
        protected virtual void SpawnComponents()
        {
        }
        
        protected virtual void OnSubscribe()
        {
        }

        protected virtual void OnUnsubscribe()
        {
        }

        protected virtual void OnInjectProperties()
        {
        }

        #region ��ʼ����ж��

        protected override void BeforeInit()
        {
            base.BeforeInit();
            
            // ������
            SpawnComponents();

            // ��ʼ�����������
            Components.Init();

            // ����ע��
            OnInjectProperties();
        }

        /// <summary>
        ///     ��ʼ��
        /// </summary>
        /// <param name="arg"></param>
        protected override void OnInit()
        {
            base.OnInit();
            
            // �¼�����
            OnSubscribe();

            // ����¼�����
            // ע�⣺Ϊ��������¼����Ĳ���������д���
            // ��Ϊ������п��ܶ�������������¼������ö���Ŀ������п�����δ������
            Components.Subscribe();
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

        public override void AfterStart()
        {
            base.AfterStart();
            Components.AfterStart();
        }

        /// <summary>
        /// ����
        /// </summary>
        protected sealed override void OnReset()
        {
            base.OnReset();

            // �������
            Components.Reset();
        }

        /// <summary>
        ///     ж��
        /// </summary>
        protected sealed override void OnDestroy()
        {
            base.OnDestroy();
            
            // ������
            OnUnsubscribe();

            // �������
            Components.Destroy();
        }

        /// <summary>
        ///     ִ�������޸�֪ͨ
        /// </summary>
        /// <param name="block"></param>
        protected override void DoPropertyChangedNotification(IBlock<TPKey> block)
        {
            base.DoPropertyChangedNotification(block);

            // ֪ͨ���
            foreach (var component in Components)
            {
                component.OnPropertyChanged(block);
            }
        }

        #endregion

        #region �����ʵ��

        private ComponentsMgr<TPKey> _components;
        public ComponentsMgr<TPKey> Components
        {
            get
            {
                return _components ??
                       (_components =
                           ObjFactory.Create<ComponentsMgr<TPKey>>(new UniqueMgrArg(this)));
            }
        }

        public Component<TPKey> GetComponent(short key)
        {
            return Components.Get(key);
        }

        public bool RemoveComponent(short key)
        {
            return Components.Destroy(key);
        }

        public T GetComponent<T>() where T : Component<TPKey>
        {
            return Components.GetFirst<T>();
        }

        public T GetComponent<T>(short cpId) where T : Component<TPKey>
        {
            return Components.Get(cpId) as T;
        }

        public List<T> GetComponents<T>() where T : Component<TPKey>
        {
            return Components.Get<T>();
        }

        public T AddComponent<T>(short id) where T : Component<TPKey>, new()
        {
            return Components.AddComponent<T>(id);
        }

        public List<short> RemoveComponent<T>() where T : Component<TPKey>
        {
            return Components.Destroy<T>();
        }

        public IEnumerator GetEnumerator()
        {
            return Components.GetEnumerator();
        }

        IEnumerator<Component<TPKey>> IEnumerable<Component<TPKey>>.GetEnumerator()
        {
            return (IEnumerator<Component<TPKey>>) GetEnumerator();
        }

        #endregion
    }
}