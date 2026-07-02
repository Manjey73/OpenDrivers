
namespace ScadaCommFunc
{
    public static class Timers
    {
        // Таймеры МЭК и другие
        // IEC timers and others
        #region Ticks
        public static long Ticks()
        {
            DateTime now = DateTime.Now;
            long time = now.Ticks / 10000;
            return time;
        }
        #endregion Ticks

        #region Ton
        public class TON
        {
            public TON()
            {
            }

            public TON(bool IN, ulong PT, ulong ET, bool Q)
            {
                this.IN = IN;
                this.PT = PT;
                this.ET = ET;
                this.Q = Q;
            }

            public bool IN; // входная переменная
            public ulong PT; // входная переменная
            public ulong ET; // выходная переменная - текущее значение таймера
            public bool Q; // выходная переменная
            bool _M; // внутренний флаг
            ulong _StartTime;

            public bool Run(bool IN)
            {
                if (!IN)
                {
                    Q = false;
                    ET = 0;
                    _M = false;
                }
                else
                {
                    if (!_M)
                    {
                        _M = true; // взводим флаг М
                        _StartTime = (ulong)Ticks();
                        // ET = 0; // сразу = 0
                    }
                    else
                    {
                        if (!Q)
                            ET = (ulong)Ticks() - _StartTime; // вычисляем время
                    }
                    if (ET >= PT)
                        Q = true;
                }
                return Q;
            }
        }
        #endregion Ton




    }
}
