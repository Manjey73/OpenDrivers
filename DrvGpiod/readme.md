[DrvGpiod v6.1.0.0](https://github.com/Manjey73/OpenDrivers/releases/download/DrvGpiod/DrvGpiod.zip)

                                                                               Russian
Драйвер управления GPIO для Scada v6.1
Использовался пакет Nuget System.Device.Gpio v2.2.0 в качестве прокладки для пакета gpiod (apt install gpiod)
В некоторых системах пакет установлен.

Что касается нумерации gpio pin то в OrangPi можно посмотреть по команде gpioinfo и проверить по расчету. Например контакт PG7 — G = 7-я буква алфавита.
(7-1)*32 + 7 = 199 номер пина.

Для других процессоров не всегда можно посчитать.
Например ПК на DIN рейку JetHome JetHub D1 https://jethome.ru/d1/
Я так и не смог понять, как узнать точный номер pin для ввода в конфигурацию, подходят номера, указанные в документации.

Если входы/выходы уже сконфигурированы системой, PinMode можно не указывать, но если потребуется использовать стартовое значение PinValue при запуске драйвера то должен быть указан так же как в системе (на конфликтность не проверял)
Orange Pi не понимает типы входов InputPullDown и InputPullUp, только Input

На Raspberry пока не проверял.

                                                                                 English


The GPIO control driver for Scada v6.1 Used the Nuget System.Device package.Gpio v2.2.0 as a gasket for the gpiod package (apt install gpiod) On some systems, the package is installed.

As for the numbering of gpio pin, in Orange you can look at the gpioinfo command and check by calculation. For example, contact PG7 — G = 7th letter of the alphabet. (7-1)*32 + 7 = 199 pin number.

For other processors, it is not always possible to calculate. For example a PC on a DIN rail JetHome JetHub D1 https://jethome.ru/d1 / I could not figure out how to find out the exact pin number for entering into the configuration, the numbers specified in the documentation are suitable.

If the inputs/outputs are already configured by the system, pinMode can not be specified, but if you need to use the starting PinValue value when starting the driver, it should be specified in the same way as in the system (I did not check for conflicts) Orange Pi does not understand the input types InputPullDown and InputPullUp, only Input

I haven't checked it on Raspberry yet.
