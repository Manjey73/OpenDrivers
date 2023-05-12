[DrvGpiod v6.1.0.0](https://github.com/Manjey73/OpenDrivers/releases/download/DrvGpiod/DrvGpiod.zip)

Драйвер управления GPIO для Scada v6.1
Использовался пакет Nuget System.Device.Gpio v2.2.0 в качестве прокладки для пакета gpiod (apt install gpiod)
В некоторых системах пакет установлен.

Что касается нумерации gpio pin то в OrangPi можно посмотреть по команде gpioinfo и проверить по расчету. Например контакт PG7 — G = 7-я буква алфавита.
(7-1)*32 + 7 = 199 номер пина.

Для других процессоров не всегда можно посчитать.
Например ПК на DIN рейку JetHome JetHub D1 https://jethome.ru/d1/
Я так и не смог понять, как узнать точный номер pin для ввода в конфигурацию, подходят номера, указанные в документации.
Шаблон для входов/выходов прилагаю.

<?xml version="1.0" encoding="utf-8"?>
<DevTemplate Name="JetHub_Gpio">
  <Gpios>
    <Gpiod Name="GOUT1" Active="true" Code="out1" Pin="456" />
    <Gpiod Name="GOUT2" Active="true" Code="out2" Pin="455" />
    <Gpiod Name="GOUT3" Active="true" Code="out3" Pin="454" />
    <Gpiod Name="GIN_1" Active="true" Code="in1" Pin="472" />
    <Gpiod Name="GIN_2" Active="true" Code="in2" Pin="471" />
    <Gpiod Name="GIN_3" Active="true" Code="in3" Pin="470" />
    <Gpiod Name="GIN_4" Active="true" Code="in4" Pin="469" />
  </Gpios>
</DevTemplate>

Если входы/выходы уже сконфигурированы системой, PinMode можно не указывать, но если потребуется использовать стартовое значение PinValue при запуске драйвера то должен быть указан так же как в системе (на конфликтность не проверял)
Orange Pi не понимает типы входов InputPullDown и InputPullUp, только Input

На Raspberry пока не проверял.
