using System;
using System.Collections;
using System.Threading.Tasks;

namespace socket4net
{
    public interface IScheduler
    {
        /// <summary>
        ///     ����һ���ڵ�ǰ�̵߳ȴ�ms�����ö����
        ///     ����Э����
        /// </summary>
        /// <param name="ms"></param>
        /// <returns></returns>
        IEnumerator WaitFor(uint ms);

        void StartCoroutine(Func<IEnumerator> fun);
        void StartCoroutine(Func<object[], IEnumerator> fun, params object[] args);

        /// <summary>
        ///     ���������
        /// </summary>
        void ClearTimers();

        /// <summary>
        ///     ��ʱdelay����periodΪ�����ظ�ִ��action
        /// </summary>
        void InvokeRepeating(Action action, uint delay, uint period);

        /// <summary>
        ///     ��ʱdelay��ִ��action
        /// </summary>
        /// <param name="action"></param>
        /// <param name="delay"></param>
        void Invoke(Action action, uint delay);

        /// <summary>
        ///     ��whenʱ���ִ��action
        /// </summary>
        /// <param name="action"></param>
        /// <param name="when"></param>
        void Invoke(Action action, DateTime when);

        /// <summary>
        ///     ÿ�� hour:min:s ִ��action
        ///     �磺ÿ��20:15ִ��action����ʱ hour == 20 min == 15 s == 0
        /// </summary>
        /// <param name="action"></param>
        /// <param name="hour"></param>
        /// <param name="min"></param>
        /// <param name="s"></param>
        void Invoke(Action action, int hour, int min, int s);

        /// <summary>
        ///     ÿ�� time ִ��action
        ///     ע��time���Ǽ��
        /// </summary>
        /// <param name="action"></param>
        /// <param name="time"></param>
        void Invoke(Action action, TimeSpan time);

        /// <summary>
        ///     ȡ��action�ĵ���
        /// </summary>
        /// <param name="action"></param>
        void CancelInvoke(Action action);

        /// <summary>
        ///     ÿ�յ�timesʱ���ִ��action
        /// </summary>
        /// 
        Task<bool> InvokeAsync(Action action, params TimeSpan[] times);

        /// <summary>
        ///     ��whensָ����ʱ���ִ��action
        /// </summary>
        Task<bool> InvokeAsync(Action action, params DateTime[] whens);
        Task<bool> InvokeAsync(Action action, TimeSpan time);
        Task<bool> InvokeAsync(Action action, DateTime when);
        Task<bool> InvokeAsync(Action action, uint delay);
    }
}