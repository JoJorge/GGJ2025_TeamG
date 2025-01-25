using UnityEngine;
using System.Collections;

namespace CliffLeeCL
{
    /// <summary>
    /// The class provides two types of timer: normal timer and count down timer.
    /// </summary>
    /// <para>For normal timer, use <see cref="StartTimer"/>, <see cref="StopTimer"/>, and use <see cref="CurrentTime"/> to get timer's time. Will call callback functions when timer is stopped.</para>
    /// <para>For count down timer, use <see cref="StartCountDownTimer(float, bool, TimeIsUpHandler[])"/>, <see cref="StopCountDownTimer"/>. Will call callback functions when time's up.</para>
    public class Timer : MonoBehaviour
    {
        /// <summary>
        /// Define the <see cref="timeIsUpHandler"/> function's signature.
        /// </summary>
        public delegate void TimeIsUpHandler();
        /// <summary>
        /// The callback function will be called when time's up.
        /// </summary>
        TimeIsUpHandler timeIsUpHandler;

        /// <summary>
        /// Keep the value of timer's current time.
        /// </summary>
        /// <seealso cref="CurrentTime"/>
        float currentTime = 0.0f;
        /// <summary>
        /// The property that is related to currentTime. (Getter only.)
        /// </summary>
        /// <seealso cref="currentTime"/>
        public float CurrentTime
        {
            get { return currentTime; }
        }

        IEnumerator countDownTimer = null;
        /// <summary>
        /// Whether the timer is started.
        /// </summary>
        bool isTimerStarted = false;
        /// <summary>
        /// Whether the count down process is repetitive.
        /// </summary>
        bool isCountDownRepetitive = false;

        /// <summary>
        /// Update is called every frame, if the MonoBehaviour is enabled.
        /// </summary>
        void Update()
        {
            if (isTimerStarted)
                currentTime += Time.deltaTime;
        }

        /// <summary>
        /// Start the timer without the callback functions.
        /// </summary>
        /// <seealso cref="StartTimer(TimeIsUpHandler[])"/>
        /// <seealso cref="StopTimer"/>
        public void StartTimer()
        {
            currentTime = 0.0f;
            isTimerStarted = true;
            timeIsUpHandler = null;
        }

        /// <summary>
        /// Start the timer and set the callback functions.
        /// </summary>
        /// <param name="callback">The callback function will be called when timer is stopped.</param>
        /// <seealso cref="StartTimer"/>
        /// <seealso cref="StopTimer"/>
        public void StartTimer(params TimeIsUpHandler[] callback)
        {
            StartTimer();
            foreach (TimeIsUpHandler listener in callback)
                timeIsUpHandler += listener;
        }

        /// <summary>
        /// Stop the timer, call and clear the callback functions.
        /// </summary>
        /// <seealso cref="StartTimer"/>
        /// <seealso cref="StartTimer(TimeIsUpHandler[])"/>
        public void StopTimer()
        {
            if (isTimerStarted)
            {
                isTimerStarted = false;
                if (timeIsUpHandler != null)
                {
                    timeIsUpHandler();
                    timeIsUpHandler = null;
                }
            }
        }

        /// <summary>
        /// Start the count down timer and set the callback functions.
        /// </summary>
        /// <param name="time">The count down time.</param>
        /// <param name="isRepetitive">Whether the count down process is repetitive.</param>
        /// <param name="callback">The callback function will be called when time's up.</param>
        /// <seealso cref="StopCountDownTimer"/>
        public void StartCountDownTimer(float time, bool isRepetitive = false, params TimeIsUpHandler[] callback)
        {
            StopCountDownTimer();
            isTimerStarted = true;
            isCountDownRepetitive = isRepetitive;
            foreach (TimeIsUpHandler listener in callback)
                timeIsUpHandler += listener;
 
            countDownTimer = CountDownTimer(time);
            StartCoroutine(countDownTimer);
        }

        /// <summary>
        /// Stop the count down timer and clear the callback functions.
        /// </summary>
        /// <seealso cref="StartCountDownTimer(float, bool, TimeIsUpHandler[])"/>
        public void StopCountDownTimer()
        {
            isTimerStarted = false;
            if (countDownTimer != null)
                StopCoroutine(countDownTimer);
            timeIsUpHandler = null;
        }

        /// <summary>
        /// Coroutine that will do the count down calculation and call callback function when time's up.
        /// </summary>
        /// <param name="time">The count down time.</param>
        /// <returns>Interface that all coroutines use.</returns>
        /// <seealso cref="isCountDownRepetitive"/>
        IEnumerator CountDownTimer(float time)
        {
            do
            {
                currentTime = 0.0f;
                yield return new WaitForSeconds(time);

                if (timeIsUpHandler != null)
                    timeIsUpHandler();
            } while (isCountDownRepetitive);
            isTimerStarted = false;
        }
    }
}
