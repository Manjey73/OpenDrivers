
namespace ScadaCommFunc
{
    public static class ControlMod
    {
        // OSCAT Basic HYST is a standard Hysteresis module, its function depends on the input values ON and OFF.
        #region Oscat Basic HYST
        public class Hyst
        {
            public Hyst() { }

            public Hyst(double inCnl, double Low, double High, bool Q, bool mode = true)
            {
                this.inCnl = inCnl;
                this.Low = Low;
                this.High = High;
                this.Q = Q;
                this.mode = mode;
            }

            public double inCnl; // входная переменная Double (input value)
            public double Low; // входная переменная ON: Double (upper threshold)
            public double High; // выходная переменная OFF: Double (lower threshold)
            public bool Q; // выходная переменная Q: BOOL (output)
            public bool mode; // входная переменная режима работы, по умолчанию нагрев = true, охлаждение = false // the input variable of the operating mode, by default heating = true, cooling = false

            bool _mode; // внутренний флаг

            public void Run()
            {
                // Копирование входных переменных во внутренние при программы
                bool res = false;

                        // Копирование входных переменных во внутренние в начале цикла программы
                        _mode = mode;
                        // Инициализация выходных переменных если требуется.
                        res = Q; // для возможности задания retain при перезапусках
                        // тело программы
                        if (_mode)
                        {
                            if (inCnl < Low) res = true;
                            if (inCnl > High) res = false;
                        }
                        else
                        {
                            if (inCnl < Low) res = false;
                            if (inCnl > High) res = true;
                        }
                        // тело программы

                        // Копирование выходных переменных из внутренних в конце цикла программы, для некоторых необязательно, будут обработаны программой
                        Q = res;
            }
        }
        #endregion Oscat Basic HYST


        // OSCAT Basic 
    }
}
